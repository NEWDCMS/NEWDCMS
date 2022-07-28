using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DCMS.Core.Domain.Census
{
    /// <summary>
    /// 餐饮普查卡信息
    /// </summary>
    public class Restaurant : BaseEntity
    {

        private ICollection<SalesProduct> _salesInfos;
        private ICollection<DoorheadPhoto> _doorheadPhotos;
        private ICollection<DisplayPhoto> _displayPhotos;


        /// <summary>
        /// 用户标识
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// 纬度坐标
        /// </summary>
        public double? Latitude { get; set; }

        /// <summary>
        /// 经度坐标
        /// </summary>
        public double? Longitude { get; set; }

        /// <summary>
        /// 地图标注位置
        /// </summary>
        public string Location { get; set; }

        public DateTime UpdateDate { get; set; }
        /// <summary>
        /// 基本信息
        /// </summary>
        public virtual RestaurantBaseInfo BaseInfo { get; set; }

        /// <summary>
        /// 营业信息
        /// </summary>
        public virtual RestaurantBusinessInfo BusinessInfo { get; set; }


        #region  导航属性
        /// <summary>
        /// 销售信息
        /// </summary>
        public virtual ICollection<SalesProduct> SalesInfos
        {
            get { return _salesInfos ?? (_salesInfos = new List<SalesProduct>()); }
            protected set { _salesInfos = value; }
        }

        /// <summary>
        /// 门头照片
        /// </summary>
        public virtual ICollection<DoorheadPhoto> DoorheadPhotos
        {
            get { return _doorheadPhotos ?? (_doorheadPhotos = new List<DoorheadPhoto>()); }
            set { _doorheadPhotos = value; }
        }

        /// <summary>
        /// 陈列照片
        /// </summary>
        public virtual ICollection<DisplayPhoto> DisplayPhotos
        {
            get { return _displayPhotos ?? (_displayPhotos = new List<DisplayPhoto>()); }
            set { _displayPhotos = value; }
        }

        #endregion
    }

    /// <summary>
    /// 基本信息
    /// </summary>
    public class RestaurantBaseInfo : BaseEntity
    {


        /// <summary>
        /// 终端编号
        /// </summary>
        public string EndPointNumber { get; set; }
        /// <summary>
        /// 大区
        /// </summary>
        public string SaleRegion { get; set; }
        /// <summary>
        /// 业务部
        /// </summary>
        public string SalesDepartment { get; set; }
        /// <summary>
        /// 城市
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// 区/县
        /// </summary>
        public string DistrictOrCounty { get; set; }
        /// <summary>
        /// 城区/乡镇
        /// </summary>
        public string CityOrTown { get; set; }
        /// <summary>
        /// 终端店名
        /// </summary>
        public string EndPointStorsName { get; set; }
        /// <summary>
        /// 营业电话
        /// </summary>
        public string EndPointTelphone { get; set; }

        /// <summary>
        /// 终端地址(（详细街道/门牌号）)
        /// </summary>
        public string EndPointAddress { get; set; }

        public DateTime UpdateDate { get; set; }

        #region  导航属性

        [Key, ForeignKey("Restaurant")]
        public int RestaurantId { get; set; }
        public virtual Restaurant Restaurant { get; set; }

        #endregion
    }

    /// <summary>
    /// 营业信息
    /// </summary>
    public class RestaurantBusinessInfo : BaseEntity
    {
        /// <summary>
        /// 终端类型(S超高档餐饮、A高档餐饮、B中档餐饮、C大众餐饮、D低档餐饮)
        /// </summary>
        public string EndPointType { get; set; }

        /// <summary>
        /// 是否连锁
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool IsChain { get; set; }

        /// <summary>
        /// 包厢数(终端中的包厢个数)
        /// </summary>
        public int PrivateRoomes { get; set; }

        /// <summary>
        /// 桌台数
        /// </summary>
        public int TableNumber { get; set; }

        /// <summary>
        /// 座位数
        /// </summary>
        public int Seats { get; set; }

        /// <summary>
        /// 经营特色(炒菜/火锅/烧烤/小吃（面馆）快餐/西餐（含日韩料理/自助餐/其它)
        /// </summary>
        public string Characteristics { get; set; }

        /// <summary>
        /// 是否协议店
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool IsAgreement { get; set; }

        /// <summary>
        /// 合作方式(专营保量、专营不保量、混营保量、混营不保量)
        /// </summary>
        public string ModeOfCooperation { get; set; }

        /// <summary>
        /// 终端状态(专销、强势混销、弱势混销、空白)
        /// </summary>
        public string EndPointStates { get; set; }

        public string PerConsumptions { get; set; }

        /// <summary>
        /// 负责人姓名
        /// </summary>
        public string ChargePerson { get; set; }

        /// <summary>
        /// 电话(终端负责人的电话)
        /// </summary>
        public string Telphone { get; set; }

        /// <summary>
        /// 终端关键人1姓名
        /// </summary>
        public string TopContacts { get; set; }

        /// <summary>
        /// 电话
        /// </summary>
        public string TopContactPhone { get; set; }

        /// <summary>
        /// 职位
        /// </summary>
        public string Position { get; set; }

        /// <summary>
        /// 本品冰展柜投入数量
        /// </summary>
        public int ShowcaseNum { get; set; }

        public DateTime UpdateDate { get; set; }

        #region  导航属性

        [Key, ForeignKey("Restaurant")]
        public int RestaurantId { get; set; }
        public virtual Restaurant Restaurant { get; set; }

        #endregion

    }

}
