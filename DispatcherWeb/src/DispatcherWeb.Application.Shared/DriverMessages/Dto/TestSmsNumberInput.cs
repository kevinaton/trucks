using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.DriverMessages.Dto
{
    public class TestSmsNumberInput
    {
		public string CountryCode { get; set; }
		public string PhoneNumber { get; set; }

		public string FullPhoneNumber => $"{CountryCode}{PhoneNumber}";
	}
}
