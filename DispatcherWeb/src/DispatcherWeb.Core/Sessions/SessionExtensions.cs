using Abp.UI;
using DispatcherWeb.Runtime.Session;

namespace DispatcherWeb.Sessions
{
    public static class SessionExtensions
    {
        public static int GetOfficeIdOrThrow(this AspNetZeroAbpSession session)
        {
            if (session.OfficeId.HasValue)
            {
                return session.OfficeId.Value;
            }
            throw new UserFriendlyException("You must have an assigned Office in User Details to use that function");
        }

        public static int GetCustomerIdOrThrow(this AspNetZeroAbpSession session)
        {
            if (session.CustomerId.HasValue)
            {
                return session.CustomerId.Value;
            }
            throw new UserFriendlyException("You must be a customer contact to use that function.");
        }
    }
}
