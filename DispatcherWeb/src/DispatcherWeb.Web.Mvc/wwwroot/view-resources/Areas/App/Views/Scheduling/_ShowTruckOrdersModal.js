(function ($) {
    app.modals.ShowTruckOrdersModal = function () {

        var _modalManager;
        var _schedulingService = abp.services.app.scheduling;
        var _$form = null;
        var _dtHelper = abp.helper.dataTables;
        var _permissions = {
            editOrder: abp.auth.isGranted('Pages.Orders.Edit')
        };

        this.init = function (modalManager) {
            _modalManager = modalManager;
            _$form = _modalManager.getModal().find('form');

            var $truckOrdersTable = _modalManager.getModal().find('#TruckOrdersTable');
            var $truckOrdersGrid = $truckOrdersTable.DataTableInit({
                paging: false,
                ordering: false,
                ajax: function (data, callback, settings) {
                    _schedulingService.getTruckOrderLinesPaged({
                        scheduleDate: _$form.find('#ScheduleDate').val(),
                        shift: _$form.find('#Shift').val(),
                        truckId: _$form.find('#TruckId').val()
                    }).done(function (abpResult) {
                        callback(_dtHelper.fromAbpResult(abpResult));
                    });
                },
                columns: [
                    {
                        className: 'control responsive',
                        orderable: false,
                        title: "&nbsp;",
                        width: "10px",
                        render: function () {
                            return '';
                        }
                    },
                    {
                        data: "driverName",
                        title: "Driver"
                    },
                    {
                        data: "startTime",
                        title: "Time on Job",
                        render: function (data, type, full, meta) {
                            return _dtHelper.renderTime(data, '');
                        }
                    },
                    {
                        data: "customer",
                        title: "Customer"
                    },
                    {
                        data: "loadAtName",
                        title: "Load At"
                    },
                    {
                        data: "deliverToName",
                        title: "Deliver to"
                    },
                    {
                        data: "utilization",
                        title: "Portion of day"
                    },
                    {
                        data: "item",
                        title: "Item"
                    },
                    {
                        data: "quantityFormatted",
                        title: "Quantity"
                    },
                    {
                        data: "sharedDateTime",
                        title: "Shared",
                        render: function (data, type, full, meta) {
                            return _dtHelper.renderDateTime(data);
                        }
                    },
                    {
                        title: "",
                        data: null,
                        orderable: false,
                        autoWidth: false,
                        width: "100px",
                        responsivePriority: 2,
                        render: function (data, type, full, meta) {
                            return _permissions.editOrder ? '<a class="btn btn-default">View</a>' : '';
                        }
                    }
                ]
            });

            $truckOrdersTable.hide();
            modalManager.getModal().on('shown.bs.modal', function () {
                $truckOrdersTable.show(0);
                $truckOrdersGrid.columns.adjust().responsive.recalc();
            });

            $truckOrdersTable.on('click', 'td a', function (e) {
                e.preventDefault();
                var orderId = _dtHelper.getRowData(this).orderId;
                window.location = abp.appPath + 'app/Orders/Details/' + orderId;
            });

        };

    };
})(jQuery);