(function ($) {
    app.modals.SendMessageModal = function () {

        var _modalManager;
        var _leaseHaulerContactMessageAppService = abp.services.app.leaseHaulerContactMessage;
        var _$form = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            abp.helper.ui.initControls();

            _$form.find("#ContactIds").select2Init();

        };

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var model = _$form.serializeFormWithMultipleToObject();
            if (!$.isArray(model.ContactIds)) {
                model.ContactIds = [model.ContactIds];
            }

            if (model.ContactIds.length === 0) {
                delete model.ContactIds;
            }

            _modalManager.setBusy(true);
            _leaseHaulerContactMessageAppService.sendMessage(model).done(function (result) {
                if (!result) {
                    abp.notify.error('There were some errors.');
                } else {
                    abp.notify.info('Sent successfully.');
                }
                _modalManager.close();
                abp.event.trigger('app.sendMessageModalSaved');
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };
    };
})(jQuery);