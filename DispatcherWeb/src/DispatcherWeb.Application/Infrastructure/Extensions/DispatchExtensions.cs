using DispatcherWeb.Dispatching;
using DispatcherWeb.Dispatching.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DispatcherWeb.Infrastructure.Extensions
{
    public static class DispatchExtensions
    {
        public static IQueryable<RawDispatchDto> ToRawDispatchDto(this IQueryable<Dispatch> query)
        {
            return DispatchingAppService.ToRawDispatchDto(query);
        }

    }
}
