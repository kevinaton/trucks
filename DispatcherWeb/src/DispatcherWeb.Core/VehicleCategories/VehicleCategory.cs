using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;

namespace DispatcherWeb.VehicleCategories
{
    [Table("VehicleCategory")]
    public class VehicleCategory : Entity
    {
        [StringLength(50)]
        public string Name { get; set; }

        public AssetType AssetType { get; set; }

        public bool IsPowered { get; set; }

        public int SortOrder { get; set; }

    }
}
