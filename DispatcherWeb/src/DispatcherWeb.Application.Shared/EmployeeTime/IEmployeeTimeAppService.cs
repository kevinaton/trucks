﻿using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.EmployeeTime.Dto;
using System.Threading.Tasks;

namespace DispatcherWeb.EmployeeTime
{
    public interface IEmployeeTimeAppService : IApplicationService
    {
        Task<EmployeeTimeIndexDto> GetEmployeeTimeIndexModel();
        Task<EmployeeTimeEditDto> GetEmployeeTimeForEdit(NullableIdDto nullableIdDto);
        Task AddBulkTime(AddBulkTimeDto addBulkTimeDto);
    }
}
