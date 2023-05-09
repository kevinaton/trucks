using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Common.Dto;
using DispatcherWeb.Configuration;
using DispatcherWeb.Dto;
using DispatcherWeb.Locations;
using DispatcherWeb.Orders;
using DispatcherWeb.Projects.Dto;
using DispatcherWeb.Quotes;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Projects
{
    [AbpAuthorize]
    public class ProjectAppService : DispatcherWebAppServiceBase, IProjectAppService
    {
        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<ProjectService> _projectServiceRepository;
        private readonly IRepository<Location> _locationRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<Quote> _quoteRepository;

        public ProjectAppService(
            IRepository<Project> projectRepository,
            IRepository<ProjectService> projectServiceRepository,
            IRepository<Location> locationRepository,
            IRepository<Order> orderRepository,
            IRepository<Quote> quoteRepository
            )
        {
            _projectRepository = projectRepository;
            _projectServiceRepository = projectServiceRepository;
            _locationRepository = locationRepository;
            _orderRepository = orderRepository;
            _quoteRepository = quoteRepository;
        }

        [AbpAuthorize(AppPermissions.Pages_Projects)]
        public async Task<PagedResultDto<ProjectDto>> GetProjects(GetProjectsInput input)
        {
            var query = _projectRepository.GetAll()
                .WhereIf(!input.Name.IsNullOrEmpty(),
                    x => x.Name.Contains(input.Name))
                .WhereIf(input.Status >= 0,
                    x => x.Status == input.Status)
                .WhereIf(input.StartDateStart.HasValue,
                    x => x.StartDate >= input.StartDateStart)
                .WhereIf(input.StartDateEnd.HasValue,
                    x => x.StartDate <= input.StartDateEnd)
                .WhereIf(input.EndDateStart.HasValue,
                    x => x.EndDate >= input.EndDateStart)
                .WhereIf(input.EndDateEnd.HasValue,
                    x => x.EndDate <= input.EndDateEnd);

            var totalCount = await query.CountAsync();

            var items = await query
                .Select(x => new ProjectDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    Status = x.Status,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    Location = x.Location
                })
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<ProjectDto>(
                totalCount,
                items);
        }

        public async Task<PagedResultDto<SelectListDto>> GetProjectsSelectList(GetSelectListInput input)
        {
            var query = _projectRepository.GetAll()
                .Select(x => new SelectListDto
                {
                    Id = x.Id.ToString(),
                    Name = x.Name
                });

            return await query.GetSelectListResult(input);
        }

        public async Task<PagedResultDto<SelectListDto>> GetActiveOrPendingProjectsSelectList(GetSelectListInput input)
        {
            var query = _projectRepository.GetAll()
                .Where(x => x.Status != QuoteStatus.Inactive)
                .Select(x => new SelectListDto
                {
                    Id = x.Id.ToString(),
                    Name = x.Name
                });

            return await query.GetSelectListResult(input);
        }

        [AbpAuthorize(AppPermissions.Pages_Projects)]
        public async Task<ProjectEditDto> GetProjectForEdit(NullableIdNameDto input)
        {
            ProjectEditDto projectEditDto;

            if (input.Id.HasValue)
            {
                projectEditDto = await _projectRepository.GetAll()
                    .Select(project => new ProjectEditDto
                    {
                        Id = project.Id,
                        Name = project.Name,
                        Description = project.Description,
                        Location = project.Location,
                        StartDate = project.StartDate,
                        EndDate = project.EndDate,
                        Status = project.Status,
                        PONumber = project.PONumber,
                        Directions = project.Directions,
                        Notes = project.Notes,
                        ChargeTo = project.ChargeTo
                    })
                    .SingleAsync(x => x.Id == input.Id.Value);
            }
            else
            {
                projectEditDto = new ProjectEditDto
                {
                    Name = input.Name,
                };
            }

            return projectEditDto;
        }

        [AbpAuthorize(AppPermissions.Pages_Projects)]
        public async Task<ProjectEditDto> EditProject(ProjectEditDto model)
        {
            var project = model.Id.HasValue ? await _projectRepository.GetAsync(model.Id.Value) : new Project();

            if (project.Status != model.Status && model.Status == QuoteStatus.Inactive)
            {
                // Whenever the status is changed to inactive, the end date should default to today regardless of whether this is a new project
                model.EndDate = await GetToday();
            }

            project.Name = model.Name;
            project.Description = model.Description;
            //project.Location = model.Location;
            project.StartDate = model.StartDate;
            project.EndDate = model.EndDate;
            project.Status = model.Status;
            //project.PONumber = model.PONumber;
            //project.Directions = model.Directions;
            //project.Notes = model.Notes;
            //if (Session.OfficeCopyChargeTo)
            //{
            //    project.ChargeTo = model.ChargeTo;
            //}

            model.Id = await _projectRepository.InsertOrUpdateAndGetIdAsync(project);
            return model;
        }

        public async Task<bool> CanDeleteProject(EntityDto input)
        {
            var project = await _projectRepository.GetAll()
                .Where(x => x.Id == input.Id)
                .Select(x => new
                {
                    HasQuotes = x.Quotes.Any()
                }).SingleAsync();

            if (project.HasQuotes)
            {
                return false;
            }

            var hasOrders = await _orderRepository.GetAll().Where(x => x.ProjectId == input.Id).AnyAsync();
            if (hasOrders)
            {
                return false;
            }

            return true;
        }

        [AbpAuthorize(AppPermissions.Pages_Projects)]
        public async Task DeleteProject(EntityDto input)
        {
            var canDelete = await CanDeleteProject(input);
            if (!canDelete)
            {
                throw new UserFriendlyException("You can't delete selected row because it has data associated with it.");
            }
            await _projectServiceRepository.DeleteAsync(x => x.ProjectId == input.Id);
            await _projectRepository.DeleteAsync(input.Id);
        }

        [AbpAuthorize(AppPermissions.Pages_Projects)]
        public async Task InactivateProject(EntityDto input)
        {
            var today = await GetToday();
            var project = await _projectRepository.GetAsync(input.Id);
            project.EndDate = today;
            project.Status = QuoteStatus.Inactive;

            var quotes = await _quoteRepository.GetAll().Where(x => x.ProjectId == input.Id).ToListAsync();

            foreach (var quote in quotes)
            {
                quote.InactivationDate = today;
                quote.Status = QuoteStatus.Inactive;
            }
        }

        //*********************//

        [AbpAuthorize(AppPermissions.Pages_Projects)]
        public async Task<PagedResultDto<ProjectServiceDto>> GetProjectServices(GetProjectServicesInput input)
        {
            var query = _projectServiceRepository.GetAll();

            var totalCount = await query.CountAsync();

            var items = await query
                .Where(x => x.ProjectId == input.ProjectId)
                .WhereIf(input.LoadAtId.HasValue || input.ForceDuplicateFilters,
                         x => x.LoadAtId == input.LoadAtId)
                .WhereIf(input.DeliverToId.HasValue || input.ForceDuplicateFilters,
                         x => x.DeliverToId == input.DeliverToId)
                .WhereIf(input.ServiceId.HasValue,
                         x => x.ServiceId == input.ServiceId)
                .WhereIf(input.MaterialUomId.HasValue,
                         x => x.MaterialUomId == input.MaterialUomId)
                .WhereIf(input.FreightUomId.HasValue,
                         x => x.FreightUomId == input.FreightUomId)
                .WhereIf(input.Designation.HasValue,
                         x => x.Designation == input.Designation)
                .Select(x => new ProjectServiceDto
                {
                    Id = x.Id,
                    LoadAtNamePlain = x.LoadAt.Name + x.LoadAt.StreetAddress + x.LoadAt.City + x.LoadAt.State, //for sorting
                    LoadAt = x.LoadAt == null ? null : new LocationNameDto
                    {
                        Name = x.LoadAt.Name,
                        StreetAddress = x.LoadAt.StreetAddress,
                        City = x.LoadAt.City,
                        State = x.LoadAt.State
                    },
                    DeliverToNamePlain = x.DeliverTo.Name + x.DeliverTo.StreetAddress + x.DeliverTo.City + x.DeliverTo.State, //for sorting
                    DeliverTo = x.DeliverTo == null ? null : new LocationNameDto
                    {
                        Name = x.DeliverTo.Name,
                        StreetAddress = x.DeliverTo.StreetAddress,
                        City = x.DeliverTo.City,
                        State = x.DeliverTo.State
                    },
                    ServiceName = x.Service.Service1,
                    MaterialUomName = x.MaterialUom.Name,
                    FreightUomName = x.FreightUom.Name,
                    Designation = x.Designation,
                    PricePerUnit = x.PricePerUnit,
                    FreightRate = x.FreightRate,
                    LeaseHaulerRate = x.LeaseHaulerRate,
                    MaterialQuantity = x.MaterialQuantity,
                    FreightQuantity = x.FreightQuantity
                })
                .OrderBy(input.Sorting)
                //.PageBy(input)
                .ToListAsync();

            return new PagedResultDto<ProjectServiceDto>(
                totalCount,
                items);
        }

        [AbpAuthorize(AppPermissions.Pages_Projects)]
        public async Task<ProjectServiceEditDto> GetProjectServiceForEdit(GetProjectServiceForEditInput input)
        {
            ProjectServiceEditDto projectServiceEditDto;

            if (input.Id.HasValue)
            {
                projectServiceEditDto = await _projectServiceRepository.GetAll()
                    .Select(x => new ProjectServiceEditDto
                    {
                        Id = x.Id,
                        ProjectId = x.ProjectId,
                        LoadAtId = x.LoadAtId,
                        LoadAtNamePlain = x.LoadAt.Name + x.LoadAt.StreetAddress + x.LoadAt.City + x.LoadAt.State, //for sorting
                        LoadAt = x.LoadAt == null ? null : new LocationNameDto
                        {
                            Name = x.LoadAt.Name,
                            StreetAddress = x.LoadAt.StreetAddress,
                            City = x.LoadAt.City,
                            State = x.LoadAt.State
                        },
                        DeliverToId = x.DeliverToId,
                        DeliverToNamePlain = x.DeliverTo.Name + x.DeliverTo.StreetAddress + x.DeliverTo.City + x.DeliverTo.State, //for sorting
                        DeliverTo = x.DeliverTo == null ? null : new LocationNameDto
                        {
                            Name = x.DeliverTo.Name,
                            StreetAddress = x.DeliverTo.StreetAddress,
                            City = x.DeliverTo.City,
                            State = x.DeliverTo.State
                        },
                        ServiceId = x.ServiceId,
                        ServiceName = x.Service.Service1,
                        MaterialUomId = x.MaterialUomId,
                        MaterialUomName = x.MaterialUom.Name,
                        FreightUomId = x.FreightUomId,
                        FreightUomName = x.FreightUom.Name,
                        Designation = x.Designation,
                        PricePerUnit = x.PricePerUnit,
                        FreightRate = x.FreightRate,
                        LeaseHaulerRate = x.LeaseHaulerRate,
                        MaterialQuantity = x.MaterialQuantity,
                        FreightQuantity = x.FreightQuantity,
                        Note = x.Note
                    })
                    .SingleAsync(x => x.Id == input.Id.Value);
            }
            else if (input.ProjectId.HasValue)
            {
                projectServiceEditDto = new ProjectServiceEditDto
                {
                    ProjectId = input.ProjectId.Value
                };
            }
            else
            {
                throw new ArgumentNullException(nameof(input.ProjectId));
            }

            return projectServiceEditDto;
        }

        [AbpAuthorize(AppPermissions.Pages_Projects)]
        public async Task EditProjectService(ProjectServiceEditDto model)
        {
            await _projectServiceRepository.InsertOrUpdateAndGetIdAsync(new ProjectService
            {
                Id = model.Id ?? 0,
                ProjectId = model.ProjectId,
                LoadAtId = model.LoadAtId,
                DeliverToId = model.DeliverToId,
                ServiceId = model.ServiceId,
                MaterialUomId = model.MaterialUomId,
                FreightUomId = model.FreightUomId,
                Designation = model.Designation,
                PricePerUnit = model.PricePerUnit,
                FreightRate = model.FreightRate,
                LeaseHaulerRate = model.LeaseHaulerRate,
                MaterialQuantity = model.MaterialQuantity,
                FreightQuantity = model.FreightQuantity,
                Note = model.Note,
                TenantId = Session.TenantId ?? 0
            });
        }

        [AbpAuthorize(AppPermissions.Pages_Projects)]
        public async Task DeleteProjectService(EntityDto input)
        {
            await _projectServiceRepository.DeleteAsync(input.Id);
        }
    }
}
