using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Drivers.Dto
{
    public class TimeClassificationDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsProductionBased { get; set; }
        public decimal? DefaultRate { get; set; }

    }
}
