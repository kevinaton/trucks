using System.Threading.Tasks;
using Abp.Application.Services;
using DispatcherWeb.QuickbooksDesktop.Dto;

namespace DispatcherWeb.QuickbooksDesktop
{
    public interface IQuickbooksDesktopAppService : IApplicationService
    {
        Task<ExportInvoicesToIIFResult> ExportInvoicesToIIF(ExportInvoicesToIIFInput input);
    }
}
