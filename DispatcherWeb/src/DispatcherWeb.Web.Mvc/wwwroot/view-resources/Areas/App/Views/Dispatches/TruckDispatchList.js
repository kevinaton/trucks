(function () {
    $(function () {
        var _dispatchingService = abp.services.app.dispatching;
        var _truckDispatches = [];

        var _viewDispatchModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Dispatches/ViewDispatchModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Dispatches/_ViewDispatchModal.js',
            modalClass: 'ViewDispatchModal'
        });

        var _duplicateDispatchMessageModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Dispatches/DuplicateDispatchMessageModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Dispatches/_DuplicateDispatchMessageModal.js',
            modalClass: 'DuplicateDispatchMessageModal'
        });

        var _setDispatchTimeOnJobModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Dispatches/SetDispatchTimeOnJobModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Dispatches/_SetDispatchTimeOnJobModal.js',
            modalClass: 'SetDispatchTimeOnJobModal'
        });

        if (abp.setting.getInt('App.DispatchingAndMessaging.DispatchVia') !== abp.enums.dispatchVia.driverApplication) {
            $('#DispatchListViewFilter option[value="1"]').remove();
        }

        var $viewFilterSelect = $("#DispatchListViewFilter").select2Init({
            showAll: true,
            allowClear: false
        });

        var $officeIdFilterSelect = $("#OfficeIdFilter").select2Init({
            abpServiceMethod: abp.services.app.office.getOfficesSelectList,
            showAll: true,
            allowClear: true
        });

        abp.helper.ui.addAndSetDropdownValue($officeIdFilterSelect, abp.session.officeId, abp.session.officeName);

        $("#RemoveDispatchesButton").tooltip();
        $("#RemoveDispatchesButton").click(async function (e) {
            e.preventDefault();
            if (await abp.message.confirm(
                app.localize('RemoveDispatchesPrompt')
            )) {
                _dispatchingService.removeAllDispatches().done(function () {
                    abp.notify.info('Successfully removed.');
                    waitForSyncOrReloadList();
                });
            }
        });

        app.localStorage.getItem('truckdispatchlist_filter', function (result) {
            var filter = result || {};

            if (filter.officeId) {
                abp.helper.ui.addAndSetDropdownValue($officeIdFilterSelect, filter.officeId, filter.officeName);
            }
            if (filter.view) {
                if (abp.setting.getInt('App.DispatchingAndMessaging.DispatchVia') !== abp.enums.dispatchVia.driverApplication && filter.view === "1") {
                    filter.view = "0";
                    filter.viewName = "Open dispatches";
                }
                abp.helper.ui.addAndSetDropdownValue($viewFilterSelect, filter.view, filter.viewName);
            }
            reloadList();

            var filterControls = $officeIdFilterSelect.add($viewFilterSelect);
            filterControls.on('change', function () {
                cacheFilterData();
                reloadList();
            });

        });

        function cacheFilterData() {
            var officeData = $officeIdFilterSelect.select2('data');
            var viewData = $viewFilterSelect.select2('data');
            var filter = {
                officeId: officeData[0].id,
                officeName: officeData[0].text,
                view: viewData[0].id,
                viewName: viewData[0].text
            };
            app.localStorage.setItem('truckdispatchlist_filter', filter);
        }

        function getTruckDispatches(additionalFilter) {
            var filter = {
                officeId: $officeIdFilterSelect.val() || null,
                view: $viewFilterSelect.val()
            };
            $.extend(filter, additionalFilter);
            return _dispatchingService.truckDispatchList(filter);
        }

        var $dispatchList = $('#DispatchList');
        function reloadList() {
            abp.ui.setBusy($dispatchList);
            getTruckDispatches().then(truckDispatches => {
                _truckDispatches = truckDispatches;
                abp.ui.clearBusy($dispatchList);
                renderDispatchList();
            }).fail(function () {
                abp.ui.clearBusy($dispatchList);
                abp.notify.error('Error detail not sent by server.', 'An error has occurred!');
            });
        }

        function reloadUpdatedDispatches(syncRequest) {
            console.log(syncRequest);
            let modifiedDispatchIds = syncRequest.changes.filter(c => c.entityType === abp.enums.entityEnum.dispatch && c.changeType === abp.enums.changeType.modified).map(c => c.entity.id);
            let removedDispatchIds = syncRequest.changes.filter(c => c.entityType === abp.enums.entityEnum.dispatch && c.changeType === abp.enums.changeType.removed).map(c => c.entity.id);
            let modifiedClockInUserIds = syncRequest.changes.filter(c => c.entityType === abp.enums.entityEnum.employeeTime).map(c => c.entity.userId);
            let modifiedDriverAssignmentTruckIds = syncRequest.changes.filter(c => c.entityType === abp.enums.entityEnum.driverAssignment).map(c => c.entity.truckId);

            if (removedDispatchIds.length) {
                console.log('Removed dispatch ids: ' + removedDispatchIds.join());
                _truckDispatches.forEach(truckDispatch => {
                    truckDispatch.dispatches = truckDispatch.dispatches.filter(d => !removedDispatchIds.includes(d.id));
                });
                renderDispatchList();
            }
            if (modifiedDispatchIds.length) {
                //reload only part of the list
                getTruckDispatches({
                    dispatchIds: modifiedDispatchIds
                }).then(modifiedTruckDispatches => {
                    console.log('Received modified truck dispatches', modifiedTruckDispatches);
                    var missingDispatchIds = modifiedDispatchIds.filter(id => !modifiedTruckDispatches.some(m => m.dispatches.some(d => d.id === id)));
                    if (missingDispatchIds.length) {
                        console.log('Removing missing dispatch ids: ' + missingDispatchIds.join());
                        _truckDispatches.forEach(truckDispatch => {
                            truckDispatch.dispatches = truckDispatch.dispatches.filter(d => !missingDispatchIds.includes(d.id));
                        });
                    }
                    modifiedTruckDispatches.forEach(modifiedTruckDispatch => {
                        let existingTruckDispatch = _truckDispatches.find(x => x.officeId === modifiedTruckDispatch.officeId && x.truckId === modifiedTruckDispatch.truckId && x.driverId === modifiedTruckDispatch.driverId);
                        if (!existingTruckDispatch) {
                            _truckDispatches.push(modifiedTruckDispatch);
                        } else {
                            let dispatches = existingTruckDispatch.dispatches
                                .filter(x => !modifiedDispatchIds.includes(x.id))
                                .concat(modifiedTruckDispatch.dispatches);
                            sortDispatches(dispatches);
                            //todo we cannot merge until the issue with warnings is fixed
                            //$.extend(existingTruckDispatch, modifiedTruckDispatch, { dispatches });
                            //temp:
                            existingTruckDispatch.dispatches = dispatches;
                        }
                    });
                    renderDispatchList();
                });
            }
            if (modifiedClockInUserIds.length) {
                _dispatchingService.getTruckDispatchClockInInfo({
                    userIds: modifiedClockInUserIds
                }).then(clockInInfoList => {
                    clockInInfoList.forEach(clockInInfo => {
                        let matchingTruckDispatches = _truckDispatches.filter(t => t.userId === clockInInfo.userId);
                        matchingTruckDispatches.forEach(truckDispatch => {
                            $.extend(truckDispatch, clockInInfo);
                        });
                    });
                    renderDispatchList();
                });
            }
            if (modifiedDriverAssignmentTruckIds.length) {
                _dispatchingService.getTruckDispatchDriverAssignmentInfo({
                    truckIds: modifiedDriverAssignmentTruckIds
                }).then(assignmentList => {
                    assignmentList.forEach(assignment => {
                        let matchingTruckDispatches = _truckDispatches.filter(t => t.truckId === assignment.truckId);
                        matchingTruckDispatches.forEach(truckDispatch => {
                            $.extend(truckDispatch, assignment);
                        });
                    });
                    renderDispatchList();
                });
            }
        }

        abp.event.on('app.duplicateDispatchMessageModalSaved', function () {
            waitForSyncOrReloadList();
        });

        abp.event.on('app.setDispatchTimeOnJobModalSaved', function (updatedDispatchData) {
            waitForSyncOrReloadList();
            _truckDispatches.forEach(truckDispatch => {
                let updatedDispatch = truckDispatch.dispatches.find(x => x.id == updatedDispatchData.id);
                if (!updatedDispatch) {
                    return;
                }
                if (updatedDispatch.timeOnJob !== updatedDispatchData.timeOnJob) {
                    updatedDispatch.timeOnJob = updatedDispatchData.timeOnJob;
                }
                let index = truckDispatch.dispatches.indexOf(updatedDispatch);
                if (index > 0 && !(areTwoTimesOnJobInOrderOrNull(truckDispatch.dispatches[index - 1], truckDispatch.dispatches[index]))
                    || index < truckDispatch.dispatches.length - 1 && !(areTwoTimesOnJobInOrderOrNull(truckDispatch.dispatches[index], truckDispatch.dispatches[index + 1]))
                ) {
                    abp.message.warn(app.localize('DispatchesTimeOnJobAreNotInOrderWarning'));
                }
            });
        });

        function areTwoTimesOnJobInOrderOrNull(dispatchOne, dispatchTwo) {
            if (!dispatchOne.timeOnJob || !dispatchTwo.timeOnJob) {
                return true;
            }
            if (moment(dispatchOne.timeOnJob) > moment(dispatchTwo.timeOnJob)) {
                return false;
            }
            return true;
        }

        $dispatchList.on('click', 'a.btnView', function (e) {
            e.preventDefault();
            let dispatch = $(this).closest('.dispatch');
            _viewDispatchModal.open({ id: dispatch.data('id') });
        });
        $dispatchList.on('click', 'a.btnDuplicate', function (e) {
            e.preventDefault();
            let dispatch = $(this).closest('.dispatch');
            _duplicateDispatchMessageModal.open({ id: dispatch.data('id') });
        });
        $dispatchList.on('click', 'a.btnEndMultipleLoadsDispatch', function (e) {
            e.preventDefault();
            let dispatch = $(this).closest('.dispatch');
            _dispatchingService.endMultipleLoadsDispatch(dispatch.data('id')).done(function () {
                abp.notify.info(app.localize('RunUntilStoppedEndedSuccessfully'));
                waitForSyncOrReloadList();
            });
        });
        $dispatchList.on('click', 'a.btnAddLoadInfo', function (e) {
            e.preventDefault();
            let dispatch = $(this).closest('.dispatch');
            var url = abp.appPath + 'app/acknowledge/' + dispatch.data('short-guid');
            window.open(url, '_blank');
        });
        $dispatchList.on('click', 'a.btnMarkAsDelivered', function (e) {
            e.preventDefault();
            let dispatch = $(this).closest('.dispatch');
            _dispatchingService.completeDispatch({ Guid: dispatch.data('guid') }).done(function () {
                abp.notify.info('Completed successfully.');
                waitForSyncOrReloadList();
            });
        });
        $dispatchList.on('click', 'a.btnCancel', function (e) {
            e.preventDefault();
            let dispatch = $(this).closest('.dispatch');
            var acknowledged = dispatch.data('acknowledged');
            var dispatchId = dispatch.data('id');
            abp.dispatches.cancel(acknowledged, dispatchId, function () {
                waitForSyncOrReloadList();
            });
        });
        $dispatchList.on('click', 'a.btnSendSyncRequest', function (e) {
            e.preventDefault();
            var driverId = $(this).closest('.dispatch').data('driverid');
            _dispatchingService.sendSilentSyncPushToDrivers({
                driverIds: [driverId]
            }).done(function (success) {
                if (success) {
                    abp.notify.success('Sync request successfully sent.');
                } else {
                    abp.notify.error('Sync request was unsuccessful.');
                }
            });
        });
        $dispatchList.on('click', 'a.btnChangeDispatchTimeOnJob', function (e) {
            e.preventDefault();
            let dispatch = $(this).closest('.dispatch');
            _setDispatchTimeOnJobModal.open({ id: dispatch.data('id') });
        });

        function initButtons() {

            $('.truck-dispatch-row').sortable({
                itemSelector: '.dispatch.draggable',
                containerSelector: '.truck-dispatch-row',
                exclude: '.kt-section__content',
                vertical: false,
                placeholder: '<div class="placeholder"></div>',
                pullPlaceholder: false,
                onDrop: function ($item, container, _super, event) {
                    $item.removeClass(container.group.options.draggedClass).removeAttr("style");
                    $("body").removeClass(container.group.options.bodyClass);
                    reorderTruckDispatches(container.el);
                },
                onMousedown: function ($item, _super, event) {
                    if (!event.target.nodeName.match(/^(input|select|textarea|button)$/i)
                        && $(event.target).parentsUntil('.dropdown', '.dropdown-menu, .btn').length === 0) {
                        event.preventDefault();
                        return true;
                    }
                }
            });
        }

        function reorderTruckDispatches(row) {
            var originalOrder = row.data('dispatches');
            var dispatches = row.find('.dispatch').toArray().map(x => $(x).data('id'));
            if (originalOrder === dispatches.join()) {
                return;
            }
            //console.log({
            //    originalOrder, dispatches: dispatches.join()
            //});
            abp.ui.setBusy($dispatchList);
            _dispatchingService.reorderDispatches({
                orderedDispatchIds: dispatches
            }).done(function () {
                row.data('dispatches', dispatches.join());
                abp.ui.clearBusy($dispatchList);
            }).fail(function () {
                abp.ui.clearBusy($dispatchList);
                reloadList();
            });
        }

        abp.event.on('abp.signalR.receivedSyncRequest', (syncRequest) => {
            reloadUpdatedDispatches(syncRequest);
        });
        abp.helper.signalr.startDispatcherConnection();

        function waitForSyncOrReloadList() {
            //the sync request is received before the action is complete, so there's no need to wait or reload
        }

        var dispatchListRenderedForDay = moment().startOf('day');
        setInterval(refreshWarnings, 60 * 1000);
        async function refreshWarnings() {
            if (!moment().startOf('day').isSame(dispatchListRenderedForDay)) {
                reloadList();
                return;
            }
            $dispatchList.find('.truck-dispatch-row').each((i, el) => {
                let truckDispatchRowDiv = $(el);
                let truckDispatch = truckDispatchRowDiv.data('truckDispatch');

                let warningContainerDiv = truckDispatchRowDiv.find('.truck-dispatch > .text-center');
                warningContainerDiv.find('.dispatch-error-icon').remove();
                warningContainerDiv.append(
                    renderTruckDispatchWarnings(truckDispatch)
                );
            });
        }

        function renderDispatchList() {
            $dispatchList.empty();
            let offices = _truckDispatches
                .map(x => ({ id: x.officeId, name: x.officeName }))
                .filter((value, index, array) => !array.some(x => value.id === x.id));
            let currentOfficeId = null;
            let thereAreDispatchesToDisplay = false;
            _truckDispatches.forEach(truckDispatch => {
                thereAreDispatchesToDisplay = true;
                sortDispatches(truckDispatch.dispatches);

                if (offices.length > 1 && currentOfficeId !== truckDispatch.officeId) {
                    $dispatchList.append(
                        $('<div class="row margin-top-10">').append(
                            $('<h3>').text(truckDispatch.officeName)
                        )
                    );
                }

                let truckDispatchDiv = $('<div class="row truck-dispatch-row">')
                    .data('dispatches', truckDispatch.dispatches.map(x => x.id).join())
                    .data('truckDispatch', truckDispatch);

                truckDispatchDiv.append(
                    $('<div class="truck-dispatch">').append(
                        $('<div class="text-center">').append(
                            $('<p class="mb-0">').text(truckDispatch.truckCode)
                        ).append(
                            $('<p class="mb-0">').text(truckDispatch.fullName)
                        ).append(
                            renderTruckDispatchWarnings(truckDispatch)
                        )
                    )
                );

                let firstActiveDispatch = true;
                truckDispatch.dispatches.forEach(dispatch => {
                    let dispatchDiv = $('<div class="p-3 dispatch">')
                        .addClass(getDispatchBoderColorClass(dispatch, truckDispatch.dispatches))
                        .addClass(dispatch.isDraggable ? 'draggable' : null)
                        .data('id', dispatch.id)
                        .data('short-guid', dispatch.shortGuid)
                        .data('guid', dispatch.guid)
                        .data('acknowledged', !!dispatch.acknowledged)
                        .data('driverid', truckDispatch.driverId);

                    let deliveryDate = moment(dispatch.deliveryDate).utc().format('l');
                    let timeOnJob = dispatch.timeOnJob ? moment(dispatch.timeOnJob).utc().format('LT') : '';
                    let shiftName = abp.helper.getShiftName(dispatch.shift);

                    dispatchDiv.append(
                        $('<div>').text(`${deliveryDate} ${timeOnJob} ${shiftName}`)
                    ).append(
                        $('<div>').text(dispatch.customerName)
                    ).append(
                        $('<div>').text(dispatch.loadAtName)
                    ).append(
                        $('<div>').text(dispatch.deliverToName)
                    ).append(
                        $('<div>').text(dispatch.item)
                    ).append(
                        $('<div>').text(renderUom(dispatch.materialUom, dispatch.freightUom))
                    ).append(
                        dispatch.isMultipleLoads
                            ? $('<div>').text(app.localize('RunUntilStopped'))
                            : $()
                    ).append(
                        $('<div class="status text-nowrap">')
                            .append($('<span>').text(dispatch.statusName + ' '))
                            .append($('<span class="align-right">').text(dispatch.statusTime))
                    ).append(
                        renderDispatchLinks(dispatch, firstActiveDispatch)
                    );

                    if ([
                        abp.enums.dispatchStatus.created,
                        abp.enums.dispatchStatus.sent,
                        abp.enums.dispatchStatus.acknowledged,
                        abp.enums.dispatchStatus.loaded
                    ].includes(dispatch.status)) {
                        firstActiveDispatch = false;
                    }

                    truckDispatchDiv.append(dispatchDiv);
                });
                $dispatchList.append(truckDispatchDiv);
            });

            if (!thereAreDispatchesToDisplay) {
                $dispatchList.append(
                    $('<div>').text('There are no dispatches to display')
                );
            }

            initButtons();
        }

        function renderUom(a, b) {
            let result = [];
            if (a) {
                result.push(a);
            }
            if (b && a !== b) {
                result.push(b);
            }
            return result.length ? result.join('/ ') : '';
        }

        function renderTruckDispatchWarnings(truckDispatch) {
            let showUnacknowledgedDispatchWarning = false;
            let showLateClockInWarning = false;
            let today = moment().startOf('day');
            let isTodayDispatch = (x) => moment(x.deliveryDate, ['YYYY-MM-DDTHH:mm:ss']).isSame(today);
            let hasDispatchesForToday = truckDispatch.dispatches.length && truckDispatch.dispatches.some(isTodayDispatch);
            let startTime = truckDispatch.startTime ? moment(truckDispatch.startTime, ['YYYY-MM-DDTHH:mm:ss']) : abp.helper.getDefaultStartTime();
            let isPastStartTime = startTime < moment();
            let firstTodaysDispatch = truckDispatch.dispatches.length && truckDispatch.dispatches.find(isTodayDispatch);
            if (firstTodaysDispatch) {
                let isDriverApplicationEnabled = abp.setting.getInt('App.DispatchingAndMessaging.DispatchVia') === abp.enums.dispatchVia.driverApplication;
                let isUnacknowledged = firstTodaysDispatch.status === abp.enums.dispatchStatus.created || firstTodaysDispatch.status === abp.enums.dispatchStatus.sent;

                showUnacknowledgedDispatchWarning = isUnacknowledged && isPastStartTime;
                showLateClockInWarning = hasDispatchesForToday && isDriverApplicationEnabled && !truckDispatch.hasClockedInToday && isPastStartTime;
            }

            let driverWarnings = $();
            if (showUnacknowledgedDispatchWarning) {
                driverWarnings = driverWarnings.add(
                    $('<i class="dispatch-error-icon fa fa-exclamation-triangle"></i>').attr('title', app.localize('UnacknowledgedDisaptchWarning'))
                );
            }
            if (showLateClockInWarning) {
                driverWarnings = driverWarnings.add(
                    $('<i class="dispatch-error-icon fa fa-clock"></i>').attr('title', app.localize('LateClockInWarning'))
                );
            }
            return driverWarnings;
        }

        function renderDispatchLinks(dispatch, firstActiveDispatch) {
            let linkContainer = $(`
<div class="kt-section__content kt-section__content--solid">
    <div class="dropdown">
        <button class="btn btn-primary btn-sm" type="button" id="dropdownMenu2" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
            <i class="fa fa-ellipsis-h"></i>
        </button>
        <div class="dropdown-menu" aria-labelledby="dropdownMenu">
        </div>
    </div>
</div>
`);
            var links = linkContainer.find('.dropdown-menu');
            var renderLink = function renderLink(className, text, iconClass) {
                return $('<a href="#" class="dropdown-item">')
                    .addClass(className)
                    .attr('title', text)
                    .text(text)
                    .prepend(iconClass ? $('<i>').addClass(iconClass) : $());
            };
            links.append(renderLink('btnView', app.localize('View'), 'fa fa-eye'));
            if (abp.auth.hasPermission('Pages.Dispatches.Edit')) {
                if (dispatch.status !== abp.enums.dispatchStatus.error) {
                    links.append(renderLink('btnDuplicate', app.localize('Copy'), 'fa fa-clone'));
                }
                if (dispatch.isMultipleLoads) {
                    links.append(renderLink('btnEndMultipleLoadsDispatch', app.localize('EndMultipleDispatches')));
                }
                if (firstActiveDispatch && (dispatch.status === abp.enums.dispatchStatus.created || dispatch.status === abp.enums.dispatchStatus.sent)) {
                    links.append(renderLink('btnAddLoadInfo', app.localize('Acknowledge')));
                }
                if (firstActiveDispatch && dispatch.status === abp.enums.dispatchStatus.acknowledged) {
                    links.append(renderLink('btnAddLoadInfo', app.localize('AddLoadInfo')));
                }
                if (firstActiveDispatch && (dispatch.status === abp.enums.dispatchStatus.loaded)) {
                    links.append(renderLink('btnMarkAsDelivered', app.localize('MarkAsDelivered')));
                }
                if (dispatch.status !== abp.enums.dispatchStatus.completed && dispatch.status !== abp.enums.dispatchStatus.canceled) {
                    links.append(renderLink('btnCancel', app.localize('Cancel'), 'fa fa-times'));
                }
            }
            if (firstActiveDispatch && abp.auth.hasPermission('Pages.Dispatches.SendSyncRequest')) {
                links.append(renderLink('btnSendSyncRequest', app.localize('SendSyncRequest'), 'fa fa-sync'));
            }
            links.append(renderLink('btnChangeDispatchTimeOnJob', app.localize('ChangeTimeOnJob'), 'fa fa-clock'));

            return linkContainer;
        }

        function sortDispatches(dispatches) {
            //.OrderByDescending(d => d.Status == DispatchStatus.Loaded)
            //.ThenByDescending(d => d.Status == DispatchStatus.Acknowledged)
            //.ThenBy(d => d.SortOrder)
            dispatches.sort((a, b) => a.sortOrder - b.sortOrder);
            dispatches.sort(getSortHandlerForDispatchStatus(abp.enums.dispatchStatus.acknowledged));
            dispatches.sort(getSortHandlerForDispatchStatus(abp.enums.dispatchStatus.loaded));
        }

        function getSortHandlerForDispatchStatus(dispatchStatus) {
            return (a, b) => {
                if (a.status === b.status) {
                    return 0;
                }
                if (a.status === dispatchStatus) {
                    return -1;
                }
                if (b.status == dispatchStatus) {
                    return 1;
                }
                return 0;
            };
        }

        function getDispatchBoderColorClass(dispatch, dispatches) {
            if (dispatch.status === abp.enums.dispatchStatus.loaded && dispatches.indexOf(dispatch) === dispatches.length - 1) {
                return 'last-loaded';
            }
            return null;
        }

    });
})();
