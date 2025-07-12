using Frameworker.EntityFrameworkCore.Impl;

namespace Frameworker.EntityFrameworkCore;

public interface IPagedList<T>
{
    PagedInfo PageInfo { get; set; }
        
    IReadOnlyCollection<T> Items { get; set;}
}