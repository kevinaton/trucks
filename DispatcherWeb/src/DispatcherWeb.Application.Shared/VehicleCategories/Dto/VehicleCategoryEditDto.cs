using System.ComponentModel.DataAnnotations;

namespace DispatcherWeb.VehicleCategories.Dto
{
    public class VehicleCategoryEditDto
    {
        public int? Id { get; set; }

        [Display(Description = "Name")]
        public string Name { get; set; }

        [Display(Description = "Asset Type")]
        public AssetType AssetType { get; set; }

        [Display(Description = "Is Powered")]
        public bool IsPowered { get; set; }

        public int SortOrder { get; set; }
    }
}
