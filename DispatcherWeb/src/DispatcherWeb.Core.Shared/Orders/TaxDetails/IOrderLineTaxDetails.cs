namespace DispatcherWeb.Orders.TaxDetails
{
    public interface IOrderLineTaxDetails
    {
        bool IsTaxable { get; }
        decimal MaterialPrice { get; }
        decimal FreightPrice { get; }
    }
}
