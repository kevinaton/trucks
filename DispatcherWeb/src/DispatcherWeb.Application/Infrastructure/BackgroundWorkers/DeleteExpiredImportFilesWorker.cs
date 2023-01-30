using System;
using System.Linq;
using Abp.Collections.Extensions;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using Abp.Timing;
using DispatcherWeb.Infrastructure.AzureBlobs;

namespace DispatcherWeb.Infrastructure.BackgroundWorkers
{
    public class DeleteExpiredImportFilesWorker : PeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly ISecureFileBlobService _secureFileBlobService;

        public DeleteExpiredImportFilesWorker(
            AbpTimer timer,
            ISecureFileBlobService secureFileBlobService
        ) : base(timer)
        {
            _secureFileBlobService = secureFileBlobService;
            Timer.Period = AppConsts.DeleteExpiredImportFilesWorkerPeriod;
        }

        [UnitOfWork]
        protected override void DoWork()
        {
            DeleteExpiredImportFiles();
        }

        private void DeleteExpiredImportFiles()
        {   
            Logger.Info("Start DeleteExpiredImportFiles()");
            _secureFileBlobService.DeleteExpiredFiles(TimeSpan.FromDays(AppConsts.ImportFilesExpireDayNumber));
        }

    }
}
