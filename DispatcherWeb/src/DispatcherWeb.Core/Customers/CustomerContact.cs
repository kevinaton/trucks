using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.Customers
{
    [Table("CustomerContact")]
    public class CustomerContact : FullAuditedEntity, IMustHaveTenant
    {
        public CustomerContact()
        {
        }

        public int TenantId { get; set; }

        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Firstname is a required field")]
        [StringLength(EntityStringFieldLengths.CustomerContact.FirstName)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Lastname is a required field")]
        [StringLength(EntityStringFieldLengths.CustomerContact.LastName)]
        public string LastName { get; set; }

        [StringLength(EntityStringFieldLengths.General.PhoneNumber)]
        public string PhoneNumber { get; set; }

        [StringLength(EntityStringFieldLengths.General.PhoneNumber)]
        public string Fax { get; set; }

        [StringLength(EntityStringFieldLengths.General.Email)]
        public string Email { get; set; }

        [StringLength(EntityStringFieldLengths.CustomerContact.Title)]
        public string Title { get; set; }

        public bool IsActive { get; set; }
        public bool HasCustomerPortalAccess { get; set; }

        public virtual Customer Customer { get; set; }

    }
}
