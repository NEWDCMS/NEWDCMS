using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace DCMS.ViewModel.Models.Terminals
{
    public partial class ChannelModel : BaseEntityModel
    {

        /// <summary>
        /// 序号
        /// </summary>
        public int? OrderNo { get; set; } = 0;
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Describe { get; set; }
        /// <summary>
        /// 预设属性(枚举1 特殊通道 2 餐饮 3 小超 4 大超 5 其他)
        /// </summary>
        public byte Attribute { get; set; }
        /// <summary>
        /// 是否删除
        /// </summary>
        public bool Deleted { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// 预设属性名称
        /// </summary>
        public string AttributeName { get; set; }
        /// <summary>
        /// 预设属性集合
        /// </summary>
        public SelectList Attributes { get; set; }
    }

    public class ChannelListModel : BaseModel
    {
        public int ChannelId { get; set; }
        public SelectList Items { get; set; }
    }
}