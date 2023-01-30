using System.Threading.Tasks;

namespace DispatcherWeb.Trucks
{
    public interface ITruckTelematicsAppService
    {
        Task UpdateMileageForAllTenantsAsync();
        Task<bool> IsGpsIntegrationConfigured();
        Task<bool> IsDtdTrackerConfigured();
        Task<bool> IsIntelliShiftConfigured();
        Task<(int trucksUpdated, int trucksIgnored)> UpdateMileageForTenantAsync(int tenantId, long userId);
        Task SyncWialonDeviceTypesInternal();
        void UploadTruckPositionsToWialon();
        Task SyncWithIntelliShift();
    }
}