﻿namespace DispatcherWeb.Infrastructure.Extensions
{
    public static class BoolExtensions
    {
        public static string ToLowerCaseString(this bool value) => value.ToString().ToLowerInvariant();
    }
}
