using DCMS.Core;
using DCMS.Core.Domain.Products;
using System.Collections.Generic;

namespace DCMS.Services.Products
{
    public interface IStatisticalTypeService
    {
        void DeleteStatisticalTypes(StatisticalTypes statisticalTypes);
        IList<StatisticalTypes> GetAllStatisticalTypess();
        IList<StatisticalTypes> GetAllStatisticalTypess(int? store);
        IPagedList<StatisticalTypes> GetAllStatisticalTypess(int? store, string name = null, int pageIndex = 0, int pageSize = int.MaxValue);
        StatisticalTypes GetStatisticalTypesById(int? store, int statisticalTypesId);
        IList<StatisticalTypes> GetStatisticalTypessByIds(int[] sIds);
        void InsertStatisticalTypes(StatisticalTypes statisticalTypes);
        void UpdateStatisticalTypes(StatisticalTypes statisticalTypes);
    }
}