using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services;
using DispatcherWeb.Dispatching.Dto;
using DispatcherWeb.Dispatching.Dto.DispatchSender;

namespace DispatcherWeb.Dispatching
{
    public interface IDispatchSender
    {
        Dispatch AddDispatch(DispatchEditDto dispatchDto);
        Task<SendSmsResult[]> BatchSendSms(params SendSmsInput[] inputs);
        Task<bool> CanAddDispatchBasedOnTime(CanAddDispatchBasedOnTimeInput input);
        Task CleanUp();
        Task CreateDispatchesForDateShift(CreateDispatchesForDateShiftInput input);
        Task<string> CreateDispatchMessageFromTemplate(OrderLineDataForDispatchMessage orderLine, bool firstDispatchForDay = false);
        Task<SendDispatchMessageDto> CreateSendDispatchMessageDto(int orderLineId, bool firstDispatchForDay = false);
        Task EnsureCanCreateDispatchAsync(int orderLineId, int newTruckCount, int newDispatchCount, bool multipleLoads);
        Task<CreateDispatchesAndSendSmsToEachDriverResult> SendDispatchMessage(SendDispatchMessageInput input, bool skipSmsIfDispatchesExist = true);
        Task<SendSmsResult> SendSms(SendSmsInput input);
    }
}
