using DCMS.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DCMS.Core.Domain.Configuration
{
    public class CompanySetting : ISettings
    {

        #region 商品精细化

        /// <summary>
        /// 单据生产日期功能
        /// </summary>
        public int OpenBillMakeDate { get; set; }

        /// <summary>
        /// 多单位商品价格
        /// </summary>
        public int MulProductPriceUnit { get; set; }

        /// <summary>
        /// 允许创建多个相同条码的商品
        /// </summary>
        public bool AllowCreateMulSameBarcode { get; set; }

        #endregion

        #region 开单选项

        /// <summary>
        /// 默认价格体系方案(默认成本价)
        /// </summary>
        public string DefaultPricePlan { get; set; } = "0_5";

        /// <summary>
        /// 政策商品默认价格（默认销售价）
        /// </summary>
        public string DefaultPolicyPrice { get; set; } = "0_2";
        /// <summary>
        /// 默认进价
        /// </summary>
        public int DefaultPurchasePrice { get; set; }

        /// <summary>
        /// 商品变价参考标准
        /// </summary>
        public int VariablePriceCommodity { get; set; }

        /// <summary>
        /// 单据合计取整精度
        /// </summary>
        public int AccuracyRounding { get; set; }

        /// <summary>
        /// 开单展示条码
        /// </summary>
        public int MakeBillDisplayBarCode { get; set; }

        /// <summary>
        /// 销售单/退货单交易日期只允许选择
        /// </summary>
        public int AllowSelectionDateRange { get; set; }

        /// <summary>
        /// 对接票证通系统
        /// </summary>
        public bool DockingTicketPassSystem { get; set; }

        /// <summary>
        /// 允许在销售订单和销售单中开退货商品
        /// </summary>
        public bool AllowReturnInSalesAndOrders { get; set; }
        /// <summary>
        /// 保存换货单后，系统自动审核
        /// </summary>
        public bool SubmitExchangeBillAutoAudits { get; set; }

        /// <summary>
        /// APP销售开单可指定送货员
        /// </summary>
        public bool AppMaybeDeliveryPersonnel { get; set; }


        /// <summary>
        /// App保存销售订单/退货订单后，系统自动审核
        /// </summary>
        public bool AppSubmitOrderAutoAudits { get; set; }


        /// <summary>
        /// App保存调拨单后，系统自动审核
        /// </summary>
        public bool AppSubmitTransferAutoAudits { get; set; }


        /// <summary>
        /// App保存费用支出单后，系统自动审核
        /// </summary>
        public bool AppSubmitExpenseAutoAudits { get; set; }

        /// <summary>
        /// App保存销售单/销售退货单后，系统自动审核
        /// </summary>
        public bool AppSubmitBillReturnAutoAudits { get; set; }

        /// <summary>
        /// App允许红冲单据
        /// </summary>
        public bool AppAllowWriteBack { get; set; }

        /// <summary>
        /// 允许预收款支付成负数
        /// </summary>
        public bool AllowAdvancePaymentsNegative { get; set; }

        /// <summary>
        /// 只展示已开预收款单的预收款账户
        /// </summary>
        public bool ShowOnlyPrepaidAccountsWithPrepaidReceipts { get; set; }


        /// <summary>
        /// 分口味核算库存商品，只打印主商品
        /// </summary>
        public bool TasteByTasteAccountingOnlyPrintMainProduct { get; set; }

        /// <summary>
        /// APP提交收款单后，系统自动审核
        /// </summary>
        public bool AutoApproveConsumerPaidBill { get; set; }

        #endregion

        #region 库存管理

        /// <summary>
        /// App开销售单时只展示有库存商品
        /// </summary>
        public bool APPOnlyShowHasStockProduct { get; set; }

        /// <summary>
        /// 开单时显示订单占用库存
        /// </summary>
        public bool APPShowOrderStock { get; set; }


        #endregion

        #region 业务员管理

        //业务员在店时间
        public int OnStoreStopSeconds { get; set; }

        //业务员轨迹实时上报 定位时间段
        public bool EnableSalesmanTrack { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public string Start { get; set; }
        //public int Start { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public string End { get; set; }
        //public int End { get; set; }

        /// <summary>
        /// 定位轨迹上报频率60s
        /// </summary>
        public int FrequencyTimer { get; set; }

        /// <summary>
        /// 业务员只能看到自己片区的客户
        /// </summary>
        public bool SalesmanOnlySeeHisCustomer { get; set; }

        /// <summary>
        /// 业务员必须先拜访门店才能开单
        /// </summary>
        public bool SalesmanVisitStoreBefore { get; set; }

        /// <summary>
        /// 拜访必须拍门头照片
        /// </summary>
        public bool SalesmanVisitMustPhotographed { get; set; }

        /// <summary>
        /// 签收距离
        /// </summary>
        public double SalesmanDeliveryDistance { get; set; }

        /// <summary>
        /// 门头照片数
        /// </summary>
        public int DoorheadPhotoNum { get; set; } = 1;

        /// <summary>
        /// 陈列照片数
        /// </summary>
        public int DisplayPhotoNum { get; set; } = 4;
        /// <summary>
        /// 业务开展时间段是否启用
        /// </summary>
        public bool EnableBusinessTime { get; set; }
        /// <summary>
        /// 业务开展开始时间
        /// </summary>
        public string BusinessStart { get; set; }
        /// <summary>
        /// 业务开展结束时间
        /// </summary>
        public string BusinessEnd { get; set; }
        /// <summary>
        /// 业务员线路启用
        /// </summary>
        public bool EnableBusinessVisitLine { get; set; } = false;


        public List<SalesmanManagement> SalesmanManagements { get; set; } = new List<SalesmanManagement>();

        #endregion

        #region 财务管理


        /// <summary>
        /// 参考成本价
        /// </summary>
        public int ReferenceCostPrice { get; set; }

        /// <summary>
        /// 进货平均价计算历史次数
        /// </summary>
        public int AveragePurchasePriceCalcNumber { get; set; } = 5;

        /// <summary>
        /// 允许负库存月结
        /// </summary>
        public bool AllowNegativeInventoryMonthlyClosure { get; set; }


        #endregion

        #region 其他设置

        /// <summary>
        /// 启用税务功能
        /// </summary>
        public bool EnableTaxRate { get; set; }

        /// <summary>
        /// 税率
        /// </summary>
        public double TaxRate { get; set; }


        /// <summary>
        /// 门头陈列照片水印增加内容
        /// </summary>
        public string PhotographedWater { get; set; }

        #endregion

        #region 清除数据

        /// <summary>
        /// 档案数据
        /// </summary>
        public bool ClearArchiveDatas { get; set; }

        /// <summary>
        /// 单据数据
        /// </summary>
        public bool ClearBillDatas { get; set; }


        #endregion

        #region 公共参数

        #endregion


    }



    public class SalesmanManagement
    {
        #region 业务员管理

        public int UserId { get; set; } = 0;
        public string UserName { get; set; }

        public int OnStoreStopSeconds { get; set; } = 0;

        public bool EnableSalesmanTrack { get; set; }

        public string Start { get; set; }

        public string End { get; set; }

        public int FrequencyTimer { get; set; } = 0;

        public bool SalesmanOnlySeeHisCustomer { get; set; }

        public bool SalesmanVisitStoreBefore { get; set; }
        public bool SalesmanVisitMustPhotographed { get; set; }

        public double SalesmanDeliveryDistance { get; set; }

        public int DoorheadPhotoNum { get; set; } = 1;
        public int DisplayPhotoNum { get; set; } = 4;
        public bool EnableBusinessTime { get; set; }
        public string BusinessStart { get; set; }

        public string BusinessEnd { get; set; }
        public bool EnableBusinessVisitLine { get; set; } = false;

        #endregion


    }

}
