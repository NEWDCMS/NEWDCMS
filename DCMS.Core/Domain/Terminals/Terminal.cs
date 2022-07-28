using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DCMS.Core.Domain.Terminals
{

    /// <summary>
    /// 终端信息结构
    /// </summary>
    //[Table("CRM_Terminals")]
    public class Terminal : BaseEntity
    {

        /// <summary>
        /// 区域编码
        /// </summary>
        public string QuyuCode { get; set; } = "";

        #region DCMS

        /// <summary>
        /// 终端名称
        /// </summary>
        public string Name { get; set; } = "";
        /// <summary>
        /// 助记名
        /// </summary>
        public string MnemonicName { get; set; } = "";
        /// <summary>
        /// 老板姓名
        /// </summary>
        public string BossName { get; set; } = "";
        /// <summary>
        /// 老板电话
        /// </summary>
        public string BossCall { get; set; } = "";
        /// <summary>
        /// 状态
        /// </summary>
        //[Column("Related", "BIT(1)")]
        public bool Status { get; set; }
        /// <summary>
        /// 最大欠款额度
        /// </summary>
        public decimal MaxAmountOwed { get; set; } = 0;
        /// <summary>
        /// 终端编码
        /// </summary>
        public string Code { get; set; } = "";
        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; } = "";
        /// <summary>
        /// 备注
        /// </summary>
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
        public int LineId { get; set; } = 0;
        /// <summary>
        /// 客户等级
        /// </summary>
        public int RankId { get; set; } = 0;
        /// <summary>
        /// 付款方式
        /// </summary>
        public int PaymentMethod { get; set; } = 0;
        /// <summary>
        /// 经度
        /// </summary>
        public double Location_Lng { get; set; } = 0;
        /// <summary>
        /// 纬度
        /// </summary>
        public double Location_Lat { get; set; } = 0;

        /// <summary>
        /// 营业编号
        /// </summary>
        public string BusinessNo { get; set; } = "";
        /// <summary>
        /// 食品经营许可证号
        /// </summary>
        public string FoodBusinessLicenseNo { get; set; } = "";
        /// <summary>
        /// 企业注册号
        /// </summary>
        public string EnterpriseRegNo { get; set; } = "";
        /// <summary>
        /// 是否删除
        /// </summary>
        //[Column("Related", "BIT(1)")]
        public bool Deleted { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public int CreatedUserId { get; set; } = 0;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// 数据类型 注意：这里 包含终端，供应商，选择终端弹出时使用。
        /// Mapping 时忽略此字段，Ignore(c => c.DataTypeId);
        /// </summary>
        public int DataTypeId { get; set; } = 0;

        /// <summary>
        /// 门头照片
        /// </summary>
        public string DoorwayPhoto { get; set; } = "";

        /// <summary>
        /// 是否关联业务
        /// </summary>
        //[Column("Related", "BIT(1)")]
        public bool Related { get; set; }



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
        /// 是否新增
        /// </summary>
        public bool IsNewAdd { get; set; }
        #endregion


        #endregion

        #region CRM
        /// <summary>
        /// 业务伙伴编号
        /// </summary>
        public string PARTNER { get; set; } = "";
        /// <summary>
        /// 终端状态
        /// </summary>
        public string ZZSTATUS1 { get; set; } = "";
        /// <summary>
        /// 终端类别
        /// </summary>
        public string ZZVIRTUAL { get; set; } = "";
        ///// <summary>
        ///// 搜索帮助字段
        ///// </summary>
        //public string MC_NAME1 { get; set; }
        /// <summary>
        /// 电话号码: 拨区号 + 号码
        /// </summary>
        public string TEL_NUMBER { get; set; } = "";
        /// <summary>
        /// 地区
        /// </summary>
        public string REGION { get; set; } = "";
        /// <summary>
        /// 城市
        /// </summary>
        public string ZZCITY { get; set; } = "";
        /// <summary>
        /// 区县
        /// </summary>
        public string ZZCOUNTY { get; set; } = "";
        /// <summary>
        /// 街道
        /// </summary>
        public string ZZSTREET_NUM { get; set; } = "";
        ///// <summary>
        ///// 街道2
        ///// </summary>
        //public string STR_SUPPL1 { get; set; }
        /// <summary>
        /// 城市邮政编码
        /// </summary>
        public string POST_CODE1 { get; set; } = "";
        ///// <summary>
        ///// 联系人
        ///// </summary>
        //public string ZZPERSON { get; set; }
        ///// <summary>
        ///// 联系人电话
        ///// </summary>
        //public string ZZTELPHONE { get; set; }
        /// <summary>
        /// 终端所属业务线
        /// </summary>
        public string ZZSTORE_TYPE1 { get; set; } = "";
        /// <summary>
        /// 一级终端类型
        /// </summary>
        public string ZZSTORE_TYPE2 { get; set; } = "";
        /// <summary>
        /// 二级终端类型
        /// </summary>
        public string ZZSTORE_TYPE3 { get; set; } = "";
        /// <summary>
        /// 三级终端类型
        /// </summary>
        public string ZZSTORE_TYP4 { get; set; } = "";
        /// <summary>
        /// 终端合作性质
        /// </summary>
        public string ZZFLD00005V { get; set; } = "";
        /// <summary>
        /// 制高点分类
        /// </summary>
        public string ZZGDFL { get; set; } = "";
        /// <summary>
        /// 制高点分级编码
        /// </summary>
        public string ZZGDFJ { get; set; } = "";
        /// <summary>
        /// 终端业态
        /// </summary>
        public string ZZBN_TYPE { get; set; } = "";
        /// <summary>
        /// 业务员拜访频次
        /// </summary>
        public string ZZVISIT { get; set; } = "";
        ///// <summary>
        ///// 是否协议店
        ///// </summary>
        //public string ZZPROTOCOL { get; set; }
        /// <summary>
        /// 经营起始年
        /// </summary>
        public string ZZAGE { get; set; } = "";
        /// <summary>
        /// 是否连锁店
        /// </summary>
        public string ZZWHET_CHAIN { get; set; } = "";
        /// <summary>
        /// 餐饮店内营业面积（平米）
        /// </summary>
        public string ZZINNER_AREA { get; set; } = "";
        /// <summary>
        /// 营业时间
        /// </summary>
        public string ZZOPEN_TIME { get; set; } = "";
        ///// <summary>
        ///// 赊款额（万元）
        ///// </summary>
        //public string ZZFLD00005D { get; set; }
        /// <summary>
        /// 终端地理归属
        /// </summary>
        public string ZZGEO { get; set; } = "";
        /// <summary>
        /// 终端特征
        /// </summary>
        public string ZZCHARACTER { get; set; } = "";
        /// <summary>
        /// 包厢（个）
        /// </summary>
        public string ZZBOX { get; set; } = "";
        /// <summary>
        /// 散台数
        /// </summary>
        public string ZZTABLE { get; set; } = "";


        /// <summary>
        /// 对象创建日期(2020-01-01)
        /// </summary>
        public string CRDAT { get; set; } = "";
        /// <summary>
        /// 对象创建时间(00:00:00)
        /// </summary>
        public string CRTIM { get; set; } = "";
        /// <summary>
        /// 最后更改对象的日期(2020-01-01)
        /// </summary>
        public string CHDAT { get; set; } = "";
        /// <summary>
        /// 对象最后更改的时间(00:00:00)
        /// </summary>
        public string CHTIM { get; set; } = "";


        /// <summary>
        /// 工作站主管
        /// </summary>
        public string ZZDIRECTOR { get; set; } = "";
        /// <summary>
        /// 业务员名称
        /// </summary>
        public string ZZSALESMAN { get; set; } = "";
        public string ZZOFFICE_ID { get; set; } = "";
        /// <summary>
        /// 办事处
        /// </summary>
        public string ZZGROUP_ID { get; set; } = "";
        /// <summary>
        /// 工作站
        /// </summary>
        public string ZZGZZ_ID { get; set; } = "";

        #endregion
    }


    /// <summary>
    /// 用于表示经销商账户余额
    /// </summary>
    public class TerminalBalance : BaseEntity
    {
        /// <summary>
        /// 科目Id
        /// </summary>
        public int AccountingOptionId { get; set; }

        /// <summary>
        /// 科目名称
        /// </summary>
        public string AccountingName { get; set; }

        /// <summary>
        /// 最大欠款额度
        /// </summary>
        public decimal MaxOweCashBalance { get; set; } = 0;
        /// <summary>
        /// 剩余预收款金额
        /// </summary>
        public decimal AdvanceAmountBalance { get; set; } = 0;
        /// <summary>
        /// 总欠款
        /// </summary>
        public decimal TotalOweCash { get; set; } = 0;
        /// <summary>
        /// 剩余欠款额度
        /// </summary>
        public decimal OweCashBalance { get; set; } = 0;

    }


    public class NewTerminal : BaseEntity
    {
        public int CreatedUserId { get; set; }
        public int TerminalId { get; set; }
        public int Status { get; set; }
        public DateTime CreatedOnUtc { get; set; }
    }

}
