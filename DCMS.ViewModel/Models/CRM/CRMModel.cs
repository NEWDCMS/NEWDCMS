using DCMS.Web.Framework.Models;
using System;


namespace DCMS.ViewModel.Models.CRM
{

    /// <summary>
    /// 关系结构
    /// </summary>
    public class CRM_RELATIONModel : BaseModel
    {
        #region DCMS

        public int TerminalId { get; set; } = 0;
        public int StoreId { get; set; } = 0;
        public DateTime CreatedOnUtc { get; set; }

        #endregion

        #region CRM

        /// <summary>
        /// 业务伙伴编号
        /// </summary>
        public string PARTNER1 { get; set; }
        /// <summary>
        /// 业务伙伴编号
        /// </summary>
        public string PARTNER2 { get; set; }
        /// <summary>
        /// 关系类别
        /// </summary>
        public string RELTYP { get; set; }
        /// <summary>
        /// 关系类别
        /// </summary>
        public string ZUPDMODE { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime ZDATE { get; set; }

        #endregion
    }

    /// <summary>
    /// 区域返回信息结构
    /// </summary>
    public class CRM_RETURNModel : BaseModel
    {

        //业务伙伴编号
        public string PARTNER1 { get; set; }

        //地址编号 业务伙伴编号
        public string PARTNER2 { get; set; }

        //银行账户号码 员工编号
        public string ZUSER { get; set; }

        //账户分配状态 增量——类型
        public string ZTYPE { get; set; }

        //UUID 子类型
        public string ZTYPE_1 { get; set; }

        //创建时间
        public string ZDATE { get; set; }

        //营销中心编码
        public string ZBAIOS { get; set; }
    }

    /// <summary>
    /// 组织层级关系
    /// </summary>
    public class CRM_ORGModel : BaseModel
    {

        /// <summary>
        /// 组织标识
        /// </summary>
        public string ORG_ID { get; set; }

        /// <summary>
        /// 对象缩写
        /// </summary>
        public string ORG_SHORT { get; set; }

        /// <summary>
        /// 对象名称
        /// </summary>
        public string ORG_TEXT { get; set; }

        /// <summary>
        /// 对象内部编号
        /// </summary>
        public string ORG_BP { get; set; }

        /// <summary>
        /// 上级组织标识
        /// </summary>
        public string ORG_UP { get; set; }

        /// <summary>
        /// 上级组织缩写
        /// </summary>
        public string ORG_UP_SHORT { get; set; }

        /// <summary>
        /// 上级组织名称
        /// </summary>
        public string ORG_UP_TEXT { get; set; }

        /// <summary>
        /// 上级组织内部编号
        /// </summary>
        public string ORG_UP_BP { get; set; }

        /// <summary>
        /// 组织层级编码
        /// </summary>
        public string ORG_FUN { get; set; }

        /// <summary>
        /// 修改日期
        /// </summary>
        public string ZSNCHDAT { get; set; }
    }

    /// <summary>
    /// 业务员级组织岗位信息
    /// </summary>
    public class CRM_BPModel : BaseModel
    {

        /// <summary>
        /// 业务员编号
        /// </summary>
        public string ZUSER_BP { get; set; }

        /// <summary>
        /// 业务员名称
        /// </summary>
        public string ZUSER_NAME_TXT { get; set; }

        /// <summary>
        /// 岗位编码
        /// </summary>
        public string ZPOSITION { get; set; }

        /// <summary>
        /// 岗位名称
        /// </summary>
        public string ZPOSITION_TXT { get; set; }

        /// <summary>
        /// 总部编码
        /// </summary>
        public string ZORG1 { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string ZORG1_TXT { get; set; }

        /// <summary>
        /// 区域编码
        /// </summary>
        public string ZORG2 { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string ZORG2_TXT { get; set; }

        /// <summary>
        /// 营销中心编码
        /// </summary>
        public string ZORG3 { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string ZORG3_TXT { get; set; }

        /// <summary>
        /// 虚拟销售分公司编码
        /// </summary>
        public string ZORG4 { get; set; }

        /// <summary>
        /// 虚拟销售分公司编码
        /// </summary>
        public string ZORG4_TXT { get; set; }

        /// <summary>
        /// 大区编码
        /// </summary>
        public string ZORG5 { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string ZORG5_TXT { get; set; }

        /// <summary>
        /// 业务部编码
        /// </summary>
        public string ZORG6 { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string ZORG6_TXT { get; set; }

        /// <summary>
        /// 工作站编码
        /// </summary>
        public string ZORG7 { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string ZORG7_TXT { get; set; }

        /// <summary>
        /// 最小组织编码
        /// </summary>
        public string MINI_ORG { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string MINI_TXT { get; set; }

        /// <summary>
        /// 领导岗位
        /// </summary>
        public string LEADPOS { get; set; }

        /// <summary>
        /// 有效起始日期
        /// </summary>
        public string DATE_FROM { get; set; }

        /// <summary>
        /// 有效截止日期
        /// </summary>
        public string DATE_TO { get; set; }

        /// <summary>
        /// 业务伙伴分组
        /// </summary>
        public string BU_GROUP { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string GROUP_TXT { get; set; }
    }

    /// <summary>
    /// 终端三级类型配置
    /// </summary>
    public class CRM_ZSNTM0040Model : BaseModel
    {

        /// <summary>
        /// 终端所属业务线
        /// </summary>
        public string ZTYPE0_NUM { get; set; }

        /// <summary>
        /// 业务线名称
        /// </summary>
        public string ZTYPE0_NAME { get; set; }

        /// <summary>
        /// 终端一级类型编码
        /// </summary>
        public string ZTYPE1_NUM { get; set; }

        /// <summary>
        /// 终端一级类型名称
        /// </summary>
        public string ZTYPE1_NAME { get; set; }

        /// <summary>
        /// 终端二级类型编码
        /// </summary>
        public string ZTYPE2_NUM { get; set; }

        /// <summary>
        /// 终端二级类型名称
        /// </summary>
        public string ZTYPE2_NAME { get; set; }

        /// <summary>
        /// 终端三级类型编码
        /// </summary>
        public string ZTYPE3_NUM { get; set; }

        /// <summary>
        /// 终端三级类型名称
        /// </summary>
        public string ZTYPE3_NAME { get; set; }

        /// <summary>
        /// 营销中心编码
        /// </summary>
        public string ZAREA_NUM { get; set; }

        /// <summary>
        /// 营销中心名称
        /// </summary>
        public string ZAREA_NAME { get; set; }
    }

    /// <summary>
    /// 终端制高点分类分级配置
    /// </summary>
    public class CRM_HEIGHT_CONFModel : BaseModel
    {

        /// <summary>
        /// 营销中心编码
        /// </summary>
        public string SALEORG { get; set; }

        /// <summary>
        /// 制高点分类编码
        /// </summary>
        public string HEIGHT { get; set; }

        /// <summary>
        /// 制高点分级编码
        /// </summary>
        public string ZZGDFJ { get; set; }

        /// <summary>
        /// 分类描述
        /// </summary>
        public string TEXT { get; set; }

        /// <summary>
        /// 分级描述
        /// </summary>
        public string ZZGDFJMS { get; set; }
    }

    /// <summary>
    /// 终端业态配置表
    /// </summary>
    public class CRM_BUSTATModel : BaseModel
    {

        /// <summary>
        /// 终端所属业务线
        /// </summary>
        public string ZZSTORE_TYPE1 { get; set; }

        /// <summary>
        /// 业态代码
        /// </summary>
        public string ZBN_TYPE { get; set; }

        /// <summary>
        /// 业态描述
        /// </summary>
        public string ZBN_TYPE_TXT { get; set; }
    }
}
