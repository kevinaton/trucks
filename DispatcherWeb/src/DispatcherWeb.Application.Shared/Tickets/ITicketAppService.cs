using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using DispatcherWeb.Tickets.Dto;

namespace DispatcherWeb.Tickets
{
    public interface ITicketAppService
    {
        Task<EditOrderTicketOutput> EditOrderTicket(OrderTicketEditDto model);
        Task<TicketEditDto> GetTicketEditDto(NullableIdDto input);
        Task<TicketEditDto> EditTicket(TicketEditDto model);
        Task<TicketPhotoDto> GetTicketPhoto(int ticketId);

        Task<PagedResultDto<TicketListItemViewModel>> LoadTicketsByOrderLineId(int orderLineId);
        Task<string> CheckTruckIsOutofServiceOrInactive(TicketEditDto model);

        Task<DeleteTicketOutput> DeleteTicket(EntityDto input);
        Task MarkAsBilledTicket(EntityDto input);
        Task<IList<TicketOrderLineDto>> LookForExistingOrderLines(LookForExistingOrderLinesInput input);
        Task<TicketPhotoDto> GetTicketPhotosForInvoice(int invoiceId);
        Task<GetDriverForTicketTruckResult> GetDriverForTicketTruck(GetDriverForTicketTruckInput input);
        Task<GetTruckForTicketDriverResult> GetTruckForTicketDriver(GetTruckForTicketDriverInput input);
    }
}