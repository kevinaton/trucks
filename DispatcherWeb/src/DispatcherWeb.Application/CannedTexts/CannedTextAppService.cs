using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using DispatcherWeb.Authorization;
using DispatcherWeb.CannedTexts.Dto;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Offices;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.CannedTexts
{
	[AbpAuthorize]
	public class CannedTextAppService : DispatcherWebAppServiceBase, ICannedTextAppService
	{
		private readonly IRepository<CannedText> _cannedTextRepository;
		private readonly ISingleOfficeAppService _singleOfficeService;

		public CannedTextAppService(
			IRepository<CannedText> cannedTextRepository,
			ISingleOfficeAppService singleOfficeService
		)
		{
			_cannedTextRepository = cannedTextRepository;
			_singleOfficeService = singleOfficeService;
		}

		[AbpAuthorize(AppPermissions.Pages_CannedText)]
		public async Task<PagedResultDto<CannedTextDto>> GetCannedTexts(GetCannedTextsInput input)
		{
			var query = _cannedTextRepository.GetAll()
				.WhereIf(input.OfficeId.HasValue,
						 x => x.OfficeId == input.OfficeId);

			var totalCount = await query.CountAsync();

			var items = await query
				.Select(x => new CannedTextDto
				{
					Id = x.Id,
					OfficeName = x.Office.Name,
					Name = x.Name,
					Text = x.Text.Substring(0, 51)
				})
				.OrderBy(input.Sorting)
				.PageBy(input)
				.ToListAsync();

			return new PagedResultDto<CannedTextDto>(
				totalCount,
				items);
		}

		public async Task<PagedResultDto<SelectListDto>> GetCannedTextsSelectList(GetSelectListInput input)
		{
			var query = _cannedTextRepository.GetAll()
				.Where(x => x.OfficeId == OfficeId)
				.Select(x => new SelectListDto
				{
					Id = x.Id.ToString(),
					Name = x.Name
				});

			return await query.GetSelectListResult(input);
		}

		public async Task<CannedTextEditDto> GetCannedTextForEdit(NullableIdDto input)
		{
			CannedTextEditDto cannedTextEditDto;

			if (input.Id.HasValue)
			{
				cannedTextEditDto = await _cannedTextRepository.GetAll()
					.Where(x => x.Id == input.Id)
					.Select(x => new CannedTextEditDto
					{
						Id = x.Id,
						Name = x.Name,
						OfficeId = x.OfficeId,
						OfficeName = x.Office.Name,
						Text = x.Text
					})
					.FirstAsync();
			}
			else
			{
				cannedTextEditDto = new CannedTextEditDto();
			}
			await _singleOfficeService.FillSingleOffice(cannedTextEditDto);

			return cannedTextEditDto;
		}

		[AbpAuthorize(AppPermissions.Pages_CannedText)]
		public async Task EditCannedText(CannedTextEditDto model)
		{
			await _cannedTextRepository.InsertOrUpdateAndGetIdAsync(new CannedText
			{
				Id = model.Id ?? 0,
				Name = model.Name,
				OfficeId = model.OfficeId,
				Text = model.Text,
				TenantId = Session.TenantId ?? 0
			});
		}

		[AbpAuthorize(AppPermissions.Pages_CannedText)]
		public async Task DeleteCannedText(EntityDto input)
		{
			await _cannedTextRepository.DeleteAsync(input.Id);
		}
	}
}
