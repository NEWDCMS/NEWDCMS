//
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using System.Collections.Generic;



namespace DCMS.ViewModel.Models.Logging
{


    public partial class ActivityLogTypeListModel : BaseEntityModel
    {
        public IList<ActivityLogTypeModel> LogTypeItems { get; set; }
    }

    public partial class ActivityLogTypeModel : BaseEntityModel
    {

        [HintDisplayName("", "")]
        public string Name { get; set; }

        public string SystemKeyword { get; set; }


        [HintDisplayName("", "")]
        public bool Enabled { get; set; }
    }
}