using DCMS.Web.Framework.Models;
using System;

namespace DCMS.ViewModel.Models.Home
{
    /// <summary>
    /// Represents a jsdcms news details model
    /// </summary>
    public partial class DCMSNewsDetailsModel : BaseModel
    {
        #region Properties

        public string Title { get; set; }

        public string Url { get; set; }

        public string Summary { get; set; }

        public DateTimeOffset PublishDate { get; set; }

        #endregion
    }
}