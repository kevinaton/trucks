using System;
using System.Collections.Generic;
using System.Text;
using DispatcherWeb.Dto;

namespace DispatcherWeb.DriverApp.FcmPushMessages.Dto
{
    public class GetInput : PagedInputDto
    {
        public bool? Received { get; set; }
        public string Token { get; set; }
    }
}
