using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DispatcherWeb.Web.Models.Account
{
    public class LightLoginViewModel : LoginViewModel
    {
        [Required]
        public string TenancyName { get; set; }
    }
}
