using DCMS.Core.Domain.Common;
using DCMS.Core.Domain.Terminals;
using System.Collections.Generic;

namespace DCMS.Services.Terminals
{
    /// <summary>
    /// 片区信息接口
    /// </summary>
    public partial interface IDistrictService
    {
        IList<District> GetAll(int? storeId);
        /// <summary>
        /// 绑定片区信息
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        IList<District> BindDistrictList(int? storeId);

        District GetDistrictById(int? store, int id);
        IList<District> GetDistrictByIds(int? store, int[] ids);

        IList<District> GetDistrictByParentId(int? storeId, int parentId);
        IList<District> GetAllDistrictByStoreId(int? store);
        void InsertDistrict(District district);
        void DeleteDistrict(District district);
        void UpdateDistrict(District district);
        IList<ZTree> GetListZTreeVM(int? store);
        List<int> GetDistricts(int? store, int Id);

        int GetDistrictByName(int store, string name);

        List<int> GetSubDistrictIds(int storeId, int districtId);
        List<int> GetSonDistrictIds(int storeId, int districtId);

        bool CheckDistrictRoot(int storeId);

        IList<District> GetAllDistrictByStoreId(int? storeId, int? userId);

        bool HadInstall(int storeId);

        bool InstallDistrict(int storeId);
        /// <summary>
        /// 获取子片区
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="districtId"></param>
        /// <returns></returns>
        IList<District> GetChildDistrict(int storeId, int districtId);
        /// <summary>
        /// 获取父片区
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        IList<District> GetParentDistrict(int storeId, int parentId);
        /// <summary>
        /// 获取用户绑定的片区
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        IList<District> GetUserDistrict(int storeId, int userId);
    }
}
