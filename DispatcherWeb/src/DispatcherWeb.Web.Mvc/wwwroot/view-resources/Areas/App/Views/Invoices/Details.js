(function () {
    $(function () {

        var _invoiceService = abp.services.app.invoice;
        var _dtHelper = abp.helper.dataTables;
        var _invoiceId = $("#Id").val();
        var _invoiceLines = null;
        var _invoiceLineGridData = null;
        var _customerInvoicingMethod = $('#CustomerInvoicingMethod').val() ? Number($('#CustomerInvoicingMethod').val()) : null;
        var showFuelSurchargeOnInvoice = Number($("#ShowFuelSurchargeOnInvoice").val());
        var showBottomFuelSurchargeLine = showFuelSurchargeOnInvoice === abp.enums.showFuelSurchargeOnInvoiceEnum.singleLineItemAtTheBottom;
        var showFuelSurchargeLinePerTicket = showFuelSurchargeOnInvoice === abp.enums.showFuelSurchargeOnInvoiceEnum.lineItemPerTicket;
        var form = $("#InvoiceForm");

        $('form').validate();
        $.validator.addMethod(
            "regex",
            function (value, element, regexp) {
                var re = new RegExp(regexp, 'i');
                return this.optional(element) || re.test(value);
            },
            "Please check your input."
        );
        $('#EmailAddress').rules('add', { regex: app.regex.emails });

        var _selectCustomerTicketsModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Invoices/SelectCustomerTicketsModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Invoices/_SelectCustomerTicketsModal.js',
            modalClass: 'SelectCustomerTicketsModal',
            modalSize: 'lg'
        });

        var _createOrEditTicketModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Tickets/CreateOrEditTicketModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Tickets/_CreateOrEditTicketModal.js',
            modalClass: 'CreateOrEditTicketModal'
        });

        var _emailInvoicePrintOutModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Invoices/EmailInvoicePrintOutModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Invoices/_EmailInvoicePrintOutModal.js',
            modalClass: 'EmailInvoicePrintOutModal'
        });

        $("#IssueDate").datepickerInit();
        $("#IssueDate").on('dp.change', function () {
            calculateDueDate();
        });
        $("#DueDate").datepickerInit();

        async function calculateDueDate() {
            var issueDate = $("#IssueDate").val();
            if (!issueDate) {
                return;
            }
            var terms = $("#Terms").val();
            var dueDate = await abp.services.app.invoice.calculateDueDate({
                issueDate: issueDate,
                terms: terms
            });
            if (dueDate) {
                $("#DueDate").val(moment(dueDate).utc().format('L'));
            }
        }

        var saveInvoiceAsync = function (callback) {
            if (!form.valid()) {
                form.showValidateMessage();
                return;
            }

            removeEmptyInvoiceLines();

            var invoice = form.serializeFormToObject();
            invoice.InvoiceLines = _invoiceLines;
            abp.ui.setBusy(form);
            _invoiceService.editInvoice(invoice).done(function (id) {
                abp.notify.info('Saved successfully.');
                _invoiceId = id;
                $("#Id").val(_invoiceId);
                history.replaceState({}, "", abp.appPath + 'app/invoices/details/' + _invoiceId);
                $("#InvoiceForm").dirtyForms('setClean');
                if (callback)
                    callback();
            }).always(function () {
                abp.ui.clearBusy(form);
            });
        };

        $("#CustomerId").select2Init({
            abpServiceMethod: abp.services.app.invoice.getActiveCustomersSelectList,
            showAll: false,
            allowClear: false
        });

        $("#CustomerId").change(function () {
            let dropdown = $("#CustomerId");
            let dropdownData = dropdown.select2('data');
            if (dropdownData && dropdownData.length) {
                if (dropdownData[0].item) {
                    let i = dropdownData[0].item;
                    $("#EmailAddress").val(i.invoiceEmail);
                    $("#BillingAddress").val(i.fullAddress);
                    $("#Terms").val(i.terms).change();
                    _customerInvoicingMethod = i.invoicingMethod;
                    $("#CustomerInvoicingMethod").val(i.invoicingMethod);
                }
                $(this).removeUnselectedOptions();
            }

            refreshAddUnbilledTicketsVisibility();
            refreshJobNumberVisibilityAndText();
        });
        refreshAddUnbilledTicketsVisibility();

        function refreshAddUnbilledTicketsVisibility() {
            $("#AddUnbilledTicketsButton").hide();
            var customerId = $("#CustomerId").val();
            if (customerId && _invoiceLines) {
                let filter = getCustomerTicketFilter(customerId);
                _invoiceService.getCustomerHasTickets(filter).done(function (hasTickets) {
                    if (!hasTickets || $("#CustomerId").val() !== customerId) {
                        return;
                    }
                    $("#AddUnbilledTicketsButton").show();
                });
            }
        }

        function refreshJobNumberVisibilityAndText() {
            //$('#JobNumberBlock h4').text(getSelectedJobNumbers().join('; '));
            //$('#JobNumberBlock').toggle(_customerInvoicingMethod === abp.enums.invoicingMethod.separateTicketsByJobNumber);
            var jobNumbers = getSelectedJobNumbers().join('; ');
            if (jobNumbers) {
                let maxLength = Number($("#JobNumber").attr('maxlength'));
                jobNumbers = abp.utils.truncate(jobNumbers, maxLength, true);
                $("#JobNumber").val(jobNumbers);
            }
            var poNumbers = getSelectedPoNumbers().join('; ');
            if (poNumbers) {
                let maxLength = Number($("#PoNumber").attr('maxlength'));
                jobNumbers = abp.utils.truncate(jobNumbers, maxLength, true);
                $("#PoNumber").val(poNumbers);
            }
        }

        $("#Terms").select2Init({
            showAll: true,
            allowClear: true
        });
        $("#Terms").change(function () {
            calculateDueDate();
        });

        function disableCustomerDropdownIfNeeded() {
            if (_invoiceLines && _invoiceLines.filter(x => x.ticketId).length) {
                $("#CustomerId").prop('disabled', true);
            }
        }

        function setFormDirty() {
            var dirtyFormsField = $("#DirtyFormsField");
            var i = Number(dirtyFormsField.val());
            dirtyFormsField.val(++i).change();
        }

        function recalculateLineNumbers() {
            if (!_invoiceLines) {
                return;
            }
            let i = 1;
            _invoiceLines.forEach(x => x.lineNumber = i++)
        }

        function round(num) {
            return abp.utils.round(num);
        }

        function serializeInvoice() {
            //_receipt = $("#InvoiceForm").serializeFormToObject();
            //_receipt.FreightTotal = Number(_receipt.FreightTotal) || 0;
            //_receipt.MaterialTotal = Number(_receipt.MaterialTotal) || 0;
            //_receipt.SalesTaxRate = Number(_receipt.SalesTaxRate) || 0;
        }
        serializeInvoice();

        $('#TaxRate').change(function () {
            recalculateTotals();
            reloadInvoiceLinesGrid();
        });

        //var _recalculateTotalsInProgressCount = 0;
        function recalculateTotals(sender) {
            let senderRowData = sender ? _dtHelper.getRowData(sender) : null;
            if (!_invoiceLines) {
                return;
            }
            let totalTax = 0;
            let subtotal = 0;
            let totalAmount = 0;
            let taxRate = round($('#TaxRate').val());
            _invoiceLines.forEach(function (invoiceLine) {
                if (!invoiceLine.ticketId) {
                    invoiceLine.materialExtendedAmount = abp.utils.round((invoiceLine.quantity || 0) * (invoiceLine.materialRate || 0));
                    invoiceLine.freightExtendedAmount = abp.utils.round((invoiceLine.quantity || 0) * (invoiceLine.freightRate || 0));
                }
                let calcResult = abp.helper.calculateOrderLineTotal(invoiceLine.materialExtendedAmount, invoiceLine.freightExtendedAmount, invoiceLine.isTaxable !== false, taxRate); //isTaxable == null should be processed as true
                invoiceLine.tax = calcResult.tax;
                invoiceLine.subtotal = calcResult.subtotal || 0;
                invoiceLine.extendedAmount = calcResult.total || 0;
                if (senderRowData === invoiceLine) {
                    if (!invoiceLine.ticketId) {
                        $(sender).closest('tr').find('.freight-total-cell').text(invoiceLine.freightExtendedAmount);
                        $(sender).closest('tr').find('.material-total-cell').text(invoiceLine.materialExtendedAmount);
                        $(sender).closest('tr').find('.description-cell textarea').val(invoiceLine.description);
                    }
                    $(sender).closest('tr').find('.total-cell').text(_dtHelper.renderMoney(invoiceLine.subtotal));
                    $(sender).closest('tr').find('.tax-cell input').prop('checked', invoiceLine.tax > 0);
                }
                subtotal += invoiceLine.subtotal || 0;
                totalTax += invoiceLine.tax || 0;
                totalAmount += invoiceLine.extendedAmount || 0;
            });
            $(".Subtotal").text(abp.helper.dataTables.renderMoney(subtotal));
            $(".TaxAmount").text(abp.helper.dataTables.renderMoney(totalTax));
            $(".BalanceDue").text(abp.helper.dataTables.renderMoney(totalAmount));
        }

        $("#InvoiceForm").dirtyForms(); //{ ignoreSelector: '' }
        //$("#InvoiceForm").uniform();


        //function getTicketFromRowData(rowData) {
        //    return {
        //        id: rowData.id,
        //        orderLineId: rowData.orderLineId,
        //        ticketNumber: rowData.ticketNumber,
        //        ticketDateTime: rowData.ticketDateTime,
        //        materialQuantity: rowData.materialQuantity,
        //        freightQuantity: rowData.freightQuantity,
        //        truckId: rowData.truckId,
        //        truckCode: rowData.truck //api is expecting 'truckCode' on edit, but sends 'truck' in grid data
        //    };
        //}


        function addEmptyRowIfNeeded() {
            if (!_invoiceLines.filter(x => !x.childInvoiceLineKind).length) {
                addEmptyInvoiceLine();
            }
        }

        var invoiceLinesTable = $('#InvoiceLinesTable');
        var invoiceLinesGrid = invoiceLinesTable.DataTableInit({
            paging: false,
            info: false,
            serverSide: true,
            ordering: false,
            processing: true,
            ajax: function (data, callback, settings) {
                if (_invoiceLineGridData) {
                    callback(_invoiceLineGridData);
                    return;
                }
                if (_invoiceId) {
                    var abpData = _dtHelper.toAbpData(data);
                    $.extend(abpData, { invoiceId: _invoiceId });
                    _invoiceService.getInvoiceLines(abpData).done(function (abpResult) {
                        _invoiceLineGridData = _dtHelper.fromAbpResult(abpResult);
                        _invoiceLines = abpResult.items;
                        addEmptyRowIfNeeded();
                        disableCustomerDropdownIfNeeded();
                        refreshAddUnbilledTicketsVisibility();
                        //refreshJobNumberVisibilityAndText();
                        //recalculateTotals();
                        callback(_invoiceLineGridData);
                    });
                } else {
                    _invoiceLineGridData = _dtHelper.getEmptyResult();
                    _invoiceLines = _invoiceLineGridData.data;
                    addEmptyRowIfNeeded();
                    refreshAddUnbilledTicketsVisibility();
                    //refreshJobNumberVisibilityAndText();
                    callback(_invoiceLineGridData);
                }

            },
            editable: {
                saveCallback: function (rowData, cell) {
                    setFormDirty();
                    recalculateTotals(cell);
                },
                isReadOnly: (rowData) => !!rowData.childInvoiceLineKind,
            },
            columns: [
                {
                    width: '20px',
                    className: 'control responsive',
                    orderable: false,
                    render: function () {
                        return '';
                    }
                },
                {
                    data: "ticketNumber",
                    title: "Ticket #"
                    //className: "cell-text-wrap all",
                },
                {
                    data: "truckCode",
                    render: function (data, type, full, meta) {
                        return (full.leaseHaulerName ? _dtHelper.renderText(full.leaseHaulerName) + ' ' : '')
                            + (_dtHelper.renderText(full.truckCode) || '');
                    },
                    title: "Truck"
                },
                {
                    data: "jobNumber",
                    title: "Job #",
                    editable: {
                        editor: _dtHelper.editors.text,
                        maxLength: abp.entityStringFieldLengths.orderLine.jobNumber,
                        isReadOnly: function (rowData, isRowReadOnly) {
                            return isRowReadOnly || rowData.ticketId;
                        },
                        editCompleteCallback: function (editResult, rowData, $cell) {
                            refreshJobNumberVisibilityAndText();
                        }
                    }
                },
                {
                    data: "deliveryDateTime",
                    render: function (data, type, full, meta) { return _dtHelper.renderDate(full.deliveryDateTime); }, //utc datetime
                    title: "Date"
                },
                {
                    data: "itemName",
                    title: "Item",
                    className: "all item-cell",
                    editable: {
                        editor: _dtHelper.editors.dropdown,
                        idField: 'itemId',
                        nameField: 'itemName',
                        dropdownOptions: {
                            abpServiceMethod: abp.services.app.service.getServicesWithTaxInfoSelectList,
                            showAll: false,
                            allowClear: false
                        },
                        editStartingCallback: function editStartingCallback(rowData, cell, selectedOption) {
                            console.log(selectedOption);
                            if (selectedOption && selectedOption.item) {
                                rowData.isTaxable = selectedOption.item.isTaxable;
                                rowData.description = selectedOption.name;
                            }
                        },
                        isReadOnly: function (rowData, isRowReadOnly) {
                            return isRowReadOnly || rowData.ticketId;
                        }
                    }
                },
                {
                    data: "description",
                    title: "Description",
                    width: '240px',
                    className: "all description-cell",
                    editable: {
                        editor: _dtHelper.editors.textarea,
                        maxLength: 1000,
                        isReadOnly: function (rowData, isRowReadOnly) {
                            return isRowReadOnly || rowData.ticketId;
                        }
                    }
                },
                {
                    data: "quantity",
                    title: "Qty",
                    className: "all quantity-cell",
                    editable: {
                        editor: _dtHelper.editors.decimal,
                        maxValue: app.consts.maxQuantity,
                        minValue: 0,
                        allowNull: false,
                        isReadOnly: function (rowData, isRowReadOnly) {
                            return isRowReadOnly || rowData.ticketId;
                        }
                    }
                },
                {
                    data: "materialRate",
                    title: "Material<br>Rate",
                    className: "all material-rate-cell",
                    editable: {
                        editor: _dtHelper.editors.decimal,
                        maxValue: app.consts.maxDecimal,
                        minValue: 0,
                        allowNull: true,
                        isReadOnly: function (rowData, isRowReadOnly) {
                            return isRowReadOnly || rowData.ticketId;
                        }
                    }
                },
                {
                    data: "freightRate",
                    title: "Freight<br>Rate",
                    className: "all freight-rate-cell",
                    editable: {
                        editor: _dtHelper.editors.decimal,
                        maxValue: app.consts.maxDecimal,
                        minValue: 0,
                        allowNull: true,
                        isReadOnly: function (rowData, isRowReadOnly) {
                            return isRowReadOnly || rowData.ticketId;
                        }
                    }
                },
                {
                    data: "materialExtendedAmount",
                    //render: function (data, type, full, meta) { return _dtHelper.renderMoney(full.materialExtendedAmount); },
                    title: "Material",
                    className: "all material-total-cell",
                    editable: {
                        editor: _dtHelper.editors.decimal,
                        maxValue: app.consts.maxDecimal,
                        minValue: 0,
                        allowNull: false,
                        isReadOnly: function (rowData, isRowReadOnly) {
                            return true; //isRowReadOnly || !rowData.ticketId;
                        }
                    }
                },
                {
                    data: "freightExtendedAmount",
                    //render: function (data, type, full, meta) { return _dtHelper.renderMoney(full.freightExtendedAmount); },
                    title: "Freight",
                    className: "all freight-total-cell",
                    editable: {
                        editor: _dtHelper.editors.decimal,
                        maxValue: 1000000,
                        minValue: 0,
                        allowNull: false,
                        isReadOnly: function (rowData, isRowReadOnly) {
                            return true; //isRowReadOnly || !rowData.ticketId;
                        }
                    }
                },
                //{
                //    data: "fuelSurcharge",
                //    title: "Fuel",
                //    visible: abp.setting.getBoolean('App.Fuel.ShowFuelSurcharge')
                //},
                {
                    title: "Tax",
                    data: null,
                    render: function (data, type, full, meta) { return _dtHelper.renderCheckbox(full.tax > 0); },
                    className: "all checkmark text-center tax-cell",
                    //render: function (data, type, full, meta) { return full.isTaxable; },
                    //data: "isTaxable",
                },
                {
                    data: "subtotal",
                    class: "total-cell",
                    render: function (data, type, full, meta) { return _dtHelper.renderMoney(full.subtotal); },
                    title: "Total"
                },
                {
                    data: null,
                    orderable: false,
                    autoWidth: false,
                    width: "10px",
                    responsivePriority: 1,
                    render: function (data, type, full, meta) {
                        return full.childInvoiceLineKind ? '' : '<div class="dropdown action-button">'
                            + '<ul class="dropdown-menu dropdown-menu-right">'
                            + '<li><a class="btnDeleteRow" title="Delete"><i class="fa fa-trash"></i> Delete</a></li>'
                            + (full.ticketId ? '<li><a class="btnViewAssociatedTicket" title="View associated ticket"><i class="fa fa-edit"></i> View associated ticket</a></li>' : '')
                            + '</ul>'
                            + '<button class="btn btn-primary btn-sm" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-ellipsis-h"></i></button>'
                            + '</div>';
                    }

                }
            ]
        });

        var reloadInvoiceLinesGrid = function () {
            invoiceLinesGrid.ajax.reload();
        };

        //function getNextLineNumber() {
        //    return _invoiceLines.map(function (x) { return x.lineNumber; }).reduce((a, b) => a > b ? a : b, 0) + 1;
        //}

        $("#AddInvoiceLineButton").click(function (e) {
            e.preventDefault();
            addEmptyInvoiceLine();
            reloadInvoiceLinesGrid();
            setFormDirty();
        });

        function getBottomFuelSurchargeLine() {
            if (!_invoiceLines) {
                return null;
            }
            return _invoiceLines.find(x => x.childInvoiceLineKind === abp.enums.childInvoiceLineKind.bottomFuelSurchargeLine);
        }

        function addInvoiceLineInternal(line) {
            if (showBottomFuelSurchargeLine) {
                let bottomFuelLine = getBottomFuelSurchargeLine();
                let bottomFuelSurchargeLineIndex = _invoiceLines.indexOf(bottomFuelLine);
                if (bottomFuelSurchargeLineIndex !== -1) {
                    _invoiceLines.splice(bottomFuelSurchargeLineIndex, 0, line);
                    setAmountToFuelSurchargeLine(bottomFuelLine, bottomFuelLine.extendedAmount + line.fuelSurcharge);
                    if (bottomFuelLine.extendedAmount === 0) {
                        _invoiceLines.splice(_invoiceLines.indexOf(bottomFuelLine), 1);
                    }
                } else {
                    _invoiceLines.push(line);
                    bottomFuelLine = getNewBottomFuelSurchargeLine();
                    if (bottomFuelLine.extendedAmount !== 0) {
                        _invoiceLines.push(bottomFuelLine);
                    }
                }
            } else if (showFuelSurchargeLinePerTicket) {
                _invoiceLines.push(line);
                if (line.fuelSurcharge) {
                    _invoiceLines.push(getNewTicketFuelSurchargeLine(line));
                }
            } else {
                _invoiceLines.push(line);
            }
        }

        function getFuelService() {
            var fuelService = {
                id: Number($("#FuelServiceId").val()),
                name: $("#FuelServiceName").val(),
                isTaxable: $("#FuelServiceIsTaxable").val().toLowerCase() === 'true',
            };
            if (!fuelService.id) {
                abp.message.error(app.localize('PleaseSelectItemToUseForFuelSurchargeOnInvoiceInSettings')).then(() => {
                    location = abp.appPath + '/Settings';
                });
                throw new Error('FuelServiceId is not set');
            }
            return fuelService;
        }

        function setItemToFuelSurchargeLine(fuelLine) {
            let fuelService = getFuelService();
            fuelLine.description = fuelService.name;
            fuelLine.itemId = fuelService.id;
            fuelLine.itemName = fuelService.name;
            fuelLine.isTaxable = fuelService.isTaxable;
        }

        function setAmountToFuelSurchargeLine(fuelLine, fuelAmount) {
            fuelLine.quantity = 1;
            fuelLine.freightRate = fuelAmount;
            fuelLine.freightExtendedAmount = fuelAmount;
            fuelLine.subtotal = fuelAmount;
            fuelLine.extendedAmount = fuelAmount;
        }

        function getNewBottomFuelSurchargeLine() {
            if (!showBottomFuelSurchargeLine) {
                return null;
            }
            let fuelLine = getEmptyInvoiceLine();
            setItemToFuelSurchargeLine(fuelLine);
            setAmountToFuelSurchargeLine(fuelLine, _invoiceLines.map(x => x.fuelSurcharge).reduce((a, b) => a + b, 0));
            fuelLine.deliveryDateTime = null;
            fuelLine.childInvoiceLineKind = abp.enums.childInvoiceLineKind.bottomFuelSurchargeLine;
            return fuelLine;
        }

        function getNewTicketFuelSurchargeLine(invoiceLine) {
            if (!showFuelSurchargeLinePerTicket) {
                return null;
            }
            invoiceLine.guid = invoiceLine.guid || app.guid();
            let fuelLine = getEmptyInvoiceLine();
            setItemToFuelSurchargeLine(fuelLine);
            setAmountToFuelSurchargeLine(fuelLine, invoiceLine.fuelSurcharge);
            fuelLine.deliveryDateTime = invoiceLine.deliveryDateTime;
            fuelLine.parentInvoiceLineGuid = invoiceLine.guid;
            fuelLine.childInvoiceLineKind = abp.enums.childInvoiceLineKind.fuelSurchargeLinePerTicket;
            return fuelLine;
        }

        function getEmptyInvoiceLine() {
            return {
                //isNew: true,
                id: 0,
                lineNumber: 0, //getNextLineNumber(),
                carrierId: null,
                carrierName: null,
                deliveryDateTime: null,
                description: null,
                itemId: null,
                itemName: null,
                quantity: 0,
                freightRate: null,
                materialRate: null,
                subtotal: 0,
                extendedAmount: 0,
                freightExtendedAmount: 0,
                leaseHaulerName: null,
                materialExtendedAmount: 0,
                tax: 0,
                isTaxable: true,
                ticketId: null,
                ticketNumber: null,
                truckCode: null,
                jobNumber: null,
                fuelSurcharge: 0,
                guid: null,
                parentInvoiceLineGuid: null,
                parentInvoiceLineId: null,
                childInvoiceLineKind: null
            };
        }

        function addEmptyInvoiceLine() {
            var newInvoiceLine = getEmptyInvoiceLine();
            addInvoiceLineInternal(newInvoiceLine);
            recalculateLineNumbers();
            //disableCustomerDropdownIfNeeded();
            //recalculateTotals();
        }

        $(".SaveInvoiceButton").click(function (e) {
            e.preventDefault();
            saveInvoiceAsync(function () {
                //reloadInvoiceLinesGrid();
                setTimeout(() => abp.ui.setBusy(form), 100);
                location.reload();
            });
        });

        $("#MarkReadyForExportButton").click(function (e) {
            e.preventDefault();
            $("#Status").val(abp.enums.invoiceStatus.readyForExport);
            saveInvoiceAsync(function () {
                //reloadInvoiceLinesGrid();
                setTimeout(() => abp.ui.setBusy(form), 100);
                location.reload();
            });
        });

        $("#SaveAndPrintButton").click(function (e) {
            e.preventDefault();
            let reportWindow = window.open('');
            saveInvoiceAsync(function () {
                reportWindow.location = abp.appPath + 'app/invoices/GetInvoicePrintOut?invoiceId=' + _invoiceId;
                reportWindow.focus();
                setTimeout(() => abp.ui.setBusy(form), 100);
                setTimeout(() => location.reload(), 3000);
            });
        });

        $("#SaveAndSendButton").click(function (e) {
            e.preventDefault();
            saveInvoiceAsync(function () {
                _emailInvoicePrintOutModal.open({ id: _invoiceId });
            });
        });

        abp.event.on('app.emailInvoicePrintOutModalSent', function (e) {
            abp.ui.setBusy(form);
            location.reload();
        });

        function getCustomerTicketFilter(customerId) {
            let filter = {
                customerId: customerId,
                isBilled: false,
                isVerified: true,
                hasInvoiceLineId: false,
                hasRevenue: true
            };
            if (_invoiceLines) {
                let excludeTicketIds = _invoiceLines.filter(x => x.ticketId !== null).map(x => x.ticketId);
                if (excludeTicketIds.length) {
                    filter.excludeTicketIds = excludeTicketIds;
                }
            }
            return filter;
        }

        function getSelectedJobNumbers() {
            let jobNumbers = [];
            if (_invoiceLines) {
                _invoiceLines.map(l => l.jobNumber).filter(l => l).forEach(j => {
                    if (!jobNumbers.includes(j)) {
                        jobNumbers.push(j);
                    }
                });
            }
            return jobNumbers;
        }

        function getSelectedPoNumbers() {
            let poNumbers = [];
            if (_invoiceLines) {
                _invoiceLines.map(l => l.poNumber).filter(l => l).forEach(j => {
                    if (!poNumbers.includes(j)) {
                        poNumbers.push(j);
                    }
                });
            }
            return poNumbers;
        }

        $("#AddUnbilledTicketsButton").click(function (e) {
            e.preventDefault();
            var customerId = $("#CustomerId").val();
            if (!customerId) {
                //shouldn't happen in production as the button would be hidden when there's no customer
                abp.message.warn('Please select the customer first');
                return;
            }
            _selectCustomerTicketsModal.open().then(($modal, modal) => {
                let filter = getCustomerTicketFilter(customerId);
                let selectedJobNumbers = getSelectedJobNumbers();
                modal.setFilter(filter, _customerInvoicingMethod, selectedJobNumbers);
            });
        });

        abp.event.on('app.customerTicketsSelectedModal', function (e) {
            //console.log({ tickets: e.selectedTickets });
            //let nextLineNumber = getNextLineNumber();
            let newItems = e.selectedTickets.map(x => ({
                id: 0,
                lineNumber: 0, //nextLineNumber++,
                carrierId: x.carrierId,
                carrierName: x.carrierName,
                deliveryDateTime: x.ticketDateTime,
                description: x.description,
                itemId: x.serviceId,
                itemName: x.serviceName,
                quantity: x.quantity,
                freightRate: x.freightRate,
                materialRate: x.materialRate,
                subtotal: x.subtotal,
                extendedAmount: x.total,
                freightExtendedAmount: x.freightTotal,
                leaseHaulerName: x.leaseHaulerName,
                materialExtendedAmount: x.materialTotal,
                tax: x.tax,
                isTaxable: x.isTaxable,
                ticketId: x.id,
                ticketNumber: x.ticketNumber,
                truckCode: x.truckCode,
                jobNumber: x.jobNumber,
                poNumber: x.poNumber,
                fuelSurcharge: x.fuelSurcharge,
                guid: null,
                parentInvoiceLineGuid: null,
                parentInvoiceLineId: null,
                childInvoiceLineKind: null
            }));
            newItems.forEach(x => addInvoiceLineInternal(x));
            recalculateLineNumbers();
            setFormDirty();
            disableCustomerDropdownIfNeeded();
            //console.log({_invoiceLines});
            recalculateTotals();
            reloadInvoiceLinesGrid();
            refreshAddUnbilledTicketsVisibility();
            refreshJobNumberVisibilityAndText();
            setTimeout(function () {
                //fixes the issue with colspan being applied to the first cell sometimes for some reason
                reloadInvoiceLinesGrid();
            }, 1000);
        });

        function getInvoiceStatus() {
            return Number($("#Status").val());
        }

        function isDraftInvoice() {
            return getInvoiceStatus() === abp.enums.invoiceStatus.draft;
        }

        invoiceLinesTable.on('click', '.btnViewAssociatedTicket', function (e) {
            e.preventDefault();
            var invoiceLine = _dtHelper.getRowData(this);
            if (invoiceLine.ticketId) {
                _createOrEditTicketModal.open({ id: invoiceLine.ticketId, readOnly: true });
            } else {
                abp.message.warn('The row doesn\'t have a ticket associated with it');
                return;
            }
        });

        function isInvoiceLineEmpty(invoiceLine) {
            return !invoiceLine.description
                && !invoiceLine.ticketId
                && !invoiceLine.freightExtendedAmount
                && !invoiceLine.materialExtendedAmount;
        }

        function removeEmptyInvoiceLines() {
            if (!_invoiceLines) {
                return;
            }
            for (var i = 0; i < _invoiceLines.length; i++) {
                if (isInvoiceLineEmpty(_invoiceLines[i])) {
                    _invoiceLines.splice(i, 1);
                    i--;
                }
            }
            recalculateLineNumbers();
            reloadInvoiceLinesGrid();
        }

        invoiceLinesTable.on('click', '.btnDeleteRow', async function (e) {
            e.preventDefault();
            var invoiceLine = _dtHelper.getRowData(this);
            if (await abp.message.confirm(
                isDraftInvoice()
                    ? 'You are about to delete this line item. Are you sure you want to do this?'
                    : 'You are about to delete this line item and it has already been sent to the customer. Are you sure you want to do this?')
            ) {
                var index = _invoiceLines.indexOf(invoiceLine);
                if (index !== -1) {
                    _invoiceLines.splice(index, 1);
                    if (showBottomFuelSurchargeLine) {
                        let bottomFuelLine = getBottomFuelSurchargeLine();
                        if (bottomFuelLine) {
                            setAmountToFuelSurchargeLine(bottomFuelLine, bottomFuelLine.extendedAmount - invoiceLine.fuelSurcharge);
                            if (bottomFuelLine.extendedAmount === 0) {
                                _invoiceLines.splice(_invoiceLines.indexOf(bottomFuelLine), 1);
                            }
                        }
                    } else if (showFuelSurchargeLinePerTicket) {
                        let childRows = _invoiceLines.filter(x => x.parentInvoiceLineGuid && x.parentInvoiceLineGuid === invoiceLine.guid
                            || x.parentInvoiceLineId && x.parentInvoiceLineId === invoiceLine.id);
                        for (let childRow of childRows) {
                            let childRowIndex = _invoiceLines.indexOf(childRow);
                            _invoiceLines.splice(childRowIndex, 1);
                        }
                    }
                    addEmptyRowIfNeeded();
                    setFormDirty();
                    recalculateLineNumbers();
                    recalculateTotals();
                    refreshAddUnbilledTicketsVisibility();
                    refreshJobNumberVisibilityAndText();
                    reloadInvoiceLinesGrid();
                }
            }
        });

        //$('#Status').on('change', enableOrDisableButtonsDependingOnInvoiceStatus);

        //enableOrDisableButtonsDependingOnInvoiceStatus();
        //function enableOrDisableButtonsDependingOnInvoiceStatus() {
        //    var $statusCtrl = $('#Status');
        //    $('#CreateNewInvoiceTicketButton, #CreateNewQuoteButton').prop('disabled', $statusCtrl.val() == $statusCtrl.data('inactive-status'));
        //}

    });
})();