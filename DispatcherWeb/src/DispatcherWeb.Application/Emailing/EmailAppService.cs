﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.Mail;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using DispatcherWeb.Emailing.Dto;
using Abp.Linq.Extensions;
using DispatcherWeb.Authorization;
using DispatcherWeb.Customers;
using DispatcherWeb.Notifications;
using DispatcherWeb.Orders;
using DispatcherWeb.Quotes;
using Microsoft.EntityFrameworkCore;
using DispatcherWeb.Invoices;

namespace DispatcherWeb.Emailing
{
    [AbpAuthorize]
    public class EmailAppService : DispatcherWebAppServiceBase, IEmailAppService
    {
        private readonly IRepository<TrackableEmail, Guid> _emailRepository;
        private readonly IRepository<TrackableEmailEvent> _eventRepository;
        private readonly IRepository<TrackableEmailReceiver> _receiverRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<Quote> _quoteRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<Invoice> _invoiceRepository;
        private readonly IAppNotifier _appNotifier;

        public EmailAppService(
            IRepository<TrackableEmail, Guid> emailRepository,
            IRepository<TrackableEmailEvent> eventRepository,
            IRepository<TrackableEmailReceiver> receiverRepository,
            IRepository<Customer> customerRepository,
            IRepository<Quote> quoteRepository,
            IRepository<Order> orderRepository,
            IRepository<Invoice> invoiceRepository,
            IAppNotifier appNotifier
            )
        {
            _emailRepository = emailRepository;
            _eventRepository = eventRepository;
            _receiverRepository = receiverRepository;
            _customerRepository = customerRepository;
            _quoteRepository = quoteRepository;
            _orderRepository = orderRepository;
            _invoiceRepository = invoiceRepository;
            _appNotifier = appNotifier;
        }

        [AbpAllowAnonymous]
        public async Task TrackEvents(List<TrackEventInput> inputList)
        {
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant, AbpDataFilters.MustHaveTenant))
            {
                var emailIds = inputList.Select(x => x.TrackableEmailId).ToList();
                var emails = await _emailRepository.GetAll()
                    .Include(x => x.Receivers)
                    .Include(x => x.QuoteEmails)
                    .Include(x => x.OrderEmails)
                    .Include(x => x.InvoiceEmails)
                    .Include(x => x.CreatorUser)
                    .Where(x => emailIds.Contains(x.Id))
                    .ToListAsync();

                foreach (var input in inputList
                    .OrderBy(x => x.TrackableEmailId)
                    .ThenBy(x => x.Email)
                    .ThenBy(x => x.Timestamp)
                    .ThenBy(x => x.DeliveryStatus.GetOrder())
                    .ToList())
                {
                    var email = emails.FirstOrDefault(x => x.Id == input.TrackableEmailId && x.TenantId == input.TrackableEmailTenantId);
                    var receiver = email?.Receivers.FirstOrDefault(x => !string.IsNullOrEmpty(x.Email) && x.Email.Equals(input.Email, StringComparison.InvariantCultureIgnoreCase));

                    if (receiver != null)
                    {
                        if (receiver.DeliveryStatus == EmailDeliveryStatus.Opened)
                        {
                            continue;
                        }

                        receiver.DeliveryStatus = input.DeliveryStatus;

                        if (input.DeliveryStatus.IsFailed())
                        {
                            if (email.CreatorUser != null)
                            {
                                if (email.QuoteEmails.Any())
                                {
                                    var quoteId = email.QuoteEmails.First().QuoteId;
                                    var quote = await _quoteRepository.GetAsync(quoteId);
                                    var user = email.CreatorUser.ToUserIdentifier();
                                    await _appNotifier.QuoteEmailDeliveryFailed(user, quote, receiver);
                                }

                                if (email.OrderEmails.Any())
                                {
                                    var orderId = email.OrderEmails.First().OrderId;
                                    var order = await _orderRepository.GetAsync(orderId);
                                    var user = email.CreatorUser.ToUserIdentifier();
                                    await _appNotifier.OrderEmailDeliveryFailed(user, order, receiver);
                                }

                                if (email.InvoiceEmails.Any())
                                {
                                    var invoiceId = email.InvoiceEmails.First().InvoiceId;
                                    var user = email.CreatorUser.ToUserIdentifier();
                                    await _appNotifier.InvoiceEmailDeliveryFailed(user, invoiceId, receiver);
                                }
                            }
                        }
                    }

                    var trackEvent = new TrackableEmailEvent
                    {
                        Email = input.Email,
                        EmailDeliveryStatus = input.DeliveryStatus,
                        Event = input.Event,
                        SendGridEventId = input.SendGridEventId,
                        FailReason = input.Reason,
                        SendGridEventTimestamp = input.Timestamp,
                        TrackableEmailId = email?.Id,
                        TrackableEmailReceiverId = receiver?.Id
                    };
                    trackEvent.TruncateFieldsIfNeeded();
                    await _eventRepository.InsertAsync(trackEvent);
                }

                foreach (var email in emails)
                {
                    CalculateEmailDeliveryStatus(email);
                }
            }
        }

        private void CalculateEmailDeliveryStatus(TrackableEmail email)
        {
            email.CalculatedDeliveryStatus = email.Receivers
                                                 .Where(x => !x.IsSender)
                                                 .Select(x => x.DeliveryStatus)
                                                 .GetLowestStatus() ?? EmailDeliveryStatus.NotProcessed;
        }

        [AbpAllowAnonymous]
        public async Task TrackEmailOpen(TrackEmailOpenInput input)
        {
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant, AbpDataFilters.MustHaveTenant))
            {
                var receiver = await _receiverRepository.GetAll()
                    .Include(x => x.TrackableEmail.Receivers)
                    .Include(x => x.TrackableEmail.InvoiceEmails)
                    .Where(x => x.TrackableEmailId == input.Id && x.Email == input.Email)
                    .FirstOrDefaultAsync();
                if (receiver == null || receiver.DeliveryStatus == EmailDeliveryStatus.Opened)
                {
                    return;
                }

                receiver.DeliveryStatus = EmailDeliveryStatus.Opened;

                //to keep track of the date when it was opened
                await _eventRepository.InsertAsync(new TrackableEmailEvent
                {
                    TrackableEmailId = input.Id,
                    TrackableEmailReceiverId = receiver.Id,
                    Email = input.Email,
                    EmailDeliveryStatus = EmailDeliveryStatus.Opened
                });

                if (!receiver.IsSender && receiver.TrackableEmail.InvoiceEmails.Any())
                {
                    var invoiceId = receiver.TrackableEmail.InvoiceEmails.First().InvoiceId;
                    var invoice = await _invoiceRepository.GetAsync(invoiceId);
                    if (invoice.Status == InvoiceStatus.Viewed)
                    {
                        return;
                    }
                    invoice.Status = InvoiceStatus.Viewed;
                }

                CalculateEmailDeliveryStatus(receiver.TrackableEmail);
            }
        }

        [AbpAllowAnonymous]
        [RemoteService(false)]
        public async Task<Guid> AddTrackableEmailAsync(MailMessage mail)
        {
            var trackableEmail = CreateTrackableEmail(mail);
            var id = await _emailRepository.InsertAndGetIdAsync(trackableEmail);
            var receivers = mail.GetTrackableEmailReceivers(id).SetTenantId(Session.TenantId);
            receivers.ForEach(async x => await _receiverRepository.InsertAsync(x)); 
            return id;
        }

        [AbpAllowAnonymous]
        [RemoteService(false)]
        public Guid AddTrackableEmail(MailMessage mail)
        {
            var trackableEmail = CreateTrackableEmail(mail);
            var id = _emailRepository.InsertAndGetId(trackableEmail);
            var receivers = mail.GetTrackableEmailReceivers(id).SetTenantId(Session.TenantId);
            receivers.ForEach(x => _receiverRepository.Insert(x));
            return id;
        }

        private TrackableEmail CreateTrackableEmail(MailMessage mail)
        {
            var email = new TrackableEmail
            {
                Subject = mail.Subject,
                TenantId = Session.TenantId
            };

            email.TruncateFieldsIfNeeded();

            return email;
        }

        public async Task<GetEmailHistoryInput> GetEmailHistoryInput(GetEmailHistoryInput input)
        {
            if (input.QuoteId.HasValue)
            {
                var quote = await _quoteRepository.GetAll()
                    .Where(x => x.Id == input.QuoteId)
                    .Select(x => new
                    {
                        QuoteName = x.Name,
                        CustomerName = x.Customer.Name,
                        x.CustomerId
                    })
                    .FirstOrDefaultAsync();

                input.CustomerId = quote.CustomerId;
                input.CustomerName = quote.CustomerName;
                input.QuoteName = quote.QuoteName;
            }
            else if(input.CustomerId.HasValue)
            {
                input.CustomerName = await _customerRepository.GetAll()
                    .Where(x => x.Id == input.CustomerId)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync();
            }

            return input;
        }

        [AbpAuthorize(AppPermissions.Pages_Quotes_View)]
        public async Task<PagedResultDto<EmailHistoryDto>> GetEmailHistory(GetEmailHistoryInput input)
        {
            var query = _receiverRepository.GetAll()
                .Include(x => x.TrackableEmail)
                .Include(x => x.Events)
                .Where(x => !x.IsSender)
                .WhereIf(input.CustomerId.HasValue, x => x.TrackableEmail.QuoteEmails.Any(q => q.Quote.CustomerId == input.CustomerId)
                                                        || x.TrackableEmail.OrderEmails.Any(o => o.Order.CustomerId == input.CustomerId))
                .WhereIf(input.QuoteId.HasValue, x => x.TrackableEmail.QuoteEmails.Any(q => q.QuoteId == input.QuoteId)
                                                        || x.TrackableEmail.OrderEmails.Any(o => o.Order.QuoteId == input.QuoteId))
                .WhereIf(input.OrderId.HasValue, x => x.TrackableEmail.OrderEmails.Any(o => o.OrderId == input.OrderId));

            var totalCount = await query.CountAsync();
            
            var items = await query
                .Select(x => new EmailHistoryDto
                {
                    EmailId = x.TrackableEmail.Id,
                    EmailSubject = x.TrackableEmail.Subject,
                    ReceiverId = x.Id,
                    ReceiverEmail = x.Email,
                    ReceiverKind = x.ReceiverKind,
                    EmailCreationTime = x.TrackableEmail.CreationTime,
                    EmailCreatorUserName = x.TrackableEmail.CreatorUser.Name + " " + x.TrackableEmail.CreatorUser.Surname,
                    ReceiverDeliveryStatus = x.DeliveryStatus,
                    Events = x.Events.Select(e => new EmailHistoryEventDto
                    {
                        Id = e.Id,
                        CreationTime = e.CreationTime,
                        EmailDeliveryStatus = e.EmailDeliveryStatus
                    }).ToList()
                })
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<EmailHistoryDto>(
                totalCount,
                items);
        }
    }
}
