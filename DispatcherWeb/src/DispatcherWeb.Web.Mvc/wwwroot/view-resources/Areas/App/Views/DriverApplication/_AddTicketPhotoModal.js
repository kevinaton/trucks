(function ($) {
    app.modals.AddTicketPhotoModal = function () {

        var _modalManager;
        var _$form = null;
        var _$saveButton = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            //_$form.validate();

            _$saveButton = _modalManager.getModal().find('.save-button');
            _$saveButton.prop('disabled', true);

            var $fileInput = _$form.find('#TicketPhoto');
            $fileInput.change(function () {
                _$saveButton.prop('disabled', !$fileInput.val());
            });

            abp.helper.ui.initControls();

            _$form.ajaxForm({
                beforeSubmit: function (formData, jqForm, options) {
                    var $fileInput = _$form.find('input[name=TicketPhoto]');
                    var files = $fileInput.get()[0].files;

                    if (!files.length) {
                        return false;
                    }

                    var file = files[0];

                    //File type check
                    var type = '|' + file.type.slice(file.type.lastIndexOf('/') + 1) + '|';
                    if ('|jpg|jpeg|png|gif|'.indexOf(type) === -1) {
                        abp.message.warn(app.localize('File_Invalid_Type_Error'));
                        return false;
                    }

                    //File size check
                    if (file.size > 8388608) //8 MB
                    {
                        abp.message.warn(app.localize('File_SizeLimit_Error'));
                        return false;
                    }

                    _modalManager.setBusy(true);
                    return true;
                },
                success: function (response) {
                    _modalManager.setBusy(false);
                    if (response.success) {
                        abp.notify.info(app.localize('SavedSuccessfully'));
                        _modalManager.close();
                        abp.event.trigger('app.ticketPhotoAddedModal', {
                            photoId: response.result.id
                        });
                    } else {
                        abp.message.error(response.error.message);
                    }
                },
                error: function (errorResponse) {
                    _modalManager.setBusy(false);
                    if (errorResponse.responseJSON && errorResponse.responseJSON.error && errorResponse.responseJSON.error.message) {
                        abp.message.error(errorResponse.responseJSON.error.message, 'Error');
                    } else {
                        abp.message.error('An error occurred during the request', 'Error');
                    }
                }
            });
        };

        this.save = function () {

            //if (!_$form.valid()) {
            //    _$form.showValidateMessage();
            //    return;
            //}

            _$form.submit();
        };
    };
})(jQuery);