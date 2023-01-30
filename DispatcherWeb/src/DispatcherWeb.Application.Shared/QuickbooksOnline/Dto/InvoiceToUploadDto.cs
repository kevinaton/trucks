using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DispatcherWeb.QuickbooksOnline.Dto
{
    public class InvoiceToUploadDto<TInvoice> //DispatcherWeb.Invoices.Invoice isn't accessible from this layer
    {
        public TInvoice Invoice { get; set; }
        public int InvoiceId { get; set; }
        public string BillingAddress { get; set; }
        public string EmailAddress { get; set; }
        public CustomerToUploadDto Customer { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? IssueDate { get; set; }
        public string Message { get; set; }
        public decimal Tax { get; set; }
        public decimal TaxRate { get; set; }
        public decimal TotalAmount { get; set; }
        public BillingTermsEnum? Terms { get; set; }
        public List<InvoiceLineToUploadDto> InvoiceLines { get; set; }

        public string GetJobNumberOnly()
        {
            if (InvoiceLines == null || Customer == null)
            {
                return null;
            }

            var jobNumbers = InvoiceLines.Select(x => x.JobNumber).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
            if (Customer.InvoicingMethod == InvoicingMethodEnum.SeparateTicketsByJobNumber && jobNumbers.Count <= 1)
            {
                return jobNumbers.FirstOrDefault();
            }

            return null;
        }

        public string GetPoNumberOrJobNumber()
        {
            var jobNumber = GetJobNumberOnly();
            if (!string.IsNullOrEmpty(jobNumber))
            {
                return jobNumber;
            }

            if (InvoiceLines == null || Customer == null)
            {
                return null;
            }

            var poNumbers = InvoiceLines.Select(x => x.PONumber).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
            if (poNumbers.Count <= 1)
            {
                return poNumbers.FirstOrDefault();
            }

            return null;
        }
    }
}
