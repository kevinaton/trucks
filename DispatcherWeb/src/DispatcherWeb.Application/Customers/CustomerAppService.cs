using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Castle.Core.Internal;
using DispatcherWeb.Authorization;
using DispatcherWeb.Customers.Dto;
using DispatcherWeb.Customers.Exporting;
using DispatcherWeb.Dto;
using DispatcherWeb.Features;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Orders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Customers
{
    [AbpAuthorize]
    public class CustomerAppService : DispatcherWebAppServiceBase, ICustomerAppService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IRepository<CustomerContact> _customerContactRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly ICustomerListCsvExporter _customerListCsvExporter;
        private readonly ICustomerContactUserLinkService _customerContactUserLinkService;

        public CustomerAppService(
            ICustomerRepository customerRepository,
            IRepository<CustomerContact> customerContactRepository,
            IRepository<Order> orderRepository,
            ICustomerListCsvExporter customerListCsvExporter,
            ICustomerContactUserLinkService customerContactUserLinkService
            )
        {
            _customerRepository = customerRepository;
            _customerContactRepository = customerContactRepository;
            _orderRepository = orderRepository;
            _customerListCsvExporter = customerListCsvExporter;
            _customerContactUserLinkService = customerContactUserLinkService;
        }

        [AbpAuthorize(AppPermissions.Pages_Customers)]
        public async Task<PagedResultDto<CustomerDto>> GetCustomers(GetCustomersInput input)
        {
            var query = GetFilteredCustomerQuery(input);

            var totalCount = await query.CountAsync();

            var items = await GetCustomerDtoQuery(query)
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<CustomerDto>(
                totalCount,
                items);
        }

        private IQueryable<Customer> GetFilteredCustomerQuery(IGetCustomerListFilter input) =>
            _customerRepository.GetAll()
                .WhereIf(!input.Name.IsNullOrEmpty(), x => x.Name.StartsWith(input.Name))
                .WhereIf(input.Status == FilterActiveStatus.Active, x => x.IsActive)
                .WhereIf(input.Status == FilterActiveStatus.Inactive, x => !x.IsActive);

        private IQueryable<CustomerDto> GetCustomerDtoQuery(IQueryable<Customer> query) =>
            query.Select(x => new CustomerDto
            {
                Id = x.Id,
                Name = x.Name,
                AccountNumber = x.AccountNumber,
                Address1 = x.Address1,
                Address2 = x.Address2,
                City = x.City,
                State = x.State,
                ZipCode = x.ZipCode,
                CountryCode = x.CountryCode,
                IsActive = x.IsActive
            });

        [AbpAuthorize(AppPermissions.Pages_Customers)]
        [HttpPost]
        public async Task<FileDto> GetCustomersToCsv(GetCustomersInput input)
        {
            var query = GetFilteredCustomerQuery(input);
            var items = await GetCustomerDtoQuery(query)
                .OrderBy(input.Sorting)
                .ToListAsync();

            if (!items.Any())
            {
                throw new UserFriendlyException(L("ThereIsNoDataToExport"));
            }

            return _customerListCsvExporter.ExportToFile(items);
        }

        public async Task<PagedResultDto<SelectListDto>> GetActiveCustomersSelectList(GetActiveCustomersSelectListInput input)
        {
            var query = _customerRepository.GetAll()
                .Where(x => x.IsActive
                    || input.IncludeInactiveWithInvoices && x.Invoices.Any())
                .Select(x => new SelectListDto<CustomerSelectListInfoDto>
                {
                    Id = x.Id.ToString(),
                    Name = x.Name,
                    Item = new CustomerSelectListInfoDto
                    {
                        AccountNumber = x.AccountNumber,
                        CustomerIsCod = x.IsCod,
                    }
                });

            return await query.GetSelectListResult(input);
        }

        public async Task<PagedResultDto<SelectListDto>> GetCustomersSelectList(GetSelectListInput input)
        {
            var query = _customerRepository.GetAll()
                .Select(x => new SelectListDto<CustomerSelectListInfoDto>
                {
                    Id = x.Id.ToString(),
                    Name = x.Name,
                });

            return await query.GetSelectListResult(input);
        }

        public async Task<PagedResultDto<SelectListDto>> GetCustomersWithOrdersSelectList(GetCustomersWithOrdersSelectListInput input)
        {
            var query = _customerRepository.GetAll()
                .Where(c =>
                    c.Orders.Any(o =>
                        (!input.DateBegin.HasValue || o.DeliveryDate >= input.DateBegin) &&
                        (!input.DateEnd.HasValue || o.DeliveryDate <= input.DateEnd)
                    )
                )
                .Select(c => new SelectListDto
                {
                    Id = c.Id.ToString(),
                    Name = c.Name,
                });

            return await query.GetSelectListResult(input);
        }

        [HttpPost]
        public async Task<ListResultDto<SelectListDto>> GetCustomersByIdsSelectList(GetItemsByIdsInput input)
        {
            var items = await _customerRepository.GetAll()
                .Where(x => input.Ids.Contains(x.Id))
                .Select(x => new SelectListDto
                {
                    Id = x.Id.ToString(),
                    Name = x.Name
                })
                .OrderBy(x => x.Name)
                .ToListAsync();

            return new ListResultDto<SelectListDto>(items);
        }

        [AbpAuthorize(AppPermissions.Pages_Customers)]
        public async Task<CustomerEditDto> GetCustomerForEdit(NullableIdNameDto input)
        {
            CustomerEditDto customerEditDto;

            if (input.Id.HasValue)
            {
                var customer = await _customerRepository.GetAsync(input.Id.Value);
                customerEditDto = new CustomerEditDto
                {
                    Id = customer.Id,
                    Name = customer.Name,
                    AccountNumber = customer.AccountNumber,
                    CustomerIsCod = customer.IsCod,
                    Address1 = customer.Address1,
                    Address2 = customer.Address2,
                    City = customer.City,
                    State = customer.State,
                    ZipCode = customer.ZipCode,
                    CountryCode = customer.CountryCode,
                    BillingAddress1 = customer.BillingAddress1,
                    BillingAddress2 = customer.BillingAddress2,
                    BillingCity = customer.BillingCity,
                    BillingCountryCode = customer.BillingCountryCode,
                    BillingState = customer.BillingState,
                    BillingZipCode = customer.BillingZipCode,
                    InvoiceEmail = customer.InvoiceEmail,
                    PreferredDeliveryMethod = customer.PreferredDeliveryMethod,
                    Terms = customer.Terms,
                    IsActive = customer.IsActive,
                    InvoicingMethod = customer.InvoicingMethod
                };
            }
            else
            {
                customerEditDto = new CustomerEditDto
                {
                    Name = input.Name,
                    IsActive = true
                };
            }

            return customerEditDto;
        }

        [AbpAuthorize(AppPermissions.Pages_Customers)]
        public async Task<CustomerEditDto> EditCustomer(CustomerEditDto model)
        {
            Customer entity = model.Id.HasValue ?
                await _customerRepository.GetAsync(model.Id.Value) :
                new Customer();

            entity.Name = model.Name;
            entity.AccountNumber = model.AccountNumber;
            entity.IsCod = model.CustomerIsCod;
            entity.Address1 = model.Address1;
            entity.Address2 = model.Address2;
            entity.City = model.City;
            entity.State = model.State;
            entity.ZipCode = model.ZipCode;
            entity.CountryCode = model.CountryCode;
            entity.BillingAddress1 = model.BillingAddress1;
            entity.BillingAddress2 = model.BillingAddress2;
            entity.BillingCity = model.BillingCity;
            entity.BillingCountryCode = model.BillingCountryCode;
            entity.BillingState = model.BillingState;
            entity.BillingZipCode = model.BillingZipCode;
            entity.InvoiceEmail = model.InvoiceEmail;
            entity.PreferredDeliveryMethod = model.PreferredDeliveryMethod;
            entity.Terms = model.Terms;
            entity.InvoicingMethod = model.InvoicingMethod;
            entity.IsActive = model.IsActive;


            model.Id = await _customerRepository.InsertOrUpdateAndGetIdAsync(entity);
            return model;
        }

        [AbpAuthorize(AppPermissions.Pages_Customers)]
        public async Task<CustomerEditDto> GetCustomerIfExistsOrNull(GetCustomerIdIfExistsOrNullInput input)
        {
            var customer = await _customerRepository.GetAll()
                .Where(c => c.Name == input.Name)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.AccountNumber,
                    c.IsCod
                })
                .FirstOrDefaultAsync();
            return customer == null ? null : new CustomerEditDto()
            {
                Id = customer.Id,
                Name = customer.Name,
                AccountNumber = customer.AccountNumber,
                CustomerIsCod = customer.IsCod,
            };
        }

        public async Task<bool> CanDeleteCustomer(EntityDto input)
        {
            var record = await _customerRepository.GetAll()
                .Where(x => x.Id == input.Id)
                .Select(x => new
                {
                    HasContacts = x.CustomerContacts.Any()
                })
                .SingleAsync();

            if (record.HasContacts)
            {
                return false;
            }

            var hasOrders = await _orderRepository.GetAll().Where(x => x.CustomerId == input.Id).AnyAsync();
            if (hasOrders)
            {
                return false;
            }

            return true;
        }

        [AbpAuthorize(AppPermissions.Pages_Customers)]
        public async Task DeleteCustomer(EntityDto input)
        {
            var canDelete = await CanDeleteCustomer(input);
            if (!canDelete)
            {
                throw new UserFriendlyException("You can't delete selected row because it has data associated with it.");
            }
            await _customerRepository.DeleteAsync(input.Id);
        }

        //*************************************************//

        [AbpAuthorize(AppPermissions.Pages_Customers)]
        public async Task<PagedResultDto<CustomerContactDto>> GetCustomerContacts(GetCustomerContactsInput input)
        {
            var query = _customerContactRepository.GetAll()
                            .Where(x => x.CustomerId == input.CustomerId);

            var totalCount = await query.CountAsync();

            var items = await query
                .Select(x => new CustomerContactDto
                {
                    Id = x.Id,
                    CustomerId = x.CustomerId,
                    Name = x.Name,
                    PhoneNumber = x.PhoneNumber,
                    Fax = x.Fax,
                    Email = x.Email,
                    Title = x.Title,
                    IsActive = x.IsActive,
                })
                .OrderBy(input.Sorting)
                .ToListAsync();

            return new PagedResultDto<CustomerContactDto>(
                totalCount,
                items);
        }

        [AbpAuthorize(AppPermissions.Pages_Customers)]
        public async Task<int> GetCustomerContactDuplicateCount(GetCustomerContactDuplicateCountInput input)
        {
            return await _customerContactRepository.GetAll()
                .Where(x => x.CustomerId == input.CustomerId && x.Id != input.ExceptId && x.Name == input.Name)
                .CountAsync();
        }

        [HttpPost]
        public async Task<ListResultDto<SelectListDto>> GetCustomerContactsByIdsSelectList(GetItemsByIdsInput input)
        {
            var items = await _customerContactRepository.GetAll()
                .Where(x => input.Ids.Contains(x.Id))
                .Select(x => new SelectListDto
                {
                    Id = x.Id.ToString(),
                    Name = x.Name
                })
                .OrderBy(x => x.Name)
                .ToListAsync();

            return new ListResultDto<SelectListDto>(items);
        }

        public async Task<ListResultDto<SelectListDto>> GetContactsForCustomer(NullableIdDto input)
        {
            if (input.Id == null)
            {
                return new ListResultDto<SelectListDto>();
            }
            var contacts = await _customerContactRepository.GetAll()
                .Where(x => x.CustomerId == input.Id && x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => new SelectListDto<CustomerContactSelectListInfoDto>
                {
                    Id = x.Id.ToString(),
                    Name = x.Name,
                    Item = new CustomerContactSelectListInfoDto
                    {
                        PhoneNumber = x.PhoneNumber
                    }
                })
                .ToListAsync();
            return new ListResultDto<SelectListDto>(contacts);
        }

        [AbpAuthorize(AppPermissions.Pages_Customers)]
        public async Task<CustomerContactEditDto> GetCustomerContactForEdit(GetCustomerContactForEditInput input)
        {
            CustomerContactEditDto customerContactEditDto;

            if (input.Id.HasValue)
            {
                var customerContact = await _customerContactRepository.GetAsync(input.Id.Value);
                customerContactEditDto = new CustomerContactEditDto
                {
                    Id = customerContact.Id,
                    CustomerId = customerContact.CustomerId,
                    Name = customerContact.Name,
                    PhoneNumber = customerContact.PhoneNumber,
                    Fax = customerContact.Fax,
                    Email = customerContact.Email,
                    Title = customerContact.Title,
                    IsActive = customerContact.IsActive,
                    HasCustomerPortalAccess = customerContact.HasCustomerPortalAccess
                };
            }
            else
            {
                customerContactEditDto = new CustomerContactEditDto
                {
                    Name = input.Name,
                    CustomerId = input.CustomerId ?? throw new UserFriendlyException("Please select a customer first"),
                    IsActive = true,
                };
            }

            return customerContactEditDto;
        }

        [AbpAuthorize(AppPermissions.Pages_Customers)]
        public async Task<int> EditCustomerContact(CustomerContactEditDto model)
        {
            var customerContact = new CustomerContact
            {
                Id = model.Id ?? 0,
                CustomerId = model.CustomerId,
                Name = model.Name,
                PhoneNumber = model.PhoneNumber,
                Fax = model.Fax,
                Email = model.Email,
                Title = model.Title,
                TenantId = Session.TenantId ?? 0,
                IsActive = model.IsActive,
                HasCustomerPortalAccess = model.HasCustomerPortalAccess
            };

            var customerContactId = await _customerContactRepository.InsertOrUpdateAndGetIdAsync(customerContact);

            await _customerContactUserLinkService.UpdateUser(customerContact);

            return customerContactId;
        }

        public async Task<bool> CanDeleteCustomerContact(EntityDto input)
        {
            var hasOrders = await _orderRepository.GetAll().Where(x => x.ContactId == input.Id).AnyAsync();
            if (hasOrders)
            {
                return false;
            }

            return true;
        }

        [AbpAuthorize(AppPermissions.Pages_Customers)]
        public async Task DeleteCustomerContact(EntityDto input)
        {
            var canDelete = await CanDeleteCustomerContact(input);
            if (!canDelete)
            {
                throw new UserFriendlyException("You can't delete selected row because it has data associated with it.");
            }

            var customerContact = await _customerContactRepository.GetAll().FirstAsync(x => x.Id == input.Id);

            await _customerContactUserLinkService.EnsureCanDeleteCustomerContact(customerContact);
            await _customerContactRepository.DeleteAsync(customerContact);
        }

        [AbpAuthorize(AppPermissions.Pages_Customers_Merge)]
        public async Task MergeCustomers(DataMergeInput input)
        {
            await _customerRepository.MergeCustomersAsync(input.IdsToMerge, input.MainRecordId, AbpSession.TenantId);
        }

        [AbpAuthorize(AppPermissions.Pages_Customers_Merge)]
        public async Task MergeCustomerContacts(DataMergeInput input)
        {
            await _customerRepository.MergeCustomerContactsAsync(input.IdsToMerge, input.MainRecordId, AbpSession.TenantId);
        }
    }
}
