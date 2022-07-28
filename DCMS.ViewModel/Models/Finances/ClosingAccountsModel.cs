using DCMS.Core.Domain.Finances;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace DCMS.ViewModel.Models.Finances
{

    public partial class CheckAccountModel
    {

        public IList<ClosingAccountsModel> ClosingAccounts = new List<ClosingAccountsModel>();

    }

    public class CheckTask
    {
        public int Type { get; set; }
        public int Step { get; set; }
        public string Name { get; set; }
        public string Action { get; set; }
    }

    public partial class CheckOutModel : BaseModel
    {
        /// <summary>
        /// 结转当期
        /// </summary>
        public ClosingAccountsModel Period { get; set; } = new ClosingAccountsModel();

        /// <summary>
        /// 结转任务
        /// </summary>
        public CheckTask Task { get; set; } = new CheckTask();


        //成本

        /// <summary>
        /// 成本汇总
        /// </summary>
        public List<CostPriceSummery> CostPriceSummeries { get; set; } = new List<CostPriceSummery>();

        /// <summary>
        /// 销售类结转金额
        /// </summary>
        public decimal CostOfSales { get; set; } = 0;
        /// <summary>
        /// 库存损失类结转金额
        /// </summary>
        public decimal CostOfStockLoss { get; set; } = 0;
        /// <summary>
        /// 库存调整类结转金额
        /// </summary>
        public decimal CostOfStockAdjust { get; set; } = 0;
        /// <summary>
        /// 拆装组合类结转金额
        /// </summary>
        public decimal CostOfJointGoods { get; set; } = 0;
        /// <summary>
        /// 采购退货类结转金额
        /// </summary>
        public decimal CostOfPurchaseReject { get; set; } = 0;
        /// <summary>
        /// 价格调整类结转金额
        /// </summary>
        public decimal CostOfPriceAdjust { get; set; } = 0;



        //损益

        /// <summary>
        /// 收入类科目合计
        /// </summary>
        public decimal RevenueTotal { get; set; } = 0;
        /// <summary>
        /// 费用类科目合计
        /// </summary>
        public decimal ExpenseTotal { get; set; } = 0;
        /// <summary>
        /// 年末合计
        /// </summary>
        public decimal YearTotal { get; set; } = 0;


        /// <summary>
        /// 收入凭证
        /// </summary>
        [BindRequired]
        public RecordingVoucher IncomeVoucher { get; set; } = new RecordingVoucher();
        /// <summary>
        /// 收入凭证
        /// </summary>
        [BindRequired]
        public RecordingVoucher ExpenseVoucher { get; set; } = new RecordingVoucher();
        /// <summary>
        /// 年末凭证
        /// </summary>
        [BindRequired]
        public RecordingVoucher UnAllotProfirVoucher { get; set; } = new RecordingVoucher();

        /// <summary>
        /// 科目余额
        /// </summary>
        [BindRequired]
        public List<TrialBalance> TrialBalances { get; set; } = new List<TrialBalance>();


    }




    public partial class CostPriceSummeryListModel : BaseModel
    {
        public CostPriceSummeryListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
        }


        public string ProductName { get; set; }
        public int ProductId { get; set; }
        public SelectList Dates { get; set; }

        public string RecordTime { get; set; }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<CostPriceSummeryModel> Items { get; set; }
    }


    public partial class CostPriceChangeRecordsListModel : BaseModel
    {
        public CostPriceChangeRecordsListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
        }

        public string DateName { get; set; }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<CostPriceChangeRecordsModel> Items { get; set; }
    }



    public class ClosingAccountsModel : BaseEntityModel
    {
        /// <summary>
        /// 结转日期(例：2019-04)
        /// </summary>
        public DateTime ClosingAccountDate { get; set; }

        /// <summary>
        /// (锁账状态)
        /// </summary>
        public bool LockStatus { get; set; }

        /// <summary>
        /// 结账人
        /// </summary>
        public int? CheckUserId { get; set; }

        /// <summary>
        /// 是否已结
        /// </summary>
        public bool CheckStatus { get; set; }

        /// <summary>
        /// 结账时间
        /// </summary>
        public DateTime CheckDate { get; set; }

        /// <summary>
        /// 是否有前驱
        /// </summary>
        public virtual bool HasPrecursor { get; set; }

        /// <summary>
        /// 是否有后继
        /// </summary>
        public virtual bool HasSuccessor { get; set; }
    }


    /// <summary>
    /// 用于表示成本汇总表
    /// </summary>
    public class CostPriceSummeryModel : BaseEntityModel
    {
        /// <summary>
        /// 经销商Id
        /// </summary>
        //移除

        /// <summary>
        /// 结转日期
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// 商品编码
        /// </summary>
        public string ProductCode { get; set; }

        /// <summary>
        /// 商品Id
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// 商品名称
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        public int UnitId { get; set; }
        public string UnitName { get; set; }


        #region 期初

        /// <summary>
        /// 数量
        /// </summary>
        public int InitQuantity { get; set; }

        /// <summary>
        /// 价格
        /// </summary>
        public decimal InitPrice { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        public decimal InitAmount { get; set; }

        #endregion

        #region 收入

        /// <summary>
        /// 数量
        /// </summary>
        public int InQuantity { get; set; }

        /// <summary>
        /// 价格
        /// </summary>
        public decimal InPrice { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        public decimal InAmount { get; set; }

        #endregion

        #region 支出

        /// <summary>
        /// 数量
        /// </summary>
        public int OutQuantity { get; set; }

        /// <summary>
        /// 价格
        /// </summary>
        public decimal OutPrice { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        public decimal OutAmount { get; set; }

        #endregion

        #region 结存

        /// <summary>
        /// 数量
        /// </summary>
        public int EndQuantity { get; set; }

        /// <summary>
        /// 价格
        /// </summary>
        public decimal EndPrice { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        public decimal EndAmount { get; set; }

        #endregion

        /// <summary>
        /// 商品明细记录
        /// </summary>
        public IList<CostPriceChangeRecordsModel> Records { get; set; } = new List<CostPriceChangeRecordsModel>();

    }


    /// <summary>
    /// 用于表示成本变化明细记录
    /// </summary>
    public class CostPriceChangeRecordsModel : BaseEntityModel
    {

        /// <summary>
        /// 经销商Id
        /// </summary>
        //移除

        /// <summary>
        /// 汇总Id
        /// </summary>
        public int CostPriceSummeryId { get; set; }

        /// <summary>
        /// 日期
        /// </summary>
        public DateTime Date { get; set; }


        /// <summary>
        /// 单据类型
        /// </summary>
        public int BillTypeId { get; set; }
        public string BillTypeName { get; set; }

        /// <summary>
        /// 单据编号
        /// </summary>
        public string BillNumber { get; set; }

        /// <summary>
        /// 商品编码
        /// </summary>
        public string ProductCode { get; set; }

        /// <summary>
        /// 商品Id
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// 商品名称
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        public int UnitId { get; set; }
        public string UnitName { get; set; }

        #region 收入

        /// <summary>
        /// 数量
        /// </summary>
        public int InQuantity { get; set; }

        /// <summary>
        /// 成本单价
        /// </summary>
        public decimal InPrice { get; set; }

        /// <summary>
        /// 成本金额
        /// </summary>
        public decimal InAmount { get; set; }

        #endregion

        #region 支出

        /// <summary>
        /// 数量
        /// </summary>
        public int OutQuantity { get; set; }

        /// <summary>
        /// 成本单价
        /// </summary>
        public decimal OutPrice { get; set; }

        /// <summary>
        /// 成本金额
        /// </summary>
        public decimal OutAmount { get; set; }

        #endregion

        #region 结存

        /// <summary>
        /// 数量
        /// </summary>
        public int EndQuantity { get; set; }

        /// <summary>
        /// 成本单价
        /// </summary>
        public decimal EndPrice { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        public decimal EndAmount { get; set; }

        #endregion
    }
}
