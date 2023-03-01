using System;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Drivers.Dto
{
    public class GetNotAssignedDriversSelectListInput : GetSelectListInput
    {
        public DateTime Date { get; set; }
        public Shift? Shift { get; set; }
        public int? OfficeId { get; set; }
    }
}
