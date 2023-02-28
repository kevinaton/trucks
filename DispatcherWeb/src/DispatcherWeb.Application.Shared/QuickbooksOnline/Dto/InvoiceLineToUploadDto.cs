using System;
using System.Linq;
using DispatcherWeb.Orders.TaxDetails;

namespace DispatcherWeb.QuickbooksOnline.Dto
{
    public class InvoiceLineToUploadDto : IOrderLineTaxTotalDetails
    {
        public DateTime? DeliveryDateTime { get; set; }
        public string Description { get; set; }
        public string DescriptionAndTicketWithTruck => GetDescriptionAndTicketWithTruck();
        public decimal Subtotal { get; set; }
        public decimal ExtendedAmount { get; set; }
        public decimal FreightExtendedAmount { get; set; }
        public decimal MaterialExtendedAmount { get; set; }
        public decimal Tax { get; set; }
        public bool? IsTaxable { get; set; }
        bool IOrderLineTaxDetails.IsTaxable => IsTaxable ?? true;
        public string LeaseHaulerName { get; set; }
        public short LineNumber { get; set; }
        public string TicketNumber { get; set; }
        public string TruckCode { get; set; }
        public decimal Quantity { get; set; }
        public int? ItemId { get; set; }
        public string ItemName { get; set; }
        public bool? ItemIsInQuickBooks { get; set; }
        public ServiceType? ItemType { get; set; }
        public string ItemIncomeAccount { get; set; }
        public TicketToUploadDto Ticket { get; set; }

        decimal IOrderLineTaxTotalDetails.TotalAmount { get => ExtendedAmount; set => ExtendedAmount = value; }
        decimal IOrderLineTaxDetails.MaterialPrice => MaterialExtendedAmount;
        decimal IOrderLineTaxDetails.FreightPrice => FreightExtendedAmount;

        public string PONumber { get; set; }
        public string JobNumber { get; set; }
        public ChildInvoiceLineKind? ChildInvoiceLineKind { get; set; }

        private string GetDescriptionAndTicketWithTruck()
        {
            string ticket = null;
            string truck = null;
            if (!string.IsNullOrEmpty(TicketNumber))
            {
                ticket = $"Ticket: {TicketNumber}";
            }
            if (!string.IsNullOrEmpty(TruckCode))
            {
                truck = $"Truck: {TruckCode}";
            }
            if (ticket == null && truck == null)
            {
                return Description;
            }
            var ticketAndTruck = string.Join(", ", new[] { ticket, truck }.Where(x => !string.IsNullOrEmpty(x)));
            return Description + " " + ticketAndTruck;
        }

        public InvoiceLineToUploadDto Clone()
        {
            return new InvoiceLineToUploadDto
            {
                DeliveryDateTime = DeliveryDateTime,
                Description = Description,
                Subtotal = Subtotal,
                ExtendedAmount = ExtendedAmount,
                FreightExtendedAmount = FreightExtendedAmount,
                MaterialExtendedAmount = MaterialExtendedAmount,
                Tax = Tax,
                IsTaxable = IsTaxable,
                LeaseHaulerName = LeaseHaulerName,
                LineNumber = LineNumber,
                TicketNumber = TicketNumber,
                TruckCode = TruckCode,
                Quantity = Quantity,
                ItemId = ItemId,
                ItemName = ItemName,
                ItemIsInQuickBooks = ItemIsInQuickBooks,
                ItemType = ItemType,
                ItemIncomeAccount = ItemIncomeAccount,
                Ticket = Ticket?.Clone(),
                PONumber = PONumber,
                JobNumber = JobNumber
            };
        }
    }
}
