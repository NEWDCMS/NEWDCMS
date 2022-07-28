using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using System;


namespace DCMS.ViewModel.Models.Logging
{


    public partial class ActivityLogModel : BaseEntityModel
    {
        [HintDisplayName("", "")]
        public string ActivityLogTypeName { get; set; }
        [HintDisplayName("", "")]
        public int UserId { get; set; }
        [HintDisplayName("", "")]
        public string Comment { get; set; }
        [HintDisplayName("", "")]
        public DateTime CreatedOnUtc { get; set; }
    }
}
