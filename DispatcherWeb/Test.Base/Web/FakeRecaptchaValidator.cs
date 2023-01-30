﻿using System.Threading.Tasks;
using DispatcherWeb.Security.Recaptcha;

namespace DispatcherWeb.Test.Base.Web
{
    public class FakeRecaptchaValidator : IRecaptchaValidator
    {
        public Task ValidateAsync(string captchaResponse)
        {
            return Task.CompletedTask;
        }
    }
}
