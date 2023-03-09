(function ($) {
    app.modals.AddSignatureModal = function () {

        var _modalManager;
        var _dispatchingService = abp.services.app.dispatching;
        var _signaturePad = null;
        var _$form = null;
        var _canvas = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();
            _canvas = _modalManager.getModal().find('canvas')[0];
            _signaturePad = new SignaturePad(_canvas);

            abp.helper.ui.initControls();

            _modalManager.onOpen(function () {
                resizeCanvas();
            });

        };

        function resizeCanvas() {
            var ratio = Math.max(window.devicePixelRatio || 1, 1);
            _canvas.width = _canvas.offsetWidth * ratio;
            _canvas.height = _canvas.offsetHeight * ratio;
            _canvas.getContext("2d").scale(ratio, ratio);

            _signaturePad.clear();
        }
        //window.onresize = resizeCanvas;

        this.save = function () {

            if (_signaturePad.isEmpty()) {
                abp.message.warn('Please provide a signature first.');
                return;
            }

            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var formData = _$form.serializeFormToObject();

            _modalManager.setBusy(true);
            _dispatchingService.addSignature({
                signatureName: formData.SignatureName,
                signature: _signaturePad.toDataURL(),
                guid: formData.Guid
            }).done(function () {
                _modalManager.close();
                abp.event.trigger('app.signatureAddedModal');
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };
    };
})(jQuery);