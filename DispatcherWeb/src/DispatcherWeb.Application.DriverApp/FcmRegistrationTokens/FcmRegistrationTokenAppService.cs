using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.DriverApp.FcmRegistrationTokens.Dto;
using DispatcherWeb.WebPush;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.DriverApp.FcmRegistrationTokens
{
    [AbpAuthorize]
    public class FcmRegistrationTokenAppService : DispatcherWebDriverAppAppServiceBase, IFcmRegistrationTokenAppService
    {
        private readonly IRepository<FcmRegistrationToken> _fcmRegistrationTokenRepository;

        public FcmRegistrationTokenAppService(
            IRepository<FcmRegistrationToken> fcmRegistrationTokenRepository
            )
        {
            _fcmRegistrationTokenRepository = fcmRegistrationTokenRepository;
        }

        public async Task Post(PostInput input)
        {
            if (input.MobilePlatform == 0)
            {
                throw new UserFriendlyException("MobilePlatform is required");
            }

            var userId = AbpSession.GetUserId();
            var existingToken = await _fcmRegistrationTokenRepository.GetAll()
                .Where(x => x.UserId == userId && x.Token == input.Token)
                .FirstOrDefaultAsync();

            if (existingToken != null)
            {
                existingToken.LastModificationTime = Clock.Now;
                existingToken.LastModifierUserId = userId;
                existingToken.MobilePlatform = input.MobilePlatform;
            }
            else
            {
                await _fcmRegistrationTokenRepository.InsertAsync(new FcmRegistrationToken
                {
                    UserId = userId,
                    TenantId = AbpSession.TenantId,
                    Token = input.Token,
                    MobilePlatform = input.MobilePlatform
                });
            }
        }

        public async Task Delete(DeleteInput input)
        {
            var userId = AbpSession.GetUserId();
            var existingToken = await _fcmRegistrationTokenRepository.GetAll()
                .Where(x => x.UserId == userId && x.Token == input.Token)
                .FirstOrDefaultAsync();

            if (existingToken == null)
            {
                return;
            }

            await _fcmRegistrationTokenRepository.DeleteAsync(existingToken);
        }
    }
}
