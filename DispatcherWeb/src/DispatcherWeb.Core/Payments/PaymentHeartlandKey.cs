using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DispatcherWeb.Payments
{
    [Table("PaymentHeartlandKey")]
    public class PaymentHeartlandKey : FullAuditedEntity
    {
        [Required]
        [StringLength(50)] //30
        public string PublicKey { get; set; }
    }
}
