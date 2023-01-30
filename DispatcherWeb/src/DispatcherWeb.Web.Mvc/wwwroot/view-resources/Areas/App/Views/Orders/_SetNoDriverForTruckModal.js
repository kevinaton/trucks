(function($) {
    app.modals.SetNoDriverForTruckModal = function () {

        var _modalManager;
        var _driverAssignmentService = abp.services.app.driverAssignment;
        var _$form = null;

        this.init = function(modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();
            
            abp.helper.ui.initControls();

            var dateInput = _$form.find("#Date");
            var startDateInput = _$form.find("#StartDate");
            var endDateInput = _$form.find("#EndDate");
            
            dateInput.val(startDateInput.val() + ' - ' + endDateInput.val());

            dateInput.daterangepicker({
                    autoUpdateInput: false,
                    locale: {
                        cancelLabel: 'Cancel'
                    }
                },
                function (start, end, label) {
                    startDateInput.val(start.format('MM/DD/YYYY'));
                    endDateInput.val(end.format('MM/DD/YYYY'));
                });

            dateInput.on('apply.daterangepicker', function (ev, picker) {
                $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
                startDateInput.val(picker.startDate.format('MM/DD/YYYY'));
                endDateInput.val(picker.endDate.format('MM/DD/YYYY'));
            });

            dateInput.on('cancel.daterangepicker', function (ev, picker) {
            });

            _$form.find('#Shift').select2Init({ minimumResultsForSearch: 5, allowClear: false });
        };

        this.save = async function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }
            
            var formData = _$form.serializeFormToObject();

            try {
                _modalManager.setBusy(true);
                let openDispatchesResult = await _driverAssignmentService.thereAreOpenDispatchesForTruckOnDate({
                    truckId: formData.TruckId,
                    startDate: formData.StartDate,
                    endDate: formData.EndDate,
                    shift: formData.Shift
                });
                _modalManager.setBusy(false);
                if (openDispatchesResult.thereAreAcknowledgedDispatches) {
                    if (!await abp.message.confirm(
                        'There are acknowledged dispatches for this truck. Unacknowledged dispatches will be canceled, but you will need to deal with the acknowledged dispatch. Are you sure you want to do this?'
                    )) {
                        return;
                    }
                } else if (openDispatchesResult.thereAreUnacknowledgedDispatches) {
                    if (!await abp.message.confirm(
                        'There are open dispatches for this truck. Removing the driver will cancel these dispatches. Are you sure you want to do this?'
                    )) {
                        return;
                    }
                }

                _modalManager.setBusy(true);
                let saveResult = await _driverAssignmentService.setNoDriverForTruck({
                    truckId: formData.TruckId,
                    startDate: formData.StartDate,
                    endDate: formData.EndDate,
                    shift: formData.Shift
                });
                abp.notify.info('Saved successfully.');
                _modalManager.close();
                if (saveResult.truckWasRemovedFromOrders) {
                    abp.message.info('There were orders associated with this truck. The truck has been removed from those orders.');
                }
                abp.event.trigger('app.noDriverForTruckModalSet');
                
            }
            finally {
                _modalManager.setBusy(false);
            }
        };
    };
})(jQuery);