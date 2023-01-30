using System;

namespace DispatcherWeb.Infrastructure.Extensions
{
    public static class DecimalExtensions
    {
        public static decimal RoundTo(this decimal value, int decimals) => decimal.Round(value, decimals);
    }
}
