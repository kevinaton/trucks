(function($) {
    app.modals.SetTruckUtilizationModal = function () {

        var _modalManager;
        var _schedulingService = abp.services.app.scheduling;
        var _$form = null;

        this.init = function(modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();
            
            abp.helper.ui.initControls();

            _$form.find("#TimeOnJob").timepickerInit({ stepping: 1 });

            var that = this;
            var maxUtilization = abp.utils.round(_$form.find("#MaxUtilization").val());
            _$form.find('.SetUtilizationButton').each(function () {
                var utilization = abp.utils.round($(this).data('utilization'));
                if (utilization > maxUtilization) {
                    $(this).attr('disabled', true);
                } else {
                    $(this).click(function () {
                        _$form.find("#Utilization").val(utilization);
                        that.save();
                    });
                }
            });
        };

        this.save = async function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }
            
            var formData = _$form.serializeFormToObject();

            await saveDetails(formData);

            if (formData.Utilization > 0) {
                saveUtilization();
            } else {
                abp.scheduling.checkExistingDispatchesBeforeRemovingTruck(
                    formData.OrderLineTruckId,
                    formData.TruckCode,
                    function () {
                        saveUtilization();
                    },
                    function () {},
                    function () {
                        markAsDone();
                    }
                );
            }

            async function saveDetails(formData) {
                try {
                    _modalManager.setBusy(true);
                    await _schedulingService.setOrderLineTruckDetails({
                        orderLineTruckId: formData.OrderLineTruckId,
                        timeOnJob: formData.TimeOnJob
                    })
                } finally {
                    _modalManager.setBusy(false);
                }
            }

            function saveUtilization() {
                _modalManager.setBusy(true);
                _schedulingService.setOrderTruckUtilization({
                    orderLineTruckId: formData.OrderLineTruckId,
                    maxUtilization: formData.MaxUtilization,
                    utilization: formData.Utilization
                }).done(function () {
                    abp.notify.info('Saved successfully.');
                    _modalManager.close();
                    abp.event.trigger('app.truckUtilizationModalSaved');
                }).fail(function () {
                    var $utilization = _$form.find("#Utilization");
                    $utilization.val($utilization.data('initial-value'));
                }).always(function () {
                    _modalManager.setBusy(false);
                });
            }

            function markAsDone() {
                _schedulingService.deleteOrderLineTruck({
                    orderLineTruckId: formData.OrderLineTruckId,
                    orderLineId: formData.OrderLineId,
                    markAsDone: true
                }).done(function () {
                    abp.notify.info('Mark as done successfully.');
                    _modalManager.close();
                    abp.event.trigger('app.truckUtilizationModalSaved');
                }).fail(function () {
                    var $utilization = _$form.find("#Utilization");
                    $utilization.val($utilization.data('initial-value'));
                }).always(function () {
                    _modalManager.setBusy(false);
                });

            }
        };
    };
})(jQuery);