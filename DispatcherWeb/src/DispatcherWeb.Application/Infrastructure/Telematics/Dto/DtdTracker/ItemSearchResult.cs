using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Infrastructure.Telematics.Dto.DtdTracker
{
    public class ItemSearchResult<TItem> : WialonResult
    {
        public TItem Item { get; set; }
    }
}
