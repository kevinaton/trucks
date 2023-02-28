
using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Timing.Timezone;
using DispatcherWeb.Authorization;
using DispatcherWeb.QuoteHistory.Dto;
using DispatcherWeb.Quotes;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.QuoteHistory
{
    [AbpAuthorize(AppPermissions.Pages_Quotes_View)]
    public class QuoteHistoryAppService : DispatcherWebAppServiceBase, IQuoteHistoryAppService
    {
        private readonly IRepository<QuoteHistoryRecord> _quoteHistoryRepository;
        private readonly ITimeZoneConverter _timeZoneConverter;

        public QuoteHistoryAppService(
            IRepository<QuoteHistoryRecord> quoteHistoryRepository,
            ITimeZoneConverter timeZoneConverter
            )
        {
            _quoteHistoryRepository = quoteHistoryRepository;
            _timeZoneConverter = timeZoneConverter;
        }

        public async Task<PagedResultDto<QuoteHistoryDto>> GetQuoteHistory(GetQuoteHistoryInput input)
        {
            input.StartDate = GetLocalDateTime(input.StartDate);
            input.EndDate = GetLocalDateTime(input.EndDate);

            var query = _quoteHistoryRepository.GetAll()
                .WhereIf(input.StartDate.HasValue, x => x.DateTime >= input.StartDate)
                .WhereIf(input.EndDate.HasValue, x => x.DateTime < input.EndDate.Value.AddDays(1))
                .WhereIf(input.CustomerId.HasValue, x => x.Quote.CustomerId == input.CustomerId)
                .WhereIf(input.QuoteId.HasValue, x => x.QuoteId == input.QuoteId);

            var totalCount = await query.CountAsync();

            var items = await query
                .Select(x => new QuoteHistoryDto
                {
                    Id = x.Id,
                    QuoteId = x.QuoteId,
                    QuoteName = x.Quote.Name,
                    DateTime = x.DateTime,
                    ChangedByUserName = x.CreatorUser.Name + " " + x.CreatorUser.Surname,
                    ChangeType = x.ChangeType
                })
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<QuoteHistoryDto>(
                totalCount,
                items);

            // Local functions
            DateTime? GetLocalDateTime(DateTime? date)
            {
                if (date.HasValue)
                {
                    Debug.Assert(AbpSession.UserId != null, "AbpSession.UserId != null");
                    return _timeZoneConverter.Convert(date, AbpSession.TenantId, AbpSession.UserId.Value);
                }

                return null;
            }
        }

        public async Task<QuoteHistoryDto> GetQuoteHistoryDetails(EntityDto input)
        {
            var record = await _quoteHistoryRepository.GetAll()
                .Select(x => new QuoteHistoryDto
                {
                    Id = x.Id,
                    QuoteId = x.QuoteId,
                    QuoteName = x.Quote.Name,
                    DateTime = x.DateTime,
                    ChangedByUserName = x.CreatorUser.Name + " " + x.CreatorUser.Surname,
                    ChangeType = x.ChangeType,
                })
                .FirstAsync(x => x.Id == input.Id);

            return record;
        }

        public async Task<PagedResultDto<QuoteFieldDiffDto>> GetQuoteFieldDiffDtos(EntityDto input)
        {
            var items = await _quoteHistoryRepository.GetAll()
                .Where(qh => qh.Id == input.Id)
                .SelectMany(qh => qh.FieldDiffs)
                .Select(s => new QuoteFieldDiffDto
                {
                    Id = s.Id,
                    Field = s.Field,
                    OldId = s.OldId,
                    NewId = s.NewId,
                    OldDisplayValue = s.OldDisplayValue,
                    NewDisplayValue = s.NewDisplayValue
                })
                .ToListAsync();
            return new PagedResultDto<QuoteFieldDiffDto>(items.Count, items);
        }

    }
}
