(function($) {
    app.modals.CreateOrEditOrderLineOfficeAmountModal = function () {

        var _modalManager;
        var _orderAppService = abp.services.app.order;
        var _$form = null;

        this.init = function(modalManager) {
            _modalManager = modalManager;
            
            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            abp.helper.ui.initControls();

            var actualQuantityInput = _$form.find("#ActualQuantity");
            
            if (actualQuantityInput.val() !== "") {
                actualQuantityInput.val(abp.utils.round(actualQuantityInput.val()));
            }
        };

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var orderLineOfficeAmount = _$form.serializeFormToObject();
            
            _modalManager.setBusy(true);
            _orderAppService.editOrderLineOfficeAmount(orderLineOfficeAmount).done(function (e) {
                abp.notify.info('Saved successfully.');
                _modalManager.close();
                abp.event.trigger('app.createOrEditOrderLineOfficeAmountModalSaved', e);
            }).always(function () {
                _modalManager.setBusy(false);
            });
            
        };
    };
})(jQuery);