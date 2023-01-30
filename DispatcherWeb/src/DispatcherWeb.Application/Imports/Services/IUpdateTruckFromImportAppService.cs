namespace DispatcherWeb.Imports.Services
{
    public interface IUpdateTruckFromImportAppService
    {
        void UpdateMileageAndHours(
            int tenantId,
            long userId
        );
    }
}