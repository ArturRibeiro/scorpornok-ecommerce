using System.Runtime.Serialization;

namespace Frameworker.EntityFrameworkCore.Impl;

[Serializable]
public class PagedInfo : IPagedInfo
{
    [DataMember] public int CurrentPage { get; set; }
    [DataMember] public int PageSize { get; set; }
    [DataMember] public int TotalItems { get; set; }
    [DataMember] public int TotalPages { get; set; }
    [DataMember] public bool HasPreviousPage => CurrentPage > 1;
    [DataMember] public bool HasNextPage => CurrentPage < TotalPages;

    public static PagedInfo Create
        (int totalItems, int currentPage, int pageSize)
        => new()
        {
            TotalItems = totalItems,
            CurrentPage = currentPage,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        };
}