(function($) {
    app.modals.CreateQuoteFromOrderModal = function () {

        var _modalManager;
        var _quoteService = abp.services.app.quote;
        var _$form = null;

        this.init = function(modalManager) {
            _modalManager = modalManager;

            _modalManager.getModal().find('.save-button').text('OK');

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            abp.helper.ui.initControls();

        };

        this.save = function () {
            if (!_$form.valid()) {
            	_$form.showValidateMessage();
                return;
            }
            
            var formData = _$form.serializeFormToObject();
            
            _modalManager.setBusy(true);
            _quoteService.createQuoteFromOrder(formData).done(function (newQuoteId) {
                abp.notify.info('Created successfully.');
                _modalManager.close();
                abp.event.trigger('app.quoteCreatedFromOrderModal', {
                    newQuoteId: newQuoteId
                });
                abp.message.confirmWithOptions({
                        text: '',
                        title: 'The new quote has been created. Do you want us to open the quote for you?',
                        cancelButtonText: 'No'
                    },
                    function (isConfirmed) {
                        if (isConfirmed) {
                            window.location = abp.appPath + 'app/Quotes/Details/' + newQuoteId;
                        }
                    }
                );
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };
    };
})(jQuery);