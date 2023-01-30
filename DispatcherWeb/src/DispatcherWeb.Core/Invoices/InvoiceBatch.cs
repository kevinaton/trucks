﻿using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DispatcherWeb.Invoices
{
    [Table("InvoiceBatch")]
    public class InvoiceBatch : FullAuditedEntity, IMustHaveTenant
    {
        public InvoiceBatch()
        {
            Invoices = new HashSet<Invoice>();
        }

        public int TenantId { get; set; }

        public virtual ICollection<Invoice> Invoices { get; set; }
    }
}
