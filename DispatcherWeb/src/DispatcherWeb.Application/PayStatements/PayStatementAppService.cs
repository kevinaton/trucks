using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Common.Dto;
using DispatcherWeb.Configuration;
using DispatcherWeb.Drivers;
using DispatcherWeb.Dto;
using DispatcherWeb.Orders;
using DispatcherWeb.Orders.Reports;
using DispatcherWeb.Orders.RevenueBreakdownReport;
using DispatcherWeb.Orders.RevenueBreakdownReport.Dto;
using DispatcherWeb.PayStatements.Dto;
using DispatcherWeb.PayStatements.Exporting;
using DispatcherWeb.TimeClassifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.PayStatements
{
    [AbpAuthorize]
    public class PayStatementAppService : DispatcherWebAppServiceBase, IPayStatementAppService
    {
        private readonly IRepository<PayStatement> _payStatementRepository;
        private readonly IRepository<PayStatementDetail> _payStatementDetailRepository;
        private readonly IRepository<PayStatementTicket> _payStatementTicketRepository;
        private readonly IRepository<PayStatementTime> _payStatementTimeRepository;
        private readonly IRepository<PayStatementDriverDateConflict> _payStatementDriverDateConflictRepository;
        private readonly IRepository<EmployeeTimePayStatementTime> _employeeTimePayStatementTimeRepository;
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IRepository<Driver> _driverRepository;
        private readonly IRepository<TimeClassification> _timeClassificationRepository;
        private readonly IRevenueBreakdownTimeCalculator _revenueBreakdownTimeCalculator;
        private readonly DriverPayStatementReportGenerator _driverPayStatementReportGenerator;
        private readonly DriverPayStatementWarningReportGenerator _driverPayStatementWarningReportGenerator;
        private readonly IPayStatementDetailsCsvExporter _payStatementDetailsCsvExporter;

        public PayStatementAppService(
            IRepository<PayStatement> payStatementRepository,
            IRepository<PayStatementDetail> payStatementDetailRepository,
            IRepository<PayStatementTicket> payStatementTicketRepository,
            IRepository<PayStatementTime> payStatementTimeRepository,
            IRepository<PayStatementDriverDateConflict> payStatementDriverDateConflictRepository,
            IRepository<EmployeeTimePayStatementTime> employeeTimePayStatementTimeRepository,
            IRepository<Ticket> ticketRepository,
            IRepository<Driver> driverRepository,
            IRepository<TimeClassification> timeClassificationRepository,
            IRevenueBreakdownTimeCalculator revenueBreakdownTimeCalculator,
            DriverPayStatementReportGenerator driverPayStatementReportGenerator,
            DriverPayStatementWarningReportGenerator driverPayStatementWarningReportGenerator,
            IPayStatementDetailsCsvExporter payStatementDetailsCsvExporter
            )
        {
            _payStatementRepository = payStatementRepository;
            _payStatementDetailRepository = payStatementDetailRepository;
            _payStatementTicketRepository = payStatementTicketRepository;
            _payStatementTimeRepository = payStatementTimeRepository;
            _payStatementDriverDateConflictRepository = payStatementDriverDateConflictRepository;
            _employeeTimePayStatementTimeRepository = employeeTimePayStatementTimeRepository;
            _ticketRepository = ticketRepository;
            _driverRepository = driverRepository;
            _timeClassificationRepository = timeClassificationRepository;
            _revenueBreakdownTimeCalculator = revenueBreakdownTimeCalculator;
            _driverPayStatementReportGenerator = driverPayStatementReportGenerator;
            _driverPayStatementWarningReportGenerator = driverPayStatementWarningReportGenerator;
            _payStatementDetailsCsvExporter = payStatementDetailsCsvExporter;
        }

        [AbpAuthorize(AppPermissions.Pages_Backoffice_DriverPay)]
        public async Task<PagedResultDto<PayStatementDto>> GetPayStatements(GetPayStatementsInput input)
        {
            var query = _payStatementRepository.GetAll()
                .WhereIf(input.OfficeId.HasValue, x => x.OfficeId == input.OfficeId || x.OfficeId == null)
                .WhereIf(input.StatementDateBegin.HasValue, x => x.StatementDate >= input.StatementDateBegin)
                .WhereIf(input.StatementDateEnd.HasValue, x => x.StatementDate <= input.StatementDateEnd);

            var totalCount = await query.CountAsync();

            var items = await query
                .Select(x => new PayStatementDto
                {
                    Id = x.Id,
                    StatementDate = x.StatementDate,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    IncludeProductionPay = x.IncludeProductionPay,
                    IncludeHourly = x.IncludeHourly,
                    IncludeSalary = x.IncludeSalary,
                    DriverDateConflicts = x.PayStatementDriverDateConflicts.Select(c => new PayStatementDriverDateConflictDto
                    {
                        Date = c.Date,
                        DriverName = c.Driver.LastName + ", " + c.Driver.FirstName,
                        ConflictKind = c.ConflictKind
                    })
                    .OrderBy(c => c.DriverName)
                    .ThenBy(c => c.Date)
                    .ToList()
                })
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<PayStatementDto>(
                totalCount,
                items);
        }

        public async Task<bool> HaveEmployeeTimeWithNoEnd(AddPayStatementInput input)
        {
            if (!input.IncludeHourly)
            {
                return false;
            }

            return await _revenueBreakdownTimeCalculator.HaveTimeWithNoEndDate(new FillDriversTimeInput
            {
                DeliveryDateBegin = input.StartDate,
                DeliveryDateEnd = input.EndDate,
                ExcludeTimeWithPayStatements = true,
                LocalEmployeesOnly = input.LocalEmployeesOnly
            });
        }

        private async Task<List<PayStatementCreationTicketDto>> GetProductionPayTickets(GetProductionPayTicketsInput input)
        {
            var timezone = await GetTimezone();
            var startDateInUtc = input.StartDate?.ConvertTimeZoneFrom(timezone);
            var endDateInUtc = input.EndDate.AddDays(1).ConvertTimeZoneFrom(timezone);

            var tickets = await _ticketRepository.GetAll()
                .WhereIf(input.OfficeId.HasValue, x => x.OfficeId == input.OfficeId)
                .WhereIf(input.LocalEmployeesOnly, x => x.Driver.OfficeId != null)
                .WhereIf(startDateInUtc.HasValue, x => x.TicketDateTime >= startDateInUtc)
                .Where(x =>
                    x.TicketDateTime < endDateInUtc
                    && x.IsVerified
                    && x.DriverId != null
                    && x.OrderLine.ProductionPay
                    && !x.PayStatementTickets.Any()
                    && x.OrderLineId != null)
                .Select(x => new PayStatementCreationTicketDto
                {
                    TicketId = x.Id,
                    TicketDateTime = x.TicketDateTime,
                    TicketCreationTime = x.CreationTime,
                    DriverId = x.DriverId.Value,
                    UserId = x.Driver.UserId,
                    Quantity = x.Quantity,
                    FreightRateToPayDrivers = x.OrderLine.FreightRateToPayDrivers
                }).ToListAsync();

            return tickets;
        }

        [HttpPost]
        public async Task<int> GetPastProductionPayTicketCount(AddPayStatementInput input)
        {
            return (await GetPastProductionPayTickets(input)).Count;
        }

        /// <summary>
        /// Returns tickets with ticket date in prior pay period and creation date after the statement was generated
        /// </summary>
        [HttpPost]
        public async Task<List<PayStatementCreationTicketDto>> GetPastProductionPayTickets(AddPayStatementInput input)
        {
            var result = new List<PayStatementCreationTicketDto>();
            var timezone = await GetTimezone();

            var pastTickets = await GetProductionPayTickets(new GetProductionPayTicketsInput
            {
                StartDate = null,
                EndDate = input.StartDate.AddDays(-1),
                LocalEmployeesOnly = input.LocalEmployeesOnly,
                OfficeId = input.OfficeId
            });

            if (!pastTickets.Any())
            {
                return result;
            }

            var minTicketDate = pastTickets.Where(x => x.TicketDateTime.HasValue)
                .Min(x => x.TicketDateTime.Value).ConvertTimeZoneTo(timezone).Date;
            var maxTicketDate = pastTickets.Where(x => x.TicketDateTime.HasValue)
                .Max(x => x.TicketDateTime.Value).ConvertTimeZoneTo(timezone).Date;

            var payStatements = await _payStatementRepository.GetAll()
                .Where(x => x.IncludeProductionPay && x.EndDate >= minTicketDate && x.StartDate <= maxTicketDate)
                .Select(x => new
                {
                    x.StartDate,
                    x.EndDate,
                    x.CreationTime
                }).ToListAsync();

            foreach (var ticket in pastTickets)
            {
                if (!ticket.TicketDateTime.HasValue)
                {
                    continue;
                }
                var ticketDate = ticket.TicketDateTime.Value.ConvertTimeZoneTo(timezone).Date;
                var matchingPayStatements = payStatements
                    .Where(x => x.StartDate <= ticketDate && x.EndDate >= ticketDate)
                    .ToList();

                if (!matchingPayStatements.Any())
                {
                    continue;
                }

                if (matchingPayStatements.Any(x => x.CreationTime < ticket.TicketCreationTime))
                {
                    result.Add(ticket);
                }
            }

            return result;
        }

        [AbpAuthorize(AppPermissions.Pages_Backoffice_DriverPay)]
        public async Task AddPayStatement(AddPayStatementInput input)
        {
            var payStatement = new PayStatement
            {
                StartDate = input.StartDate,
                EndDate = input.EndDate,
                StatementDate = await GetToday(),
                IncludeProductionPay = input.IncludeProductionPay,
                IncludeHourly = input.IncludeHourly,
                IncludeSalary = input.IncludeSalary,
                OfficeId = input.OfficeId,
            };
            _payStatementRepository.Insert(payStatement);

            var allowProductionPay = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.AllowProductionPay);
            var timezone = await GetTimezone();
            var startDateInUtc = input.StartDate.ConvertTimeZoneFrom(timezone);
            var endDateInUtc = input.EndDate.AddDays(1).ConvertTimeZoneFrom(timezone);

            var drivers = await _driverRepository.GetAll()
                    .WhereIf(input.OfficeId.HasValue, x => x.OfficeId == input.OfficeId)
                    .WhereIf(input.LocalEmployeesOnly, x => x.OfficeId != null)
                    .Select(x => new
                    {
                        DriverId = x.Id,
                        FullName = x.LastName + ", " + x.FirstName,
                        x.UserId,
                        x.IsInactive,
                        TimeClassifications = x.EmployeeTimeClassifications.Select(e => new
                        {
                            e.TimeClassificationId,
                            TimeClassificationName = e.TimeClassification.Name,
                            e.PayRate,
                            e.TimeClassification.IsProductionBased
                        }).ToList()
                    })
                    .OrderByDescending(x => !x.IsInactive)
                    .ToListAsync();

            var tenantTimeClassifications = await _timeClassificationRepository.GetAll()
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.IsProductionBased
                })
                .ToListAsync();

            var addedDetails = new List<PayStatementDetail>();
            var driversWithMissingRates = new List<string>();
            var driverIdsIncludedInReport = new List<int>();

            if (input.IncludeProductionPay && allowProductionPay)
            {
                var tickets = await GetProductionPayTickets(new GetProductionPayTicketsInput(input));

                if (input.IncludePastTickets)
                {
                    var pastTickets = await GetPastProductionPayTickets(input);
                    tickets = pastTickets.Union(tickets).ToList();
                }

                foreach (var driverGroup in tickets.GroupBy(x => x.DriverId))
                {
                    var driverId = driverGroup.Key;
                    var driver = drivers.FirstOrDefault(x => x.DriverId == driverId);
                    var productionPay = driver?.TimeClassifications.FirstOrDefault(x => x.IsProductionBased);
                    if (!driverIdsIncludedInReport.Contains(driverId))
                    {
                        driverIdsIncludedInReport.Add(driverId);
                    }

                    if (driver == null)
                    {
                        continue;
                    }

                    if (productionPay == null || productionPay.PayRate == 0)
                    {
                        var timeClassificationName = productionPay?.TimeClassificationName
                            ?? tenantTimeClassifications.FirstOrDefault(x => x.IsProductionBased)?.Name;
                        driversWithMissingRates.Add($"{driver.FullName} - {timeClassificationName}");
                        continue;
                    }

                    var payStatementDetail = new PayStatementDetail
                    {
                        DriverId = driverId,
                        PayStatement = payStatement
                    };
                    payStatement.PayStatementDetails.Add(payStatementDetail);
                    _payStatementDetailRepository.Insert(payStatementDetail);
                    addedDetails.Add(payStatementDetail);
                    foreach (var ticket in driverGroup)
                    {
                        var payStatementTicket = new PayStatementTicket
                        {
                            PayStatementDetail = payStatementDetail,
                            TimeClassificationId = productionPay.TimeClassificationId,
                            TicketId = ticket.TicketId,
                            Quantity = ticket.Quantity,
                            FreightRate = ticket.FreightRateToPayDrivers ?? 0,
                            DriverPayRate = productionPay.PayRate,
                        };
                        payStatementTicket.Total = Math.Round(payStatementTicket.Quantity * payStatementTicket.FreightRate * payStatementTicket.DriverPayRate / 100, 2);
                        payStatementDetail.PayStatementTickets.Add(payStatementTicket);
                        payStatementDetail.ProductionBasedTotal += payStatementTicket.Total;
                        _payStatementTicketRepository.Insert(payStatementTicket);
                    }
                    payStatementDetail.Total += payStatementDetail.ProductionBasedTotal;
                }
            }

            //if (input.IncludeSalary)
            //{
            //    foreach (var driver in drivers.Where(x => x.PayMethod == PayMethod.Salary))
            //    {
            //        var payStatementDetails = new PayStatementDetail
            //        {
            //            DriverId = driver.DriverId,
            //            PayAmount = driver.PayRate,
            //            PayMethod = driver.PayMethod,
            //            PayRate = driver.PayRate,
            //            PayStatement = payStatement
            //        };
            //        payStatement.PayStatementDetails.Add(payStatementDetails);
            //        _payStatementDetailRepository.Insert(payStatementDetails);
            //    }
            //}

            if (input.IncludeHourly)
            {
                var addedTimeRecords = new List<PayStatementTime>();
                var processedEmployeeTimeIds = new List<int>();
                await _revenueBreakdownTimeCalculator.FillDriversTime(new FillDriversTimeInput
                {
                    DeliveryDateBegin = input.StartDate,
                    DeliveryDateEnd = input.EndDate,
                    ExcludeTimeWithPayStatements = true,
                    LocalEmployeesOnly = input.LocalEmployeesOnly
                }, (e) =>
                {
                    if (e.IsProductionBased)
                    {
                        return;
                    }
                    var driver = drivers.FirstOrDefault(x => x.UserId == e.UserId);
                    if (driver == null)
                    {
                        return;
                    }
                    var driverId = driver.DriverId;
                    if (!driverIdsIncludedInReport.Contains(driverId))
                    {
                        driverIdsIncludedInReport.Add(driverId);
                    }
                    var detailRecord = addedDetails.FirstOrDefault(x => x.DriverId == driverId);
                    if (detailRecord == null)
                    {
                        detailRecord = new PayStatementDetail
                        {
                            DriverId = driverId,
                            PayStatement = payStatement,
                        };
                        addedDetails.Add(detailRecord);
                        payStatement.PayStatementDetails.Add(detailRecord);
                        _payStatementDetailRepository.Insert(detailRecord);
                    }

                    var timeClassification = driver.TimeClassifications.FirstOrDefault(x => x.TimeClassificationId == e.TimeClassificationId);
                    if (timeClassification == null || timeClassification.PayRate == 0)
                    {
                        var timeClassificationName = timeClassification?.TimeClassificationName
                            ?? tenantTimeClassifications.FirstOrDefault(x => x.Id == e.TimeClassificationId)?.Name;
                        driversWithMissingRates.Add($"{driver.FullName} - {timeClassificationName}");
                        return;
                    }

                    var timeRecord = addedTimeRecords
                        .FirstOrDefault(x => x.PayStatementDetail.DriverId == driverId
                            && x.Date == e.DeliveryDate
                            && x.TimeClassificationId == e.TimeClassificationId);
                    if (timeRecord == null)
                    {
                        timeRecord = new PayStatementTime
                        {
                            Date = e.DeliveryDate,
                            TimeClassificationId = e.TimeClassificationId,
                            DriverPayRate = timeClassification.PayRate,
                            PayStatementDetail = detailRecord,
                            Quantity = 0,
                            Total = 0
                        };
                        addedTimeRecords.Add(timeRecord);
                        detailRecord.PayStatementTimeRecords.Add(timeRecord);
                        _payStatementTimeRepository.Insert(timeRecord);
                    }
                    if (!processedEmployeeTimeIds.Contains(e.EmployeeTimeId))
                    {
                        var employeeTimePayStatementTime = new EmployeeTimePayStatementTime
                        {
                            EmployeeTimeId = e.EmployeeTimeId,
                            PayStatementTime = timeRecord
                        };
                        timeRecord.EmployeeTimeRecords.Add(employeeTimePayStatementTime);
                        _employeeTimePayStatementTimeRepository.Insert(employeeTimePayStatementTime);
                        processedEmployeeTimeIds.Add(e.EmployeeTimeId);
                    }

                    timeRecord.Quantity += e.HoursToAdd;
                });
                addedTimeRecords.ForEach(t => t.Quantity = Math.Round(t.Quantity, 2));
                addedTimeRecords.ForEach(t => t.Total = t.Quantity * (t.DriverPayRate ?? 0));
                addedDetails.ForEach(d => d.Total = d.TimeBasedTotal = d.PayStatementTimeRecords.Distinct().Sum(t => t.Total));
            }

            driversWithMissingRates = driversWithMissingRates.Distinct().OrderBy(x => x).ToList();
            if (driversWithMissingRates.Any())
            {
                throw new UserFriendlyException(L("DriversDontHaveRates_SetAndTryAgain"), string.Join("; \n", driversWithMissingRates));
            }

            addedDetails.ForEach(d => d.Total = d.TimeBasedTotal + d.ProductionBasedTotal);

            if (!payStatement.PayStatementDetails.Any())
            {
                throw new UserFriendlyException(L("NoDataForSelectedPeriod"));
            }

            //issues:
            var ticketsForDates = await _ticketRepository.GetAll()
                    .WhereIf(input.OfficeId.HasValue, x => x.OfficeId == input.OfficeId)
                    .WhereIf(input.LocalEmployeesOnly, x => x.Driver.OfficeId != null)
                    .Where(x => x.TicketDateTime >= startDateInUtc
                        && x.TicketDateTime < endDateInUtc
                        && x.IsVerified
                        && x.DriverId.HasValue
                        //&& driverIdsIncludedInReport.Contains(x.DriverId.Value)
                        && x.OrderLineId != null
                        && x.OrderLine.ProductionPay)
                    .Select(x => new
                    {
                        x.TicketDateTime,
                        DriverId = x.DriverId.Value,
                        x.Driver.UserId,
                        DriverName = x.Driver.LastName + ", " + x.Driver.FirstName,
                    })
                    .ToListAsync();

            var conflicts = new List<PayStatementDriverDateConflict>();

            await _revenueBreakdownTimeCalculator.FillDriversTime(new FillDriversTimeInput
            {
                DeliveryDateBegin = input.StartDate,
                DeliveryDateEnd = input.EndDate,
                ExcludeTimeWithPayStatements = false,
                LocalEmployeesOnly = input.LocalEmployeesOnly
            }, (e) =>
            {
                var dateBeginUtc = e.DeliveryDate.ConvertTimeZoneFrom(timezone);
                var dateEndUtc = dateBeginUtc.AddDays(1);
                var matches = ticketsForDates.Where(t => t.UserId == e.UserId && t.TicketDateTime >= dateBeginUtc && t.TicketDateTime < dateEndUtc);

                var driver = drivers.Where(x => x.UserId == e.UserId).FirstOrDefault();
                if (driver == null /*|| !driverIdsIncludedInReport.Contains(driver.DriverId)*/) //ignore the time of not included in the report drivers
                {
                    return;
                }

                if (e.IsProductionBased)
                {
                    if (!allowProductionPay)
                    {
                        return;
                    }

                    if (matches.Any())
                    {
                        return;
                    }

                    if (conflicts.Any(x => x.DriverId == driver.DriverId && x.Date == e.DeliveryDate && x.ConflictKind == DriverDateConflictKind.ProductionPayTimeButNoTickets))
                    {
                        return;
                    }

                    var conflict = new PayStatementDriverDateConflict
                    {
                        DriverId = driver.DriverId,
                        Date = e.DeliveryDate,
                        PayStatement = payStatement,
                        ConflictKind = DriverDateConflictKind.ProductionPayTimeButNoTickets
                    };
                    conflicts.Add(conflict);
                    _payStatementDriverDateConflictRepository.Insert(conflict);
                }
                else
                {
                    if (!matches.Any())
                    {
                        return;
                    }

                    var match = matches.First();
                    if (conflicts.Any(x => x.DriverId == match.DriverId && x.Date == e.DeliveryDate && x.ConflictKind == DriverDateConflictKind.BothProductionAndHourlyPay))
                    {
                        return;
                    }

                    var conflict = new PayStatementDriverDateConflict
                    {
                        DriverId = match.DriverId,
                        Date = e.DeliveryDate,
                        PayStatement = payStatement,
                        ConflictKind = DriverDateConflictKind.BothProductionAndHourlyPay
                    };
                    conflicts.Add(conflict);
                    _payStatementDriverDateConflictRepository.Insert(conflict);
                }
            });
        }

        [AbpAuthorize(AppPermissions.Pages_Backoffice_DriverPay)]
        public async Task<PayStatementEditDto> GetPayStatementForEdit(EntityDto input)
        {
            var model = await _payStatementRepository.GetAll()
                    .Select(x => new PayStatementEditDto
                    {
                        Id = x.Id,
                        StatementDate = x.StatementDate,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate
                    })
                    .SingleAsync(x => x.Id == input.Id);

            return model;
        }

        public async Task<PagedResultDto<PayStatementItemEditDto>> GetPayStatementItems(GetPayStatementItemsInput input)
        {
            var items = new List<PayStatementItemEditDto>();
            var tickets = await _payStatementTicketRepository.GetAll()
                .Where(x => x.PayStatementDetail.PayStatementId == input.PayStatementId)
                .Select(t => new PayStatementItemEditDto
                {
                    ItemKind = PayStatementItemKind.Ticket,
                    Id = t.Id,
                    Date = t.Ticket.TicketDateTime,
                    DriverId = t.PayStatementDetail.DriverId,
                    DriverName = t.PayStatementDetail.Driver.LastName + ", " + t.PayStatementDetail.Driver.FirstName,
                    TimeClassificationId = t.TimeClassificationId,
                    TimeClassificationName = t.TimeClassification.Name,
                    IsProductionPay = t.TimeClassification.IsProductionBased,
                    DriverPayRate = t.DriverPayRate,
                    Quantity = t.Quantity,
                    Total = t.Total,
                    CustomerName = t.Ticket.Customer.Name,
                    DeliverToNamePlain = t.Ticket.OrderLine.DeliverTo.Name + t.Ticket.OrderLine.DeliverTo.StreetAddress + t.Ticket.OrderLine.DeliverTo.City + t.Ticket.OrderLine.DeliverTo.State, //for sorting
                    DeliverTo = t.Ticket.OrderLine.DeliverTo == null ? null : new LocationNameDto
                    {
                        Name = t.Ticket.OrderLine.DeliverTo.Name,
                        StreetAddress = t.Ticket.OrderLine.DeliverTo.StreetAddress,
                        City = t.Ticket.OrderLine.DeliverTo.City,
                        State = t.Ticket.OrderLine.DeliverTo.State
                    },
                    LoadAtNamePlain = t.Ticket.OrderLine.LoadAt.Name + t.Ticket.OrderLine.LoadAt.StreetAddress + t.Ticket.OrderLine.LoadAt.City + t.Ticket.OrderLine.LoadAt.State, //for sorting
                    LoadAt = t.Ticket.OrderLine.LoadAt == null ? null : new LocationNameDto
                    {
                        Name = t.Ticket.OrderLine.LoadAt.Name,
                        StreetAddress = t.Ticket.OrderLine.LoadAt.StreetAddress,
                        City = t.Ticket.OrderLine.LoadAt.City,
                        State = t.Ticket.OrderLine.LoadAt.State
                    },
                    FreightRateToPayDrivers = t.Ticket.OrderLine.FreightRateToPayDrivers,
                    Item = t.Ticket.OrderLine.Service.Service1,
                    JobNumber = t.Ticket.OrderLine.JobNumber,
                }).ToListAsync();

            var timezone = await GetTimezone();
            tickets.ForEach(t => t.Date = t.Date?.ConvertTimeZoneTo(timezone));
            items.AddRange(tickets);

            var time = await _payStatementTimeRepository.GetAll()
                .Where(x => x.PayStatementDetail.PayStatementId == input.PayStatementId)
                .Select(t => new PayStatementItemEditDto
                {
                    ItemKind = PayStatementItemKind.Time,
                    Id = t.Id,
                    Date = t.Date,
                    DriverId = t.PayStatementDetail.DriverId,
                    DriverName = t.PayStatementDetail.Driver.LastName + ", " + t.PayStatementDetail.Driver.FirstName,
                    TimeClassificationId = t.TimeClassificationId,
                    TimeClassificationName = t.TimeClassification.Name,
                    IsProductionPay = t.TimeClassification.IsProductionBased,
                    DriverPayRate = t.DriverPayRate,
                    Quantity = t.Quantity,
                    Total = t.Total,
                }).ToListAsync();
            items.AddRange(time);

            items = items.AsQueryable()
                .OrderBy(input.Sorting)
                //.PageBy(input)
                .ToList();

            return new PagedResultDto<PayStatementItemEditDto>(
                items.Count,
                items);
        }

        public async Task<PayStatementItemEditDto> EditPayStatementItem(PayStatementItemEditDto model)
        {
            var timeClassification = await _timeClassificationRepository.GetAll()
                    .Where(x => x.Id == model.TimeClassificationId)
                    .Select(x => new
                    {
                        x.IsProductionBased
                    }).FirstAsync();
            model.IsProductionPay = timeClassification.IsProductionBased;

            if (model.ItemKind == PayStatementItemKind.Ticket)
            {
                var item = await _payStatementTicketRepository.GetAsync(model.Id);
                item.TimeClassificationId = model.TimeClassificationId;
                item.DriverPayRate = model.DriverPayRate ?? 0;
                item.Quantity = model.Quantity;
                if (timeClassification.IsProductionBased && item.DriverPayRate > 100)
                {
                    item.DriverPayRate = 0;
                }
                var oldTotal = item.Total;
                item.Total = timeClassification.IsProductionBased
                    ? Math.Round(item.Quantity * item.DriverPayRate * (item.Ticket.OrderLine.FreightRateToPayDrivers ?? 0) / 100, 2)
                    : Math.Round(item.Quantity * item.DriverPayRate, 2);
                var newTotal = item.Total;

                model.DriverPayRate = item.DriverPayRate;
                model.Total = item.Total;

                if (oldTotal == newTotal)
                {
                    return model;
                }

                var payStatementDetail = await _payStatementDetailRepository.GetAll()
                    .Include(x => x.PayStatementTickets)
                    .Where(x => x.Id == item.PayStatementDetailId)
                    .FirstOrDefaultAsync();
                payStatementDetail.ProductionBasedTotal = payStatementDetail.PayStatementTickets.Sum(x => x.Total);
                payStatementDetail.Total = payStatementDetail.ProductionBasedTotal + payStatementDetail.TimeBasedTotal;
            }
            else if (model.ItemKind == PayStatementItemKind.Time)
            {
                var item = await _payStatementTimeRepository.GetAsync(model.Id);
                item.TimeClassificationId = model.TimeClassificationId;
                item.DriverPayRate = model.DriverPayRate ?? 0;
                item.Quantity = model.Quantity;
                var oldTotal = item.Total;
                item.Total = Math.Round(item.Quantity * (item.DriverPayRate ?? 0), 2);
                var newTotal = item.Total;

                model.DriverPayRate = item.DriverPayRate;
                model.Total = item.Total;

                if (oldTotal == newTotal)
                {
                    return model;
                }

                var payStatementDetail = await _payStatementDetailRepository.GetAll()
                    .Include(x => x.PayStatementTimeRecords)
                    .Where(x => x.Id == item.PayStatementDetailId)
                    .FirstOrDefaultAsync();
                payStatementDetail.TimeBasedTotal = payStatementDetail.PayStatementTimeRecords.Sum(x => x.Total);
                payStatementDetail.Total = payStatementDetail.ProductionBasedTotal + payStatementDetail.TimeBasedTotal;
            }
            else
            {
                throw new ArgumentException(nameof(model.ItemKind));
            }

            return model;
        }

        private async Task<PayStatementReportDto> GetPayStatementReportDto(EntityDto input)
        {
            var item = await _payStatementRepository.GetAll()
                .Where(x => x.Id == input.Id)
                .Select(x => new PayStatementReportDto
                {
                    Id = x.Id,
                    StatementDate = x.StatementDate,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    IncludeProductionPay = x.IncludeProductionPay,
                    IncludeHourly = x.IncludeHourly,
                    IncludeSalary = x.IncludeSalary,
                    Drivers = x.PayStatementDetails.Select(d => new PayStatementReportDetailDto
                    {
                        DriverName = d.Driver.LastName + ", " + d.Driver.FirstName,
                        ProductionBasedTotal = d.ProductionBasedTotal,
                        TimeBasedTotal = d.TimeBasedTotal,
                        Total = d.Total,
                        Tickets = d.PayStatementTickets.Select(t => new PayStatementReportTicketDto
                        {
                            CustomerName = t.Ticket.Customer.Name,
                            DeliverTo = t.Ticket.OrderLine.DeliverTo == null ? null : new LocationNameDto
                            {
                                Name = t.Ticket.OrderLine.DeliverTo.Name,
                                StreetAddress = t.Ticket.OrderLine.DeliverTo.StreetAddress,
                                City = t.Ticket.OrderLine.DeliverTo.City,
                                State = t.Ticket.OrderLine.DeliverTo.State
                            },
                            LoadAt = t.Ticket.OrderLine.LoadAt == null ? null : new LocationNameDto
                            {
                                Name = t.Ticket.OrderLine.LoadAt.Name,
                                StreetAddress = t.Ticket.OrderLine.LoadAt.StreetAddress,
                                City = t.Ticket.OrderLine.LoadAt.City,
                                State = t.Ticket.OrderLine.LoadAt.State
                            },
                            FreightRateToPayDrivers = t.Ticket.OrderLine.FreightRateToPayDrivers ?? 0,
                            DriverPayRate = t.DriverPayRate,
                            TimeClassificationId = t.TimeClassificationId,
                            TimeClassificationName = t.TimeClassification.Name,
                            IsProductionPay = t.TimeClassification.IsProductionBased,
                            Item = t.Ticket.OrderLine.Service.Service1,
                            JobNumber = t.Ticket.OrderLine.JobNumber,
                            Quantity = t.Quantity,
                            TicketDateTime = t.Ticket.TicketDateTime,
                            Total = t.Total,
                        }).ToList(),
                        TimeRecords = d.PayStatementTimeRecords.Select(t => new PayStatementReportTimeDto
                        {
                            Date = t.Date,
                            Quantity = t.Quantity,
                            TimeClassificationId = t.TimeClassificationId,
                            TimeClassificationName = t.TimeClassification.Name,
                            IsProductionPay = t.TimeClassification.IsProductionBased,
                            DriverPayRate = t.DriverPayRate,
                            Total = t.Total
                        }).ToList()
                    }).OrderBy(p => p.DriverName).ToList()
                }).FirstAsync();

            var timezone = await GetTimezone();
            item.Drivers.ForEach(d => d.Tickets.ForEach(t => t.TicketDateTime = t.TicketDateTime?.ConvertTimeZoneTo(timezone)));
            item.CurrencyCulture = await SettingManager.GetCurrencyCultureAsync();

            return item;
        }

        public async Task<DriverPayStatementReport> GetDriverPayStatementReport(GetDriverPayStatementReportInput input)
        {
            var data = await GetPayStatementReportDto(new EntityDto(input.Id));
            var report = _driverPayStatementReportGenerator.GenerateReportAndZip(data, input);
            return report;
        }

        public async Task<DriverPayStatementReport> GetDriverPayStatementWarningsReport(EntityDto input)
        {
            var data = await _payStatementRepository.GetAll()
                .Where(x => x.Id == input.Id)
                .Select(x => new PayStatementWarningReportDto
                {
                    EndDate = x.EndDate,
                    DriverDateConflicts = x.PayStatementDriverDateConflicts.Select(c => new PayStatementDriverDateConflictDto
                    {
                        Date = c.Date,
                        DriverName = c.Driver.LastName + ", " + c.Driver.FirstName,
                        ConflictKind = c.ConflictKind
                    })
                    .OrderBy(c => c.DriverName)
                    .ThenBy(c => c.Date)
                    .ToList()
                })
                .FirstAsync();

            var report = _driverPayStatementWarningReportGenerator.GenerateReport(data, input);
            return report;
        }

        [AbpAuthorize(AppPermissions.Pages_Backoffice_DriverPay)]
        public async Task DeletePayStatement(EntityDto input)
        {
            await _payStatementDriverDateConflictRepository.DeleteAsync(x => x.PayStatementId == input.Id);
            await _employeeTimePayStatementTimeRepository.DeleteAsync(x => x.PayStatementTime.PayStatementDetail.PayStatementId == input.Id);
            await _payStatementTimeRepository.DeleteAsync(x => x.PayStatementDetail.PayStatementId == input.Id);
            await _payStatementTicketRepository.DeleteAsync(x => x.PayStatementDetail.PayStatementId == input.Id);
            await _payStatementDetailRepository.DeleteAsync(x => x.PayStatementId == input.Id);
            await _payStatementRepository.DeleteAsync(input.Id);
        }

        public async Task<FileDto> ExportPayStatementToCsv(EntityDto input)
        {
            var details = await GetPayStatementForEdit(input);
            var items = await GetPayStatementItems(new GetPayStatementItemsInput
            {
                PayStatementId = input.Id,
                Sorting = nameof(PayStatementItemEditDto.DriverName)
            });

            return await _payStatementDetailsCsvExporter.ExportToFileAsync(items.Items.ToList(), details);
        }
    }
}
