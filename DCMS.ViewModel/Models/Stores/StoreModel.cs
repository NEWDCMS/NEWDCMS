using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;


namespace DCMS.ViewModel.Models.Stores
{
    //public partial class StoreModel : BaseEntityModel
    //{

    //    [HintDisplayName("识别码", "识别码")]
    //    public string Code { get; set; }

    //    [HintDisplayName("经销商名称", "经销商名称")]
    //    public string Name { get; set; }


    //    [HintDisplayName("URL", "经销商URL")]
    //    public string Url { get; set; }


    //    [HintDisplayName("是否启用SSL", "是否启用SSL")]
    //    public bool SslEnabled { get; set; }

    //    [HintDisplayName("安全URL", "安全URL(HTTPS)")]
    //    public string SecureUrl { get; set; }


    //    [HintDisplayName("HOST列表", "逗号分隔的HTTP_HOST列表")]
    //    public string Hosts { get; set; }

    //    [HintDisplayName("排序", "排序")]
    //    public int DisplayOrder { get; set; } = 0;

    //    [HintDisplayName("创建时间", "创建时间")]
    //    public DateTime CreatedOnUtc { get; set; }


    //    [HintDisplayName("是否启用", "是否启用")]
    //    public bool Published { get; set; }
    //}


    public partial class StoreModel : BaseEntityModel
    {
        /// <summary>
        /// 所属区域
        /// </summary>
        public string Target { get; set; }
        [HintDisplayName("关键字", "搜索关键字")]
        public string Key { get; set; }
        public int RowIndex { get; set; } = 0;

        [HintDisplayName("所属区域", "所属区域")]
        public int? BranchId { get; set; } = 0;
        public SelectList Branches { get; set; }


        [HintDisplayName("公司名称", "公司名称")]
        public string CompanyName { get; set; }

        [HintDisplayName("公司地址", "公司地址")]
        public string CompanyAddress { get; set; }

        [HintDisplayName("联系电话", "联系电话")]
        public string CompanyPhoneNumber { get; set; }


        [HintDisplayName("识别码", "识别码")]
        public string Code { get; set; }

        [HintDisplayName("经销商名称", "经销商名称")]
        public string Name { get; set; }


        [HintDisplayName("URL", "经销商URL")]
        public string Url { get; set; }


        [HintDisplayName("是否启用SSL", "是否启用SSL")]
        public bool SslEnabled { get; set; }

        [HintDisplayName("安全URL", "安全URL(HTTPS)")]
        public string SecureUrl { get; set; }


        [HintDisplayName("HOST列表", "逗号分隔的HTTP_HOST列表")]
        public string Hosts { get; set; }

        [HintDisplayName("排序", "排序")]
        public int DisplayOrder { get; set; }

        [HintDisplayName("创建时间", "创建时间")]
        public DateTime CreatedOnUtc { get; set; }


        [HintDisplayName("是否启用", "是否启用")]
        public bool Published { get; set; }

        [HintDisplayName("分数", "分数")]
        public int StarRate { get; set; }

        public string DealerNumber { get; set; }
        public string MarketingCenter { get; set; }
        public string SalesArea { get; set; }
        public string BusinessDepartment { get; set; }
    }




}