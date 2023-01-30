using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.TimeOffs.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.TimeOffs
{
    public interface ITimeOffAppService : IApplicationService
    {
        Task<TimeOffEditDto> GetTimeOffForEdit(NullableIdDto nullableIdDto);
    }
}
