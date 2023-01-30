using DispatcherWeb.Infrastructure.Telematics.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Infrastructure.Telematics
{
    public interface ITelematics
    {
        Task<IEnumerable<TruckCurrentData>> GetCurrentDataForAllTrucksAsync();
        Task<string[]> GetDeviceIdsByTruckCodesAsync(string[] truckCodes);
        Task CheckCredentialsAsync();
    }
}
