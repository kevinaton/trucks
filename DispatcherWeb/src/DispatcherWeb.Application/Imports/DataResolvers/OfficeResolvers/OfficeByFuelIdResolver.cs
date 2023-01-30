using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Castle.Core.Internal;
using DispatcherWeb.Offices;

namespace DispatcherWeb.Imports.DataResolvers.OfficeResolvers
{
    public class OfficeByFuelIdResolver : OfficeResolverBase, ITransientDependency, IOfficeResolver
    {
        protected readonly IRepository<Office> _officeRepository;
        public OfficeByFuelIdResolver(IRepository<Office> officeRepository) 
        {
            _officeRepository = officeRepository;
        }

        protected override Dictionary<string, int> GetOfficeStringValueIdDictionary() =>
            _officeRepository.GetAll()
                .Where(o => !o.FuelIds.IsNullOrEmpty())
                .Select(o => new { o.Id, o.FuelIds })
                .ToList()
                .Select(o => new { o.Id, FuelIds = o.FuelIds.Split('|') })
                .SelectMany(o => o.FuelIds, (o, fuelId) => new { Id = o.Id, FuelId = fuelId })
                .ToDictionary(o => o.FuelId, o => o.Id);
    }
}
