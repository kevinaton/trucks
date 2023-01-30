using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DispatcherWeb.Web.DriverApplication.Models.DriverApplication
{
    public class IndexViewModel
    {
        public string IdentityServerUri { get; set; }
        public string ApiUri { get; set; }
        public string DriverAppUri { get; set; }
        public string WebPushServerPublicKey { get; set; }
    }
}
