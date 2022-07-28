using DCMS.Web.Framework.Models;
using System.Collections.Generic;

namespace DCMS.ViewModel.Models.Logging
{
    public partial class ActivityLogTypeSearchModel : BaseSearchModel
    {
        #region Properties       

        public IList<ActivityLogTypeModel> ActivityLogTypeListModel { get; set; }

        #endregion
    }
}