using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Offices;
using DispatcherWeb.Storage;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Infrastructure.AzureBlobs
{
    public class LogoProvider : DispatcherWebDomainServiceBase, ILogoProvider/*, ITransientDependency*/
    {
        private readonly IBinaryObjectManager _binaryObjectManager;
        private readonly IRepository<Office> _officeRepository;

        public LogoProvider(
            IBinaryObjectManager binaryObjectManager,
            IRepository<Office> officeRepository
            )
        {
            _binaryObjectManager = binaryObjectManager;
            _officeRepository = officeRepository;
        }

        public async Task<string> GetReportLogoAsBase64StringAsync(int? officeId)
        {
            Guid? reportsLogoId = null;
            
            if (officeId != null)
            {
                var office = await _officeRepository.GetAll()
                    .Where(x => x.Id == officeId)
                    .Select(x => new 
                    {
                        x.ReportsLogoId
                    })
                    .FirstAsync();
                reportsLogoId = office.ReportsLogoId;
            }
            
            if (reportsLogoId == null)
            {
                //using (CurrentUnitOfWork.SetTenantId(null))
                //{
                var tenant = await TenantManager.Tenants
                    .Where(x => x.Id == Session.GetTenantId())
                    .Select(x => new
                    {
                        x.ReportsLogoId
                    })
                    .FirstAsync();
                //}
                reportsLogoId = tenant.ReportsLogoId;
            }
            return await _binaryObjectManager.GetImageAsBase64StringAsync(reportsLogoId);
        }
    }
}
