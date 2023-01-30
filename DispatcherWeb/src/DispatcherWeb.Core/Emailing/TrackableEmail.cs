using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Authorization.Users;

namespace DispatcherWeb.Emailing
{
    public class TrackableEmail : AuditedEntity<Guid>, IMayHaveTenant
    {
        private static class FieldLengthes
        {
            public const int Subject = 200;
        }

        public TrackableEmail()
        {
            Events = new HashSet<TrackableEmailEvent>();
            Receivers = new HashSet<TrackableEmailReceiver>();
            QuoteEmails = new HashSet<QuoteEmail>();
            OrderEmails = new HashSet<OrderEmail>();
            InvoiceEmails = new HashSet<InvoiceEmail>();
        }

        public int? TenantId { get; set; }

        [StringLength(FieldLengthes.Subject)]
        public string Subject { get; set; }

        public EmailDeliveryStatus CalculatedDeliveryStatus { get; set; }

        public virtual ICollection<TrackableEmailEvent> Events { get; set; }

        public virtual ICollection<TrackableEmailReceiver> Receivers { get; set; }

        public virtual ICollection<QuoteEmail> QuoteEmails { get; set; }

        public virtual ICollection<OrderEmail> OrderEmails { get; set; }

        public virtual ICollection<InvoiceEmail> InvoiceEmails { get; set; }

        public virtual User CreatorUser { get; set; }

        public void TruncateFieldsIfNeeded()
        {
            if (Subject?.Length > FieldLengthes.Subject)
            {
                Subject = Subject.Substring(0, FieldLengthes.Subject);
            }
        }

    }
}
