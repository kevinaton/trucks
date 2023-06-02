using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.LuckStone
{
    [Table("LuckStoneEarnings")]
    public class LuckStoneEarnings : FullAuditedEntity, IMustHaveTenant
    {
        //LuckStoneTicketId – primary key, unique constraint

        public int TenantId { get; set; }

        public DateTime TicketDateTime { get; set; }

        [StringLength(EntityStringFieldLengths.LuckStoneEarnings.Site)]
        public string Site { get; set; }

        [StringLength(EntityStringFieldLengths.LuckStoneEarnings.HaulerRef)]
        public string HaulerRef { get; set; }

        [StringLength(EntityStringFieldLengths.LuckStoneEarnings.CustomerName)]
        public string CustomerName { get; set; }

        [StringLength(EntityStringFieldLengths.LuckStoneEarnings.LicensePlate)]
        public string LicensePlate { get; set; }

        public decimal HaulPaymentRate { get; set; }

        public decimal NetTons { get; set; }

        public decimal HaulPayment { get; set; }

        [StringLength(EntityStringFieldLengths.LuckStoneEarnings.Uom)]
        public string HaulPaymentRateUom { get; set; }

        public decimal FscAmount { get; set; }

        [StringLength(EntityStringFieldLengths.LuckStoneEarnings.ProductDescription)]
        public string ProductDescription { get; set; }


        public int BatchId { get; set; }

        public virtual LuckStoneEarningsBatch Batch { get; set; }
    }
}
