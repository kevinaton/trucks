//import { App } from '../../js/site.js';
window._dispatches = null;
window._currentDispatch = null;
window._info = null;
window._config = null;
//do not cache it for now unless specifically asked to change this logic
let clockWasStartedSincePageLoad = false;
let clockIntervalId;
let $elapsedTime;
let useLocalDebug = false;
let treatDriverAsAdminGuid = 'd9b04af2-9518-4394-87d5-e59783b1309d';

function isAdminOrDebug() {
    return _info && (_info.isAdmin || useLocalDebug && _info.driverGuid === treatDriverAsAdminGuid) || false;
}

window.addEventListener('error', async function (e) {
    //let parsedEvent = JSON.parse(JSON.stringify(e, ["message", "filename", "lineno", "colno", "error"]));
    await App.logError('App error: ', e);
    return false;
});

window.addEventListener('unhandledrejection', async function (e) {
    await App.logError('App unhandled rejection: ', e.reason && e.reason.message, e);
});

window.addEventListener("beforeunload", async function (e) {
    await App.logInfo('App beforeunload event fired');
});

function isTicketNumberRequired() {
    return !getCreateNewTicketValue() && _info.requireToEnterTickets;
}

function getCreateNewTicketValue() {
    return $('#CreateNewTicket').val() === "True";
}

function setCreateNewTicketValue(createNewTicket) {
    $('#TicketNumber').prop('disabled', createNewTicket);
    $('#CreateNewTicket').val(createNewTicket ? "True" : "False");
    if (isTicketNumberRequired()) {
        $('#TicketNumberLabel').addClass('required-label');
    } else {
        $('#TicketNumberLabel').removeClass('required-label');
    }
}

async function checkForIncomingData() {
    if (!(await App.getSwInfo()).requestUiUpdate) {
        return;
    }
    await App.logInfo('checkForIncomingData: noticed requestUiUpdate flag, calling updateCurrentView');
    //todo only if no dispatch or current dispatch was updated. or sart time (which will only matter when no current dispatch).
    await updateCurrentView();
}

async function reloadAllFromCacheIfNeeded() {
    if (!(await App.getSwInfo()).requestUiUpdate) {
        return;
    }
    await App.logInfo('reloadAllFromCacheIfNeeded: requestUiUpdate flag was set, getting new dispatches from cache');
    if (!_dispatches) {
        await App.logWarn('reloadAllFromCacheIfNeeded: no disaptches were cached yet, exiting');
        return; //excpected to update only, _dispatches is null on initial load
    }
    try {
        //App.ui.setBusy('Refreshing the dispatches list from cache');
        let mergedDispatchIds = [];
        let addedDispatchIds = [];
        var allDispatches = await getCachedDispatches();
        if (allDispatches && allDispatches.length) {
            allDispatches.forEach(newDispatch => {
                var matchingExisting = _dispatches.filter(x => x.id === newDispatch.id);
                if (matchingExisting.length) {
                    mergedDispatchIds.push(newDispatch.id);
                    matchingExisting.forEach(x => {
                        $.extend(x, newDispatch);
                    });
                } else {
                    addedDispatchIds.push(newDispatch.id);
                    _dispatches.push(newDispatch);
                }
            });
        }
        await App.updateSwInfo(swInfo => {
            swInfo.requestUiUpdate = false;
        });
        await App.logInfo(`reloadAllFromCacheIfNeeded: processed ${allDispatches.length} dispatches, added ${(addedDispatchIds.length ? addedDispatchIds.join(', ') : 'none')}, merged ${(mergedDispatchIds.join(', '))}`);
        await App.logDebug({ _dispatches });
        if (_currentDispatch && (_currentDispatch.dispatchStatus === App.Enums.DispatchStatus.created || _currentDispatch.dispatchStatus === App.Enums.DispatchStatus.sent)) {
            _currentDispatch = null; //order could have changed and we might need to select the new first dispatch
        }
    }
    catch (e) {
        await App.logError('reloadAllFromCacheIfNeeded exception: ', e);
        //await App.ui.showFatalError();
        //throw e;
        App.ui.setBusy('Reloading the page to show the new changes');
        location.reload();
    }
    finally {
        //App.ui.clearBusy();
    }
}

async function updateCurrentDispatch() {
    //await reloadAllFromCacheIfNeeded();
    if (!_currentDispatch && _dispatches.length || _currentDispatch && App.isCompletedOrCanceledDispatch(_currentDispatch)) {
        let oldDispatch = _currentDispatch;
        _currentDispatch = getFirstUndeliveredDispatch();
        setCreateNewTicketValue(false);
        await App.logInfo(`_currentDispatch changed from ${(oldDispatch && oldDispatch.id || 'null')} to ${(_currentDispatch && _currentDispatch.id || 'null')}`);
    }
    //await updateCurrentView();
}

function getFirstUndeliveredDispatch() {
    var dispatch =
        getFirstDispatchWithStatusIn([App.Enums.DispatchStatus.loaded]) ||
        getFirstDispatchWithStatusIn([App.Enums.DispatchStatus.acknowledged]) ||
        getFirstDispatchWithStatusIn([App.Enums.DispatchStatus.created, App.Enums.DispatchStatus.sent]);
    return dispatch;
}

function getFirstDispatchWithStatusIn(statuses) {
    console.log(`entered getFirstDispatchWithStatusIn [${statuses.map(getDispatchStatusName).join(', ')}]`);
    if (!_dispatches || !_dispatches.length) {
        return null;
    }
    _dispatches.forEach(d => d.sortOrder = d.sortOrder || d.dispatchOrder || d.id); //convert old field to the new field, temp, remove later
    var minOrderDispatch = null;
    for (let i = 0; i < _dispatches.length; i++) {
        //if (!App.isCompletedOrCanceledDispatch(_dispatches[i])) {
        //    return _dispatches[i];
        //}
        if (statuses.includes(_dispatches[i].dispatchStatus)) {
            if (minOrderDispatch === null) {
                minOrderDispatch = _dispatches[i];
            } else if (_dispatches[i].sortOrder < minOrderDispatch.sortOrder) {
                minOrderDispatch = _dispatches[i];
            }
        }
    }
    return minOrderDispatch;
}

window.hideAllViews = hideAllViews;
function hideAllViews() {
    $("#clockDiv").hide();
    $("#clockInView").hide();
    $("#hasScheduledStartTime").hide();
    $("#noScheduledStartTime").hide();
    $("#acknowledgeDispatchView").hide();
    $("#completeDeliveryView").hide();
    $("#loadInfoView").hide();
    $("#noDispatchView").hide();
    $(".MarkDispatchCompleteLinkContainer").hide();
}

function hasDispatchesForDate(isoDate) {
    for (let i = 0; i < _dispatches.length; i++) {
        if (isoDate === dayjs(_dispatches[i].date, App.Consts.DateTimeFormat).format(App.Consts.DateIsoFormat)
            && !App.isCompletedOrCanceledDispatch(_dispatches[i])) {
            return true;
        }
    }
    return false;
}

function shouldUseHourlyWorkflow() {
    return currentDispatchHasHourlyFreight() && !_currentDispatch.isMultipleLoads;
}

function shouldShowModifyTicketButtonOnLoadView() {
    //return designationHasMaterial() || _currentDispatch.freightUomName !== "Hours" && _currentDispatch.freightUomName !== "Hour";
    return !currentDispatchHasHourlyFreight();
}

function shouldShowModifyTicketButtonOnDeliveryView() {
    return !currentDispatchHasHourlyFreight() || _currentDispatch.isMultipleLoads;
}

function currentDispatchHasHourlyFreight() {
    return !designationHasMaterial() && (_currentDispatch.freightUomName === "Hours" || _currentDispatch.freightUomName === "Hour");
}

async function showWarningIfDispatchWasCanceled() {
    if (!_currentDispatch
        || !_currentDispatch.numberOfAddedLoads //at least one load needs to be finished
        || _currentDispatch.isMultipleLoads
        || _currentDispatch.isExtraLoad //already notified for this dispatch
        || !_currentDispatch.numberOfLoadsToFinish) {
        return;
    }
    if (_currentDispatch.numberOfAddedLoads > _currentDispatch.numberOfLoadsToFinish
        || _currentDispatch.dispatchStatus === App.Enums.DispatchStatus.acknowledged
        && _currentDispatch.numberOfAddedLoads === _currentDispatch.numberOfLoadsToFinish) {
        await updateDispatch(_currentDispatch, async (dispatchChange) => {
            dispatchChange.isExtraLoad = true;
        });
        showValidationError('Run Until Stopped was changed while you were offline. Check with your dispatcher to see how to handle.', 'Warning');
    }
}

function getAmountToEdit(val) {
    if (val === 0 || Number(val) === 0) {
        return '';
    }
    return val;
}

window.updateCurrentView = updateCurrentView;
async function updateCurrentView() {
    await reloadAllFromCacheIfNeeded();
    await updateCurrentDispatch();
    //App.ui.setBusy('Displaying dispatch');
    hideAllViews();

    if (_currentDispatch) {
        //$("#acknowledgeDispatchView #DispatchId").val(_currentDispatch.dispatchId);
        //$("#acknowledgeDispatchView #Guid").val(_currentDispatch.guid);
        $(".deliveryDate").text(dayjs(_currentDispatch.date, App.Consts.DateTimeFormat).format(App.Consts.DateFormat));
        $(".shiftName").text(_info.useShifts ? getShiftName(_currentDispatch.shift) : "");
        $(".timeOnJob").text(_currentDispatch.timeOnJob ? dayjs(_currentDispatch.timeOnJob, App.Consts.DateTimeFormat).format(App.Consts.ShortTimeFormat) : '');
        $(".customerName").text(_currentDispatch.customerName);
        $(".isMultipleLoads").text(_currentDispatch.isMultipleLoads ? "Yes" : "No");
        let pickupAt = formatDispatchLoadAt(_currentDispatch);
        $(".pickupAt").text(pickupAt);
        $(".pickupAtContainer").toggle(!!pickupAt);
        $(".materialQuantity").text(_currentDispatch.materialQuantity);
        $(".materialUomName").text(_currentDispatch.materialUomName);
        $(".freightQuantity").text(_currentDispatch.freightQuantity);
        $(".freightUomName").text(_currentDispatch.freightUomName);
        $(".itemName").text(_currentDispatch.item);
        $(".chargeTo").text(_currentDispatch.chargeTo);
        $(".note").text(_currentDispatch.note);
        $(".jobNumber").text(_currentDispatch.jobNumber).closest('.form-group').toggle(!!_currentDispatch.jobNumber);
        let customerAddress = formatDispatchDeliverTo(_currentDispatch);
        $(".customerAddress").text(customerAddress);
        $(".customerAddressContainer").toggle(!!customerAddress);
        $("#Designation").val(_currentDispatch.designation);
        $("#TicketNumber").val(_currentDispatch.ticketNumber);
        $("#Amount").val(getAmountToEdit(_currentDispatch.amount));

        if (_info.hideTicketControls) {
            $('.modifyTicketButton').hide();
        } else {
            $('.modifyTicketButton').show();
            if (shouldShowModifyTicketButtonOnLoadView()) {
                $("#loadInfoView .modifyTicketButton").show();
            }
            else {
                $("#loadInfoView .modifyTicketButton").hide();
            }
            if (shouldShowModifyTicketButtonOnDeliveryView()) {
                $("#completeDeliveryView .modifyTicketButton").show();
            }
            else {
                $("#completeDeliveryView .modifyTicketButton").hide();
            }
        }
        let modifyTicketText = _currentDispatch.isTicketAdded ? "Edit Ticket" : "Add Ticket";
        $(".modifyTicketButton").text(modifyTicketText);
        $("#modifyTicketModal .modal-title").text(modifyTicketText);

        if (_info.requireToEnterTickets) {
            $("label[for=Amount]").addClass('required-label');
        } else {
            $("label[for=Amount]").removeClass('required-label');
        }

        if (isTicketNumberRequired()) {
            $("label[for=TicketNumber]").addClass('required-label');
        } else {
            $("label[for=TicketNumber]").removeClass('required-label');
        }

        if (_currentDispatch.isSignatureAdded || _info.hideTicketControls) {
            $("#AddSignatureButton").hide();
        } else {
            $("#AddSignatureButton").show();
        }

        if (!!_currentDispatch.hasTickets && _currentDispatch.isMultipleLoads) {
            $(".MarkDispatchCompleteLinkContainer").show();
        }

        if (shouldUseHourlyWorkflow()) {
            $("#SaveLoadButton").text("Clock in to job");
            $("#CompleteDeliveryButton").text("Complete Job");
        } else {
            $("#SaveLoadButton").text("Loaded");
            $("#CompleteDeliveryButton").text("Delivered");
        }

    }
    $('.dateShiftLabel').text(_info.useShifts ? "Date/Shift:" : "Date:");
    $("#ClockOutButton").text(_info.isClockStarted ? "Clock Out" : "Clock In").prop('disabled', !_info.isDriver);
    $("#ChangeTimeCodeButtonContainer").toggle(_info.isClockStarted && !_info.driverLeaseHaulerId);

    $('#AcknowledgeDispatchButton').prop('disabled', /*!_info.isClockStarted*/ false);
    $('#CompleteDeliveryButton').prop('disabled', !_info.isClockStarted);
    $('#SaveLoadButton').prop('disabled', !_info.isClockStarted);
    $('.modifyTicketButton').prop('disabled', !_info.isClockStarted);
    $('#AddSignatureButton').prop('disabled', !_info.isClockStarted);
    $('.TakeTicketPhotoButton').prop('disabled', !!_info.lastTakenTicketPhoto);
    if (_info.lastTakenTicketPhoto) {
        $('#TicketPhotoSelectedMessage').show();
    } else {
        $('#TicketPhotoSelectedMessage').hide();
    }

    let $signatureViewText = $('#signatureViewText');
    if (_info.textForSignatureView) {
        $signatureViewText.find('label').text(_info.textForSignatureView);
        $signatureViewText.show();
    } else {
        $signatureViewText.hide();
    }

    $('#changeAssociatedTruck, #setTenancy').closest('li').toggle(isAdminOrDebug() && _info.dispatchesLockedToTruck);
    $('#showPendingChanges').closest('li').toggle(isAdminOrDebug());
    $('#showErrorLogs').closest('li').toggle(isAdminOrDebug());

    let showClockInView = !_info.isClockStarted /*&& !clockWasStartedSincePageLoad*/ && !_info.driverLeaseHaulerId || !_info.isDriver;
    $("#loadingDiv").hide();
    if (showClockInView) {
        $("#clockInView").show();
        if (_info.isDriver) {
            $("#clockDiv").show();
        }
        try {
            let db = await App.getDb();
            let hasClockedInToday = _info.committedElapsedSeconds > 0;
            let todayDate = dayjs().format(App.Consts.DateIsoFormat);
            //let tomorrowDate = dayjs().add(1, 'day').format(App.Consts.DateIsoFormat);
            let todayStartInfo = await db.get(App.DbStores.startTimes, todayDate);
            //let tomorrowStartInfo = await db.get(App.DbStores.startTimes, tomorrowDate);
            let nextStartInfo = todayStartInfo && todayStartInfo.nextAssignmentDate ? await db.get(App.DbStores.startTimes, todayStartInfo.nextAssignmentDate) : null;

            let showStartTimeForDispatch = function (dispatch) {
                if (dispatch.timeOnJob) {
                    $("#scheduledStartTimeCaption").text(`Scheduled start time for ${_info.driverName} in truck number ${dispatch.truckCode}:`);
                    $("#scheduledStartTime").text(dayjs(dispatch.timeOnJob, App.Consts.DateTimeFormat).format(App.Consts.DateTimeGeneralFormat));
                    $("#hasScheduledStartTime").show();
                } else {
                    let date = dayjs(dispatch.date, App.Consts.DateIsoFormat).format(App.Consts.DateFormat);
                    $("#noScheduledStartTimeCaption").text(`${_info.driverName}, you have assignments for ${date} in ${dispatch.truckCode}, but no start time is specified. Show up at the default start time.`);
                    $("#noScheduledStartTime").show();
                }
            };

            let showStartTimeForInfo = function (startInfo) {
                if (startInfo.startTime) {
                    //let time = dayjs(startInfo.startTime, App.Consts.DateTimeFormat).format(App.Consts.ShortTimeFormat);
                    $("#scheduledStartTimeCaption").text(`Scheduled start time for ${_info.driverName} in truck number ${startInfo.truckCode}:`);
                    $("#scheduledStartTime").text(dayjs(startInfo.startTime, App.Consts.DateTimeFormat).format(App.Consts.DateTimeGeneralFormat));
                    $("#hasScheduledStartTime").show();
                } else {
                    let date = dayjs(startInfo.date, App.Consts.DateIsoFormat).format(App.Consts.DateFormat);
                    $("#noScheduledStartTimeCaption").text(`${_info.driverName}, you have assignments for ${date} in ${startInfo.truckCode}, but no start time is specified. Show up at the default start time.`);
                    $("#noScheduledStartTime").show();
                }
            };

            let showNoAssignmentMessage = function (checkLater) {
                $("#noScheduledStartTimeCaption").text(`${_info.driverName}, you don’t have any assignments scheduled. ${(checkLater ? 'Check back later or c' : 'C')}ontact the dispatcher if you don’t think this is correct.`);
                $("#noScheduledStartTime").show();
            };

            let showYouHaveOpenDispatchesMessage = function () {
                $("#noScheduledStartTimeCaption").text('You have open dispatches for today.');
                $("#noScheduledStartTime").show();
            };

            let showNotADriverMessage = function () {
                if (_info.isAdmin) {
                    $("#noScheduledStartTimeCaption").text('You are logged in as admin. No dispatches will be loaded.');
                } else {
                    $("#noScheduledStartTimeCaption").text('You are not logged in as a driver. No dispatches will be loaded.');
                }
                $("#noScheduledStartTime").show();
            };

            if (!_info.isDriver) {
                showNotADriverMessage();
            } else if (!hasClockedInToday) {
                if (todayStartInfo && todayStartInfo.hasDriverAssignment) {
                    showStartTimeForInfo(todayStartInfo);
                } else if (nextStartInfo && nextStartInfo.hasDriverAssignment) {
                    showStartTimeForInfo(nextStartInfo);
                } else {
                    showNoAssignmentMessage();
                }
            } else {
                if (hasDispatchesForDate(todayDate)) {
                    showYouHaveOpenDispatchesMessage();
                } else {
                    if (nextStartInfo && nextStartInfo.hasDriverAssignment) {
                        showStartTimeForInfo(nextStartInfo);
                    } else {
                        showNoAssignmentMessage(true);
                    }
                }
            }
        }
        catch (e) {
            await App.logError('updateCurrentView exception: ', e);
            await App.ui.showFatalError();
            throw e;
        }
        finally {
            App.ui.clearBusy();
        }
    }
    if (!_currentDispatch) {
        if (!showClockInView) {
            $("#noDispatchView").show();
            $("#clockDiv").show();
        }
        await App.logInfo(`updateCurrentView: no current dispatch; showClockInView: ${showClockInView}`);
    } else if (_currentDispatch.dispatchStatus === App.Enums.DispatchStatus.created || _currentDispatch.dispatchStatus === App.Enums.DispatchStatus.sent) {
        $("#acknowledgeDispatchView").show();
        $("#clockDiv").show();
        await App.logInfo(`updateCurrentView: showing acknowledgeDispatchView for ${getDispatchStatusName(_currentDispatch.dispatchStatus)} dispatch ${_currentDispatch.id}`);
    } else if (_currentDispatch.dispatchStatus === App.Enums.DispatchStatus.acknowledged) {
        $("#loadInfoView").show();
        $("#clockDiv").show();
        await App.logInfo(`updateCurrentView: showing loadInfoView for ${getDispatchStatusName(_currentDispatch.dispatchStatus)} dispatch ${_currentDispatch.id}`);
    } else if (_currentDispatch.dispatchStatus === App.Enums.DispatchStatus.loaded) {
        $("#completeDeliveryView").show();
        $("#clockDiv").show();
        await App.logInfo(`updateCurrentView: showing completeDeliveryView for ${getDispatchStatusName(_currentDispatch.dispatchStatus)} dispatch ${_currentDispatch.id}`);
    } else {
        await App.logWarn(`updateCurrentView: unexpected dispatch ${_currentDispatch.id} status: ${getDispatchStatusName(_currentDispatch.dispatchStatus)}`);
    }
    //App.ui.clearBusy();
    await showWarningIfDispatchWasCanceled();
}

function getShiftName(shift) {
    if (!_info || !_info.shiftNames) {
        return '';
    }
    return _info.shiftNames[shift] || '';
}

function formatDispatchLoadAt(d) {
    if (!d.loadAtName && !d.loadAtAddress) {
        return d.pickupAt;
    }
    let pickupAt = (d.loadAtName ? d.loadAtName + '. ' : '') + d.loadAtAddress;
    return pickupAt;
}

function formatDispatchDeliverTo(d) {
    if (!d.deliverToName && !d.deliverToAddress) {
        return d.customerAddress;
    }
    let customerAddress = (d.deliverToName ? d.deliverToName + '. ' : '') + d.deliverToAddress;
    return customerAddress;
}

function designationIsFreightOnly() {
    return _currentDispatch && App.Enums.Designations.freightOnly.includes(_currentDispatch.designation);
}
function designationIsMaterialOnly() {
    return _currentDispatch && App.Enums.Designations.materialOnly.includes(_currentDispatch.designation);
}
function designationHasMaterial() {
    return _currentDispatch && App.Enums.Designations.hasMaterial.includes(_currentDispatch.designation);
}
function designationIsFreightAndMaterial() {
    return _currentDispatch && App.Enums.Designations.freightAndMaterial.includes(_currentDispatch.designation);
}

function validateTicketFields(ticket) {
    if (_info.hideTicketControls) {
        return true;
    }

    let isTicketPhotoValid = true;
    let isTicketNumberValid = true;
    let isTicketNumberRequiredToo = false;
    let isAmountValid = true;
    let isAmountRequiredToo = false;

    if (_info.requireToEnterTickets) {
        if (ticket.CreateNewTicket === 'False' && ticket.TicketNumber === '') {
            isTicketNumberValid = false;
        }

        if (ticket.Amount === '' || ticket.Amount === null || !(Number(ticket.Amount) > 0)) {
            isAmountValid = false;
        }
    } else {
        if (ticket.CreateNewTicket === 'False' && ticket.TicketNumber === '' && Number(ticket.Amount) !== 0 && ticket.Amount !== '') {
            isTicketNumberRequiredToo = true;
        }
        if (ticket.Amount === '' || ticket.Amount === null) {
            isAmountRequiredToo = true;
        }
        if (ticket.Amount !== '' && ticket.TicketNumber !== '') {
            isTicketNumberRequiredToo = false;
        }
        if (ticket.Amount === '' && ticket.TicketNumber === '' && ticket.CreateNewTicket !== 'True') {
            isAmountValid = true;
            isAmountRequiredToo = false;
            isTicketNumberRequiredToo = false;
        }
    }

    if (_info.requireTicketPhoto && !_currentDispatch.isTicketPhotoAdded && !_info.lastTakenTicketPhoto) {
        isTicketPhotoValid = false;
    }

    if (!isTicketPhotoValid
        || !isTicketNumberValid
        || isTicketNumberRequiredToo
        || !isAmountValid
        || isAmountRequiredToo) {
        let message = 'Please check the following: \n'
            + (isTicketNumberValid ? '' : '"Ticket" - This field is required.\n')
            + (!isTicketNumberRequiredToo ? '' : 'Please fill "Ticket" as well.\n')
            + (isAmountValid ? '' : '"Actual Quantity" - This field is required.\n')
            + (!isAmountRequiredToo ? '' : 'Please fill "Actual Quantity" as well.\n')
            + (isTicketPhotoValid ? '' : 'Ticket Photo is required.\n');
        showValidationError(message, 'Some of the data is invalid');
        return false;
    }
    return true;
}

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

let offlineIndicator = null;
if (typeof window !== 'undefined' && typeof document !== 'undefined') {
    offlineIndicator = document.getElementById('offlineIndicator');

    navigator.onLine ? hideOfflineIndicator() : showOfflineIndicator();
    window.addEventListener('online', hideOfflineIndicator);
    window.addEventListener('offline', showOfflineIndicator);
}

async function showOfflineIndicator() {
    await App.logInfo('went offline');
    offlineIndicator && (offlineIndicator.className = 'showOfflineNotification');
}
async function hideOfflineIndicator() {
    offlineIndicator && (offlineIndicator.className = 'hideOfflineNotification');
    //await App.uploadChangesQueue(); //there's the same code in serviceworker
    if (pageLoaded) {
        await App.logInfo('went online');
        try {
            if (App.hasSyncManagerSupport()) {
                await App.requestSyncWithApi();
                await App.requestLargeQueueUpload();
            } else {
                //App.ui.setBusy('Synchronizing with API');
                //using requestSyncWithApi in foreground causes _dispatches to not be updated with new dispatches and them only appearing in local db
                //await App.requestSyncWithApi();
                App.ui.setBusy('Reloading settings and clock-in info');
                _info = await App.initInfo();
                App.ui.setBusy('Uploading changes queue');
                let unlockSyncMutex = null;
                try {
                    unlockSyncMutex = await App.lockSyncMutex(false, 'for online event', App.Mutexes.lightQueue);
                    if (unlockSyncMutex) {
                        await App.uploadChangesQueue(App.DbStores.lightChangesQueue);
                        await unlockSyncMutex();
                    }
                    await refreshDispatches();
                    unlockSyncMutex = await App.lockSyncMutex(false, 'for online event', App.Mutexes.largeQueue);
                    if (unlockSyncMutex) {
                        await App.uploadChangesQueue(App.DbStores.largeChangesQueue);
                        await unlockSyncMutex();
                    }
                    unlockSyncMutex = await App.lockSyncMutex(false, 'for online event', App.Mutexes.logs);
                    if (unlockSyncMutex) {
                        await App.uploadLogs();
                        await unlockSyncMutex();
                        unlockSyncMutex = null;
                    }
                } finally {
                    if (unlockSyncMutex) {
                        await unlockSyncMutex();
                    }
                }
            }
        }
        catch (e) {
            await App.logError('hideOfflineIndicator exception: ', e);
            //await App.ui.showFatalError(); //no need to throw an exception, they can keep working
            //throw e;
        }
        finally {
            if (!App.hasSyncManagerSupport()) {
                App.ui.clearBusy();
            }
        }
    }
}

async function getCachedDispatches() {
    let db = await App.getDb();
    let cachedDispatches = await db.getAll(App.DbStores.dispatches);
    await applyLocalChangesToDispatches(cachedDispatches);
    return cachedDispatches;
}

async function applyLocalChangesToDispatches(apiDispatches) {
    let db = await App.getDb();
    var dispatchChanges = await db.getAll(App.DbStores.dispatchChanges);
    dispatchChanges.forEach(dispatchChange => {
        var matchingDispatches = apiDispatches.filter(x => x.id === dispatchChange.id);
        matchingDispatches.forEach(matchingDispatch => {
            //todo maybe check some conditions too, e.g. cancelled dispatch that was already modified by driver? (in that case, honor the original (from-api) value and ignore the new (local) status from the dispatchChange?), don't throw errors
            dispatchChange.dispatchStatus = App.getHigherPriorityDispatchStatus(matchingDispatch.dispatchStatus, dispatchChange.dispatchStatus);
            if (matchingDispatch.numberOfAddedLoads > dispatchChange.numberOfAddedLoads) {
                dispatchChange.numberOfAddedLoads = matchingDispatch.numberOfAddedLoads;
            }
            $.extend(matchingDispatch, dispatchChange);
        });
    });
}

async function refreshDispatchesIfNoBackgroundSync() {
    if (App.hasSyncManagerSupport() && App.hasNotificationSupport()) {
        return;
    }
    await refreshDispatches();
}

async function refreshDispatches(throwOnError = false) {
    try {
        await App.logInfo('refreshDispatches: entered');
        //init already cached list of dispatches
        if (!_dispatches) {
            await App.logInfo('refreshDispatches: no cached dispatches, loading dispatches from the cache');
            App.ui.setBusy('Getting cached dispatches');
            _dispatches = await getCachedDispatches();
            await App.logInfo(`refreshDispatches: loaded ${_dispatches && _dispatches.length} dispatches from the cache`);
        }
        //get new and modified dispatches
        App.ui.setBusy('Getting new dispatches from API');
        var newDispatches = await App.tryGetAndStoreNewDispatches(throwOnError);
        await App.logInfo(`refreshDispatches: received ${newDispatches && newDispatches.length} new/changed dispatches`);
        if (newDispatches && newDispatches.length) {
            await applyLocalChangesToDispatches(newDispatches);
            let mergedCount = 0, addedCount = 0;
            newDispatches.forEach(newDispatch => {
                var matchingExisting = _dispatches.filter(x => x.id === newDispatch.id);
                if (matchingExisting.length) {
                    mergedCount++;
                    matchingExisting.forEach(existing => {
                        $.extend(existing, newDispatch);
                    });
                } else {
                    addedCount++;
                    _dispatches.push(newDispatch);
                }
            });
            await App.logInfo(`refreshDispatches: merged ${mergedCount}, added ${addedCount} dispatches`);
        }
        await App.logDebug({ _dispatches });
        App.ui.setBusy();
        await App.updateSwInfo(swInfo => {
            swInfo.requestUiUpdate = false;
        });
        await updateCurrentView();
    }
    catch (e) {
        await App.logError('refreshDispatches exception: ', e);
        await App.ui.showFatalError();
        throw e;
    }
    finally {
        App.ui.clearBusy();
    }
}

async function clockIn(clockInModel) {
    clockInModel = clockInModel || {};
    let timeClassificationId = clockInModel.timeClassificationId;
    try {
        await App.logInfo(`clockIn: entered, timeClassificationId: ${timeClassificationId}`);
        App.ui.setBusy();
        _info.isClockStarted = true;
        _info.timeClassificationId = timeClassificationId;
        _info.employeeTimeDescription = clockInModel.description;
        
        clockWasStartedSincePageLoad = true;
        _info.lastClockStartDateTime = dayjs().format(App.Consts.DateTimeFormat);
        startClock();
        let clockInData = {
            timeClassificationId: timeClassificationId,
            description: clockInModel.description
        };
        App.ui.setBusy('Getting current position');
        let locationResult = await App.getLocation();
        if (locationResult) {
            clockInData.latitude = locationResult.coords.latitude;
            clockInData.longitude = locationResult.coords.longitude;
        }
        App.ui.setBusy('Saving the changes');
        await App.deleteOldLogs();
        await App.enqueueChange({
            action: App.Enums.DriverApplicationAction.clockIn,
            clockInData: clockInData
        }, App.DbStores.lightChangesQueue);
        
        let db = await App.getDb();
        await db.put(App.DbStores.info, _info);

        await updateCurrentView();
        await App.logInfo(`clockIn: completed successfully`);
    }
    catch (e) {
        await App.logError('clockIn exception: ', e);
        await App.ui.showFatalError();
        throw e;
    }
    finally {
        App.ui.clearBusy();
    }
    if (await App.reloadIfPendingReloadRequest()) {
        return;
    }
}

async function clockOut() {
    try {
        await App.logInfo(`clockOut: entered`);
        App.ui.setBusy();
        _info.isClockStarted = false;
        
        stopClock();
        var lastStartMoment = dayjs(_info.lastClockStartDateTime, App.Consts.DateTimeFormat);
        var nowMoment = dayjs();
        var uncommittedElapsedSeconds = nowMoment.diff(lastStartMoment, 'second'); //moment.duration(nowMoment.diff(lastStartMoment)).asSeconds();
        App.ui.setBusy('Saving the changes');
        await App.enqueueChange({
            action: App.Enums.DriverApplicationAction.clockOut
        }, App.DbStores.lightChangesQueue);
        _info.committedElapsedSeconds += uncommittedElapsedSeconds;
        
        let db = await App.getDb();
        await db.put(App.DbStores.info, _info);

        await updateCurrentView();
        await App.logInfo(`clockOut: completed successfully`);
    }
    catch (e) {
        await App.logError('clockOut exception: ', e);
        await App.ui.showFatalError();
        throw e;
    }
    finally {
        App.ui.clearBusy();
    }
    if (await App.reloadIfPendingReloadRequest()) {
        return;
    }
}

async function getTimeToDisplay() {
    var todayMoment = dayjs().startOf('day');
    if (todayMoment.format(App.Consts.DateTimeFormat) !== dayjs(_info.committedElapsedSecondsForDay, App.Consts.DateTimeFormat).format(App.Consts.DateTimeFormat)) {
        //todo test us resetting the timer
        await App.logDebug('today is different from _info.committedElapsedSecondsForDay');
        _info.committedElapsedSeconds = 0;
        _info.committedElapsedSecondsForDay = todayMoment.format(App.Consts.DateTimeFormat);
        _info.isClockStarted = false;
        stopClock(); //do not clock-out, just stop the ticking of the clock without saving the uncommitted seconds (will be saved on back-end automatically)
        let db = await App.getDb();
        await db.put(App.DbStores.info, _info);
    }

    if (_info.isClockStarted) {
        var lastStartMoment = dayjs(_info.lastClockStartDateTime, App.Consts.DateTimeFormat);
        var nowMoment = dayjs();
        var uncommittedElapsedSeconds = nowMoment.diff(lastStartMoment, 'second'); //moment.duration(nowMoment.diff(lastStartMoment)).asSeconds();
        var timeToDisplay = dayjs(todayMoment).add(_info.committedElapsedSeconds + uncommittedElapsedSeconds, 'seconds').format(App.Consts.TimeFormat);
        return timeToDisplay;
    } else {
        return dayjs(todayMoment).add(_info.committedElapsedSeconds, 'seconds').format(App.Consts.TimeFormat);
    }
}

function startClock() {
    clockIntervalId = setInterval(async function () {
        $elapsedTime.text(await getTimeToDisplay());
    }, 1000);
}

function stopClock() {
    clearInterval(clockIntervalId);
}

function getWasWelcomeShown() {
    return localStorage.getItem('DtdWelcomeWasShown') === 'true';
}

function setWasWelcomeShown(val) {
    localStorage.setItem('DtdWelcomeWasShown', val.toString());
}

function rand() {
    return Math.floor(Math.random() * 1000);
}

function pushRandState() {
    //history.pushState(null, null, location.origin + '?' + rand());
    history.pushState(null, null, location.origin);
}

function getHoursPassedSinceAcknowledgement() {
    var acknowledgeMoment = dayjs(_currentDispatch.acknowledged, App.Consts.DateTimeFormat);
    var nowMoment = dayjs();
    var hours = nowMoment.diff(acknowledgeMoment, 'hour', true); //moment.duration(nowMoment.diff(acknowledgeMoment)).asHours();
    return App.round(hours);
}

function getHoursPassedSinceLoaded() {
    var loadMoment = dayjs(_currentDispatch.loaded, App.Consts.DateTimeFormat);
    var nowMoment = dayjs();
    var hours = nowMoment.diff(loadMoment, 'hour', true); //moment.duration(nowMoment.diff(acknowledgeMoment)).asHours();
    return App.round(hours);
}

var pageLoaded = false;
$(async function () {
    pushRandState();
    window.onpopstate = function () {
        pushRandState();
    };

    try {
        App.ui.setBusy();
        let appVersion = $('.app-version span').text();
        await App.logInfo(`page load started, app version: ${appVersion}`);
        await App.tryLogBatteryInfo();
        let db = await App.getDb();

        await App.logDebug('startup: init config');
        _config = await App.initConfig({
            identityServerUri: $("#IdentityServerUri").val(),
            apiUri: $("#ApiUri").val(),
            driverAppUri: $("#DriverAppUri").val(),
            webPushServerPublicKey: $("#WebPushServerPublicKey").val()
        });

        if ('serviceWorker' in navigator) {
            navigator.serviceWorker.register('/serviceworker.js')
                .then((reg) => {
                    console.log('Service worker registered.', reg);
                });
        }

        await App.logDebug('checking if user is authenticated');
        var isAuthorized = await App.isAuthorized();
        if (!isAuthorized) {
            if (getWasWelcomeShown()) {
                await App.ensureAuthorized();
            } else {
                $('#welcomeModal').modal({
                    backdrop: 'static',
                    keyboard: false
                });
                return;
            }
        }

        await App.logDebug('startup: request notification permission if needed');
        await requestNotificationPermissionIfNeeded();
        await App.logDebug('startup: init info');
        _info = await App.initInfo();
        if (!_info) {
            return; //location.reload is already initiated by App.initInfo()
        }
        clockWasStartedSincePageLoad = _info.isClockStarted;
        App.ui.clearBusy();

        if (await App.reloadIfPendingReloadRequest()) {
            return;
        }

        await App.logDebug('startup: refresh dispatches');
        await refreshDispatches();

        $elapsedTime = $('#ElapsedTime');
        $elapsedTime.text(await getTimeToDisplay());
        if (_info.isClockStarted) {
            startClock();
        }

        //only needed for browsers with background sync support, otherwise we will upload and receive the changes in the foreground
        if (App.hasSyncManagerSupport() || App.hasNotificationSupport()) {
            let refreshDataIntervalId = setInterval(async function () {
                //if (await App.reloadIfPendingReloadRequest()) {
                //    return;
                //}
                await checkForIncomingData();
                //todo prevent data loss while entering something on a page!
            }, 3000);

            //let callBackgroundSyncIntervalId = setInterval(async function () {
            //    await App.logInfo('Calling background sync from the page');
            //    let swRegistration = await navigator.serviceWorker.ready;
            //    let db = await App.getDb();
            //    await App.tryLogBatteryInfo();
            //    await App.logInfo('sync tags before sync.register: ', await swRegistration.sync.getTags());
            //    if ((await App.getSwInfo()).pendingIncomingChanges) {
            //        await App.logInfo('pendingIncomingChanges was set, requesting syncDataWithApi');
            //        await App.registerSyncEvent(swRegistration, 'syncDataWithApi');
            //    } else {
            //        if (await db.count(App.DbStores.lightChangesQueue)) {
            //            await App.registerSyncEvent(swRegistration, 'uploadLightChangesQueue');
            //        }
            //    }
            //    if (await db.count(App.DbStores.largeChangesQueue)) {
            //        await App.registerSyncEvent(swRegistration, 'uploadLargeChangesQueue');
            //    }
            //    await App.registerSyncEvent(swRegistration, 'uploadLogs');
            //    await App.logInfo('sync tags after sync.register: ', await swRegistration.sync.getTags());
            //}, 15 * 60 * 1000);
        }
        let todayStartInfo = await db.get(App.DbStores.startTimes, dayjs().format(App.Consts.DateIsoFormat));
        await App.logInfo(`page load finished, app version: ${appVersion}, truck: ${(todayStartInfo && todayStartInfo.truckCode || '-')}, driver: ${_info.driverName}`);
        await App.logInfo(`useragent: ${navigator.userAgent}`);
        pageLoaded = true;
    }
    catch (e) {
        await App.logError('initial load exception: ', e);
        await App.ui.showFatalError();
        throw e;
    }
    finally {
        App.ui.clearBusy();
    }
    //############

});

$('#ClockOutButton').click(async function () {
    pushRandState();
    if (_info.isClockStarted) {
        await clockOut();
    } else {
        if (_info.driverLeaseHaulerId) {
            await clockIn({ timeClassificationId: null });
        } else if (_currentDispatch && _currentDispatch.productionPay && _currentDispatch.dispatchStatus === App.Enums.DispatchStatus.loaded) {
            await clockIn({ timeClassificationId: _info.productionPayId });
        } else {
            let clockInModel = await promptForClockInModel();
            if (!clockInModel) {
                return;
            }
            await clockIn(clockInModel);
        }
    }
});

$('#ChangeTimeCodeButton').click(async function () {
    pushRandState();
    await App.logInfo(`ChangeTimeCodeButton: clicked`);
    if (!_info.isClockStarted) {
        return;
    }
    let clockInModel = await promptForClockInModel();
    if (!clockInModel) {
        return;
    }
    await clockOut();
    await clockIn(clockInModel);
    await App.logInfo(`ChangeTimeCodeButton: completed`);
});

$('#AcknowledgeDispatchButton').click(async function () {
    try {
        pushRandState();
        let currentDispatch = _currentDispatch;
        await App.logInfo(`AcknowledgeDispatchButton clicked for dispatch ${currentDispatch && currentDispatch.id}`);
        var date = dayjs(currentDispatch.date, App.Consts.DateTimeFormat);
        var tomorrow = dayjs().startOf('day').add(1, 'd');
        if (date >= tomorrow) {
            if (!await App.ui.confirm('This dispatch is for a different date. Are you sure you want to acknowledge and work it now?')) {
                await App.logInfo(`AcknowledgeDispatchButton user rejected acknowledgement for different date`);
                return;
            }
        }

        if (_info.isClockStarted && !_info.driverLeaseHaulerId) {
            if (currentDispatch.productionPay && _info.timeClassificationId !== _info.productionPayId) {
                let prompt = 'You are about to start a production based job. Do you want to change the time to be based on production pay now?';
                if (await App.ui.confirm(prompt, 'Yes', 'No')) {
                    await clockOut();
                    await clockIn({ timeClassificationId: _info.productionPayId });
                }
            } else if (!currentDispatch.productionPay && _info.timeClassificationId === _info.productionPayId) {
                let prompt = 'You are about to start an hourly pay rate based job. Do you want to change the time to be based on hourly pay rate now?';
                if (await App.ui.confirm(prompt, 'Yes', 'No')) {
                    let clockInModel = await promptForClockInModel();
                    if (!clockInModel) {
                        return;
                    }
                    await clockOut();
                    await clockIn(clockInModel);
                }
            }
        }

        App.ui.setBusy('Saving the changes');
        await App.enqueueChange({
            action: App.Enums.DriverApplicationAction.acknowledgeDispatch,
            acknowledgeDispatchData: {
                DispatchId: currentDispatch.dispatchId
            }
        }, App.DbStores.lightChangesQueue);

        await App.updateDispatch(currentDispatch, async (dispatchChange) => {
            dispatchChange.dispatchStatus = App.Enums.DispatchStatus.acknowledged;
            dispatchChange.acknowledged = dayjs().format(App.Consts.DateTimeFormat);
        });
        await updateCurrentView();
    }
    catch (e) {
        await App.logError('acknowledge dispatch button exception: ', e);
        await App.ui.showFatalError();
        throw e;
    }
    finally {
        App.ui.clearBusy();
    }
    if (await App.reloadIfPendingReloadRequest()) {
        return;
    }
});

$(".modifyTicketButton").click(function () {
    showAddOrEditTicketModal();
});

$(".MarkDispatchCompleteLink").click(async function (e) {
    e.preventDefault();
    try {
        pushRandState();
        let currentDispatch = _currentDispatch;
        await App.logInfo(`MarkDispatchCompleteLink clicked for dispatch ${currentDispatch && currentDispatch.id}`);
        await refreshDispatchesIfNoBackgroundSync();

        let prompt = 'You are about to mark this dispatch complete. Are you sure you want to do this?';
        if (!await App.ui.confirm(prompt, 'Yes', 'No')) {
            return;
        }

        App.ui.setBusy('Saving the changes');
        await App.enqueueChange({
            action: App.Enums.DriverApplicationAction.markDispatchComplete,
            markDispatchCompleteData: {
                DispatchId: currentDispatch.dispatchId
            }
        }, App.DbStores.lightChangesQueue);

        await App.updateDispatch(currentDispatch, async (dispatchChange) => {
            dispatchChange.dispatchStatus = App.Enums.DispatchStatus.completed;
        });
        await updateCurrentView();
    }
    catch (e) {
        await App.logError('MarkDispatchCompleteLink exception: ', e);
        await App.ui.showFatalError();
        throw e;
    }
    finally {
        App.ui.clearBusy();
    }
    if (await App.reloadIfPendingReloadRequest()) {
        return;
    }
});

$('#SaveLoadButton').click(async function () {
    try {
        pushRandState();
        let currentDispatch = _currentDispatch;
        await App.logInfo(`SaveLoadButton clicked for dispatch ${currentDispatch && currentDispatch.id}`);
        await refreshDispatchesIfNoBackgroundSync();

        if (currentDispatch.productionPay && _info.timeClassificationId !== _info.productionPayId) {
            await App.logInfo(`SaveLoadButton: currently clocked in as ${_info.timeClassificationId}, changing to ${_info.productionPayId}`);
            await clockOut();
            await clockIn({ timeClassificationId: _info.productionPayId });
        }

        App.ui.setBusy();
        
        let dispatchTicket = {
            Guid: currentDispatch.guid,
            TimeClassificationId: _info.timeClassificationId
        };

        App.ui.setBusy('Getting current position');
        let locationResult = await App.getLocation();
        if (locationResult) {
            dispatchTicket.sourceLatitude = locationResult.coords.latitude;
            dispatchTicket.sourceLongitude = locationResult.coords.longitude;
        }

        App.ui.setBusy('Saving the changes');
        await App.enqueueChange({
            action: App.Enums.DriverApplicationAction.loadDispatch,
            loadDispatchData: dispatchTicket
        }, App.DbStores.lightChangesQueue);
        
        await App.updateDispatch(currentDispatch, async (dispatchChange) => {
            dispatchChange.dispatchStatus = App.Enums.DispatchStatus.loaded;
            dispatchChange.loaded = dayjs().format(App.Consts.DateTimeFormat);
            dispatchChange.numberOfAddedLoads = currentDispatch.numberOfAddedLoads ? currentDispatch.numberOfAddedLoads + 1 : 1;
        });
        await updateCurrentView();
    }
    catch (e) {
        await App.logError('save load button exception: ', e);
        await App.ui.showFatalError();
        throw e;
    }
    finally {
        App.ui.clearBusy();
    }
    if (await App.reloadIfPendingReloadRequest()) {
        return;
    }
});

$('#CreateNewTicketButton').click(function () {
    pushRandState();
    var createNewTicketOldValue = getCreateNewTicketValue();
    setCreateNewTicketValue(!createNewTicketOldValue);
});

function validateSignature(currentDispatch) {
    let isSignatureValid = true;
    if (_info.requireSignature && !currentDispatch.isSignatureAdded) {
        isSignatureValid = false;
    }

    if (!isSignatureValid) {
        let message = 'Please check the following: \n'
            + 'Signature is required.\n';
        showValidationError(message, 'Some of the data is invalid');
    }

    return isSignatureValid;
}

function validateTicket(currentDispatch) {
    if (_info.requireToEnterTickets && !currentDispatch.isTicketAdded) {
        showValidationError('Ticket is required!', 'Validation error');
        return false;
    }
    return true;
}

function validateTicketPhoto(currentDispatch) {
    if (_info.requireTicketPhoto && !currentDispatch.isTicketPhotoAdded && !_info.lastTakenTicketPhoto) {
        showValidationError('Ticket Photo is required!', 'Validation error');
        return false;
    }
    return true;
}

async function completeDispatch(currentDispatch) {
    var dispatchTicket = {
        //DispatchId: currentDispatch.dispatchId,
        Guid: currentDispatch.guid,
        IsMultipleLoads: currentDispatch.isMultipleLoads,
        TimeClassificationId: _info.timeClassificationId
    };

    if (currentDispatch.isMultipleLoads) {
        var continueMultiload = await App.ui.confirm("Do you want to continue delivering to this job?", "Yes", "No", "");
        if (continueMultiload === null) {
            return false;
        }
        dispatchTicket.ContinueMultiload = continueMultiload;
    }

    App.ui.setBusy('Getting current position');
    let locationResult = await App.getLocation();
    if (locationResult) {
        dispatchTicket.destinationLatitude = locationResult.coords.latitude;
        dispatchTicket.destinationLongitude = locationResult.coords.longitude;
    }

    App.ui.setBusy('Saving the changes');
    if (!currentDispatch.isTicketAdded) {
        let ticketDispatch = {
            Guid: currentDispatch.guid,
            DispatchStatus: currentDispatch.dispatchStatus,
            TimeClassificationId: _info.timeClassificationId
        };
        if (currentDispatchHasHourlyFreight() && !_currentDispatch.isMultipleLoads) {
            ticketDispatch.Amount = getHoursPassedSinceLoaded();
        }
        await App.enqueueChangeOnly({
            action: App.Enums.DriverApplicationAction.modifyDispatchTicket,
            loadDispatchData: ticketDispatch
        }, App.DbStores.lightChangesQueue);
    }
    await App.enqueueChange({
        action: App.Enums.DriverApplicationAction.completeDispatch,
        completeDispatchData: dispatchTicket
    }, App.DbStores.lightChangesQueue);

    await App.updateDispatch(currentDispatch, async (dispatchChange) => {
        if (currentDispatch.isMultipleLoads && dispatchTicket.ContinueMultiload) {
            dispatchChange.dispatchStatus = App.Enums.DispatchStatus.acknowledged;
            dispatchChange.isTicketAdded = false;
            dispatchChange.isTicketPhotoAdded = false;
            dispatchChange.ticketNumber = null;
            dispatchChange.amount = 0;
            //dispatchChange.createNewTicket = false;
            dispatchChange.isSignatureAdded = false;
        } else {
            dispatchChange.dispatchStatus = App.Enums.DispatchStatus.completed;
        }
    });

    return true;
}

$('#CompleteDeliveryButton').click(async function () {
    try {
        pushRandState();
        let currentDispatch = _currentDispatch;
        await App.logInfo(`CompleteDeliveryButton clicked for dispatch ${currentDispatch && currentDispatch.id}`);
        await refreshDispatchesIfNoBackgroundSync();

        if (shouldUseHourlyWorkflow()) {

            if (!validateSignature(currentDispatch)) {
                return;
            }

            showAddOrEditTicketModal();
            return;
        }

        App.ui.setBusy();
        if (!validateSignature(currentDispatch)) {
            return;
        }

        if (!currentDispatch.isExtraLoad) {
            if (!validateTicket(currentDispatch)) {
                return;
            }

            if (!validateTicketPhoto(currentDispatch)) {
                return;
            }
        }

        if (!await completeDispatch(currentDispatch)) {
            return;
        }
        await updateCurrentView();
    }
    catch (e) {
        await App.logError('complete delivery button exception: ', e);
        await App.ui.showFatalError();
        throw e;
    }
    finally {
        App.ui.clearBusy();
    }
    if (await App.reloadIfPendingReloadRequest()) {
        return;
    }
});

$('#AddSignatureButton').click(function () {
    signaturePad.clear();
});

$('#SaveSignatureButton').click(async function () {
    try {
        pushRandState();
        let currentDispatch = _currentDispatch;
        await App.logInfo(`SaveSignatureButton clicked for dispatch ${(currentDispatch && currentDispatch.id)}`);
        App.ui.setBusy();
        if (signaturePad.isEmpty()) {
            var message = 'Please provide a signature first.';
            $('#validationMessage').text(message);
            $('#validationError').modal('show');
            return;
        }

        var uploadDeferredData = {
            deferredId: uuidv4(),
            destination: App.Enums.DeferredBinaryObjectDestination.loadSignature,
            bytesString: signaturePad.toDataURL()
        };
        var addSignatureData = {
            guid: currentDispatch.guid,
            //signature: ,
            deferredSignatureId: uploadDeferredData.deferredId,
            signatureName: $('#addSignatureModal #SignatureName').val()
        };

        App.ui.setBusy('Saving the changes');
        $('#addSignatureModal').modal('hide');
        await App.enqueueChange({
            action: App.Enums.DriverApplicationAction.addSignature,
            addSignatureData: addSignatureData
        }, App.DbStores.lightChangesQueue);
        await App.enqueueChange({
            action: App.Enums.DriverApplicationAction.uploadDeferredBinaryObject,
            uploadDeferredData: uploadDeferredData
        }, App.DbStores.largeChangesQueue);


        await App.updateDispatch(currentDispatch, async (dispatchChange) => {
            dispatchChange.isSignatureAdded = true;
        });
        await updateCurrentView();
    }
    catch (e) {
        await App.logError('save signature button exception: ', e);
        await App.ui.showFatalError();
        throw e;
    }
    finally {
        App.ui.clearBusy();
    }
    if (await App.reloadIfPendingReloadRequest()) {
        return;
    }
});

function showAddOrEditTicketModal() {
    if (shouldUseHourlyWorkflow() /*&& !_currentDispatch.isTicketAdded*/) {
        $('#modifyTicketModal #Amount').val(getHoursPassedSinceLoaded());
    }
    $('#modifyTicketModal').modal('show');
}

$('#SaveTicketButton').click(async function () {
    try {
        pushRandState();
        let currentDispatch = _currentDispatch;
        await App.logInfo(`SaveTicketButton clicked for dispatch ${currentDispatch && currentDispatch.id}`);
        App.ui.setBusy();

        let uploadDeferredData = _info.lastTakenTicketPhoto ? {
            deferredId: uuidv4(),
            destination: App.Enums.DeferredBinaryObjectDestination.ticketPhoto,
            bytesString: _info.lastTakenTicketPhoto
        } : null;
        var dispatchTicket = {
            Guid: currentDispatch.guid,
            TimeClassificationId: _info.timeClassificationId,
            //DispatchId: currentDispatch.dispatchId,
            //LoadId //todo
            TicketNumber: $('#modifyTicketModal #TicketNumber').val(),
            Amount: $('#modifyTicketModal #Amount').val(),
            //Designation: $('#modifyTicketModal #Designation').val(),
            CreateNewTicket: $('#modifyTicketModal #CreateNewTicket').val(),
            //TicketPhotoBase64: _info.lastTakenTicketPhoto,
            TicketPhotoFilename: _info.lastTakenTicketPhotoFilename,
            DeferredPhotoId: uploadDeferredData !== null ? uploadDeferredData.deferredId : null,
            TicketControlsWereHidden: _info.hideTicketControls,
            IsEdit: currentDispatch.isTicketAdded,
            DispatchStatus: currentDispatch.dispatchStatus
        };

        if (!validateTicketFields(dispatchTicket)) {
            return;
        }

        App.ui.setBusy('Saving the changes');
        $('#modifyTicketModal').modal('hide');
        await App.enqueueChange({
            action: App.Enums.DriverApplicationAction.modifyDispatchTicket,
            loadDispatchData: dispatchTicket
        }, App.DbStores.lightChangesQueue);
        if (uploadDeferredData !== null) {
            await App.enqueueChange({
                action: App.Enums.DriverApplicationAction.uploadDeferredBinaryObject,
                uploadDeferredData: uploadDeferredData
            }, App.DbStores.largeChangesQueue);
        }

        let hadPhoto = !!_info.lastTakenTicketPhoto;
        _info.lastTakenTicketPhoto = null;
        _info.lastTakenTicketPhotoFilename = null;
        let db = await App.getDb();
        await db.put(App.DbStores.info, _info);            

        await App.updateDispatch(currentDispatch, async (dispatchChange) => {
            dispatchChange.isTicketAdded = true; //for the current load
            dispatchChange.isTicketPhotoAdded = dispatchChange.isTicketPhotoAdded || hadPhoto;
            dispatchChange.hasTickets = true; //across all loads
            dispatchChange.ticketNumber = dispatchTicket.TicketNumber;
            dispatchChange.amount = dispatchTicket.Amount;
            dispatchChange.createNewTicket = dispatchTicket.CreateNewTicket;
        });

        if (shouldUseHourlyWorkflow()) {
            if (!await completeDispatch(currentDispatch)) {
                await updateCurrentView();
                return;
            }
        }

        await updateCurrentView();
    }
    catch (e) {
        await App.logError('save ticket button exception: ', e);
        await App.ui.showFatalError();
        throw e;
    }
    finally {
        App.ui.clearBusy();
    }
    if (await App.reloadIfPendingReloadRequest()) {
        return;
    }
});

$('#modifyTicketModal').on('hidden.bs.modal', async function () {
    $("#TicketNumber").val(_currentDispatch.ticketNumber);
    $("#Amount").val(getAmountToEdit(_currentDispatch.amount));
});

var canvas = $('#addSignatureModal canvas')[0];
var signaturePad = new SignaturePad(canvas);

function resizeCanvas() {
    var ratio = Math.max(window.devicePixelRatio || 1, 1);
    canvas.width = canvas.offsetWidth * ratio;
    canvas.height = canvas.offsetHeight * ratio;
    canvas.getContext("2d").scale(ratio, ratio);

    signaturePad.clear();
}
//window.onresize = resizeCanvas;

$('#addSignatureModal').on('shown.bs.modal', function () {
    resizeCanvas();
});

var saveTicketPhotoButton = $('#saveTicketPhotoButton');
var ticketPhotoFileInput = $('#TicketPhoto');
ticketPhotoFileInput.change(function () {
    saveTicketPhotoButton.prop('disabled', !ticketPhotoFileInput.val());
});
$('.TakeTicketPhotoButton').click(function () {
    pushRandState();
    ticketPhotoFileInput.val('');
    saveTicketPhotoButton.prop('disabled', true);
    $('#addTicketPhotoModal').modal('show');
});
$('#RemoveTicketPhoto').click(async function (e) {
    e.preventDefault();
    if (await App.ui.confirm("Are you sure you want to delete ticket photo?")) {
        _info.lastTakenTicketPhoto = null;
        _info.lastTakenTicketPhotoFilename = null;
        let db = await App.getDb();
        await db.put(App.DbStores.info, _info);
        $('.TakeTicketPhotoButton').prop('disabled', false);
        $('#TicketPhotoSelectedMessage').hide();
    }
});
function validateTicketPhotoFile() {
    let files = ticketPhotoFileInput.get()[0].files;

    if (!files.length) {
        return false;
    }

    let file = files[0];

    //File type check
    let type = '|' + file.type.slice(file.type.lastIndexOf('/') + 1) + '|';
    if ('|jpg|jpeg|png|gif|'.indexOf(type) === -1) {
        showValidationError('Invalid file type!', 'Validation error');
        return false;
    }
    //File size check
    if (file.size > 8388608) //8 MB
    {
        showValidationError('Size of the file exceeds allowed limits!', 'Validation error');
        return false;
    }
    return true;
}
saveTicketPhotoButton.click(function () {
    if (!validateTicketPhotoFile()) {
        return;
    }
    const file = ticketPhotoFileInput[0].files[0];
    const reader = new FileReader();

    reader.addEventListener("load", async function () {
        _info.lastTakenTicketPhoto = reader.result;
        _info.lastTakenTicketPhotoFilename = file.name;
        let db = await App.getDb();
        await db.put(App.DbStores.info, _info);
        $('.TakeTicketPhotoButton').prop('disabled', true);
        $('#addTicketPhotoModal').modal('hide');
        $('#TicketPhotoSelectedMessage').show();
    }, false);

    reader.readAsDataURL(file);
});

$('#logoutButton').click(async function (e) {
    e.preventDefault();
    try {
        await App.logInfo(`LogoutButton clicked`);
        await App.enqueueSubscriptionRemoval();
        App.ui.clearBusy();
        if (!await forceUploadChanges()) {
            return;
        }

        App.ui.setBusy('Clearing cache');
        await App.clearCache();
        App.ui.setBusy('Redirecting');
        App.getUserManager().signoutRedirect();
    }
    catch (e) {
        await App.logError('logout button exception: ', e);
        await App.ui.showFatalError();
        throw e;
    }
    finally {
        //App.ui.clearBusy();
    }
});

//called on logout
async function forceUploadChanges() {
    let unlockSyncMutex = null;
    try {
        await App.logInfo('forceUploadChanges: started');
        if (!navigator.onLine) {
            await App.logInfo('forceUploadChanges: navigator.onLine is false, exiting');
            showValidationError('Please make sure you have an active internet connection', 'Sync unavailable');
            return false;
        }

        let forceUploadSuccess = true;

        var tryToLockMutexAndRunCallbackAsync = async function (mutexName, asyncCallback) {
            try {
                unlockSyncMutex = await App.lockSyncMutex(false, 'for forceUploadChanges/logout', mutexName);
                if (unlockSyncMutex) {
                    await asyncCallback();
                } else {
                    forceUploadSuccess = false;
                }
            } catch (e) {
                await App.logError('forceUploadChanges exception: ', e);
                forceUploadSuccess = false;
            } finally {
                if (unlockSyncMutex) {
                    await unlockSyncMutex();
                    unlockSyncMutex = null;
                }
            }
        };

        await tryToLockMutexAndRunCallbackAsync(App.Mutexes.lightQueue, async function () {
            await App.logInfo('forceUploadChanges: uploading light change queue');
            App.ui.setBusy('Uploading the local changes queue');
            await App.uploadChangesQueue(App.DbStores.lightChangesQueue);
        });

        await tryToLockMutexAndRunCallbackAsync(App.Mutexes.largeQueue, async function () {
            await App.logInfo('forceUploadChanges: uploading large change queue');
            App.ui.setBusy('Uploading the local changes queue');
            await App.uploadChangesQueue(App.DbStores.largeChangesQueue);
        });

        await tryToLockMutexAndRunCallbackAsync(App.Mutexes.logs, async function () {
            await App.logInfo('forceUploadChanges: uploading the logs');
            App.ui.setBusy('Uploading the changes');
            await uploadLogs();
        });

        await App.logInfo('forceUploadChanges: completed. success: ' + forceUploadSuccess);
        if (!forceUploadSuccess) {
            await App.logWarn('forceUploadChanges: failed to lock mutex, sync already in progress');
            showValidationError('Background sync is already in progress, please try again in a few minutes', 'Sync unavailable');
            return false;
        }
        return true;
    } catch (e) {
        await App.logError('forceUploadChanges exception: ', e);
        showValidationError('Please make sure you have a good internet connection or try again later', 'Sync failed');
        return false;
    }
    finally {
        if (unlockSyncMutex) {
            await unlockSyncMutex();
        }
        App.ui.clearBusy();
    }
}

$('#forceRefreshButton').click(async function (e) {
    e.preventDefault();
    pushRandState();
    $('.navbar-toggle').click();
    let unlockSyncMutex = null;
    try {
        await App.logInfo('forceRefresh: Button clicked');
        if (!navigator.onLine) {
            await App.logInfo('forceRefresh: navigator.onLine is false, exiting');
            showValidationError('Please make sure you have an active internet connection', 'Sync unavailable');
            return;
        }

        let uploadSuccess = true;

        var tryToLockMutexAndRunCallbackAsync = async function (mutexName, asyncCallback) {
            try {
                unlockSyncMutex = await App.lockSyncMutex(false, 'for forceRefresh', mutexName);
                if (unlockSyncMutex) {
                    await asyncCallback();
                } else {
                    uploadSuccess = false;
                }
            } catch (e) {
                await App.logError('forceRefresh exception: ', e);
                uploadSuccess = false;
            } finally {
                if (unlockSyncMutex) {
                    await unlockSyncMutex();
                    unlockSyncMutex = null;
                }
            }
        };

        App.ui.setBusy('Reloading settings and clock-in info');
        _info = await App.initInfo();
        await App.logInfo('forceRefresh: uploading change queues');

        await tryToLockMutexAndRunCallbackAsync(App.Mutexes.lightQueue, async function () {
            await App.logInfo('forceRefresh: uploading light change queue');
            App.ui.setBusy('Uploading the local changes queue');
            await App.uploadChangesQueue(App.DbStores.lightChangesQueue);
        });

        await tryToLockMutexAndRunCallbackAsync(App.Mutexes.largeQueue, async function () {
            await App.logInfo('forceRefresh: uploading large change queue');
            App.ui.setBusy('Uploading the local changes queue');
            await App.uploadChangesQueue(App.DbStores.largeChangesQueue);
        });

        App.ui.clearBusy();
        try {
            await App.logInfo('forceRefresh: downloading new dispatches');
            await refreshDispatches(true);
        } catch (e) {
            await App.logError('forceRefresh exception: ', e);
            uploadSuccess = false;
        }

        await tryToLockMutexAndRunCallbackAsync(App.Mutexes.logs, async function () {
            await App.logInfo('forceRefresh: uploading the logs');
            App.ui.setBusy('Uploading the changes');
            await uploadLogs();
        });

        await App.logDebug('forceRefresh: updating SW');
        await App.updateServiceWorker(true);

        if (!uploadSuccess) {
            await App.logWarn('forceRefresh: failed to lock mutex, sync already in progress');
            showValidationError('Background sync is already in progress, please try again in a few minutes', 'Sync unavailable', async function () {
                setTimeout(() => location.reload(), 100);
            });
            return;
        }
        await App.logInfo('forceRefresh: completed successfully, reloading the page');
        App.ui.setBusy('Completed successfully, reloading the page');
        //with a timeout so that 'finally' block executes and unlocks our mutex
        setTimeout(() => location.reload(), 3000);
    } catch (e) {
        await App.logError('forceRefresh exception: ', e);
        App.ui.clearBusy();
        showValidationError('Please make sure you have a good internet connection or try again later', 'Sync failed');
    }
    finally {
        if (unlockSyncMutex) {
            await unlockSyncMutex();
        }
    }
});

$('#changeAssociatedTruck').click(function (e) {
    e.preventDefault();
    pushRandState();
    if (!isAdminOrDebug() || !_info.dispatchesLockedToTruck) {
        return;
    }
    //todo
});

$('#setTenancy').click(function (e) {
    e.preventDefault();
    pushRandState();
    if (!isAdminOrDebug() || !_info.dispatchesLockedToTruck) {
        return;
    }
    //todo
});

$('.customerAddressContainer').click(function (e) {
    e.preventDefault();
    //let address = $(".customerAddress").text();
    if (!_currentDispatch) {
        return;
    }
    openMapApp(_currentDispatch.deliverToAddress, _currentDispatch.deliverToLatitude, _currentDispatch.deliverToLongitude);
});

$('.pickupAtContainer').click(function (e) {
    e.preventDefault();
    //let address = $(".pickupAt").text();
    if (!_currentDispatch) {
        return;
    }
    openMapApp(_currentDispatch.loadAtAddress, _currentDispatch.loadAtLatitude, _currentDispatch.loadAtLongitude);
});

function openMapApp(address, latitude, longitude) {
    address = encodeURIComponent(address);
    if (navigator.platform.indexOf('iPhone') !== -1
        || navigator.platform.indexOf('iPad') !== -1
        || navigator.platform.indexOf('iPod') !== -1) {
        if (latitude && longitude) {
            window.open('http://maps.apple.com/?daddr=' + latitude + ',' + longitude);
        } else if (address) {
            window.open('http://maps.apple.com/?q=' + address);
        }
    } else {
        if (latitude && longitude) {
            window.open('https://maps.google.com/?q=' + latitude + ',' + longitude);
        } else if (address) {
            window.open('https://maps.google.com/maps?q=' + address);
        }
    }
}

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

async function refreshLogRecordsList() {
    let startDate = $('#LogFilterStartDate').val();
    let endDate = $('#LogFilterEndDate').val();
    let descDirection = $('#LogFilterDescDirection').prop('checked');
    let levelsToInclude = $('.log-filter-level:checked').toArray().map(x => $(x).attr('data-level'));
    if (!levelsToInclude.length) {
        levelsToInclude = ['info', 'warn', 'error'];
    }

    App.ui.setBusy('Loading the logs');
    let errorLogsList = $('#errorLogsList');
    errorLogsList.empty();
    let logRecords = await getLogRecords(startDate, endDate);
    logRecords = logRecords.filter(x => levelsToInclude.includes(x.level));
    errorLogsList.append($('<span>').html(`<b>Number of matched log records: ${logRecords.length}</b><br>`));
    errorLogsList.append('<hr>');
    let errorLogsSubList = $('<div>').appendTo(errorLogsList);
    for (let logRecord of logRecords) {
        let description = `date/time: ${logRecord.datetime}
${(logRecord.level === 'error' ? '[[[b]]]' : '')}level: ${logRecord.level}${(logRecord.level === 'error' ? '[[[/b]]]' : '')}
message: ${logRecord.message}`;
        let escapedDescription = $('<span>').text(description).html();
        escapedDescription = escapedDescription
            .split('\n').join('<br>')
            .split('[[[b]]]').join('<b>')
            .split('[[[/b]]]').join('</b>');
        if (descDirection) {
            errorLogsSubList.prepend('<hr>');
        }
        let div = $('<div>').css('word-break', 'break-all').append($('<span>').html(escapedDescription))[descDirection ? 'prependTo' : 'appendTo'](errorLogsSubList);
        if (!descDirection) {
            errorLogsSubList.append('<hr>');
        }
    }
    App.ui.clearBusy();
}

$('#refreshLogList').click(async function (e) {
    e.preventDefault();
    await refreshLogRecordsList();
});

$('#showErrorLogs').click(async function (e) {
    e.preventDefault();
    if (!isAdminOrDebug()) {
        return;
    }
    $('.navbar-toggle').click();

    $('#LogFilterStartDate').val(dayjs().format('YYYY-MM-DD')); //+ 'T00:00:00'
    $('#LogFilterEndDate').val(dayjs().add(1, 'day').format('YYYY-MM-DD'));

    await refreshLogRecordsList();
    $('#errorLogsModal').modal('show');
});

$('#clearAllLogs').click(async function (e) {
    e.preventDefault();
    if (!isAdminOrDebug()) {
        return;
    }
    if (!confirm('Are you sure you want to clear all logs?')) {
        return;
    }
    let db = await App.getDb();
    await db.clear(App.DbStores.logs);
    $('#errorLogsModal').modal('hide');
    alert('Cleared successfully');
});

$('#showPendingChanges').click(async function (e) {
    e.preventDefault();
    if (!isAdminOrDebug()) {
        return;
    }
    $('.navbar-toggle').click();
    App.ui.setBusy('Loading pending changes queue');
    let pendingChangesList = $('#pendingChangesModal .modal-body #pendingChangesList');
    pendingChangesList.empty();
    let db = await App.getDb();
    let queueNames = [App.DbStores.lightChangesQueue, App.DbStores.largeChangesQueue];
    for (let queueName of queueNames) {
        let queueItems = await db.getAll(queueName);
        pendingChangesList.append($('<span>').html(`<b>Queue: ${queueName}</b><br>`));
        pendingChangesList.append($('<span>').text(`Number of pending changes: ${queueItems.length}`));
        pendingChangesList.append('<hr>');
        for (let queueItem of queueItems) {
            let description = `id: ${queueItem.id}
action: ${getDriverApplicationActionName(queueItem.action)}
action time: ${queueItem.actionTime}`;

            switch (queueItem.action) {
                case App.Enums.DriverApplicationAction.clockIn:
                    break;
                case App.Enums.DriverApplicationAction.clockOut:
                    break;
                case App.Enums.DriverApplicationAction.acknowledgeDispatch:
                    break;
                case App.Enums.DriverApplicationAction.loadDispatch:
                case App.Enums.DriverApplicationAction.modifyDispatchTicket:
                    if (queueItem.loadDispatchData.TicketPhotoBase64) {
                        description += `\n [[[b]]]Has photo attached: ${Math.round(queueItem.loadDispatchData.TicketPhotoBase64.length / 1024)} kb[[[/b]]]`;
                    } else {
                        description += '\n Has no photo';
                    }
                    if (queueItem.loadDispatchData.DeferredPhotoId) {
                        description += `\n DeferredId: ${queueItem.loadDispatchData.DeferredPhotoId}`;
                    }
                    break;
                case App.Enums.DriverApplicationAction.completeDispatch:
                    break;
                case App.Enums.DriverApplicationAction.addSignature:
                    if (queueItem.addSignatureData.signature) {
                        description += `\n [[[b]]]Singature size: ${Math.round((queueItem.addSignatureData.signature && queueItem.addSignatureData.signature.length || 0) / 512)} kb[[[/b]]]`;
                    }
                    if (queueItem.addSignatureData.deferredSignatureId) {
                        description += `\n DeferredId: ${queueItem.addSignatureData.deferredSignatureId}`;
                    }
                    break;
                case App.Enums.DriverApplicationAction.saveDriverPushSubscription:
                    break;
                case App.Enums.DriverApplicationAction.removeDriverPushSubscription:
                    break;
                case App.Enums.DriverApplicationAction.uploadDeferredBinaryObject:
                    description += `\n DeferredId: ${queueItem.uploadDeferredData.deferredId}`;
                    description += `\n [[[b]]]File size: ${Math.round(queueItem.uploadDeferredData.bytesString.length / 1024)} kb[[[/b]]]`;
                    break;
            }
            let escapedDescription = $('<span>').text(description).html();
            escapedDescription = escapedDescription
                .split('\n').join('<br>')
                .split('[[[b]]]').join('<b>')
                .split('[[[/b]]]').join('</b>');
            let div = $('<div>').css('word-break', 'break-all').append($('<span>').html(escapedDescription)).appendTo(pendingChangesList);
            let button = $('<button type="button">').text('Show all details').click(function () {
                if (queueItem.loadDispatchData && queueItem.loadDispatchData.TicketPhotoBase64) {
                    queueItem.loadDispatchData.TicketPhotoBase64 = '---trimmed---';
                }
                if (queueItem.addSignatureData && queueItem.addSignatureData.signature) {
                    queueItem.addSignatureData.signature = '---trimmed---';
                }
                if (queueItem.uploadDeferredData && queueItem.uploadDeferredData.bytesString) {
                    queueItem.uploadDeferredData.bytesString = '---trimmed---';
                }
                div.append($('<br>')).append($('<span>').text(JSON.stringify(queueItem)));
                button.remove();
            });
            button.appendTo(pendingChangesList);
            $('<hr>').appendTo(pendingChangesList);
        }
    }

    App.ui.clearBusy();
    $('#pendingChangesModal').modal('show');
});

$("#clearAllPendingPhotos").click(async function (e) {
    e.preventDefault();
    if (!isAdminOrDebug()) {
        return;
    }
    if (!confirm('Are you sure you want to clear All Pending Photos?')) {
        return;
    }
    $('#pendingChangesModal').modal('hide');
    App.ui.setBusy('Clearing all pending photos');
    let db = await App.getDb();
    //old queue
    let queueItems = await db.getAll(App.DbStores.changesQueue);
    let tx = db.transaction(App.DbStores.changesQueue, App.DbTransactionModes.readwrite);
    let anyUpdated = false;
    for (let queueItem of queueItems) {
        if (queueItem.loadDispatchData && queueItem.loadDispatchData.TicketPhotoBase64) {
            queueItem.loadDispatchData.TicketPhotoBase64 = null;
            anyUpdated = true;
            tx.store.put(queueItem);
        }
    }
    if (anyUpdated) {
        await tx.done;
    }
    //large queue
    queueItems = await db.getAll(App.DbStores.largeChangesQueue);
    tx = db.transaction(App.DbStores.largeChangesQueue, App.DbTransactionModes.readwrite);
    anyUpdated = false;
    let clearedDeferredIds = [];
    for (let queueItem of queueItems) {
        if (queueItem.action === App.Enums.DriverApplicationAction.uploadDeferredBinaryObject
                && queueItem.uploadDeferredData.destination === App.Enums.DeferredBinaryObjectDestination.ticketPhoto) {
            anyUpdated = true;
            clearedDeferredIds.push(queueItem.uploadDeferredData.deferredId);
            tx.store.delete(queueItem.id);
        }
    }
    //light queue
    queueItems = await db.getAll(App.DbStores.lightChangesQueue);
    tx = db.transaction(App.DbStores.lightChangesQueue, App.DbTransactionModes.readwrite);
    anyUpdated = false;
    for (let queueItem of queueItems) {
        if (queueItem.loadDispatchData && queueItem.loadDispatchData.DeferredPhotoId) {
            if (clearedDeferredIds.includes(queueItem.loadDispatchData.DeferredPhotoId)) {
                queueItem.loadDispatchData.DeferredPhotoId = null;
                anyUpdated = true;
                tx.store.put(queueItem);
            }
        }
    }
    if (anyUpdated) {
        await tx.done;
    }
    alert('Cleared successfully');
    App.ui.setBusy('Reloading the page');
    window.location.reload();
});

$("#clearAllPendingSignatures").click(async function (e) {
    e.preventDefault();
    if (!isAdminOrDebug()) {
        return;
    }
    if (!confirm('Are you sure you want to clear All Pending Signatures?')) {
        return;
    }
    $('#pendingChangesModal').modal('hide');
    App.ui.setBusy('Clearing all pending photos');
    let db = await App.getDb();
    //old queue
    let queueItems = await db.getAll(App.DbStores.changesQueue);
    let tx = db.transaction(App.DbStores.changesQueue, App.DbTransactionModes.readwrite);
    let anyUpdated = false;
    for (let queueItem of queueItems) {
        if (queueItem.action === App.Enums.DriverApplicationAction.addSignature) {
            anyUpdated = true;
            tx.store.delete(queueItem.id);
        }
    }
    if (anyUpdated) {
        await tx.done;
    }
    //large queue
    queueItems = await db.getAll(App.DbStores.largeChangesQueue);
    tx = db.transaction(App.DbStores.largeChangesQueue, App.DbTransactionModes.readwrite);
    anyUpdated = false;
    //let clearedDeferredIds = [];
    for (let queueItem of queueItems) {
        if (queueItem.action === App.Enums.DriverApplicationAction.uploadDeferredBinaryObject
                && queueItem.uploadDeferredData.destination === App.Enums.DeferredBinaryObjectDestination.loadSignature) {
            anyUpdated = true;
            //clearedDeferredIds.push(queueItem.uploadDeferredData.deferredId);
            tx.store.delete(queueItem.id);
        }
    }
    if (anyUpdated) {
        await tx.done;
    }
    //light queue
    queueItems = await db.getAll(App.DbStores.lightChangesQueue);
    tx = db.transaction(App.DbStores.lightChangesQueue, App.DbTransactionModes.readwrite);
    anyUpdated = false;
    for (let queueItem of queueItems) {
        if (queueItem.action === App.Enums.DriverApplicationAction.addSignature) {
            anyUpdated = true;
            tx.store.delete(queueItem.id);
        }
    }
    if (anyUpdated) {
        await tx.done;
    }
    alert('Cleared successfully');
    App.ui.setBusy('Reloading the page');
    window.location.reload();
});

$("#clearAllPendingData").click(async function (e) {
    e.preventDefault();
    if (!isAdminOrDebug()) {
        return;
    }
    if (!confirm('Are you sure you want to clear All Pending Data and cache?')) {
        return;
    }
    if (!confirm('Please make sure you have a stable internet connection before wiping the cache, otherwise the app won\'t load until you get back to a stable connection.')) {
        return;
    }
    $('#pendingChangesModal').modal('hide');
    App.ui.setBusy('Clearing all pending data and cache');
    await clearCache();
    let db = await App.getDb();
    await db.clear(App.DbStores.dispatchChanges);
    await db.clear(App.DbStores.changesQueue);
    await db.clear(App.DbStores.lightChangesQueue);
    await db.clear(App.DbStores.largeChangesQueue);
    await db.clear(App.DbStores.startTimes);
    //await db.clear(App.DbStores.config);
    alert('Cleared successfully');
    App.ui.setBusy('Reloading the page');
    window.location.reload();
});

$('#loginButton').click(async function (e) {
    e.preventDefault();
    setWasWelcomeShown(true);
    $('#welcomeModal').modal('hide');
    await App.ensureAuthorized();
});


//Select time classification modal:
let timeClassificationPopupPromiseResolve = null;
var promptForClockInModel = async function () {
    let oldTimeClassificationPromiseResolve = timeClassificationPopupPromiseResolve;
    timeClassificationPopupPromiseResolve = null;
    if (oldTimeClassificationPromiseResolve) {
        oldTimeClassificationPromiseResolve(null);
    }
    var timeClassificationInput = $('#TimeClassificationId');
    timeClassificationInput.empty();
    if (!_info.timeClassifications || !_info.timeClassifications.length) {
        //$('<option value="">None</option>').appendTo(timeClassificationInput);
        //timeClassificationInput.val('');
        showValidationError("You don't have any time classifications or pay rates configured. Please contact your dispatcher and let them know.", "");
        return Promise.resolve(null);
    }
    _info.timeClassifications.forEach(c => {
        $('<option>').attr('value', c.id).text(c.name).appendTo(timeClassificationInput);
        if (c.isDefault) {
            timeClassificationInput.val(c.id);
        }
    });
    if (!_info.timeClassifications.filter(c => c.isDefault).length) {
        timeClassificationInput.val(_info.timeClassifications[0].id);
    }
    $("#EmployeeTimeDescription").val('');
    return new Promise((resolve) => {
        timeClassificationPopupPromiseResolve = resolve;
        $('#selectTimeClassificationModal').modal('show');
    });
};

$('#ClockInWithTimeClassificationButton').click(async function () {
    var timeClassificationId = $('#TimeClassificationId').val();
    timeClassificationId = timeClassificationId ? Number(timeClassificationId) : null;
    if (_currentDispatch && _currentDispatch.productionPay && timeClassificationId !== _info.productionPayId) {
        let prompt = 'The pay for your next dispatch is based on production, but you chose a time classification that is an hourly rate. Are you sure you want to do this?';
        if (!await App.ui.confirm(prompt)) {
            return; //do not resolve the promise, keep the modal open, let them pick another value
        }
    } else if (_currentDispatch && !_currentDispatch.productionPay && timeClassificationId === _info.productionPayId) {
        let prompt = 'Your next dispatch is based on an hourly pay rate and you have chosen production pay. Are you sure you want to do this?';
        if (!await App.ui.confirm(prompt)) {
            return; //do not resolve the promise, keep the modal open, let them pick another value
        }
    }
    var employeeTimeDescription = $("#EmployeeTimeDescription").val();
    if (employeeTimeDescription && employeeTimeDescription.length > 200) {
        showValidationError('The description cannot be longer than 200 characters', 'Validation error');
        return;
    }
    if (timeClassificationId === null) {
        resolveTimeClassificationPopupPromiseWith(null);
    } else {
        resolveTimeClassificationPopupPromiseWith({
            timeClassificationId: timeClassificationId,
            description: employeeTimeDescription
        });
    }
    $('#selectTimeClassificationModal').modal('hide');
});

function resolveTimeClassificationPopupPromiseWith(value) {
    if (timeClassificationPopupPromiseResolve) {
        let resolve = timeClassificationPopupPromiseResolve;
        timeClassificationPopupPromiseResolve = null;
        resolve(value);
    }
}
$('#selectTimeClassificationModal').on('hidden.bs.modal', async function () {
    resolveTimeClassificationPopupPromiseWith(null);
});



// PWA install handling

let deferredInstallPrompt = null;
const installButton = document.getElementById('installPwaButton');

window.addEventListener('beforeinstallprompt', function (evt) {
    // Save event & show the install button.
    deferredInstallPrompt = evt;
    installButton.removeAttribute('hidden');
});


installButton.addEventListener('click', function installPWA(evt) {
    installButton.setAttribute('hidden', true);
    deferredInstallPrompt.prompt();

    deferredInstallPrompt.userChoice
        .then((choice) => {
            if (choice.outcome === 'accepted') {
                console.log('User accepted the A2HS prompt', choice);
            } else {
                console.log('User dismissed the A2HS prompt', choice);
            }
            deferredInstallPrompt = null;
        });
});

window.addEventListener('appinstalled', function logAppInstalled(evt) {
    // Code to log the event
    $("#installPwaButton").hide();
    console.log('Dump Truck Dispatcher was installed.', evt);
});

if ('serviceWorker' in navigator) {
    navigator.serviceWorker.addEventListener('message', event => {
        console.log(`The service worker sent me a message: ${JSON.stringify(event.data)}`);
    });
}