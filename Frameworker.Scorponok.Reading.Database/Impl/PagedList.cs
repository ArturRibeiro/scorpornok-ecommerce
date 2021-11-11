using System;
using System.Collections.Generic;
using System.Linq;

namespace Frameworker.Scorponok.Reading.Database.Impl
{
    [Serializable]
    public class PagedList<T> : IPagedList<T>
    {
        /// <summary>
        /// 
        /// </summary>
        public PagedInfo PageInfo { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IReadOnlyCollection<T> Items { get; set; }

        public PagedList()
        {
            
        }
        
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="source">source</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="totalCount">Total count</param>
        private PagedList(IEnumerable<T> source, int pageIndex, int pageSize, int? totalCount)
        {
            var internalList = source  ?? new List<T>();
            pageSize = Math.Max(pageSize, 1);
            this.PageInfo = PagedInfo.Factory.Create(source, pageIndex, pageSize, totalCount);
            this.Items = internalList.ToList().AsReadOnly();
        }

        #region Factory

        public static class Factory
        {
            public static PagedList<T> Create(IEnumerable<T> source, int pageIndex, int pageSize, int? totalCount = null) 
                => new PagedList<T>(source, pageIndex, pageSize, totalCount);
        }


        #endregion
    }
}