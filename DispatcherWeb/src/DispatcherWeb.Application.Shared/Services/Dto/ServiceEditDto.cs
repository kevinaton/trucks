namespace DispatcherWeb.Services.Dto
{
    public class ServiceEditDto
    {
        public int? Id { get; set; }

        public string Service1 { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; set; }

        public ServiceType? Type { get; set; }

        public bool IsTaxable { get; set; }

        public string IncomeAccount { get; set; }
    }
}
