using Abp.Application.Services.Dto;

namespace DispatcherWeb.Projects.Dto
{
    public class GetProjectServiceForEditInput : NullableIdDto
    {
        public GetProjectServiceForEditInput()
        {
        }

        public GetProjectServiceForEditInput(int? id, int? projectId)
            : base(id)
        {
            ProjectId = projectId;
        }

        public int? ProjectId { get; set; }
    }
}
