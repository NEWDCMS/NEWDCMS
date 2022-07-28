using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;


namespace DCMS.ViewModel.Models.Common
{
    /// <summary>
    /// Represents an URL record search model
    /// </summary>
    public partial class UrlRecordSearchModel : BaseSearchModel
    {
        #region Properties

        [HintDisplayName("SEO 名称", "")]
        public string SeName { get; set; }

        #endregion
    }
}