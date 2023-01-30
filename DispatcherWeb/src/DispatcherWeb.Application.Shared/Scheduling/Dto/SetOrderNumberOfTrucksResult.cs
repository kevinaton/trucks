namespace DispatcherWeb.Scheduling.Dto
{
    public class SetOrderNumberOfTrucksResult
    {
        public double? NumberOfTrucks { get; set; }
        public decimal? OrderUtilization { get; set; }
        public StaggeredTimeKind StaggeredTimeKind { get; set; }
        public bool IsTimeStaggered { get; set; }
        public bool IsTimeEditable { get; set; }
    }
}
