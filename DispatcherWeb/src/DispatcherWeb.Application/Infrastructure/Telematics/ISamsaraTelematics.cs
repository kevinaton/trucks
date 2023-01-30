using DispatcherWeb.Infrastructure.Telematics.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Infrastructure.Telematics
{
    public interface ISamsaraTelematics
    {
        Task<List<TruckCurrentData>> GetCurrentDataForAllTrucksAsync();
    }
}
