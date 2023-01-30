using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.Payments.Dto;
using System.Threading.Tasks;

namespace DispatcherWeb.Payments
{
    public interface IPaymentAppService : IApplicationService
    {
        Task<AuthorizeChargeResult> AuthorizeCharge(AuthorizeChargeDto input);
        Task CancelAuthorization(EntityDto input);
        Task<CaptureAuthorizationResult> CaptureAuthorization(CaptureAuthorizationDto input);
        Task RefundPayment(EntityDto input);
        Task UpdatePaymentsFromHeartland(UpdatePaymentsFromHeartlandInput input);
        Task<int> GetHeartlandPublicKeyIdAsync();
    }
}
