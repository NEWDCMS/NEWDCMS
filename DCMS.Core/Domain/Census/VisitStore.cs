using System;
using System.Collections.Generic;

namespace DCMS.Core.Domain.Census
{

    public class VisitBase : BaseEntity
    {
        /// <summary>
        /// 业务员
        /// </summary>
        public int BusinessUserId { get; set; }

        /// <summary>
        /// 业务员
        /// </summary>
        public string BusinessUserName { get; set; }

        /// <summary>
        /// 纬度坐标
        /// </summary>
        public double? Latitude { get; set; } = 0;

        /// <summary>
        /// 经度坐标
        /// </summary>
        public double? Longitude { get; set; } = 0;

    }


    /// <summary>
    /// 用于门店拜访记录表
    /// </summary>
    public class VisitStore : VisitBase
    {

        //private ICollection<DoorheadPhoto> _doorheadPhotos;
        //private ICollection<DisplayPhoto> _displayPhotos;


        /// <summary>
        /// 客户Id
        /// </summary>
        public int TerminalId { get; set; }

        public string TerminalName { get; set; }

        /// <summary>
        /// 片区Id
        /// </summary>
        public int? DistrictId { get; set; } = 0;

        /// <summary>
        /// 渠道Id
        /// </summary>
        public int? ChannelId { get; set; } = 0;


        /// <summary>
        /// 客户编号
        /// </summary>
        public string CodeNumber { get; set; }


        /// <summary>
        /// 状态
        /// </summary>
        public int VisitTypeId { get; set; }
        public string VisitTypeName { get; set; }
        public VisitTypeEnum VisitType
        {
            get { return (VisitTypeEnum)VisitTypeId; }
            set { VisitTypeId = (int)value; }
        }

        /// <summary>
        /// 签到时间
        /// </summary>

        public DateTime SigninDateTime { get; set; }


        /// <summary>
        /// 签退时间
        /// </summary>
        public DateTime? SignOutDateTime { get; set; } = DateTime.Now;


        /// <summary>
        /// 预计下次订货时间
        /// </summary>
        public int? NextOrderDays { get; set; } = 0;

        /// <summary>
        /// 上次签到时间
        /// </summary>
        public int LastSigninDateTime { get; set; } = 0;

        /// <summary>
        /// 上次采购时间
        /// </summary>
        public int LastPurchaseDateTime { get; set; } = 0;

        /// <summary>
        /// 上次采购时间
        /// </summary>
        public DateTime? LastPurchaseDate { get; set; }

        /// <summary>
        /// 在店时间 （秒）
        /// </summary>
        public int? OnStoreStopSeconds { get; set; } = 0;


        /// <summary>
        /// 线路
        /// </summary>
        public int? LineId { get; set; } = 0;
        public string LineName { get; set; }



        /// <summary>
        /// 销售单
        /// </summary>
        public int? SaleBillId { get; set; } = 0;
        /// <summary>
        /// 销订单
        /// </summary>
        public int? SaleReservationBillId { get; set; } = 0;
        /// <summary>
        /// 退货单
        /// </summary>
        public int? ReturnBillId { get; set; } = 0;
        /// <summary>
        /// 退订单
        /// </summary>
        public int? ReturnReservationBillId { get; set; } = 0;

        /// <summary>
        /// 销售金额
        /// </summary>
        public decimal? SaleAmount { get; set; } = 0;
        /// <summary>
        /// 销订金额
        /// </summary>
        public decimal? SaleOrderAmount { get; set; } = 0;
        /// <summary>
        /// 退货金额
        /// </summary>
        public decimal? ReturnAmount { get; set; } = 0;
        /// <summary>
        /// 退订金额
        /// </summary>
        public decimal? ReturnOrderAmount { get; set; } = 0;

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 未拜访天数
        /// </summary>
        public int? NoVisitedDays { get; set; } = 0;


        public int SignTypeId { get; set; }
        public SignEnum SignType
        {
            get { return (SignEnum)SignTypeId; }
            set { SignTypeId = (int)value; }
        }

        #region  导航属性

        /// <summary>
        /// 门头照片
        /// </summary>
        //public virtual ICollection<DoorheadPhoto> DoorheadPhotos
        //{
        //    get { return _doorheadPhotos ?? (_doorheadPhotos = new List<DoorheadPhoto>()); }
        //    set { _doorheadPhotos = value; }
        //}

        /// <summary>
        /// 陈列照片
        /// </summary>
        //public virtual ICollection<DisplayPhoto> DisplayPhotos
        //{
        //    get { return _displayPhotos ?? (_displayPhotos = new List<DisplayPhoto>()); }
        //    set { _displayPhotos = value; }
        //}

        #endregion
    }

    /// <summary>
    /// 用于轨迹实时上报
    /// </summary>
    public class Tracking : VisitBase
    {
        /// <summary>
        /// 省份
        /// </summary>
        public string Province { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// 县区
        /// </summary>
        public string County { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 上报时间
        /// </summary>
        public DateTime CreateDateTime { get; set; }

    }


    public class QueryVisitStoreAndTracking
    {
        /// <summary>
        /// 经销商Id
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// 业务员
        /// </summary>
        public int BusinessUserId { get; set; }

        /// <summary>
        /// 业务员
        /// </summary>
        public string BusinessUserName { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public string Ctype { get; set; }

        /// <summary>
        /// 终端名称
        /// </summary>
        public string TerminalName { get; set; }

        /// <summary>
        /// 纬度坐标
        /// </summary>
        public double? Latitude { get; set; } = 0;

        /// <summary>
        /// 经度坐标
        /// </summary>
        public double? Longitude { get; set; } = 0;

        /// <summary>
        /// 上报时间
        /// </summary>
        public DateTime CreateDateTime { get; set; }
    }



    /// <summary>
    /// 客户拜访活跃度排行榜
    /// </summary>
    public class UserActivityRanking
    {
        /// <summary>
        /// 客户Id
        /// </summary>
        public int? TerminalId { get; set; } = 0;

        /// <summary>
        /// 客户名称
        /// </summary>
        public string TerminalName { get; set; }

        /// <summary>
        /// 拜访天数
        /// </summary>
        public int? VisitDaySum { get; set; } = 0;

    }

    public class BaseReach
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int LineId { get; set; }
        public string LineName { get; set; }
    }

    public class ReachQuery : BaseReach
    {
        public int OnStoreSeconds { get; set; }
        public DateTime SigninTime { get; set; }
        public bool Status { get; set; }
        public string TerminalName { get; set; }
        public int TerminalId { get; set; }
    }

    public class ReachOnlineQuery : BaseReach
    {
        public string TerminalName { get; set; }
        public int TerminalId { get; set; }
    }


    public class Reach : ReachQuery
    {
        public List<ReachQuery> RecordDatas { get; set; }
    }
}
