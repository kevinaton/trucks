using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using DispatcherWeb.Dispatching.Dto;
using DispatcherWeb.Dto;
using MigraDoc.DocumentObjectModel;

namespace DispatcherWeb.Dispatching
{
    public interface IDispatchingAppService
    {
        Task SendDispatchMessage(SendDispatchMessageInput input);
        Task<DriverInfoBaseDto> GetDriverInfo(GetDriverInfoInput input);
        Task UpdateDispatchTicket(DispatchTicketDto dispatchTicket);
        Task UpdateDispatchTicket2(UpdateDispatchTicketInput input);
        Task<CompleteDispatchResult> CompleteDispatch(CompleteDispatchDto completeDispatch);
        Task<List<SelectListDto>> GetUnitOfMeasureSelectList(Guid dispatchGuid);
        Task CancelDispatch(CancelDispatchDto cancelDispatch);
        Task CancelOrEndAllDispatches(CancelOrEndAllDispatchesInput input);

        Task<ViewDispatchDto> ViewDispatch(int dispatchId);
        Task<SetDispatchTimeOnJobDto> GetDispatchTimeOnJob(int dispatchId);
        Task<List<TruckDispatchListItemDto>> TruckDispatchList(TruckDispatchListInput input);
        Task<bool> CanAddDispatchBasedOnTime(CanAddDispatchBasedOnTimeInput input);
        Task<SendDispatchMessageDto> CreateSendDispatchMessageDto(int orderLineId, bool firstDispatchForDay = false);
        Task<bool> OtherDispatchesExist(OtherDispatchesExistInput input);
        Task DuplicateDispatch(DuplicateDispatchInput input);
        Task SendOrdersToDrivers(SendOrdersToDriversInput input);
        Task CancelDispatches(CancelDispatchesInput input);
        Task DeleteUnacknowledgedDispatches(DeleteUnacknowledgedDispatchesInput input);
        Task<bool> GetDispatchTruckStatus(int dispatchId);
        Task AcknowledgeDispatch(AcknowledgeDispatchInput input);
        Task<GetNextDispatchTodayResult> GetNextDispatchToday(Guid acknowledgedDispatchGuid);
        Task<PagedResultDto<DispatchListDto>> GetDispatchPagedList(GetDispatchPagedListInput input);
        Task<int?> GetTenantIdFromDispatch(Guid dispatchGuid);
        Task<Document> GetDriverActivityDetailReport(GetDriverActivityDetailReportInput input);
        Task SendCompletedDispatchNotificationIfNeeded(int dispatchId);
    }
}