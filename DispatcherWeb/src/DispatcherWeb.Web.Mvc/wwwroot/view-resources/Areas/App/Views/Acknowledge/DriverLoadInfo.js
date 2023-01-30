(function () {
    $(function () {
        'use strict';

        var _dispatchingService = abp.services.app.dispatching;

        $('form').submit(function (e) {
            e.preventDefault();

            var form = $(this);
            if (!form.valid()) {
                form.showValidateMessage();
                return;
            }
            var dispatchTicket = form.serializeFormToObject();

            if (window.validateTicketFields && !window.validateTicketFields(dispatchTicket)) {
                return;
            }

            abp.helper.getLocation(function (position) {
                if (position) {
                    dispatchTicket.sourceLatitude = position.coords.latitude;
                    dispatchTicket.sourceLongitude = position.coords.longitude;
                }
                save();
            });

            function save() {
                abp.ui.setBusy(form);
                _dispatchingService.updateDispatchTicket(dispatchTicket)
                    .done(function () {
                        abp.notify.info('Saved successfully.');
                        window.location = window.location.href.split('?')[0];
                    })
                    .always(function () {
                        abp.ui.clearBusy(form);
                    });
            }

        });

    });
})();

