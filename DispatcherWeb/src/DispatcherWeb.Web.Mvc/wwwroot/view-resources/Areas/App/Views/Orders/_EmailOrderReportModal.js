(function($) {
    app.modals.EmailOrderReportModal = function () {

        var _modalManager;
        var _orderAppService = abp.services.app.order;
        var _$form = null;

        this.init = function(modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();
            $.validator.addMethod(
                "regex",
                function (value, element, regexp) {
                    var re = new RegExp(regexp, 'i');
                    return this.optional(element) || re.test(value);
                },
                "Please check your input."
            );
            _$form.find('#From').rules('add', { regex: app.regex.email });
            _$form.find('#To').rules('add', { regex: app.regex.emails });
            _$form.find('#CC').rules('add', { regex: app.regex.emails });

            abp.helper.ui.initControls();

            var saveButton = _modalManager.getModal().find('.save-button');
            saveButton.find('span').text('Send');
            saveButton.find('i.fa-save').removeClass('fa-save').addClass('fa-envelope-o');
        };

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var formData = _$form.serializeFormToObject();

            _modalManager.setBusy(true);
            _orderAppService.emailOrderReport(formData).done(function () {
                abp.notify.info('Sent successfully.');
                _modalManager.close();
                abp.event.trigger('app.emailOrderReportModalSent');
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };
    };
})(jQuery);