using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Finances;
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace DCMS.ViewModel.Models.Finances
{
    public class BalanceSheetListModel
    {
        /// <summary>
        /// 资产
        /// </summary>
        public BalanceSheetTotalModel AssetsTrees { get; set; } = new BalanceSheetTotalModel();
        /// <summary>
        /// 负债
        /// </summary>
        public BalanceSheetTotalModel LiabilitiesTrees { get; set; } = new BalanceSheetTotalModel();
        /// <summary>
        /// 所有者权益
        /// </summary>
        public BalanceSheetTotalModel EquitiesTrees { get; set; } = new BalanceSheetTotalModel();

        public string RecordTime { get; set; }
        public SelectList Dates { get; set; }
    }

    public class BalanceSheetTotalModel
    {
        public decimal? TotalInitialBalance { get; set; } = 0;
        public decimal? TotalEndBalance { get; set; } = 0;
        public List<AccountingOptionTree<BalanceSheet>> Trees { get; set; } = new List<AccountingOptionTree<BalanceSheet>>();
    }

    /// <summary>
    /// 用于表示资产负债表
    /// </summary>
    public class BalanceSheetModel : BaseEntityModel
    {
        [HintDisplayName("科目类别", "科目类别")]
        public int AccountingTypeId { get; set; } = 0;
        public string AccountingTypeName { get; set; }


        [HintDisplayName("财务科目", "财务科目")]
        public int AccountingOptionId { get; set; } = 0;
        public string AccountingOptionName { get; set; }
        public string AccountingOptionCode { get; set; }

        [HintDisplayName("结转期数", "结转期数（短日期）")]
        public DateTime PeriodDate { get; set; }


        [HintDisplayName("年初余额", "年初余额")]
        public decimal? InitialBalance { get; set; } = 0;

        [HintDisplayName("期末余额", "期末余额")]
        public decimal? EndBalance { get; set; } = 0;


        [HintDisplayName("创建日期", "创建日期")]
        public DateTime CreatedOnUtc { get; set; }
    }

    //public class BalanceSheetAccountingOptionTree
    //{
    //    public bool Visible { get; set; }
    //    public AccountingOptionModel Option { get; set; }
    //    public int MaxItems { get; set; } = 0;
    //    public List<BalanceSheetAccountingOptionTree> Children { get; set; }
    //    public BalanceSheetModel BalanceSheet { get; set; } = new BalanceSheetModel();
    //}
}