using DispatcherWeb.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Trucks.Dto
{
    public class GetVehicleCategoriesSelectListInput : GetSelectListInput
    {
        public bool? IsPowered { get; set; }
        public bool? IsInUse { get; set; }
    }
}
