using DCMS.Core;
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;


namespace DCMS.ViewModel.Models
{
    public interface IParentList
    {
        SelectList ParentList { get; set; }
    }


    public abstract class BaseBillModel : BaseEntityModel
    {
        /// <summary>
        /// 单据枚举
        /// </summary>
        public int BillTypeEnumId { get; set; }

        [HintDisplayName("单据编号", "单据编号")]
        public string BillNumber { get; set; }

        [HintDisplayName("渠道", "渠道")]
        public int ChannelId { get; set; } = 0;
        /// <summary>
        /// 启用税务功能
        /// </summary>
        public bool EnableTaxRate { get; set; } = false;
        /// <summary>
        /// 税率
        /// </summary>
        public double TaxRate { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 制单人
        /// </summary>
        public int MakeUserId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        public BillTypeEnum BillType
        {
            get { return (BillTypeEnum)BillTypeEnumId; }
            set { BillTypeEnumId = (int)value; }
        }

    }

}
