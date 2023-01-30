using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Rest.Api.V2010.Account.Call;

namespace DispatcherWeb.Infrastructure.Extensions
{
    public static class MessageResourceExtensions
    {
        public static SmsStatus ToSmsStatus(this MessageResource.StatusEnum statusEnum)
        {
            if (statusEnum == MessageResource.StatusEnum.Accepted) return SmsStatus.Accepted;
            if (statusEnum == MessageResource.StatusEnum.Delivered) return SmsStatus.Delivered;
            if (statusEnum == MessageResource.StatusEnum.Failed) return SmsStatus.Failed;
            if (statusEnum == MessageResource.StatusEnum.Queued) return SmsStatus.Queued;
            if (statusEnum == MessageResource.StatusEnum.Received) return SmsStatus.Received;
            if (statusEnum == MessageResource.StatusEnum.Receiving) return SmsStatus.Receiving;
            if (statusEnum == MessageResource.StatusEnum.Sending) return SmsStatus.Sending;
            if (statusEnum == MessageResource.StatusEnum.Sent) return SmsStatus.Sent;
            if (statusEnum == MessageResource.StatusEnum.Undelivered) return SmsStatus.Undelivered;
            throw new ArgumentException();
        }
    }
}
