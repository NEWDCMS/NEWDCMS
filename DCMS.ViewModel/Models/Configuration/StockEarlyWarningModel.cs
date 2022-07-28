using DCMS.ViewModel.Models.Products;
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;



namespace DCMS.ViewModel.Models.Configuration
{

    public partial class StockEarlyWarningListModel : BaseModel
    {
        public StockEarlyWarningListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<StockEarlyWarningModel> Items { get; set; }


        public string Key { get; set; }

        [HintDisplayName("仓库", "仓库")]
        public int? WareHouseId { get; set; } = 0;
        public string WareHouseName { get; set; }
        public SelectList WareHouses { get; set; }
    }


    //[Validator(typeof(StockEarlyWarningValidator))]
    public class StockEarlyWarningModel : ProductBaseModel
    {




        [HintDisplayName("仓库", "仓库")]
        public int WareHouseId { get; set; } = 0;
        public string WareHouseName { get; set; }
        public SelectList WareHouses { get; set; }

        public SelectList UnitLists { get; set; }


        [HintDisplayName("辅助单位", "辅助单位")]
        public string AuxiliaryUnit { get; set; }


        [HintDisplayName("缺货预警数", "缺货预警数")]
        public int ShortageWarningQuantity { get; set; } = 0;


        [HintDisplayName("积压预警数", "积压预警数")]
        public int BacklogWarningQuantity { get; set; } = 0;

        [HintDisplayName("创建时间", "创建时间")]
        public DateTime CreatedOnUtc { get; set; }
    }
}