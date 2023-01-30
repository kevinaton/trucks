using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.LeaseHaulerRequests;

namespace DispatcherWeb.LeaseHaulers
{
    [Table("LeaseHauler")]
    public class LeaseHauler : FullAuditedEntity, IMustHaveTenant
    {
        public LeaseHauler()
        {
            LeaseHaulerContacts = new HashSet<LeaseHaulerContact>();
            LeaseHaulerDrivers = new HashSet<LeaseHaulerDriver>();
            LeaseHaulerTrucks = new HashSet<LeaseHaulerTruck>();
        }

        public int TenantId { get; set; }

        /// <summary>
        /// HaulingCompany's Tenant id. Only set for MaterialCompany LeaseHaulers which represent a whole HaulingCompany tenant
        /// </summary>
        public int? HaulingCompanyTenantId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxStreetAddressLength)]
        public string StreetAddress1 { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxStreetAddressLength)]
        public string StreetAddress2 { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxCityLength)]
        public string City { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxStateLength)]
        public string State { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxZipCodeLength)]
        public string ZipCode { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxCountryCodeLength)]
        public string CountryCode { get; set; }

        [StringLength(30)]
        public string AccountNumber { get; set; }

        [StringLength(20)]
        public string PhoneNumber { get; set; }

        public bool IsActive { get; set; }

        public virtual ICollection<LeaseHaulerContact> LeaseHaulerContacts { get; set; }

        public virtual ICollection<LeaseHaulerDriver> LeaseHaulerDrivers { get; set; }

        public virtual ICollection<LeaseHaulerTruck> LeaseHaulerTrucks { get; set; }

        public virtual ICollection<AvailableLeaseHaulerTruck> AvailableLeaseHaulerTrucks { get; set; }
    }
}
