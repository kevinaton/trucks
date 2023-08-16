using System.Threading.Tasks;

namespace DispatcherWeb.Infrastructure.AzureBlobs
{
    public interface ILogoProvider
    {
        Task<string> GetReportLogoAsBase64StringAsync(int? officeId);
    }
}
