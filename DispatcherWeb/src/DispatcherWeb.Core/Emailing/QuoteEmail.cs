﻿using System;
using Abp.Domain.Entities;
using DispatcherWeb.Quotes;

namespace DispatcherWeb.Emailing
{
    public class QuoteEmail : Entity
    {
        public int QuoteId { get; set; }
        public Guid EmailId { get; set; }
        public virtual Quote Quote { get; set; }
        public virtual TrackableEmail Email { get; set; }
    }
}
