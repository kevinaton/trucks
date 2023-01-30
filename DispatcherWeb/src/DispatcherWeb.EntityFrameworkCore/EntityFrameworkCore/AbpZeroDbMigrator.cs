using System.Diagnostics;
using System.Transactions;
using Abp.Data;
using Abp.Domain.Uow;
using Abp.EntityFrameworkCore;
using Abp.MultiTenancy;
using Abp.Zero.EntityFrameworkCore;
using DispatcherWeb.Migrations.Seed.Tenants;

namespace DispatcherWeb.EntityFrameworkCore
{
    public class AbpZeroDbMigrator : AbpZeroDbMigrator<DispatcherWebDbContext>
    {
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly DbMigratorConnectionStringResolver _connectionStringResolver;
        private readonly IDbContextResolver _dbContextResolver;

        public AbpZeroDbMigrator(
            IUnitOfWorkManager unitOfWorkManager,
            DbMigratorConnectionStringResolver connectionStringResolver,
            IDbContextResolver dbContextResolver) :
            base(
                unitOfWorkManager,
                connectionStringResolver,
                dbContextResolver)
        {
            _unitOfWorkManager = unitOfWorkManager;
            _connectionStringResolver = connectionStringResolver;
            _dbContextResolver = dbContextResolver;
        }

        public override void CreateOrMigrateForTenant(AbpTenantBase tenant)
        {
            base.CreateOrMigrateForTenant(tenant);
            SeedTenant(tenant);
        }

        private void SeedTenant(AbpTenantBase tenant)
        {
            var args = new DbPerTenantConnectionStringResolveArgs(
                tenant == null ? (int?)null : (int?)tenant.Id,
                tenant == null ? MultiTenancySides.Host : MultiTenancySides.Tenant
            );

            args["DbContextType"] = typeof(DispatcherWebDbContext);
            args["DbContextConcreteType"] = typeof(DispatcherWebDbContext);

            var nameOrConnectionString = ConnectionStringHelper.GetConnectionString(
                _connectionStringResolver.GetNameOrConnectionString(args)
            );

            using(var uow = _unitOfWorkManager.Begin(TransactionScopeOption.Suppress))
            {
                using(var context = _dbContextResolver.Resolve<DispatcherWebDbContext>(nameOrConnectionString, null))
                {
                    Debug.Assert(tenant != null, nameof(tenant) + " != null");
                    new DefaultLocationsCreator(context, tenant.Id).Create();
                    new DefaultServiceCreator(context, tenant.Id).Create();
                    new DefaultUnitOfMeasureCreator(context, tenant.Id).Create();
                    new DefaultTimeClassificationCreator(context, tenant.Id).Create();
                }

                _unitOfWorkManager.Current.SaveChanges();
                uow.Complete();
            }
        }

        public void SeedExistingTenant(AbpTenantBase tenant)
        {
            var args = new DbPerTenantConnectionStringResolveArgs(
                tenant == null ? (int?)null : (int?)tenant.Id,
                tenant == null ? MultiTenancySides.Host : MultiTenancySides.Tenant
            );

            args["DbContextType"] = typeof(DispatcherWebDbContext);
            args["DbContextConcreteType"] = typeof(DispatcherWebDbContext);

            var nameOrConnectionString = ConnectionStringHelper.GetConnectionString(
                _connectionStringResolver.GetNameOrConnectionString(args)
            );

            using (var uow = _unitOfWorkManager.Begin(TransactionScopeOption.Suppress))
            {
                using (_unitOfWorkManager.Current.SetTenantId(tenant.Id))
                {
                    using (var context = _dbContextResolver.Resolve<DispatcherWebDbContext>(nameOrConnectionString, null))
                    {
                        Debug.Assert(tenant != null, nameof(tenant) + " != null");
                        new TenantRoleAndUserBuilder(context, tenant.Id).Create();
                    }

                    _unitOfWorkManager.Current.SaveChanges();
                    uow.Complete();
                }
            }
        }
    }
}
