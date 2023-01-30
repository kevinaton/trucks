using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using DispatcherWeb.DriverApp.OrderLineTrucks.Dto;
using DispatcherWeb.Orders;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.DriverApp.OrderLineTrucks
{
    [AbpAuthorize]
    public class OrderLineTruckAppService : DispatcherWebDriverAppAppServiceBase, IOrderLineTruckAppService
    {
        private readonly IRepository<OrderLineTruck> _orderLineTruckRepository;

        public OrderLineTruckAppService(
            IRepository<OrderLineTruck> orderLineTruckRepository
            )
        {
            _orderLineTruckRepository = orderLineTruckRepository;
        }

        public async Task<IPagedResult<OrderLineTruckDto>> Get(GetInput input)
        {
            var currentUserId = Session.GetUserId();
            var query = _orderLineTruckRepository.GetAll()
                .Where(x => input.Ids.Contains(x.Id))
                .Select(x => new OrderLineTruckDto
                {
                    Id = x.Id,
                    DriverNote = x.DriverNote,
                })
                .OrderBy(x => x.Id);

            var totalCount = await query.CountAsync();
            var items = await query
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<OrderLineTruckDto>(
                totalCount,
                items);
        }

        public async Task Post(OrderLineTruckDto model)
        {
            var orderLineTruck = await _orderLineTruckRepository.GetAsync(model.Id);
            if (orderLineTruck == null)
            {
                Logger.Error($"OrderLineTruck.Post: OrderLineTruck with id {model.Id} for tenant {AbpSession.TenantId} and user {AbpSession.UserId} wasn't found");
                throw new UserFriendlyException($"OrderLineTruck with specified Id ('{model.Id}') wasn't found");
            }

            if (!await _orderLineTruckRepository.GetAll().AnyAsync(x => x.Id == model.Id && x.Driver.UserId == Session.UserId))
            {
                Logger.Error($"OrderLineTruck.Post: OrderLineTruck with id {model.Id} is not assigned to user {AbpSession.UserId}");
                throw new UserFriendlyException($"You cannot modify OrderLineTrucks assigned to other users");
            }

            orderLineTruck.DriverNote = model.DriverNote;
        }


    }
}
