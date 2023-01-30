using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Infrastructure.Telematics.Dto.DtdTracker
{
    public class ItemsSearchResult<TItem> : WialonResult
    {
        public List<TItem> Items { get; set; }
    }
}
