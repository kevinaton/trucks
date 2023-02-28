using System.Collections.Generic;
using DispatcherWeb.Dto;
using DispatcherWeb.Tickets.Dto;

namespace DispatcherWeb.Tickets.Exporting
{
    public interface ITicketListCsvExporter
    {
        FileDto ExportToFile(List<TicketListViewDto> ticketDtos, string fileName);
    }
}
