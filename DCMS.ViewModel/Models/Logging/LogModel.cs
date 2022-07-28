//
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using System;


namespace DCMS.ViewModel.Models.Logging
{
    public partial class LogModel : BaseEntityModel
    {
        [HintDisplayName("", "")]
        public string LogLevel { get; set; }

        [HintDisplayName("", "")]

        public string ShortMessage { get; set; }

        [HintDisplayName("", "")]

        public string FullMessage { get; set; }

        [HintDisplayName("", "")]

        public string IpAddress { get; set; }

        [HintDisplayName("", "")]
        public int? UserId { get; set; }

        [HintDisplayName("", "")]
        public string UserEmail { get; set; }

        [HintDisplayName("", "")]

        public string PageUrl { get; set; }

        [HintDisplayName("", "")]

        public string ReferrerUrl { get; set; }

        [HintDisplayName("", "")]
        public DateTime CreatedOn { get; set; }
    }
}