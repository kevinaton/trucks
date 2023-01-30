//import { openDB, deleteDB, wrap, unwrap } from '../lib/idb/with-async-ittr.js';

//export let App = {};
var App = {};
var simulateIosForDebug = false;
//todo do not re-authorize from service worker?

dayjs.extend(dayjs_plugin_customParseFormat);
dayjs.extend(dayjs_plugin_utc);
//dayjs.extend(duration);

const defaultHttpRequestTimeout = 60000; //1 min

App.Mutex = function Mutex() {
    let previousLockPromise = Promise.resolve();
    this.lock = () => {
        let _resolveCurrent;
        const currentLockPromise = new Promise(resolve => {
            _resolveCurrent = () => resolve();
        });
        const unlockPromise = previousLockPromise.then(() => _resolveCurrent);
        previousLockPromise = currentLockPromise;
        return unlockPromise;
    };
};

App.sleepAsync = function sleepAsync(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
};

//if (!('toJSON' in Error.prototype)) {
//    Object.defineProperty(Error.prototype, 'toJSON', {
//        value: function () {
//            var alt = {};

//            Object.getOwnPropertyNames(this).forEach(function (key) {
//                alt[key] = this[key];
//            }, this);

//            return alt;
//        },
//        configurable: true,
//        writable: true
//    });
//}

App.isAuthorized = isAuthorized;
async function isAuthorized() {
    let userManager = App.getUserManager();
    let user = await userManager.getUser();
    return user !== null;
}

App.ensureAuthorized = ensureAuthorized;
function ensureAuthorized() {
    let userManager = App.getUserManager();
    return userManager.getUser().then(user => {
        return new Promise(async (resolve, reject) => {
            await App.logInfo(`ensureAuthorized: entered, user is ${user ? 'not null' : 'null'}, expired: ${user && user.expired}, access_token is ${user && user.access_token ? 'not null' : 'null'}`);
            if (user) {
                if (user.access_token && !user.expired) {
                    resolve(user);
                    return;
                }
                try {
                    await App.logInfo(`ensureAuthorized: trying silent renew`);
                    let renewedUser = await userManager.signinSilent();
                    await App.logInfo(`ensureAuthorized: silently renewed auth`);
                    resolve(renewedUser);
                    return;
                } catch (error) {
                    await App.logWarn('ensureAuthorized: failed silent auth renew');
                }
            }
            //when no user or when caught exception on signinSilent:
            if (user) {
                //when caught exception on signinSilent:
                await App.updateServiceWorker(true);
            }
            App.ui.setBusy("Redirecting to authenticate");
            await App.logInfo(`ensureAuthorized: redirecting to authenticate`);
            await userManager.signinRedirect().then(() => { }, reject);
        }).then(user => {
            return refreshCachedAuthHeaderIfNeeded(user.access_token).then(() => {
                return user;
            });
        });
    });
}

function getHttpRequestTimeout() {
    return App && App._info && App._info.httpRequestTimeout || defaultHttpRequestTimeout;
}

async function refreshCachedAuthHeaderIfNeeded(token) {
    let authHeader = 'Bearer ' + token;
    if (!App._info || App._info.cachedAuthHeader === authHeader) {
        return;
    }
    App._info.cachedAuthHeader = authHeader;
    let db = await App.getDb();
    await db.put(App.DbStores.info, App._info);
}

App.getFromApi = getFromApi;
function getFromApi(options) {
    options.data = options.data || {};
    if (App._info && App.getDriverGuidOrNull(App._info)) {
        options.data.driverGuid = App.getDriverGuidOrNull(App._info);
        options.data.deviceId = App._config.deviceId;
    }
    options.method = options.method || 'GET';
    if (typeof window === 'undefined') {
        return postDataToApiWithCachedAuth(options.path, options.data, options.method);
    }
    return ensureAuthorized().then(user => {
        let headers = {
            Authorization: 'Bearer ' + user.access_token
        };
        if (options.method && options.method.toLowerCase() === 'post') {
            headers['Accept'] = 'application/json';
            headers['Content-Type'] = 'application/json';
            options.data = JSON.stringify(options.data || 'null');
        }
        return $.Deferred(def => {
            $.ajax({
                url: App._config.apiUri + options.path,
                type: options.method,
                data: options.data,
                headers: headers,
                dataType: 'json',
                timeout: getHttpRequestTimeout(),
                success: async function (data) {
                    //await App.logDebug(data);
                    //if (data.result) { //data.result can be null and it is not an error (e.g. for DateTime? result)
                    if (data.success) {
                        //success: true
                        //error: null
                        //unAuthorizedRequest: false
                        def.resolve(data.result);
                    } else {
                        await App.logError('An error returned from the backend (data.success != true)', data);
                        def.reject('getFromApi unsuccessful call');
                    }
                },
                error: async function (e, textStatus) {
                    if (e.status === 401) {
                        //App.ui.setBusy('Renewing authentication'); //debug
                        await App.logInfo('Renewing auth after receiving 401 in getFromApi');
                        App.getUserManager().signinSilent().then(async renewedUser => {
                            //App.ui.setBusy('Repeating API call'); //debug
                            await App.logInfo('Repeating API call');
                            getFromApi(options).then(result => {
                                def.resolve(result);
                            }).catch(error => {
                                def.reject(error);
                            });
                        }, async (error) => {
                            await App.logInfo('Renew failed, redirecting to authenticate');
                            await App.updateServiceWorker(true);
                            App.ui.setBusy("Redirecting to authenticate");
                            App.getUserManager().signinRedirect().then(() => { }, (error) => def.reject(error));
                        });
                        return;
                    }
                    if (e.responseJSON && e.responseJSON.error && e.responseJSON.error.message) {
                        await App.logError(e.responseJSON.error.message);
                        if (App.ui.showValidationError) {
                            let callback = null;
                            if (e.responseJSON.error.message === 'The current user isn\'t a driver!') {
                                callback = async () => {
                                    App.getUserManager().signoutRedirect();
                                };
                            }
                            App.ui.showValidationError(e.responseJSON.error.message, 'Error occurred while receiving info from API', callback);
                        }
                    }
                    await App.logError('error during getFromApi', e, textStatus);
                    def.reject('getFromApi unsuccessful call');
                }
            });
        });
    });
}

App.getDispatchesFromApi = getDispatchesFromApi;
async function getDispatchesFromApi(updatedAfterDateTime) {
    updatedAfterDateTime = updatedAfterDateTime || '';
    var newDispatches = await getFromApi({
        path: '/api/services/app/Dispatching/GetDriverDispatchesForCurrentUser',
        data: {
            UpdatedAfterDateTime: updatedAfterDateTime
        }
    });
    return newDispatches;
}

App.getScheduledStartTimeInfoFromApi = getScheduledStartTimeInfoFromApi;
async function getScheduledStartTimeInfoFromApi() {
    var result = await getFromApi({ path: '/api/services/app/DriverApplication/GetScheduledStartTimeInfo' });
    return result;
}


(typeof window !== 'undefined') && (window.App = App);
App.Consts = App.Consts || {};
App.Consts.DbName = 'DispatchesDb';
App.Consts.DbVersion = 11;
App.DbStores = {
    dispatches: 'dispatches',
    info: 'info',
    swInfo: 'swInfo', //service worker info
    config: 'config',
    //todo add update queue
    changesQueue: 'changesQueue',
    lightChangesQueue: 'lightChangesQueue',
    largeChangesQueue: 'largeChangesQueue',
    dispatchChanges: 'dispatchChanges',
    startTimes: 'startTimes',
    logs: 'logs',
    pushEventQueue: 'pushEventQueue'
};
App.DbTransactionModes = {
    readonly: 'readonly',
    readwrite: 'readwrite'
};
App.Mutexes = {
    lightQueue: 'lightQueue',
    largeQueue: 'largeQueue',
    logs: 'logs'
};
//formats for storing
App.Consts.DateTimeFormat = 'YYYY-MM-DDTHH:mm:ss';
App.Consts.DateIsoFormat = 'YYYY-MM-DD';
//formats for displaying
App.Consts.TimeFormat = 'H:mm:ss';
App.Consts.ShortTimeFormat = 'h:mm A';
App.Consts.DateFormat = 'MM/DD/YYYY';
App.Consts.DateTimeGeneralFormat = 'MM/DD/YYYY h:mm A';

App.Enums = App.Enums || {};
App.Enums.DispatchStatus = {
    created: 0,
    sent: 1,
    acknowledged: 3,
    loaded: 4,
    completed: 5,
    error: 6,
    canceled: 7
};
App.Enums.Designation = {
    freightOnly: 1,
    materialOnly: 2,
    freightAndMaterial: 3,
    //rental: 4,
    backhaulFreightOnly: 5,
    backhaulFreightAndMaterial: 9,
    disposal: 6,
    backHaulFreightAndDisposal: 7,
    straightHaulFreightAndDisposal: 8
};
App.Enums.Designations = {
    hasMaterial: [
        App.Enums.Designation.materialOnly,
        App.Enums.Designation.freightAndMaterial,
        App.Enums.Designation.backhaulFreightAndMaterial,
        App.Enums.Designation.disposal,
        App.Enums.Designation.backHaulFreightAndDisposal,
        App.Enums.Designation.straightHaulFreightAndDisposal
    ],
    materialOnly: [
        App.Enums.Designation.materialOnly
    ],
    freightOnly: [
        App.Enums.Designation.freightOnly,
        App.Enums.Designation.backhaulFreightOnly
    ],
    freightAndMaterial: [
        App.Enums.Designation.freightAndMaterial,
        App.Enums.Designation.backhaulFreightAndMaterial,
        App.Enums.Designation.disposal,
        App.Enums.Designation.backHaulFreightAndDisposal,
        App.Enums.Designation.straightHaulFreightAndDisposal
    ]
};
App.Enums.DriverApplicationAction = {
    clockIn: 1,
    clockOut: 2,
    acknowledgeDispatch: 3,
    loadDispatch: 4,
    completeDispatch: 5,
    addSignature: 6,
    saveDriverPushSubscription: 7,
    removeDriverPushSubscription: 8,
    modifyDispatchTicket: 9,
    uploadDeferredBinaryObject: 10,
    uploadLogs: 11,
    modifyEmployeeTime: 12,
    removeEmployeeTime: 13,
    addDriverNote: 14,
    cancelDispatch: 15,
    markDispatchComplete: 16
};
App.Enums.PushAction = {
    none: 0,
    sync: 1,
    silentSync: 2
};
App.Enums.DeferredBinaryObjectDestination = {
    ticketPhoto: 1,
    loadSignature: 2
};

function getDriverApplicationActionName(val) {
    switch (val) {
        case App.Enums.DriverApplicationAction.clockIn: return 'clockIn';
        case App.Enums.DriverApplicationAction.clockOut: return 'clockOut';
        case App.Enums.DriverApplicationAction.acknowledgeDispatch: return 'acknowledgeDispatch';
        case App.Enums.DriverApplicationAction.loadDispatch: return 'loadDispatch';
        case App.Enums.DriverApplicationAction.completeDispatch: return 'completeDispatch';
        case App.Enums.DriverApplicationAction.addSignature: return 'addSignature';
        case App.Enums.DriverApplicationAction.saveDriverPushSubscription: return 'saveDriverPushSubscription';
        case App.Enums.DriverApplicationAction.removeDriverPushSubscription: return 'removeDriverPushSubscription';
        case App.Enums.DriverApplicationAction.modifyDispatchTicket: return 'modifyDispatchTicket';
        case App.Enums.DriverApplicationAction.uploadDeferredBinaryObject: return 'uploadDeferredBinaryObject';
        default: return 'unknown (' + val + ')';
    }
}

function getDispatchStatusName(val) {
    switch (val) {
        case App.Enums.DispatchStatus.created: return 'created';
        case App.Enums.DispatchStatus.sent: return 'sent';
        case App.Enums.DispatchStatus.acknowledged: return 'acknowledged';
        case App.Enums.DispatchStatus.loaded: return 'loaded';
        case App.Enums.DispatchStatus.completed: return 'completed';
        case App.Enums.DispatchStatus.error: return 'error';
        case App.Enums.DispatchStatus.canceled: return 'canceled';
        default: return 'unknown (' + val + ')';
    }
}

App.getDb = getDb;
async function getDb() {
    if (App._db) {
        return App._db;
    }
    const db = await idb.openDB(App.Consts.DbName, App.Consts.DbVersion, {
        async upgrade(db, oldVersion, newVersion, transaction) {
            //do not store the logs during db migration
            console.log('Upgrading indexed db', { oldVersion, newVersion });
            if (oldVersion < 1) {
                db.createObjectStore(App.DbStores.dispatches, { keyPath: 'id' });
            }
            if (oldVersion < 2) {
                db.createObjectStore(App.DbStores.info, { keyPath: 'id' });
                db.createObjectStore(App.DbStores.changesQueue, { keyPath: 'id', autoIncrement: true });
            }
            if (oldVersion < 3) {
                db.createObjectStore(App.DbStores.config, { keyPath: 'id' });
            }
            if (oldVersion < 4) {
                db.createObjectStore(App.DbStores.dispatchChanges, { keyPath: 'id' });
            }
            if (oldVersion < 5) {
                db.createObjectStore(App.DbStores.swInfo, { keyPath: 'id' });
            }
            if (oldVersion < 6) {
                db.createObjectStore(App.DbStores.startTimes, { keyPath: 'date' });
            }
            if (oldVersion < 7) {
                db.createObjectStore(App.DbStores.lightChangesQueue, { keyPath: 'id', autoIncrement: true });
                db.createObjectStore(App.DbStores.largeChangesQueue, { keyPath: 'id', autoIncrement: true });
            }
            if (oldVersion < 8) {
                let logsStore = db.createObjectStore(App.DbStores.logs, { keyPath: 'id', autoIncrement: true });
                logsStore.createIndex('datetime', 'datetime', { unique: false });
                logsStore.createIndex('level', 'level', { unique: false });
            }
            if (oldVersion < 10) {
                let logsStore = transaction.objectStore(App.DbStores.logs);
                let logCursor = await logsStore.openCursor();
                while (logCursor) {
                    let logRecord = { isUploaded: 0, ...logCursor.value };
                    await logCursor.update(logRecord);
                    logCursor = await logCursor.continue();
                }
                logsStore.createIndex('isUploaded', 'isUploaded', { unique: false });
            }
            if (oldVersion < 11) {
                db.createObjectStore(App.DbStores.pushEventQueue, { keyPath: 'id', autoIncrement: true });
            }
        }
    });
    App._db = db;
    return db;
}

App._config = null;
App.initConfig = initConfig;
async function initConfig(parsedConfig) {
    let db = await getDb();
    App._config = await db.get(App.DbStores.config, 1);
    if (!App._config) {
        if (!parsedConfig) {
            return null;
        }
        App._config = {
            id: 1,
            identityServerUri: parsedConfig.identityServerUri,
            apiUri: parsedConfig.apiUri,
            driverAppUri: parsedConfig.driverAppUri,
            webPushServerPublicKey: parsedConfig.webPushServerPublicKey,
            deviceGuid: uuidv4()
        };
        await db.add(App.DbStores.config, App._config);
        await App.logDebug('added config to db');
    } else {
        if (parsedConfig) {
            if (!App._config.webPushServerPublicKey || App._config.webPushServerPublicKey !== parsedConfig.webPushServerPublicKey) {
                App._config.webPushServerPublicKey = parsedConfig.webPushServerPublicKey;
                await db.put(App.DbStores.config, App._config);
            }
        }
        if (!App._config.deviceGuid) {
            App._config.deviceGuid = uuidv4();
            await db.put(App.DbStores.config, App._config);
        }
    }
    App._oidcConfig = {
        authority: App._config.identityServerUri,
        client_id: 'driverapplicationclient',
        redirect_uri: `${App._config.driverAppUri}/Authentication/Callback`,
        silent_redirect_uri: `${App._config.driverAppUri}/Authentication/SilentCallback`,
        post_logout_redirect_uri: `${App._config.driverAppUri}/Authentication/SignoutCallback`,
        //invalid_signin_redirect_uri: `${App._config.driverAppUri}/`,
        response_type: 'id_token token',
        scope: 'openid profile default-api',
        monitorSession: true,
        //monitorSession: false,
        userStore: new Oidc.WebStorageStateStore({ store: Oidc.Global.localStorage })
    };

    return App._config;
}

App._userManager = null;
App.getUserManager = getUserManager;
function getUserManager() {
    if (App._userManager) {
        return App._userManager;
    }
    return App._userManager = new Oidc.UserManager(App._oidcConfig);
}

async function getInfoFromApi(input) {
    let requestNewDeviceId = !App._config.deviceId;
    let infoFromApi = await getFromApi({
        path: '/api/services/app/DriverApplication/PostDriverAppInfo',
        method: 'POST',
        data: {
            RequestNewDeviceId: requestNewDeviceId,
            Useragent: requestNewDeviceId ? navigator.userAgent : null,
            PushSubscription: JSON.parse(input && input.pushSubscriptionString || 'null'),
            DeviceId: App._config.deviceId
        }
    });
    return infoFromApi;
}

async function storeDeviceIdIfNeeded(infoFromApi) {
    if (!App._config.deviceId && infoFromApi.deviceId) {
        App._config.deviceId = infoFromApi.deviceId;
        let db = await getDb();
        await db.put(App.DbStores.config, App._config);
        await App.logInfo(`Stored new deviceId ${App._config.deviceId}`);
    }
}

App._info = null;
App.initInfo = initInfo;
async function initInfo() {
    let db = await getDb();
    App._info = await db.get(App.DbStores.info, 1);
    await App.logDebug({ infoFromDb: App._info });
    //if (!App._info && !navigator.onLine) {
    //    return null;
    //}
    if (!App._info) {
        //first initialization, the internet connection is expected (since they loaded the pages somehow)
        let pushSubscriptionString = null;
        if (hasNotificationSupport()) {
            pushSubscriptionString = JSON.stringify(await getOrRequestPushSubscription());
        }
        let infoFromApi = await getInfoFromApi({
            pushSubscriptionString: pushSubscriptionString
        });
        await App.logDebug({ infoFromApi });
        App._info = {
            id: 1,
            //lastApiRequestDate has been moved to swInfo
            //lastApiRequestDate: null, //update to the max timestamp on retreiving the records
            isClockStarted: infoFromApi.elapsedTime.clockIsStarted, //false,
            committedElapsedSeconds: infoFromApi.elapsedTime.committedElapsedSeconds,
            committedElapsedSecondsForDay: infoFromApi.elapsedTime.committedElapsedSecondsForDay, //todo reset committedElapsedSeconds if 'today' is different from this value
            lastClockStartDateTime: infoFromApi.elapsedTime.lastClockStartTime,
            driverGuid: App.getDriverGuidOrNull(infoFromApi),
            driverName: infoFromApi.driverName,
            driverLeaseHaulerId: infoFromApi.driverLeaseHaulerId,
            userId: infoFromApi.userId,
            isDriver: infoFromApi.isDriver,
            isAdmin: infoFromApi.isAdmin,
            shiftNames: infoFromApi.shiftNames,
            useShifts: infoFromApi.useShifts,
            useBackgroundSync: infoFromApi.useBackgroundSync,
            httpRequestTimeout: infoFromApi.httpRequestTimeout,
            hideTicketControls: infoFromApi.hideTicketControls,
            requireToEnterTickets: infoFromApi.requireToEnterTickets,
            requireSignature: infoFromApi.requireSignature,
            requireTicketPhoto: infoFromApi.requireTicketPhoto,
            textForSignatureView: infoFromApi.textForSignatureView,
            dispatchesLockedToTruck: infoFromApi.dispatchesLockedToTruck,
            timeClassifications: infoFromApi.timeClassifications,
            productionPayId: infoFromApi.productionPayId,
            lastTakenTicketPhoto: null,
            lastTakenTicketPhotoFilename: null,
            pushSubscriptionString: pushSubscriptionString
        };
        await db.add(App.DbStores.info, App._info);
        await App.logInfo('Added App._info to db');
        await storeDeviceIdIfNeeded(infoFromApi);
    } else {
        if (navigator.onLine) {
            try {
                let infoFromApi = await getInfoFromApi();
                if (App.getDriverGuidOrNull(App._info) !== App.getDriverGuidOrNull(infoFromApi) || App._info.userId !== infoFromApi.userId) {
                    await App.logWarn('Received driver guid doesn\'t match with cached driver guid', { cached: App._info.driverGuid, received: infoFromApi.driverGuid });
                    await clearCache();
                    location.reload();
                    return null;
                }
                App._info.driverGuid = App.getDriverGuidOrNull(infoFromApi);
                App._info.driverName = infoFromApi.driverName;
                App._info.isDriver = infoFromApi.isDriver;
                App._info.isAdmin = infoFromApi.isAdmin;
                App._info.userId = infoFromApi.userId;
                App._info.shiftNames = infoFromApi.shiftNames;
                App._info.useShifts = infoFromApi.useShifts;
                App._info.useBackgroundSync = infoFromApi.useBackgroundSync;
                App._info.httpRequestTimeout = infoFromApi.httpRequestTimeout;
                App._info.hideTicketControls = infoFromApi.hideTicketControls;
                App._info.requireToEnterTickets = infoFromApi.requireToEnterTickets;
                App._info.requireSignature = infoFromApi.requireSignature;
                App._info.requireTicketPhoto = infoFromApi.requireTicketPhoto;
                App._info.textForSignatureView = infoFromApi.textForSignatureView;
                App._info.dispatchesLockedToTruck = infoFromApi.dispatchesLockedToTruck;
                App._info.timeClassifications = infoFromApi.timeClassifications;
                App._info.productionPayId = infoFromApi.productionPayId;
                await db.put(App.DbStores.info, App._info);
                await App.logInfo('Updated App._info values from API');
                await storeDeviceIdIfNeeded(infoFromApi);
            } catch (e) {
                await App.logWarn('initInfo: exception during refreshing driver info from API, using cached values until page reload', e);
                return App._info;
            }
            try {
                await subscribeToPushNotificationsIfNeeded();
            } catch (e) {
                await App.logWarn('initInfo: exception during refreshing push subscription / notification permission', e);
                return App._info;
            }
        }
    }
    return App._info;
}

App.getSwInfo = getSwInfo;
async function getSwInfo() {
    let db = await App.getDb();
    return await db.get(App.DbStores.swInfo, 1) || { id: 1 };
}

App.clearCache = clearCache;
async function clearCache() {
    await App.logInfo('Clearing driver\'s cache...');
    let db = await App.getDb();
    await db.clear(App.DbStores.info);
    await db.clear(App.DbStores.swInfo);
    await db.clear(App.DbStores.dispatches);
    await db.clear(App.DbStores.startTimes);
    await App.invalidateSwDbCache();
}

App.getLocation = getLocation;
async function getLocation() {
    return new Promise((resolve, reject) => {
        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition((pos) => resolve(pos), onError);
        } else {
            resolve(null);
        }

        async function onError(error) {
            //todo add toaster notifications?
            //abp.notify.error('Location error: ' + error.message);
            await App.logWarn('Location error: ' + error.message);
            resolve(null);
        }
    });
}

App.getHigherPriorityDispatchStatus = getHigherPriorityDispatchStatus;
function getHigherPriorityDispatchStatus(source, localChange) {
    if (isCompletedOrCanceledDispatchStatus(source)) {
        return source;
    }
    if (isCompletedOrCanceledDispatchStatus(localChange)) {
        return localChange;
    }
    //created: 0,
    //sent: 1,
    //acknowledged: 3,
    //loaded: 4,
    //completed: 5,
    //error: 6,
    //canceled: 7
    //if (a === App.Enums.DispatchStatus.completed
    //todo, how do we know which change has taken place later in time than the other one (e.g. driver is offline and both are clicking 'loaded', 'delivered', 'acknowledged')
    return typeof localChange !== "undefined" ? localChange : source;
}

App.isCompletedOrCanceledDispatch = isCompletedOrCanceledDispatch;
function isCompletedOrCanceledDispatch(dispatch) {
    return App.isCompletedOrCanceledDispatchStatus(dispatch.dispatchStatus);
}

App.isCompletedOrCanceledDispatchStatus = isCompletedOrCanceledDispatchStatus;
function isCompletedOrCanceledDispatchStatus(dispatchStatus) {
    return dispatchStatus === App.Enums.DispatchStatus.completed || dispatchStatus === App.Enums.DispatchStatus.canceled;
}

App.updateDispatch = updateDispatch;
async function updateDispatch(currentMergedDispatch, changeCallback) {
    let db = await App.getDb();
    let currentChange = await db.get(App.DbStores.dispatchChanges, currentMergedDispatch.id);
    currentChange = currentChange || { id: currentMergedDispatch.id };
    await changeCallback(currentChange);
    await db.put(App.DbStores.dispatchChanges, currentChange);
    if (typeof $ !== 'undefined') {
        $.extend(currentMergedDispatch, currentChange);
    } else {
        await App.logError("$ is undefined in updateDispatch");
    }
}

App.updateSwInfo = updateSwInfo;
async function updateSwInfo(editCallback) {
    let db = await App.getDb();
    const tx = db.transaction(App.DbStores.swInfo, App.DbTransactionModes.readwrite);
    let swInfo = await tx.store.get(1) || { id: 1 };
    editCallback(swInfo); //editCallback cannot be async, otherwise the transaction will be closed on awaiting inside the callback
    tx.store.put(swInfo);
    await tx.done;
}

App.tryGetAndStoreNewDispatches = tryGetAndStoreNewDispatches;
async function tryGetAndStoreNewDispatches(throwOnError = false) {
    try {
        await App.logInfo(`tryGetAndStoreNewDispatches: entered, navigator.onLine: ${navigator.onLine}, ` + App._info ? App._info.isDriver ? 'isDriver:true' : 'isDriver:false' : 'App._info is null');
        if (navigator.onLine && App._info && App._info.isDriver) {
            let newDispatches = [];
            let db = await App.getDb();
            let lastApiRequestDate = (await getSwInfo()).lastApiRequestDate;
            await App.logInfo(`tryGetAndStoreNewDispatches: lastApiRequestDate: ${lastApiRequestDate}`);
            newDispatches = await getDispatchesFromApi(lastApiRequestDate);
            let newDispatchIds = newDispatches && newDispatches.map(x => x.id).join(', ') || '-';
            await App.logInfo(`tryGetAndStoreNewDispatches: received ${newDispatches && newDispatches.length} new/changed dispatches from API: ${newDispatchIds}`);
            await App.logDebug({ newDispatches });
            if (newDispatches && newDispatches.length) {
                const tx = db.transaction(App.DbStores.dispatches, App.DbTransactionModes.readwrite);
                newDispatches.forEach(dispatch => {
                    tx.store.put(dispatch);
                });
                await tx.done;
                await App.logInfo('tryGetAndStoreNewDispatches: saved new dispatches to db');

                let maxDate = newDispatches.map(a => a.lastUpdateDateTime).reduce((a, b) => !b || a > b ? a : b, lastApiRequestDate);
                await App.logInfo(`tryGetAndStoreNewDispatches: new lastApiRequestDate: ${maxDate}`);
                let updatedRequestUiUpdate = false;
                await App.updateSwInfo(swInfo => {
                    var wasDifferent = swInfo.lastApiRequestDate !== maxDate;
                    swInfo.lastApiRequestDate = maxDate;
                    if (wasDifferent) {
                        swInfo.requestUiUpdate = true;
                        updatedRequestUiUpdate = true;
                    }
                });
                await App.logInfo(`stored new lastApiRequestDate, requestUiUpdate: ${updatedRequestUiUpdate}`);
            }
            await App.logInfo(`tryGetAndStoreNewDispatches: getting scheduled start time info`);
            let scheduledStartTimeInfoArr = await getScheduledStartTimeInfoFromApi();
            await App.logInfo(`tryGetAndStoreNewDispatches: received ${scheduledStartTimeInfoArr && scheduledStartTimeInfoArr.length} scheduled start time records`);
            if (scheduledStartTimeInfoArr && scheduledStartTimeInfoArr.length) {
                for (let i = 0; i < scheduledStartTimeInfoArr.length; i++) {
                    var startInfo = scheduledStartTimeInfoArr[i];
                    startInfo.date = dayjs(startInfo.date, App.Consts.DateTimeFormat).format(App.Consts.DateIsoFormat);
                    startInfo.nextAssignmentDate = startInfo.nextAssignmentDate ? dayjs(startInfo.nextAssignmentDate, App.Consts.DateTimeFormat).format(App.Consts.DateIsoFormat) : null;
                    await db.put(App.DbStores.startTimes, startInfo);
                }
                if (!App._info.clockIsStarted) {
                    await App.logInfo(`tryGetAndStoreNewDispatches: setting requestUiUpdate flag`);
                    await App.updateSwInfo(swInfo => {
                        swInfo.requestUiUpdate = true;
                    });
                }
            }
            await App.logInfo(`tryGetAndStoreNewDispatches: finished processing`);
            return newDispatches || [];
        } else {
            await App.logInfo(`tryGetAndStoreNewDispatches: exiting because offline or not a driver`);
            return [];
        }
    } catch (e) {
        await App.logError('tryGetAndStoreNewDispatches exception: ', e);
        if (throwOnError) {
            throw e;
        }
        return [];
    }
}

async function postData(url = '', data = {}, method = 'POST', authHeader = null) {
    // Default options are marked with *
    var headers = {
        'Content-Type': 'application/json'
        // 'Content-Type': 'application/x-www-form-urlencoded',
    };
    if (authHeader) {
        headers.Authorization = authHeader;
    }
    const abortController = new AbortController();
    var options = {
        method: method, // *GET, POST, PUT, DELETE, etc.
        mode: 'cors', // no-cors, *cors, same-origin
        cache: 'no-cache', // *default, no-cache, reload, force-cache, only-if-cached
        credentials: 'same-origin', // include, *same-origin, omit
        headers: headers,
        redirect: 'follow', // manual, *follow, error
        referrerPolicy: 'no-referrer', // no-referrer, *client
        signal: abortController.signal
    };
    if (method === 'POST' && data) {
        options.body = JSON.stringify(data); // body data type must match "Content-Type" header
    }
    if (method === 'GET' && data) {
        url += '?' + new URLSearchParams(data).toString();
    }
    let timeoutTimer = setTimeout(async () => {
        await App.logError('Aborting fetch due to timeout, url: ' + url);
        abortController.abort();
    }, getHttpRequestTimeout());
    const response = await fetch(url, options).finally(() => clearTimeout(timeoutTimer));
    return await response.json(); // parses JSON response into native JavaScript objects
}

App.postDataToApiWithCachedAuth = postDataToApiWithCachedAuth;
async function postDataToApiWithCachedAuth(path = '', data = {}, method = 'POST') {
    if (!App._info.cachedAuthHeader && !App.getDriverGuidOrNull(App._info)) {
        throw new Error("No cached auth header & no cached driverGuid");
    }
    var result = await postData(App._config.apiUri + path, data, method/*, App._info.cachedAuthHeader*/);
    await App.logDebug({ apiRequestResult: result });
    if (!result.success) {
        var error = result.error ? result.error.code + ' ' + result.error.message + ' ' + result.error.details : 'Unknown error on API request';
        await App.logError('postDataToApiWithCachedAuth error, !result.success: ', error, result);
        throw Error(error);
    }
    return result.result;
}

async function postDataToApiNoAuth(path = '', data = {}, method = 'POST') {
    var result = await postData(App._config.apiUri + path, data, method);
    await App.logDebug({ apiRequestResult: result });
    if (!result.success) {
        var error = result.error ? result.error.code + ' ' + result.error.message + ' ' + result.error.details : 'Unknown error on API request';
        await App.logError('postDataToApiNoAuth error, !result.success: ', error, result);
        throw Error(error);
    }
    return result.result;
}

//async function enqueueChangeAndRunSync(change) {
//    await enqueueChangeOnly(change);
//    await uploadChangesQueue();
//}

App.enqueueChange = enqueueChange;
async function enqueueChange(change, queueName, syncIncomingChanges = true) {
    try {
        if (navigator.onLine) {
            //ensure cachedAuthHeader is refreshed
            await ensureAuthorized();
        }
    }
    catch (e) {
        await App.logWarn('ensureAuthorized call failed', e);
    }
    var changeId = await enqueueChangeOnly(change, queueName);
    if (syncIncomingChanges) {
        await requestSyncWithApi();
        await requestLogsUpload();
        //if (queueName === App.DbStores.largeChangesQueue) {
            await requestLargeQueueUpload();
        //}
    } else {
        await requestChangesQueueUpload();
        await requestLogsUpload();
    }
    return changeId;
}

App.enqueueChangeOnly = enqueueChangeOnly;
async function enqueueChangeOnly(change, queueName) {
    queueName = queueName || App.DbStores.lightChangesQueue;
    let db = await getDb();
    await App.logInfo(`Enqueue change ${getDriverApplicationActionName(change.action)} to ${queueName}`);
    change.actionTimeInUtc = dayjs().utc().format(App.Consts.DateTimeFormat);
    change.driverGuid = App.getDriverGuidOrNull(App._info);
    change.deviceId = App._config.deviceId;
    var newId = await db.add(queueName, change);
    await App.logDebug('added new change to the queue ' + queueName, change);
    return newId;
}

let unsupportedDeviceWarningShownOnce = false;
function showUnsupportedDeviceWarningOnce() {
    if (!unsupportedDeviceWarningShownOnce && typeof window !== 'undefined' && window.showValidationError) {
        showValidationError("Your browser does not support all of the required functions and the app may not behave as intended.", "Warning: Unsupported browser");
        unsupportedDeviceWarningShownOnce = true;
    }
}

App.hasSyncManagerSupport = hasSyncManagerSupport;
function hasSyncManagerSupport() {
    if (simulateIosForDebug) {
        return false;
    }
    if (App._info && !App._info.useBackgroundSync) {
        return false;
    }
    return typeof window !== 'undefined' && 'serviceWorker' in navigator && 'SyncManager' in window;
}

//called on refresh button click before refreshing dispatches
App.requestChangesQueueUpload = requestChangesQueueUpload;
async function requestChangesQueueUpload() {
    if (hasSyncManagerSupport()) {
        try {
            await App.logDebug('sync manager is supported');
            let swRegistration = await navigator.serviceWorker.ready;
            await App.logDebug('service worker is ready');
            await App.logInfo('entered requestChangesQueueUpload');
            await App.tryLogBatteryInfo();
            await App.logInfo('sync tags before sync.register: ', await swRegistration.sync.getTags());
            await App.registerSyncEvent(swRegistration, 'uploadLightChangesQueue');
            await App.logDebug('sent uploadChangesQueue (light) request to the service worker');
            await App.registerSyncEvent(swRegistration, 'uploadLargeChangesQueue');
            await App.logDebug('sent uploadChangesQueue (large) request to the service worker');
            await App.logInfo('sync tags after sync.register: ', await swRegistration.sync.getTags());
        } catch (e) {
            await App.logWar('background sync call failed, using direct sync', e);
            //showUnsupportedDeviceWarningOnce();
            await tryLockAndUploadAllChangesQueues();
        }
    } else {
        await App.logDebug('background sync call is unavailable, using direct sync');
        //showUnsupportedDeviceWarningOnce();
        await tryLockAndUploadAllChangesQueues();
    }
}

//called after enqueueChangeOnly
App.requestSyncWithApi = requestSyncWithApi;
async function requestSyncWithApi() {
    if (hasSyncManagerSupport()) {
        try {
            await App.logInfo('entered requestSyncWithApi');
            await App.logDebug('sync manager is supported');
            let swRegistration = await navigator.serviceWorker.ready;
            await App.logDebug('service worker is ready');
            await App.tryLogBatteryInfo();
            await App.logInfo('sync tags before sync.register: ', await swRegistration.sync.getTags());
            await App.registerSyncEvent(swRegistration, 'syncDataWithApi'); //uploadChangesQueue
            await App.logInfo('sync tags after sync.register: ', await swRegistration.sync.getTags());
            await App.logDebug('sent syncDataWithApi request to the service worker');
        } catch (e) {
            await App.logWarn('background sync call failed, using direct sync', e);
            //showUnsupportedDeviceWarningOnce();
            await tryLockAndSyncDataWithApi();
        }
    } else {
        await App.logDebug('background sync call is unavailable, using direct sync');
        //showUnsupportedDeviceWarningOnce();
        await tryLockAndSyncDataWithApi();
    }
}

//called after enqueueChangeOnly
App.requestLargeQueueUpload = requestLargeQueueUpload;
async function requestLargeQueueUpload() {
    if (hasSyncManagerSupport()) {
        try {
            await App.logDebug('sync manager is supported');
            let swRegistration = await navigator.serviceWorker.ready;
            await App.logDebug('service worker is ready');
            await swRegistration.sync.register('uploadLargeChangesQueue');
            await App.logDebug('sent uploadLargeChangesQueue request to the service worker');
        } catch (e) {
            await App.logWarn('background sync call failed, using direct sync', e);
            //showUnsupportedDeviceWarningOnce();
            await tryLockAndLargeQueueUpload();
        }
    } else {
        await App.logDebug('background sync call is unavailable, using direct sync');
        //showUnsupportedDeviceWarningOnce();
        await tryLockAndLargeQueueUpload();
    }
}

App.requestLogsUpload = requestLogsUpload;
async function requestLogsUpload() {
    if (hasSyncManagerSupport()) {
        try {
            await App.logDebug('sync manager is supported');
            let swRegistration = await navigator.serviceWorker.ready;
            await App.logDebug('service worker is ready');
            await App.registerSyncEvent(swRegistration, 'uploadLogs');
            await App.logDebug('sent uploadLogs request to the service worker');
        } catch (e) {
            await App.logWarn('background sync call failed, using direct sync', e);
            //showUnsupportedDeviceWarningOnce();
            await tryLockAndUploadLogs();
        }
    } else {
        await App.logDebug('background sync call is unavailable, using direct sync');
        //showUnsupportedDeviceWarningOnce();
        await tryLockAndUploadLogs();
    }
}

App.getConfigOnlyFromDb = getConfigOnlyFromDb;
async function getConfigOnlyFromDb() {
    let db = await App.getDb();
    App._config = App._config || await db.get(App.DbStores.config, 1);
    if (!App._config) {
        await App.logWarn("getConfigOnlyFromDb: App._config wasn't stored to the db yet, exiting sync");
        return false;
    }
    return true;
}

App.getConfigAndInfoFromDb = getConfigAndInfoFromDb;
async function getConfigAndInfoFromDb() {
    let db = await App.getDb();
    App._info = App._info || await db.get(App.DbStores.info, 1);
    App._config = App._config || await db.get(App.DbStores.config, 1);
    if (!App._info || !App._config) {
        await App.logWarn("getConfigAndInfoFromDb: App._info or App._config wasn't stored to the db yet, exiting sync");
        return false;
    }
    if (!App._info.cachedAuthHeader) {
        await App.logWarn("getConfigAndInfoFromDb: App._info.cachedAuthHeader is null yet");
        return false;
    }
    return true;
}

//for iOS with no background sync manager
App.tryLockAndUploadAllChangesQueues = tryLockAndUploadAllChangesQueues;
async function tryLockAndUploadAllChangesQueues() {
    let unlockSyncMutex = null;
    try {
        unlockSyncMutex = await App.lockSyncMutex(false, 'for uploading all change queues', App.Mutexes.lightQueue);
        if (unlockSyncMutex) {
            await uploadChangesQueue(App.DbStores.lightChangesQueue);
            await unlockSyncMutex();
        }
        unlockSyncMutex = await App.lockSyncMutex(false, 'for uploading all change queues', App.Mutexes.largeQueue);
        if (unlockSyncMutex) {
            await uploadChangesQueue(App.DbStores.largeChangesQueue);
        }
    } catch (e) {
        await App.logError('error during tryUploadAllChangeQueues', e);
    } finally {
        if (unlockSyncMutex) {
            await unlockSyncMutex();
        }
    }
}

App.tryLockAndLargeQueueUpload = tryLockAndLargeQueueUpload;
async function tryLockAndLargeQueueUpload() {
    let unlockSyncMutex = null;
    try {
        unlockSyncMutex = await App.lockSyncMutex(false, 'for uploading largeChangesQueue', App.Mutexes.largeQueue);
        if (!unlockSyncMutex) {
            return;
        }
        await uploadChangesQueue(App.DbStores.largeChangesQueue);
    } catch (e) {
        await App.logError('error during tryLockAndLargeQueueUpload', e);
    } finally {
        if (unlockSyncMutex) {
            await unlockSyncMutex();
        }
    }
}

App.tryLockAndUploadLogs = tryLockAndUploadLogs;
async function tryLockAndUploadLogs() {
    let unlockSyncMutex = null;
    try {
        unlockSyncMutex = await App.lockSyncMutex(false, 'for uploadLogs', App.Mutexes.logs);
        if (!unlockSyncMutex) {
            return;
        }
        return await uploadLogs();
    } catch (e) {
        await App.logError('error during tryLockAndUploadLogs', e);
    } finally {
        if (unlockSyncMutex) {
            await unlockSyncMutex();
        }
    }
    return true;
}

//let _uploadLogsMutex = new App.Mutex();
App.uploadLogs = uploadLogs;
async function uploadLogs() {
    const batchSize = 200;
    await App.logInfo('uploadLogs: entered'); // and awaiting mutex lock
    //const unlockUploadLogsMutex = await _uploadLogsMutex.lock();
    //try {
        //await App.logInfo('uploadLogs: received mutex lock');
        if (!await App.getConfigOnlyFromDb()) {
            throw 'getConfigOnlyFromDb failed';
        }
        let db = await getDb();
        let allUnsentLogRecords = await db.getAllFromIndex(App.DbStores.logs, 'isUploaded', 0);
        if (!allUnsentLogRecords.length) {
            await App.logDebug('no log records to upload');
            return true;
        }
        await App.logInfo(`uploadLogs: ${allUnsentLogRecords.length} records to upload`);
        if (!App._info) {
            App._info = await db.get(App.DbStores.info, 1);
        }
        let info = App._info;
        for (var i = 0; i < allUnsentLogRecords.length; i += batchSize) {
            let logRecords = allUnsentLogRecords.slice(i, i + batchSize);
            //group into smaller batches by driverGuid (in case there are logs with different driverGuid in the batch)
            var batches = [];
            for (let logRecord of logRecords) {
                if (!App.getDriverGuidOrNull(logRecord)) {
                    //populate driverGuid field of historical log records
                    logRecord.driverGuid = App.getDriverGuidOrNull(info);
                }
                let batch = batches.find(x => x.driverGuid === logRecord.driverGuid);
                if (!batch) {
                    batch = {
                        dataToSend: {
                            action: App.Enums.DriverApplicationAction.uploadLogs,
                            actionTimeInUtc: dayjs().utc().format(App.Consts.DateTimeFormat),
                            driverGuid: logRecord.driverGuid,
                            deviceId: App._config.deviceId,
                            deviceGuid: App._config.deviceGuid,
                            uploadLogsData: []
                        },
                        driverGuid: logRecord.driverGuid,
                        includedLogRecords: []
                    };
                    batches.push(batch);
                }
                batch.dataToSend.uploadLogsData.push({
                    id: logRecord.id,
                    dateTime: logRecord.datetime,
                    level: logRecord.level,
                    message: logRecord.message,
                    sw: logRecord.sw
                });
                batch.includedLogRecords.push(logRecord);
            }
            await App.logDebug('got batches', batches);
            //upload batches
            for (let batch of batches) {
                //if (!batch.driverGuid) {
                //    await App.logDebug('batch with no driverGuid, skipping the batch', batch);
                //    continue;
                //}
                await App.logDebug('sending batch', batch);
                try {
                    App.logDebug('about to send log batch data: ', batch.dataToSend);
                    if (batch.driverGuid) {
                        var responseTmp = await postDataToApiNoAuth('/api/services/app/dispatching/ExecuteDriverApplicationAction', batch.dataToSend);
                    } else {
                        batch.dataToSend.timezoneOffset = new Date().getTimezoneOffset();
                        var responseTmp = await postDataToApiNoAuth('/api/services/app/dispatching/UploadAnonymousLogs', batch.dataToSend);
                    }
                } catch (e) {
                    await App.logError('Error during uploadLogs/postDataToApiNoAuth: ', 'batch size: ' + batch.dataToSend.uploadLogsData.length, e);
                    throw e;
                }
                await App.logDebug('received response', responseTmp);
                const tx = db.transaction(App.DbStores.logs, App.DbTransactionModes.readwrite);
                batch.includedLogRecords.forEach(logRecord => {
                    logRecord.isUploaded = 1;
                    tx.store.put(logRecord);
                });
                await tx.done;
                await App.logDebug('updated isUploaded for the logRecords in the current batch');
            }
        }
        await App.logInfo('uploadLogs: finished uploading logs');
        return true;
    //} finally {
    //    //unlockUploadLogsMutex();
    //}
}

App.uploadChangesQueue = uploadChangesQueue;
async function uploadChangesQueue(queueName) {
    queueName = queueName || App.DbStores.lightChangesQueue;
    await App.logInfo('entered uploadChangesQueue for ' + queueName);
    if (!await App.getConfigOnlyFromDb()) {
        throw 'getConfigOnlyFromDb failed';
    }
    let db = await getDb();
    let queueItems = await db.getAll(queueName);
    await App.logInfo(`${queueName} items count: ${queueItems.length}`);
    await App.logDebug('queue to upload', queueItems);
    for (let queueItem of queueItems) {
        await App.logDebug('sending queueItem', queueItem);
        try {
            var responseTmp = await postDataToApiNoAuth('/api/services/app/dispatching/ExecuteDriverApplicationAction', queueItem);
        } catch (e) {
            await App.logError('Error during uploadChangesQueue/postDataToApiNoAuth: ', 'change id: ' + queueItem.id, e);
            throw e;
        }
        await App.logDebug('received response', responseTmp);
        await db.delete(queueName, queueItem.id);
        await App.logDebug('deleted queueItem from the queue');
    }
    await App.logInfo('finished uploading queue ' + queueName);
    return true;
}

App.tryLockAndSyncDataWithApi = tryLockAndSyncDataWithApi;
async function tryLockAndSyncDataWithApi() {
    let unlockSyncMutex = null;
    try {
        unlockSyncMutex = await App.lockSyncMutex(false, 'for syncDataWithApi', App.Mutexes.lightQueue);
        if (!unlockSyncMutex) {
            return;
        }
        return await syncDataWithApi();
    }
    catch (e) {
        await App.logError('tryLockAndSyncDataWithApi: error during syncDataWithApi', e);
        return false;
    } finally {
        if (unlockSyncMutex) {
            await unlockSyncMutex();
        }
    }
}

//usually it is run from serviceworker, except for iOS
//it is being called often and should only upload light changes, large changes will be uploaded separately
App.syncDataWithApi = syncDataWithApi;
async function syncDataWithApi() {
    try {
        await App.logInfo('entered syncDataWithApi');
        await App.logInfo('syncDataWithApi: getting _config');
        if (!await App.getConfigOnlyFromDb()) {
            await logWarn('getConfigOnlyFromDb failed, skipping the changes upload');
            return {
                receivedDispatches: false
            };
        }
        await App.logInfo('syncDataWithApi: uploadChangesQueue(lightChangesQueue) started');
        await App.uploadChangesQueue(App.DbStores.lightChangesQueue);
        await App.logInfo('syncDataWithApi: getting _info');
        if (!await App.getConfigAndInfoFromDb()) {
            await App.logWarn('getConfigAndInfoFromDb failed, skipping getting the new changes');
            return {
                receivedDispatches: false
            };
        }
        await App.logInfo('syncDataWithApi: getting new dispatches');
        let newDispatches = await App.tryGetAndStoreNewDispatches(true);
        await App.logInfo(`syncDataWithApi: removing pendingIncomingChanges flag`);
        await App.updateSwInfo(swInfo => {
            swInfo.pendingIncomingChanges = false;
        });
        await App.logInfo('syncDataWithApi: Sync completed');
        return {
            //otherwise it would have thrown an exception on error, no need to check for length. 0 received dispatches shouldn't be considered an error (e.g. if other sync completed just before this push message is processed)
            receivedDispatches: true //newDispatches && newDispatches.length
        };
    }
    catch (e) {
        await App.logError('error during syncDataWithApi', e);
        throw e;
        //return {
        //    receivedDispatches: false
        //};
    }
}

App.requireNotificationPermission = requireNotificationPermission;
async function requireNotificationPermission() {
    //require that notifications are allowed, unless not supported by browser
    if (!hasNotificationSupport()) {
        return;
    }
    await requestNotificationPermissionIfNeeded();
    await subscribeToPushNotificationsIfNeeded();
}

App.hasNotificationSupport = hasNotificationSupport;
function hasNotificationSupport() {
    if (simulateIosForDebug) {
        return false;
    }
    return 'PushManager' in window && 'Notification' in window;
}

async function requestNotificationPermissionIfNeeded() {
    if (!hasNotificationSupport()) {
        return;
    }

    let permissionState = await getNotificationPermissionState();
    if (permissionState === 'denied') {
        await App.logError("Notifications permission is still denied");
        await throwNotificationPermissionRequiredError();
    }
    if (permissionState !== 'granted') {
        await new Promise((resolve) => {
            showValidationError("In order to receive new dispatches we are using Push API, and for it to work you need to allow notifications for this website. Please allow notifications on the next popup.", "Permission is required", async () => resolve());
        });
        App.ui.setBusy('', ' ', true);
        permissionState = await askNotificationPermission();
        App.ui.clearBusy();
        if (permissionState === 'denied') {
            await App.logError("Notifications permission was denied");
            await throwNotificationPermissionRequiredError();
        }
    }
    if (permissionState !== 'granted') {
        await App.logError("Unexpected permission state " + permissionState);
        await throwNotificationPermissionRequiredError();
    }
}

async function subscribeToPushNotificationsIfNeeded() {
    if (!App._info.isDriver) {
        return;
    }
    if (!hasNotificationSupport()) {
        return;
    }
    if (App._info.pushSubscriptionString) {
        let pushSubscription = await getOrRequestPushSubscription();
        let pushSubscriptionString = JSON.stringify(pushSubscription);
        if (App._info.pushSubscriptionString === pushSubscriptionString) {
            return;
        }
        await App.enqueueSubscriptionRemoval();
        await App.enqueueSubscriptionStoring(pushSubscription);
    } else {
        await subscribeToPushNotifications();
    }
}

async function subscribeToPushNotifications() {
    try {
        App.ui.setBusy('Subscribing to push notifications');
        let pushSubscription = await getOrRequestPushSubscription();
        //await App.logDebug(JSON.parse(JSON.stringify(pushSubscription)));

        App.ui.setBusy('Saving the push subscription');
        await App.enqueueSubscriptionStoring(pushSubscription);
    }
    catch (e) {
        await App.logError('subscribeToPushNotifications exception: ', e);
        await App.ui.showFatalError('Unable to subscribe to push notifications');
        throw e;
    }
    finally {
        App.ui.clearBusy();
    }
}

App.enqueueSubscriptionStoring = async function (pushSubscription) {
    let pushSubscriptionString = JSON.stringify(pushSubscription);
    let db = await getDb();
    App._info = await db.get(App.DbStores.info, 1);
    App._info.pushSubscriptionString = pushSubscriptionString;
    await db.put(App.DbStores.info, App._info);
    await App.enqueueChange({
        action: App.Enums.DriverApplicationAction.saveDriverPushSubscription,
        pushSubscriptionData: {
            //do not use pushSubscription variable directly, the value has to be stringified and parsed back
            pushSubscription: JSON.parse(App._info.pushSubscriptionString)
        }
    }, App.DbStores.lightChangesQueue, false);
};

App.enqueueSubscriptionRemoval = enqueueSubscriptionRemoval;
async function enqueueSubscriptionRemoval() {
    if (App._info.pushSubscriptionString && App.getDriverGuidOrNull(App._info)) {
        //App.ui.setBusy('Unsubscribing from push notifications');
        await App.enqueueChangeOnly({
            action: App.Enums.DriverApplicationAction.removeDriverPushSubscription,
            pushSubscriptionData: {
                pushSubscription: JSON.parse(App._info.pushSubscriptionString)
            }
        }, App.DbStores.lightChangesQueue, false);
    }
}

function getNotificationPermissionState() {
    if (!hasNotificationSupport()) {
        return Promise.resolve(false);
    }

    if (navigator.permissions) {
        return navigator.permissions.query({ name: 'notifications' })
            .then((result) => {
                return result.state;
            });
    }

    return new Promise((resolve) => {
        resolve(Notification.permission);
    });
}

async function throwNotificationPermissionRequiredError() {
    await App.ui.showFatalError("You have to allow these notifications manually in the settings of your browser", "Notifications permission is required", true, 'icon-notification1');
    throw new Error("Notifications permission is denied");
}

function askNotificationPermission() {
    return new Promise(function (resolve, reject) {
        if (!hasNotificationSupport()) {
            reject();
        }
        const permissionResult = Notification.requestPermission(function (result) {
            resolve(result);
        });

        if (permissionResult) {
            permissionResult.then(resolve, reject);
        }
    });
}

App.getOrRequestPushSubscription = getOrRequestPushSubscription;
function getOrRequestPushSubscription() {
    return getSWRegistration()
        .then(function (registration) {
            const subscribeOptions = {
                userVisibleOnly: true,
                applicationServerKey: urlBase64ToUint8Array(App._config.webPushServerPublicKey)
            };

            return registration.pushManager.subscribe(subscribeOptions);
        })
        .then(async function (pushSubscription) {
            await App.logDebug('Received PushSubscription: ', JSON.stringify(pushSubscription));
            return pushSubscription;
        });
}

function getSWRegistration() {
    return navigator.serviceWorker.ready;
}

function urlBase64ToUint8Array(base64String) {
    const padding = '='.repeat((4 - base64String.length % 4) % 4);
    const base64 = (base64String + padding)
        .replace(/\-/g, '+')
        .replace(/_/g, '/');

    const rawData = atob(base64);
    const outputArray = new Uint8Array(rawData.length);

    for (let i = 0; i < rawData.length; ++i) {
        outputArray[i] = rawData.charCodeAt(i);
    }
    return outputArray;
}

function uuidv4() {
    return ([1e7] + -1e3 + -4e3 + -8e3 + -1e11).replace(/[018]/g, c =>
        (c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> c / 4).toString(16)
    );
}

App.round = round;
function round(num) {
    num = Number(num);
    if (isNaN(num))
        return null;
    return Math.round((num + 0.00001) * 100) / 100;
}


App.ui = App.ui || {};
App.ui.setBusy = function (description, title, hideSpinner) {
    $('body').addClass('wait-popup-open');
    $('.wait-popup .description').text(description || '');
    $('.wait-popup .title').text(title || 'Please wait');
    $('.wait-popup .icon-spinner')[hideSpinner ? "hide" : "show"]();
    $('.wait-popup').show();
};

App.ui.clearBusy = function () {
    $('body').removeClass('wait-popup-open');
    $('.wait-popup').hide();
};

if (typeof $ !== 'undefined') {
    $('.error-popup .reload').click(() => location.reload());
}

App.ui.showFatalError = async function (description, title, hideReload, iconClass) {
    if (!description && !title && $('body').hasClass('error-popup-open')) {
        return;
    }
    await App.tryLockAndUploadLogs();
    if (iconClass) {
        $('.error-popup p.icon').replaceWith($('<p>').addClass("icon " + iconClass));
    }
    $('body').addClass('error-popup-open');
    $('.error-popup .description').text(description || '');
    $('.error-popup .title').text(title || 'An error occurred');
    $('.error-popup').show();
    $('.error-popup .reload')[hideReload ? "hide" : "show"]();
};

if (typeof $ !== 'undefined') {
    let confirmationPopupPromiseResolve = null;
    App.ui.confirm = async function (message, yesCaption = "Yes", noCaption = "Cancel", title = "Are you sure?") {
        let oldConfirmationPromiseResolve = confirmationPopupPromiseResolve;
        let oldConfirmationText = $('#confirmationPopupMessage').text();
        let yesButton = $('#confirmationPopup #confirmationPopupYesButton');
        let noButton = $('#confirmationPopup #confirmationPopupNoButton');
        let titleElement = $('#confirmationPopup .modal-title');
        let oldYesCaption = yesButton.text();
        let oldNoCaption = noButton.text();
        let oldTitle = titleElement.text();
        yesButton.text(yesCaption);
        noButton.text(noCaption);
        titleElement.text(title);
        return new Promise((resolve) => {
            confirmationPopupPromiseResolve = resolve;
            $('#confirmationPopupMessage').text(message);
            $('#confirmationPopup').modal('show');
        }).then(value => {
            if (oldConfirmationPromiseResolve) {
                setTimeout(() => {
                    confirmationPopupPromiseResolve = oldConfirmationPromiseResolve;
                    $('#confirmationPopupMessage').text(oldConfirmationText);
                    yesButton.text(oldYesCaption);
                    noButton.text(oldNoCaption);
                    titleElement.text(oldTitle);
                    $('#confirmationPopup').modal('show');
                }, 500);
            }
            return value;
        });
    };

    $('#confirmationPopup #confirmationPopupYesButton').click(async function () {
        if (confirmationPopupPromiseResolve) {
            let resolve = confirmationPopupPromiseResolve;
            confirmationPopupPromiseResolve = null;
            resolve(true);
        }
        $('#confirmationPopup').modal('hide');
    });
    $('#confirmationPopup #confirmationPopupNoButton').click(async function () {
        if (confirmationPopupPromiseResolve) {
            let resolve = confirmationPopupPromiseResolve;
            confirmationPopupPromiseResolve = null;
            resolve(false);
        }
    });
    $('#confirmationPopup').on('hidden.bs.modal', async function () {
        if (confirmationPopupPromiseResolve) {
            let resolve = confirmationPopupPromiseResolve;
            confirmationPopupPromiseResolve = null;
            resolve(null);
        }
    });
} else {
    App.ui.confirm = function () {
        throw new Error("App.ui.confirm should only be called from the ui actions and not from the functions used by service worker");
    };
}

App.logError = logError;
async function logError() {
    try {
        console.error.apply(this, arguments);
        let message = getLogMessageFromArguments(arguments);
        await storeLogRecord(message, 'error');
        //we'll run it on a fatal error instead
        //await App.tryLockAndUploadLogs();
    } catch (e) {
        console.error('error during logError', e);
    }
}

App.logInfo = logInfo;
async function logInfo() {
    try {
        console.log.apply(this, arguments);
        let message = getLogMessageFromArguments(arguments);
        await storeLogRecord(message, 'info');
    } catch (e) {
        console.error('error during logInfo', e);
    }
}

App.logWarn = logWarn;
async function logWarn() {
    try {
        console.warn.apply(this, arguments);
        let message = getLogMessageFromArguments(arguments);
        await storeLogRecord(message, 'warn');
    } catch (e) {
        console.error('error during logWarn', e);
    }
}

App.logDebug = logDebug;
async function logDebug() {
    console.log.apply(this, arguments);
}

function getAllPropertyNames(obj) {
    var props = [];
    do {
        Object.getOwnPropertyNames(obj).forEach(function (prop) {
            if (props.indexOf(prop) === -1) {
                props.push(prop);
            }
        });
        obj = Object.getPrototypeOf(obj);
        if (obj && obj.constructor && obj.constructor.name === 'Event') {
            obj = null;
        }
    } while (obj);

    return props;
}

function getLogMessageFromArguments(args) {
    let messageLines = [];
    for (let i = 0; i < args.length; i++) {
        if (typeof args[i] === 'string') {
            messageLines.push(args[i]);
        } else if (args[i] && typeof args[i] === 'object') {
            if (args[i] instanceof DOMException) {
                messageLines.push(JSON.stringify(args[i].toString()));
            } else if (args[i] instanceof Error || args[i] instanceof ErrorEvent || Object.getPrototypeOf(args[i])) {
                messageLines.push(JSON.stringify(args[i], getAllPropertyNames(args[i])));
            } else {
                messageLines.push(JSON.stringify(args[i], Object.getOwnPropertyNames(args[i])));
            }
        } else {
            messageLines.push(JSON.stringify(args[i]));
        }
    }
    return messageLines.join('\n');
}

async function storeLogRecord(message, level) {
    let db = await getDb();
    if (!App._info) {
        App._info = await db.get(App.DbStores.info, 1);
    }
    let sw = typeof window === 'undefined';
    let logRecord = {
        datetime: dayjs().utc().format(App.Consts.DateTimeFormat),
        message: message,
        level: level,
        isUploaded: 0,
        driverGuid: App.getDriverGuidOrNull(App._info),
        sw: sw
    };
    await db.add(App.DbStores.logs, logRecord);
}

App.deleteOldLogs = deleteOldLogs;
async function deleteOldLogs() {
    try {
        let endDate = dayjs().add(-3, 'days').utc().format(App.Consts.DateTimeFormat);
        await App.logInfo('entered deleteOldLogs, deleting records older than ' + endDate);
        let dateRange = IDBKeyRange.upperBound(endDate);

        let db = await App.getDb();
        let tx = db.transaction(App.DbStores.logs, App.DbTransactionModes.readwrite);
        let logStore = tx.objectStore(App.DbStores.logs);
        //let cursor = await logStore.index('datetime').openCursor(dateRange);
        let cursor = await logStore.index('datetime').openKeyCursor(dateRange);
        let i = 0;
        while (cursor) {
            i++;
            //await cursor.delete();
            await logStore.delete(cursor.primaryKey);
            cursor = await cursor.continue();
        }
        await tx.done;
        await App.logInfo(`deleted ${i} old log records`);
    }
    catch (e) {
        await App.logError('deleteOldLogs error: ', e);
    }
}

App.isEmptyGuid = isEmptyGuid;
function isEmptyGuid(guid) {
    return !guid || guid === '00000000-0000-0000-0000-000000000000';
}

App.getDriverGuidOrNull = getDriverGuidOrNull;
function getDriverGuidOrNull(info) {
    return info && !App.isEmptyGuid(info.driverGuid) ? info.driverGuid : null;
}

App.addSyncTagSuffix = addSyncTagSuffix;
function addSyncTagSuffix(tag) {
    let moment = dayjs();
    let minutesQuarter = Math.floor(moment.minute() / 15) * 15;
    moment = moment.minute(minutesQuarter);
    return tag + '#' + moment.format('YYYY-MM-DDTHH:mm');
}

App.getSyncTagWithoutSuffix = getSyncTagWithoutSuffix;
function getSyncTagWithoutSuffix(tag) {
    return (tag || "").split('#')[0];
}

App.registerSyncEvent = registerSyncEvent;
async function registerSyncEvent(swRegistration, tag) {
    if (swRegistration && swRegistration.sync && swRegistration.sync.register) {
        //await swRegistration.sync.register(App.addSyncTagSuffix(tag));
        await swRegistration.sync.register(tag);
    }
}

App.lockSyncMutex = async function (retryUntilLocked = true, comment = '', mutexName = 'common') {
    let lockGuid = uuidv4();
    await App.logInfo(`lockSyncMutex: entered and trying to lock ${lockGuid};${mutexName}Mutex ${comment}`);

    let overwrittenLockGuid = null;
    let overwrittenLockDate = null;
    let tryToLock = async function () {
        let updated = false;
        await App.updateSwInfo(swInfo => {
            if (App.isSyncMutexLocked(swInfo, mutexName)) {
                return;
            }
            //old values are for log purposes only
            overwrittenLockGuid = swInfo[mutexName + 'MutexGuid'];
            overwrittenLockDate = swInfo[mutexName + 'MutexDate'];
            swInfo[`is${mutexName}MutexLocked`] = true;
            swInfo[mutexName + 'MutexGuid'] = lockGuid;
            swInfo[mutexName + 'MutexDate'] = dayjs().format(App.Consts.DateTimeFormat);
            updated = true;
        });
        if (updated) {
            //await App.sleepAsync(200);
        }
        return await getSwInfo();
    };

    let isLocked = false;
    while (!isLocked) {
        let swInfo = await tryToLock();
        if (!swInfo[`is${mutexName}MutexLocked`]) {
            //unexpected
            await App.logWarn(`lockSyncMutex: failed to lock ${lockGuid};${mutexName}Mutex, is${mutexName}MutexLocked is false, retrying`);
            //await App.sleepAsync(200);
            continue;
        }
        if (swInfo[mutexName + 'MutexGuid'] !== lockGuid) {
            //was locked or other thread locked simultaneously with us
            await App.logInfo(`lockSyncMutex: failed to lock ${lockGuid};${mutexName}Mutex, already locked with guid ${swInfo[mutexName + 'MutexGuid']}, ${(retryUntilLocked ? 'retrying in 10s' : 'returning false')}`);
            if (retryUntilLocked) {
                await App.sleepAsync(10 * 1000);
                continue;
            } else {
                return false;
            }
        }
        isLocked = true;
        if (overwrittenLockGuid || overwrittenLockDate) {
            await App.logWarn(`lockSyncMutex: overwrote other lock with guid ${overwrittenLockGuid} and date ${overwrittenLockDate}`);
        }
        await App.logInfo(`lockSyncMutex: locked successfully with guid ${lockGuid};${mutexName}Mutex and date ${swInfo[mutexName + 'MutexDate']}; ${comment}`);
        return async function unlockSyncMutex() {
            await App.logInfo(`lockSyncMutex: unlocking guid ${lockGuid};${mutexName}Mutex ${comment}`);
            let remainingLockGuidValue = null;
            await App.updateSwInfo(swInfo => {
                if (swInfo[mutexName + 'MutexGuid'] !== lockGuid) {
                    remainingLockGuidValue = swInfo[mutexName + 'MutexGuid'];
                    return;
                }
                swInfo[`is${mutexName}MutexLocked`] = false;
                swInfo[mutexName + 'MutexGuid'] = null;
                swInfo[mutexName + 'MutexDate'] = null;
            });
            if (remainingLockGuidValue) {
                await App.logWarn(`lockSyncMutex: unlocking skipped because stored guid ${remainingLockGuidValue} didn't match our ${lockGuid}`);
            }
        };
    }
};

App.isSyncMutexLocked = function (swInfo, mutexName = 'common') {
    if (!swInfo[`is${mutexName}MutexLocked`] || !swInfo[mutexName + 'MutexDate']) {
        return false;
    }

    //more than 10 minutes has passed since the lock
    if (dayjs(swInfo[mutexName + 'MutexDate'], App.Consts.DateTimeFormat).add(10, 'm') < dayjs()) {
        return false;
    }

    return true;
};

App.requestPageReload = async function requestPageReload() {
    await App.logWarn(`requesting page reload from SW`);
    await App.updateSwInfo(swInfo => {
        swInfo.requestPageReload = true;
    });
};

App.reloadIfPendingReloadRequest = async function reloadIfPendingReloadRequest() {
    if (!(await App.getSwInfo()).requestPageReload) {
        return false;
    }
    await App.logWarn(`reloading page on request from SW`);
    await App.updateSwInfo(swInfo => {
        swInfo.requestPageReload = false;
    });
    App.ui.setBusy('Reloading the page to receive the new changes');
    setTimeout(() => location.reload(), 500);
    return true;
};

App.tryLogBatteryInfo = async function () {
    try {
        if ('getBattery' in navigator) {
            let battery = await navigator.getBattery();
            await App.logInfo(`Battery: charging: ${battery.charging}, level: ${battery.level}, chargingTime: ${battery.chargingTime}, dischargingTime: ${battery.dischargingTime}`);
        }
    } catch (e) {
        await App.logError('tryLogBattery error', e);
    }
};

//returns true if the event has been chosen as a main one and should run
App.mergePushEvent = async function (eventData) {
    if (!eventData || !eventData.Guid) {
        return false;
    }
    let db = await App.getDb();
    let ourQueueId = await db.add(App.DbStores.pushEventQueue, {
        guid: eventData.Guid
    });
    await App.logInfo(`mergePushEvent: ${eventData.Guid} enqueued as ${ourQueueId}`);
    await App.sleepAsync(3 * 1000);
    let allEvents = await db.getAll(App.DbStores.pushEventQueue);
    let allIds = allEvents.map(x => x.id);
    if (!allIds.length) {
        await App.logInfo(`mergePushEvent: ${eventData.Guid} merged with another, the queue was empty`);
        return false;
    }
    if (!allIds.includes(ourQueueId)) {
        await App.logInfo(`mergePushEvent: ${eventData.Guid} merged with another, ${ourQueueId} wasn't found in the queue`);
        return false;
    }
    let maxQueueId = allIds.reduce((prev, cur) => prev > cur ? prev : cur, ourQueueId);
    if (maxQueueId > ourQueueId) {
        await App.logInfo(`mergePushEvent: ${eventData.Guid} merged with another, ${ourQueueId} is lower than ${maxQueueId}`);
        return false;
    }

    //now we chose ourself to run and can delete older records
    const tx = db.transaction(App.DbStores.pushEventQueue, App.DbTransactionModes.readwrite);
    allIds.forEach(id => {
        tx.store.delete(id);
    });
    await tx.done;

    await App.logInfo(`mergePushEvent: ${eventData.Guid} was chosen as a main push event with queueId ${ourQueueId}`);

    return true;
};

//App.clearPushEventQueue = async function () {
//    try {
//        let db = await App.getDb();
//        await db.clear(App.DbStores.pushEventQueue);
//    } catch (e) {
//        await App.logError('clearPushEventQueue error', e);
//    }
//};

App.invalidateSwDbCache = async function () {
    //send message to sw to reset cached db records
    App.sendMessageToSw({
        kind: 'invalidateDbCache'
    });
};

App.sendMessageToSw = function (message) {
    let controller = navigator.serviceWorker && navigator.serviceWorker.controller;
    if (controller) {
        controller.postMessage(message);
    }
};

App.sendMessageToClientsAsync = async function (message) {
    if (self.clients) {
        let clients = await self.clients.matchAll();
        clients.forEach(c => {
            c.postMessage(message);
        });
    }
};

App.saveFileFromMemory = function (filename, data) {
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

App.updateServiceWorker = async function(setBusy) {
    try {
        let sw = await getSWRegistration();
        if (sw) {
            if (setBusy) {
                App.ui.setBusy('Checking if an update is available');
            }
            await sw.update();
        }
    } catch (e) {
        await App.logError('updateServiceWorker: failed: ', e);
    }
}