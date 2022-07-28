using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DCMS.ViewModel.Models.StockCycleManage
{
    public partial class StockCycleManageModel : BaseEntityModel
    {
        [DisplayName("开始时间")]
        [UIHint("DateTime")]
        public DateTime StartTime { get; set; }

        [DisplayName("开始时间")]
        [UIHint("DateTime")]
        public DateTime EndTime { get; set; }
        [DisplayName("工厂ID")]
        public int FactorId { get; set; }
        public SelectList Searchfactory { get; set; }
    }
}
