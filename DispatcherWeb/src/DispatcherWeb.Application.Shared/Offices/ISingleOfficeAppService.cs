using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using DispatcherWeb.Offices.Dto;

namespace DispatcherWeb.Offices
{
	public interface ISingleOfficeAppService
	{
		Task<bool> IsSingleOffice();
		Task<KeyValuePair<int, string>> GetSingleOfficeIdName();
		Task Reset();
		Task FillSingleOffice(IOfficeIdNameDto officeIdNameDto);
	}
}