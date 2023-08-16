using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using DispatcherWeb.ReportCenter.Services;
using GrapeCity.ActiveReports;
using GrapeCity.ActiveReports.Web.Viewer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DispatcherWeb.ReportCenter.Models.ReportDataDefinitions.Base
{
    public interface IReportDataDefinition
    {
        ReportAppService ReportAppService { get; }

        PageReport ThisPageReport { get; set; }

        bool HasTenantsParameter { get; }

        Task<(bool IsMasterDataSource, object DataSourceJson)> LocateDataSource(LocateDataSourceArgs arg);

        Task Initialize();

        MemoryStream OpenReportAsPdf(int? entityId);

        ILogger Logger { get; }

        IHttpClientFactory HttpClientFactory { get; }

        IHttpContextAccessor HttpContextAccessor { get; }

        Task PostInitialize();
    }
}