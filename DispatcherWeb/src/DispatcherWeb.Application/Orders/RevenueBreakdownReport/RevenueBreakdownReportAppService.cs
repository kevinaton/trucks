using System;
using System.Collections.Generic;
using System.Linq;
using Abp.Linq.Extensions;
using Abp.Configuration;
using System.Threading.Tasks;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Timing.Timezone;
using DispatcherWeb.Authorization;
using DispatcherWeb.Configuration;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Infrastructure.Reports;
using DispatcherWeb.Orders.RevenueBreakdownReport.Dto;
using Microsoft.EntityFrameworkCore;
using DispatcherWeb.Tickets;
using DispatcherWeb.Common.Dto;

namespace DispatcherWeb.Orders.RevenueBreakdownReport
{
    public class RevenueBreakdownReportAppService : ReportAppServiceBase<RevenueBreakdownReportInput>
    {
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly IRepository<ReceiptLine> _receiptLineRepository;
        private readonly IRevenueBreakdownTimeCalculator _revenueBreakdownTimeCalculator;

        public RevenueBreakdownReportAppService(
            ITimeZoneConverter timeZoneConverter,
            IRepository<Ticket> ticketRepository,
            IRepository<OrderLine> orderLineRepository,
            IRepository<ReceiptLine> receiptLineRepository,
            IRevenueBreakdownTimeCalculator revenueBreakdownTimeCalculator
        ) : base(timeZoneConverter)
        {
            _ticketRepository = ticketRepository;
            _orderLineRepository = orderLineRepository;
            _receiptLineRepository = receiptLineRepository;
            _revenueBreakdownTimeCalculator = revenueBreakdownTimeCalculator;
        }

        protected override string ReportPermission => AppPermissions.Pages_Reports_RevenueBreakdown;
        protected override string ReportFileName => "RevenueBreakdown";
        protected override Task<string> GetReportFilename(string extension, RevenueBreakdownReportInput input)
        {
            return Task.FromResult($"{ReportFileName}_{input.DeliveryDateBegin:yyyyMMdd}to{input.DeliveryDateEnd:yyyyMMdd}.{extension}");
        }

        protected override void InitPdfReport(PdfReport report)
        {
        }

        protected override Task<bool> CreatePdfReport(PdfReport report, RevenueBreakdownReportInput input)
        {
            throw new NotImplementedException();
        }

        protected override Task<bool> CreateCsvReport(CsvReport report, RevenueBreakdownReportInput input)
        {
            return CreateReport(report, input, () => new RevenueBreakdownTableCsv(report.CsvWriter));
        }

        private async Task<bool> CreateReport(
            IReport report,
            RevenueBreakdownReportInput input,
            Func<IRevenueBreakdownTable> createRevenueBreakdownTable
        )
        {
            DateTime today = GetLocalDateTimeNow().Date;
            report.AddReportHeader($"Revenue Breakdown Report for {input.DeliveryDateBegin:d} - {input.DeliveryDateEnd:d}");

            var showFuelSurcharge = await SettingManager.GetSettingValueAsync<bool>(AppSettings.Fuel.ShowFuelSurcharge);

            var revenueBreakdownItems = await GetRevenueBreakdownItems(input);
            if (revenueBreakdownItems.Count == 0)
            {
                return false;
            }

            var revenueBreakdownTable = createRevenueBreakdownTable();

            revenueBreakdownTable.AddColumnHeaders(
                "Customer",
                "Delivery Date",
                await SettingManager.UseShifts() ? "Shift" : null,
                "Load At",
                "Deliver To",
                "Item",
                "Material UOM",
                "Freight UOM",
                "Material Rate",
                "Freight Rate",
                "Planned Material Quantity",
                "Planned Freight Quantity",
                "Actual Material Quantity",
                "Actual Freight Quantity",
                "Freight Revenue",
                "Material Revenue",
                showFuelSurcharge ? "Fuel Surcharge" : null,
                "Total Revenue",
                //"Driver Time",
                //"Revenue/hr"
                "# of tickets",
                "Price Override"
            );
            var shiftDictionary = await SettingManager.GetShiftDictionary();
            var currencyCulture = await SettingManager.GetCurrencyCultureAsync();

            foreach (var item in revenueBreakdownItems)
            {
                revenueBreakdownTable.AddRow(
                    item.Customer,
                    item.DeliveryDate?.ToString("d") ?? "",
                    item.Shift.HasValue && shiftDictionary.ContainsKey(item.Shift.Value) ? shiftDictionary[item.Shift.Value] : await SettingManager.UseShifts() ? "" : null,
                    item.LoadAtName,
                    item.DeliverToName,
                    item.Item,
                    item.MaterialUom,
                    item.FreightUom,
                    item.MaterialRate?.ToString("C", currencyCulture) ?? "",
                    item.FreightRate?.ToString("C", currencyCulture) ?? "",
                    item.PlannedMaterialQuantity?.ToString("N4") ?? "",
                    item.PlannedFreightQuantity?.ToString("N4") ?? "",
                    item.ActualMaterialQuantity?.ToString("N4") ?? "",
                    item.ActualFreightQuantity?.ToString("N4") ?? "",
                    item.FreightRevenue.ToString("C", currencyCulture),
                    item.MaterialRevenue.ToString("C", currencyCulture),
                    showFuelSurcharge ? item.FuelSurcharge.ToString("C", currencyCulture) : null,
                    item.TotalRevenue.ToString("C", currencyCulture),
                    item.DriverTime.ToString("g"),
                    item.RevenuePerHour?.ToString("C", currencyCulture) ?? "",
                    item.TicketCount?.ToString() ?? "",
                    item.PriceOverride?.ToString("C", currencyCulture) ?? ""
                );
            }

            return true;
        }

        private async Task<List<RevenueBreakdownItem>> GetRevenueBreakdownItems(RevenueBreakdownReportInput input)
        {
            bool allowAddingTickets = await SettingManager.GetSettingValueAsync<bool>(AppSettings.General.AllowAddingTickets);
            var items = allowAddingTickets ? await GetRevenueBreakdownItemsFromTickets(input) : await GetRevenueBreakdownItemsFromOrderLines(input);
            //var items = await GetRevenueBreakdownItemsFromReceiptLines(input);

            await _revenueBreakdownTimeCalculator.FillDriversTimeForOrderLines(items, input);

            return items;
        }

        private async Task<List<RevenueBreakdownItem>> GetRevenueBreakdownItemsFromReceiptLines(RevenueBreakdownReportInput input)
        {
            return await _receiptLineRepository.GetAll()
                .WhereIf(input.CustomerId.HasValue, rl => rl.Receipt.CustomerId == input.CustomerId.Value)
                .WhereIf(input.OfficeId.HasValue, rl => rl.Receipt.OfficeId == input.OfficeId.Value)
                .WhereIf(input.LoadAtId.HasValue, rl => rl.LoadAtId == input.LoadAtId.Value)
                .WhereIf(input.DeliverToId.HasValue, rl => rl.DeliverToId == input.DeliverToId.Value)
                .WhereIf(input.ServiceId.HasValue, rl => rl.ServiceId == input.ServiceId.Value)
                .WhereIf(!input.Shifts.IsNullOrEmpty() && !input.Shifts.Contains(Shift.NoShift), rl => rl.Receipt.Shift.HasValue && input.Shifts.Contains(rl.Receipt.Shift.Value))
                .WhereIf(!input.Shifts.IsNullOrEmpty() && input.Shifts.Contains(Shift.NoShift), rl => !rl.Receipt.Shift.HasValue || input.Shifts.Contains(rl.Receipt.Shift.Value))
                .Where(rl => rl.Receipt.DeliveryDate >= input.DeliveryDateBegin)
                .Where(rl => rl.Receipt.DeliveryDate <= input.DeliveryDateEnd)
                .Select(rl => new RevenueBreakdownItem
                {
                    Customer = rl.Receipt.Customer.Name,
                    DeliveryDate = rl.Receipt.DeliveryDate,
                    Shift = rl.Receipt.Shift,
                    LoadAt = rl.LoadAt == null ? null : new LocationNameDto
                    {
                        Name = rl.LoadAt.Name,
                        StreetAddress = rl.LoadAt.StreetAddress,
                        City = rl.LoadAt.City,
                        State = rl.LoadAt.State
                    },
                    DeliverTo = rl.DeliverTo == null ? null : new LocationNameDto
                    {
                        Name = rl.DeliverTo.Name,
                        StreetAddress = rl.DeliverTo.StreetAddress,
                        City = rl.DeliverTo.City,
                        State = rl.DeliverTo.State
                    },
                    Item = rl.Service.Service1,
                    MaterialUom = rl.MaterialUom.Name,
                    FreightUom = rl.FreightUom.Name,
                    FreightRate = rl.FreightRate,
                    MaterialRate = rl.MaterialRate,
                    PlannedMaterialQuantity = rl.OrderLine == null ? 0 : rl.OrderLine.MaterialQuantity,
                    PlannedFreightQuantity = rl.OrderLine == null ? 0 : rl.OrderLine.FreightQuantity,
                    ActualMaterialQuantity = rl.MaterialQuantity ?? 0,
                    ActualFreightQuantity = rl.FreightQuantity ?? 0,
                    FreightPriceOriginal = rl.FreightAmount,
                    MaterialPriceOriginal = rl.MaterialAmount,
                    IsFreightPriceOverridden = rl.IsFreightAmountOverridden,
                    IsMaterialPriceOverridden = rl.IsMaterialAmountOverridden
                })
                .ToListAsync();
        }

        private async Task<List<RevenueBreakdownItem>> GetRevenueBreakdownItemsFromOrderLines(RevenueBreakdownReportInput input)
        {
            return await _orderLineRepository.GetAll()
                .WhereIf(input.CustomerId.HasValue, ol => ol.Order.CustomerId == input.CustomerId.Value)
                .WhereIf(input.OfficeId.HasValue, ol => ol.Order.LocationId == input.OfficeId.Value)
                .WhereIf(input.LoadAtId.HasValue, ol => ol.LoadAtId == input.LoadAtId.Value)
                .WhereIf(input.DeliverToId.HasValue, ol => ol.DeliverToId == input.DeliverToId.Value)
                .WhereIf(input.ServiceId.HasValue, ol => ol.ServiceId == input.ServiceId.Value)
                .WhereIf(!input.Shifts.IsNullOrEmpty() && !input.Shifts.Contains(Shift.NoShift), ol => ol.Order.Shift.HasValue && input.Shifts.Contains(ol.Order.Shift.Value))
                .WhereIf(!input.Shifts.IsNullOrEmpty() && input.Shifts.Contains(Shift.NoShift), ol => !ol.Order.Shift.HasValue || input.Shifts.Contains(ol.Order.Shift.Value))
                .Where(ol => ol.Order.DeliveryDate >= input.DeliveryDateBegin)
                .Where(ol => ol.Order.DeliveryDate <= input.DeliveryDateEnd)
                .Select(ol => new RevenueBreakdownItem
                {
                    Customer = ol.Order.Customer.Name,
                    DeliveryDate = ol.Order.DeliveryDate,
                    Shift = ol.Order.Shift,
                    LoadAt = ol.LoadAt == null ? null : new LocationNameDto
                    {
                        Name = ol.LoadAt.Name,
                        StreetAddress = ol.LoadAt.StreetAddress,
                        City = ol.LoadAt.City,
                        State = ol.LoadAt.State
                    },
                    DeliverTo = ol.DeliverTo == null ? null : new LocationNameDto
                    {
                        Name = ol.DeliverTo.Name,
                        StreetAddress = ol.DeliverTo.StreetAddress,
                        City = ol.DeliverTo.City,
                        State = ol.DeliverTo.State
                    },
                    Item = ol.Service.Service1,
                    MaterialUom = ol.MaterialUom.Name,
                    FreightUom = ol.FreightUom.Name,
                    FreightRate = ol.FreightPricePerUnit,
                    MaterialRate = ol.MaterialPricePerUnit,
                    PlannedMaterialQuantity = ol.MaterialQuantity,
                    PlannedFreightQuantity = ol.FreightQuantity,
                    //TODO are we changing office amounts to receipts?
                    ActualMaterialQuantity = ol.OfficeAmounts.FirstOrDefault(oa => oa.OfficeId == input.OfficeId).ActualQuantity ?? 0,
                    ActualFreightQuantity = ol.OfficeAmounts.FirstOrDefault(oa => oa.OfficeId == input.OfficeId).ActualQuantity ?? 0,
                    FreightPriceOriginal = ol.FreightPrice,
                    MaterialPriceOriginal = ol.MaterialPrice,
                    IsFreightPriceOverridden = ol.IsFreightPriceOverridden,
                    IsMaterialPriceOverridden = ol.IsMaterialPriceOverridden
                })
                .ToListAsync();
        }
        private async Task<List<RevenueBreakdownItem>> GetRevenueBreakdownItemsFromTickets(RevenueBreakdownReportInput input)
        {
            return await _orderLineRepository.GetAll()
                .Where(ol => ol.Tickets.Any())
                .WhereIf(input.CustomerId.HasValue, ol => ol.Order.CustomerId == input.CustomerId.Value)
                .WhereIf(input.OfficeId.HasValue, ol => ol.Order.LocationId == input.OfficeId.Value)
                .WhereIf(input.LoadAtId.HasValue, ol => ol.LoadAtId == input.LoadAtId.Value)
                .WhereIf(input.DeliverToId.HasValue, ol => ol.DeliverToId == input.DeliverToId.Value)
                .WhereIf(input.ServiceId.HasValue, ol => ol.ServiceId == input.ServiceId.Value)
                .WhereIf(!input.Shifts.IsNullOrEmpty() && !input.Shifts.Contains(Shift.NoShift), ol => ol.Order.Shift.HasValue && input.Shifts.Contains(ol.Order.Shift.Value))
                .WhereIf(!input.Shifts.IsNullOrEmpty() && input.Shifts.Contains(Shift.NoShift), ol => !ol.Order.Shift.HasValue || input.Shifts.Contains(ol.Order.Shift.Value))
                .Where(ol => ol.Order.DeliveryDate >= input.DeliveryDateBegin)
                .Where(ol => ol.Order.DeliveryDate <= input.DeliveryDateEnd)
                .Select(ol => new RevenueBreakdownItemFromTickets
                {
                    Customer = ol.Order.Customer.Name,
                    DeliveryDate = ol.Order.DeliveryDate,
                    Shift = ol.Order.Shift,
                    LoadAt = ol.LoadAt == null ? null : new LocationNameDto
                    {
                        Name = ol.LoadAt.Name,
                        StreetAddress = ol.LoadAt.StreetAddress,
                        City = ol.LoadAt.City,
                        State = ol.LoadAt.State
                    },
                    DeliverTo = ol.DeliverTo == null ? null : new LocationNameDto
                    {
                        Name = ol.DeliverTo.Name,
                        StreetAddress = ol.DeliverTo.StreetAddress,
                        City = ol.DeliverTo.City,
                        State = ol.DeliverTo.State
                    },
                    Item = ol.Service.Service1,
                    MaterialUom = ol.MaterialUom.Name,
                    FreightUom = ol.FreightUom.Name,
                    FreightRate = ol.FreightPricePerUnit,
                    MaterialRate = ol.MaterialPricePerUnit,
                    PlannedMaterialQuantity = ol.MaterialQuantity,
                    PlannedFreightQuantity = ol.FreightQuantity,
                    Tickets = ol.Tickets.Select(t => new TicketQuantityDto
                    {
                        Designation = ol.Designation,
                        FreightUomId = ol.FreightUomId,
                        MaterialUomId = ol.MaterialUomId,
                        Quantity = t.Quantity,
                        TicketUomId = t.UnitOfMeasureId,
                        FuelSurcharge = t.FuelSurcharge
                    }).ToList(),
                    FreightPriceOriginal = ol.FreightPrice,
                    MaterialPriceOriginal = ol.MaterialPrice,
                    IsFreightPriceOverridden = ol.IsFreightPriceOverridden,
                    IsMaterialPriceOverridden = ol.IsMaterialPriceOverridden
                })
                .ToListAsync<RevenueBreakdownItem>();
        }
    }
}
