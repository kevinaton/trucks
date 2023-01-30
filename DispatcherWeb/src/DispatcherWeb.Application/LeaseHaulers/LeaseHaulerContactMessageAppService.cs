using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Abp.Extensions;
using Abp.Domain.Repositories;
using Abp.Net.Mail;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Dispatching.Dto;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Infrastructure.Messages;
using DispatcherWeb.Infrastructure.Sms;
using DispatcherWeb.Infrastructure.Sms.Dto;
using DispatcherWeb.LeaseHaulers.Dto;
using DispatcherWeb.LeaseHaulers.Exporting;
using DispatcherWeb.Notifications;
using Microsoft.EntityFrameworkCore;
using Twilio.Exceptions;

namespace DispatcherWeb.LeaseHaulers
{
    public class LeaseHaulerContactMessageAppService : DispatcherWebAppServiceBase
    {
        private readonly IRepository<LeaseHaulerContact> _leaseHaulerContactRepository;
        private readonly ISmsMessageSender _smsMessageSender;
        private readonly IEmailMessageSender _emailMessageSender;

        public LeaseHaulerContactMessageAppService(
            IRepository<LeaseHaulerContact> leaseHaulerContactRepository,
            ISmsMessageSender smsMessageSender,
            IEmailMessageSender emailMessageSender
        )
        {
            _leaseHaulerContactRepository = leaseHaulerContactRepository;
            _smsMessageSender = smsMessageSender;
            _emailMessageSender = emailMessageSender;
        }

        public async Task<bool> SendMessage(SendMessageInput input)
        {
            if (input.Body.IsNullOrWhiteSpace())
            {
                throw new UserFriendlyException("Body text is required to send a message.");
            }

            var leaseHaulerContactQuery = _leaseHaulerContactRepository.GetAll();
            leaseHaulerContactQuery = leaseHaulerContactQuery.Where(d => input.ContactIds.Contains(d.Id));
            var contacts = await leaseHaulerContactQuery
                .Select(c => new 
                {
                    Id = c.Id,
                    PhoneNumber = c.CellPhoneNumber,
                    Email = c.Email,
                    FullName = c.Name
                })
                .ToListAsync();

            bool success = true;
            foreach (var contact in contacts)
            {
                switch (input.MessageType)
                {
                    case LeaseHaulerMessageType.Sms:
                        success = await _smsMessageSender.SendSmsMessageAndNotifyErrors(contact.PhoneNumber, input.Body, contact.FullName) 
                                  && success;
                        break;
                    case LeaseHaulerMessageType.Email:
                        success = await _emailMessageSender.SendEmailMessageAndNotifyErrors(
                                      await SettingManager.GetSettingValueAsync(EmailSettingNames.DefaultFromAddress), contact.Email, input.Subject, input.Body, contact.FullName) 
                                  && success;
                        break;
                    default:
                        throw new ArgumentException($"Wrong LeaseHaulerMessageType: {input.MessageType}");
                }
            }

            return success;
        }


    }
}
