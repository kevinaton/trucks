(function () {
    $(function () {

        var _quoteService = abp.services.app.quote;
        var _quoteHistoryService = abp.services.app.quoteHistory;
        var _dtHelper = abp.helper.dataTables;
        var _quoteId = $("#Id").val();
        var _permissions = {
            edit: abp.auth.hasPermission('Pages.Quotes.Edit'),
            createItems: abp.auth.hasPermission('Pages.Quotes.Items.Create')
        };

        var _createOrEditQuoteServiceModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Quotes/CreateOrEditQuoteServiceModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Quotes/_CreateOrEditQuoteServiceModal.js',
            modalClass: 'CreateOrEditQuoteServiceModal'
        });

        var _createOrEditCustomerModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Customers/CreateOrEditCustomerModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Customers/_CreateOrEditCustomerModal.js',
            modalClass: 'CreateOrEditCustomerModal',
            modalSize: 'lg'
        });

        var _createOrEditCustomerContactModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Customers/CreateOrEditCustomerContactModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Customers/_CreateOrEditCustomerContactModal.js',
            modalClass: 'CreateOrEditCustomerContactModal'
        });

        var _emailQuoteReportModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Quotes/EmailQuoteReportModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Quotes/_EmailQuoteReportModal.js',
            modalClass: 'EmailQuoteReportModal'
        });

        var _addToProjectModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Quotes/AddToProjectModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Quotes/_AddToProjectModal.js',
            modalClass: 'AddToProjectModal'
        });

        var _viewQuoteDeliveriesModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Quotes/ViewQuoteDeliveriesModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Quotes/_ViewQuoteDeliveriesModal.js',
            modalClass: 'ViewQuoteDeliveriesModal',
            modalSize: 'lg'
        });

        var _viewQuoteHistoryModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/QuoteHistory/ViewQuoteHistoryModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/QuoteHistory/_ViewQuoteHistoryModal.js',
            modalClass: 'ViewQuoteHistoryModal',
            modalSize: 'lg'
        });

        var saveQuoteAsync = function (callback) {
            var form = $("#QuoteForm");
            if (!form.valid()) {
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

            var quote = form.serializeFormToObject();
            abp.ui.setBusy(form);
            _quoteService.editQuote(quote).done(function (data) {
                abp.notify.info('Saved successfully.');
                $("#Id").val(data);
                _quoteId = data;
                history.replaceState({}, "", abp.appPath + 'app/quotes/details/' + data);
                showEditingBlocks();
                $("#QuoteForm").dirtyForms('setClean');
                $("#QuoteForm").uniform.update();

                $('#AddNewCustomerButton span').addClass('fa-edit').removeClass('fa-plus');

                if (callback)
                    callback();
            }).always(function () {
                abp.ui.clearBusy(form);
            });
        };

        function isNewOrChangedQuote() {
            return _quoteId === '' || $("#QuoteForm").dirtyForms('isDirty');
        }

        function showEditingBlocks() {
            $('.editing-only-block').not(":visible").slideDown();
        }

        if (!_permissions.edit) {
            $("#SaveQuoteButton").hide();
            $("#CopyQuoteButton").hide();
            $("#AddNewCustomerButton").hide();
            $("#AddNewContactButton").hide();
            $("#DeleteSelectedQuoteServicesButton").hide();

            if (!_permissions.createItems) {
                $("#CreateNewQuoteServiceButton").hide();
            }
        }

        $("#ProposalDate").datepickerInit();
        $("#ProposalExpiryDate").datepickerInit();
        $("#InactivationDate").datepickerInit();

        $("#SalesPersonId").select2Init({
            abpServiceMethod: abp.services.app.user.getSalespersonsSelectList,
            minimumInputLength: 0,
            allowClear: false
        });
        $("#Status").select2Init({ allowClear: false });
        $("#ContactId").select2Init();
        $("#CustomerId").select2Init({
            abpServiceMethod: abp.services.app.customer.getActiveCustomersSelectList
        });
        var contactChildDropdown = abp.helper.ui.initChildDropdown({
            parentDropdown: $("#CustomerId"),
            childDropdown: $("#ContactId"),
            abpServiceMethod: abp.services.app.customer.getContactsForCustomer
        });

        $("#CustomerId").change(function () {
            var projectName = $("#ProjectName").val();
            var customerName = $("#CustomerName").val();
            $("Name").val(projectName + ' for ' + customerName);
        });

        $("#FuelSurchargeCalculationId").select2Init({
            abpServiceMethod: abp.services.app.fuelSurchargeCalculation.getFuelSurchargeCalculationsSelectList,
            minimumInputLength: 0,
            //showAll: true,
            allowClear: false
        });
        $("#FuelSurchargeCalculationId").change(function () {
            let dropdownData = $("#FuelSurchargeCalculationId").select2('data');
            let selectedOption = dropdownData && dropdownData.length && dropdownData[0];
            let canChangeBaseFuelCost = selectedOption && selectedOption.item.canChangeBaseFuelCost || false;
            $("#BaseFuelCostContainer").toggle(canChangeBaseFuelCost);
            $("#BaseFuelCost").val(selectedOption && selectedOption.item.baseFuelCost);
            $("#FuelSurchargeCalculationId").removeUnselectedOptions();
        });

        $("#QuoteForm").dirtyForms();
        $("#QuoteForm").uniform();

        if (_quoteId === "") {
            $("#CustomerId").data('select2').open();
            $("#CustomerId").data('select2').focus();
        }

        var quoteServicesTable = $('#QuoteServicesTable');
        var quoteServicesGrid = quoteServicesTable.DataTableInit({
            paging: false,
            info: false,
            ordering: true,
            language: {
                emptyTable: app.localize("NoDataInTheTableClick{0}ToAdd", app.localize("AddQuoteItem"))
            },
            ajax: function (data, callback, settings) {
                if (_quoteId === '') {
                    callback(_dtHelper.getEmptyResult());
                    return;
                }
                var abpData = _dtHelper.toAbpData(data);
                $.extend(abpData, { quoteId: _quoteId });
                _quoteService.getQuoteServices(abpData).done(function (abpResult) {
                    callback(_dtHelper.fromAbpResult(abpResult));
                    abp.helper.ui.initControls();
                });
            },
            footerCallback: function (tfoot, data, start, end, display) {
                var materialTotal = data.map(function (x) { return x.extendedMaterialPrice; }).reduce(function (a, b) { return a + b; }, 0);
                var serviceTotal = data.map(function (x) { return x.extendedServicePrice; }).reduce(function (a, b) { return a + b; }, 0);

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

        var reloadQuoteServicesGrid = function () {
            quoteServicesGrid.ajax.reload();
        };

        abp.event.on('app.createOrEditQuoteServiceModalSaved', function () {
            reloadQuoteServicesGrid();
            reloadQuoteHistoryGrid();
        });

        $("#CreateNewQuoteServiceButton").click(function (e) {
            e.preventDefault();
            if (_quoteId === '') {
                saveQuoteAsync(function () {
                    reloadQuoteServicesGrid();
                    reloadQuoteHistoryGrid();
                    _createOrEditQuoteServiceModal.open({ quoteId: _quoteId });
                });
            } else {
                _createOrEditQuoteServiceModal.open({ quoteId: _quoteId });
            }
        });

        $("#CreateNewQuoteButton").click(function (e) {
            e.preventDefault();
            saveQuoteAsync(function () {
                abp.ui.setBusy();
                window.location = abp.appPath + 'app/quotes/details/?projectId=' + $("#ProjectId").val();
            });
        });

        $("#SaveQuoteButton").click(function (e) {
            e.preventDefault();
            if ($("#InactivationDate").val() !== '') {
                var option = $("#Status").getSelectedDropdownOption().val();
                if (parseInt(option) !== abp.enums.projectStatus.inactive) {
                    abp.message.warn('Only inactive quotes can have an inactivation date.');
                    return false;
                }
            }
            saveQuoteAsync(function () {
                reloadQuoteServicesGrid();
                reloadQuoteHistoryGrid();
            });
        });

        var quoteHistoryTable = $('#QuoteHistoryTable');
        var quoteHistoryGrid = quoteHistoryTable.DataTableInit({
            ajax: function (data, callback, settings) {
                if (_quoteId === '') {
                    callback(_dtHelper.getEmptyResult());
                    return;
                }
                var abpData = _dtHelper.toAbpData(data);
                $.extend(abpData, { quoteId: _quoteId });
                _quoteHistoryService.getQuoteHistory(abpData).done(function (abpResult) {
                    callback(_dtHelper.fromAbpResult(abpResult));
                });
            },
            columns: [
                {
                    width: '20px',
                    className: 'control responsive',
                    orderable: false,
                    render: function () {
                        return '';
                    },
                },
                {
                    data: "dateTime",
                    render: function (data, type, full, meta) { return _dtHelper.renderUtcDateTime(full.dateTime); },
                    title: "When changed"
                },
                {
                    data: "changedByUserName",
                    render: function (data, type, full, meta) { return _dtHelper.renderText(full.changedByUserName); },
                    title: "Changed by"
                },
                {
                    data: "changeType",
                    render: function (data, type, full, meta) { return _dtHelper.renderText(full.changeTypeName); },
                    title: "Type of change"
                },
                {
                    data: null,
                    orderable: false,
                    name: "Actions",
                    width: "10px",
                    responsivePriority: 1,
                    className: "actions",
                    defaultContent: '<div class="dropdown action-button">'
                        + '<ul class="dropdown-menu dropdown-menu-right">'
                        + '<li><a class="btnShowHistoryDetails"><i class="fa fa-edit"></i> Details</a></li>'
                        + '</ul>'
                        + '<button class="btn btn-primary btn-sm" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-ellipsis-h"></i></button>'
                        + '</div>'
                }
            ],
            order: [[1, "desc"]]

        });

        var reloadQuoteHistoryGrid = function () {
            quoteHistoryGrid.ajax.reload();
        };

        $("#CopyQuoteButton").click(function (e) {
            e.preventDefault();
            saveQuoteAsync(function () {
                abp.ui.setBusy();
                window.location = abp.appPath + 'app/quotes/copy/' + _quoteId;
            });
        });

        function openReport() {
            abp.helper.promptForHideLoadAtOnQuote().then(function (hideLoadAt) {
                window.open(abp.appPath + 'app/quotes/getreport?quoteId=' + _quoteId + '&hideLoadAt=' + hideLoadAt);
            });
        }

        $("#PrintQuoteButton").click(function (e) {
            e.preventDefault();
            if (isNewOrChangedQuote() || $("#BaseFuelCost").val() === '') { //force the user to enter the 'base fuel cost' value before allowing to print the quote
                saveQuoteAsync(function () {
                    //popup is being blocked in async
                    abp.message.success("Quote was saved", "").done(function () {
                        openReport();
                    });
                });
            } else {
                openReport();
            }
        });

        $("#EmailQuoteButton").click(function (e) {
            e.preventDefault();
            saveQuoteAsync(function () {
                _emailQuoteReportModal.open({ id: _quoteId });
            });
        });

        $("#GoToProjectButton").click(function (e) {
            e.preventDefault();
            window.location = abp.appPath + 'app/projects/details/' + $("#ProjectId").val();
        });

        $("#RemoveProjectButton").click(function (e) {
            e.preventDefault();
            $("#ProjectId").val('');
            $("#ProjectName").val('');
            $("#projectRelatedButtons").hide();
            $("#noProjectButtons").show();
            saveQuoteAsync();
        });

        $("#AddToProjectButton").click(function (e) {
            e.preventDefault();
            _addToProjectModal.open();
        });

        abp.event.on('app.addToProjectModalSaved', function (e) {
            $("#ProjectId").val(e.id);
            $("#ProjectName").val(e.name);
            $("#noProjectButtons").hide();
            $("#projectRelatedButtons").show();
            saveQuoteAsync();
        });

        quoteServicesTable.on('click', '.btnEditRow', function (e) {
            e.preventDefault();
            var quoteServiceId = _dtHelper.getRowData(this).id;
            _createOrEditQuoteServiceModal.open({ id: quoteServiceId });
        });

        quoteServicesTable.on('click', '.btnDeleteRow', async function (e) {
            e.preventDefault();
            var quoteServiceId = _dtHelper.getRowData(this).id;
            if (await abp.message.confirm('Are you sure you want to delete the item?')) {
                _quoteService.deleteQuoteServices({
                    ids: [quoteServiceId]
                }).done(function () {
                    abp.notify.info('Successfully deleted.');
                    reloadQuoteServicesGrid();
                });
            }
        });

        quoteServicesTable.on('click', '.btnShowDeliveriesForRow', function (e) {
            e.preventDefault();
            var row = _dtHelper.getRowData(this);
            _viewQuoteDeliveriesModal.open({
                quoteServiceId: row.id,
                quotedMaterialQuantity: row.materialQuantity || 0,
                quotedFreightQuantity: row.freightQuantity || 0
            });
        });

        quoteHistoryTable.on('click', '.btnShowHistoryDetails', function (e) {
            e.preventDefault();
            var quoteHistoryId = _dtHelper.getRowData(this).id;
            _viewQuoteHistoryModal.open({ id: quoteHistoryId });
        });

        $("#AddNewCustomerButton").click(function (e) {
            e.preventDefault();
            var customerId = _quoteId ? $('#CustomerId').val() : null;
            _createOrEditCustomerModal.open({ id: customerId });
        });

        abp.event.on('app.customerNameExists', function (e) {
            selectCustomerInControl(e);
        });
        abp.event.on('app.createOrEditCustomerModalSaved', function (e) {
            selectCustomerInControl(e);
        });
        function selectCustomerInControl(e) {
            if ($('#CustomerId').val() !== e.item.id.toString()) {
                abp.helper.ui.addAndSetDropdownValue($("#CustomerId"), e.item.id, e.item.name);
            } else {
                $('#CustomerId option[value="' + e.item.id + '"]').text(e.item.name);
                $('#CustomerId').select2Init();
            }
        }

        $("#AddNewContactButton").click(function (e) {
            e.preventDefault();
            var customerId = $("#CustomerId").val();
            if (!customerId) {
                abp.notify.warn("Select a customer first");
                $("#CustomerId").focus();
                return;
            }
            _createOrEditCustomerContactModal.open({ customerId: customerId });
        });

        abp.event.on('app.createOrEditCustomerContactModalSaved', function (e) {
            contactChildDropdown.updateChildDropdown(function () {
                contactChildDropdown.childDropdown.val(e.item.Id).change();
            });
        });
        abp.event.on('app.mergeModalFinished', function () {
            contactChildDropdown.updateChildDropdown(function () {
                contactChildDropdown.childDropdown.val('').change();
            });
        });

        var statusChanging = false;
        $("#Status").change(function () {
            if (statusChanging) {
                return;
            }
            var option = $("#Status").getSelectedDropdownOption().val();
            if (option == abp.enums.projectStatus.pending || option == abp.enums.projectStatus.active) {
                $("#InactivationDate").val('');
            } else {
                var now = moment(new Date());
                $("#InactivationDate").val(moment(now).format('MM/DD/YYYY'));
            }
        });

    });
})();