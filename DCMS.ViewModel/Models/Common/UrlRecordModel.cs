using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
namespace DCMS.ViewModel.Models.Common
{
    public partial class UrlRecordModel : BaseEntityModel
    {
        #region Properties

        [HintDisplayName("Name", "")]
        public string Name { get; set; }

        [HintDisplayName("EntityId", "")]
        public int EntityId { get; set; }

        [HintDisplayName("EntityName", "")]
        public string EntityName { get; set; }

        [HintDisplayName("IsActive", "")]
        public bool IsActive { get; set; }

        [HintDisplayName("Details", "")]
        public string DetailsUrl { get; set; }

        #endregion
    }
}