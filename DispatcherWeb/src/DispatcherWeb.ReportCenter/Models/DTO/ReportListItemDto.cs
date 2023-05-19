using System.Linq;

namespace DispatcherWeb.ReportCenter.Models.DTO
{
    public class ReportListItemDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string NameAndDescription => string.Join(" - ", new[] { Name, Description }.Where(x => !string.IsNullOrWhiteSpace(x)));
        public string Path { get; set; }
    }
}
