namespace TwoHandApp.Models.Pagination;

public class ResponsePaginationModel<T>
{

    public T Data { get; protected set; }

    public int CurrentPage { get; protected set; }

    public int TotalPages { get; protected set; }

    public int PageSize { get; protected set; }

    public int TotalCount { get; protected set; }


    public static ResponsePaginationModel<T> Ok(T data, int currentPage, int totalPages, int pageSize, int totalCount)
    {
        return new ResponsePaginationModel<T>
        {
            Data = data,
            CurrentPage = currentPage,
            TotalPages = totalPages,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}