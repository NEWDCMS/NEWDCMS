using System.ComponentModel;

namespace DCMS.Core
{

    /// <summary>
    /// 用户登录状态枚举
    /// </summary>
    public enum DCMSStatusCode : int
    {
        /// <summary>
        /// 正常
        /// </summary>
        Normal = 0,
        /// <summary>
        /// 成功
        /// </summary>
        Successful = 1,
        /// <summary>
        /// 失败
        /// </summary>
        Failed = -1,

        #region 用户相关
        /// <summary>
        /// 账户信息不存在（电子邮箱/用户名/手机号无效）
        /// </summary>
        UserNotExist = 1001,
        /// <summary>
        /// 该用户已被删除
        /// </summary>
        Deleted = 1002,
        /// <summary>
        /// 帐户尚未激活
        /// </summary>
        NotActive = 1003,
        /// <summary>
        /// 用户该没有注册
        /// </summary>
        NotRegistered = 1004,
        /// <summary>
        /// 密码错误
        /// </summary>
        WrongPassword = 1005,
        /// <summary>
        /// 未知错误
        /// </summary>
        None = 1006,
        /// <summary>
        /// 重复设备登录
        /// </summary>
        RepeatDeviceLogin = 1008,
        /// <summary>
        /// 锁定
        /// </summary>
        LockedOut = 1009
        #endregion
    }


    #region 档案
    [Description("渠道档案管理中，渠道预设属性字段")]
    public enum ChannelAttributeType
    {
        [Description("现代零售")]
        MR = 1,
        [Description("现饮")]
        RTD = 2,
        [Description("非现饮")]
        NN = 3
    }
    [Description("终端管理中，付款方式")]
    public enum PaymentMethodType
    {
        [Description("现结")]
        XianJie = 1,
        [Description("挂账")]
        GuaZhang = 2
    }

    public enum TerminalDataType
    {
        [Description("客户")]
        Terminal = 1,
        [Description("供应商")]
        Manufacturer = 2
    }



    #endregion

    #region 仓库
    [Description("仓库档案管理中，仓库类型字段")]
    public enum WareHouseType
    {
        [Description("仓库")]
        Normal = 1,
        [Description("车辆")]
        Car = 2
    }

    [Description("调拨单管理中，调拨单状态字段")]
    public enum TransferStatus
    {
        [Description("待审核")]
        DaiShenHe = 0,
        [Description("已审核")]
        YiShenHe = 1,
        [Description("红冲")]
        HongChong = 2
    }

    [Description("盘点盈亏单管理中，盈亏单状态字段")]
    public enum StockAdjustStatus
    {
        [Description("待审核")]
        DaiShenHe = 0,
        [Description("已审核")]
        YiShenHe = 1,
        [Description("红冲")]
        HongChong = 2
    }

    [Description("盘点盈亏商品中，盈亏类型")]
    public enum StockAdjustDetailTypeEnum
    {
        [Description("亏损")]
        KuiSun = 0,
        [Description("盈利")]
        YingLi = 1,
    }

    [Description("报损单管理中，报损单状态字段")]
    public enum DamagedStatus
    {
        [Description("待审核")]
        DaiShenHe = 0,
        [Description("已审核")]
        YiShenHe = 1,
        [Description("红冲")]
        HongChong = 2
    }
    [Description("调价单管理中，调价单状态字段")]
    public enum ChangePriceStatus
    {
        [Description("待审核")]
        DaiShenHe = 0,
        [Description("已审核")]
        YiShenHe = 1,
        [Description("红冲")]
        HongChong = 2
    }

    [Description("盘点单状态（整仓、部分），盘点单状态（整仓、部分）")]
    public enum InventorysetStatus
    {
        /// <summary>
        /// 进行中
        /// </summary>
        [Description("进行中")]
        Pending = 1,

        /// <summary>
        /// 已结束
        /// </summary>
        [Description("已结束")]
        Completed = 2,

        /// <summary>
        /// 已取消
        /// </summary>
        [Description("已取消")]
        Canceled = 3

    }

    /// <summary>
    /// 库存进出库类型
    /// </summary>
    public enum DirectionEnum
    {
        /// <summary>
        /// 无
        /// </summary>
        [Description("无")]
        Null = 0,

        /// <summary>
        /// 入
        /// </summary>
        [Description("入")]
        In = 1,

        /// <summary>
        /// 出
        /// </summary>
        [Description("出")]
        Out = 2
    }

    /// <summary>
    /// 库存数量类型
    /// </summary>
    public enum StockQuantityType
    {
        /// <summary>
        /// 可用库存量
        /// </summary>
        [Description("可用库存量")]
        UsableQuantity = 1,

        /// <summary>
        /// 现货库存量
        /// </summary>
        [Description("现货库存量")]
        CurrentQuantity = 2,

        /// <summary>
        /// 预占库存量
        /// </summary>
        [Description("预占库存量")]
        OrderQuantity = 3,

        /// <summary>
        /// 锁定库存量
        /// </summary>
        [Description("锁定库存量")]
        LockQuantity = 4,
    }

    /// <summary>
    /// 库存数量乘数类型(-1 数量减少，0 清空数量，1 数量增加)
    /// </summary>
    public enum StockFlowMultipleEnum
    {
        /// <summary>
        /// -1
        /// </summary>
        Negative = -1,

        /// <summary>
        /// 0
        /// </summary>
        Zero = 0,

        /// <summary>
        /// 1
        /// </summary>
        Positive = 1,
    }


    /// <summary>
    /// 库存流水变更类型
    /// </summary>
    public enum StockFlowChangeTypeEnum
    {
        /// <summary>
        /// 保存
        /// </summary>
        Save = 1,

        /// <summary>
        /// 审核
        /// </summary>
        Audited = 2,

        /// <summary>
        /// 红冲
        /// </summary>
        Reversed = 3

    }



    public enum PriceShowType
    {
        /// <summary>
        /// 按照商品档案中预设的批发价进行计算
        /// </summary>
        [Description("售价")]
        TradePrice = 0,

        /// <summary>
        /// 按照现在此商品在库存中的平均价格计算
        /// </summary>
        [Description("现在加权平均价")]
        AveragePrice = 1,

        /// <summary>
        /// 按照商品档案中预设的进价进行计算
        /// </summary>
        [Description("现在预设进价")]
        PurchasePrice = 2,

    }

    public enum UnitShowType
    {
        /// <summary>
        /// 基本单位
        /// </summary>
        [Description("基本单位")]
        BaseUnit = 0,

        /// <summary>
        /// 大包单位
        /// </summary>
        [Description("大包单位")]
        BigUnit = 1,

    }

    #endregion

    #region 单据类型

    public enum BillStates
    {
        [Description("草稿")]
        Draft = 0,
        [Description("已审核")]
        Audited = 1,
        [Description("已红冲")]
        Reversed = 2
    }

    [Description("操作源")]
    public enum OperationEnum
    {
        [Description("PC")]
        PC = 0,
        [Description("APP")]
        APP = 1,
    }

    /// <summary>
    /// 单据类型
    /// </summary>
    [Description("单据类型")]
    public enum BillTypeEnum
    {
        None = 0,

        #region 销售

        [Description("换货单,EB")]
        ExchangeBill = 10,

        /// <summary>
        /// 销售订单
        /// </summary>
        [Description("销售订单,XD")]
        SaleReservationBill = 11,


        /// <summary>
        /// 销售单
        /// </summary>
        [Description("销售单,XS")]
        SaleBill = 12,

        /// <summary>
        /// 退货订单
        /// </summary>
        [Description("退货订单,TD")]
        ReturnReservationBill = 13,

        /// <summary>
        /// 退货单
        /// </summary>
        [Description("退货单,TH")]
        ReturnBill = 14,

        /// <summary>
        /// 车辆对货单
        /// </summary>
        [Description("车辆对货单,CG")]
        CarGoodBill = 15,

        /// <summary>
        /// 收款对账
        /// </summary>
        [Description("收款对账,FRA")]
        FinanceReceiveAccount = 16,

        /// <summary>
        /// 仓库分拣
        /// </summary>
        [Description("仓库分拣,PB")]
        PickingBill = 17,

        /// <summary>
        /// 装车调度单
        /// </summary>
        [Description("装车调度单,DD")]
        DispatchBill = 18,

        /// <summary>
        /// 订单转销售单
        /// </summary>
        [Description("订单转销售单,CR")]
        ChangeReservation = 19,

        #endregion

        #region 采购

        /// <summary>
        /// 采购订单
        /// </summary>
        [Description("采购订单,CD")]
        PurchaseReservationBill = 21,

        /// <summary>
        /// 采购单
        /// </summary>
        [Description("采购单,CG")]
        PurchaseBill = 22,

        /// <summary>
        /// 采购退货订单
        /// </summary>
        [Description("采购退货订单,CT")]
        PurchaseReturnReservationBill = 23,

        /// <summary>
        /// 采购退货单
        /// </summary>
        [Description("采购退货单,TG")]
        PurchaseReturnBill = 24,

        #endregion

        #region 仓库

        /// <summary>
        /// 调拨单
        /// </summary>
        [Description("调拨单,DBD")]
        AllocationBill = 31,

        /// <summary>
        /// 盘点盈亏单
        /// </summary>
        [Description("盘点盈亏单,PDYK")]
        InventoryProfitLossBill = 32,

        /// <summary>
        /// 成本调价单
        /// </summary>
        [Description("成本调价单,TJD")]
        CostAdjustmentBill = 33,

        /// <summary>
        /// 报损单
        /// </summary>
        [Description("报损单,BSD")]
        ScrapProductBill = 34,

        /// <summary>
        /// 盘点单（整仓）
        /// </summary>
        [Description("盘点单（整仓）,PDD-ALL")]
        InventoryAllTaskBill = 35,

        /// <summary>
        /// 盘点单（部分）
        /// </summary>
        [Description("盘点单（部分）,PDD-PART")]
        InventoryPartTaskBill = 36,

        /// <summary>
        /// 组合单
        /// </summary>
        [Description("组合单,ZHD")]
        CombinationProductBill = 37,

        /// <summary>
        /// 拆分单
        /// </summary>
        [Description("拆分单,CFD")]
        SplitProductBill = 38,

        /// <summary>
        /// 门店库存上报
        /// </summary>
        [Description("门店库存上报,KCSB")]
        InventoryReportBill = 39,

        #endregion

        #region 财务

        /// <summary>
        /// 收款单
        /// </summary>
        [Description("收款单,SK")]
        CashReceiptBill = 41,

        /// <summary>
        /// 付款单
        /// </summary>
        [Description("付款单,FK")]
        PaymentReceiptBill = 42,

        /// <summary>
        /// 预收款单
        /// </summary>
        [Description("预收款单,YSK")]
        AdvanceReceiptBill = 43,

        /// <summary>
        /// 预付款单
        /// </summary>
        [Description("预付款单,YFK")]
        AdvancePaymentBill = 44,

        /// <summary>
        /// 费用支出
        /// </summary>
        [Description("费用支出,FYZC")]
        CostExpenditureBill = 45,

        /// <summary>
        /// 费用合同
        /// </summary>
        [Description("费用合同,FYHT")]
        CostContractBill = 46,

        /// <summary>
        /// 其他收入
        /// </summary>
        [Description("其他收入,CWSR")]
        FinancialIncomeBill = 47,

        #endregion

        /// <summary>
        /// 整车装车单
        /// </summary>
        [Description("整车装车单,ALB")]
        AllLoadBill = 48,

        /// <summary>
        /// 拆零装车单
        /// </summary>
        [Description("拆零装车单,ZLB")]
        ZeroLoadBill = 49,

        /// <summary>
        /// 整箱拆零合并单
        /// </summary>
        [Description("整箱拆零合并单,AMB")]
        AllZeroMergerBill = 50,

        /// <summary>
        /// 记账凭证
        /// </summary>
        [Description("记账凭证,AVB")]
        AccountingVoucher = 51,

        /// <summary>
        /// 库存表
        /// </summary>
        [Description("库存表")]
        StockReport = 52,

        /// <summary>
        /// 销售汇总(客户/商品)
        /// </summary>
        [Description("销售汇总(客户/商品)")]
        SaleSummeryReport = 53,

        /// <summary>
        /// 调拨汇总表
        /// </summary>
        [Description("调拨汇总表")]
        TransferSummaryReport = 54,

        /// <summary>
        /// 销售汇总(商品)
        /// </summary>
        [Description("销售汇总(商品)")]
        SaleSummeryProductReport = 55,

        /// <summary>
        /// 录入凭证
        /// </summary>
        [Description("录入凭证,PZ")]
        RecordingVoucher = 56,

        /// <summary>
        /// 其他类型单据
        /// </summary>
        [Description("其他类型单据,QT")]
        Other = 99,


    }

    /// <summary>
    /// 凭证来源
    /// </summary>
    public enum VouchSourceEnum : int
    {
        /// <summary>
        /// 手工生成
        /// </summary>
        [Description("手工生成")]
        Manual = 0,
        /// <summary>
        /// 系统生成
        /// </summary>
        [Description("系统生成")]
        Auto = 1,
        /// <summary>
        /// 应收系统
        /// </summary>
        [Description("应收系统")]
        receivable = 2,
        /// <summary>
        /// 应付系统
        /// </summary>
        [Description("应付系统")]
        Cope = 3,
        /// <summary>
        /// 固定资产
        /// </summary>
        [Description("固定资产")]
        Fixed = 7,
        /// <summary>
        /// 工资系统
        /// </summary>
        [Description("工资系统")]
        Wage = 5,
    }

    /// <summary>
    /// 单据类型(用于报表)
    /// </summary>
    public enum BillTypeReportEnum
    {
        [Description("采购")]
        Purchase = 22,

        [Description("退购")]
        Return = 24,

        [Description("销售")]
        Sales = 12,

        [Description("退售")]
        SalesReturn = 14,

        [Description("组合")]
        Combination = 37,

        [Description("拆分")]
        Split = 38,

        [Description("调入")]
        Input = 311,

        [Description("调出")]
        Callout = 312,

        [Description("盘盈")]
        Trading = 321,

        [Description("盘亏")]
        Inventory = 322,

        [Description("报损")]
        Damage = 34
    }

    //销售单	退货单	销售订单	退货订单	调拨单	采购单	采购退货单	盘点单	借货单	还货单
    public enum WHAEnum
    {
        /// <summary>
        /// 换货单
        /// </summary>
        [Description("换货单")]
        ExchangeBill = 10,

        /// <summary>
        /// 销售订单
        /// </summary>
        [Description("销售订单")]
        SaleReservationBill = 11,

        /// <summary>
        /// 销售单
        /// </summary>
        [Description("销售单")]
        SaleBill = 12,

        /// <summary>
        /// 退货订单
        /// </summary>
        [Description("退货订单")]
        ReturnReservationBill = 13,

        /// <summary>
        /// 退货单
        /// </summary>
        [Description("退货单")]
        ReturnBill = 14,

        /// <summary>
        /// 采购单
        /// </summary>
        [Description("采购单")]
        PurchaseBill = 22,

        /// <summary>
        /// 采购退货单
        /// </summary>
        [Description("采购退货单")]
        PurchaseReturnBill = 24,

        /// <summary>
        /// 调拨单
        /// </summary>
        [Description("调拨单")]
        AllocationBill = 31,

        /// <summary>
        /// 盘点单
        /// </summary>
        [Description("盘点单")]
        InventoryAllTaskBill = 35,

        /// <summary>
        /// 借货单
        /// </summary>
        [Description("借货单")]
        LoanGoodsBill = 77,

        /// <summary>
        /// 还货单（整仓）
        /// </summary>
        [Description("还货单")]
        ReturnGoodsBill = 78
    }

    #endregion

    #region 销售

    #region 销售订单

    /// <summary>
    /// 付款方式
    /// </summary>
    public enum SaleReservationBillPayType
    {
        /// <summary>
        /// 现结
        /// </summary>
        [Description("现结")]
        CashTerm = 1,

        /// <summary>
        /// 挂账
        /// </summary>
        [Description("挂账")]
        OnAccount = 2,
    }


    /// <summary>
    /// 销售订单查询条件过滤
    /// </summary>
    public enum SaleReservationBillFilter
    {
        [Description("按审核时间")]
        AuditingDate = 0,

        [Description("显示红冲数据")]
        RedData = 1,

        [Description("退货订单")]
        ReturnOrder = 2,

        [Description("已转订单")]
        ChangeOrder = 3,

    }
    #endregion

    #region 销售单

    /// <summary>
    /// 销售单查询条件过滤
    /// </summary>
    public enum SaleBillFilter
    {
        /// <summary>
        /// 按审核时间
        /// </summary>
        [Description("按审核时间")]
        AuditingDate = 0,

        /// <summary>
        /// 显示红冲数据
        /// </summary>
        [Description("显示红冲数据")]
        RedData = 1,

        /// <summary>
        /// 显示退货单
        /// </summary>
        [Description("显示退货单")]
        ReturnOrder = 2,

    }

    /// <summary>
    /// 销售单支付方式
    /// </summary>
    [Description("销售单支付方式")]
    public enum SaleBillPaymentMethodType
    {
        /// <summary>
        /// 已收款单据
        /// </summary>
        [Description("已收款单据")]
        AlreadyBill = 1,

        /// <summary>
        /// 欠款单据
        /// </summary>
        [Description("欠款单据")]
        OweBill = 2,

    }

    /// <summary>
    /// 销售单单据来源
    /// </summary>
    [Description("销售单单据来源")]
    public enum SaleBillSourceType
    {
        [Description("订单转成")]
        Order = 1,

        [Description("非订单转成")]
        UnOrder = 2,
    }

    #endregion

    #region 退货订单

    /// <summary>
    /// 退货单付款方式
    /// </summary>
    [Description("退货单支付方式")]
    public enum ReturnReservationBillPayType
    {
        /// <summary>
        /// 现结
        /// </summary>
        [Description("现结")]
        CashTerm = 1,

        /// <summary>
        /// 挂账
        /// </summary>
        [Description("挂账")]
        OnAccount = 2,

    }

    /// <summary>
    /// 退货订单查询条件过滤
    /// </summary>
    public enum ReturnReservationBillFilter
    {
        /// <summary>
        /// 按审核时间
        /// </summary>
        [Description("按审核时间")]
        AuditingDate = 0,

        /// <summary>
        /// 显示红冲数据
        /// </summary>
        [Description("显示红冲数据")]
        RedData = 1,

        /// <summary>
        /// 已转订单
        /// </summary>
        [Description("已转订单")]
        ChangeOrder = 3,

    }


    #endregion

    #region 退货单

    /// <summary>
    /// 付款方式
    /// </summary>
    public enum ReturnBillPayType
    {
        /// <summary>
        /// 现结
        /// </summary>
        [Description("现结")]
        CashTerm = 1,

        /// <summary>
        /// 挂账
        /// </summary>
        [Description("挂账")]
        OnAccount = 2,
    }

    /// <summary>
    /// 退货单查询条件过滤
    /// </summary>
    public enum ReturnBillFilter
    {
        /// <summary>
        /// 按审核时间
        /// </summary>
        [Description("按审核时间")]
        AuditingDate = 0,

        /// <summary>
        /// 显示红冲数据
        /// </summary>
        [Description("显示红冲数据")]
        RedData = 1,

        /// <summary>
        /// 已转订单
        /// </summary>
        [Description("已转订单")]
        ChangeOrder = 3,

    }

    /// <summary>
    /// 退货单支付方式
    /// </summary>
    [Description("退货单支付方式")]
    public enum ReturnBillPaymentMethodType
    {
        /// <summary>
        /// 已收款单据
        /// </summary>
        [Description("已收款单据")]
        AlreadyBill = 1,

        /// <summary>
        /// 欠款单据
        /// </summary>
        [Description("欠款单据")]
        OweBill = 2,

    }

    /// <summary>
    /// 退货单单据来源
    /// </summary>
    [Description("退货单单据来源")]
    public enum ReturnBillSourceType
    {
        /// <summary>
        /// 订单转成
        /// </summary>
        [Description("订单转成")]
        Order = 1,

        /// <summary>
        /// 非订单转成
        /// </summary>
        [Description("非订单转成")]
        UnOrder = 2,

    }

    #endregion

    #region 收款对账单
    /// <summary>
    /// 收款对账单状态
    /// </summary>
    [Description("收款对账单状态")]
    public enum FinanceReceiveAccountStatus
    {
        /// <summary>
        /// 未上交
        /// </summary>
        [Description("未上交")]
        NotHandedIn = 0,

        /// <summary>
        /// 已上交
        /// </summary>
        [Description("已上交")]
        HandedIn = 1,

        /// <summary>
        /// 已撤销
        /// </summary>
        [Description("已撤销")]
        Cancled = 2,

    }
    #endregion

    #region 仓库分拣
    /// <summary>
    /// 整箱拆零合并拣货状态
    /// </summary>
    [Description("整箱拆零合并拣货状态")]
    public enum PickingWholeScrapStatus
    {
        /// <summary>
        /// 待整箱合并拆零拣货
        /// </summary>
        [Description("待整箱合并拆零拣货")]
        WaitWholeScrap = 1,

        /// <summary>
        /// 已整箱合并拆零拣货
        /// </summary>
        [Description("已整箱合并拆零拣货")]
        AlreadyWholeScrap = 2,

    }

    /// <summary>
    /// 拆零拣货状态
    /// </summary>
    [Description("拆零拣货状态")]
    public enum PickingScrapStatus
    {
        /// <summary>
        /// 待拆零拣货
        /// </summary>
        [Description("待拆零拣货")]
        WaitScrap = 1,

        /// <summary>
        /// 待整箱拣货
        /// </summary>
        [Description("待整箱拣货")]
        WaitWhole = 2,

        /// <summary>
        /// 已拆零拣货
        /// </summary>
        [Description("已拆零拣货")]
        AlreadyScrap = 3,

        /// <summary>
        /// 已整箱拣货
        /// </summary>
        [Description("已整箱拣货")]
        AlreadyWhole = 4,
    }

    /// <summary>
    /// 仓库分拣查询条件过滤
    /// </summary>
    public enum PickingFilter
    {
        /// <summary>
        /// 整箱拆零合并拣货
        /// </summary>
        [Description("整箱拆零合并拣货")]
        WholeScrap = 1,
    }


    #endregion

    #region 装车调度
    /// <summary>
    /// 装车调度状态
    /// </summary>
    [Description("装车调度状态")]
    public enum DispatchStatus
    {
        /// <summary>
        /// 待调度
        /// </summary>
        [Description("待调度")]
        WaitDispatch = 0,

        /// <summary>
        /// 已调度
        /// </summary>
        [Description("已调度")]
        AlreadyDispatch = 1,

    }


    /// <summary>
    /// 装车调度查询条件过滤
    /// </summary>
    public enum DispatchFilter
    {
        /// <summary>
        /// 展示红冲的调度单
        /// </summary>
        [Description("展示红冲的调度单")]
        ShowRedDispatch = 1,
    }

    /// <summary>
    /// 生成调拨单
    /// </summary>
    public enum DispatchBillAutoAllocationEnum
    {
        /// <summary>
        /// 不自动生成调拨单
        /// </summary>
        [Description("不自动生成调拨单")]
        UnAuto = 1,

        /// <summary>
        /// 自动生成调拨单
        /// </summary>
        [Description("自动生成调拨单")]
        Auto = 2
    }

    /// <summary>
    /// 创建装车调度时打印
    /// </summary>
    public enum DispatchBillCreatePrintEnum
    {
        /// <summary>
        /// 打印整箱拆零合并单
        /// </summary>
        [Description("打印整箱拆零合并单")]
        PrintWholeScrap = 1,

        /// <summary>
        /// 打印整箱装车单
        /// </summary>
        [Description("打印整箱装车单")]
        PrintWholeCar = 2,

        /// <summary>
        /// 为每个客户打印拆零装车单
        /// </summary>
        [Description("为每个客户打印拆零装车单")]
        PrintEveryScrapCar = 3,

        /// <summary>
        /// 打印订单
        /// </summary>
        [Description("打印订单")]
        PrintOrder = 4
    }


    #endregion

    #region 订单转销售单

    [Description("订单转销售单单据类型")]
    public enum ChangeBillTypeEnum
    {
        /// <summary>
        /// 销售订单
        /// </summary>
        [Description("销售订单,XD")]
        SaleReservationBill = 11,

        /// <summary>
        /// 退货订单
        /// </summary>
        [Description("退货订单,TD")]
        ReturnReservationBill = 13,
    }

    /// <summary>
    /// 订单转销售单查询条件过滤
    /// </summary>
    public enum ChangeReservationFilter
    {
        /// <summary>
        /// 未转订单
        /// </summary>
        [Description("未转订单")]
        UnChange = 1,

        /// <summary>
        /// 已调度订单
        /// </summary>
        [Description("已调度订单")]
        Dispatch = 2,
    }



    #endregion


    #region 销售报表

    #region 销售明细表
    /// <summary>
    /// 查询条件过滤
    /// </summary>
    public enum SaleReportItemFilter
    {
        /// <summary>
        /// 费用合同兑现商品
        /// </summary>
        [Description("费用合同兑现商品")]
        CostContractProduct = 1,
    }

    /// <summary>
    /// 销售类型
    /// </summary>
    public enum SaleReportItemSaleTypeEnum
    {
        /// <summary>
        /// 销售商品
        /// </summary>
        [Description("销售商品")]
        SaleProduct = 1,

        /// <summary>
        /// 退货商品
        /// </summary>
        [Description("退货商品")]
        ReturnProduct = 2,

    }

    /// <summary>
    /// 支付方式
    /// </summary>
    public enum SaleReportItemPayTypeEnum
    {
        /// <summary>
        /// 已收款销售单
        /// </summary>
        [Description("已收款销售单")]
        AlreadyReceiptCash = 1,

        /// <summary>
        /// 欠款销售单
        /// </summary>
        [Description("欠款销售单")]
        Overdraft = 2,
    }

    #endregion

    #region 销售汇总（按商品）
    /// <summary>
    /// 查询条件过滤
    /// </summary>
    public enum SaleReportSummaryProductFilter
    {
        /// <summary>
        /// 费用合同兑现商品
        /// </summary>
        [Description("费用合同兑现商品")]
        CostContractProduct = 1,
    }

    /// <summary>
    /// 支付方式
    /// </summary>
    public enum SaleReportSummaryProductPayTypeEnum
    {
        /// <summary>
        /// 已收款销售单
        /// </summary>
        [Description("已收款销售单")]
        AlreadyReceiptCash = 1,

        /// <summary>
        /// 欠款销售单
        /// </summary>
        [Description("欠款销售单")]
        Overdraft = 2,
    }

    #endregion

    #region 订单明细


    /// <summary>
    /// 销售类型
    /// </summary>
    public enum SaleReportOrderItemSaleTypeEnum
    {
        /// <summary>
        /// 销售商品
        /// </summary>
        [Description("销售商品")]
        SaleProduct = 11,

        /// <summary>
        /// 退货商品
        /// </summary>
        [Description("退货商品")]
        ReturnProduct = 12,

    }

    /// <summary>
    /// 查询条件过滤
    /// </summary>
    public enum SaleReportOrderItemFilter
    {
        /// <summary>
        /// 费用合同兑现商品
        /// </summary>
        [Description("费用合同兑现商品")]
        CostContractProduct = 1,

        /// <summary>
        /// 只展示占用库存商品
        /// </summary>
        [Description("只展示占用库存商品")]
        IccupyStock = 2,

    }
    #endregion


    #region 订单汇总（按商品）
    /// <summary>
    /// 查询条件过滤
    /// </summary>
    public enum SaleReportSummaryOrderProductFilter
    {
        /// <summary>
        /// 费用合同兑现商品
        /// </summary>
        [Description("费用合同兑现商品")]
        CostContractProduct = 1,
    }

    #endregion

    #region 费用合同明细表

    /// <summary>
    /// 兑现方式
    /// </summary>
    public enum CostContractItemCashTypeEnum
    {
        /// <summary>
        /// 商品
        /// </summary>
        [Description("商品")]
        Product = 0,

        /// <summary>
        /// 现金
        /// </summary>
        [Description("现金")]
        Cash = 1,

    }

    /// <summary>
    /// 状态
    /// </summary>
    public enum CostContractStatusEnum
    {
        /// <summary>
        /// 正常
        /// </summary>
        [Description("正常")]
        Normal = 0,

        /// <summary>
        /// 已终止
        /// </summary>
        [Description("已终止")]
        Termination = 1,
    }

    /// <summary>
    /// 调拨明细审核状态
    /// </summary>
    public enum StatusEnum
    {
        /// <summary>
        /// 已审核
        /// </summary>
        [Description("已审核")]
        approved = 1,

        /// <summary>
        /// 未审核
        /// </summary>
        [Description("未审核")]
        audit = 0,
    }
    #endregion

    #region 销量走势图

    /// <summary>
    /// 销售类型
    /// </summary>
    public enum SaleReportSaleQuantityTrendGroupByTypeEnum
    {
        /// <summary>
        /// 日
        /// </summary>
        [Description("日")]
        Day = 1,

        /// <summary>
        /// 星期
        /// </summary>
        [Description("星期")]
        Week = 2,

        /// <summary>
        /// 月
        /// </summary>
        [Description("月")]
        Month = 3

    }

    #endregion


    public enum StatisticalTypeEnum
    {
        /// <summary>
        /// 商品其他统计类别
        /// </summary>
        [Description("商品其他统计类别")]
        OtherTypeId = 999
    }



    #endregion


    #region 销售商品类型
    /// <summary>
    /// 销售商品类型
    /// </summary>
    [Description("销售商品类型")]
    public enum SaleProductTypeEnum
    {
        /// <summary>
        /// 促销活动销售商品
        /// </summary>
        [Description("促销活动销售商品")]
        CampaignBuyProduct = 1,

        /// <summary>
        /// 促销活动赠送商品
        /// </summary>
        [Description("促销活动赠送商品")]
        CampaignGiveProduct = 2,

        /// <summary>
        /// 费用合同按月兑付
        /// </summary>
        [Description("按月兑付")]
        CostContractByMonth = 3,

        /// <summary>
        /// 费用合同按单位量总计兑付
        /// </summary>
        [Description("按单位量总计兑付")]
        CostContractUnitQuantity = 4,

        /// <summary>
        /// 费用合同从主管赠品扣减
        /// </summary>
        [Description("从主管赠品扣减")]
        CostContractManageGift = 5,

    }
    #endregion


    #endregion

    #region 采购

    #region 采购单
    /// <summary>
    /// 采购单查询条件过滤
    /// </summary>
    public enum PurchaseFilter
    {
        /// <summary>
        /// 按审核时间
        /// </summary>
        [Description("按审核时间")]
        AuditingDate = 0,

        /// <summary>
        /// 显示红冲数据
        /// </summary>
        [Description("显示红冲数据")]
        RedData = 1,

    }
    #endregion

    #region 采购退货单
    /// <summary>
    /// 采购退货单查询条件过滤
    /// </summary>
    public enum PurchaseReturnFilter
    {
        /// <summary>
        /// 按审核时间
        /// </summary>
        [Description("按审核时间")]
        AuditingDate = 0,

        /// <summary>
        /// 显示红冲数据
        /// </summary>
        [Description("显示红冲数据")]
        RedData = 1,

    }
    #endregion

    #endregion

    #region 财务

    /// <summary>
    /// 收款状态
    /// </summary>
    [Description("收款状态")]
    public enum ReceiptStatus : int
    {
        [Description("未收款|danger")]
        None = 0,
        [Description("部分收|warning")]
        Part = 1,
        [Description("已收款|primary")]
        Received = 2,
    }


    /// <summary>
    /// 付款状态
    /// </summary>
    [Description("付款状态")]
    public enum PayStatus : int
    {
        [Description("未付款|danger")]
        None = 0,
        [Description("部分付|warning")]
        Part = 1,
        [Description("已付款|primary")]
        Paid = 2,
    }


    /// <summary>
    /// 凭证生成方式
    /// </summary>
    public enum GenerateMode : int
    {
        /// <summary>
        /// 手工录入
        /// </summary>
        [Description("手工录入")]
        Manual = 0,

        /// <summary>
        /// 自动生成
        /// </summary>
        [Description("自动生成")]
        Auto = 1,
    }

    /// <summary>
    /// 费用合同类型
    /// </summary>
    public enum ContractTypeEnum
    {
        //合同类型(0:按月兑付,1:按年兑付,2:从主管赠品扣减)

        /// <summary>
        /// 按月兑付
        /// </summary>
        [Description("按月兑付")]
        ByMonth = 0,

        /// <summary>
        /// 按单位量总计兑付
        /// </summary>
        [Description("按单位量总计兑付")]
        UnitQuantity = 1,

        /// <summary>
        /// 从主管赠品扣减
        /// </summary>
        [Description("从主管赠品扣减")]
        ManageGift = 2,
    }

    /// <summary>
    /// 费用合同项目类型
    /// </summary>
    public enum CostContractCTypeEnum
    {
        //0:商品，1：现金
        /// <summary>
        /// 商品
        /// </summary>
        [Description("商品")]
        Product = 0,

        /// <summary>
        /// 现金
        /// </summary>
        [Description("现金")]
        Cash = 1,

    }


    #endregion

    #region 系统设置

    /// <summary>
    /// 打印模式
    /// </summary>
    public enum PrintMode : int
    {
        /// <summary>
        /// 紧缩模式
        /// </summary>
        [Description("紧缩模式")]
        Crunch = 1,

        /// <summary>
        /// 宽松模式
        /// </summary>
        [Description("宽松模式")]
        Loose = 2
    }

    /// <summary>
    /// 打印方式
    /// </summary>
    public enum PrintMethod : int
    {
        /// <summary>
        /// PDF版
        /// </summary>
        [Description("PDF版")]
        PDF = 1,

        /// <summary>
        /// 自定义版
        /// </summary>
        [Description("自定义版")]
        Custom = 2
    }

    /// <summary>
    /// 打印纸张类型
    /// </summary>
    public enum PaperType : int
    {
        /// <summary>
        /// 标准票据打印纸（21.59*13.97cm）
        /// </summary>
        [Description("标准票据打印纸（21.59*13.97cm）")]
        Standard1 = 1,

        /// <summary>
        /// 标准票据打印纸（21.59*9.31cm）
        /// </summary>
        [Description("标准票据打印纸（21.59*9.31cm）")]
        Standard2 = 2,

        /// <summary>
        /// 标准票据打印纸（21.59*6.99cm）
        /// </summary>
        [Description("标准票据打印纸（21.59*6.99cm）")]
        Standard3 = 3,

        /// <summary>
        /// A4打印纸（21*29.7cm）
        /// </summary>
        [Description("A4打印纸（21*29.7cm）")]
        A4 = 4,

        /// <summary>
        /// 自定义
        /// </summary>
        [Description("自定义")]
        Custom = 5
    }

    /// <summary>
    /// 边距类型
    /// </summary>
    public enum PrintType : int
    {
        /// <summary>
        /// 无边距打印（推荐）
        /// </summary>
        [Description("无边距打印（推荐）")]
        UnBorder = 1,

        /// <summary>
        /// 有边距打印
        /// </summary>
        [Description("有边距打印")]
        Border = 2,

        /// <summary>
        /// 自定义
        /// </summary>
        [Description("自定义")]
        Custom = 3
    }


    //单据生产日期功能
    public enum OpenBillMakeDate : int
    {
        /// <summary>
        /// 关闭单据生产日期功能
        /// </summary>
        [Description("关闭单据生产日期功能")]
        Close = 0,

        /// <summary>
        /// 开启销售单生产日期功能
        /// </summary>
        [Description("开启销售单生产日期功能")]
        Open = 1,

        /// <summary>
        /// 开启所有单据生产日期功能
        /// </summary>
        [Description("开启所有单据生产日期功能")]
        OpenAll = 2
    }

    public enum MuitProductPriceUnit : int
    {
        /// <summary>
        /// 不同单位价格独立
        /// </summary>
        [Description("不同单位价格独立")]
        Independence = 0,

        /// <summary>
        /// 单位换算率保持同比例
        /// </summary>
        [Description("单位换算率保持同比例")]
        ConversionRate = 1
    }


    public enum DefaultPurchasePrice : int
    {
        /// <summary>
        /// 上次进价
        /// </summary>
        [Description("上次进价")]
        NextPriice = 0,

        /// <summary>
        /// 预设进价
        /// </summary>
        [Description("预设进价")]
        PresetPrice = 1
    }


    public enum VariablePriceCommodity : int
    {
        /// <summary>
        /// 价格体系
        /// </summary>
        [Description("价格体系")]
        PriceSystem = 0,

        /// <summary>
        /// 批发价
        /// </summary>
        [Description("批发价")]
        TradePrice = 1
    }


    public enum AccuracyRounding : int
    {
        /// <summary>
        /// 元
        /// </summary>
        [Description("元")]
        Yuan = 0,

        /// <summary>
        /// 角
        /// </summary>
        [Description("角")]
        Jiao = 1,

        /// <summary>
        /// 分
        /// </summary>
        [Description("分")]
        Feng = 2
    }

    public enum MakeBillDisplayBarCode : int
    {
        /// <summary>
        /// 小包条码
        /// </summary>
        [Description("小包条码")]
        ParcelBarCode = 0,

        /// <summary>
        /// 实际单位条码
        /// </summary>
        [Description("实际单位条码")]
        ActualUnitBarcode = 1
    }


    public enum ReferenceCostPrice : int
    {
        /// <summary>
        /// 预设进价
        /// </summary>
        [Description("预设进价")]
        PresetPurchasePrice = 1,

        /// <summary>
        /// 平均进价
        /// </summary>
        [Description("平均进价")]
        AveragePurchasePrice = 2
    }


    public enum TemplateType : int
    {
        /// <summary>
        /// 单据模板
        /// </summary>
        [Description("单据模板")]
        Bill = 0,

        /// <summary>
        /// 报表模板
        /// </summary>
        [Description("报表模板")]
        Report = 1
    }

    public enum OpenChoiceGift : int
    {
        /// <summary>
        /// 只能按系统预设搭赠
        /// </summary>
        [Description("只能按系统预设搭赠")]
        System = 0,

        /// <summary>
        /// 业务员可自由搭赠
        /// </summary>
        [Description("业务员可自由搭赠")]
        Freedom = 1
    }

    public enum AllowViewReport : int
    {
        /// <summary>
        /// 允许查看自己单据报表
        /// </summary>
        [Description("允许查看自己单据报表")]
        Us = 0,

        /// <summary>
        /// 允许查看本部门单据报表
        /// </summary>
        [Description("允许查看本部门单据报表")]
        This = 1,

        /// <summary>
        /// 允许查看所有部门单据报表
        /// </summary>
        [Description("允许查看所有部门单据报表")]
        All = 2
    }


    #endregion

    #region 报表


    #region 客户价值分析
    /// <summary>
    /// 客户价值分析
    /// </summary>
    public enum MarketReportTerminalValueAnalysisTerminalValueEnum
    {
        /// <summary>
        /// 重要价值客户
        /// </summary>
        [Description("重要价值客户")]
        ImportantValue = 1,

        /// <summary>
        /// 重要保持客户
        /// </summary>
        [Description("重要保持客户")]
        ImportantKeeping = 2,

        /// <summary>
        /// 重要发展客户
        /// </summary>
        [Description("重要发展客户")]
        ImportantDevelopment = 3,

        /// <summary>
        /// 重要挽留客户
        /// </summary>
        [Description("重要挽留客户")]
        ImportantRetention = 4,

        /// <summary>
        /// 一般价值客户
        /// </summary>
        [Description("一般价值客户")]
        GeneralValue = 5

    }
    #endregion

    #region 客户流失预警
    /// <summary>
    /// 客户流失预警
    /// </summary>
    public enum MarketReportTerminalLossWarningTerminalValueEnum
    {
        /// <summary>
        /// 重要价值客户
        /// </summary>
        [Description("重要价值客户")]
        ImportantValue = 1,

        /// <summary>
        /// 重要保持客户
        /// </summary>
        [Description("重要保持客户")]
        ImportantKeeping = 2,

        /// <summary>
        /// 重要发展客户
        /// </summary>
        [Description("重要发展客户")]
        ImportantDevelopment = 3,

        /// <summary>
        /// 重要挽留客户
        /// </summary>
        [Description("重要挽留客户")]
        ImportantRetention = 4,

        /// <summary>
        /// 一般价值客户
        /// </summary>
        [Description("一般价值客户")]
        GeneralValue = 5

    }
    #endregion


    #endregion



    #region 拜访

    public enum SignEnum
    {
        [Description("已签到")]
        CheckIn = 1,

        [Description("已签退")]
        Signed = 2,
    }


    public enum VisitTypeEnum
    {
        [Description("全部")]
        All = 1,


        [Description("计划内拜访")]
        PlanIn = 2,


        [Description("计划外拜访")]
        PlantOut = 3,


        [Description("未拜访")]
        None = 4
    }

    #endregion


    #region 会计科目枚举

    public enum AccountingEnum
    {
        /// <summary>
        /// 资产类
        /// </summary>
        [Description("资产类")]
        Assets = 1,
        /// <summary>
        /// 负债类
        /// </summary>
        [Description("负债类")]
        Liability = 2,
        /// <summary>
        /// 权益类
        /// </summary>
        [Description("权益类")]
        Rights = 3,
        /// <summary>
        /// 损益类（收入）
        /// </summary>
        [Description("损益类（收入）")]
        Income = 4,
        /// <summary>
        /// 损益类（支出）
        /// </summary>
        [Description("损益类（支出）")]
        Expense = 5,
        /// <summary>
        /// 其他类
        /// </summary>
        [Description("其他类")]
        Other = 6

    }

    #endregion

    #region 会计科目代码枚举
    public enum AccountingCodeEnum : int
    {
        /// <summary>
        /// 库存现金:defaultCode(1001)
        /// </summary>
        [Description("库存现金")]
        HandCash = 1,
        /// <summary>
        /// 银行存款:defaultCode(xxx)
        /// </summary>
        [Description("银行存款")]
        BankDeposits = 2,
        /// <summary>
        /// 应收账款:defaultCode(xxx)
        /// </summary>
        [Description("应收账款")]
        AccountsReceivable = 3,
        /// <summary>
        /// 预付账款:defaultCode(xxx)
        /// </summary>
        [Description("预付账款")]
        AdvancePayment = 4,
        /// <summary>
        /// 应收利息:defaultCode(xxx)
        /// </summary>
        [Description("应收利息")]
        InterestReceivable = 5,

        /// <summary>
        /// 库存商品:defaultCode(xxx)
        /// </summary>
        [Description("库存商品")]
        InventoryGoods = 6,
        /// <summary>
        /// 固定资产:defaultCode(xxx)
        /// </summary>
        [Description("固定资产")]
        FixedAssets = 7,
        /// <summary>
        /// 累计折旧:defaultCode(xxx)
        /// </summary>
        [Description("累计折旧")]
        AccumulatedDepreciation = 8,
        /// <summary>
        /// 固定资产清理:defaultCode(xxx)
        /// </summary>
        [Description("固定资产清理")]
        LiquidationFixedAssets = 9,
        /// <summary>
        /// 现金:defaultCode(xxx)
        /// </summary>
        [Description("现金")]
        Cash = 10,
        /// <summary>
        /// 银行:defaultCode(xxx)
        /// </summary>
        [Description("银行")]
        Bank = 11,
        /// <summary>
        /// 微信:defaultCode(xxx)
        /// </summary>
        [Description("微信")]
        WChat = 12,
        /// <summary>
        /// 支付宝:defaultCode(xxx)
        /// </summary>
        [Description("支付宝")]
        PayTreasure = 13,
        /// <summary>
        /// 预付款:defaultCode(xxx)
        /// </summary>
        [Description("预付款")]
        Imprest = 14,
        /// <summary>
        /// 短期借款:defaultCode(xxx)
        /// </summary>
        [Description("短期借款")]
        ShortBorrowing = 15,
        /// <summary>
        /// 应付账款
        /// </summary>
        [Description("应付账款")]
        AccountsPayable = 16,
        /// <summary>
        /// 预收账款:defaultCode(xxx)
        /// </summary>
        [Description("预收账款")]
        AdvanceReceipt = 17,
        /// <summary>
        /// 订货款:defaultCode(xxx)
        /// </summary>
        [Description("订货款")]
        Order = 18,
        /// <summary>
        /// 应付职工薪酬:defaultCode(xxx)
        /// </summary>
        [Description("应付职工薪酬")]
        EmployeePayable = 19,
        /// <summary>
        /// 应交税费:defaultCode(xxx)
        /// </summary>
        [Description("应交税费")]
        PayableTaxes = 20,
        /// <summary>
        /// 应付利息:defaultCode(xxx)
        /// </summary>
        [Description("应付利息")]
        InterestPayable = 21,
        /// <summary>
        /// 其他应付款:defaultCode(xxx)
        /// </summary>
        [Description("其他应付款")]
        OtherPayables = 22,
        /// <summary>
        /// 长期借款:defaultCode(xxx)
        /// </summary>
        [Description("长期借款")]
        LongBorrowing = 23,
        /// <summary>
        /// 预收款:defaultCode(xxx)
        /// </summary>
        [Description("预收款")]
        AdvancesReceived = 24,
        /// <summary>
        /// 应交增值税:defaultCode(xxx)
        /// </summary>
        [Description("应交增值税")]
        VATPayable = 25,
        /// <summary>
        /// 进项税额:defaultCode(xxx)
        /// </summary>
        [Description("进项税额")]
        InputTax = 26,
        /// <summary>
        /// 已交税金:defaultCode(xxx)
        /// </summary>
        [Description("已交税金")]
        PayTaxes = 27,
        /// <summary>
        /// 转出未交增值税:defaultCode(xxx)
        /// </summary>
        [Description("转出未交增值税")]
        TransferTaxes = 28,
        /// <summary>
        /// 销项税额:defaultCode(xxx)
        /// </summary>
        [Description("销项税额")]
        OutputTax = 29,
        /// <summary>
        /// 未交增值税:defaultCode(xxx)
        /// </summary>
        [Description("未交增值税")]
        UnpaidVAT = 30,
        /// <summary>
        /// 实收资本:defaultCode(xxx)
        /// </summary>
        [Description("实收资本")]
        PaidCapital = 31,
        /// <summary>
        /// 资本公积:defaultCode(xxx)
        /// </summary>
        [Description("资本公积")]
        CapitalReserves = 32,
        /// <summary>
        /// 盈余公积:defaultCode(xxx)
        /// </summary>
        [Description("盈余公积")]
        SurplusReserve = 33,
        /// <summary>
        /// 本年利润:defaultCode(xxx)
        /// </summary>
        [Description("本年利润")]
        ThisYearProfits = 34,
        /// <summary>
        /// 利润分配:defaultCode(xxx)
        /// </summary>
        [Description("利润分配")]
        ProfitDistribution = 35,
        /// <summary>
        /// 法定盈余公积:defaultCode(xxx)
        /// </summary>
        [Description("法定盈余公积")]
        LegalSurplus = 36,
        /// <summary>
        /// 未分配利润:defaultCode(xxx)
        /// </summary>
        [Description("未分配利润")]
        UndistributedProfit = 37,
        /// <summary>
        /// 任意盈余公积:defaultCode(xxx)
        /// </summary>
        [Description("任意盈余公积")]
        ArbitrarySurplus = 38,
        /// <summary>
        /// 主营业务收入:defaultCode(xxx)
        /// </summary>
        [Description("主营业务收入")]
        MainIncome = 39,
        /// <summary>
        /// 其他业务收入:defaultCode(xxx)
        /// </summary>
        [Description("其他业务收入")]
        OtherIncome = 40,
        /// <summary>
        /// 盘点报溢收入:defaultCode(xxx)
        /// </summary>
        [Description("盘点报溢收入")]
        TakeStockIncome = 41,
        /// <summary>
        /// 成本调价收入:defaultCode(xxx)
        /// </summary>
        [Description("成本调价收入")]
        CostIncome = 42,
        /// <summary>
        /// 厂家返点:defaultCode(xxx)
        /// </summary>
        [Description("厂家返点")]
        ManufacturerRebates = 43,
        /// <summary>
        /// 商品拆装收入:defaultCode(xxx)
        /// </summary>
        [Description("商品拆装收入")]
        GoodsIncome = 44,
        /// <summary>
        /// 采购退货收入:defaultCode(xxx)
        /// </summary>
        [Description("采购退货收入")]
        PurchaseIncome = 45,
        /// <summary>
        /// 主营业务成本:defaultCode(xxx)
        /// </summary>
        [Description("主营业务成本")]
        MainCost = 46,
        /// <summary>
        /// 其他业务成本:defaultCode(xxx)
        /// </summary>
        [Description("其他业务成本")]
        OtherCost = 47,
        /// <summary>
        /// 销售费用:defaultCode(xxx)
        /// </summary>
        [Description("销售费用")]
        SaleFees = 48,
        /// <summary>
        /// 管理费用:defaultCode(xxx)
        /// </summary>
        [Description("管理费用")]
        ManageFees = 49,
        /// <summary>
        /// 财务费用:defaultCode(xxx)
        /// </summary>
        [Description("财务费用")]
        FinanceFees = 50,
        /// <summary>
        /// 盘点亏损:defaultCode(xxx)库存现金
        /// </summary>
        [Description("盘点亏损")]
        InventoryLoss = 51,
        /// <summary>
        /// 成本调价损失:defaultCode(xxx)
        /// </summary>
        [Description("成本调价损失")]
        CostLoss = 52,
        /// <summary>
        /// 采购退货损失:defaultCode(xxx)
        /// </summary>
        [Description("采购退货损失")]
        PurchaseLoss = 53,
        /// <summary>
        /// 优惠:defaultCode(xxx)
        /// </summary>
        [Description("优惠")]
        Preferential = 54,
        /// <summary>
        /// 刷卡手续费:defaultCode(xxx)
        /// </summary>
        [Description("刷卡手续费")]
        CardFees = 55,
        /// <summary>
        /// 陈列费:defaultCode(xxx)
        /// </summary>
        [Description("陈列费")]
        DisplayFees = 56,
        /// <summary>
        /// 油费:defaultCode(xxx)
        /// </summary>
        [Description("油费")]
        OilFees = 57,
        /// <summary>
        /// 车辆费:defaultCode(xxx)
        /// </summary>
        [Description("车辆费")]
        CarFees = 58,
        /// <summary>
        /// 用餐费:defaultCode(xxx)
        /// </summary>
        [Description("用餐费")]
        MealsFees = 59,
        /// <summary>
        /// 运费:defaultCode(xxx)
        /// </summary>
        [Description("运费")]
        TransferFees = 60,
        /// <summary>
        /// 折旧费用:defaultCode(xxx)
        /// </summary>
        [Description("折旧费用")]
        OldFees = 61,
        /// <summary>
        /// 0.5元奖盖:defaultCode(xxx)
        /// </summary>
        [Description("0.5元奖盖")]
        BottleCapsFees = 62,
        /// <summary>
        /// 2元瓶盖:defaultCode(xxx)
        /// </summary>
        [Description("2元瓶盖")]
        TwoCapsFees = 63,
        /// <summary>
        /// 50元瓶盖:defaultCode(xxx)
        /// </summary>
        [Description("50元瓶盖")]
        FiftyCapsFees = 64,
        /// <summary>
        /// 办公费:defaultCode(xxx)
        /// </summary>
        [Description("办公费")]
        OfficeFees = 65,
        /// <summary>
        /// 房租:defaultCode(xxx)
        /// </summary>
        [Description("房租")]
        HouseFees = 66,
        /// <summary>
        /// 物业管理费:defaultCode(xxx)
        /// </summary>
        [Description("物业管理费")]
        ManagementFees = 67,
        /// <summary>
        /// 水电费:defaultCode(xxx)
        /// </summary>
        [Description("水电费")]
        WaterFees = 68,
        /// <summary>
        /// 累计折旧:defaultCode(xxx)
        /// </summary>
        [Description("累计折旧")]
        AccumulatedFees = 69,
        /// <summary>
        /// 汇兑损益:defaultCode(xxx)
        /// </summary>
        [Description("汇兑损益")]
        ExchangeLoss = 70,
        /// <summary>
        /// 利息:defaultCode(xxx)
        /// </summary>
        [Description("利息")]
        Interest = 71,
        /// <summary>
        /// 手续费:defaultCode(xxx)
        /// </summary>
        [Description("手续费")]
        PoundageFees = 72,
        /// <summary>
        /// 其他账户:defaultCode(xxx)
        /// </summary>
        [Description("其他账户")]
        OtherAccount = 73,
        /// <summary>
        /// 银行:defaultCode(xxx)
        /// </summary>
        [Description("其他银行")]
        BankTwo = 74,
        /// <summary>
        /// 赠品:defaultCode(xxx)
        /// </summary>
        [Description("赠品")]
        Gifts = 75,

        /// <summary>
        /// 营业外支出:defaultCode(xxx)
        /// </summary>
        [Description("营业外支出")]
        NonOperatingExpenses = 78,

        /// <summary>
        /// 营业外收入:defaultCode(xxx)
        /// </summary>
        [Description("营业外收入")]
        NonOperatingIncome = 79,
    }
    #endregion



    public enum MTypeEnum : int
    {
        #region 消息 perfix="msg"

        [Description("审批")]
        Message = 0,

        [Description("收款")]
        Receipt = 1,

        [Description("交账")]
        Hold = 2,

        [Description("推送")]
        Push = 3,

        #endregion


        #region 通知  perfix="not"

        [Description("审核完成")]
        Audited = 4,

        [Description("调度完成")]
        Scheduled = 5,

        [Description("盘点完成")]
        InventoryCompleted = 6,

        [Description("转单/签收完成")]
        TransferCompleted = 7,

        [Description("库存预警")]
        InventoryWarning = 8,

        [Description("签到异常")]
        CheckException = 9,

        [Description("客户流失预警")]
        LostWarning = 10,

        [Description("开单异常")]
        LedgerWarning = 11,

        [Description("交账完成/撤销")]
        Paymented = 12

        #endregion
    }

    /// <summary>
    /// 赠品类型
    /// </summary>
    public enum GiveTypeEnum
    {
        /// <summary>
        /// 普通赠品
        /// </summary>
        [Description("普通赠品")]
        General = 1,

        /// <summary>
        /// 订货赠品
        /// </summary>
        [Description("订货赠品")]
        Order = 2,

        /// <summary>
        /// 促销赠品
        /// </summary>
        [Description("促销赠品")]
        Promotional = 3,

        /// <summary>
        /// 费用合同
        /// </summary>
        [Description("费用合同")]
        Contract = 4
    }


    /// <summary>
    /// 快速调拨类型
    /// </summary>
    public enum AllocationTypeEnum
    {
        /// <summary>
        /// 按拒收商品调拨
        /// </summary>
        [Description("按拒收商品调拨")]
        ByRejection = 1,

        /// <summary>
        /// 按销补货
        /// </summary>
        [Description("按销补货")]
        BySaleAdd = 2,

        /// <summary>
        /// 按退调拨
        /// </summary>
        [Description("按退调拨")]
        ByReturn = 3,

        /// <summary>
        /// 按库存调拨
        /// </summary>
        [Description("按库存调拨")]
        ByStock = 4
    }

    /// <summary>
    /// 快速调拨获取方式
    /// </summary>
    public enum QuickAllocationEnum
    {

        /// <summary>
        /// 加载今天拒收的商品
        /// </summary>
        [Description("加载今天拒收的商品")]
        LoadRejectionToday = 1,
        /// <summary>
        /// 加载昨天拒收的商品
        /// </summary>
        [Description("加载昨天拒收的商品")]
        LoadRejectionYestDay = 2,
        /// <summary>
        /// 加载前天拒收的商品
        /// </summary>
        [Description("加载前天拒收的商品")]
        LoadRejectionBeforeYestday = 3,

        /// <summary>
        /// 加载今天销售的商品
        /// </summary>
        [Description("加载今天销售的商品")]
        LoadSaleToday = 4,
        /// <summary>
        /// 加载昨天销售的商品
        /// </summary>
        [Description("加载昨天销售的商品")]
        LoadSaleYestDay = 5,
        /// <summary>
        /// 加载近三天销售的商品
        /// </summary>
        [Description("加载近三天销售的商品")]
        LoadSaleNearlyThreeDays = 6,
        /// <summary>
        /// 加载上次调拨后销售的商品
        /// </summary>
        [Description("加载上次调拨后销售的商品")]
        LoadSaleLast = 7,

        /// <summary>
        /// 加载今天退货的商品
        /// </summary>
        [Description("加载今天退货的商品")]
        LoadReturnToday = 8,
        /// <summary>
        /// 加载昨天退货的商品
        /// </summary>
        [Description("加载昨天退货的商品")]
        LoadReturnYestDay = 9,
        /// <summary>
        /// 加载前天退货的商品
        /// </summary>
        [Description("加载前天退货的商品")]
        LoadReturnBeforeYestday = 10
    }

    /// <summary>
    /// 签收状态枚举
    /// </summary>
    public enum SignStatusEnum
    {
        /// <summary>
        /// 待签收
        /// </summary>
        [Description("待签收")]
        Pending = 0,

        /// <summary>
        /// 已签收
        /// </summary>
        [Description("已签收")]
        Done = 1,

        /// <summary>
        /// 拒收
        /// </summary>
        [Description("拒收")]
        Rejection = 2
    }



    /// <summary>
    /// 权限粒度控制枚举
    /// （xxx命名+后缀）命名规则：
    /// 保存：xxx + Save
    /// 修改：xxx + Update
    /// 删除：xxx + Delete
    /// 查看：xxx + View
    /// 导出：xxx + Export
    /// 导入：xxx + Import
    /// 审核：xxx + Approved
    /// 驳回：xxx + Reject  
    /// 调度：xxx + Scheduling 
    /// 重置账户（特）：xxx + ResetAccount  
    /// 重置密码（特）：xxx + ResetPassWord 
    /// 调拨：xxx + Allotted
    /// 打印：xxx + Print
    /// 结账：xxx + Check
    /// 反结账：xxx + OutCheck
    /// </summary>
    public enum AccessGranularityEnum
    {
        /// <summary>
        ///品牌档案 - 保存
        /// </summary>
        BrandArchivesSave = 509,
        /// <summary>
        ///单位档案 - 保存
        /// </summary>
        UnitArchivesSave = 512,
        /// <summary>
        ///统计类别 - 保存
        /// </summary>
        StatisticalTypeSave = 515,
        /// <summary>
        ///赠品额度 - 保存
        /// </summary>
        GiftAmountUpdateSave = 518,
        /// <summary>
        ///促销活动 - 保存
        /// </summary>
        SalesPromotionSave = 521,
        /// <summary>
        ///销售订单 - 保存保存
        /// </summary>
        SaleReservationBillSave = 125,
        /// <summary>
        ///退货订单 - 保存保存
        /// </summary>
        ReturnOrderSave = 132,
        /// <summary>
        ///销售单 - 保存保存
        /// </summary>
        SaleBillSave = 139,
        /// <summary>
        ///退货单 - 保存保存
        /// </summary>
        ReturnBillsSave = 146,
        /// <summary>
        ///订货单 - 保存保存
        /// </summary>
        //PlaceOrderBillsSave = 153,
        /// <summary>
        ///采购订单 - 保存保存
        /// </summary>
        PurchaseOrderSave = 207,
        /// <summary>
        ///采购单 - 保存保存
        /// </summary>
        PurchaseBillsSave = 214,
        /// <summary>
        ///采购退货单 - 保存保存
        /// </summary>
        PurchaseReturnSave = 221,
        /// <summary>
        ///调拨单 - 保存保存
        /// </summary>
        AllocationFormSave = 234,
        /// <summary>
        ///盘点盈亏单 - 保存保存
        /// </summary>
        InventoryProfitlossSave = 241,
        /// <summary>
        ///成本调价单 - 保存保存
        /// </summary>
        CostAdjustmentSave = 248,
        /// <summary>
        ///报损单 - 保存保存
        /// </summary>
        ReportLossSave = 255,
        /// <summary>
        ///盘点单(整仓) - 保存保存
        /// </summary>
        InventoryAllSave = 262,
        /// <summary>
        ///盘点单(部分) - 保存保存
        /// </summary>
        InventorySingleSave = 269,
        /// <summary>
        ///组合单 - 保存保存
        /// </summary>
        CombinationsSave = 276,
        /// <summary>
        ///拆分单 - 保存保存
        /// </summary>
        SplitOrderSave = 283,
        /// <summary>
        ///库存上报 - 保存保存
        /// </summary>
        StockReportSave = 288,
        /// <summary>
        ///收款单 - 保存保存
        /// </summary>
        ReceiptBillsSave = 314,
        /// <summary>
        ///付款单 - 保存保存
        /// </summary>
        PaymentBillsSave = 321,
        /// <summary>
        ///预收款单 - 保存保存
        /// </summary>
        ReceivablesBillsSave = 328,
        /// <summary>
        ///预付款单 - 保存保存
        /// </summary>
        PrepaidBillsSave = 335,
        /// <summary>
        ///费用支出 - 保存保存
        /// </summary>
        ExpenseExpenditureSave = 342,
        /// <summary>
        ///费用合同 - 保存保存
        /// </summary>
        CostContractSave = 349,
        /// <summary>
        ///财务收入 - 保存保存
        /// </summary>
        OtherIncomeSave = 357,
        /// <summary>
        ///录入凭证 - 保存保存
        /// </summary>
        EntryVoucherSave = 367,
        /// <summary>
        ///商品档案 - 保存保存
        /// </summary>
        ProductArchivesSave = 385,
        /// <summary>
        ///价格方案 - 保存保存
        /// </summary>
        PricesPlanSave = 395,
        /// <summary>
        ///上次售价 - 保存保存
        /// </summary>
        LastSalePriceSave = 403,
        /// <summary>
        ///终端档案 - 保存保存
        /// </summary>
        EndPointListSave = 407,
        /// <summary>
        ///供应商档案 - 保存保存
        /// </summary>
        SupplierSave = 417,
        /// <summary>
        ///应收款期初 - 保存保存
        /// </summary>
        EarlyReceivablesSave = 421,
        /// <summary>
        ///品牌档案 - 删除
        /// </summary>
        BrandArchivesDelete = 511,
        /// <summary>
        ///单位档案 - 删除
        /// </summary>
        UnitArchivesDelete = 514,
        /// <summary>
        ///统计类别 - 删除
        /// </summary>
        StatisticalTypeDelete = 517,
        /// <summary>
        ///赠品额度 - 删除
        /// </summary>
        GiftAmountDelete = 520,
        /// <summary>
        ///促销活动 - 删除
        /// </summary>
        SalesPromotionDelete = 523,
        /// <summary>
        ///用户列表 - 删除
        /// </summary>
        UserDelete = 108,
        /// <summary>
        ///角色管理 - 删除
        /// </summary>
        UserRoleDelete = 115,
        /// <summary>
        ///模块管理 - 删除
        /// </summary>
        ModuleDelete = 120,
        /// <summary>
        ///退货订单 - 删除
        /// </summary>
        ReturnOrderDelete = 133,
        /// <summary>
        ///退货单 - 删除
        /// </summary>
        ReturnBillsDelete = 147,
        /// <summary>
        ///订货单 - 删除
        /// </summary>
        //PlaceOrderBillsDelete = 154,
        /// <summary>
        ///采购订单 - 删除
        /// </summary>
        PurchaseOrderDelete = 208,
        /// <summary>
        ///采购单 - 删除
        /// </summary>
        PurchaseBillsDelete = 215,
        /// <summary>
        ///采购退货单 - 删除
        /// </summary>
        PurchaseReturnDelete = 222,
        /// <summary>
        ///调拨单 - 删除
        /// </summary>
        AllocationFormDelete = 235,
        /// <summary>
        ///盘点盈亏单 - 删除
        /// </summary>
        InventoryProfitlossDelete = 242,
        /// <summary>
        ///成本调价单 - 删除
        /// </summary>
        CostAdjustmentDelete = 249,
        /// <summary>
        ///报损单 - 删除
        /// </summary>
        ReportLossDelete = 256,
        /// <summary>
        ///盘点单(整仓) - 删除
        /// </summary>
        InventoryAllDelete = 263,
        /// <summary>
        ///盘点单(部分) - 删除
        /// </summary>
        InventorySingleDelete = 270,
        /// <summary>
        ///组合单 - 删除
        /// </summary>
        CombinationsDelete = 277,
        /// <summary>
        ///拆分单 - 删除
        /// </summary>
        SplitOrderDelete = 284,
        /// <summary>
        ///收款单 - 删除
        /// </summary>
        ReceiptBillsDelete = 315,
        /// <summary>
        ///付款单 - 删除
        /// </summary>
        PaymentBillsDelete = 322,
        /// <summary>
        ///预收款单 - 删除
        /// </summary>
        ReceivablesBillsDelete = 329,
        /// <summary>
        ///预付款单 - 删除
        /// </summary>
        PrepaidBillsDelete = 336,
        /// <summary>
        ///费用支出 - 删除
        /// </summary>
        ExpenseExpenditureDelete = 343,
        /// <summary>
        ///费用合同 - 删除
        /// </summary>
        CostContractDelete = 350,
        /// <summary>
        ///财务收入 - 删除
        /// </summary>
        OtherIncomeDelete = 358,
        /// <summary>
        ///录入凭证 - 删除
        /// </summary>
        EntryVoucherDelete = 368,
        /// <summary>
        ///销售单 - 删除
        /// </summary>
        SaleBillsDelete = 504,
        /// <summary>
        ///销售订单 - 删除
        /// </summary>
        SaleOrderDelete = 507,
        /// <summary>
        ///期末结转 - 反结账
        /// </summary>
        FinancialOutCheck = 363,
        /// <summary>
        ///销售订单 - 审核
        /// </summary>
        SaleReservationBillApproved = 127,
        /// <summary>
        ///退货订单 - 审核
        /// </summary>
        ReturnOrderApproved = 134,
        /// <summary>
        ///销售单 - 审核
        /// </summary>
        SaleBillApproved = 141,
        /// <summary>
        ///退货单 - 审核
        /// </summary>
        ReturnBillsApproved = 148,
        /// <summary>
        ///订货单 - 审核
        /// </summary>
        //PlaceOrderBillsApproved = 155,
        /// <summary>
        ///采购订单 - 审核
        /// </summary>
        PurchaseOrderApproved = 209,
        /// <summary>
        ///采购单 - 审核
        /// </summary>
        PurchaseBillsApproved = 216,
        /// <summary>
        ///采购退货单 - 审核
        /// </summary>
        PurchaseReturnApproved = 223,
        /// <summary>
        ///调拨单 - 审核
        /// </summary>
        AllocationFormApproved = 236,
        /// <summary>
        ///盘点盈亏单 - 审核
        /// </summary>
        InventoryProfitlossApproved = 243,
        /// <summary>
        ///成本调价单 - 审核
        /// </summary>
        CostAdjustmentApproved = 250,
        /// <summary>
        ///报损单 - 审核
        /// </summary>
        ReportLossApproved = 257,
        /// <summary>
        ///盘点单(整仓) - 审核
        /// </summary>
        InventoryAllApproved = 264,
        /// <summary>
        ///盘点单(部分) - 审核
        /// </summary>
        InventorySingleApproved = 271,
        /// <summary>
        ///组合单 - 审核
        /// </summary>
        CombinationsApproved = 278,
        /// <summary>
        ///拆分单 - 审核
        /// </summary>
        SplitOrderApproved = 285,
        /// <summary>
        ///收款单 - 审核
        /// </summary>
        ReceiptBillsApproved = 316,
        /// <summary>
        ///付款单 - 审核
        /// </summary>
        PaymentBillsApproved = 323,
        /// <summary>
        ///预收款单 - 审核
        /// </summary>
        ReceivablesBillsApproved = 330,
        /// <summary>
        ///预付款单 - 审核
        /// </summary>
        PrepaidBillsApproved = 337,
        /// <summary>
        ///费用支出 - 审核
        /// </summary>
        ExpenseExpenditureApproved = 344,
        /// <summary>
        ///费用合同 - 审核
        /// </summary>
        CostContractApproved = 351,
        /// <summary>
        ///财务收入 - 审核
        /// </summary>
        OtherIncomeApproved = 359,
        /// <summary>
        ///录入凭证 - 审核
        /// </summary>
        EntryVoucherApproved = 369,
        /// <summary>
        ///销售订单 - 导出
        /// </summary>
        SaleReservationBillExport = 124,
        /// <summary>
        ///退货订单 - 导出
        /// </summary>
        ReturnOrderExport = 131,
        /// <summary>
        ///销售单 - 导出
        /// </summary>
        SaleBillExport = 138,
        /// <summary>
        ///退货单 - 导出
        /// </summary>
        ReturnBillsExport = 145,
        /// <summary>
        ///订货单 - 导出
        /// </summary>
        //PlaceOrderBillsExport = 152,
        /// <summary>
        ///收款对账单 - 导出
        /// </summary>
        AccountReceivableExport = 161,
        /// <summary>
        ///销售明细表 - 导出
        /// </summary>
        SaleDetailsExport = 175,
        /// <summary>
        ///销售汇总(按商品) - 导出
        /// </summary>
        SaleSummaryByProductExport = 177,
        /// <summary>
        ///销售汇总(按客户) - 导出
        /// </summary>
        SaleSummaryByCustomerExport = 179,
        /// <summary>
        ///销售汇总(按业务员) - 导出
        /// </summary>
        SaleSummaryByBUserExport = 181,
        /// <summary>
        ///销售汇总(客户/商品) - 导出
        /// </summary>
        SaleSummaryByCPExport = 183,
        /// <summary>
        ///销售汇总(按仓库) - 导出
        /// </summary>
        SaleSummaryByStockExport = 185,
        /// <summary>
        ///销售汇总(按品牌) - 导出
        /// </summary>
        SaleSummaryByBrandExport = 187,
        /// <summary>
        ///订单明细 - 导出
        /// </summary>
        OrderDetailsExport = 189,
        /// <summary>
        ///订单汇总(按商品) - 导出
        /// </summary>
        OrderSummaryByProductExport = 191,
        /// <summary>
        ///费用合同明细表 - 导出
        /// </summary>
        ExpenseDetailsExport = 193,
        /// <summary>
        ///订货汇总 - 导出
        /// </summary>
        OrderSummaryExport = 195,
        /// <summary>
        ///赠品汇总 - 导出
        /// </summary>
        GiftSummaryExport = 197,
        /// <summary>
        ///采购订单 - 导出
        /// </summary>
        PurchaseOrderExport = 206,
        /// <summary>
        ///采购单 - 导出
        /// </summary>
        PurchaseBillsExport = 213,
        /// <summary>
        ///采购退货单 - 导出
        /// </summary>
        PurchaseReturnExport = 220,
        /// <summary>
        ///采购明细表 - 导出
        /// </summary>
        PurchaseReportExport = 226,
        /// <summary>
        ///采购汇总（按商品） - 导出
        /// </summary>
        PurchaseSummaryByProductExport = 228,
        /// <summary>
        ///采购汇总（按供应商） - 导出
        /// </summary>
        PurchaseSummaryBySupplierExport = 230,
        /// <summary>
        ///调拨单 - 导出
        /// </summary>
        AllocationFormExport = 233,
        /// <summary>
        ///盘点盈亏单 - 导出
        /// </summary>
        InventoryProfitlossExport = 240,
        /// <summary>
        ///成本调价单 - 导出
        /// </summary>
        CostAdjustmentExport = 247,
        /// <summary>
        ///报损单 - 导出
        /// </summary>
        ReportLossExport = 254,
        /// <summary>
        ///盘点单(整仓) - 导出
        /// </summary>
        InventoryAllExport = 261,
        /// <summary>
        ///盘点单(部分) - 导出
        /// </summary>
        InventorySingleExport = 268,
        /// <summary>
        ///组合单 - 导出
        /// </summary>
        CombinationsExport = 275,
        /// <summary>
        ///拆分单 - 导出
        /// </summary>
        SplitOrderExport = 282,
        /// <summary>
        ///库存表 - 导出
        /// </summary>
        StockReportingExport = 290,
        /// <summary>
        ///库存变化表(汇总) - 导出
        /// </summary>
        StockChangeExport = 292,
        /// <summary>
        ///库存变化表(按单据) - 导出
        /// </summary>
        StockChangeByBillsExport = 294,
        /// <summary>
        ///门店库存上报表 - 导出
        /// </summary>
        StoreStockReportingExport = 296,
        /// <summary>
        ///门店库存上报汇总表 - 导出
        /// </summary>
        StoreStockSummeryReportingExport = 298,
        /// <summary>
        ///调拨明细表 - 导出
        /// </summary>
        AllocatingDetailsExport = 300,
        /// <summary>
        ///调拨汇总表（按商品） - 导出
        /// </summary>
        AllocatingSummeryByProductExport = 302,
        /// <summary>
        ///成本汇总表 - 导出
        /// </summary>
        CostSummaryReportingExport = 304,
        /// <summary>
        ///库存滞销报表 - 导出
        /// </summary>
        InventoryUnsalableReportExport = 306,
        /// <summary>
        ///库存预警表 - 导出
        /// </summary>
        StockWaringExport = 308,
        /// <summary>
        ///临期预警表 - 导出
        /// </summary>
        EarlyWarningExport = 310,
        /// <summary>
        ///收款单 - 导出
        /// </summary>
        ReceiptBillsExport = 313,
        /// <summary>
        ///付款单 - 导出
        /// </summary>
        PaymentBillsExport = 320,
        /// <summary>
        ///预收款单 - 导出
        /// </summary>
        ReceivablesBillsExport = 327,
        /// <summary>
        ///预付款单 - 导出
        /// </summary>
        PrepaidBillsExport = 334,
        /// <summary>
        ///费用支出 - 导出
        /// </summary>
        ExpenseExpenditureExport = 341,
        /// <summary>
        ///费用合同 - 导出
        /// </summary>
        CostContractExport = 348,
        /// <summary>
        ///财务收入 - 导出
        /// </summary>
        OtherIncomeExport = 356,
        /// <summary>
        ///录入凭证 - 导出
        /// </summary>
        EntryVoucherExport = 366,
        /// <summary>
        ///科目余额表 - 导出
        /// </summary>
        AccountBalanceExport = 372,
        /// <summary>
        ///资产负债表 - 导出
        /// </summary>
        BalanceSheetExport = 375,
        /// <summary>
        ///利润表 - 导出
        /// </summary>
        ProfitStatementExport = 378,
        /// <summary>
        ///明细分类账 - 导出
        /// </summary>
        SubsidiaryLedgerExport = 381,
        /// <summary>
        ///商品档案 - 导出
        /// </summary>
        ProductArchivesExport = 384,
        /// <summary>
        ///价格方案 - 导出
        /// </summary>
        PricesPlanExport = 394,
        /// <summary>
        ///上次售价 - 导出
        /// </summary>
        LastSalePriceExport = 402,
        /// <summary>
        ///终端档案 - 导出
        /// </summary>
        EndPointListExport = 406,
        /// <summary>
        ///供应商档案 - 导出
        /// </summary>
        SupplierExport = 416,
        /// <summary>
        ///应收款期初 - 导出
        /// </summary>
        EarlyReceivablesExport = 420,
        /// <summary>
        ///客户往来账 - 导出
        /// </summary>
        CustomerCurrentAccountExport = 441,
        /// <summary>
        ///客户应收款 - 导出
        /// </summary>
        CustomerReceivableExport = 443,
        /// <summary>
        ///供应商往来账 - 导出
        /// </summary>
        SupplierCurrentAccountExport = 445,
        /// <summary>
        ///供应商应付款 - 导出
        /// </summary>
        SupplierShouldPayExport = 447,
        /// <summary>
        ///预收款余额 - 导出
        /// </summary>
        PrepaidBalanceExport = 449,
        /// <summary>
        ///预付款余额 - 导出
        /// </summary>
        PrepaymentBalanceExport = 451,
        /// <summary>
        ///业务员业绩 - 导出
        /// </summary>
        SalespersonPerformanceExport = 453,
        /// <summary>
        ///员工提成汇总表 - 导出
        /// </summary>
        RoyaltySummaryExport = 455,
        /// <summary>
        ///业务员拜访记录 - 导出
        /// </summary>
        VisitingRecordsExport = 457,
        /// <summary>
        ///拜访达成表 - 导出
        /// </summary>
        VisitingScheduleExport = 459,
        /// <summary>
        ///业务员外勤轨迹 - 导出
        /// </summary>
        SalesmanTrackExport = 461,
        /// <summary>
        ///客户活跃度 - 导出
        /// </summary>
        CustomerActivityExport = 463,
        /// <summary>
        ///客户价值分析 - 导出
        /// </summary>
        CustomerAnalysisExport = 465,
        /// <summary>
        ///客户流失预警 - 导出
        /// </summary>
        LossWarningExport = 467,
        /// <summary>
        ///铺市率报表 - 导出
        /// </summary>
        MarketRateReportExport = 469,
        /// <summary>
        ///客户拜访排行 - 导出
        /// </summary>
        VisitsRankingExport = 471,
        /// <summary>
        ///新增客户分析 - 导出
        /// </summary>
        NewCustomerAnalysisExport = 473,
        /// <summary>
        ///客户拜访分析 - 导出
        /// </summary>
        CustomerVisitAnalysisExport = 475,
        /// <summary>
        ///用户列表 - 导出
        /// </summary>
        UserListExport = 492,
        /// <summary>
        ///订货单 - 恢复
        /// </summary>
        //PlaceOrderBillsReset = 173,
        /// <summary>
        ///销售订单 - 打印
        /// </summary>
        SaleReservationPrint = 123,
        /// <summary>
        ///退货订单 - 打印
        /// </summary>
        ReturnOrderPrint = 130,
        /// <summary>
        ///销售单 - 打印
        /// </summary>
        SaleBillPrint = 137,
        /// <summary>
        ///退货单 - 打印
        /// </summary>
        ReturnBillsPrint = 144,
        /// <summary>
        ///订货单 - 打印
        /// </summary>
        //PlaceOrderBillsPrint = 151,
        /// <summary>
        ///车辆对货单 - 打印
        /// </summary>
        VehicleToGoodsPrint = 158,
        /// <summary>
        ///仓库分拣 - 打印
        /// </summary>
        StockSortingPrint = 165,
        /// <summary>
        ///装车调度 - 打印
        /// </summary>
        TrackSchedulingPrint = 167,
        /// <summary>
        ///采购订单 - 打印
        /// </summary>
        PurchaseOrderPrint = 205,
        /// <summary>
        ///采购单 - 打印
        /// </summary>
        PurchaseBillsPrint = 212,
        /// <summary>
        ///采购退货单 - 打印
        /// </summary>
        PurchaseReturnPrint = 219,
        /// <summary>
        ///调拨单 - 打印
        /// </summary>
        AllocationFormPrint = 232,
        /// <summary>
        ///盘点盈亏单 - 打印
        /// </summary>
        InventoryProfitlossPrint = 239,
        /// <summary>
        ///成本调价单 - 打印
        /// </summary>
        CostAdjustmentPrint = 246,
        /// <summary>
        ///报损单 - 打印
        /// </summary>
        ReportLossPrint = 253,
        /// <summary>
        ///盘点单(整仓) - 打印
        /// </summary>
        InventoryAllPrint = 260,
        /// <summary>
        ///盘点单(部分) - 打印
        /// </summary>
        InventorySinglePrint = 267,
        /// <summary>
        ///组合单 - 打印
        /// </summary>
        CombinationsPrint = 274,
        /// <summary>
        ///拆分单 - 打印
        /// </summary>
        SplitOrderPrint = 281,
        /// <summary>
        ///收款单 - 打印
        /// </summary>
        ReceiptBillsPrint = 312,
        /// <summary>
        ///付款单 - 打印
        /// </summary>
        PaymentBillsPrint = 319,
        /// <summary>
        ///预收款单 - 打印
        /// </summary>
        ReceivablesBillsPrint = 326,
        /// <summary>
        ///预付款单 - 打印
        /// </summary>
        PrepaidBillsPrint = 333,
        /// <summary>
        ///费用支出 - 打印
        /// </summary>
        ExpenseExpenditurePrint = 340,
        /// <summary>
        ///费用合同 - 打印
        /// </summary>
        CostContractPrint = 347,
        /// <summary>
        ///财务收入 - 打印
        /// </summary>
        OtherIncomePrint = 355,
        /// <summary>
        ///录入凭证 - 打印
        /// </summary>
        EntryVoucherPrint = 365,
        /// <summary>
        ///科目余额表 - 打印
        /// </summary>
        AccountBalancePrint = 371,
        /// <summary>
        ///资产负债表 - 打印
        /// </summary>
        BalanceSheetPrint = 374,
        /// <summary>
        ///利润表 - 打印
        /// </summary>
        ProfitStatementPrint = 377,
        /// <summary>
        ///明细分类账 - 打印
        /// </summary>
        SubsidiaryLedgerPrint = 380,
        /// <summary>
        ///商品档案 - 打印
        /// </summary>
        ProductArchivesPrint = 383,
        /// <summary>
        ///品牌档案 - 打印
        /// </summary>
        BrandArchivesPrint = 387,
        /// <summary>
        ///单位档案 - 打印
        /// </summary>
        UnitArchivesPrint = 389,
        /// <summary>
        ///统计类别 - 打印
        /// </summary>
        StatisticalTypePrint = 391,
        /// <summary>
        ///价格方案 - 打印
        /// </summary>
        PricesPlanPrint = 393,
        /// <summary>
        ///赠品额度 - 打印
        /// </summary>
        GiftAmountPrint = 397,
        /// <summary>
        ///促销活动 - 打印
        /// </summary>
        SalesPromotionPrint = 399,
        /// <summary>
        ///上次售价 - 打印
        /// </summary>
        LastSalePricePrint = 401,
        /// <summary>
        ///终端档案 - 打印
        /// </summary>
        EndPointListPrint = 405,
        /// <summary>
        ///仓库档案 - 打印
        /// </summary>
        StockListPrint = 409,
        /// <summary>
        ///渠道档案 - 打印
        /// </summary>
        ChannelListPrint = 411,
        /// <summary>
        ///终端等级 - 打印
        /// </summary>
        CustomerLelelPrint = 413,
        /// <summary>
        ///供应商档案 - 打印
        /// </summary>
        SupplierPrint = 415,
        /// <summary>
        ///应收款期初 - 打印
        /// </summary>
        EarlyReceivablesPrint = 419,
        /// <summary>
        ///提成方案	 - 打印
        /// </summary>
        PercentagePlanPrint = 423,
        /// <summary>
        ///员工提成 - 打印
        /// </summary>
        PercentageListPrint = 426,
        /// <summary>
        ///员工档案 - 打印
        /// </summary>
        UserListPrint = 428,
        /// <summary>
        ///操作员角色 - 打印
        /// </summary>
        UserRolePrint = 434,
        /// <summary>
        ///制定线路 - 打印
        /// </summary>
        DrawLinePrint = 437,
        /// <summary>
        ///分配线路 - 打印
        /// </summary>
        DistributionLinePrint = 439,
        /// <summary>
        ///收款对账单 - 撤销上交
        /// </summary>
        AccountReceivableRevocation = 163,
        /// <summary>
        ///提成方案	 - 方案设置
        /// </summary>
        PercentagePlan = 424,
        /// <summary>
        ///用户列表 - 更新
        /// </summary>
        UserUpdate = 107,
        /// <summary>
        ///角色管理 - 更新
        /// </summary>
        UserRoleUpdate = 114,
        /// <summary>
        ///模块管理 - 更新
        /// </summary>
        ModuleUpdate = 119,
        /// <summary>
        ///期末结转 - 期末结账
        /// </summary>
        FinancialCheck = 362,
        /// <summary>
        ///操作员角色 - 权限设置
        /// </summary>
        UserRole = 435,
        /// <summary>
        ///订单转销售单 - 查看	
        /// </summary>
        OrderToSaleBillsView = 170,
        /// <summary>
        ///用户列表 - 查看
        /// </summary>
        UserView = 106,
        /// <summary>
        ///系统角色管理 - 查看
        /// </summary>
        ManageUserRoleView = 109,
        /// <summary>
        ///模块管理 - 查看
        /// </summary>
        ModuleView = 118,
        /// <summary>
        ///经销商管理 - 查看
        /// </summary>
        StoreView = 121,
        /// <summary>
        ///销售订单 - 查看
        /// </summary>
        SaleReservationBillListView = 122,
        /// <summary>
        ///退货订单 - 查看
        /// </summary>
        ReturnOrderView = 129,
        /// <summary>
        ///销售单 - 查看
        /// </summary>
        SaleBillListView = 136,
        /// <summary>
        ///退货单 - 查看
        /// </summary>
        ReturnBillsView = 143,
        /// <summary>
        ///订货单 - 查看
        /// </summary>
        //PlaceOrderBillsView = 150,
        /// <summary>
        ///收款对账单 - 查看
        /// </summary>
        AccountReceivableView = 160,
        /// <summary>
        ///仓库分拣 - 查看
        /// </summary>
        StockSortingView = 164,
        /// <summary>
        ///装车调度 - 查看
        /// </summary>
        TrackSchedulingView = 166,
        /// <summary>
        ///拜访门店 - 查看
        /// </summary>
        VisitStoreView = 172,
        /// <summary>
        ///销售明细表 - 查看
        /// </summary>
        SaleDetailsView = 174,
        /// <summary>
        ///销售汇总(按商品) - 查看
        /// </summary>
        SaleSummaryByProductView = 176,
        /// <summary>
        ///销售汇总(按客户) - 查看
        /// </summary>
        SaleSummaryByCustomerView = 178,
        /// <summary>
        ///销售汇总(按业务员) - 查看
        /// </summary>
        SaleSummaryByBUserView = 180,
        /// <summary>
        ///销售汇总(客户/商品) - 查看
        /// </summary>
        SaleSummaryByCPView = 182,
        /// <summary>
        ///销售汇总(按仓库) - 查看
        /// </summary>
        SaleSummaryByStockView = 184,
        /// <summary>
        ///销售汇总(按品牌) - 查看
        /// </summary>
        SaleSummaryByBrandView = 186,
        /// <summary>
        ///订单明细 - 查看
        /// </summary>
        OrderDetailsView = 188,
        /// <summary>
        ///订单汇总(按商品) - 查看
        /// </summary>
        OrderSummaryByProductView = 190,
        /// <summary>
        ///费用合同明细表 - 查看
        /// </summary>
        ExpenseDetailsView = 192,
        /// <summary>
        ///订货汇总 - 查看
        /// </summary>
        OrderSummaryView = 194,
        /// <summary>
        ///赠品汇总 - 查看
        /// </summary>
        GiftSummaryView = 196,
        /// <summary>
        ///热销排行榜 - 查看
        /// </summary>
        HotRatingView = 198,
        /// <summary>
        ///销量走势图 - 查看
        /// </summary>
        SaleChartView = 199,
        /// <summary>
        ///销售商品成本利润 - 查看
        /// </summary>
        CostProfitView = 200,
        /// <summary>
        ///销售额分析 - 查看
        /// </summary>
        SalesAnalysisView = 201,
        /// <summary>
        ///订单额分析 - 查看
        /// </summary>
        OrderAnalysisView = 202,
        /// <summary>
        ///热订排行榜 - 查看
        /// </summary>
        HotSalesAnalysisView = 203,
        /// <summary>
        ///采购订单 - 查看
        /// </summary>
        PurchaseOrderView = 204,
        /// <summary>
        ///采购单 - 查看
        /// </summary>
        PurchaseBillsView = 211,
        /// <summary>
        ///采购退货单 - 查看
        /// </summary>
        PurchaseReturnView = 218,
        /// <summary>
        ///采购明细表 - 查看
        /// </summary>
        PurchaseReportView = 225,
        /// <summary>
        ///采购汇总（按商品） - 查看
        /// </summary>
        PurchaseSummaryByProductView = 227,
        /// <summary>
        ///采购汇总（按供应商） - 查看
        /// </summary>
        PurchaseSummaryBySupplierView = 229,
        /// <summary>
        ///调拨单 - 查看
        /// </summary>
        AllocationFormView = 231,
        /// <summary>
        ///盘点盈亏单 - 查看
        /// </summary>
        InventoryProfitlossView = 238,
        /// <summary>
        ///成本调价单 - 查看
        /// </summary>
        CostAdjustmentView = 245,
        /// <summary>
        ///报损单 - 查看
        /// </summary>
        ReportLossView = 252,
        /// <summary>
        ///盘点单(整仓) - 查看
        /// </summary>
        InventoryAllView = 259,
        /// <summary>
        ///盘点单(部分) - 查看
        /// </summary>
        InventorySingleView = 266,
        /// <summary>
        ///组合单 - 查看
        /// </summary>
        CombinationsView = 273,
        /// <summary>
        ///拆分单 - 查看
        /// </summary>
        SplitOrderView = 280,
        /// <summary>
        ///库存上报 - 查看
        /// </summary>
        StockReportView = 287,
        /// <summary>
        ///库存表 - 查看
        /// </summary>
        StockReportingView = 289,
        /// <summary>
        ///库存变化表(汇总) - 查看
        /// </summary>
        StockChangeView = 291,
        /// <summary>
        ///库存变化表(按单据) - 查看
        /// </summary>
        StockChangeByBillsView = 293,
        /// <summary>
        ///门店库存上报表 - 查看
        /// </summary>
        StoreStockReportingView = 295,
        /// <summary>
        ///门店库存上报汇总表 - 查看
        /// </summary>
        StoreStockSummeryReportingView = 297,
        /// <summary>
        ///调拨明细表 - 查看
        /// </summary>
        AllocatingDetailsView = 299,
        /// <summary>
        ///调拨汇总表（按商品） - 查看
        /// </summary>
        AllocatingSummeryByProductView = 301,
        /// <summary>
        ///成本汇总表 - 查看
        /// </summary>
        CostSummaryReportingView = 303,
        /// <summary>
        ///库存滞销报表 - 查看
        /// </summary>
        InventoryUnsalableReportView = 305,
        /// <summary>
        ///库存预警表 - 查看
        /// </summary>
        StockWaringReportView = 307,
        /// <summary>
        ///临期预警表 - 查看
        /// </summary>
        EarlyWarningView = 309,
        /// <summary>
        ///收款单 - 查看
        /// </summary>
        ReceiptBillsView = 311,
        /// <summary>
        ///付款单 - 查看
        /// </summary>
        PaymentBillsView = 318,
        /// <summary>
        ///预收款单 - 查看
        /// </summary>
        ReceivablesBillsView = 325,
        /// <summary>
        ///预付款单 - 查看
        /// </summary>
        PrepaidBillsView = 332,
        /// <summary>
        ///费用支出 - 查看
        /// </summary>
        ExpenseExpenditureView = 339,
        /// <summary>
        ///费用合同 - 查看
        /// </summary>
        CostContractView = 346,
        /// <summary>
        ///财务收入 - 查看
        /// </summary>
        OtherIncomeView = 354,
        /// <summary>
        ///期末结转 - 查看
        /// </summary>
        FinancialView = 361,
        /// <summary>
        ///录入凭证 - 查看
        /// </summary>
        EntryVoucherView = 364,
        /// <summary>
        ///科目余额表 - 查看
        /// </summary>
        AccountBalanceView = 370,
        /// <summary>
        ///资产负债表 - 查看
        /// </summary>
        BalanceSheetView = 373,
        /// <summary>
        ///利润表 - 查看
        /// </summary>
        ProfitStatementView = 376,
        /// <summary>
        ///明细分类账 - 查看
        /// </summary>
        SubsidiaryLedgerView = 379,
        /// <summary>
        ///商品档案 - 查看
        /// </summary>
        ProductArchivesView = 382,
        /// <summary>
        ///品牌档案 - 查看
        /// </summary>
        BrandArchivesView = 386,
        /// <summary>
        ///单位档案 - 查看
        /// </summary>
        UnitArchivesView = 388,
        /// <summary>
        ///统计类别 - 查看
        /// </summary>
        StatisticalTypeView = 390,
        /// <summary>
        ///价格方案 - 查看
        /// </summary>
        PricesPlanView = 392,
        /// <summary>
        ///赠品额度 - 查看
        /// </summary>
        GiftAmountView = 396,
        /// <summary>
        ///促销活动 - 查看
        /// </summary>
        SalesPromotionView = 398,
        /// <summary>
        ///上次售价 - 查看
        /// </summary>
        LastSalePriceView = 400,
        /// <summary>
        ///终端档案 - 查看
        /// </summary>
        EndPointListView = 404,
        /// <summary>
        ///仓库档案 - 查看
        /// </summary>
        StockListView = 408,
        /// <summary>
        ///渠道档案 - 查看
        /// </summary>
        ChannelListView = 410,
        /// <summary>
        ///终端等级 - 查看
        /// </summary>
        CustomerLelelView = 412,
        /// <summary>
        ///供应商档案 - 查看
        /// </summary>
        SupplierView = 414,
        /// <summary>
        ///应收款期初 - 查看
        /// </summary>
        EarlyReceivablesView = 418,
        /// <summary>
        ///提成方案	 - 查看
        /// </summary>
        PercentagePlanView = 422,
        /// <summary>
        ///员工提成 - 查看
        /// </summary>
        PercentageListView = 425,
        /// <summary>
        ///员工档案 - 查看
        /// </summary>
        UserListView = 427,
        /// <summary>
        /// 操作员角色 - 查看
        /// </summary>
        UserRoleView = 433,
        /// <summary>
        ///制定线路 - 查看
        /// </summary>
        DrawLineView = 436,
        /// <summary>
        ///分配线路 - 查看
        /// </summary>
        DistributionLineView = 438,
        /// <summary>
        ///客户往来账 - 查看
        /// </summary>
        CustomerCurrentAccountView = 440,
        /// <summary>
        ///客户应收款 - 查看
        /// </summary>
        CustomerReceivableView = 442,
        /// <summary>
        ///供应商往来账 - 查看
        /// </summary>
        SupplierCurrentAccountView = 444,
        /// <summary>
        ///供应商应付款 - 查看
        /// </summary>
        SupplierShouldPayView = 446,
        /// <summary>
        ///预收款余额 - 查看
        /// </summary>
        PrepaidBalanceView = 448,
        /// <summary>
        ///预付款余额 - 查看
        /// </summary>
        PrepaymentBalanceView = 450,
        /// <summary>
        ///业务员业绩 - 查看
        /// </summary>
        SalespersonPerformanceView = 452,
        /// <summary>
        ///员工提成汇总表 - 查看
        /// </summary>
        RoyaltySummaryView = 454,
        /// <summary>
        ///业务员拜访记录 - 查看
        /// </summary>
        VisitingRecordsView = 456,
        /// <summary>
        ///拜访达成表 - 查看
        /// </summary>
        VisitingScheduleView = 458,
        /// <summary>
        ///业务员外勤轨迹 - 查看
        /// </summary>
        SalesmanTrackView = 460,
        /// <summary>
        ///客户活跃度 - 查看
        /// </summary>
        CustomerActivityView = 462,
        /// <summary>
        ///客户价值分析 - 查看
        /// </summary>
        CustomerAnalysisView = 464,
        /// <summary>
        ///客户流失预警 - 查看
        /// </summary>
        LossWarningView = 466,
        /// <summary>
        ///铺市率报表 - 查看
        /// </summary>
        MarketRateReportView = 468,
        /// <summary>
        ///客户拜访排行 - 查看
        /// </summary>
        VisitsRankingView = 470,
        /// <summary>
        ///新增客户分析 - 查看
        /// </summary>
        NewCustomerAnalysisView = 472,
        /// <summary>
        ///客户拜访分析 - 查看
        /// </summary>
        CustomerVisitAnalysisView = 474,
        /// <summary>
        ///库存预警设置 - 查看
        /// </summary>
        StockWarningSettingView = 476,
        /// <summary>
        ///APP打印设置 - 查看
        /// </summary>
        AppPrintSettingView = 478,
        /// <summary>
        ///电脑打印设置 - 查看
        /// </summary>
        PcPrintSettingView = 480,
        /// <summary>
        ///会计科目 - 查看
        /// </summary>
        AccountingSubjectsSettingView = 482,
        /// <summary>
        ///公司设置 - 查看
        /// </summary>
        CompanySettingView = 484,
        /// <summary>
        ///价格体系设置 - 查看
        /// </summary>
        TierPricesSettingView = 486,
        /// <summary>
        ///打印模板 - 查看
        /// </summary>
        PrintTemplateSettingView = 488,
        /// <summary>
        ///备注设置 - 查看
        /// </summary>
        NoteSettingView = 490,
        /// <summary>
        ///商品设置 - 查看
        /// </summary>
        ProductSettingView = 494,

        /// <summary>
        /// 财务设置 - 查看
        /// </summary>
        FinanceSettingView = 525,

        /// <summary>
        /// 初始数据 - 查看
        /// </summary>
        StoreDataInitView = 526,

        /// <summary>
        ///用户列表 - 添加
        /// </summary>
        UserAdd = 105,
        /// <summary>
        ///角色管理 - 添加
        /// </summary>
        UserRoleAdd = 116,
        /// <summary>
        ///模块管理 - 添加
        /// </summary>
        ModuleAdd = 117,
        /// <summary>
        ///员工档案 - 状态修改
        /// </summary>
        UserListStatusUpdate = 429,
        /// <summary>
        ///收款对账单 - 确认上交
        /// </summary>
        AccountReceivableSave = 162,
        /// <summary>
        ///销售订单 - 红冲终止	
        /// </summary>
        SaleReservationBillReverse = 128,
        /// <summary>
        ///退货订单 - 红冲终止 
        /// </summary>
        ReturnOrdernReverse = 135,
        /// <summary>
        ///销售单 - 红冲终止 
        /// </summary>
        SaleBillReverse = 142,
        /// <summary>
        ///退货单 - 红冲终止 
        /// </summary>
        ReturnBillsReverse = 149,
        /// <summary>
        ///订货单 - 红冲终止 
        /// </summary>
        //PlaceOrderBillsReverse = 156,
        /// <summary>
        ///装车调度 - 红冲终止
        /// </summary>
        TrackSchedulingReverse = 168,
        /// <summary>
        ///采购订单 - 红冲终止
        /// </summary>
        PurchaseOrderReverse = 210,
        /// <summary>
        ///采购单 - 红冲终止
        /// </summary>
        PurchaseBillsReverse = 217,
        /// <summary>
        ///采购退货单 - 红冲终止
        /// </summary>
        PurchaseReturnReverse = 224,
        /// <summary>
        ///调拨单 - 红冲终止
        /// </summary>
        AllocationFormReverse = 237,
        /// <summary>
        ///盘点盈亏单 - 红冲终止
        /// </summary>
        InventoryProfitlossReverse = 244,
        /// <summary>
        ///成本调价单 - 红冲终止
        /// </summary>
        CostAdjustmentReverse = 251,
        /// <summary>
        ///报损单 - 红冲终止
        /// </summary>
        ReportLossReverse = 258,
        /// <summary>
        ///盘点单(整仓) - 红冲终止
        /// </summary>
        InventoryAllReverse = 265,
        /// <summary>
        ///盘点单(部分) - 红冲终止
        /// </summary>
        InventorySingleReverse = 272,
        /// <summary>
        ///组合单 - 红冲终止
        /// </summary>
        CombinationsReverse = 279,
        /// <summary>
        ///拆分单 - 红冲终止
        /// </summary>
        SplitOrderReverse = 286,
        /// <summary>
        ///收款单 - 红冲终止
        /// </summary>
        ReceiptBillsReverse = 317,
        /// <summary>
        ///付款单 - 红冲终止
        /// </summary>
        PaymentBillsReverse = 324,
        /// <summary>
        ///预收款单 - 红冲终止
        /// </summary>
        ReceivablesBillsReverse = 331,
        /// <summary>
        ///预付款单 - 红冲终止
        /// </summary>
        PrepaidBillsReverse = 338,
        /// <summary>
        ///费用支出 - 红冲终止
        /// </summary>
        ExpenseExpenditureReverse = 345,
        /// <summary>
        ///费用合同 - 红冲终止
        /// </summary>
        CostContractReverse = 352,
        /// <summary>
        ///收款单 - 红冲终止
        /// </summary>
        OtherIncomeReverse = 360,
        /// <summary>
        ///商品设置 - 编辑
        /// </summary>
        ProductSettingUpdate = 508,
        /// <summary>
        ///品牌档案 - 编辑
        /// </summary>
        BrandArchivesUpdate = 510,
        /// <summary>
        ///单位档案 - 编辑
        /// </summary>
        UnitArchivesUpdate = 513,
        /// <summary>
        ///统计类别 - 编辑
        /// </summary>
        StatisticalTypeUpdate = 516,
        /// <summary>
        ///赠品额度 - 编辑
        /// </summary>
        GiftAmountUpdate = 519,
        /// <summary>
        ///促销活动 - 编辑
        /// </summary>
        SalesPromotionUpdate = 522,
        /// <summary>
        ///库存预警设置 - 编辑
        /// </summary>
        StockWarningSettingUpdate = 477,
        /// <summary>
        ///APP打印设置 - 编辑
        /// </summary>
        AppPrintSettingUpdate = 479,
        /// <summary>
        ///电脑打印设置 - 编辑
        /// </summary>
        PcPrintSettingUpdate = 481,
        /// <summary>
        ///会计科目 - 编辑
        /// </summary>
        AccountingSubjectsSettingUpdate = 483,
        /// <summary>
        ///公司设置 - 编辑
        /// </summary>
        CompanySettingUpdate = 485,
        /// <summary>
        ///价格体系设置 - 编辑
        /// </summary>
        TierPricesSettingUpdate = 487,
        /// <summary>
        ///打印模板 - 编辑
        /// </summary>
        PrintTemplateSettingUpdate = 489,
        /// <summary>
        ///备注设置 - 编辑
        /// </summary>
        NoteSettingUpdate = 491,

        /// <summary>
        /// 财务设置 - 编辑
        /// </summary>
        FinanceSettingUpdate = 524,

        /// <summary>
        /// 初始数据 - 编辑
        /// </summary>
        StoreDataInitUpdate = 527,

        /// <summary>
        ///装车调度 - 调度
        /// </summary>
        TrackSchedulingScheduling = 169,
        /// <summary>
        ///车辆对货单 - 调拨
        /// </summary>
        VehicleToGoodsAllotted = 159,
        /// <summary>
        ///员工档案 - 账户修改
        /// </summary>
        UserListUpdate = 432,
        /// <summary>
        ///车辆对货单 - 车辆对货单
        /// </summary>
        VehicleToGoodsView = 157,
        /// <summary>
        ///订单转销售单 - 转单
        /// </summary>
        OrderToSaleBillsTransferBill = 171,
        /// <summary>
        ///员工档案 - 释放账号
        /// </summary>
        UserListResetAccount = 431,
        /// <summary>
        ///员工档案 - 重置密码
        /// </summary>
        UserListResetPassWord = 430,
        /// <summary>
        ///费用合同 - 驳回
        /// </summary>
        CostContractReject = 353,

        /// <summary>
        /// 换货单
        /// </summary>
        ExchangeBillsView = 150,
        ExchangeBillsPrint = 151,
        ExchangeBillsExport = 152,
        ExchangeBillsSave = 153,
        ExchangeBillsDelete = 154,
        ExchangeBillsApproved = 155,
        ExchangerBillsReverse = 156,
        ExchangeBillsReset = 173,

        //业绩考核
        AssessmentView = 600,
        /// <summary>
        ///拜访达成表 - 查看
        /// </summary>
        VisitingScheduleOnlineView = 759,
    }

    /// <summary>
    /// 允许的系统名称后缀
    /// </summary>
    public enum AccessStateEnum
    {
        /// 保存：xxx + Save
        Save = 0,
        /// 修改：xxx + Update
        Update = 1,
        /// 删除：xxx + Delete
        Delete = 2,
        /// 查看：xxx + View
        View = 3,
        /// 导出：xxx + Export
        Export = 4,
        /// 导入：xxx + Import
        Import = 5,
        /// 审核：xxx + Approved
        Approved = 6,
        /// 驳回：xxx + Reject
        Reject = 7,
        /// 调度：xxx + Scheduling
        Scheduling = 8,
        /// 重置账户（特）：xxx + ResetAccount
        ResetAccount = 9,
        /// 重置密码（特）：xxx + ResetPassWord
        ResetPassWord = 10,
        /// 调拨：xxx + Allotted
        Allotted = 11,
        /// 打印：xxx + Print
        Print = 12,
        /// 结账：xxx + Check
        Check = 13,
        /// 反结账：xxx + OutCheck
        OutCheck = 14
    }

    /// <summary>
    /// 成本月结
    /// </summary>
    public enum CostOfSettleEnum
    {
        /// <summary>
        /// 价格调整类
        /// </summary>
        CostOfPriceAdjust = 100,
        /// <summary>
        /// 采购退货类
        /// </summary>
        CostOfPurchaseReject = 101,
        /// <summary>
        /// 拆装组合类   
        /// </summary>
        CostOfJointGoods = 102,
        /// <summary>
        /// 库存调整类
        /// </summary>
        CostOfStockAdjust = 103,
        /// <summary>
        /// 库存损失类   
        /// </summary>
        CostOfStockLoss = 104,
        /// <summary>
        /// 销售类    
        /// </summary>
        CostOfSales = 105,

    }

    /// <summary>
    /// 成本计算方法
    /// </summary>
    public enum CostMethodEnum
    {
        /// <summary>
        /// 全月一次加权平均法：
        /// 加权平均法是指在一个计算期内（一般为一个月）、综合计算每种商品的加权平均单价、再乘以销售数量、计算商品销售成本的一种方法。其计算公式如下：
        /// 
        /// 存货单位成本=(月初库存存货的实际成本 + 本月收入存货的实际成本)÷(月初库存存货数量 + 本月收入存货数量)；
        /// 本月发出存货成本=本月发出存货的数量×存货单位成本。
        /// 或者 
        /// 加权平均单价＝（期初结存商品金额+本期收入商品金额-本期非销售发出商品金额）÷（期初结存商品数量+本期收入商品数量-本期非销售发出商品数量） 
        /// 本期商品销售成本＝本期商品销售数量×加权平均单价
        /// 
        /// 在日常工作中，由于计算加权平均单价往往不能整除，计算的结果必然会产生尾差，为了保证期末库存商品数额的准确性，可以采用逆算成本的方法。其计算公式如下：
        /// 期末结存商品金额＝期末结存商品数量×加权平均单价
        /// 本期商品销售成本＝期初结存商品金额+本期收入商品金额-本期非销售发出商品金额-期末结存商品金额
        /// </summary>
        AVERAGE = 0,
        /// <summary>
        /// 先进先出法：
        /// 先进先出法是根据先购进先销售的原则，以先购进商品的价格，先作为商品销售成本的一种计算方法。这种方法根据需要，可以用顺算成本的方法逐日结转成本，
        /// 也可以用逆算成本的方法定期结转成本。
        /// 采用顺算成本方法计算商品销售成本的具体做法是；先按最早购进商品的进价计算，销售完了，再按第二批购进商品的进价计算，依次类推。
        /// 如果销售的商品属于前后两批购进的，单价又不相同时，就要分别用两个单价计算。
        /// 采用逆算成本方法计算商品销售成本的具体做法是：根据先进先出原则的推理也就是后进后出的原则，在先计算期末结存商品金额时，
        /// 若期末结存商品数量小于或等于最后一批购进商品的数量，即按该批商品的单价计算期末结存商品金额；若期末结存商品数量大于最后一批购进商品的数量，
        /// 即从该批商品开始向前推算，直到与期末结存商品数量相等时为止，然后，将这一系列金额相加，其总和即为期末结存商品金额。
        /// 计算出期末结存商品金额后，再采用逆算成本的方法，计算本期商品销售成本。
        /// </summary>
        FIFO = 1,
        /// <summary>
        /// 定额成本（个别记价法）：
        /// 个别计价法又称分批实际进价法，是认定每一件或每一批商品的实际进价，计算该件或该批商品销售成本的一种方法。
        /// 在整批购进分批销售时，可以根据该批商品的实际购进单价，乘以销售量来计算商品销售成本。其计算公式如下：
        /// 
        /// 商品销售成本＝商品销售数量×该件（批次）商品购进单价
        /// 
        /// 采用个别计价法，对每件或每批购进的商品应分别存放，并分户登记库存商品明细账。对每次销售的商品，应在专用发票上注明进货件别或批次，便于按照该件或该批的实际购进单价计算商品销售成本。采用个别计价法计算商品销售成本，可以逐日结转商品销售成本。这种方法计算的商品销售成本最为准确，但计算起来工作量最为繁重，适用于能分清进货件别或批次的库存商品、直运商品、委托代销商品和分期收款发出商品等。
        /// </summary>
        STD = 2
    }

    /// <summary>
    /// 科目方向
    /// </summary>
    public enum DirectionsTypeEnum
    {
        /// <summary>
        /// 借
        /// </summary>
        IN = 0,
        /// <summary>
        /// 贷
        /// </summary>
        OUT = 1,
        /// <summary>
        /// 平
        /// </summary>
        BALANCE = 2
    }

    public enum FeedbackTypeEnum
    {
        /// <summary>
        /// 功能异常
        /// </summary>
        [Description("功能异常")]
        Exception = 0,

        /// <summary>
        /// 体验改善
        /// </summary>
        [Description("体验改善")]
        Perfect = 1,

        /// <summary>
        /// 意见建议
        /// </summary>
        [Description("意见建议")]
        Opinion = 2,

        /// <summary>
        /// 其他
        /// </summary>
        [Description("其它")]
        Other = 3
    }


    public enum PaperTypeEnum
    {
        [Description("无")]
        noPaper = 0,
        [Description("自定义")]
        custom = 1,
        [Description("标准票据打印纸(215.9*139.7mm)")]

        P1 =2,
        [Description("标准票据打印纸(215.9*93.1mm)")]
        P2 = 3,
        [Description("标准票据打印纸(215.9*69.9mm)")]
        P3 = 4,

        [Description("A1(594*841mm)")]
        A1 = 5,
        [Description("A2(420*594mm)")]
        A2 = 6,
        [Description("A3(297*420mm)")]
        A3 = 7,
        [Description("A4(210*297mm)")]
        A4 = 8,
        [Description("A5(148*210mm)")]
        A5 = 9,

        [Description("B1(728*1030mm)")]
        B1 = 10,
        [Description("B2(515*728mm)")]
        B2 = 11,
        [Description("B3(364*515mm)")]
        B3 = 12,
        [Description("B4(257*364mm)")]
        B4 = 13,
        [Description("B5(182*257mm)")]
        B5 = 14
    }
}
