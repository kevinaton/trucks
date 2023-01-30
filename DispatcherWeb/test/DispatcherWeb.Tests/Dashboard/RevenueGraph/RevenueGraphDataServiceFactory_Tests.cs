using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Dependency;
using DispatcherWeb.Dashboard.RevenueGraph;
using DispatcherWeb.Dashboard.RevenueGraph.DataServices;
using DispatcherWeb.Dashboard.RevenueGraph.Dto;
using DispatcherWeb.Dashboard.RevenueGraph.Factories;
using Xunit;

namespace DispatcherWeb.Tests.Dashboard.RevenueGraph
{
    public class RevenueGraphDataServiceFactory_Tests : AppTestBase
    {
        private readonly IIocResolver _iocResolver;
        private readonly IScopedIocResolver _scope;

        public RevenueGraphDataServiceFactory_Tests()
        {
            _iocResolver = Resolve<IIocResolver>();
            _scope = _iocResolver.CreateScope();
        }

        [Fact]
        public void Test_CreateRevenueGraphDataService_should_return_RevenueGraphDataDailyService_instance()
        {
            // Act
            var instance1 = RevenueGraphDataServiceFactory.CreateRevenueGraphDataService(_iocResolver, RevenueGraphDatePeriod.Daily, RevenueCalculateType.ByTickets);
            var instance2 = RevenueGraphDataServiceFactory.CreateRevenueGraphDataService(_iocResolver, RevenueGraphDatePeriod.Daily, RevenueCalculateType.ByOfficeAmount);

            // Assert
            Assert.True(instance1 is RevenueGraphDataDailyService);
            Assert.True(instance2 is RevenueGraphDataDailyService);
        }

        [Fact]
        public void Test_CreateRevenueGraphDataService_should_return_RevenueGraphDataWeeklyService_instance()
        {
            // Act
            var instance1 = RevenueGraphDataServiceFactory.CreateRevenueGraphDataService(_iocResolver, RevenueGraphDatePeriod.Weekly, RevenueCalculateType.ByTickets);
            var instance2 = RevenueGraphDataServiceFactory.CreateRevenueGraphDataService(_iocResolver, RevenueGraphDatePeriod.Weekly, RevenueCalculateType.ByOfficeAmount);

            // Assert
            Assert.True(instance1 is RevenueGraphDataWeeklyService);
            Assert.True(instance2 is RevenueGraphDataWeeklyService);
        }

        [Fact]
        public void Test_CreateRevenueGraphDataService_should_return_RevenueGraphDataMonthlyService_instance()
        {
            // Act
            var instance1 = RevenueGraphDataServiceFactory.CreateRevenueGraphDataService(_iocResolver, RevenueGraphDatePeriod.Monthly, RevenueCalculateType.ByTickets);
            var instance2 = RevenueGraphDataServiceFactory.CreateRevenueGraphDataService(_iocResolver, RevenueGraphDatePeriod.Monthly, RevenueCalculateType.ByOfficeAmount);

            // Assert
            Assert.True(instance1 is RevenueGraphDataMonthlyService);
            Assert.True(instance2 is RevenueGraphDataMonthlyService);
        }

        public override void Dispose()
        {
            _scope.Dispose();
            base.Dispose();
        }

    }
}
