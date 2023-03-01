using System.Collections.Generic;
using System.Linq;
using Abp.Dependency;
using Abp.Domain.Repositories;
using DispatcherWeb.Offices;

namespace DispatcherWeb.Imports.DataResolvers.OfficeResolvers
{
    public class OfficeByNameResolver : OfficeResolverBase, ITransientDependency, IOfficeResolver
    {
        private readonly IRepository<Office> _officeRepository;

        public OfficeByNameResolver(
            IRepository<Office> officeRepository
        )
        {
            _officeRepository = officeRepository;
        }

        protected override Dictionary<string, int> GetOfficeStringValueIdDictionary() =>
            _officeStringValueIdDictionary = _officeRepository.GetAll()
                .GroupBy(o => o.Name) // In case there are offices with the same name
                .Select(o => new { o.First().Id, Name = o.Key })
                .ToDictionary(o => o.Name.ToLowerInvariant(), o => o.Id);
    }
}
