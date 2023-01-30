using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DispatcherWeb.Infrastructure.Extensions;
using Newtonsoft.Json;

namespace DispatcherWeb.Infrastructure.Utilities
{
    public static class Utility
    {
		public static string GetCronString(DayOfWeekBitFlag dayOfWeek, TimeSpan time)
		{
			return $"{time.Minutes} {time.Hours} * * {dayOfWeek.ToCronDayOfWeek()}";
		}

        public static string Serialize(object value)
        {
            string jsonString = JsonConvert.SerializeObject(value);
            return jsonString;
        }

        public static T Deserialize<T>(string value)
        {
            return JsonConvert.DeserializeObject<T>(value);
        }

    }
}
