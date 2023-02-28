namespace DispatcherWeb.Drivers.Dto
{
    public class GetDriverPayRateInput
    {
        public int? UserId { get; set; }
        public int? DriverId { get; set; }
        public int? TimeClassificationId { get; set; }
        public bool? ProductionPay { get; set; }
    }
}
