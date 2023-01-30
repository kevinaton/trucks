using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Extensions;
using Castle.Core.Logging;
using DispatcherWeb.Configuration;
using Microsoft.Extensions.Configuration;

namespace DispatcherWeb.Encryption
{
    public class EncryptionService : ITransientDependency, IEncryptionService
    {
        private readonly IConfigurationRoot _appConfiguration;

        public ILogger Logger { get; set; }

        public EncryptionService(
            IAppConfigurationAccessor configurationAccessor
        )
        {
            _appConfiguration = configurationAccessor.Configuration;
            Logger = NullLogger.Instance;
        }

        public string Encrypt(string plainText) => EncryptionHelper.Encrypt(plainText, GetKey());
        public string Decrypt(string ciphertext) => EncryptionHelper.Decrypt(ciphertext, GetKey());
        public string EncryptIfNotEmpty(string plainText) => string.IsNullOrEmpty(plainText) ? plainText : Encrypt(plainText);
        public string DecryptIfNotEmpty(string cipherText) => string.IsNullOrEmpty(cipherText) ? cipherText : Decrypt(cipherText);

        private string GetKey()
        {
            string key = _appConfiguration[DispatcherWebConsts.InternalNotesEncryptionKey];
            if (key.IsNullOrEmpty())
            {
                throw new ApplicationException($"The {DispatcherWebConsts.InternalNotesEncryptionKey} is empty!");
            }
            return key;
        }
    }

}
