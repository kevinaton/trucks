(function ($) {
    app.modals.JobSummaryModal = function () {

        var _schedulingService = abp.services.app.scheduling;
        var _modalManager;
        var _modal;

        var _visGroups;
        var _visItems;

        const pad = (number, length) => {
            var str = '' + number;
            while (str.length < length) {
                str = '0' + str;
            }
            return str;
        };

        const YYYYMMDDHHMMSS = (date) => {
            var yyyy = date.getFullYear().toString();
            var MM = pad(date.getMonth() + 1, 2);
            var dd = pad(date.getDate(), 2);
            var hh = pad(date.getHours(), 2);
            var mm = pad(date.getMinutes(), 2)
            var ss = pad(date.getSeconds(), 2)
            //return `${yyyy}-${MM}-${dd} ${hh}:${mm}:${ss}`;
            return `${hh}:${mm}:${ss}`;
        };

        const renderTrucksActivityTimeLine = (orderTrucksData) => {

            _visGroups = new vis.DataSet();
            _visItems = new vis.DataSet();

            const pad = (num, size) => {
                num = num.toString();
                while (num.length < size) num = "0" + num;
                return num;
            };

            const getElapsedTime = (startTime, endTime) => {
                // compute and strip the miliseconds
                var timeDiff = endTime - startTime;
                timeDiff /= 1000;
                // get and remove seconds from the date
                var seconds = Math.round(timeDiff % 60);
                timeDiff = Math.floor(timeDiff / 60);
                // get and remove minutes from the date
                var minutes = Math.round(timeDiff % 60);
                timeDiff = Math.floor(timeDiff / 60);
                // get and remove hours from the date
                var hours = Math.round(timeDiff % 24);
                timeDiff = Math.floor(timeDiff / 24);
                // the rest of timeDiff is number of days
                var days = timeDiff;
                return `${days} days, ${pad(hours, 2)}:${pad(minutes, 2)}:${pad(seconds, 2)}`;
            };

            for (var i = 0; i < orderTrucksData.orderTrucks.length; i++) {

                var orderTruck = orderTrucksData.orderTrucks[i];
                var idx = i + 1;

                _visGroups.add({
                    id: `${idx}-${orderTruck.truckId}`,
                    order: idx,
                    orderTruck: orderTruck,
                });

                for (var x = 0; x < orderTruck.tripCycles.length; x++) {
                    var cycle = orderTruck.tripCycles[x];
                    var startDateTime = Date.parse(cycle.startDateTime);
                    var endDateTime = Date.parse(cycle.endDateTime);
                    var toolTip = `Start: ${YYYYMMDDHHMMSS(new Date(startDateTime))}<br>End: ${YYYYMMDDHHMMSS(new Date(endDateTime))}<br>Elapsed: ${getElapsedTime(startDateTime, endDateTime)}`;
                    _visItems.add({
                        id: `${idx}-${cycle.cycleId}`,
                        group: `${idx}-${orderTruck.truckId}`,
                        start: startDateTime,
                        end: endDateTime,
                        type: 'range',
                        content: cycle.label,
                        className: cycle.truckTripType === 0 ? "load-trip" : "delivery-trip",
                        title: toolTip
                    });
                }
            }

            var options = {
                stack: true,
                maxHeight: 640,
                horizontalScroll: false,
                verticalScroll: true,
                start: orderTrucksData.earliest,
                end: orderTrucksData.latest,
                orientation: {
                    axis: "both",
                    item: "top"
                },
                groupTemplate: (item, element, data) => {
                    if (!item) return;
                    var truckId = parseInt(item.id.split('-')[1]);
                    var activityData = orderTrucksData.orderTrucks.filter(p => p.truckId === truckId)[0];

                    var grid = document.createElement('table');
                    grid.className = "jobsummarymodal-sidepanel";
                    var gridRow = grid.insertRow(0);
                    gridRow.className = "grid-data";
                    var cell1 = gridRow.insertCell(0);
                    cell1.textContent = activityData.truckCode;
                    var cell2 = gridRow.insertCell(1);
                    cell2.textContent = activityData.loadsCount;
                    cell2.className = `loads-count-${activityData.unitOfMeasure.replace(/\s+/g, '-').toLowerCase()}`;
                    var cell3 = gridRow.insertCell(2);
                    cell3.textContent = activityData.quantity;
                    cell3.className = `loads-quantity-${activityData.unitOfMeasure.replace(/\s+/g, '-').toLowerCase()}`;
                    var cell4 = gridRow.insertCell(3);
                    cell4.textContent = activityData.unitOfMeasure;
                    return grid;
                },
                tooltip: {
                    followMouse: true,
                    overflowMethod: "cap"
                },
                onInitialDrawComplete: () => {
                    _modal.find("ul.legend").css("display", "block");
                },
                stack: false
            };

            // create a Timeline
            var container = _modal.find('#trucks-timeline')[0];
            var timeLine = new vis.Timeline(container, null, options);
            timeLine.setGroups(_visGroups);
            timeLine.setItems(_visItems);

            const debounce = (func, wait = 100) => {
                let timeout;
                return function (...args) {
                    clearTimeout(timeout);
                    timeout = setTimeout(() => {
                        func.apply(this, args);
                    }, wait);
                };
            }

            let groupFocus = (e) => {
                let visibleGroups = timeLine.getVisibleGroups();
                let visibleItems = visibleGroups.reduce((result, groupId) => {
                    let group = timeLine.itemSet.groups[groupId];
                    if (group.items) {
                        result = result.concat(Object.keys(group.items));
                    }
                    return result;
                }, [])
                timeLine.focus(visibleItems);
            }
            timeLine.on("scroll", debounce(groupFocus, 200));
            timeLine.fit();

            renderGroupHeaderGrids();
            renderGroupFooterGrids();
            renderLegends();

        };

        /* Render the group table header */
        const renderGroupHeaderGrids = () => {
            var headerGrid = document.createElement('table');
            headerGrid.className = "grid-header";
            headerGrid.style.height = "58px";
            var gridRow = headerGrid.insertRow(0);
            var cell1 = gridRow.insertCell(0);
            cell1.textContent = "Truck";
            cell1.style.width = "80px";
            cell1.style.fontWeight = 500;
            var cell2 = gridRow.insertCell(1);
            cell2.textContent = "Loads";
            cell2.style.width = "60px";
            cell2.style.fontWeight = 500;
            var cell3 = gridRow.insertCell(2);
            cell3.textContent = "Qty";
            cell3.style.width = "60px";
            cell3.style.fontWeight = 500;
            var cell4 = gridRow.insertCell(3);
            cell4.textContent = "UOM";
            cell4.style.width = "80px";
            cell4.style.fontWeight = 500;
            _modal.find("#trucks-timeline div.vis-timeline>div:nth-child(1)").prepend(headerGrid.outerHTML);
        };

        /* Render the group totals summary footer */
        const renderGroupFooterGrids = () => {

            var footerGrid = document.createElement('table');
            footerGrid.className = "grid-footer";
            footerGrid.style.height = "100%";
            footerGrid.style.textAlign = "center";

            var groupedTrucks = [];
            var allGroups = _visGroups.get();

            $.each(allGroups, (idx, group) => {

                var groupedTruck = groupedTrucks.filter((g) => g.unitOfMeasure === group.orderTruck.unitOfMeasure)[0];

                if (!groupedTruck) {
                    groupedTruck = {
                        loadsCountSum: group.orderTruck.loadsCount,
                        quantitySum: group.orderTruck.quantity,
                        unitOfMeasure: group.orderTruck.unitOfMeasure,
                    };
                    groupedTrucks.push(groupedTruck);
                }
                else {
                    groupedTruck.loadsCountSum += group.orderTruck.loadsCount;
                    groupedTruck.quantitySum += group.orderTruck.quantity
                }
            });

            var footerGridWrapper = document.createElement('div');
            footerGridWrapper.style.height = "52px";
            footerGridWrapper.style.position = "fixed";

            var drawFooterSummaryRow = (loadsCounts, loadsQuantities, unitOfMeasure, labelTotals) => {
                var gridRow = footerGrid.insertRow(0);
                var cell1 = gridRow.insertCell(0);
                cell1.textContent = labelTotals ? "Totals" : "";
                cell1.style.width = "80px";
                cell1.style.fontWeight = 500;
                var cell2 = gridRow.insertCell(1);
                cell2.textContent = loadsCounts;
                cell2.style.width = "60px";
                cell2.style.fontWeight = 500;
                var cell3 = gridRow.insertCell(2);
                cell3.textContent = loadsQuantities;
                cell3.style.width = "60px";
                cell3.style.fontWeight = 500;
                var cell4 = gridRow.insertCell(3);
                cell4.textContent = unitOfMeasure;
                cell4.style.width = "80px";
                cell4.style.fontWeight = 500;
                footerGridWrapper.appendChild(footerGrid);
            };

            if (!groupedTrucks || groupedTrucks.length === 0) {
                drawFooterSummaryRow(0, 0, "", true);
            }
            else {
                for (var i = 0; i < groupedTrucks.length; i++) {
                    var groupSummary = groupedTrucks[i];
                    drawFooterSummaryRow(groupSummary.loadsCountSum, groupSummary.quantitySum, groupSummary.unitOfMeasure, i === 0);
                }
            }

            _modal.find("#trucks-timeline div.vis-timeline > div.vis-left").append(footerGridWrapper.outerHTML);
        };

        /* Render the legend */
        const renderLegends = () => {
            var legend = document.createElement('div');
            legend.className = "legend-wrapper";
            legend.style.cssText = "display: flex; flex-direction: row; justify-content: center; align-items: center;";
            var refreshLink = document.createElement('a');
            refreshLink.id = "refresh-view";
            refreshLink.text = "Refresh View";
            refreshLink.className = "refresh-view";
            var ulLegend = document.createElement('ul');
            ulLegend.className = "legend";
            var span1 = document.createElement('span');
            span1.textContent = "Travel to load site";
            var li1 = document.createElement("li");
            li1.className = "load-site";
            li1.appendChild(span1);
            ulLegend.appendChild(li1);
            var span2 = document.createElement('span');
            span2.textContent = "Travel to delivery site";
            var li2 = document.createElement("li");
            li2.className = "delivery-site";
            li2.appendChild(span2);
            ulLegend.appendChild(li2);
            legend.appendChild(refreshLink);
            legend.appendChild(ulLegend);
            _modal.find("div.modal-header").append(legend.outerHTML);
        };

        this.init = async function (modalManager) {

            _modalManager = modalManager;
            _modal = _modalManager.getModal();
            let modalContent = _modalManager.getModalContent();

            try {
                _modalManager.setBusy(true);
                abp.ui.setBusy(modalContent);

                var orderLineId = _modal.find("#OrderLineId").val();

                var orderTrucksData = await _schedulingService.getOrderTrucksAndDetails(orderLineId);
                renderTrucksActivityTimeLine(orderTrucksData);
            } catch (e) {
                setTimeout(() => _modalManager.close(), 500);
            } finally {
                _modalManager.setBusy(false);
                abp.ui.clearBusy(modalContent);
            }
        }
    };

})(jQuery);