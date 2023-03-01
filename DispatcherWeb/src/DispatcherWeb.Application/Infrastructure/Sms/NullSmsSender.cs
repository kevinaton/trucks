using System;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Castle.Core.Logging;
using DispatcherWeb.Infrastructure.Sms.Dto;
using DispatcherWeb.Sms;

namespace DispatcherWeb.Infrastructure.Sms
{
    public class NullSmsSender : ISmsSender
    {
        private readonly IRepository<SentSms> _sentSmsRepository;

        public ILogger Logger { get; set; }
        public IAbpSession AbpSession { get; set; }

        public NullSmsSender(
            IRepository<SentSms> sentSmsRepository
        )
        {
            _sentSmsRepository = sentSmsRepository;
            Logger = NullLogger.Instance;
            AbpSession = NullAbpSession.Instance;
        }

        public async Task<SmsSendResult> SendAsync(string text, string toPhoneNumber, bool trackStatus = false, bool useTenantPhoneNumberOnly = false, bool insertEntity = true)
        {
            Logger.Info($"NullSmsSender: Sending a message with text: '{text}' to number: '{toPhoneNumber}'");
#if DEBUG
            var smsLogger = Logger.CreateChildLogger("SmsLogger");
            smsLogger.Info($"SMS message:\n Body: {text}\n To: {toPhoneNumber}\n");
#endif

            var sid = Guid.NewGuid().ToString();
            var sentSms = new SentSms
            {
                FromSmsNumber = "",
                ToSmsNumber = toPhoneNumber,
                TenantId = AbpSession.TenantId,
                Sid = sid,
                Status = SmsStatus.Unknown
            };
            bool sentSmsEntityIsInserted = false;
            if (trackStatus)
            {
                sentSmsEntityIsInserted = true;
                await _sentSmsRepository.InsertAndGetIdAsync(sentSms);
            }
            else if (insertEntity)
            {
                sentSmsEntityIsInserted = true;
                await _sentSmsRepository.InsertAsync(sentSms);
            }

            return new SmsSendResult
            {
                Sid = sid,
                Status = SmsStatus.Unknown,
                ErrorCode = null,
                ErrorMessage = null,
                SentSmsEntity = sentSms,
                SentSmsEntityIsInserted = sentSmsEntityIsInserted
            };
        }
    }
}
