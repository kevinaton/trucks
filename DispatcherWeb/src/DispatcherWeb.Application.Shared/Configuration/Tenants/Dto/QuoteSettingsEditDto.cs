namespace DispatcherWeb.Configuration.Tenants.Dto
{
    public class QuoteSettingsEditDto
    {
        public bool PromptForDisplayingQuarryInfoOnQuotes { get; set; }

        public string QuoteEmailBodyTemplate { get; set; }

        public string QuoteEmailSubjectTemplate { get; set; }

        public string QuoteChangedNotificationEmailBodyTemplate { get; set; }

        public string QuoteChangedNotificationEmailSubjectTemplate { get; set; }
    }
}