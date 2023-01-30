(function ($) {
    app.modals.AssignDriverForTruckModal = function () {

        var _modalManager;
        var _driverAssignmentService = abp.services.app.driverAssignment;
        var _schedulingService = abp.services.app.scheduling;
        var _$form = null;
        var _originalDriverId = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            var allowSubcontractorsToDriveCompanyOwnedTrucks = abp.setting.getBoolean('App.LeaseHaulers.AllowSubcontractorsToDriveCompanyOwnedTrucks');

            var driverIdDropdown = _$form.find("#DriverId");
            driverIdDropdown.select2Init({
                abpServiceMethod: abp.services.app.driver.getDriversSelectList,
                abpServiceParams: {
                    officeId: allowSubcontractorsToDriveCompanyOwnedTrucks ? null : _$form.find('#OfficeId').val(),
                    includeLeaseHaulerDrivers: allowSubcontractorsToDriveCompanyOwnedTrucks,
                },
                showAll: true
            });

            _originalDriverId = driverIdDropdown.val();

        };

        this.save = async function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var input = _$form.serializeFormToObject();

            try {
                _modalManager.setBusy(true);
                //let setDriverResult = await _driverAssignmentService.setDriverForTruck(input);
                //if (!setDriverResult.success && setDriverResult.orderLineTruckExists) {
                //    if (!await abp.message.confirm(
                //        'The previous driver has open dispatches for this date and shift. Would you like those dispatches to be canceled?'
                //    )) {
                //        _modalManager.close();
                //        return;
                //    }

                //    _modalManager.setBusy(true);
                //    await _schedulingService.deleteOrderLineTrucks({
                //        truckId: input.TruckId,
                //        date: input.Date,
                //        shift: input.Shift
                //    });

                //    setDriverResult = await _driverAssignmentService.setDriverForTruck(input);
                //}

                //if (!setDriverResult.success) {
                //    abp.message.error('Error detail not sent by server.', 'An error has occurred!');
                //    return;
                //}

                if (_originalDriverId) {
                    var validationResult = await _driverAssignmentService.hasOrderLineTrucks({
                        driverId: _originalDriverId,
                        truckId: input.TruckId,
                        officeId: input.OfficeId,
                        date: input.Date,
                        shift: input.Shift
                    });
                    if (validationResult.hasOrderLineTrucks) {
                        _modalManager.setBusy(false);
                        var userResponse = await swal(
                            app.localize("DriverAlreadyScheduledForTruck{0}Prompt_YesToReplace_NoToCreateNew", input.TruckCode),
                            {
                                buttons: {
                                    no: "No",
                                    yes: "Yes"
                                }
                            }
                        );
                        _modalManager.setBusy(true);
                        if (userResponse === 'no') {
                            input.CreateNewDriverAssignment = true;
                        } else {
                            if (validationResult.hasOpenDispatches) {
                                abp.message.error(app.localize("CannotChangeDriverBecauseOfDispatchesError"));
                                return;
                            }
                        }
                    }
                }

                await _driverAssignmentService.setDriverForTruck(input);
                abp.notify.info('Saved successfully.');
                _modalManager.close();
                abp.event.trigger('app.assignDriverForTruckModalSaved');
            }
            finally {
                _modalManager.setBusy(false);
            }
        };
    };
})(jQuery);