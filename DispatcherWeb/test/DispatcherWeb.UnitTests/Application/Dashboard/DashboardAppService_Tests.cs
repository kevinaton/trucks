using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Dashboard;
using DispatcherWeb.Orders;
using DispatcherWeb.Trucks;
using DispatcherWeb.UnitTests.TestUtilities;
using DispatcherWeb.VehicleMaintenance;
using NSubstitute;
using Shouldly;
using Xunit;

namespace DispatcherWeb.UnitTests.Application.Dashboard
{
	public class DashboardAppService_Tests
	{
		//private int _outOfServiceHistoryId = 1;
		//private readonly DateTime _now = ClockProvider.DateTimeNow;
		//private readonly IDashboardAppService _dashboardAppService;
		//private readonly IRepository<OutOfServiceHistory> _outOfServiceHistoryRepository;

		//public DashboardAppService_Tests()
		//{
		//	var orderRepository = Substitute.For<IRepository<Order>>();
		//	var truckRepository = Substitute.For<IRepository<Truck>>();
		//	_outOfServiceHistoryRepository = Substitute.For<IRepository<OutOfServiceHistory>>();

		//	_dashboardAppService = new DashboardAppService(
		//		orderRepository,
		//		truckRepository,
		//		_outOfServiceHistoryRepository
		//	);
		//}

		//[Fact]
		//public async Task Test_ReturnToService_should_set_InServiceDate_and_IsOutOfService_true()
		//{
		//	// Arrange
		//	Truck truck = new Truck() { IsOutOfService = true };
		//	OutOfServiceHistory outOfServiceHistory = new OutOfServiceHistory()
		//	{
		//		Id = _outOfServiceHistoryId,
		//		OutOfServiceDate = new DateTime(2018, 1, 1),
		//		Truck = truck,
		//	};
		//	var outOfServiceHistoryMockSet = new List<OutOfServiceHistory>
		//	{
		//		outOfServiceHistory,
		//	}.CreateMockSet();
		//	IQueryable<OutOfServiceHistory> query = Substitute.For<IQueryable<OutOfServiceHistory>>();
		//	query.Include(Arg.Any<Expression<Func<OutOfServiceHistory, Truck>>>()).Returns(x => outOfServiceHistoryMockSet);
		//	_outOfServiceHistoryRepository.GetAll().Returns(query);


		//	// Act
		//	await _dashboardAppService.ReturnToService(_outOfServiceHistoryId);

		//	// Assert
		//	outOfServiceHistory.InServiceDate.ShouldBe(_now);
		//	truck.IsOutOfService.ShouldBeFalse();
		//}

	}
}
