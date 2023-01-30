namespace DispatcherWeb.Services.Dto
{
    public class ServicePriceEditDto
    {
        public int? Id { get; set; }

        public int ServiceId { get; set; }

        public int OfficeId { get; set; }

        public int? MaterialUomId { get; set; }

        public string MaterialUomName { get; set; }

        public int? FreightUomId { get; set; }

        public string FreightUomName { get; set; }

        public decimal? PricePerUnit { get; set; }

        public decimal? FreightRate { get; set; }

        public DesignationEnum Designation { get; set; }
    }
}
