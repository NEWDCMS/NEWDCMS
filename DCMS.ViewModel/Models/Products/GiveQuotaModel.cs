using DCMS.Core;
using DCMS.ViewModel.Models.Users;
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using System;
using System.Collections.Generic;

namespace DCMS.ViewModel.Models.Products
{
    public partial class GiveQuotaListModel : BaseModel
    {
        public GiveQuotaListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Managers = new List<UserModel>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<GiveQuotaModel> Items { get; set; }
        public IList<UserModel> Managers { get; set; }
        public int? UserId { get; set; } = 0;
        public int? Year { get; set; } = 0;
        public int? GiveQuotaId { get; set; } = 0;

        public string Remark { get; set; }

    }

    public partial class GiveQuotaModel : BaseEntityModel
    {
        public IList<GiveQuotaOptionModel> Items { get; set; }
        //移除 = 0;
        public int? UserId { get; set; } = 0;
        public int? Year { get; set; } = 0;
        public string Remark { get; set; }

    }

    public partial class GiveQuotaOptionModel : BaseEntityModel
    {
        public int GiveQuotaId { get; set; } = 0;




        [HintDisplayName("商品编号", "商品编号")]
        public string ProductSKU { get; set; }

        [HintDisplayName("商品名称", "商品名称")]
        public int ProductId { get; set; } = 0;
        public string ProductName { get; set; }

        [HintDisplayName("条形码", "条形码")]
        public string BarCode { get; set; }

        [HintDisplayName("单位换算", "单位换算")]
        public string UnitConversion { get; set; }

        [HintDisplayName("单位", "单位")]
        public string UnitName { get; set; }
        public int? UnitId { get; set; } = 0;
        public Dictionary<string, int> Units { get; set; }

        public string SmallUnitName { get; set; }

        public decimal? Jan { get; set; } = 0;
        public decimal? Feb { get; set; } = 0;
        public decimal? Mar { get; set; } = 0;
        public decimal? Apr { get; set; } = 0;
        public decimal? May { get; set; } = 0;
        public decimal? Jun { get; set; } = 0;
        public decimal? Jul { get; set; } = 0;
        public decimal? Aug { get; set; } = 0;
        public decimal? Sep { get; set; } = 0;
        public decimal? Oct { get; set; } = 0;
        public decimal? Nov { get; set; } = 0;
        public decimal? Dec { get; set; } = 0;


        /// <summary>
        /// 各月余额
        /// </summary>
        public decimal? Jan_Balance { get; set; } = 0;
        public decimal? Feb_Balance { get; set; } = 0;
        public decimal? Mar_Balance { get; set; } = 0;
        public decimal? Apr_Balance { get; set; } = 0;
        public decimal? May_Balance { get; set; } = 0;
        public decimal? Jun_Balance { get; set; } = 0;
        public decimal? Jul_Balance { get; set; } = 0;
        public decimal? Aug_Balance { get; set; } = 0;
        public decimal? Sep_Balance { get; set; } = 0;
        public decimal? Oct_Balance { get; set; } = 0;
        public decimal? Nov_Balance { get; set; } = 0;
        public decimal? Dec_Balance { get; set; } = 0;


        [HintDisplayName("总计", "总计")]
        public decimal? Total { get; set; } = 0;

        [HintDisplayName("备注", "备注")]
        public string Remark { get; set; }

    }

    /// <summary>
    /// 用于表示赠送记录
    /// </summary>
    public class GiveQuotaRecordsModel : BaseEntityModel
    {



        /// <summary>
        /// 单据来源（销售/订）
        /// </summary>
        public int? BillId { get; set; }

        /// <summary>
        /// 单据来源（促销）
        /// </summary>
        public int? CampaignId { get; set; }

        /// <summary>
        /// 单据来源（合同）
        /// </summary>
        public int? ContractId { get; set; }


        public int BusinessUserId { get; set; }
        public string BusinessUserName { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        public int TerminalId { get; set; }
        public string TerminalName { get; set; }
        public string TerminalCode { get; set; }

        /// <summary>
        /// 商品类别
        /// </summary>
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; }


        /// <summary>
        /// 成本计算方式
        /// </summary>
        public int? CostingCalCulateMethodId { get; set; } = 0;


        /// <summary>
        /// 商品名称
        /// </summary>
        public int ProductId { get; set; }
        public string ProductName { get; set; }

        /// <summary>
        /// 条形码
        /// </summary>
        public string BarCode { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        public int? UnitId { get; set; }
        public string UnitName { get; set; }

        /// <summary>
        /// 单位换算
        /// </summary>
        public string UnitConversion { get; set; }

        /// <summary>
        /// 赠品类型
        /// </summary>
        public int GiveTypeId { get; set; }

        public GiveTypeEnum GiveType
        {
            get { return (GiveTypeEnum)GiveTypeId; }
            set { GiveTypeId = (int)value; }
        }

        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity { get; set; } = 0;

        /// <summary>
        /// 成本
        /// </summary>
        public decimal CostAmount { get; set; } = 0;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }


        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }


        /// <summary>
        /// 兑付年份
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// 兑付月份
        /// </summary>
        public int Monthly { get; set; }

    }

    /// <summary>
    /// 项目保存或者编辑
    /// </summary>
    public class GiveQuotaUpdateModel : BaseEntityModel
    {
        //移除 = 0;
        public int? UserId { get; set; } = 0;
        public int? Year { get; set; } = 0;
        public string Remark { get; set; }

        /// <summary>
        /// 项目
        /// </summary>
        public List<GiveQuotaOptionModel> Items { get; set; }

    }



}