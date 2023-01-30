﻿using System;
using System.Collections.Generic;
using System.Globalization;

namespace DispatcherWeb.Infrastructure.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime value)
        {
            if (value.DayOfWeek == DayOfWeek.Sunday)
            {
                return value.Date;
            }

            return value.AddDays(-(int)value.DayOfWeek).Date;
        }

        public static DateTime EndOfWeek(this DateTime value)
        {
            return value.StartOfWeek().AddDays(6);
        }

        public static DateTime StartOfMonth(this DateTime value)
        {
            return value.AddDays(-(value.Day - 1));
        }

        public static DateTime EndOfMonth(this DateTime value)
        {
            return value.StartOfMonth().AddMonths(1).AddDays(-1);
        }

        public static DateTime EndOfDay(this DateTime value) => value.Date.AddDays(1).AddMilliseconds(-1);

        public static int GetWeekOfYear(this DateTime date)
        {
            var cal = CultureInfo.CurrentCulture.Calendar;
            return cal.GetWeekOfYear(date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
        }
    }
}
