using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DispatcherWeb.Dto;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DispatcherWeb.Infrastructure.Extensions
{
    public static class SelectListExtensions
    {
        public static List<SelectListItem> ToSelectListItems(this IEnumerable<SelectListDto> items)
        {
            return items.Select(x => new SelectListItem()
            {
                Value = x.Id,
                Text = x.Name
            }).ToList();
        }

        public static async Task<List<SelectListItem>> ToSelectListItemsAsync(this Task<List<SelectListDto>> items)
        {
            return (await items).Select(x => new SelectListItem()
            {
                Value = x.Id,
                Text = x.Name
            }).ToList();
        }

        public static async Task<List<SelectListDto>> GetEnumItemsSelectList<TEnum>() where TEnum : Enum =>
            await Task.FromResult(Enum.GetValues(typeof(TEnum))
                            .Cast<TEnum>()
                            .Select(e => new SelectListDto { Id = e.ToString(), Name = Enum.GetName(typeof(TEnum), e) }).ToList());
    }
}
