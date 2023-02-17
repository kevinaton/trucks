﻿(function () {
    $(function () {
        var _hostDashboardService = abp.services.app.hostDashboard;
        var _dtHelper = abp.helper.dataTables;

        var _$container = $("#HostDashboard");
        var _$dateRangePicker = _$container.find(".dashboard-report-range");
        var _$expiringTenantsTable = _$container.find(".expiring-tenants-table");
        var _$recentTenantsTable = _$container.find(".recent-tenants-table");
        var _$seeAllExpiringTenantsButton = _$container.find(".see-all-expiring-tenants");
        var _$seeAllRecentTenantsButton = _$container.find(".see-all-recent-tenants");
        var _$newTenantsStatusTitle = _$container.find(".new-tenants-statistics .status-title");
        var _$newTenantsStatisticsCountPlaceholder = _$container.find(".new-tenants-statistics .new-tenants-count");
        var _$newSubscriptionAmountTitle = _$container.find(".new-subscription-statistics .status-title");
        var _$newSubscriptionAmountPlaceholder = _$container.find(".new-subscription-statistics .new-subscription-amount");
        var _$dashboardStatisticsPlaceholder1 = _$container.find(".dashboard-statistics1 .dashboard-placeholder1");
        var _$dashboardStatisticsPlaceholder2 = _$container.find(".dashboard-statistics2 .dashboard-placeholder2");
        var _$expiringTenantsCaptionHelper = _$container.find(".expiring-tenants .caption-helper");
        var _$recentTenantsCaptionHelper = _$container.find(".recent-tenants .caption-helper");

        var _$kpiPlaceholders = _$container.find(".kpi-statistics .kpi");

        var _$refreshButton = _$container.find("button[name='RefreshButton']");

        var _expiringTenantsData = [], _recentTenantsData = [];

        var _selectedDateRange = {
            startDate: moment().add(-1, 'days').startOf('day'),
            endDate: moment().add(-1, 'days').endOf("day")
        };

        var showSelectedDate = function () {
            if (_$dateRangePicker.attr("data-display-range") !== "0") {
                _$dateRangePicker.find(".m-subheader__daterange-date").text(_selectedDateRange.startDate.format("LL") +
                    " - " +
                    _selectedDateRange.endDate.format("LL"));
            }
        };

        var populateExpiringTenantsTable = function (expiringTenants, subscriptionEndAlertDayCount, maxExpiringTenantsShownCount, subscriptionEndDateStart, subscriptionEndDateEnd) {
            _$expiringTenantsCaptionHelper.text(app.localize("ExpiringTenantsHelpText", subscriptionEndAlertDayCount, maxExpiringTenantsShownCount));
            _expiringTenantsData = expiringTenants;
            _expiringTenantsDataTable.ajax.reload();

            _$seeAllExpiringTenantsButton
                .data("subscriptionEndDateStart", subscriptionEndDateStart)
                .data("subscriptionEndDateEnd", subscriptionEndDateEnd)
                .click(function () {
                    window.open(abp.appPath + "App/Tenants?" +
                        "subscriptionEndDateStart=" + encodeURIComponent($(this).data("subscriptionEndDateStart")) + "&" +
                        "subscriptionEndDateEnd=" + encodeURIComponent($(this).data("subscriptionEndDateEnd")));
                });
        };

        var populateRecentTenantsTable = function (recentTenants, recentTenantsDayCount, maxRecentTenantsShownCount, creationDateStart) {
            _$recentTenantsCaptionHelper.text(app.localize("RecentTenantsHelpText", recentTenantsDayCount, maxRecentTenantsShownCount));
            _recentTenantsData = recentTenants;
            _recentTenantsDataTable.ajax.reload();

            _$seeAllRecentTenantsButton
                .data("creationDateStart", creationDateStart)
                .click(function () {
                    window.open(abp.appPath + "App/Tenants?" +
                        "creationDateStart=" + encodeURIComponent($(this).data("creationDateStart")));
                });
        };

        var getCurrentDateRangeText = function () {
            return _selectedDateRange.startDate.format("L") + " - " + _selectedDateRange.endDate.format("L");
        };

        var writeNewTenantsCount = function (newTenantsCount) {
            _$newTenantsStatusTitle.text(getCurrentDateRangeText());
            _$newTenantsStatisticsCountPlaceholder.text(newTenantsCount);
        };

        var writeNewSubscriptionsAmount = function (newSubscriptionAmount) {
            _$newSubscriptionAmountTitle.text(getCurrentDateRangeText());
            _$newSubscriptionAmountPlaceholder.text(newSubscriptionAmount);
        };

        //this is a sample placeholder. You can put your own statistics here.
        var writeDashboardPlaceholder1 = function (value) {
            _$dashboardStatisticsPlaceholder1.text(value);
        };

        //this is a sample placeholder. You can put your own statistics here.
        var writeDashboardPlaceholder2 = function (value) {
            _$dashboardStatisticsPlaceholder2.text(value);
        };

        var writeKpi = function (kpi) {
            $.each(kpi, function (kpiName, kpiValue) {
                _$kpiPlaceholders.filter('.kpi-' + kpiName).text(kpiValue);
            });
        };

        var getAllDataAndDrawCharts = function () {
            abp.ui.setBusy(_$container);
            var dashboardStatisticsData = _hostDashboardService
                .getDashboardStatisticsData({
                    startDate: _selectedDateRange.startDate,
                    endDate: _selectedDateRange.endDate
                });
            var dashboardKpiData = _hostDashboardService
                .getDashboardKpiData({
                    startDate: _selectedDateRange.startDate.format("YYYY-MM-DDT00:00:00"),
                    endDate: _selectedDateRange.endDate.format("YYYY-MM-DDT23:59:59.999")
                });
            $.when(dashboardStatisticsData, dashboardKpiData).done(function (result, kpiResult) {
                result = result[0];
                kpiResult = kpiResult[0];
                //counts
                writeNewTenantsCount(result.newTenantsCount);
                writeNewSubscriptionsAmount(result.newSubscriptionAmount);
                writeDashboardPlaceholder1(result.dashboardPlaceholder1);
                writeDashboardPlaceholder2(result.dashboardPlaceholder2);

                writeKpi(kpiResult);

                //tables
                populateExpiringTenantsTable(
                    result.expiringTenants,
                    result.subscriptionEndAlertDayCount,
                    result.maxExpiringTenantsShownCount,
                    result.subscriptionEndDateStart,
                    result.subscriptionEndDateEnd
                );

                populateRecentTenantsTable(
                    result.recentTenants,
                    result.recentTenantsDayCount,
                    result.maxRecentTenantsShownCount,
                    result.tenantCreationStartDate
                );
            }).always(function () {
                abp.ui.clearBusy(_$container);
            });
        };

        function initRequestsTable() {
            $('#TableOfRequests').DataTableInit({
                paging: false,
                serverSide: false,
                processing: false,
                info: false
            });
        }

        var $recentlyActiveUsersTable = $('#RecentlyActiveUsersTable');
        var $showRecentlyActiveUsersButton = $('#ShowRecentlyActiveUsersButton');
        $showRecentlyActiveUsersButton.click(function () {
            $showRecentlyActiveUsersButton.off('click').slideUp();
            $recentlyActiveUsersTable.closest('.m-portlet__body').slideDown();
            $recentlyActiveUsersTable.DataTableInit({
                paging: false,
                ordering: false,
                info: false,
                ajax: function (data, callback, settings) {
                    _hostDashboardService.getMostRecentActiveUsers().done(function (abpResult) {
                        callback(_dtHelper.fromAbpResult(abpResult));
                    });
                },
                columns: [
                    {
                        data: 'fullName',
                        title: 'User full name',
                        targets: 0
                    },
                    {
                        data: 'tenancyName',
                        title: 'Tenant name',
                        targets: 1
                    },
                    {
                        data: 'lastTransaction',
                        title: 'Time of the last transaction',
                        render: function (data, type, full, meta) { return _dtHelper.renderDateTime(data); },
                        targets: 2
                    },
                    {
                        data: 'numberOfTransactions',
                        title: 'Number of transactions in the last hour',
                        targets: 3
                    }
                ]

            });
        });


        $('#EditionIdFilter').select2Init({
            allowClear: true,
            abpServiceMethod: abp.services.app.edition.getEditionsSelectList,
            showAll: true
        }).change(function () {
            var resetPaging = true;
            var callback = null;
            $tenantStatisticsGrid.ajax.reload(callback, resetPaging);
        });

        $('#StatusFilter').select2Init({
            allowClear: false,
            showAll: true
        }).change(function () {
            var resetPaging = true;
            var callback = null;
            $tenantStatisticsGrid.ajax.reload(callback, resetPaging);
        });

        var _tenantStatisticsTotal = {};
        var $tenantStatisticsTable = $('#TenantStatisticsTable');
        var $tenantStatisticsGrid = $tenantStatisticsTable.DataTableInit({
            //paging: false,
            //ordering: false,
            //info: false,
            pageLength: 20,
            ajax: function (data, callback, settings) {
                var abpData = _dtHelper.toAbpData(data);
                $.extend(abpData, {
                    startDate: _selectedDateRange.startDate.format("YYYY-MM-DDT00:00:00"),
                    endDate: _selectedDateRange.endDate.format("YYYY-MM-DDT23:59:59.999"),
                    editionId: $('#EditionIdFilter').val(),
                    status: $('#StatusFilter').val()
                });
                _hostDashboardService.getTenantStatistics(abpData).done(function (abpResult) {
                    _tenantStatisticsTotal = abpResult.total;
                    callback(_dtHelper.fromAbpResult(abpResult));
                });
            },
            "footerCallback": function (row, data, start, end, display) {
                var api = this.api();

                // Remove the formatting to get integer data for summation
                var intVal = function (i) {
                    return typeof i === 'string' ?
                        i.replace(/[\$,]/g, '') * 1 :
                        typeof i === 'number' ?
                            i : 0;
                };

                var sumIntColumn = function (colIndex) {
                    return api.column(colIndex, { page: 'current' }).data().reduce(function (a, b) { return intVal(a) + intVal(b); }, 0);
                };

                // Update footer
                $(api.column(0).footer()).text('Total');
                for (var i = 1; i <= 8; i++) {
                    let column = api.column(i);
                    $(column.footer()).text(_tenantStatisticsTotal && _tenantStatisticsTotal[column.dataSrc()] || '0');
                    //$(api.column(i).footer()).text(sumIntColumn(i));
                }
            },
            columns: [
                {
                    data: 'tenantName',
                    title: 'Tenant name',
                    targets: 0
                },
                {
                    data: 'numberOfTrucks',
                    title: '# Trucks',
                    targets: 1
                },
                {
                    data: 'numberOfUsers',
                    title: '# Users',
                    targets: 2
                },
                {
                    data: 'numberOfUsersActive',
                    title: '# Users Active',
                    targets: 3
                },
                {
                    data: 'orderLines',
                    title: 'Order lines',
                    targets: 4
                },
                {
                    data: 'trucksScheduled',
                    title: 'Trucks Sched',
                    targets: 5
                },
                {
                    data: 'leaseHaulersOrderLines',
                    title: 'LH Trucks Sched',
                    targets: 6
                },
                {
                    data: 'ticketsCreated',
                    title: 'Tickets',
                    targets: 7
                },
                {
                    data: 'smsSent',
                    title: 'SMS',
                    targets: 8
                }
            ]

        });

        var $requestsTable = $('#RequestsTable');
        $requestsTable.DataTableInit({
            //paging: false,
            //ordering: false,
            //info: false,
            order: [[4, 'desc']],
            ajax: function (data, callback, settings) {
                var abpData = _dtHelper.toAbpData(data);
                _hostDashboardService.getRequests(abpData).done(function (abpResult) {
                    callback(_dtHelper.fromAbpResult(abpResult));
                });
            },
            columns: [
                {
                    data: 'serviceAndMethodName',
                    title: 'Request name',
                    render: function (data, type, full, meta) { return _dtHelper.renderText(full.serviceName + '.' + full.methodName); },
                    targets: 0
                },
                {
                    data: 'averageExecutionDuration',
                    title: 'Ave. Exec. Time',
                    targets: 1
                },
                {
                    data: 'numberOfTransactions',
                    title: '# yesterday',
                    targets: 2
                },
                {
                    data: 'lastWeekNumberOfTransactions',
                    title: '# in last week',
                    targets: 3
                },
                {
                    data: 'lastMonthNumberOfTransactions',
                    title: '# in last month',
                    targets: 4
                }
            ]

        });

        var _expiringTenantsDataTable = null;
        var initExpiringTenantsTable = function () {
            _expiringTenantsDataTable = _$expiringTenantsTable.DataTableInit({
                paging: false,
                serverSide: false,
                processing: false,
                info: false,
                listAction: {
                    ajaxFunction: function () {
                        return $.Deferred(function ($dfd) {
                            $dfd.resolve({
                                "items": _expiringTenantsData,
                                "totalCount": _expiringTenantsData.length
                            });
                        });
                    }
                },
                columns: [
                    {
                        targets: 0,
                        data: "tenantName"
                    },
                    {
                        targets: 1,
                        data: "remainingDayCount"
                    }
                ]
            });
        };

        var _recentTenantsDataTable = null;
        var initRecentTenantsTable = function () {
            _recentTenantsDataTable = _$recentTenantsTable.DataTableInit({
                paging: false,
                serverSide: false,
                processing: false,
                info: false,
                listAction: {
                    ajaxFunction: function () {
                        return $.Deferred(function ($dfd) {
                            $dfd.resolve({
                                "items": _recentTenantsData,
                                "totalCount": _recentTenantsData.length
                            });
                        });
                    }
                },
                columns: [
                    {
                        targets: 0,
                        data: "name"
                    },
                    {
                        targets: 1,
                        data: "creationTime",
                        render: function (creationTime) {
                            return moment(creationTime).format("L LT");
                        }
                    }
                ]
            });
        };

        var refreshAllData = function () {
            showSelectedDate();
            getAllDataAndDrawCharts();
        };

        var reloadTenantStatisticsGrid = function () {
            $tenantStatisticsGrid.ajax.reload();
        };

        var initialize = function () {
            initExpiringTenantsTable();
            initRecentTenantsTable();
            refreshAllData();
        };

        _$dateRangePicker.daterangepicker(
            $.extend(true, app.createDateRangePickerOptions(), _selectedDateRange), function (start, end, label) {
                _selectedDateRange.startDate = start;
                _selectedDateRange.endDate = end;
                refreshAllData();
                reloadTenantStatisticsGrid();
            });

        _$refreshButton.click(function () {
            refreshAllData();
            reloadTenantStatisticsGrid();
        });

        initialize();

        $('#ExportRequestsToCsvButton').click(function () {
            var $button = $(this);
            abp.ui.setBusy($button);
            _hostDashboardService
                .getRequestsToCsv({})
                .done(function (result) {
                    app.downloadTempFile(result);
                }).always(function () {
                    abp.ui.clearBusy($button);
                });
        });

    });
})();