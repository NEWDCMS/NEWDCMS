using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;


namespace DCMS.ViewModel.Models.Users
{

    public class BranchModel : BaseEntityModel, IParentList
    {

        [JsonIgnore]
        public SelectList StoreList { get; set; }

        [HintDisplayName("", "")]
        [JsonProperty("storeName")]
        public string StoreName { get; set; }

        /// <summary>
        /// 部门编码
        /// </summary>
        [HintDisplayName("部门编码", "部门编码")]
        [JsonProperty("deptCode")]
        public int DeptCode { get; set; } = 0;

        /// <summary>
        /// 部门名称
        /// </summary>
        [JsonProperty("name")]
        [HintDisplayName("部门名称", "部门名称")]
        public string DeptName { get; set; }

        [JsonIgnore]
        /// <summary>
        /// 简述
        /// </summary>
        [HintDisplayName("简述", "简述")]
        public int DeptShort { get; set; } = 0;

        /// <summary>
        /// 描述
        /// </summary>
        [JsonProperty("desc")]
        [HintDisplayName("描述", "描述")]
        public string DeptDesc { get; set; }

        /// <summary>
        /// 所属父级
        /// </summary>
        [JsonProperty("parentId")]
        [HintDisplayName("所属父级", "所属父级")]
        public int ParentId { get; set; } = 0;
        [JsonIgnore]
        [HintDisplayName("所属父级", "所属父级")]
        public string ParentName { get; set; }
        [JsonIgnore]
        public SelectList ParentList { get; set; }

        /// <summary>
        /// 级别深度树
        /// </summary>
        [JsonProperty("path")]
        [HintDisplayName("级别", "级别深度树")]
        public string TreePath { get; set; }

        [JsonIgnore]
        /// <summary>
        /// 直接上级领导
        /// </summary>
        [HintDisplayName("直接上级领导", "直接上级领导")]
        public string BranchLeader { get; set; }

        [JsonIgnore]
        /// <summary>
        /// 创建时间
        /// </summary>
        [HintDisplayName("创建时间", "创建时间")]
        public DateTime? CreateDateTime { get; set; }


        [JsonProperty("order")]
        [HintDisplayName("排序", "排序")]
        public int? DisplayOrder { get; set; } = 0;


        [JsonProperty("status")]
        [HintDisplayName("状态", "状态")]
        public bool Status { get; set; }
    }
}