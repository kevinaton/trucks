(function ($) {
    app.modals.SelectCustomerTicketsModal = function () {

        var _modalManager;
        var _invoiceService = abp.services.app.invoice;
        var _dtHelper = abp.helper.dataTables;
        var _filter = null;
        var _customerInvoicingMethod = null;
        var _selectedJobNumbers = null;
        var _customerTicketsGrid = null;
        var _customerTicketsGridOptions = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            //_$form = _modalManager.getModal().find('form');

            abp.helper.ui.initControls();

            let saveButton = _modalManager.getModal().find('.save-button');
            saveButton.find('span').text('Add selected tickets');
            saveButton.find('i').removeClass('fa-save').addClass('fa-plus');

            var customerTicketsTable = _modalManager.getModal().find('#CustomerTicketsTable');
            _customerTicketsGridOptions = {
                paging: false,
                serverSide: true,
                processing: true,
                info: false,
                selectionColumnOptions: {},
                ajax: async function (data, callback, settings) {
                    if (!_filter) {
                        callback(abp.helper.dataTables.getEmptyResult());
                        return;
                    }
                    var abpData = _dtHelper.toAbpData(data);
                    $.extend(abpData, _filter);
                    _invoiceService.getCustomerTickets(abpData).done(function (abpResult) {
                        callback(_dtHelper.fromAbpResult(abpResult));
                        //abp.helper.ui.initControls();
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
                        data: "ticketDateTime",
                        render: function (data, type, full, meta) { return (_dtHelper.renderDateShortTime(full.ticketDateTime) || '').replace(' ', '<br>'); },
                        title: "Date Time"
                    },
                    {
                        data: "jobNumber",
                        title: "Job Nbr"
                    },
                    {
                        data: "serviceName",
                        title: "Product / Service",
                        width: "50px"
                    },
                    {
                        data: "ticketNumber",
                        title: "Ticket #"
                    },
                    {
                        data: "truckCode",
                        title: "Truck"
                    },
                    {
                        data: "loadAtNamePlain",
                        render: function (data, type, full, meta) { return _dtHelper.renderText(full.loadAtName); },
                        title: "Load At"
                    },
                    {
                        data: "deliverToName",
                        title: "Deliver To"
                    }
                ]
            };
            _customerTicketsGrid = customerTicketsTable.DataTableInit(_customerTicketsGridOptions);

            _modalManager.getModal().on('shown.bs.modal', function () {
                _customerTicketsGrid
                    .columns.adjust()
                    .responsive.recalc();
            });
        };

        function reloadCustomerTicketsGrid() {
            _customerTicketsGrid && _customerTicketsGrid.ajax.reload();
        }

        this.setFilter = function (filter, customerInvoicingMethod, selectedJobNumbers) {
            _filter = filter;
            _customerInvoicingMethod = customerInvoicingMethod;
            _selectedJobNumbers = selectedJobNumbers;
            reloadCustomerTicketsGrid();
        };

        this.save = async function () {
            var selectedRows = _customerTicketsGridOptions.selectionColumnOptions.getSelectedRows();
            //console.log(selectedRows);

            if (selectedRows && selectedRows.length && _customerInvoicingMethod === abp.enums.invoicingMethod.separateTicketsByJobNumber) {
                let newJobNumbers = [];
                selectedRows.map(r => r.jobNumber).filter(j => j).forEach(j => {
                    if (!newJobNumbers.includes(j)) {
                        newJobNumbers.push(j);
                    }
                });
                let showJobNumberWarning = newJobNumbers.length > 1;
                if (!showJobNumberWarning && _selectedJobNumbers && _selectedJobNumbers.length) {
                    if (newJobNumbers.filter(n => !_selectedJobNumbers.includes(n)).length) {
                        showJobNumberWarning = true;
                    }
                }
                if (showJobNumberWarning) {
                    if (!await abp.message.confirm('This customer wants separate tickets per job number and you have selected some tickets with different job numbers. Are you sure you want to save this invoice with multiple job numbers?')) {
                        return;
                    }
                }
            }

            abp.event.trigger('app.customerTicketsSelectedModal', {
                selectedTickets: selectedRows
            });
            _modalManager.close();
        };
    };
})(jQuery);