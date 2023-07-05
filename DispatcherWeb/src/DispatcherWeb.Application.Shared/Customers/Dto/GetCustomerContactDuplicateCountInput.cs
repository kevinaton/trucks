namespace DispatcherWeb.Customers.Dto
{
    public class GetCustomerContactDuplicateCountInput
    {
        public int CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? ExceptId { get; set; }
    }
}
