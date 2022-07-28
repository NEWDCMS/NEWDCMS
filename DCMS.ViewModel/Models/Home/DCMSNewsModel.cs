using DCMS.Web.Framework.Models;
using System.Collections.Generic;

namespace DCMS.ViewModel.Models.Home
{
    /// <summary>
    /// Represents a jsdcms news model
    /// </summary>
    public partial class DCMSNewsModel : BaseModel
    {
        #region Ctor

        public DCMSNewsModel()
        {
            Items = new List<DCMSNewsDetailsModel>();
        }

        #endregion

        #region Properties

        public List<DCMSNewsDetailsModel> Items { get; set; }

        public bool HasNewItems { get; set; }

        public bool HideAdvertisements { get; set; }

        #endregion
    }
}