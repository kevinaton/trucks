namespace DispatcherWeb.Scheduling.Dto
{
    public class ScheduleTruckFullDto : ScheduleTruckDto
    {
        public bool HasNoDriver { get; set; }
        public bool HasDriverAssignment { get; set; }

        public static ScheduleTruckFullDto GetFrom(ScheduleTruckDto other)
        {
            var result = new ScheduleTruckFullDto();
            result.CopyAllFieldsFrom(other);
            return result;
        }
    }
}
