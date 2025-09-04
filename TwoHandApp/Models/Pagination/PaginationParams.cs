namespace TwoHandApp.Models.Pagination;

public class PaginationParams
{
    //private const int MaxPageSize = 100;

    private int pageSize = 10;

    public int PageNumber { get; set; } = 1;


    public int PageSize
    {
        get
        {
            return pageSize;
        }
        set
        {
            //pageSize = ((value > MaxPageSize) ? MaxPageSize : value);
            pageSize = value;
        }
    }
}
