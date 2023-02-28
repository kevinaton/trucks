using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Dependency;
using DispatcherWeb.Dashboard.RevenueGraph;
using DispatcherWeb.Dashboard.RevenueGraph.DataItemsQueryServices;
using DispatcherWeb.Dashboard.RevenueGraph.Factories;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Dashboard.RevenueGraph
{
    public class RevenueGraphDataItemsQueryServiceFactory_Tests : AppTestBase
    {
        private readonly IIocResolver _iocResolver;
        private readonly IScopedIocResolver _scope;

        public RevenueGraphDataItemsQueryServiceFactory_Tests()
        {
            _iocResolver = Resolve<IIocResolver>();
            _scope = _iocResolver.CreateScope();
        }

        [Fact]
        public void Test_CreateRevenueGraphDataService_should_return_IRevenueGraphByTicketsDataService_instance()
        {
            // Act
            var instance = RevenueGraphDataItemsQueryServiceFactory.CreateRevenueGraphDataItemsQueryService(_iocResolver, RevenueCalculateType.ByTickets);

            // Assert
            Assert.True(instance is IRevenueGraphByTicketsDataItemsQueryService);
        }

        [Fact]
        public void Test_CreateRevenueGraphDataService_should_return_IRevenueGraphByOfficeAmountDataItemsQueryService_instance()
        {
            // Act
            var instance = RevenueGraphDataItemsQueryServiceFactory.CreateRevenueGraphDataItemsQueryService(_iocResolver, RevenueCalculateType.ByReceipts);

            // Assert
            Assert.True(instance is IRevenueGraphByReceiptsDataItemsQueryService);
        }

        public override void Dispose()
        {
            _scope.Dispose();
            base.Dispose();
        }
    }
}
