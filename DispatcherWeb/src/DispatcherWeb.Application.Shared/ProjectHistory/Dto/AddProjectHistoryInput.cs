namespace DispatcherWeb.ProjectHistory.Dto
{
    public class AddProjectHistoryInput
    {
        public ProjectHistoryAction Action { get; set; }

        public int ProjectId { get; set; }

        public AddProjectHistoryInput()
        {
        }

        public AddProjectHistoryInput(int projectId, ProjectHistoryAction action)
        {
            ProjectId = projectId;// project.Id;
            Action = action;
        }
    }
}
