using System.Collections.Generic;
using Frameworker.Scorponok.Reading.Database.Impl;

namespace Frameworker.Scorponok.Reading.Database.Extensions
{
    public static class DbConnectionExtensions
    {
        public static PagedList<T> ToPagedList<T>(this IEnumerable<T> list, int pageIndex, int pageSize) 
            => PagedList<T>.Factory.Create(list, pageIndex, pageSize);
    }
}