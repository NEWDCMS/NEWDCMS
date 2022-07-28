using DCMS.Core;
using DCMS.Core.Domain.WareHouses;
using System.Collections.Generic;

namespace DCMS.Services.WareHouses
{
    public interface IWareHouseService
    {
        IList<WareHouse> GetWareHouseList(int? storeId);

        /// <summary>
        /// 绑定仓库信息
        /// </summary>
        /// <param name="storeId"></param>  
        /// <returns></returns>
        IList<WareHouse> BindWareHouseList(int? storeId, WHAEnum? wHA, int userId = 0);
        IList<WareHouse> BindWareHouseList(int? storeId, int? type, WHAEnum? wHA, int userId = 0);

        IPagedList<WareHouse> GetWareHouseList(string searchStr, int? storeId, int? type, int pageIndex, int pageSize);
        WareHouse GetWareHouseById(int? store, int id);
        string GetWareHouseName(int? store, int id);
        WareHouse GetWareHouseByName(int store, string name);
        IList<WareHouse> GetWareHouseByIds(int? store, int[] ids, bool platform = false);
        IList<WareHouse> GetWareHouseIdsByWareHouseIds(int[] idArr);
        IList<WareHouse> GetWareHouseIdsBytype(int store, int type = 2);
        void InsertWareHouse(WareHouse wareHouse);
        void DeleteWareHouse(WareHouse wareHouse);
        void UpdateWareHouse(WareHouse wareHouse);

        /// <summary>
        /// 验证当前商品是否正在盘点
        /// </summary>
        /// <param name="storeId">经销商Id</param>
        /// <param name="wareHouseId">仓库Id</param>
        /// <param name="productIds">商品Ids</param>
        /// <param name="errMsg">错误信息</param>
        /// <returns></returns>
        bool CheckProductInventory(int storeId, int wareHouseId, int[] productIds, out string errMsg);

    }
}
