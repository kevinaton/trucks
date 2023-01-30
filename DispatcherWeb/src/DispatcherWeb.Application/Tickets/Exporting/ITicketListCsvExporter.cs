using DispatcherWeb.Dto;
using DispatcherWeb.Tickets.Dto;
using System.Collections.Generic;

namespace DispatcherWeb.Tickets.Exporting
{
    public interface ITicketListCsvExporter
    {
        FileDto ExportToFile(List<TicketListViewDto> ticketDtos, string fileName);
    }
}
