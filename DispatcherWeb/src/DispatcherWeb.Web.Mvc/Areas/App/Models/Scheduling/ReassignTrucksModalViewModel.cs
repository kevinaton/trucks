using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DispatcherWeb.Web.Areas.App.Models.Scheduling
{
    public class ReassignTrucksModalViewModel
    {
        public int OrderLineId { get; set; }
        public IEnumerable<SelectListItem> Trucks { get; set; }

    }
}
