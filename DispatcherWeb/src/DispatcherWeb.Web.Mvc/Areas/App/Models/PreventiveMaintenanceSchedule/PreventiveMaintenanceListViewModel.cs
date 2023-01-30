using DispatcherWeb.PreventiveMaintenanceSchedule.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DispatcherWeb.Web.Areas.App.Models.PreventiveMaintenanceSchedule
{
    public class PreventiveMaintenanceListViewModel
    {
        public bool DisableLoadState { get; set; }

        public GetPreventiveMaintenancePagedListInput Filter { get; set; }
    }
}
