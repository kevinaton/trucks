﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp;
using Abp.Notifications;
using Abp.Timing;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Infrastructure.Messages;
using DispatcherWeb.LeaseHaulerRequests;
using DispatcherWeb.LeaseHaulerRequests.Dto;
using DispatcherWeb.Notifications;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.LeaseHaulerRequests
{
    public class LeaseHaulerRequestSendAppService_SendRequests_Tests : LeaseHaulerRequestAppService_Tests_Base, IAsyncLifetime
    {
        private ILeaseHaulerRequestSendAppService _leaseHaulerRequestSendAppService;
        private IEmailMessageSender _emailMessageSender;
        private ISmsMessageSender _smsMessageSender;
        private IAppNotifier _appNotifier;

        public new async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _emailMessageSender = Substitute.For<IEmailMessageSender>();
            _smsMessageSender = Substitute.For<ISmsMessageSender>();
            _appNotifier = Substitute.For<IAppNotifier>();
            _leaseHaulerRequestSendAppService = Resolve<ILeaseHaulerRequestSendAppService>(
                new { emailMessageSender = _emailMessageSender, smsMessageSender = _smsMessageSender, appNotifier = _appNotifier });
            ((DispatcherWebAppServiceBase)_leaseHaulerRequestSendAppService).Session = CreateSession();
        }


        [Fact]
        public async Task Test_SendRequests_should_send_Email_and_Sms_and_create_LeaseHaulerRequest_entity()
        {
            // Arrange
            var leaseHauler = await CreateLeaseHauler();
            var leaseHaulerContact = await CreateLeaseHaulerContact(leaseHauler.Id);
            var model = new SendRequestsInput()
            {
                Date = Clock.Now.Date,
                Shift = Shift.Shift1,
                OfficeId = _officeId,
                LeaseHaulerIds = new []{ leaseHauler.Id },
                Message = "message",
            };
            _emailMessageSender.SendEmailMessageAndNotifyErrors(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).ReturnsForAnyArgs(true);
            _smsMessageSender.SendSmsMessageAndNotifyErrors(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).ReturnsForAnyArgs(true);

            // Act
            var result = await _leaseHaulerRequestSendAppService.SendRequests(model);

            // Assert
            result.ShouldBeTrue();
            var leaseHaulerRequests = await UsingDbContextAsync(async context => await context.LeaseHaulerRequests.ToListAsync());
            leaseHaulerRequests.Count.ShouldBe(1);
            var createdLeaseHaulerRequest = leaseHaulerRequests[0];
            createdLeaseHaulerRequest.Guid.ShouldNotBe(new Guid());
            createdLeaseHaulerRequest.Date.ShouldBe(model.Date);
            createdLeaseHaulerRequest.Shift.ShouldBe(model.Shift);
            createdLeaseHaulerRequest.OfficeId.ShouldBe(model.OfficeId);
            createdLeaseHaulerRequest.LeaseHaulerId.ShouldBe(leaseHauler.Id);
            createdLeaseHaulerRequest.Sent.ShouldNotBeNull();
            createdLeaseHaulerRequest.Available.ShouldBeNull();
            createdLeaseHaulerRequest.Approved.ShouldBeNull();

            string url = $"http://test.com/app/leasehaulerrequests/availabletrucks/{createdLeaseHaulerRequest.Guid.ToShortGuid()}";
            string message = $"{url}\n{model.Message}";
            await _emailMessageSender.Received().SendEmailMessageAndNotifyErrors(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Is<string>(message), Arg.Any<string>());
            await _smsMessageSender.Received().SendSmsMessageAndNotifyErrors(Arg.Any<string>(), Arg.Is<string>(message), Arg.Any<string>());
        }

        [Fact]
        public async Task Test_SendRequests_should_send_Email_and_Sms_to_all_contacts_and_create_LeaseHaulerRequest_entity_even_if_some_contact_have_bad_number()
        {
            // Arrange
            var leaseHauler = await CreateLeaseHauler();
            await CreateLeaseHaulerContact(leaseHauler.Id);
            await CreateLeaseHaulerContact(leaseHauler.Id);
            var model = new SendRequestsInput()
            {
                Date = Clock.Now.Date,
                Shift = Shift.Shift1,
                OfficeId = _officeId,
                LeaseHaulerIds = new []{ leaseHauler.Id },
                Message = "message",
            };
            _emailMessageSender.SendEmailMessageAndNotifyErrors(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).ReturnsForAnyArgs(false, true);
            _smsMessageSender.SendSmsMessageAndNotifyErrors(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).ReturnsForAnyArgs(false, true);

            // Act
            var result = await _leaseHaulerRequestSendAppService.SendRequests(model);

            // Assert
            result.ShouldBeFalse();
            await _emailMessageSender.ReceivedWithAnyArgs(2).SendEmailMessageAndNotifyErrors(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
            await _smsMessageSender.ReceivedWithAnyArgs(2).SendSmsMessageAndNotifyErrors(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
            var leaseHaulerRequests = await UsingDbContextAsync(async context => await context.LeaseHaulerRequests.ToListAsync());
            leaseHaulerRequests.Count.ShouldBe(1);
        }

        [Fact]
        public async Task Test_SendRequests_should_not_send_Email_and_Sms_and_create_LeaseHaulerRequest_entity_when_there_is_no_LeaseHaulerContact()
        {
            // Arrange
            var leaseHauler = await CreateLeaseHauler();
            var leaseHauler2 = await CreateLeaseHauler();
            await CreateLeaseHaulerContact(leaseHauler2.Id);
            var model = new SendRequestsInput()
            {
                Date = Clock.Now.Date,
                Shift = Shift.Shift1,
                OfficeId = _officeId,
                LeaseHaulerIds = new []{ leaseHauler.Id },
                Message = "message",
            };
            _emailMessageSender.SendEmailMessageAndNotifyErrors(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).ReturnsForAnyArgs(true);
            _smsMessageSender.SendSmsMessageAndNotifyErrors(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).ReturnsForAnyArgs(true);

            // Act
            var result = await _leaseHaulerRequestSendAppService.SendRequests(model);

            // Assert
            result.ShouldBeFalse();
            await _emailMessageSender.DidNotReceiveWithAnyArgs().SendEmailMessageAndNotifyErrors(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
            await _smsMessageSender.DidNotReceiveWithAnyArgs().SendSmsMessageAndNotifyErrors(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
            await _appNotifier.Received().SendMessageAsync(Arg.Any<UserIdentifier>(), Arg.Any<string>(), NotificationSeverity.Error);
            var leaseHaulerRequests = await UsingDbContextAsync(async context => await context.LeaseHaulerRequests.ToListAsync());
            leaseHaulerRequests.Count.ShouldBe(0);
        }

        [Fact]
        public async Task Test_SendRequests_should_not_send_Email_and_Sms_and_create_LeaseHaulerRequest_entity_when_there_is_no_Dispatch_LeaseHaulerContact()
        {
            // Arrange
            var leaseHauler = await CreateLeaseHauler();
            var contact = await CreateLeaseHaulerContact(leaseHauler.Id, isDispatcher: false);
            var model = new SendRequestsInput()
            {
                Date = Clock.Now.Date,
                Shift = Shift.Shift1,
                OfficeId = _officeId,
                LeaseHaulerIds = new []{ leaseHauler.Id },
                Message = "message",
            };
            _emailMessageSender.SendEmailMessageAndNotifyErrors(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).ReturnsForAnyArgs(true);
            _smsMessageSender.SendSmsMessageAndNotifyErrors(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).ReturnsForAnyArgs(true);

            // Act
            var result = await _leaseHaulerRequestSendAppService.SendRequests(model);

            // Assert
            result.ShouldBeFalse();
            await _emailMessageSender.DidNotReceiveWithAnyArgs().SendEmailMessageAndNotifyErrors(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
            await _smsMessageSender.DidNotReceiveWithAnyArgs().SendSmsMessageAndNotifyErrors(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
            await _appNotifier.Received().SendMessageAsync(Arg.Any<UserIdentifier>(), Arg.Any<string>(), NotificationSeverity.Error);
            var leaseHaulerRequests = await UsingDbContextAsync(async context => await context.LeaseHaulerRequests.ToListAsync());
            leaseHaulerRequests.Count.ShouldBe(0);
        }

    }
}
