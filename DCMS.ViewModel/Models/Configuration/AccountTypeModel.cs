//
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DCMS.ViewModel.Models.Configuration
{

    public partial class AccountingTypeListModel : BaseModel
    {
        public AccountingTypeListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            AccountingTypes = new List<AccountingTypeModel>();
        }
        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<AccountingTypeModel> AccountingTypes { get; set; }
        public SelectList Stores { get; set; }
        public int AccountingTypeId { get; set; } = 0;

        public string Name { get; set; }

    }


    /// <summary>
    /// 表示科目类别
    /// </summary>
    public class AccountingTypeModel : BaseEntityModel
    {

        [HintDisplayName("类别名称", "类别名称")]
        public string Name { get; set; }

        [HintDisplayName("账户余额", "账户余额")]
        public string Amount { get; set; }


        [HintDisplayName("是否启用", "是否启用")]
        public bool Enabled { get; set; }


        [HintDisplayName("排序", "排序")]
        public int? DisplayOrder { get; set; } = 0;


        /// <summary>
        /// 会计科目（AccountingEnum 枚举类型）
        /// </summary>
        [HintDisplayName("会计科目分类Id", "会计科目Id")]
        public int AccountingId { get; set; }
        [HintDisplayName("会计科目分类名称", "会计科目分类名称")]
        public string AccountingOptionName { get; set; }
        public SelectList Accountings { get; set; }
    }


    /// <summary>
    /// 会计科目弹出框查询
    /// </summary>
    public partial class AccountingOptionListModel : BaseModel
    {
        public AccountingOptionListModel()
        {
            AccountingTypes = new List<AccountingTypeModel>();
        }
        public IList<AccountingTypeModel> AccountingTypes { get; set; }
        public int AccountingTypeId { get; set; } = 0;
        public int StoreId { get; set; }
        /// <summary>
        /// 供应商Id 取预付款时用
        /// </summary>
        public int ManufacturerId { get; set; } = 0;

        /// <summary>
        /// 终端Id 取预收款时用
        /// </summary>
        public int TerminalId { get; set; } = 0;

        /// <summary>
        /// 是否多项选择
        /// </summary>
        public int Multi { get; set; }

        public int Self { get; set; }
        public int ifcheck { get; set; }
        public int selectIndex { get; set; }
        public string PageTable { get; set; }
        /// <summary>
        /// 弹出框默认科目
        /// </summary>
        public string AccountCodeTypeIds { get; set; }

    }


    public partial class AccountingOptionModel : BaseEntityModel
    {



        [HintDisplayName("科目类别", "科目类别")]
        [JsonProperty("accountingTypeId")]
        public int AccountingTypeId { get; set; } = 0;

        [JsonIgnore]
        public string AccountingTypeName { get; set; }


        [HintDisplayName("父级科目", "父级科目")]
        [JsonProperty("parentId")]
        public int? ParentId { get; set; } = 0;
        public string ParentName { get; set; }

        [JsonProperty("number")]
        public int Number { get; set; }

        [XmlIgnore]
        public SelectList PartentAccounts { get; set; }

        [HintDisplayName("科目类型", "科目类型")]
        [JsonProperty("accountCodeTypeId")]
        public int? AccountCodeTypeId { get; set; } = 0;
        public string AccountCodeTypeName { get; set; }
        [XmlIgnore]
        public SelectList AccountCodeTypes { get; set; }

        public bool IsEndPoint { get; set; }

        [HintDisplayName("科目名称", "科目名称")]
        [JsonProperty("name")]
        public string Name { get; set; }


        [HintDisplayName("科目代码", "科目代码")]
        [JsonProperty("code")]
        public string Code { get; set; }


        [HintDisplayName("排序", "排序")]
        [JsonProperty("displayOrder")]
        public int? DisplayOrder { get; set; } = 0;


        [HintDisplayName("是否启用", "是否启用")]
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }


        [HintDisplayName("是否默认账户", "是否默认账户")]
        [JsonProperty("isDefault")]
        public bool IsDefault { get; set; }

        [HintDisplayName("初始余额", "初始余额")]
        [JsonProperty("initBalance")]
        public decimal? InitBalance { get; set; } = 0;

        [JsonIgnore]
        public bool Selected { get; set; }

        [JsonProperty("isLeaf")]
        public bool? IsLeaf { get; set; }
        [JsonProperty("isCustom")]
        public bool? IsCustom { get; set; } //是否自定义会计科目

        [JsonProperty("balance")]
        public decimal Balance { get; set; }
    }


}

