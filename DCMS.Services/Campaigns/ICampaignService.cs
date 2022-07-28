using DCMS.Core;
using DCMS.Core.Domain.Campaigns;
using System;
using System.Collections.Generic;

namespace DCMS.Services.Campaigns
{
    public interface ICampaignService
    {
        void DeleteCampaign(Campaign campaign);
        void DeleteCampaignBuyProduct(CampaignBuyProduct campaignBuyProduct);
        void DeleteCampaignChannel(CampaignChannel productCampaign);
        void DeleteCampaignGiveProduct(CampaignGiveProduct campaignGiveProduct);
        IList<Campaign> GetAllCampaigns();
        IPagedList<Campaign> GetAllCampaigns(int? store, string name = "", string billNumber = "", string remark = "", int channelId = 0, DateTime? start = null, DateTime? end = null, bool showExpire = false, bool? enabled = null, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

        IPagedList<Campaign> GetAvailableCampaigns(string key, int storeId, int channelId, int pageIndex, int pageSize);
        IList<CampaignBuyProduct> GetCampaignBuyByCampaignId(int campaignId);
        IList<CampaignGiveProduct> GetCampaignGiveByCampaignId(int campaignId);

        CampaignBuyProduct GetCampaignBuyProductById(int campaignBuyProductId);
        IList<CampaignBuyProduct> GetCampaignBuyProductsByCampaignId(int campaignId, int? userId, int? storeId, int pageIndex, int pageSize);
        Campaign GetCampaignById(int? storeId, int campaignId);
        CampaignChannel GetCampaignChannelById(int productCampaignId);
        IPagedList<CampaignChannel> GetCampaignChannelsByCampaignId(int campaignId, int? userId, int? storeId, int pageIndex, int pageSize);
        IList<CampaignChannel> GetCampaignChannelsByCampaignId(int? store, int campaignId);
        IList<CampaignGiveProduct> GetCampaignGiveProductByCampaignId(int campaignId, int? userId, int? storeId, int pageIndex, int pageSize);
        CampaignGiveProduct GetCampaignGiveProductById(int campaignGiveProductId);
        void InsertCampaign(Campaign campaign);
        void InsertCampaignBuyProduct(CampaignBuyProduct campaignBuyProduct);
        void InsertCampaignChannel(CampaignChannel productCampaign);
        void InsertCampaignGiveProduct(CampaignGiveProduct campaignGiveProduct);
        void UpdateCampaign(Campaign campaign);
        void UpdateCampaignBuyProduct(CampaignBuyProduct campaignBuyProduct);
        void UpdateCampaignChannel(CampaignChannel productCampaign);
        void UpdateCampaignGiveProduct(CampaignGiveProduct campaignGiveProduct);

        IList<CampaignBuyProduct> GetCampaignBuyProducts(int? storeId, int? channelId, int pageIndex, int pageSize);
        IList<CampaignGiveProduct> GetCampaignGiveProducts(int? storeId, int? channelId, int pageIndex, int pageSize);

        /// <summary>
        /// 获取可用 赠品额度信息
        /// </summary>
        /// <param name="storeId">经销商Id</param>
        /// <param name="channelId">渠道Id</param>
        /// <returns></returns>
        IList<Campaign> GetAvailableCampaigns(int storeId, int channelId);

        IList<Campaign> GetCampaignsByIds(int[] ids);

        IList<CampaignBuyProduct> GetCampaignBuyByIds(int[] ids);
        IList<CampaignGiveProduct> GetCampaignGiveByIds(int[] ids);

        IList<CampaignBuyProduct> GetCampaignBuyByCampaignIds(int[] ids);
        IList<CampaignGiveProduct> GetCampaignGiveByCampaignIds(int[] ids);

        Tuple<List<CampaignBuyProduct>, List<CampaignGiveProduct>> GetAvailableCampaigns(string key, int storeId, int channelId, int pageIndex, int pageSize, out int totalCount);

        BaseResult BillCreateOrUpdate(int storeId, int userId, int? campaignId, Campaign campaign, CampaignUpdate data, List<CampaignBuyProduct> buyItems, List<CampaignGiveProduct> giveItems, bool isAdmin = false);


    }
}