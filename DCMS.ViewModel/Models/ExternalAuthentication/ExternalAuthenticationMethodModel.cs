using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;

namespace DCMS.ViewModel.Models.ExternalAuthentication
{

    public partial class ExternalAuthenticationMethodModel : BaseModel, IPluginModel
    {
        #region Properties

        [HintDisplayName("Admin.Configuration.ExternalAuthenticationMethods.Fields.FriendlyName", "")]
        public string FriendlyName { get; set; }

        [HintDisplayName("Admin.Configuration.ExternalAuthenticationMethods.Fields.SystemName", "")]
        public string SystemName { get; set; }

        [HintDisplayName("Admin.Configuration.ExternalAuthenticationMethods.Fields.DisplayOrder", "")]
        public int DisplayOrder { get; set; }

        [HintDisplayName("Admin.Configuration.ExternalAuthenticationMethods.Fields.IsActive", "")]
        public bool IsActive { get; set; }

        [HintDisplayName("Admin.Configuration.ExternalAuthenticationMethods.Configure", "")]
        public string ConfigurationUrl { get; set; }

        public string LogoUrl { get; set; }

        #endregion
    }
}