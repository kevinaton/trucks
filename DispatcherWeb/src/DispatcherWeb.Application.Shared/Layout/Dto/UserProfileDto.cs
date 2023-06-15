using Abp.Localization;
using System.Collections.Generic;
using DispatcherWeb.Sessions.Dto;

namespace DispatcherWeb.Layout.Dto
{
	public class UserProfileDto
	{
		public GetCurrentLoginInformationsOutput LoginInformations { get; set; }

        public IReadOnlyList<LanguageInfo> Languages { get; set; }

        public LanguageInfo CurrentLanguage { get; set; }

        public bool IsMultiTenancyEnabled { get; set; }

        public bool IsImpersonatedLogin { get; set; }

        public int SubscriptionExpireNootifyDayCount { get; set; }
    }
}

