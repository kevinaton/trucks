using System.IO;
using System.Threading.Tasks;
using GrapeCity.ActiveReports;
using GrapeCity.ActiveReports.Document;
using GrapeCity.ActiveReports.Web.Viewer;

namespace DispatcherWeb.ReportCenter.Models.ReportDataDefinitions.Base
{
    public interface IReportDataDefinition
    {
        PageReport ThisPageReport { get; set; }

        bool HasTenantsParameter { get; }

        Task<object> LocateDataSource(LocateDataSourceArgs arg);

        Task Initialize();

        MemoryStream OpenReportAsPdf(int? entityId);
    }
}
