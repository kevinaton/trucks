(function () {
    $(function () {

        var _orderService = abp.services.app.order;
        var _schedulingService = abp.services.app.scheduling;
        var _truckService = abp.services.app.truck;
        var _dispatchingService = abp.services.app.dispatching;
        var _driverAssignmentService = abp.services.app.driverAssignment;
        var _dtHelper = abp.helper.dataTables;
        var _permissions = {
            edit: abp.auth.hasPermission('Pages.Orders.Edit'),
            print: abp.auth.hasPermission('Pages.PrintOrders'),
            editTickets: abp.auth.hasPermission('Pages.Tickets.Edit'),
            editQuotes: abp.auth.hasPermission('Pages.Quotes.Edit'),
            driverMessages: abp.auth.hasPermission('Pages.DriverMessages'),
            trucks: abp.auth.hasPermission('Pages.Trucks')
        };
        var _features = {
            allowSharedOrders: abp.features.isEnabled('App.AllowSharedOrdersFeature'),
            allowMultiOffice: abp.features.isEnabled('App.AllowMultiOfficeFeature'),
            allowSendingOrdersToDifferentTenant: abp.features.isEnabled('App.AllowSendingOrdersToDifferentTenant')
        };
        var _vehicleCategories = null;
        var _loadingState = false;
        var _trucksWereLoadedOnce = false;
        var _scheduleTrucks = [];
        var _driverAssignments = [];

        var isGeotabEnabled = abp.features.isEnabled('App.GeotabFeature');
        var isLeaseHaulerEnabled = abp.features.isEnabled('App.AllowLeaseHaulersFeature');
        var isSmsIntegrationEnabled = abp.features.isEnabled('App.SmsIntegrationFeature');
        var isDispatchViaGeotabEnabled = false;

        var dispatchVia = abp.setting.getInt('App.DispatchingAndMessaging.DispatchVia');
        var allowSmsMessages = abp.setting.getBoolean('App.DispatchingAndMessaging.AllowSmsMessages');
        var hasDispatchPermissions = abp.auth.hasPermission('Pages.Dispatches.Edit');
        var showDispatchItems = dispatchVia !== abp.enums.dispatchVia.none && hasDispatchPermissions;
        var showDispatchViaSmsItems = dispatchVia !== abp.enums.dispatchVia.none && hasDispatchPermissions;
        var showDispatchViaGeotabItems = isDispatchViaGeotabEnabled && hasDispatchPermissions;
        var showProgressColumn = dispatchVia === abp.enums.dispatchVia.driverApplication;

        var _addOrderTruckModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Scheduling/AddOrderTruckModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Scheduling/_AddOrderTruckModal.js',
            modalClass: 'AddOrderTruckModal'
        });

        var _setTruckUtilizationModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Scheduling/SetTruckUtilizationModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Scheduling/_SetTruckUtilizationModal.js',
            modalClass: 'SetTruckUtilizationModal',
            modalSize: 'sm'
        });

        var _changeOrderLineUtilizationModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Scheduling/ChangeOrderLineUtilizationModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Scheduling/_ChangeOrderLineUtilizationModal.js',
            modalClass: 'ChangeOrderLineUtilizationModal',
            modalSize: 'sm'
        });

        var _setOrderOfficeIdModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Scheduling/SetOrderOfficeIdModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Scheduling/_SetOrderOfficeIdModal.js',
            modalClass: 'SetOrderOfficeIdModal',
            modalSize: 'sm'
        });

        var _setOrderDateModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Scheduling/SetOrderDateModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Scheduling/_SetOrderDateModal.js',
            modalClass: 'SetOrderDateModal',
            modalSize: 'sm'
        });

        var _copyOrderModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Orders/CopyOrderModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_CopyOrderModal.js',
            modalClass: 'CopyOrderModal'
        });

        var _createQuoteFromOrderModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Orders/CreateQuoteFromOrderModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_CreateQuoteFromOrderModal.js',
            modalClass: 'CreateQuoteFromOrderModal'
        });

        var _shareOrderLineModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Orders/ShareOrderLineModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_ShareOrderLineModal.js',
            modalClass: 'ShareOrderLineModal',
            modalSize: 'sm'
        });

        var _sendOrderLineToHaulingCompanyModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Scheduling/SendOrderLineToHaulingCompanyModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Scheduling/_SendOrderLineToHaulingCompanyModal.js',
            modalClass: 'SendOrderLineToHaulingCompanyModal',
            modalSize: 'sm'
        });

        var _setNoDriverForTruckModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Orders/SetNoDriverForTruckModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_SetNoDriverForTruckModal.js',
            modalClass: 'SetNoDriverForTruckModal'
        });

        var _assignDriverForTruckModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Scheduling/AssignDriverForTruckModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Scheduling/_AssignDriverForTruckModal.js',
            modalClass: 'AssignDriverForTruckModal'
        });

        var _setDefaultDriverForTruckModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Orders/SetDefaultDriverForTruckModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_SetDefaultDriverForTruckModal.js',
            modalClass: 'SetDefaultDriverForTruckModal'
        });

        var _addSharedTruckModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Trucks/AddSharedTruckModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Trucks/_AddSharedTruckModal.js',
            modalClass: 'AddSharedTruckModal'
        });

        var _showTruckOrdersModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Scheduling/ShowTruckOrdersModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Scheduling/_ShowTruckOrdersModal.js',
            modalClass: 'ShowTruckOrdersModal',
            modalSize: 'lg'
        });

        var _addOutOfServiceReasonModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Trucks/AddOutOfServiceReasonModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Trucks/_AddOutOfServiceReasonModal.js',
            modalClass: 'AddOutOfServiceReasonModal'
        });

        var _tripsReportModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Scheduling/TripsReportModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Scheduling/_TripsReportModal.js',
            modalClass: 'TripsReportModal'
        });

        var _cycleTimesModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Scheduling/CycleTimesModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Scheduling/_CycleTimesModal.js',
            modalClass: 'CycleTimesModal',
            modalSize: 'lg'
        });

        var _sendDispatchMessageModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Scheduling/SendDispatchMessageModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Scheduling/_SendDispatchMessageModal.js',
            modalClass: 'SendDispatchMessageModal'
        });

        var _activateClosedTrucksModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Scheduling/ActivateClosedTrucksModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Scheduling/_ActivateClosedTrucksModal.js',
            modalClass: 'ActivateClosedTrucksModal'
        });

        var _sendDriverMessageModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/DriverMessages/SendMessageModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/DriverMessages/_SendMessageModal.js',
            modalClass: 'SendMessageModal'
        });

        var _jobSummaryModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Scheduling/JobSummaryModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Scheduling/_JobSummaryModal.js',
            modalClass: 'JobSummaryModal',
            modalId: 'JobSummaryModal'
        });

        var _createOrEditTicketModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Orders/CreateOrEditTicketModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_CreateOrEditTicketModal.js',
            modalClass: 'CreateOrEditTicketModal',
            modalSize: 'lg'
        });

        var _printOrderWithDeliveryInfoModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Orders/PrintOrderWithDeliveryInfoModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_PrintOrderWithDeliveryInfoModal.js',
            modalClass: 'PrintOrderWithDeliveryInfoModal'
        });

        var _assignTrucksModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Scheduling/AssignTrucksModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Scheduling/_AssignTrucksModal.js',
            modalClass: 'AssignTrucksModal',
            modalSize: 'lg'
        });

        var _changeDriverForOrderLineTruckModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Scheduling/ChangeDriverForOrderLineTruckModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Scheduling/_ChangeDriverForOrderLineTruckModal.js',
            modalClass: 'ChangeDriverForOrderLineTruckModal'
        });

        var _sendOrdersToDriversModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Orders/SendOrdersToDriversModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_SendOrdersToDriversModal.js',
            modalClass: 'SendOrdersToDriversModal',
            //modalSize: 'sm'
        });

        var _createOrEditJobModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Orders/CreateOrEditJobModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_CreateOrEditJobModal.js',
            modalClass: 'CreateOrEditJobModal',
            modalSize: 'lg'
        });

        var _specifyPrintOptionsModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Orders/SpecifyPrintOptionsModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_SpecifyPrintOptionsModal.js',
            modalClass: 'SpecifyPrintOptionsModal',
            modalSize: 'sm'
        });

        var _reassignTrucksModal = abp.helper.createModal('ReassignTrucks', 'Scheduling');

        var _createOrEditLeaseHaulerRequestModal = abp.helper.createModal('CreateOrEditLeaseHaulerRequest', 'LeaseHaulerRequests');

        $("#DateFilter").val(moment().format("MM/DD/YYYY"));
        //$("#DateFilter").val(moment("02/16/2017", "MM/DD/YYYY").format("MM/DD/YYYY")); //debug
        $("#DateFilter").datepicker();
        $("#OfficeIdFilter").select2Init({
            abpServiceMethod: abp.services.app.office.getOfficesSelectList,
            showAll: true,
            allowClear: false
        });
        abp.helper.ui.addAndSetDropdownValue($("#OfficeIdFilter"), abp.session.officeId, abp.session.officeName);
        $('#ShiftFilter').select2Init({ allowClear: false });

        refreshHideProgressBarCheckboxVisibility();
        refreshDateRelatedButtonsVisibility();

        $("#TruckTileChooseGroupingButton > .btn").click(function () {
            refreshView($(this));
        });
        function refreshView($button) {
            $button.addClass("active").siblings().removeClass("active");
            var truckTileGroupingCategory = $button.data('category');
            app.localStorage.setItem('truckTileGroupingCategory', truckTileGroupingCategory);
            if (truckTileGroupingCategory) {
                $('#TruckTiles').hide();
                if (_trucksWereLoadedOnce) {
                    $('#TruckTilesByCategory').show();
                } else {
                    $('#TruckTilesByCategory').hide();
                }
            } else {
                $('#TruckTiles').show();
                $('#TruckTilesByCategory').hide();
            }
        }
        function updateTruckTileGroupingContainerVisibilityFromCache() {
            app.localStorage.getItem('truckTileGroupingCategory', function (result) {
                var truckTileGroupingCategory = result || false;
                var button = $('#TruckTileChooseGroupingButton > .btn[data-category="' + truckTileGroupingCategory + '"]');
                refreshView(button);
            });
        }
        updateTruckTileGroupingContainerVisibilityFromCache();

        var truckTiles = $("#TruckTiles");
        async function reloadTruckTiles() {
            if (_vehicleCategories === null) {
                await loadVehicleCategoriesAsync();
            }
            var truckTilesByCategory = $('.schedule-truck-tiles[data-truck-category-id]');

            var options = {};
            $.extend(options, _dtHelper.getFilterData());
            _schedulingService.getScheduleTrucks(options).done(function (result) {
                var trucks = result.items;
                _scheduleTrucks = trucks;
                truckTiles.empty();
                truckTilesByCategory.empty();
                $.each(trucks, function (ind, truck) {
                    var tileWrapper = $('<div class="truck-tile-wrap">')
                        .addClass(getTruckTileOfficeClass(truck) ? '' : getTruckTileClass(truck)); //don't add the color class (getTruckTileClass) to the wrapper if the tile is office-specific (when getTruckTileOfficeClass(truck) != '')
                    $('<div class="truck-tile"></div>')
                        .data('truck', truck)
                        .addClass(getTruckTileClass(truck))
                        .addClass(getTruckTileOfficeClass(truck))
                        .addClass(getTruckTilePointerClass(truck))
                        .text(truck.truckCode)
                        .attr('title', getTruckTileTitle(truck))
                        .appendTo(tileWrapper);
                    tileWrapper
                        .appendTo(truckTiles);
                    var truckTilesContainer = truckTilesByCategory.closest(`[data-truck-category-id="${truck.vehicleCategory.id}"`);
                    tileWrapper.clone(true, true).appendTo(truckTilesContainer);
                });
                var tiles = $('#TruckTilesByCategory div.schedule-truck-tiles');
                $.each(tiles, function (ind, tile) {
                    let $truckGroup = $(tile).parents('div.m-accordion__item');
                    var $collapsableHeader = $truckGroup.children('div.m-accordion__item-head');
                    if ($(tile).has('div.truck-tile-wrap').length === 0) {
                        //$collapsableHeader.parent().remove();
                        $truckGroup.hide();
                        //$collapsableHeader.removeAttr('data-toggle');
                        //$collapsableHeader.find('span:nth-child(3)').remove();
                    } else {
                        $truckGroup.show();
                        var $dataToggleAttr = $collapsableHeader.attr('data-toggle');
                        if (typeof $dataToggleAttr !== typeof undefined && $dataToggleAttr !== false) {
                            return;
                        }
                        $collapsableHeader.attr('data-toggle', 'collapse');
                        $collapsableHeader.append('<span class="m-accordion__item-mode"></span>');
                    }
                });
                _trucksWereLoadedOnce = true;
                updateTruckTileGroupingContainerVisibilityFromCache();
            });
        }

        async function reloadDriverAssignments() {
            var result = await _driverAssignmentService.getAllDriverAssignmentsLite(_dtHelper.getFilterData());
            result.items.sort((a, b) => {
                if (a.driverLastName < b.driverLastName) return -1;
                if (a.driverLastName > b.driverLastName) return 1;
                if (a.driverFirstName < b.driverFirstName) return -1;
                if (a.driverFirstName > b.driverFirstName) return 1;
                if (a.truckCode < b.truckCode) return -1;
                if (a.truckCode > b.truckCode) return 1;
                return 0;
            });
            _driverAssignments = result.items;
        }

        async function loadVehicleCategoriesAsync() {
            _vehicleCategories = await _truckService.getVehicleCategories();
            renderVehicleCategories();
        }
        function renderVehicleCategories() {
            $('#TruckTilesByCategory').empty();
            _vehicleCategories.forEach((vehicleCategory, i) => {
                $('#TruckTilesByCategory').append(
                    $('<div class="m-accordion__item">').append(
                        $('<div class="m-accordion__item-head collapsed" role="tab" data-toggle="collapse" aria-expanded="false">')
                            .attr('id', `collapse${i}_head`)
                            .attr('href', `#collapse${i}`)
                            .append(
                                $('<span class="m-accordion__item-icon"></span>')
                            )
                            .append(
                                $('<span class="m-accordion__item-title"></span>').text(vehicleCategory.name + 's')
                            ).append(
                                $('<span class="m-accordion__item-mode"></span>')
                            )
                    ).append(
                        $('<div class="m-accordion__item-body collapse" role="tabpanel">')
                            .attr('id', `collapse${i}`)
                            .attr('aria-labelledby', `collapse${i}_head`)
                            .append(
                                $('<div class="m-accordion__item-content">')
                                    .append(
                                        $('<div class="schedule-truck-tiles truck-tiles-by-category"></div>')
                                            .attr('data-truck-category-id', vehicleCategory.id)
                                            .attr('id', `TruckTiles${vehicleCategory.id}`)
                                    )
                            )
                    )
                );
            });
        }

        var _truckOrdersModalOpening = false;
        $('.schedule-truck-tiles').on('click', 'div.truck-tile.hand', function () {
            if (_truckOrdersModalOpening) {
                return;
            }
            _truckOrdersModalOpening = true;
            var $truckDiv = $(this);
            var truckCode = $truckDiv.text();
            var truckId = $truckDiv.data('truck').id
            var filterData = _dtHelper.getFilterData();
            var date = filterData.date;
            _showTruckOrdersModal.open({ truckId: truckId, truckCode: truckCode, scheduleDate: date, shift: filterData.shift });
            _truckOrdersModalOpening = false;
        });

        function removeScheduleTruckFromArray(trucks, truck) {
            var index = trucks.indexOf(truck);
            if (index !== -1) {
                trucks.splice(index, 1);
            }
        }

        function orderHasTruck(order, truck) {
            return order.trucks.some(orderTruck => orderTruck.truckId === truck.id);
        }

        function canAddTruckWithDriverToOrder(truck, driverId, order) {
            var validateUtilization = abp.setting.getBoolean('App.DispatchingAndMessaging.ValidateUtilization');
            if (isTodayOrFutureDate(order)
                && (truck.utilization >= 1 && validateUtilization
                    || truckHasNoDriver(truck) && truckCategoryNeedDriver(truck)
                    || truck.isOutOfService
                    || truck.vehicleCategory.assetType === abp.enums.assetType.trailer)) {
                return false;
            }
            if (validateUtilization && order.trucks.some(olt => !olt.isDone && (olt.truckId === truck.id || olt.driverId === driverId))) {
                return false;
            }

            return true;
        }

        function getScheduleTrucksForOrder(order, query, syncCallback, asyncCallback) {
            var result = [];
            query = (query || '').toLowerCase();
            _scheduleTrucks.forEach(truck => {
                if (!canAddTruckWithDriverToOrder(truck, truck.driverId, order)) {
                    return;
                }

                var truckCode = (truck.truckCode || '').toLowerCase();
                if (!truckCode.startsWith(query)) {
                    return;
                }
                result.push({
                    id: 0,
                    parentId: null,
                    orderId: order.orderId,
                    truckId: truck.id,
                    truckCode: truck.truckCode,
                    driverId: truck.driverId,
                    textForLookup: truck.truckCode,
                    officeId: truck.officeId,
                    isExternal: truck.isExternal,
                    leaseHaulerId: truck.leaseHaulerId,
                    sharedOfficeId: truck.sharedFromOfficeId,
                    vehicleCategory: {
                        id: truck.vehicleCategory.id,
                        name: truck.vehicleCategory.name,
                        assetType: truck.vehicleCategory.assetType,
                        isPowered: truck.vehicleCategory.isPowered,
                        sortOrder: truck.vehicleCategory.sortOrder
                    },
                    canPullTrailer: truck.canPullTrailer,
                    alwaysShowOnSchedule: truck.alwaysShowOnSchedule
                });
            });

            if (!result.length) {
                _driverAssignments
                    .filter(da => da.driverId && da.driverIsActive && !da.driverIsExternal)
                    .forEach(driverAssignment => {
                        var truck = _scheduleTrucks.find(t => t.id === driverAssignment.truckId);
                        if (!truck) {
                            return;
                        }

                        if (!canAddTruckWithDriverToOrder(truck, driverAssignment.driverId, order)) {
                            return;
                        }

                        var driverName = ((driverAssignment.driverLastName || '') + ", " + (driverAssignment.driverFirstName || ''));
                        var driverNameWithTruck = driverName + " - " + (truck.truckCode || '');
                        //if (!driverName.toLowerCase().startsWith(query)) { //only matching by driverName causes issues when the text is autocompleted with Tab first and only then they hit Enter, as opposed to just hitting Enter. We need to match against a complete string to avoid issues
                        if (!driverNameWithTruck.toLowerCase().startsWith(query)) {
                            return;
                        }

                        result.push({
                            id: 0,
                            parentId: null,
                            orderId: order.orderId,
                            truckId: truck.id,
                            truckCode: truck.truckCode,
                            driverId: driverAssignment.driverId,
                            textForLookup: driverNameWithTruck,
                            officeId: truck.officeId,
                            isExternal: truck.isExternal,
                            leaseHaulerId: truck.leaseHaulerId,
                            sharedOfficeId: truck.sharedFromOfficeId,
                            vehicleCategory: {
                                id: truck.vehicleCategory.id,
                                name: truck.vehicleCategory.name,
                                assetType: truck.vehicleCategory.assetType,
                                isPowered: truck.vehicleCategory.isPowered,
                                sortOrder: truck.vehicleCategory.sortOrder
                            },
                            canPullTrailer: truck.canPullTrailer,
                            alwaysShowOnSchedule: truck.alwaysShowOnSchedule
                        });

                    });
            }

            syncCallback(result);
        }

        function getOrderPriorityClass(order) {
            switch (order.priority) {
                case abp.enums.orderPriority.high: return 'order-priority-icon-high fas fa-arrow-circle-up';
                case abp.enums.orderPriority.medium: return 'order-priority-icon-medium far fa-circle';
                case abp.enums.orderPriority.low: return 'order-priority-icon-low fas fa-arrow-circle-down';
            }
            return '';
        }

        function getOrderPriorityTitle(order) {
            switch (order.priority) {
                case abp.enums.orderPriority.high: return 'High Priority';
                case abp.enums.orderPriority.medium: return 'Medium Priority';
                case abp.enums.orderPriority.low: return 'Low Priority';
            }
            return '';
        }

        function getTruckTileClass(truck) {
            if (truck.isOutOfService)
                return "gray";
            if (truckHasNoDriver(truck) && truckCategoryNeedDriver(truck))
                return "blue";
            if (truck.utilization >= 1)
                return "red";
            if (truck.utilization > 0)
                return "yellow";
            return "green";
        }

        function truckCategoryNeedDriver(truck) {
            return truck.vehicleCategory.isPowered &&
                (isLeaseHaulerEnabled || (!truck.alwaysShowOnSchedule && !truck.isExternal)); //&& truck.officeId !== null
        }

        function getTruckTileOfficeClass(truck) {
            var officeId = parseInt($('#OfficeIdFilter').val());
            if (truck.sharedWithOfficeId !== null && truck.sharedWithOfficeId !== officeId) {
                return 'truck-office-' + truck.officeId;
            }
            if (truck.officeId !== officeId) {
                return 'truck-office-' + truck.officeId;
            }
            return '';
        }

        function getTruckTilePointerClass(truck) {
            if (truck.utilization > 0) {
                return 'hand';
            }
            return '';
        }

        function getTruckTileTitle(truck) {
            var title = truck.truckCode;
            if (truckCategoryNeedDriver(truck)) {
                title += ' - ' + truck.driverName;
            }
            return title;
        }

        function getTruckTagClass(truck) {
            let isLeaseHauler = truck.alwaysShowOnSchedule || truck.isExternal; // || truck.officeId === null;
            switch (truck.vehicleCategory.assetType) {
                case abp.enums.assetType.dumpTruck: return isLeaseHauler ? 'leased-dump-truck' : 'dump-truck';
                case abp.enums.assetType.tractor: return isLeaseHauler ? 'leased-tractor' : 'tractor';
                case abp.enums.assetType.trailer: return 'trailer';
            }
            return '';
        }

        async function setOrderIsClosedValue(orderLineId, val, isCancelled) {
            try {
                abp.ui.setBusy();
                await _schedulingService.setOrderLineIsComplete({
                    orderLineId: orderLineId,
                    isComplete: val,
                    isCancelled: isCancelled
                }).done(function () {
                    reloadMainGrid(null, false);
                    reloadTruckTiles();
                });
            }
            finally {
                abp.ui.clearBusy();
            }
        }

        function setTruckIsOutOfServiceValue(truckId, val) {
            if (!val) {
                _truckService.setTruckIsOutOfService({
                    truckId: truckId,
                    isOutOfService: val
                }).done(function () {
                    reloadMainGrid(null, false);
                    reloadTruckTiles();
                });
            } else {
                _addOutOfServiceReasonModal.open({ truckId: truckId, date: _dtHelper.getFilterData().date });
            }
        }
        abp.event.on('app.addOutOfServiceReasonModalSaved', function () {
            reloadMainGrid(null, false);
            reloadTruckTiles();
        });

        function isAllowedToEditOrder(orderLine) {
            return true; //orderLine.officeId === abp.session.officeId;
        }

        function isAllowedToEditOrderClosedState(orderLine) {
            return true; //orderLine.officeId === abp.session.officeId;
        }

        function isAllowedToEditOrderTrucks(orderLine) {
            return orderLine.isClosed === false; //&&
            //(orderLine.officeId === abp.session.officeId || orderLine.isShared);
        }

        function isTodayOrFutureDate(orderLine) {
            var today = new Date(moment().format("YYYY-MM-DD") + 'T00:00:00Z');
            return (new Date(orderLine.date)).getTime() >= today.getTime();
        }

        function hasOrderEditPermissions() {
            return _permissions.edit;
        }
        function hasTicketEditPermissions() {
            return _permissions.edit && _permissions.editTickets;
        }
        function hasOrderPrintPermissions() {
            return _permissions.print;
        }
        function hasTrucksPermissions() {
            return _permissions.trucks;
        }

        function handleQuantityCellCreation(fieldName, saveMethod, shouldRenderFunc) {
            return function (cell, cellData, rowData, rowIndex, colIndex) {
                $(cell).empty();
                if (shouldRenderFunc && !shouldRenderFunc(rowData)) {
                    $(cell).text('-');
                    $(cell).removeClass('cell-editable');
                    return;
                }
                if (!hasOrderEditPermissions() || !isAllowedToEditOrder(rowData)) {
                    $(cell).text(rowData[fieldName]);
                    $(cell).removeClass('cell-editable');
                    return;
                }
                var editor = $('<input type="text">').appendTo($(cell));
                editor.val(rowData[fieldName]);
                editor.focusout(async function () {
                    var newValue = $(this).val();
                    if (newValue === (rowData[fieldName] || "").toString()) {
                        return;
                    }
                    if (isNaN(newValue) || parseFloat(newValue) > 1000000 || parseFloat(newValue) < 0) {
                        abp.message.error('Please enter a valid number!');
                        $(this).val(rowData[fieldName]);
                        return;
                    }
                    newValue = newValue === "" ? null : abp.utils.round(parseFloat(newValue));

                    var tempRowData = {
                        materialQuantity: rowData.materialQuantity,
                        freightQuantity: rowData.freightQuantity
                    };
                    tempRowData[fieldName] = newValue;

                    if (!await abp.scheduling.checkExistingDispatchesBeforeSettingQuantityAndNumberOfTrucksZero(
                        rowData.id, tempRowData.materialQuantity, tempRowData.freightQuantity, rowData.numberOfTrucks
                    )) {
                        reloadMainGrid(null, false);
                        return;
                    }

                    abp.ui.setBusy(cell);
                    var saveData = {
                        orderLineId: rowData.id
                    };
                    saveData[fieldName] = newValue;
                    saveMethod(saveData).done(function (result) {
                        rowData[fieldName] = result[fieldName];
                        abp.notify.info('Saved successfully.');
                        if (newValue === 0 || newValue === null) {
                            if (!rowData.materialQuantity && !rowData.freightQuantity) {
                                reloadMainGrid(null, false);
                                reloadTruckTiles();
                            }
                        }
                    }).fail(function () {
                        reloadMainGrid(null, false);
                    }).always(function () {
                        abp.ui.clearBusy(cell);
                    });
                });
            };
        }

        var staggeredIcon = ' <span class="far fa-clock staggered-icon pull-right" title="Staggered"></span>';
        function handleTimepickerCellCreation(fieldName, saveMethod, isTimeEditableField, isTimeStaggeredField) {
            return function (cell, cellData, rowData, rowIndex, colIndex) {
                $(cell).empty();
                var isTimeStaggered = function (rowData) {
                    return isTimeStaggeredField ? rowData[isTimeStaggeredField] : false;
                }
                var isTimeEditable = function (rowData) {
                    return isTimeEditableField ? rowData[isTimeEditableField] : true;
                }
                var getFormattedCellValue = function () {
                    return _dtHelper.renderTime(rowData[fieldName], '');
                };
                var oldValue = getFormattedCellValue();
                var setRowValue = function (value) {
                    rowData[fieldName] = value;
                    oldValue = value;
                };
                if (!isAllowedToEditOrder(rowData) || !isTimeEditable(rowData)) {
                    var text = getFormattedCellValue();
                    if (isTimeStaggered(rowData)) {
                        $(cell).html(text + staggeredIcon);
                    } else {
                        $(cell).text(text);
                    }
                    $(cell).removeClass('cell-editable');
                    return;
                }
                var editor = $('<input type="text">').appendTo($(cell));
                if (isTimeStaggered(rowData)) {
                    editor.addClass('with-staggered-icon');
                    $(staggeredIcon).removeClass('pull-right').appendTo($(cell));
                }
                editor.val(oldValue);
                editor.focusout(function (e) {
                    var newValue = $(this).val();
                    if (newValue === (oldValue || "")) {
                        return;
                    }
                    if (newValue && !abp.helper.isTimeString(newValue)) {
                        abp.message.error('Enter a valid time in a format like 11:24 PM');
                        $(this).val(oldValue);
                        return;
                    }
                    abp.ui.setBusy(cell);
                    var request = {
                        orderLineId: rowData.id
                    };
                    request[fieldName] = newValue;
                    saveMethod(
                        request
                    ).done(function () {
                        setRowValue(newValue);
                        abp.notify.info('Saved successfully.');
                    }).always(function () {
                        abp.ui.clearBusy(cell);
                    });
                });
            };
        }

        function handleLocationCellClickForEdit(idField) {
            return function (cell, cellData, rowData, rowIndex, colIndex) {
                $(cell).click(function () {
                    if (!hasOrderEditPermissions()) return;
                    _schedulingService.isOrderLineFieldReadonly({ orderLineId: rowData.id, fieldName: abp.utils.toPascalCase(idField) })
                        .done(function (result) {
                            if (result) {
                                return;
                            }
                            _createOrEditJobModal.open({
                                orderLineId: rowData.id,
                                focusFieldId: idField
                            });
                        });
                });
            };
        }

        function refreshDateRelatedButtonsVisibility() {
            refreshDriverAssignmentButtonVisibility();
            refreshSendOrdersToDriversButtonVisibility();
        }

        function refreshDriverAssignmentButtonVisibility() {
            $('#AddDefaultDriverAssignmentsButton').closest('li').toggle(!isPastDate());
        }

        function refreshSendOrdersToDriversButtonVisibility() {
            $('#SendOrdersToDriversButton').closest('li').toggle(!isPastDate());
        }

        function refreshProgressBarColumnVisibility() {
            scheduleGrid.column('progress:name').visible(!$('#HideProgressBar').is(':checked') && showDispatchItems && showProgressColumn && isToday());
        }

        function refreshHideProgressBarCheckboxVisibility() {
            if (!showDispatchItems || !showProgressColumn || !isToday()) {
                $('#HideProgressBar').closest('div').hide();
            } else {
                $('#HideProgressBar').closest('div').show();
            }
        }

        $('#DateFilter').on('dp.change', function () {
            if (moment($(this).val(), 'MM/DD/YYYY').isValid()) {
                reloadTruckTiles();
                reloadDriverAssignments();
                reloadMainGrid();
                refreshHideProgressBarCheckboxVisibility();
                refreshDateRelatedButtonsVisibility();
            }
        });
        $('#DateFilter').blur(function () {
            if (!moment($(this).val(), 'MM/DD/YYYY').isValid()) {
                $(this).val(moment().format("MM/DD/YYYY"));
            }
        });

        $('#OfficeIdFilter, #HideCompletedOrders, #ShiftFilter, #HideProgressBar').change(function () {
            if (!_loadingState && moment($('#DateFilter').val(), 'MM/DD/YYYY').isValid()) {
                reloadTruckTiles();
                reloadDriverAssignments();
                reloadMainGrid();
            }
        });
        //reloadTruckTiles();

        // Menu functions definitions
        var menuFunctions = {
            isVisible: {},
            fn: {}
        };
        menuFunctions.isVisible.viewEdit = function (rowData) {
            return hasOrderEditPermissions();
        };
        menuFunctions.fn.viewEdit = function (element) {
            var orderId = _dtHelper.getRowData(element).orderId;
            window.location = abp.appPath + 'app/Orders/Details/' + orderId;
        };
        menuFunctions.isVisible.markComplete = function (rowData) {
            return hasOrderEditPermissions() && rowData.isClosed === false && isAllowedToEditOrderClosedState(rowData);
        };
        menuFunctions.fn.markComplete = async function (element, options) {
            options = options || {};
            var actionDescription = options.isCancelled ? 'cancelled' : 'complete';
            var orderLineId = _dtHelper.getRowData(element).id;
            try {
                abp.ui.setBusy();
                let prompt = ``;
                let isOpenDispatchesExist = false;
                if (await _schedulingService.openDispatchesExist(orderLineId)) {
                    isOpenDispatchesExist = true;
                    prompt = `There are open dispatches associated with this order. Marking this order ${actionDescription} will remove these dispatches. Are you sure you want to do this?`;
                }
                abp.ui.clearBusy();
                if (isOpenDispatchesExist && !await abp.message.confirm(prompt)) {
                    return;
                }
                await setOrderIsClosedValue(orderLineId, true, options.isCancelled);
            }
            finally {
                abp.ui.clearBusy();
            }
        };
        menuFunctions.isVisible.reOpenJob = function (rowData) {
            return hasOrderEditPermissions() && rowData.isClosed === true && isAllowedToEditOrderClosedState(rowData);
        };
        menuFunctions.fn.reOpenJob = async function (element) {
            await setOrderIsClosedValue(_dtHelper.getRowData(element).id, false);
            abp.notify.info('Saved successfully.');
        };
        menuFunctions.isVisible.copy = function (rowData) {
            return hasOrderEditPermissions() && isAllowedToEditOrderClosedState(rowData);
        };
        menuFunctions.fn.copy = async function (element) {
            var orderLine = _dtHelper.getRowData(element);
            try {
                abp.ui.setBusy();
                let hasMultipleItems = await _orderService.doesOrderHaveMultipleLines(orderLine.orderId);
                abp.ui.clearBusy();
                if (!hasMultipleItems) {
                    _copyOrderModal.open({
                        orderId: orderLine.orderId
                    });
                    return;
                }
                let multipleOrderLinesResponse = await swal(
                    "You have selected to copy an order with multiple line items. Select the button below for how you want to handle this copy.",
                    {
                        buttons: {
                            cancel: "Cancel",
                            single: "Single line item",
                            all: "All line items"
                        }
                    }
                );
                var copyOrderParams = {
                    orderId: orderLine.orderId
                };
                switch (multipleOrderLinesResponse) {
                    case "single":
                        copyOrderParams.orderLineId = orderLine.id;
                        break;
                    case "all":
                        break;
                    default:
                        return;
                }
                _copyOrderModal.open(copyOrderParams);

            }
            finally {
                abp.ui.clearBusy();
            }
        };
        menuFunctions.isVisible.share = function (rowData) {
            var today = new Date(moment().format("YYYY-MM-DD") + 'T00:00:00Z');
            return _features.allowSharedOrders && _features.allowMultiOffice && hasOrderEditPermissions() && !rowData.isClosed && isAllowedToEditOrder(rowData) && (new Date(rowData.date)).getTime() >= today.getTime();
        };
        menuFunctions.fn.share = function (element) {
            var orderLineId = _dtHelper.getRowData(element).id;
            _shareOrderLineModal.open({ id: orderLineId }).fail(handlePopupException);
        };
        menuFunctions.isVisible.transfer = function (rowData) {
            return _features.allowMultiOffice && hasOrderEditPermissions() && !rowData.isClosed && !isOrderLineShared(rowData) && isAllowedToEditOrder(rowData);
        };
        menuFunctions.fn.transfer = function (element) {
            var orderLineId = _dtHelper.getRowData(element).id;
            _setOrderOfficeIdModal.open({ id: orderLineId }).fail(handlePopupException);
        };
        menuFunctions.isVisible.sendOrderToLeaseHauler = function (rowData) {
            return _features.allowSendingOrdersToDifferentTenant && hasOrderEditPermissions() && !rowData.isClosed && !isOrderLineShared(rowData) && !isPastDate();
        };
        menuFunctions.fn.sendOrderToLeaseHauler = function (element) {
            var orderLineId = _dtHelper.getRowData(element).id;
            _sendOrderLineToHaulingCompanyModal.open({ orderLineId: orderLineId }).fail(handlePopupException);
        };
        menuFunctions.isVisible.changeDate = function (rowData) {
            return hasOrderEditPermissions() && !rowData.isClosed && isAllowedToEditOrder(rowData);
        };
        menuFunctions.fn.changeDate = function (element) {
            var orderLine = _dtHelper.getRowData(element);
            _setOrderDateModal.open({ orderLineId: orderLine.id }).fail(handlePopupException);
        };
        menuFunctions.isVisible.reassignTrucks = function (rowData) {
            return hasOrderEditPermissions() && isAllowedToEditOrder(rowData) && isTodayOrFutureDate(rowData);
        };
        menuFunctions.fn.reassignTrucks = function (element) {
            var orderLine = _dtHelper.getRowData(element);
            _reassignTrucksModal.open({ orderLineId: orderLine.id }).fail(handlePopupException);
        };
        menuFunctions.isVisible.delete = function (rowData) {
            return hasOrderEditPermissions() && isAllowedToEditOrder(rowData);
        };
        menuFunctions.fn.delete = async function (element) {
            var orderLine = _dtHelper.getRowData(element);
            if (!await abp.message.confirm('Are you sure you want to delete the order line for the "' + orderLine.customerName + '"?')) {
                return;
            }
            try {
                abp.ui.setBusy();
                let hasMultipleLines = await _orderService.doesOrderHaveMultipleLines(orderLine.orderId);
                if (!hasMultipleLines) {
                    await deleteOrder(orderLine.orderId);
                    return;
                }
                abp.ui.clearBusy();
                let multipleOrderLinesResponse = await swal(
                    "There are multiple line items associated with this order. Select the button below corresponding with what you want to delete.",
                    {
                        buttons: {
                            cancel: "Cancel",
                            single: "Single line item",
                            all: "Entire order"
                        }
                    }
                );
                abp.ui.setBusy();
                switch (multipleOrderLinesResponse) {
                    case "single":
                        await deleteOrderLine(orderLine.id, orderLine.orderId);
                        return;
                    case "all":
                        await deleteOrder(orderLine.orderId);
                        return;
                    default:
                        return;
                }
            }
            finally {
                abp.ui.clearBusy();
            }

            async function deleteOrderLine(orderLineId, orderId) {
                await _orderService.deleteOrderLine({
                    id: orderLineId,
                    orderId: orderId
                }).done(function () {
                    abp.notify.info('Successfully deleted.');
                    reloadMainGrid();
                });
            }
            async function deleteOrder(orderId) {
                await _orderService.deleteOrder({
                    id: orderId
                }).done(function () {
                    abp.notify.info('Successfully deleted.');
                    reloadMainGrid();
                });
            }
        };
        menuFunctions.fn.printNoPrices = function (element) {
            var orderId = _dtHelper.getRowData(element).orderId;
            window.open(abp.appPath + 'app/orders/GetWorkOrderReport?' + 'id=' + orderId + '&hidePrices=true');
        };
        menuFunctions.fn.printCombinedPrices = function (element) {
            var orderId = _dtHelper.getRowData(element).orderId;
            window.open(abp.appPath + 'app/orders/GetWorkOrderReport?' + 'id=' + orderId);
        };
        menuFunctions.fn.printSeparatePrices = function (element) {
            var orderId = _dtHelper.getRowData(element).orderId;
            var options = app.order.getOrderWithSeparatePricesReportOptions({ id: orderId });
            window.open(abp.appPath + 'app/orders/GetWorkOrderReport?' + $.param(options));
        };
        menuFunctions.fn.printBackOfficeDetail = function (element) {
            var orderId = _dtHelper.getRowData(element).orderId;
            var options = app.order.getBackOfficeReportOptions({ id: orderId });
            window.open(abp.appPath + 'app/orders/GetWorkOrderReport?' + $.param(options));
        };
        menuFunctions.fn.printWithDeliveryInfo = function (element) {
            var orderId = _dtHelper.getRowData(element).orderId;
            _printOrderWithDeliveryInfoModal.open({ id: orderId });
        };
        menuFunctions.isVisible.tickets = function (rowData) {
            return hasTicketEditPermissions() && (
                rowData.officeId === abp.session.officeId
                || rowData.sharedOfficeIds.indexOf(abp.session.officeId) !== -1
                || !abp.setting.getBoolean('App.General.SplitBillingByOffices')
            );
        };
        menuFunctions.fn.tickets = function (element) {
            var orderLineId = _dtHelper.getRowData(element).id;
            _createOrEditTicketModal.open({ orderLineId: orderLineId });
        };
        menuFunctions.isVisible.showMap = function (rowData) {
            return showDispatchViaGeotabItems && isOrderLineBelongToOrSharedWithUsersOffice(rowData);
        };
        menuFunctions.fn.showMap = function (element) {
            var orderLineId = _dtHelper.getRowData(element).id;
            window.open('scheduling/ShowMap?orderLineId=' + orderLineId, '_blank');
        };
        menuFunctions.isVisible.showTripsReport = function (rowData) {
            return showDispatchViaGeotabItems && isOrderLineBelongToOrSharedWithUsersOffice(rowData);
        };
        menuFunctions.fn.showTripsReport = function (element) {
            _tripsReportModal.open();
        };
        menuFunctions.isVisible.showCycleTimes = function (rowData) {
            return showDispatchViaGeotabItems && isOrderLineBelongToOrSharedWithUsersOffice(rowData);
        };
        menuFunctions.fn.showCycleTimes = function (element) {
            _cycleTimesModal.open();
        };
        function thereAreNotDoneTrucks(rowData) {
            return rowData.trucks.some(function (truck) {
                return !truck.isDone && truck.vehicleCategory.isPowered;
            });
        }
        function thereAreNotDoneAndNotLeasedTrucks(rowData) {
            return rowData.trucks.some(function (truck) {
                return !truck.isDone && truck.vehicleCategory.isPowered && !truck.alwaysShowOnSchedule; //truck.officeId == null (external lease haulers) are included
            });
        }
        menuFunctions.isVisible.dispatchToDriver = function (rowData) {
            var today = new Date(moment().format("YYYY-MM-DD") + 'T00:00:00Z');
            return showDispatchItems && !rowData.isClosed && rowData.trucks.length > 0 &&
                thereAreNotDoneTrucks(rowData) &&
                (new Date(rowData.date)).getTime() >= today.getTime()
                && isOrderLineBelongToOrSharedWithUsersOffice(rowData);
        };
        menuFunctions.fn.dispatchToDriver = function (element) {
            var orderLineId = _dtHelper.getRowData(element).id;
            sendDispatchMessage({ orderLineId: orderLineId });
        };
        menuFunctions.isVisible.messageToDriver = function (rowData) {
            return allowSmsMessages && _permissions.driverMessages && thereAreNotDoneTrucks(rowData);
        };
        menuFunctions.fn.messageToDriver = function (element) {
            var orderLineId = _dtHelper.getRowData(element).id;
            _sendDriverMessageModal.open({ orderLineId: orderLineId });
        };
        menuFunctions.isVisible.viewDispatches = function (rowData) {
            return showDispatchViaSmsItems && isOrderLineBelongToOrSharedWithUsersOffice(rowData);
        };
        menuFunctions.fn.viewDispatches = function (element) {
            var orderLineId = _dtHelper.getRowData(element).id;
            window.location.href = abp.appPath + 'app/Dispatches/?orderLineId=' + orderLineId;
        };
        menuFunctions.isVisible.activateClosedTrucks = function (rowData) {
            return hasOrderEditPermissions() &&
                !rowData.isClosed &&
                rowData.trucks.some(function (truck) {
                    return truck.isDone;
                });
        };
        menuFunctions.fn.activateClosedTrucks = function (element) {
            var rowData = _dtHelper.getRowData(element);
            if (rowData.maxUtilization === 0 || rowData.maxUtilization - rowData.utilization <= 0) {
                abp.notify.error("Increase # of Trucks");
                return;
            }

            var orderLineId = _dtHelper.getRowData(element).id;
            _activateClosedTrucksModal.open({ orderLineId: orderLineId });
        };
        menuFunctions.isVisible.viewJobSummary = function (rowData) {
            return true;
        };
        menuFunctions.fn.viewJobSummary = function (element) {
            var data = _dtHelper.getRowData(element);
            var orderLineId = data.id;
            _jobSummaryModal.open({ orderLineId }).fail(handlePopupException);
        };

        function isOrderLineBelongToOrSharedWithUsersOffice(rowData) {
            return true;
            //return rowData.officeId === abp.session.officeId
            //    || rowData.sharedOfficeIds.indexOf(abp.session.officeId) !== -1;
        }

        function getResponsivePriorityByName(name) {
            //the highest will be hidden first
            let columnsToHide = [
                'isClosed',
                'priority',
                'jobNumber',
                'quantityFormatted',
                'time',
                'item',
                'progress',
                'customerName'
            ];
            var index = columnsToHide.indexOf(name);
            if (index === -1) {
                return 0;
            }
            return columnsToHide.length - index;
        }

        var scheduleTable = $('#ScheduleTable');
        //scheduleTable.append('<tfoot><tr>' + '<th></th>'.repeat(16) + '</tr></tfoot>');
        var scheduleGrid = scheduleTable.DataTableInit({
            stateSave: true,
            stateDuration: 0,
            stateLoadCallback: function (settings, callback) {
                app.localStorage.getItem('schedule_filter', function (result) {
                    var filter = result || {};

                    _loadingState = true;
                    if (filter.date) {
                        $('#DateFilter').val(filter.date);
                        refreshHideProgressBarCheckboxVisibility();
                        refreshDateRelatedButtonsVisibility();
                    }
                    if (filter.shift) {
                        $('#ShiftFilter').val(filter.shift).trigger("change");
                    }
                    if (filter.officeId) {
                        abp.helper.ui.addAndSetDropdownValue($("#OfficeIdFilter"), filter.officeId, filter.officeName);
                        $('#OfficeIdFilter').val(filter.officeId).trigger("change");
                    }
                    if (filter.hideCompletedOrders) {
                        $('#HideCompletedOrders').prop('checked', true);
                    }
                    if (filter.hideProgressBar) {
                        $('#HideProgressBar').prop('checked', true);
                    }
                    _loadingState = false;
                    reloadTruckTiles();
                    reloadDriverAssignments();

                    app.localStorage.getItem('schedule_grid', function (result) {
                        callback(JSON.parse(result));
                    });
                });
            },
            stateSaveCallback: function (settings, data) {
                delete data.columns;
                delete data.search;
                app.localStorage.setItem('schedule_grid', JSON.stringify(data));
                app.localStorage.setItem('schedule_filter', _dtHelper.getFilterData());
            },
            searching: false,
            paging: false,
            serverSide: true,
            processing: true,
            pageLength: 100,
            ajax: function (data, callback, settings) {
                var abpData = _dtHelper.toAbpData(data);
                $.extend(abpData, _dtHelper.getFilterData());
                _schedulingService.getScheduleOrders(abpData).done(function (abpResult) {
                    refreshProgressBarColumnVisibility();
                    callback(_dtHelper.fromAbpResult(abpResult));
                    $('.tt-input').attr('size', '1'); //fixes the size of Trucks enter-new-tag-input (by default it's too big and sometimes jumps to a new line)
                });
            },
            order: [[3, 'asc']],
            headerCallback: function (thead, data, start, end, display) {
                $(thead).find('th').eq(0).css('padding', '8px');
            },
            footerCallback: function (row, data, start, end, display) {
                recalculateFooterTotals();
            },
            responsive: {
                details: {
                    renderer: function (api, rowIdx, columns) {
                        var data = $.map(columns, function (col, i) {
                            return col.hidden ?
                                '<tr data-dt-row="' + col.rowIndex + '" data-dt-column="' + col.columnIndex + '">' +
                                '<td>' + col.title + (col.title ? ':' : '') + '</td> ' +
                                '<td>' + col.data + '</td>' +
                                '</tr>' :
                                '';
                        }).join('');

                        return data ? $('<table/>').append(data) : false;
                    }
                }
            },

            columns: [
                {
                    className: 'control responsive',
                    orderable: false,
                    title: "&nbsp;",
                    width: "10px",
                    render: function () {
                        return '';
                    }
                },
                {
                    data: "priority",
                    title: "",
                    width: "10px",
                    responsivePriority: getResponsivePriorityByName('priority'),
                    className: 'small-padding',
                    render: function (data, type, full, meta) {
                        return '<i class="' + getOrderPriorityClass(full) + '" title="' + getOrderPriorityTitle(full) + '"></i>';
                    }
                },
                {
                    data: "customerIsCod",
                    title: "COD",
                    sorting: false,
                    //orderable: false,
                    render: function (data, type, full, meta) { return _dtHelper.renderCheckbox(data); },
                    className: "checkmark all p-1",
                    width: "20px"
                },
                {
                    data: 'note',
                    title: "",
                    orderable: false,
                    className: "checkmark all p-2",
                    width: "20px",
                    render: function (data, type, full, meta) {
                        return '';
                    },
                    createdCell: function (cell, cellData, rowData, rowIndex, colIndex) {
                        let icon = $('<i class="la la-files-o directions-icon" data-toggle="tooltip" data-html="true"></i>');
                        if (rowData.note) {
                            icon.prop('title', abp.utils.replaceAll(rowData.note, '\n', '<br>') + '<br><br><b>Click icon to edit comments</b>');
                        } else {
                            icon.addClass('gray');
                            icon.prop('title', '<b>Click icon to add comments</b>');
                        }
                        icon.click(function () {
                            _createOrEditJobModal.open({
                                orderLineId: rowData.id,
                                focusFieldId: 'Note'
                            });
                        });
                        $(cell).append(icon);
                    }
                },
                {
                    className: "dont-break-out all",
                    data: "customerName",
                    responsivePriority: getResponsivePriorityByName('customerName'),
                    title: "Customer"
                },
                {
                    className: "job-number-column",
                    data: "jobNumber",
                    title: app.localize('JobNbr'),
                    responsivePriority: getResponsivePriorityByName('jobNumber')
                },
                {
                    data: "time",
                    render: function (data, type, full, meta) { return _dtHelper.renderTime(full.time, '') + (full.isTimeStaggered ? staggeredIcon : ''); },
                    title: 'Time on Job',
                    titleHoverText: 'Time on Job',
                    className: "time-on-job-column",
                    responsivePriority: getResponsivePriorityByName('time'),
                    width: "94px",
                    createdCell: function (cell, cellData, rowData, rowIndex, colIndex) {
                        $(cell).click(function () {
                            if (rowData["isTimeEditable"]) {
                                _createOrEditJobModal.open({
                                    orderLineId: rowData.id,
                                    focusFieldId: 'TimeOnJob'
                                });
                            }
                        });
                    },
                    orderable: true
                },
                {
                    data: "loadAtNamePlain",
                    render: function (data, type, full, meta) { return _dtHelper.renderText(full.loadAtName); },
                    title: "Load at",
                    className: "load-at all",
                    createdCell: handleLocationCellClickForEdit('LoadAtId')
                },
                {
                    data: "deliverToNamePlain",
                    render: function (data, type, full, meta) { return _dtHelper.renderText(full.deliverToName); },
                    title: "Deliver to",
                    className: 'deliver-to-column all',
                    createdCell: handleLocationCellClickForEdit('DeliverToId')
                },
                {
                    data: "item",
                    title: "Item",
                    responsivePriority: getResponsivePriorityByName('item')
                },
                {
                    data: "quantityFormatted",
                    orderable: false,
                    title: "Quantity",
                    responsivePriority: getResponsivePriorityByName('quantityFormatted'),
                    width: "100px",
                    render: function (data, type, full, meta) {
                        let span = $('<span>').text(data);
                        return abp.utils.replaceAll(span.html(), '\n', '<br>');
                    }
                },
                //{
                //    data: "materialUom",
                //    title: "Mat. UOM"
                //},
                //{
                //    data: "materialQuantity",
                //    responsivePriority: 1,
                //    title: '<span title="Material Quantity">Mat. Qty</span>', //was 'Quantity of material or service'
                //    width: "65px",
                //    className: "cell-editable",
                //    createdCell: handleQuantityCellCreation("materialQuantity", _schedulingService.setOrderLineMaterialQuantity,
                //        rowData => abp.enums.designations.hasMaterial(rowData.designation))
                //},
                //{
                //    data: "freightUom",
                //    title: "Freight UOM"
                //},
                //{
                //    data: "freightQuantity",
                //    responsivePriority: 1,
                //    title: '<span title="Freight Quantity">Freight Qty</span>', //was 'Quantity of material or service'
                //    width: "65px",
                //    className: "cell-editable",
                //    createdCell: handleQuantityCellCreation("freightQuantity", _schedulingService.setOrderLineFreightQuantity,
                //        rowData => !abp.enums.designations.materialOnly.includes(rowData.designation))
                //},
                {
                    data: "numberOfTrucks",
                    name: "numberOfTrucks",
                    orderable: false,
                    title: '<span title="# of Trucks">Req. Truck</span>',
                    width: "65px",
                    //responsivePriority: 1,
                    className: "cell-number-of-trucks all",
                    createdCell: function (cell, cellData, rowData, rowIndex, colIndex) {
                        $(cell).click(function () {
                            if (!hasOrderEditPermissions()) return;
                            _createOrEditJobModal.open({
                                orderLineId: rowData.id,
                                focusFieldId: 'NumberOfTrucks'
                            });
                        });
                    }
                },
                {
                    data: "scheduledTrucks",
                    name: "scheduledTrucks",
                    orderable: false,
                    title: 'Sched. Trucks',
                    width: "65px",
                    //responsivePriority: 1,
                    className: "cell-editable cell-scheduled-trucks all",
                    createdCell: function (cell, cellData, rowData, rowIndex, colIndex) {
                        $(cell).empty();
                        if (!hasOrderEditPermissions() || !isAllowedToEditOrder(rowData)) {
                            $(cell).text(rowData.scheduledTrucks);
                            $(cell).removeClass('cell-editable');
                            return;
                        }
                        var editor = $('<input type="text">').appendTo($(cell));
                        editor.val(rowData.scheduledTrucks);
                        editor.focusout(function () {
                            var newValue = $(this).val();
                            if (newValue === (rowData.scheduledTrucks || "").toString()) {
                                return;
                            }
                            if (isNaN(newValue) || parseFloat(newValue) < 0) {
                                abp.message.error('Please enter a valid number!');
                                $(this).val(rowData.scheduledTrucks);
                                return;
                            }
                            else if (parseFloat(newValue) > 999) {
                                abp.message.error('Please enter a valid number less than 1,000!');
                                $(this).val(rowData.scheduledTrucks);
                                return;
                            }

                            newValue = newValue === "" ? null : abp.utils.round(parseFloat(newValue));
                            $(this).val(newValue);
                            var saveCallback = function () {
                                abp.ui.setBusy(cell);
                                _schedulingService.setOrderLineScheduledTrucks({
                                    orderLineId: rowData.id,
                                    scheduledTrucks: newValue
                                }).done(function (result) {
                                    rowData.scheduledTrucks = result.scheduledTrucks;
                                    rowData.utilization = result.orderUtilization;
                                    rowData.maxUtilization = abp.utils.round(result.scheduledTrucks || 0);

                                    updateRowAppearance(editor, rowData);
                                    //scheduleGrid.draw(false);
                                    recalculateFooterTotals();
                                    abp.notify.info('Saved successfully.');
                                }).always(function () {
                                    abp.ui.clearBusy(cell);
                                });
                            };
                            if (rowData.utilization > 0) {
                                if (abp.utils.round(newValue) < rowData.utilization) {
                                    abp.message.warn(app.localize('RemoveSomeTrucks'))
                                        .then(function () {
                                            editor.val(rowData.scheduledTrucks);
                                        });
                                    return;
                                }
                            }
                            saveCallback();
                        });
                    }
                },
                {
                    data: null,
                    orderable: false,
                    render: function (data, type, full, meta) {
                        return data.trucks.map(function (t) { return t.truckCode; }).join(', ');
                    },
                    title: "Trucks",
                    //responsivePriority: 0,
                    className: "trucks all",
                    createdCell: function (cell, cellData, rowData, rowIndex, colIndex) {
                        $(cell).empty();
                        var cancelRemoveTag = false;
                        $(cell).css('min-width', '195px');
                        //$(cell).css('white-space', 'normal !important');
                        if (!hasOrderEditPermissions() || !isAllowedToEditOrderTrucks(rowData)) {
                            $(cell).addClass("readonly");
                        }
                        var editor = $('<input type="text" class="truck-cell-editor">').appendTo($(cell));
                        editor.tagsinput({
                            itemValue: 'truckId',
                            itemText: 'truckCode',
                            allowDuplicates: true,
                            tagClass: function (truck) {
                                return 'truck-tag ' + getTruckTagClass(truck) + ' truck-office-' + truck.officeId +
                                    (!isTruckFromOfficeOrSharedOffice(truck) ? ' another-office' : '') +
                                    (truck.isDone ? ' truck-isdone' : '');
                            },
                            typeaheadjs: {
                                displayKey: 'textForLookup',
                                source: function (query, syncCallback, asyncCallback) {
                                    getScheduleTrucksForOrder(rowData, query, syncCallback, asyncCallback);
                                },
                                limit: 500
                            }
                        });
                        $.each(rowData.trucks, function (int, truck) {
                            editor.tagsinput('add', truck);
                        });

                        var orderLineIsFullyUtilized = function () {
                            if (!abp.setting.getBoolean('App.DispatchingAndMessaging.ValidateUtilization')) {
                                return false;
                            }
                            return rowData.maxUtilization === 0 || rowData.maxUtilization - rowData.utilization <= 0;
                        };

                        $(cell).find('.tag').not('.truck-isdone').draggable({
                            containment: $('table tbody'),
                            revert: 'invalid'
                        });
                        $(cell).droppable({
                            accept: function (event) {
                                if (orderLineIsFullyUtilized() || rowData.isClosed) {
                                    return false;
                                } else {
                                    return true;
                                }
                            },
                            drop: function (event, ui) {
                                var tag = ui.helper.data('item');
                                var moveTruck = function () {
                                    _schedulingService.moveTruck({
                                        truckId: tag.truckId,
                                        sourceOrderLineTruckId: tag.id,
                                        destinationOrderLineId: rowData.id
                                    }).done(function (result) {
                                        reloadMainGrid(null, false);
                                        reloadTruckTiles();
                                        if (result.success) {
                                            abp.notify.info('Truck has been successfully moved.');
                                        } else {
                                            if (result.orderLineTruckExists) {
                                                if (tag.vehicleCategory.assetType === abp.enums.assetType.trailer) {
                                                    abp.notify.warn("This trailer is already associated with this order line and can't be added again.");
                                                } else {
                                                    abp.notify.warn("This truck is already assigned to this order and can't be added again. Instead, adjust the utilization if that is what you intend.");
                                                }
                                            }
                                        }
                                    }).fail(function () {
                                        reloadMainGrid(null, false);
                                    });
                                };

                                abp.services.app.scheduling.hasDispatches({
                                    orderLineTruckId: tag.id
                                }).done(function (result) {
                                    if (result.acknowledgedOrLoaded || result.unacknowledged) {
                                        abp.notify.error('There are active dispatches!');
                                        reloadMainGrid(null, false);
                                    } else {
                                        moveTruck();
                                    }
                                });

                            }
                        });

                        editor.on('beforeItemAdd', function (event) {
                            if (cancelRemoveTag) {
                                cancelRemoveTag = false;
                                return;
                            }
                            var tag = event.item;
                            if (!hasOrderEditPermissions() || !isAllowedToEditOrderTrucks(rowData)) {
                                event.cancel = true;
                                return;
                            }
                            if (orderLineIsFullyUtilized()) {
                                event.cancel = true;
                                abp.notify.error("Increase # of Trucks");
                                return;
                            }
                            if (!event.options || !event.options.preventPost) {
                                var onFail = function (errorMessage) {
                                    editor.tagsinput('remove', tag, { preventPost: true });
                                    abp.notify.error(errorMessage || 'Unknown error occurred on saving the OrderTruck');
                                    reloadMainGrid(null, false);
                                    reloadTruckTiles();
                                    reloadDriverAssignments();
                                };
                                _schedulingService.addOrderLineTruck({
                                    orderLineId: rowData.id,
                                    truckId: tag.truckId,
                                    driverId: tag.driverId
                                }).done(function (result) {
                                    if (result.isFailed) {
                                        onFail(result.errorMessage);
                                    } else {
                                        abp.notify.info('Saved successfully.');
                                        rowData.utilization = result.orderUtilization;
                                        $.extend(true, tag, result.item);
                                        rowData.trucks.push(tag);
                                        updateRowAppearance(editor, rowData);
                                        $(cell).find('.tag').draggable({
                                            containment: $('table tbody'),
                                            revert: 'invalid'
                                        });

                                        reloadTruckTiles();
                                        reloadDriverAssignments();
                                        if (tag.canPullTrailer && abp.setting.getBoolean('App.DispatchingAndMessaging.ShowTrailersOnSchedule')) {
                                            var popupOptions = {
                                                title: "Select Trailer",
                                                orderLineId: rowData.id,
                                                parentId: result.item.id,
                                                onlyTrailers: true,
                                                //isPowered: false,
                                                parentTruckId: result.item.truckId
                                            };
                                            $.extend(popupOptions, _dtHelper.getFilterData());
                                            _addOrderTruckModal.open(popupOptions);
                                        }
                                    }
                                }).fail(function () { onFail(); });
                            }
                        });
                        editor.on('beforeItemRemove', function (event) {
                            var tag = event.item;
                            if (!hasOrderEditPermissions() || !isAllowedToEditOrderTrucks(rowData)) {
                                event.cancel = true;
                                return;
                            }
                            if (!event.options || !event.options.preventPost) {
                                var deleteOrderLineTruck = function (markAsDone) {
                                    _schedulingService.deleteOrderLineTruck({
                                        orderLineTruckId: tag.id,
                                        orderLineId: rowData.id,
                                        markAsDone: markAsDone
                                    }).done(function (result) {
                                        rowData.utilization = result.orderLineUtilization;
                                        let trailers = rowData.trucks.filter(x => x.parentId === tag.id);
                                        if (markAsDone) {
                                            trailers.forEach(function (trailer) {
                                                trailer.utilization = 0;
                                                trailer.isDone = true;
                                            });
                                            abp.notify.info('Marked as done successfully.');
                                        } else {
                                            removeScheduleTruckFromArray(rowData.trucks, tag);
                                            trailers.forEach(function (trailer) {
                                                removeScheduleTruckFromArray(rowData.trucks, trailer);
                                                editor.tagsinput('remove', trailer, { preventPost: true });
                                            });
                                            abp.notify.info('Removed successfully.');
                                        }
                                        updateRowAppearance(editor, rowData);
                                        reloadTruckTiles();
                                        editor.tagsinput('refresh');
                                    }).fail(function () {
                                        editor.tagsinput('add', tag, { preventPost: true });
                                        abp.notify.error('Unknown error occurred on removing the OrderTruck');
                                    });
                                };

                                abp.scheduling.checkExistingDispatchesBeforeRemovingTruck(
                                    tag.id,
                                    tag.truckCode,
                                    function () {
                                        deleteOrderLineTruck(false);
                                    },
                                    function () {
                                        cancelRemoveTag = true;
                                        editor.tagsinput('add', tag, { preventPost: true });
                                    },
                                    function () {
                                        cancelRemoveTag = true;
                                        tag.isDone = true;
                                        editor.tagsinput('add', tag, { preventPost: true });
                                        deleteOrderLineTruck(true);
                                    }
                                );
                            }
                        });
                    }

                },
                {
                    data: "amount",
                    orderable: false,
                    name: "progress",
                    visible: showDispatchItems && showProgressColumn && !$('#HideProgressBar').is(':checked'),
                    responsivePriority: getResponsivePriorityByName('progress'),
                    title: "Progress",
                    render: function (data, type, full, meta) {
                        if (full.isCancelled) {
                            return app.localize('Cancel');
                        }
                        if (!showProgressColumn || !isToday()) {
                            return '';
                        }

                        let shouldRenderProgressBar = true;
                        let shouldShowAmountsTooltip = true;
                        let shouldShowNumberOfLoads = false;

                        let designationIsFreightOnly = abp.enums.designations.freightOnly.includes(full.designation);
                        let designationHasMaterial = abp.enums.designations.hasMaterial.includes(full.designation);
                        let amountPercent = 0;
                        let amountOrdered = full.amountOrdered || 0;
                        let amountLoaded = full.amountLoaded || 0;
                        let amountDelivered = full.amountDelivered || 0;

                        if (!designationHasMaterial && !designationIsFreightOnly) {
                            //If the designation is anything else, do not show the progress bar. 
                            //Only show the number of loads in the cell. Display the quantities based on the UOM on hover.
                            shouldRenderProgressBar = false;
                            shouldShowNumberOfLoads = true;
                        }

                        if (!amountOrdered) {
                            //order quantity is not specified, then the % complete can’t be calculated. 
                            //Show the number of loads in the column, but don’t show the progress bar.
                            shouldRenderProgressBar = false;
                            shouldShowNumberOfLoads = true;
                        }

                        if (designationIsFreightOnly) {
                            switch ((full.freightUom || '').toLowerCase()) {
                                case 'hour':
                                case 'hours':
                                    shouldRenderProgressBar = false;
                                    shouldShowNumberOfLoads = true;
                                    //amountLoaded = abp.utils.round(full.hoursOnDispatchesLoaded);
                                    //amountDelivered = abp.utils.round(full.hoursOnDispatches);
                                    break;
                            }
                        }

                        if (shouldRenderProgressBar) {
                            amountPercent = abp.utils.round(amountDelivered / amountOrdered * 100);
                        }

                        if (isNaN(amountPercent) || amountPercent === null) {
                            amountPercent = 0;
                        }

                        if (isNaN(amountLoaded) || amountLoaded === null) {
                            amountLoaded = 0;
                        }

                        if (isNaN(amountDelivered) || amountDelivered === null) {
                            amountDelivered = 0;
                        }

                        if (full.cargoCapacityRequiredError) {
                            shouldRenderProgressBar = false;
                            shouldShowNumberOfLoads = true;
                            //return getCargoCapacityErrorIcon(full.cargoCapacityRequiredError);
                        }

                        let tooltipTags = 'data-toggle="tooltip" data-html="true" title="<div class=\'text-left\'>Amount loaded: ' + amountLoaded +
                            '</div><div class=\'text-left\'>Amount delivered: ' + amountDelivered +
                            '</div><div class=\'text-left\'>Number of loads: ' + (full.loadCount || '0') + '</div>"';

                        if (!shouldShowAmountsTooltip) {
                            tooltipTags = '';
                        }

                        if (shouldRenderProgressBar) {
                            return '<div class="progress" ' + tooltipTags + '>' +
                                '<div class="progress-bar ' + (amountPercent > 100 ? 'progress-bar-overflown' : '') + '" role="progressbar" aria-valuenow="' + amountPercent + '" aria-valuemin="0" aria-valuemax="100" style="width:' + amountPercent + '%">' +
                                amountPercent + '%' +
                                '</div>' +
                                '</div>';
                        } else if (shouldShowNumberOfLoads) {
                            return (full.cargoCapacityRequiredError ? getCargoCapacityErrorIcon(full.cargoCapacityRequiredError) : '')
                                + '<span ' + tooltipTags + '>' + (full.loadCount || '0') + '</span>';
                        } else {
                            return '';
                        }
                    }
                },
                {
                    data: "isClosed",
                    render: function (data, type, full, meta) { return _dtHelper.renderCheckbox(full.isClosed); },
                    className: "checkmark is-closed-column",
                    width: "40px",
                    responsivePriority: getResponsivePriorityByName('isClosed'),
                    title: "Closed",
                    titleHoverText: "Closed"
                },
                {
                    data: null,
                    orderable: false,
                    name: "Actions",
                    title: "",
                    width: "10px",
                    //responsivePriority: 0, used class 'all' instead of this
                    className: "actions all",
                    //defaultContent: "<button type='button' class='btn btn-sm btn-default btnEditRow' title='Edit'><i class='fa fa-edit'></i>Edit</button>\n",
                    render: function (data, type, full, meta) {
                        var content = actionMenuHasItems() ?
                            '<div class="dropdown">' +
                            '<button class="btn btn-primary btn-sm btnActions"><i class="fa fa-ellipsis-h"></i></button>'
                            + '</div>' : '';
                        return content;

                    }
                }
            ],
            createdRow: function (row, data, index) {
                if (data.isClosed) {
                    $(row).addClass('order-closed');
                }
                if (isOrderLineShared(data)) {
                    $(row).addClass('order-shared');
                }
                if (!data.isClosed && data.utilization < data.maxUtilization) {
                    $(row).addClass('order-not-fully-utilized');
                }
                if (!data.isClosed && data.scheduledTrucks < data.numberOfTrucks) {
                    $(row).addClass('reqtruck-red');
                }
            },
            preDrawCallback: function (settings) {
                // check if filter includes current day or futures dates
                if (!isPastDate()) {
                    scheduleGrid.settings().context[0].oLanguage.sEmptyTable = "<span>There are no jobs for this date.</span><br /><button id='#howToAddaJob' class='btn btn-primary btn-sm mt-2'>Click here to see how to add a job</button>";
                } else {
                    scheduleGrid.settings().context[0].oLanguage.sEmptyTable = "No data available in table";
                }
            },
            drawCallback: function (settings) {
                $('table [data-toggle="tooltip"]').tooltip();
            }
        });

        function isOrderLineShared(orderLine) {
            return orderLine.isShared || orderLine.haulingCompanyOrderLineId || orderLine.materialCompanyOrderLineId;
        }

        function recalculateFooterTotals() {
            var api = scheduleGrid;
            var numberOfTrucksColumnName = 'numberOfTrucks:name';
            var scheduledTrucksColumnName = 'scheduledTrucks:name';

            var pageTotalReqTrucks = api.column(numberOfTrucksColumnName, { page: 'current' }).data().reduce((a, b) => parseFloat(a) + parseFloat(b || 0), 0);
            var pageTotalSchedTrucks = api.column(scheduledTrucksColumnName, { page: 'current' }).data().reduce((a, b) => parseFloat(a) + parseFloat(b || 0), 0);

            $(api.column(numberOfTrucksColumnName).footer()).html(pageTotalReqTrucks.toFixed(2));
            $(api.column(scheduledTrucksColumnName).footer()).html(pageTotalSchedTrucks.toFixed(2));
        }

        function getCargoCapacityErrorIcon(cargoCapacityRequiredError) {
            let errorIcon = $('<i class="fa fa-exclamation-circle color-red mr-2"></i>').attr('title', cargoCapacityRequiredError);
            return $('<div>').append(errorIcon).html();
        }

        function updateRowAppearance(element, rowData) {
            var row = $(element).closest('tr');
            if (!rowData.isClosed && rowData.utilization < rowData.maxUtilization) {
                row.addClass('order-not-fully-utilized');
            } else {
                row.removeClass('order-not-fully-utilized');
            }
            if (rowData.scheduledTrucks < rowData.numberOfTrucks) {
                row.addClass('reqtruck-red');
            } else {
                row.removeClass('reqtruck-red');
            }
        }

        function truckHasNoDriver(truck) {
            return !truck.isExternal && (truck.hasNoDriver || !truck.hasDefaultDriver && !truck.hasDriverAssignment);
        }

        function truckHasOrderLineTrucks(truck) {
            var orders = scheduleGrid.data().toArray();
            return orders.some(o => o.trucks.some(olt => olt.truckId === truck.id));
        }

        function isPastDate() {
            var isPastDate = moment($("#DateFilter").val(), 'MM/DD/YYYY') < moment().startOf('day');
            return isPastDate;
        }

        function isFutureDate() {
            var isFutureDate = moment($("#DateFilter").val(), 'MM/DD/YYYY') > moment().startOf('day');
            return isFutureDate;
        }

        function isToday() {
            var isToday = moment($("#DateFilter").val(), 'MM/DD/YYYY').isSame(moment().startOf('day'));
            return isToday;
        }

        //automatically select first dropdown value in trucks tagsinput on enter keypress
        $('body').on('keydown', '.tt-input', function (e) {
            if (e.keyCode === 13) {
                e = $.Event('keydown'); e.which = 40; $(this).trigger(e); //down arrow
                e = $.Event('keydown'); e.which = 13; $(this).trigger(e); //enter
            }
        });

        scheduleTable.on('dblclick', '.truck-tag', function () {
            var orderLine = _dtHelper.getRowData(this);
            if (!hasOrderEditPermissions() || !isAllowedToEditOrderTrucks(orderLine)) {
                return;
            }
            var truck = $(this).data('item');
            if (truck.isDone) {
                return;
            }
            if (truck.vehicleCategory.assetType === abp.enums.assetType.trailer) {
                abp.message.info("Trailer utilization can't be changed.");
                return;
            }
            if (!truck.vehicleCategory.isPowered) {
                abp.message.info("Utilization can't be changed for this type of truck");
                return;
            }
            _setTruckUtilizationModal.open({ id: truck.id }).fail(handlePopupException);
        });

        function isTruckFromOfficeOrSharedOffice(truck) {
            return true; //truck.officeId === abp.session.officeId || truck.sharedOfficeId === abp.session.officeId;
        }

        function handlePopupException(failResult) {
            if (failResult && failResult.loadResponseObject && failResult.loadResponseObject.userFriendlyException) {
                var param = failResult.loadResponseObject.userFriendlyException.parameters;
                if (param && param.Kind === "EntityDeletedException") {
                    reloadMainGrid(null, false);
                    reloadTruckTiles();
                }
            }
        }

        async function sendDispatchMessage(options) {
            if (dispatchVia === abp.enums.dispatchVia.simplifiedSms //sendSmsOnDispatching is always set to dontSend when using simplifiedSms, so we're not checking that value in this case
                || (dispatchVia === abp.enums.dispatchVia.driverApplication)
            ) {
                _sendDispatchMessageModal.open(options);
            } else if (dispatchVia === abp.enums.dispatchVia.driverApplication) {
                try {
                    abp.ui.setBusy();
                    await _dispatchingService.sendDispatchMessageNonInteractive(options);
                    abp.notify.info(app.localize('DispatchesBeingCreated'));
                }
                finally {
                    abp.ui.clearBusy();
                }
            } else {
                abp.message.warn("Dispatch via is not set to Driver Application");
            }
        }

        function reloadMainGrid(callback, resetPaging) {
            resetPaging = resetPaging === undefined ? true : resetPaging;
            scheduleGrid.ajax.reload(callback, resetPaging);
        }

        abp.event.on('app.addOrderTruckModalSaved', function () {
            reloadMainGrid(null, false);
            reloadTruckTiles();
            reloadDriverAssignments();
        });

        abp.event.on('app.assignDriverForTruckModalSaved', function () {
            reloadMainGrid(null, false);
            reloadTruckTiles();
            reloadDriverAssignments();
        });

        abp.event.on('app.truckUtilizationModalSaved', function () {
            reloadMainGrid(null, false);
            reloadTruckTiles();
        });

        abp.event.on('app.orderOfficeIdModalSaved', function () {
            reloadMainGrid(null, false);
            reloadTruckTiles();
        });

        abp.event.on('app.orderDirectionsModalSaved', function () {
            reloadMainGrid(null, false);
            reloadTruckTiles();
        });

        abp.event.on('app.orderLineNoteModalSaved', function () {
            reloadMainGrid(null, false);
            reloadTruckTiles();
        });

        abp.event.on('app.orderDateModalSaved', function () {
            reloadMainGrid(null, false);
            reloadTruckTiles();
        });

        abp.event.on('app.orderModalCopied', function () {
            reloadMainGrid(null, false);
            reloadTruckTiles();
        });

        abp.event.on('app.quoteCreatedFromOrderModal', function () {
            reloadMainGrid(null, false);
            reloadTruckTiles();
        });

        abp.event.on('app.orderModalShared', function () {
            reloadMainGrid(null, false);
            reloadTruckTiles();
        });

        abp.event.on('app.orderLineSentToHaulingCompany', function () {
            reloadMainGrid(null, false);
            reloadTruckTiles();
        });

        abp.event.on('app.noDriverForTruckModalSet', function () {
            reloadMainGrid(null, false);
            reloadTruckTiles();
            reloadDriverAssignments();
        });

        abp.event.on('app.defaultDriverForTruckModalSet', function () {
            reloadMainGrid(null, false);
            reloadTruckTiles();
            reloadDriverAssignments();
        });

        abp.event.on('app.sharedTruckModalAdded', function () {
            reloadMainGrid(null, false);
            reloadTruckTiles();
            reloadDriverAssignments();
        });

        abp.event.on('app.activateClosedTrucksModalSaved', function () {
            reloadMainGrid(null, false);
            reloadTruckTiles();
            reloadDriverAssignments();
        });

        abp.event.on('app.reassignModalSaved', function () {
            reloadMainGrid(null, false);
            reloadTruckTiles();
            reloadDriverAssignments();
        });

        abp.event.on('app.trucksAssignedModal', function () {
            reloadMainGrid(null, false);
            reloadTruckTiles();
            reloadDriverAssignments();
        });

        abp.event.on('app.createOrEditLeaseHaulerRequestModalSaved', function () {
            //reloadMainGrid(null, false);
            reloadTruckTiles();
            reloadDriverAssignments();
        });

        abp.event.on('app.changeDriverForOrderLineTruckModalSaved', function () {
            reloadMainGrid(null, false);
            reloadTruckTiles();
            reloadDriverAssignments();
        });

        abp.event.on('app.createOrEditJobModalSaved', function () {
            reloadMainGrid(null, false);
            //reloadTruckTiles();
            //reloadDriverAssignments();
        });

        function actionMenuHasItems() {
            return _permissions.edit ||
                _permissions.editTickets ||
                _permissions.print ||
                _permissions.driverMessages;
        }

        scheduleTable.on('click', '.btnActions', function (e) {
            e.preventDefault();
            var button = $(this);
            var position = button[0].getBoundingClientRect();
            position.x += $(window).scrollLeft();
            position.y += $(window).scrollTop();
            button.contextMenu({ x: position.x, y: position.y });
        });

        $('#AddLeaseHaulerRequestButton').click(function (e) {
            e.preventDefault();
            _createOrEditLeaseHaulerRequestModal.open({
                date: $('#DateFilter').val()
            });
        });

        $('#MarkAllJobsCompletedButton').click(async function (e) {
            e.preventDefault();
            try {
                let prompt = `Are you sure you want to mark all jobs complete?`;
                if (!await abp.message.confirm(prompt)) {
                    return;
                }
                abp.ui.setBusy();
                await _schedulingService.setAllOrderLinesIsComplete(_dtHelper.getFilterData()).done(function () {
                    abp.notify.info('Marked all jobs complete');
                    reloadMainGrid(null, false);
                    reloadTruckTiles();
                });
            }
            finally {
                abp.ui.clearBusy();
            }
        });

        $('#AddDefaultDriverAssignmentsButton').click(async function (e) {
            e.preventDefault();
            try {
                abp.ui.setBusy();
                await _driverAssignmentService.addDefaultDriverAssignments(_dtHelper.getFilterData()).done(function () {
                    abp.notify.info('Added default driver assignments');
                    //reloadMainGrid(null, false);
                    reloadTruckTiles();
                    reloadDriverAssignments();
                });
            }
            finally {
                abp.ui.clearBusy();
            }
        });

        $('#SendOrdersToDriversButton').click(function (e) {
            e.preventDefault();
            var filterData = _dtHelper.getFilterData();
            _sendOrdersToDriversModal.open({
                deliveryDate: filterData.date,
                shift: filterData.shift,
                selectedOffices: [
                    {
                        id: filterData.officeId,
                        name: filterData.officeName
                    }
                ]
            });
        });

        $('#AddJobButton').click(function (e) {
            e.preventDefault();
            var filterData = _dtHelper.getFilterData();
            var date = filterData.date;
            if (abp.setting.getBoolean('App.DispatchingAndMessaging.DefaultDesignationToMaterialOnly')) {
                date = moment().format('L');
            }
            _createOrEditJobModal.open({
                deliveryDate: date,
                shift: filterData.shift,
                officeId: filterData.officeId,
                officeName: filterData.officeName
            });
        });

        $("#PrintScheduleButton").click(async function (e) {
            e.preventDefault();
            var printOptions = await _specifyPrintOptionsModal.open().then((modal, modalObject) => {
                return modalObject.getResultPromise();
            });

            var date = _dtHelper.getFilterData().date;
            var reportParams = {
                date: date,
                ...printOptions
            };
            _orderService.doesOrderSummaryReportHaveData(reportParams).done(function (result) {
                if (!result) {
                    abp.message.warn('There are no orders to print for ' + date + '.');
                    return;
                }
                window.open(abp.appPath + 'app/orders/GetOrderSummaryReport?' + $.param(reportParams));
            });
        });

        $("#PrintAllOrdersButton").click(async function (e) {
            e.preventDefault();
            var printOptions = await _specifyPrintOptionsModal.open().then((modal, modalObject) => {
                return modalObject.getResultPromise();
            });

            var date = _dtHelper.getFilterData().date;
            var reportParams = {
                date: date,
                splitRateColumn: true,
                ...printOptions
            };
            _orderService.doesWorkOrderReportHaveData(reportParams).done(function (result) {
                if (!result) {
                    abp.message.warn('There are no orders to print for ' + date + '.');
                    return;
                }
                window.open(abp.appPath + 'app/orders/GetWorkOrderReport?' + $.param(reportParams));
            });
        });

        $('#ScheduleTable tbody tr').contextmenu({ 'target': '#context-menu' });

        scheduleTable.contextMenu({
            selector: 'tbody tr',
            zIndex: 103,
            events: {
                show: function (options) {
                    var rowData = _dtHelper.getRowData(this);
                    if ($.isEmptyObject(rowData) || !actionMenuHasItems()) {
                        return false;
                    }
                    return true;
                }
            },
            items: {
                editJob: {
                    name: app.localize('EditJob'),
                    visible: function () {
                        var rowData = _dtHelper.getRowData(this);
                        return _permissions.edit;
                    },
                    callback: function () {
                        var orderLine = _dtHelper.getRowData(this);
                        _createOrEditJobModal.open({
                            orderLineId: orderLine.id
                        });
                    }
                },
                orderGroup: {
                    name: app.localize('Schedule_DataTable_MenuItems_Order'),
                    visible: function () {
                        var rowData = _dtHelper.getRowData(this);
                        return menuFunctions.isVisible.viewEdit(rowData) ||
                            menuFunctions.isVisible.markComplete(rowData) ||
                            menuFunctions.isVisible.reOpenJob(rowData) ||
                            menuFunctions.isVisible.copy(rowData) ||
                            menuFunctions.isVisible.share(rowData) ||
                            menuFunctions.isVisible.transfer(rowData) ||
                            menuFunctions.isVisible.sendOrderToLeaseHauler(rowData) ||
                            menuFunctions.isVisible.changeDate(rowData) ||
                            menuFunctions.isVisible.delete(rowData);
                    },
                    items: {
                        viewEdit: {
                            name: app.localize('Schedule_DataTable_MenuItems_ViewEdit'),
                            visible: function () {
                                return menuFunctions.isVisible.viewEdit(_dtHelper.getRowData(this));
                            },
                            callback: function () {
                                menuFunctions.fn.viewEdit(this);
                            }
                        },
                        markComplete: {
                            name: app.localize('Schedule_DataTable_MenuItems_MarkComplete'),
                            visible: function () {
                                return menuFunctions.isVisible.markComplete(_dtHelper.getRowData(this));
                            },
                            callback: function () {
                                menuFunctions.fn.markComplete(this, { isCancelled: false });
                            }
                        },
                        cancel: {
                            name: app.localize('Cancel'),
                            visible: function () {
                                return menuFunctions.isVisible.markComplete(_dtHelper.getRowData(this));
                            },
                            callback: function () {
                                menuFunctions.fn.markComplete(this, { isCancelled: true });
                            }
                        },
                        reOpenJob: {
                            name: app.localize('Schedule_DataTable_MenuItems_ReOpenJob'),
                            visible: function () {
                                return menuFunctions.isVisible.reOpenJob(_dtHelper.getRowData(this));
                            },
                            callback: function () {
                                menuFunctions.fn.reOpenJob(this);
                            }
                        },
                        copy: {
                            name: app.localize('Schedule_DataTable_MenuItems_Copy'),
                            visible: function () {
                                return menuFunctions.isVisible.copy(_dtHelper.getRowData(this));
                            },
                            callback: function () {
                                menuFunctions.fn.copy(this);
                            }
                        },
                        share: {
                            name: app.localize('Schedule_DataTable_MenuItems_Share'),
                            visible: function () {
                                return menuFunctions.isVisible.share(_dtHelper.getRowData(this));
                            },
                            callback: function () {
                                menuFunctions.fn.share(this);
                            }
                        },
                        transfer: {
                            name: app.localize('Schedule_DataTable_MenuItems_Transfer'),
                            visible: function () {
                                return menuFunctions.isVisible.transfer(_dtHelper.getRowData(this));
                            },
                            callback: function () {
                                menuFunctions.fn.transfer(this);
                            }
                        },
                        sendOrderToLeaseHauler: {
                            name: app.localize('SendOrderToLeaseHauler'),
                            visible: function () {
                                return menuFunctions.isVisible.sendOrderToLeaseHauler(_dtHelper.getRowData(this));
                            },
                            callback: function () {
                                menuFunctions.fn.sendOrderToLeaseHauler(this);
                            }
                        },
                        changeDate: {
                            name: app.localize('Schedule_DataTable_MenuItems_ChangeDate'),
                            visible: function () {
                                return menuFunctions.isVisible.changeDate(_dtHelper.getRowData(this));
                            },
                            callback: function () {
                                menuFunctions.fn.changeDate(this);
                            }
                        },
                        reassignTrucks: {
                            name: app.localize('Schedule_DataTable_MenuItems_ReassignTrucks'),
                            visible: function () {
                                return menuFunctions.isVisible.reassignTrucks(_dtHelper.getRowData(this));
                            },
                            callback: function () {
                                menuFunctions.fn.reassignTrucks(this);
                            }
                        },
                        delete: {
                            name: app.localize('Schedule_DataTable_MenuItems_Delete'),
                            visible: function () {
                                return menuFunctions.isVisible.delete(_dtHelper.getRowData(this));
                            },
                            callback: function () {
                                menuFunctions.fn.delete(this);
                            }
                        }
                    }
                },
                printGroup: {
                    name: app.localize('Schedule_DataTable_MenuItems_PrintOrder'),
                    visible: function () {
                        return hasOrderPrintPermissions();
                    },
                    items: {
                        printWithoutPrices: {
                            name: app.localize('Schedule_DataTable_MenuItems_NoPrices'),
                            callback: function () {
                                menuFunctions.fn.printNoPrices(this);
                            }
                        },
                        printCombinedPrices: {
                            name: app.localize('Schedule_DataTable_MenuItems_CombinedPrices'),
                            callback: function () {
                                menuFunctions.fn.printCombinedPrices(this);
                            }
                        },
                        printSeparatePrices: {
                            name: app.localize('Schedule_DataTable_MenuItems_SeparatePrices'),
                            callback: function () {
                                menuFunctions.fn.printSeparatePrices(this);
                            }
                        },
                        printForBackOffice: {
                            name: app.localize('Schedule_DataTable_MenuItems_BackOfficeDetail'),
                            visible: function () {
                                return true;
                            },
                            callback: function () {
                                menuFunctions.fn.printBackOfficeDetail(this);
                            }
                        },
                        printWithDeliveryInfo: {
                            name: app.localize('Schedule_DataTable_MenuItems_WithDeliveryInfo'),
                            visible: function () {
                                return true;
                            },
                            callback: function () {
                                menuFunctions.fn.printWithDeliveryInfo(this);
                            }
                        }
                    }
                },
                assignTrucks: {
                    name: app.localize('AssignTrucks'),
                    visible: function () {
                        var orderLine = _dtHelper.getRowData(this);
                        return hasOrderEditPermissions() && isAllowedToEditOrder(orderLine) && isTodayOrFutureDate(orderLine);
                    },
                    callback: function () {
                        var orderLine = _dtHelper.getRowData(this);
                        var popupOptions = {
                            orderLineId: orderLine.id
                        };
                        $.extend(popupOptions, _dtHelper.getFilterData());
                        _assignTrucksModal.open(popupOptions);
                    }
                },
                changeOrderLineUtilization: {
                    name: app.localize('ChangeUtilization'),
                    visible: function () {
                        var orderLine = _dtHelper.getRowData(this);
                        return hasOrderEditPermissions() && isAllowedToEditOrder(orderLine) && isTodayOrFutureDate(orderLine)
                            && !orderLine.isClosed && orderLine.trucks.length > 0 &&
                            thereAreNotDoneTrucks(orderLine);
                    },
                    callback: function () {
                        var orderLine = _dtHelper.getRowData(this);
                        _changeOrderLineUtilizationModal.open({ id: orderLine.id });
                    }
                },
                tickets: {
                    name: app.localize('Schedule_DataTable_MenuItems_Tickets'),
                    visible: function () {
                        return menuFunctions.isVisible.tickets(_dtHelper.getRowData(this));
                    },
                    callback: function () {
                        menuFunctions.fn.tickets(this);
                    }
                },
                showMap: {
                    name: app.localize('Schedule_DataTable_MenuItems_ShowMap'),
                    visible: function () {
                        return menuFunctions.isVisible.showMap(_dtHelper.getRowData(this));
                    },
                    callback: function () {
                        menuFunctions.fn.showMap(this);
                    }
                },
                showTripsReport: {
                    name: app.localize('Schedule_DataTable_MenuItems_ShowTripsReport'),
                    visible: function () {
                        return menuFunctions.isVisible.showTripsReport(_dtHelper.getRowData(this));
                    },
                    callback: function () {
                        menuFunctions.fn.showTripsReport(this);
                    }
                },
                showCycleTimes: {
                    name: app.localize('Schedule_DataTable_MenuItems_ShowCycleTimes'),
                    visible: function () {
                        return menuFunctions.isVisible.showCycleTimes(_dtHelper.getRowData(this));
                    },
                    callback: function () {
                        menuFunctions.fn.showCycleTimes(this);
                    }
                },
                dispatchToDriver: {
                    name: app.localize('Schedule_DataTable_MenuItems_DispatchToDriver'),
                    visible: function () {
                        return menuFunctions.isVisible.dispatchToDriver(_dtHelper.getRowData(this));
                    },
                    callback: function () {
                        menuFunctions.fn.dispatchToDriver(this);
                    }
                },
                messageToDriver: {
                    name: app.localize('Schedule_DataTable_MenuItems_MessageToDriver'),
                    visible: function () {
                        return menuFunctions.isVisible.messageToDriver(_dtHelper.getRowData(this));
                    },
                    callback: function () {
                        menuFunctions.fn.messageToDriver(this);
                    }
                },
                viewDispatches: {
                    name: app.localize('Schedule_DataTable_MenuItems_ViewDispatches'),
                    visible: function () {
                        return menuFunctions.isVisible.viewDispatches(_dtHelper.getRowData(this));
                    },
                    callback: function () {
                        menuFunctions.fn.viewDispatches(this);
                    }
                },
                activateClosedTrucks: {
                    name: 'Activate closed truck',
                    visible: function () {
                        return menuFunctions.isVisible.activateClosedTrucks(_dtHelper.getRowData(this));
                    },
                    callback: function () {
                        menuFunctions.fn.activateClosedTrucks(this);
                    }
                },
                /*viewJobSummary: {
                    name: app.localize('Schedule_DataTable_MenuItems_JobSummary'),
                    visible: function () {
                        return menuFunctions.isVisible.viewJobSummary(_dtHelper.getRowData(this));
                    },
                    callback: function () {
                        menuFunctions.fn.viewJobSummary(this);
                    }
                },*/
            }
        });

        scheduleTable.contextMenu({
            selector: '.truck-tag',
            zIndex: 103,
            events: {
                show: function () {
                    var orderLine = _dtHelper.getRowData(this);
                    if (!hasOrderEditPermissions() || !isAllowedToEditOrderTrucks(orderLine)) {
                        return false;
                    }
                    return true;
                }
            },
            items: {
                dispatchToDriver: {
                    name: app.localize('Schedule_DataTable_MenuItems_DispatchToDriver'),
                    visible: function () {
                        var truck = $(this).data('item');
                        var rowData = _dtHelper.getRowData(this);
                        var today = new Date(moment().format("YYYY-MM-DD") + 'T00:00:00Z');
                        return showDispatchItems && !rowData.isClosed && rowData.trucks.length > 0 &&
                            !truck.isDone && truck.vehicleCategory.isPowered &&
                            (new Date(rowData.date)).getTime() >= today.getTime()
                            && isOrderLineBelongToOrSharedWithUsersOffice(rowData);
                    },
                    callback: function () {
                        var orderLineId = _dtHelper.getRowData(this).id;
                        var orderLineTruckId = $(this).data('item').id;
                        sendDispatchMessage({
                            orderLineId: orderLineId,
                            selectedOrderLineTruckId: orderLineTruckId
                        });
                    }
                },
                messageToDriver: {
                    name: app.localize('Schedule_DataTable_MenuItems_MessageToDriver'),
                    visible: function () {
                        var truck = $(this).data('item');
                        return allowSmsMessages && _permissions.driverMessages && !truck.isDone && truck.vehicleCategory.isPowered;
                    },
                    callback: function () {
                        var orderLineId = _dtHelper.getRowData(this).id;
                        var driverId = $(this).data('item').driverId;
                        _sendDriverMessageModal.open({
                            orderLineId: orderLineId,
                            selectedDriverId: driverId
                        });
                    }
                },
                changeUtilization: {
                    name: app.localize('ChangeUtilization'),
                    visible: function () {
                        var truck = $(this).data('item');
                        return !truck.isDone && truck.vehicleCategory.assetType !== abp.enums.assetType.trailer
                            && truck.vehicleCategory.isPowered;
                    },
                    callback: function () {
                        var truck = $(this).data('item');
                        _setTruckUtilizationModal.open({ id: truck.id }).fail(handlePopupException);
                    }
                },
                viewDispatches: {
                    name: app.localize('Schedule_DataTable_MenuItems_ViewDispatches'),
                    visible: function () {
                        var truck = $(this).data('item');
                        return showDispatchViaSmsItems && truck.vehicleCategory.assetType !== abp.enums.assetType.trailer;
                    },
                    callback: function () {
                        var orderLineId = _dtHelper.getRowData(this).id;
                        var truckId = $(this).data('item').truckId;
                        window.location.href = abp.appPath + 'app/Dispatches/?orderLineId=' + orderLineId + '&truckId=' + truckId;
                    }
                },
                deleteOrderLineTruck: {
                    name: app.localize('RemoveTruckFromJob'),
                    visible: function () {
                        var truck = $(this).data('item');
                        return !truck.isDone;
                    },
                    callback: function () {
                        var truck = $(this).data('item');
                        var editor = $(this).closest('td').find('.truck-cell-editor');
                        editor.tagsinput('remove', truck);
                    }
                },
                changeDriverForOrderLineTruck: {
                    name: app.localize('ChangeDriver'),
                    visible: function () {
                        var truck = $(this).data('item');
                        return hasTrucksPermissions()
                            && truck.vehicleCategory.isPowered;
                    },
                    callback: function () {
                        var orderLineTruckId = $(this).data('item').id;
                        _changeDriverForOrderLineTruckModal.open({ orderLineTruckId: orderLineTruckId });
                    }
                }
            }
        });

        $.contextMenu({
            selector: '.truck-tile',
            zIndex: 103,
            events: {
                show: function (options) {
                    if (!hasTrucksPermissions()) {
                        return false;
                    }
                    var anyItemIsVisible = false;
                    for (var itemName in options.items) {
                        if (!options.items.hasOwnProperty(itemName)) {
                            continue;
                        }
                        var item = options.items[itemName];

                        if (item.visible.apply(this)) {
                            anyItemIsVisible = true;
                            break;
                        }
                    }
                    if (!anyItemIsVisible) {
                        return false;
                    }
                    //var truck = $(this).data('truck');
                    //if (!abp.session.officeId || $("#OfficeIdFilter").val() !== abp.session.officeId.toString() 
                    //    || truck.officeId !== abp.session.officeId && (truck.sharedWithOfficeId !== abp.session.officeId || isPastDate())
                    //) {
                    //    return false;
                    //}
                    return true;
                }
            },
            items: {
                notOutOfService: {
                    name: 'Place back in service',
                    visible: function () {
                        var truck = $(this).data('truck');
                        return !truck.isExternal
                            && !truck.alwaysShowOnSchedule
                            && truck.isOutOfService; // && truck.officeId === abp.session.officeId;
                    },
                    callback: function () {
                        var truck = $(this).data('truck'); //category, hasNoDriver, id, isOutOfService, officeId, truckCode, utilization, utilizationList
                        setTruckIsOutOfServiceValue(truck.id, false);
                    }
                },
                outOfService: {
                    name: 'Place out of service',
                    visible: function () {
                        var truck = $(this).data('truck');
                        return !truck.isExternal
                            && !truck.alwaysShowOnSchedule
                            && !truck.isOutOfService; //&& truck.officeId === abp.session.officeId;
                    },
                    callback: function () {
                        var truck = $(this).data('truck');
                        setTruckIsOutOfServiceValue(truck.id, true);
                    }
                },
                noDriverForTruck: {
                    name: 'No driver for truck',
                    visible: function () {
                        var truck = $(this).data('truck');
                        return !truck.isExternal &&
                            !truckHasNoDriver(truck) &&
                            truck.vehicleCategory.isPowered &&
                            !isPastDate();
                    },
                    callback: function () {
                        var truck = $(this).data('truck');
                        var date = _dtHelper.getFilterData().date;
                        _setNoDriverForTruckModal.open({
                            truckId: truck.id,
                            startDate: date,
                            endDate: date
                        });
                    }
                },
                assignDriverForTruck: {
                    name: 'Assign driver',
                    visible: function () {
                        var truck = $(this).data('truck');
                        return !truck.isExternal
                            && truckHasNoDriver(truck)
                            && truck.vehicleCategory.isPowered;
                    },
                    callback: function () {
                        var truck = $(this).data('truck');
                        var filterData = _dtHelper.getFilterData();
                        _assignDriverForTruckModal.open({
                            truckId: truck.id,
                            truckCode: truck.truckCode,
                            leaseHaulerId: truck.leaseHaulerId,
                            date: filterData.date,
                            shift: filterData.shift,
                            officeId: filterData.officeId
                        });
                    }
                },
                changeDriverForTruck: {
                    name: 'Change driver',
                    visible: function () {
                        var truck = $(this).data('truck');
                        return (truck.isExternal || !truckHasNoDriver(truck))
                            && truck.vehicleCategory.isPowered;
                    },
                    callback: function () {
                        var truck = $(this).data('truck');
                        var filterData = _dtHelper.getFilterData();
                        _assignDriverForTruckModal.open({
                            truckId: truck.id,
                            truckCode: truck.truckCode,
                            leaseHaulerId: truck.leaseHaulerId,
                            date: filterData.date,
                            shift: filterData.shift,
                            officeId: filterData.officeId,
                            driverId: truck.driverId,
                            driverName: truck.driverName
                        });
                    }
                },
                defaultDriverForTruck: {
                    name: 'Assign default driver back to truck',
                    visible: function () {
                        var truck = $(this).data('truck');
                        return !truck.isExternal && truck.hasNoDriver && truck.hasDefaultDriver && !isPastDate();
                    },
                    callback: function () {
                        var truck = $(this).data('truck');
                        var date = _dtHelper.getFilterData().date;
                        _setDefaultDriverForTruckModal.open({
                            truckId: truck.id,
                            startDate: date,
                            endDate: date
                        });
                    }
                },
                removeLhTruckFromSchedule: {
                    name: 'Remove from schedule',
                    visible: function () {
                        var truck = $(this).data('truck');
                        return truck.isExternal && !truckHasOrderLineTrucks(truck);
                    },
                    callback: async function () {
                        var truck = $(this).data('truck');
                        var filterData = _dtHelper.getFilterData();
                        await abp.services.app.leaseHaulerRequestEdit.removeAvailableLeaseHaulerTruckFromSchedule({
                            truckId: truck.id,
                            date: filterData.date,
                            shift: filterData.shift,
                            officeId: filterData.officeId,
                        });
                        abp.notify.info('Successfully removed.');
                        reloadTruckTiles();
                        reloadDriverAssignments();
                    }
                },
                separator1: {
                    "type": "cm_separator",
                    visible: function () {
                        var truck = $(this).data('truck');
                        return !truck.isExternal && !truck.alwaysShowOnSchedule
                            && (_features.allowMultiOffice && truck.sharedWithOfficeId === null && !truck.isOutOfService
                            || truck.sharedWithOfficeId !== null && truck.sharedWithOfficeId !== abp.session.officeId);
                    }
                },
                addSharedTruck: {
                    name: 'Share',
                    visible: function () {
                        var truck = $(this).data('truck');
                        return !truck.isExternal && !truck.alwaysShowOnSchedule
                            && _features.allowMultiOffice && truck.sharedWithOfficeId === null && !truck.isOutOfService;
                    },
                    callback: function () {
                        var truck = $(this).data('truck');
                        var date = _dtHelper.getFilterData().date;
                        _addSharedTruckModal.open({
                            truckId: truck.id,
                            startDate: date,
                            endDate: date
                        });
                    }
                },
                deleteSharedTruck: {
                    name: 'Revoke Share',
                    visible: function () {
                        var truck = $(this).data('truck');
                        return !truck.isExternal && !truck.alwaysShowOnSchedule
                            && truck.sharedWithOfficeId !== null && truck.sharedWithOfficeId !== abp.session.officeId;
                    },
                    disabled: function () {
                        var truck = $(this).data('truck');
                        return truck.officeId !== abp.session.officeId || truck.actualUtilization !== 0;
                    },
                    callback: async function () {
                        var truck = $(this).data('truck');
                        var date = _dtHelper.getFilterData().date;
                        if (await abp.message.confirm('Are you sure you want to revoke the share for the truck for selected date?')) {
                            _truckService.deleteSharedTruck({
                                truckId: truck.id,
                                date: date
                            }).done(function () {
                                abp.notify.info('Successfully revoked.');
                                reloadMainGrid(null, false);
                                reloadTruckTiles();
                                reloadDriverAssignments();
                            });
                        }
                    }
                }
            }
        });

    });
})();