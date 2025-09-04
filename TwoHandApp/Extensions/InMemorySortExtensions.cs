using System.Reflection;

namespace TwoHandApp.Models.Pagination;

public static class InMemorySortExtensions
{
    public static IEnumerable<T> ApplySorting<T>(
        this IEnumerable<T> source,
        List<(string field, string order)> sortOrders)
    {
        if (sortOrders == null || !sortOrders.Any())
            return source;

        IOrderedEnumerable<T> orderedQuery = null;

        foreach (var (field, order) in sortOrders)
        {
            if (field == null)
                continue;

            var prop = typeof(T).GetProperty(field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (prop == null)
                continue;

            if (orderedQuery == null)
            {
                orderedQuery = order.Equals("asc", StringComparison.OrdinalIgnoreCase)
                    ? source.OrderBy(x => prop.GetValue(x,null))
                    : source.OrderByDescending(x => prop.GetValue(x, null));
            }
            else
            {
                orderedQuery = order.Equals("asc", StringComparison.OrdinalIgnoreCase)
                    ? orderedQuery.ThenBy(x => prop.GetValue(x, null))
                    : orderedQuery.ThenByDescending(x => prop.GetValue(x, null));
            }
        }

        return orderedQuery ?? source;
    }
}