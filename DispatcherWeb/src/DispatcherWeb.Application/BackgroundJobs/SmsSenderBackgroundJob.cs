using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Notifications;
using Abp.Runtime.Session;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Drivers;
using DispatcherWeb.Infrastructure.Sms;
using DispatcherWeb.Infrastructure.Sms.Dto;
using DispatcherWeb.Notifications;
using DispatcherWeb.Sms;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.BackgroundJobs
{
    public class SmsSenderBackgroundJob : AsyncBackgroundJob<SmsSenderBackgroundJobArgs>, ITransientDependency
    {
        private readonly IAppNotifier _appNotifier;
        private readonly IAbpSession _abpSession;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        //private readonly IRepository<TrackableSms> _trackableSmsSender;
        private readonly ISmsSender _smsSender;
        private readonly IRepository<SentSms> _sentSmsRepository;
        private readonly IRepository<Dispatch> _dispatchRepository;
        private readonly IRepository<Driver> _driverRepository;

        public SmsSenderBackgroundJob(
            IAppNotifier appNotifier,
            IAbpSession abpSession,
            IUnitOfWorkManager unitOfWorkManager,
            //IRepository<TrackableSms> trackableSmsSender,
            ISmsSender smsSender,
            IRepository<SentSms> sentSmsRepository,
            IRepository<Dispatch> dispatchRepository,
            IRepository<Driver> driverRepository
            )
        {
            _appNotifier = appNotifier;
            _abpSession = abpSession;
            _unitOfWorkManager = unitOfWorkManager;
            //_trackableSmsSender = trackableSmsSender;
            _smsSender = smsSender;
            _sentSmsRepository = sentSmsRepository;
            _dispatchRepository = dispatchRepository;
            _driverRepository = driverRepository;
        }

        public override async Task ExecuteAsync(SmsSenderBackgroundJobArgs args)
        {
            using (_abpSession.Use(args.RequestorUser.TenantId, args.RequestorUser.UserId))
            {
                List<(SmsSenderBackgroundJobArgsSms input, SmsSendResult sendResult)> resultList;
                if (args.SmsInputs.Any(x => x.TrackStatus))
                {
                    //we're starting a UOW before sending an sms beceause SmsSender inserts a record into SentSms table
                    resultList = await WithUnitOfWork(args.RequestorUser, async () => await SendSmsBatches(args.SmsInputs));
                }
                else
                {
                    resultList = await SendSmsBatches(args.SmsInputs);
                }

                await WithUnitOfWork(args.RequestorUser, async () =>
                {
                    var errorList = resultList.Where(x => x.sendResult.ErrorCode.HasValue).ToList();
                    if (errorList.Any())
                    {
                        if (errorList.Any(r => r.input.DispatchId.HasValue && r.input.CancelDispatchOnError))
                        {
                            var dispatchIds = resultList
                                .Where(r => r.input.DispatchId.HasValue && r.input.CancelDispatchOnError && r.sendResult.ErrorCode.HasValue)
                                .Select(r => r.input.DispatchId.Value)
                                .Distinct().ToList();
                            var dispatches = await _dispatchRepository.GetAll()
                                .Where(x => dispatchIds.Contains(x.Id))
                                .ToListAsync();

                            dispatches.ForEach(x => x.Status = DispatchStatus.Error);
                        }
                    }

                    foreach (var sentSms in resultList
                        .Where(x => x.sendResult.SentSmsEntityIsInserted == false)
                        .Select(x => x.sendResult.SentSmsEntity)
                        .OfType<SentSms>()
                        .ToList())
                    {
                        _sentSmsRepository.Insert(sentSms);
                    }
                    await CurrentUnitOfWork.SaveChangesAsync();

                    if (errorList.Any())
                    {
                        var driverIds = errorList.Where(x => x.input.DriverId.HasValue).Select(x => x.input.DriverId.Value).Distinct().ToList();
                        var driverNames = await _driverRepository.GetAll().Select(x => new { x.Id, FullName = x.FirstName + " " + x.LastName }).ToListAsync();
                        foreach (var error in errorList)
                        {
                            string detailedErrorMessage = $"Unable to send the SMS to {error.input.ToPhoneNumber}. {error.sendResult.ErrorMessage}";
                            if (error.input.DriverId.HasValue)
                            {
                                var driverName = driverNames.FirstOrDefault(x => x.Id == error.input.DriverId)?.FullName;
                                if (!string.IsNullOrEmpty(driverName))
                                {
                                    detailedErrorMessage = $"Unable to send the dispatch for {driverName} with phone number {error.input.ToPhoneNumber}. {error.sendResult.ErrorMessage}";
                                }
                                Logger.Error($"There was an error while sending the sms to DriverId: {(error.input.DriverId.HasValue ? error.input.DriverId.ToString() : "null")}");
                            }
                            await _appNotifier.SendMessageAsync(
                                args.RequestorUser,
                                detailedErrorMessage,
                                NotificationSeverity.Error
                            );
                        }
                    }

                    return true;
                });
            }
        }

        private async Task<T> WithUnitOfWork<T>(UserIdentifier requestorUser, Func<Task<T>> action)
        {
            using (var uow = _unitOfWorkManager.Begin())
            using (CurrentUnitOfWork.SetTenantId(requestorUser.TenantId))
            using (_abpSession.Use(requestorUser.TenantId, requestorUser.UserId))
            {
                var result = await action();

                uow.Complete();

                return result;
            }
        }

        private async Task<List<(SmsSenderBackgroundJobArgsSms input, SmsSendResult sendResult)>> SendSmsBatches(List<SmsSenderBackgroundJobArgsSms> inputs)
        {
            var resultList = new List<(SmsSenderBackgroundJobArgsSms input, SmsSendResult sendResult)>();
            foreach (var input in inputs)
            {
                try
                {
                    var result = await _smsSender.SendAsync(input.Text, input.ToPhoneNumber, input.TrackStatus, input.UseTenantPhoneNumberOnly, false);
                    resultList.Add((input, result));
                }
                catch (Exception ex) //or ApiException
                {
                    Logger.Error("Error during batch sms sending", ex);
                    resultList.Add((input, new SmsSendResult(null, SmsStatus.Failed, 0, ex.Message)));
                }
            }
            return resultList;
        }
    }
}
