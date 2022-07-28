using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;



namespace DCMS.ViewModel.Models.Users
{
    public class UserGroupListModel : BaseEntityModel
    {
        public UserGroupListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<UserGroupModel> UserGroupItems { get; set; }

        /// <summary>
        /// 组名
        /// </summary>
        [HintDisplayName("关键字", "关键字")]
        public string GroupName { get; set; }


        public SelectList Stores { get; set; }

    }

}