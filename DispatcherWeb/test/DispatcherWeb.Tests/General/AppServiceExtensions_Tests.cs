using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.UI;
using DispatcherWeb.Customers;
using DispatcherWeb.Offices;
using DispatcherWeb.Orders;
using DispatcherWeb.Services;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.General
{
    public class AppServiceExtensions_Tests : AppTestBase
    {
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public AppServiceExtensions_Tests()
        {
            _unitOfWorkManager = Resolve<IUnitOfWorkManager>();
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
