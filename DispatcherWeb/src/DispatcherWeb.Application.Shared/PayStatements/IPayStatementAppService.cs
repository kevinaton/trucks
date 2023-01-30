using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.PayStatements.Dto;
using System.Threading.Tasks;

namespace DispatcherWeb.PayStatements
{
    public interface IPayStatementAppService : IApplicationService
    {
        Task<DriverPayStatementReport> GetDriverPayStatementReport(GetDriverPayStatementReportInput input);
        Task<DriverPayStatementReport> GetDriverPayStatementWarningsReport(EntityDto input);
        Task<PayStatementEditDto> GetPayStatementForEdit(EntityDto entityDto);
    }
}
