using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.WebPush
{
    public class FcmRegistrationTokenDto
    {
        public long UserId { get; set; }
        
        public string Token { get; set; }

        public MobilePlatform MobilePlatform { get; set; }
        public int Id { get; set; }
    }
}
