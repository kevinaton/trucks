using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.TimeClassifications.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.TimeClassifications
{
    public interface ITimeClassificationAppService : IApplicationService
    {
        Task<TimeClassificationEditDto> GetTimeClassificationForEdit(NullableIdDto nullableIdDto);
    }
}
