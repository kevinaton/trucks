using System.Threading.Tasks;
using GrapeCity.ActiveReports;
using GrapeCity.ActiveReports.Web.Viewer;

namespace DispatcherWeb.ReportCenter.Models.ReportDataDefinitions.Base
{
    public interface IReportDataDefinition
    {
        PageReport ThisPageReport { get; set; }

        Task<object> LocateDataSource(LocateDataSourceArgs arg);

        Task Initialize();
    }
}
