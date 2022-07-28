using DCMS.Core;
using System;
using System.Collections.Generic;

namespace DCMS.Web.Framework.Models.Extensions
{
    /// <summary>
    /// Represents model extensions
    /// </summary>
    public static class ModelExtensions
    {
        /// <summary>
        /// 根据分页请求将列表转换为分页列表
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="list">List of objects</param>
        /// <param name="pagingRequestModel">Paging request model</param>
        /// <returns>Paged list</returns>
        public static IPagedList<T> ToPagedList<T>(this IList<T> list, IPagingRequestModel pagingRequestModel)
        {
            return new PagedList<T>(list, pagingRequestModel.Page - 1, pagingRequestModel.PageSize);
        }

        /// <summary>
        /// 准备要在网格中显示的传递列表模型
        /// </summary>
        /// <typeparam name="TListModel">列表类型</typeparam>
        /// <typeparam name="TModel">模型</typeparam>
        /// <typeparam name="TObject">对象类型</typeparam>
        /// <param name="listModel">列表模型</param>
        /// <param name="searchModel">搜索模型</param>
        /// <param name="objectList">分页列表模型</param>
        /// <param name="dataFillFunction">用于填充模型数据的委托</param>
        /// <returns>模型列表</returns>
        public static TListModel PrepareToGrid<TListModel, TModel, TObject>(this TListModel listModel,
            BaseSearchModel searchModel, IPagedList<TObject> objectList, Func<IEnumerable<TModel>> dataFillFunction)
            where TListModel : BasePagedListModel<TModel>
            where TModel : BaseModel
        {
            if (listModel == null)
            {
                throw new ArgumentNullException(nameof(listModel));
            }

            listModel.Data = dataFillFunction?.Invoke();
            listModel.Draw = searchModel?.Draw;
            listModel.RecordsTotal = objectList?.TotalCount ?? 0;
            listModel.RecordsFiltered = objectList?.TotalCount ?? 0;

            return listModel;
        }
    }
}