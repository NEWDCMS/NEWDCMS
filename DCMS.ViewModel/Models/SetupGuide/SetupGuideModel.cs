using DCMS.ViewModel.Models.Common;
using DCMS.ViewModel.Models.Configuration;
using DCMS.ViewModel.Models.Users;
using DCMS.Web.Framework;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DCMS.ViewModel.Models.SetupGuide
{
    public class SetupGuideModel
    {
        public SetupGuideStepOneModel StepOneModel { get; set; }

        public SetupGuideStepTwoModel StepTwoModel { get; set; } = new SetupGuideStepTwoModel();
        public SetupGuideStepThreeModel StepThreeModel { get; set; } = new SetupGuideStepThreeModel();
    }

    public class SetupGuideStepOneModel
    {

        [DisplayName("名称")]
        public string Name { get; set; }
        [DisplayName("负责人姓名")]
        public string ManagerName { get; set; }

        [DisplayName("手机号")]
        [RegularExpression(@"^1[345789][0-9]{9}$", ErrorMessage = "手机号格式不正确")]
        public string MobileNumber { get; set; }

        [DisplayName("邮箱")]
        [RegularExpression(@"^[\w-]+(\.[\w-]+)*@([\w-]+\.)+[a-zA-Z]+$", ErrorMessage = "邮箱格式不正确")]
        public string Email { get; set; }

        [DisplayName("管理账号")]
        public string UserName { get; set; }

        [DataType(DataType.Password)]
        [HintDisplayName("密码", "密码")]
        public string Password { get; set; }

    }

    public class SetupGuideStepTwoModel
    {
        public string SelectUserRoleIds { get; set; }
        /// <summary>
        /// 用户角色
        /// </summary>
        public CheckBoxModel UserRoleCheckModel { get; set; } = new CheckBoxModel();

        public string SelectUserGroupIds { get; set; }
        /// <summary>
        /// 用户组
        /// </summary>
        public CheckBoxModel UserGroupCheckModel { get; set; } = new CheckBoxModel();

        public List<UserRoleModel> UserRoles { get; set; }
        public List<ModuleModel> Modules { get; set; }
        public List<PermissionRecordModel> PermissionRecords { get; set; }


        /// <summary>
        /// 数据和频道
        /// </summary>
        public DataChannelPermissionModel DataChannelPermission { get; set; } = new DataChannelPermissionModel();
    }

    public class SetupGuideStepThreeModel
    {
        /// <summary>
        /// APP打印
        /// </summary>
        //public APPPrintSettingModel AppPrintSetting { get; set; }
        /// <summary>
        /// PC打印
        /// </summary>
        //public PCPrintSettingModel PCPrintSetting { get; set; }
        /// <summary>
        /// 公司设置
        /// </summary>
        //public CompanySettingModel CompanySetting { get; set; }
        /// <summary>
        /// 商品设置
        /// </summary>
        //public ProductSettingModel ProductSetting { get; set; }

        //public FinanceSettingModel FinanceSetting { get; set; }
        public List<PrintTemplateModel> PrintTemplates { get; set; }
        public List<AccountingOptionModel> AccountingOptions { get; set; }
    }
}
