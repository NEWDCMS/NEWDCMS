using DCMS.ViewModel.Models.Stores;
using DCMS.ViewModel.Models.Users;
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;


namespace DCMS.ViewModel.Models.Products
{

    public partial class ManufacturerListModel : BaseModel
    {
        public ManufacturerListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
        }

        public string Target { get; set; }
        public int RowIndex { get; set; } = 0;

        [HintDisplayName("关键字", "搜索关键字")]
        public string Key { get; set; }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<ManufacturerModel> Items { get; set; }
    }
    //public class ManufacturerModel : BaseEntityModel
    //{
    //    public ManufacturerModel()
    //    {
    //        Status = true;
    //    }
    //    public int StoreId { get; set; } = 0;

    //    /// <summary>
    //    /// 提供商名
    //    /// </summary>
    //    public string Name { get; set; }

    //    /// <summary>
    //    /// 描述
    //    /// </summary>
    //    public string Description { get; set; }
    //    /// <summary>
    //    /// 助记码
    //    /// </summary>
    //    public string MnemonicName { get; set; }
    //    /// <summary>
    //    /// 联系人
    //    /// </summary>
    //    public string ContactName { get; set; }
    //    /// <summary>
    //    /// 联系人电话
    //    /// </summary>
    //    public string ContactPhone { get; set; }
    //    /// <summary>
    //    /// 地址
    //    /// </summary>
    //    public string Address { get; set; }
    //    /// <summary>
    //    /// 状态
    //    /// </summary>
    //    public bool Status { get; set; }
    //    /// <summary>
    //    /// 是否删除
    //    /// </summary>
    //    public bool Deleted { get; set; }
    //    /// <summary>
    //    /// 价格范围
    //    /// </summary>
    //    public string PriceRanges { get; set; }


    //    /// <summary>
    //    /// 排序
    //    /// </summary>
    //    public int DisplayOrder { get; set; } = 0;

    //    /// <summary>
    //    /// 插件时间
    //    /// </summary>
    //    public DateTime CreatedOnUtc { get; set; }

    //    /// <summary>
    //    /// 更新时间
    //    /// </summary>
    //    public DateTime UpdatedOnUtc { get; set; }

    //    /// <summary>
    //    /// 经销商 欠 供应商 总欠款
    //    /// </summary>
    //    public decimal OweCashTotal { get; set; }

    //}



    public partial class ManufacturerModel : BaseEntityModel
    {

        [HintDisplayName("名称", "名称")]
        public string Name { get; set; }

        [HintDisplayName("描述", "描述")]
        public string Description { get; set; }

        [HintDisplayName("助记名", "助记名")]
        public string MnemonicName { get; set; }

        [HintDisplayName("联系人", "联系人")]
        public string ContactName { get; set; }

        [HintDisplayName("联系电话", "联系电话")]
        public string ContactPhone { get; set; }

        [HintDisplayName("地址", "地址")]
        public string Address { get; set; }

        [HintDisplayName("价格范围", "价格范围")]
        public string PriceRanges { get; set; }

        [HintDisplayName("是否发布", "是否发布")]
        public bool Published { get; set; }

        [HintDisplayName("是否删除", "是否删除")]
        public bool Deleted { get; set; }

        [HintDisplayName("排序", "排序")]
        public int DisplayOrder { get; set; }

        [HintDisplayName("状态", "状态")]
        public bool Status { get; set; }

        [HintDisplayName("访问控制", "访问控制")]
        public bool SubjectToAcl { get; set; }
        [HintDisplayName("允许访问的用户角色", "")]
        public List<UserRoleModel> AvailableUserRoles { get; set; }
        public int[] SelectedCustomerRoleIds { get; set; }

        //库存映射
        [HintDisplayName("限制库存", "是否限制库存")]
        public bool LimitedToStores { get; set; }
        [HintDisplayName("有效库存", "有效库存")]
        public List<StoreModel> AvailableStores { get; set; }
        public int[] SelectedStoreIds { get; set; }

        public decimal OweCashTotal { get; set; }
        public DateTime CreatedOnUtc { get; set; }

        public DateTime UpdatedOnUtc { get; set; }
    }

    #region 

    public partial class ManufacturerProductModel : BaseEntityModel
    {
        public int ManufacturerId { get; set; }

        public int ProductId { get; set; }

        [HintDisplayName("商品名称", "")]
        public string ProductName { get; set; }

        [HintDisplayName("是否特色商品", "是否特色商品")]
        public bool IsFeaturedProduct { get; set; }

        [HintDisplayName("排序", "排序")]
        public int DisplayOrder1 { get; set; }
    }

    public partial class AddManufacturerProductModel : BaseModel
    {
        public AddManufacturerProductModel()
        {
            AvailableCategories = new List<SelectListItem>();
            AvailableManufacturers = new List<SelectListItem>();
            AvailableStores = new List<SelectListItem>();
            AvailableVendors = new List<SelectListItem>();
            AvailableProductTypes = new List<SelectListItem>();
        }

        [HintDisplayName("搜索商品名称", "搜索商品名称")]
        public string SearchProductName { get; set; }
        [HintDisplayName("搜索商品类别", "搜索商品类别")]
        public int SearchCategoryId { get; set; }
        [HintDisplayName("搜索商品提供商", "搜索商品提供商")]
        public int SearchManufacturerId { get; set; }
        [HintDisplayName("搜索经销商", "搜索经销商")]
        public int SearchStoreId { get; set; }
        [HintDisplayName("搜索供货商", "搜索供货商")]
        public int SearchVendorId { get; set; }
        [HintDisplayName("搜索商品类型", "搜索商品类型")]
        public int SearchProductTypeId { get; set; }

        public IList<SelectListItem> AvailableCategories { get; set; }
        public IList<SelectListItem> AvailableManufacturers { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }
        public IList<SelectListItem> AvailableVendors { get; set; }
        public IList<SelectListItem> AvailableProductTypes { get; set; }

        public int ManufacturerId { get; set; }

        public int[] SelectedProductIds { get; set; }
    }

    #endregion


}