using System.IO.Compression;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Common.Dto;
using DispatcherWeb.Configuration;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.LeaseHaulerStatements.Dto;
using DispatcherWeb.LeaseHaulerStatements.Exporting;
using DispatcherWeb.Orders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.LeaseHaulerStatements
{
    [AbpAuthorize]
    public class LeaseHaulerStatementAppService : DispatcherWebAppServiceBase, ILeaseHaulerStatementAppService
    {
        private readonly IRepository<LeaseHaulerStatement> _leaseHaulerStatementRepository;
        private readonly IRepository<LeaseHaulerStatementTicket> _leaseHaulerStatementTicketRepository;
        private readonly IRepository<Ticket> _tickerRepository;
        private readonly ILeaseHaulerStatementCsvExporter _leaseHaulerStatementCsvExporter;

        public LeaseHaulerStatementAppService(
            IRepository<LeaseHaulerStatement> leaseHaulerStatementRepository,
            IRepository<LeaseHaulerStatementTicket> leaseHaulerStatementTicketRepository,
            IRepository<Ticket> tickerRepository,
            ILeaseHaulerStatementCsvExporter leaseHaulerStatementCsvExporter
            )
        {
            _leaseHaulerStatementRepository = leaseHaulerStatementRepository;
            _leaseHaulerStatementTicketRepository = leaseHaulerStatementTicketRepository;
            _tickerRepository = tickerRepository;
            _leaseHaulerStatementCsvExporter = leaseHaulerStatementCsvExporter;
        }


        [AbpAuthorize(AppPermissions.Pages_LeaseHaulerStatements)]
        public async Task<PagedResultDto<LeaseHaulerStatementDto>> GetLeaseHaulerStatements(GetLeaseHaulerStatementsInput input)
        {
            var query = _leaseHaulerStatementRepository.GetAll()
                .WhereIf(input.StatementId.HasValue, x => x.Id == input.StatementId)
                .WhereIf(input.StatementDateBegin.HasValue, x => x.StatementDate >= input.StatementDateBegin)
                .WhereIf(input.StatementDateEnd.HasValue, x => x.StatementDate <= input.StatementDateEnd);

            var totalCount = await query.CountAsync();

            var items = await query
                .Select(x => new LeaseHaulerStatementDto
                {
                    Id = x.Id,
                    StatementDate = x.StatementDate,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    Customers = x.LeaseHaulerStatementTickets
                        .Where(t => t.Ticket.OrderLineId != null)
                        .Select(t => t.Ticket.OrderLine.Order.Customer)
                        .Select(c => new LeaseHaulerStatementCustomerDto
                        {
                            CustomerId = c.Id,
                            CustomerName = c.Name
                        })
                        .ToList()
                })
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            items.ForEach(x => x.Customers = x.Customers.GroupBy(c => c.CustomerId).Select(c => c.First()).OrderBy(c => c.CustomerName).ToList());

            return new PagedResultDto<LeaseHaulerStatementDto>(
                totalCount,
                items);
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulerStatements)]
        public async Task AddLeaseHaulerStatement(AddLeaseHaulerStatementInput input)
        {
            var timezone = await GetTimezone();
            var startDateInUtc = input.StartDate?.ConvertTimeZoneFrom(timezone);
            var endDateInUtc = input.EndDate?.AddDays(1).ConvertTimeZoneFrom(timezone);

            var brokerageFeeRate = await SettingManager.GetSettingValueAsync<decimal>(AppSettings.LeaseHaulers.BrokerFee) / 100;

            var tickets = await _tickerRepository.GetAll()
                .WhereIf(startDateInUtc.HasValue, x => x.TicketDateTime >= startDateInUtc)
                .WhereIf(endDateInUtc.HasValue, x => x.TicketDateTime < endDateInUtc)
                .Where(x => x.Driver.IsExternal && x.LeaseHaulerStatementTicket == null && x.IsVerified)
                .WhereIf(input.LeaseHaulerIds?.Any() == true, x => input.LeaseHaulerIds.Contains(x.CarrierId.Value))
                .Select(x => new
                {
                    TicketDateTime = x.TicketDateTime,
                    TicketId = x.Id,
                    LeaseHaulerId = x.Driver.LeaseHaulerDriver.LeaseHaulerId,
                    TruckId = x.TruckId,
                    Quantity = x.Quantity,
                    Rate = x.OrderLine.LeaseHaulerRate,
                    FuelSurcharge = x.FuelSurcharge
                }).ToListAsync();

            if (!tickets.Any())
            {
                throw new UserFriendlyException(L("NoDataForSelectedPeriod"));
            }

            var today = await GetToday();
            var leaseHaulerStatement = new LeaseHaulerStatement
            {
                StartDate = input.StartDate ?? tickets.Where(x => x.TicketDateTime.HasValue).Min(x => x.TicketDateTime)?.ConvertTimeZoneTo(timezone).Date ?? today,
                EndDate = input.EndDate ?? tickets.Where(x => x.TicketDateTime.HasValue).Max(x => x.TicketDateTime)?.ConvertTimeZoneTo(timezone).Date ?? today,
                StatementDate = today
            };
            _leaseHaulerStatementRepository.Insert(leaseHaulerStatement);

            foreach (var ticket in tickets)
            {
                var statementTicket = new LeaseHaulerStatementTicket
                {
                    TicketId = ticket.TicketId,
                    LeaseHaulerId = ticket.LeaseHaulerId,
                    TruckId = ticket.TruckId,
                    Quantity = ticket.Quantity,
                    Rate = ticket.Rate,
                    FuelSurcharge = ticket.FuelSurcharge
                };
                statementTicket.LeaseHaulerStatement = leaseHaulerStatement;
                statementTicket.BrokerFee = (ticket.Rate ?? 0) * ticket.Quantity * brokerageFeeRate;
                statementTicket.ExtendedAmount = ticket.Quantity * (ticket.Rate ?? 0) - statementTicket.BrokerFee + statementTicket.FuelSurcharge;
                await _leaseHaulerStatementTicketRepository.InsertAsync(statementTicket);
            }
        }

        private async Task<LeaseHaulerStatementReportDto> GetLeaseHaulerStatementReportDto(GetLeaseHaulerStatementsToCsvInput input)
        {
            var item = await _leaseHaulerStatementRepository.GetAll()
                .Where(x => x.Id == input.Id)
                .Select(x => new LeaseHaulerStatementReportDto
                {
                    Id = x.Id,
                    StatementDate = x.StatementDate,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    Tickets = x.LeaseHaulerStatementTickets.Select(t => new LeaseHaulerStatementTicketReportDto
                    {
                        OrderDate = t.Ticket.OrderLine.Order.DeliveryDate,
                        Shift = t.Ticket.OrderLine.Order.Shift,
                        CustomerName = t.Ticket.Customer.Name,
                        ServiceName = t.Ticket.Service.Service1,
                        TicketNumber = t.Ticket.TicketNumber,
                        TicketDateTime = t.Ticket.TicketDateTime,
                        CarrierName = t.Ticket.Carrier.Name,
                        TruckCode = t.TruckId.HasValue ? t.Truck.TruckCode : t.Ticket.TruckCode,
                        DriverName = t.Ticket.Driver.FirstName + " " + t.Ticket.Driver.LastName,
                        LoadAt = t.Ticket.LoadAt == null ? null : new LocationNameDto
                        {
                            Name = t.Ticket.LoadAt.Name,
                            StreetAddress = t.Ticket.LoadAt.StreetAddress,
                            City = t.Ticket.LoadAt.City,
                            State = t.Ticket.LoadAt.State
                        },
                        DeliverTo = t.Ticket.DeliverTo == null ? null : new LocationNameDto
                        {
                            Name = t.Ticket.DeliverTo.Name,
                            StreetAddress = t.Ticket.DeliverTo.StreetAddress,
                            City = t.Ticket.DeliverTo.City,
                            State = t.Ticket.DeliverTo.State
                        },
                        UomName = t.Ticket.UnitOfMeasure.Name,
                        Quantity = t.Quantity,
                        Rate = t.Rate,
                        BrokerFee = t.BrokerFee,
                        FuelSurcharge = t.FuelSurcharge,
                        ExtendedAmount = t.ExtendedAmount
                    }).ToList()
                }).FirstAsync();

            foreach (var ticket in item.Tickets)
            {
                ticket.ShiftName = await SettingManager.GetShiftName(ticket.Shift, Session.TenantId);
            }

            var timezone = await GetTimezone();
            item.Tickets.ForEach(t => t.TicketDateTime = t.TicketDateTime?.ConvertTimeZoneTo(timezone));
            //item.CurrencyCulture = await SettingManager.GetCurrencyCultureAsync();

            return item;
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulerStatements)]
        [HttpPost]
        public async Task<FileDto> GetLeaseHaulerStatementsToCsv(GetLeaseHaulerStatementsToCsvInput input)
        {
            var data = await GetLeaseHaulerStatementReportDto(input);
            var filename = $"LeaseHaulerStatement{data.Id}";
            if (input.SplitByLeaseHauler)
            {
                var csvList = data.Tickets
                    .GroupBy(x => x.CarrierName)
                    .Select(group => 
                    {
                        var carrierData = data.Clone();
                        carrierData.FileName = $"{filename}-{group.Key}.csv";
                        carrierData.Tickets = group.ToList();
                        return _leaseHaulerStatementCsvExporter.ExportToFileBytes(carrierData);
                    })
                    .ToList();

                var zipFile = csvList.ToZipFile(filename + ".zip", CompressionLevel.Optimal);
                return _leaseHaulerStatementCsvExporter.StoreTempFile(zipFile);
            }
            else
            {
                data.FileName = $"{filename}.csv";
                return _leaseHaulerStatementCsvExporter.ExportToFile(data);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulerStatements)]
        public async Task DeleteLeaseHaulerStatement(EntityDto input)
        {
            await _leaseHaulerStatementTicketRepository.DeleteAsync(x => x.LeaseHaulerStatementId == input.Id);
            await _leaseHaulerStatementRepository.DeleteAsync(input.Id);
        }
    }
}
