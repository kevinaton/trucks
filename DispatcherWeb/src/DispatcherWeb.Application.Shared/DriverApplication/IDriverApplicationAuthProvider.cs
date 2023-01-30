using DispatcherWeb.DriverApplication.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.DriverApplication
{
    public interface IDriverApplicationAuthProvider
    {
        Task<AuthDriverByDriverGuidResult> AuthDriverByDriverGuid(Guid driverGuid);
        Task<AuthDriverByDriverGuidResult> AuthDriverByDriverGuidIfNeeded(Guid? driverGuid);
        Task<int> GetDriverIdFromSessionOrGuid(Guid? driverGuid);
    }
}
