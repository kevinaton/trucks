using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Collections.Extensions;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Timing.Timezone;
using DispatcherWeb.Authorization;
using DispatcherWeb.Common.Dto;
using DispatcherWeb.Configuration;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Infrastructure.Reports;
using DispatcherWeb.Orders.RevenueBreakdownReport.Dto;
using DispatcherWeb.Tickets;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Orders.RevenueBreakdownReport
{
    public class RevenueBreakdownReportAppService : ReportAppServiceBase<RevenueBreakdownReportInput>
    {
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly IRevenueBreakdownTimeCalculator _revenueBreakdownTimeCalculator;

        public RevenueBreakdownReportAppService(
            ITimeZoneConverter timeZoneConverter,
            IRepository<OrderLine> orderLineRepository,
            IRevenueBreakdownTimeCalculator revenueBreakdownTimeCalculator
        ) : base(timeZoneConverter)
        {
            _orderLineRepository = orderLineRepository;
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
            var showDriverPayRateColumn = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.AllowDriverPayRateDifferentFromFreightRate);

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
                showDriverPayRateColumn ? "Driver Pay Rate" : null,
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
                    showDriverPayRateColumn ? item.DriverPayRate?.ToString("C", currencyCulture) : null,
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
            var items = await GetRevenueBreakdownItemsFromTickets(input);

            await _revenueBreakdownTimeCalculator.FillDriversTimeForOrderLines(items, input);

            return items;
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
                    DriverPayRate = ol.FreightRateToPayDrivers,
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
