using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.LeaseHaulers.Dto
{
    public class SendMessageInput
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        public int[] ContactIds { get; set; }
        public LeaseHaulerMessageType MessageType { get; set; }

    }
}
