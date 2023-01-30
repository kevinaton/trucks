using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.Locations
{
    [Table("Location")]
    public class Location : FullAuditedEntity, IMustHaveTenant
    {
        public Location()
        {
            SupplierContacts = new HashSet<SupplierContact>();
        }

        public int TenantId { get; set; }

        [StringLength(EntityStringFieldLengths.Location.Name)]
        public string Name { get; set; }

        public int? CategoryId { get; set; }
        
        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxStreetAddressLength)]
        public string StreetAddress { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxCityLength)]
        public string City { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxStateLength)]
        public string State { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxZipCodeLength)]
        public string ZipCode { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxCountryCodeLength)]
        public string CountryCode { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public bool IsActive { get; set; }

        public PredefinedLocationKind? PredefinedLocationKind { get; set; }

        [StringLength(EntityStringFieldLengths.Location.Abbreviation)]
        public string Abbreviation { get; set; }

        [StringLength(EntityStringFieldLengths.Location.Notes)]
        public string Notes { get; set; }

        public string PlaceId { get; set; }

        public int? MergedToId { get; set; }

        public virtual LocationCategory Category { get; set; }

        public virtual ICollection<SupplierContact> SupplierContacts { get; set; }
    }
}
