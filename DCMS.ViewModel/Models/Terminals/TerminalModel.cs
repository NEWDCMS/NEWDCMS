using DCMS.Core.Domain.Terminals;
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DCMS.ViewModel.Models.Terminals
{


    public partial class TerminalListModel : BaseModel
    {
        public TerminalListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
        }

        public string Target { get; set; }

        [HintDisplayName("关键字", "搜索关键字")]
        public string Key { get; set; }

        public List<int> ExcludeIds { get; set; }
        public int RowIndex { get; set; } = 0;
        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<TerminalModel> Items { get; set; }
        public List<District> DistrictList { get; set; }
        public IEnumerable<SelectListItem> ChannelList { get; set; }
        public IEnumerable<SelectListItem> RankList { get; set; }
        public IEnumerable<SelectListItem> LineList { get; set; }
    }


    public partial class TerminalModel : BaseEntityModel
    {
        public TerminalModel()
        {
            Status = true;
        }
        /// <summary>
        /// 客户姓名
        /// </summary>
        [DisplayName("名称")]
        public string Name { get; set; }
        /// <summary>
        /// 助记名
        /// </summary>
        [DisplayName("助记码")]
        public string MnemonicName { get; set; }
        /// <summary>
        /// 老板姓名
        /// </summary>
        [DisplayName("老板姓名")]
        public string BossName { get; set; }
        /// <summary>
        /// 老板电话
        /// </summary>
        [DisplayName("联系电话")]
        public string BossCall { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        [DisplayName("状态")]
        public bool Status { get; set; }
        /// <summary>
        /// 最大欠款额度
        /// </summary>
        [DisplayName("最大欠款额度")]
        public decimal? MaxAmountOwed { get; set; } = 0;
        /// <summary>
        /// 终端编码
        /// </summary>
        [DisplayName("终端编码")]
        public string Code { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        [DisplayName("地址")]
        public string Address { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [DisplayName("备注")]
        public string Remark { get; set; } = "";
        /// <summary>
        /// 片区Id
        /// </summary>
        public int DistrictId { get; set; } = 0;
        /// <summary>
        /// 渠道Id
        /// </summary>
        public int ChannelId { get; set; } = 0;
        /// <summary>
        /// 线路Id
        /// </summary>
        public int? LineId { get; set; } = 0;
        /// <summary>
        /// 客户等级
        /// </summary>
        public int RankId { get; set; } = 0;
        /// <summary>
        /// 付款方式
        /// </summary>
        [DisplayName("付款方式")]
        public int PaymentMethod { get; set; } = 0;
        /// <summary>
        /// 经度
        /// </summary>
        [DisplayName("经度")]
        public double? Location_Lng { get; set; } = 0;
        /// <summary>
        /// 纬度
        /// </summary>
        [DisplayName("纬度")]
        public double? Location_Lat { get; set; } = 0;

        /// <summary>
        /// 营业编号
        /// </summary>
        [DisplayName("营业编号")]
        public string BusinessNo { get; set; }
        /// <summary>
        /// 食品经营许可证号
        /// </summary>
        [DisplayName("许可证号")]
        public string FoodBusinessLicenseNo { get; set; }
        /// <summary>
        /// 企业注册号
        /// </summary>
        [DisplayName("企业注册号")]
        public string EnterpriseRegNo { get; set; } 
        /// <summary>
        /// 是否删除
        /// </summary>
        public bool Deleted { get; set; }

        /// <summary>
        /// 片区名称
        /// </summary>
        [DisplayName("片区")]
        public string DistrictName { get; set; }
        /// <summary>
        /// 渠道名称
        /// </summary>
        [DisplayName("渠道")]
        public string ChannelName { get; set; }
        /// <summary>
        /// 线路名称
        /// </summary>
        [DisplayName("线路")]
        public string LineName { get; set; }
        /// <summary>
        /// 等级名称
        /// </summary>
        [DisplayName("终端等级")]
        public string RankName { get; set; }

        public SelectList DistrictList { get; set; }
        public SelectList ChannelList { get; set; }
        public SelectList RankList { get; set; }
        public SelectList LineList { get; set; }
        public SelectList PaymentMethodType { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public int CreatedUserId { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        ///是否具有赠品相关
        /// </summary>
        public bool HasGives { get; set; }

        /// <summary>
        /// 门头照片
        /// </summary>
        public string DoorwayPhoto { get; set; } = "";


        /// <summary>
        /// 是否关联业务
        /// </summary>
        public bool Related { get; set; }

        /// <summary>
        /// 是否新增
        /// </summary>
        public bool IsNewAdd { get; set; }

        #region 协议信息

        /// <summary>
        /// 是否协议店
        /// </summary>
        public bool IsAgreement { get; set; }

        #endregion


        #region 合作信息

        /// <summary>
        /// 合作意向
        /// </summary>
        public string Cooperation { get; set; } = "";
        /// <summary>
        /// 展示是否陈列
        /// </summary>
        public bool IsDisplay { get; set; }
        /// <summary>
        /// 展示是否生动化
        /// </summary>
        public bool IsVivid { get; set; }
        /// <summary>
        /// 展示是否促销
        /// </summary>
        public bool IsPromotion { get; set; }
        /// <summary>
        /// 展示其它
        /// </summary>
        public string OtherRamark { get; set; } = "";
        /// <summary>
        /// 经销商编码
        /// </summary>
        public string StoreCode { get; set; }
        /// <summary>
        /// 经销商名称
        /// </summary>
        public string StoreName { get; set; }


        #endregion

        public double Distance { get; set; }

    }
}