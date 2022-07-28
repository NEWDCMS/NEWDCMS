using System;
using System.ComponentModel.DataAnnotations.Schema;


namespace DCMS.Core.Domain.Stores
{
    /*
    /// <summary>
    /// 表示一个网点(我们的经销商)
    /// </summary>
    public partial class Store : BaseEntity
    {
        /// <summary>
        /// 所属区域
        /// </summary>
        public int? BranchId { get; set; } = 0;

        /// <summary>
        /// 识别码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 经销商名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 经销商URL
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 是否启用SSL
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool SslEnabled { get; set; }

        /// <summary>
        /// 安全URL (HTTPS)
        /// </summary>
        public string SecureUrl { get; set; }

        /// <summary>
        /// 逗号分隔的HTTP_HOST列表 
        /// </summary>
        public string Hosts { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool Published { get; set; }


        /// <summary>
        /// 星级别
        /// </summary>
        public int StarRate { get; set; } = 0;


        /// <summary>
        /// ERP经销商编号
        /// </summary>
        public string DealerNumber { get; set; }

        /// <summary>
        /// ERP营销中心
        /// </summary>
        public string MarketingCenter { get; set; }
        public string MarketingCenterCode { get; set; }

        /// <summary>
        /// ERP销售大区
        /// </summary>
        public string SalesArea { get; set; }
        public string SalesAreaCode { get; set; }

        /// <summary>
        /// ERP业务部
        /// </summary>
        public string BusinessDepartment { get; set; }
        public string BusinessDepartmentCode { get; set; }

        /// <summary>
        /// 是否激活（安装初始向导户标志为：1）
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool Actived { get; set; }

        [Column(TypeName = "BIT(1)")]
        public bool Setuped { get; set; }
    }
    */


    /// <summary>
    /// 渠道商（经销商）基本信息结构
    /// </summary>
    //[Table("CRM_Stores")]
    public class Store : BaseEntity
    {

        /// <summary>
        /// 区域编码
        /// </summary>
        public string QuyuCode { get; set; } = "";

        #region DCMS

        /// <summary>
        /// 所属区域
        /// </summary>
        public int BranchId { get; set; } = 0;

        /// <summary>
        /// 识别码
        /// </summary>
        public string Code { get; set; } = "";

        /// <summary>
        /// 经销商名称
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// 经销商URL
        /// </summary>
        public string Url { get; set; } = "";

        /// <summary>
        /// 是否启用SSL
        /// </summary>
        //[Column("SslEnabled", "BIT(1)")]
        public bool SslEnabled { get; set; }

        /// <summary>
        /// 安全URL (HTTPS)
        /// </summary>
        public string SecureUrl { get; set; } = "";

        /// <summary>
        /// 逗号分隔的HTTP_HOST列表 
        /// </summary>
        public string Hosts { get; set; } = "";

        /// <summary>
        /// 排序
        /// </summary>
        public int DisplayOrder { get; set; } = 0;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        //[Column("Published", "BIT(1)")]
        public bool Published { get; set; }


        /// <summary>
        /// 星级别
        /// </summary>
        public int StarRate { get; set; } = 0;


        /// <summary>
        /// ERP经销商编号
        /// </summary>
        public string DealerNumber { get; set; } = "";

        /// <summary>
        /// ERP营销中心
        /// </summary>
        public string MarketingCenter { get; set; } = "";
        public string MarketingCenterCode { get; set; } = "";

        /// <summary>
        /// ERP销售大区
        /// </summary>
        public string SalesArea { get; set; } = "";
        public string SalesAreaCode { get; set; } = "";

        /// <summary>
        /// ERP业务部
        /// </summary>
        public string BusinessDepartment { get; set; } = "";
        public string BusinessDepartmentCode { get; set; } = "";

        /// <summary>
        /// 是否激活（安装初始向导户标志为：1）
        /// </summary>
        public bool Actived { get; set; }

        /// <summary>
        /// 是否已经初始化
        /// </summary>
        public bool Setuped { get; set; }

        #endregion

        #region CRM

        /// <summary>
        /// 业务伙伴编号
        /// </summary>
        public string PARTNER { get; set; }

        /// <summary>
        /// 区域个性化系统编号
        /// </summary>
        public string ZZFLD0000CF { get; set; }

        ///// <summary>
        ///// 组织名称
        ///// </summary>
        //public string NAME_ORG1 { get; set; }

        /// <summary>
        /// 中心归档标志
        /// </summary>
        public string XDELE { get; set; }

        /// <summary>
        /// 联系人
        /// </summary>
        public string ZZPERSON { get; set; }

        /// <summary>
        /// 联系人电话
        /// </summary>
        public string ZZTELPHONE { get; set; }

        /// <summary>
        /// 地区
        /// </summary>
        public string REGION { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        public string ZZCITY { get; set; }

        /// <summary>
        /// 区县
        /// </summary>
        public string ZZCOUNTY { get; set; }

        /// <summary>
        /// 街道
        /// </summary>
        public string ZZSTREET_NUM { get; set; }

        /// <summary>
        /// 村
        /// </summary>
        public string ZZLILLAGE_NUM { get; set; }

        /// <summary>
        /// 详细地址
        /// </summary>
        public string ZZADD_DETAIL { get; set; }

        /// <summary>
        /// 客户大类
        /// </summary>
        public string ZZQUDAO_TYPE { get; set; }

        /// <summary>
        /// 经销商类型
        /// </summary>
        public string ZZCLIENT_TYPE { get; set; }

        /// <summary>
        /// 终端合作性质
        /// </summary>
        public string ZZFLD00005V { get; set; }

        /// <summary>
        /// 经销模式
        /// </summary>
        public string ZZDILIVER_MODEL { get; set; }

        /// <summary>
        /// 企业性质
        /// </summary>
        public string ZZRUN_CHARA { get; set; }

        /// <summary>
        /// 身份证号
        /// </summary>
        public string ZZCARDID { get; set; }

        /// <summary>
        /// 统一社会信用代码
        /// </summary>
        public string ZZBUSINESS { get; set; }

        /// <summary>
        /// 税务登记证编号
        /// </summary>
        public string ZZTAX { get; set; }

        /// <summary>
        /// 酒类专卖批发许可证
        /// </summary>
        public string ZZALCOHOL { get; set; }

        /// <summary>
        /// 业务人员数量
        /// </summary>
        public string ZZMANAGEMENT { get; set; }

        /// <summary>
        /// 专职送货工人数量
        /// </summary>
        public string ZZFULLTIME { get; set; }

        /// <summary>
        /// 固定仓储面积（平米）
        /// </summary>
        public string ZZSTORAGE { get; set; }

        /// <summary>
        /// 仓库数量
        /// </summary>
        public string ZZWAREHOUSE1 { get; set; }

        /// <summary>
        /// 机动车数量
        /// </summary>
        public string ZZCAR { get; set; }

        /// <summary>
        /// 非机动车数量
        /// </summary>
        public string ZZNONCAR { get; set; }

        /// <summary>
        /// 是否当地重点快消品一批商
        /// </summary>
        public string ZZFLD0000CH { get; set; }

        /// <summary>
        /// 代理品牌明细
        /// </summary>
        public string ZZWILLINGNESS { get; set; }

        /// <summary>
        /// 社会背景
        /// </summary>
        public string ZZBACK_GROUND { get; set; }

        /// <summary>
        /// 社会背景
        /// </summary>
        public string ZZBACKGROUND { get; set; }

        /// <summary>
        /// 合作评价
        /// </summary>
        public string ZZCOMMENT { get; set; }

        /// <summary>
        /// 大区
        /// </summary>
        public string ZZOFFICE_ID { get; set; }

        /// <summary>
        /// 办事处
        /// </summary>
        public string ZZGROUP_ID { get; set; }

        /// <summary>
        /// 工作站
        /// </summary>
        public string ZZGZZ_ID { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public string ZDATE { get; set; }

        #endregion
    }
}
