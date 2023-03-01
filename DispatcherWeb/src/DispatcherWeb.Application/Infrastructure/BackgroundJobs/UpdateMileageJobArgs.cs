namespace DispatcherWeb.Infrastructure.BackgroundJobs
{
    public class UpdateMileageJobArgs
    {
        public int TenantId { get; set; }
        public long UserId { get; set; }

    }
}
