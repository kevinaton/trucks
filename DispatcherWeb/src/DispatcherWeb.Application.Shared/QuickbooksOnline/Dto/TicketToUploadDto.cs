using System;

namespace DispatcherWeb.QuickbooksOnline.Dto
{
    public class TicketToUploadDto
    {
        public DateTime? TicketDateTimeUtc { get; set; }
        //public decimal? OrderMaterialPrice { get; set; }
        //public decimal? OrderFreightPrice { get; set; }

        public TicketToUploadDto Clone()
        {
            return new TicketToUploadDto
            {
                TicketDateTimeUtc = TicketDateTimeUtc,
                //OrderMaterialPrice = OrderMaterialPrice,
                //OrderFreightPrice = OrderFreightPrice,
            };
        }
    }
}
