using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using DispatcherWeb.Configuration;
using DispatcherWeb.Exceptions;
using DispatcherWeb.OrderPayments.Dto;
using DispatcherWeb.Orders;
using DispatcherWeb.Orders.TaxDetails;
using DispatcherWeb.Payments;
using DispatcherWeb.Payments.Dto;
using GlobalPayments.Api.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace DispatcherWeb.OrderPayments
{
    [AbpAuthorize]
    public class OrderPaymentAppService : DispatcherWebAppServiceBase, IOrderPaymentAppService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfigurationRoot _appConfiguration;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderPayment> _orderPaymentRepository;
        private readonly IRepository<Receipt> _receiptRepository;
        private readonly IPaymentAppService _paymentAppService;
        private readonly IOfficeSettingsManager _officeSettingsManager;
        private readonly OrderTaxCalculator _orderTaxCalculator;

        public OrderPaymentAppService(
            IWebHostEnvironment hostingEnvironment,
            IRepository<Order> orderRepository,
            IRepository<OrderPayment> orderPaymentRepository,
            IRepository<Receipt> receiptRepository,
            IPaymentAppService paymentAppService,
            IOfficeSettingsManager officeSettingsManager,
            OrderTaxCalculator orderTaxCalculator
            )
        {
            _env = hostingEnvironment;
            _appConfiguration = _env.GetAppConfiguration();
            _orderRepository = orderRepository;
            _orderPaymentRepository = orderPaymentRepository;
            _receiptRepository = receiptRepository;
            _paymentAppService = paymentAppService;
            _officeSettingsManager = officeSettingsManager;
            _orderTaxCalculator = orderTaxCalculator;
        }

        public async Task<AuthorizeOrderChargeDto> GetAuthorizeOrderChargeModel(EntityDto input)
        {
            var order = await _orderRepository.GetAll()
                .Include(x => x.OrderLines)
                .Where(x => x.Id == input.Id)
                .Select(x => new
                {
                    Customer = new
                    {
                        x.Customer.CreditCardToken,
                        x.Customer.CreditCardStreetAddress,
                        x.Customer.CreditCardZipCode
                    },
                    x.SalesTax,
                    x.SalesTaxRate,
                    //x.CODTotal,
                    OfficeId = x.LocationId,
                    SharedOffices = x.SharedOrders.Select(o => o.OfficeId).ToList(),
                    OrderLines = x.OrderLines.Select(o => new OrderLineTaxDetailsDto
                    {
                        FreightPrice = o.FreightPrice,
                        MaterialPrice = o.MaterialPrice,
                        IsTaxable = o.Service.IsTaxable
                    }).ToList()
                }).FirstAsync();

            if (order.OfficeId != OfficeId && !order.SharedOffices.Contains(OfficeId))
            {
                throw new UserFriendlyException("You can't authorize a payment for an order which is not shared with your office", "");
            }

            var orderTaxDetails = new OrderTaxDetailsDto
            {
                SalesTax = order.SalesTax,
                SalesTaxRate = order.SalesTaxRate
            };

            await _orderTaxCalculator.CalculateTotalsAsync(orderTaxDetails, order.OrderLines);

            return new AuthorizeOrderChargeDto
            {
                OrderId = input.Id,
                AuthorizationAmount = orderTaxDetails.CODTotal,
                CreditCardToken = order.Customer.CreditCardToken,
                StreetAddress = order.Customer.CreditCardStreetAddress,
                ZipCode = order.Customer.CreditCardZipCode
            };
        }

        public async Task<CaptureOrderAuthorizationDto> GetCaptureOrderAuthorizationModel(EntityDto input)
        {
            var receipt = await _receiptRepository.GetAsync(input.Id);

            if (receipt == null)
            {
                throw new UserFriendlyException("Receipt wasn't found", "Please refresh the page and try again.");
            }

            var order = await _receiptRepository.GetAll()
                .Where(x => x.Id == receipt.Id)
                .Select(x => new
                {
                    Id = x.OrderId,
                    OfficeId = x.Order.LocationId,
                    SharedOffices = x.Order.SharedOrders.Select(o => o.OfficeId).ToList()
                }).FirstOrDefaultAsync();

            if (order == null)
            {
                throw await GetOrderNotFoundException(input);
            }

            if (order.OfficeId != OfficeId && !order.SharedOffices.Contains(OfficeId))
            {
                throw new UserFriendlyException("You can't capture payment authorization for an order which is not shared with your office", "");
            }

            var authorizationPayment = await _orderPaymentRepository.GetAll()
                .Where(x => x.OrderId == order.Id && x.OfficeId == OfficeId)
                .Select(x => x.Payment)
                .FirstOrDefaultAsync(x => !x.IsCancelledOrRefunded && x.AuthorizationTransactionId != null);

            if (authorizationPayment == null || authorizationPayment.AuthorizationAmount == null)
            {
                throw new UserFriendlyException("Order is not authorized", "Please refresh the page and try again.");
            }

            return new CaptureOrderAuthorizationDto
            {
                ReceiptId = input.Id,
                AuthorizationAmount = authorizationPayment.AuthorizationAmount.Value
            };
        }

        public async Task<AuthorizeOrderChargeResult> AuthorizeOrderCharge(AuthorizeOrderChargeDto input)
        {
            Logger.Info("Entered AuthorizeOrderCharge with input: " + SerializeToJson(input));

            var order = await _orderRepository.GetAll()
                .Include(x => x.Customer)
                .Include(x => x.Office)
                .Include(x => x.SharedOrders)
                .Where(x => x.Id == input.OrderId)
                .FirstAsync();

            if (order.LocationId != OfficeId && !order.SharedOrders.Any(s => s.OfficeId == OfficeId))
            {
                throw new UserFriendlyException("You can't authorize a payment for an order which is not shared with your office", "");
            }

            var orderHasPreviousAuthorization = await _orderPaymentRepository.GetAll()
                .Where(x => x.OrderId == order.Id && x.OfficeId == OfficeId)
                .Select(x => x.Payment)
                .AnyAsync(x => !x.IsCancelledOrRefunded && x.AuthorizationTransactionId != null);

            if (orderHasPreviousAuthorization)
            {
                throw new UserFriendlyException("Order is already authorized", "Please refresh the page and try again.");
            }

            input.PaymentDescription = $"OrderId: {order.Id} for {order.Office?.Name}, Customer: {order.Customer.Name}";
            var result = await _paymentAppService.AuthorizeCharge(input);

            if (input.SaveCreditCardForFutureUse)
            {
                order.Customer.CreditCardToken = input.CreditCardToken;
                order.Customer.CreditCardStreetAddress = input.StreetAddress;
                order.Customer.CreditCardZipCode = input.ZipCode;
            }

            await _orderPaymentRepository.InsertAsync(new OrderPayment
            {
                OrderId = order.Id,
                PaymentId = result.PaymentId,
                TenantId = Session.TenantId ?? 0,
                OfficeId = OfficeId
            });

            return new AuthorizeOrderChargeResult
            {
                AuthorizationDateTime = result.AuthorizationDateTime
            };
        }

        public async Task CancelOrderAuthorization(EntityDto input)
        {
            var order = await _orderRepository.GetAll()
                .Include(x => x.Customer)
                .Include(x => x.Office)
                .Include(x => x.SharedOrders)
                .Where(x => x.Id == input.Id)
                .FirstAsync();

            if (order.LocationId != OfficeId && !order.SharedOrders.Any(s => s.OfficeId == OfficeId))
            {
                throw new UserFriendlyException("You can't cancel payment authorization for an order which is not shared with your office", "");
            }

            var orderPayment = await _orderPaymentRepository.GetAll()
                .Include(x => x.Payment)
                .Where(x => x.OrderId == order.Id && x.OfficeId == OfficeId)
                .FirstOrDefaultAsync(x => !x.Payment.IsCancelledOrRefunded && x.Payment.AuthorizationTransactionId != null);

            var authorizationPayment = orderPayment?.Payment;

            if (authorizationPayment == null)
            {
                throw new UserFriendlyException("Order is not authorized", "Please refresh the page and try again.");
            }

            await _paymentAppService.CancelAuthorization(new EntityDto(authorizationPayment.Id));
        }

        public async Task<CaptureAuthorizationResult> CaptureOrderAuthorization(CaptureOrderAuthorizationDto input)
        {
            var receipt = await _receiptRepository.GetAsync(input.ReceiptId);

            if (receipt == null)
            {
                throw new UserFriendlyException("Receipt wasn't found", "Please refresh the page and try again.");
            }

            var order = await _orderRepository.GetAll()
                .Include(x => x.Customer)
                .Include(x => x.Office)
                .Include(x => x.SharedOrders)
                .Where(x => x.Id == receipt.OrderId)
                .FirstAsync();

            if (receipt.OfficeId != OfficeId)
            {
                throw new UserFriendlyException("You can't capture payment authorization for a receipt which is not assigned to your office", "");
            }

            if (order.LocationId != OfficeId && !order.SharedOrders.Any(s => s.OfficeId == OfficeId))
            {
                throw new UserFriendlyException("You can't capture payment authorization for an order which is not shared with your office", "");
            }

            var orderPayment = await _orderPaymentRepository.GetAll()
                .Include(x => x.Payment)
                .Where(x => x.OrderId == order.Id && x.OfficeId == OfficeId)
                .FirstOrDefaultAsync(x => !x.Payment.IsCancelledOrRefunded && x.Payment.AuthorizationTransactionId != null);

            var authorizationPayment = orderPayment?.Payment;

            if (authorizationPayment == null)
            {
                throw new UserFriendlyException("Order is not authorized", "Please refresh the page and try again.");
            }

            orderPayment.ReceiptId = input.ReceiptId;

            return await _paymentAppService.CaptureAuthorization(new CaptureAuthorizationDto
            {
                PaymentId = authorizationPayment.Id,
                ActualAmount = input.ActualAmount
            });
        }

        public async Task RefundOrderPayment(EntityDto input)
        {
            var order = await _orderRepository.GetAll()
                .Include(x => x.Customer)
                .Include(x => x.Office)
                .Include(x => x.SharedOrders)
                .Where(x => x.Id == input.Id)
                .FirstAsync();

            if (order.LocationId != OfficeId && !order.SharedOrders.Any(s => s.OfficeId == OfficeId))
            {
                throw new UserFriendlyException("You can't refund a payment for an order which is not shared with your office", "");
            }

            var orderPayment = await _orderPaymentRepository.GetAll()
                .Include(x => x.Receipt)
                .Include(x => x.Payment)
                .Where(x => x.OrderId == order.Id && x.OfficeId == OfficeId)
                .FirstOrDefaultAsync(x => !x.Payment.IsCancelledOrRefunded && x.Payment.AuthorizationCaptureTransactionId != null);

            var payment = orderPayment?.Payment;

            if (payment == null)
            {
                throw new UserFriendlyException("You can't refund a payment unless the payment was finalized", "Please refresh the page and try again.");
            }

            if (orderPayment.Receipt == null)
            {
                throw new UserFriendlyException("Receipt wasn't found", "Please refresh the page and try again.");
            }

            if (orderPayment.Receipt.OfficeId != OfficeId)
            {
                throw new UserFriendlyException("You can't refund a payment for a receipt which is not assigned to your office", "");
            }

            await _paymentAppService.RefundPayment(new EntityDto(payment.Id));
        }

        private async Task<Exception> GetOrderNotFoundException(EntityDto input)
        {
            if (await _orderRepository.IsEntityDeleted(input, CurrentUnitOfWork))
            {
                return new EntityDeletedException("Order", "This order has been deleted and can’t be edited");
            }

            return new Exception($"Order with id {input.Id} wasn't found and is not deleted");
        }

        private string SerializeToJson(ApiException e)
        {
            if (e is GatewayException ge)
            {
                return SerializeToJson(new
                {
                    ge.ResponseCode,
                    ge.ResponseMessage,
                    ge.Message,
                    InnerExceptionMessage = ge.InnerException?.Message
                });
            }

            return SerializeToJson(new
            {
                e.Message,
                InnerExceptionMessage = e.InnerException?.Message
            });
        }

        private string SerializeToJson(object value)
        {
            return JsonConvert.SerializeObject(value, Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            });
        }
    }
}
