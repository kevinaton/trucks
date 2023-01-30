using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Configuration.Host.Dto
{
    public class SmsSettingsEditDto
    {
		public string AccountSid { get; set; }
		public string AuthToken { get; set; }
		public string PhoneNumber { get; set; }
    }
}
