using DCMS.Core;
using DCMS.Core.Domain.Products;
using System.Collections.Generic;

namespace DCMS.Services.Products
{
    public partial interface IManufacturerService
    {
        IList<Manufacturer> GetAllManufacturers(int? storeId = null);
        /// <summary>
        /// 绑定供应商信息
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        IList<Manufacturer> BindManufacturerList(int? storeId);

        IPagedList<Manufacturer> GetAllManufactureies(string searchStr, int? storeId = null, int pageIndex = 0, int pageSize = int.MaxValue);
        Manufacturer GetManufacturerById(int? store, int id);
        string GetManufacturerName(int? store, int id);
        IList<Manufacturer> GetManufacturersByIds(int? store, int[] idArr);
        void InsertManufacturer(Manufacturer manufacturer);
        void DeleteManufacturer(Manufacturer manufacturer);
        void UpdateManufacturer(Manufacturer manufacturer);
        IList<int> GetManufacturerId(int ManufacturerId);
        Dictionary<int, string> GetManufacturerDictsByIds(int storeId, int[] ids);
        int ManufacturerByName(int store, string name);
    }
}
