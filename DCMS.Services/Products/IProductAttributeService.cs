using DCMS.Core.Domain.Products;
using System.Collections.Generic;

namespace DCMS.Services.Products
{
    public interface IProductAttributeService
    {
        void DeleteProductAttribute(ProductAttribute productAttribute);
        void DeleteProductVariantAttribute(ProductVariantAttribute productVariantAttribute);
        void DeleteProductVariantAttributeCombination(ProductVariantAttributeCombination combination);
        void DeleteProductVariantAttributeValue(ProductVariantAttributeValue productVariantAttributeValue);
        IList<ProductAttribute> GetAllProductAttributes(int? store, string name);
        IList<ProductVariantAttributeCombination> GetAllProductVariantAttributeCombinations(int productId);
        ProductAttribute GetProductAttributeById(int? store, int productAttributeId);
        ProductVariantAttribute GetProductVariantAttributeById(int? store, int productVariantAttributeId);
        ProductVariantAttributeCombination GetProductVariantAttributeCombinationById(int productVariantAttributeCombinationId);
        IList<ProductVariantAttribute> GetProductVariantAttributesByProductId(int? store, int productId);
        ProductVariantAttributeValue GetProductVariantAttributeValueById(int? store, int productVariantAttributeValueId);
        IList<ProductVariantAttributeValue> GetProductVariantAttributeValues(int? store, int productVariantAttributeId);
        void InsertProductAttribute(ProductAttribute productAttribute);
        void InsertProductVariantAttribute(ProductVariantAttribute productVariantAttribute);
        void InsertProductVariantAttributeCombination(ProductVariantAttributeCombination combination);
        void InsertProductVariantAttributeValue(ProductVariantAttributeValue productVariantAttributeValue);
        void UpdateProductAttribute(ProductAttribute productAttribute);
        void UpdateProductVariantAttribute(ProductVariantAttribute productVariantAttribute);
        void UpdateProductVariantAttributeCombination(ProductVariantAttributeCombination combination);
        void UpdateProductVariantAttributeValue(ProductVariantAttributeValue productVariantAttributeValue);
    }
}