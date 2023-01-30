using DispatcherWeb.Offices.Dto;
using System.Collections.Generic;

namespace DispatcherWeb.Web.Areas.App.Models.Layout
{
    public class OfficeTruckStylesViewModel
    {
        public IReadOnlyList<OfficeDto> Offices { get; set; }
    }
}
