using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DCMS.ViewModel.Models.Report
{
    public partial class VisitReachedListModel : BaseModel
    {
        public VisitReachedListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Items = new List<VisitReachedModel>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<VisitReachedModel> Items { get; set; }

        #region 用于条件检索
        [HintDisplayName("业务员", "业务员")]
        public int? BusinessUserId { get; set; } = 0;
        public string BusinessUserName { get; set; }
        public SelectList BusinessUsers { get; set; }

        [HintDisplayName("线路", "线路")]
        public int? LineId { get; set; } = 0;
        public string LineName { get; set; }
        public SelectList Lines { get; set; }

        [HintDisplayName("开始日期", "开始日期")]
        [UIHint("DateTimeNullable")]
        public DateTime? StartTime { get; set; }

        [HintDisplayName("结束日期", "结束日期")]
        [UIHint("DateTimeNullable")]
        public DateTime? EndTime { get; set; }
        #endregion

    }

    public partial class VisitReachedModel
    {
        public int StoreId { get; set; } = 0;
        //签到时间
        public DateTime SigninDateTime { get; set; }

        /// <summary>
        /// 业务员
        /// </summary>
        public int BusinessUserId { get; set; } = 0;
        public string BusinessUserName { get; set; }

        //线路
        public int LineId { get; set; } = 0;
        public string LineName { get; set; }
        //计划拜访数
        public int PlanVisitCount { get; set; } = 0;
        //实际拜访数
        public int ActualVisitCount { get; set; } = 0;
        //成交店数
        public int ReachedStoreCount { get; set; } = 0;
        //成交单数
        public int ReachedBillCount { get; set; } = 0;
        //拜访成功率
        public string VisitSuccessRate { get; set; }
        //成交率
        public string CloseRate { get; set; }
        //定位时长
        public string OnStoreStopSeconds { get; set; }
        /// <summary>
        /// 终端数
        /// </summary>
        public int TerminalCount { get; set; } = 0;
        //未拜访数
        public int UnVisitCount { get; set; } = 0;
    }
}
