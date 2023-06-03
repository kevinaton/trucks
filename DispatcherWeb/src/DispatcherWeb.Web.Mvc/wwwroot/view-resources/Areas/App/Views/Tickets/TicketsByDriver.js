(function () {

    var _dtHelper = abp.helper.dataTables;
    var _ticketService = abp.services.app.ticket;
    var _orderService = abp.services.app.order;
    var _validateTrucksAndDrivers = abp.setting.getBoolean('App.General.ValidateDriverAndTruckOnTickets');
    var _orderLines = [];
    var _leaseHaulers = [];
    var _tickets = [];
    var _drivers = [];
    var _trucks = [];
    var _driverAssignments = [];
    var _dailyFuelCost = null;
    var _$currentFuelCostInput = $('#CurrentFuelCost');
    var _hasOpenOrders = false;
    var _orderLineBlocks = [];
    var _leaseHaulerBlocks = [];
    var _$ticketPhotoInput = $('#TicketPhoto');
    var _expandAllPanelsButton = $('#ExpandAllPanelsButton');
    var _ticketForPhotoUpload = null;
    var _blockForPhotoUpload = null;
    var _driverIdFilterInput = null;
    var _driverIdFilter = null;
    var _initializing = 0;
    var _date = null;

    var _selectDriverModal = new app.ModalManager({
        viewUrl: abp.appPath + 'app/Tickets/SelectDriverModal',
        scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Tickets/_SelectDriverModal.js',
        modalClass: 'SelectDriverModal'
    });

    var _selectDateModal = new app.ModalManager({
        viewUrl: abp.appPath + 'app/Tickets/SelectDateModal',
        scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Tickets/_SelectDateModal.js',
        modalClass: 'SelectDateModal'
    });



    initFilterControls();
    //reloadAllData();
    $('[data-toggle="tooltip"]').tooltip();


    function saveFilterState() {
        app.localStorage.setItem('tickets_by_driver_filter', _dtHelper.getFilterData());
    }

    function loadFilterState() {
        app.localStorage.getItem('tickets_by_driver_filter', function (result) {
            var filter = result || {};

            if (filter.date) {
                $('#DateFilter').val(filter.date);
                reloadAllData();
            }
        });
    }

    function initFilterControls() {
        $("#DateFilter")
            //.val(moment().format("MM/DD/YYYY"))
            //.val(moment('10/20/2021').format("MM/DD/YYYY"))
            .blur(function () {
                if (!moment($(this).val(), 'MM/DD/YYYY').isValid()) {
                    $(this).val(moment().format("MM/DD/YYYY"));
                }
            })
            .on('dp.change', function () {
                let newDate = $("#DateFilter").val();
                if (moment(newDate, 'MM/DD/YYYY').isValid() && newDate !== _date) {
                    _date = newDate;
                    saveFilterState();
                    reloadAllData();
                }
            })
            .datepicker({
                format: 'L',
                useCurrent: false,
                //viewMode: 'days',
                //keepInvalid: true,
                //defaultDate: ""
            });

        loadFilterState();

        //debug
        window.reloadAllData = reloadAllData;
        window.renderView = renderView;
        window.getOrderLineBlocks = function () {
            return _orderLineBlocks;
        };
        window.getLeaseHaulerBlocks = function () {
            return _leaseHaulerBlocks;
        }
    }

    function initDriverFilter() {
        if (!_drivers || !_orderLineBlocks) {
            return;
        }

        if (!_driverIdFilterInput) {
            _driverIdFilterInput = $("#DriverIdFilter");
            _driverIdFilterInput.select2Init({
                showAll: true,
                allowClear: true
            }).change(function () {
                _driverIdFilter = Number(_driverIdFilterInput.val()) || null;
                _orderLineBlocks.forEach(block => {
                    block.ui && block.ui.updateVisibility();
                });
                _leaseHaulerBlocks.forEach(lhBlock => {
                    lhBlock.ui && lhBlock.ui.updateVisibility();
                    lhBlock.ui && lhBlock.ui.updateTicketCounters();
                });
            });
        }

        let driversForFilter = _drivers
            .filter(d => _orderLineBlocks.some(o => o.driverId === d.id)) //d => d.isActive
            .sort(orderDriverByName);

        if (!driversForFilter.some(d => d.id === _driverIdFilter)) {
            _driverIdFilterInput.val(null).change();
        }

        _driverIdFilterInput.find('option').not('[value=""]').remove();
        _driverIdFilterInput.append(
            driversForFilter
                .map(d => $('<option>').attr('value', d.id).text(d.name))
                .reduce((prev, curr) => prev ? prev.add(curr) : curr, $())
        );
    }

    function reloadAllDataAndThrow(e) {
        return reloadAllData({
            suppressWarnings: true
        }).then(() => { throw e; });
    }

    function reloadAllData(loadDataOptions) {
        $('#TicketList').empty();
        $('#CurrentFuelCostContainer').hide();
        _orderLines = [];
        _leaseHaulers = [];
        _tickets = [];
        _drivers = [];
        _trucks = [];
        _driverAssignments = [];
        _dailyFuelCost = null;
        _hasOpenOrders = false;
        _orderLineBlocks.forEach(function (block) {
            if (block.ui) {
                block.ui.destroy();
                block.ui.card && block.ui.card.remove();
            }
        });
        _orderLineBlocks = []; //one per unique orderLine-driver (later per unique orderLine-driver-truck)
        _leaseHaulerBlocks = [];
        return loadData(loadDataOptions);
    }

    function loadData(loadDataOptions) {
        loadDataOptions = loadDataOptions || {};
        var filter = _dtHelper.getFilterData();
        if (!filter.date) {
            _date = null;
            return;
        }
        _date = filter.date;

        abp.ui.setBusy();
        logTimeIfNeeded('sending request to getTicketsByDriver');
        return _ticketService.getTicketsByDriver(filter).then(function (result) {
            logTimeIfNeeded('received getTicketsByDriver data');
            console.log(result);
            if (result.orderLines && result.orderLines.length) {
                _orderLines = result.orderLines;
            }
            if (result.tickets && result.tickets.length) {
                _tickets = result.tickets;
            }
            if (result.drivers && result.drivers.length) {
                _drivers = result.drivers;
            }
            if (result.trucks && result.trucks.length) {
                _trucks = result.trucks;
            }
            if (result.driverAssignments && result.driverAssignments.length) {
                _driverAssignments = result.driverAssignments;
            }
            if (result.leaseHaulers && result.leaseHaulers.length) {
                _leaseHaulers = result.leaseHaulers;
            }
            _dailyFuelCost = result.dailyFuelCost;
            if (result.hasOpenOrders !== _hasOpenOrders) {
                _hasOpenOrders = result.hasOpenOrders;
                if (_hasOpenOrders && !loadDataOptions.suppressWarnings) {
                    abp.message.warn(app.localize('SomeOfTheOrdersAreStillOpenWillNotBeDisplayed'));
                }
            }

            populateLeaseHaulerBlocks(_leaseHaulers);
            populateOrderLineBlocks(_orderLines, _tickets);
            renderView();
            abp.ui.clearBusy();
        }, function (err) {
            abp.ui.clearBusy();
            throw err;
        });
    }

    function populateLeaseHaulerBlocks(leaseHaulers) {

        _leaseHaulerBlocks.push({
            leaseHaulerId: null,
            leaseHaulerName: app.localize('UnknownDriver'),
            knownDriver: false,
            ui: null
        });

        _leaseHaulerBlocks.push({
            leaseHaulerId: null,
            leaseHaulerName: app.localize('Internal'),
            knownDriver: true,
            ui: null
        });

        leaseHaulers.forEach(function (leaseHauler) {
            _leaseHaulerBlocks.push({
                leaseHaulerId: leaseHauler.id,
                leaseHaulerName: leaseHauler.name,
                knownDriver: true,
                ui: null
            });
        });
    }

    function populateOrderLineBlocks(orderLines, tickets) {
        if (orderLines) {
            logTimeIfNeeded('started to populate orderLineBlocks from OrderLines');
            orderLines.forEach(function (orderLine) {
                if (orderLine.isCancelled) {
                    return;
                }
                orderLine.orderLineTrucks.filter(olt => olt.driverId).forEach(function (olt) {
                    var existingOrderLineTruck = _orderLineBlocks.find(block => block.orderLineId === orderLine.id && block.driverId === olt.driverId);
                    if (existingOrderLineTruck) {
                        return;
                    }
                    _orderLineBlocks.push({
                        orderLineId: orderLine.id,
                        driverId: olt.driverId,
                        orderLine: orderLine,
                        driver: _drivers.find(d => d.id === olt.driverId),
                        ui: null
                    });
                });
            });
        }
        if (tickets) {
            logTimeIfNeeded('started to populate OrderLineBlocks from Tickets');
            tickets.forEach(function (ticket) {
                var existingOrderLineTruck = _orderLineBlocks.find(block => block.orderLineId === ticket.orderLineId && block.driverId === ticket.driverId);
                if (existingOrderLineTruck) {
                    return;
                }
                let orderLine = _orderLines.find(o => o.id == ticket.orderLineId);
                if (!orderLine) {
                    return; //an orderLine wasn't loaded because it's not closed yet
                }
                _orderLineBlocks.push({
                    orderLineId: ticket.orderLineId,
                    driverId: ticket.driverId,
                    orderLine: orderLine,
                    driver: _drivers.find(d => d.id === ticket.driverId),
                    ui: null
                });
            });
        }
        logTimeIfNeeded('started to sort orderLineBlocks by driver name')
        _orderLineBlocks.sort((a, b) => {
            if (a.driver == b.driver) {
                return 0;
            }
            let driverA = (a.driver && a.driver.name || '').toUpperCase();
            let driverB = (b.driver && b.driver.name || '').toUpperCase();
            if (driverA < driverB) {
                return -1;
            }
            if (driverA > driverB) {
                return 1;
            }
            let customerA = (a.orderLine.customerName || '').toUpperCase();
            let customerB = (b.orderLine.customerName || '').toUpperCase();
            if (customerA < customerB) {
                return -1;
            }
            if (customerA > customerB) {
                return 1;
            }
            return 0;
        });
        logTimeIfNeeded('started to init driver filter');
        initDriverFilter();
        logTimeIfNeeded('finished populating OrderLineBlocks (in-memory)');
    }

    function orderDriverByName(a, b) {
        let driverA = (a.name || '').toUpperCase();
        let driverB = (b.name || '').toUpperCase();
        if (driverA < driverB) {
            return -1;
        }
        if (driverA > driverB) {
            return 1;
        }
        return 0;
    }

    function findBlockByTicket(ticket) {
        var driverId = ticket.driver && ticket.driver.id || null;
        var orderLineId = ticket.orderLineId;
        return _orderLineBlocks.find(b => b.orderLine.id === orderLineId && b.driverId === driverId);
    }

    function getTicketsForLeaseHaulerBlock(lhBlock) {
        var leaseHaulerId = lhBlock.leaseHaulerId;
        if (lhBlock.knownDriver) {
            let matchingDriverIds = _drivers.filter(d => d.leaseHaulerId === leaseHaulerId && (!_driverIdFilter || _driverIdFilter === d.id)).map(d => d.id);
            var tickets = _tickets.filter(t => matchingDriverIds.includes(t.driverId));
            return tickets;
        } else {
            var tickets = _tickets.filter(t => !_driverIdFilter && t.driverId === null && leaseHaulerId === null);
            return tickets;
        }
    }

    function getTicketsForOrderLineBlock(block) {
        var orderLineId = block.orderLine && block.orderLine.id;
        var driverId = block.driver && block.driver.id || null;
        if (!orderLineId) {
            return [];
        }
        var tickets = _tickets.filter(t => t.orderLineId === orderLineId && t.driverId === driverId);
        return tickets;
    }

    function getTicketsForOrderLine(orderLine) {
        var orderLineId = orderLine && orderLine.id;
        if (!orderLineId) {
            return [];
        }
        var tickets = _tickets.filter(t => t.orderLineId === orderLineId);
        return tickets;
    }

    function saveChanges(model) {
        return _ticketService.editTicketsByDriver(model).then(saveResult => {
            abp.notify.info('Successfully saved');
            return saveResult;
        }, (e) => reloadAllDataAndThrow(e));
    }

    function saveOrderLine(orderLine) {
        return saveChanges({
            orderLines: [orderLine]
        }).then((saveResult) => {
            if (saveResult && saveResult.orderLines && saveResult.orderLines.length === 1) {
                orderLine.fuelSurchargeRate = saveResult.orderLines[0].fuelSurchargeRate;
            } else {
                return reloadAllData();
            }

            var affectedBlocks = _orderLineBlocks.filter(o => o.orderLine.id === orderLine.id);
            affectedBlocks.forEach(function (affectedBlock) {
                updateCardFromModel(affectedBlock);
                refreshFieldHighlighting(affectedBlock);
            });

            return saveResult;
        }, (e) => reloadAllDataAndThrow(e));
    }

    function saveTicket(ticket) {
        return saveChanges({
            tickets: [ticket]
        }).then((saveResult) => {
            if (saveResult && saveResult.tickets && saveResult.tickets.length === 1) {
                ticket.id = saveResult.tickets[0].id;
                //ticket.ticketDateTime = saveResult.tickets[0].ticketDateTime;
            } else {
                return reloadAllData();
            }
            return saveResult;
        }, (e) => reloadAllDataAndThrow(e));
    }

    function updateCardFromModel(block) {
        if (!block.ui) {
            return;
        }
        _initializing++;
        setInputOrDropdownValue(block.ui.driver, block.driver && block.driver.id, block.driver && block.driver.name);
        setInputOrDropdownValue(block.ui.customer, block.orderLine.customerId, block.orderLine.customerName);
        block.ui.orderId.val(block.orderLine.orderId);
        block.ui.jobNumber.val(block.orderLine.jobNumber);
        setInputOrDropdownValue(block.ui.loadAt, block.orderLine.loadAtId, block.orderLine.loadAtName);
        setInputOrDropdownValue(block.ui.deliverTo, block.orderLine.deliverToId, block.orderLine.deliverToName);
        setInputOrDropdownValue(block.ui.item, block.orderLine.serviceId, block.orderLine.serviceName);
        setInputOrDropdownValue(block.ui.uom, block.orderLine.uomId, block.orderLine.uomName);
        block.ui.freightRate.val(block.orderLine.freightRate);
        block.ui.freightRateToPayDrivers.val(block.orderLine.freightRateToPayDrivers);
        block.ui.materialRate.val(block.orderLine.materialRate);
        block.ui.fuelSurchargeRate.val(block.orderLine.fuelSurchargeRate);

        if (block.orderLine.isMaterialTotalOverridden || block.orderLine.isFreightTotalOverridden) {
            block.ui.clickableWarningIcon
                .attr('title', getClickableWarningHoverText(block))
                .show()
                .click(() => {
                    askToResetOrderLineOverrideFlags(block);
                });
        } else {
            block.ui.clickableWarningIcon
                .attr('title', '')
                .hide()
                .off('click');
        }

        updateCardReadOnlyState(block);
        if (abp.auth.hasPermission('EditInvoicedOrdersAndTickets')
            && block.isReadOnly
            && !block.overrideReadOnlyState
        ) {
            block.ui.overrideReadOnlyStateButton
                .show()
                .click(() => {
                    askToOverrideReadOnlyState(block);
                });
        } else {
            block.ui.overrideReadOnlyStateButton
                .hide()
                .off('click');
        }
        if (block.orderLine.note) {
            block.ui.orderLineNoteIcon
                .prop('title', abp.utils.replaceAll(block.orderLine.note, '\n', '<br>'))
                .tooltip()
                .show();
        }

        var noteIcons = $();
        var notes = block.orderLine.orderLineTrucks.filter(olt => olt.driverId === block.driverId && olt.driverNote).map(x => x.driverNote);
        notes.forEach(note => {
            let icon = $('<i class="la la-files-o directions-icon" data-toggle="tooltip" data-html="true"></i>');
            icon.prop('title', abp.utils.replaceAll(note, '\n', '<br>'));
            noteIcons = noteIcons.add(icon);
        });
        block.ui.driverNoteIconsContainer.empty().append(noteIcons);
        noteIcons.tooltip();

        _initializing--;
    }

    function getClickableWarningHoverText(block) {
        let warningText = '';
        if (block.orderLine.isMaterialTotalOverridden || block.orderLine.isFreightTotalOverridden) {
            warningText += app.localize('OrderItemHasOverriddenTotalValues') + ': ' + getOverriddenOrderLineTotals(block);
        }
        return warningText;
    }

    function getOverriddenOrderLineTotals(block) {
        let totals = [
            block.orderLine.isMaterialTotalOverridden ? `Material: ${block.orderLine.materialTotal}` : null,
            block.orderLine.isFreightTotalOverridden ? `Freight: ${block.orderLine.freightTotal}` : null,
        ];
        return totals.filter(x => x).join(', ');
    }

    async function askToResetOrderLineOverrideFlags(block) {
        if (!await abp.message.confirm('', app.localize('ClearOverriddenValues{0}Prompt', getOverriddenOrderLineTotals(block)))) {
            return;
        }
        var affectedBlocks = _orderLineBlocks.filter(o => o.orderLine.id === block.orderLine.id);
        try {
            affectedBlocks.forEach(x => x.ui && abp.ui.setBusy(x.ui.card));
            let result = await _orderService.resetOverriddenOrderLineValues({ id: block.orderLine.id });
            block.orderLine.materialTotal = result.materialTotal;
            block.orderLine.freightTotal = result.freightTotal;
            block.orderLine.isFreightTotalOverridden = false;
            block.orderLine.isMaterialTotalOverridden = false;
            affectedBlocks.forEach(function (affectedBlock) {
                updateCardFromModel(affectedBlock);
            });
        }
        finally {
            affectedBlocks.forEach(x => x.ui && abp.ui.clearBusy(x.ui.card));
        }
    }

    async function askToOverrideReadOnlyState(block) {
        if (!await abp.message.confirm('', app.localize('OverrideReadOnlyStateOfOrderLineBlockPrompt'))) {
            return;
        }
        block.overrideReadOnlyState = true;
        updateCardReadOnlyState(block);
        updateCardFromModel(block);
        block.ui.reloadGrid();
    }

    function setInputOrDropdownValue(inputControl, idValue, textValue) {
        if (inputControl.is('select')) {
            abp.helper.ui.addAndSetDropdownValue(inputControl, idValue, textValue);
        } else if (inputControl.is('input')) {
            inputControl.val(textValue);
        }
    }

    function handleBlockDropdownChange(block, dropdown) {
        var newId = Number(dropdown.val()) || null;
        var newName = newId ? dropdown.getSelectedDropdownOption().text() : null;
        var idField = dropdown.attr('name');
        var nameField = dropdown.data('nameField');

        var affectedBlocks = _orderLineBlocks.filter(o => o.orderLine.id === block.orderLine.id);
        affectedBlocks.forEach(function (affectedBlock) {
            affectedBlock.orderLine[idField] = newId;
            if (nameField) {
                affectedBlock.orderLine[nameField] = newName;
            }
            if (affectedBlock !== block) {
                updateCardFromModel(affectedBlock);
            }
        });

        return {
            newId,
            newName
        };
    }

    async function handleBlockNumberChangeAsync(block, input, additionalValidationCallback) {
        var newVal = Number(input.val()) || 0;
        var field = input.attr('name');

        if (input.attr('data-rule-min')) {
            let min = Number(input.attr('data-rule-min'));
            newVal = newVal < min ? min : newVal;
        }
        if (input.attr('data-rule-max')) {
            let max = Number(input.attr('data-rule-max'));
            newVal = newVal > max ? max : newVal;
        }

        if (additionalValidationCallback) {
            if (!await additionalValidationCallback(newVal)) {
                input.val(block.orderLine[field] || 0);
                return null;
            }
        }

        var affectedBlocks = _orderLineBlocks.filter(o => o.orderLine.id === block.orderLine.id);
        affectedBlocks.forEach(function (affectedBlock) {
            affectedBlock.orderLine[field] = newVal;
            updateCardFromModel(affectedBlock);
            refreshFieldHighlighting(affectedBlock);
        });

        return {
            newVal
        };
    }

    function hanldeBlockTextChange(block, input) {
        var newVal = input.val() || '';
        var field = input.attr('name');

        if (input.attr('maxlength')) {
            let maxLength = Number(input.attr('maxlength'));
            if (newVal.length > maxLength) {
                newVal = newVal.substring(0, maxLength);
            }
        }

        var affectedBlocks = _orderLineBlocks.filter(o => o.orderLine.id === block.orderLine.id);
        affectedBlocks.forEach(function (affectedBlock) {
            affectedBlock.orderLine[field] = newVal;
            updateCardFromModel(affectedBlock);
        });

        return {
            newVal
        };
    }

    function refreshFieldHighlighting(block) {
        if (block.ui) {
            let highlightRates = !(block.orderLine.freightRate || block.orderLine.materialRate);
            block.ui.freightRate.add(block.ui.materialRate)
                .toggleClass('highlight-yellow', highlightRates)
                .attr('title', highlightRates ? app.localize('BothRatesArentSet') : '');
        }
    }

    async function reloadBlocksAfterTicketTransfer(sourceBlock, tickets, newDriverId) {
        //reload this grid (should become empty)
        sourceBlock.ui.reloadTickets();
        sourceBlock.ui.updateVisibility();
        //revert driver dropdown value on the source block
        updateCardFromModel(sourceBlock);
        //create a new panel if needed
        populateOrderLineBlocks(null, tickets);

        //destroy an old orderLineBlock if needed
        let sourceOrderLine = _orderLines.find(ol => ol.id === sourceBlock.orderLineId);
        let sourceTickets = getTicketsForOrderLineBlock(sourceBlock);
        let sourceLhBlock = sourceBlock.leaseHaulerBlock;
        if (!sourceOrderLine.orderLineTrucks.some(olt => olt.driverId === sourceBlock.driverId) && !sourceTickets.length) {
            sourceBlock.ui.destroy();
            sourceBlock.ui.card && sourceBlock.ui.card.remove();
            _orderLineBlocks = _orderLineBlocks.filter(b => b !== sourceBlock);
            if (sourceLhBlock) {
                sourceLhBlock.orderLineBlocks = sourceLhBlock.orderLineBlocks.filter(x => x !== sourceBlock);
            }
        }
        if (sourceLhBlock) {
            sourceLhBlock.ui.updateVisibility();
            sourceLhBlock.ui.updateTicketCounters();
        }
        let driver = _drivers.find(d => d.id === newDriverId);
        let leaseHaulerId = driver && driver.leaseHaulerId;
        _leaseHaulerBlocks.filter(lhBlock => lhBlock.leaseHaulerId === leaseHaulerId && lhBlock.knownDriver === !!driver).forEach(lhBlock => {
            lhBlock.ui && lhBlock.ui.initOrderLineBlocks();
            lhBlock.ui && lhBlock.ui.updateVisibility();
            lhBlock.ui && lhBlock.ui.updateTicketCounters();
        });


        renderView();
        //reload the grid of the existing or new panel
        let targetBlock = _orderLineBlocks.find(o => o.orderLine.id === sourceBlock.orderLine.id && (o.driver && o.driver.id || null) === newDriverId);
        if (targetBlock) {
            //it's now possible to transfer orderline blocks between LH blocks (only from Unknown Driver block to other blocks), so we are updating all affected lhBlocks
            //if (targetBlock.leaseHaulerBlock && targetBlock.leaseHaulerBlock.ui.isExpanded) {
            //    targetBlock.leaseHaulerBlock.ui.initOrderLineBlocks();
            //}
            if (targetBlock.leaseHaulerBlock) {
                if (!targetBlock.leaseHaulerBlock.ui.isExpanded) {
                    await targetBlock.leaseHaulerBlock.ui.toggle(true);
                }
            }
            if (targetBlock.ui) {
                targetBlock.ui.reloadTickets();
                await focusOnBlock(targetBlock);
            }
        }
    }

    function sleepAsync(ms) {
        return new Promise((resolve) => setTimeout(() => resolve(), ms));
    }

    async function focusOnBlock(block) {
        if (block.ui) {
            //focusOnPlaceholderOrDropdown(block.ui.driver);

            _initializing++;
            if (block.ui.driver.is('input')) {
                block.ui.driver.focus();
                await sleepAsync(50);
                block.ui.driver.is('select') && block.ui.driver.select2('focus');
            } else if (block.ui.driver.is('select')) {
                block.ui.driver.select2('focus');
            }
            await sleepAsync(50);
            _initializing--;
        }
    }

    function replaceDropdownPlaceholderWithDropdownOnFocus(block, uiField, callback) {
        let ui = block.ui;
        ui[uiField].on('focus', function () {
            ui[uiField] = replaceDropdownPlaceholderWithDropdown(ui[uiField]);
            updateCardFromModel(block);
            callback(ui[uiField]);
            if (!_initializing) {
                ui[uiField].select2('open');
            }
        });
    }

    function updateTicketCounters(ui, tickets, orderLineBlocks) {
        let verifiedCount = tickets.filter(t => t.isVerified).length;
        let enteredCount = tickets.filter(t => !t.isVerified && t.quantity && t.ticketNumber).length;
        let missingCount = tickets.filter(t => !t.isVerified && (!t.quantity || !t.ticketNumber)).length; //tickets.length - verifiedCount - enteredCount;
        setTicketCounterValue(ui.verifiedTicketCount, verifiedCount);
        setTicketCounterValue(ui.enteredTicketCount, enteredCount);
        setTicketCounterValue(ui.missingTicketCount, missingCount);
        if (orderLineBlocks) {
            //leahse hauler grouping level
            let emptyBlockCount = orderLineBlocks.filter(b => getTicketsForOrderLineBlock(b).length === 0).length;
            setTicketCounterValue(ui.emptyOrderLineBlockCount, emptyBlockCount);
        } else {
            //orderline-driver grouping level
            ui.emptyOrderLineBlockCount.toggle(tickets.length === 0);
        }
    }

    function setTicketCounterValue(uiControl, count) {
        uiControl.text(count).toggle(count > 0);
    }

    function getOrderLineBlocksForLeaseHaulerBlock(lhBlock) {
        return _orderLineBlocks.filter(block => block.driver && block.driver.leaseHaulerId === lhBlock.leaseHaulerId && lhBlock.knownDriver
            || !block.driver && lhBlock.leaseHaulerId === null && !lhBlock.knownDriver);
    }

    function updateCardReadOnlyState(block) {
        let orderLineTickets = getTicketsForOrderLine(block.orderLine);
        let isReadOnly = !block.overrideReadOnlyState && orderLineTickets.some(t => t.isReadOnly);
        if (block.isReadOnly === isReadOnly) {
            return;
        }
        block.isReadOnly = isReadOnly;

        let controls = [
            block.ui.driver,
            //block.ui.customer, //always readonly
            //block.ui.orderId, //always readonly
            block.ui.jobNumber,
            block.ui.loadAt,
            block.ui.deliverTo,
            block.ui.item,
            block.ui.uom,
            block.ui.freightRate,
            block.ui.freightRateToPayDrivers,
            block.ui.materialRate
            //block.ui.fuelSurchargeRate //always readonly
        ];
        controls.forEach(c => c.prop('disabled', isReadOnly));

        let blockTickets = getTicketsForOrderLineBlock(block);
        if (block.overrideReadOnlyState && blockTickets.some(t => t.isReadOnly)) {
            blockTickets.forEach(t => t.isReadOnly = false);
            block.ui.reloadGrid();
        }
    }

    async function checkTruckAssignment(block, truckId, driverId) {
        return;
        let noOrderLineTruck = truckId
            && !block.orderLine.orderLineTrucks
                .some(olt => olt.truckId === truckId);
        if (noOrderLineTruck) {
            if (!await abp.message.confirm(app.localize('DriverWasntAssignedToThisTruckAreYouSure'))) {
                throw new Error('Save was cancelled');
            }
            block.orderLine.orderLineTrucks.push({
                truckId: truckId,
                driverId: driverId,
            });
        }
    }

    function isTicketEmpty(ticket) {
        return !ticket.ticketNumber
            && !ticket.ticketDateTime
            && !ticket.quantity
            && !ticket.truckId;
    }

    function getAssignedTruckForBlock(block) {
        let driverId = block.driver && block.driver.id || null;
        if (!driverId) {
            return null;
        }
        let driverAssignment = _driverAssignments.find(x => x.driverId === driverId && x.shift === block.orderLine.shift);
        if (driverAssignment) {
            return _trucks.find(x => x.id === driverAssignment.truckId);
        }
        return _trucks.find(x => x.defaultDriverId === driverId);
    }

    function getEmptyTicket(block) {
        let truck = getAssignedTruckForBlock(block);
        return {
            id: 0,
            orderLineId: block.orderLine.id,
            driverId: block.driver && block.driver.id || null,
            ticketNumber: '',
            ticketDateTime: block.orderLine.orderDate,
            isVerified: false,
            quantity: 0,
            uomName: block.orderLine.uomName,
            uomId: block.orderLine.uomId,
            truckId: truck && truck.id || null,
            truckCode: truck && truck.truckCode || null,
            ticketPhotoId: null,
            receiptLineId: null,
            isReadOnly: false
        };
    }

    function logTimeIfNeeded(message) {
        if (localStorage.getItem('logTicketsByDrivers')) {
            console.log(moment().toISOString() + ' ' + message);
        }
    }

    function renderView() {
        var mainContainer = $('#TicketList');

        logTimeIfNeeded('started rendering');

        if (abp.features.isEnabled('App.AllowLeaseHaulersFeature')) {

            let newLhBlocks = [];
            _leaseHaulerBlocks.forEach(lhBlock => {
                if (lhBlock.ui) {
                    return;
                }
                renderLeaseHaulerBlock(lhBlock);
                newLhBlocks.push(lhBlock);
            });

            logTimeIfNeeded('started to append LH blocks');
            if (newLhBlocks.length) {
                mainContainer.append(newLhBlocks.map(lhBlock => lhBlock.ui.card));
            }

            setTimeout(() => {

                logTimeIfNeeded('(async) started to init missing orderLineBlocks for expanded LH blocks')
                _leaseHaulerBlocks.forEach(lhBlock => lhBlock.ui && lhBlock.ui.isExpanded && lhBlock.ui.initOrderLineBlocks());

                logTimeIfNeeded('(async) started to updateTicketCounters');
                newLhBlocks.forEach(lhBlock => lhBlock.ui.updateTicketCounters());

                if (_leaseHaulerBlocks.length === newLhBlocks.length) {
                    let visibleLhBlocks = newLhBlocks.filter(x => x.ui.isVisible);
                    if (visibleLhBlocks.length === 1 && visibleLhBlocks[0].leaseHaulerId === null && visibleLhBlocks[0].knownDriver) {
                        visibleLhBlocks[0].ui.toggle(true);
                    }
                }

                //logTimeIfNeeded('(async) started to update visibility');
                //newLhBlocks.forEach(function (lhBlock) {
                //    lhBlock.ui.updateVisibility();
                //});
                logTimeIfNeeded('(async) finished async rendering tasks');
            }, 0);
        }
        else {
            renderOrderLineBlocks(_orderLineBlocks, mainContainer);
        }

        updateCurrentFuelCostContainer();

        logTimeIfNeeded('finished rendering');
    }

    function updateCurrentFuelCostContainer() {
        if (!abp.setting.getBoolean('App.Fuel.ShowFuelSurcharge')) {
            $('#CurrentFuelCostContainer').hide();
            return;
        }

        $('#CurrentFuelCostContainer').show();
        $('#CurrentFuelCost').val(getDailyFuelCostFormatted());

        _$currentFuelCostInput.prop("disabled", true);
        $('#EditCurrentFuelCostButton').prop("disabled", false);
    }

    function getDailyFuelCostFormatted() {
        return (_dailyFuelCost && _dailyFuelCost.cost || 0).toFixed(2);
    }

    $('#EditCurrentFuelCostButton').click(function () {
        $(this).prop("disabled", true);
        _$currentFuelCostInput.prop("disabled", false);
        _$currentFuelCostInput.focus();
    });

    _$currentFuelCostInput.blur(async function () {
        let newValue = $(this).val();
        let oldValue = getDailyFuelCostFormatted();

        if (newValue === oldValue) {
            updateCurrentFuelCostContainer();
            return;
        }
        if (!newValue) {
            abp.notify.error('Current Fuel Cost is required!');
            updateCurrentFuelCostContainer();
            return;
        }
        if (isNaN(newValue)) {
            abp.notify.error('Please enter a valid number!');
            updateCurrentFuelCostContainer();
            return;
        }
        if (parseFloat(newValue) < 0) {
            abp.notify.error('Please enter a valid positive number!');
            updateCurrentFuelCostContainer();
            return;
        }

        try {
            abp.ui.setBusy($('#CurrentFuelCostContainer'));
            _dailyFuelCost = await _ticketService.editCurrentFuelCost({
                date: $("#DateFilter").val(),
                cost: newValue
            });
            abp.notify.info('Saved successfully.');
            return reloadAllData();
        }
        finally {
            abp.ui.clearBusy($('#CurrentFuelCostContainer'));
            updateCurrentFuelCostContainer();
        }

    });

    function renderOrderLineBlocks(orderLineBlocks, parentContainer) {
        logTimeIfNeeded('started rendering orderLineBlocks');

        let newBlocks = [];

        orderLineBlocks.forEach(function (block) {
            if (block.ui) {
                return;
            }
            renderOrderLineBlock(block);
            newBlocks.push(block);
        });

        logTimeIfNeeded('started to append');
        if (newBlocks.length) {
            parentContainer.append(newBlocks.map(block => block.ui.card));
        }

        setTimeout(() => {
            logTimeIfNeeded('(async) started to updateTicketCounters');
            newBlocks.forEach((block, i) => block.ui.updateTicketCounters(i === 0));

            logTimeIfNeeded('(async) finished async rendering tasks');
        }, 0);
    }

    function renderLeaseHaulerBlock(lhBlock) {
        var ui = {
        };
        lhBlock.ui = ui;

        ui.card = $('<div class="card card-collapsable card-collapse bg-superlight mb-4">').append(
            $('<div class="card-header bg-superlight">').append(
                $('<div class="m-form m-form--label-align-right">').append(
                    $('<div class="row align-items-center">').append(
                        $('<div class="col-lg-3 col-md-4 col-sm-6 pt-2">').append(
                            $('<h5>').text(lhBlock.leaseHaulerName)
                        )
                    ).append(
                        $('<div class="col-lg-7 col-md-4 col-sm-10 col-8 pb-2">').append(
                            renderTicketCounts(ui, 'leaseHauler')
                        )
                    ).append(
                        renderToggleButton(ui)
                    )
                )
            )
        ).append(
            ui.body = $('<div class="card-body pb-0" style="display: none">')
        );


        lhBlock.ui.updateTicketCounters = function () {
            let tickets = getTicketsForLeaseHaulerBlock(lhBlock);
            updateTicketCounters(lhBlock.ui, tickets, lhBlock.orderLineBlocks.filter(b => !_driverIdFilter || _driverIdFilter === b.driverId));
        };

        lhBlock.orderLineBlocks = getOrderLineBlocksForLeaseHaulerBlock(lhBlock);
        lhBlock.orderLineBlocks.forEach(block => block.leaseHaulerBlock = lhBlock);
        lhBlock.ui.hasBeenExpanded = false;

        lhBlock.ui.initOrderLineBlocks = function () {
            let moreOrderLineBlocks = getOrderLineBlocksForLeaseHaulerBlock(lhBlock);
            moreOrderLineBlocks.forEach(block => {
                if (!lhBlock.orderLineBlocks.includes(block)) {
                    lhBlock.orderLineBlocks.push(block);
                    block.leaseHaulerBlock = lhBlock;
                }
            });
            renderOrderLineBlocks(lhBlock.orderLineBlocks, lhBlock.ui.body);
        };

        lhBlock.ui.isExpanded = false;
        var slideIsInProgress = false;
        let togglePromiseResolves = [];
        lhBlock.ui.toggle = function (isExpanded) {
            return new Promise((resolve) => {
                togglePromiseResolves.push(resolve);
                if (slideIsInProgress) {
                    return;
                }
                if (isExpanded === undefined) {
                    isExpanded = !lhBlock.ui.isExpanded;
                }

                if (lhBlock.ui.isExpanded === isExpanded) {
                    return;
                }

                slideIsInProgress = true;
                let hasBeenExpanded = lhBlock.ui.hasBeenExpanded;
                lhBlock.ui.isExpanded = isExpanded;

                var card = lhBlock.ui.card;
                if (isExpanded) { //card.hasClass('card-collapse')
                    lhBlock.ui.hasBeenExpanded = true;

                    var slideDown = function () {
                        logTimeIfNeeded('(async) started slide down');
                        lhBlock.ui.body.slideDown({
                            complete: () => {
                                logTimeIfNeeded('(async) finished slide down');
                                slideIsInProgress = false;
                                let resolvers = togglePromiseResolves;
                                togglePromiseResolves = [];
                                resolvers.forEach(x => x());
                            }
                        });
                    };
                    var initOrderLineBlocks = function () {
                        logTimeIfNeeded('started to init order line blocks for LH');
                        lhBlock.ui.initOrderLineBlocks();
                    };
                    var setBusyIfNeededAnd = function (callback) {
                        if (!hasBeenExpanded && lhBlock.orderLineBlocks.length > 20) {
                            abp.ui.setBusy(lhBlock.ui.card);
                            callback && setTimeout(callback, 200);
                        } else {
                            callback && callback();
                        }
                    };
                    var clearBusyIfNeededAnd = function (callback) {
                        if (!hasBeenExpanded) {
                            setTimeout(() => {
                                abp.ui.clearBusy(lhBlock.ui.card);
                                callback && callback();
                            }, 0);
                        } else {
                            callback && callback();
                        }
                    }

                    logTimeIfNeeded('started expanding order line block, set busy');
                    setBusyIfNeededAnd(() => {
                        initOrderLineBlocks();
                        setTimeout(slideDown, 0);
                        clearBusyIfNeededAnd(() => {
                            logTimeIfNeeded('finished initializing order line blocks for LH, clearing busy');
                        });
                    });

                } else {
                    lhBlock.ui.isExpanded = false;
                    lhBlock.ui.body.slideUp({
                        complete: () => {
                            slideIsInProgress = false;
                            let resolvers = togglePromiseResolves;
                            togglePromiseResolves = [];
                            resolvers.forEach(x => x());
                        }
                    });
                }
                card.toggleClass('card-collapse', !isExpanded);
            });
        }

        lhBlock.ui.toggleCardDetailsButton.click(function (e) {
            e.preventDefault();
            lhBlock.ui.toggle();
        });

        lhBlock.ui.isVisible = true;
        lhBlock.ui.updateVisibility = function () {
            let hasChildren = lhBlock.orderLineBlocks && lhBlock.orderLineBlocks.filter(block => !_driverIdFilter || block.driverId === _driverIdFilter).length > 0 || false;
            lhBlock.ui.isVisible = hasChildren;
            lhBlock.ui.card.toggle(hasChildren);
        };

        lhBlock.ui.updateVisibility();
    }

    function renderOrderLineBlock(block) {
        var ui = {
        };
        block.ui = ui;

        ui.card = $('<div class="card card-collapsable card-collapse bg-light mb-4">').append(
            $('<div class="card-header bg-light">').append(
                $('<div class="m-form m-form--label-align-right">').append(
                    ui.form = $('<form>')
                )
            )
        ).append(
            ui.body = $('<div class="card-body py-0" style="display: none">').append(
                ui.table = $('<table class="table table-striped table-bordered table-hover order-line-tickets-table"></table>')
            ).append(
                $('<div class="row">').append(
                    $('<div class="col-sm-12 d-flex justify-content-end mb-3">').append(
                        ui.addTicketRowButton = $('<button type="button" class="btn btn-primary">').text(app.localize('Add'))
                    )
                )
            )
        );

        ui.form.append(
            $('<div class="d-flex justify-content-end"></div>').append(
                renderOrderLineNoteIcon(ui),
                renderOverrideReadOnlyStateButton(ui)
            )
        ).append(
            $('<div class="row align-items-center">').append(
                renderDropdownPlaceholder(ui, 'driver', app.localize('Driver'), 'driverId')
            ).append(
                renderDisabledInput(ui, 'customer', app.localize('Customer'), 'customerId', 'customerName')
            ).append(
                renderDisabledInput(ui, 'orderId', app.localize('OrderId'), 'orderId', '')
            ).append(
                renderTextInput(ui, 'jobNumber', app.localize('JobNbr'), 'jobNumber', abp.entityStringFieldLengths.orderLine.jobNumber)
            ).append(
                renderDropdownPlaceholder(ui, 'loadAt', app.localize('LoadAt'), 'loadAtId', 'loadAtName')
            ).append(
                renderDropdownPlaceholder(ui, 'deliverTo', app.localize('DeliverTo'), 'deliverToId', 'deliverToName')
            ).append(
                renderDropdownPlaceholder(ui, 'item', app.localize('Item'), 'serviceId', 'serviceName')
            ).append(
                renderDropdownPlaceholder(ui, 'uom', app.localize('UOM'), 'uomId', 'uomName')
            ).append(
                renderRateInput(ui, 'freightRate', app.localize('FreightRate'), 'freightRate')
            ).append(
                renderRateInput(ui, 'freightRateToPayDrivers', app.localize('FreightRateToPayDriversShort'), 'freightRateToPayDrivers')
                    .toggle(abp.setting.getBoolean('App.TimeAndPay.AllowDriverPayRateDifferentFromFreightRate'))
            ).append(
                renderRateInput(ui, 'materialRate', app.localize('MaterialRate'), 'materialRate')
            ).append(
                renderDisabledInput(ui, 'fuelSurchargeRate', app.localize('FuelSurchargeRate'), 'fuelSurchargeRate', '')
                    .toggle(abp.setting.getBoolean('App.Fuel.ShowFuelSurcharge'))
            ).append(
                $('<div class="form-group col-lg-2 col-md-4 col-sm-1 d-sm-none-">').append(
                    renderClickableWarningIcon(ui)
                ).append(
                    renderDriverNoteIconsContainer(ui)
                )
            )
        ).append(
            $('<div class="row align-items-center">').append(
                $('<div class="col-lg-10 col-md-8 col-sm-10 col-8 pb-2">').append(
                    renderTicketCounts(ui, 'orderLine')
                )
            ).append(
                renderToggleButton(ui)
            )
        );

        updateCardFromModel(block);
        refreshFieldHighlighting(block);

        replaceDropdownPlaceholderWithDropdownOnFocus(block, 'driver', dropdown => {
            let leaseHaulerId = (block.driver && block.driver.leaseHaulerId) || null;
            block.ui.driver.append(
                _drivers
                    .filter(d => d.isActive
                        && (d.leaseHaulerId === leaseHaulerId || !block.driver)
                        && d.id !== (block.driver && block.driver.id))
                    .map(d => $('<option>').attr('value', d.id).text(d.name))
                    .reduce((prev, curr) => prev ? prev.add(curr) : curr, $())
                //.forEach(d => block.ui.driver.append(d));
            ).select2Init({
                showAll: true,
                allowClear: false
            }).change(function () {
                if (_initializing) {
                    return;
                }
                var newDriverId = Number(block.ui.driver.val()) || null;
                //var newDriver = newDriverId ? _drivers.find(d => d.id === newDriverId) : null;
                let tickets = getTicketsForOrderLineBlock(block).filter(t => !isTicketEmpty(t));
                tickets.forEach(t => t.driverId = newDriverId);
                saveChanges({
                    tickets: tickets
                }).then((saveResult) => {
                    reloadBlocksAfterTicketTransfer(block, tickets, newDriverId);
                });
            });
        });

        //changing the customer was disallowed in #10977
        //replaceDropdownPlaceholderWithDropdownOnFocus(block, 'customer', dropdown => {
        //    block.ui.customer.select2Init({
        //        abpServiceMethod: abp.services.app.customer.getActiveCustomersSelectList,
        //        allowClear: false
        //    }).change(function () {
        //        if (_initializing) {
        //            return;
        //        }
        //        var { newId, newName } = handleBlockDropdownChange(block, $(this));
        //        var orderId = block.orderLine.orderId;
        //        var affectedBlocks = _orderLineBlocks.filter(o => o.orderLine.orderId === orderId);
        //        affectedBlocks.forEach(function (affectedBlock) {
        //            if (affectedBlock !== block && affectedBlock.orderLineId !== block.orderLineId) {
        //                affectedBlock.orderLine.customerId = newId;
        //                affectedBlock.orderLine.customerName = newName;
        //                updateCardFromModel(affectedBlock);
        //            }
        //        });
        //        saveChanges({
        //            orderLines: [block.orderLine]
        //        });
        //    });
        //});

        replaceDropdownPlaceholderWithDropdownOnFocus(block, 'loadAt', dropdown => {
            block.ui.loadAt.select2Location({
                predefinedLocationCategoryKind: abp.enums.predefinedLocationCategoryKind.unknownLoadSite,
                showAll: false,
                allowClear: true
            }).change(function () {
                if (_initializing) {
                    return;
                }
                handleBlockDropdownChange(block, $(this));
                saveChanges({
                    orderLines: [block.orderLine]
                });
            });
        });

        replaceDropdownPlaceholderWithDropdownOnFocus(block, 'deliverTo', dropdown => {
            block.ui.deliverTo.select2Location({
                predefinedLocationCategoryKind: abp.enums.predefinedLocationCategoryKind.unknownDeliverySite,
                showAll: false,
                allowClear: true
            }).change(function () {
                if (_initializing) {
                    return;
                }
                handleBlockDropdownChange(block, $(this));
                saveChanges({
                    orderLines: [block.orderLine]
                });
            });
        });

        replaceDropdownPlaceholderWithDropdownOnFocus(block, 'item', dropdown => {
            block.ui.item.select2Init({
                abpServiceMethod: abp.services.app.service.getServicesWithTaxInfoSelectList,
                showAll: true,
                allowClear: false
            }).change(function () {
                if (_initializing) {
                    return;
                }
                handleBlockDropdownChange(block, $(this));
                saveChanges({
                    orderLines: [block.orderLine]
                });
            });
        });

        replaceDropdownPlaceholderWithDropdownOnFocus(block, 'uom', dropdown => {
            block.ui.uom.select2Uom().change(function () {
                if (_initializing) {
                    return;
                }
                var oldName = block.orderLine.uomName;
                var { newId, newName } = handleBlockDropdownChange(block, $(this));
                showUomWarningIfNeeded(oldName, newName);
                var orderLineId = block.orderLine.id;
                var affectedTickets = _tickets.filter(t => t.orderLineId === orderLineId);
                affectedTickets.forEach(function (affectedTicket) {
                    affectedTicket.uomId = newId;
                    affectedTicket.uomName = newName;
                });
                var affectedBlocks = _orderLineBlocks.filter(o => o.orderLine.id === orderLineId);
                affectedBlocks.forEach(b => b.ui && b.ui.reloadGrid());
                saveChanges({
                    orderLines: [block.orderLine]
                });
            });
        });

        //should work for all orderline text fields
        block.ui.jobNumber.focusout(function () {
            if (_initializing) {
                return;
            }
            var field = $(this).attr('name');
            if ($(this).val() === block.orderLine[field]) {
                return;
            }
            hanldeBlockTextChange(block, $(this));
            saveOrderLine(block.orderLine);
        });

        block.ui.freightRate.add(
            block.ui.freightRateToPayDrivers
        ).add(
            block.ui.materialRate
        ).focusout(async function () {
            if (_initializing) {
                return;
            }
            var input = $(this);
            var field = input.attr('name');

            var additionalValidationCallback = async (newValue) => {
                if (newValue === block.orderLine[field]) {
                    return false;
                }
                if (!await validateNewRateAsync(newValue, field, input, block)) {
                    return false;
                }
                await syncFreightRateToPayDriversIfNeeded(newValue, field, input, block);
                return true;
            };

            var result = await handleBlockNumberChangeAsync(block, input, additionalValidationCallback);
            if (!result) {
                return;
            }
            saveOrderLine(block.orderLine);
        });

        var setDesignationForBlock = function (block, newDesignation) {
            block.orderLine.designation = newDesignation;
            var affectedBlocks = _orderLineBlocks.filter(o => o.orderLine.id === block.orderLine.id);
            affectedBlocks.forEach(function (affectedBlock) {
                affectedBlock.orderLine.designation = block.orderLine.designation;
            });
        };

        var validateNewRateAsync = async function (newValue, field, input, block) {
            let oldValue = block.orderLine[field] || 0;
            if (field === 'freightRateToPayDrivers') {
                let freightRate = block.orderLine.freightRate || 0;
                if (newValue && !oldValue && !freightRate) {
                    abp.message.error('Freight rate has to be specified first');
                    return false;
                }
                return true;
            }
            let fieldIsFreight = field === 'freightRate';
            let otherField = fieldIsFreight ? 'materialRate' : 'freightRate';
            let otherFieldDisplayName = fieldIsFreight ? 'material' : 'freight';
            let fieldDisplayName = fieldIsFreight ? 'freight' : 'material';
            let otherValue = block.orderLine[otherField] || 0;
            if (oldValue && !newValue) {
                if (!otherValue) {
                    //the designation can't be changed to 'nothing' when neither rate is specified
                    abp.message.error('At least one rate has to be specified');
                    return false;
                } else {
                    let allowedDesignations = fieldIsFreight ? abp.enums.designations.materialOnly : abp.enums.designations.freightOnly;
                    if (allowedDesignations.includes(block.orderLine.designation)) {
                        return true;
                    }
                    if (!await abp.message.confirm(`Changing this ${fieldDisplayName} rate to zero will change the designation to ${otherFieldDisplayName} only. Is this what you want to do?`)) {
                        return false;
                    }
                    setDesignationForBlock(block, fieldIsFreight ? abp.enums.designation.materialOnly : abp.enums.designation.freightOnly);
                    return true;
                }
            } else if (!oldValue && newValue) {
                let allowedDesignations = [];
                var newDesignation;
                var newDesignationDisplayValue;
                if (!otherValue) {
                    allowedDesignations = fieldIsFreight ? abp.enums.designations.freightOnly : abp.enums.designations.materialOnly;
                    newDesignation = fieldIsFreight ? abp.enums.designation.freightOnly : abp.enums.designation.materialOnly;
                    newDesignationDisplayValue = fieldIsFreight ? "freight only" : "material only";
                } else {
                    allowedDesignations = abp.enums.designations.freightAndMaterial;
                    newDesignation = abp.enums.designation.freightAndMaterial;
                    newDesignationDisplayValue = "freight and material";
                }
                if (allowedDesignations.includes(block.orderLine.designation)) {
                    return true;
                }
                if (!await abp.message.confirm(`Adding a ${fieldDisplayName} rate will change this designation to ${newDesignationDisplayValue}. Is this what you want to do?`)) {
                    return false;
                }
                setDesignationForBlock(block, newDesignation);
                return true;
            }

            return true;
        };

        var syncFreightRateToPayDriversIfNeeded = async function (newValue, field, input, block) {
            let oldValue = block.orderLine[field] || 0;
            if (field === 'freightRate') {
                if (!newValue
                    || oldValue === (block.orderLine.freightRateToPayDrivers)
                    || !abp.setting.getBoolean('App.TimeAndPay.AllowDriverPayRateDifferentFromFreightRate')
                ) {
                    block.orderLine.freightRateToPayDrivers = newValue;
                    var affectedBlocks = _orderLineBlocks.filter(o => o.orderLine.id === block.orderLine.id);
                    affectedBlocks.forEach(function (affectedBlock) {
                        affectedBlock.orderLine.freightRateToPayDrivers = block.orderLine.freightRateToPayDrivers;
                    });
                }
            }
        };

        block.ui.destroyGrid = function () {
            if (!block.ui.grid) {
                return;
            }
            block.ui.grid.destroy();
            block.ui.grid = null;
        };

        let saveQueueForIsVerified = new _dtHelper.editors.DelayedSaveQueue({
            delay: 200,
            setBusyOnSave: true,
            saveCallback: async function (updatedRows, cells) {
                await _ticketService.setIsVerifiedForTickets(updatedRows).then(() => {
                    abp.notify.info('Successfully saved');
                }, (e) => reloadAllDataAndThrow(e));
            }
        });

        let refreshTicketDateWarning = function (ticket, cell) {
            let orderDateMoment = moment(block.orderLine.orderDate, ['YYYY-MM-DDTHH:mm:ss']);
            let ticketDateMoment = ticket.ticketDateTime && moment(ticket.ticketDateTime, ['YYYY-MM-DDTHH:mm:ss']);
            if (ticketDateMoment && !orderDateMoment.isSame(ticketDateMoment, 'day')) {
                if (cell.find('.input-validation-icon').length) {
                    return;
                }
                cell.find('input')
                    .addClass('padding-left-25')
                    .parent()
                    .addClass('input-validation-icon-container')
                    .append(
                        $('<i class="la la-exclamation-circle text-danger input-validation-icon">').attr('title', app.localize('TicketDateIsDifferentFromOrder'))
                    );
            } else {
                cell.find('input')
                    .removeClass('padding-left-25')
                    .parent()
                    .removeClass('input-validation-icon-container');
                cell.find('.input-validation-icon').remove();
            }
        }

        block.ui.initGrid = function () {
            if (block.ui.grid) {
                return;
            }
            block.ui.grid = block.ui.table.DataTableInit({
                paging: false,
                info: false,
                ordering: false,
                language: {
                    emptyTable: app.localize("NoDataInTheTableClickAddBelow")
                },
                ajax: function (data, callback, settings) {
                    var tickets = getTicketsForOrderLineBlock(block);
                    callback(_dtHelper.fromAbpResult({
                        items: tickets,
                        totalCount: tickets.length
                    }));
                },
                editable: {
                    saveCallback: async function (rowData, cell) {
                        block.ui.updateTicketCounters();
                        let oldId = rowData.id;
                        await saveTicket(rowData);
                        if (rowData.id !== oldId) {
                            $(cell).closest('tr').find('td.actions').html(getTicketGridActionButton(rowData));
                        }
                    },
                    isReadOnly: (rowData) => rowData.isReadOnly
                },
                columns: [
                    {
                        width: '20px',
                        className: 'control responsive',
                        orderable: false,
                        render: function () { return ''; }
                    },
                    {
                        data: "isVerified",
                        title: "Verified",
                        editable: {
                            editor: _dtHelper.editors.checkbox,
                            isReadOnly: (rowData, rowIsReadOnly) => false, //the column is always editable, ignore the row's readonly state
                            saveCallback: function (rowData, cell) { //override grid's saveCallback
                                block.ui.updateTicketCounters();
                                if (rowData.id) {
                                    saveQueueForIsVerified.add(rowData, cell);
                                }
                            },
                            addHeaderCheckbox: true
                        }
                    },
                    {
                        data: "ticketDateTime",
                        title: "Time",
                        render: function (data, type, full, meta) { return _dtHelper.renderActualUtcDateTime(full.ticketDateTime, ''); },
                        className: "all",
                        width: "170px",
                        editable: {
                            editor: _dtHelper.editors.datetime,
                            editCompleteCallback: function (editResult, rowData, cell) {
                                refreshTicketDateWarning(rowData, cell);
                            }
                            //this was needed for "time" editor, not needed for "datetime" editor
                            //convertDisplayValueToData: function (displayValue, rowData) {
                            //    let ticket = rowData;
                            //    let newTime = moment(displayValue, ['YYYY-MM-DDTHH:mm:ss', 'hh:mm A']);
                            //    let dateToUse = ticket.ticketDateTime ? moment(ticket.ticketDateTime, ['YYYY-MM-DDTHH:mm:ss']).startOf('day') : moment(block.orderLine.orderDate, ['YYYY-MM-DDTHH:mm:ss']);
                            //    let newValue = dateToUse.set({
                            //        hour: newTime.hour(),
                            //        minute: newTime.minute()
                            //    });
                            //    return newValue.format('YYYY-MM-DDTHH:mm:ss');
                            //}
                        },
                        createdCell: function (cell, cellData, rowData, rowIndex, colIndex) {
                            refreshTicketDateWarning(rowData, $(cell));
                            $(cell).css('min-width', '170px'); //default is 150px and then we need to leave space for the warning icon
                        }
                    },
                    {
                        data: "ticketNumber",
                        title: "Ticket Number",
                        className: "cell-text-wrap all",
                        editable: {
                            editor: _dtHelper.editors.text,
                            required: true,
                            maxLength: 20
                        }
                    },
                    {
                        data: "quantity",
                        title: "Quantity",
                        className: "all",
                        editable: {
                            editor: _dtHelper.editors.quantity
                        }
                    },
                    {
                        data: "uomName",
                        title: "UOM"
                    },
                    {
                        data: "truckCode",
                        title: "Truck",
                        width: "90px",
                        className: "all",
                        editable: {
                            editor: _dtHelper.editors.dropdown,
                            idField: 'truckId',
                            nameField: 'truckCode',
                            dropdownOptions: {
                                abpServiceMethod: abp.services.app.truck.getTrucksSelectList,
                                abpServiceParams: {
                                    allOffices: true,
                                    includeLeaseHaulerTrucks: true,
                                    activeOnly: true,
                                    //orderLineId: _validateTrucksAndDrivers ? _orderLineId : null
                                },
                                showAll: false,
                                allowClear: false
                            },
                            validate: async function (rowData, newId) {
                                try {
                                    newId = Number(newId) || null;
                                    await checkTruckAssignment(block, newId, rowData.driverId);
                                }
                                catch {
                                    return false;
                                }
                                return true;
                            },
                        }
                    },
                    {
                        data: 'ticketPhotoId',
                        width: "10px",
                        className: "all",
                        render: function (data, type, full, meta) {
                            return full.ticketPhotoId ? '<i class="la la-file-image-o showTicketPhotoButton"></i>' : '';
                        }
                    },
                    {
                        data: null,
                        orderable: false,
                        responsivePriority: 1,
                        name: "Actions",
                        width: "10px",
                        className: "actions all",
                        render: function (data, type, full, meta) {
                            return getTicketGridActionButton(full);
                        }
                    }
                ]
            });

        };

        function getTicketGridActionButton(ticket) {
            let uploadButtonCaption = ticket.ticketPhotoId ? 'Replace image' : 'Add image';
            return '<div class="dropdown action-button">'
                + '<ul class="dropdown-menu dropdown-menu-right">'
                + (ticket.isReadOnly || !ticket.id ? '' : '<li><a class="btnChangeDriver dropdown-item"><i class="fas fa-user-friends"></i> Change driver</a></li>')
                + (ticket.isReadOnly || !ticket.id ? '' : '<li><a class="btnChangeDate dropdown-item"><i class="fas fa-calendar"></i> Change ticket date</a></li>')
                + `<li><a class="btnUploadTicketPhotoForRow dropdown-item"><i class="la la-file-image-o"></i> ${uploadButtonCaption}</a></li>`
                + (ticket.ticketPhotoId ? '<li><a class="showTicketPhotoButton dropdown-item"><i class="la la-file-image-o"></i> View image</a></li>' : '')
                + (ticket.ticketPhotoId ? '<li><a class="btnDeleteTicketPhotoForRow dropdown-item"><i class="la la-file-image-o"></i> Delete image</a></li>' : '')
                + (ticket.isReadOnly ? '' : '<li><a class="btnDeleteRow dropdown-item" title="Delete"><i class="fa fa-trash"></i> Delete entire ticket</a></li>')
                + '</ul>'
                + '<button class="btn btn-primary btn-sm" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-ellipsis-h"></i></button>'
                + '</div>';
        }

        block.ui.reloadGrid = function () {
            return new Promise(resolve => {
                if (block.ui.grid) {
                    block.ui.grid.ajax.reload(() => resolve(), /*resetPaging*/ false);
                } else {
                    resolve();
                }
            });
        };

        block.ui.updateTicketCounters = function (updateParent) {
            if (updateParent === undefined) {
                updateParent = true;
            }
            let tickets = getTicketsForOrderLineBlock(block);
            updateTicketCounters(block.ui, tickets);
            if (block.leaseHaulerBlock && updateParent) {
                block.leaseHaulerBlock.ui.updateTicketCounters();
            }
        };

        block.ui.reloadTickets = function () {
            block.ui.reloadGrid();
            block.ui.updateTicketCounters();
        };

        var slideIsInProgress = false;
        block.ui.toggleCardDetailsButton.click(function (e) {
            e.preventDefault();
            if (slideIsInProgress) {
                return;
            }
            slideIsInProgress = true;
            var card = block.ui.card;
            if (card.hasClass('card-collapse')) {
                block.ui.initGrid();
                block.ui.body.slideDown({
                    complete: () => {
                        slideIsInProgress = false;
                    }
                });
            } else {
                block.ui.body.slideUp({
                    complete: () => {
                        block.ui.destroyGrid();
                        slideIsInProgress = false;
                    }
                });
            }
            card.toggleClass('card-collapse');
        });

        block.ui.addTicketRow = async function (focusOnCell) {
            if (block.orderLine.isMaterialTotalOverridden || block.orderLine.isFreightTotalOverridden) {
                let tickets = getTicketsForOrderLineBlock(block);
                if (tickets.length >= 1) {
                    abp.message.error(app.localize('OrderLineWithOverriddenTotalCanOnlyHaveSingleTicketError'));
                    return;
                }
            }

            _tickets.push(getEmptyTicket(block));
            await block.ui.reloadGrid();
            block.ui.updateTicketCounters();
            if (focusOnCell) {
                block.ui.table.find('tbody tr').last().find('td.cell-editable').first().find('input').focus();
            }
        };

        block.ui.addTicketRowButton.click(function () {
            block.ui.addTicketRow(true);
        });

        block.ui.table.on('keydown', '.dropdown.action-button', function (e) {
            var charCode = e.which || e.keyCode;
            if (charCode === 9) {
                var row = $(this).closest('tr');
                var rows = row.closest('tbody').find('tr');
                if (rows.index(row) === rows.length - 1) {
                    e.preventDefault();
                    block.ui.addTicketRow(true);
                }
            }
        });

        block.ui.table.on('click', '.btnDeleteRow', async function (e) {
            e.preventDefault();
            var ticket = _dtHelper.getRowData(this);
            if (ticket.receiptLineId) {
                abp.message.error('You can\'t delete tickets associated with receipts');
                return;
            }
            if (await abp.message.confirm('Are you sure you want to delete the ticket?')) {
                if (ticket.id) {
                    await _ticketService.deleteTicket({ id: ticket.id });
                }
                _tickets.splice(_tickets.indexOf(ticket), 1);
                block.ui.reloadTickets();
                abp.notify.info('Successfully deleted.');
            }
        });

        block.ui.table.on('click', '.btnUploadTicketPhotoForRow', async function (e) {
            e.preventDefault();
            var ticket = _dtHelper.getRowData(this);
            //we'll save the ticket later, after they select the file
            //if (!ticket.id) {
            //    await saveTicket(ticket);
            //}
            _ticketForPhotoUpload = ticket;
            _blockForPhotoUpload = block;
            _$ticketPhotoInput.click();
        });

        block.ui.table.on('click', '.btnDeleteTicketPhotoForRow', async function (e) {
            e.preventDefault();
            var ticket = _dtHelper.getRowData(this);
            if (!await abp.message.confirm('Are you sure you want to delete the image?')) {
                return;
            }

            abp.ui.setBusy(block.ui.card);

            _ticketService.deleteTicketPhoto({
                ticketId: ticket.id
            }).done(function () {
                ticket.ticketPhotoId = null;
                block.ui.reloadGrid();
            }).always(function () {
                abp.ui.clearBusy(block.ui.card);
            });
        });

        block.ui.table.on('click', '.showTicketPhotoButton', function (e) {
            e.preventDefault();
            var ticket = _dtHelper.getRowData(this);
            let url = abp.appPath + 'app/Tickets/GetTicketPhoto/' + ticket.id;
            window.open(url);
        });

        block.ui.table.on('click', '.btnChangeDriver', function (e) {
            e.preventDefault();
            var ticket = _dtHelper.getRowData(this);
            _selectDriverModal.open({}).done(function (modal, modalObject) {
                let leaseHaulerId = (block.driver && block.driver.leaseHaulerId) || null;
                modalObject.setDrivers(_drivers.filter(d => d.isActive && (d.leaseHaulerId === leaseHaulerId || !block.driver)));
                modalObject.saveCallback = async function (modalResult) {
                    let newDriverId = modalResult.driverId;
                    let newDriver = modalResult.driver;
                    if (!newDriverId || !newDriver || newDriverId === ticket.driverId) {
                        return;
                    }
                    await checkTruckAssignment(block, ticket.truckId, newDriverId);

                    ticket.driverId = newDriverId;
                    return saveTicket(ticket).then((saveResult) => {
                        reloadBlocksAfterTicketTransfer(block, [ticket], newDriverId);
                    });
                };
            });
        });

        block.ui.table.on('click', '.btnChangeDate', function (e) {
            e.preventDefault();
            var ticket = _dtHelper.getRowData(this);
            _selectDateModal.open({}).done(function (modal, modalObject) {
                modalObject.setDate(moment(ticket.ticketDateTime, ['YYYY-MM-DDTHH:mm:ss']).format('L'));
                modalObject.saveCallback = async function (modalResult) {
                    if (!modalResult.date) {
                        return;
                    }

                    let ticketDateTime = moment(ticket.ticketDateTime || block.orderLine.orderDate, ['YYYY-MM-DDTHH:mm:ss']);
                    let newDate = moment(modalResult.date, ['L', 'YYYY-MM-DDTHH:mm:ss']);

                    if (ticketDateTime.isSame(newDate, 'day')) {
                        return;
                    }

                    let newValue = newDate.set({
                        hour: ticketDateTime.hour(),
                        minute: ticketDateTime.minute()
                    });

                    ticket.ticketDateTime = newValue.format('YYYY-MM-DDTHH:mm:ss');
                    return saveTicket(ticket).then((saveResult) => {
                        block.ui.reloadGrid();
                    });
                };
            });
        });

        block.ui.updateVisibility = function () {
            block.ui.card.toggle(!_driverIdFilter || block.driverId === _driverIdFilter);
        };

        block.ui.destroy = function () {
            block.ui.destroyGrid();
            let controls = [
                block.ui.driver,
                block.ui.customer,
                block.ui.loadAt,
                block.ui.deliverTo,
                block.ui.item,
                block.ui.uom
            ];
            controls.forEach(control => {
                if (control.is('select')) {
                    control.select2('destroy');
                }
            });
        }

        updateCardReadOnlyState(block);
        block.ui.updateVisibility();
    }

    function renderDisabledInput(ui, uiField, labelText, idField, nameField) {
        let id = abp.helper.getUniqueElementId();
        var result = $('<div class="form-group col-lg-3 col-md-4 col-sm-6">').append(
            $('<label class="control-label">').attr('for', id).text(labelText)
        ).append(
            ui[uiField] = $('<input type="text" class="form-control select2-placeholder-input-control">').attr('name', idField).attr('id', id).prop('disabled', true)
        );
        if (nameField) {
            ui[uiField].data('nameField', nameField);
        }

        return result;
    }

    function renderDropdownPlaceholder(ui, uiField, labelText, idField, nameField) {
        let id = abp.helper.getUniqueElementId();
        var result = $('<div class="form-group col-lg-3 col-md-4 col-sm-6">').append(
            $('<label class="control-label">').attr('for', id).text(labelText)
        ).append(
            ui[uiField] = $('<input type="text" class="form-control select2-placeholder-input-control">').attr('name', idField).attr('id', id)
        );
        if (nameField) {
            ui[uiField].data('nameField', nameField);
        }

        return result;
    }

    function replaceDropdownPlaceholderWithDropdown(placeholderControl) {
        let newControl = $('<select class="form-control">').attr('name', placeholderControl.attr('name')).attr('id', placeholderControl.attr('id')).append(
            $('<option>').attr('value', '').html('&nbsp;')
        );
        newControl.prop('disabled', placeholderControl.prop('disabled'));
        if (placeholderControl.data('nameField')) {
            newControl.data('nameField', placeholderControl.data('nameField'));
        }
        placeholderControl.replaceWith(newControl);
        return newControl;
    }

    function renderDropdown(ui, uiField, labelText, idField, nameField) {
        let id = abp.helper.getUniqueElementId();
        var result = $('<div class="form-group col-lg-3 col-md-4 col-sm-6">').append(
            $('<label class="control-label">').attr('for', id).text(labelText)
        ).append(
            ui[uiField] = $('<select class="form-control">').attr('name', idField).attr('id', id).append(
                $('<option>').attr('value', '').html('&nbsp;')
            )
        );
        if (nameField) {
            ui[uiField].data('nameField', nameField);
        }
        return result;
    }

    function renderRateInput(ui, uiField, labelText, nameOnForm) {
        let id = abp.helper.getUniqueElementId();
        return $('<div class="form-group col-lg-3 col-md-4 col-sm-6">').append(
            $('<label class="control-label">').attr('for', id).text(labelText).css('white-space', 'nowrap')
        ).append(
            ui[uiField] = $('<input class="form-control" type="text" data-rule-number="true" data-rule-min="0">').attr('data-rule-max', app.consts.maxDecimal).attr('name', nameOnForm).attr('id', id)
        );
    }

    function renderTextInput(ui, uiField, labelText, nameOnForm, maxlength) {
        let id = abp.helper.getUniqueElementId();
        var result = $('<div class="form-group col-lg-3 col-md-4 col-sm-6">').append(
            $('<label class="control-label">').attr('for', id).text(labelText)
        ).append(
            ui[uiField] = $('<input type="text" class="form-control">').attr('name', nameOnForm).attr('id', id).attr('maxlength', maxlength)
        );

        return result;
    }

    function renderOverrideReadOnlyStateButton(ui) {
        return ui.overrideReadOnlyStateButton = $('<button class="btn btn-default" type="button"><span class="fa fa-edit"></span></button>').hide();
    }

    function renderOrderLineNoteIcon(ui) {
        return ui.orderLineNoteIcon = $('<i class="la la-files-o directions-icon order-line-note-icon" data-toggle="tooltip" data-html="true"></i>').hide();
    }

    function renderClickableWarningIcon(ui) {
        return $('<div class="clickable-warning-icon-container">').append(
            ui.clickableWarningIcon = $('<i class="fas fa-exclamation-triangle clickable-warning-icon lg"></i>').hide()
        );
    }

    function renderDriverNoteIconsContainer(ui) {
        return ui.driverNoteIconsContainer = $('<div class="driver-note-icons-container">');
    }

    function renderTicketCounts(ui, groupingLevel) {
        let emptyBlockCountTitle = 'Driver jobs with no tickets';
        let emptyBlockCountText = '...'
        if (groupingLevel === 'orderLine') {
            emptyBlockCountTitle = 'No tickets for this job';
            emptyBlockCountText = '?';
        }

        return $('<div>').append(
            ui.verifiedTicketCount = $('<span class="circle-small bg-green">').attr('title', 'Verified tickets').text("...").hide()
        ).append(
            ui.enteredTicketCount = $('<span class="circle-small bg-yellow">').attr('title', 'Entered tickets').text("...").hide()
        ).append(
            ui.missingTicketCount = $('<span class="circle-small bg-red">').attr('title', 'Missing tickets').text("...").hide()
        ).append(
            ui.emptyOrderLineBlockCount = $('<span class="circle-small bd-unavailable">').attr('title', emptyBlockCountTitle).text(emptyBlockCountText).hide()
        );
    }

    function renderToggleButton(ui) {
        return $('<div class="col-lg-2 col-md-4 col-sm-2 col-4 pb-2">').append(
            $() //$('<label class="control-label">&nbsp;</label>')
        ).append(
            $('<div class="d-flex justify-content-end">').append(
                ui.toggleCardDetailsButton = $('<button type="button" class="btn btn-primary" data-card-tool="toggle"><span class="arrow la la-angle-up"></span></button>')
            )
        );
    }

    function refreshExpandAllButton() {
        //_expandAllPanelsButton.text(haveCollapsedCards() ? 'Expand All' : 'Collapse All');
        //_expandAllPanelsButton.toggle(haveVisibleCards());
    }

    function showUomWarningIfNeeded(oldName, newName) {
        if (oldName === newName || !abp.setting.getBoolean('App.TimeAndPay.PreventProductionPayOnHourlyJobs')) {
            return;
        }
        let hours = ['hour', 'hours']
        newName = (newName || '').toLowerCase();
        oldName = (oldName || '').toLowerCase();
        if (hours.includes(newName) || hours.includes(oldName)) {
            abp.message.warn(app.localize('TimeEntiresWarningOnHoursUomChange'));
        }
    }

    _$ticketPhotoInput.change(async function () {
        if (!_ticketForPhotoUpload || !_blockForPhotoUpload) {
            return;
        }

        if (!abp.helper.validateTicketPhoto(_$ticketPhotoInput)) {
            return;
        }

        if (!_ticketForPhotoUpload.id) {
            await saveTicket(_ticketForPhotoUpload);
        }

        const file = _$ticketPhotoInput[0].files[0];
        const reader = new FileReader();
        let block = _blockForPhotoUpload;

        reader.addEventListener("load", function () {
            _ticketService.addTicketPhoto({
                ticketId: _ticketForPhotoUpload.id,
                ticketPhoto: reader.result,
                ticketPhotoFilename: file.name
            }).done(function (result) {
                _ticketForPhotoUpload.ticketPhotoId = result.ticketPhotoId;
                _blockForPhotoUpload.ui.reloadGrid();
            }).always(function () {
                _ticketForPhotoUpload = null;
                _blockForPhotoUpload = null;
                _$ticketPhotoInput.val('');
                abp.ui.clearBusy(block.ui.card);
            });
        }, false);

        abp.ui.setBusy(block.ui.card);
        reader.readAsDataURL(file);
    });


})();