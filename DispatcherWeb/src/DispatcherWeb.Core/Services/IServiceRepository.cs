using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Repositories;

namespace DispatcherWeb.Services
{
    public interface IServiceRepository : IRepository<Service>
    {
        Task MergeServicesAsync(List<int> recordIds, int mainRecordId, int? tenantId);
    }
}
