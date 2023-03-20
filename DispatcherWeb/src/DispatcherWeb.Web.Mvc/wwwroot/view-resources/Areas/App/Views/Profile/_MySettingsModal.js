(function () {
    app.modals.MySettingsModal = function () {

        var _profileService = abp.services.app.profile;
        var _initialTimezone = null;
        var _initialDontShowZeroQuantityWarning = null;
        var _initialPlaySoundForNotifications = null;

        var _modalManager;
        var _$form = null;
        var _$optionsForm = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;
            var $modal = _modalManager.getModal();

            _$form = $modal.find('form[name=MySettingsModalForm]');
            _$form.validate();
            _$optionsForm = $modal.find('form[name=OptionsModalForm]');

            _initialTimezone = _$form.find("[name='Timezone']").val();
            _initialDontShowZeroQuantityWarning = _$optionsForm.find("#DontShowZeroQuantityWarning").is(":checked");
            _initialPlaySoundForNotifications = _$optionsForm.find("#PlaySoundForNotifications").is(":checked");

            var $btnEnableGoogleAuthenticator = $modal.find('#btnEnableGoogleAuthenticator');

            $btnEnableGoogleAuthenticator.click(function () {
                _profileService.updateGoogleAuthenticatorKey()
                    .done(function (result) {
                        $modal.find('.google-authenticator-enable').toggle();
                        $modal.find('img').attr('src', result.qrCodeSetupImageUrl);
                    }).always(function () {
                        _modalManager.setBusy(false);
                    });
            });

            var $SmsVerification = $modal.find('#btnSmsVerification');
            var smsVerificationModal = new app.ModalManager({
                viewUrl: abp.appPath + 'App/Profile/SmsVerificationModal',
                scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/Profile/_SmsVerificationModal.js',
                modalClass: 'SmsVerificationModal'
            });

            $SmsVerification.click(function () {
                _profileService.sendVerificationSms()
                    .done(function () {
                        smsVerificationModal.open({}, function () {
                            $('#SpanSmsVerificationVerified').show();
                            $('#SpanSmsVerificationUnverified').hide();
                            _$form.find(".tooltips").tooltip();
                        });
                    });
            });

            _$form.find(".tooltips").tooltip();
        };

        this.save = function () {
            if (!_$form.valid()) {
                return;
            }

            var profile = _$form.serializeFormToObject();
            profile.Options = _$optionsForm.serializeFormToObject();

            var hostEmailPreferenceCheckboxes = _$optionsForm.find(".HostEmailPreferenceCheckbox:checked");
            var hostEmailPreference = hostEmailPreferenceCheckboxes.map((_, x) => Number($(x).val())).toArray().reduce((a, b) => a | b, 0);
            profile.Options.HostEmailPreference = hostEmailPreference;

            _modalManager.setBusy(true);
            _profileService.updateCurrentUserProfile(profile)
                .done(function () {
                    $('#HeaderCurrentUserName').text(profile.UserName);
                    abp.notify.info(app.localize('SavedSuccessfully'));
                    _modalManager.close();

                    var newTimezone = _$form.find("[name='Timezone']").val();

                    if (abp.clock.provider.supportsMultipleTimezone && _initialTimezone !== newTimezone) {
                        abp.message.info(app.localize('TimeZoneSettingChangedRefreshPageNotification')).done(function () {
                            window.location.reload();
                        });
                    }

                    var newDontShowZeroQuantityWarning = _$optionsForm.find("#DontShowZeroQuantityWarning").is(":checked");
                    var newPlaySoundForNotifications = _$optionsForm.find("#PlaySoundForNotifications").is(":checked");
                    if (newDontShowZeroQuantityWarning !== _initialDontShowZeroQuantityWarning || newPlaySoundForNotifications !== _initialPlaySoundForNotifications) {
                        abp.message.info(app.localize('OptionsChangedRefreshPageNotification')).done(function () {
                            window.location.reload();
                        });
                    }

                }).always(function () {
                    _modalManager.setBusy(false);
                });
        };
    };
})();