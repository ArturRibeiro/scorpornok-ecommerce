using System.Collections.Generic;
using Frameworker.Scorponok.Reading.Database.Impl;

namespace Frameworker.Scorponok.Reading.Database
{
    public interface IPagedList<T>
    {
        PagedInfo PageInfo { get; set; }
        
        IReadOnlyCollection<T> Items { get; set;}
    }
}