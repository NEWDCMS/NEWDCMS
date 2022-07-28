using DCMS.Core;
using DCMS.Core.Domain.Census;
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace DCMS.ViewModel.Models.Report
{
    public partial class VisitStoreListModel : BaseModel
    {
        public VisitStoreListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Items = new List<VisitStoreModel>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<VisitStoreModel> Items { get; set; }

        #region 用于条件检索
        [HintDisplayName("客户", "客户")]
        public int? TerminalId { get; set; } = 0;
        public string TerminalName { get; set; }

        [HintDisplayName("业务员", "业务员")]
        public int? BusinessUserId { get; set; } = 0;
        public string BusinessUserName { get; set; }
        public SelectList BusinessUsers { get; set; }


        [HintDisplayName("客户渠道", "客户渠道")]
        public int? ChannelId { get; set; } = 0;
        public string ChannelName { get; set; }
        public SelectList Channels { get; set; }

        [HintDisplayName("客户片区", "客户片区")]
        public int? DistrictId { get; set; } = 0;
        public string DistrictName { get; set; }
        public SelectList Districts { get; set; }

        [HintDisplayName("签到日期", "签到日期")]
        [UIHint("DateTime")]
        public DateTime SigninDateTime { get; set; }

        [HintDisplayName("签退日期", "签退日期")]
        [UIHint("DateTime")]
        public DateTime SignOutDateTime { get; set; }

        [HintDisplayName("状态", "状态")]
        public int? VisitTypeId { get; set; } = 0;
        public string VisitTypeName { get; set; }
        public SelectList VisitTypes { get; set; }
        #endregion

    }

    /// <summary>
    /// 用于添加门店拜访记录
    /// </summary>
    public partial class VisitStoreModel : BaseEntityModel
    {
        /// <summary>
        /// 客户Id
        /// </summary>
        public int TerminalId { get; set; } = 0;
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
        /// 业务员
        /// </summary>
        public int BusinessUserId { get; set; } = 0;
        public string BusinessUserName { get; set; }
        public string FaceImage { get; set; }

        /// <summary>
        /// 客户编号
        /// </summary>
        public string CodeNumber { get; set; }


        /// <summary>
        /// 状态
        /// </summary>
        public int VisitTypeId { get; set; } = 0;
        public string VisitTypeName { get; set; }

        [JsonIgnore]
        public VisitTypeEnum VisitType { get; set; }

        public int SignTypeId { get; set; } = 0;

        [JsonIgnore]
        public SignEnum SignType { get; set; }


        /// <summary>
        /// 签到时间
        /// </summary>

        public DateTime SigninDateTime { get; set; }


        /// <summary>
        /// 签退时间
        /// </summary>
        public DateTime? SignOutDateTime { get; set; }


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
        /// 纬度坐标
        /// </summary>
        public double? Latitude { get; set; } = 0;

        /// <summary>
        /// 经度坐标
        /// </summary>
        public double? Longitude { get; set; } = 0;

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
        /// 是否下单
        /// </summary>
        [JsonIgnore]
        public bool IsCreateOrder { get; set; } = false;

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 未拜访天数
        /// </summary>
        public int? NoVisitedDays { get; set; } = 0;

        public List<DisplayPhoto> DisplayPhotos { get; set; }
        public List<DoorheadPhoto> DoorheadPhotos { get; set; }

        public double? Distance { get; set; }
        public bool Abnormal { get; set; }
    }

    /// <summary>
    /// 业务员外勤轨迹
    /// </summary>
    public partial class BusinessVisitModel : BaseModel
    {
        public BusinessVisitModel()
        {
            Items = new List<BusinessVisitListModel>();
        }

        public IList<BusinessVisitListModel> Items { get; set; }

        #region 用于条件检索

        [HintDisplayName("签到日期", "签到日期")]
        public DateTime SigninDateTime { get; set; }

        #endregion
    }


    public class VisitOnlineModel : BaseModel
    {
        public SelectList BusinessUsers { get; set; }
        public List<LineVisitRecord> Lines { get; set; } = new List<LineVisitRecord>();
    }
    public class LineVisitRecord
    {
        public int UserId { get; set; } = 0;
        public string UserName { get; set; }
        public int LineId { get; set; } = 0;
        public string LineName { get; set; }
        public List<VisitStoreModel> VisitRecords { get; set; } = new List<VisitStoreModel>();
    }


    /// <summary>
    /// 业务员拜访列表
    /// </summary>
    public partial class BusinessVisitListModel
    {
        public int StoreId { get; set; } = 0;
        public int BusinessUserId { get; set; } = 0;
        public string BusinessUserName { get; set; }
        /// <summary>
        /// 拜访数
        /// </summary>
        public int? VisitedCount { get; set; } = 0;
        /// <summary>
        /// 未拜访数
        /// </summary>
        public int? NoVisitedCount { get; set; } = 0;
        /// <summary>
        /// 异常拜访数
        /// </summary>
        public int? ExceptVisitCount { get; set; } = 0;

        /// <summary>
        /// 拜访记录
        /// </summary>
        public List<VisitStoreModel> VisitRecords { get; set; } = new List<VisitStoreModel>();


        /// <summary>
        /// 跟踪记录
        /// </summary>
        public List<QueryVisitStoreAndTracking> RealTimeTracks { get; set; } = new List<QueryVisitStoreAndTracking>();




    }

    /// <summary>
    /// 业务员拜访排行
    /// </summary>
    public partial class BusinessVisitRankModel
    {
        public int StoreId { get; set; } = 0;
        public int BusinessUserId { get; set; } = 0;
        public string BusinessUserName { get; set; }
        /// <summary>
        /// 拜访数
        /// </summary>
        public int? VisitedCount { get; set; } = 0;

        /// <summary>
        /// 客户数
        /// </summary>
        public int? CustomerCount { get; set; } = 0;

        /// <summary>
        /// 未拜访数
        /// </summary>
        public int? NoVisitedCount { get; set; } = 0;

        /// <summary>
        /// 异常拜访数
        /// </summary>
        public int? ExceptVisitCount { get; set; } = 0;


    }

    /// <summary>
    /// 用于轨迹实时上报
    /// </summary>
    public partial class TrackingModel : BaseEntityModel
    {

        /// <summary>
        /// 业务员
        /// </summary>
        public int BusinessUserId { get; set; } = 0;

        /// <summary>
        /// 业务员
        /// </summary>
        public string BusinessUserName { get; set; }

        /// <summary>
        /// 纬度坐标
        /// </summary>
        public double Latitude { get; set; } = 0;

        /// <summary>
        /// 经度坐标
        /// </summary>
        public double Longitude { get; set; } = 0;

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
}
