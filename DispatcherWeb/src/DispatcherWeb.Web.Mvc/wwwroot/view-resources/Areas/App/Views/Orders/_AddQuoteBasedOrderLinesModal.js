(function($) {
    app.modals.AddQuoteBasedOrderLinesModal = function () {

        var _modalManager;
        var _orderAppService = abp.services.app.order;
        var _dtHelper = abp.helper.dataTables;
        var _filter = null;
        var _grid = null;
        var _gridOptions = null;


        this.init = function(modalManager) {
            _modalManager = modalManager;
            
            let saveButton = _modalManager.getModal().find('.save-button');
            saveButton.find('span').text('Add selected items');
            saveButton.find('i').removeClass('fa-save').addClass('fa-plus');

            var table = _modalManager.getModal().find('#QuoteItemsTable');
            _gridOptions = {
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
                    _orderAppService.getOrderLines(abpData).done(function (abpResult) {
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
                        data: "loadAtNamePlain",
                        render: (data, type, full, meta) => _dtHelper.renderText(full.loadAtName),
                        title: "Load At"
                    },
                    {
                        data: "deliverToNamePlain",
                        render: (data, type, full, meta) => _dtHelper.renderText(full.deliverToName),
                        title: "Deliver To"
                    },
                    {
                        data: "serviceName",
                        title: "Item",
                        //width: "50px"
                    }
                ]
            };
            _grid = table.DataTableInit(_gridOptions);

            _modalManager.getModal().on('shown.bs.modal', function () {
                _grid
                    .columns.adjust()
                    .responsive.recalc();
            });
        };

        function reloadGrid() {
            if (_grid) {
                _grid.ajax.reload();
            }
        }

        this.setFilter = function (filter) {
            _filter = filter;
            reloadGrid();
        };

        
        this.save = async function () {
            var selectedRows = _gridOptions.selectionColumnOptions.getSelectedRows();
            abp.event.trigger('app.quoteBasedOrderLinesSelectedModal', {
                selectedLines: selectedRows
            });

            _modalManager.close();
        }

    };
})(jQuery);