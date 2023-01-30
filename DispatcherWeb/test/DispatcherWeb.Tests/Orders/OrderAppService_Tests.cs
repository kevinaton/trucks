using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Abp.Configuration.Startup;
using Abp.MultiTenancy;
using Abp.Runtime;
using Abp.Runtime.Session;
using Abp.UI;
using DispatcherWeb.Customers;
using DispatcherWeb.Offices;
using DispatcherWeb.Orders;
using DispatcherWeb.Orders.Dto;
using DispatcherWeb.Runtime.Session;
using DispatcherWeb.Services;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Orders
{
    public class OrderAppService_Tests : AppTestBase
    {
        private readonly IOrderAppService _orderAppService;

        public OrderAppService_Tests()
        {
            _orderAppService = Resolve<IOrderAppService>();

            ((DispatcherWebAppServiceBase)_orderAppService).Session = CreateSession();
        }

        [Fact]
        public async Task Test_DeleteOrderLine_should_update_price_fields()
        {
            var orderEntity = await CreateOrder();

            int orderLineId = orderEntity.OrderLines.First(ol => ol.FreightPricePerUnit == 2).Id;
            DeleteOrderLineInput input = new DeleteOrderLineInput()
            {
                Id = orderLineId,
                OrderId = orderEntity.Id,
            };
            await _orderAppService.DeleteOrderLine(input);

            var orderEntityResult = await UsingDbContextAsync(async context =>
            {
                return await context.Orders
                    .Include(o => o.OrderLines)
                    .FirstAsync(o => o.Id == orderEntity.Id);
            });
            orderEntityResult.Id.ShouldBe(orderEntity.Id);
            orderEntityResult.OrderLines.Count(ol => !ol.IsDeleted).ShouldBe(1);
            orderEntityResult.FreightTotal.ShouldBe(0 + 2 * 20); //  2nd OrderLine
            orderEntityResult.MaterialTotal.ShouldBe(0 + 2 * 30);
            orderEntityResult.SalesTax.ShouldBe(2);
            orderEntityResult.CODTotal.ShouldBe(102);
            var orderLine = orderEntityResult.OrderLines.First(ol => !ol.IsDeleted);
            orderLine.FreightPrice.ShouldBe(20 * 2);
            orderLine.MaterialPrice.ShouldBe(30 * 2);
            orderLine.LineNumber.ShouldBe(1);
        }

        [Fact]
        public async Task Test_SetSharedOrderLines_should_create_SharedOrderLine()
        {
            // Assign
            var orderEntity = await CreateOrder();
            int orderLineId = orderEntity.OrderLines.First().Id;
            int[] shareOfficeIds = await UsingDbContextAsync(async context =>
            {
                int[] existedOffices = await context.Offices.Select(o => o.Id).ToArrayAsync();
                await context.Offices.AddAsync(new Office() { TenantId = 1, Name = "Office2", TruckColor = "222" });
                await context.Offices.AddAsync(new Office() { TenantId = 1, Name = "Office3", TruckColor = "333" });
                await context.SaveChangesAsync();
                return await context.Offices.Where(o => !existedOffices.Contains(o.Id)).Select(o => o.Id).ToArrayAsync();
            });
            shareOfficeIds.Length.ShouldBe(2);

            // Act
            await _orderAppService.SetSharedOrderLines(new SetSharedOrderLineInput()
            {
                OrderLineId = orderLineId,
                CheckedOfficeIds = shareOfficeIds,
            });

            // Assert
            var sharedOrderLines = await UsingDbContextAsync(async context => await context.SharedOrderLines.Where(sol => sol.OrderLine.OrderId == orderEntity.Id).ToListAsync());
            sharedOrderLines.Count.ShouldBe(2);
            sharedOrderLines[0].OrderLineId.ShouldBe(orderLineId);
            sharedOrderLines[1].OrderLineId.ShouldBe(orderLineId);
            sharedOrderLines.Count(sol => sol.OfficeId == shareOfficeIds[0]).ShouldBe(1);
            sharedOrderLines.Count(sol => sol.OfficeId == shareOfficeIds[1]).ShouldBe(1);
        }

        [Fact]
        public async Task Test_SetSharedOrderLines_should_delete_SharedOrderLine()
        {
            // Assign
            var orderEntity = await CreateOrder();
            int orderLineId = orderEntity.OrderLines.First().Id;
            int[] shareOfficeIds = await UsingDbContextAsync(async context =>
            {
                int[] existedOffices = await context.Offices.Select(o => o.Id).ToArrayAsync();
                await context.Offices.AddAsync(new Office() { TenantId = 1, Name = "Office2", TruckColor = "222" });
                await context.Offices.AddAsync(new Office() { TenantId = 1, Name = "Office3", TruckColor = "333" });
                await context.SaveChangesAsync();
                return await context.Offices.Where(o => !existedOffices.Contains(o.Id)).Select(o => o.Id).ToArrayAsync();
            });
            shareOfficeIds.Length.ShouldBe(2);

            await _orderAppService.SetSharedOrderLines(new SetSharedOrderLineInput()
            {
                OrderLineId = orderLineId,
                CheckedOfficeIds = shareOfficeIds,
            });

            // Act
            await _orderAppService.SetSharedOrderLines(new SetSharedOrderLineInput()
            {
                OrderLineId = orderLineId,
                CheckedOfficeIds = new int[] {},
            });

            // Assert
            var sharedOrderLines = await UsingDbContextAsync(async context => await context.SharedOrderLines.Where(sol => !sol.IsDeleted && sol.OrderLine.OrderId == orderEntity.Id).ToListAsync());
            sharedOrderLines.Count.ShouldBe(0);
        }



        private async Task<Order> CreateOrder()
        {
            var orderEntity = await UsingDbContextAsync(async context =>
            {
                Order order = new Order
                {
                    TenantId = 1,
                    DeliveryDate = DateTime.Today,
                    Customer = new Customer() { TenantId = 1, Name = "Cust" },
                    Office = new Office() { TenantId = 1, Name = "Office1", TruckColor = "fff" },
                    SalesTaxRate = 2,
                };
                order.OrderLines.Add(new OrderLine()
                {
                    TenantId = 1,
                    Designation = DesignationEnum.FreightAndMaterial,
                    FreightPricePerUnit = 2,
                    MaterialPricePerUnit = 3,
                    MaterialQuantity = 5,
                    FreightPrice = 2 * 5,
                    MaterialPrice = 3 * 5,
                    Service = new Service()
                    {
                        TenantId = 1,
                        Service1 = "sss",
                    },
                    MaterialUomId = 1,
                });
                order.OrderLines.Add(new OrderLine()
                {
                    TenantId = 1,
                    Designation = DesignationEnum.FreightAndMaterial,
                    FreightPricePerUnit = 20,
                    MaterialPricePerUnit = 30,
                    MaterialQuantity = 2,
                    FreightPrice = 20 * 2,
                    MaterialPrice = 30 * 2,
                    Service = new Service()
                    {
                        TenantId = 1,
                        Service1 = "sss",
                    },
                    MaterialUomId = 1,
                });
                await context.Orders.AddAsync(order);

                var user = await context.Users.FirstAsync(u => u.TenantId == 1);
                user.Office = order.Office;
                await context.SaveChangesAsync();

                return order;
            });
            return orderEntity;
        }

    }
}
