namespace DispatcherWeb.Quotes.Dto
{
    public class SetQuoteStatusInput
    {
        public int Id { get; set; }

        public ProjectStatus Status { get; set; }
    }
}
