using System;
using System.Collections.Generic;
using Abp.Configuration;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Timing.Timezone;
using DispatcherWeb.Authorization;
using DispatcherWeb.Configuration;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Infrastructure.Reports;
using DispatcherWeb.Orders.RevenueBreakdownByTruckReport.Dto;
using DispatcherWeb.Orders.RevenueBreakdownReport;

namespace DispatcherWeb.Orders.RevenueBreakdownByTruckReport
{
    public class RevenueBreakdownByTruckReportAppService : ReportAppServiceBase<RevenueBreakdownByTruckReportInput>
    {
        private readonly IRevenueBreakdownTimeCalculator _revenueBreakdownTimeCalculator;
        private readonly IIocResolver _iocResolver;

        public RevenueBreakdownByTruckReportAppService(
            IRevenueBreakdownTimeCalculator revenueBreakdownTimeCalculator,
            ITimeZoneConverter timeZoneConverter,
            IIocResolver iocResolver
        ) : base(timeZoneConverter)
        {
            _revenueBreakdownTimeCalculator = revenueBreakdownTimeCalculator;
            _iocResolver = iocResolver;
        }

        protected override string ReportPermission => AppPermissions.Pages_Reports_RevenueBreakdownByTruck;
        protected override string ReportFileName => "RevenueBreakdownByTruck";
        protected override Task<string> GetReportFilename(string extension, RevenueBreakdownByTruckReportInput input)
        {
            return Task.FromResult($"{ReportFileName}_{input.DeliveryDateBegin:yyyyMMdd}to{input.DeliveryDateEnd:yyyyMMdd}.{extension}");
        }

        protected override void InitPdfReport(PdfReport report)
        {
        }

        protected override Task<bool> CreatePdfReport(PdfReport report, RevenueBreakdownByTruckReportInput input)
        {
            throw new NotImplementedException();
        }

        protected override Task<bool> CreateCsvReport(CsvReport report, RevenueBreakdownByTruckReportInput input)
        {
            return CreateReport(report, input, () => new RevenueBreakdownByTruckTableCsv(report.CsvWriter));
        }

        private async Task<bool> CreateReport(
            IReport report,
			RevenueBreakdownByTruckReportInput input,
            Func<IRevenueBreakdownByTruckTable> createRevenueBreakdownTable
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
                "Delivery Date",
                await SettingManager.UseShifts() ? "Shift" : null,
                "Truck",
				"Material Revenue",
				"Freight Revenue",
                showFuelSurcharge ? "Fuel Surcharge" : null,
				"Total Revenue",
			    "Driver Time",
			    "Revenue/hr"
            );
			var shiftDictionary = await SettingManager.GetShiftDictionary();
            var currencyCulture = await SettingManager.GetCurrencyCultureAsync();

            foreach (var item in revenueBreakdownItems)
            {
                revenueBreakdownTable.AddRow(
                    item.DeliveryDate?.ToString("d") ?? "",
					item.Shift.HasValue && shiftDictionary.ContainsKey(item.Shift.Value) ? shiftDictionary[item.Shift.Value] : null,
                    item.Truck,
					item.MaterialRevenue.ToString("C", currencyCulture),
                    item.FreightRevenue.ToString("C", currencyCulture),
                    showFuelSurcharge ? item.FuelSurcharge.ToString("C", currencyCulture) : null,
                    item.TotalRevenue.ToString("C", currencyCulture),
                    item.DriverTime.ToString("g"),
                    item.RevenuePerHour?.ToString("C", currencyCulture) ?? ""
                );
            }

            return true;
        }

        private async Task<List<RevenueBreakdownByTruckItem>> GetRevenueBreakdownItems(RevenueBreakdownByTruckReportInput input)
        {
            bool allowAddingTickets = await SettingManager.GetSettingValueAsync<bool>(AppSettings.General.AllowAddingTickets);
            IRevenueBreakdownByTruckReportDataService reportDataService = allowAddingTickets
                ? _iocResolver.Resolve<IRevenueBreakdownByTruckReportByTicketsDataService>()
                : _iocResolver.Resolve<IRevenueBreakdownByTruckReportByOrderLinesDataService>();
            var items = await reportDataService.GetRevenueBreakdownItems(input);
            _iocResolver.Release(reportDataService);

            await _revenueBreakdownTimeCalculator.FillDriversTimeForTrucks(items, input);

            return items;
        }

    }
}
