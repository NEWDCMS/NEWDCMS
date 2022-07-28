
using DCMS.Core.Domain.Products;

namespace DCMS.Services.Products
{
    /// <summary>
    /// À©Õ¹
    /// </summary>
    public static class ProductAttributeExtensions
    {

        public static bool ShouldHaveValues(this ProductVariantAttribute productVariantAttribute)
        {
            if (productVariantAttribute == null)
            {
                return false;
            }

            if (productVariantAttribute.AttributeControlType == AttributeControlType.TextBox ||
                productVariantAttribute.AttributeControlType == AttributeControlType.MultilineTextbox)
            {
                return false;
            }

            //other attribute controle types support values
            return true;
        }
    }
}
