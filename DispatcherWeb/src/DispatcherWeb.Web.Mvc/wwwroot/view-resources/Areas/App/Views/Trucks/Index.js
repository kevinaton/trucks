﻿(function () {
    $(function () {

        var _truckService = abp.services.app.truck;
        var _truckTelematicsService = abp.services.app.truckTelematics;
        var _dtHelper = abp.helper.dataTables;
        var _createOrEditModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Trucks/CreateOrEditModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Trucks/_CreateOrEditModal.js',
            modalClass: 'CreateOrEditTruckModal'
        });
        var _addOutOfServiceReasonModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Trucks/AddOutOfServiceReasonModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Trucks/_AddOutOfServiceReasonModal.js',
            modalClass: 'AddOutOfServiceReasonModal'
        });

        var $officeIdFilterSelect = $("#OfficeIdFilter").select2Init({
            abpServiceMethod: abp.services.app.office.getOfficesSelectList,
            showAll: true,
            noSearch: true
        });

        if ($officeIdFilterSelect.data('filter-office-id')) {
            abp.helper.ui.addAndSetDropdownValue($("#OfficeIdFilter"), $officeIdFilterSelect.data('filter-office-id'), $officeIdFilterSelect.data('filter-office-name'));
        } else {
            abp.helper.ui.addAndSetDropdownValue($("#OfficeIdFilter"), abp.session.officeId, abp.session.officeName);
        }

        $("#VehicleCategoryIdFilter").select2Init({
            abpServiceMethod: abp.services.app.truck.getVehicleCategoriesSelectList,
            abpServiceParams: {
                isInUse: true
            },
            allowClear: true,
            showAll: true,
            noSearch: true
        });

        $("#StatusFilter").select2Init({
            allowClear: false,
            showAll: true,
            noSearch: true
        });

        var trucksTable = $('#TrucksTable');

        var trucksGrid = trucksTable.DataTableInit({
            paging: true,
            serverSide: true,
            processing: true,
            ajax: function (data, callback, settings) {
                var abpData = _dtHelper.toAbpData(data);
                $.extend(abpData, _dtHelper.getFilterData());
                _truckService.getTrucks(abpData).done(function (abpResult) {
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
                    responsivePriority: 1,
                    data: "truckCode",
                    title: app.localize('TruckCode')
                },
                {
                    data: "officeName",
                    title: app.localize('Office'),
                    visible: abp.features.getValue('App.AllowMultiOfficeFeature') === "true"
                },
                {
                    data: "vehicleCategoryName",
                    title: app.localize('Category')
                },
                {
                    data: "defaultDriverName",
                    title: app.localize('DefaultDriver')
                },
                {
                    data: "isActive",
                    render: function (data, type, full, meta) { return _dtHelper.renderCheckbox(full.isActive); },
                    className: "checkmark",
                    title: "Active"
                },
                {
                    data: "isOutOfService",
                    render: function (data, type, full, meta) { return _dtHelper.renderCheckbox(full.isOutOfService); },
                    className: "checkmark",
                    title: app.localize('OutOfService')
                },
                {
                    data: "currentMileage",
                    title: app.localize('CurrentMileage')
                },              
                {
                    data: null,
                    orderable: false,
                    autoWidth: false,
                    width: "10px",
                    responsivePriority: 2, 
                    render: function (data, type, full, meta) {
                        return '<div class="dropdown">'
                            + '<button class="btn btn-primary btn-sm" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-ellipsis-h"></i></button>'
                            + '<ul class="dropdown-menu dropdown-menu-right">'
                            + '<li><a class="btnEditRow" title="Edit"><i class="fa fa-edit"></i> Edit</a></li>'
                            + (
                                full.isOutOfService
                                ? '<li><a class="btnBackInService"><i class="fa fa-thumbs-up"></i> Return to service</a></li>'
                                : '<li><a class="btnOutOfService"><i class="fa fa-wrench"></i> Place out of service</a></li>'
                            )
                            + '<li><a class="btnDeleteRow" title="Delete"><i class="fa fa-trash"></i> Delete</a></li>'
                            + '</ul>'
                            + '</div>';
                    }
                }

            ],
            createdRow: function (row, data, index) {
                $('td', row).eq(7).addClass(getUntilDueClass(data.dueDateStatus, data.dueMileageStatus));
            }
        });

        function getUntilDueClass(dueDateStatus, dueMileageStatus) {
            if (dueDateStatus === true || dueMileageStatus === true) {
                return 'untildue-overdue';
            } else if (dueDateStatus === false || dueMileageStatus === false) {
                return 'untildue-warning';
            } else {
                return 'untildue-notreached';
            }
        }
        var reloadMainGrid = function () {
            trucksGrid.ajax.reload();
        };

        trucksTable.on('click', '.btnEditRow', function () {
            var record = _dtHelper.getRowData(this);
            _createOrEditModal.open({ id: record.id });
        });

        trucksTable.on('click', '.btnDeleteRow', async function (e) {
            e.preventDefault();
            var truckId = _dtHelper.getRowData(this).id;
            if (await abp.message.confirm(
                'Are you sure you want to delete the truck?'
            )) {
                _truckService.deleteTruck({
                    id: truckId
                }).done(function () {
                    abp.notify.info('Successfully deleted.');
                    reloadMainGrid();
                });
            }
        });

        trucksTable.on('click', '.btnOutOfService', function () {
            var record = _dtHelper.getRowData(this);
            _addOutOfServiceReasonModal.open({ truckId: record.id, date: moment().format("MM/DD/YYYY") });
        });

        trucksTable.on('click', '.btnBackInService', function () {
            var record = _dtHelper.getRowData(this);
            _truckService.setTruckIsOutOfService({
                truckId: record.id,
                isOutOfService: false
            }).done(function () {
                reloadMainGrid();
            });
        });

        abp.event.on('app.createOrEditTruckModalSaved', function () {
            reloadMainGrid();
        });
        abp.event.on('app.currentMileageSaved', function () {
            reloadMainGrid();
        });
        abp.event.on('app.addOutOfServiceReasonModalSaved', function () {
            reloadMainGrid();
        });

        $("#CreateNewTruckButton").click(function (e) {
            e.preventDefault();
            _createOrEditModal.open();
        });



        $('#ShowAdvancedFiltersSpan').click(function () {
            $('#ShowAdvancedFiltersSpan').hide();
            $('#HideAdvancedFiltersSpan').show();
            $('#AdvacedAuditFiltersArea').slideDown();
        });

        $('#HideAdvancedFiltersSpan').click(function () {
            $('#HideAdvancedFiltersSpan').hide();
            $('#ShowAdvancedFiltersSpan').show();
            $('#AdvacedAuditFiltersArea').slideUp();
        });

        $("#SearchButton").click(function (e) {
            console.log(1);
            e.preventDefault();
            reloadMainGrid();
        });

        $("#ClearSearchButton").click(function () {
            $(this).closest('form')[0].reset();
            $('#Filter_IsOutOfService').val('');
            $('#Filter_PlatesExpiringThisMonth').val(false);
            $(".filter").change();
            reloadMainGrid();
        });

        $('#UpdateMileageButton').click(function () {
            abp.ui.setBusy($('#UpdateMileageButton'));
            _truckTelematicsService.scheduleUpdateMileage()
                .done(function (result) {
                    if (result) {
                        abp.notify.info('Update mileage scheduled.');
                    }
                }).always(function() {
                    abp.ui.clearBusy($('#UpdateMileageButton'));
                });
        });

        $('#SyncWithWialonButton').click(async function () {
            try {
                abp.ui.setBusy($('#SyncWithWialonButton'));
                let result = await _truckTelematicsService.syncWithWialon({});
                if (result.additionalNumberOfTrucksRequired) {
                    if (await abp.message.confirm(app.localize('ReachedNumberOfTrucks_DoYouWantToUpgrade'))) {
                        abp.ui.setBusy($('#SyncWithWialonButton'));
                        await _truckTelematicsService.syncWithWialon({ increaseNumberOfTrucksIfNeeded: true });
                    }
                }

                abp.notify.info('Trucks sync with Wialon complete');
                reloadMainGrid();
            } finally {
                abp.ui.clearBusy($('#SyncWithWialonButton'));
            }
        });

        $('#SyncWithIntelliShiftButton').click(async function () {
            try {
                abp.ui.setBusy($('#SyncWithIntelliShiftButton'));
                await _truckTelematicsService.syncWithIntelliShift();
                abp.notify.info('Trucks sync with IntelliShift complete');
                reloadMainGrid();
            } finally {
                abp.ui.clearBusy($('#SyncWithIntelliShiftButton'));
            }
        });
        $('#ExportTrucksToCsvButton').click(function () {
            var $button = $(this);
            var abpData = {};
            $.extend(abpData, _dtHelper.getFilterData());
            abp.ui.setBusy($button);
            _truckService
                .getTrucksToCsv(abpData)
                .done(function (result) {
                    app.downloadTempFile(result);
                }).always(function () {
                    abp.ui.clearBusy($button);
                });
        });


    });
})();