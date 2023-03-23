﻿(function ($) {
    app.modals.SpecifyPrintOptionsModal = function () {

        var _modalManager;
        var _$form = null;
        var _hidePricesInput = null;
        var _resultPromise = null;
        var _resolveResultPromise = null;
        var _rejectResultPromise = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            var saveButton = _modalManager.getModal().find('.save-button');
            _modalManager.getModal().on('shown.bs.modal', function () {
                saveButton.focus();
            });

            abp.helper.ui.initControls();

            _hidePricesInput = _$form.find("#HidePrices");

            _modalManager.onCloseOnce(function () {
                if (_rejectResultPromise) {
                    _rejectResultPromise();
                }
            });
        };

        this.getResultPromise = function () {
            if (!_resultPromise) {
                _resultPromise = new Promise((resolve, reject) => {
                    _resolveResultPromise = resolve;
                    _rejectResultPromise = reject;
                });
            }
            return _resultPromise;
        };

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            if (!_resolveResultPromise) {
                console.error("getResultPromise hasn't been called yet");
                return;
            }

            _modalManager.setBusy(true);

            var hidePrices = _hidePricesInput.is(":checked");

            _modalManager.close();
            _resolveResultPromise({
                hidePrices: hidePrices
            });
        };
    };
})(jQuery);