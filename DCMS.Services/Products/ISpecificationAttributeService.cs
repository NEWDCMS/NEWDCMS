using DCMS.Core.Data;
using DCMS.Core.Domain.Products;
using System.Collections.Generic;

namespace DCMS.Services.Products
{
    public interface ISpecificationAttributeService
    {
        void DeleteProductSpecificationAttribute(ProductSpecificationAttribute productSpecificationAttribute);
        void DeleteSpecificationAttribute(SpecificationAttribute specificationAttribute);
        void DeleteSpecificationAttributeOption(SpecificationAttributeOption specificationAttributeOption);
        ProductSpecificationAttribute GetProductSpecificationAttributeById(int productSpecificationAttributeId);
        IList<ProductSpecificationAttribute> GetProductSpecificationAttributesByProductId(int? store, int productId);
        IList<ProductSpecificationAttribute> GetProductSpecificationAttributesByProductId(int? store, int productId, bool? allowFiltering, bool? showOnProductPage, bool noCache = false);
        IList<ProductSpecificationAttribute> GetAllProductSpecificationAttributesByProductIds(int? store, int[] productIds);
        SpecificationAttribute GetSpecificationAttributeById(int specificationAttributeId);
        IList<int> GetpProductIds(int specificationAttributeId);
        SpecificationAttributeOption GetSpecificationAttributeOptionById(int specificationAttributeOptionId);
        IList<SpecificationAttributeOption> GetSpecificationAttributeOptionByIds(int? store, List<int> ids, bool platform = false);

        string GetSpecificationAttributeOptionName(int? store, int specificationAttributeOptionId);
        int GetSpecificationAttributeOptionId(int store, string specificationAttributeOptionName);
        IList<SpecificationAttributeOption> GetSpecificationAttributeOptionsBySpecificationAttribute(int store, int specificationAttributeId);
        IList<SpecificationAttributeOption> GetSpecificationAttributeOptionsBySpecificationAttribute(int specificationAttributeId);
        IList<SpecificationAttribute> GetSpecificationAttributes(string name);
        IList<SpecificationAttribute> GetSpecificationAttributesBtStore(int? storeId);
        void InsertProductSpecificationAttribute(ProductSpecificationAttribute productSpecificationAttribute);
        void InsertSpecificationAttribute(SpecificationAttribute specificationAttribute);
        void InsertSpecificationAttributeOption(SpecificationAttributeOption specificationAttributeOption);
        void InsertProductSpecificationAttribute(List<ProductSpecificationAttribute> productSpecificationAttribute);
        void InsertProductSpecificationAttribute(IUnitOfWork uow, List<ProductSpecificationAttribute> productSpecificationAttribute);
        void UpdateProductSpecificationAttribute(ProductSpecificationAttribute productSpecificationAttribute);
        void updateProductSpecificationAttribute(List<ProductSpecificationAttribute> productSpecificationAttribute);
        void UpdateSpecificationAttribute(SpecificationAttribute specificationAttribute);
        void UpdateSpecificationAttributeOption(SpecificationAttributeOption specificationAttributeOption);
        IList<SpecificationAttributeOption> GetSpecificationAttributeOptionsByStore(int? store);
        ProductSpecificationAttribute GetProductSpecAttributeById(int productSpecificationAttributeId,int productId,int specificationAttributeOptionId);
    }

}