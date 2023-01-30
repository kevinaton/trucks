using Abp.Domain.Entities;
using DispatcherWeb.Invoices;
using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Emailing
{
    public class InvoiceEmail : Entity
    {
        public int InvoiceId { get; set; }
        public Guid EmailId { get; set; }
        public virtual Invoice Invoice { get; set; }
        public virtual TrackableEmail Email { get; set; }
    }
}
