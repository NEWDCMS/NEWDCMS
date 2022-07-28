using DCMS.Core;
using DCMS.Core.Domain.Stores;
using DCMS.Core.Domain.Users;
using DCMS.Core.Domain.Terminals;
using System.Collections.Generic;

namespace DCMS.Services.Stores
{
    /// <summary>
    /// 经销商服务接口
    /// </summary>
    public partial interface IStoreService
    {
        void DeleteStore(Store store);
        IPagedList<Store> GetAllStores(string name = null, int pageIndex = 0, int pageSize = int.MaxValue);
        IList<Store> GetAllStores(bool loadCacheableCopy = true);
        IList<Corporations> GetAllfactory();
        IList<Store> BindStoreList();
        Store GetStoreByUserId(int userId);
        Store GetManageStore();
        string GetStoreName(int storeId);
        Store GetStoreById(int storeId);
        IList<Store> GetStoresByIds(int[] sIds);
        void InsertStore(Store store);
        void UpdateStore(Store store);
        bool CheckStoreCode(string storeCode);
        bool AddStoreScript(Store store, User user);
        string[] GetNotExistingStores(string[] storeIdsNames);
        IList<Terminal> GetTerminals(int? storeId);
    }
}