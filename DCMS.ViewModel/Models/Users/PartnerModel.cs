using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;


namespace DCMS.ViewModel.Models.Users
{

    public partial class PartnerListModel : BaseModel
    {

        public PartnerListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Lists = new List<PartnerModel>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<PartnerModel> Lists { get; set; }


        [HintDisplayName("用户名", "用户名")]
        public string UserName { get; set; }

        [HintDisplayName("邮箱", "邮箱")]
        public string Email { get; set; }

        [HintDisplayName("密码", "密码")]
        public string Password { get; set; }


    }


    /// <summary>
    /// (合作者账户)管理表
    /// </summary>
    public class PartnerModel : BaseEntityModel
    {

        public PartnerModel()
        {
        }

        [HintDisplayName("用户唯一码", "用户唯一码")]
        public string UserGuid { get; set; }

        [HintDisplayName("账户名", "账户名")]
        public string UserName { get; set; }

        [HintDisplayName("邮箱", "邮箱")]
        public string Email { get; set; }

        [HintDisplayName("密码", "密码")]
        public string Password { get; set; }

        [HintDisplayName("密码格式ID", "密码格式ID")]
        public int PasswordFormatId { get; set; }

        public IList<SelectListItem> PasswordFormats { get; set; }


        [HintDisplayName("PasswordSalt", "PasswordSalt")]
        public string PasswordSalt { get; set; }

        [HintDisplayName("激活状态", "激活状态")]
        public bool Active { get; set; }

        [HintDisplayName("系统用户", "系统用户")]
        public bool IsSystemAccount { get; set; }

        [HintDisplayName("上次IP", "上次IP")]
        public string LastIpAddress { get; set; }

        [HintDisplayName("创建时间", "创建时间")]
        public System.DateTime CreatedOnUtc { get; set; }

        [HintDisplayName("AccessKey", "AccessKey")]
        public string AccessKey { get; set; }


    }


}
