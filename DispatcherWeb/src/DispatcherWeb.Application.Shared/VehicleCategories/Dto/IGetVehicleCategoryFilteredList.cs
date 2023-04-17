using DispatcherWeb;

namespace DispatcherWeb.VehicleCategories.Dto
{
    public interface IGetVehicleCategoryFilteredList
    {
        string Name { get; set; }

        AssetType? AssetType { get; set; }

        bool? IsPowered { get; set; }
    }
}

/*
 
Name 
AssetType 
IsPowered 
SortOrder 

*/