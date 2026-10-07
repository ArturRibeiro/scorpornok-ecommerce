namespace Frameworker.EntityFrameworkCore;

public interface IPagedInfo
{
    int CurrentPage { get; }
    int PageSize { get; }
    int TotalItems { get; }
    int TotalPages { get; }
}