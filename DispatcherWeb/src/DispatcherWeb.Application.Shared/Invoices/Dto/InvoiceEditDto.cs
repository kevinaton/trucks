using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.Invoices.Dto
{
    public class InvoiceEditDto
    {
        public int? Id { get; set; }

        [Required]
        public int? CustomerId { get; set; }

        public string CustomerName { get; set; }

        [StringLength(EntityStringFieldLengths.General.Email)]
        public string EmailAddress { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.FullAddress)]
        public string BillingAddress { get; set; }

        public BillingTermsEnum? Terms { get; set; }

        [Required]
        public DateTime? IssueDate { get; set; }

        public DateTime? DueDate { get; set; }

        public decimal BalanceDue { get; set; }

        public decimal TaxRate { get; set; }

        public decimal Subtotal { get; set; }

        public decimal TaxAmount { get; set; }

        public InvoiceStatus Status { get; set; }

        public string StatusName => Status.GetDisplayName();

        public int? UploadBatchId { get; set; }

        public int? BatchId { get; set; }

        public string Message { get; set; }

        public string JobNumber { get; set; }

        public string PoNumber { get; set; }

        public InvoicingMethodEnum? CustomerInvoicingMethod { get; set; }

        public ShowFuelSurchargeOnInvoiceEnum ShowFuelSurchargeOnInvoice { get; set; }

        public int? FuelServiceId { get; set; }

        public string FuelServiceName { get; set; }

        public bool FuelServiceIsTaxable { get; set; }

        public List<InvoiceLineEditDto> InvoiceLines { get; set; }
    }
}
