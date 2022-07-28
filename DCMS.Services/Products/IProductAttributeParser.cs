using DCMS.Core.Domain.Products;
using System.Collections.Generic;

namespace DCMS.Services.Products
{
    public interface IProductAttributeParser
    {
        string AddProductAttribute(string attributes, ProductVariantAttribute pva, string value);
        bool AreProductAttributesEqual(string attributes1, string attributes2);
        ProductVariantAttributeCombination FindProductVariantAttributeCombination(Product product, string attributesXml);
        IList<string> GenerateAllCombinations(Product product);
        IList<int> ParseProductVariantAttributeIds(string attributes);
        IList<ProductVariantAttribute> ParseProductVariantAttributes(string attributes);
        IList<ProductVariantAttributeValue> ParseProductVariantAttributeValues(string attributes);
        IList<string> ParseValues(string attributes, int productVariantAttributeId);
    }
}