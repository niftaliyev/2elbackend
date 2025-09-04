namespace TwoHandApp.Models.Pagination;

public static class PaginationQuery
{
    public static IEnumerable<T> Pagination<T>(this IEnumerable<T> values, int? page = 1, int? pageSize = null)
    {
        if (page.HasValue && pageSize.HasValue)
        {
            values = values.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value);
        }
        else if (pageSize.HasValue)
        {
            values = values.Take(pageSize.Value);
        }

        return values;
    }
}