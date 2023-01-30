using System.Collections.Generic;
using DispatcherWeb.Dispatching.Dto;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Dispatching.Exporting
{
	public interface IDispatchListCsvExporter
	{
		FileDto ExportToFile(List<DispatchListDto> dispatchDtos);
	}
}