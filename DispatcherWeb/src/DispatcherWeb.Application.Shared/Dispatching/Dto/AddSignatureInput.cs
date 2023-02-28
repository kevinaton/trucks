using System;

namespace DispatcherWeb.Dispatching.Dto
{
    public class AddSignatureInput
    {
        public Guid Guid { get; set; }
        public string Signature { get; set; }
        public string SignatureName { get; set; }
        public Guid? DeferredSignatureId { get; set; }
    }
}
