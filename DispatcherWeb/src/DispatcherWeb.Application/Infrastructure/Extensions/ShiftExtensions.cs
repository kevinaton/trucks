using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Infrastructure.Extensions
{
    public static class ShiftExtensions
    {
        public static Shift?[] ToNullableArrayWithNullElementIfEmpty(this Shift[] shifts) =>
			shifts?.ToNullIfEmpty()?.Select(x => (Shift?)x).ToArray() ?? new Shift?[] { null };

		private static Shift[] ToNullIfEmpty(this Shift[] shifts) => shifts.Length != 0 ? shifts : null;
	}
}
