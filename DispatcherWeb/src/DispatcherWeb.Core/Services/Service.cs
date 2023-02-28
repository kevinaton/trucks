using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Projects;
using DispatcherWeb.Quotes;

namespace DispatcherWeb.Services
{
    [Table("Service")]
    public class Service : FullAuditedEntity, IMustHaveTenant
    {
        public Service()
        {
            OfficeServicePrices = new HashSet<OfficeServicePrice>();
        }

        public int TenantId { get; set; }

        [Column("Service")]
        [Required(ErrorMessage = "Service is a required field")]
        [StringLength(EntityStringFieldLengths.Service.Service1)]
        public string Service1 { get; set; }

        [StringLength(EntityStringFieldLengths.Service.Description)]
        public string Description { get; set; }

        public bool IsActive { get; set; }

        public ServiceType? Type { get; set; }

        public bool IsTaxable { get; set; }

        [StringLength(EntityStringFieldLengths.Service.IncomeAccount)]
        public string IncomeAccount { get; set; }

        public bool IsInQuickBooks { get; set; }

        public int? MergedToId { get; set; }

        public virtual ICollection<OfficeServicePrice> OfficeServicePrices { get; set; }

        public virtual ICollection<ProjectService> ProjectServices { get; set; }

        public virtual ICollection<QuoteService> QuoteServices { get; set; }

    }
}
