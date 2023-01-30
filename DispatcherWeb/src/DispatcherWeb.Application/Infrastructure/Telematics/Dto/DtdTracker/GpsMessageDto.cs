using System;
using Abp.Extensions;

namespace DispatcherWeb.Infrastructure.Telematics.Dto.DtdTracker
{
    public class GpsMessageDto
    {
        public DateTime Timestamp { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public decimal? AltitudeInMeters { get; set; }
        public int SpeedInKMPH { get; set; }

        public override string ToString()
        {
            var altitude = AltitudeInMeters.HasValue ? $"ALT:{AltitudeInMeters}" : "";
            return $"REG;{Timestamp.ToUnixTimestamp()};{Longitude};{Latitude};{SpeedInKMPH};0;{altitude};;;;;";
        }
    }
}
