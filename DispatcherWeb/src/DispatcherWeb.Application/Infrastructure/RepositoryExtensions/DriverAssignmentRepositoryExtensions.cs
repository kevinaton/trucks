using System;
using System.Linq;
using Abp.Domain.Repositories;
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
                .Where(da => da.Date == date && da.Shift == shift &&
                            (!officeId.HasValue || da.OfficeId == officeId));
        }
    }
}
