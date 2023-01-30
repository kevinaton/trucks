using DispatcherWeb.Payments.Dto;
using System.ComponentModel.DataAnnotations;

namespace DispatcherWeb.OrderPayments.Dto
{
    public class AuthorizeOrderChargeDto : AuthorizeChargeDto
    {
        public int OrderId { get; set; }
        public bool SaveCreditCardForFutureUse { get; set; }
    }
}
