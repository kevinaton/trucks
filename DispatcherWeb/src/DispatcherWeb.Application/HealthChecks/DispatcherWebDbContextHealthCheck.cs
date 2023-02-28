using System.Threading;
using System.Threading.Tasks;
using DispatcherWeb.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DispatcherWeb.HealthChecks
{
    public class DispatcherWebDbContextHealthCheck : IHealthCheck
    {
        private readonly DatabaseCheckHelper _checkHelper;

        public DispatcherWebDbContextHealthCheck(DatabaseCheckHelper checkHelper)
        {
            _checkHelper = checkHelper;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            if (_checkHelper.Exist("db"))
            {
                return Task.FromResult(HealthCheckResult.Healthy("DispatcherWebDbContext connected to database."));
            }

            return Task.FromResult(HealthCheckResult.Unhealthy("DispatcherWebDbContext could not connect to database"));
        }
    }
}
