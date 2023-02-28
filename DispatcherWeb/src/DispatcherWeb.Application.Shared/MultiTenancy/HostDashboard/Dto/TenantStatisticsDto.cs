namespace DispatcherWeb.MultiTenancy.HostDashboard.Dto
{
    public class TenantStatisticsDto
    {
        public int TenantId { get; set; }
        public string TenantName { get; set; }
        public int NumberOfTrucks { get; set; }
        public int NumberOfUsers { get; set; }
        public int NumberOfUsersActive { get; set; }
        public int OrderLines { get; set; }
        public int TrucksScheduled { get; set; }
        public int LeaseHaulersOrderLines { get; set; }
        public int TicketsCreated { get; set; }
        public int SmsSent { get; set; }
    }
}
