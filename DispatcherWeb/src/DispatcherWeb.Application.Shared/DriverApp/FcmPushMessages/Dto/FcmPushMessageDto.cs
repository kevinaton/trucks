﻿using System;

namespace DispatcherWeb.DriverApp.FcmPushMessages.Dto
{
    public class FcmPushMessageDto
    {
        public string JsonPayload { get; set; }
        public DateTime? SentAtDateTime { get; set; }
        public DateTime? ReceivedAtDateTime { get; set; }
    }
}
