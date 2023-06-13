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

        Task<(bool IsMasterDataSource, object DataSourceJson)> LocateDataSource(LocateDataSourceArgs arg);

        Task Initialize();

        ILogger Logger { get; }
    }
}