using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using System;
using System.Collections.Generic;



namespace DCMS.ViewModel.Models.Products
{

    public partial class RecentPriceListModel : BaseModel
    {
        public RecentPriceListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
        }


        [HintDisplayName("客户名称", "客户名称")]
        public int? TerminalId { get; set; } = 0;
        public string TerminalName { get; set; }


        [HintDisplayName("商品名称", "商品名称")]
        public int? ProductId { get; set; } = 0;
        public string ProductName { get; set; }


        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<RecentPriceModel> Items { get; set; }
    }


    public partial class RecentPriceModel : BaseEntityModel
    {

        public string StoreName { get; set; }


        [HintDisplayName("客户名称", "客户名称")]
        public int CustomerId { get; set; } = 0;
        public string CustomerName { get; set; }


        [HintDisplayName("商品名称", "商品名称")]
        public int ProductId { get; set; } = 0;
        public string ProductName { get; set; }


        [HintDisplayName("小单位价格", "小单位价格")]
        public decimal? SmallUnitPrice { get; set; } = 0;


        [HintDisplayName("中单位价格", "中单位价格")]
        public decimal? StrokeUnitPrice { get; set; } = 0;


        [HintDisplayName("大单位价格", "大单位价格")]
        public decimal? BigUnitPrice { get; set; } = 0;


        [HintDisplayName("修改时间", "修改时间")]
        public DateTime UpdateTime { get; set; }

    }


    public class RecentPriceUpdateModel : BaseEntityModel
    {
        /// <summary>
        /// 项目
        /// </summary>
        public List<RecentPriceModel> Items { get; set; }
    }


}