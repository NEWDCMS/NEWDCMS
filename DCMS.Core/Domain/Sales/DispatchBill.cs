using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DCMS.Core.Domain.Sales
{
    /// <summary>
    /// 装车调度单
    /// </summary>
    public class DispatchBill : BaseBill<DispatchItem>
    {
        public DispatchBill()
        {
            BillType = BillTypeEnum.DispatchBill;
        }

        /// <summary>
        /// 送货员Id
        /// </summary>
        public int DeliveryUserId { get; set; }

        /// <summary>
        /// 车辆
        /// </summary>
        public int CarId { get; set; }

        /// <summary>
        /// 部门ID
        /// </summary>
        public int? BranchId { get; set; } = 0;

        #region 打印记录
        /// <summary>
        /// 打印整箱拆零合并单 用户id
        /// </summary>
        public int? PrintWholeScrapUserId { get; set; } = 0;
        /// <summary>
        /// 打印整箱拆零合并单 状态
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool? PrintWholeScrapStatus { get; set; }
        /// <summary>
        /// 打印整箱拆零合并单 时间
        /// </summary>
        public DateTime? PrintWholeScrapDate { get; set; }
        /// <summary>
        /// 打印整箱拆零合并单 打印次数
        /// </summary>
        public int? PrintWholeScrapNum { get; set; } = 0;

        /// <summary>
        /// 打印整箱装车单 用户id
        /// </summary>
        public int? PrintWholeCarUserId { get; set; } = 0;
        /// <summary>
        /// 打印整箱装车单 状态
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool? PrintWholeCarStatus { get; set; }
        /// <summary>
        /// 打印整箱装车单 时间
        /// </summary>
        public DateTime? PrintWholeCarDate { get; set; }
        /// <summary>
        /// 打印整箱装车单 打印次数
        /// </summary>
        public int? PrintWholeCarNum { get; set; } = 0;

        /// <summary>
        /// 为每个客户打印拆零装车单 用户id
        /// </summary>
        public int? PrintEveryScrapCarUserId { get; set; } = 0;
        /// <summary>
        /// 为每个客户打印拆零装车单 状态
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool? PrintEveryScrapCarStatus { get; set; }
        /// <summary>
        /// 为每个客户打印拆零装车单 时间
        /// </summary>
        public DateTime? PrintEveryScrapCarDate { get; set; }
        /// <summary>
        /// 为每个客户打印拆零装车单 打印次数
        /// </summary>
        public int? PrintEveryScrapCarNum { get; set; } = 0;

        /// <summary>
        /// 打印订单 用户id
        /// </summary>
        public int? PrintOrderUserId { get; set; } = 0;
        /// <summary>
        /// 打印订单 状态
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool? PrintOrderStatus { get; set; }
        /// <summary>
        /// 打印订单 时间
        /// </summary>
        public DateTime? PrintOrderDate { get; set; }
        /// <summary>
        /// 打印订单 打印次数
        /// </summary>
        public int? PrintOrderNum { get; set; } = 0;
        #endregion



        /// <summary>
        /// 操作源
        /// </summary>
        public int? Operation { get; set; } = 0;
        public OperationEnum Operations
        {
            get { return (OperationEnum)Operation; }
            set { Operation = (int)value; }
        }

        /// <summary>
        /// 调度状态 已调度则：true
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool BillStatus { get; set; }

    }

    /// <summary>
    /// 调度单明细
    /// </summary>
    public class DispatchItem : BaseEntity
    {

        /// <summary>
        /// 调度单Id
        /// </summary>
        public int? DispatchBillId { get; set; } = 0;

        /// <summary>
        /// 单据类型
        /// </summary>
        public int BillTypeId { get; set; }

        /// <summary>
        /// 单据Id
        /// </summary>
        public int BillId { get; set; }

        /// <summary>
        /// 转单
        /// </summary>
        public int? ToBillId { get; set; } = 0;

        /// 签收人
        /// </summary>
        public int? SignUserId { get; set; } = 0;
        /// <summary>
        /// 签收状态：0待签收，1已签收，2拒收
        /// </summary>
        public int SignStatus { get; set; }
        /// <summary>
        /// 签收时间
        /// </summary>
        public DateTime? SignDate { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// 终端
        /// </summary>
        public int TerminalId { get; set; }
        /// <summary>
        /// 签收验证码
        /// </summary>
        public string VerificationCode { get; set; }

        #region 导航

        public virtual DispatchBill DispatchBill { get; set; }


        #endregion

    }


    /// <summary>
    /// 用于表示单据签收
    /// </summary>
    public class DeliverySign : BaseEntity
    {

        private ICollection<RetainPhoto> _retainPhotos;

      
        /// <summary>
        /// 单据类型
        /// </summary>
        public int BillTypeId { get; set; }

        /// <summary>
        /// 原来单据
        /// </summary>
        public int BillId { get; set; }
        public string BillNumber { get; set; }


        /// <summary>
        /// 转单
        /// </summary>
        public int ToBillId { get; set; }
        public string ToBillNumber { get; set; }

        /// <summary>
        /// 客户Id
        /// </summary>
        public int TerminalId { get; set; }

        /// <summary>
        /// 业务员
        /// </summary>
        public int BusinessUserId { get; set; }

        /// <summary>
        /// 送货员
        /// </summary>
        public int DeliveryUserId { get; set; }


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
        public int SignUserId { get; set; }


        /// <summary>
        /// 签收时间
        /// </summary>
        public DateTime? SignedDate { get; set; }


        /// <summary>
        /// 金额
        /// </summary>
        public decimal SumAmount { get; set; }

        /// <summary>
        /// 签名
        /// </summary>
        public string Signature { get; set; }


     /// <summary>
        /// 签收状态：0待签收，1已签收，2拒收
        /// </summary>
        public int SignStatus { get; set; }

        #region  导航属性

        /// <summary>
        /// 留存凭证照片
        /// </summary>
        public virtual ICollection<RetainPhoto> RetainPhotos
        {
            get { return _retainPhotos ?? (_retainPhotos = new List<RetainPhoto>()); }
            set { _retainPhotos = value; }
        }

        #endregion
    }


    /// <summary>
    /// 用于表示送货签收(销售订单、退货订单，换货单，销售单（车销），费用支出单)
    /// </summary>
    public class DeliverySignUpdate : BaseEntity
    {
        //"Missing type map configuration or unsupported mapping.\r\n\r\nMapping types:\r\nRetainPhotoModel -> Retain…"
        /// <summary>
        /// 留存凭证照片
        /// </summary>
        public List<RetainPhoto> RetainPhotos { get; set; } = new List<RetainPhoto>();

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
    }
}
