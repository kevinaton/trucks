using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Infrastructure.Telematics.Dto.DtdTracker
{
    public class WialonListResult<T> : List<T>, IWialonResult
    {
        public int ErrorCode { get; set; }
        public string ErrorReason { get; set; }
    }
}
