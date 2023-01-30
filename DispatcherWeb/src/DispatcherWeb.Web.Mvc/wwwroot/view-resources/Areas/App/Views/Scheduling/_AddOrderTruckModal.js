(function($) {
    app.modals.AddOrderTruckModal = function () {

        var _modalManager;
        var _schedulingService = abp.services.app.scheduling;
        var _$form = null;

        this.init = function(modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();
            
            abp.helper.ui.initControls();

            var model = _$form.serializeFormToObject();

            _schedulingService.getTrucksForOrderLine({
                officeId: model.OfficeId,
                date: model.Date,
                shift: model.Shift,
                orderLineId: model.OrderLineId,
                onlyTrailers: model.OnlyTrailers,
                isPowered: model.IsPowered
            }).done(function (result) {
                $.each(result.items, function (ind, val) {
                    var optionTag = val.truckId === parseInt(model.DefaultTrailerId) ? '<option selected="selected"></option>' : '<option></option>';
                        $(optionTag).text(val.truckCode).attr('value', val.truckId).appendTo($("#TruckId"));
                });
            });

            $("#TruckId").select2Init({
                showAll: true
            });
            
        };

        this.save = async function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }
            
            var model = _$form.serializeFormToObject();

            if (model.OnlyTrailers) {
                _modalManager.setBusy(true);
                if (await _schedulingService.isTrailerAssignedToAnotherTruck({
                    trailerId: model.TruckId,
                    parentTruckId: model.ParentTruckId,
                    date: model.Date,
                    shift: model.Shift
                })) {
                    if (!await abp.message.confirm('This trailer is already scheduled on another truck. Do you still want to use this trailer?')) {
                        _modalManager.setBusy(false);
                        return;
                    }
                }
            }

            _modalManager.setBusy(true);
            _schedulingService.addOrderLineTruck({
                orderLineId: model.OrderLineId,
                truckId: model.TruckId,
                parentId: model.ParentId
            }).done(function () {
                abp.notify.info('Saved successfully.');
                _modalManager.close();
                abp.event.trigger('app.addOrderTruckModalSaved');
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };
    };
})(jQuery);