namespace DispatcherWeb.Orders.TaxDetails
{
    public class OrderLineTaxDetailsDto : IOrderLineTaxDetails
    {
        public bool IsTaxable { get; set; }
        public decimal MaterialPrice { get; set; }
        public decimal FreightPrice { get; set; }
    }
}
