using System.Collections.Generic;
using System.Linq;

namespace DispatcherWeb.Infrastructure.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<(T Item, int Index)> WithIndex<T>(this IEnumerable<T> source) =>
            source.Select((item, index) => (item, index));
    }
}
