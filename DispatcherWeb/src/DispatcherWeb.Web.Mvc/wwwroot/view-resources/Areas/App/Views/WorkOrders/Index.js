﻿(function () {
    $(function () {

        var _workOrderService = abp.services.app.workOrder;
        var _dtHelper = abp.helper.dataTables;

        initFilterControls();

        var workOrdersTable = $('#WorkOrdersTable');
        var vehicleIssuesGrid = workOrdersTable.DataTableInit({
            stateSave: true,
            stateDuration: 0,
            stateLoadCallback: function (settings, callback) {
                app.localStorage.getItem('workorders_filter', function (result) {
                    var filter = result || {};

                    if (filter.issueDateFilter) {
                        $('#IssueDateFilter').val(filter.issueDateFilter);
                    } else {
                        $('#IssueDateFilter').val('');
                    }
                    if (filter.startDateFilter) {
                        $('#StartDateFilter').val(filter.startDateFilter);
                    } else {
                        $('#StartDateFilter').val('');
                    }
                    if (filter.completionDateFilter) {
                        $('#CompletionDateFilter').val(filter.completionDateFilter);
                    } else {
                        $('#CompletionDateFilter').val('');
                    }
                    if (filter.truckId) {
                        abp.helper.ui.addAndSetDropdownValue($("#TruckFilter"), filter.truckId, filter.truckCode);
                    }
                    if (filter.assignedToId) {
                        abp.helper.ui.addAndSetDropdownValue($("#AssignedToFilter"), filter.assignedToId, filter.assignedToName);
                    }

                    app.localStorage.getItem('workorders_grid', function (result) {
                        callback(JSON.parse(result));
                    });
                });
            },
            stateSaveCallback: function (settings, data) {
                delete data.columns;
                delete data.search;
                app.localStorage.setItem('workorders_grid', JSON.stringify(data));
                app.localStorage.setItem('workorders_filter', _dtHelper.getFilterData());
            },
            ajax: function (data, callback, settings) {
                var abpData = _dtHelper.toAbpData(data);
                $.extend(abpData, _dtHelper.getFilterData());
                $.extend(abpData, _dtHelper.getDateRangeObject(abpData.issueDateFilter, 'issueDateBegin', 'issueDateEnd'));
                $.extend(abpData, _dtHelper.getDateRangeObject(abpData.startDateFilter, 'startDateBegin', 'startDateEnd'));
                $.extend(abpData, _dtHelper.getDateRangeObject(abpData.completionDateFilter, 'completionDateBegin', 'completionDateEnd'));
                _workOrderService.getWorkOrderPagedList(abpData).done(function (abpResult) {
                    callback(_dtHelper.fromAbpResult(abpResult));
                    abp.helper.ui.initControls();
                });
            },
            order: [[0, 'asc']],
            columns: [
                {
                    width: '20px',
                    className: 'control responsive',
                    orderable: false,
                    render: function () {
                        return '';
                    },
                    targets: 0
                }, 
                {
                    targets: 1,
                    data: "issueDate",
                    title: "Issue Date",
                    render: function (data, type, full, meta) { return _dtHelper.renderUtcDate(data); }
                },
                {
                    targets: 2,
                    data: "startDate",
                    title: "Start Date",
                    render: function (data, type, full, meta) { return _dtHelper.renderUtcDate(data); }
                },
                {
                    targets: 3,
                    data: "completionDate",
                    title: "Completion Date",
                    render: function (data, type, full, meta) { return _dtHelper.renderUtcDate(data); }
                },
                {
                    targets: 4,
                    data: "status",
                    title: "Status"
                },
                {
                    targets: 5,
                    data: "vehicle",
                    title: "Vehicle"
                },
                {
                    targets: 6,
                    data: "note",
                    title: "Note"
                },
                {
                    targets: 7,
                    data: "odometer",
                    title: "Odometer"
                },
                {
                    targets: 8,
                    data: "assignedTo",
                    title: "Assigned to"
                },
                {
                    targets: 9,
                    name: "Actions",
                    data: null,
                    orderable: false,
                    autoWidth: false,
                    defaultContent: '',
                    responsivePriority: 1,
                    width: '10px',
                    rowAction: {
                        items: [{
                            text: '<i class="fa fa-edit"></i> ' + app.localize('Edit'),
                            visible: function (data) {
                                return data.record.canEdit;
                            },
                            action: function (data) {
                                var workOrderId = data.record.id;
                                window.location.href = abp.appPath + 'app/WorkOrders/Details/' + workOrderId;
                            }
                        }, {
                            text: '<i class="fa fa-trash"></i> ' + app.localize('Delete'),
                            visible: function (data) {
                                return data.record.canEdit;
                            },
                            action: async function (data) {
                                var workOrderId = data.record.id;
                                if (await abp.message.confirm(
                                    'Are you sure you want to delete the Work Order?'
                                )) {
                                    _workOrderService.deleteWorkOrder({
                                        id: workOrderId
                                    }).done(function () {
                                        abp.notify.info('Successfully deleted.');
                                        reloadMainGrid();
                                    });
                                }
                            }
                        }]
                    }
                }
                
            ]
        });

        function initFilterControls() {
            var drpOptions = {
                locale: {
                    cancelLabel: 'Clear'
                }
            };
            $("#StartDateFilter, #CompletionDateFilter, #IssueDateFilter").daterangepicker(drpOptions)
                .on('apply.daterangepicker', function (ev, picker) {
                    $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
                })
                .on('cancel.daterangepicker', function (ev, picker) {
                    $(this).val('');
                });

            $("#TruckFilter").select2Init({
                abpServiceMethod: abp.services.app.truck.getTrucksSelectList,
                abpServiceParams: { allOffices: true, inServiceOnly: true },
                showAll: false,
                allowClear: true
            });
            $('#AssignedToFilter').select2Init({
                abpServiceMethod: abp.services.app.user.getMaintenanceUsersSelectList,
                showAll: false,
                allowClear: true
            });
            $("#StatusFilter").select2Init({
                showAll: true,
                allowClear: true 
            });
        }


        var reloadMainGrid = function () {
            vehicleIssuesGrid.ajax.reload();
        };

        abp.event.on('app.createOrEditServiceModalSaved', function () {
            reloadMainGrid();
        });

        $("#CreateNewWorkOrderButton").click(function (e) {
            e.preventDefault();
            window.location.href = $(this).attr('formaction');
        });


        $("#SearchButton").closest('form').submit(function (e) {
            e.preventDefault();
            reloadMainGrid();
        });

        $("#ClearSearchButton").click(function () {
            $(this).closest('form')[0].reset();
            $(".filter").change();
            reloadMainGrid();
        });

    });
})();