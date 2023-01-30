using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure.SecureFiles.Dto;

namespace DispatcherWeb.SecureFiles
{
    public interface ISecureFilesAppService : IApplicationService
	{
		//Task<SecureFileDefinitionDto> PostGetNewLink(SecureFileDefinitionDto dto);
		//Task<PagedFilteredResultDto<SecureFileDefinitionDto>> GetSecureFiles(GetSecureFilesInput input);
		//Task DeleteSecureFiles(EntityDto<Guid> input);
		//Task<bool> IsSecureFileDefinitionExist(Guid id);
		Task<Guid> GetSecureFileDefinitionId();
	}
}
