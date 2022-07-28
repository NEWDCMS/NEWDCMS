using DCMS.Web.Framework.Models;

namespace DCMS.ViewModel.Models.Global.Common
{
    public class DashboardReportModel : BaseModel
    {
        /// <summary>
        /// 今日销售额
        /// </summary>
        public decimal TodaySaleAmount { get; set; }

        /// <summary>
        /// 昨日销售额
        /// </summary>
        public decimal YesterdaySaleAmount { get; set; }

        /// <summary>
        /// 今日订单数
        /// </summary>
        public int TodayOrderQuantity { get; set; }

        /// <summary>
        /// 昨日订单数
        /// </summary>
        public int YesterdayOrderQuantity { get; set; }

        /// <summary>
        /// 今日新增客户
        /// </summary>
        public int TodayAddTerminalQuantity { get; set; }

        /// <summary>
        /// 昨日新增客户
        /// </summary>
        public int YesterdayAddTerminalQuantity { get; set; }

        /// <summary>
        /// 今日拜访客户
        /// </summary>
        public int TodayVisitQuantity { get; set; }

        /// <summary>
        /// 昨日拜访客户
        /// </summary>
        public int YesterdayVisitQuantity { get; set; }
    }
}
