using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace DCMS.ViewModel.Models.Configuration
{
    /// <summary>
    /// Represents a setting list model
    /// </summary>
    public partial class SettingListModel : BasePagedListModel<SettingModel>
    {
        public SelectList Stores { get; set; }
        public string Keys { get; set; }

        [HintDisplayName("高级配置类型", "高级配置类型")]
        public int? SettingTypeId { get; set; } = 0;
        public string SettingTypeName { get; set; }
        public SelectList SettingTypes { get; set; }
    }


    //public partial class SettingListModel : BaseModel
    //{
    //    public SettingListModel()
    //    {
    //        PagingFilteringContext = new PagingFilteringModel();
    //        Lists = new List<SettingModel>();
    //    }

    //    public PagingFilteringModel PagingFilteringContext { get; set; }
    //    public IList<SettingModel> Lists { get; set; }
    //    public SelectList Stores { get; set; }

    //    public string Keys { get; set; }

    //    [HintDisplayName("高级配置类型", "高级配置类型")]
    //    public int? SettingTypeId { get; set; } = 0;
    //    public string SettingTypeName { get; set; }
    //    public SelectList SettingTypes { get; set; }

    //}

}