using DCMS.Web.Framework.Models;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DCMS.ViewModel.Models.StockCycleManage
{
    public partial class DealerinventorycycleListModels : BaseEntityModel
    {
        [DisplayName("开始时间")]
        [UIHint("DateTime")]
        public DateTime QueryData { get; set; }
    }
}
