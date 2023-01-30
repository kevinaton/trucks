using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Infrastructure.Telematics.Dto.Samsara
{
    public class PaginationInfo
    {
        public string EndCursor { get; set; }
        public bool HasNextPage { get; set; }
    }
}
