(function ($) {
    app.modals.SendLeaseHaulerRequestModal = function () {

        var _modalManager;
        var _leaseHaulerRequestSendAppService = abp.services.app.leaseHaulerRequestSend;
        var _$form = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            _$form.find('#Date').datepickerInit();
            _$form.find('#Shift').select2Init({ allowClear: false });
            _$form.find('#LeaseHaulerIds').select2Init({
                abpServiceMethod: abp.services.app.leaseHauler.getLeaseHaulersSelectList,
                allowClear: false
            });
            _$form.find('#OfficeId').select2Init({
                abpServiceMethod: abp.services.app.office.getOfficesSelectList,
                showAll: true,
                noSearch: true,
                allowClear: false
            });
            abp.helper.ui.addAndSetDropdownValue($("#OfficeId"),
                abp.session.officeId,
                abp.session.officeName);

            abp.helper.ui.initCannedTextLists();

        };

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var formData = _$form.serializeFormWithMultipleToObject();
            if (!$.isArray(formData.LeaseHaulerIds)) {
                formData.LeaseHaulerIds = [formData.LeaseHaulerIds];
            }

            _modalManager.setBusy(true);
            _leaseHaulerRequestSendAppService.sendRequests(formData).done(function (result) {
                if (result) {
                    abp.notify.info('Sent successfully.');
                } else {
                    abp.notify.warn('Some requests were not been sent.');
                }
                _modalManager.close();
                abp.event.trigger('app.sendLeaseHaulerRequestModalSaved');
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };
    };
})(jQuery);