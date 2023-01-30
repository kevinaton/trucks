using Abp.EntityFrameworkCore;
using Abp.UI;
using DispatcherWeb.Locations;
using DispatcherWeb.Orders;
using DispatcherWeb.Projects;
using DispatcherWeb.Quotes;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DispatcherWeb.EntityFrameworkCore.Repositories
{
    public class LocationRepository : DispatcherWebRepositoryBase<Location>, ILocationRepository
    {
        public LocationRepository(IDbContextProvider<DispatcherWebDbContext> dbContextProvider)
          : base(dbContextProvider)
        {
        }

        public async Task MergeLocationsAsync(List<int> recordIds, int mainRecordId, int? tenantId)
        {
            recordIds.RemoveAll(x => x == mainRecordId);

            var allRecordIds = recordIds.Union(new[] { mainRecordId }).Distinct().ToList();
            var records = await GetAll()
                .Include(x => x.SupplierContacts)
                .Where(x => allRecordIds.Contains(x.Id))
                .ToListAsync();

            var mainRecord = records.FirstOrDefault(x => x.Id == mainRecordId);
            records.RemoveAll(x => x.Id == mainRecordId);

            if (records.Any(x => x.PredefinedLocationKind != null))
            {
                throw new UserFriendlyException("You can't merge predefined Locations");
            }

            if (mainRecord == null || !records.Any())
            {
                return;
            }

            foreach (var record in records)
            {
                foreach (var contact in record.SupplierContacts.ToList())
                {
                    record.SupplierContacts.Remove(contact);
                    mainRecord.SupplierContacts.Add(contact);
                    contact.LocationId = mainRecordId;
                }
                record.MergedToId = mainRecordId;
            }
            var context = await GetContextAsync();

            await context.SaveChangesAsync();

            await context.MergeEntitiesAsync(nameof(OrderLine), nameof(OrderLine.LoadAtId), tenantId, mainRecordId, allRecordIds);
            await context.MergeEntitiesAsync(nameof(OrderLine), nameof(OrderLine.DeliverToId), tenantId, mainRecordId, allRecordIds);
            await context.MergeEntitiesAsync(nameof(ReceiptLine), nameof(OrderLine.LoadAtId), tenantId, mainRecordId, allRecordIds);
            await context.MergeEntitiesAsync(nameof(ReceiptLine), nameof(OrderLine.DeliverToId), tenantId, mainRecordId, allRecordIds);
            await context.MergeEntitiesAsync(nameof(Ticket), nameof(OrderLine.LoadAtId), tenantId, mainRecordId, allRecordIds);
            await context.MergeEntitiesAsync(nameof(Ticket), nameof(OrderLine.DeliverToId), tenantId, mainRecordId, allRecordIds);
            await context.MergeEntitiesAsync(nameof(ProjectService), nameof(ProjectService.LoadAtId), tenantId, mainRecordId, allRecordIds);
            await context.MergeEntitiesAsync(nameof(ProjectService), nameof(ProjectService.DeliverToId), tenantId, mainRecordId, allRecordIds);
            await context.MergeEntitiesAsync(nameof(QuoteService), nameof(QuoteService.LoadAtId), tenantId, mainRecordId, allRecordIds);
            await context.MergeEntitiesAsync(nameof(QuoteService), nameof(QuoteService.DeliverToId), tenantId, mainRecordId, allRecordIds);

            foreach (var record in records)
            {
                await DeleteAsync(record);
            }
        }

        public async Task MergeSupplierContactsAsync(List<int> recordIds, int mainRecordId, int? tenantId)
        {
            var context = await GetContextAsync();

            recordIds.RemoveAll(x => x == mainRecordId);

            var allRecordIds = recordIds.Union(new[] { mainRecordId }).Distinct().ToList();
            var records = await context.SupplierContacts
                .Where(x => allRecordIds.Contains(x.Id))
                .ToListAsync();

            var mainRecord = records.FirstOrDefault(x => x.Id == mainRecordId);
            records.RemoveAll(x => x.Id == mainRecordId);

            if (mainRecord == null || !records.Any())
            {
                return;
            }

            //var recordIdsString = string.Join(",", recordIds);
            //await Context.Database.ExecuteSqlCommandAsync($"Update dbo.[Order] set SupplierContactId = {mainRecordId} where SupplierContactId in ({recordIdsString})");
            //await Context.Database.ExecuteSqlCommandAsync($"Update dbo.Project set SupplierContactId = {mainRecordId} where SupplierContactId in ({recordIdsString})");
            //await Context.Database.ExecuteSqlCommandAsync($"Update dbo.Quote set SupplierContactId = {mainRecordId} where SupplierContactId in ({recordIdsString})");

            records.ForEach(x => x.MergedToId = mainRecordId);
            await context.SaveChangesAsync();

            foreach (var record in records)
            {
                context.SupplierContacts.Remove(record);
            }
        }
    }
}
