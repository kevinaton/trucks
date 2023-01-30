using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Orders.TaxDetails;
using DispatcherWeb.Services;
using DispatcherWeb.Locations;
using DispatcherWeb.UnitsOfMeasure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DispatcherWeb.Orders
{
    [Table("ReceiptLine")]
    public class ReceiptLine : FullAuditedEntity, IMustHaveTenant
    {
        public ReceiptLine()
        {
            Tickets = new HashSet<Ticket>();
        }

        public int TenantId { get; set; }

        public int ReceiptId { get; set; }

        public int? OrderLineId { get; set; }

        public virtual OrderLine OrderLine { get; set; }

        public virtual Receipt Receipt { get; set; }

        public int LineNumber { get; set; }

        public int? LoadAtId { get; set; }

        public virtual Location LoadAt { get; set; }

        public int? DeliverToId { get; set; }

        public virtual Location DeliverTo { get; set; }

        [Required(ErrorMessage = "Service/Product Item is a required field")]
        public int ServiceId { get; set; }

        public virtual Service Service { get; set; }

        [Required(ErrorMessage = "Designation is a required field")]
        public DesignationEnum Designation { get; set; }

        //[Required(ErrorMessage = "Material UOM is a required field")]
        public int? MaterialUomId { get; set; }

        public UnitOfMeasure MaterialUom { get; set; }
        
        [Column(TypeName = "money")]
        public decimal MaterialRate { get; set; }
                
        public decimal? MaterialQuantity { get; set; }

        [Column(TypeName = "money")]
        public decimal MaterialAmount { get; set; }


        //[Required(ErrorMessage = "Freight UOM is a required field")]
        public int? FreightUomId { get; set; }

        public UnitOfMeasure FreightUom { get; set; }

        [Column(TypeName = "money")]
        public decimal FreightRate { get; set; }

        public decimal? FreightQuantity { get; set; }

        [Column(TypeName = "money")]
        public decimal FreightAmount { get; set; }

        public bool IsMaterialAmountOverridden { get; set; }

        public bool IsFreightAmountOverridden { get; set; }

        public bool IsMaterialRateOverridden { get; set; }

        public bool IsFreightRateOverridden { get; set; }

        public ICollection<Ticket> Tickets { get; set; }

        public static ReceiptLine FromOrderLine(OrderLine orderLine, decimal materialQuantity, decimal freightQuantity)
        {
            var freightAmount = (orderLine.FreightPricePerUnit ?? 0) * freightQuantity;
            var materialAmount = (orderLine.MaterialPricePerUnit ?? 0) * materialQuantity;
            return new ReceiptLine
            {
                Designation = orderLine.Designation,
                FreightAmount = orderLine.IsFreightPriceOverridden ? orderLine.FreightPrice : freightAmount,
                MaterialAmount = orderLine.IsMaterialPriceOverridden ? orderLine.MaterialPrice : materialAmount,
                //Receipt = receipt,
                MaterialUomId = orderLine.MaterialUomId,
                MaterialQuantity = materialQuantity,
                FreightQuantity = freightQuantity,
                FreightRate = orderLine.FreightPricePerUnit ?? 0,
                MaterialRate = orderLine.MaterialPricePerUnit ?? 0,
                IsFreightAmountOverridden = orderLine.IsFreightPriceOverridden,
                IsMaterialAmountOverridden = orderLine.IsMaterialPriceOverridden,
                IsFreightRateOverridden = orderLine.IsFreightPricePerUnitOverridden,
                IsMaterialRateOverridden = orderLine.IsMaterialPricePerUnitOverridden,
                FreightUomId = orderLine.FreightUomId,
                OrderLineId = orderLine.Id,
                LineNumber = orderLine.LineNumber,
                ServiceId = orderLine.ServiceId,
                TenantId = orderLine.TenantId,
                LoadAtId = orderLine.LoadAtId,
                DeliverToId = orderLine.DeliverToId
            };
        }
    }
}
