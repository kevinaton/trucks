using System;

namespace DispatcherWeb.ProjectHistory.Dto
{
    public class ProjectHistoryDto
    {
        public int Id { get; set; }

        public string ProjectName { get; set; }

        public int ProjectId { get; set; }

        public string UserName { get; set; }

        public DateTime? DateTime { get; set; }

        public ProjectHistoryAction Action { get; set; }
        public string ActionName => Action.GetDisplayName();
    }
}
