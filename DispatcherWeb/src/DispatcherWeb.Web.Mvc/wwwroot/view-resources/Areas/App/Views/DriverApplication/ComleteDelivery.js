'use strict';
(function () {
    $(function () {

        var _dispatchingService = abp.services.app.dispatching;
        var _addSignatureModal = new app.ModalManager({
            viewUrl: abp.appPath + 'App/DriverApplication/AddSignatureModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/DriverApplication/_AddSignatureModal.js',
            modalClass: 'AddSignatureModal'
        });

        $('form').submit(function (e) {
            e.preventDefault();

            var form = $(this);
            if (!form.valid()) {
                form.showValidateMessage();
                return;
            }

            var dispatchTicket = form.serializeFormToObject();

            abp.helper.getLocation(function (position) {
                if (position) {
                    dispatchTicket.destinationLatitude = position.coords.latitude;
                    dispatchTicket.destinationLongitude = position.coords.longitude;
                }
                save();
            });

            function save() {
                abp.ui.setBusy(form);
                _dispatchingService.completeDispatch(dispatchTicket)
                    .done(function (result) {
                        if (result.isCanceled) {
                            window.location.reload(true);
                            abp.notify.info('The Dispatch is canceled by dispatcher.');
                        } else if (result.notFound) {
                            window.location.reload(true);
                            abp.notify.info('The Dispatch was not found.');
                        } else {
                            abp.notify.info('Saved successfully.');
                            window.location.reload(true);
                        }
                    })
                    .always(function () {
                        abp.ui.clearBusy(form);
                    });
            }

        });

        $('#AddSignatureButton').click(function (e) {
            e.preventDefault();
            var guid = $('#Guid').val();
            _addSignatureModal.open({guid: guid});
        });

        abp.event.on('app.signatureAddedModal', function () {
            $("#AddSignatureButton").closest('div.row').hide();
        });
    });
})();

