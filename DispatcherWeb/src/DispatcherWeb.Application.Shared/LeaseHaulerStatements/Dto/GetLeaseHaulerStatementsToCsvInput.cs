namespace DispatcherWeb.LeaseHaulerStatements.Dto
{
    public class GetLeaseHaulerStatementsToCsvInput
    {
        public int Id { get; set; }

        public bool SplitByLeaseHauler { get; set; }
    }
}
