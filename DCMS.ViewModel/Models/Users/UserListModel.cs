using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DCMS.ViewModel.Models.Users
{
    public partial class UserListModel : BaseModel
    {
        public UserListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
        }

        [DisplayName("有效用户角色")]

        public List<UserRoleModel> AvailableUserRoles { get; set; }

        [DisplayName("搜索用户角色列表")]
        public int[] SearchUserRoleIds { get; set; }

        [DisplayName("搜索邮箱")]

        public string SearchEmail { get; set; }

        [DisplayName("搜索用户名")]

        public string SearchUsername { get; set; }
        public bool UsernamesEnabled { get; set; }

        [DisplayName("搜索用户真实姓名")]

        public string SearchRealName { get; set; }

        [DisplayName("经销商")]

        public int SearchStore { get; set; }
        public SelectList SearchStores { get; set; }
        public bool StoreEnabled { get; set; }

        [DisplayName("搜索电话")]
        public string SearchPhone { get; set; }


        [DisplayName("开始时间")]
        [UIHint("DateTime")]
        public DateTime StartTime { get; set; }

        [DisplayName("开始时间")]
        [UIHint("DateTime")]
        public DateTime EndTime { get; set; }


        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<UserModel> UserItems { get; set; }

    }

}


