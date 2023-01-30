//window._info = null;
//window._config = null;

//window.addEventListener('error', async function (e) {
//    let parsedEvent = JSON.parse(JSON.stringify(e, ["message", "filename", "lineno", "colno", "error"]));
//    await App.logError('App error: ', e);
//    return false;
//});

//window.addEventListener('unhandledrejection', async function (e) {
//    await App.logError('App unhandled rejection: ', e.reason && e.reason.message, e);
//});

//window.addEventListener("beforeunload", async function (e) {
//    await App.logInfo('App beforeunload event fired');
//});


App.ui = App.ui || {};
App.ui.showValidationError = showValidationError;
window.showValidationError = showValidationError;
function showValidationError(message, caption, okButtonCallback) {
    var sanitizedMessage = $("<div>").text(message).html();
    message = sanitizedMessage.split('\n').join('<br>');
    $('#validationMessage').html(message || '');
    $('#validationError .modal-title').text(caption || '');
    $('#validationError').modal('show');
    if (okButtonCallback) {
        validationErrorOkButtonCallback = async () => {
            await okButtonCallback();
            validationErrorOkButtonCallback = null;
        };
    } else {
        validationErrorOkButtonCallback = null;
    }
}
let validationErrorOkButtonCallback = null;
$('#validationErrorOkButton').click(async function () {
    if (validationErrorOkButtonCallback) {
        await validationErrorOkButtonCallback();
    }
});
$('#validationError').on('hidden.bs.modal', async function () {
    if (validationErrorOkButtonCallback) {
        await validationErrorOkButtonCallback();
    }
});

//let offlineIndicator = null;
//if (typeof window !== 'undefined' && typeof document !== 'undefined') {
//    offlineIndicator = document.getElementById('offlineIndicator');

//    navigator.onLine ? hideOfflineIndicator() : showOfflineIndicator();
//    window.addEventListener('online', hideOfflineIndicator);
//    window.addEventListener('offline', showOfflineIndicator);
//}

//async function showOfflineIndicator() {
//    //await App.logInfo('went offline');
//    offlineIndicator && (offlineIndicator.className = 'showOfflineNotification');
//}
//async function hideOfflineIndicator() {
//    offlineIndicator && (offlineIndicator.className = 'hideOfflineNotification');
//    //await App.uploadChangesQueue(); //there's the same code in serviceworker
//}

$(async function () {
    try {
        let logs = await getLogRecords();
        $("#loadingDiv").hide();
        $("#saveLogsDiv").show();
        $("#SaveLogsButton").click(function (e) {
            e.preventDefault();
            App.saveFileFromMemory("logs-" + dayjs().format(App.Consts.DateTimeFormat.replaceAll(':', '_')) + ".json", JSON.stringify(logs));
        });
    }
    catch (e) {
        await App.logError('initial load exception: ', e);
        await App.ui.showFatalError();
        throw e;
    }
});


async function getLogRecords(startDate, endDate) {
    let db = await App.getDb();

    let dateRange = null;
    if (startDate && endDate) {
        dateRange = IDBKeyRange.bound(startDate, endDate, false, true);
    } else if (startDate) {
        dateRange = IDBKeyRange.lowerBound(startDate);
    } else if (endDate) {
        dateRange = IDBKeyRange.upperBound(endDate);
    }

    let logRecords = await db.getAllFromIndex(App.DbStores.logs, 'datetime', dateRange);
    return logRecords;

    //let allRecords = await db.getAll(App.DbStores.logs);
    //return allRecords;
}


App.saveFileFromMemory = App.saveFileFromMemory || function (filename, data) {
    const blob = new Blob([data], { type: 'application/json' });
    if (window.navigator.msSaveOrOpenBlob) {
        window.navigator.msSaveBlob(blob, filename);
    }
    else {
        const elem = window.document.createElement('a');
        elem.href = window.URL.createObjectURL(blob);
        elem.download = filename;
        document.body.appendChild(elem);
        elem.click();
        document.body.removeChild(elem);
    }
};