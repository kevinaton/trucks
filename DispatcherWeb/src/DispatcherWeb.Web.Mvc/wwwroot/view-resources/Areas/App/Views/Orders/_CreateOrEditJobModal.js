(function ($) {
    app.modals.CreateOrEditJobModal = function () {

        var _modalManager;
        var _orderAppService = abp.services.app.order;
        var _$form = null;
        var _quoteId = null;
        var _quoteServiceId = null;
        var _pricing = null;
        var _orderLine = null;
        var _orderId = null;
        var _model = null;
        var _initializing = false;
        var _recalculating = false;
        var _permissions = {
            edit: abp.auth.hasPermission('Pages.Orders.Edit')
        };
        var _allowCounterSales = abp.setting.getBoolean('App.DispatchingAndMessaging.AllowCounterSalesForUser') && abp.setting.getBoolean('App.DispatchingAndMessaging.AllowCounterSalesForTenant');
        var _saveEventArgs = {
            reloadMaterialTotalIfNotOverridden: false,
            reloadFreightTotalIfNotOverridden: false
        };
        var _quoteDropdown = null;
        var _deliveryDateDropdown = null;
        var _loadAtDropdown = null;
        var _deliverToDropdown = null;
        var _serviceDropdown = null;
        var _materialUomDropdown = null;
        var _freightUomDropdown = null;
        var _designationDropdown = null;
        var _isMaterialPricePerUnitOverriddenInput = null;
        var _isFreightPricePerUnitOverriddenInput = null;
        var _materialQuantityInput = null;
        var _freightQuantityInput = null;
        var _materialPricePerUnitInput = null;
        var _freightPricePerUnitInput = null;
        var _freightRateToPayDriversInput = null;
        var _materialPriceInput = null; //total for the item
        var _freightPriceInput = null; //total for the item
        var _numberOfTrucksInput = null;
        var _timeOnJobInput = null;
        var _isMaterialPriceOverriddenInput = null;
        var _isFreightPriceOverriddenInput = null;
        var _unlockMaterialPriceButton = null;
        var _unlockFreightPriceButton = null;
        var _wasProductionPay = null;
        var _ratesLastValue = {};
        var _addLocationTarget = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            var _createOrEditCustomerModal = new app.ModalManager({
                viewUrl: abp.appPath + 'app/Customers/CreateOrEditCustomerModal',
                scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Customers/_CreateOrEditCustomerModal.js',
                modalClass: 'CreateOrEditCustomerModal',
                modalSize: 'lg'
            });

            var createOrEditServiceModal = new app.ModalManager({
                viewUrl: abp.appPath + 'app/Services/CreateOrEditServiceModal',
                scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Services/_CreateOrEditServiceModal.js',
                modalClass: 'CreateOrEditServiceModal',
                modalSize: 'lg'
            });

            var createOrEditLocationModal = new app.ModalManager({
                viewUrl: abp.appPath + 'app/Locations/CreateOrEditLocationModal',
                scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Locations/_CreateOrEditLocationModal.js',
                modalClass: 'CreateOrEditLocationModal',
                modalSize: 'lg'
            });

            var setStaggeredTimesModal = new app.ModalManager({
                viewUrl: abp.appPath + 'app/Orders/SetStaggeredTimesModal',
                scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_SetStaggeredTimesModal.js',
                modalClass: 'SetStaggeredTimesModal'
            });

            var _selectOrderQuoteModal = new app.ModalManager({
                viewUrl: abp.appPath + 'app/Orders/SelectOrderQuoteModal',
                scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_SelectOrderQuoteModal.js',
                modalClass: 'SelectOrderQuoteModal'
            });

            var _addQuoteBasedOrderLinesModal = new app.ModalManager({
                viewUrl: abp.appPath + 'app/Orders/AddQuoteBasedOrderLinesModal',
                scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_AddQuoteBasedOrderLinesModal.js',
                modalClass: 'AddQuoteBasedOrderLinesModal',
                modalSize: 'lg'
            });

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            abp.helper.ui.initControls();

            _timeOnJobInput = _$form.find("#TimeOnJob");

            _numberOfTrucksInput = _$form.find("#NumberOfTrucks");
            _numberOfTrucksInput.change(function () {
                updateStaggeredTimeControls();
            }).change();

            _$form.find("#SetStaggeredTimeButton").click(function () {
                var orderLineId = _$form.find("#OrderLineId").val();
                if (orderLineId) {
                    setStaggeredTimesModal.open({ orderLineId });
                } else {
                    _orderLine = _orderLine || {};
                    let model = {
                        orderLineId: null,
                        staggeredTimeKind: Number(_$form.find("#StaggeredTimeKind").val()),
                        staggeredTimeInterval: _orderLine.staggeredTimeInterval,
                        firstStaggeredTimeOnJob: _orderLine.firstStaggeredTimeOnJob
                    };
                    setStaggeredTimesModal.open({}).done(function (modal, modalObject) {
                        modalObject.setModel(model);
                        modalObject.saveCallback = function (model) {
                            _orderLine.updateStaggeredTime = true;
                            _orderLine.staggeredTimeKind = model.staggeredTimeKind;
                            _orderLine.staggeredTimeInterval = model.staggeredTimeInterval;
                            _orderLine.firstStaggeredTimeOnJob = model.firstStaggeredTimeOnJob;
                        };
                    });
                }
            }).tooltip();

            _modalManager.on('app.staggeredTimesSetModal', function (e) {
                _$form.find("#StaggeredTimeKind").val(e.staggeredTimeKind);
                updateTimeOnJobInput();
            });
            updateTimeOnJobInput();


            _quoteId = _$form.find("#QuoteId").val();
            _quoteServiceId = _$form.find("#QuoteServiceId").val();
            _orderId = _$form.find("#OrderId").val();

            _quoteDropdown = _$form.find("#QuoteId");
            _deliveryDateDropdown = _$form.find("#DeliveryDate");
            _loadAtDropdown = _$form.find("#LoadAtId");
            _deliverToDropdown = _$form.find("#DeliverToId");
            _serviceDropdown = _$form.find("#JobServiceId");
            _materialUomDropdown = _$form.find("#MaterialUomId");
            _freightUomDropdown = _$form.find("#FreightUomId");
            _designationDropdown = _$form.find("#JobDesignation");
            _customerDropdown = _$form.find("#JobCustomerId");

            _projectInput = _$form.find("#ProjectId");
            _isMaterialPricePerUnitOverriddenInput = _$form.find("#IsMaterialPricePerUnitOverridden");
            _isFreightPricePerUnitOverriddenInput = _$form.find("#IsFreightPricePerUnitOverridden");
            _materialQuantityInput = _$form.find("#MaterialQuantity");
            _freightQuantityInput = _$form.find("#FreightQuantity");
            _materialPricePerUnitInput = _$form.find("#MaterialPricePerUnit");
            _freightPricePerUnitInput = _$form.find("#FreightPricePerUnit");
            _freightRateToPayDriversInput = _$form.find("#FreightRateToPayDrivers");

            _ratesLastValue = {
                freightPricePerUnit: Number(_freightPricePerUnitInput.val()) || 0
            };

            _materialPriceInput = _$form.find("#MaterialPrice"); //total for item
            _freightPriceInput = _$form.find("#FreightPrice"); //total for item
            var leaseHaulerRateInput = _$form.find("#LeaseHaulerRate");
            _isMaterialPriceOverriddenInput = _$form.find("#IsMaterialPriceOverridden");
            _isFreightPriceOverriddenInput = _$form.find("#IsFreightPriceOverridden");

            _materialPriceInput.val(round(_materialPriceInput.val()).toFixed(2));
            _freightPriceInput.val(round(_freightPriceInput.val()).toFixed(2));
            if (leaseHaulerRateInput.val() !== "") {
                leaseHaulerRateInput.val(abp.utils.round(leaseHaulerRateInput.val()).toFixed(2));
            }

            if (!abp.setting.getBoolean('App.LeaseHaulers.ShowLeaseHaulerRateOnOrder')) {
                leaseHaulerRateInput.closest('.form-group').hide();
            }

            //Init field editors

            _deliveryDateDropdown.datepickerInit();

            _$form.find("#JobShift").select2Init({
                showAll: true,
                allowClear: false
            });

            _$form.find("#JobLocationId").select2Init({
                abpServiceMethod: abp.services.app.office.getOfficesSelectList,
                showAll: true,
                allowClear: false
            });

            _customerDropdown.select2Init({
                abpServiceMethod: abp.services.app.customer.getActiveCustomersSelectList,
                showAll: false,
                allowClear: true,
                addItemCallback: async function (newItemName) {
                    _createOrEditCustomerModal.open({ name: newItemName });
                }
            });

            _$form.find("#JobPriority").select2Init({
                showAll: true,
                allowClear: false
            });

            _$form.find("#AutoGenerateTicketNumber").change(updateTicketNumberVisibility);

            _$form.find("#RequiresCustomerNotification").change(updateCustomerNotificationControlsVisibility);

            var quoteChildDropdown = abp.helper.ui.initChildDropdown({
                parentDropdown: _customerDropdown,
                childDropdown: _quoteDropdown,
                abpServiceMethod: abp.services.app.quote.getQuotesForCustomer,
                abpServiceData: { hideInactive: true },
                optionCreatedCallback: function (option, val) {
                    switch (val.item.status) {
                        case abp.enums.projectStatus.pending: option.addClass("quote-pending"); break;
                        case abp.enums.projectStatus.active: option.addClass("quote-active"); break;
                        case abp.enums.projectStatus.inactive: option.addClass("quote-inactive"); break;
                    }
                }
            });

            _customerDropdown.change(function () {
                var dropdownData = _customerDropdown.select2('data');
                if (dropdownData && dropdownData.length) {
                    if (dropdownData[0].item) {
                        _$form.find("#CustomerAccountNumber").val(dropdownData[0].item.accountNumber);
                        if (dropdownData[0].item.customerIsCod) {
                            _$form.find("#CustomerAccountNumber").val('COD');
                            _$form.find("#CustomerAccountNumber").addClass('cod-account-number');
                        } else {
                            _$form.find("#CustomerAccountNumber").removeClass('cod-account-number');
                        }
                    }
                }
                showSelectOrderLineButton();
            });

            _$form.find("#JobFuelSurchargeCalculationId").select2Init({
                abpServiceMethod: abp.services.app.fuelSurchargeCalculation.getFuelSurchargeCalculationsSelectList,
                showAll: true,
                allowClear: true
            });
            _$form.find("#JobFuelSurchargeCalculationId").change(function () {
                if (_quoteId !== '') {
                    //_$form.find("#BaseFuelCostContainer").toggle(false);
                } else {
                    let dropdownData = _$form.find("#JobFuelSurchargeCalculationId").select2('data');
                    let selectedOption = dropdownData && dropdownData.length && dropdownData[0];
                    let canChangeBaseFuelCost = selectedOption?.item?.canChangeBaseFuelCost || false;
                    _$form.find("#BaseFuelCostContainer").toggle(canChangeBaseFuelCost);
                    _$form.find("#BaseFuelCost").val(selectedOption?.item?.baseFuelCost || 0);
                    _$form.find("#JobFuelSurchargeCalculationId").removeUnselectedOptions();
                }
            });

            quoteChildDropdown.onChildDropdownUpdated(function (data) {
                var hasActiveOrPendingQuotes = false;
                $.each(data.items, function (ind, val) {
                    if (val.item.status === abp.enums.projectStatus.pending || val.item.status === abp.enums.projectStatus.active) {
                        hasActiveOrPendingQuotes = true;
                    }
                });
                if (hasActiveOrPendingQuotes && data.items.length > 0) {
                    _selectOrderQuoteModal.open();
                }
            });

            _modalManager.on('app.selectOrderQuoteModal.requestInput', function (callback) {
                callback(_quoteDropdown);
            });

            //Quote change handling

            function updateInputValueIfSourceIsNotNull(input, sourceValue) {
                if (sourceValue !== null && sourceValue !== '') {
                    _$form.find(input).val(sourceValue).change();
                }
            }

            function updateInputValue(input, sourceValue) {
                _$form.find(input).val(sourceValue).change();
            }

            var handleQuoteChangeAsync = async function (quoteId, option) {
                if (quoteId === _quoteId) {
                    return;
                }
                _quoteId = quoteId;

                disableQuoteRelatedFieldsIfNeeded();

                _projectInput.val(option.data('projectId'));
                if (_quoteId !== '') {
                    _$form.find("#ContactId").val(option.data('contactId')).change();
                }
                updateInputValue("#PONumber", option.data('poNumber'));
                updateInputValue("#SpectrumNumber", option.data('spectrumNumber'));
                updateInputValue("#Directions", option.data('directions'));
                if (abp.session.officeCopyChargeTo) {
                    updateInputValue("#ChargeTo", option.data('chargeTo'));
                }

                if (_quoteId !== '') {
                    let fuelSurchargeCalculationId = option.data('fuelSurchargeCalculationId');
                    if (fuelSurchargeCalculationId) {
                        abp.helper.ui.addAndSetDropdownValue(_$form.find("#JobFuelSurchargeCalculationId"), fuelSurchargeCalculationId, option.data('fuelSurchargeCalculationName'));
                        updateInputValue("#BaseFuelCost", option.data('baseFuelCost'));
                    } else {
                        _$form.find("#JobFuelSurchargeCalculationId").val(null).change();
                        updateInputValue("#BaseFuelCost", 0);
                    }
                    _$form.find("#BaseFuelCostContainer").toggle(option.data('canChangeBaseFuelCost') === true);
                    _$form.find("#JobFuelSurchargeCalculationId").prop("disabled", true);
                    _$form.find("#BaseFuelCost").prop("disabled", true);
                } else {
                    let defaultFuelSurchargeCalculationId = abp.setting.getInt('App.Fuel.DefaultFuelSurchargeCalculationId');
                    let defaultFuelSurchargeCalculationName = _$form.find("#DefaultFuelSurchargeCalculationName").val();
                    if (defaultFuelSurchargeCalculationId > 0) {
                        abp.helper.ui.addAndSetDropdownValue(_$form.find("#JobFuelSurchargeCalculationId"), defaultFuelSurchargeCalculationId, defaultFuelSurchargeCalculationName);
                    } else {
                        _$form.find("#JobFuelSurchargeCalculationId").val(null).change();
                    }
                    updateInputValue("#BaseFuelCost", _$form.find("#DefaultBaseFuelCost").val());
                    _$form.find("#BaseFuelCostContainer").toggle(_$form.find("#DefaultCanChangeBaseFuelCost").val() === 'True');
                    _$form.find("#JobFuelSurchargeCalculationId").prop("disabled", false);
                    _$form.find("#BaseFuelCost").prop("disabled", false);
                }
                if (_quoteId) {
                    try {
                        abp.ui.setBusy();
                        let quoteLinesData = await _orderAppService.getOrderLines({ quoteId: _quoteId });
                        if (quoteLinesData.items.length === 1) {
                            setOrderLine(quoteLinesData.items[0]);
                        } else if (quoteLinesData.items.length > 1) {
                            openAddQuoteBasedOrderLinesModal();
                        }
                    }
                    finally {
                        abp.ui.clearBusy();
                    }
                }
            };

            function openAddQuoteBasedOrderLinesModal() {
                _addQuoteBasedOrderLinesModal.open({
                    titleText: "Select the desired line item",
                    saveButtonText: "Add Item",
                    limitSelectionToSingleOrderLine: true
                }).done(function (modal, modalObject) {
                    modalObject.setFilter({ quoteId: _quoteId });
                    showSelectOrderLineButton();
                });
            }

            _$form.find("#OpenQuoteBasedOrderLinesModalButton").click(function () {
                openAddQuoteBasedOrderLinesModal();
            });

            var quoteIdChanging = false;
            _quoteDropdown.change(async function () {
                if (quoteIdChanging) {
                    return;
                }
                var newQuoteId = _quoteDropdown.val();
                var option = _quoteDropdown.getSelectedDropdownOption();
                if (option.data('status') === abp.enums.projectStatus.pending) {
                    if (await abp.message.confirm(
                        "This quote is 'Pending'. If you add this quote to the order it will be changed to 'Active'. If you do not want the quote to be activated, select 'Cancel'."
                    )) {
                        option.data('status', abp.enums.projectStatus.active);
                        await handleQuoteChangeAsync(newQuoteId, option);
                        abp.services.app.quote.setQuoteStatus({ id: _quoteId, status: abp.enums.projectStatus.active });
                    } else {
                        quoteIdChanging = true;
                        //check if the old value is still in the dropdown, and set the quote to "" if not
                        if (_quoteDropdown.getDropdownOption(_quoteId).length) {
                            _quoteDropdown.val(_quoteId).change();
                        } else {
                            _quoteDropdown.val('').change();
                            _projectInput.val('').change();
                        }
                        quoteIdChanging = false;
                    }
                } else {
                    await handleQuoteChangeAsync(newQuoteId, option);
                }
            });

            _modalManager.on('app.quoteBasedOrderLinesSelectedModal', function (eventData) {
                if (!eventData.selectedLines.length) {
                    return;
                }
                if (isNewOrder()) {
                    setOrderLine(eventData.selectedLines[0]);
                } else {
                    //abp.ui.setBusy();
                    setOrderLine(eventData.selectedLines[0]);
                    //saving those will add more order lines to the job instead of replacing the single one, so saving is commented out for now.
                    //this way, they will save the main modal and will replace the values in the single existing orderline
                    //_orderAppService.editOrderLines(eventData.selectedLines).done(function () {
                    //    abp.notify.info('Saved successfully.');
                    //}).always(function () {
                    //    abp.ui.clearBusy();
                    //});
                }
            });

            _loadAtDropdown.select2Location({
                predefinedLocationCategoryKind: abp.enums.predefinedLocationCategoryKind.unknownLoadSite,
                addItemCallback: abp.auth.hasPermission('Pages.Locations') ? async function (newItemName) {
                    _addLocationTarget = "LoadAtId";
                    createOrEditLocationModal.open({ mergeWithDuplicateSilently: true, name: newItemName });
                } : undefined
            });
            _deliverToDropdown.select2Location({
                predefinedLocationCategoryKind: abp.enums.predefinedLocationCategoryKind.unknownDeliverySite,
                addItemCallback: abp.auth.hasPermission('Pages.Locations') ? async function (newItemName) {
                    _addLocationTarget = "DeliverToId";
                    createOrEditLocationModal.open({ mergeWithDuplicateSilently: true, name: newItemName });
                } : undefined
            });
            _serviceDropdown.select2Init({
                abpServiceMethod: abp.services.app.service.getServicesWithTaxInfoSelectList,
                showAll: false,
                allowClear: false,
                addItemCallback: abp.auth.hasPermission('Pages.Services') ? async function (newItemName) {
                    createOrEditServiceModal.open({ name: newItemName });
                } : undefined
            });
            _materialUomDropdown.select2Uom();
            _freightUomDropdown.select2Uom();
            _designationDropdown.select2Init({
                showAll: true,
                allowClear: false
            });

            if (isNewOrder()) {
                setDefaultValueForProductionPay();
            }

            _designationDropdown.change(function () {
                updateDesignationRelatedFieldsVisibility();
                setDefaultValuesForCounterSaleDesignationIfNeeded();
                updateSaveButtonsVisibility();

                if (designationHasMaterial()) {
                    enableMaterialFields();
                } else {
                    disableMaterialFields();
                }
                if (designationIsMaterialOnly()) {
                    disableFreightFields();
                } else {
                    enableFreightFields();
                }
                updateProductionPay();
            }).change();

            _modalManager.on('app.createOrEditServiceModalSaved', function (e) {
                abp.helper.ui.addAndSetDropdownValue(_serviceDropdown, e.item.Id, e.item.Service1);
                _$form.find("#IsTaxable").val(e.item.isTaxable ? "True" : "False");
                _serviceDropdown.change();
            });

            _modalManager.on('app.createOrEditLocationModalSaved', function (e) {
                if (!_addLocationTarget) {
                    return;
                }
                var targetDropdown = _$form.find("#" + _addLocationTarget);
                abp.helper.ui.addAndSetDropdownValue(targetDropdown, e.item.id, e.item.displayName);
                targetDropdown.change();
            });

            reloadPricing();
            refreshHighlighting();

            _serviceDropdown.change(function () {
                var sender = $(this);
                reloadPricing(function () {
                    recalculate(sender);
                });
                var dropdownData = _serviceDropdown.select2('data');
                if (dropdownData && dropdownData.length && dropdownData[0].item) {
                    _$form.find("#IsTaxable").val(dropdownData[0].item.isTaxable ? "True" : "False");
                }
            });
            _materialUomDropdown.change(function () {
                var sender = $(this);
                reloadPricing(function () {
                    recalculate(sender);
                });
            });
            _freightUomDropdown.change(function () {
                var sender = $(this);
                reloadPricing(function () {
                    recalculate(sender);
                });
                disableProductionPayIfNeeded(true);
            });

            abp.helper.ui.syncUomDropdowns(_materialUomDropdown, _freightUomDropdown, _designationDropdown, _materialQuantityInput, _freightQuantityInput);

            _materialQuantityInput.change(function () {
                recalculate($(this));
            });
            _freightQuantityInput.change(function () {
                recalculate($(this));
            });
            _materialPricePerUnitInput.change(function () {
                recalculate($(this));
            });

            _freightPricePerUnitInput.change(function () {
                /* #12546: If the “Freight Rate to Pay Drivers” textbox isn’t being displayed, the FreightRateToPayDrivers 
                property should be updated to the same value as the “Freight Rate”.  
                When the “Freight Rate” is changed and the driver pay rate was the same as the prior “Freight Rate”, 
                the “Freight Rate to Pay Drivers” should be changed to be the same as the “Freight Rate”. 
                */
                var newFreightPricePerUnit = Number(_freightPricePerUnitInput.val()) || 0;
                var freightRateToPayDrivers = Number(_freightRateToPayDriversInput.val()) || 0;

                if (_freightRateToPayDriversInput.css("display") == "none"
                    || _freightRateToPayDriversInput.css("visibility") == "hidden"
                    || _ratesLastValue.freightPricePerUnit !== newFreightPricePerUnit && _ratesLastValue.freightPricePerUnit === freightRateToPayDrivers
                ) {
                    _freightRateToPayDriversInput.val(newFreightPricePerUnit);
                }
                _ratesLastValue.freightPricePerUnit = newFreightPricePerUnit;

                recalculate($(this));
            });

            _materialPriceInput.change(function () {
                _saveEventArgs.reloadMaterialTotalIfNotOverridden = true;
                setIsMaterialPriceOverridden(true);
            });
            _freightPriceInput.change(function () {
                _saveEventArgs.reloadFreightTotalIfNotOverridden = true;
                setIsFreightPriceOverridden(true);
            });

            _modalManager.getModal().find(".save-job-button").click(function (e) {
                e.preventDefault();
                saveJobAsync(function () {
                    _modalManager.close();
                });
            });

            _modalManager.getModal().find("#SaveAndPrintButton").click(function (e) {
                e.preventDefault();
                saveJobAsync(function (saveResult) {
                    if (!saveResult.ticketId) {
                        abp.message.error(app.localize('TicketNumberIsRequired'));
                        return;
                    }
                    _$form.find("#TicketId").val(saveResult.ticketId);
                    window.open(abp.appPath + 'app/tickets/GetTicketPrintOut?ticketId=' + saveResult.ticketId);
                    _modalManager.close();
                });
            });

            initOverrideButtons();
            disableProductionPayIfNeeded(false);
            disableQuoteRelatedFieldsIfNeeded();
            disableFieldsIfEditingJob();
            updateDesignationRelatedFieldsVisibility();
            if (isNewOrder()) {
                setDefaultValuesForCounterSaleDesignationIfNeeded();
            }
            updateSaveButtonsVisibility();
            disableJobEditIfNeeded();
            disableTaxControls();
            updateCustomerNotificationControlsVisibility();
        };

        this.focusOnDefaultElement = function () {
            var focusFieldId = _$form.find('#FocusFieldId').val();
            if (focusFieldId !== '') {
                _$form.find('#' + focusFieldId).focus();
            }
        }

        function disableOrderEditForHaulingCompany() {
            _$form.find('input,select,textarea').not('#SalesTaxRate, #JobFuelSurchargeCalculationId, #BaseFuelCost, .order-line-field').attr('disabled', true);
        }

        function disableJobEditIfNeeded() {
            if (!_permissions.edit) {
                _$form.find('input,select,textarea,button').attr('disabled', true);
                _modalManager.getModal().find('.save-button-dropdown').hide();
            } else {
                if (_$form.find("#MaterialCompanyOrderId").val()) {
                    disableOrderEditForHaulingCompany();
                }
            }
        }

        function disableTaxControls() {
            var taxCalculationType = abp.setting.getInt('App.Invoice.TaxCalculationType');
            switch (taxCalculationType) {
                case abp.enums.taxCalculationType.noCalculation:
                    _$form.find("#SalesTaxRate").parent().hide();
                    break;
                default:
                    _$form.find("#SalesTax").parent().hide();
                    break;
            }
        }

        function isNewOrder() {
            return _orderId === '';
        }

        function setOrderLine(orderLine) {
            if (_orderId) {
                orderLine.orderId = _orderId;
            }

            _orderLine = orderLine;
            if (!_$form) {
                return;
            }

            _initializing = true;
            //_$form.find("#Id").val(_orderLine.id);
            //_$form.find("#OrderId").val(_orderLine.orderId);
            //_$form.find("#QuoteId").val(_orderLine.quoteId);
            _$form.find("#QuoteServiceId").val(_orderLine.quoteServiceId);
            _$form.find("#IsMaterialPricePerUnitOverridden").val(_orderLine.isMaterialPricePerUnitOverridden ? "True" : "False");
            _$form.find("#IsFreightPricePerUnitOverridden").val(_orderLine.isFreightPricePerUnitOverridden ? "True" : "False");
            _$form.find("#IsMaterialPriceOverridden").val(_orderLine.isMaterialPriceOverridden ? "True" : "False");
            _$form.find("#IsFreightPriceOverridden").val(_orderLine.isFreightPriceOverridden ? "True" : "False");
            _$form.find("#IsTaxable").val(_orderLine.isTaxable ? "True" : "False");
            _$form.find("#StaggeredTimeKind").val(_orderLine.staggeredTimeKind);
            //_$form.find("#LineNumber").val(_orderLine.lineNumber);
            abp.helper.ui.addAndSetDropdownValue(_$form.find("#JobDesignation"), _orderLine.designation, _orderLine.designationName);
            abp.helper.ui.addAndSetDropdownValue(_$form.find("#LoadAtId"), _orderLine.loadAtId, _orderLine.loadAtName);
            abp.helper.ui.addAndSetDropdownValue(_$form.find("#DeliverToId"), _orderLine.deliverToId, _orderLine.deliverToName);
            abp.helper.ui.addAndSetDropdownValue(_$form.find("#JobServiceId"), _orderLine.serviceId, _orderLine.serviceName);
            abp.helper.ui.addAndSetDropdownValue(_$form.find("#MaterialUomId"), _orderLine.materialUomId, _orderLine.materialUomName);
            abp.helper.ui.addAndSetDropdownValue(_$form.find("#FreightUomId"), _orderLine.freightUomId, _orderLine.freightUomName);
            _$form.find("#MaterialPricePerUnit").val(_orderLine.materialPricePerUnit);
            _$form.find("#FreightPricePerUnit").val(_orderLine.freightPricePerUnit);
            _ratesLastValue.freightPricePerUnit = Number(_orderLine.freightPricePerUnit) || 0;
            _$form.find("#FreightRateToPayDrivers").val(_orderLine.freightRateToPayDrivers);
            _$form.find("#LeaseHaulerRate").val(_orderLine.leaseHaulerRate);
            _$form.find("#MaterialQuantity").val(_orderLine.materialQuantity);
            _$form.find("#FreightQuantity").val(_orderLine.freightQuantity);
            _$form.find("#MaterialPrice").val(_orderLine.materialPrice);
            _$form.find("#FreightPrice").val(_orderLine.freightPrice);
            _$form.find("#NumberOfTrucks").val(_orderLine.numberOfTrucks);
            _$form.find("#IsMultipleLoads").prop('checked', _orderLine.isMultipleLoads);
            _$form.find("#ProductionPay").prop('checked', _orderLine.productionPay);
            _$form.find("#TimeOnJob").val(_orderLine.timeOnJob);
            _$form.find("#JobNumber").val(_orderLine.jobNumber);
            _$form.find("#Note").val(_orderLine.note);

            //_quoteId = _$form.find("#QuoteId").val();
            _quoteServiceId = _$form.find("#QuoteServiceId").val();

            updateStaggeredTimeControls();
            updateTimeOnJobInput();
            updateProductionPay();
            disableProductionPayIfNeeded(false);
            disableQuoteRelatedFieldsIfNeeded();
            showSelectOrderLineButton();

            _initializing = false;
            reloadPricing();

            refreshTotalFields();
            refreshOverrideButtons();
            refreshHighlighting();
        }

        function setDefaultValueForProductionPay() {
            _$form.find("#ProductionPay").prop('checked', abp.setting.getBoolean('App.TimeAndPay.DefaultToProductionPay'));
        }

        function showSelectOrderLineButton() {
            if (_quoteId && !_quoteServiceId) {
                _$form.find("#JobDesignation").parent().hide();
                _$form.find("#OpenQuoteBasedOrderLinesModalButton").parent().show();
            } else {
                _$form.find("#JobDesignation").parent().show();
                _$form.find("#OpenQuoteBasedOrderLinesModalButton").parent().hide();
            }
        }

        function disableQuoteRelatedFieldsIfNeeded() {
            var isQuoteSet = !!_quoteId;
            _designationDropdown
                .add(_loadAtDropdown)
                //.add(_deliverToDropdown)
                .add(_serviceDropdown)
                .add(_freightUomDropdown)
                .add(_materialUomDropdown)
                //.add(_freightPricePerUnitInput)
                //.add(_materialPricePerUnitInput)
                //.add(_freightQuantityInput)
                //.add(_materialQuantityInput)
                .prop('disabled', isQuoteSet);
        }

        function disableFieldsIfEditingJob() {
            if (!isNewOrder()) {
                _customerDropdown
                    .add(_designationDropdown)
                    .prop('disabled', true);
            }
        }

        function updateDesignationRelatedFieldsVisibility() {
            var designationRelatedFields = _$form.find("#DesignationRelatedFields");
            if (_designationDropdown.val()) {
                updateControlsVisibility();
                designationRelatedFields.show();
            } else {
                designationRelatedFields.hide();
            }
        }

        function updateControlsVisibility() {
            var designation = Number(_designationDropdown.val());
            var designationIsCounterSale = designation === abp.enums.designation.materialOnly && _allowCounterSales;
            _deliverToDropdown.closest('.form-group').toggle(!designationIsCounterSale);
            _$form.find("#LeaseHaulerRate").closest('.form-group').toggle(!designationIsCounterSale);
            _$form.find("#NumberOfTrucks").closest('.form-group').toggle(!designationIsCounterSale);
            _$form.find("#IsMultipleLoads").closest('.form-group').toggle(!designationIsCounterSale);
            _$form.find("#TimeOnJob").closest('.form-group').toggle(!designationIsCounterSale);
            _$form.find("#ChargeTo").closest('.form-group').toggle(!designationIsCounterSale);
            _$form.find("#JobPriority").closest('.form-group').toggle(!designationIsCounterSale);
            _$form.find("#ProductionPay").closest('.form-group').toggle(!designationIsCounterSale);
            _$form.find("#AutoGenerateTicketNumber").closest('.form-group').toggle(designationIsCounterSale);
            _$form.find("#TicketNumber").closest('.form-group').toggle(designationIsCounterSale && !_$form.find('#AutoGenerateTicketNumber').is(':checked'));
        }

        function setDefaultValuesForCounterSaleDesignationIfNeeded() {
            var designation = Number(_designationDropdown.val());
            if (designation !== abp.enums.designation.materialOnly || !_allowCounterSales) {
                return;
            }
            if (_$form.find("#DefaultLoadAtLocationId").val()) {
                abp.helper.ui.addAndSetDropdownValue(_$form.find("#LoadAtId"), _$form.find("#DefaultLoadAtLocationId").val(), _$form.find("#DefaultLoadAtLocationName").val());
            }
            if (_$form.find("#DefaultServiceId").val()) {
                abp.helper.ui.addAndSetDropdownValue(_$form.find("#JobServiceId"), _$form.find("#DefaultServiceId").val(), _$form.find("#DefaultServiceName").val());
            }
            if (_$form.find("#DefaultMaterialUomId").val()) {
                abp.helper.ui.addAndSetDropdownValue(_$form.find("#MaterialUomId"), _$form.find("#DefaultMaterialUomId").val(), _$form.find("#DefaultMaterialUomName").val());
            }
        }

        function updateTicketNumberVisibility() {
            _$form.find("#TicketNumber").closest('.form-group').toggle(!_$form.find('#AutoGenerateTicketNumber').is(':checked'));
        }

        function updateCustomerNotificationControlsVisibility() {
            _$form.find("#CustomerNotificationContactName").closest('.form-group').toggle(_$form.find('#RequiresCustomerNotification').is(':checked'));
            _$form.find("#CustomerNotificationPhoneNumber").closest('.form-group').toggle(_$form.find('#RequiresCustomerNotification').is(':checked'));
        }

        function updateSaveButtonsVisibility() {
            var designation = Number(_designationDropdown.val());
            var designationIsCounterSale = designation === abp.enums.designation.materialOnly && _allowCounterSales;
            _modalManager.getModal().find(".save-button-container").toggle(!designationIsCounterSale);
            _modalManager.getModal().find(".save-and-print-buttons-container").toggle(designationIsCounterSale);
        }

        function initOverrideButtons() {
            _unlockMaterialPriceButton = _$form.find("#UnlockMaterialTotalButton");
            _unlockMaterialPriceButton.click(function () {
                setIsMaterialPriceOverridden(!getIsMaterialPriceOverridden());
                refreshTotalFields();
                refreshOverrideButtons();
                refreshHighlighting();
                if (getIsMaterialPriceOverridden()) {
                    _$form.find("#IsMultipleLoads").prop('checked', false).change();
                }
            });
            _unlockFreightPriceButton = _$form.find("#UnlockFreightTotalButton");
            _unlockFreightPriceButton.click(function () {
                setIsFreightPriceOverridden(!getIsFreightPriceOverridden());
                refreshTotalFields();
                refreshOverrideButtons();
                refreshHighlighting();
                if (getIsFreightPriceOverridden()) {
                    _$form.find("#IsMultipleLoads").prop('checked', false).change();
                }
            });
            refreshOverrideButtons();
        }

        function refreshTotalFields() {
            if (getIsMaterialPriceOverridden()) {
                _materialPriceInput.prop('disabled', false);
            } else {
                _materialPriceInput.prop('disabled', true);
                recalculate(_materialQuantityInput);
            }
            if (getIsFreightPriceOverridden()) {
                _freightPriceInput.prop('disabled', false);
            } else {
                _freightPriceInput.prop('disabled', true);
                recalculate(_freightQuantityInput);
            }
        }

        function refreshOverrideButtons() {
            refreshOverrideButton(_unlockMaterialPriceButton, getIsMaterialPriceOverridden());
            refreshOverrideButton(_unlockFreightPriceButton, getIsFreightPriceOverridden());
        }

        function refreshOverrideButton(button, isOverridden) {
            button
                .attr('title', app.localize(isOverridden ? "RemoveOverriddenValue" : "OverrideAutocalculatedValue"))
                .find('.fas').removeClass('fa-unlock fa-lock').addClass(isOverridden ? 'fa-unlock' : 'fa-lock');
        }

        function reloadPricing(callback) {
            if (_initializing) {
                return;
            }
            if (_serviceDropdown.val() === ''
                || _materialUomDropdown.val() === '' && _freightUomDropdown.val() === '') {
                _pricing = null;
                refreshHighlighting();
                if (callback)
                    callback();
                return;
            }
            abp.services.app.service.getServicePricing({
                serviceId: _serviceDropdown.val(),
                materialUomId: _materialUomDropdown.val(),
                freightUomId: _freightUomDropdown.val(),
                quoteServiceId: _quoteServiceId
            }).done(function (pricing) {
                _pricing = pricing;
                refreshHighlighting();
                if (callback)
                    callback();
            });
        }

        function refreshHighlighting() {
            if (_pricing && _pricing.quoteBasedPricing) {
                _serviceDropdown.addClass("quote-based-pricing");
                _materialUomDropdown.addClass("quote-based-pricing");
                _freightUomDropdown.addClass("quote-based-pricing");
            } else {
                _serviceDropdown.removeClass("quote-based-pricing");
                _materialUomDropdown.removeClass("quote-based-pricing");
                _freightUomDropdown.removeClass("quote-based-pricing");
            }

            if (getIsMaterialPricePerUnitOverridden()) {
                _materialPricePerUnitInput.addClass("overridden-price");
            } else {
                _materialPricePerUnitInput.removeClass("overridden-price");
            }

            if (getIsFreightPricePerUnitOverridden()) {
                _freightPricePerUnitInput.addClass("overridden-price");
            } else {
                _freightPricePerUnitInput.removeClass("overridden-price");
            }

            if (getIsMaterialPriceOverridden()) {
                _materialPriceInput.addClass("overridden-price");
            } else {
                _materialPriceInput.removeClass("overridden-price");
            }

            if (getIsFreightPriceOverridden()) {
                _freightPriceInput.addClass("overridden-price");
            } else {
                _freightPriceInput.removeClass("overridden-price");
            }
        }

        function getIsFreightPricePerUnitOverridden() {
            return _isFreightPricePerUnitOverriddenInput.val() === "True";
        }

        function setIsFreightPricePerUnitOverridden(val) {
            _isFreightPricePerUnitOverriddenInput.val(val ? "True" : "False");
        }

        function getIsMaterialPricePerUnitOverridden() {
            return _isMaterialPricePerUnitOverriddenInput.val() === "True";
        }

        function setIsMaterialPricePerUnitOverridden(val) {
            _isMaterialPricePerUnitOverriddenInput.val(val ? "True" : "False");
        }

        function getIsFreightPriceOverridden() {
            return _isFreightPriceOverriddenInput.val() === "True";
        }

        function setIsFreightPriceOverridden(val) {
            _isFreightPriceOverriddenInput.val(val ? "True" : "False");
        }

        function getIsMaterialPriceOverridden() {
            return _isMaterialPriceOverriddenInput.val() === "True";
        }

        function setIsMaterialPriceOverridden(val) {
            _isMaterialPriceOverriddenInput.val(val ? "True" : "False");
        }

        function updateTimeOnJobInput() {
            if (Number(_$form.find("#StaggeredTimeKind").val()) > 0) {
                disableTimeOnJobInput();
            } else {
                enableTimeOnJobInput();
            }
        }

        function enableTimeOnJobInput() {
            if (_timeOnJobInput.val() === 'Staggered') {
                _timeOnJobInput.val('');
            }
            _timeOnJobInput.prop('disabled', false).timepickerInit({ stepping: 1 });
        }

        function disableTimeOnJobInput() {
            var timeOnJobTimepicker = _timeOnJobInput.data('DateTimePicker');
            timeOnJobTimepicker && timeOnJobTimepicker.destroy();
            _timeOnJobInput.prop('disabled', true).val('Staggered');
        }

        function updateStaggeredTimeControls() {
            var isStaggeredTimeButtonVisible = Number(_numberOfTrucksInput.val()) > 1;
            _$form.find("#SetStaggeredTimeButton").closest('.input-group-btn').toggle(isStaggeredTimeButtonVisible);
            if (!isStaggeredTimeButtonVisible && Number(_$form.find('#StaggeredTimeKind').val()) > 0) {
                _$form.find('#StaggeredTimeKind').val("0");
                enableTimeOnJobInput();
            }
        }

        function disableProductionPayIfNeeded(forceUncheck) {
            if (!shouldDisableProductionPay()) {
                enableProductionPay();
            } else {
                let productionPayInput = _$form.find('#ProductionPay')

                if (forceUncheck) {
                    productionPayInput.prop('checked', false);
                } else {
                    if (productionPayInput.is(':checked')) {
                        return;
                    }
                }

                disableProductionPay();
            }
        }

        function shouldDisableProductionPay() {
            if (abp.setting.getBoolean('App.TimeAndPay.PreventProductionPayOnHourlyJobs')) {
                let freightUom = _freightUomDropdown.getSelectedDropdownOption().text();
                if (['hours', 'hour'].includes((freightUom || '').toLowerCase())) {
                    return true;
                }
            }
            return false;
        }

        function disableProductionPay() {
            let productionPayInput = _$form.find('#ProductionPay');
            productionPayInput.prop('disabled', true);
            productionPayInput.closest('label').attr('title', app.localize('PreventProductionPayOnHourlyJobsHint')).tooltip();
        }

        function enableProductionPay() {
            let productionPayInput = _$form.find('#ProductionPay');
            productionPayInput.prop('disabled', false);
            productionPayInput.closest('label').attr('title', '').tooltip('dispose');
        }

        function setFreightRateFromPricingIfNeeded(rate, sender) {
            if (getIsFreightPricePerUnitOverridden() || designationIsMaterialOnly()) {
                return;
            }
            //when quantity changes, don't reset the rate from pricing unless the rate was empty
            if ((sender.is(_materialQuantityInput) || sender.is(_freightQuantityInput)) && _freightPricePerUnitInput.val()) {
                return;
            }
            _freightPricePerUnitInput.val(rate).change();
        }

        function setMaterialRateFromPricingIfNeeded(rate, sender) {
            if (getIsMaterialPricePerUnitOverridden() || !designationHasMaterial()) {
                return;
            }
            //when quantity changes, don't reset the rate from pricing unless the rate was empty
            if ((sender.is(_materialQuantityInput) || sender.is(_freightQuantityInput)) && _materialPricePerUnitInput.val()) {
                return;
            }
            _materialPricePerUnitInput.val(rate);
        }

        function recalculate(sender) {
            if (_initializing || _recalculating) {
                return;
            }
            _recalculating = true;

            var freightRatePricing =
                _pricing && _pricing.quoteBasedPricing && _pricing.quoteBasedPricing.freightRate !== null ? _pricing.quoteBasedPricing.freightRate
                    : _pricing && _pricing.hasPricing && _pricing.freightRate !== null ? _pricing.freightRate
                        : null;

            var materialRatePricing =
                _pricing && _pricing.quoteBasedPricing && _pricing.quoteBasedPricing.pricePerUnit !== null ? _pricing.quoteBasedPricing.pricePerUnit
                    : _pricing && _pricing.hasPricing && _pricing.pricePerUnit !== null ? _pricing.pricePerUnit
                        : null;

            if (freightRatePricing !== null) {
                if (sender.is(_freightPricePerUnitInput)) {
                    setIsFreightPricePerUnitOverridden(freightRatePricing !== Number(_freightPricePerUnitInput.val()));
                } else {
                    setFreightRateFromPricingIfNeeded(freightRatePricing, sender);
                }
            } else {
                //no freight pricing
                if (!getIsFreightPricePerUnitOverridden() && (sender.is(_freightUomDropdown) || sender.is(_serviceDropdown))) {
                    _freightPricePerUnitInput.val('').change();
                }
            }

            if (materialRatePricing !== null) {
                if (sender.is(_materialPricePerUnitInput)) {
                    setIsMaterialPricePerUnitOverridden(materialRatePricing !== Number(_materialPricePerUnitInput.val()));
                } else {
                    setMaterialRateFromPricingIfNeeded(materialRatePricing, sender);
                }
            } else {
                //no material pricing
                if (!getIsMaterialPricePerUnitOverridden() && (sender.is(_materialUomDropdown) || sender.is(_serviceDropdown))) {
                    _materialPricePerUnitInput.val('');
                }
            }

            var materialPricePerUnit = _materialPricePerUnitInput.val();
            var freightPricePerUnit = _freightPricePerUnitInput.val();
            var materialQuantity = _materialQuantityInput.val();
            var freightQuantity = _freightQuantityInput.val();
            var materialPrice = round(materialPricePerUnit * materialQuantity);
            var freightPrice = round(freightPricePerUnit * freightQuantity);

            if (!getIsMaterialPriceOverridden()) {
                _materialPriceInput.val(materialPrice.toFixed(2));
            }
            if (!getIsFreightPriceOverridden()) {
                _freightPriceInput.val(freightPrice.toFixed(2));
            }

            refreshHighlighting();
            _saveEventArgs.reloadMaterialTotalIfNotOverridden = true;
            _saveEventArgs.reloadFreightTotalIfNotOverridden = true;
            _recalculating = false;
        }

        function round(num) {
            return abp.utils.round(num);
        }

        function designationHasMaterial() {
            var designation = Number(_designationDropdown.val());
            return abp.enums.designations.hasMaterial.includes(designation);
        }
        function designationIsMaterialOnly() {
            return abp.enums.designations.materialOnly.includes(Number(_designationDropdown.val()));
        }

        function disableMaterialFields() {
            _$form.find("label[for=MaterialUomId]").removeClass('required-label');
            _$form.find('#MaterialPricePerUnit').val('').closest('.form-group').hide();
            _$form.find('#MaterialPrice').val('0').closest('.form-group').hide();
            _$form.find('#MaterialUomId').val('').change().closest('.form-group').hide();
            _$form.find('#MaterialQuantity').val('').closest('.form-group').hide();
        }
        function enableMaterialFields() {
            _$form.find("label[for=MaterialUomId]").addClass('required-label');
            _$form.find('#MaterialPricePerUnit').closest('.form-group').show();
            _$form.find('#MaterialPrice').closest('.form-group').show();
            _$form.find('#MaterialUomId').closest('.form-group').show();
            _$form.find('#MaterialQuantity').closest('.form-group').show();
        }
        function disableFreightFields() {
            _$form.find("label[for=FreightUomId]").removeClass('required-label');
            _$form.find('#FreightPricePerUnit').val('').closest('.form-group').hide();
            _$form.find('#FreightRateToPayDrivers').val('').closest('.form-group').hide();
            _$form.find('#FreightPrice').val('0').closest('.form-group').hide();
            _$form.find('#FreightUomId').val('').change().closest('.form-group').hide();
            _$form.find('#FreightQuantity').val('').closest('.form-group').hide();
        }
        function enableFreightFields() {
            _$form.find("label[for=FreightUomId]").addClass('required-label');
            _$form.find('#FreightPricePerUnit').closest('.form-group').show();
            if (abp.setting.getBoolean('App.TimeAndPay.AllowDriverPayRateDifferentFromFreightRate')) {
                _$form.find('#FreightRateToPayDrivers').closest('.form-group').show();
            }
            _$form.find('#FreightPrice').closest('.form-group').show();
            _$form.find('#FreightUomId').closest('.form-group').show();
            _$form.find('#FreightQuantity').closest('.form-group').show();
        }

        function updateProductionPay() {
            let productionPay = _$form.find('#ProductionPay');
            let productionPayContainer = productionPay.closest('.form-group');
            if (designationIsMaterialOnly()) {
                if (_wasProductionPay === null) {
                    _wasProductionPay = productionPay.is(':checked');
                }
                productionPay.prop('checked', false);
                productionPayContainer.hide();
            } else {
                if (_wasProductionPay !== null) {
                    if (shouldDisableProductionPay()) {
                        disableProductionPay();
                    } else {
                        productionPay.prop('checked', _wasProductionPay);
                    }
                    _wasProductionPay = null;
                }
                productionPayContainer.show();
            }
        }

        async function checkForOrderDuplicates(model, callback) {
            if (model.orderId) {
                callback();
                return;
            }

            _modalManager.setBusy(true);
            try {
                let duplicateCount = await _orderAppService.getOrderDuplicateCount({
                    id: model.orderId,
                    customerId: model.customerId,
                    quoteId: model.quoteId,
                    deliveryDate: model.deliveryDate
                });


                if (duplicateCount > 0) {
                    var customerName = _$form.find("#CustomerId").getSelectedDropdownOption().text();
                    if (!await abp.message.confirm(
                        'You already have an order scheduled for ' + model.deliveryDate + ' for ' + customerName + '. Are you sure you want to save this order?'
                    )) {
                        return;
                    }
                }

                callback();
            }
            finally {
                _modalManager.setBusy(false);
            }
        }

        function hasMissingQuantityOrNumberOfTrucks(model) {
            var designation = Number(_designationDropdown.val());
            var designationIsCounterSale = designation === abp.enums.designation.materialOnly && _allowCounterSales;
            if (designationIsCounterSale) {
                if (model.materialQuantity || model.freightQuantity) {
                    return false;
                }
            } else {
                if (model.materialQuantity || model.freightQuantity || model.numberOfTrucks) {
                    return false;
                }
            }
            return true;
        }

        var saveJobAsync = async function (callback) {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var model = _$form.serializeFormToObject();
            model.OrderId = model.OrderId ? Number(model.OrderId) : null;
            model.OrderLineId = model.OrderLineId ? Number(model.OrderLineId) : null;
            model.QuoteId = model.QuoteId ? Number(model.QuoteId) : null;
            model.QuoteServiceId = model.QuoteServiceId ? Number(model.QuoteServiceId) : null;

            if (!parseFloat(model.MaterialQuantity) && parseFloat(model.MaterialPrice)
                || !parseFloat(model.FreightQuantity) && parseFloat(model.FreightPrice)) {
                abp.message.error(app.localize('QuantityIsRequiredWhenTotalIsSpecified'));
                return;
            }

            if (model.RequiresCustomerNotification && (model.CustomerNotificationContactName === "" || model.CustomerNotificationPhoneNumber === "")) {
                abp.message.error('Please check the following: \n'
                    + (model.CustomerNotificationContactName ? '' : '"Contact Name" - This field is required.\n')
                    + (model.CustomerNotificationPhoneNumber ? '' : '"Phone Number" - This field is required.\n'), 'Some of the data is invalid');
                return;
            }

            if (model.CustomerNotificationPhoneNumber) {
                if (!_$form.find("#CustomerNotificationPhoneNumber")[0].checkValidity()) {
                    abp.message.error(app.localize('IncorrectPhoneNumberFormatError'));
                    return;
                }
            }

            if (!validateFields(model)) {
                return;
            }

            if (Number(model.StaggeredTimeKind) !== abp.enums.staggeredTimeKind.none) {
                model.TimeOnJob = null;
            }

            _model = _model || {};
            _model.orderId = model.OrderId;
            _model.orderLineId = model.OrderLineId;
            _model.quoteId = model.QuoteId;
            _model.quoteServiceId = model.QuoteServiceId;
            _model.isMaterialPricePerUnitOverridden = model.IsMaterialPricePerUnitOverridden === "True";
            _model.isFreightPricePerUnitOverridden = model.IsFreightPricePerUnitOverridden === "True";
            _model.isMaterialPriceOverridden = model.IsMaterialPriceOverridden === "True";
            _model.isFreightPriceOverridden = model.IsFreightPriceOverridden === "True";
            _model.isTaxable = model.IsTaxable === "True";
            _model.staggeredTimeKind = Number(model.StaggeredTimeKind) || 0;
            //_model.lineNumber = Number(model.LineNumber); //?
            _model.deliveryDate = model.DeliveryDate;
            _model.customerId = model.CustomerId;
            _model.chargeTo = model.ChargeTo;
            _model.poNumber = model.PONumber;
            _model.spectrumNumber = model.SpectrumNumber;
            _model.directions = model.Directions;
            _model.salesTaxRate = model.SalesTaxRate;
            _model.salesTax = model.SalesTax;
            _model.priority = model.Priority;
            _model.shift = model.Shift;
            _model.officeId = model.OfficeId;
            _model.locationId = model.LocationId;
            _model.projectId = model.ProjectId;
            _model.contactId = model.ContactId;
            _model.designation = model.Designation;
            _model.designationName = Number(model.Designation) ? _$form.find("#JobDesignation option:selected").text() : null;
            _model.loadAtId = model.LoadAtId;
            _model.loadAtName = Number(model.LoadAtId) ? _$form.find("#LoadAtId option:selected").text() : null;
            _model.deliverToId = model.DeliverToId;
            _model.deliverToName = Number(model.DeliverToId) ? _$form.find("#DeliverToId option:selected").text() : null;
            _model.serviceId = model.ServiceId;
            _model.serviceName = Number(model.ServiceId) ? _$form.find("#JobServiceId option:selected").text() : null;
            _model.materialUomId = model.MaterialUomId;
            _model.materialUomName = Number(model.MaterialUomId) ? _$form.find("#MaterialUomId option:selected").text() : null;
            _model.freightUomId = model.FreightUomId;
            _model.freightUomName = Number(model.FreightUomId) ? _$form.find("#FreightUomId option:selected").text() : null;
            _model.materialPricePerUnit = Number(model.MaterialPricePerUnit) || 0;
            _model.freightPricePerUnit = Number(model.FreightPricePerUnit) || 0;
            _model.freightRateToPayDrivers = Number(model.FreightRateToPayDrivers) || 0;
            _model.leaseHaulerRate = Number(model.LeaseHaulerRate) || 0;
            _model.materialQuantity = Number(model.MaterialQuantity) || 0;
            _model.freightQuantity = Number(model.FreightQuantity) || 0;
            _model.materialPrice = Number(model.MaterialPrice) || 0;
            _model.freightPrice = Number(model.FreightPrice) || 0;
            _model.numberOfTrucks = Number(model.NumberOfTrucks) || 0;
            _model.isMultipleLoads = !!model.IsMultipleLoads;
            _model.productionPay = !!model.ProductionPay;
            _model.timeOnJob = model.TimeOnJob;
            _model.jobNumber = model.JobNumber;
            _model.note = model.Note;
            _model.autoGenerateTicketNumber = !!model.AutoGenerateTicketNumber;
            _model.ticketId = model.TicketId;
            _model.ticketNumber = model.TicketNumber;
            _model.fuelSurchargeCalculationId = model.FuelSurchargeCalculationId;
            _model.baseFuelCost = model.BaseFuelCost;
            _model.requiresCustomerNotification = !!model.RequiresCustomerNotification;
            _model.customerNotificationContactName = model.CustomerNotificationContactName;
            _model.customerNotificationPhoneNumber = model.CustomerNotificationPhoneNumber;

            let materialQuantity = model.MaterialQuantity === "" ? null : abp.utils.round(parseFloat(model.MaterialQuantity));
            let freightQuantity = model.FreightQuantity === "" ? null : abp.utils.round(parseFloat(model.FreightQuantity));
            let numberOfTrucks = model.NumberOfTrucks === "" ? null : abp.utils.round(parseFloat(model.NumberOfTrucks));

            if (model.OrderLineId && !await abp.scheduling.checkExistingDispatchesBeforeSettingQuantityAndNumberOfTrucksZero(model.OrderLineId, materialQuantity, freightQuantity, numberOfTrucks)) {
                _modalManager.close();
                return;
            }

            if (hasMissingQuantityOrNumberOfTrucks(_model) && !abp.setting.getBoolean('App.UserOptions.DontShowZeroQuantityWarning')) {
                if (!await abp.message.confirm(app.localize('MissingQtyOrNbrOfTrucksOnJobConfirmation'))) {
                    return;
                }
            }

            checkForOrderDuplicates(_model, function () {
                _modalManager.setBusy(true);
                _orderAppService.editJob(_model).done(function (result) {
                    if (!result.completed) {
                        abp.helper.showTruckWarning(result.notAvailableTrucks, 'already scheduled for this date. Do you want to continue with remaining trucks?',
                            function (isConfirmed) {
                                if (isConfirmed) {
                                    _model.removeNotAvailableTrucks = true;
                                    _orderAppService.editJob(_model).done(function (result) {
                                        notifyAndFinish(result);
                                    });
                                }
                            }
                        );
                    } else {
                        notifyAndFinish(result);
                    }

                    function notifyAndFinish(result) {
                        abp.notify.info('Saved successfully.');
                        abp.event.trigger('app.createOrEditJobModalSaved', result);
                        if (callback) {
                            callback(result);
                        }
                    }

                }).always(function () {
                    _modalManager.setBusy(false);
                });
            });
        }

        function validateFields(orderLine) {
            var isFreightUomValid = true;
            if (!designationIsMaterialOnly()) {
                if (!Number(orderLine.FreightUomId)) {
                    isFreightUomValid = false;
                }
            }

            var isMaterialUomValid = true;
            if (designationHasMaterial()) {
                if (!Number(orderLine.MaterialUomId)) {
                    isMaterialUomValid = false;
                }
            }

            if (!isFreightUomValid
                || !isMaterialUomValid) {
                abp.message.error('Please check the following: \n'
                    + (isMaterialUomValid ? '' : '"Material UOM" - This field is required.\n')
                    + (isFreightUomValid ? '' : '"Freight UOM" - This field is required.\n'), 'Some of the data is invalid');
                return false;
            }
            return true;
        }

    };
})(jQuery);