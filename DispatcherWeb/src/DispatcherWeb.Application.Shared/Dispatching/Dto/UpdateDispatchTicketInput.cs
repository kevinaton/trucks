namespace DispatcherWeb.Dispatching.Dto
{
    public class UpdateDispatchTicketInput
    {
        public DispatchTicketDto DispatchTicket { get; set; }
        public DriverApplicationActionInfo Info { get; set; }
    }
}
