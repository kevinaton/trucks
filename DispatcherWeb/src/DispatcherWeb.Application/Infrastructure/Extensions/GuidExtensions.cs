using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Infrastructure.Extensions
{
    public static class GuidExtensions
    {
        public static string ToShortGuid(this Guid guid)
        {
            return new ShortGuid(guid);
        }
    }
}
