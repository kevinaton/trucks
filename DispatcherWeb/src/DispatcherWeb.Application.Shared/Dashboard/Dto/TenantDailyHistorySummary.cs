using System.Collections.Generic;
using Newtonsoft.Json;

namespace DispatcherWeb.Dashboard.Dto
{
    public class TenantDailyHistorySummary
    {
        public int TenantId { get; set; }
        public string TenantName { get; set; }
        public int MonthYear { get; set; }
        public string MonthName { get; set; }
        public int MonthNumber { get; set; }
        public int ActiveUsers { get; set; }
        public int TrucksScheduled { get; set; }
        public int OrderLines { get; set; }
        public int Tickets { get; set; }
        public int Sms { get; set; }
        public int TenantsAdded { get; set; }

        public static List<TenantDailyHistorySummary> ReadFromJson(string jsonSource) =>
            JsonConvert.DeserializeObject<List<TenantDailyHistorySummary>>(jsonSource);
    }

    public class TenantDto
    {
        public int TenantId { get; set; }
        public string TenantName { get; set; }

        public static List<TenantDto> ReadFromJson(string jsonSource) =>
            JsonConvert.DeserializeObject<List<TenantDto>>(jsonSource);
    }
}
