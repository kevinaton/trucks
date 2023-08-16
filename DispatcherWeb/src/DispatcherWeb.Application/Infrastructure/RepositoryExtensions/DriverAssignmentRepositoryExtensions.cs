using System;
using System.Linq;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using DispatcherWeb.Drivers;

namespace DispatcherWeb.Infrastructure.RepositoryExtensions
{
    public static class DriverAssignmentRepositoryExtensions
    {
        public static IQueryable<DriverAssignment> GetAll(
            this IRepository<DriverAssignment> driverAssignmentRepository,
            DateTime date,
            Shift? shift,
            int? officeId
        )
        {
            return driverAssignmentRepository.GetAll()
                .WhereIf(officeId.HasValue, da => da.OfficeId == officeId)
                .Where(da => da.Date == date && da.Shift == shift);
        }
    }
}
