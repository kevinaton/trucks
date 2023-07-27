(function ($) {
    app.modals.SelectInvoiceStatusesModal = function () {

        var _modalManager;
        var _$form = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');

            _$form.validate();

            abp.helper.ui.initControls();
                        
            _$form.find('#InvoiceStatuses').select2Init({
                showAll: true,
                allowClear: true
            });
        };

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            let result = _$form.find('#InvoiceStatuses').val();

            _modalManager.setResult(result);
            _modalManager.close();
        };
    };
})(jQuery);