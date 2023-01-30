using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Customers.Dto
{
    public class GetCustomersWithOrdersSelectListInput : GetSelectListInput
	{
		public DateTime? DateBegin { get; set; }
		public DateTime? DateEnd { get; set; }
    }
}
