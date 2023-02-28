using System;

namespace DispatcherWeb.Dispatching.Dto
{
    public class UploadDeferredBinaryObjectInput
    {
        public Guid DeferredId { get; set; }
        public DeferredBinaryObjectDestination Destination { get; set; }
        public string BytesString { get; set; }
    }
}
