﻿using System;
using Abp.Domain.Entities;
using DispatcherWeb.Orders;

namespace DispatcherWeb.Emailing
{
    public class OrderEmail : Entity
    {
        public int OrderId { get; set; }
        public Guid EmailId { get; set; }
        public virtual Order Order { get; set; }
        public virtual TrackableEmail Email { get; set; }
    }
}
