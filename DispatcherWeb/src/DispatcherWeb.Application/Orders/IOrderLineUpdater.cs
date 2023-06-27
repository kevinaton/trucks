using System;
using System.Threading.Tasks;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Infrastructure.EntityUpdaters;

namespace DispatcherWeb.Orders
{
    public interface IOrderLineUpdater : IEntityUpdater<OrderLine>
    {
        Task<Order> GetOrderAsync();
        Task MarkAffectedDispatchesWhereAsync(Func<Dispatch, bool> wherePredicate);
        void SuppressReadOnlyChecker(bool suppressReadOnlyChecker = true);
        void UpdateStaggeredTimeOnTrucksOnSave();
    }
}
