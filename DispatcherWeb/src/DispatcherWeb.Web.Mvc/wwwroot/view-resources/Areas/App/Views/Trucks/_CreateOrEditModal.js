(function ($) {
    app.modals.CreateOrEditTruckModal = function () {

        var _modalManager;
        var _truckService = abp.services.app.truck;
        var _$form = null;
        var _dtHelper = abp.helper.dataTables;
        var _wasActive = null;


        var _createOrEditPreventiveMaintenanceModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/PreventiveMaintenanceSchedule/CreateOrEditPreventiveMaintenanceModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/PreventiveMaintenanceSchedule/_CreateOrEditPreventiveMaintenanceModal.js',
            modalClass: 'CreateOrEditPreventiveMaintenanceModal'

        });

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('#DriverdetailsForm');
            _$form.validate({ ignore: "" });
            _$form.find('#CurrentMileage').rules('add', { mileage: true });

            abp.helper.ui.initControls();

            var $reasonCtrl = _$form.find('#Reason');
            var $reasonDiv = $reasonCtrl.parent();

            _$form.find('#IsOutOfService').on('change', function (e) {
                if ($(this).is(':checked')) {
                    $reasonDiv.show('medium');
                    $reasonCtrl.rules('add', { required: true });
                } else {
                    $reasonDiv.hide('medium');
                    $reasonCtrl.rules('remove', 'required');
                }
            }).change();

            var $inactivationDateCtrl = _$form.find('#InactivationDate');
            var $inactivationDateDiv = $inactivationDateCtrl.parent();

            var isActiveCheckbox = _$form.find('#IsActive');
            isActiveCheckbox.on('change', function (e) {
                if ($(this).is(':checked')) {
                    $inactivationDateCtrl.removeAttr('required');
                    $inactivationDateDiv.hide('medium');
                } else {
                    $inactivationDateDiv.show('medium');
                    $inactivationDateCtrl.attr('required', 'required');
                    if (!$inactivationDateCtrl.val()) {
                        $inactivationDateCtrl.val(moment().format('MM/DD/YYYY'));
                    }
                }
            });
            _wasActive = isActiveCheckbox.is(':checked');

            _$form.find('.datepicker').datepickerInit();

            $('#InServiceDate').data('DateTimePicker').minDate(moment('1/1/1980', 'M/D/YYYY')).maxDate(moment('1/1/2100', 'M/D/YYYY'));


            _$form.find("select#OfficeId").select2Init({
                abpServiceMethod: abp.services.app.office.getOfficesSelectList,
                showAll: true,
                allowClear: false
            });

            var $defaultDriverId = _$form.find("#DefaultDriverId");
            var $defaultTrailerId = _$form.find("#DefaultTrailerId");
            var vehicleCategoryDropdown = _$form.find("#VehicleCategoryId");
            var canPullTrailerCheckbox = _$form.find('#CanPullTrailer');

            var defaultDriverIdLastValue = $defaultDriverId.val();
            var defaultTrailerIdLastValue = $defaultTrailerId.val();

            vehicleCategoryDropdown.change(function () {
                var dropdownData = vehicleCategoryDropdown.select2('data');
                let isPowered = null;
                let assetType = null;
                if (dropdownData && dropdownData.length && dropdownData[0].item) {
                    isPowered = dropdownData[0].item.isPowered;
                    assetType = dropdownData[0].item.assetType;
                }
                _$form.find("#VehicleCategoryIsPowered").val(isPowered);
                _$form.find("#VehicleCategoryAssetType").val(assetType);
                _$form.find('#IsApportioned').closest('.form-group').toggle(isPowered === true);
                _$form.find('#BedConstruction').closest('.form-group').toggle([abp.enums.assetType.dumpTruck, abp.enums.assetType.trailer].includes(assetType));
                canPullTrailerCheckbox.closest('.form-group').toggle(isPowered === true);
                canPullTrailerCheckbox.prop('checked', assetType === abp.enums.assetType.tractor).change();
                var shouldDisableDefaultDriver = isPowered !== true;
                if (shouldDisableDefaultDriver) {
                    defaultDriverIdLastValue = $defaultDriverId.val();
                    $defaultDriverId.val(null).change();
                } else {
                    $defaultDriverId.val(defaultDriverIdLastValue).change();
                }
                $defaultDriverId.prop('disabled', shouldDisableDefaultDriver);

                vehicleCategoryDropdown.find('option').not(`[value=""],[value="${vehicleCategoryDropdown.val()}"]`).remove();
            });

            canPullTrailerCheckbox.change(function () {
                let canPullTrailer = canPullTrailerCheckbox.is(':checked');
                var shouldHideDefaultTrailer = !canPullTrailer; //assetType !== abp.enums.assetType.tractor;
                if (shouldHideDefaultTrailer) {
                    defaultTrailerIdLastValue = $defaultTrailerId.val();
                    $defaultTrailerId.val(null).change();
                    //$defaultTrailerId.closest('div').hide();
                    $defaultTrailerId.closest('div').slideUp();
                } else {
                    $defaultTrailerId.val(defaultTrailerIdLastValue).change();
                    //$defaultTrailerId.closest('div').show();
                    $defaultTrailerId.closest('div').slideDown();
                }
            });

            $defaultDriverId.select2Init({
                abpServiceMethod: abp.services.app.driver.getDriversSelectList,
                abpServiceParams: {
                    includeLeaseHaulerDrivers: abp.setting.getBoolean('App.LeaseHaulers.AllowSubcontractorsToDriveCompanyOwnedTrucks')
                },
                showAll: true,
                //dropdownParent: $("#" + _modalManager.getModalId())
            });

            $defaultTrailerId.select2Init({
                abpServiceMethod: abp.services.app.truck.getActiveTrailersSelectList,
                showAll: true,
                //dropdownParent: $("#" + _modalManager.getModalId())
            });

            vehicleCategoryDropdown.select2Init({
                abpServiceMethod: abp.services.app.truck.getVehicleCategoriesSelectList,
                showAll: true,
                allowClear: false
            });

            $("#FuelType").select2Init({
                allowClear: true,
                showAll: true
            });





            var $modal = _modalManager.getModal();
            $modal.find('#SaveCurrentMileageAndHours').click(function (e) {
                e.preventDefault();
                var $currentMileage = $modal.find('#CurrentMileage');
                var $currentHours = $modal.find('#CurrentHours');
                if (!$currentMileage.valid() || !$currentHours.valid()) {
                    return;
                }
                _modalManager.setBusy(true);
                _truckService.saveCurrentMileageAndHours({ truckId: getTruckId(), currentMileage: $currentMileage.val(), currentHours: $currentHours.val() }).done(function () {
                    abp.notify.info('Saved successfully.');
                    abp.event.trigger('app.currentMileageSaved');
                }).always(function () {
                    _modalManager.setBusy(false);
                });
            });

            var preventiveMaintenanceTable = $modal.find('#PreventiveMaintenanceTable');
            var preventiveMaintenanceGrid = preventiveMaintenanceTable.DataTableInit({
                paging: false,
                info: false,
                ordering: false,
                ajax: function (data, callback, settings) {
                    _truckService.getPreventiveMaintenanceDueByTruck(getTruckId()).done(function (abpResult) {
                        callback(_dtHelper.fromAbpResult(abpResult));
                    });
                },
                columns: [
                    {
                        data: "vehicleServiceName",
                        title: "Service	Name"
                    },
                    {
                        data: "dueDate",
                        title: "Due Date",
                        render: function (data, type, full, meta) {
                            return _dtHelper.renderUtcDate(data);
                        }
                    },
                    {
                        data: "dueMileage",
                        title: "Due Mileage"
                    },
                    {
                        data: "dueHour",
                        title: "Due Hours"
                    }
                ]
            });
            $modal.find('#AddServiceReminder').click(function (e) {
                e.preventDefault();
                _createOrEditPreventiveMaintenanceModal.open({ truckId: getTruckId(), truckCode: _$form.find('#TruckCode').val() });
            });
            abp.event.on('app.createOrEditPreventiveMaintenanceModal', function () {
                preventiveMaintenanceGrid.ajax.reload();
            });


            var maintenanceHistoryTable = $modal.find('#MaintenanceHistoryTable');
            var maintenanceHistoryGrid = maintenanceHistoryTable.DataTableInit({
                paging: false,
                info: false,
                ordering: false,
                ajax: function (data, callback, settings) {
                    _truckService.getServiceHistoryByTruck(getTruckId()).done(function (abpResult) {
                        callback(_dtHelper.fromAbpResult(abpResult));
                    });
                },
                columns: [
                    {
                        data: "vehicleServiceName",
                        title: "Service"
                    },
                    {
                        data: "completionDate",
                        title: "Date",
                        render: function (data, type, full, meta) {
                            return _dtHelper.renderUtcDate(data);
                        }
                    },
                    {
                        data: "mileage",
                        title: "Mileage"
                    }
                ]
            });

            _$form.find('#File').click(function (e) {
                if (app.showWarningIfFreeVersion()) {
                    e.preventDefault();
                    return;
                }
                if (!_$form.valid()) {
                    e.preventDefault();
                    _$form.showValidateMessage();
                }
            });
            _$form.find('#File').fileupload({
                add: function (e, data) {
                    var truckId = getTruckId();
                    if (!truckId) {
                        saveTruck(function (truckId) {
                            addFile(data, truckId);
                        });
                    } else {
                        addFile(data, truckId);
                    }
                },
                submit: function (e, data) {
                    _modalManager.setBusy(true);
                },
                done: function (e, data) {
                    console.log(data);
                    var id = data.result.result.id;
                    $.get('/app/Trucks/GetFileRow?id=' + id, function (htmlRow) {
                        var $tableBody = $('#FilesTable tbody');
                        $tableBody.append(htmlRow);
                    });

                    _modalManager.setBusy(false);
                }
            });
            function addFile(data, truckId) {
                data.formData = { 'id': truckId };
                data.submit();
            }

            $('#FilesTable').on('click', 'button', function (e) {
                e.preventDefault();
                var $button = $(this);
                var id = $button.data('id');
                abp.ui.setBusy($button);
                _truckService.deleteFile({ id: id })
                    .done(function () {
                        $button.closest('tr').remove();
                    })
                    .always(function () {
                        abp.ui.clearBusy($button);
                    }
                    );
            });


            function getTruckId() {
                return _$form.find('#Id').val();
            }

            var dtdTrackerDeviceTypeInput = _$form.find("#DtdTrackerDeviceTypeId");
            dtdTrackerDeviceTypeInput.select2Init({
                abpServiceMethod: abp.services.app.truckTelematics.getWialonDeviceTypesSelectList
            });
            
            dtdTrackerDeviceTypeInput.change(function () {
                var dropdownData = dtdTrackerDeviceTypeInput.select2('data');
                var serverAddress = '';
                var deviceTypeName = '';
                if (dropdownData && dropdownData.length && dropdownData[0].item) {
                    serverAddress = dropdownData[0].item.serverAddress;
                    deviceTypeName = dropdownData[0].name;
                }
                _$form.find("#DtdTrackerServerAddress").val(serverAddress);
                _$form.find("#DtdTrackerDeviceTypeName").val(deviceTypeName);
                dtdTrackerDeviceTypeInput.removeUnselectedOptions();
            });

        };

        //_$generalTab = _modalManager.getModal().find('#GeneralTab');
        //_$generalTab.validate({ ignore: "" });

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            saveTruck(function () {
                _modalManager.close();
            });
        };

        async function saveTruck(doneAction) {
            var truck = _$form.serializeFormToObject();
            truck.files = getFiles();

            try {
                if (truck.DefaultTrailerId !== '' && truck.CanPullTrailer) { //truck.VehicleCategoryAssetType === abp.enums.assetType.tractor.toString()
                    _modalManager.setBusy(true);
                    let tractorWithDefaultTrailer = await _truckService.getTractorWithDefaultTrailer({
                        trailerId: truck.DefaultTrailerId,
                        tractorId: truck.Id
                    });
                    if (tractorWithDefaultTrailer) {
                        _modalManager.setBusy(false);
                        let isConfirmed = await abp.message.confirm('Trailer ' + _$form.find("#DefaultTrailerId option:selected").text()
                            + ' is already defaulted to truck ' + tractorWithDefaultTrailer
                            + '. If you continue with this operation, the trailer will be moved to this new truck. Is this what you want to do?');
                        if (!isConfirmed) {
                            return;
                        }
                    }
                }

                if (truck.VehicleCategoryAssetType === abp.enums.assetType.trailer.toString() && _wasActive && !truck.IsActive) {
                    _modalManager.setBusy(true);
                    let tractorWithDefaultTrailer = await _truckService.getTractorWithDefaultTrailer({
                        trailerId: truck.Id
                    });
                    if (tractorWithDefaultTrailer) {
                        _modalManager.setBusy(false);
                        let isConfirmed = await abp.message.confirm("This trailer is currently set as the default trailer on truck "
                            + tractorWithDefaultTrailer
                            + ". Are you sure you want to make this trailer inactive?");
                        if (!isConfirmed) {
                            return;
                        }
                    }
                }

                if (truck.DefaultDriverId) {
                    _modalManager.setBusy(true);
                    let truckWithSameDefaultDriver = await _truckService.getTruckCodeByDefaultDriver({
                        exceptTruckId: truck.Id,
                        defaultDriverId: truck.DefaultDriverId
                    });
                    if (truckWithSameDefaultDriver) {
                        _modalManager.setBusy(false);
                        let isConfirmed = await abp.message.confirm('Driver ' + _$form.find("#DefaultDriverId option:selected").text()
                            + ' is already associated with another truck ' + truckWithSameDefaultDriver
                            + '. Do you want to change the default truck assignment for this driver?');
                        if (!isConfirmed) {
                            return;
                        }
                    }
                }

                _modalManager.setBusy(true);
                let editResult = await _truckService.editTruck(truck);

                if (editResult.neededBiggerNumberOfTrucks > 0) {
                    if (!await abp.message.confirm(app.localize('ReachedNumberOfTrucks_DoYouWantToUpgrade'))) {
                        _modalManager.setBusy(false);
                        throw new Error('Couldn\'t save because number of trucks limit is reached');
                    }

                    _modalManager.setBusy(true);
                    await _truckService.updateMaxNumberOfTrucksFeatureAndNotifyAdmins({
                        newValue: editResult.neededBiggerNumberOfTrucks
                    });

                    editResult = await _truckService.editTruck(truck);
                }

                $('#Id').val(editResult.id);
                abp.notify.info('Saved successfully.');
                var message = '';
                message += editResult.thereAreOrdersInTheFuture ? app.localize('ThereAreOrdersInTheFuture') + '\n' : '';
                message += editResult.thereWereAssociatedOrders ? app.localize('ThereWereOrdersAssociatedWithThisTruck') + '\n' : '';
                message += editResult.thereWereCanceledDispatches
                    ? app.localize('ThereWereCanceledDispatches') + '\n'
                    : '';
                message += editResult.thereWereNotCanceledDispatches
                    ? app.localize('ThereWereNotCanceledDispatches') + '\n'
                    : '';
                if (message) {
                    abp.message.info(message, 'Message');
                }
                abp.event.trigger('app.createOrEditTruckModalSaved');
                if (doneAction) {
                    doneAction(editResult.id);
                }
                
            } finally {
                _modalManager.setBusy(false);
            }
        }

        function getFiles() {
            var $fileRows = _$form.find('#FilesTable tbody tr');
            var files = [];
            $fileRows.each(function (i) {
                var $row = $(this);
                var file = {
                    Id: $row.data('id'),
                    FileId: $row.data('file-id'),
                    Title: $row.find('td:nth-child(1) input').val(),
                    Description: $row.find('td:nth-child(2) textarea').val()
                };

                files.push(file);
            });
            return files;
        }

    };
})(jQuery);