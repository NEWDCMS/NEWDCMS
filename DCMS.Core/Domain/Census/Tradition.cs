using System;
using System.Collections.Generic;

namespace DCMS.Core.Domain.Census
{
    /// <summary>
    /// 传统普查卡信息
    /// </summary>
    public class Tradition : BaseEntity
    {

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




        #region  导航属性


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

}
