using Abp.AutoMapper;
using DispatcherWeb.MultiTenancy;
using DispatcherWeb.MultiTenancy.Dto;
using DispatcherWeb.Web.Areas.App.Models.Common;

namespace DispatcherWeb.Web.Areas.App.Models.Tenants
{
    [AutoMapFrom(typeof(GetTenantFeaturesEditOutput))]
    public class TenantFeaturesEditViewModel : GetTenantFeaturesEditOutput, IFeatureEditViewModel
    {
        public Tenant Tenant { get; set; }

        public TenantFeaturesEditViewModel(Tenant tenant, GetTenantFeaturesEditOutput output)
        {
            Tenant = tenant;
            this.FeatureValues = output.FeatureValues;
            this.Features = output.Features;
            //output.MapTo(this);
        }
    }
}