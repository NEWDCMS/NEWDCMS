using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.Users;

namespace DCMS.Services.Products
{
    public interface IProductAttributeFormatter
    {
        string FormatAttributes(Product product, string attributes);
        string FormatAttributes(Product product, string attributes, User user, string serapator = "<br />", bool htmlEncode = true, bool renderPrices = true, bool renderProductAttributes = true, bool renderGiftCardAttributes = true, bool allowHyperlinks = true);
    }
}