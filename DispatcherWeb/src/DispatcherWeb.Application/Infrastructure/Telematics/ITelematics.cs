using System.Collections.Generic;
using System.Threading.Tasks;
using DispatcherWeb.Infrastructure.Telematics.Dto;

namespace DispatcherWeb.Infrastructure.Telematics
{
    public interface ITelematics
    {
        Task<IEnumerable<TruckCurrentData>> GetCurrentDataForAllTrucksAsync();
        Task<string[]> GetDeviceIdsByTruckCodesAsync(string[] truckCodes);
        Task CheckCredentialsAsync();
    }
}
