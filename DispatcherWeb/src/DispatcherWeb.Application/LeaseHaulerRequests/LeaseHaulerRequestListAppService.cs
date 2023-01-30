using System;
using System.Linq.Dynamic.Core;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using DispatcherWeb.Authorization;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.LeaseHaulerRequests.Dto;
using Microsoft.EntityFrameworkCore;
using DispatcherWeb.Orders;

namespace DispatcherWeb.LeaseHaulerRequests
{
    [AbpAuthorize(AppPermissions.Pages_LeaseHaulerRequests)]
    public class LeaseHaulerRequestListAppService : DispatcherWebAppServiceBase
    {
        private readonly IRepository<LeaseHaulerRequest> _leaseHaulerRequestRepository;
        private readonly IRepository<OrderLineTruck> _orderLineTruckRepository;

        public LeaseHaulerRequestListAppService(
            IRepository<LeaseHaulerRequest> leaseHaulerRequestRepository,
            IRepository<OrderLineTruck> orderLineTruckRepository
        )
        {
            _leaseHaulerRequestRepository = leaseHaulerRequestRepository;
            _orderLineTruckRepository = orderLineTruckRepository;
        }

        public async Task<PagedResultDto<LeaseHaulerRequestDto>> GetLeaseHaulerRequestPagedList(GetLeaseHaulerRequestPagedListInput input)
        {
            var query = GetFilteredLeaseHaulerRequestQuery(input);

            var totalCount = await query.CountAsync();

            var items = await (await GetLeaseHaulerRequestDtoQuery(query))
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<LeaseHaulerRequestDto>(totalCount, items);
        }

        private IQueryable<LeaseHaulerRequest> GetFilteredLeaseHaulerRequestQuery(IGetLeaseHaulerRequestPagedListInput input) =>
            _leaseHaulerRequestRepository.GetAll()
                .Where(lhr => lhr.Date >= input.DateBegin && lhr.Date < input.DateEnd.AddDays(1))
                .WhereIf(input.OfficeId.HasValue, lhr => lhr.OfficeId == input.OfficeId)
                .WhereIf(input.Shift.HasValue && input.Shift != Shift.NoShift, lhr => lhr.Shift == input.Shift)
                .WhereIf(input.Shift.HasValue && input.Shift == Shift.NoShift, da => da.Shift == null)
                .WhereIf(input.LeaseHaulerId.HasValue, lhr => lhr.LeaseHaulerId == input.LeaseHaulerId);

        private async Task<IQueryable<LeaseHaulerRequestDto>> GetLeaseHaulerRequestDtoQuery(IQueryable<LeaseHaulerRequest> query)
        {
            var shiftDictionary = await SettingManager.GetShiftDictionary();
            return query.Select(lhr => new LeaseHaulerRequestDto
            {
                Id = lhr.Id,
                Date = lhr.Date,
                Shift = lhr.Shift.HasValue ? shiftDictionary[lhr.Shift.Value] : "",
                LeaseHauler = lhr.LeaseHauler.Name,
                Sent = lhr.Sent,
                Available = lhr.Available,
                Approved = lhr.Approved,
                Scheduled = _orderLineTruckRepository.GetAll()
                    .Where(olt => olt.OrderLine.Order.LocationId == lhr.OfficeId
                                  && olt.OrderLine.Order.Shift == lhr.Shift
                                  && olt.OrderLine.Order.DeliveryDate == lhr.Date
                                  && olt.Truck.LeaseHaulerTruck.LeaseHaulerId == lhr.LeaseHaulerId
                    )
                    .Select(olt => olt.TruckId)
                    .Distinct()
                    .Count(),
            });
        }
    }
}
