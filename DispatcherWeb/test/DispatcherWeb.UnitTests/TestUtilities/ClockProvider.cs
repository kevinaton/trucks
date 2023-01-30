using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Timing;

namespace DispatcherWeb.UnitTests.TestUtilities
{
	public class ClockProvider : IClockProvider
	{
		private static readonly DateTime dateTimeNow = DateTime.UtcNow;
		public static DateTime DateTimeNow => dateTimeNow;

		static ClockProvider()
		{
			Clock.Provider = new ClockProvider();
		}

		public DateTime Normalize(DateTime dateTime) => dateTime;
		public DateTime Now => dateTimeNow;
		public DateTimeKind Kind => DateTimeKind.Utc;
		public bool SupportsMultipleTimezone => false;
	}
}
