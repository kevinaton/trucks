namespace DispatcherWeb.Orders.TaxDetails
{
    public class OrderLineTaxTotalDetailsDto : IOrderLineTaxTotalDetails
    {
        public decimal Tax { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TotalAmount { get; set; }

        public bool IsTaxable { get; set; }

        public decimal MaterialPrice { get; set; }

        public decimal FreightPrice { get; set; }
    }
}
