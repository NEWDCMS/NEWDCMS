using DCMS.ViewModel.Models.Products;
using DCMS.ViewModel.Models.Terminals;
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace DCMS.ViewModel.Models.Campaigns
{
    /// <summary>
    /// 用于列表和搜索表单
    /// </summary>
    public partial class CampaignListModel : BaseModel
    {
        public CampaignListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<CampaignModel> Items { get; set; }

        [HintDisplayName("单据编号", "单据编号")]
        public string BillNumber { get; set; }

        [HintDisplayName("开始时间", "开始时间")]
        [UIHint("DateTime")] public DateTime StartTime { get; set; }

        [HintDisplayName("结束时间", "结束时间")]
        [UIHint("DateTime")] public DateTime EndTime { get; set; }

        [HintDisplayName("活动名称", "活动名称")]
        public string Name { get; set; }

        [HintDisplayName("备注", "备注")]
        public string Remark { get; set; }

        [HintDisplayName("客户渠道", "客户渠道")]
        public int? ChannelId { get; set; }
        public SelectList Channels { get; set; }

        [HintDisplayName("展示过期活动", "展示过期活动")]
        public bool ShowExpire { get; set; }

        [HintDisplayName("展示停用活动  ", "展示停用活动  ")]
        public bool Enabled { get; set; }
    }


    /// <summary>
    /// 用于表示促销活动
    /// </summary>
    public class CampaignModel : BaseEntityModel
    {
        public CampaignModel()
        {
            Channels = new List<ChannelModel>();
            CampaignBuyProducts = new List<CampaignBuyProductModel>();
            CampaignGiveProducts = new List<CampaignGiveProductModel>();
        }



        [HintDisplayName("活动名称", "活动名称")]
        public string Name { get; set; }

        [HintDisplayName("开始时间", "开始时间")]
        [UIHint("DateTime")] public DateTime StartTime { get; set; }

        [HintDisplayName("结束时间", "结束时间")]
        [UIHint("DateTime")] public DateTime EndTime { get; set; }

        [HintDisplayName("备注", "备注")]
        public string Remark { get; set; }

        [HintDisplayName("销售备注", "销售备注")]
        public string SaleRemark { get; set; }

        [HintDisplayName("单据编号", "单据编号")]
        public string BillNumber { get; set; }

        [HintDisplayName("制单人", "制单人")]
        public int MakeUserId { get; set; } = 0;
        public string MakeUserName { get; set; }

        [HintDisplayName("制单时间", "创建时间")]
        public string CreatedOnUtc { get; set; }

        [HintDisplayName("促销天数", "促销天数")]
        public int TotalDay { get; set; }

        [HintDisplayName("剩余天数", "剩余天数")]
        public int ValidlDay { get; set; } = 0;

        [HintDisplayName("是否有效", "是否有效")]
        public bool Enabled { get; set; }

        [HintDisplayName("渠道", "渠道")]
        public List<ChannelModel> Channels { get; set; }
        public int[] SelectedChannelIds { get; set; }

        /// <summary>
        /// TPM协议编码（PZC00000000000）
        /// </summary>
        [HintDisplayName("TPM协议编码", "TPM协议编码")]
        public string ProtocolNum { get; set; }
        public List<CampaignBuyProductModel> CampaignBuyProducts { get; set; }
        public List<CampaignGiveProductModel> CampaignGiveProducts { get; set; }

    }

    /// <summary>
    /// 用于表示购买商品
    /// </summary>
    public class CampaignBuyProductModel : ProductModel
    {
        [HintDisplayName("活动", "活动")]
        public int CampaignId { get; set; } = 0;
        public decimal Price { get; set; } = 0;
        /// <summary>
        /// 购买商品类型
        /// </summary>
        public int BuyProductTypeId { get; set; }
        public string BuyProductTypeName { get; set; }
    }

    /// <summary>
    /// 用于表示赠送商品
    /// </summary>
    public class CampaignGiveProductModel : ProductModel
    {
        [HintDisplayName("活动", "活动")]
        public int CampaignId { get; set; } = 0;
        public string CampaignName { get; set; }
        public decimal Price { get; set; } = 0;
        /// <summary>
        /// 赠送商品类型
        /// </summary>
        public int GiveProductTypeId { get; set; }
        public string GiveProductTypeName { get; set; }
    }

    /// <summary>
    /// 用于表示购买、赠送商品
    /// </summary>
    public class CampaignBuyGiveProductModel
    {
        [HintDisplayName("活动", "活动")]
        public int CampaignId { get; set; } = 0;
        public string CampaignName { get; set; }

        /// <summary>
        /// 赠品类型 关联 GiveTypeEnum 枚举
        /// </summary>
        public int? GiveTypeId { get; set; } = 0;

        #region 购买

        /// <summary>
        /// 购买信息
        /// </summary>
        public string BuyProductMessage { get; set; }

        public List<CampaignBuyProductModel> CampaignBuyProducts = new List<CampaignBuyProductModel>();

        #endregion

        #region 赠送

        /// <summary>
        /// 赠送信息
        /// </summary>
        public string GiveProductMessage { get; set; }

        public List<CampaignGiveProductModel> CampaignGiveProducts = new List<CampaignGiveProductModel>();

        #endregion

        [HintDisplayName("销售赠送关联号", "销售赠送关联号")]
        public string CampaignLinkNumber { get; set; }


        [HintDisplayName("销售商品倍数", "销售商品倍数")]
        public int SaleBuyQuantity { get; set; } = 1;

    }

    public class CampaignUpdateModel : BaseEntityModel
    {
        /// <summary>
        /// 活动名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndTime { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 展示停用活动
        /// </summary>
        public bool Enabled { get; set; }
        /// <summary>
        /// TPM协议编码
        /// </summary>
        public string ProtocolNum { get; set; }

        /// <summary>
        /// 渠道
        /// </summary>
        public int[] SelectedChannelIds { get; set; }

        public List<CampaignBuyProductModel> CampaignBuyProducts { get; set; } = new List<CampaignBuyProductModel>();
        public List<CampaignGiveProductModel> CampaignGiveProducts { get; set; } = new List<CampaignGiveProductModel>();

    }


}
