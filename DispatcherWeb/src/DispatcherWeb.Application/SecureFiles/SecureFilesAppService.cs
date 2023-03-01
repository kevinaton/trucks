using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Domain.Repositories;
using DispatcherWeb.Authorization;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Infrastructure.SecureFiles.Dto;

namespace DispatcherWeb.SecureFiles
{
    public class SecureFilesAppService : DispatcherWebAppServiceBase, ISecureFilesAppService
    {
        private readonly IRepository<SecureFileDefinition, Guid> _secureFileDefinitionRepository;
        private readonly ISecureFileBlobService _secureFileBlobService;

        public SecureFilesAppService(
            IRepository<SecureFileDefinition, Guid> secureFileDefinitionRepository,
            ISecureFileBlobService secureFileBlobService
        )
        {
            _secureFileDefinitionRepository = secureFileDefinitionRepository;
            _secureFileBlobService = secureFileBlobService;
        }

        [AbpAuthorize(AppPermissions.Pages_Imports)]
        public async Task<SecureFileDefinitionDto> PostGetNewLink(SecureFileDefinitionDto dto)
        {
            SecureFileDefinition entity = await _secureFileDefinitionRepository
                .FirstOrDefaultAsync(x => x.Client == dto.Client && x.Description == dto.Description);
            if (entity == null)
            {
                entity = new SecureFileDefinition()
                {
                    Id = new Guid(),
                    Client = dto.Client,
                    Description = dto.Description
                };
                await _secureFileDefinitionRepository.InsertOrUpdateAsync(entity);
            }
            dto.Id = entity.Id;
            return dto;
        }

        //[AbpAuthorize(AppPermissions.Pages_Imports)]
        //public async Task<PagedFilteredResultDto<SecureFileDefinitionDto>> GetSecureFiles(GetSecureFilesInput input)
        //{
        //	var query = _secureFileDefinitionRepository.GetAll();
        //	var totalCount = await query.CountAsync();
        //	var filteredCount = totalCount;
        //	var items = await query
        //		.OrderBy(x => x.Client)
        //		.Select(x => new SecureFileDefinitionDto()
        //		{
        //			Id = x.Id,
        //			Client = x.Client,
        //			Description = x.Description
        //		})
        //		.ToListAsync()
        //		;

        //	foreach (var item in items)
        //	{
        //		item.FileNames = _secureFileBlobService.GetSecureFileNames(item.Id).Select(x => x.Substring(37)).ToArray();
        //	}

        //	return new PagedFilteredResultDto<SecureFileDefinitionDto>(
        //		totalCount,
        //		filteredCount,
        //		items
        //	);

        //}

        //[AbpAuthorize(AppPermissions.Pages_SecureFiles)]
        //public async Task DeleteSecureFiles(EntityDto<Guid> input)
        //{
        //	var secureFileDefinition = await _secureFileDefinitionRepository.FirstOrDefaultAsync(x => x.Id == input.Id);
        //	if (secureFileDefinition != null)
        //	{
        //		await _secureFileBlobService.DeleteSecureFilesAsync(input.Id);
        //		await _secureFileDefinitionRepository.DeleteAsync(x => x.Id == input.Id);
        //	}
        //}

        //[AbpAllowAnonymous]
        //public async Task<bool> IsSecureFileDefinitionExist(Guid id)
        //{
        //	return await _secureFileDefinitionRepository.GetAll().AnyAsync(x => x.Id == id);
        //}

        [AbpAuthorize(AppPermissions.Pages_Imports)]
        public async Task<Guid> GetSecureFileDefinitionId()
        {
            Debug.Assert(AbpSession.TenantId.HasValue);
            var tenant = await TenantManager.GetByIdAsync(AbpSession.TenantId.Value);
            var id = (await _secureFileDefinitionRepository.FirstOrDefaultAsync(x => x.Client == tenant.TenancyName))?.Id;
            if (id == null)
            {
                SecureFileDefinitionDto dto = new SecureFileDefinitionDto()
                {
                    Client = tenant.TenancyName,
                    Description = "Created by uploading prospects",
                };
                await PostGetNewLink(dto);
                id = dto.Id;
            }
            return id.Value;
        }
    }
}
