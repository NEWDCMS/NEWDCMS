using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace DCMS.ViewModel.Models.Sales
{

    public partial class CarGoodBillListModel : BaseModel, IParentList
    {
        public CarGoodBillListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Lists = new List<CarGoodBillModel>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<CarGoodBillModel> Lists { get; set; }

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
        [UIHint("DateTime")]
        public DateTime StartTime { get; set; }

        [DisplayName("结束时间")]
        [UIHint("DateTime")]
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 默认售价下拉
        /// </summary>
        public IEnumerable<SelectListItem> DefaultAmounts { get; set; }


        [HintDisplayName("状态", "状态")]
        public int? AuditingStatus { get; set; } = 0;
        //public SaleReservationBillStatus SaleReservationBillStatus { get; set; }


        [HintDisplayName("过滤", "过滤")]
        public int[] SaleReservationBillFilterSelectedIds { get; set; }
        public IEnumerable<SelectListItem> SaleReservationBillFilters { get; set; }

    }



    /// <summary>
    /// 车辆对货单
    /// </summary>
    public class CarGoodBillModel : BaseEntityModel
    {
        //继承基类的Id为 销售单、退货单 单Id

        /// <summary>
        /// 经销商Id
        /// </summary>
        //移除 = 0;

        /// <summary>
        /// 单据Id
        /// </summary>
        public int BillId { get; set; }

        /// <summary>
        /// 单据单号
        /// </summary>
        public string BillNumber { get; set; }

        /// <summary>
        /// 单据类型Id
        /// </summary>
        public int BillType { get; set; } = 0;

        /// <summary>
        /// 单据类型名称
        /// </summary>
        public string BillTypeName { get; set; }

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
        /// 客户Id
        /// </summary>
        public int TerminalId { get; set; } = 0;

        /// <summary>
        /// 客户名称
        /// </summary>
        public string TerminalName { get; set; }

        /// <summary>
        /// 转单时间
        /// </summary>
        public DateTime? ChangeDate { get; set; }

        /// <summary>
        /// 仓库Id
        /// </summary>
        public int WareHouseId { get; set; } = 0;

        /// <summary>
        /// 仓库名称
        /// </summary>
        public string WareHouseName { get; set; }

        /// <summary>
        /// 调拨车辆Id
        /// </summary>
        public int CarId { get; set; } = 0;
        /// <summary>
        /// 调拨车辆名称
        /// </summary>
        public string CarName { get; set; }

        /// <summary>
        /// 商品Id
        /// </summary>
        public int ProductId { get; set; } = 0;

        /// <summary>
        /// 商品名称
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 销售订单数量
        /// </summary>
        public int SaleReservationBillQty { get; set; } = 0;

        /// <summary>
        /// 销售单数量
        /// </summary>
        public int SaleBillQty { get; set; } = 0;

        /// <summary>
        /// 退货订单数量
        /// </summary>
        public int ReturnReservationBillQty { get; set; } = 0;

        /// <summary>
        /// 退货单数量
        /// </summary>
        public int ReturnBillQty { get; set; } = 0;

        /// <summary>
        /// 拒收
        /// </summary>
        public int RefuseQty { get; set; } = 0;

        /// <summary>
        /// 退货
        /// </summary>
        public int ReturnRealQty { get; set; } = 0;

        /// <summary>
        /// 送货员单选
        /// </summary>
        public SelectList DeliveryUsers { get; set; }


    }

    public class CarGoodDetailModel : BaseEntityModel
    {
        /// <summary>
        /// 商品Id
        /// </summary>
        public int ProductId { get; set; } = 0;

        /// <summary>
        /// 商品名称
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 拒收
        /// </summary>
        public int RefuseQty { get; set; } = 0;

        /// <summary>
        /// 退货
        /// </summary>
        public int ReturnRealQty { get; set; } = 0;

        /// <summary>
        /// 合计
        /// </summary>
        public int Total { get; set; } = 0;

    }

}
