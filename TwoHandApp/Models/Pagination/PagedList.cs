namespace TwoHandApp.Models.Pagination;

public class PagedList<T> : List<T>
{
    public PaginationData PaginationData { get; set; }

    public static PagedList<T> Empty => new PagedList<T>(new List<T>(), 0, 1, 1);

    public PagedList(IEnumerable<T> items, int count, int pageNumber, int pageSize)
    {
        PaginationData = new PaginationData
        {
            TotalCount = count,
            PageSize = pageSize,
            CurrentPage = pageNumber,
            TotalPages = (int)Math.Ceiling((double)count / (double)pageSize)
        };
        AddRange(items);
    }
}