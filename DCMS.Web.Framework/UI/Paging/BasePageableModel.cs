
using DCMS.Core;
using DCMS.Web.Framework.Models;
using System;

namespace DCMS.Web.Framework.UI.Paging
{
    /// <summary>
    /// 分页模型基类
    /// </summary>
    public abstract class BasePageableModel : BaseModel, IPageableModel
    {


        /// <summary>
        /// Ctor
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="pagedList">Entities (models)</param>
        public virtual void LoadPagedList<T>(IPagedList<T> pagedList)
        {
            FirstItem = (pagedList.PageIndex * pagedList.PageSize) + 1;
            HasNextPage = pagedList.HasNextPage;
            HasPreviousPage = pagedList.HasPreviousPage;
            LastItem = Math.Min(pagedList.TotalCount, ((pagedList.PageIndex * pagedList.PageSize) + pagedList.PageSize));
            PageNumber = pagedList.PageIndex + 1;
            PageSize = pagedList.PageSize;
            TotalItems = pagedList.TotalCount;
            TotalPages = pagedList.TotalPages;
        }


        public int PageIndex
        {
            get
            {
                if (PageNumber > 0)
                {
                    return PageNumber - 1;
                }

                return 0;
            }
        }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }


        public int TotalItems { get; set; }


        public int TotalPages { get; set; }

        public int FirstItem { get; set; }

        public int LastItem { get; set; }

        public bool HasPreviousPage { get; set; }

        public bool HasNextPage { get; set; }

    }

}