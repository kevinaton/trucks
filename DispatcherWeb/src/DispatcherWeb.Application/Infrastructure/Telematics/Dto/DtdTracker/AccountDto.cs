﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Infrastructure.Telematics.Dto.DtdTracker
{
    public class AccountDto
    {
        [JsonProperty("nm")]
        public string Name { get; set; }
    }
}
