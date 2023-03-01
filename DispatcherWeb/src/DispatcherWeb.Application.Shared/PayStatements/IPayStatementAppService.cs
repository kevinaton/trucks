using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.PayStatements.Dto;

namespace DispatcherWeb.PayStatements
{
    public interface IPayStatementAppService : IApplicationService
    {
        Task<DriverPayStatementReport> GetDriverPayStatementReport(GetDriverPayStatementReportInput input);
        Task<DriverPayStatementReport> GetDriverPayStatementWarningsReport(EntityDto input);
        Task<PayStatementEditDto> GetPayStatementForEdit(EntityDto entityDto);
    }
}
