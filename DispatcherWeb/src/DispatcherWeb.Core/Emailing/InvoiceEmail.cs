﻿using System;
using Abp.Domain.Entities;
using DispatcherWeb.Invoices;

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
