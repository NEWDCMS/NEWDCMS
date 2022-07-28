using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace DCMS.ViewModel.Models.Logging
{

    public partial class LogSearchModel : BaseSearchModel
    {
        #region Ctor

        public LogSearchModel()
        {
            AvailableLogLevels = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        [HintDisplayName("Admin.System.Log.List.CreatedOnFrom", "")]
        [UIHint("DateNullable")]
        public DateTime? CreatedOnFrom { get; set; }

        [HintDisplayName("Admin.System.Log.List.CreatedOnTo", "")]
        [UIHint("DateNullable")]
        public DateTime? CreatedOnTo { get; set; }

        [HintDisplayName("Admin.System.Log.List.Message", "")]
        public string Message { get; set; }

        [HintDisplayName("Admin.System.Log.List.LogLevel", "")]
        public int LogLevelId { get; set; }

        public IList<SelectListItem> AvailableLogLevels { get; set; }

        #endregion
    }
}