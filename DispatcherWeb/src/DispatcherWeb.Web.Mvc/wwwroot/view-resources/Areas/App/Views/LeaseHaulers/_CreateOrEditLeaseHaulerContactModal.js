(function($) {
    app.modals.CreateOrEditLeaseHaulerContactModal = function () {

        var _modalManager;
        var _leaseHaulerService = abp.services.app.leaseHauler;
        var _$form = null;

        this.init = function(modalManager) {
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
            _$form.find('#CellPhoneNumber').rules('add', { regex: app.regex.cellPhoneNumber });
            _$form.find('#Email').rules('add', { regex: app.regex.email });

            abp.helper.ui.initControls();

            setRequiredAttributesAccordingToPreferredFormat();

            _$form.find('#NotifyPreferredFormat').change(function () {
                setRequiredAttributesAccordingToPreferredFormat();
            });

            function setRequiredAttributesAccordingToPreferredFormat() {
                var preferredFormat = _$form.find('#NotifyPreferredFormat').val();

                var $emailAddress = _$form.find('#Email');
                if (preferredFormat == 1 /* Email */ || preferredFormat == 3 /* Both */) {
                    addRequired($emailAddress);
                } else {
                    removeRequired($emailAddress);
                }

                var $cellPhoneNumber = _$form.find('#CellPhoneNumber');
                if (preferredFormat == 2 /* SMS */ || preferredFormat == 3 /* Both */) {
                    addRequired($cellPhoneNumber);
                } else {
                    removeRequired($cellPhoneNumber);
                }
            }

            function addRequired($ctrl) {
                $ctrl.attr('required', 'required');
                $ctrl.closest('.form-group').find('label').addClass('required-label');
            }
            function removeRequired($ctrl) {
                $ctrl.removeAttr('required').removeAttr('aria-required');
                $ctrl.closest('.form-group').removeClass('has-error');
                $ctrl.closest('.form-group').find('label').removeClass('required-label');
            }

        };

        this.save = function () {
            if (!_$form.valid()) {
            	_$form.showValidateMessage();
                return;
            }

            var leaseHaulerContact = _$form.serializeFormToObject();

            if ($('#IsDispatcher').is(':checked') && parseInt(leaseHaulerContact.NotifyPreferredFormat) === 0) {
                abp.message.error('You have checked the "Receives Truck Requests". You must select the "Preferred Format" other than "Neither".', 'Some of the data is invalid');
                return;
            }

            _modalManager.setBusy(true);
            _leaseHaulerService.editLeaseHaulerContact(leaseHaulerContact).done(function (data) {
                abp.notify.info('Saved successfully.');
                _modalManager.close();
                leaseHaulerContact.Id = data;
                abp.event.trigger('app.createOrEditLeaseHaulerContactModalSaved', {
                    item: leaseHaulerContact
                });
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };
    };
})(jQuery);