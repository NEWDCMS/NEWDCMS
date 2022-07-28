using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Users;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using DCMS.Services.Security;
using System;


namespace DCMS.Services.Users
{

    /// <summary>
    /// 用户注册服务
    /// </summary>
    public partial class UserRegistrationService : BaseService, IUserRegistrationService
    {
        private readonly UserSettings _userSettings;
        private readonly IUserService _userService;
        private readonly IEncryptionService _encryptionService;
        //private readonly IGenericAttributeService _genericAttributeService;
        //private readonly IStoreService _storeService;
        //private readonly IWorkContext _workContext;

        public UserRegistrationService(UserSettings userSettings,
            IUserService userService,
            IEncryptionService encryptionService,
            IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {
            _userSettings = userSettings;
            _userService = userService;
            _encryptionService = encryptionService;
            //_genericAttributeService = genericAttributeService;
            //_storeService = storeService;
            //_workContext = workContext;
        }



        /// <summary>
        /// 检查输入的密码是否与保存的密码匹配
        /// </summary>
        /// <param name="userPassword"></param>
        /// <param name="enteredPassword"></param>
        /// <returns></returns>
        protected bool PasswordsMatch(UserPassword userPassword, string enteredPassword)
        {
            if (userPassword == null || string.IsNullOrEmpty(enteredPassword))
            {
                return false;
            }

            var savedPassword = string.Empty;
            switch (userPassword.PasswordFormat)
            {
                case PasswordFormat.Clear:
                    savedPassword = enteredPassword;
                    break;
                case PasswordFormat.Encrypted:
                    savedPassword = _encryptionService.EncryptText(enteredPassword);
                    break;
                case PasswordFormat.Hashed:
                    savedPassword = _encryptionService.CreatePasswordHash(enteredPassword, userPassword.PasswordSalt, _userSettings.HashedPasswordFormat);
                    break;
            }

            if (userPassword.Password == null)
            {
                return false;
            }

            return userPassword.Password.Equals(savedPassword);
        }

        /// <summary>
        /// 验证用户
        /// </summary>
        /// <param name="usernameOrEmailOrMobileNumber"></param>
        /// <param name="password"></param>
        /// <param name="isTrader"></param>
        /// <param name="appId"></param>
        /// <returns></returns>
        public virtual DCMSStatusCode ValidateUser(ref User user, string usernameOrEmailOrMobileNumber, string password, bool isTrader = false, string appId = "", bool isPlatform = false)
        {

            //Email
            if (CommonHelper.IsValidEmail(usernameOrEmailOrMobileNumber))
            {
                user = _userService.GetUserByEmailNoCache(0, usernameOrEmailOrMobileNumber, isTrader);
            }
            //MobileNumber
            else if (CommonHelper.IsValidMobileNumber(usernameOrEmailOrMobileNumber))
            {
                user = _userService.GetUserByMobileNamberNoCache(0, usernameOrEmailOrMobileNumber, isTrader);
            }
            //Usernames
            else
            {
                user = _userService.GetUserByUsernameNoCache(0, usernameOrEmailOrMobileNumber, isTrader);
            }
            //
            if (user == null)
            {
                return DCMSStatusCode.UserNotExist;
            }

            if (isTrader && user.StoreId == 0)
            {
                return DCMSStatusCode.UserNotExist;
            }

            if (user.Deleted)
            {
                return DCMSStatusCode.Deleted;
            }

            if (!user.SystemName.Equals("Administrators"))
            {
                if (isPlatform)
                {
                    if (!user.UseACLPc)
                    {
                        return DCMSStatusCode.NotActive;
                    }
                }
                else
                {
                    if (!user.UseACLMobile)
                    {
                        return DCMSStatusCode.NotActive;
                    }
                }
            }

            //用户尚未激活
            if (!user.Active)
            {
                return DCMSStatusCode.NotActive;
            }

            //判断是否系统账户（管理平台登录时使用）   2021-09-26 mu 注释
            //if (isPlatform)
            //{
            //    if (!user.IsPlatformCreate || !user.IsSystemAccount)
            //    {
            //        return DCMSStatusCode.UserNotExist;
            //    }
            //}

            //检查用户是否被锁定
            if (user.CannotLoginUntilDateUtc.HasValue && user.CannotLoginUntilDateUtc.Value > DateTime.Now)
            {
                return DCMSStatusCode.LockedOut;
            }

            string pwd = user.PasswordFormat switch
            {
                PasswordFormat.Encrypted => _encryptionService.EncryptText(password),
                PasswordFormat.Hashed => _encryptionService.CreatePasswordHash(password, user.PasswordSalt, _userSettings.HashedPasswordFormat),
                _ => password,
            };

            //如果用户信息在其它地方被修改
            if (string.IsNullOrEmpty(user.Password) && string.IsNullOrEmpty(user.PasswordSalt))
            {
                return DCMSStatusCode.Deleted;
            }
            //"C24B818907C6F5E94992B51AAB9F2B7AFB742D7A"
            //"A95FB62D98531FCD0320F0DBE9CE27B2FCE8B065"
            bool isValid = pwd.Equals(user.Password);
            if (!isValid)
            {
                //密码错误时失败尝试超过次数，账户将被锁定
                user.FailedLoginAttempts++;
                if (_userSettings.FailedPasswordAllowedAttempts > 0 &&
                    user.FailedLoginAttempts >= _userSettings.FailedPasswordAllowedAttempts)
                {
                    //锁定
                    user.CannotLoginUntilDateUtc = DateTime.UtcNow.AddMinutes(_userSettings.FailedPasswordLockoutMinutes);
                    //重置计数器
                    user.FailedLoginAttempts = 0;
                }
                _userService.UpdateUser(user);

                return DCMSStatusCode.WrongPassword;

            }
            else
            {
                //保存最后登陆时间
                //if (!string.IsNullOrEmpty(appId))
                //{
                //    if (!string.IsNullOrEmpty(user.AppId))
                //    {
                //        if (!user.AppId.ToUpper().Equals(appId.ToUpper()))
                //        {
                //            return DCMSStatusCode.RepeatDeviceLogin;
                //        }
                //    }
                //    user.AppId = appId;
                //}


                //user.FailedLoginAttempts = 0;
                //user.CannotLoginUntilDateUtc = null;
                //user.RequireReLogin = false;
                //user.LastLoginDateUtc = DateTime.Now;
                //user.LastActivityDateUtc = DateTime.Now;
                //_userService.UpdateUser(user);


                return DCMSStatusCode.Successful;
            }
        }

        /// <summary>
        /// 设置用户名
        /// </summary>
        /// <param name="user"></param>
        /// <param name="newUsername"></param>
        public virtual void SetUsername(User user, string newUsername)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (!_userSettings.UsernamesEnabled)
            {
                throw new DCMSException("用户名被禁用");
            }

            newUsername = newUsername.Trim();

            if (newUsername.Length > DCMSUserServiceDefaults.UserUsernameLength)
            {
                throw new DCMSException("用户名太长！");
            }

            var user2 = _userService.GetUserByUsername(0, newUsername);
            if (user2 != null && user.Id != user2.Id)
            {
                throw new DCMSException("用户名已经存在！");
            }

            user.Username = newUsername;
            _userService.UpdateUser(user);
        }


        /// <summary>
        /// 更改密码
        /// </summary>
        /// <param name="request">Request</param>
        /// <returns>Result</returns>
        public virtual PasswordChangeResult ChangePassword(ChangePasswordRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            var result = new PasswordChangeResult();
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                result.AddError("邮箱必须提供");
                return result;
            }
            if (string.IsNullOrWhiteSpace(request.NewPassword))
            {
                result.AddError("新密码必须提供");
                return result;
            }

            var user = _userService.GetUserByEmail(0, request.Email, noTracking:true);
            if (user == null)
            {
                result.AddError("邮箱不存在");
                return result;
            }


            var requestIsValid = false;
            if (request.ValidateRequest)
            {
                //password
                string oldPwd = "";
                switch (user.PasswordFormat)
                {
                    case PasswordFormat.Encrypted:
                        oldPwd = _encryptionService.EncryptText(request.OldPassword);
                        break;
                    case PasswordFormat.Hashed:
                        oldPwd = _encryptionService.CreatePasswordHash(request.OldPassword, user.PasswordSalt, _userSettings.HashedPasswordFormat);
                        break;
                    default:
                        oldPwd = request.OldPassword;
                        break;
                }

                bool oldPasswordIsValid = oldPwd == user.Password;

                if (!oldPasswordIsValid)
                {
                    result.AddError("新旧密码不匹配");
                }

                if (oldPasswordIsValid)
                {
                    requestIsValid = true;
                }
            }
            else
            {
                requestIsValid = true;
            }

            if (requestIsValid)
            {
                switch (request.NewPasswordFormat)
                {
                    case PasswordFormat.Clear:
                        {
                            user.Password = request.NewPassword;
                        }
                        break;
                    case PasswordFormat.Encrypted:
                        {
                            user.Password = _encryptionService.EncryptText(request.NewPassword);
                        }
                        break;
                    case PasswordFormat.Hashed:
                        {
                            string saltKey = _encryptionService.CreateSaltKey(5);
                            user.PasswordSalt = saltKey;
                            user.Password = _encryptionService.CreatePasswordHash(request.NewPassword, saltKey, _userSettings.HashedPasswordFormat);
                        }
                        break;
                    default:
                        break;
                }
                user.PasswordFormat = request.NewPasswordFormat;
                _userService.UpdateUser(user);
            }

            return result;
        }

    }
}
