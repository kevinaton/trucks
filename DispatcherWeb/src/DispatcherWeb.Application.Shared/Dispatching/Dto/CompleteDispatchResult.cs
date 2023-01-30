using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Dispatching.Dto
{
    public class CompleteDispatchResult
    {
		public bool NextDispatch { get; set; }
        public string NextDispatchShortGuid { get; set; }
		public bool IsCanceled { get; set; }
		public bool NotFound { get; set; }
        public bool IsCompleted { get; set; }
    }
}
