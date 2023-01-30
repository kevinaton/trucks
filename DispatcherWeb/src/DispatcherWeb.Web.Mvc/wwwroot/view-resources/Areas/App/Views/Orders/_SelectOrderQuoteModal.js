(function($) {
    app.modals.SelectOrderQuoteModal = function () {

        var _modalManager;
        var _$form = null;
        var _capturedQuoteInput = null;
        var _quoteInput = null;

        this.init = function(modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();
            
            abp.helper.ui.initControls();

            var cancelButton = _modalManager.getModal().find('.close-button');
            cancelButton.hide();
            var okButton = _modalManager.getModal().find('.save-button');
            okButton.find('span').text('OK');
            okButton.find('.fa').remove();

            var quoteInputContainer = _$form.find("#QuoteInputContainer");
            
            abp.event.trigger('app.selectOrderQuoteModal.requestInput', function (input) {
                _capturedQuoteInput = $(input);
                _quoteInput = _capturedQuoteInput.clone();
                _quoteInput.attr('id', 'PopupQuoteId');
                _quoteInput.appendTo(quoteInputContainer);
            });
        };

        this.save = function () {
            if (_capturedQuoteInput) {
                _capturedQuoteInput.val(_quoteInput.val());
            }

            _modalManager.close();

            if (_capturedQuoteInput) {
                _capturedQuoteInput.change();
            }
        };
    };
})(jQuery);