using System;


namespace DCMS.Core.Domain.CSMS
{
    /// <summary>
    /// 终端签收上报 
    /// </summary>
    public partial class TerminalSignReport : BaseEntity
    {


        /// <summary>
        /// 出库单来源 1:PC 2：小程序 3：SFA代下单 4：STTS 5：DCMS
        /// </summary>
        public int source { get; set; }


        /// <summary>
        /// 类型 0 送货 1 退货
        /// </summary>
        public int signType { get; set; }


        /// <summary>
        /// 流水号（系统编码+订单号）
        /// </summary>
        public string sttsBillNo { get; set; }


        /// <summary>
        /// 经销商编号
        /// </summary>
        public string dealerCode { get; set; }


        /// <summary>
        /// 经销商名称
        /// </summary>
        public string dealerName { get; set; }


        /// <summary>
        /// 出库对象编码（终端）
        /// </summary>
        public string whichCode { get; set; }

        /// <summary>
        /// 无用商品明细字符串
        /// </summary>
        public string orderDetail { get; set; } = "";

        /// <summary>
        /// 出库对象名称（终端名称）
        /// </summary>
        public string whichName { get; set; }


        /// <summary>
        /// 签收时间
        /// </summary>

        public string sttsSignDate { get; set; }


        /// <summary>
        /// 出库时间
        /// </summary>
        public string sttsCreatedDate { get; set; }


        /// <summary>
        /// 终端签收经度
        /// </summary>
        public string longitude { get; set; }


        /// <summary>
        /// 终端签收纬度
        /// </summary>
        public string latitude { get; set; }


        /// <summary>
        /// 营销中心编码
        /// </summary>
        public string region { get; set; }


        /// <summary>
        /// 出库对象类型，1：终端，2：1批，3:2批
        /// </summary>
        public string directType { get; set; }


        /// <summary>
        /// 出库类型,1.正常,2:调拨
        /// </summary>
        public string orderType { get; set; }

        /// <summary>
        /// DC经销商Id 父类已经存在
        /// </summary> 
        //public int storeId { get; set; }

        /// <summary>
        /// 上报状态：0 待上报，1 已经上报，未确认，2 已经上报，已确认
        /// </summary>
        public int sendStatus { get; set; } 

        /// <summary>
        /// 失败重试次数
        /// </summary>
        public int sentTries { get; set; }

        /// <summary>
        /// 确认重试次数
        /// </summary>
        public int cimformTries { get; set; }

        /// <summary>
        /// 上次上报时间
        /// </summary>
        public DateTime? sentOnUtc { get; set; }

        /// <summary>
        /// 状态确认时间
        /// </summary>
        public DateTime? cimformOnUtc { get; set; }
        
    }
}
