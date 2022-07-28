using DCMS.Core;
using DCMS.Core.Domain.Users;
using DCMS.Core.ZXing;
using DCMS.Services.Authentication;
using DCMS.Services.Events;
using DCMS.Services.Logging;
using DCMS.Services.Users;
using DCMS.ViewModel.Models.Users;
using DCMS.Web.Factories;
using DCMS.Web.Framework.Controllers;
using DCMS.Web.Framework.Mvc.Filters;
using DCMS.Web.Framework.Security;
using DCMS.Web.Framework.UI;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Threading;
using DCMS.Core.Caching;
using Newtonsoft.Json;

namespace DCMS.Web.Controllers
{
    public partial class AccountController : BaseController
    {
        private readonly UserSettings _userSettings;
        private readonly IAuthenticationService _authenticationService;
        private readonly IUserActivityService _userActivityService;
        private readonly IUserModelFactory _userModelFactory;
        private readonly IUserRegistrationService _userRegistrationService;
        private readonly IUserService _userService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IWorkContext _workContext;
        private readonly IStaticCacheManager _cacheManager;


        public AccountController(
            UserSettings userSettings,
            IAuthenticationService authenticationService,
            IUserActivityService userActivityService,
            IUserModelFactory userModelFactory,
            IUserRegistrationService userRegistrationService,
            IUserService userService,
            IEventPublisher eventPublisher,
            IStaticCacheManager cacheManager,
            IWorkContext workContext)
        {
            _workContext = workContext;
            _userSettings = userSettings;
            _authenticationService = authenticationService;
            _userActivityService = userActivityService;
            _userModelFactory = userModelFactory;
            _userRegistrationService = userRegistrationService;
            _userService = userService;
            _eventPublisher = eventPublisher;
            _cacheManager = cacheManager;
        }



        #region 登录 / 注销


        [CheckAccessPublicStore(true)]
        [HttpsRequirement(SslRequirement.Yes)]
        public virtual IActionResult Login(bool? checkoutAsGuest)
        {
            // 生成二维码的内容
            string ConntionId = CommonHelper.GenerateStr(32) + CommonHelper.GetTimeStamp(DateTime.Now);
            string strCode = LayoutExtensions.ResourceServerUrl("/updater/app/download?ConntionId=" + ConntionId);
            var model = _userModelFactory.PrepareLoginModel(checkoutAsGuest);
            model.Code = BarcodeHelper.GenerateQR(strCode, 300, 300);//二维码
            return View(model);
        }

        [HttpGet]
        public JsonResult GenerateQRCode(string code)
        {
            var data = new { UUID = code, Type = "Login" };
            var qrcode = BarcodeHelper.GenerateQR(JsonConvert.SerializeObject(data) , 300, 300);
            return Json(new
            {
                Success = true,
                Data = qrcode
            });
        }


        [HttpPost]
        //[ValidateCaptcha]
        [CheckAccessPublicStore(true)]
        [AutoValidateAntiforgeryToken]
        public virtual IActionResult Login(LoginModel model, string returnUrl, bool captchaValid)
        {
            //暂时不启用验证码 [ValidateCaptcha]
            //if (_captchaSettings.Enabled && _captchaSettings.ShowOnLoginPage && !captchaValid)
            //{
            //    ModelState.AddModelError("", "验证码错误");
            //}

            if (ModelState.IsValid)
            {
                if (_userSettings.UsernamesEnabled && model.UserName != null)
                {
                    model.UserName = model.UserName.Trim();
                }

                User user = null;
                var loginResult = _userRegistrationService.ValidateUser(ref user, model.UserName, model.Password, isTrader: true,isPlatform:true);
                if (user != null)
                {
                    switch (loginResult)
                    {
                        case DCMSStatusCode.Successful:
                            {
                                //登录
                                _authenticationService.SignIn(user, model.RememberMe);

                                //引发登录事件
                                _eventPublisher.Publish(new UserLoggedinEvent(user));

                                //记录活动日志
                                _userActivityService.InsertActivity(user, "PublicStore.Login", "用户登录", user);


                                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                                {
                                    return RedirectToRoute("Homepage", new { store = user.StoreId });
                                }
                                else
                                {
                                    return RedirectToRoute("Homepage", new { store = user.StoreId });
                                }
                            }
                        case DCMSStatusCode.UserNotExist:
                            ModelState.AddModelError("", "用户不存在！");
                            break;
                        case DCMSStatusCode.Deleted:
                            ModelState.AddModelError("", "用户已经删除！");
                            break;
                        case DCMSStatusCode.NotActive:
                            ModelState.AddModelError("", "用户未激活！");
                            break;
                        case DCMSStatusCode.NotRegistered:
                            ModelState.AddModelError("", "用户不是注册用户！");
                            break;
                        case DCMSStatusCode.LockedOut:
                            ModelState.AddModelError("", "用户被锁定！");
                            break;
                        case DCMSStatusCode.WrongPassword:
                        default:
                            ModelState.AddModelError("", "提供的凭证无效或者错误的验证信息！");
                            break;
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", "提供的凭证无效或者错误的身份验证信息！");
            }

            model = _userModelFactory.PrepareLoginModel(false);
            return View(model);
        }


        /// <summary>
        /// 自动登陆
        /// </summary>
        /// <param name="uuid"></param>
        /// <param name="userid"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        [CheckAccessPublicStore(true)]
        public async Task<IActionResult> AutoAuthenticate(string uuid,int userid, string pwd)
        {
            return await Task.Run(() =>
            {
                if (string.IsNullOrEmpty(uuid))
                    return Json(new { Success = false, Massage = "凭证无效" });

                try
                {
                    var cacheKey = new CacheKey($"QRCLIENTINFOLIST_UUID_{uuid}");
                    var cuuid = _cacheManager.Get<string>(cacheKey, null);

                    if (string.IsNullOrEmpty(cuuid))
                        return Json(new { Success = false, Massage = "二维码已经过期" });

                    User user = _userService.GetUserById(userid);
                    if (user == null)
                        return Json(new { Success = false });

                    if (user.Password == pwd)
                    {
                        //登录
                        _authenticationService.SignIn(user, true);

                        //引发登录事件
                        _eventPublisher.Publish(new UserLoggedinEvent(user));

                        _cacheManager.Remove(cacheKey);

                        return Json(new { Success = true, Store = user.StoreId });
                    }

                    return Json(new { Success = false });
                }
                catch (Exception)
                {
                    return Json(new { Success = false });
                }
            });
        }


        /// <summary>
        /// 注销
        /// </summary>
        /// <returns></returns>
        [CheckAccessPublicStore(true)]
        public virtual IActionResult Logout()
        {
            _userActivityService.InsertActivity(_workContext.CurrentUser, "PublicStore.Logout", "用户注销", _workContext.CurrentUser);
            _authenticationService.SignOut();
            _eventPublisher.Publish(new UserLoggedOutEvent(_workContext.CurrentUser));
            return RedirectToRoute("Login");
        }


        #endregion

    }
}