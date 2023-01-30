'use strict';
(function () {
    $(function () {

        var _dispatchingService = abp.services.app.dispatching;

        $('form').submit(async function (e) {
            e.preventDefault();

            var form = $(this);
            if (!form.valid()) {
                form.showValidateMessage();
                return;
            }

            var date = moment($("#DeliveryDate").val(), 'MM/DD/YYYY');
            var tomorrow = moment().startOf('day').add(1, 'd');
            if (date >= tomorrow) {
                if (!await abp.message.confirm(
                    'This dispatch is for a different date. Are you sure you want to acknowledge and work it now?'
                )) {
                    return;
                }
            }

            abp.ui.setBusy(form);
            _dispatchingService.acknowledgeDispatch({ DispatchId: form.find('#DispatchId').val() })
                .done(function () {
                    window.location.reload(true);
                })
                .always(function () {
                    abp.ui.clearBusy(form);
                });
        });

    });
})();

