(function ($) {
    app.modals.CreateOrEditQuoteModal = function () {
        let _modalManager;
        let _quoteService = abp.services.app.quote;
        let _$quoteId = null;
        let _dtHelper = abp.helper.dataTables;
        let _permissions = {
            edit: abp.auth.hasPermission('Pages.Quotes.Edit'),
            createItems: abp.auth.hasPermission('Pages.Quotes.Items.Create')
        };
        let statusChanging = false;

        let _$form = null;
        let _$quoteIdInput = null;
        let _$customerInput = null;
        let _$proposalDateInput = null;
        let _$proposalExpiryDateInput = null;
        let _$inactivationDateInput = null;
        let _$quoteSalesPersonIdInput = null;
        let _$quoteFuelSurchargeCalculationIdInput = null;

        let _$baseFuelCostInput = null;
        let _$statusInput = null;
        let _$contactIdInput = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            const _createOrEditQuoteServiceModal = new app.ModalManager({
                viewUrl: abp.appPath + 'app/Quotes/CreateOrEditQuoteServiceModal',
                scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Quotes/_CreateOrEditQuoteServiceModal.js',
                modalClass: 'CreateOrEditQuoteServiceModal'
            });

            const _createOrEditCustomerModal = new app.ModalManager({
                viewUrl: abp.appPath + 'app/Customers/CreateOrEditCustomerModal',
                scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Customers/_CreateOrEditCustomerModal.js',
                modalClass: 'CreateOrEditCustomerModal',
                modalSize: 'lg'
            });

            const _createOrEditCustomerContactModal = new app.ModalManager({
                viewUrl: abp.appPath + 'app/Customers/CreateOrEditCustomerContactModal',
                scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Customers/_CreateOrEditCustomerContactModal.js',
                modalClass: 'CreateOrEditCustomerContactModal'
            });

            _$form = _modalManager.getModal().find('form');
            //_$form.validate();

            abp.helper.ui.initControls();

            _$projectInput = _$form.find('#ProjectName');
            _$quoteIdInput = _$form.find('#Id');
            _$nameInput = _$form.find('#Name');
            _$customerInput = _$form.find('#QuoteCustomer');
            _$proposalDateInput = _$form.find('#ProposalDate');
            _$proposalExpiryDateInput = _$form.find('#ProposalExpiryDate');
            _$inactivationDateInput = _$form.find('#InactivationDate');
            _$quoteSalesPersonIdInput = _$form.find('#QuoteSalesPersonId');
            _$quoteFuelSurchargeCalculationIdInput = _$form.find('#QuoteFuelSurchargeCalculationId');
            _$baseFuelCostInput = _$form.find('#BaseFuelCost');
            _$statusInput = _$form.find('#Status');
            _$contactIdInput = _$form.find('#ContactId');

            _$proposalDateInput.datepickerInit();
            _$proposalExpiryDateInput.datepickerInit();
            _$inactivationDateInput.datepickerInit();

            _$quoteSalesPersonIdInput.select2Init({
                abpServiceMethod: abp.services.app.user.getSalespersonsSelectList,
                showAll: false,
                allowClear: false
            });

            _$statusInput.select2Init({
                showAll: true,
                allowClear: false
            });

            _$contactIdInput.select2Init({
                showAll: false,
                allowClear: true,
                addItemCallback: async function (newItemName) {
                    var customerId = _$customerInput.val();
                    if (!customerId) {
                        abp.notify.warn("Select a customer first");
                        _$customerInput.focus();
                        return false;
                    }
                    var result = await app.getModalResultAsync(
                        _createOrEditCustomerContactModal.open({ name: newItemName, customerId: customerId })
                    );
                    contactChildDropdown.updateChildDropdown(function () {
                        contactChildDropdown.childDropdown.val(result.Id).change();
                    });
                    return false;
                }
            });

            _$customerInput.select2Init({
                abpServiceMethod: abp.services.app.customer.getActiveCustomersSelectList,
                showAll: false,
                allowClear: false,
                addItemCallback: async function (newItemName) {
                    var result = await app.getModalResultAsync(
                        _createOrEditCustomerModal.open({ name: newItemName })
                    );
                    return {
                        id: result.id,
                        name: result.name
                    };
                }
            });

            _$quoteFuelSurchargeCalculationIdInput.select2Init({
                abpServiceMethod: abp.services.app.fuelSurchargeCalculation.getFuelSurchargeCalculationsSelectList,
                showAll: true,
                allowClear: true
            });

            _$quoteId = _$quoteIdInput.val();

            const contactChildDropdown = abp.helper.ui.initChildDropdown({
                parentDropdown: _$customerInput,
                childDropdown: _$contactIdInput,
                abpServiceMethod: abp.services.app.customer.getContactsForCustomer
            });

            //_$customerInput.change(function () {
            //    var projectName = $("#ProjectName").val();
            //    var customerName = $("#CustomerName").val();
            //    $("Name").val(projectName + ' for ' + customerName);
            //})

            _$statusInput.change(function () {
                if (statusChanging) {
                    return;
                }
                var option = _$statusInput.getSelectedDropdownOption().val();
                if (option == abp.enums.quoteStatus.pending || option == abp.enums.quoteStatus.active) {
                    _$inactivationDateInput.val('');
                } else {
                    var now = moment(new Date());
                    _$inactivationDateInput.val(moment(now).format('MM/DD/YYYY'));
                }
            });

            _$quoteFuelSurchargeCalculationIdInput.change(function () {
                let dropdownData = _$quoteFuelSurchargeCalculationIdInput.select2('data');
                let selectedOption = dropdownData && dropdownData.length && dropdownData[0];
                let canChangeBaseFuelCost = selectedOption?.item?.canChangeBaseFuelCost || false;
                $("#BaseFuelCostContainer").toggle(canChangeBaseFuelCost);
                _$baseFuelCostInput.val(selectedOption?.item?.baseFuelCost || 0);
                _$quoteFuelSurchargeCalculationIdInput.removeUnselectedOptions();
            });

            const quoteServicesTable = $('#QuoteServicesTable');
            const quoteServicesGrid = quoteServicesTable.DataTableInit({
                paging: false,
                info: false,
                ordering: true,
                language: {
                    emptyTable: app.localize("NoDataInTheTableClick{0}ToAdd", app.localize("AddQuoteItem"))
                },
                ajax: function (data, callback, settings) {
                    if (_$quoteId === "") {
                        callback(_dtHelper.getEmptyResult());
                        return;
                    }

                    var abpData = _dtHelper.toAbpData(data);
                    $.extend(abpData, { quoteId: _$quoteId });

                    _quoteService.getQuoteServices(abpData).done(function (abpResult) {
                        callback(_dtHelper.fromAbpResult(abpResult));
                        abp.helper.ui.initControls();
                    })
                },
                footerCallback: function (tfoot, data, start, end, display) {
                    const materialTotal = data.map(x => x.extendedMaterialPrice).reduce((a, b) => a + b, 0);
                    const serviceTotal = data.map(x => x.extendedServicePrice).reduce((a, b) => a + b, 0);

                    let grid = this;
                    let setTotalFooterValue = function (columnName, total, visible) {
                        let footerCell = grid.api().column(columnName + ':name').footer();
                        $(footerCell).html(visible ? "Total: " + _dtHelper.renderMoney(total) : '');
                    }

                    setTotalFooterValue('extendedMaterialPrice', materialTotal, data.length);
                    setTotalFooterValue('extendedServicePrice', serviceTotal, data.length);
                },
                massDeleteOptions: {
                    enabled: _permissions.edit,
                    deleteButton: $("#DeleteSelectedQuoteServicesButton"),
                    deleteServiceMethod: _quoteService.deleteQuoteServices
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
                        data: "id",
                        visible: false
                    },
                    {
                        responsivePriority: 2,
                        data: "loadAtNamePlain",
                        render: function (data, type, full, meta) { return _dtHelper.renderText(full.loadAtName); },
                        title: "Load At"
                    },
                    {
                        responsivePriority: 2,
                        data: "deliverToNamePlain",
                        render: function (data, type, full, meta) { return _dtHelper.renderText(full.deliverToName); },
                        title: "Deliver To"
                    },
                    {
                        data: "serviceName",
                        title: "Item"
                    },
                    {
                        data: "materialUomName",
                        title: "Material UOM"
                    },
                    {
                        data: "freightUomName",
                        title: "Freight UOM"
                    },
                    {
                        data: "designation",
                        render: function (data, type, full, meta) { return _dtHelper.renderText(full.designationName); },
                        title: "Designation",
                        orderable: false
                    },
                    {
                        data: "pricePerUnit",
                        render: function (data, type, full, meta) { return _dtHelper.renderMoney(full.pricePerUnit); },
                        title: "Material Rate"
                    },
                    {
                        data: "freightRate",
                        render: function (data, type, full, meta) { return _dtHelper.renderMoney(full.freightRate); },
                        title: "Freight Rate"
                    },
                    {
                        data: "leaseHaulerRate",
                        render: function (data, type, full, meta) { return _dtHelper.renderMoney(data); },
                        title: "LH Rate",
                        visible: abp.setting.getBoolean('App.LeaseHaulers.ShowLeaseHaulerRateOnQuote')
                    },
                    {
                        data: "materialQuantity",
                        title: "Material<br>Quantity"
                    },
                    {
                        data: "freightQuantity",
                        title: "Freight<br>Quantity"
                    },
                    {
                        data: "extendedMaterialPrice",
                        render: function (data, type, full, meta) { return _dtHelper.renderMoney(full.extendedMaterialPrice); },
                        name: "extendedMaterialPrice",
                        title: "Extended<br>Material Price",
                        orderable: false
                    },
                    {
                        data: "extendedServicePrice",
                        render: function (data, type, full, meta) { return _dtHelper.renderMoney(full.extendedServicePrice); },
                        name: "extendedServicePrice",
                        title: "Extended<br>Freight Price",
                        orderable: false
                    },
                    {
                        data: null,
                        orderable: false,
                        visible: _permissions.edit,
                        width: "10px",
                        className: "actions",
                        responsivePriority: 3,
                        defaultContent: '<div class="dropdown action-button">'
                            + '<ul class="dropdown-menu dropdown-menu-right">'
                            + '<li><a class="btnEditRow" title="Edit"><i class="fa fa-edit"></i> Edit</a></li>'
                            + '<li><a class="btnShowDeliveriesForRow" title="Show deliveries"><i class="fa fa-truck-loading"></i>Show deliveries</a></li>'
                            + '<li><a class="btnDeleteRow" title="Delete"><i class="fa fa-trash"></i> Delete</a></li>'
                            + '</ul>'
                            + '<button class="btn btn-primary btn-sm" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-ellipsis-h"></i></button>'
                            + '</div>'
                    }
                ]
            });

            const reloadQuoteServicesGrid = () => {
                quoteServicesGrid.ajax.reload();
            };

            $("#CreateNewQuoteServiceButton").click(function (e) {
                e.preventDefault();

                if (_$quoteId === "") {
                    saveQuoteAsync(function () {
                        reloadQuoteServicesGrid();
                        _createOrEditQuoteServiceModal.open({ quoteId: _$quoteId });
                    });
                } else {
                    _createOrEditQuoteServiceModal.open({ quoteId: _$quoteId });
                }
            });

            _modalManager.getModal().find(".save-quote-button").click(function (e) {
                e.preventDefault();
                saveQuoteAsync(function () {
                    _modalManager.close();
                });
            });

            abp.event.on('app.createOrEditQuoteServiceModalSaved', function () {
                reloadQuoteServicesGrid();
            });
        }

        this.focusOnDefaultElement = function () {
            if (_$quoteId === "") {
                _$customerInput.data('select2').focus();
            }
        };

        const saveQuoteAsync = (callback) => {
            if (_$form) {
                if (!_$form.valid()) {
                    form.showValidateMessage();
                    return;
                }

                if (!abp.helper.validateStartEndDates(
                    { value: $("#ProposalDate").val(), title: $('label[for="ProposalDate"]').text() },
                    { value: $("#ProposalExpiryDate").val(), title: $('label[for="ProposalExpiryDate"]').text() },
                    { value: $("#InactivationDate").val(), title: $('label[for="InactivationDate"]').text() }
                )) {
                    return;
                }

                let quote = form.serializeFormToObject();
                abp.ui.setBusy(form);
                _modalManager.setBusy(true);

                _quoteService.editQuote(quote).done(function (data) {
                    abp.notify.info('Saved successfully.');

                    $("#QuoteForm").dirtyForms('setClean');
                    $("#QuoteForm").uniform.update();

                    if (callback)
                        callback();
                }).always(function () {
                    abp.ui.clearBusy(form);
                });
            }
        };
    }
})(jQuery);