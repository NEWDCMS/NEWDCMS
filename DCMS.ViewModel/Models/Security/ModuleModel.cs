using DCMS.Core.Domain.Common;
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;
using System.Xml.Serialization;

namespace DCMS.ViewModel.Models.Users
{

    public partial class ModuleListModel : BaseEntityModel
    {
        public string Name { get; set; }
        public SelectList Stores { get; set; }
    }


    public class BaseModule : BaseEntityModel
    {

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("parentId")]
        public int? ParentId { get; set; } = 0;

        [JsonProperty("linkUrl")]
        public string LinkUrl { get; set; }
    }

    public class ModuleModel : BaseModule
    {

        [HintDisplayName("上级模块", "上级模块名称")]

        public string ParentName { get; set; }
        [XmlIgnore]
        public SelectList ParentList { get; set; }

        [HintDisplayName("菜单图标", "用于菜单显示的图标")]

        [JsonProperty("icon")]
        public string Icon { get; set; }


        [HintDisplayName("是否是菜单", "是否是菜单,用于菜单管理")]

        [JsonProperty("isMenu")]
        public bool IsMenu { get; set; }


        [HintDisplayName("描述", "模块描述信息")]

        [JsonProperty("description")]
        public string Description { get; set; }


        [HintDisplayName("是否启用", "是否启用菜单")]

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }



        [HintDisplayName("是否系统模块", "是否系统模块")]

        [JsonProperty("issystem")]
        public bool IsSystem { get; set; }


        [HintDisplayName("是否在手机端显示", "是否在手机端显示")]

        [JsonProperty("showmobile")]
        public bool ShowMobile { get; set; }


        [HintDisplayName("是否管理平台模块", "是否管理平台模块")]

        [JsonProperty("ispaltform")]
        public bool IsPaltform { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [JsonProperty("createdOnUtc")]
        public DateTime? CreatedOnUtc { get; set; }


        [HintDisplayName("排序", "排序")]
        [JsonProperty("displayOrder")]
        public int? DisplayOrder { get; set; } = 0;



        [HintDisplayName("布局", "菜单布局")]
        public int LayoutPositionId { get; set; }

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


        [HintDisplayName("控制器", "控制器")]
        [JsonProperty("controller")]
        public string Controller { get; set; }


        [HintDisplayName("方法", "方法")]
        [JsonProperty("action")]
        public string Action { get; set; }


        public bool Selected { get; set; }
    }



}