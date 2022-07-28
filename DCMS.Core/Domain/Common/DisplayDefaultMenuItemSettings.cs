using DCMS.Core.Configuration;

namespace DCMS.Core.Domain.Common
{

    public class DisplayDefaultMenuItemSettings : ISettings
    {
        public bool DisplayHomepageMenuItem { get; set; }
        public bool DisplayNewProductsMenuItem { get; set; }
        public bool DisplayProductSearchMenuItem { get; set; }
        public bool DisplayUserInfoMenuItem { get; set; }

        public bool DisplayContactUsMenuItem { get; set; }
    }
}