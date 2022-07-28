using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DCMS.ViewModel.Models.Users
{
    public partial class UserRoleListModel : BaseEntityModel
    {
        public UserRoleListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
        }

        [HintDisplayName("角色名", "角色名")]

        public string Name { get; set; }

        [HintDisplayName("启用", "启用")]
        public bool Active { get; set; }

        [HintDisplayName("是否系统角色", "是否系统角色")]
        public bool IsSystemRole { get; set; }

        [HintDisplayName("系统名", "系统名")]
        public string SystemName { get; set; }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<UserRoleModel> UserRoleItems { get; set; }


        public SelectList Stores { get; set; }
    }


    public partial class UserRoleModel : BaseEntityModel
    {

        [HintDisplayName("角色名", "角色名")]

        public string Name { get; set; }

        [HintDisplayName("启用", "是否启用")]
        public bool Active { get; set; }

        [HintDisplayName("是否系统角色", "是否系统角色")]
        public bool IsSystemRole { get; set; }

        [HintDisplayName("系统名", "系统名必须是英文单词组合,如：管理员(Administrator),经销商管理员(MAdministrator)")]
        public string SystemName { get; set; }

        [HintDisplayName("允许使用电脑端", "允许使用电脑端")]
        public bool UseACLPc { get; set; }

        [HintDisplayName("允许使用手机端", "允许使用手机端")]
        public bool UseACLMobile { get; set; }

        public string StoreName { get; set; }
        [XmlIgnore]
        public SelectList Stores { get; set; }

        [HintDisplayName("描述", "描述")]
        public string Description { get; set; }


    }

    public partial class UserRoleQuery
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SystemName { get; set; }
    }

}