using Abp.Application.Services;
using DispatcherWeb.QuickbooksDesktop.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.QuickbooksDesktop
{
    public interface IQuickbooksDesktopAppService : IApplicationService
    {
        Task<ExportInvoicesToIIFResult> ExportInvoicesToIIF();
    }
}
