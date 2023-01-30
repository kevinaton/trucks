namespace DispatcherWeb.Dispatching.Dto
{
    public class GetOrderTotalsResult
    {
        public decimal? FreightQuantity { get; set; }
        public decimal? MaterialQuantity { get; set; }
        public decimal? ActualAmount { get; set; }

        public bool AmountExceedsQuantity => ActualAmount > FreightQuantity || ActualAmount > MaterialQuantity;
    }
}
