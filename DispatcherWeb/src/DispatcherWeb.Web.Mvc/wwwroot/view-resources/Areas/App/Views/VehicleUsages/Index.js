﻿(function () {
    $(function () {

        var _vehicleUsageService = abp.services.app.vehicleUsage;
        var _dtHelper = abp.helper.dataTables;
        var _canEdit = abp.auth.hasPermission('Pages.VehicleUsages.Edit');

        var _createOrEditVehicleUsageModal = abp.helper.createModal('CreateOrEditVehicleUsage', 'VehicleUsages');
        var _importVehicleUsageModal = abp.helper.createModal('ImportVehicleUsage', 'VehicleUsages');

        initFilterControls();

        var vehicleUsagesTable = $('#VehicleUsagesTable');
        var vehicleUsagesGrid = vehicleUsagesTable.DataTableInit({
            stateSave: true,
            stateDuration: 0,
            stateLoadCallback: function (settings, callback) {
                app.localStorage.getItem('vehicleusages_filter', function (result) {
                    var filter = result || {};

                    setOffice(filter);
                    if (filter.readingDateTime) {
                        $('#ReadingDateTimeFilter').val(filter.readingDateTime);
                    } else {
                        $('#ReadingDateTimeFilter').val('');
                    }
                    if (filter.truckId) {
                        abp.helper.ui.addAndSetDropdownValue($("#TruckFilter"), filter.truckId, filter.truckCode);
                    }

                    app.localStorage.getItem('vehicleusages_grid', function (result) {
                        callback(JSON.parse(result));
                    });
                });
            },
            stateSaveCallback: function (settings, data) {
                delete data.columns;
                delete data.search;
                app.localStorage.setItem('vehicleusages_grid', JSON.stringify(data));
                app.localStorage.setItem('vehicleusages_filter', _dtHelper.getFilterData());
            },
            ajax: function (data, callback, settings) {
                var abpData = _dtHelper.toAbpData(data);
                $.extend(abpData, _dtHelper.getFilterData());
                $.extend(abpData, _dtHelper.getDateRangeObject(abpData.readingDateTime, 'readingDateTimeBegin', 'readingDateTimeEnd'));
                _vehicleUsageService.getVehicleUsagePagedList(abpData).done(function (abpResult) {
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
                    data: "truckCode",
                    title: "Truck"
                },
                {
                    targets: 2,
                    data: "readingDateTime",
                    title: "Reading Date Time",
                    render: function (data, type, full, meta) { return _dtHelper.renderDateTime(data); }
                },
                {
                    targets: 3,
                    data: "reading",
                    title: "Reading"
                },
                {
                    targets: 4,
                    data: "readingType",
                    title: "Reading Type",
                    render: function (data, type, full, meta) { return full.readingTypeName; }
                },
                {
                    targets: 5,
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
                                return _canEdit;
                            },
                            action: function (data) {
                                var vehicleUsageId = data.record.id;
                                _createOrEditVehicleUsageModal.open({ id: vehicleUsageId, officeId: $('#OfficeIdFilter').val() });
                            }
                        }, {
                            text: '<i class="fa fa-trash"></i> ' + app.localize('Delete'),
                            visible: function (data) {
                                return _canEdit;
                            },
                            action: async function (data) {
                                var vehicleUsageId = data.record.id;
                                if (await abp.message.confirm(
                                    'Are you sure you want to delete the vehicle usage entry?'
                                )) {
                                    _vehicleUsageService.deleteVehicleUsage({
                                        id: vehicleUsageId
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
            $("#OfficeIdFilter").select2Init({
                abpServiceMethod: abp.services.app.office.getOfficesSelectList,
                showAll: true,
                allowClear: false
            });
            $("#ReadingDateTimeFilter").daterangepicker(drpOptions)
                .on('apply.daterangepicker', function (ev, picker) {
                    $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
                })
                .on('cancel.daterangepicker', function (ev, picker) {
                    $(this).val('');
                });

            $("#ReadingTypeFilter").select2Init({
                showAll: true,
                allowClear: true 
            });

            initTruckFilter();
        }
        function initTruckFilter() {
            $("#TruckFilter").val('');
            $("#TruckFilter").select2Init({
                abpServiceMethod: abp.services.app.truck.getTrucksSelectList,
                abpServiceParams: { officeId: $('#OfficeIdFilter').val(), allOffices: true },
                showAll: false,
                allowClear: true
            });
        }

        $('#OfficeIdFilter').on('change', function () {
            initTruckFilter();
        });

        var reloadMainGrid = function () {
            vehicleUsagesGrid.ajax.reload();
        };

        abp.event.on('app.createOrEditVehicleUsageModalSaved', function () {
            reloadMainGrid();
        });

        $("#CreateNewVehicleUsageButton").click(function (e) {
            e.preventDefault();
            _createOrEditVehicleUsageModal.open({ id: null, officeId: $('#OfficeIdFilter').val() });
        });
        $("#ImportVehicleUsageButton").click(function (e) {
            e.preventDefault();
            _importVehicleUsageModal.open();
        });

        $("#SearchButton").closest('form').submit(function (e) {
            e.preventDefault();
            reloadMainGrid();
        });

        $("#ClearSearchButton").click(function () {
            $(this).closest('form')[0].reset();
            setOffice({});
            $(".filter").change();
            reloadMainGrid();
        });

        function setOffice(filter) {
            if (!filter.officeId) {
                filter.officeId = abp.session.officeId;
                filter.officeName = abp.session.officeName;
            }

            if (filter.officeId) {
                abp.helper.ui.addAndSetDropdownValue($("#OfficeIdFilter"), filter.officeId, filter.officeName);
            }
        }
    });
})();