using System;

namespace DispatcherWeb.Projects.Dto
{
    public class ProjectDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public QuoteStatus Status { get; set; }
        public string StatusName => Status.GetDisplayName();
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Location { get; set; }
    }
}
