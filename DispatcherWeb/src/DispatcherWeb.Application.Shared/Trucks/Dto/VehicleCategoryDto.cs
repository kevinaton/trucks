using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Trucks.Dto
{
    public class VehicleCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public AssetType AssetType { get; set; }
        public bool IsPowered { get; set; }
        public int SortOrder { get; set; }
    }
}
