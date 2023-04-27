﻿using System;
using System.Collections.Generic;

namespace DispatcherWeb.Quotes.Dto
{
    public class QuoteDto
    {
        public int Id { get; set; }
        public string QuoteName { get; set; }
        public string ProjectName { get; set; }
        public string Description { get; set; }
        public string CustomerName { get; set; }
        public string ContactName { get; set; }
        public string SalesPersonName { get; set; }
        public List<EmailDeliveryStatus> EmailDeliveryStatuses { get; set; }
        public EmailDeliveryStatus? CalculatedEmailDeliveryStatus => EmailDeliveryStatuses.GetLowestStatus();
        public DateTime? QuoteDate { get; set; }
        public QuoteStatus Status { get; set; }
    }
}
