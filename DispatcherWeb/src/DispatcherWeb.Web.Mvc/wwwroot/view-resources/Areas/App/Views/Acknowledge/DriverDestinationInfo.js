(function () {
    $(function () {
        'use strict';

        var _dispatchingService = abp.services.app.dispatching;

        $('#driverDestinationForm').submit(function (e) {
            e.preventDefault();

            var form = $(this);
            if (!form.valid()) {
                form.showValidateMessage();
                return;
            }

            var dispatchTicket = form.serializeFormToObject();

            if (dispatchTicket.IsMultipleLoads === "True") {
                $('#continueMultiloadPopup').modal('show');
            } else {
                save(dispatchTicket);
            }
        });

        $('#continueMultiloadNoButton').click(function (e) {
            e.preventDefault();
            var form = $('#driverDestinationForm');
            var dispatchTicket = form.serializeFormToObject();
            dispatchTicket.ContinueMultiload = false;
            save(dispatchTicket);
        });

        $('#continueMultiloadYesButton').click(function (e) {
            e.preventDefault();
            var form = $('#driverDestinationForm');
            var dispatchTicket = form.serializeFormToObject();
            dispatchTicket.ContinueMultiload = true;
            save(dispatchTicket);
        });

        function save(dispatchTicket) {
            var form = $('#driverDestinationForm');

            abp.helper.getLocation(function (position) {
                if (position) {
                    dispatchTicket.destinationLatitude = position.coords.latitude;
                    dispatchTicket.destinationLongitude = position.coords.longitude;
                }

                abp.ui.setBusy(form);
                _dispatchingService.completeDispatch(dispatchTicket)
                    .done(function (result) {
                        if (result.isCanceled) {
                            window.location.reload();
                            abp.notify.info('The Dispatch is canceled by dispatcher.');
                        } else if (result.isCompleted) {
                            window.location.reload();
                            abp.notify.info('The Dispatch is already completed.');
                        } else if (result.notFound) {
                            window.location.reload();
                            abp.notify.info('The Dispatch was not found.');
                        } else {
                            abp.notify.info('Saved successfully.');
                            var redirectUrl = abp.appPath.slice(0, -1);
                            if (result.nextDispatchShortGuid) {
                                redirectUrl += result.nextDispatchShortGuid;
                            } else {
                                redirectUrl += form.data('url-completed');
                            }
                            window.location = redirectUrl;
                        }
                    })
                    .always(function () {
                        abp.ui.clearBusy(form);
                    });
            });
        }

    });
})();

