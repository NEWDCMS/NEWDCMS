using DCMS.Core.Configuration;

namespace DCMS.Core.Domain.Common
{

    public class DisplayDefaultFooterItemSettings : ISettings
    {

        public bool DisplaySitemapFooterItem { get; set; }

        public bool DisplayContactUsFooterItem { get; set; }

        public bool DisplayProductSearchFooterItem { get; set; }

        public bool DisplayNewsFooterItem { get; set; }

        public bool DisplayRecentlyViewedProductsFooterItem { get; set; }


        public bool DisplayCompareProductsFooterItem { get; set; }


        public bool DisplayNewProductsFooterItem { get; set; }


        public bool DisplayUserInfoFooterItem { get; set; }


        public bool DisplayUserOrdersFooterItem { get; set; }


        public bool DisplayUserAddressesFooterItem { get; set; }


        public bool DisplayShoppingCartFooterItem { get; set; }


        public bool DisplayWishlistFooterItem { get; set; }
        public bool DisplayApplyVendorAccountFooterItem { get; set; }
    }
}
