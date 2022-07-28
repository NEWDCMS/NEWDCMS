using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using System;
using System.Collections.Generic;


namespace DCMS.ViewModel.Models.Products
{
    public partial class StatisticalTypeListModel : BaseModel
    {
        public StatisticalTypeListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
        }

        public string Name { get; set; }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<StatisticalTypeModel> Items { get; set; }
    }


    //[Validator(typeof(StatisticalTypeValidator))]
    public partial class StatisticalTypeModel : BaseEntityModel
    {



        [HintDisplayName("键名", "键名")]
        public string Name { get; set; }


        [HintDisplayName("键值", "键值")]
        public string Value { get; set; }

        /// <summary>
        /// 创建时间
        [HintDisplayName("创建时间", "创建时间")]
        public DateTime CreatedOnUtc { get; set; }

    }
}