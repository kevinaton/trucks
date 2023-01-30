(function ($) {
    app.modals.CreateOrEditSupplierContactModal = function () {

        var _modalManager;
        var _locationService = abp.services.app.location;
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

        };

        this.save = function () {
            if (!_$form.valid()) {
                return;
            }
            var supplierContact = _$form.serializeFormToObject();         

            _modalManager.setBusy(true);
            _locationService.editSupplierContact(supplierContact).done(function (data) {
                abp.notify.info('Saved successfully.');
                _modalManager.close();
                supplierContact.Id = data;
                abp.event.trigger('app.createOrEditSupplierContactModalSaved', {
                    item: supplierContact
                });
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };
    };
})(jQuery);