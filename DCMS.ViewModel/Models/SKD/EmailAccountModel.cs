//
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;


namespace DCMS.ViewModel.Models.Tasks
{
    public partial class EmailAccountListModel : BaseEntityModel
    {
        public EmailAccountListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Lists = new List<EmailAccountModel>();
        }
        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<EmailAccountModel> Lists { get; set; }


        public SelectList Stores { get; set; }

        [HintDisplayName("用户名", "用户名")]
        public string UserName { get; set; }

    }

    //[Validator(typeof(EmailAccountValidator))]
    public partial class EmailAccountModel : BaseEntityModel
    {

        /// <summary>
        /// 邮件地址
        /// </summary>
        [HintDisplayName("邮件地址", "邮件地址")]

        public string Email { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
        [HintDisplayName("显示名称", "显示名称")]

        public string DisplayName { get; set; }

        /// <summary>
        /// 主机
        /// </summary>
        [HintDisplayName("主机", "主机")]

        public string Host { get; set; }

        /// <summary>
        /// 端口
        /// </summary>
        [HintDisplayName("端口", "端口")]
        public int Port { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        [HintDisplayName("用户名", "用户名")]

        public string Username { get; set; }

        /// <summary>
        /// 口令
        /// </summary>
        [HintDisplayName("口令", "口令")]

        public string Password { get; set; }

        /// <summary>
        /// 是否启用SSL
        /// </summary>
        [HintDisplayName("是否启用SSL", "是否启用SSL")]
        public bool EnableSsl { get; set; }

        /// <summary>
        /// 是的使用默认认证
        /// </summary>
        [HintDisplayName("是的使用默认认证", "是的使用默认认证")]
        public bool UseDefaultCredentials { get; set; }

        /// <summary>
        /// 是否默认账户
        /// </summary>
        [HintDisplayName("是否默认账户", "是否默认账户")]
        public bool IsDefaultEmailAccount { get; set; }

        /// <summary>
        /// 测试邮件地址
        /// </summary>
        [HintDisplayName("测试邮件地址", "测试邮件地址")]

        public string SendTestEmailTo { get; set; }

    }

}