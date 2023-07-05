using DispatcherWeb.Customers;

namespace DispatcherWeb.Infrastructure.Extensions
{
    public static class EntityExtensions
    {
        public static string FullName(this CustomerContact customerContact) =>
            customerContact.FullName(false);

        public static string FullName(this CustomerContact customerContact, bool lastNameFirst)
        {
            if (customerContact == null)
                return string.Empty;

            var fullName = (lastNameFirst ? $"{customerContact.LastName}, {customerContact.FirstName}".Trim() :
                        $"{customerContact.FirstName} {customerContact.LastName}".Trim());

            if (fullName.IndexOf(',') == fullName.Length - 1)
            {
                fullName = fullName[..(fullName.IndexOf(",") - 1)];
            }

            return fullName;
        }
    }
}
