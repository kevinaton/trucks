(function ($) {
    app.modals.ViewDispatchModal = function () {

        var _modalManager;
        var _dispatchingAppService = abp.services.app.dispatching;
        var _$form = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();



        };

    };
})(jQuery);