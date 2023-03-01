using System;

namespace DispatcherWeb.Infrastructure.Extensions
{
    public static class EnumExtensions
    {
        public static string ToIntString(this Enum value) => Convert.ToInt32(value).ToString();
    }
}
