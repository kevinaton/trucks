using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Imports.Dto
{
    public class ValidateTruxFileResult
    {
        public bool IsValid { get; set; }
        public List<int> DuplicateShiftAssignments { get; set; } = new List<int>();
        public int TotalRecordCount { get; internal set; }
    }
}
