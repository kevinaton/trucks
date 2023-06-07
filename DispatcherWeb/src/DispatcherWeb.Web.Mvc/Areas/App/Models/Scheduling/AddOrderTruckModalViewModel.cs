using System;
using DispatcherWeb.Scheduling.Dto;

namespace DispatcherWeb.Web.Areas.App.Models.Scheduling
{
    public class AddOrderTruckModalViewModel : AddOrderLineTruckInput
    {
        public int OfficeId { get; set; }
        public DateTime Date { get; set; }
        public Shift? Shift { get; set; }
        public bool OnlyTrailers { get; set; }
        public bool? IsPowered { get; set; }
        public string Title { get; set; }
        public int? CurrentTrailerId { get; set; }
        public int ParentTruckId { get; set; }
    }
}
