﻿(function ($) {
    app.modals.UploadSignaturePictureModal = function () {

        var _modalManager;
        var uploadedFileName = null;

        var _profileService = abp.services.app.profile;

        this.init = function (modalManager) {
            _modalManager = modalManager;
            _modal = _modalManager.getModal();

            $('#UploadSignaturePictureModalForm input[name=SignaturePicture]').change(function () {
                var filename = $(this).val().split('\\').pop();
                _modal.find("#FileNamePlaceholder").val(filename);
                $('#UploadSignaturePictureModalForm').submit();
            });

            $('#UploadSignaturePictureModalForm').ajaxForm({
                beforeSubmit: function (formData, jqForm, options) {

                    var $fileInput = $('#UploadSignaturePictureModalForm input[name=SignaturePicture]');
                    var files = $fileInput.get()[0].files;
                    console.log(files);

                    if (!files.length) {
                        return false;
                    }

                    var file = files[0];

                    //File type check
                    var type = '|' + file.type.slice(file.type.lastIndexOf('/') + 1) + '|';
                    if ('|jpg|jpeg|png|gif|'.indexOf(type) === -1) {
                        abp.message.warn(app.localize('ProfilePicture_Warn_FileType'));
                        return false;
                    }

                    //File size check
                    if (file.size > 1048576) //1MB
                    {
                        abp.message.warn(app.localize('ProfilePicture_Warn_SizeLimit', 1));//app.maxSignaturePictureBytesUserFriendlyValue
                        return false;
                    }

                    return true;
                },
                success: function (response) {
                    console.log(response);
                    if (response.success) {
                        var $signaturePictureResize = $('#SignaturePictureResize');

                        var signatureFilePath = abp.appPath + 'Temp/Downloads/' + response.result.fileName + '?v=' + new Date().valueOf();
                        uploadedFileName = response.result.fileName;

                        $signaturePictureResize.show();
                        $signaturePictureResize.attr('src', signatureFilePath);

                    } else {
                        abp.message.error(response.error.message);
                    }
                }
            });

            $('#SignaturePictureResize').hide();
        };

        this.save = function () {
            var $fileInput = $('#UploadSignaturePictureModalForm input[name=SignaturePicture]');
            var files = $fileInput.get()[0].files;
            if (!files.length) {
                abp.message.warn(app.localize('File_Empty_Error'));
            }

            if (!uploadedFileName) {
                return;
            }

            _profileService.updateSignaturePicture({
                fileName: uploadedFileName
            }).done(function () {
                _modalManager.close();
            });
        };
    };
})(jQuery);