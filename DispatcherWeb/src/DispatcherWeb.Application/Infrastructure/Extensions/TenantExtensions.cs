using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.MultiTenancy;
using DispatcherWeb.Storage;

namespace DispatcherWeb.Infrastructure.Extensions
{
    public static class TenantExtensions
    {
        public static async Task<string> GetLogoAsBase64StringAsync(this IBinaryObjectManager binaryObjectManager, Tenant tenant)
        {
            return await binaryObjectManager.GetImageAsBase64StringAsync(tenant.ReportsLogoId);
        }

	}
}
