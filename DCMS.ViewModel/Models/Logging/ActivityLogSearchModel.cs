//
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;



namespace DCMS.ViewModel.Models.Logging
{
    public partial class ActivityLogSearchModel : BaseModel
    {
        public ActivityLogSearchModel()
        {
            ActivityLogType = new List<SelectListItem>();
            PagingFilteringContext = new PagingFilteringModel();
        }

        [HintDisplayName("", "")]
        [UIHint("DateNullable")]
        public DateTime? CreatedOnFrom { get; set; }

        [HintDisplayName("", "")]
        [UIHint("DateNullable")]
        public DateTime? CreatedOnTo { get; set; }

        [HintDisplayName("", "")]
        public int ActivityLogTypeId { get; set; }

        [HintDisplayName("", "")]
        public IList<SelectListItem> ActivityLogType { get; set; }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<ActivityLogModel> LogItems { get; set; }
    }
}