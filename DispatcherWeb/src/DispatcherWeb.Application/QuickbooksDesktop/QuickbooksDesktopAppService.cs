using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.Timing;
using DispatcherWeb.Configuration;
using DispatcherWeb.Customers;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.QuickbooksDesktop.Dto;
using DispatcherWeb.QuickbooksDesktop.Models;
using DispatcherWeb.QuickbooksOnline;
using DispatcherWeb.Services;
using DispatcherWeb.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DispatcherWeb.QuickbooksDesktop
{
    public class QuickbooksDesktopAppService : DispatcherWebAppServiceBase, IQuickbooksDesktopAppService
    {
        private const int CustomerNameMaxLength = 41;
        private const string Yes = "Y";
        private const string No = "N";
        const string DateFormat = "MM/dd/yy"; //"yyyy-MM-dd"

        private readonly IConfigurationRoot _appConfiguration;
        private readonly IRepository<Invoices.Invoice> _invoiceRepository;
        private readonly IRepository<Service> _serviceRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<Invoices.InvoiceUploadBatch> _invoiceUploadBatchRepository;
        private readonly IBinaryObjectManager _binaryObjectManager;

        public QuickbooksDesktopAppService(
            IAppConfigurationAccessor configurationAccessor,
            IRepository<Invoices.Invoice> invoiceRepository,
            IRepository<Service> serviceRepository,
            IRepository<Customer> customerRepository,
            IRepository<Invoices.InvoiceUploadBatch> invoiceUploadBatchRepository,
            IBinaryObjectManager binaryObjectManager
            )
        {
            _appConfiguration = configurationAccessor.Configuration;
            _invoiceRepository = invoiceRepository;
            _serviceRepository = serviceRepository;
            _customerRepository = customerRepository;
            _invoiceUploadBatchRepository = invoiceUploadBatchRepository;
            _binaryObjectManager = binaryObjectManager;
        }

        public async Task<ExportInvoicesToIIFResult> ExportInvoicesToIIF(ExportInvoicesToIIFInput input)
        {
            var invoicesToUpload = await _invoiceRepository.GetAll()
                .Where(x => x.QuickbooksExportDateTime == null)
                .WhereIf(input.InvoiceStatuses.Any(), x => input.InvoiceStatuses.Contains(x.Status))
                .ToInvoiceToUploadList(await GetTimezone());

            var invoiceNumberPrefix = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.Quickbooks.InvoiceNumberPrefix);
            var taxCalculationType = (TaxCalculationType)await SettingManager.GetSettingValueAsync<int>(AppSettings.Invoice.TaxCalculationType);
            if (taxCalculationType == TaxCalculationType.MaterialTotal)
            {
                invoicesToUpload.SplitMaterialAndFreightLines();
            }

            //foreach (var invoice in invoicesToUpload)
            //{
            //    invoice.RecalculateTotals(taxCalculationType);
            //}

            if (!invoicesToUpload.Any(x => x.InvoiceLines.Any()))
            {
                return new ExportInvoicesToIIFResult
                {
                    ErrorMessage = "There are no new Invoices to export"
                };
            }

            var uploadedInvoices = new List<Invoices.Invoice>();

            var DefaultIncomeAccountName = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.QuickbooksDesktop.DefaultIncomeAccountName); //"Income Services";
            var DefaultIncomeAccountType = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.QuickbooksDesktop.DefaultIncomeAccountType); //"INC";
            var AccountsReceivableAccountName = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.QuickbooksDesktop.AccountsReceivableAccountName); //"Accounts Receivable";
            var TaxAccountName = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.QuickbooksDesktop.TaxAccountName); //"Sales Tax Payable";
            var TaxAgencyVendorName = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.QuickbooksDesktop.TaxAgencyVendorName); //"TaxAgencyVendor";

            //var Memo = "System Generated Invoice";

            const string TaxInventoryItemName = "Add Tax";
            const string HasCleared = No;
            var todayFormatted = (await GetToday()).ToString(DateFormat);
            var timezone = await GetTimezone();


            var s = new StringBuilder();


            AccountRow.HeaderRow.AppendRow(s);
            new AccountRow
            {
                Name = AccountsReceivableAccountName,
                AccountType = AccountTypes.AccountsReceivable
            }.AppendRow(s);

            new AccountRow
            {
                Name = DefaultIncomeAccountName,
                AccountType = DefaultIncomeAccountType //AccountTypes.Income
            }.AppendRow(s);

            new AccountRow
            {
                Name = TaxAccountName,
                AccountType = AccountTypes.OtherCurrentLiability
            }.AppendRow(s);

            VendorRow.HeaderRow.AppendRow(s);
            new VendorRow
            {
                Name = TaxAgencyVendorName,
                VendorType = VendorTypes.TaxAgency
            }.AppendRow(s);

            var tooLongItemNames = new List<string>();

            ItemRow.HeaderRow.AppendRow(s);
            foreach (var itemGroup in invoicesToUpload
                .SelectMany(x => x.InvoiceLines)
                .Where(x => x.ItemId != null && x.ItemIsInQuickBooks != true)
                .GroupBy(x => x.ItemName))
            {
                var invoiceLine = itemGroup.First();

                if (invoiceLine.ItemType == ServiceType.InventoryPart)
                {
                    continue; //Exclude "Inventory Part" until someone asks for it as an option.
                    //Inventory parts have more required fields, e.g. AssetAccount, CogsAccount, maybe Cost
                }

                var itemType = ItemTypes.FromServiceType(invoiceLine.ItemType);
                if (itemType == null)
                {
                    continue;
                }

                if (invoiceLine.ItemName?.Length > EntityStringFieldLengths.Service.ServiceInQuickBooks)
                {
                    tooLongItemNames.Add(invoiceLine.ItemName);
                    continue;
                }

                if (invoiceLine.Quantity == 0)
                {
                    invoiceLine.Quantity = 1;
                }
                new ItemRow
                {
                    Name = invoiceLine.ItemName,
                    ItemType = itemType, //ticket.ServiceHasMaterialPricing ? ItemTypes.NonInventoryPartItem : ItemTypes.ServiceItem,
                    Account = !invoiceLine.ItemIncomeAccount.IsNullOrEmpty() ? invoiceLine.ItemIncomeAccount : DefaultIncomeAccountName,
                    //AssetAccount = ASSETACCNT,
                    //CogsAccount = COGSACCNT,
                    Price = (invoiceLine.Subtotal / invoiceLine.Quantity).ToString(),
                    //Cost = Convert.ToString(invoiceLine.FreightExtendedAmount + invoiceLine.MaterialExtendedAmount),
                    Taxable = Yes,
                    //PaymentMethod = PAYMETH,
                    //TaxAgency = TAXVEND,
                    //TaxDistrict = TAXDIST,
                }.AppendRow(s);
            }

            if (tooLongItemNames.Any())
            {
                return new ExportInvoicesToIIFResult
                {
                    ErrorMessage = "The following products/services exceed the limit of 31 characters: \r\n" + string.Join(", \r\n", tooLongItemNames)
                };
            }

            foreach (var invoiceTax in invoicesToUpload.Where(x => x.TaxRate > 0).GroupBy(x => x.TaxRate))
            {
                new ItemRow
                {
                    Name = TaxInventoryItemName + " " + invoiceTax.Key + "%",
                    ItemType = ItemTypes.SalesTax,
                    Account = TaxAccountName,
                    //AssetAccount
                    //CogsAccount
                    Price = invoiceTax.Key + "%",
                    Cost = "0",
                    Taxable = No,
                    TaxAgency = TaxAgencyVendorName,
                }.AppendRow(s);
            }


            foreach (var invoice in invoicesToUpload)
            {
                if (!invoice.InvoiceLines.Any())
                {
                    continue;
                }

                var customerName = RemoveRestrictedCharacters(invoice.Customer.Name).Truncate(CustomerNameMaxLength);
                if (!invoice.Customer.IsInQuickBooks)
                {
                    CustomerRow.HeaderRow.AppendRow(s);
                    new CustomerRow
                    {
                        Name = customerName,
                        Email = invoice.Customer.InvoiceEmail,
                        Taxable = Yes
                    }.SetShippingAddress(invoice.Customer.ShippingAddress)
                    .SetBillingAddress(invoice.Customer.BillingAddress)
                    .AppendRow(s);
                }

                TransactionRow.HeaderRow.AppendRow(s);
                TransactionLineRow.HeaderRow.AppendRow(s);
                TransactionEndRow.HeaderRow.AppendRow(s);

                new TransactionRow
                {
                    TransactionId = invoice.InvoiceId.ToString(),
                    TransactionType = TransactionTypes.Invoice,
                    Date = invoice.IssueDate?.ToString(DateFormat) ?? todayFormatted,
                    DueDate = invoice.DueDate?.ToString(DateFormat),
                    Account = AccountsReceivableAccountName,
                    Name = customerName,
                    Amount = Convert.ToString(invoice.TotalAmount),
                    DocNumber = invoiceNumberPrefix + invoice.InvoiceId.ToString(),
                    InvoiceMemo = invoice.Message,
                    PoNumber = invoice.GetPoNumberOrJobNumber(),
                    HasCleared = HasCleared,
                    NameIsTaxable = Yes,
                    Terms = invoice.Terms.GetDisplayName()
                }.SetAddress(customerName, invoice.BillingAddress)
                .AppendRow(s);

                foreach (var lineItem in invoice.InvoiceLines)
                {
                    if (lineItem.Quantity == 0)
                    {
                        lineItem.Quantity = 1;
                    }
                    new TransactionLineRow
                    {
                        TransactionLineId = lineItem.LineNumber.ToString(),
                        TransactionType = TransactionTypes.Invoice,
                        Date = invoice.IssueDate?.ToString(DateFormat) ?? todayFormatted,
                        ServiceDate = lineItem.DeliveryDateTime?.ToString(DateFormat),
                        Account = !string.IsNullOrEmpty(lineItem.ItemIncomeAccount) ? lineItem.ItemIncomeAccount : DefaultIncomeAccountName,
                        Name = customerName,
                        Amount = (-1 * lineItem.Subtotal).ToString(),
                        DocNumber = invoice.InvoiceId.ToString(),
                        Memo = lineItem.DescriptionAndTicketWithTruck,
                        HasCleared = HasCleared,
                        Quantity = (-1 * lineItem.Quantity).ToString(),
                        Price = (-1 * (lineItem.Subtotal / lineItem.Quantity)).ToString(),
                        //Price = (-1 * (lineItem.Ticket?.OrderMaterialPrice ?? 0 + lineItem.Ticket?.OrderFreightPrice ?? 0)).ToString(),
                        Item = lineItem.ItemName,
                        Taxable = lineItem.Tax > 0 ? Yes : No,
                    }.AppendRow(s);
                }

                if (invoice.Tax > 0)
                {
                    new TransactionLineRow
                    {
                        TransactionLineId = (invoice.InvoiceLines.Max(x => x.LineNumber) + 1).ToString(),
                        TransactionType = TransactionTypes.Invoice,
                        Date = invoice.IssueDate?.ToString(DateFormat) ?? todayFormatted,
                        Account = TaxAccountName,
                        Name = TaxAgencyVendorName,
                        Amount = invoice.Tax.ToString(),
                        DocNumber = invoice.InvoiceId.ToString(),
                        HasCleared = HasCleared,
                        Price = invoice.TaxRate + "%",
                        Item = TaxInventoryItemName + " " + invoice.TaxRate + "%",
                        Taxable = No,
                        Extra = ExtraKeywords.InvoiceLine.AutoSalesTax
                    }.AppendRow(s);
                }
                new TransactionEndRow().AppendRow(s);

                uploadedInvoices.Add(invoice.Invoice);
            }

            var iifContents = s.ToString();
            var iifBytes = Encoding.UTF8.GetBytes(iifContents);

            var invoiceUploadBatch = new Invoices.InvoiceUploadBatch { TenantId = AbpSession.TenantId ?? 0 };
            await _invoiceUploadBatchRepository.InsertAndGetIdAsync(invoiceUploadBatch);

            foreach (var invoice in uploadedInvoices)
            {
                invoice.QuickbooksExportDateTime = Clock.Now;
                invoice.UploadBatchId = invoiceUploadBatch.Id;
                invoice.Status = InvoiceStatus.Sent;
            }
            invoiceUploadBatch.FileGuid = await _binaryObjectManager.UploadByteArrayAsync(iifBytes, AbpSession.TenantId ?? 0);

            await CurrentUnitOfWork.SaveChangesAsync();

            var serviceIds = invoicesToUpload
                .SelectMany(x => x.InvoiceLines)
                .Where(x => x.ItemId != null && x.ItemIsInQuickBooks != true)
                .Select(x => x.ItemId)
                .Distinct()
                .ToList();

            if (serviceIds.Any())
            {
                var services = await _serviceRepository.GetAll().Where(x => serviceIds.Contains(x.Id)).ToListAsync();
                services.ForEach(x => x.IsInQuickBooks = true);
                await CurrentUnitOfWork.SaveChangesAsync();
            }

            var customerIds = invoicesToUpload
                .Where(x => !x.Customer.IsInQuickBooks)
                .Select(x => x.Customer.Id)
                .Distinct()
                .ToList();

            if (customerIds.Any())
            {
                var customers = await _customerRepository.GetAll().Where(x => customerIds.Contains(x.Id)).ToListAsync();
                customers.ForEach(x => x.IsInQuickBooks = true);
                await CurrentUnitOfWork.SaveChangesAsync();
            }

            return new ExportInvoicesToIIFResult
            {
                FileBytes = iifBytes,
                UploadBatchId = invoiceUploadBatch.Id
            };
        }

        private string RemoveRestrictedCharacters(string val)
        {
            return val?.Replace(":", "").Replace("\t", "").Replace("\r", "").Replace("\n", "");
        }
    }
}
