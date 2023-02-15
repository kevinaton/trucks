using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Features;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Domain.Uow;
using Abp.Events.Bus;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.Runtime.Security;
using Microsoft.EntityFrameworkCore;
using DispatcherWeb.Authorization;
using DispatcherWeb.Editions.Dto;
using DispatcherWeb.MultiTenancy.Dto;
using DispatcherWeb.Url;
using Abp.Domain.Repositories;
using DispatcherWeb.Drivers;
using DispatcherWeb.PayStatements;
using DispatcherWeb.LeaseHaulerStatements;
using DispatcherWeb.Orders;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Invoices;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Emailing;
using DispatcherWeb.Payments;
using DispatcherWeb.Sms;
using DispatcherWeb.TimeOffs;

namespace DispatcherWeb.MultiTenancy
{
    [AbpAuthorize(AppPermissions.Pages_Tenants)]
    public class TenantAppService : DispatcherWebAppServiceBase, ITenantAppService
    {
        public IAppUrlService AppUrlService { get; set; }
        public IEventBus EventBus { get; set; }
        private readonly IRepository<Driver> _driverRepository;
        private readonly IRepository<EmployeeTimePayStatementTime> _employeeTimePayStatementTimeRepository;
        private readonly IRepository<Drivers.EmployeeTime> _employeeTimeRepository;
        private readonly IRepository<PayStatement> _payStatementRepository;
        private readonly IRepository<PayStatementDetail> _payStatementDetailRepository;
        private readonly IRepository<PayStatementDriverDateConflict> _payStatementDriverDateConflictRepository;
        private readonly IRepository<PayStatementTime> _payStatementTimeRepository;
        private readonly IRepository<PayStatementTicket> _payStatementTicketRepository;
        private readonly IRepository<LeaseHaulerStatementTicket> _leaseHaulerStatementTicketRepository;
        private readonly IRepository<LeaseHaulerStatement> _leaseHaulerStatementRepository;
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IRepository<Load> _loadRepository;
        private readonly IRepository<InvoiceUploadBatch> _invoiceUploadBatchRepository;
        private readonly IRepository<InvoiceEmail> _invoiceEmailRepository;
        private readonly IRepository<InvoiceLine> _invoiceLineRepository;
        private readonly IRepository<Invoice> _invoiceRepository;
        private readonly IRepository<InvoiceBatch> _invoiceBatchRepository;
        private readonly IRepository<Dispatch> _dispatchRepository;
        private readonly IRepository<OrderLineTruck> _orderLineTruckRepository;
        private readonly IRepository<OrderLineOfficeAmount> _orderLineOfficeAmountRepository;
        private readonly IRepository<Payment> _paymentsRepository;
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly IRepository<OrderEmail> _orderEmailRepository;
        private readonly IRepository<OrderTruck> _orderTruckRepository;
        private readonly IRepository<OrderPayment> _orderPaymentRepository;
        private readonly IRepository<ReceiptLine> _receiptLineRepository;
        private readonly IRepository<Receipt> _receiptRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<DriverMessage> _driverMessageRepository;
        private readonly IRepository<SentSms> _sentSmsRepository;
        private readonly IRepository<TimeOff> _timeOffRepository;
        private readonly IRepository<DriverAssignment> _driverAssignmentRepository;

        public TenantAppService(
            IRepository<Driver> driverRepository,
            IRepository<EmployeeTimePayStatementTime> employeeTimePayStatementTimeRepository,
            IRepository<Drivers.EmployeeTime> employeeTimeRepository,
            IRepository<PayStatement> payStatementRepository,
            IRepository<PayStatementDetail> payStatementDetailRepository,
            IRepository<PayStatementDriverDateConflict> payStatementDriverDateConflictRepository,
            IRepository<PayStatementTime> payStatementTimeRepository,
            IRepository<PayStatementTicket> payStatementTicketRepository,
            IRepository<LeaseHaulerStatementTicket> leaseHaulerStatementTicketRepository,
            IRepository<LeaseHaulerStatement> leaseHaulerStatementRepository,
            IRepository<Ticket> ticketRepository,
            IRepository<Load> loadRepository,
            IRepository<InvoiceUploadBatch> invoiceUploadBatchRepository,
            IRepository<InvoiceEmail> invoiceEmailRepository,
            IRepository<InvoiceLine> invoiceLineRepository,
            IRepository<Invoice> invoiceRepository,
            IRepository<InvoiceBatch> invoiceBatchRepository,
            IRepository<Dispatch> dispatchRepository,
            IRepository<OrderLineTruck> orderLineTruckRepository,
            IRepository<OrderLineOfficeAmount> orderLineOfficeAmountRepository,
            IRepository<Payment> paymentsRepository,
            IRepository<OrderLine> orderLineRepository,
            IRepository<OrderEmail> orderEmailRepository,
            IRepository<OrderTruck> orderTruckRepository,
            IRepository<OrderPayment> orderPaymentRepository,
            IRepository<ReceiptLine> receiptLineRepository,
            IRepository<Receipt> receiptRepository,
            IRepository<Order> orderRepository,
            IRepository<DriverMessage> driverMessageRepository,
            IRepository<SentSms> sentSmsRepository,
            IRepository<TimeOff> timeOffRepository,
            IRepository<DriverAssignment> driverAssignmentRepository
        )
        {
            _driverRepository = driverRepository;
            _employeeTimePayStatementTimeRepository = employeeTimePayStatementTimeRepository;
            _employeeTimeRepository = employeeTimeRepository;
            _payStatementRepository = payStatementRepository;
            _payStatementDetailRepository = payStatementDetailRepository;
            _payStatementDriverDateConflictRepository = payStatementDriverDateConflictRepository;
            _payStatementTimeRepository = payStatementTimeRepository;
            _payStatementTicketRepository = payStatementTicketRepository;
            _leaseHaulerStatementTicketRepository = leaseHaulerStatementTicketRepository;
            _leaseHaulerStatementRepository = leaseHaulerStatementRepository;
            _ticketRepository = ticketRepository;
            _loadRepository = loadRepository;
            _invoiceUploadBatchRepository = invoiceUploadBatchRepository;
            _invoiceEmailRepository = invoiceEmailRepository;
            _invoiceLineRepository = invoiceLineRepository;
            _invoiceRepository = invoiceRepository;
            _invoiceBatchRepository = invoiceBatchRepository;
            _dispatchRepository = dispatchRepository;
            _orderLineTruckRepository = orderLineTruckRepository;
            _orderLineOfficeAmountRepository = orderLineOfficeAmountRepository;
            _paymentsRepository = paymentsRepository;
            _orderLineRepository = orderLineRepository;
            _orderEmailRepository = orderEmailRepository;
            _orderTruckRepository = orderTruckRepository;
            _orderPaymentRepository = orderPaymentRepository;
            _receiptLineRepository = receiptLineRepository;
            _receiptRepository = receiptRepository;
            _orderRepository = orderRepository;
            _driverMessageRepository = driverMessageRepository;
            _sentSmsRepository = sentSmsRepository;
            _timeOffRepository = timeOffRepository;
            _driverAssignmentRepository = driverAssignmentRepository;
            AppUrlService = NullAppUrlService.Instance;
            EventBus = NullEventBus.Instance;
        }

        public async Task<PagedResultDto<TenantListDto>> GetTenants(GetTenantsInput input)
        {
            var query = TenantManager.Tenants
                .Include(t => t.Edition)
                .WhereIf(!input.Filter.IsNullOrWhiteSpace(), t => t.Name.Contains(input.Filter) || t.TenancyName.Contains(input.Filter))
                .WhereIf(input.CreationDateStart.HasValue, t => t.CreationTime >= input.CreationDateStart.Value)
                .WhereIf(input.CreationDateEnd.HasValue, t => t.CreationTime <= input.CreationDateEnd.Value)
                .WhereIf(input.SubscriptionEndDateStart.HasValue, t => t.SubscriptionEndDateUtc >= input.SubscriptionEndDateStart.Value.ToUniversalTime())
                .WhereIf(input.SubscriptionEndDateEnd.HasValue, t => t.SubscriptionEndDateUtc <= input.SubscriptionEndDateEnd.Value.ToUniversalTime())
                .WhereIf(input.EditionIdSpecified, t => t.EditionId == input.EditionId)
                .WhereIf(input.Status == FilterActiveStatus.Active, x => x.IsActive)
                .WhereIf(input.Status == FilterActiveStatus.Inactive, x => !x.IsActive);

            var tenantCount = await query.CountAsync();
            var tenants = await query.OrderBy(input.Sorting).PageBy(input).ToListAsync();

            //to do: Replace/remove automapper implementation.
            return new PagedResultDto<TenantListDto>(
                tenantCount,
                ObjectMapper.Map<List<TenantListDto>>(tenants)
                );
        }

        [AbpAuthorize(AppPermissions.Pages_Tenants_Create)]
        [UnitOfWork(IsDisabled = true)]
        public async Task CreateTenant(CreateTenantInput input)
        {
            await TenantManager.CreateWithAdminUserAsync(
                input.CompanyName,
                input.AdminFirstName,
                input.AdminLastName,
                input.AdminPassword,
                input.AdminEmailAddress,
                input.ConnectionString,
                input.IsActive,
                input.EditionId,
                input.ShouldChangePasswordOnNextLogin,
                input.SendActivationEmail,
                input.SubscriptionEndDateUtc?.ToUniversalTime(),
                input.IsInTrialPeriod,
                AppUrlService.CreateEmailActivationUrlFormat
            );
        }

        [AbpAuthorize(AppPermissions.Pages_Tenants_Edit)]
        public async Task<TenantEditDto> GetTenantForEdit(EntityDto input)
        {
            var tenantEditDto = ObjectMapper.Map<TenantEditDto>(await TenantManager.GetByIdAsync(input.Id));
            tenantEditDto.ConnectionString = SimpleStringCipher.Instance.Decrypt(tenantEditDto.ConnectionString);
            return tenantEditDto;
        }

        [AbpAuthorize(AppPermissions.Pages_Tenants_Edit)]
        public async Task UpdateTenant(TenantEditDto input)
        {
            await TenantManager.CheckEditionAsync(input.EditionId, input.IsInTrialPeriod);

            input.ConnectionString = SimpleStringCipher.Instance.Encrypt(input.ConnectionString);
            var tenant = await TenantManager.GetByIdAsync(input.Id);
             if (tenant.EditionId != input.EditionId)
            {
                await EventBus.TriggerAsync(new TenantEditionChangedEventData
                {
                    TenantId = input.Id,
                    OldEditionId = tenant.EditionId,
                    NewEditionId = input.EditionId
                });
            }
           ObjectMapper.Map(input, tenant);
            tenant.SubscriptionEndDateUtc = tenant.SubscriptionEndDateUtc?.ToUniversalTime();

            await TenantManager.UpdateAsync(tenant);
        }

        [AbpAuthorize(AppPermissions.Pages_Tenants_Delete)]
        public async Task DeleteTenant(EntityDto input)
        {
            var tenant = await TenantManager.GetByIdAsync(input.Id);
            await TenantManager.DeleteAsync(tenant);
        }

        [AbpAuthorize(AppPermissions.Pages_Tenants_DeleteDispatchData)]
        public async Task DeleteDispatchDataForTenant(EntityDto input)
        {
            using (CurrentUnitOfWork.SetTenantId(input.Id))
            {
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
                {
                    await _employeeTimePayStatementTimeRepository.DeleteInBatchesAsync(x => true, CurrentUnitOfWork);
                    await _employeeTimeRepository.HardDeleteInBatchesAsync(x => true, CurrentUnitOfWork);
                    await _payStatementTimeRepository.DeleteInBatchesAsync(x => true, CurrentUnitOfWork);
                    await _payStatementTicketRepository.DeleteInBatchesAsync(x => true, CurrentUnitOfWork);
                    await _payStatementDetailRepository.DeleteInBatchesAsync(x => true, CurrentUnitOfWork);
                    await _payStatementDriverDateConflictRepository.DeleteInBatchesAsync(x => true, CurrentUnitOfWork);
                    await _payStatementRepository.HardDeleteInBatchesAsync(x => true, CurrentUnitOfWork);
                    await _leaseHaulerStatementTicketRepository.DeleteInBatchesAsync(x => true, CurrentUnitOfWork);
                    await _leaseHaulerStatementRepository.HardDeleteInBatchesAsync(x => true, CurrentUnitOfWork);
                    await _ticketRepository.HardDeleteInBatchesAsync(x => true, CurrentUnitOfWork);
                    await _loadRepository.HardDeleteInBatchesAsync(x => true, CurrentUnitOfWork);
                    await _invoiceUploadBatchRepository.HardDeleteInBatchesAsync(x => true, CurrentUnitOfWork);
                    await _invoiceEmailRepository.DeleteInBatchesAsync(x => x.Invoice.TenantId == input.Id, CurrentUnitOfWork);
                    await _invoiceLineRepository.HardDeleteInBatchesAsync(x => true, CurrentUnitOfWork);
                    await _invoiceRepository.HardDeleteInBatchesAsync(x => true, CurrentUnitOfWork);
                    await _invoiceBatchRepository.HardDeleteInBatchesAsync(x => true, CurrentUnitOfWork);
                    await _dispatchRepository.HardDeleteInBatchesAsync(x => true, CurrentUnitOfWork);
                    await _orderLineTruckRepository.HardDeleteInBatchesAsync(x => true, CurrentUnitOfWork);
                    await _orderLineOfficeAmountRepository.HardDeleteInBatchesAsync(x => true, CurrentUnitOfWork);
                    await _orderPaymentRepository.HardDeleteInBatchesAsync(x => true, CurrentUnitOfWork);
                    await _paymentsRepository.HardDeleteInBatchesAsync(x => true, CurrentUnitOfWork);
                    await _orderLineRepository.HardDeleteInBatchesAsync(x => true, CurrentUnitOfWork);
                    await _orderEmailRepository.DeleteInBatchesAsync(x => x.Order.TenantId == input.Id, CurrentUnitOfWork);
                    await _orderTruckRepository.HardDeleteInBatchesAsync(x => true, CurrentUnitOfWork);
                    await _receiptLineRepository.HardDeleteInBatchesAsync(x => true, CurrentUnitOfWork);
                    await _receiptRepository.HardDeleteInBatchesAsync(x => true, CurrentUnitOfWork);
                    await _orderRepository.HardDeleteInBatchesAsync(x => true, CurrentUnitOfWork);
                    await _driverMessageRepository.HardDeleteInBatchesAsync(x => true, CurrentUnitOfWork);
                    await _sentSmsRepository.DeleteInBatchesAsync(x => true, CurrentUnitOfWork);
                    await _timeOffRepository.HardDeleteInBatchesAsync(x => true, CurrentUnitOfWork);
                    await _driverAssignmentRepository.HardDeleteInBatchesAsync(x => true, CurrentUnitOfWork);
                }
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Tenants_ChangeFeatures)]
        public async Task<GetTenantFeaturesEditOutput> GetTenantFeaturesForEdit(EntityDto input)
        {
            var features = FeatureManager.GetAll()
                .Where(f => f.Scope.HasFlag(FeatureScopes.Tenant));
            var featureValues = await TenantManager.GetFeatureValuesAsync(input.Id);

            return new GetTenantFeaturesEditOutput
            {
                Features = ObjectMapper.Map<List<FlatFeatureDto>>(features).OrderBy(f => f.DisplayName).ToList(),
                FeatureValues = featureValues.Select(fv => new NameValueDto(fv)).ToList()
            };
        }

        [AbpAuthorize(AppPermissions.Pages_Tenants_ChangeFeatures)]
        public async Task UpdateTenantFeatures(UpdateTenantFeaturesInput input)
        {
            await TenantManager.SetFeatureValuesAsync(input.Id, input.FeatureValues.Select(fv => new NameValue(fv.Name, fv.Value)).ToArray());
        }

        [AbpAuthorize(AppPermissions.Pages_Tenants_ChangeFeatures)]
        public async Task ResetTenantSpecificFeatures(EntityDto input)
        {
            await TenantManager.ResetAllFeaturesAsync(input.Id);
        }

        public async Task UnlockTenantAdmin(EntityDto input)
        {
            using (CurrentUnitOfWork.SetTenantId(input.Id))
            {
                var tenantAdmin = await UserManager.GetAdminAsync();
                if (tenantAdmin != null)
                {
                    tenantAdmin.Unlock();
                }
            }
        }

        public async Task AddMonthToDriverDOTRequirements(EntityDto input)
        {            
            var driversQuery = _driverRepository.GetAll()
                .Where(x => !x.IsExternal && x.TenantId == input.Id);
            var drivers = await driversQuery.ToListAsync();
            foreach (var item in drivers)
            {
                if (item.LicenseExpirationDate.HasValue)
                    item.LicenseExpirationDate = item.LicenseExpirationDate.Value.AddMonths(1);

                if (item.LastPhysicalDate.HasValue)
                    item.LastPhysicalDate = item.LastPhysicalDate.Value.AddMonths(1);

                if (item.NextPhysicalDueDate.HasValue)
                    item.NextPhysicalDueDate = item.NextPhysicalDueDate.Value.AddMonths(1);

                if (item.LastMvrDate.HasValue)
                    item.LastMvrDate = item.LastMvrDate.Value.AddMonths(1);

                if (item.NextMvrDueDate.HasValue)
                    item.NextMvrDueDate = item.NextMvrDueDate.Value.AddMonths(1);

                await _driverRepository.UpdateAsync(item);
            }
            
        }
    }
}
