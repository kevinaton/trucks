(function ($) {
    app.modals.CreateOrEditCustomerContactModal = function () {

        var _modalManager;
        var _customerService = abp.services.app.customer;
        var _$form = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();
            $.validator.addMethod(
                "regex",
                function (value, element, regexp) {
                    var re = new RegExp(regexp, 'i');
                    return this.optional(element) || re.test(value);
                },
                "Please check your input."
            );
            _$form.find('#Email').rules('add', { regex: app.regex.email });
            _$form.find('#Email').change(e => {
                var email = $(e.target).val();
                var visibility = _$form.valid() && email.length > 0 ? "visible" : "hidden";
                _$form.find('#HasCustomerPortalAccessContainer').css("visibility", visibility);
            });

            abp.helper.ui.initControls();

        };

        this.save = function () {

            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var customerContact = _$form.serializeFormToObject();
            customerContact.Name = customerContact.Name.trim().split(/[\s,\t,\n]+/).join(' ');

            _modalManager.setBusy(true);
            _customerService.getCustomerContactDuplicateCount({
                customerId: customerContact.CustomerId,
                exceptId: customerContact.Id,
                Name: customerContact.Name
            }).done(async function (duplicateCount) {

                if (duplicateCount) {
                    _modalManager.setBusy(false);
                    if (!await abp.message.confirm('This customer already has a contact by this name. Are you sure you want to add this contact?')) {
                        return;
                    }
                }

                abp.ui.setBusy(_$form);
                _modalManager.setBusy(true);

                _customerService.editCustomerContact(customerContact).done(function (result) {
                    abp.notify.info('Saved successfully.');
                    customerContact.Id = result;
                    abp.event.trigger('app.createOrEditCustomerContactModalSaved', {
                        item: customerContact
                    });
                    _modalManager.setResult(customerContact);
                    _modalManager.close();
                }).always(function () {
                    abp.ui.clearBusy(_$form);
                    _modalManager.setBusy(false);
                });;
            });
        };
    };
})(jQuery);