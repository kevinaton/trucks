using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Configuration;
using DispatcherWeb.Configuration;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DispatcherWeb.Designations
{
    [AbpAuthorize]
    public class DesignationAppService : DispatcherWebAppServiceBase, IDesignationAppService
    {
        public DesignationAppService() 
        {
        }

        public async Task<IEnumerable<SelectListItem>> GetDesignationSelectListItemsAsync(DesignationEnum? selectedDesignation = null)
        {
            var designationList = new List<SelectListItem>();
            designationList.Add(GetSelectListItemFromEnum(DesignationEnum.FreightOnly));
            designationList.Add(GetSelectListItemFromEnum(DesignationEnum.MaterialOnly));
            
            if (selectedDesignation == DesignationEnum.CounterSale 
                || await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.AllowCounterSales))
            {
                designationList.Add(GetSelectListItemFromEnum(DesignationEnum.CounterSale));
            }
            
            designationList.Add(GetSelectListItemFromEnum(DesignationEnum.FreightAndMaterial));
            designationList.Add(GetSelectListItemFromEnum(DesignationEnum.BackhaulFreightOnly));
            designationList.Add(GetSelectListItemFromEnum(DesignationEnum.BackhaulFreightAndMaterial));
            designationList.Add(GetSelectListItemFromEnum(DesignationEnum.Disposal));
            designationList.Add(GetSelectListItemFromEnum(DesignationEnum.BackHaulFreightAndDisposal));
            designationList.Add(GetSelectListItemFromEnum(DesignationEnum.StraightHaulFreightAndDisposal));

            return designationList;
        }

        public static SelectListItem GetSelectListItemFromEnum(DesignationEnum designation)
        {
            return new SelectListItem
            {
                Value = ((int)designation).ToString(),
                Text = designation.GetDisplayName()
            };
        }
    }
}
