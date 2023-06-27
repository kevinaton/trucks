'use strict';
//import { App } from './js/site.js';
// Update cache names any time any of the cached files change.
const CACHE_NAME = 'static-cache-v139';
const LIBS_CACHE_NAME = 'static-libs-cache-v12';
const DATA_CACHE_NAME = 'data-cache-v5';
const CACHE_NAMES = [CACHE_NAME, LIBS_CACHE_NAME, DATA_CACHE_NAME];
const DEBUG_SERVICE_WORKER = false;
self.importScripts('./bundles/swLibs.min.js', './js/site.js');

//List of files to cache
const FILES_TO_CACHE = [
    '/',
    '/views/driverApplication/index.js',
    '/js/site.js',
    '/bundles/all.min.css',
    '/css/fonts/icomoon.ttf',
    '/Authentication/Callback',
    '/Authentication/SignoutCallback',
    '/Authentication/LoggedOut',
    '/Authentication/SilentCallback'
];

const LIBS_TO_CACHE = [
    '/favicon.ico',
    '/images/app-logo-512x512.png',
    '/images/app-logo-dump-truck-130x35.gif',
    '/images/apple-touch-icon-57x57.png',
    '/images/apple-touch-icon-60x60.png',
    '/images/apple-touch-icon-72x72.png',
    '/images/apple-touch-icon-76x76.png',
    '/images/apple-touch-icon-114x114.png',
    '/images/apple-touch-icon-120x120.png',
    '/images/apple-touch-icon-144x144.png',
    '/images/apple-touch-icon-152x152.png',
    '/images/favicon-16x16.png',
    '/images/favicon-32x32.png',
    '/images/favicon-96x96.png',
    '/images/favicon-128.png',
    '/images/favicon-192x192.png',
    '/images/favicon-196x196.png',
    '/images/install.svg',
    '/images/mstile-70x70.png',
    '/images/mstile-144x144.png',
    '/images/mstile-150x150.png',
    '/images/mstile-310x150.png',
    '/images/mstile-310x310.png',
    '/images/refresh.svg',
    '/images/logo-dtd.png',
    '/manifest.json',
    '/bundles/allLibs.min.js',
    '/bundles/swLibs.min.js',
    '/lib/oidc-client/oidc-client.min.js'
];

async function preCacheFiles() {
    let cache = await caches.open(CACHE_NAME);
    await App.logInfo('[ServiceWorker] Pre-caching pages ' + CACHE_NAME);
    await cache.addAll(FILES_TO_CACHE);
    await App.logInfo('[ServiceWorker] Pre-caching pages is complete');
    
    let libsCache = await caches.open(LIBS_CACHE_NAME);
    await App.logInfo('[ServiceWorker] Pre-caching libs ' + LIBS_CACHE_NAME);
    await libsCache.addAll(LIBS_TO_CACHE);
    await App.logInfo('[ServiceWorker] Pre-caching libs is complete');

    await App.requestPageReload();
}

self.addEventListener('error', async function (e) {
    if (typeof App !== 'undefined') {
        await App.logError('[ServiceWorker] error: ', e);
    }
});

self.addEventListener('unhandledrejection', async function (e) {
    if (typeof App !== 'undefined') {
        await App.logError('[ServiceWorker] unhandled rejection: ', e.reason && e.reason.message, e);
    }
});

// Cache the files
self.addEventListener('install', (evt) => {
    evt.waitUntil(async function () {
        await App.logInfo('[ServiceWorker] Install');
        await preCacheFiles();
    }());

    self.skipWaiting();
});

// Remove old cached data and files on the activate event
self.addEventListener('activate', (evt) => {
    evt.waitUntil(async function () {
        await App.logInfo('[ServiceWorker] Activate');
        let keyList = await caches.keys();
        for (let key of keyList) {
            if (!CACHE_NAMES.includes(key)) {
                await App.logInfo('[ServiceWorker] Removing old cache ' + key);
                await caches.delete(key);
            } else {
                await App.logInfo('[ServiceWorker] Keeping cache ' + key);
            }
        }
    }());

    self.clients.claim();
});

self.addEventListener('fetch', (evt) => {

    if (evt.request.url.includes('/api/')) {
        if (DEBUG_SERVICE_WORKER) {
            console.log('[Service Worker] Fetch (data)', evt.request.url);
        }
        evt.respondWith(fetch(evt.request));
        //evt.respondWith(
        //    caches.open(DATA_CACHE_NAME).then((cache) => {
        //        return fetch(evt.request)
        //            .then((response) => {
        //                // If the response was good, clone it and store it in the cache.
        //                if (response.status === 200) {
        //                    cache.put(evt.request.url, response.clone());
        //                }
        //                return response;
        //            }).catch((err) => {
        //                // Network request failed, try to get it from the cache.
        //                return cache.match(evt.request);
        //            });
        //    }));
        return;
    }
    // Fetch files from the cache
    evt.respondWith(async function() {
        let fileCache = await caches.open(CACHE_NAME);
        let response = await fileCache.match(evt.request);
        if (!response) {
            let libCache = await caches.open(LIBS_CACHE_NAME);
            response = await libCache.match(evt.request);
        }
        if (!response || DEBUG_SERVICE_WORKER) {
            console.log('[Service Worker] Fetch (' + (response ? 'Serving from Cache' : 'Serving from Fetch') + ')', evt.request.url);
        }
        return response || fetch(evt.request);
    }());

    return;
});

self.addEventListener('sync', function (event) {
    event.waitUntil(async function () {
        let unlockSyncMutex = null;
        try {
            await App.logInfo(`[ServiceWorker] received sync event ${event.tag}, lastChance: ${event.lastChance}`);
            await App.tryLogBatteryInfo();
            
            let tag = App.getSyncTagWithoutSuffix(event.tag);
            switch (tag) {
                case 'syncDataWithApi':
                    unlockSyncMutex = await App.lockSyncMutex(true, `for sync event ${event.tag}`, App.Mutexes.lightQueue);
                    await App.logInfo(`[ServiceWorker] locked mutex for ${event.tag}`);
                    await App.syncDataWithApi();
                    break;
                case 'uploadLightChangesQueue':
                    unlockSyncMutex = await App.lockSyncMutex(true, `for sync event ${event.tag}`, App.Mutexes.lightQueue);
                    await App.logInfo(`[ServiceWorker] locked mutex for ${event.tag}`);
                    await App.uploadChangesQueue(App.DbStores.lightChangesQueue);
                    break;
                case 'uploadLargeChangesQueue':
                    unlockSyncMutex = await App.lockSyncMutex(true, `for sync event ${event.tag}`, App.Mutexes.largeQueue);
                    await App.logInfo(`[ServiceWorker] locked mutex for ${event.tag}`);
                    await App.uploadChangesQueue(App.DbStores.largeChangesQueue);
                    break;
                case 'uploadLogs':
                    unlockSyncMutex = await App.lockSyncMutex(true, `for sync event ${event.tag}`, App.Mutexes.logs);
                    await App.logInfo(`[ServiceWorker] locked mutex for ${event.tag}`);
                    await App.uploadLogs();
                    break;
            }
            await App.logInfo(`[ServiceWorker] completed sync event ${event.tag}`);
        } catch (e) {
            await App.logError('[ServiceWorker] unhandled exception during sync event handling', e);
            throw e;
        } finally {
            if (unlockSyncMutex) {
                await unlockSyncMutex();
            }
        }
    }());
});

self.addEventListener('push', function (event) {
    if (event.data) {
        console.log('[ServiceWorker] This push event has data: ', event.data.json());
    } else {
        console.log('[ServiceWorker] This push event has no data.');
    }
    let eventData = event.data && event.data.json();
    if (eventData && (eventData.Action === App.Enums.PushAction.sync || eventData.Action === App.Enums.PushAction.silentSync)) {
        event.waitUntil(async function () {
            let unlockSyncMutex = null;
            let message = 'Couldn\'t receive incoming dispatches';
            try {
                await App.logInfo('[ServiceWorker] Received push event ' + eventData.Guid);
                if (!await App.mergePushEvent(eventData)) {
                    message = 'Receiving dispatches updates';
                    return;
                }
                await App.updateSwInfo(swInfo => {
                    swInfo.pendingIncomingChanges = true;
                });
                await App.tryLogBatteryInfo();
                await App.logInfo(`[ServiceWorker] For push ${eventData.Guid}: Set pendingIncomingChanges flag and trying to run sync`);

                var waitForMutexAndTryCallbackAsync = async function (mutexName, asyncCallback) {
                    try {
                        unlockSyncMutex = await App.lockSyncMutex(true, `for push ${eventData.Guid}`, mutexName);
                        await asyncCallback();
                    } catch (e) {
                        await App.logError(`[ServiceWorker] unhandled exception during push event ${eventData.Guid} handling`, e);
                    } finally {
                        if (unlockSyncMutex) {
                            await unlockSyncMutex();
                            unlockSyncMutex = null;
                        }
                    }
                };

                await waitForMutexAndTryCallbackAsync(App.Mutexes.lightQueue, async function () {
                    let syncResult = await App.syncDataWithApi();
                    let receivedDispatches = syncResult && syncResult.receivedDispatches;
                    if (receivedDispatches) {
                        message = /*eventData.Message ? eventData.Message :*/ 'Received dispatches updates';
                    } else {
                        await App.logError(`[ServiceWorker] For push ${eventData.Guid}: Couldn\'t receive incoming dispatches`);
                        //DOMException: Attempted to register a sync event without a window or registration tag too long.
                        //await App.registerSyncEvent(self.registration, 'syncDataWithApi');
                    }
                });

                await waitForMutexAndTryCallbackAsync(App.Mutexes.logs, async function () {
                    await App.logInfo(`[ServiceWorker] For push ${eventData.Guid}: uploading logs`);
                    await App.uploadLogs();
                });

                await waitForMutexAndTryCallbackAsync(App.Mutexes.largeQueue, async function () {
                    await App.logInfo(`[ServiceWorker] For push ${eventData.Guid}: uploading largeChangesQueue`);
                    await App.uploadChangesQueue(App.DbStores.largeChangesQueue);
                });

            } catch (e) {
                await App.logError(`[ServiceWorker] unhandled exception during push event ${eventData.Guid} handling`, e);
            } finally {
                if (unlockSyncMutex) {
                    await unlockSyncMutex();
                }
                await App.logInfo(`[ServiceWorker] For push ${eventData.Guid}: finished processing`);
                await self.registration.showNotification(message);
            }
        }());
    }
    //previous silent implementation that doesn't work when window is unavailable
    //else if (eventData && eventData.Action === App.Enums.PushAction.silentSync) {
    //    App.registerSyncEvent(self.registration, 'syncDataWithApi');
    //}
});

async function logSwScriptLoadedEvent() {
    if (typeof App !== 'undefined') {
        await App.logInfo('[ServiceWorker] script loaded');
    }
}
logSwScriptLoadedEvent();

self.addEventListener('message', async event => {
    console.log(`[ServiceWorker] received message: ${JSON.stringify(event.data)}`);
    if (!event.data.kind) {
        return;
    }
    switch (event.data.kind) {
        case 'invalidateDbCache': invalidateDbCache(); break;
    }
});

function invalidateDbCache() {
    if (App && App._info) {
        App._info = null;
    }
}