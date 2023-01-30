using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Dispatching.Dto
{
    public class UploadDeferredBinaryObjectInput
    {
        public Guid DeferredId { get; set; }
        public DeferredBinaryObjectDestination Destination { get; set; }
        public string BytesString { get; set; }
    }
}
