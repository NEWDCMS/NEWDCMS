using DCMS.Core.Domain.Terminals;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace DCMS.Core.Domain.Campaigns
{

    /// <summary>
    /// 用于表示促销活动
    /// </summary>
    public class Campaign : BaseEntity
    {

        private ICollection<CampaignChannel> _campaignChannels;
        private ICollection<CampaignBuyProduct> _buyProducts;
        private ICollection<CampaignGiveProduct> _giveProducts;



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
        /// 销售备注
        /// </summary>
        public string SaleRemark { get; set; }

        /// <summary>
        /// 单据编号
        /// </summary>
        public string BillNumber { get; set; }

        /// <summary>
        /// 制单人
        /// </summary>
        public int MakeUserId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// TPM协议编码（PZC00000000000）
        /// </summary>
        public string ProtocolNum { get; set; }

        /// <summary>
        /// 是否有效
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool Enabled { get; set; }


        public virtual ICollection<CampaignBuyProduct> BuyProducts
        {
            get { return _buyProducts ?? (_buyProducts = new List<CampaignBuyProduct>()); }
            protected set { _buyProducts = value; }
        }

        public virtual ICollection<CampaignGiveProduct> GiveProducts
        {
            get { return _giveProducts ?? (_giveProducts = new List<CampaignGiveProduct>()); }
            protected set { _giveProducts = value; }
        }

        public virtual ICollection<CampaignChannel> CampaignChannels
        {
            get { return _campaignChannels ?? (_campaignChannels = new List<CampaignChannel>()); }
            protected set { _campaignChannels = value; }
        }


        #region 方法



        public void SetCampaignChannels(List<CampaignChannel> ccs)
        {
            _campaignChannels = ccs;
        }


        public void RemoveAddCampaignChannels(CampaignChannel cc)
        {
            CampaignChannels.Remove(cc);
            _campaignChannels = null;
        }


        //referencing loop detected for property 'Campaign' with type 'DCMS.Core.Domain.Campaigns.Campaign'. Path 

        #endregion

    }

    /// <summary>
    /// 用于表示购买商品
    /// </summary>
    public class CampaignBuyProduct : BaseEntity
    {
        /// <summary>
        /// 活动
        /// </summary>
        public int CampaignId { get; set; }

        /// <summary>
        /// 商品
        /// </summary>
        public int ProductId { get; set; }


        /// <summary>
        /// 单位Id
        /// </summary>
        public int? UnitId { get; set; } = 0;

        /// <summary>
        ///数量
        /// </summary>
        public int Quantity { get; set; }


        public decimal? Price { get; set; } = 0;


        [JsonIgnore]
        public virtual Campaign Campaign { get; set; }

    }

    /// <summary>
    /// 用于表示赠送商品
    /// </summary>
    public class CampaignGiveProduct : BaseEntity
    {
        /// <summary>
        /// 活动
        /// </summary>
        public int CampaignId { get; set; }

        /// <summary>
        /// 商品
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// 单位Id
        /// </summary>
        public int? UnitId { get; set; } = 0;

        /// <summary>
        ///数量
        /// </summary>
        public int Quantity { get; set; }

        public decimal? Price { get; set; } = 0;

        [JsonIgnore]
        public virtual Campaign Campaign { get; set; }
    }

    /// <summary>
    /// 用于表示促销渠道
    /// </summary>
    public class CampaignChannel : BaseEntity
    {
        public int CampaignId { get; set; }
        public int ChannelId { get; set; }



        /// <summary>
        /// 活动
        /// </summary>
        [JsonIgnore]
        public virtual Campaign Campaign { get; set; }
        /// <summary>
        /// 渠道
        /// </summary>
        public virtual Channel Channel { get; set; }
    }


    public class CampaignUpdate : BaseEntity
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

        public List<CampaignBuyProduct> CampaignBuyProducts { get; set; }
        public List<CampaignGiveProduct> CampaignGiveProducts { get; set; }


    }

}
