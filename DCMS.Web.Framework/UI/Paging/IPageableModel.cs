
namespace DCMS.Web.Framework.UI.Paging
{
    /// <summary>
    /// 分页对象集合
    /// </summary>
    public interface IPageableModel
    {

        int PageIndex { get; }

        int PageNumber { get; }

        int PageSize { get; }

        int TotalItems { get; }

        int TotalPages { get; }

        int FirstItem { get; }

        int LastItem { get; }

        bool HasPreviousPage { get; }

        bool HasNextPage { get; }
    }


    /// <summary>
    /// 泛型 <see cref="IPageableModel"/>
    /// </summary>
    /// <typeparam name="T">要分页的对象类型</typeparam>
    public interface IPagination<T> : IPageableModel
    {

    }

}