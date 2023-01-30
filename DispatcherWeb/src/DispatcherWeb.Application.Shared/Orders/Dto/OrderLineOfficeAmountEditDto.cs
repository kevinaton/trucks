namespace DispatcherWeb.Orders.Dto
{
    public class OrderLineOfficeAmountEditDto
    {
        public int Id { get; set; }
        public int OrderLineId { get; set; }
        //public int OfficeId { get; set; }
        public decimal? ActualQuantity { get; set; }
    }
}
