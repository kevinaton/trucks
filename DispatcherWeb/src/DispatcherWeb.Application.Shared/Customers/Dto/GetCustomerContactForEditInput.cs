using Abp.Application.Services.Dto;

namespace DispatcherWeb.Customers.Dto
{
    public class GetCustomerContactForEditInput : NullableIdDto
    {
        public int? CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
