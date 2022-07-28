using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DCMS.ViewModel.Models.Stores
{

    public partial class StoreListModel : BaseModel
    {
        public StoreListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
        }

        [DisplayName("搜索名称")]
        public string SearchName { get; set; }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<StoreModel> StoreItems { get; set; }

    }

    public class SotreInitDataModel : BaseEntityModel
    {
        [HintDisplayName("初始管理员权限员工", "初始管理员权限员工")]
        public int AdminUserId { get; set; } = 0;
        [HintDisplayName("初始管理员权限员工", "初始管理员权限员工")]
        public string AdminUserName { get; set; }
        public SelectList AdminUsers { get; set; }
    }

    public class SotreInitUserModel : BaseEntityModel
    {
        [HintDisplayName("用户名", "用户名")]
        public string UserName { get; set; }

        [HintDisplayName("电话", "电话")]
        public string UserPhone { get; set; }

        [HintDisplayName("邮箱", "邮箱")]
        public string UserEmail { get; set; }

        [DataType(DataType.Password)]
        [HintDisplayName("密码", "密码")]
        public string Password { get; set; }
    }
}


