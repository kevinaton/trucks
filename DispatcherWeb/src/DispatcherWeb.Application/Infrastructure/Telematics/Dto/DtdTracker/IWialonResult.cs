using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Infrastructure.Telematics.Dto.DtdTracker
{
    public interface IWialonResult
    {
        int ErrorCode { get; set; }
        string ErrorReason { get; set; }
    }
}
