using Abp.AutoMapper;
using DispatcherWeb.MultiTenancy.Dto;

namespace DispatcherWeb.Web.Models.TenantRegistration
{
    [AutoMapFrom(typeof(RegisterTenantOutput))]
    public class TenantRegisterResultViewModel : RegisterTenantOutput
    {
        public string TenantLoginAddress { get; set; }
    }
}