namespace DispatcherWeb.Services.Dto
{
    public interface IGetServiceListFilter
    {
        string Name { get; set; }
        FilterActiveStatus Status { get; set; }
    }
}
