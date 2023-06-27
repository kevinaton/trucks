using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.VehicleCategories;

namespace DispatcherWeb.Quotes
{
    [Table("QuoteServiceVehicleCategory")]
    public class QuoteServiceVehicleCategory : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        public int QuoteServiceId { get; set; }
        public virtual QuoteService QuoteService { get; set; }

        public int VehicleCategoryId { get; set; }
        public virtual VehicleCategory VehicleCategory { get; set; }
    }
}
