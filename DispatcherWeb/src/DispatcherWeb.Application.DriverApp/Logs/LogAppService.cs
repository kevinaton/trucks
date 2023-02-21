using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using DispatcherWeb.DriverApp.Logs.Dto;
using DispatcherWeb.Drivers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DispatcherWeb.DriverApp.Logs
{
    [AbpAuthorize]
    public class LogAppService : DispatcherWebDriverAppAppServiceBase, ILogAppService
    {
        private readonly IRepository<DriverApplicationLog> _driverApplicationLogRepository;
        private readonly IRepository<Driver> _driverRepository;

        public LogAppService(
            IRepository<DriverApplicationLog> driverApplicationLogRepository,
            IRepository<Driver> driverRepository
            )
        {
            _driverApplicationLogRepository = driverApplicationLogRepository;
            _driverRepository = driverRepository;
        }

        [AbpAllowAnonymous]
        public async Task Post(PostInput input)
        {
            if (input.DeviceGuid == null)
            {
                throw new UserFriendlyException("DeviceGuid is required");
            }
            if (input.Logs == null || input.Logs.Count == 0)
            {
                throw new UserFriendlyException("Logs are required");
            }

            var userIds = input.Logs
                .Where(x => x.UserId.HasValue)
                .Select(x => x.UserId)
                .Distinct()
                .ToList();
            var drivers = await _driverRepository.GetAll()
                .Where(x => userIds.Contains(x.UserId))
                .Select(x => new
                {
                    x.Id,
                    x.UserId,
                    x.TenantId,
                    x.IsInactive
                })
                .OrderByDescending(x => !x.IsInactive)
                .ToListAsync();

            var i = 0;
            foreach (var log in input.Logs)
            {
                var driver = drivers.FirstOrDefault(x => x.UserId == log.UserId);
                _driverApplicationLogRepository.Insert(new DriverApplicationLog
                {
                    OriginalLogId = log.Id,
                    //ServiceWorker = log.Sw,
                    BatchOrder = i++,
                    DateTime = log.DateTime,
                    DriverId = driver?.Id,
                    Level = log.Level,
                    Message = log.Message,
                    TenantId = driver?.TenantId,
                    UserId = log.UserId,
                    DeviceGuid = input.DeviceGuid,
                    AppVersion = log.AppVersion
                });
            }
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        [AbpAllowAnonymous]
        public Task<GetMinLevelToUploadResult> GetMinLevelToUpload(GetMinLevelToUploadInput input)
        {
            if (!input.DeviceGuid.HasValue)
            {
                throw new UserFriendlyException("DeviceGuid is required");
            }

            return Task.FromResult(new GetMinLevelToUploadResult
            {
                MinLevelToUpload = LogLevel.Warning
            });
        }
    }
}
