using DispatcherWeb.Dto;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DispatcherWeb.Web.Areas.App.Models.Settings
{
    public class QuickbooksOnlineViewModel
    {
        public List<SelectListItem> IncomeAccountList { get; set; }
        public string DefaultIncomeAccountId { get; set; }
        public string DefaultIncomeAccountName { get; set; }
    }
}
