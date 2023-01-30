namespace DispatcherWeb.Orders.Dto
{
    public class GetOrderLineOfficeAmountForEditInput
    {
        public int OrderLineId { get; set; }

        public GetOrderLineOfficeAmountForEditInput()
        {
        }

        public GetOrderLineOfficeAmountForEditInput(int orderLineId)
        {
            OrderLineId = orderLineId;
        }
    }
}
