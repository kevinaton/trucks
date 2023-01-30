﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Offices;
using DispatcherWeb.UnitsOfMeasure;

namespace DispatcherWeb.Services
{
    [Table("OfficeServicePrice")]
    public class OfficeServicePrice : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        [Required(ErrorMessage = "Service/Product Item is a required field")]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Office is a required field")]
        public int OfficeId { get; set; }

        public int? MaterialUomId { get; set; }

        public int? FreightUomId { get; set; }

        [Column(TypeName = "money")]
        public decimal? PricePerUnit { get; set; }

        [Column(TypeName = "money")]
        public decimal? FreightRate { get; set; }

        [Required(ErrorMessage = "Designation is a required field")]
        public DesignationEnum Designation { get; set; }

        public virtual Service Service { get; set; }

        public virtual Office Office { get; set; }

        public virtual UnitOfMeasure MaterialUom { get; set; }

        public virtual UnitOfMeasure FreightUom { get; set; }
    }
}
