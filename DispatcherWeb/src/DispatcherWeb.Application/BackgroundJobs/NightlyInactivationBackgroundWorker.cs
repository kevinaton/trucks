using System;
using System.Linq;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using Abp.Timing;
using DispatcherWeb.Projects;
using DispatcherWeb.Quotes;

namespace DispatcherWeb.BackgroundJobs
{
    public class NightlyInactivationBackgroundWorker : PeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly IRepository<BackgroundJobHistory> _backgroundJobHistoryRepository;
        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<Quote> _quoteRepository;

        public NightlyInactivationBackgroundWorker(
            AbpTimer timer,
            IRepository<BackgroundJobHistory> backgroundJobHistoryRepository,
            IRepository<Project> projectRepository,
            IRepository<Quote> quoteRepository
            )
            : base(timer)
        {
            _backgroundJobHistoryRepository = backgroundJobHistoryRepository;
            _projectRepository = projectRepository;
            _quoteRepository = quoteRepository;

            Timer.Period = (int)TimeSpan.FromHours(1).TotalMilliseconds;
            Timer.RunOnStart = true;
        }

        [UnitOfWork]
        protected override void DoWork()
        {
            var timeZone = SettingManager.GetSettingValue(TimingSettingNames.TimeZone);
            
            var lastMidnightDate = DateTime.Now.ConvertTimeZoneTo(timeZone).Date;

            var lastRun = _backgroundJobHistoryRepository.GetAll()
                .Where(x => x.Job == BackgroundJobEnum.NightlyInactivation)
                .OrderByDescending(x => x.StartTime)
                .Select(x => x.StartTime)
                .FirstOrDefault();
            
            if (lastRun >= lastMidnightDate)
            {
                return;
            }

            var historyRecord = new BackgroundJobHistory
            {
                Job = BackgroundJobEnum.NightlyInactivation,
                StartTime = DateTime.Now
            };
            historyRecord.Id = _backgroundJobHistoryRepository.InsertAndGetId(historyRecord);

            var projectsToDeactivate = _projectRepository
                .GetAllIncluding(x => x.Quotes)
                .Where(x => x.Status != ProjectStatus.Inactive
                        && x.EndDate < lastMidnightDate)
                .ToList();
            
            var totalProjectsDeactivated = 0;
            var totalChildQuotesDeactivated = 0;
            var i = 0;
            foreach (var project in projectsToDeactivate)
            {
                project.Status = ProjectStatus.Inactive;
                foreach (var quote in project.Quotes)
                {
                    quote.Status = ProjectStatus.Inactive;
                    quote.InactivationDate = project.EndDate;
                    totalChildQuotesDeactivated++;
                    i++;
                }

                totalProjectsDeactivated++;
                if (i++ >= 500)
                {
                    i = 0;
                    CurrentUnitOfWork.SaveChanges();
                }
            }

            CurrentUnitOfWork.SaveChanges();

            var quotesToDeactivate = _quoteRepository.GetAll()
                .Where(x => x.Status != ProjectStatus.Inactive
                            && x.InactivationDate < lastMidnightDate)
                .ToList();

            var totalQuotesDeactivated = 0;
            foreach (var quote in quotesToDeactivate)
            {
                quote.Status = ProjectStatus.Inactive;

                totalQuotesDeactivated++;
                if (i++ >= 500)
                {
                    i = 0;
                    CurrentUnitOfWork.SaveChanges();
                }
            }

            CurrentUnitOfWork.SaveChanges();

            historyRecord.EndTime = DateTime.Now;
            historyRecord.Completed = true;
            historyRecord.Details = $"Total projects deactivated: {totalProjectsDeactivated} (including {totalChildQuotesDeactivated} associated quotes). Total quotes deactivated: {totalQuotesDeactivated}.";
            CurrentUnitOfWork.SaveChanges();
        }
    }
}
