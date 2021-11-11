namespace Frameworker.Scorponok.Reading.Database
{
    public interface IPagedInfo
    {
        int PageIndex { get; set; }
        int PageSize { get; set; }
        int TotalCount { get; set; }
        int TotalPages { get; set; }
        bool HasPreviousPage { get; set; }
        bool HasNextPage { get; set; }
    }
}