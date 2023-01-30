using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.DriverApp.OrderLineTrucks.Dto
{
    public class OrderLineTruckDto
    {
        public int Id { get; set; }

        [StringLength(EntityStringFieldLengths.OrderLineTruck.DriverNote)]
        public string DriverNote { get; set; }
    }
}
