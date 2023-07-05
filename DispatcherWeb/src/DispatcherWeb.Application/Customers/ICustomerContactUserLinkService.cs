using System.Threading.Tasks;
using Abp.Application.Services;
using DispatcherWeb.Authorization.Users;

namespace DispatcherWeb.Customers
{
    public interface ICustomerContactUserLinkService : IApplicationService
    {
        Task UpdateCustomerContact(User user);
        Task<User> UpdateUser(CustomerContact CustomerContact, bool sendEmail = true);

    }
}