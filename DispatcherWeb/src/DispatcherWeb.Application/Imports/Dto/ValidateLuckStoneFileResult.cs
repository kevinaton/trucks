using System.Collections.Generic;

namespace DispatcherWeb.Imports.Dto
{
    public class ValidateLuckStoneFileResult
    {
        public bool IsValid { get; set; }
        public List<int> DuplicateLuckStoneTicketIds { get; set; } = new List<int>();
        public int TotalRecordCount { get; internal set; }
    }
}
