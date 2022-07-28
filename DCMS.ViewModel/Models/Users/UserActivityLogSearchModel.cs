using DCMS.Web.Framework.Models;

namespace DCMS.ViewModel.Models.Users
{

    public partial class UserActivityLogSearchModel : BaseSearchModel
    {
        #region Properties

        public int UserId { get; set; }

        #endregion
    }
}