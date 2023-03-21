using System;
using System.Collections.Generic;
using System.Text;
using DispatcherWeb.Dto;

namespace DispatcherWeb.MultiTenancy.Dto
{
    public class GetTenantsSelectListInput : GetSelectListInput
    {
        public List<int> EditionIds { get; set; }
        public bool? ActiveFilter { get; set; }
    }
}
