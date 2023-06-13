using System.IO;
using System.Threading.Tasks;
using GrapeCity.ActiveReports;
using GrapeCity.ActiveReports.Web.Viewer;
using Microsoft.Extensions.Logging;

namespace DispatcherWeb.ReportCenter.Models.ReportDataDefinitions.Base
{
    public interface IReportDataDefinition
    {
        PageReport ThisPageReport { get; set; }

        bool HasTenantsParameter { get; }

        Task<object> LocateDataSource(LocateDataSourceArgs arg);

        Task Initialize();

        MemoryStream OpenReportAsPdf(int? entityId);

        ILogger Logger { get; }
    }
}