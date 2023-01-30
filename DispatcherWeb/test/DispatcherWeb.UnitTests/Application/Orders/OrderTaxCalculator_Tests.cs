using System.Collections.Generic;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using DispatcherWeb.Configuration;
using DispatcherWeb.Orders;
using DispatcherWeb.Orders.Dto;
using DispatcherWeb.Orders.TaxDetails;
using NSubstitute;
using Shouldly;
using Xunit;

namespace DispatcherWeb.UnitTests.Application.Orders
{
    public class OrderTaxCalculator_Tests
    {
        private readonly OrderTaxCalculator _orderTaxCalculator;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly ISettingManager _settingManager;

        public OrderTaxCalculator_Tests()
        {
            _settingManager = Substitute.For<ISettingManager>();
            _unitOfWorkManager = Substitute.For<IUnitOfWorkManager>();
            var orderRepository = Substitute.For<IRepository<Order>>();
            var orderLineRepository = Substitute.For<IRepository<OrderLine>>();
            var receiptRepository = Substitute.For<IRepository<Receipt>>();
            var receiptLineRepository = Substitute.For<IRepository<ReceiptLine>>();

            _orderTaxCalculator = new OrderTaxCalculator(
                _settingManager, 
                orderRepository, 
                orderLineRepository,
                receiptRepository,
                receiptLineRepository,
                _unitOfWorkManager
            );
        }

        private IOrderTaxDetails GetSampleOrder()
        {
            return new OrderTaxDetailsDto
            {
                Id = 1,
                SalesTaxRate = 10
            };
        }

        private IEnumerable<IOrderLineTaxDetails> GetSampleOrderLines()
        {
            return new List<OrderLineTaxDetailsDto>
            {
                new OrderLineTaxDetailsDto { FreightPrice = 30, MaterialPrice = 220, IsTaxable = true },
                new OrderLineTaxDetailsDto { FreightPrice = 600, MaterialPrice = 0, IsTaxable = true },
                new OrderLineTaxDetailsDto { FreightPrice = 0, MaterialPrice = 400, IsTaxable = true }
            };
        }

        [Fact]
        public async void Test_GetTaxCalculationTypeAsync()
        {
            // Arrange
            _settingManager.GetSettingValueAsync(Arg.Is(AppSettings.Invoice.TaxCalculationType))
                .Returns("1", "3");

            // Act
            var a = await _orderTaxCalculator.GetTaxCalculationTypeAsync();
            var b = await _orderTaxCalculator.GetTaxCalculationTypeAsync();

            // Assert
            a.ShouldBe(TaxCalculationType.FreightAndMaterialTotal);
            b.ShouldBe(TaxCalculationType.MaterialTotal);
        }

        [Fact]
        public void Test_CalculateTotals_should_not_round_input_values()
        {
            // Arrange
            var order = GetSampleOrder();
            order.SalesTaxRate = 10.12345M;

            // Act
            OrderTaxCalculator.CalculateTotals(TaxCalculationType.FreightAndMaterialTotal, order, GetSampleOrderLines());

            // Assert
            order.SalesTaxRate.ShouldBe(10.12345M);
        }

        [Fact]
        public void Test_CalculateTotals_for_FreightAndMaterialTotal()
        {
            // Arrange
            var order = GetSampleOrder();

            // Act
            OrderTaxCalculator.CalculateTotals(TaxCalculationType.FreightAndMaterialTotal, order, GetSampleOrderLines());

            // Assert
            //specific for this calculation type
            order.SalesTax.ShouldBe(125);
            order.CODTotal.ShouldBe(1406.5M);

            //common for all types of calculation
            order.FreightTotal.ShouldBe(630);
            order.MaterialTotal.ShouldBe(620);

            //should remain unchanged
            order.Id.ShouldBe(1);
            order.SalesTaxRate.ShouldBe(10);
        }

        [Fact]
        public void Test_CalculateTotals_for_FreightAndMaterialTotal_ShouldUseBankersRounding()
        {
            // Arrange
            var order = GetSampleOrder();
            order.SalesTaxRate = 5.5M;

            var orderLines = new List<OrderLineTaxDetailsDto>
            {
                new OrderLineTaxDetailsDto { MaterialPrice = 0, FreightPrice = 300, IsTaxable = true },
                new OrderLineTaxDetailsDto { MaterialPrice = 40, FreightPrice = 0, IsTaxable = true },
                new OrderLineTaxDetailsDto { MaterialPrice = 154, FreightPrice = 21, IsTaxable = true }
            };

            // Act
            OrderTaxCalculator.CalculateTotals(TaxCalculationType.FreightAndMaterialTotal, order, orderLines);

            // Assert
            order.FreightTotal.ShouldBe(321);
            order.MaterialTotal.ShouldBe(194);
            order.SalesTax.ShouldBe(28.32M); //28.325 -> 28.32 (with banker's rounding)
            order.CODTotal.ShouldBe(552.96M); //552.955 -> 552.96 (with banker's rounding)
        }

        [Fact]
        public void Test_CalculateTotals_for_MaterialLineItemsTotal()
        {
            // Arrange
            var order = GetSampleOrder();

            // Act
            OrderTaxCalculator.CalculateTotals(TaxCalculationType.MaterialLineItemsTotal, order, GetSampleOrderLines());

            // Assert
            //specific for this calculation type
            order.SalesTax.ShouldBe(65);
            order.CODTotal.ShouldBe(1346.5M);

            //common for all types of calculation
            order.FreightTotal.ShouldBe(630);
            order.MaterialTotal.ShouldBe(620);

            //should remain unchanged
            order.Id.ShouldBe(1);
            order.SalesTaxRate.ShouldBe(10);
        }

        [Fact]
        public void Test_CalculateTotals_for_MaterialTotal()
        {
            // Arrange
            var order = GetSampleOrder();

            // Act
            OrderTaxCalculator.CalculateTotals(TaxCalculationType.MaterialTotal, order, GetSampleOrderLines());

            // Assert
            //specific for this calculation type
            order.SalesTax.ShouldBe(62);
            order.CODTotal.ShouldBe(1343.5M);

            //common for all types of calculation
            order.FreightTotal.ShouldBe(630);
            order.MaterialTotal.ShouldBe(620);

            //should remain unchanged
            order.Id.ShouldBe(1);
            order.SalesTaxRate.ShouldBe(10);
        }

        [Fact]
        public void Test_CalculateTotals_for_NoCalculation()
        {
            // Arrange
            var order = GetSampleOrder();
            order.SalesTax = 30;

            // Act
            OrderTaxCalculator.CalculateTotals(TaxCalculationType.NoCalculation, order, GetSampleOrderLines());

            // Assert
            //specific for this calculation type
            order.CODTotal.ShouldBe(1311.5M);
            order.SalesTaxRate.ShouldBe(0);
            order.SalesTax.ShouldBe(30);

            //common for all types of calculation
            order.FreightTotal.ShouldBe(630);
            order.MaterialTotal.ShouldBe(620);

            //should remain unchanged
            order.Id.ShouldBe(1);
        }

        [Fact]
        public void Test_CalculateTotals_NoCalculation_should_round_SalesTax()
        {
            // Arrange
            var order = GetSampleOrder();
            order.SalesTax = 30.12345M;

            // Act
            OrderTaxCalculator.CalculateTotals(TaxCalculationType.NoCalculation, order, GetSampleOrderLines());

            // Assert
            order.SalesTax.ShouldBe(30.12M);
        }

        [Fact]
        public void Test_CalculateTotals_for_FreightAndMaterialTotal_should_not_round_rates()
        {
            // Arrange
            var order = GetSampleOrder();
            order.SalesTaxRate = 23.678M;

            var orderLines = new List<OrderLineTaxDetailsDto>
            {
                new OrderLineTaxDetailsDto { FreightPrice = 1936.2M, IsTaxable = true }
            };
            
            // Act
            OrderTaxCalculator.CalculateTotals(TaxCalculationType.FreightAndMaterialTotal, order, orderLines);

            // Assert
            //specific for this calculation type
            order.SalesTax.ShouldBe(458.45M);
            //order.CODTotal.ShouldBe(1406.5M);

            ////common for all types of calculation
            //order.FreightTotal.ShouldBe(630);
            //order.MaterialTotal.ShouldBe(620);

            ////should remain unchanged
            //order.Id.ShouldBe(1);
            //order.SalesTaxRate.ShouldBe(10);
        }
    }
}
