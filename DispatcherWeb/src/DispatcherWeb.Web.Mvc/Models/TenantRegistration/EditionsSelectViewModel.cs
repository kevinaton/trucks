using Abp.AutoMapper;
using DispatcherWeb.MultiTenancy.Dto;

namespace DispatcherWeb.Web.Models.TenantRegistration
{
    [AutoMapFrom(typeof(EditionsSelectOutput))]
    public class EditionsSelectViewModel : EditionsSelectOutput
    {
        public EditionsSelectViewModel(EditionsSelectOutput output)
        {
            this.AllFeatures = output.AllFeatures;
            this.EditionsWithFeatures = output.EditionsWithFeatures;
            this.TenantEditionId = output.TenantEditionId;
            //output.MapTo(this);
        }
    }
}
