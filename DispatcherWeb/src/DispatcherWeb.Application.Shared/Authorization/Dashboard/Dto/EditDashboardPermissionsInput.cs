using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DispatcherWeb.Authorization.Dashboard.Dto
{
    public class EditDashboardPermissionsInput
    {
        [Required]
        public List<string> GrantedPermissionNames { get; set; }
    }
}
