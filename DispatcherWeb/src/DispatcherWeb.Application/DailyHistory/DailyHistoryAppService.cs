using System;
using System.Collections.Generic;
using System.Linq;
using Abp.Application.Services;
using Abp.Auditing;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Timing;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Customers;
using DispatcherWeb.Orders;
using DispatcherWeb.Sms;
using DispatcherWeb.Trucks;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.DailyHistory
{
    public class DailyHistoryAppService : DispatcherWebAppServiceBase, IDailyHistoryAppService
    {
        private readonly IRepository<TenantDailyHistory> _tenantDailyHistoryRepository;
        private readonly IRepository<UserDailyHistory> _userDailyHistoryRepository;
        private readonly IRepository<TransactionDailyHistory> _transactionDailyHistoryRepository;
        private readonly IRepository<Truck> _truckRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<AuditLog, long> _auditLogRepository;
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<OrderLineTruck> _orderLineTruckRepository;
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IRepository<SentSms> _sentSmsRepository;

        public DailyHistoryAppService(
            IRepository<TenantDailyHistory> tenantDailyHistoryRepository,
            IRepository<UserDailyHistory> userDailyHistoryRepository,
            IRepository<TransactionDailyHistory> transactionDailyHistoryRepository,
            IRepository<Truck> truckRepository,
            IRepository<User, long> userRepository,
            IRepository<AuditLog, long> auditLogRepository,
            IRepository<OrderLine> orderLineRepository,
            IRepository<Customer> customerRepository,
            IRepository<OrderLineTruck> orderLineTruckRepository,
            IRepository<Ticket> ticketRepository,
            IRepository<SentSms> sentSmsRepository
        )
        {
            _tenantDailyHistoryRepository = tenantDailyHistoryRepository;
            _userDailyHistoryRepository = userDailyHistoryRepository;
            _transactionDailyHistoryRepository = transactionDailyHistoryRepository;
            _truckRepository = truckRepository;
            _userRepository = userRepository;
            _auditLogRepository = auditLogRepository;
            _orderLineRepository = orderLineRepository;
            _customerRepository = customerRepository;
            _orderLineTruckRepository = orderLineTruckRepository;
            _ticketRepository = ticketRepository;
            _sentSmsRepository = sentSmsRepository;
        }

        [RemoteService(IsEnabled = false)]
        public void FillDailyHistories()
        {
            DateTime todayUtc = Clock.Now.Date;

            FillTenantDailyHistory(todayUtc);
            FillUserDailyHistory(todayUtc);
            FillTransactionDailyHistory(todayUtc);
        }

        [RemoteService(IsEnabled = true)]
        [HttpGet]
        public void FillDailyHistoriesForMonth()
        {
            DateTime todayUtc = DateTime.UtcNow.Date;
            DateTime dayUtc = DateTime.UtcNow.Date.AddMonths(-1);
            while (dayUtc <= todayUtc)
            {
                FillTenantDailyHistory(dayUtc);
                FillUserDailyHistory(dayUtc);
                FillTransactionDailyHistory(dayUtc);

                dayUtc = dayUtc.AddDays(1);
            }
        }


        [RemoteService(IsEnabled = false)]
        public void FillTenantDailyHistory(DateTime todayUtc)
        {
            Logger.Info($"FillTenantDailyHistory() started at {DateTime.UtcNow:s}");
            DateTime yesterdayUtc = todayUtc.AddDays(-1);

            using (var unitOfWork = UnitOfWorkManager.Begin(new UnitOfWorkOptions
            {
                IsTransactional = true,
                Timeout = TimeSpan.FromMinutes(10)
            }))
            using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.MustHaveTenant))
            using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var tenants = TenantManager.Tenants.Select(t => t.Id).ToList();

                var activeTrucks = GetTrucks();
                var activeUsers = GetActiveUsers();
                var usersWithActivity = GetUsersWithActivity();
                var orderLinesScheduled = GetOrderLinesScheduled();
                var orderLinesCreated = GetOrderLinesCreated();
                var activeCustomers = GetActiveCustomers();
                var internalTrucksScheduled = GetInternalTrucksScheduled();
                var internalScheduledDeliveries = GetInternalScheduledDeliveries();
                var leaseHaulerScheduledDeliveries = GetLeaseHaulerScheduledDeliveries();
                var ticketsCreated = GetTicketsCreated();
                var smsSent = GetSmsSent();

                _tenantDailyHistoryRepository.Delete(tdh => tdh.Date == yesterdayUtc);
                foreach (var tenant in tenants)
                {
                    _tenantDailyHistoryRepository.Insert(new TenantDailyHistory
                    {
                        TenantId = tenant,
                        Date = yesterdayUtc,
                        ActiveTrucks = activeTrucks.SingleOrDefault(x => x.TenantId == tenant)?.Value ?? 0,
                        ActiveUsers = activeUsers.SingleOrDefault(x => x.TenantId == tenant)?.Value ?? 0,
                        UsersWithActivity = usersWithActivity.SingleOrDefault(x => x.TenantId == tenant)?.Value ?? 0,
                        OrderLinesScheduled = orderLinesScheduled.SingleOrDefault(x => x.TenantId == tenant)?.Value ?? 0,
                        OrderLinesCreated = orderLinesCreated.SingleOrDefault(x => x.TenantId == tenant)?.Value ?? 0,
                        ActiveCustomers = activeCustomers.SingleOrDefault(x => x.TenantId == tenant)?.Value ?? 0,
                        InternalTrucksScheduled = internalTrucksScheduled.SingleOrDefault(x => x.TenantId == tenant)?.Value ?? 0,
                        InternalScheduledDeliveries = internalScheduledDeliveries.SingleOrDefault(x => x.TenantId == tenant)?.Value ?? 0,
                        LeaseHaulerScheduledDeliveries = leaseHaulerScheduledDeliveries.SingleOrDefault(x => x.TenantId == tenant)?.Value ?? 0,
                        TicketsCreated = ticketsCreated.SingleOrDefault(x => x.TenantId == tenant)?.Value ?? 0,
                        SmsSent = smsSent.SingleOrDefault(x => x.TenantId == tenant)?.Value ?? 0
                    }
                    );
                }
                unitOfWork.Complete();
            }
            Logger.Info($"FillTenantDailyHistory() ended at {DateTime.UtcNow:s}");

            // Local functions
            List<TenantDailyHistoryField> GetTrucks() =>
                _truckRepository.GetAll()
                    .Where(t => t.CreationTime < todayUtc &&
                                t.IsActive &&
                                t.VehicleCategory.IsPowered && t.LeaseHaulerTruck.AlwaysShowOnSchedule != true && t.LocationId != null)
                    .GroupBy(t => t.TenantId)
                    .Select(t => new TenantDailyHistoryField
                    {
                        TenantId = t.Key,
                        Value = t.Count()
                    })
                .ToList();

            List<TenantDailyHistoryField> GetActiveUsers() =>
                _userRepository.GetAll()
                    .Where(u => u.CreationTime < todayUtc && u.IsActive && u.TenantId != null)
                    .GroupBy(u => u.TenantId)
                    .Select(g => new TenantDailyHistoryField
                    {
                        TenantId = g.Key.Value,
                        Value = g.Count()
                    })
                    .ToList();

            List<TenantDailyHistoryField> GetUsersWithActivity() =>
                _auditLogRepository.GetAll()
                    .Where(al => al.ExecutionTime >= yesterdayUtc && al.ExecutionTime < todayUtc && al.TenantId != null && al.ImpersonatorUserId == null)
                    .GroupBy(al => new { al.TenantId, al.UserId })
                    .Select(g => new
                    {
                        g.Key.TenantId,
                        g.Key.UserId,
                    })
                    .GroupBy(x => x.TenantId)
                    .Select(g => new TenantDailyHistoryField
                    {
                        TenantId = g.Key.Value,
                        Value = g.Count()
                    })
                    .ToList();

            List<TenantDailyHistoryField> GetOrderLinesScheduled() =>
                _orderLineRepository.GetAll()
                    .Where(ol => ol.Order.DeliveryDate >= yesterdayUtc && ol.Order.DeliveryDate < todayUtc &&
                                 ol.NumberOfTrucks > 0 &&
                                 (ol.MaterialQuantity > 0 || ol.FreightQuantity > 0))
                    .GroupBy(ol => ol.TenantId)
                    .Select(g => new TenantDailyHistoryField
                    {
                        TenantId = g.Key,
                        Value = g.Count()
                    })
                    .ToList();

            List<TenantDailyHistoryField> GetOrderLinesCreated() =>
                _orderLineRepository.GetAll()
                    .Where(ol => ol.CreationTime >= yesterdayUtc && ol.CreationTime < todayUtc)
                    .GroupBy(ol => ol.TenantId)
                    .Select(g => new TenantDailyHistoryField
                    {
                        TenantId = g.Key,
                        Value = g.Count()
                    })
                    .ToList();

            List<TenantDailyHistoryField> GetActiveCustomers() =>
                _customerRepository.GetAll()
                    .Where(c => c.CreationTime < todayUtc && c.IsActive)
                    .GroupBy(c => c.TenantId)
                    .Select(g => new TenantDailyHistoryField
                    {
                        TenantId = g.Key,
                        Value = g.Count()
                    })
                    .ToList();

            List<TenantDailyHistoryField> GetInternalTrucksScheduled() =>
                _orderLineTruckRepository.GetAll()
                    .Where(olt => //olt.OrderLine.Order.DeliveryDate >= yesterdayUtc && olt.OrderLine.Order.DeliveryDate < todayUtc &&
                                  olt.CreationTime >= yesterdayUtc && olt.CreationTime < todayUtc &&
                                  olt.Truck.VehicleCategory.IsPowered && olt.Truck.LeaseHaulerTruck.AlwaysShowOnSchedule != true && olt.Truck.LocationId != null)
                    .GroupBy(olt => new { olt.TenantId, olt.TruckId })
                    .Select(g => new
                    {
                        g.Key.TenantId,
                        g.Key.TruckId
                    })
                    .GroupBy(x => x.TenantId)
                    .Select(g => new TenantDailyHistoryField
                    {
                        TenantId = g.Key,
                        Value = g.Count()
                    })
                    .ToList();

            List<TenantDailyHistoryField> GetInternalScheduledDeliveries() =>
                _orderLineTruckRepository.GetAll()
                    .Where(olt => olt.OrderLine.Order.DeliveryDate >= yesterdayUtc && olt.OrderLine.Order.DeliveryDate < todayUtc &&
                                  olt.Truck.VehicleCategory.IsPowered && olt.Truck.LeaseHaulerTruck.AlwaysShowOnSchedule != true && olt.Truck.LocationId != null)
                    .GroupBy(olt => olt.TenantId)
                    .Select(g => new TenantDailyHistoryField
                    {
                        TenantId = g.Key,
                        Value = g.Count()
                    })
                    .ToList();

            List<TenantDailyHistoryField> GetLeaseHaulerScheduledDeliveries() =>
                _orderLineTruckRepository.GetAll()
                    .Where(olt => //olt.OrderLine.Order.DeliveryDate >= yesterdayUtc && olt.OrderLine.Order.DeliveryDate < todayUtc &&
                                    olt.CreationTime >= yesterdayUtc && olt.CreationTime < todayUtc &&
                                  (olt.Truck.LocationId == null || olt.Truck.LeaseHaulerTruck.AlwaysShowOnSchedule))
                    .GroupBy(olt => olt.TenantId)
                    .Select(g => new TenantDailyHistoryField
                    {
                        TenantId = g.Key,
                        Value = g.Count()
                    })
                    .ToList();

            List<TenantDailyHistoryField> GetTicketsCreated() =>
                _ticketRepository.GetAll()
                    .Where(t => t.CreationTime >= yesterdayUtc && t.CreationTime < todayUtc)
                    .GroupBy(t => t.TenantId)
                    .Select(g => new TenantDailyHistoryField
                    {
                        TenantId = g.Key,
                        Value = g.Count()
                    })
                    .ToList();

            List<TenantDailyHistoryField> GetSmsSent() =>
                _sentSmsRepository.GetAll()
                    .Where(x => x.CreationTime >= yesterdayUtc && x.CreationTime < todayUtc && x.TenantId != null)
                    .GroupBy(x => x.TenantId)
                    .Select(g => new TenantDailyHistoryField
                    {
                        TenantId = g.Key.Value,
                        Value = g.Count()
                    })
                    .ToList();
        }

        private class TenantDailyHistoryField
        {
            public int TenantId { get; set; }
            public int Value { get; set; }
        }

        [RemoteService(IsEnabled = false)]
        public void FillUserDailyHistory(DateTime todayUtc)
        {
            Logger.Info($"FillUserDailyHistory() started at {DateTime.UtcNow:s}");

            DateTime yesterdayUtc = todayUtc.AddDays(-1);

            using (var unitOfWork = UnitOfWorkManager.Begin(new UnitOfWorkOptions
            {
                IsTransactional = true,
                Timeout = TimeSpan.FromMinutes(10)
            }))
            using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var users =
                    from a in _auditLogRepository.GetAll()
                    where a.TenantId.HasValue && a.ImpersonatorUserId == null && a.ExecutionTime < todayUtc && a.ExecutionTime >= yesterdayUtc
                    group a by a.UserId into u
                    where u.Key.HasValue
                    select new
                    {
                        UserId = u.Key.Value,
                        TenantId = u.First().TenantId,
                        NumberOfTransactions = u.Count()
                    };

                _userDailyHistoryRepository.Delete(tdh => tdh.Date == yesterdayUtc);
                foreach (var user in users)
                {
                    _userDailyHistoryRepository.Insert(new UserDailyHistory()
                    {
                        Date = yesterdayUtc,
                        UserId = user.UserId,
                        TenantId = user.TenantId,
                        NumberOfTransactions = user.NumberOfTransactions
                    });
                }
                unitOfWork.Complete();
            }

            Logger.Info($"FillUserDailyHistory() ended at {DateTime.UtcNow:s}");
        }

        [RemoteService(IsEnabled = false)]
        public void FillTransactionDailyHistory(DateTime todayUtc)
        {
            Logger.Info($"FillTransactionDailyHistory() started at {DateTime.UtcNow:s}");

            DateTime yesterdayUtc = todayUtc.AddDays(-1);

            using (var unitOfWork = UnitOfWorkManager.Begin(new UnitOfWorkOptions
            {
                IsTransactional = true,
                Timeout = TimeSpan.FromMinutes(10)
            }))
            using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var transactions =
                    from a in _auditLogRepository.GetAll()
                    where a.TenantId.HasValue && a.ImpersonatorUserId == null && a.ExecutionTime < todayUtc && a.ExecutionTime >= yesterdayUtc
                    group a by new { a.ServiceName, a.MethodName } into g
                    select new
                    {
                        SeviceName = g.Key.ServiceName,
                        MethodName = g.Key.MethodName,
                        NumberOfTransactions = g.Count(),
                        AverageExecutionDuration = g.Average(x => x.ExecutionDuration),
                    };

                _transactionDailyHistoryRepository.Delete(tdh => tdh.Date == yesterdayUtc);
                foreach (var transaction in transactions)
                {
                    _transactionDailyHistoryRepository.Insert(new TransactionDailyHistory
                    {
                        Date = yesterdayUtc,
                        ServiceName = transaction.SeviceName,
                        MethodName = transaction.MethodName,
                        NumberOfTransactions = transaction.NumberOfTransactions,
                        AverageExecutionDuration = (int)transaction.AverageExecutionDuration,
                    });
                }
                unitOfWork.Complete();
            }

            Logger.Info($"FillTransactionDailyHistory() ended at {DateTime.UtcNow:s}");
        }



    }
}
