using Abp.Domain.Repositories;
using DispatcherWeb.MultiTenancy;

namespace DispatcherWeb.Infrastructure.General
{
    public class NotAuthorizedUserAppService : DispatcherWebDomainServiceBase, INotAuthorizedUserAppService
    {
        private readonly IRepository<Tenant> _tenantRepository;

        public NotAuthorizedUserAppService(
            IRepository<Tenant> tenantRepository
        )
        {
            _tenantRepository = tenantRepository;
        }

        public string GetTenancyNameOrNull(int? tenantId)
        {
            if (tenantId == null)
            {
                return null;
            }

            using (UnitOfWorkManager.Current.SetTenantId(null))
            {
                return _tenantRepository.Get(tenantId.Value).TenancyName;
            }
        }


    }
}
