using Abp.Dependency;
using DispatcherWeb.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using WebPushLib = WebPush;

namespace DispatcherWeb.WebPush
{
    public class WebPushSender : IWebPushSender, ISingletonDependency //ITransientDependency 
    {
        private readonly IConfigurationRoot _appConfiguration;
        private readonly WebPushLib.VapidDetails _vapidDetails;

        public WebPushSender(IWebHostEnvironment env)
        {
            _appConfiguration = env.GetAppConfiguration();
            _vapidDetails = new WebPushLib.VapidDetails(
                _appConfiguration["WebPush:ContactLink"],
                _appConfiguration["WebPush:ServerPublicKey"],
                _appConfiguration["WebPush:ServerPrivateKey"]);
        }

        public async Task SendAsync(PushSubscriptionDto pushSubscriptionDto, DriverApplication.PwaPushMessage payload)
        {
            await SendAsync(pushSubscriptionDto, JsonConvert.SerializeObject(payload));
        }

        public async Task SendAsync(PushSubscriptionDto pushSubscriptionDto, object payload)
        {
            await SendAsync(pushSubscriptionDto, JsonConvert.SerializeObject(payload));
        }

        public async Task SendAsync(PushSubscriptionDto pushSubscriptionDto, string payload)
        {
            var webPushClient = new WebPushLib.WebPushClient();
            try
            {
                var pushSubscription = new WebPushLib.PushSubscription(pushSubscriptionDto.Endpoint, pushSubscriptionDto.Keys.P256dh, pushSubscriptionDto.Keys.Auth);
                await webPushClient.SendNotificationAsync(pushSubscription, payload, _vapidDetails);
            }
            catch (WebPushLib.WebPushException exception)
            {
                Console.WriteLine("Http STATUS code" + exception.StatusCode);
                throw;
            }
        }
    }
}
