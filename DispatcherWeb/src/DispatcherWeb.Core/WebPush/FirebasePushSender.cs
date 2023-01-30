using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Dependency;
using DispatcherWeb.Configuration;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace DispatcherWeb.WebPush
{
    public class FirebasePushSender : IFirebasePushSender, ISingletonDependency
    {
        private readonly IConfigurationRoot _appConfiguration;
        private FirebaseApp _firebaseApp;

        public FirebasePushSender(IWebHostEnvironment env)
        {
            _appConfiguration = env.GetAppConfiguration();
        }

        private void ConfigureIfNeeded()
        {
            if (_firebaseApp != null)
            {
                return;
            }

            //var credential = await GoogleCredential.GetApplicationDefaultAsync();
            var jsonCredential = _appConfiguration["WebPush:FirebaseCredential"];
            var credential = GoogleCredential.FromJson(jsonCredential);

            _firebaseApp = FirebaseApp.Create(new AppOptions
            {
                Credential = credential,
            });

            
        }

        public async Task SendAsync(FcmRegistrationTokenDto registrationToken, string jsonPayload) //jsonPayload should be a serialized instance of DriverApplication.FcmPushMessage
        {
            ConfigureIfNeeded();

            var messaging = FirebaseMessaging.DefaultInstance;

            await messaging.SendAsync(new Message()
            {
                Token = registrationToken.Token,
                Data = new Dictionary<string, string>()
                {
                    { "jsonPayload", jsonPayload },
                }
            });
        }
    }
}
