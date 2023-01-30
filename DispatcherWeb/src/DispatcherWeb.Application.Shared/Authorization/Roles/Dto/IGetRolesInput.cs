using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Authorization.Roles.Dto
{
    public interface IGetRolesInput : ISortedResultRequest
    {
        string Permission { get; set; }
    }
}
