﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Orders.TaxDetails
{
    public class TicketDto
    {
        public int? OfficeId { get; set; }
        public decimal MaterialQuantity { get; set;}
        public decimal FreightQuantity { get; set; }
    }
}
