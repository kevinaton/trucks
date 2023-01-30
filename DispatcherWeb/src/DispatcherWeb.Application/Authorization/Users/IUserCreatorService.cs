using Abp.Application.Services;
using DispatcherWeb.Authorization.Users.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Authorization.Users
{
    //this interface is not in DriverWeb.Application.Shared layer because User type can't be accessed from that layer
    public interface IUserCreatorService : IApplicationService
    {
        Task<User> CreateUser(CreateOrUpdateUserInput input);
    }
}
