using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Infrastructure.Extensions
{
    public static class BoolExtensions
    {
        public static string ToLowerCaseString(this bool value) => value.ToString().ToLowerInvariant();
    }
}
