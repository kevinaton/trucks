using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Net.Mail;
using Abp.Runtime.Session;
using Abp.Timing;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Infrastructure.Messages;
using DispatcherWeb.LeaseHaulerRequests.Dto;
using DispatcherWeb.LeaseHaulers;
using DispatcherWeb.Notifications;
using DispatcherWeb.Url;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.LeaseHaulerRequests
{
    public class LeaseHaulerRequestSendAppService : DispatcherWebAppServiceBase, ILeaseHaulerRequestSendAppService
    {
        private readonly IRepository<LeaseHaulerRequest> _leaseHaulerRequestRepository;
        private readonly IRepository<LeaseHaulerContact> _leaseHaulerContactRepository;
        private readonly IRepository<LeaseHauler> _leaseHaulerRepository;
        private readonly IWebUrlService _webUrlService;
        private readonly ISmsMessageSender _smsMessageSender;
        private readonly IEmailMessageSender _emailMessageSender;
        private readonly IAppNotifier _appNotifier;

        public LeaseHaulerRequestSendAppService(
            IRepository<LeaseHaulerRequest> leaseHaulerRequestRepository,
            IRepository<LeaseHaulerContact> leaseHaulerContactRepository,
            IRepository<LeaseHauler> leaseHaulerRepository,
            IWebUrlService webUrlService,
            ISmsMessageSender smsMessageSender,
            IEmailMessageSender emailMessageSender,
            IAppNotifier appNotifier
        )
        {
            _leaseHaulerRequestRepository = leaseHaulerRequestRepository;
            _leaseHaulerContactRepository = leaseHaulerContactRepository;
            _leaseHaulerRepository = leaseHaulerRepository;
            _webUrlService = webUrlService;
            _smsMessageSender = smsMessageSender;
            _emailMessageSender = emailMessageSender;
            _appNotifier = appNotifier;
        }

        public async Task<bool> SendRequests(SendRequestsInput input)
        {
            bool success = true;
            var contacts = await GetContacts(input.LeaseHaulerIds);
            foreach (int leaseHaulerId in input.LeaseHaulerIds)
            {
                var currentLeaseHaulerContacts = contacts.Where(x => x.LeaseHaulerId == leaseHaulerId).ToList();
                if (currentLeaseHaulerContacts.Count == 0)
                {
                    await NotifyLeaseHaulerHasNoDispatcherContactsError(leaseHaulerId);
                    success = false;
                    continue;
                }

                Guid guid = Guid.NewGuid();
                string message = $"{GetAvailableTrucksUrl(guid)}\n{input.Message}";

                var sendSuccess = await SendMessageToContacts(currentLeaseHaulerContacts, message);
                if (sendSuccess.successSome)
                {
                    await CreateLeaseHaulerRequest(new SendLeaseHaulerRequestDto(input, leaseHaulerId, guid));
                }

                success = success && sendSuccess.successAll;
            }

            return success;
        }

        private async Task<List<SendLeaseHaulerContact>> GetContacts(int[] leaseHaulerIds) =>
            await _leaseHaulerContactRepository.GetAll()
                .Where(lhc => leaseHaulerIds.Contains(lhc.LeaseHaulerId) && lhc.IsDispatcher)
                .Select(lhc => new SendLeaseHaulerContact
                {
                    LeaseHaulerId = lhc.LeaseHaulerId,
                    Id = lhc.Id,
                    Name = lhc.Name,
                    Email = lhc.Email,
                    CellPhoneNumber = lhc.CellPhoneNumber,
                    NotifyPreferredFormat = lhc.NotifyPreferredFormat
                })
                .ToListAsync();

        private string GetAvailableTrucksUrl(Guid guid)
        {
            string siteUrl = _webUrlService.GetSiteRootAddress();
            return $"{siteUrl}app/leasehaulerrequests/availabletrucks/{guid.ToShortGuid()}";
        }


        private async Task<(bool successAll, bool successSome)> SendMessageToContacts(List<SendLeaseHaulerContact> contacts, string message)
        {
            (bool successAll, bool successSome) success = (true, false);
            foreach (var contact in contacts)
            {
                if (contact.NotifyPreferredFormat == OrderNotifyPreferredFormat.Email || contact.NotifyPreferredFormat == OrderNotifyPreferredFormat.Both)
                {
                    UpdateSuccessTuple(await _emailMessageSender.SendEmailMessageAndNotifyErrors(await GetDefaultFromEmailAddress(), contact.Email, "Request Lease Haulers", message, contact.Name));
                }
                if (contact.NotifyPreferredFormat == OrderNotifyPreferredFormat.Sms || contact.NotifyPreferredFormat == OrderNotifyPreferredFormat.Both)
                {
                    UpdateSuccessTuple(await _smsMessageSender.SendSmsMessageAndNotifyErrors(contact.CellPhoneNumber, message, contact.Name));
                }
            }

            return success;

            // Local functions
            async Task<string> GetDefaultFromEmailAddress() => await SettingManager.GetSettingValueAsync(EmailSettingNames.DefaultFromAddress);

            void UpdateSuccessTuple(bool operationSuccess)
            {
                if (operationSuccess)
                {
                    success.successSome = true;
                }
                else
                {
                    success.successAll = false;
                }
            }
        }

        private async Task CreateLeaseHaulerRequest(SendLeaseHaulerRequestDto model)
        {
            var leaseHaulerRequest = new LeaseHaulerRequest
            {
                Guid = model.Guid,
                Date = model.Date,
                Shift = model.Shift,
                OfficeId = model.OfficeId,
                LeaseHaulerId = model.LeaseHaulerId,
                Sent = model.Sent,
            };
            await _leaseHaulerRequestRepository.InsertAsync(leaseHaulerRequest);
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        private async Task NotifyLeaseHaulerHasNoDispatcherContactsError(int leaseHaulerId)
        {
            var leaseHaulerName = await _leaseHaulerRepository.GetAll()
                .Where(lh => lh.Id == leaseHaulerId)
                .Select(lh => lh.Name)
                .FirstAsync();
            await _appNotifier.SendMessageAsync(
                AbpSession.ToUserIdentifier(),
                $"There is no dispatcher contact for lease hauler {leaseHaulerName}.",
                Abp.Notifications.NotificationSeverity.Error
            );

        }

        private class SendLeaseHaulerRequestDto
        {
            public SendLeaseHaulerRequestDto(SendRequestsInput input, int leaseHaulerId, Guid guid)
            {
                Guid = guid;
                Date = input.Date;
                Shift = input.Shift;
                OfficeId = input.OfficeId;
                LeaseHaulerId = leaseHaulerId;
                Sent = Clock.Now;
            }

            public Guid Guid { get; }
            public DateTime Date { get; }
            public Shift? Shift { get; }
            public int OfficeId { get; }

            public int LeaseHaulerId { get; }

            public DateTime Sent { get; }
        }

        private class SendLeaseHaulerContact
        {
            public int LeaseHaulerId { get; set; }
            public int Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string CellPhoneNumber { get; set; }
            public OrderNotifyPreferredFormat NotifyPreferredFormat { get; set; }
        }

    }
}
