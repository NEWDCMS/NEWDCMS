using DCMS.Core.Domain.Common;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using DCMS.Core;
using DCMS.Web.Framework.UI.Paging;


namespace DCMS.Web.Framework.Models
{
    /// <summary>
    /// 视图基类模型
    /// </summary>
    public partial class BaseModel
    {
        public virtual void BindModel(ModelBindingContext bindingContext)
        {
        }
        protected virtual void PostInitialize()
        {
        }
    }

    public abstract class BasePageableModel : BaseModel, IPageableModel
    {
        #region Methods

        public virtual void LoadPagedList<T>(IPagedList<T> pagedList)
        {
            FirstItem = (pagedList.PageIndex * pagedList.PageSize) + 1;
            HasNextPage = pagedList.HasNextPage;
            HasPreviousPage = pagedList.HasPreviousPage;
            LastItem = Math.Min(pagedList.TotalCount, ((pagedList.PageIndex * pagedList.PageSize) + pagedList.PageSize));
            PageNumber = pagedList.PageIndex + 1;
            PageSize = pagedList.PageSize;
            TotalItems = pagedList.TotalCount;
            TotalPages = pagedList.TotalPages;
        }

        #endregion

        #region Properties

        public int PageIndex
        {
            get
            {
                if (PageNumber > 0)
                    return PageNumber - 1;

                return 0;
            }
        }

    
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int FirstItem { get; set; }
        public int LastItem { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }

        #endregion
    }

    public abstract class BaseAccountModel : BaseEntityModel
    {

        public int AccountingTypeId { get; set; }
      
        public string AccountingOptionName { get; set; }


        /// <summary>
        /// 会计科目
        /// </summary>
        public int AccountingOptionId { get; set; }
        /// <summary>
        /// 单据Id
        /// </summary>
        public int BillId { get; set; }
        /// <summary>
        /// 收款金额
        /// </summary>
        public decimal CollectionAmount { get; set; }

    }


    /// <summary>
    /// 实体基类模型
    /// </summary>
    public partial class BaseEntityModel : BaseModel
    {
        [JsonProperty("Id")]
        public virtual int Id { get; set; }


        [HintDisplayName("经销商", "经销商")]
        [JsonProperty("storeId")]
        public virtual int StoreId { get; set; }

    }

    /// <summary>
    /// 表示余额（经销商、客户，业务员）
    /// </summary>
    public partial class BaseBalance : BaseEntityModel
    {
        /// <summary>
        /// 当前交易预付/预收（金额）
        /// </summary>
        public decimal AdvanceAmount { get; set; }
        /// <summary>
        /// 余额（预付/预收）
        /// </summary>
        public decimal AdvanceAmountBalance { get; set; } 
    }


    /// <summary>
    /// 查询公共类
    /// </summary>
    public partial class SaleReportQueryModel : BaseModel
    {

        [HintDisplayName("商品", "商品")]
        public int? ProductId { get; set; } = 0;
        public string ProductName { get; set; }

        [HintDisplayName("商品类别", "商品类别")]
        public int? CategoryId { get; set; } = 0;
        public string CategoryName { get; set; }
        public SelectList Categories { get; set; }

        [HintDisplayName("客户Id", "客户Id")]
        public int? TerminalId { get; set; } = 0;
        [HintDisplayName("客户", "客户")]
        public string TerminalName { get; set; }

        [HintDisplayName("单据编号", "单据编号")]
        public string BillNumber { get; set; }

        [HintDisplayName("业务员", "业务员")]
        public int? BusinessUserId { get; set; } = 0;
        public SelectList BusinessUsers { get; set; }


        [HintDisplayName("成本计算方式", "成本计算方式")]
        public int CostingCalCulateMethodId { get; set; } = 0;
        public CostingCalCulateMethod CostingCalCulateMethod { get; set; }


        [HintDisplayName("仓库", "仓库")]
        public int? WareHouseId { get; set; } = 0;
        public SelectList WareHouses { get; set; }

        [HintDisplayName("送货员", "送货员")]
        public int? DeliveryUserId { get; set; } = 0;
        public SelectList DeliveryUsers { get; set; }

        [HintDisplayName("客户等级", "客户等级")]
        public int? RankId { get; set; } = 0;
        public SelectList Ranks { get; set; }

        [HintDisplayName("客户渠道", "客户渠道")]
        public int? ChannelId { get; set; } = 0;
        public SelectList Channels { get; set; }

        [HintDisplayName("客户片区", "客户片区")]
        public int? DistrictId { get; set; } = 0;
        public SelectList Districts { get; set; }

        [HintDisplayName("品牌", "品牌")]
        public int? BrandId { get; set; } = 0;
        public string BrandName { get; set; }
        public SelectList Brands { get; set; }

        [HintDisplayName("支付方式", "支付方式")]
        public int? PayTypeId { get; set; } = 0;
        public SelectList PayTypes { get; set; }

        [DisplayName("开始时间")]
        [UIHint("DateTime")]
        public DateTime StartTime { get; set; }

        [DisplayName("结束时间")]
        [UIHint("DateTime")]
        public DateTime EndTime { get; set; }

        [HintDisplayName("备注", "备注")]
        public string Remark { get; set; }

        [HintDisplayName("状态(审核)", "状态(审核)")]
        public bool? AuditedStatus { get; set; }
    }

}