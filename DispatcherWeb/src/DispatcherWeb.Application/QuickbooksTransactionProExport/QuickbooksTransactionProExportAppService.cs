using System.Linq;
using System.Threading.Tasks;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Configuration;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Invoices;
using DispatcherWeb.QuickbooksOnline;
using DispatcherWeb.Storage;

namespace DispatcherWeb.QuickbooksTransactionProExport
{
    public class QuickbooksTransactionProExportAppService : DispatcherWebAppServiceBase, IQuickbooksTransactionProExportAppService
    {
        private readonly IRepository<Invoice> _invoiceRepository;
        private readonly IRepository<InvoiceUploadBatch> _invoiceUploadBatchRepository;
        private readonly IBinaryObjectManager _binaryObjectManager;
        private readonly ITempFileCacheManager _tempFileCacheManager;
        private readonly IInvoiceListCsvExporter _invoiceListCsvExporter;

        public QuickbooksTransactionProExportAppService(
            IRepository<Invoice> invoiceRepository,
            IRepository<InvoiceUploadBatch> invoiceUploadBatchRepository,
            IBinaryObjectManager binaryObjectManager,
            ITempFileCacheManager tempFileCacheManager,
            IInvoiceListCsvExporter invoiceListCsvExporter
            )
        {
            _invoiceRepository = invoiceRepository;
            _invoiceUploadBatchRepository = invoiceUploadBatchRepository;
            _binaryObjectManager = binaryObjectManager;
            _tempFileCacheManager = tempFileCacheManager;
            _invoiceListCsvExporter = invoiceListCsvExporter;
        }

        public async Task<FileDto> ExportInvoicesToCsv()
        {
            var invoicesToUpload = await _invoiceRepository.GetAll()
                .Where(x => x.QuickbooksExportDateTime == null && x.InvoiceLines.Any() && x.Status == InvoiceStatus.ReadyForQuickbooks)
                .ToInvoiceToUploadList(await GetTimezone());

            var invoiceNumberPrefix = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.Quickbooks.InvoiceNumberPrefix);
            var taxCalculationType = (TaxCalculationType)await SettingManager.GetSettingValueAsync<int>(AppSettings.Invoice.TaxCalculationType);
            if (taxCalculationType == TaxCalculationType.MaterialTotal)
            {
                invoicesToUpload.SplitMaterialAndFreightLines();
            }

            if (!invoicesToUpload.Any(x => x.InvoiceLines.Any()))
            {
                throw new UserFriendlyException("There are no new Invoices to export");
            }

            foreach (var invoice in invoicesToUpload)
            {
                foreach (var invoiceLine in invoice.InvoiceLines)
                {
                    if (invoiceLine.Quantity == 0)
                    {
                        invoiceLine.Quantity = 1;
                    }

                    if (invoice.DueDate == null)
                    {
                        throw new UserFriendlyException($"Invoice #{invoice.InvoiceId} doesn't have Due Date specified. Please update the invoice and try again");
                    }
                }
            }

            var invoiceUploadBatch = new InvoiceUploadBatch { TenantId = AbpSession.GetTenantId() };
            await _invoiceUploadBatchRepository.InsertAndGetIdAsync(invoiceUploadBatch);

            var result = await _invoiceListCsvExporter.ExportToFileAsync(invoicesToUpload, $"InvoicesBatch-{invoiceUploadBatch.Id}");

            foreach (var invoice in invoicesToUpload)
            {
                invoice.Invoice.QuickbooksExportDateTime = Clock.Now;
                invoice.Invoice.UploadBatchId = invoiceUploadBatch.Id;
                invoice.Invoice.Status = InvoiceStatus.Sent;
            }
            var fileBytes = _tempFileCacheManager.GetFile(result.FileToken);
            invoiceUploadBatch.FileGuid = await _binaryObjectManager.UploadByteArrayAsync(fileBytes, AbpSession.TenantId ?? 0);

            await CurrentUnitOfWork.SaveChangesAsync();

            return result;
        }
    }
}