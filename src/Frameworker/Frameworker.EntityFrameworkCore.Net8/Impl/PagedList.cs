using System.Runtime.Serialization;

namespace Frameworker.EntityFrameworkCore.Impl;

[Serializable]
public class PagedList<T> : IPagedList<T>
{
    public PagedList() { }

    public PagedList(IEnumerable<T> items, int pageIndex, int pageSize, int count)
    {
        this.Items = items.ToList().AsReadOnly();
        this.PageInfo = PagedInfo.Create(totalItems: count, currentPage: pageIndex, pageSize: pageSize);
    }
    
    [DataMember] public PagedInfo PageInfo { get; set; }
    [DataMember]public IReadOnlyCollection<T> Items { get; set; }
}

// public class PagedList<T>(IEnumerable<T> items, int pageIndex, int pageSize, int count)
//     : IPagedList<T>
// {
//     public PagedInfo PageInfo { get; set; } = PagedInfo.Create(totalItems: count, currentPage: pageIndex, pageSize: pageSize);
//
//     public IReadOnlyCollection<T> Items { get; set; } = items.ToList().AsReadOnly();
// }