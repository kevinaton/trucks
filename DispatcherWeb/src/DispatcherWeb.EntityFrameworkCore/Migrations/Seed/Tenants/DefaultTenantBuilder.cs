using System.Linq;
using Abp.MultiTenancy;
using DispatcherWeb.Editions;
using DispatcherWeb.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Migrations.Seed.Tenants
{
    public class DefaultTenantBuilder
    {
        private readonly DispatcherWebDbContext _context;

        public DefaultTenantBuilder(DispatcherWebDbContext context)
        {
            _context = context;
        }

        public void Create()
        {
            CreateDefaultTenant();
        }

        private void CreateDefaultTenant()
        {
            //Default tenant

            var defaultTenant = _context.Tenants.IgnoreQueryFilters().FirstOrDefault(t => t.TenancyName == MultiTenancy.Tenant.DefaultTenantName);
            if (defaultTenant == null)
            {
                defaultTenant = new MultiTenancy.Tenant(AbpTenantBase.DefaultTenantName, AbpTenantBase.DefaultTenantName);

                var defaultEdition = _context.Editions.IgnoreQueryFilters().OfType<SubscribableEdition>().FirstOrDefault(e => e.Name == EditionManager.DefaultEditionName);
                if (defaultEdition != null)
                {
                    defaultTenant.EditionId = defaultEdition.Id;
                }

                _context.Tenants.Add(defaultTenant);
                _context.SaveChanges();
            }
        }
    }
}
