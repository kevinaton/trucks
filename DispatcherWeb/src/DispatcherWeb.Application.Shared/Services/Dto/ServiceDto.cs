namespace DispatcherWeb.Services.Dto
{
    public class ServiceDto
    {
        public int Id { get; set; }

        public string Service1 { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; set; }

        public bool DisallowDataMerge { get; set; }
    }
}
