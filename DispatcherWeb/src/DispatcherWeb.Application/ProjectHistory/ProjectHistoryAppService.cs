using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Events.Bus.Entities;
using Abp.Events.Bus.Handlers;
using DispatcherWeb.ProjectHistory.Dto;
using DispatcherWeb.Projects;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Linq.Extensions;
using DispatcherWeb.Authorization;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.ProjectHistory
{
    [AbpAuthorize(AppPermissions.Pages_Projects)]
    public class ProjectHistoryAppService : DispatcherWebAppServiceBase,
        IProjectHistoryAppService,
        IAsyncEventHandler<EntityCreatedEventData<Project>>,
        IAsyncEventHandler<EntityUpdatedEventData<Project>>
    {
        private readonly IRepository<ProjectHistoryRecord> _projectHistoryRepository;

        public ProjectHistoryAppService(IRepository<ProjectHistoryRecord> projectHistoryRepository)
        {
            _projectHistoryRepository = projectHistoryRepository;
        }

        public async Task HandleEventAsync(EntityCreatedEventData<Project> eventData)
        {
            using (var uow = UnitOfWorkManager.Begin())
            {
                await AddProjectHistory(new AddProjectHistoryInput(eventData.Entity.Id, ProjectHistoryAction.Created));
                await uow.CompleteAsync();
            }
        }

        public async Task HandleEventAsync(EntityUpdatedEventData<Project> eventData)
        {
            using (var uow = UnitOfWorkManager.Begin())
            {
                await AddProjectHistory(new AddProjectHistoryInput(eventData.Entity.Id, ProjectHistoryAction.Edited));
                await uow.CompleteAsync();
            }
        }

        public async Task AddProjectHistory(AddProjectHistoryInput input)
        {
            await _projectHistoryRepository.InsertAsync(new ProjectHistoryRecord
            {
                Action = input.Action,
                DateTime = DateTime.UtcNow,
                OfficeId = Session.OfficeId,
                UserId = Session.UserId,
                ProjectId = input.ProjectId,
            });
        }

        public async Task<PagedResultDto<ProjectHistoryDto>> GetProjectHistory(GetProjectHistoryInput input)
        {
            var query = _projectHistoryRepository.GetAll();

            query = query
                .WhereIf(Session.OfficeId.HasValue, x => x.OfficeId == Session.OfficeId)
                .WhereIf(input.StartDate.HasValue, x => x.DateTime >= input.StartDate)
                .WhereIf(input.EndDate.HasValue, x => x.DateTime < input.EndDate.Value.AddDays(1))
                .WhereIf(input.ProjectId.HasValue, x => x.ProjectId == input.ProjectId);

            var totalCount = await query.CountAsync();

            var items = await query
                .Select(x => new ProjectHistoryDto
                {
                    Id = x.Id,
                    ProjectName = x.Project.Name,
                    ProjectId = x.ProjectId,
                    DateTime = x.DateTime,
                    UserName = x.User.Name + " " + x.User.Surname,
                    Action = x.Action
                })
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<ProjectHistoryDto>(
                totalCount,
                items);
        }
    }
}
