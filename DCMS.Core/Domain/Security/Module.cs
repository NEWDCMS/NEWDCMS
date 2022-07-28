using DCMS.Core.Domain.Common;
using DCMS.Core.Domain.Users;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DCMS.Core.Domain.Security
{
    /// <summary>
    /// 表示菜单模块
    /// </summary>
    public class Module : BaseEntity
    {
        private ICollection<PermissionRecord> _permissions;
        private ICollection<ModuleRole> _moduleRoles;
        //移除
        /// <summary>
        /// 父模块Id
        /// </summary>
        public int? ParentId { get; set; } = 0;
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 链接地址
        /// </summary>
        public string LinkUrl { get; set; }
        /// <summary>
        /// 菜单图标
        /// </summary>
        public string Icon { get; set; }
        /// <summary>
        /// 是否是菜单
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool IsMenu { get; set; }
        /// <summary>
        /// 模块编号
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 是否启用
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool Enabled { get; set; }
        /// <summary>
        /// 父级模块
        /// </summary>
        [NotMapped]
        public Module ParentModule { get; set; }
        /// <summary>
        /// 模块子集
        /// </summary>
        [NotMapped]
        [JsonIgnore]
        public IList<Module> ChildModules { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreatedOnUtc { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int? DisplayOrder { get; set; } = 0;
        /// <summary>
        /// 是否系统模块
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool IsSystem { get; set; }
        /// <summary>
        /// 是否在手机端显示
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool? ShowMobile { get; set; } = false;
        /// <summary>
        /// 是否管理平台模块
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool IsPaltform { get; set; }
        /// <summary>
        /// 布局
        /// </summary>
        public int LayoutPositionId { get; set; }
        /// <summary>
        /// 控制器
        /// </summary>
        public string Controller { get; set; }

        /// <summary>
        /// 方法
        /// </summary>
        public string Action { get; set; }
        public MenuLayoutPosition LayoutPosition
        {
            get
            {
                return (MenuLayoutPosition)LayoutPositionId;
            }
            set
            {
                LayoutPositionId = (int)value;
            }
        }

        #region 导航属性
        /// <summary>
        /// 权限集合
        /// </summary>
        public virtual ICollection<PermissionRecord> Permissions
        {
            get { return _permissions ?? (_permissions = new List<PermissionRecord>()); }
            protected set { _permissions = value; }
        }
        /// <summary>
        /// 角色集合
        /// </summary>
        public virtual ICollection<ModuleRole> ModuleRoles
        {
            get { return _moduleRoles ?? (_moduleRoles = new List<ModuleRole>()); }
            protected set { _moduleRoles = value; }
        }
        #endregion
    }

    /// <summary>
    /// 用于快速查询
    /// </summary>
    public class QueryModule
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IList<int> Codes { get; set; } = new List<int>();
        public IList<int> Permissions { get; set; } = new List<int>();
        public IList<int> ModuleRoles { get; set; } = new List<int>();
    }

    /// <summary>
    /// 表示菜单模块与角色的映射
    /// </summary>
    public class ModuleRole : BaseEntity
    {
        public int Module_Id { get; set; }
        public int UserRole_Id { get; set; }

        [JsonIgnore]
        public virtual UserRole UserRole { get; set; }
        [JsonIgnore]
        public virtual Module Module { get; set; }
    }

    /// <summary>
    /// 角色权限
    /// </summary>
    public class RoleJurisdiction : BaseEntity
    {
        /// <summary>
        /// 权限ID
        /// </summary>
        public int jurisdiction_Id { get; set; }
        /// <summary>
        /// 系统角色代码（管理员、送货员、业务员）
        /// </summary>
        public string SystemName { get; set; }
        /// <summary>
        /// 平台（0 表示PC 端，1：表示APP）
        /// </summary>
        public int Platform { get; set; }

    }
    public static class AppGlobalSettings
    {
        public class APPModule
        {
            public bool Selected { get; set; }
            public int Id { get; set; }
            public int AType { get; set; }
            public int Count { get; set; }
            public string Name { get; set; }
            public string Icon { get; set; }
            public string Color { get; set; }
            public string Navigation { get; set; }
            public string Description { get; set; }
            public BillTypeEnum BillType { get; set; } = BillTypeEnum.None;
            public ChartTemplate ChartType { get; set; }
            public List<AccessGranularityEnum> PermissionCodes { get; set; } = new List<AccessGranularityEnum>();
        }
        public enum ChartTemplate
        {
            BarChart,
            LineChart,
            PieChart,
            RadarChart,
            ScatterChart
        }
        public enum CustomTemplate
        {
            Message,
            News,
            Valid,
            Invalid,
        }
        public class ICustomTemplate
        {
            [JsonIgnore]
            public int Type { get; set; }
            [JsonIgnore]
            public CustomTemplate TemplateType { get; set; }
        }
        public class IChartTemplate
        {
            [JsonIgnore]
            public int Type { get; set; }
            [JsonIgnore]
            public ChartTemplate TemplateType { get; set; }
        }

        public class MessageInfo : ICustomTemplate
        {
            public int Index { get; set; }
            public bool Selected { get; set; }
            /// <summary>
            /// 消息：
            /// 0（审批），1（收款），2（交账），3（推送）
            /// 通知：
            /// 4 (审核完成），5 调度完成，6 盘点完成，7 转单/签收完成，8 库存预警，9 签到异常，10 客户流失预警，11 开单异常，12 交账完成/撤销
            /// </summary>
            public MTypeEnum MType { get; set; }
            public string Title { get; set; }
            public string Content { get; set; }
            public string Icon { get; set; }
            public string Description { get; set; }
            public string Color { get; set; }
            public string Navigation { get; set; }
            public int Count { get; set; }
            public DateTime? Date { get; set; }
            public MessageInfo()
            {
                TemplateType = CustomTemplate.Message;
            }
            public IList<MessageItems> Items { get; set; } = new List<MessageItems>();
        }
        public class MessageItems
        {
            /// <summary>
            /// 发送用户序列“|”分割
            /// </summary>
            public List<string> Receives { get; set; } = new List<string>();
            /// <summary>
            /// 客户序列“|”分割
            /// </summary>

            public List<string> Terminals { get; set; } = new List<string>();
            /// <summary>
            /// 商品序列“|”分割
            /// </summary>

            public List<string> Products { get; set; } = new List<string>();
            /// <summary>
            /// 单据序列“|”分割
            /// </summary>

            public List<string> Bills { get; set; } = new List<string>();
            public Guid Key { get; set; } = Guid.NewGuid();
            public bool IsRead { get; set; }
            public MTypeEnum MType { get; set; }
            public string Title { get; set; }
            public string Content { get; set; }
            public string Icon { get; set; }
            public DateTime Date { get; set; }
            public string Navigation { get; set; }
            public BillTypeEnum BillType { get; set; }
            public string BillNumber { get; set; }
            public int BillId { get; set; }
        }

        /// <summary>
        /// 用于表示报表功能项
        /// </summary>
        /// <returns></returns>
        public static List<APPModule> ParpaerReports()
        {
            var datas = new List<APPModule>
            {
                new APPModule()
                {
                    Id = 1865,
                    Navigation = "CustomerRankingPage",
                    Name = "客户排行榜 ",
                    Icon = "&#xf1fe;",
                    Color = "#53a245",
                    Selected = true,
                    ChartType = ChartTemplate.BarChart,
                    PermissionCodes = new List<AccessGranularityEnum>()
                {
                   AccessGranularityEnum.SaleSummaryByCustomerView
                }
                },
                new APPModule()
                {
                    Id = 1,
                    Navigation = "SalesRankingPage",
                    Name = "业务销售排行",
                    Icon = "&#xf201;",
                    Color = "#53a245",
                    Selected = true,
                    ChartType = ChartTemplate.BarChart,
                    PermissionCodes = new List<AccessGranularityEnum>()
                {
                   AccessGranularityEnum.SaleSummaryByBUserView
                }
                },
                new APPModule()
                {
                    Id = 2,
                    Navigation = "BrandRankingPage",
                    Name = "品牌销量汇总",
                    Icon = "&#xf03a;",
                    Color = "#53a245",
                    Selected = true,
                    ChartType = ChartTemplate.BarChart,
                    PermissionCodes = new List<AccessGranularityEnum>()
                {
                    AccessGranularityEnum.SaleSummaryByBrandView
                }
                },
                new APPModule()
                {
                    Id = 3,
                    Navigation = "HotSalesRankingPage",
                    Name = "热销排行榜",
                    Icon = "&#xf160;",
                    Color = "#53a245",
                    Selected = true,
                    ChartType = ChartTemplate.BarChart,
                    PermissionCodes = new List<AccessGranularityEnum>()
                {
                    AccessGranularityEnum.HotRatingView
                }
                },
                new APPModule()
                {
                    Id = 4,
                    Navigation = "SaleTrendChatPage",
                    Name = "销量走势图",
                    Icon = "&#xf1de;",
                    Color = "#53a245",
                    Selected = true,
                    ChartType = ChartTemplate.LineChart,
                    PermissionCodes = new List<AccessGranularityEnum>()
                {
                    AccessGranularityEnum.SaleChartView
                }
                },
                new APPModule()
                {
                    Id = 5,
                    Navigation = "UnsalablePage",
                    Name = "库存滞销报表",
                    Icon = "&#xf275;",
                    Color = "#53a245",
                    Selected = true,
                    ChartType = ChartTemplate.BarChart,
                    PermissionCodes = new List<AccessGranularityEnum>()
                {
                    AccessGranularityEnum.InventoryUnsalableReportView
                }
                },
                new APPModule()
                {
                    Id = 6,
                    Navigation = "VisitingRatePage",
                    Name = "客户拜访分析",
                    Icon = "&#xf0cb;",
                    Color = "#53a245",
                    Selected = true,
                    ChartType = ChartTemplate.PieChart,
                    PermissionCodes = new List<AccessGranularityEnum>()
                {
                    AccessGranularityEnum.CustomerVisitAnalysisView
                }
                },
                new APPModule()
                {
                    Id = 7,
                    Navigation = "SalesProfitRankingPage",
                    Name = "销售利润排行",
                    Icon = "&#xf201;",
                    Color = "#53a245",
                    Selected = true,
                    ChartType = ChartTemplate.BarChart,
                    PermissionCodes = new List<AccessGranularityEnum>()
                {
                    AccessGranularityEnum.CostProfitView
                }
                },
                new APPModule()
                {
                    Id = 8,
                    Navigation = "SalesRatePage",
                    Name = "销售额分析",
                    Icon = "&#xf0ca;",
                    Color = "#53a245",
                    Selected = true,
                    ChartType = ChartTemplate.RadarChart,
                    PermissionCodes = new List<AccessGranularityEnum>()
                {
                    AccessGranularityEnum.SalesAnalysisView
                }
                },
                new APPModule()
                {
                    Id = 9,
                    Navigation = "NewCustomersPage",
                    Name = "新增客户分析",
                    Icon = "&#xf0c0;",
                    Color = "#53a245",
                    Selected = true,
                    ChartType = ChartTemplate.LineChart,
                    PermissionCodes = new List<AccessGranularityEnum>()
                {
                    AccessGranularityEnum.NewCustomerAnalysisView
                }
                },
                new APPModule()
                {
                    Id = 10,
                    Navigation = "CustomerVisitRankPage",
                    Name = "客户拜访排行",
                    Icon = "&#xf00a;",
                    Color = "#53a245",
                    Selected = true,
                    ChartType = ChartTemplate.BarChart,
                    PermissionCodes = new List<AccessGranularityEnum>()
                {
                    AccessGranularityEnum.VisitsRankingView
                }
                },
                new APPModule()
                {
                    Id = 11,
                    Navigation = "CustomerActivityPage",
                    Name = "客户活跃度",
                    Icon = "&#xf2c7;",
                    Color = "#53a245",
                    Selected = true,
                    ChartType = ChartTemplate.BarChart,
                    PermissionCodes = new List<AccessGranularityEnum>()
                {
                    AccessGranularityEnum.CustomerActivityView
                }
                },
                new APPModule()
                {
                    Id = 12,
                    Navigation = "HotOrderRankingPage",
                    Name = "热定排行榜",
                    Icon = "&#xf1b0;",
                    Color = "#53a245",
                    Selected = true,
                    ChartType = ChartTemplate.BarChart,
                    PermissionCodes = new List<AccessGranularityEnum>()
                {
                    AccessGranularityEnum.HotSalesAnalysisView
                }
                }
            };
            return datas;
        }

        /// <summary>
        /// 用于表示APP主应用功能项
        /// </summary>
        /// <returns></returns>
        public static List<APPModule> ParpaerAPPs()
        {
            var datas = new List<APPModule>
            {
                new APPModule()
                {
                    Id = 39,
                    AType=0,
                    BillType = BillTypeEnum.SaleReservationBill,
                    Navigation = "SaleOrderBillPage",
                    Name = "销售订单",
                    Icon = "&#xf07a;",
                    Color = "#eeeeee",
                    Selected = true,
                    PermissionCodes = new List<AccessGranularityEnum>()
                    {
                        AccessGranularityEnum.SaleReservationBillListView,
                        AccessGranularityEnum.SaleReservationBillSave,
                        AccessGranularityEnum.SaleOrderDelete,
                        AccessGranularityEnum.SaleReservationBillApproved,
                        AccessGranularityEnum.SaleReservationBillExport,
                        AccessGranularityEnum.SaleReservationPrint,
                        AccessGranularityEnum.SaleReservationBillReverse
                    }
                },
                new APPModule()
                {
                    Id = 41,
                    AType=0,
                    BillType = BillTypeEnum.SaleBill,
                    Navigation = "SaleBillPage",
                    Name = "销售单",
                    Icon = "&#xf291;",
                    Color = "#eeeeee",
                    Selected = true,
                    PermissionCodes = new List<AccessGranularityEnum>()
                {
                    AccessGranularityEnum.SaleBillsDelete,
                    AccessGranularityEnum.SaleBillSave,
                    AccessGranularityEnum.SaleBillReverse,
                    AccessGranularityEnum.SaleBillPrint,
                    AccessGranularityEnum.SaleBillListView,
                    AccessGranularityEnum.SaleBillExport,
                    AccessGranularityEnum.SaleBillApproved
                }
                },
                new APPModule()
                {
                    Id = 40,
                    AType=0,
                    BillType = BillTypeEnum.ReturnReservationBill,
                    Navigation = "ReturnOrderBillPage",
                    Name = "退货订单",
                    Icon = "&#xf274;",
                    Color = "#eeeeee",
                    Selected = true,
                    PermissionCodes = new List<AccessGranularityEnum>()
                {
                    AccessGranularityEnum.ReturnOrderView,
                    AccessGranularityEnum.ReturnOrderPrint,
                    AccessGranularityEnum.ReturnOrderExport,
                    AccessGranularityEnum.ReturnOrderSave,
                    AccessGranularityEnum.ReturnOrderDelete,
                    AccessGranularityEnum.ReturnOrderApproved,
                    AccessGranularityEnum.ReturnOrdernReverse
                }
                },
                new APPModule()
                {
                    Id = 42,
                    AType=0,
                    BillType = BillTypeEnum.ReturnBill,
                    Navigation = "ReturnBillPage",
                    Name = "退货单",
                    Icon = "&#xf272;",
                    Color = "#eeeeee",
                    Selected = true,
                    PermissionCodes = new List<AccessGranularityEnum>()
                {
                    AccessGranularityEnum.ReturnBillsView,
                    AccessGranularityEnum.ReturnBillsPrint,
                    AccessGranularityEnum.ReturnBillsExport,
                    AccessGranularityEnum.ReturnBillsSave,
                    AccessGranularityEnum.ReturnBillsDelete,
                    AccessGranularityEnum.ReturnBillsApproved,
                    AccessGranularityEnum.ReturnBillsReverse
                }
                },
                new APPModule()
                {
                    Id = 81,
                    AType=0,
                    BillType = BillTypeEnum.AllocationBill,
                    Navigation = "AllocationBillPage",
                    Name = "调拨单",
                    Icon = "&#xf1b9;",
                    Color = "#eeeeee",
                    Selected = true,
                    PermissionCodes = new List<AccessGranularityEnum>()
                {
                    AccessGranularityEnum.AllocationFormView,
                    AccessGranularityEnum.AllocationFormPrint,
                    AccessGranularityEnum.AllocationFormExport,
                    AccessGranularityEnum.AllocationFormSave,
                    AccessGranularityEnum.AllocationFormDelete,
                    AccessGranularityEnum.AllocationFormApproved,
                    AccessGranularityEnum.AllocationFormReverse
                }
                },
                new APPModule()
                {
                    Id = 46,
                    AType=0,
                    BillType = BillTypeEnum.DispatchBill,
                    Navigation = "TrackAllocationBillPage",
                    Name = "装车调拨",
                    Icon = "&#xf207;",
                    Color = "#eeeeee",
                    Selected = true,
                    PermissionCodes = new List<AccessGranularityEnum>()
                {
                    AccessGranularityEnum.AllocationFormView,
                    AccessGranularityEnum.AllocationFormPrint,
                    AccessGranularityEnum.AllocationFormExport,
                    AccessGranularityEnum.AllocationFormSave,
                    AccessGranularityEnum.AllocationFormDelete,
                    AccessGranularityEnum.AllocationFormApproved,
                    AccessGranularityEnum.AllocationFormReverse
                }
                },
                new APPModule()
                {
                    Id = 46,
                    AType=0,
                    BillType = BillTypeEnum.None,
                    Navigation = "BackStockBillPage",
                    Name = "回库调拨",
                    Icon = "&#xf218;",
                    Color = "#eeeeee",
                    Selected = true,
                    PermissionCodes = new List<AccessGranularityEnum>()
                {
                    AccessGranularityEnum.AllocationFormView,
                    AccessGranularityEnum.AllocationFormPrint,
                    AccessGranularityEnum.AllocationFormExport,
                    AccessGranularityEnum.AllocationFormSave,
                    AccessGranularityEnum.AllocationFormDelete,
                    AccessGranularityEnum.AllocationFormApproved,
                    AccessGranularityEnum.AllocationFormReverse
                }
                },
                new APPModule()
                {
                    Id = 105,
                    AType=0,
                    BillType = BillTypeEnum.CashReceiptBill,
                    Navigation = "ReceiptBillPage",
                    Name = "收款单",
                    Icon = "&#xf233;",
                    Color = "#eeeeee",
                    Selected = true,
                    PermissionCodes = new List<AccessGranularityEnum>()
                {
                    AccessGranularityEnum.ReceiptBillsView,
                    AccessGranularityEnum.ReceiptBillsPrint,
                    AccessGranularityEnum.ReceiptBillsExport,
                    AccessGranularityEnum.ReceiptBillsSave,
                    AccessGranularityEnum.ReceiptBillsDelete,
                    AccessGranularityEnum.ReceiptBillsApproved,
                    AccessGranularityEnum.ReceiptBillsReverse,
                    AccessGranularityEnum.OtherIncomeReverse
                }
                },
                new APPModule()
                {
                    Id = 109,
                    AType=0,
                    BillType = BillTypeEnum.CostExpenditureBill,
                    Navigation = "CostExpenditureBillPage",
                    Name = "费用支出",
                    Icon = "&#xf09d;",
                    Color = "#eeeeee",
                    Selected = true,
                    PermissionCodes = new List<AccessGranularityEnum>()
                {
                    AccessGranularityEnum.ExpenseExpenditureView,
                    AccessGranularityEnum.ExpenseExpenditurePrint,
                    AccessGranularityEnum.ExpenseExpenditureExport,
                    AccessGranularityEnum.ExpenseExpenditureSave,
                    AccessGranularityEnum.ExpenseExpenditureDelete,
                    AccessGranularityEnum.ExpenseExpenditureApproved,
                    AccessGranularityEnum.ExpenseExpenditureReverse
                }
                },
                new APPModule()
                {
                    Id = 107,
                    AType=0,
                    BillType = BillTypeEnum.AdvanceReceiptBill,
                    Navigation = "AdvanceReceiptBillPage",
                    Name = "预收款单",
                    Icon = "&#xf20b;",
                    Color = "#eeeeee",
                    Selected = true,
                    PermissionCodes = new List<AccessGranularityEnum>()
                {
                   AccessGranularityEnum.ReceivablesBillsView,
                   AccessGranularityEnum.ReceivablesBillsPrint,
                   AccessGranularityEnum.ReceivablesBillsExport,
                   AccessGranularityEnum.ReceivablesBillsSave,
                   AccessGranularityEnum.ReceivablesBillsDelete,
                   AccessGranularityEnum.ReceivablesBillsApproved,
                   AccessGranularityEnum.ReceivablesBillsReverse
                }
                },
                new APPModule()
                {
                    Id = 110,
                    AType=0,
                    BillType = BillTypeEnum.CostContractBill,
                    Navigation = "CostContractBillPage",
                    Name = "费用合同",
                    Icon = "&#xf0ea;",
                    Color = "#eeeeee",
                    Selected = true,
                    PermissionCodes = new List<AccessGranularityEnum>()
                {
                    AccessGranularityEnum.CostContractView,
                    AccessGranularityEnum.ExpenseDetailsView,
                    AccessGranularityEnum.ExpenseDetailsExport,
                    AccessGranularityEnum.CostContractPrint,
                    AccessGranularityEnum.CostContractExport,
                    AccessGranularityEnum.CostContractSave,
                    AccessGranularityEnum.CostContractDelete,
                    AccessGranularityEnum.CostContractApproved,
                    AccessGranularityEnum.CostContractReverse,
                    AccessGranularityEnum.CostContractReject
                }
                },
                new APPModule()
                {
                    Id = 72,
                    AType=0,
                    BillType = BillTypeEnum.PurchaseBill,
                    Navigation = "PurchaseOrderBillPage",
                    Name = "采购单",
                    Icon = "&#xf218;",
                    Color = "#eeeeee",
                    Selected = true,
                    PermissionCodes = new List<AccessGranularityEnum>()
                {
                    AccessGranularityEnum.PurchaseBillsView,
                    AccessGranularityEnum.PurchaseBillsPrint,
                    AccessGranularityEnum.PurchaseBillsExport,
                    AccessGranularityEnum.PurchaseBillsSave,
                    AccessGranularityEnum.PurchaseBillsDelete,
                    AccessGranularityEnum.PurchaseBillsApproved,
                    AccessGranularityEnum.PurchaseBillsReverse
                }
                },
                new APPModule()
                {
                    Id = 86,
                    AType=0,
                    BillType = BillTypeEnum.InventoryPartTaskBill,
                    Navigation = "InventoryOPBillPage",
                    Name = "盘点单",
                    Icon = "&#xf1b3;",
                    Color = "#eeeeee",
                    Selected = true,
                    PermissionCodes = new List<AccessGranularityEnum>()
                {
                    AccessGranularityEnum.InventoryAllView,
                    AccessGranularityEnum.InventoryAllPrint,
                    AccessGranularityEnum.InventoryAllExport,
                    AccessGranularityEnum.InventoryAllSave,
                    AccessGranularityEnum.InventoryAllDelete,
                    AccessGranularityEnum.InventoryAllApproved,
                    AccessGranularityEnum.InventoryAllReverse,
                    AccessGranularityEnum.InventorySingleView,
                    AccessGranularityEnum.InventorySinglePrint,
                    AccessGranularityEnum.InventorySingleExport,
                    AccessGranularityEnum.InventorySingleSave,
                    AccessGranularityEnum.InventorySingleDelete,
                    AccessGranularityEnum.InventorySingleApproved,
                    AccessGranularityEnum.InventorySingleReverse
                }
                },
                new APPModule()
                {
                    Id = 89,
                    AType=2,
                    BillType = BillTypeEnum.InventoryReportBill,
                    Navigation = "InventoryReportPage",
                    Name = "库存上报",
                    Icon = "&#xf1c0;",
                    Color = "#eeeeee",
                    Selected = true,
                    PermissionCodes = new List<AccessGranularityEnum>()
                {
                    AccessGranularityEnum.StockReportView,
                    AccessGranularityEnum.StockReportSave,
                    AccessGranularityEnum.StoreStockReportingView,
                    AccessGranularityEnum.StoreStockReportingExport,
                    AccessGranularityEnum.StoreStockSummeryReportingView,
                    AccessGranularityEnum.StoreStockSummeryReportingExport
                }
                },
                new APPModule()
                {
                    Id = 51,
                    AType=1,
                    Navigation = "CustomerRankingPage",
                    Name = "客户排行榜 ",
                    Icon = "&#xf03a;",
                    Color = "#eeeeee",
                    Selected = true,
                    PermissionCodes = new List<AccessGranularityEnum>()
                {
                    AccessGranularityEnum.SaleSummaryByCustomerView
                }
                },
                new APPModule()
                {
                    Id = 52,
                    AType=1,
                    Navigation = "SalesRankingPage",
                    Name = "业务销售排行",
                    Icon = "&#xf0ca;",
                    Color = "#eeeeee",
                    Selected = true,
                    PermissionCodes = new List<AccessGranularityEnum>()
                {
                    AccessGranularityEnum.SaleSummaryByBUserView
                }
                },
                new APPModule()
                {
                    Id = 55,
                    AType=1,
                    Navigation = "BrandRankingPage",
                    Name = "品牌销量汇总",
                    Icon = "&#xf201;",
                    Color = "#eeeeee",
                    Selected = true,
                    PermissionCodes = new List<AccessGranularityEnum>()
                {
                     AccessGranularityEnum.SaleSummaryByBrandView
                }
                },
                new APPModule()
                {
                    Id = 61,
                    AType=1,
                    Navigation = "HotSalesRankingPage",
                    Name = "热销排行榜",
                    Icon = "&#xf012;",
                    Color = "#eeeeee",
                    Selected = true,
                    PermissionCodes = new List<AccessGranularityEnum>()
                {
                     AccessGranularityEnum.HotRatingView
                }
                },
                new APPModule()
                {
                    Id = 62,
                    AType=1,
                    Navigation = "SaleTrendChatPage",
                    Name = "销量走势图",
                    Icon = "&#xf1fe;",
                    Color = "#eeeeee",
                    Selected = true,
                    PermissionCodes = new List<AccessGranularityEnum>()
                {
                      AccessGranularityEnum.SaleChartView
                }
                },
                new APPModule()
                {
                    Id = 90,
                    AType=3,
                    Navigation = "StockQueryPage",
                    Name = "库存查询",
                    Icon = "&#xf002;",
                    Color = "#eeeeee",
                    Selected = true,
                    PermissionCodes = new List<AccessGranularityEnum>()
                {
                    AccessGranularityEnum.StockReportingView
                }
                },
                new APPModule()
                {
                    Id = 98,
                    AType=1,
                    Navigation = "UnsalablePage",
                    Name = "库存滞销报表",
                    Icon = "&#xf080;",
                    Color = "#eeeeee",
                    Selected = true,
                    PermissionCodes = new List<AccessGranularityEnum>()
                {
                    AccessGranularityEnum.InventoryUnsalableReportView
                }
                },
                new APPModule()
                {
                    Id = 147,
                    AType=2,
                    Navigation = "ReceivablesPage",
                    Name = "应收款",
                    Icon = "&#xf19d;",
                    Color = "#eeeeee",
                    Selected = true,
                    PermissionCodes = new List<AccessGranularityEnum>()
                {
                   AccessGranularityEnum.CustomerReceivableView,
                   AccessGranularityEnum.CustomerReceivableExport
                }
                },
                new APPModule()
                {
                    Id = 163,
                    AType=2,
                    Navigation = "VisitingRatePage",
                    Name = "客户拜访分析",
                    Icon = "&#xf200;",
                    Color = "#eeeeee",
                    Selected = true,
                    PermissionCodes = new List<AccessGranularityEnum>()
                {
                   AccessGranularityEnum.CustomerVisitAnalysisView,
                   AccessGranularityEnum.CustomerVisitAnalysisExport
                }
                },
                new APPModule()
                {
                    Id = 116,
                    AType=1,
                    Navigation = "SalesProfitRankingPage",
                    Name = "销售利润排行",
                    Icon = "&#xf200;",
                    Color = "#eeeeee",
                    Selected = true,
                    PermissionCodes = new List<AccessGranularityEnum>()
                {
                    AccessGranularityEnum.CostProfitView,
                    AccessGranularityEnum.ProfitStatementView,
                    AccessGranularityEnum.ProfitStatementPrint,
                    AccessGranularityEnum.ProfitStatementExport
                }
                },
                new APPModule()
                {
                    Id = 64,
                    AType=1,
                    Navigation = "SalesRatePage",
                    Name = "销售额分析",
                    Icon = "&#xf162;",
                    Color = "#eeeeee",
                    Selected = true,
                    PermissionCodes = new List<AccessGranularityEnum>()
                {
                    AccessGranularityEnum.SalesAnalysisView
                }
                },
                new APPModule()
                {
                    Id = 162,
                    AType=2,
                    Navigation = "NewCustomersPage",
                    Name = "新增客户分析",
                    Icon = "&#xf0c0;",
                    Color = "#eeeeee",
                    Selected = true,
                    PermissionCodes = new List<AccessGranularityEnum>()
                {
                    AccessGranularityEnum.NewCustomerAnalysisView
                }
                },
                new APPModule()
                {
                    Id = 161,
                    AType=2,
                    Navigation = "CustomerVisitRankPage",
                    Name = "客户拜访排行",
                    Icon = "&#xf00a;",
                    Color = "#eeeeee",
                    Selected = true,
                    PermissionCodes = new List<AccessGranularityEnum>()
                {
                    AccessGranularityEnum.VisitsRankingView
                }
                },
                new APPModule()
                {
                    Id = 157,
                    AType=2,
                    Navigation = "CustomerActivityPage",
                    Name = "客户活跃度",
                    Icon = "&#xf2c7;",
                    Color = "#eeeeee",
                    Selected = true,
                    PermissionCodes = new List<AccessGranularityEnum>()
                {
                    AccessGranularityEnum.CustomerActivityView
                }
                },
                new APPModule()
                {
                    Id = 66,
                    AType=1,
                    Navigation = "HotOrderRankingPage",
                    Name = "热定排行榜",
                    Icon = "&#xf1b0;",
                    Color = "#eeeeee",
                    Selected = true,
                    PermissionCodes = new List<AccessGranularityEnum>()
                {
                     AccessGranularityEnum.HotSalesAnalysisView
                }
                },
                new APPModule()
                {
                    Id = 65,
                    AType=1,
                    Navigation = "OrderQuantityAnalysisPage",
                    Name = "订单额分析",
                    Icon = "&#xf155;",
                    Color = "#eeeeee",
                    Selected = true,
                    PermissionCodes = new List<AccessGranularityEnum>()
                {
                    AccessGranularityEnum.OrderAnalysisView
                }
                },
                new APPModule()
                {
                    Id = 1866,
                    AType=1,
                    Navigation = "MyReportingPage",
                    Name = "我的报表",
                    Icon = "&#xf2dc;",
                    Color = "#eeeeee",
                    Selected = true,
                    PermissionCodes = new List<AccessGranularityEnum>()
                    {

                    }
                },
                new APPModule()
                {
                    Id = 44,
                    AType=0,
                    Navigation = "ReconciliationORreceivablesPage",
                    Name = "收款对账 ",
                    Icon = "&#xf0b1;",
                    Color = "#eeeeee",
                    Selected = true,
                    PermissionCodes = new List<AccessGranularityEnum>()
                    {
                        AccessGranularityEnum.AccountReceivableView,
                        AccessGranularityEnum.AccountReceivableExport,
                        AccessGranularityEnum.AccountReceivableSave,
                        AccessGranularityEnum.AccountReceivableRevocation
                    }
                },
                //new APPModule()
                //{
                //    Id = 32,
                //    AType=2,
                //    Navigation = "FieldTrackPage",
                //    Name = "外勤轨迹",
                //    Icon = "fas-street-view",
                //    Color = "#eeeeee",
                //    Selected = true,
                //    PermissionCodes = new List<AccessGranularityEnum>()
                //    {

                //    }
                //},
                new APPModule()
                {
                    Id = 1868,
                    AType=2,
                    Navigation = "InvitationPage",
                    Name = "拜访达成",
                    Icon = "fas-street-view",
                    Color = "#eeeeee",
                    Selected = true,
                    PermissionCodes = new List<AccessGranularityEnum>()
                    {

                    }
                },
                new APPModule()
                {
                    Id = 154,
                    AType=2,
                    Navigation = "VisitRecordsPage",
                    Name = "业务员拜访记录",
                    Icon = "&#xf1da;",
                    Color = "#eeeeee",
                    Selected = true,
                    PermissionCodes = new List<AccessGranularityEnum>()
                    {
                        AccessGranularityEnum.VisitingRecordsView,
                    }
                },

                new APPModule()
                {
                    Id = 1861,
                    AType=0,
                    Navigation = "ViewBillPage",
                    Name = "我的单据",
                    Icon = "&#xf07c;",
                    Color = "#eeeeee",
                    Selected = true,
                    PermissionCodes = new List<AccessGranularityEnum>()
                {
                    AccessGranularityEnum.SaleReservationBillListView,
                    AccessGranularityEnum.SaleReservationPrint,

                    AccessGranularityEnum.SaleBillListView,
                    AccessGranularityEnum.SaleBillPrint,

                    AccessGranularityEnum.ReturnOrderView,
                    AccessGranularityEnum.ReturnOrderPrint,

                    AccessGranularityEnum.ReturnBillsView,
                    AccessGranularityEnum.ReturnBillsPrint,

                    AccessGranularityEnum.AllocationFormView,
                    AccessGranularityEnum.AllocationFormPrint,

                    AccessGranularityEnum.ReceiptBillsView,
                    AccessGranularityEnum.ReceiptBillsPrint,

                    AccessGranularityEnum.ExpenseExpenditureView,
                    AccessGranularityEnum.ExpenseExpenditurePrint,

                    AccessGranularityEnum.ReceivablesBillsView,
                    AccessGranularityEnum.ReceivablesBillsPrint,

                    AccessGranularityEnum.ExpenseDetailsView,
                    AccessGranularityEnum.ExpenseDetailsExport,
                    AccessGranularityEnum.CostContractView,
                    AccessGranularityEnum.CostContractPrint,

                    AccessGranularityEnum.PurchaseBillsView,
                    AccessGranularityEnum.PurchaseBillsPrint,

                    AccessGranularityEnum.InventoryAllView,
                    AccessGranularityEnum.InventoryAllPrint,

                   AccessGranularityEnum.InventorySingleView,
                    AccessGranularityEnum.InventorySinglePrint
                }
                },
                new APPModule()
                {
                    Id = 1862,
                    AType = 0,
                    Navigation = "BillSummaryPage",
                    Name = "单据汇总",
                    Icon = "&#xf00a;",
                    Color = "#eeeeee",
                    Selected = true,
                    PermissionCodes = new List<AccessGranularityEnum>()
                {
                    AccessGranularityEnum.SaleReservationBillListView,
                    AccessGranularityEnum.SaleReservationPrint,

                    AccessGranularityEnum.SaleBillListView,
                    AccessGranularityEnum.SaleBillPrint,

                    AccessGranularityEnum.ReturnOrderView,
                    AccessGranularityEnum.ReturnOrderPrint,

                    AccessGranularityEnum.ReturnBillsView,
                    AccessGranularityEnum.ReturnBillsPrint,

                    AccessGranularityEnum.AllocationFormView,
                    AccessGranularityEnum.AllocationFormPrint,

                    AccessGranularityEnum.ReceiptBillsView,
                    AccessGranularityEnum.ReceiptBillsPrint,

                    AccessGranularityEnum.ExpenseExpenditureView,
                    AccessGranularityEnum.ExpenseExpenditurePrint,

                    AccessGranularityEnum.ReceivablesBillsView,
                    AccessGranularityEnum.ReceivablesBillsPrint,

                    AccessGranularityEnum.ExpenseDetailsView,
                    AccessGranularityEnum.ExpenseDetailsExport,
                    AccessGranularityEnum.CostContractView,
                    AccessGranularityEnum.CostContractPrint,

                    AccessGranularityEnum.PurchaseBillsView,
                    AccessGranularityEnum.PurchaseBillsPrint,

                    AccessGranularityEnum.InventoryAllView,
                    AccessGranularityEnum.InventoryAllPrint,

                    AccessGranularityEnum.InventorySingleView,
                    AccessGranularityEnum.InventorySinglePrint

                }
                },
                new APPModule()
                {
                    Id = 122,
                    AType=3,
                    Navigation = "ProductArchivesPage",
                    Name = "商品档案",
                    Icon = "&#xf290;",
                    Color = "#eeeeee",
                    Selected = true,
                    PermissionCodes = new List<AccessGranularityEnum>()
                {
                   AccessGranularityEnum.ProductArchivesView,
                   AccessGranularityEnum.ProductArchivesPrint,
                   AccessGranularityEnum.ProductArchivesExport,
                   AccessGranularityEnum.ProductArchivesSave
                }
                },

                new APPModule()
                {
                    Id = 130,
                    AType=3,
                    Navigation = "CustomerArchivesPage",
                    Name = "客户档案",
                    Icon = "&#xf0c0;",
                    Color = "#eeeeee",
                    Selected = true,
                    PermissionCodes = new List<AccessGranularityEnum>()
                    {
                        AccessGranularityEnum.EndPointListView,
                        AccessGranularityEnum.EndPointListPrint,
                        AccessGranularityEnum.EndPointListExport,
                        AccessGranularityEnum.EndPointListSave
                    }
                },
                new APPModule()
                {
                    Id = 67,
                    AType=2,
                    Navigation = "VisitStorePage",
                    Name = "拜访门店",
                    Icon = "&#xf206;",
                    Color = "#eeeeee",
                    Selected = true,
                    PermissionCodes = new List<AccessGranularityEnum>()
                    {

                    }
                },

                new APPModule()
                {
                    Id = 42,
                    AType=0,
                    Navigation = "DeliveryReceiptPage",
                    Name = "送货签收",
                    Icon = "&#xf274;",
                    Color = "#eeeeee",
                    Selected = true,
                    PermissionCodes = new List<AccessGranularityEnum>()
                    {

                    }
                },
                new APPModule()
                {
                    Id = 1864,
                    AType=0,
                    BillType = BillTypeEnum.ExchangeBill,
                    Navigation = "ExchangeBillPage",
                    Name = "换货单",
                    Icon = "&#xf1b8;",
                    Color = "#eeeeee",
                    Selected = true,
                    PermissionCodes = new List<AccessGranularityEnum>()
                    {
                        AccessGranularityEnum.ExchangeBillsView,
                        AccessGranularityEnum.ExchangeBillsSave,
                        AccessGranularityEnum.ExchangeBillsApproved,
                        AccessGranularityEnum.ExchangeBillsExport,
                        AccessGranularityEnum.ExchangeBillsPrint,
                        AccessGranularityEnum.ExchangerBillsReverse
                    }
                }
            };

            return datas;
        }

        /// <summary>
        /// 用于表示订阅频道功能项
        /// </summary>
        /// <returns></returns>
        public static List<MessageInfo> ParpaerSubscribes()
        {
            var datas = new List<MessageInfo>
            {
                //待办
                new MessageInfo()
                {
                    Index = 0,
                    MType = MTypeEnum.Message,
                    Navigation = "ViewMessagePage",
                    Title = "审批",
                    Icon = "&#xf028;",
                    Color = "#53a245",
                    Selected = true,
                    Content = "无数据内容",
                    Description = "当用新的单据需要审核时，系统将给你发送消息。该消息同时受到相关单据查看权限控制。"
                },
                new MessageInfo()
                {
                    Index = 1,
                    MType = MTypeEnum.Receipt,
                    Navigation = "ViewMessagePage",
                    Title = "收款",
                    Icon = "&#xf1cd;",
                    Color = "#53a245",
                    Selected = true,
                    Content = "无数据内容",
                    Description = "系统每天会统计出欠款近一个月的单据和门店，并推送给你消息。"
                },
                new MessageInfo()
                {
                    Index = 2,
                    MType = MTypeEnum.Hold,
                    Navigation = "ViewMessagePage",
                    Title = "交账",
                    Icon = "&#xf0cc;",
                    Color = "#53a245",
                    Selected = true,
                    Content = "无数据内容",
                    Description = "当有待交账单据时，系统会 根据你的交账习惯，提前提醒你。"
                },

                //通知
                new MessageInfo()
                {
                    Index = 4,
                    MType = MTypeEnum.Audited,
                    Navigation = "ViewMessagePage",
                    Title = "审核完成",
                    Icon = "&#xf0a1;",
                    Color = "#53a245",
                    Selected = true,
                    Content = "无数据内容",
                    Description = "你提交的单据被认人工审核后，你将立即收到审核完成的通知。"
                },
                new MessageInfo()
                {
                    Index = 5,
                    MType = MTypeEnum.Scheduled,
                    Navigation = "ViewMessagePage",
                    Title = "调度完成",
                    Icon = "&#xf02e;",
                    Color = "#53a245",
                    Selected = true,
                    Content = "无数据内容",
                    Description = "订单完成调度时，你将立即收到调度完成的通知。"
                },
                new MessageInfo()
                {
                    Index = 6,
                    MType = MTypeEnum.InventoryCompleted,
                    Navigation = "ViewMessagePage",
                    Title = "盘点完成",
                    Icon = "&#xf02d;",
                    Color = "#53a245",
                    Selected = true,
                    Content = "无数据内容",
                    Description = "仓库盘点完成时,你将立即收到仓库盘点完成通知,并告知你盘点状态。"
                },
                new MessageInfo()
                {
                    Index = 7,
                    MType = MTypeEnum.TransferCompleted,
                    Navigation = "ViewMessagePage",
                    Title = "转单/签收完成",
                    Icon = "&#xf02c;",
                    Color = "#53a245",
                    Selected = true,
                    Content = "无数据内容",
                    Description = "有订单被转单/或者订单被签收时，相关员工将立即收到完成通知，老板会收到汇总情况。"
                },
                new MessageInfo()
                {
                    Index = 8,
                    MType = MTypeEnum.InventoryWarning,
                    Navigation = "ViewMessagePage",
                    Title = "库存预警",
                    Icon = "&#xf05a;",
                    Color = "#53a245",
                    Selected = true,
                    Content = "无数据内容",
                    Description = "一旦出现商品积压，商品缺货的情况，你将立即收到库存预警通知。"
                },
                new MessageInfo()
                {
                    Index = 9,
                    MType = MTypeEnum.CheckException,
                    Navigation = "ViewMessagePage",
                    Title = "签到异常",
                    Icon = "&#xf0d1;",
                    Color = "#53a245",
                    Selected = true,
                    Content = "无数据内容",
                    Description = "当业务员签到的位置离门店过远时，你将收到通知。"
                },
                new MessageInfo()
                {
                    Index = 10,
                    MType = MTypeEnum.LostWarning,
                    Navigation = "ViewMessagePage",
                    Title = "客户流失预警",
                    Icon = "&#xf2a0;",
                    Color = "#53a245",
                    Selected = true,
                    Content = "无数据内容",
                    Description = "通过数据分析，系统会筛选出流失性可能较大的客户，并及时的推送通知你。"
                },
                new MessageInfo()
                {
                    Index = 11,
                    MType = MTypeEnum.LedgerWarning,
                    Navigation = "ViewMessagePage",
                    Title = "开单异常",
                    Icon = "&#xf071;",
                    Color = "#53a245",
                    Selected = true,
                    Content = "无数据内容",
                    Description = "当有员工在当日交账完成后，再次开具单据与收款相关的单据，且系统为自动审核时，你将收到开单异常的通知。"
                },
                new MessageInfo()
                {
                    Index = 12,
                    MType = MTypeEnum.Paymented,
                    Navigation = "ViewMessagePage",
                    Title = "交账完成/撤销 ",
                    Icon = "&#xf122;",
                    Color = "#53a245",
                    Selected = true,
                    Content = "无数据内容",
                    Description = "交账完成后者撤销时，相关业务员或者送货员将收到消息，每天：17点，老板会收到交账汇总。"
                }
            };

            return datas;
        }
    }
}
