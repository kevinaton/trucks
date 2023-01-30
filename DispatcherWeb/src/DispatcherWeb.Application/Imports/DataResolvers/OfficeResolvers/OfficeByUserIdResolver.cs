using Abp.Dependency;
using Abp.Domain.Repositories;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Offices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
