using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using DCMS.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using DCMS.ViewModel.Models.Common;


namespace DCMS.ViewModel.Models.Users
{
    /// <summary>
    /// 业务员数据模型
    /// </summary>
    public partial class BusinessUserModel : BaseEntityModel
    {
        public int? Dirtleader { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public int BranchId { get; set; } = 0;
        public string BranchName { get; set; }
        public string UserRealName { get; set; }
        public string MobileNumber { get; set; }
        public int? SalesmanExtractPlanId { get; set; } = 0;
        public int? DeliverExtractPlanId { get; set; } = 0;
        public decimal? MaxAmountOfArrears { get; set; } = 0;
        public int[] SelectedUserDistricts { get; set; }
        public List<UserDistrictsModel> AvailableUserDistricts { get; set; }
        public string FaceImage { get; set; }
        public List<PermissionRecordModel> AvailablePermissionRecords { get; set; }
    }


    public partial class UserModel : BaseEntityModel, IParentList
    {
        public UserModel()
        {
            SendEmail = new SendEmailModel();
            SendPm = new SendPmModel();
            AvailableUserRoles = new List<UserRoleModel>();
            AssociatedExternalAuthRecords = new List<AssociatedExternalAuthModel>();
            AvailableCountries = new List<SelectListItem>();
            AvailableStates = new List<SelectListItem>();
            AvailableUserDistricts = new List<UserDistrictsModel>();
        }

        public bool AllowUsersToChangeUsernames { get; set; }
        public bool UsernamesEnabled { get; set; }


        [DisplayName("直接上级")]
        public int? Dirtleader { get; set; }
        public string DirtleaderName { get; set; }
        public SelectList Dirtleaders { get; set; }


        [DisplayName("用户名")]
        public string Username { get; set; }

        [DataType(DataType.Password)]
        [HintDisplayName("密码", "密码")]

        public string Password { get; set; }

        [DisplayName("邮箱")]

        [RegularExpression(@"^[\w-]+(\.[\w-]+)*@([\w-]+\.)+[a-zA-Z]+$", ErrorMessage = "邮箱格式不正确")]
        public string Email { get; set; }

        public bool GenderEnabled { get; set; }
        [DisplayName("性别")]
        public string Gender { get; set; }
        public SelectList Genders { get; set; }



        [HintDisplayName("所属部门", "所属部门")]
        public int BranchId { get; set; } = 0;
        [HintDisplayName("所属部门", "所属部门")]
        public string BranchName { get; set; }
        public SelectList ParentList { get; set; }



        [DisplayName("真实姓名")]

        public string UserRealName { get; set; }

        public bool DateOfBirthEnabled { get; set; }
        [UIHint("DateNullable")]
        [DisplayName("生日")]
        public DateTime? DateOfBirth { get; set; }

        public bool StreetAddressEnabled { get; set; }
        [DisplayName("联系地址")]

        public string StreetAddress { get; set; }

        public bool CityEnabled { get; set; }
        [DisplayName("城市")]

        public string City { get; set; }

        public bool CountryEnabled { get; set; }
        [DisplayName("国家")]
        public int CountryId { get; set; } = 0;
        public IList<SelectListItem> AvailableCountries { get; set; }

        public bool StateProvinceEnabled { get; set; }
        [DisplayName("省份")]
        public int StateProvinceId { get; set; } = 0;
        public IList<SelectListItem> AvailableStates { get; set; }

        public bool PhoneEnabled { get; set; }
        [DisplayName("手机号")]
        [RegularExpression(@"^1[3456789][0-9]{9}$", ErrorMessage = "手机号格式不正确")]

        public string MobileNumber { get; set; }


        [DisplayName("是否有效")]
        public bool Active { get; set; }

        [DisplayName("所属经销商")]
        public int? StoreID { get; set; } = 0;

        [DisplayName("账户类型")]
        public int? AccountType { get; set; } = 0;
        public SelectList AccountTypes { get; set; }


        [DisplayName("创建时间")]
        public DateTime CreatedOn { get; set; }
        [DisplayName("最后活动时间")]
        public DateTime LastActivityDate { get; set; }

        [DisplayName("IP地址")]
        public string LastIpAddress { get; set; }

        [DisplayName("最后访问页面")]
        public string LastVisitedPage { get; set; }



        [DisplayName("员工岗位(角色)")]
        public string UserRoleNames { get; set; }
        public List<UserRoleModel> AvailableUserRoles { get; set; }
        public int[] SelectedUserRoleIds { get; set; }
        public bool AllowManagingUserRoles { get; set; }


        [DisplayName("业务员提成方案")]
        public int? SalesmanExtractPlanId { get; set; } = 0;
        public SelectList SalesmanExtractPlans { get; set; }


        [DisplayName("送货员提成方案")]
        public int? DeliverExtractPlanId { get; set; } = 0;
        public SelectList DeliverExtractPlans { get; set; }


        [DisplayName("最大欠款额度")]
        public decimal? MaxAmountOfArrears { get; set; } = 0;


        [DisplayName("片区")]
        public int[] SelectedUserDistricts { get; set; }
        public List<UserDistrictsModel> AvailableUserDistricts { get; set; }
        public string UserDistrictsZTree { get; set; }

        /// <summary>
        /// 获取或设置 是否记住登录
        /// </summary>
        [Display(Name = "记住登录")]
        public bool IsRememberLogin { get; set; }

        [HintDisplayName("允许使用电脑端", "允许使用电脑端")]
        public bool UseACLPc { get; set; }

        [HintDisplayName("允许使用手机端", "允许使用手机端")]
        public bool UseACLMobile { get; set; }

        //发送邮件
        public SendEmailModel SendEmail { get; set; }
        //发送消息
        public SendPmModel SendPm { get; set; }

        [DisplayName("扩展身份认证")]
        public IList<AssociatedExternalAuthModel> AssociatedExternalAuthRecords { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string FaceImage { get; set; }


        /// <summary>
        /// UUID
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// 用户APP访问控制
        /// </summary>
        public string AppModuleAcl { get; set; }

        /// <summary>
        /// 直属下级
        /// </summary>
        public string Subordinates { get; set; }
        public bool Selected { get; set; }


        public List<PermissionRecordModel> AvailablePermissionRecords { get; set; }
        public string StoreName { get; set; }

        #region 嵌套类

        public partial class AssociatedExternalAuthModel : BaseEntityModel
        {
            [DisplayName("邮箱")]
            public string Email { get; set; }

            [DisplayName("扩展身份认证标识")]
            public string ExternalIdentifier { get; set; }

            [DisplayName("认证方法名")]
            public string AuthMethodName { get; set; }
        }


        public partial class SendEmailModel : BaseModel
        {
            [DisplayName("标题")]

            public string Subject { get; set; }

            [DisplayName("正文内容")]

            public string Body { get; set; }
        }

        public partial class SendPmModel : BaseModel
        {
            [DisplayName("标题")]
            public string Subject { get; set; }

            [DisplayName("消息内容")]
            public string Message { get; set; }
        }


        public partial class OrderModel : BaseEntityModel
        {
            [DisplayName("订单ID")]
            public override int Id { get; set; }

            [DisplayName("订单状态")]
            public string OrderStatus { get; set; }

            [DisplayName("支付状态")]
            public string PaymentStatus { get; set; }

            [DisplayName("配送状态")]
            public string ShippingStatus { get; set; }

            [DisplayName("订单总计")]
            public string OrderTotal { get; set; }

            [DisplayName("下单时间")]
            public DateTime CreatedOn { get; set; }
        }


        public partial class ActivityLogModel : BaseEntityModel
        {
            [DisplayName("日志类型")]
            public string ActivityLogTypeName { get; set; }
            [DisplayName("详细描述")]
            public string Comment { get; set; }
            [DisplayName("创建时间")]
            public DateTime CreatedOn { get; set; }
        }
        #endregion
    }

    public partial class UserDistrictsModel : BaseEntityModel
    {
        [HintDisplayName("用户", "用户")]
        public int UserId { get; set; } = 0;
        public string UserName { get; set; }

        [HintDisplayName("片区", "片区")]
        public int DistrictsId { get; set; } = 0;
        public string DistrictsName { get; set; }
    }

    public partial class UserDistrictsQuery
    {
        public int Id { get; set; }
        public int DistrictsId { get; set; }
    }


    public partial class UserModuleACLModel
    {
        public int UserId { get; set; }
        public List<ModuleTree<ModuleModel>> MenuTrees { get; set; } = new List<ModuleTree<ModuleModel>>();
    }


    public partial class SubordinatesModel
    {
        public int UserId { get; set; }
        public List<UserModel> Subordinates { get; set; } = new List<UserModel>();
    }

    public class ClientInfo
    {
        public int StoreId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public string UUID { get; set; }
        public string ConnectionId { get; set; }
        public string Avatar { get; set; }
    }

    public class SendMessage
    {
        public int StoreId { get; set; }

        public int ToUser { get; set; }
        public string ToUserName { get; set; }
        public string ToUserAvatar { get; set; }

        public int FromUser { get; set; }
        public string FromUserName { get; set; }
        public string FromUserAvatar { get; set; }

        public string Type { get; set; }
        public string Msg { get; set; }
        public string Images { get; set; }
        
        public DateTime Date { get; set; }
    }

    public partial class LoginModel : BaseModel
    {
        [DisplayName("邮箱")]
        public string Email { get; set; }

        [DisplayName("用户名")]
        public string UserName { get; set; }

        [DataType(DataType.Password)]
        [DisplayName("密码")]
        [DCMSTrim]
        public string Password { get; set; }

        [DisplayName("记住我")]
        public bool RememberMe { get; set; }

        [DisplayName("验证码")]

        //[Remote("IsCaptchaCodeValid", "Common", ErrorMessage = "验证码错误！")]
        public string CaptchaCode { get; set; }

        public bool DisplayCaptcha { get; set; }

        public bool IsValid { get; set; }

        public string AppId { get; set; }

        public string ReturnUrl { get; set; }
        [DisplayName("二维码")]
        public string Code { get; set; }
    }

    /// <summary>
    /// AuthenticateResponse
    /// </summary>
    public partial class UserAuthenticationModel : BaseEntityModel
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string Username { get; set; }
        public string FaceImage { get; set; }
        /// <summary>
        /// 真实姓名
        /// </summary>
        public string UserRealName { get; set; }
        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// 手机号
        /// </summary>
        public string MobileNumber { get; set; }
        /// 所属经销商
        /// </summary>
        //移除 = 0;
        public string StoreName { get; set; }
        public int StarRate { get; set; }

        /// <summary>
        /// ERP经销商编号
        /// </summary>
        public string DealerNumber { get; set; }

        /// <summary>
        /// ERP营销中心
        /// </summary>
        public string MarketingCenter { get; set; }
        public string MarketingCenterCode { get; set; }

        /// <summary>
        /// ERP销售大区
        /// </summary>
        public string SalesArea { get; set; }
        public string SalesAreaCode { get; set; }

        /// <summary>
        /// ERP业务部
        /// </summary>
        public string BusinessDepartment { get; set; }
        public string BusinessDepartmentCode { get; set; }


        public List<UserRoleQuery> Roles { get; set; } = new List<UserRoleQuery>();
        public List<BaseModule> Modules { get; set; } = new List<BaseModule>();
        public List<PermissionRecordQuery> PermissionRecords { get; set; } = new List<PermissionRecordQuery>();
        public List<UserDistrictsQuery> Districts { get; set; } = new List<UserDistrictsQuery>();
        public string AppId { get; set; }

        /// <summary>
        /// Token
        /// </summary>
        public string AccessToken { get; set; }

        [JsonIgnore] 
        public string RefreshToken { get; set; }

    }

    public class RevokeTokenRequest
    {
        public string Token { get; set; }
    }

    public partial class PassWordChangeModel
    {
        [DataType(DataType.Password)]
        [HintDisplayName("旧密码", "旧密码")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [HintDisplayName("新密码", "新密码")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [HintDisplayName("确认密码", "确认密码")]
        public string AgainPassword { get; set; }
    }

    public partial class AccountActivationModel
    {
        public string Result { get; set; }
        public bool Success { get; set; }
    }

    public partial class UserAvatarModel : BaseModel
    {
        public string AvatarUrl { get; set; }
    }

    public partial class UserAssessmentListModel : BaseModel, IParentList
    {
        public UserAssessmentListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
        }

        public string Name { get; set; }

        public string Key { get; set; }

        public PagingFilteringModel PagingFilteringContext { get; set; }

        public IList<UserAssessmentModel> Lists { get; set; }
        public SelectList ParentList { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
    /// <summary>
    /// 考核目标
    /// </summary>
    public partial class UserAssessmentModel : BaseEntityModel
    {

        [HintDisplayName("年份", "年份")]
        public int Year { get; set; } = 0;
        public string Name { get; set; }

        public List<UserAssessmentItemModel> Items { get; set; }
    }


    /// <summary>
    /// 考核目标明细
    /// </summary>
    public class UserAssessmentItemModel : BaseEntityModel
    {
        public int AssessmentId { get; set; } = 0;
        /// <summary>
        /// 员工Id
        /// </summary>
        public int? UserId { get; set; } = 0;
        /// <summary>
        /// 员工姓名
        /// </summary>
        public string UserName { get; set; }
        public decimal Jan { get; set; } = 0;
        public decimal Feb { get; set; } = 0;
        public decimal Mar { get; set; } = 0;
        public decimal Apr { get; set; } = 0;
        public decimal May { get; set; } = 0;
        public decimal Jun { get; set; } = 0;
        public decimal Jul { get; set; } = 0;
        public decimal Aug { get; set; } = 0;
        public decimal Sep { get; set; } = 0;
        public decimal Oct { get; set; } = 0;
        public decimal Nov { get; set; } = 0;
        public decimal Dec { get; set; } = 0;
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        public DateTime CreatedOnUtc { get; set; }
    }

}