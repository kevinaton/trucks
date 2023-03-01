using System.Collections.Generic;
using System.Linq;
using Abp.Dependency;
using DispatcherWeb.Authorization.Users;

namespace DispatcherWeb.Imports.DataResolvers.OfficeResolvers
{
    public class OfficeByUserIdResolver : OfficeResolverBase, ITransientDependency, IOfficeResolver
    {
        private readonly UserManager _userManager;

        public OfficeByUserIdResolver(
            UserManager userManager
        )
        {
            _userManager = userManager;
        }

        protected override Dictionary<string, int> GetOfficeStringValueIdDictionary() =>
            _officeStringValueIdDictionary = _userManager.Users
                .Where(u => u.OfficeId.HasValue)
                .Select(u => new { UserId = u.Id, OfficeId = u.OfficeId.Value })
                .ToDictionary(o => o.UserId.ToString(), o => o.OfficeId);
    }
}
