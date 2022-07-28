//
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;



namespace DCMS.ViewModel.Models.Logging
{
    public partial class LogListModel : BaseModel
    {
        public LogListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
        }

        [HintDisplayName("", "")]
        [UIHint("DateTimeNullable")]
        public DateTime? CreatedOnFrom { get; set; }

        [HintDisplayName("", "")]
        [UIHint("DateTimeNullable")]
        public DateTime? CreatedOnTo { get; set; }

        [HintDisplayName("", "")]

        public string Message { get; set; }

        [HintDisplayName("", "")]
        public int LogLevelId { get; set; }


        public SelectList AvailableLogLevels { get; set; }


        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<LogModel> LogItems { get; set; }
    }
}