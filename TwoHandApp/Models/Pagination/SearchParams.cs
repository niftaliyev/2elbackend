namespace TwoHandApp.Models.Pagination;

public class SearchParams<T> : PaginationParams
{
    public List<SortingDtoModeL>? Sort { get; set; } = new();
}
