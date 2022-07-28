using System.Collections.Generic;

namespace DCMS.Core
{

    public interface IPagedList<T> : IList<T>
    {

        int PageIndex { get; }

        int PageSize { get; }


        int TotalCount { get; }


        int TotalPages { get; }


        bool HasPreviousPage { get; }

        bool HasNextPage { get; }
    }
}
