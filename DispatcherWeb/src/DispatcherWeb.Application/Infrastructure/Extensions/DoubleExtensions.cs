using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Infrastructure.Extensions
{
    public static class DoubleExtensions
    {
        public static double? Round(this double? value, int digits)
        {
            if (value == null)
            {
                return null;
            }

            return Math.Round(value.Value, digits);
        }
    }
}
