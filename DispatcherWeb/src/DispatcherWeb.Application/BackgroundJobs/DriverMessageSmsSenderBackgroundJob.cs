using System;
using System.Threading.Tasks;
using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Notifications;
using Abp.Threading;
using Abp.Timing;
using DispatcherWeb.Drivers;
using DispatcherWeb.Infrastructure.Sms;
using DispatcherWeb.Infrastructure.Sms.Dto;
using DispatcherWeb.Notifications;
using DispatcherWeb.Runtime.Session;
using Twilio.Exceptions;

namespace DispatcherWeb.BackgroundJobs
{
    public class DriverMessageSmsSenderBackgroundJob : AsyncBackgroundJob<DriverMessageSmsSenderBackgroundJobArgs>, ITransientDependency
    {
        private readonly IRepository<DriverMessage> _driverMessageRepository;
        private readonly ISmsSender _smsSender;
        private readonly AspNetZeroAbpSession _session;
        private readonly IAppNotifier _appNotifier;

        public DriverMessageSmsSenderBackgroundJob(
            IRepository<DriverMessage> driverMessageRepository,
            ISmsSender smsSender,
            AspNetZeroAbpSession session,
            IAppNotifier appNotifier
            )
        {
            _driverMessageRepository = driverMessageRepository;
            _smsSender = smsSender;
            _session = session;
            _appNotifier = appNotifier;
        }

        [UnitOfWork]
        public void Execute(DriverMessageSmsSenderBackgroundJobArgs args)
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
        public async override Task ExecuteAsync(DriverMessageSmsSenderBackgroundJobArgs args)
        {
            using (_session.Use(args.RequestorUser.TenantId, args.RequestorUser.UserId))
            using (UnitOfWorkManager.Current.SetTenantId(args.TenantId))
            {
                string smsText = $"{args.Body}"; //$"{subject} {body}";
                SmsSendResult smsSendResult;
                try
                {
                    smsSendResult = await _smsSender.SendAsync(smsText, args.CellPhoneNumber, true);
                }
                catch (ApiException e)
                {
                    Logger.Error(e.ToString);
                    //save the changes before sending the notification to avoid "Can not set TenantId to 0 for IMustHaveTenant entities!" exception
                    await CurrentUnitOfWork.SaveChangesAsync();
                    await _appNotifier.SendMessageAsync(
                        args.RequestorUser,
                        $"Unable to send the message to {args.DriverFullName}. Details: {e.Message}",
                        NotificationSeverity.Error
                    );
                    return;
                }

                var driverMessage = new DriverMessage
                {
                    TimeSent = Clock.Now,
                    DriverId = args.DriverId,
                    MessageType = DriverMessageType.Sms,
                    Subject = args.Subject,
                    Body = args.Body,
                    SentSmsId = smsSendResult.SentSmsEntity?.Id
                };

                await _driverMessageRepository.InsertAsync(driverMessage);
                await CurrentUnitOfWork.SaveChangesAsync();
            }
        }
    }
}
