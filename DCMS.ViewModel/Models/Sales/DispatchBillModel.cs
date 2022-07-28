using DCMS.Core.Domain.Sales;
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using DCMS.ViewModel.Models.Finances;

namespace DCMS.ViewModel.Models.Sales
{

    public partial class DispatchBillListModel : BaseModel, IParentList
    {
        public DispatchBillListModel()
        {
            Bills = new List<DispatchBillModel>();
        }

        public List<DispatchBillModel> Bills { get; set; }

        [HintDisplayName("客户Id", "客户Id")]
        public string TerminalId { get; set; }
        [HintDisplayName("客户", "客户")]
        public string TerminalName { get; set; }

        [HintDisplayName("单据编号", "单据编号")]
        public string BillNumber { get; set; }

        [HintDisplayName("业务员", "业务员")]
        public int? BusinessUserId { get; set; } = 0;
        public SelectList BusinessUsers { get; set; }

        [HintDisplayName("部门", "部门名称")]
        public int? DepartmentId { get; set; } = 0;
        public SelectList ParentList { get; set; }

        [HintDisplayName("仓库", "仓库")]
        public int? WareHouseId { get; set; } = 0;
        public SelectList WareHouses { get; set; }

        [HintDisplayName("送货员", "送货员")]
        public int? DeliveryUserId { get; set; } = 0;
        public SelectList DeliveryUsers { get; set; }

        [HintDisplayName("片区", "片区")]
        public int? DistrictId { get; set; } = 0;
        public SelectList Districts { get; set; }

        [HintDisplayName("备注", "备注")]
        public string Remark { get; set; }

        [DisplayName("开始时间")]

        [UIHint("DateTime")] public DateTime StartTime { get; set; }

        [DisplayName("开始时间")]

        [UIHint("DateTime")] public DateTime EndTime { get; set; }

        [HintDisplayName("状态(调度)", "状态(调度)")]
        public bool DispatchStatus { get; set; }

        [HintDisplayName("显示红冲调度单", "显示红冲调度单")]
        public bool? ShowDispatchReserved { get; set; }

        //客户渠道
        [HintDisplayName("客户渠道", "客户渠道")]
        public int? ChannelId { get; set; } = 0;
        public string ChannelName { get; set; }
        public SelectList Channels { get; set; }

        //客户等级
        [HintDisplayName("客户等级", "客户等级")]
        public int? RankId { get; set; } = 0;
        public string RankName { get; set; }
        public SelectList Ranks { get; set; }

        [HintDisplayName("单据类型", "单据类型")]
        public int? BillTypeId { get; set; } = 0;
        public string BillTypeName { get; set; }
        public SelectList BillTypes { get; set; }

    }

    /// <summary>
    /// 装车调度单
    /// </summary>
    public class DispatchBillModel : BaseEntityModel
    {

        public DispatchBillModel()
        {
            Items = new List<DispatchItemModel>();
        }

        public List<DispatchItemModel> Items { get; set; }

        /// <summary>
        /// 单据编号
        /// </summary>
        public string BillNumber { get; set; }
        public string BillBarCode { get; set; }
        public int BillId { get; set; }

        /// <summary>
        /// 单据类型
        /// </summary>
        public int BillTypeId { get; set; } = 0;

        /// <summary>
        /// 送货员Id
        /// </summary>
        [HintDisplayName("送货员", "送货员")]
        public int DeliveryUserId { get; set; } = 0;
        public SelectList DeliveryUsers { get; set; }
        /// <summary>
        /// 送货员名称
        /// </summary>
        [HintDisplayName("送货员", "送货员")]
        public string DeliveryUserName { get; set; }

        /// <summary>
        /// 车辆Id
        /// </summary>
        [HintDisplayName("车牌号", "车牌号")]
        public int CarId { get; set; } = 0;
        public SelectList Cars { get; set; }
        /// <summary>
        /// 车牌号
        /// </summary>
        [HintDisplayName("车牌号", "车牌号")]
        public string CarNO { get; set; }

        /// <summary>
        /// 部门ID
        /// </summary>
        [HintDisplayName("部门ID", "部门ID")]
        public int? BranchId { get; set; } = 0;

        #region 打印记录
        /// <summary>
        /// 打印整箱拆零合并单 用户id
        /// </summary>
        [HintDisplayName("打印整箱拆零合并单 用户id", "打印整箱拆零合并单 用户id")]
        public int? PrintWholeScrapUserId { get; set; } = 0;

        /// <summary>
        /// 打印整箱拆零合并单 状态
        /// </summary>
        [HintDisplayName("打印整箱拆零合并单 状态", "打印整箱拆零合并单 状态")]
        public bool? PrintWholeScrapStatus { get; set; }

        /// <summary>
        /// 打印整箱拆零合并单 时间
        /// </summary>
        [HintDisplayName("打印整箱拆零合并单 时间", "打印整箱拆零合并单 时间")]
        public DateTime? PrintWholeScrapDate { get; set; }

        /// <summary>
        /// 打印整箱拆零合并单 打印次数
        /// </summary>
        [HintDisplayName("打印整箱拆零合并单 打印次数", "打印整箱拆零合并单 打印次数")]
        public int? PrintWholeScrapNum { get; set; } = 0;

        /// <summary>
        /// 打印整箱装车单 用户id
        /// </summary>
        [HintDisplayName("打印整箱装车单 用户id", "打印整箱装车单 用户id")]
        public int? PrintWholeCarUserId { get; set; } = 0;

        /// <summary>
        /// 打印整箱装车单 状态
        /// </summary>
        [HintDisplayName("打印整箱装车单 状态", "打印整箱装车单 状态")]
        public bool? PrintWholeCarStatus { get; set; }

        /// <summary>
        /// 打印整箱装车单 时间
        /// </summary>
        [HintDisplayName("打印整箱装车单 时间", "打印整箱装车单 时间")]
        public DateTime? PrintWholeCarDate { get; set; }

        /// <summary>
        /// 打印整箱装车单 打印次数
        /// </summary>
        [HintDisplayName("打印整箱装车单 打印次数", "打印整箱装车单 打印次数")]
        public int? PrintWholeCarNum { get; set; } = 0;

        /// <summary>
        /// 为每个客户打印拆零装车单 用户id
        /// </summary>
        [HintDisplayName("为每个客户打印拆零装车单 用户id", "为每个客户打印拆零装车单 用户id")]
        public int? PrintEveryScrapCarUserId { get; set; } = 0;

        /// <summary>
        /// 为每个客户打印拆零装车单 状态
        /// </summary>
        [HintDisplayName("为每个客户打印拆零装车单 状态", "为每个客户打印拆零装车单 状态")]
        public bool? PrintEveryScrapCarStatus { get; set; }

        /// <summary>
        /// 为每个客户打印拆零装车单 时间
        /// </summary>
        [HintDisplayName("为每个客户打印拆零装车单 时间", "为每个客户打印拆零装车单 时间")]
        public DateTime? PrintEveryScrapCarDate { get; set; }

        /// <summary>
        /// 为每个客户打印拆零装车单 打印次数
        /// </summary>
        [HintDisplayName("为每个客户打印拆零装车单 打印次数", "为每个客户打印拆零装车单 打印次数")]
        public int? PrintEveryScrapCarNum { get; set; } = 0;

        /// <summary>
        /// 打印订单 用户id
        /// </summary>
        [HintDisplayName("打印订单 用户id", "打印订单 用户id")]
        public int? PrintOrderUserId { get; set; } = 0;

        /// <summary>
        /// 打印订单 状态
        /// </summary>
        [HintDisplayName("打印订单 状态", "打印订单 状态")]
        public bool? PrintOrderStatus { get; set; }

        /// <summary>
        /// 打印订单 时间
        /// </summary>
        [HintDisplayName("打印订单 时间", "打印订单 时间")]
        public DateTime? PrintOrderDate { get; set; }

        /// <summary>
        /// 打印订单 打印次数
        /// </summary>
        [HintDisplayName("打印订单 打印次数", "打印订单 打印次数")]
        public int? PrintOrderNum { get; set; } = 0;

        #endregion

        /// <summary>
        /// 制单人Id
        /// </summary>
        [HintDisplayName("制单人", "制单人")]
        public int MakeUserId { get; set; } = 0;
        /// <summary>
        /// 制单人名称
        /// </summary>
        [HintDisplayName("制单人", "制单人")]
        public string MakeUserName { get; set; }

        [HintDisplayName("审核人", "审核人")]
        public int? AuditedUserId { get; set; } = 0;
        [HintDisplayName("审核人", "审核人")]
        public string AuditedUserName { get; set; }
        [HintDisplayName("状态(审核)", "状态(审核)")]
        public bool AuditedStatus { get; set; }
        [HintDisplayName("审核时间", "审核时间")]
        public DateTime? AuditedDate { get; set; }

        [HintDisplayName("红冲人", "红冲人")]
        public int? ReversedUserId { get; set; } = 0;
        [HintDisplayName("红冲人", "红冲人")]
        public int? ReversedUserName { get; set; }
        [HintDisplayName("红冲状态", "红冲状态")]
        public bool ReversedStatus { get; set; }
        [HintDisplayName("红冲时间", "红冲时间")]
        public DateTime? ReversedDate { get; set; }

        [HintDisplayName("打印数", "打印数")]
        public int PrintNum { get; set; } = 0;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// 用户选择的数据
        /// </summary>
        public string SelectDatas { get; set; }

        [HintDisplayName("生成调拨单", "生成调拨单")]
        public int DispatchBillAutoAllocationBill { get; set; } = 0;
        public SelectList DispatchBillAutoAllocationBills { get; set; }

        //打印
        [HintDisplayName("打印", "打印")]
        public int DispatchBillCreatePrint { get; set; } = 0;
        public SelectList DispatchBillCreatePrints { get; set; }


        [HintDisplayName("仓库", "仓库")]
        public int WareHouseId { get; set; } = 0;
        [HintDisplayName("仓库", "仓库")]
        public string WareHouseName { get; set; }
        public SelectList WareHouses { get; set; }

        /// <summary>
        /// 调度状态 已调度则：true
        /// </summary>
        public bool BillStatus { get; set; }


        /// <summary>
        /// 调度单明细是否存在签收，存在签收则不能修改
        /// </summary>
        public bool ExistSign { get; set; }

        /// <summary>
        /// 显示XXX大XXX小
        /// </summary>
        public string OrderQuantityView { get; set; }
        /// <summary>
        /// 调度金额
        /// </summary>
        public decimal? OrderAmount { get; set; } = 0;

        /// <summary>
        /// 销售商品数量（小单位总和）
        /// </summary>
        public int? OrderQuantitySum { get; set; } = 0;
        ///// <summary>
        ///// 显示状红冲态
        ///// </summary>
        //public bool Status { get; set; }


    }


    /// <summary>
    /// 装车调度单明细
    /// </summary>
    public class DispatchItemModel : BaseEntityModel
    {
        /// <summary>
        /// 经销商Id
        /// </summary>
        //移除 = 0;

        /// <summary>
        /// 调度单Id
        /// </summary>
        public int? DispatchBillId { get; set; } = 0;

        /// <summary>
        /// 单据Id
        /// </summary>
        public int BillId { get; set; } = 0;

        /// <summary>
        /// 转单
        /// </summary>
        public int? ToBillId { get; set; } = 0;

        /// <summary>
        /// 单据编号
        /// </summary>
        public string BillNumber { get; set; }

        /// <summary>
        /// 单据类型Id
        /// </summary>
        public int BillTypeId { get; set; } = 0;

        /// <summary>
        /// 单据类型名称
        /// </summary>
        public string BillTypeName { get; set; }

        /// <summary>
        /// 交易时间
        /// </summary>
        [UIHint("DateTimeNullable")] public DateTime? TransactionDate { get; set; }

        /// <summary>
        /// 业务员Id
        /// </summary>
        public int? BusinessUserId { get; set; } = 0;
        /// <summary>
        /// 业务员名称
        /// </summary>
        public string BusinessUserName { get; set; }

        /// <summary>
        /// 送货员Id
        /// </summary>
        public int? DeliveryUserId { get; set; } = 0;

        /// <summary>
        /// 送货员名称
        /// </summary>
        public string DeliveryUserName { get; set; }

        /// <summary>
        /// 车辆
        /// </summary>
        public int CarId { get; set; } = 0;
        /// <summary>
        /// 车辆
        /// </summary>
        public string CarNo { get; set; }


        /// <summary>
        /// 客户Id
        /// </summary>
        public int? TerminalId { get; set; } = 0;

        /// <summary>
        /// 客户名称
        /// </summary>
        public string TerminalName { get; set; }

        /// <summary>
        /// 客户编码
        /// </summary>
        public string TerminalPointCode { get; set; }



        /// <summary>
        /// 客户地址
        /// </summary>
        public string TerminalAddress { get; set; }

        /// <summary>
        /// 订单金额
        /// </summary>
        public decimal? OrderAmount { get; set; } = 0;

        /// <summary>
        /// 仓库Id
        /// </summary>
        public int WareHouseId { get; set; } = 0;

        /// <summary>
        /// 仓库名称
        /// </summary>
        public string WareHouseName { get; set; }

        /// <summary>
        /// 销售商品数量（小单位总和）
        /// </summary>
        public int? OrderQuantitySum { get; set; } = 0;

        /// <summary>
        /// 显示XXX大XXX小
        /// </summary>
        public string OrderQuantityView { get; set; }

        /// <summary>
        /// 终端纬度坐标
        /// </summary>
        public double? Latitude { get; set; } = 0;
        /// <summary>
        /// 终端经度坐标
        /// </summary>
        public double? Longitude { get; set; } = 0;
        /// 签收人
        /// </summary>
        public int? SignUserId { get; set; } = 0;
        /// <summary>
        /// 签收状态：0待签收，1已签收，2拒收
        /// </summary>
        public int SignStatus { get; set; } = 0;
        /// <summary>
        /// 签收时间
        /// </summary>
        public DateTime? SignDate { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        public bool DispatchedStatus { get; set; }

        public ExchangeBillModel ExchangeBill { get; set; } = new ExchangeBillModel();
        public SaleReservationBillModel SaleReservationBill { get; set; } = new SaleReservationBillModel();
        public ReturnReservationBillModel ReturnReservationBill { get; set; } = new ReturnReservationBillModel();

        /// <summary>
        /// 签收验证码
        /// </summary>
        public string VerificationCode { get; set; }
        public decimal? SumAmount { get; set; } = 0;
    }

    /// <summary>
    /// 用于计算 XXX大XXX小时存放数据
    /// </summary>
    public class DispatchBillProductItem
    {
        /// <summary>
        /// 商品Id
        /// </summary>
        public int ProductId { get; set; } = 0;

        /// <summary>
        /// 商品小单位数量
        /// </summary>
        public int Quantity { get; set; } = 0;

        /// <summary>
        /// 大转小转换率（一个大单位数量等于多少个小单位数量）
        /// </summary>
        public int BigQuantity { get; set; } = 0;

    }

    /// <summary>
    /// 装车调度打印
    /// </summary>
    public class DispatchPrintData
    {
        [HintDisplayName("经销商", "经销商")]
        public int StoreId { get; set; } = 0;

        public int BillId { get; set; } = 0;

        [HintDisplayName("单据编号", "单据编号")]
        public string BillNumber { get; set; }
        public string BillBarCode { get; set; }

        public int CarId { get; set; } = 0;
        public string CarNo { get; set; }

        [HintDisplayName("送货员", "送货员")]
        public int DeliveryUserId { get; set; } = 0;
        [HintDisplayName("送货员", "送货员")]
        public string DeliveryUserName { get; set; }
        public SelectList DeliveryUsers { get; set; }

        private ICollection<DispatchPrintProductItem> _dispatchPrintProductItems;
        public virtual ICollection<DispatchPrintProductItem> DispatchPrintProductItems
        {
            get { return _dispatchPrintProductItems ?? (_dispatchPrintProductItems = new List<DispatchPrintProductItem>()); }
            protected set { _dispatchPrintProductItems = value; }
        }

        [HintDisplayName("合计", "合计")]
        public string Total { get; set; }
    }

    public class DispatchPrintProductItem
    {
        public int ProductId { get; set; } = 0;
        public string ProductName { get; set; }

        public int Quantity { get; set; } = 0;

        public string QuantityChange { get; set; }

    }

    /// <summary>
    /// 设置仓库
    /// </summary>
    public class DispatchSetWareHouseModel
    {

        /// <summary>
        /// 用户选择的数据
        /// </summary>
        public string SelectDatas { get; set; }

        [HintDisplayName("仓库", "仓库")]
        public int WareHouseId { get; set; } = 0;
        [HintDisplayName("仓库", "仓库")]
        public string WareHouseName { get; set; }
        public SelectList WareHouses { get; set; }
    }


    /// <summary>
    /// 用于表示送货签收
    /// </summary>
    public class DeliverySignModel : BaseEntityModel
    {
        /// <summary>
        /// 调度单Id
        /// </summary>
        public int DispatchBillId { get; set; }

        /// <summary>
        /// 单据类型
        /// </summary>
        public int BillTypeId { get; set; } = 0;

        /// <summary>
        /// 单据Id
        /// </summary>
        public int BillId { get; set; } = 0;

        /// <summary>
        /// 单据编号
        /// </summary>
        public string BillNumber { get; set; }


        /// <summary>
        /// 转单
        /// </summary>
        public int ToBillId { get; set; }
        public string ToBillNumber { get; set; }

        /// <summary>
        /// 总金额
        /// </summary>
        public decimal SumAmount { get; set; } = 0;

        /// <summary>
        /// 客户Id
        /// </summary>
        public int TerminalId { get; set; } = 0;
        public string TerminalName { get; set; }

        /// <summary>
        /// 业务员
        /// </summary>
        public int BusinessUserId { get; set; } = 0;
        public string BusinessUserName { get; set; }

        /// <summary>
        /// 送货员
        /// </summary>
        public int DeliveryUserId { get; set; } = 0;
        public string DeliveryUserName { get; set; }

        /// <summary>
        /// 终端纬度坐标
        /// </summary>
        public double? Latitude { get; set; } = 0;

        /// <summary>
        /// 终端经度坐标
        /// </summary>
        public double? Longitude { get; set; } = 0;

        /// <summary>
        /// 签收人
        /// </summary>
        public int SignUserId { get; set; } = 0;
        public string SignUserName { get; set; }

        /// <summary>
        /// 签收时间
        /// </summary>
        public DateTime? SignedDate { get; set; }

        /// <summary>
        /// 操作源
        /// </summary>
        public int? Operation { get; set; }

        /// <summary>
        /// 留存凭证照片
        /// </summary>
        public List<RetainPhotoModel> RetainPhotos { get; set; } = new List<RetainPhotoModel>();
        public ExchangeBillModel ExchangeBill { get; set; }
        public SaleBillModel SaleBill { get; set; }
        public ReturnBillModel ReturnBill { get; set; }
        public CostExpenditureBillModel CostExpenditureBill { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 老板电话
        /// </summary>
        public string BossCall { get; set; }

        /// <summary>
        /// 签名
        /// </summary>
        public string Signature { get; set; }

        /// <summary>
        /// 签收状态：0待签收，1已签收，2拒收
        /// </summary>
        public int SignStatus { get; set; }

    }


    /// <summary>
    /// 用于表示送货签收(销售订单、退货订单，换货单，销售单（车销），费用支出单)
    /// </summary>
    public class DeliverySignUpdateModel : BaseEntityModel
    {
        /// <summary>
        /// 留存凭证照片
        /// </summary>
        public List<RetainPhotoModel> RetainPhotos { get; set; }

        /// <summary>
        /// 调拨单（销售订单、退货订单，换货单）适用
        /// </summary>
        public int DispatchBillId { get; set; }

        /// <summary>
        /// 调拨明细（销售订单、退货订单，换货单）适用
        /// </summary>
        public int DispatchItemId { get; set; }

        /// <summary>
        /// 终端纬度坐标
        /// </summary>
        public double? Latitude { get; set; } = 0;

        /// <summary>
        /// 终端经度坐标
        /// </summary>
        public double? Longitude { get; set; } = 0;

        /// <summary>
        /// 单据类型
        /// </summary>
        public int BillTypeId { get; set; }
        /// <summary>
        /// 单据ID
        /// </summary>
        public int BillId { get; set; }

        /// <summary>
        /// 签名
        /// </summary>
        public string Signature { get; set; }

        //public SaleBillUpdateModel SBUM  { get; set; }
        //public CostExpenditureUpdateModel CEUM { get; set; }
    }
}
