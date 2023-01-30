using DispatcherWeb.Orders.TaxDetails;

namespace DispatcherWeb.Orders.Dto
{
    public class DeleteOrderLineOutput
    {
        public IOrderTaxDetailsWithActualAmounts OrderTaxDetails { get; set; }
    }
}
