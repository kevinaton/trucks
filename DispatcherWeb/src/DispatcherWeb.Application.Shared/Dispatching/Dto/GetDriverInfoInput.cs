using System;

namespace DispatcherWeb.Dispatching.Dto
{
    public class GetDriverInfoInput
    {
        public Guid AcknowledgeGuid { get; set; }
        public bool EditTicket { get; set; }
        public bool DoNotAcknowledge { get; set; }
    }
}
