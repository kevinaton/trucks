using System;
using System.Threading.Tasks;
using DispatcherWeb.Dto;
using DispatcherWeb.LeaseHaulerRequests.Dto;

namespace DispatcherWeb.LeaseHaulerRequests
{
    public interface ILeaseHaulerRequestEditAppService
    {
        Task<LeaseHaulerRequestEditDto> GetLeaseHaulerRequestEditDto(int? leaseHaulerRequestId, DateTime? scheduleDate);
        Task<LeaseHaulerRequestEditModel> EditLeaseHaulerRequest(LeaseHaulerRequestEditModel model);
        Task UpdateAvailable(IdValueInput<int?> input);
        Task UpdateApproved(IdValueInput<int?> input);
        Task<AvailableTrucksEditDto> GetAvailableTrucksEditDto(Guid leaseHaulerRequestShortGuid);
        Task EditAvailableTrucks(AvailableTrucksEditModel model);
    }
}