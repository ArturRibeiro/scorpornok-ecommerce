using System;
using System.Collections.Generic;
using System.Linq;

namespace Frameworker.Scorponok.Reading.Database.Impl
{
    [Serializable]
    public class PagedInfo : IPagedInfo
    {
        public int PageIndex { get; set;}
        public int PageSize { get; set;}
        public int TotalCount { get; set; }
        public int TotalPages { get; set;}
        public bool HasPreviousPage { get; set;}
        public bool HasNextPage { get; set;}

        #region Factory

        public static class Factory
        {
            public static PagedInfo Create<T>(IEnumerable<T> source, int pageIndex, int pageSize, int? totalCount)
            {
                var pagedInfo = new PagedInfo();
                pagedInfo.TotalCount = totalCount ?? source.ToList().Count;
                pagedInfo.TotalPages = pagedInfo.TotalCount / pageSize;
                if (pagedInfo.TotalCount % pageSize > 0) pagedInfo.TotalPages++;
                pagedInfo.PageSize = pageSize;
                pagedInfo.PageIndex = pageIndex;
                return pagedInfo;
            }
        }

        #endregion
    }
}