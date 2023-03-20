using System;
using System.Net.Mail;
using System.Threading.Tasks;
using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Net.Mail;
using Abp.Notifications;
using Abp.Threading;
using Abp.Timing;
using DispatcherWeb.Drivers;
using DispatcherWeb.Emailing;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Notifications;
using DispatcherWeb.Runtime.Session;

namespace DispatcherWeb.BackgroundJobs
{
    public class DriverMessageEmailSenderBackgroundJob : AsyncBackgroundJob<DriverMessageEmailSenderBackgroundJobArgs>, ITransientDependency
    {
        private readonly IRepository<DriverMessage> _driverMessageRepository;
        private readonly ITrackableEmailSender _trackableEmailSender;
        private readonly AspNetZeroAbpSession _session;
        private readonly IAppNotifier _appNotifier;

        public DriverMessageEmailSenderBackgroundJob(
            IRepository<DriverMessage> driverMessageRepository,
            ITrackableEmailSender trackableEmailSender,
            AspNetZeroAbpSession session,
            IAppNotifier appNotifier
            )
        {
            _driverMessageRepository = driverMessageRepository;
            _trackableEmailSender = trackableEmailSender;
            _session = session;
            _appNotifier = appNotifier;
        }

        [UnitOfWork]
        public void Execute(DriverMessageEmailSenderBackgroundJobArgs args)
        {
            try
            {
                AsyncHelper.RunSync(() => ExecuteAsync(args));
            }
            catch (Exception)
            {
                AsyncHelper.RunSync(() => _appNotifier.SendMessageAsync(args.RequestorUser, "Sending the message failed", NotificationSeverity.Error));
                throw;
            }
        }

        [UnitOfWork]
        public async override Task ExecuteAsync(DriverMessageEmailSenderBackgroundJobArgs args)
        {
            using (_session.Use(args.RequestorUser.TenantId, args.RequestorUser.UserId))
            using (UnitOfWorkManager.Current.SetTenantId(args.TenantId))
            {
                var message = new MailMessage(await SettingManager.GetSettingValueAsync(EmailSettingNames.DefaultFromAddress), args.EmailAddress)
                {
                    Subject = args.Subject,
                    Body = args.Body,
                    IsBodyHtml = false
                };
                var trackableEmailId = await _trackableEmailSender.SendTrackableAsync(message);
                var driverMessage = new DriverMessage
                {
                    TimeSent = Clock.Now,
                    DriverId = args.DriverId,
                    MessageType = DriverMessageType.Email,
                    Subject = args.Subject,
                    Body = args.Body.Truncate(EntityStringFieldLengths.DriverMessage.Body),
                    TrackableEmailId = trackableEmailId,
                };

                await _driverMessageRepository.InsertAsync(driverMessage);
                await CurrentUnitOfWork.SaveChangesAsync();
            }
        }
    }
}
