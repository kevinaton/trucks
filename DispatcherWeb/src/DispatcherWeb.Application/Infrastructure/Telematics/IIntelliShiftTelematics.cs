using System.Collections.Generic;
using System.Threading.Tasks;
using DispatcherWeb.Infrastructure.Telematics.Dto;
using DispatcherWeb.Infrastructure.Telematics.Dto.IntelliShift;

namespace DispatcherWeb.Infrastructure.Telematics
{
    public interface IIntelliShiftTelematics
    {
        Task<TokenLoginResult> LoginToApiAsync();
        Task<List<TruckCurrentData>> GetCurrentDataForAllTrucksAsync();
        Task<List<TruckUnitDto>> GetAllUnitsAsync(TokenLoginResult tokenLoginResult = null);
        Task<bool> UpdateUnit(int remoteVehicleId, TokenLoginResult tokenLoginResult = null, params (string PropertyName, object PropertyValue)[] fieldsToUpdate);
    }
}
