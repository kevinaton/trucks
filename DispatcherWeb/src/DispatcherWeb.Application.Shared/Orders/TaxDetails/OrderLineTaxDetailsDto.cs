namespace DispatcherWeb.Orders.TaxDetails
{
    public class OrderLineTaxDetailsDto : IOrderLineTaxDetailsWithMultipleActualAmounts
    {
        public bool IsTaxable { get; set; }
        public decimal MaterialPrice { get; set; }
        public decimal FreightPrice { get; set; }
        public bool HasAllActualAmounts { get; set; }
    }
}
