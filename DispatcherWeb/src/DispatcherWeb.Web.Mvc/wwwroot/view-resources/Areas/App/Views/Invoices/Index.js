(function () {
    $(function () {

        var _invoiceService = abp.services.app.invoice;
        var _quickbooksOnlineService = abp.services.app.quickbooksOnline;
        var _dtHelper = abp.helper.dataTables;
        var _quickbooksIntegrationKind = abp.setting.getInt('App.Invoice.Quickbooks.IntegrationKind');
        var _isQuickbooksIntegrationEnabled = _quickbooksIntegrationKind !== abp.enums.quickbooksIntegrationKind.none;

        var _emailInvoicePrintOutModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Invoices/EmailInvoicePrintOutModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Invoices/_EmailInvoicePrintOutModal.js',
            modalClass: 'EmailInvoicePrintOutModal'
        });

        $('[data-toggle="tooltip"]').tooltip();

        $("#StatusFilter").select2Init({
            showAll: true,
            allowClear: true
        });

        $("#CustomerIdFilter").select2Init({
            abpServiceMethod: abp.services.app.customer.getActiveCustomersSelectList,
            abpServiceParams: { includeInactiveWithInvoices: true },
            showAll: false,
            allowClear: true
        });

        $("#OfficeIdFilter").select2Init({
            abpServiceMethod: abp.services.app.office.getOfficesSelectList,
            showAll: true,
            allowClear: true
        });

        $("#IssueDateFilter").daterangepicker({
            autoUpdateInput: false,
            locale: {
                cancelLabel: 'Clear'
            }
        }, function (start, end, label) {
            $("#IssueDateStartFilter").val(start.format('MM/DD/YYYY'));
            $("#IssueDateEndFilter").val(end.format('MM/DD/YYYY'));
        });

        $("#IssueDateFilter").on('blur', function () {
            var startDate = $("#IssueDateStartFilter").val();
            var endDate = $("#IssueDateEndFilter").val();
            $(this).val(startDate && endDate ? startDate + ' - ' + endDate : '');
        });

        $("#IssueDateFilter").on('apply.daterangepicker', function (ev, picker) {
            $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
            $("#IssueDateStartFilter").val(picker.startDate.format('MM/DD/YYYY'));
            $("#IssueDateEndFilter").val(picker.endDate.format('MM/DD/YYYY'));
            reloadMainGrid();
        });

        $("#IssueDateFilter").on('cancel.daterangepicker', function (ev, picker) {
            $(this).val('');
            $("#IssueDateStartFilter").val('');
            $("#IssueDateEndFilter").val('');
            reloadMainGrid();
        });

        var invoicesTable = $('#InvoicesTable');
        var invoicesGrid = invoicesTable.DataTableInit({
            ajax: function (data, callback, settings) {
                var abpData = _dtHelper.toAbpData(data);
                $.extend(abpData, _dtHelper.getFilterData());
                _invoiceService.getInvoices(abpData).done(function (abpResult) {
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
                    }
                },
                {
                    data: "issueDate",
                    render: function (data, type, full, meta) { return _dtHelper.renderUtcDate(full.issueDate); },
                    title: "Issue Date",
                    width: "150px"
                },
                {
                    responsivePriority: 1,
                    data: "customerName",
                    title: "Customer"
                },
                {
                    data: "jobNumberSort",
                    render: function (data, type, full, meta) { return _dtHelper.renderText(full.jobNumber); },
                    title: "Job Nbr"
                },
                {
                    data: "totalAmount",
                    render: function (data, type, full, meta) { return _dtHelper.renderMoney(full.totalAmount); },
                    title: "Total",
                    width: "150px"
                },
                {
                    data: "status",
                    render: function (data, type, full, meta) { return _dtHelper.renderText(full.statusName); },
                    title: "Status",
                    width: "150px"
                },
                {
                    data: "quickbooksExportDateTime",
                    render: function (data, type, full, meta) { return _dtHelper.renderCheckbox(full.quickbooksExportDateTime !== null); },
                    visible: _isQuickbooksIntegrationEnabled,
                    title: "Exported",
                    class: "checkmark",
                    width: "20px"
                },
                {
                    data: null,
                    orderable: false,
                    autoWidth: false,
                    defaultContent: '',
                    responsivePriority: 2,
                    width: '10px',
                    render: function (data, type, full, meta) {
                        return '<div class="dropdown">'
                            + '<button class="btn btn-primary btn-sm" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-ellipsis-h"></i></button>'
                            + '<ul class="dropdown-menu dropdown-menu-right">'
                            + '<li><a class="btnEditRow dropdown-item" title="Edit"><i class="fa fa-edit"></i> Edit</a></li>'
                            + (_isQuickbooksIntegrationEnabled && full.quickbooksExportDateTime
                                ? '<li><a class="btnUndoInvoiceExportForRow dropdown-item"><i class="fas fa-undo"></i> Change Status to allow exporting</a></li>'
                                : '')
                            + '<li><a class="btnDeleteRow dropdown-item" title="Delete"><i class="fa fa-trash"></i> Delete</a></li>'
                            + '<li><a class="btnPrintRow dropdown-item" title="Print"><i class="fa fa-print"></i> Print</a></li>'
                            + '<li><a class="btnEmailRow dropdown-item" title="Email"><i class="fa fa-envelope"></i> Email</a></li>'
                            + (full.customerHasMaterialCompany
                                ? '<li><a class="btnSendInvoiceTicketsToCustomerTenantForRow dropdown-item"><i class="fa fa-share"></i> ' + app.localize('SendTicketsToCustomerTenant') + '</a></li>'
                                : '')
                            + '<li><a class="btnCreateTicketsFileForRow dropdown-item"><i class="fa fa-ticket-alt"></i> ' + app.localize('CreateTicketsFile') + '</a></li>'
                            + '<li><a class="btnDownloadTicketImagesForRow dropdown-item"><i class="la la-file-image-o"></i> ' + app.localize('DownloadTicketImages') + '</a></li>'
                            + '</ul>'
                            + '</div>';
                    }
                }
            ]
        });

        function reloadMainGrid() {
            invoicesGrid.ajax.reload();
        }

        $("#CreateNewInvoiceButton").click(function (e) {
            e.preventDefault();
            window.location = abp.appPath + 'app/Invoices/Details/';
        });

        invoicesTable.on('click', '.btnEditRow', function () {
            var invoiceId = _dtHelper.getRowData(this).id;
            window.location = abp.appPath + 'app/Invoices/Details/' + invoiceId;
        });

        invoicesTable.on('click', '.btnDeleteRow', async function (e) {
            e.preventDefault();
            var row = _dtHelper.getRowData(this);
            var invoiceId = row.id;
            var isExported = row.quickbooksExportDateTime !== null;
            //if (row.status !== abp.enums.invoiceStatus.draft) {
            //    abp.message.error(app.localize('InvoiceDeleteErrorNotDraft'));
            //    return;
            //}
            if (await abp.message.confirm(
                app.localize(isExported ? 'ExportedInvoiceDeletePrompt' : 'InvoiceDeletePrompt', row.id)
            )) {
                _invoiceService.deleteInvoice({
                    id: invoiceId
                }).done(function () {
                    abp.notify.info('Successfully deleted.');
                    reloadMainGrid();
                });
            }
        });

        invoicesTable.on('click', '.btnUndoInvoiceExportForRow', async function (e) {
            e.preventDefault();
            var row = _dtHelper.getRowData(this);
            var invoiceId = row.id;
            if (await abp.message.confirm(
                app.localize('UndoInvoiceExportPrompt')
            )) {
                _invoiceService.undoInvoiceExport({
                    id: invoiceId
                }).done(function () {
                    abp.notify.info('Successfully saved.');
                    reloadMainGrid();
                });
            }
        });

        invoicesTable.on('click', '.btnPrintRow', function () {
            var invoiceId = _dtHelper.getRowData(this).id;
            //setTimeout(() => window.location.reload(), 5000);
            setTimeout(() => reloadMainGrid(), 3000);
            window.open(abp.appPath + 'app/invoices/GetInvoicePrintOut?invoiceId=' + invoiceId);
        });

        invoicesTable.on('click', '.btnEmailRow', function () {
            var invoiceId = _dtHelper.getRowData(this).id;
            _emailInvoicePrintOutModal.open({ id: invoiceId });
        });

        invoicesTable.on('click', '.btnSendInvoiceTicketsToCustomerTenantForRow', async function () {
            var invoiceId = _dtHelper.getRowData(this).id;
            abp.ui.setBusy();
            try {
                await _invoiceService.sendInvoiceTicketsToCustomerTenant({ id: invoiceId });
                abp.notify.info('Sent successfully');
            }
            finally {
                abp.ui.clearBusy();
            }
        });

        invoicesTable.on('click', '.btnCreateTicketsFileForRow', function () {
            var invoiceId = _dtHelper.getRowData(this).id;
            abp.ui.setBusy();
            abp.services.app.ticket
                .getTicketsToCsv({ invoiceId: invoiceId })
                .done(function (result) {
                    app.downloadTempFile(result);
                }).always(function () {
                    abp.ui.clearBusy();
                });
        });

        invoicesTable.on('click', '.btnDownloadTicketImagesForRow', async function () {
            var invoiceId = _dtHelper.getRowData(this).id;
            try {
                abp.ui.setBusy();
                if (!await abp.services.app.ticket.invoiceHasTicketPhotos(invoiceId)) {
                    abp.message.info(app.localize('InvoiceDoesntHaveTicketImages'));
                    return;
                }
                let url = abp.appPath + 'app/Tickets/GetTicketPhotosForInvoice/' + invoiceId;
                window.open(url);
            }
            finally {
                abp.ui.clearBusy();
            }
        });

        $("#SearchButton").closest('form').submit(function (e) {
            e.preventDefault();
            reloadMainGrid();
        });

        $("#ClearSearchButton").click(function () {
            $(this).closest('form')[0].reset();
            $(".filter").change();
            $("#IssueDateStartFilter").val('');
            $("#IssueDateEndFilter").val('');
            $("#BatchIdFilter").val('');
            reloadMainGrid();
        });

        $("#UpdateQboButton").click(function (e) {
            e.preventDefault();
            let button = $(this);
            abp.ui.setBusy(button);
            _quickbooksOnlineService.uploadInvoices().done(function (result) {
                if (result.errorList.length) {
                    abp.message.warn(result.errorList.join('; \n'), 'Some of the invoices weren\'t uploaded');
                } else if (result.uploadedInvoicesCount) {
                    abp.notify.success('Invoices added to QuickBooks');
                } else {
                    abp.notify.warn('There were no invoices to upload');
                }
            }).always(function () {
                abp.ui.clearBusy(button);
                reloadMainGrid();
            });
        });

        $("#QbExportButton").click(function (e) {
            e.preventDefault();
            let button = $(this);
            let quickbooksIntegrationKind = abp.setting.getInt('App.Invoice.Quickbooks.IntegrationKind');

            switch (quickbooksIntegrationKind) {
                case abp.enums.quickbooksIntegrationKind.desktop:
                    abp.ui.setBusy(button);
                    let fileWindow = window.open(abp.appPath + 'app/QuickBooks/ExportInvoicesToIIF');
                    var awaitFileWindowInterval = setInterval(function () {
                        if (fileWindow.closed) {
                            clearInterval(awaitFileWindowInterval);
                            reloadMainGrid();
                            abp.ui.clearBusy(button);
                        }
                    }, 500);
                    break;
                case abp.enums.quickbooksIntegrationKind.qboExport:
                    abp.ui.setBusy(button);
                    abp.services.app.quickbooksOnlineExport.exportInvoicesToCsv().done(function (result) {
                        app.downloadTempFile(result);
                    }).always(() => {
                        abp.ui.clearBusy(button);
                    });
                    break;
                case abp.enums.quickbooksIntegrationKind.transactionProExport:
                    abp.ui.setBusy(button);
                    abp.services.app.quickbooksTransactionProExport.exportInvoicesToCsv().done(function (result) {
                        app.downloadTempFile(result);
                    }).always(() => {
                        abp.ui.clearBusy(button);
                    });
                    break;
                case abp.enums.quickbooksIntegrationKind.online:
                case abp.enums.quickbooksIntegrationKind.none:
                default:
                    break;
            }

            
        });


    });
})();