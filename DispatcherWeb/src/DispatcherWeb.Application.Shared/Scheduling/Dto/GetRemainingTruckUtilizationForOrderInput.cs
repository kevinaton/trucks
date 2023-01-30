namespace DispatcherWeb.Scheduling.Dto
{
    public class GetRemainingTruckUtilizationForOrderInput
    {
        public decimal OrderUtilization { get; set; }
        public decimal OrderMaxUtilization { get; set; }
        public decimal TruckUtilization { get; set; }
        public AssetType AssetType { get; set; }
        public bool IsPowered { get; set; }

        public static GetRemainingTruckUtilizationForOrderInput From(ScheduleOrderDto order, ScheduleTruckDto truck)
        {
            return new GetRemainingTruckUtilizationForOrderInput
            {
                OrderUtilization = order.Utilization,
                OrderMaxUtilization = order.MaxUtilization,
                TruckUtilization = truck.Utilization,
                AssetType = truck.VehicleCategory.AssetType,
                IsPowered = truck.VehicleCategory.IsPowered
            };
        }

        public static GetRemainingTruckUtilizationForOrderInput From(ScheduleOrderLineDto order, ScheduleTruckDto truck)
        {
            return new GetRemainingTruckUtilizationForOrderInput
            {
                OrderUtilization = order.Utilization,
                OrderMaxUtilization = order.MaxUtilization,
                TruckUtilization = truck.Utilization,
                AssetType = truck.VehicleCategory.AssetType,
                IsPowered = truck.VehicleCategory.IsPowered
            };
        }
    }
}
