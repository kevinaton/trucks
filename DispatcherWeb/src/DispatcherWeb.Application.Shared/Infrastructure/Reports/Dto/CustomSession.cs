using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Infrastructure.Reports.Dto
{
	[Serializable]
    public class CustomSession
	{
		public CustomSession(int tenantId, long userId)
		{
			TenantId = tenantId;
			UserId = userId;
		}

		public int TenantId { get; set; }
		public long UserId { get; set; }
    }
}
