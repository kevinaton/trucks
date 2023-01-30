'use strict';
(function () {
    $(function () {
        window.importFileUploadedCustomCallback = async function (result) {
            abp.ui.block();
            try {
                let file = result.id + '/' + result.blobName;
                let validationResult = await abp.services.app.importTruxEarnings.validateFile(file);
                if (validationResult.duplicateShiftAssignments.length) {
                    if (!await abp.message.confirm(app.localize('{0}DuplicateRecordsWereFound_WillBeSkippedIfYouContinueImport_AreYouSure', validationResult.duplicateShiftAssignments.length))) {
                        return;
                    }
                }
                await abp.services.app.importSchedule.scheduleImport({
                    blobName: file,
                    importType: $('#ImportType').val()
                });
                await abp.message.info("The file is scheduled for importing. You will receive a notification on completion.");
                abp.ui.block();
                location = abp.appPath + 'app/tickets';
            } finally {
                abp.ui.unblock();
            }
        };
    });
})();