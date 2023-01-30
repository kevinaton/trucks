using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.EntityFrameworkCore;
using Abp.UI;
using DispatcherWeb.Orders;
using DispatcherWeb.Projects;
using DispatcherWeb.Quotes;
using DispatcherWeb.Services;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.EntityFrameworkCore.Repositories
{
    public class ServiceRepository : DispatcherWebRepositoryBase<Service>, IServiceRepository
    {
        public ServiceRepository(IDbContextProvider<DispatcherWebDbContext> dbContextProvider) 
            : base(dbContextProvider)
        {
        }

        public async Task MergeServicesAsync(List<int> recordIds, int mainRecordId, int? tenantId)
        {
            recordIds.RemoveAll(x => x == mainRecordId);
            
            var allRecordIds = recordIds.Union(new[] { mainRecordId }).Distinct().ToList();
            var records = await GetAll()
                .Include(x => x.OfficeServicePrices)
                .Where(x => allRecordIds.Contains(x.Id))
                .ToListAsync();

            var mainRecord = records.FirstOrDefault(x => x.Id == mainRecordId);
            records.RemoveAll(x => x.Id == mainRecordId);

            if (mainRecord == null || !records.Any())
            {
                return;
            }

            foreach (var record in records)
            {
                foreach (var price in record.OfficeServicePrices.ToList())
                {
                    if (!mainRecord.OfficeServicePrices.Any(x => x.MaterialUomId == price.MaterialUomId
                                                                && x.FreightUomId == price.FreightUomId
                                                                && x.OfficeId == price.OfficeId
                                                                //&& x.DesignationId == price.DesignationId
                                                                ))
                    {
                        record.OfficeServicePrices.Remove(price);
                        mainRecord.OfficeServicePrices.Add(price);
                        price.ServiceId = mainRecordId;
                    }
                }
                record.MergedToId = mainRecordId;
            }

            var context = await GetContextAsync();

            await context.SaveChangesAsync();

            await context.MergeEntitiesAsync(nameof(OrderLine), nameof(OrderLine.ServiceId), tenantId, mainRecordId, allRecordIds);
            await context.MergeEntitiesAsync(nameof(ReceiptLine), nameof(ReceiptLine.ServiceId), tenantId, mainRecordId, allRecordIds);
            await context.MergeEntitiesAsync(nameof(ProjectService), nameof(ProjectService.ServiceId), tenantId, mainRecordId, allRecordIds);
            await context.MergeEntitiesAsync(nameof(QuoteService), nameof(QuoteService.ServiceId), tenantId, mainRecordId, allRecordIds);
            await context.MergeEntitiesAsync(nameof(Ticket), nameof(Ticket.ServiceId), tenantId, mainRecordId, allRecordIds);
            //await context.MergeEntitiesAsync(nameof(OfficeServicePrice), nameof(OfficeServicePrice.ServiceId), tenantId, mainRecordId, allRecordIds);

            foreach(var record in records)
            {
                foreach (var price in record.OfficeServicePrices.ToList())
                {
                    context.OfficeServicePrices.Remove(price);
                }
                context.Services.Remove(record);
            }
        }
    }
}
