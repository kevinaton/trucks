using System.Threading.Tasks;
using Abp.Domain.Uow;
using DispatcherWeb.Offices;
using DispatcherWeb.Web.Areas.App.Models.Layout;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.App.Views.Shared.Components.OfficeTruckStyles
{
    public class OfficeTruckStylesViewComponent : ViewComponent
    {
        private readonly IOfficeAppService _officeAppService;

        public OfficeTruckStylesViewComponent(IOfficeAppService officeAppService)
        {
            _officeAppService = officeAppService;
        }

        [UnitOfWork]
        public virtual async Task<IViewComponentResult> InvokeAsync()
        {
            var offices = await _officeAppService.GetAllOffices();

            var model = new OfficeTruckStylesViewModel
            {
                Offices = offices.Items
            };

            return View(model);
        }
    }
}