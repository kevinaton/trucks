using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.Timing;
using DispatcherWeb.Authorization;
using DispatcherWeb.DailyHistory;
using DispatcherWeb.Dto;
using DispatcherWeb.MultiTenancy.HostDashboard.Dto;
using DispatcherWeb.MultiTenancy.HostDashboard.Exporting;
using DispatcherWeb.MultiTenancy.Payments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.MultiTenancy.HostDashboard
{
    [DisableAuditing]
    [AbpAuthorize(AppPermissions.Pages_Administration_Host_Dashboard)]
    public class HostDashboardAppService : DispatcherWebAppServiceBase, IHostDashboardAppService
    {
        private const int Top20Number = 20;
        private const int SubscriptionEndAlertDayCount = 30;
        private const int MaxExpiringTenantsShownCount = 10;
        private const int MaxRecentTenantsShownCount = 10;
        private const int RecentTenantsDayCount = 7;

        private readonly ISubscriptionPaymentRepository _subscriptionPaymentRepository;
        private readonly IRepository<Tenant> _tenantRepository;
        private readonly IRepository<TenantDailyHistory> _tenantDailyHistoryRepository;
        private readonly IRepository<UserDailyHistory> _userDailyHistoryRepository;
        private readonly IRepository<TransactionDailyHistory> _transactionDailyHistoryRepository;
        private readonly IRequestsCsvExporter _requestsCsvExporter;
        private readonly IRepository<AuditLog, long> _auditLogRepository;

        public HostDashboardAppService(
            ISubscriptionPaymentRepository subscriptionPaymentRepository,
            IRepository<Tenant> tenantRepository,
            IRepository<TenantDailyHistory> tenantDailyHistoryRepository,
            IRepository<UserDailyHistory> userDailyHistoryRepository,
            IRepository<TransactionDailyHistory> transactionDailyHistoryRepository,
            IRequestsCsvExporter requestsCsvExporter,
            IRepository<AuditLog, long> auditLogRepository
        )
        {
            _subscriptionPaymentRepository = subscriptionPaymentRepository;
            _tenantRepository = tenantRepository;
            _tenantDailyHistoryRepository = tenantDailyHistoryRepository;
            _userDailyHistoryRepository = userDailyHistoryRepository;
            _transactionDailyHistoryRepository = transactionDailyHistoryRepository;
            _requestsCsvExporter = requestsCsvExporter;
            _auditLogRepository = auditLogRepository;
        }

        public async Task<HostDashboardData> GetDashboardStatisticsData(GetDashboardDataInput input)
        {
            var subscriptionEndDateEndUtc = Clock.Now.ToUniversalTime().AddDays(SubscriptionEndAlertDayCount);
            var subscriptionEndDateStartUtc = Clock.Now.ToUniversalTime();
            var tenantCreationStartDate = Clock.Now.ToUniversalTime().AddDays(-RecentTenantsDayCount);

            return new HostDashboardData
            {
                DashboardPlaceholder1 = 125,
                DashboardPlaceholder2 = 830,
                NewTenantsCount = await GetTenantsCountByDate(input.StartDate, input.EndDate),
                NewSubscriptionAmount = await GetNewSubscriptionAmount(input.StartDate, input.EndDate),
                ExpiringTenants = await GetExpiringTenantsData(subscriptionEndDateStartUtc, subscriptionEndDateEndUtc, MaxExpiringTenantsShownCount),
                RecentTenants = await GetRecentTenantsData(tenantCreationStartDate, MaxRecentTenantsShownCount),
                MaxExpiringTenantsShownCount = MaxExpiringTenantsShownCount,
                MaxRecentTenantsShownCount = MaxRecentTenantsShownCount,
                SubscriptionEndAlertDayCount = SubscriptionEndAlertDayCount,
                RecentTenantsDayCount = RecentTenantsDayCount,
                SubscriptionEndDateStart = subscriptionEndDateStartUtc,
                SubscriptionEndDateEnd = subscriptionEndDateEndUtc,
                TenantCreationStartDate = tenantCreationStartDate,
            };
        }

        public async Task<GetTenantStatisticsResult> GetTenantStatistics(GetDashboardDataInput input)
        {
            var startDate = input.StartDate;
            var endDate = input.EndDate;
            DateTime yesterday = (await GetToday()).AddDays(-1);

            var tenants = _tenantRepository.GetAll()
                .WhereIf(input.EditionId.HasValue, t => t.EditionId == input.EditionId)
                .WhereIf(input.Status == FilterActiveStatus.Active, x => x.IsActive)
                .WhereIf(input.Status == FilterActiveStatus.Inactive, x => !x.IsActive)
                .Select(t => new { t.Id, t.TenancyName });

            var periodUserStatisticsDtoList = await _userDailyHistoryRepository.GetAll()
                .Where(udh => udh.Date >= startDate && udh.Date <= endDate && udh.TenantId != null)
                .GroupBy(udh => new { udh.UserId, udh.TenantId })
                .Select(g => new
                {
                    UserId = g.Key.UserId,
                    TenantId = g.Key.TenantId
                })
                .GroupBy(u => u.TenantId)
                .Select(u => new
                {
                    TenantId = u.Key.Value,
                    NumberOfUsersActive = u.Count()
                })
                .ToListAsync();

            var periodTenantStatisticsDtoList = await _tenantDailyHistoryRepository.GetAll()
                .Include(a => a.Tenant)
                .Where(tdh => tdh.Date >= startDate && tdh.Date <= endDate)
                .Select(x => new
                {
                    x.TenantId,
                    x.Tenant.TenancyName,
                    x.OrderLinesCreated,
                    x.InternalTrucksScheduled,
                    x.LeaseHaulerScheduledDeliveries,
                    x.TicketsCreated,
                    x.SmsSent
                })
                .GroupBy(tdh => new { tdh.TenantId, tdh.TenancyName })
                .Select(g => new
                {
                    TenantId = g.Key.TenantId,
                    TenantName = g.Key.TenancyName,
                    OrderLines = g.Sum(tdh => tdh.OrderLinesCreated),
                    TrucksScheduled = g.Sum(tdh => tdh.InternalTrucksScheduled),
                    LeaseHaulersOrderLines = g.Sum(tdh => tdh.LeaseHaulerScheduledDeliveries),
                    TicketsCreated = g.Sum(tdh => tdh.TicketsCreated),
                    SmsSent = g.Sum(tdh => tdh.SmsSent)
                }).ToListAsync();

            var yesterdayTenantStatisticsDtoList = await _tenantDailyHistoryRepository.GetAll()
                .Where(tdh => tdh.Date == yesterday)
                .GroupBy(tdh => new { tdh.TenantId, tdh.Tenant.TenancyName })
                .Select(g => new
                {
                    TenantId = g.Key.TenantId,
                    NumberOfTrucks = g.Sum(tdh => tdh.ActiveTrucks),
                    NumberOfUsers = g.Sum(tdh => tdh.ActiveUsers),
                })
                .ToListAsync();

            var combinedTenantStatisticsDtoListQuery = (
                from t in await tenants.Select(x => new { x.Id, x.TenancyName }).ToListAsync()
                join pt in periodTenantStatisticsDtoList on t.Id equals pt.TenantId into ptj
                from pt in ptj.DefaultIfEmpty()
                join pu in periodUserStatisticsDtoList on t.Id equals pu.TenantId into puj
                from pu in puj.DefaultIfEmpty()
                join yt in yesterdayTenantStatisticsDtoList on t.Id equals yt.TenantId into ytj
                from yt in ytj.DefaultIfEmpty()
                select new TenantStatisticsDto
                {
                    TenantId = t.Id,
                    TenantName = t.TenancyName,
                    NumberOfTrucks = yt != null ? yt.NumberOfTrucks : 0,
                    NumberOfUsers = yt != null ? yt.NumberOfUsers : 0,
                    NumberOfUsersActive = pu != null ? pu.NumberOfUsersActive : 0,
                    OrderLines = pt != null ? pt.OrderLines : 0,
                    TrucksScheduled = pt != null ? pt.TrucksScheduled : 0,
                    LeaseHaulersOrderLines = pt != null ? pt.LeaseHaulersOrderLines : 0,
                    TicketsCreated = pt != null ? pt.TicketsCreated : 0,
                    SmsSent = pt != null ? pt.SmsSent : 0
                }).ToList();

            var count = combinedTenantStatisticsDtoListQuery.Count;

            var combinedTenantStatisticsDtoList = combinedTenantStatisticsDtoListQuery.AsQueryable()
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToList();

            var total = combinedTenantStatisticsDtoListQuery
                .GroupBy(x => 1)
                .Select(x => new TenantStatisticsDto
                {
                    LeaseHaulersOrderLines = x.Sum(y => y.LeaseHaulersOrderLines),
                    NumberOfTrucks = x.Sum(y => y.NumberOfTrucks),
                    NumberOfUsers = x.Sum(y => y.NumberOfUsers),
                    NumberOfUsersActive = x.Sum(y => y.NumberOfUsersActive),
                    OrderLines = x.Sum(y => y.OrderLines),
                    SmsSent = x.Sum(y => y.SmsSent),
                    TrucksScheduled = x.Sum(y => y.TrucksScheduled),
                    TicketsCreated = x.Sum(y => y.TicketsCreated)
                }).FirstOrDefault();

            return new GetTenantStatisticsResult(count, combinedTenantStatisticsDtoList)
            {
                Total = total
            };
        }

        public async Task<PagedResultDto<RequestDto>> GetRequests(GetRequestsInput input)
        {
            var requests = await GetRequests(false, input);
            return new PagedResultDto<RequestDto>(requests.totalCount, requests.list);
        }

        private async Task<(List<RequestDto> list, int totalCount)> GetRequests(bool getAll, GetRequestsInput input)
        {
            DateTime yesterdayUtc = DateTime.UtcNow.Date.AddDays(-1);
            DateTime weekAgoUtc = DateTime.UtcNow.Date.AddDays(-7);
            DateTime monthAgoUtc = DateTime.UtcNow.Date.AddMonths(-1);

            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var monthServiceMethodRequests = await _transactionDailyHistoryRepository.GetAll()
                    .Where(r => r.Date >= monthAgoUtc)
                    .GroupBy(r => new { r.ServiceName, r.MethodName })
                    .Select(g => new { g.Key.ServiceName, g.Key.MethodName })
                    .ToListAsync();

                var yesterdayRequests = await
                        (from tdh in _transactionDailyHistoryRepository.GetAll()
                         where tdh.Date == yesterdayUtc
                         group tdh by new { tdh.ServiceName, tdh.MethodName }
                        into g
                         select new { g.Key.ServiceName, g.Key.MethodName, NumberOfTransactions = g.Sum(x => x.NumberOfTransactions), AverageExecutionDuration = g.Sum(x => x.AverageExecutionDuration) }
                    ).ToListAsync();
                var weekRequests = await
                        (from tdh in _transactionDailyHistoryRepository.GetAll()
                         where tdh.Date >= weekAgoUtc
                         group tdh by new { tdh.ServiceName, tdh.MethodName }
                        into g
                         select new { g.Key.ServiceName, g.Key.MethodName, NumberOfTransactions = g.Sum(x => x.NumberOfTransactions), AverageExecutionDuration = g.Sum(x => x.AverageExecutionDuration) }
                    ).ToListAsync();
                var monthRequests = await
                        (from tdh in _transactionDailyHistoryRepository.GetAll()
                         where tdh.Date >= monthAgoUtc
                         group tdh by new { tdh.ServiceName, tdh.MethodName }
                        into g
                         select new { g.Key.ServiceName, g.Key.MethodName, NumberOfTransactions = g.Sum(x => x.NumberOfTransactions), AverageExecutionDuration = g.Sum(x => x.AverageExecutionDuration) }
                    ).ToListAsync();

                var requests =
                    from r in monthServiceMethodRequests

                    join yr in yesterdayRequests on new { r.ServiceName, r.MethodName } equals new { yr.ServiceName, yr.MethodName } into jyr
                    from yr in jyr.DefaultIfEmpty(new { ServiceName = "", MethodName = "", NumberOfTransactions = jyr.Sum(x => x.NumberOfTransactions), AverageExecutionDuration = jyr.Sum(x => x.AverageExecutionDuration) })

                    join wr in weekRequests on new { r.ServiceName, r.MethodName } equals new { wr.ServiceName, wr.MethodName } into jwr
                    from wr in jwr.DefaultIfEmpty(new { ServiceName = "", MethodName = "", NumberOfTransactions = jwr.Sum(x => x.NumberOfTransactions), AverageExecutionDuration = jwr.Sum(x => x.AverageExecutionDuration) })

                    join mr in monthRequests on new { r.ServiceName, r.MethodName } equals new { mr.ServiceName, mr.MethodName } into jmr
                    from mr in jmr.DefaultIfEmpty(new { ServiceName = "", MethodName = "", NumberOfTransactions = jmr.Sum(x => x.NumberOfTransactions), AverageExecutionDuration = jmr.Sum(x => x.AverageExecutionDuration) })

                        //orderby yr.NumberOfTransactions descending, wr.NumberOfTransactions descending, mr.NumberOfTransactions descending
                    select new RequestDto
                    {
                        ServiceName = r.ServiceName,
                        MethodName = r.MethodName,
                        ServiceAndMethodName = r.ServiceName + "." + r.MethodName,
                        AverageExecutionDuration = yr.AverageExecutionDuration,
                        NumberOfTransactions = yr.NumberOfTransactions,
                        LastWeekNumberOfTransactions = wr.NumberOfTransactions,
                        LastMonthNumberOfTransactions = mr.NumberOfTransactions,
                    };

                var totalCount = requests.Count();

                if (!input.Sorting.IsNullOrEmpty())
                {
                    var sorting = input.Sorting.Split(' ');
                    var direction = sorting.Skip(1).FirstOrDefault()?.ToLower() ?? "asc";
                    switch (sorting.FirstOrDefault()?.ToLower())
                    {
                        case "servicename": requests = direction == "asc" ? requests.OrderBy(x => x.ServiceName) : requests.OrderByDescending(x => x.ServiceName); break;
                        case "methodname": requests = direction == "asc" ? requests.OrderBy(x => x.MethodName) : requests.OrderByDescending(x => x.MethodName); break;
                        case "averageexecutionduration": requests = direction == "asc" ? requests.OrderBy(x => x.AverageExecutionDuration) : requests.OrderByDescending(x => x.AverageExecutionDuration); break;
                        case "numberoftransactions": requests = direction == "asc" ? requests.OrderBy(x => x.NumberOfTransactions) : requests.OrderByDescending(x => x.NumberOfTransactions); break;
                        case "lastweeknumberoftransactions": requests = direction == "asc" ? requests.OrderBy(x => x.LastWeekNumberOfTransactions) : requests.OrderByDescending(x => x.LastWeekNumberOfTransactions); break;
                        case "lastmonthnumberoftransactions": requests = direction == "asc" ? requests.OrderBy(x => x.LastMonthNumberOfTransactions) : requests.OrderByDescending(x => x.LastMonthNumberOfTransactions); break;
                    }
                }

                requests = getAll ? requests : input != null ? requests.Skip(input.SkipCount).Take(input.MaxResultCount) : requests.Take(Top20Number);

                return (requests.ToList(), totalCount);
            }
        }

        [HttpPost]
        public async Task<FileDto> GetRequestsToCsv(GetRequestsInput input)
        {
            var requests = await GetRequests(true, input);
            return _requestsCsvExporter.ExportToFile(requests.list);
        }

        public async Task<PagedResultDto<MostRecentActiveUserDto>> GetMostRecentActiveUsers()
        {
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                DateTime hourAgoUtc = DateTime.UtcNow.AddHours(-1);
                var activeUsers = await _auditLogRepository.GetAll()
                    .OrderByDescending(a => a.ExecutionTime)
                    .Where(a => a.UserId.HasValue && a.TenantId.HasValue && a.ImpersonatorUserId == null && a.ExecutionTime > hourAgoUtc)
                    .GroupBy(a => new { a.UserId, a.TenantId })
                    .Select(g => new
                    {
                        g.Key.TenantId,
                        g.Key.UserId,
                        LastTransaction = g.Max(x => x.ExecutionTime),
                        NumberOfTransactions = g.Count()
                    })
                    .Take(Top20Number)
                    .ToListAsync();

                var userIds = activeUsers.Select(x => x.UserId).Distinct().ToList();
                var tenantIds = activeUsers.Select(x => x.TenantId).Distinct().ToList();
                var users = await UserManager.Users
                    .Where(u => userIds.Contains(u.Id))
                    .Select(u => new
                    {
                        u.Id,
                        FullName = u.Name + " " + u.Surname
                    }).ToListAsync();
                var tenants = await TenantManager.Tenants
                    .Where(t => tenantIds.Contains(t.Id))
                    .Select(t => new
                    {
                        t.Id,
                        t.TenancyName
                    }).ToListAsync();

                var userList = activeUsers
                    .Select(a => new MostRecentActiveUserDto
                    {
                        TenancyName = tenants.FirstOrDefault(t => t.Id == a.TenantId)?.TenancyName,
                        FullName = users.FirstOrDefault(u => u.Id == a.UserId)?.FullName,
                        LastTransaction = a.LastTransaction,
                        NumberOfTransactions = a.NumberOfTransactions
                    })
                    .Where(a => a.LastTransaction > hourAgoUtc)
                    .ToList();

                return new PagedResultDto<MostRecentActiveUserDto>(userList.Count, userList);
            }
        }

        public async Task<HostDashboardKpiDto> GetDashboardKpiData(GetDashboardDataInput input)
        {
            var startDate = input.StartDate;
            var endDate = input.EndDate;
            DateTime yesterday = Clock.Now.ToUniversalTime().Date.AddDays(-1);
            var tdhYesterdayQuery = _tenantDailyHistoryRepository.GetAll().Where(tdh => tdh.Date == yesterday && !tdh.Tenant.IsDeleted);
            var yesterdayKpiDto = await tdhYesterdayQuery
                .GroupBy(tdh => 1)
                .Select(g => new
                {
                    ActiveTenants = g.Count(t => t.Tenant.IsActive),
                    ActiveTrucks = g.Sum(tdh => tdh.ActiveTrucks),
                    ActiveUsers = g.Sum(tdh => tdh.ActiveUsers)
                }
                )
                .FirstOrDefaultAsync();

            var tdhIntervalQuery = _tenantDailyHistoryRepository.GetAll().Where(tdh => tdh.Date >= startDate && tdh.Date <= endDate && !tdh.Tenant.IsDeleted);
            var intervalKpiDto = await tdhIntervalQuery
                .GroupBy(tdh => 1)
                .Select(g => new
                {
                    OrderLinesCreated = g.Sum(tdh => tdh.OrderLinesCreated),
                    InternalTrucksScheduled = g.Sum(tdh => tdh.InternalTrucksScheduled),
                    InternalScheduledDeliveries = g.Sum(tdh => tdh.InternalScheduledDeliveries),
                    LeaseHaulerScheduledDeliveries = g.Sum(tdh => tdh.LeaseHaulerScheduledDeliveries),
                    TicketsCreated = g.Sum(tdh => tdh.TicketsCreated),
                    SmsSent = g.Sum(tdh => tdh.SmsSent)
                }
                )
                .FirstOrDefaultAsync();

            var intervalUserKpiDto = await _userDailyHistoryRepository.GetAll()
                .Where(udh => udh.Date >= startDate && udh.Date <= endDate && udh.TenantId != null)
                .GroupBy(udh => udh.UserId)
                .CountAsync();

            return new HostDashboardKpiDto()
            {
                ActiveTenants = yesterdayKpiDto?.ActiveTenants ?? 0,
                ActiveTrucks = yesterdayKpiDto?.ActiveTrucks ?? 0,
                ActiveUsers = yesterdayKpiDto?.ActiveUsers ?? 0,

                OrderLinesCreated = intervalKpiDto?.OrderLinesCreated ?? 0,
                InternalTrucksScheduled = intervalKpiDto?.InternalTrucksScheduled ?? 0,
                InternalScheduledDeliveries = intervalKpiDto?.InternalScheduledDeliveries ?? 0,
                LeaseHaulerScheduledDeliveries = intervalKpiDto?.LeaseHaulerScheduledDeliveries ?? 0,
                TicketsCreated = intervalKpiDto?.TicketsCreated ?? 0,
                SmsSent = intervalKpiDto?.SmsSent ?? 0,

                UsersWithActivity = intervalUserKpiDto
            };
        }

        private async Task<decimal> GetNewSubscriptionAmount(DateTime startDate, DateTime endDate)
        {
            return await _subscriptionPaymentRepository.GetAll()
                .Where(s => s.CreationTime >= startDate &&
                            s.CreationTime <= endDate &&
                            s.Status == SubscriptionPaymentStatus.Completed)
                .Select(x => x.Amount)
                .SumAsync();
        }

        private async Task<int> GetTenantsCountByDate(DateTime startDate, DateTime endDate)
        {
            return await _tenantRepository.GetAll()
                .Where(t => t.CreationTime >= startDate && t.CreationTime <= endDate)
                .CountAsync();
        }

        private async Task<List<ExpiringTenant>> GetExpiringTenantsData(DateTime subscriptionEndDateStartUtc, DateTime subscriptionEndDateEndUtc, int? maxExpiringTenantsShownCount = null)
        {
            var query = _tenantRepository.GetAll().Where(t =>
                    t.SubscriptionEndDateUtc.HasValue &&
                    t.SubscriptionEndDateUtc.Value >= subscriptionEndDateStartUtc &&
                    t.SubscriptionEndDateUtc.Value <= subscriptionEndDateEndUtc)
                .Select(t => new
                {
                    t.Name,
                    t.SubscriptionEndDateUtc
                });

            if (maxExpiringTenantsShownCount.HasValue)
            {
                query = query.Take(maxExpiringTenantsShownCount.Value);
            }

            var rawData = await query.ToListAsync();

            return rawData
                .Select(t => new ExpiringTenant
                {

                    TenantName = t.Name,
                    RemainingDayCount = Convert.ToInt32(t.SubscriptionEndDateUtc.Value.Subtract(subscriptionEndDateStartUtc).TotalDays)
                })
                .OrderBy(t => t.RemainingDayCount)
                .ThenBy(t => t.TenantName)
                .ToList();
        }

        private async Task<List<RecentTenant>> GetRecentTenantsData(DateTime creationDateStart, int? maxRecentTenantsShownCount = null)
        {
            var query = _tenantRepository.GetAll()
                .Where(t => t.CreationTime >= creationDateStart)
                .OrderByDescending(t => t.CreationTime);

            if (maxRecentTenantsShownCount.HasValue)
            {
                query = (IOrderedQueryable<Tenant>)query.Take(maxRecentTenantsShownCount.Value);
            }

            return await query.Select(t => ObjectMapper.Map<RecentTenant>(t)).ToListAsync();
        }

        public async Task<TopStatsData> GetTopStatsData(GetTopStatsInput input)
        {
            return new TopStatsData
            {
                DashboardPlaceholder1 = 125,
                DashboardPlaceholder2 = 830,
                NewTenantsCount = await GetTenantsCountByDate(input.StartDate, input.EndDate),
                NewSubscriptionAmount = await GetNewSubscriptionAmount(input.StartDate, input.EndDate)
            };
        }

        public async Task<GetRecentTenantsOutput> GetRecentTenantsData()
        {
            var tenantCreationStartDate = Clock.Now.ToUniversalTime().AddDays(-RecentTenantsDayCount);

            var recentTenants = await GetRecentTenantsData(tenantCreationStartDate, MaxRecentTenantsShownCount);

            return new GetRecentTenantsOutput()
            {
                RecentTenants = recentTenants,
                TenantCreationStartDate = tenantCreationStartDate,
                RecentTenantsDayCount = RecentTenantsDayCount,
                MaxRecentTenantsShownCount = MaxRecentTenantsShownCount
            };
        }

        public async Task<GetExpiringTenantsOutput> GetSubscriptionExpiringTenantsData()
        {
            var subscriptionEndDateEndUtc = Clock.Now.ToUniversalTime().AddDays(SubscriptionEndAlertDayCount);
            var subscriptionEndDateStartUtc = Clock.Now.ToUniversalTime();

            var expiringTenants = await GetExpiringTenantsData(subscriptionEndDateStartUtc, subscriptionEndDateEndUtc,
                MaxExpiringTenantsShownCount);

            return new GetExpiringTenantsOutput()
            {
                ExpiringTenants = expiringTenants,
                MaxExpiringTenantsShownCount = MaxExpiringTenantsShownCount,
                SubscriptionEndAlertDayCount = SubscriptionEndAlertDayCount,
                SubscriptionEndDateStart = subscriptionEndDateStartUtc,
                SubscriptionEndDateEnd = subscriptionEndDateEndUtc
            };
        }

    }
}
