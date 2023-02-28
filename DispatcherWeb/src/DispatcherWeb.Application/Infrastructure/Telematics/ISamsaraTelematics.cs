using System.Collections.Generic;
using System.Threading.Tasks;
using DispatcherWeb.Infrastructure.Telematics.Dto;

namespace DispatcherWeb.Infrastructure.Telematics
{
    public interface ISamsaraTelematics
    {
        Task<List<TruckCurrentData>> GetCurrentDataForAllTrucksAsync();
    }
}
