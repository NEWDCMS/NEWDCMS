using DCMS.Core;
using DCMS.Core.Domain.Products;
using System.Collections.Generic;

namespace DCMS.Services.Products
{
    public interface IBrandService
    {
        void DeleteBrand(Brand brands);
        IPagedList<Brand> GetAllBrands(int? store, string name = null, int pageIndex = 0, int pageSize = int.MaxValue);
        IList<Brand> GetAllBrands(int? store);
        /// <summary>
        /// 绑定品牌信息
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        IList<Brand> BindBrandList(int? store);

        Brand GetBrandById(int? store, int brandsId);
        string GetBrandName(int? store, int brandId);

        int GetBrandId(int store, string brandName);

        IList<Brand> GetBrandsByIds(int? store, int[] sIds);
        IList<Brand> GetBrandsByBrandIds(int? store, int[] sIds);
        IList<Brand> GetBrandsIdsByBrandIds(int? store, int[] ids);
        void InsertBrand(Brand brands);
        void UpdateBrand(Brand brands);
        Dictionary<int, string> GetAllBrandsNames(int? store, int[] ids);
    }
}