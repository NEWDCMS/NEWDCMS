using DCMS.Core;
using DCMS.Core.Domain.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DCMS.Core.Domain.CSMS
{
    /// <summary>
    /// 产品信息
    /// </summary>
    public partial class OrderDetail : BaseEntity
    {
        [NotMapped]
        public new int StoreId { get; set; } = 0;

        public int terminalSignReportId { get; set; }

        /// <summary>
        /// 产品编码
        /// </summary>
        public string productCode { get; set; }


        /// <summary>
        /// 产品名称
        /// </summary>
        public string productName { get; set; }


        /// <summary>
        /// 产品数量
        /// </summary>
        public int productAmount { get; set; }


        /// <summary>
        /// 售卖类型 0 正常销售  1 开业赠酒  2 促销赠酒  3 试业赠酒  4 常规赠酒  5 置换用酒  6 瓶盖用酒  7 陈列赠酒  8 客情赠酒
        /// </summary>
        public int type { get; set; }


        /// <summary>
        /// 酒类别（31 成品酒 51 促销品 14 包装物）
        /// </summary>
        public int goodsCategory { get; set; }


    }
}
