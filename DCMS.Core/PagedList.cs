using System;
using System.Collections.Generic;
using System.Linq;

namespace DCMS.Core
{
    /// <summary>
    /// 分页数据类
    /// </summary>
    /// <typeparam name="T">T</typeparam>
    [Serializable]
    public class PagedList<T> : List<T>, IPagedList<T>
    {

        public PagedList(IQueryable<T> source, int pageIndex, int pageSize, bool getOnlyTotalCount = false)
        {
            var total = source.Count();
            TotalCount = total;
            TotalPages = total / (pageSize == 0 ? 1 : pageSize);

            if (total % pageSize > 0)
            {
                TotalPages++;
            }

            PageSize = pageSize;
            PageIndex = pageIndex;
            if (getOnlyTotalCount)
            {
                return;
            }

            AddRange(source.Skip(pageIndex * pageSize).Take(pageSize).ToList());
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="source">source</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        public PagedList(IList<T> source, int pageIndex, int pageSize)
        {
            TotalCount = source.Count;
            TotalPages = TotalCount / (pageSize == 0 ? 1 : pageSize);

            if (TotalCount % pageSize > 0)
            {
                TotalPages++;
            }

            PageSize = pageSize;
            PageIndex = pageIndex;
            AddRange(source.Skip(pageIndex * pageSize).Take(pageSize).ToList());
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="source">source</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="totalCount">Total count</param>
        public PagedList(IEnumerable<T> source, int pageIndex, int pageSize, int totalCount)
        {
            TotalCount = totalCount;
            TotalPages = TotalCount / (pageSize == 0 ? 1 : pageSize);

            if (TotalCount % pageSize > 0)
            {
                TotalPages++;
            }

            PageSize = pageSize;
            PageIndex = pageIndex;
            AddRange(source);
        }

        /// <summary>
        /// Page index
        /// </summary>
        public int PageIndex { get; }

        /// <summary>
        /// Page size
        /// </summary>
        public int PageSize { get; }

        /// <summary>
        /// Total count
        /// </summary>
        public int TotalCount { get; }

        /// <summary>
        /// Total pages
        /// </summary>
        public int TotalPages { get; }

        /// <summary>
        /// Has previous page
        /// </summary>
        public bool HasPreviousPage => PageIndex > 0;

        /// <summary>
        /// Has next page
        /// </summary>
        public bool HasNextPage => PageIndex + 1 < TotalPages;
    }


    public class PagedList<TSource, TResult> : List<TSource>, IPagedList<TSource>
    {

        public int PageIndex { get; }
        public int PageSize { get; }
        public int TotalCount { get; }
        public int TotalPages { get; }
        public int IndexFrom { get; }
        public IList<TResult> Items { get; }
        public bool HasPreviousPage => PageIndex - IndexFrom > 0;
        public bool HasNextPage => PageIndex - IndexFrom + 1 < TotalPages;

        public PagedList(IPagedList<TSource> source, Func<IEnumerable<TSource>, IEnumerable<TResult>> converter)
        {
            PageIndex = source.PageIndex;
            PageSize = source.PageSize;
            //IndexFrom = source.IndexFrom;
            TotalCount = source.TotalCount;
            TotalPages = source.TotalPages;

            //Items = new List<TResult>(converter(source.Items));

            AddRange(source);
        }


        public PagedList(IEnumerable<TSource> source, Func<IEnumerable<TSource>, IEnumerable<TResult>> converter, int pageIndex, int pageSize, int indexFrom)
        {
            if (indexFrom > pageIndex)
            {
                throw new ArgumentException($"indexFrom: {indexFrom} > pageIndex: {pageIndex}, must indexFrom <= pageIndex");
            }

            if (source is IQueryable<TSource> querable)
            {
                PageIndex = pageIndex;
                PageSize = pageSize;
                IndexFrom = indexFrom;
                TotalCount = querable.Count();
                TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);

                var items = querable.Skip((PageIndex - IndexFrom) * PageSize).Take(PageSize).ToArray();

                Items = new List<TResult>(converter(items));

                //AddRange(items);
            }
            else
            {
                PageIndex = pageIndex;
                PageSize = pageSize;
                IndexFrom = indexFrom;
                TotalCount = source.Count();
                TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);

                var items = source.Skip((PageIndex - IndexFrom) * PageSize).Take(PageSize).ToArray();

                Items = new List<TResult>(converter(items));
                //AddRange(items);
            }


        }

    }

    /// <summary>
    /// 分页数据类
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    [Serializable]
    public class PageData<TOutput>
    {
        public PageData(IList<TOutput> source,int total)
        {
            Total = total;
            Data = source.ToList();
        }
        /// <summary>
        /// 总数
        /// </summary>
        public int Total { get; set; }

        public List<TOutput> Data { get; set; }
    }
}
