using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using System;
using System.Collections.Generic;


namespace DCMS.ViewModel.Models.Configuration
{


    public partial class PricingStructureListModel : BaseEntityModel
    {

        public PricingStructureListModel()
        {

            Items1 = new List<PricingStructureModel>();
            Items2 = new List<PricingStructureModel>();
        }

        public List<PricingStructureModel> Items1 { get; set; }
        public List<PricingStructureModel> Items2 { get; set; }



        public string ChannelDatas { get; set; }
        public string LevelDatas { get; set; }
        public string TierPricePlanDatas { get; set; }

    }
    /// <summary>
    /// 表示价格体系配置
    /// </summary>
    public partial class PricingStructureModel : BaseEntityModel
    {



        [HintDisplayName("价格体系类别", "价格体系类别")]
        public int PriceType { get; set; } = 0;



        [HintDisplayName("客户", "客户")]
        public int CustomerId { get; set; } = 0;


        [HintDisplayName("客户名称", "客户名称")]
        public string CustomerName { get; set; }


        [HintDisplayName("渠道", "渠道")]
        public int ChannelId { get; set; } = 0;
        public string ChannelName { get; set; }

        [HintDisplayName("片区", "片区")]
        public string DistrictIds { get; set; }
        public string DistrictName { get; set; }



        [HintDisplayName("等级", "等级")]
        public int EndPointLevel { get; set; } = 0;
        public string EndPointLevelName { get; set; }


        [HintDisplayName("首选价格", "首选价格")]
        public string PreferredPrice { get; set; }
        public string PreferredPriceName { get; set; }


        [HintDisplayName("次选价格", "次选价格")]
        public string SecondaryPrice { get; set; }
        public string SecondaryPriceName { get; set; }

        [HintDisplayName("末选价格", "末选价格")]
        public string FinalPrice { get; set; }
        public string FinalPriceName { get; set; }

        [HintDisplayName("创建时间", "创建时间")]
        public DateTime CreatedOnUtc { get; set; }


        [HintDisplayName("权重", "权重")]
        public int Order { get; set; } = 0;
    }
}
