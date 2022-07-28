using DCMS.Core;
using DCMS.Core.Domain.Products;
using System.Collections.Generic;

namespace DCMS.Services.Products
{
    public interface IProductTierPricePlanService
    {
        void DeleteProductTierPricePlan(ProductTierPricePlan productTierPricePlans);
        IList<ProductTierPricePlan> GetAllProductTierPricePlans();
        IPagedList<ProductTierPricePlan> GetAllProductTierPricePlans(string name = null, int pageIndex = 0, int pageSize = int.MaxValue);
        IList<ProductTierPricePlan> GetAllProductTierPricePlans(int? store);
        ProductTierPricePlan GetProductTierPricePlanById(int? store, int productTierPricePlansId);
        IList<ProductTierPricePlan> GetProductTierPricePlansByIds(int[] sIds);
        void InsertProductTierPricePlan(ProductTierPricePlan productTierPricePlans);
        void UpdateProductTierPricePlan(ProductTierPricePlan productTierPricePlans);

        IList<ProductPricePlan> GetAllPricePlan(int? store);

        IList<ProductTierPrice> GetProductTierPricePlans(int priceplanid);
        int ProductTierPricePlansId(int store, string Name);
    }
}