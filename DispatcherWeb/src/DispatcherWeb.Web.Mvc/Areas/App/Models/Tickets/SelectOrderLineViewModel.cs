using Microsoft.AspNetCore.Mvc.Rendering;

namespace DispatcherWeb.Web.Areas.App.Models.Tickets
{
    public class SelectOrderLineViewModel
    {
        public int OrderLineId { get; set; }

        public SelectList OrderLineSelectList { get; set; }
    }
}
