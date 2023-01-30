using System;
using System.Threading.Tasks;
using DispatcherWeb.Tickets.Dto;

namespace DispatcherWeb.FuelSurchargeCalculations
{
    public interface IFuelSurchargeCalculator
    {
        Task RecalculateOrderLinesWithTickets(DateTime date);
        Task RecalculateOrderLinesWithTickets(int orderLineId);
        Task RecalculateOrderLinesWithTicketsForOrder(int orderId);
        Task RecalculateTicket(int ticketId);
    }
}
