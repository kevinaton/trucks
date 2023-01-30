using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Infrastructure.Extensions
{
    public static class EnumExtensions
    {
        public static string ToIntString(this Enum value) => Convert.ToInt32(value).ToString();
    }
}
