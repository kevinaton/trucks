using DispatcherWeb.Infrastructure.Telematics.Dto;
using DispatcherWeb.Infrastructure.Telematics.Dto.IntelliShift;
using System.Collections.Generic;
using System.Threading.Tasks;

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
