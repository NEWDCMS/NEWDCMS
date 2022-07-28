using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using System.Collections.Generic;

namespace DCMS.ViewModel.Models.Configuration
{


    public partial class RemarkConfigListModel : BaseEntityModel
    {

        public RemarkConfigListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Lists = new List<RemarkConfigModel>();
        }
        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<RemarkConfigModel> Lists { get; set; }
    }


    // [Validator(typeof(RemarkConfigValidator))]
    public partial class RemarkConfigModel : BaseEntityModel
    {


        [HintDisplayName("备注名称", "备注名称")]
        public string Name { get; set; }

        [HintDisplayName("参与价格记忆", "参与价格记忆")]
        public bool RemberPrice { get; set; }
    }
}
