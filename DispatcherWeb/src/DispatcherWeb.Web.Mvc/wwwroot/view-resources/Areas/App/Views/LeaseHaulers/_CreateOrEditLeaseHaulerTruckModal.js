(function ($) {
    app.modals.CreateOrEditLeaseHaulerTruckModal = function () {

        var _modalManager;
        var _modal;
        var _truckService = abp.services.app.truck;
        var _leaseHaulerService = abp.services.app.leaseHauler;
        var _$form = null;
        var _isTruckReadonly = false;

        this.init = function (modalManager) {
            _modalManager = modalManager;
            _modal = _modalManager.getModal();

            _$form = _modal.find('form');
            _$form.validate({ ignore: "" });

            abp.helper.ui.initControls();

            if (_$form.find('#HaulingCompanyTruckId').val()) {
                _isTruckReadonly = true;
                _modal.find('.modal-footer .save-button').hide();
                _modal.find('.modal-footer .close-button').text('Close');
                _modal.find('input,select,textarea').prop('disabled', true);
            }

            var $inactivationDateCtrl = _$form.find('#InactivationDate');
            var $inactivationDateDiv = $inactivationDateCtrl.parent();

            _$form.find('#IsActive').on('change', function (e) {
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

            _$form.find('.datepicker').datepickerInit();

            var $defaultDriverId = _$form.find("#DefaultDriverId");
            var officeDropdown = _$form.find('#OfficeId');
            var vehicleCategoryDropdown = _$form.find("#VehicleCategoryId");

            var canPullTrailerCheckbox = _$form.find('#CanPullTrailer');
            var alwaysShowOnScheduleCheckbox = _$form.find('#AlwaysShowOnSchedule');

            var defaultDriverIdLastValue = $defaultDriverId.val();

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

            $defaultDriverId.select2Init({
                abpServiceMethod: abp.services.app.leaseHauler.getLeaseHaulerDriversSelectList,
                abpServiceParams: { leaseHaulerId: _$form.find('#LeaseHaulerId').val() },
                showAll: true,
                allowClear: true
            });

            vehicleCategoryDropdown.select2Init({
                abpServiceMethod: abp.services.app.truck.getVehicleCategoriesSelectList,
                showAll: true,
                allowClear: true
            });

            officeDropdown.select2Init({
                abpServiceMethod: abp.services.app.office.getOfficesSelectList,
                showAll: true,
                allowClear: false
            });

            alwaysShowOnScheduleCheckbox.change(function () {
                alwaysShowOnSchedule = $(this).is(":checked");
                if (!alwaysShowOnSchedule) {
                    officeDropdown.closest('.form-group').hide();
                } else {
                    //abp.helper.ui.addAndSetDropdownValue(officeDropdown, abp.session.officeId, abp.session.officeName);
                    if (abp.features.isEnabled('App.AllowMultiOfficeFeature')) {
                        officeDropdown.closest('.form-group').show();
                    }
                }
            });
        };

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

            try {
                _modalManager.setBusy(true);
                let editResult = await _leaseHaulerService.editLeaseHaulerTruck(truck);

                if (editResult.neededBiggerNumberOfTrucks > 0) {
                    if (!await abp.message.confirm(app.localize('ReachedNumberOfTrucks_DoYouWantToUpgrade'))) {
                        _modalManager.setBusy(false);
                        throw new Error('Couldn\'t save because number of trucks limit is reached');
                    }

                    _modalManager.setBusy(true);
                    await _truckService.updateMaxNumberOfTrucksFeatureAndNotifyAdmins({
                        newValue: editResult.neededBiggerNumberOfTrucks
                    });

                    editResult = await _leaseHaulerService.editLeaseHaulerTruck(truck);
                }

                abp.notify.info('Saved successfully.');
                abp.event.trigger('app.createOrEditLeaseHaulerTruckModalSaved');
                if (doneAction) {
                    doneAction();
                }
            } finally {
                _modalManager.setBusy(false);
            }
        }

    };
})(jQuery);