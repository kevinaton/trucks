using System.Collections.Generic;
using System.Threading.Tasks;
using DispatcherWeb.Dto;
using DispatcherWeb.Orders.Dto;

namespace DispatcherWeb.Orders.Reports
{
    public interface IBillingReconciliationExcelExporter
    {
        Task<FileDto> ExportToFileAsync(List<BillingReconciliationReportDto> list);
    }
}
