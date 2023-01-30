using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Dispatching.Dto
{
    public class CancelDispatchDto
    {
		public int DispatchId { get; set; }

		public bool CancelAllDispatchesForDriver { get; set; }
	}
}
