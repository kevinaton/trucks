using System.Collections.Generic;
using Abp.Localization;
using DispatcherWeb.Install.Dto;

namespace DispatcherWeb.Web.Models.Install
{
    public class InstallViewModel
    {
        public List<ApplicationLanguage> Languages { get; set; }

        public AppSettingsJsonDto AppSettingsJson { get; set; }
    }
}
