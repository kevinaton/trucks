using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.DriverApp.Loads.Dto
{
    public class LoadDto
    {
        public int Id { get; set; }
        public int DispatchId { get; set; }
        public DateTime? SourceDateTime { get; set; }
        public double? SourceLatitude { get; set; }
        public double? SourceLongitude { get; set; }
        public DateTime? DestinationDateTime { get; set; }
        public double? DestinationLatitude { get; set; }
        public double? DestinationLongitude { get; set; }
        public Guid? SignatureId { get; set; }
        public string SignatureName { get; set; }
    }
}
