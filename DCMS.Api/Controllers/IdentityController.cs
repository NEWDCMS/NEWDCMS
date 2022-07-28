using DCMS.Api.Infrastructure.Mapper.Extensions;
using DCMS.Core;
using DCMS.Core.Configuration;
using DCMS.Core.Domain.Users;
using DCMS.Services.Authentication;
using DCMS.Services.Stores;
using DCMS.Services.Users;
using DCMS.ViewModel.Models.Users;
using DCMS.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DCMS.Core.Caching;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace DCMS.Api.Controllers
{
    /// <summary>
    /// 用于身份认证
    /// </summary>
    [Produces("application/json")]
    [Route("api/v{version:apiVersion}/dcms/auth")]
    public class IdentityController : BaseAPIController
    {
        //http://api.jsdcms.com:9998/api/v3/dcms/auth/user/login

        private readonly IAuthenticationService _authenticationService;
        private readonly IUserRegistrationService _userRegistrationService;
        private readonly IUserService _userService;
        private readonly IStoreService _storeService;
        private readonly IConfiguration _configuration;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IHubContext<ChatHub> _qrHub;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="authenticationService"></param>
        /// <param name="userRegistrationService"></param>
        /// <param name="storeService"></param>
        /// <param name="configuration"></param>
        /// <param name="userService"></param>
        /// <param name="logger"></param>
        public IdentityController(IAuthenticationService authenticationService,
             IUserRegistrationService userRegistrationService,
             IStoreService storeService,
             IConfiguration configuration,
             IStaticCacheManager cacheManager,
             IUserService userService,
             IHubContext<ChatHub> qrHub,
             ILogger<BaseAPIController> logger) : base(logger)
        {
            _authenticationService = authenticationService;
            _userRegistrationService = userRegistrationService;
            _storeService = storeService;
            _userService = userService;
            _configuration = configuration;
            _cacheManager = cacheManager;
            _qrHub = qrHub;
        }


        //[AllowAnonymous]
        //[HttpGet("heartbeat")]
        //public async Task<APIResult<object>> SendHeartbeat(string msg)
        //{
        //    //所有客户端发送
        //    foreach (var c in Program.ClientInfoList)
        //    {
        //        await _qrHub.Clients.Client(c.ConnectionId).SendAsync("OnHeartbeat", msg);
        //    }

        //    return this.Successful("广播成功");
        //}

        //[AllowAnonymous]
        //[HttpPost("send/message")]
        //public async Task<APIResult<object>> SendMessage(SendMessage data, string call = "Receive")
        //{
        //    try
        //    {
        //        if (data == null)
        //        {
        //            return this.Error("参数错误");
        //        }

        //        var client = Program.ClientInfoList?.Where(u => u.UserId == data.ToUser)
        //            .SingleOrDefault();

        //        if (client != null)
        //        {
        //            data.Date = DateTime.Now;
        //            await _qrHub.Clients.Client(client.ConnectionId)
        //                .SendAsync(call, JsonConvert.SerializeObject(data));

        //            //稍后存储到数据库
        //        }

        //        return this.Successful("发送成功");
        //    }
        //    catch (Exception ex)
        //    {
        //        return this.Error(ex.Message);
        //    }
        //}


        [AllowAnonymous]
        [HttpGet("online")]
        public APIResult<IList<ClientInfo>> GetOnlineUsers(int? storeId)
        {
            if (!storeId.HasValue || storeId <= 0)
            {
                var users = Program.ClientInfoList.ToList();
                return this.Successful2("获取成功", users);
            }
            else
            {
                var users = Program.ClientInfoList.Where(s => s.StoreId == storeId).ToList();
                return this.Successful2("获取成功", users);
            }
        }


        /// <summary>
        /// 用于用户登录检测
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("user/login")]
        //[AuthBaseFilter]
        public async Task<APIResult<UserAuthenticationModel>> Authenticate([FromBody] LoginModel model)
        {
            return await Task.Run(() =>
            {
                if (string.IsNullOrEmpty(model.UserName) || string.IsNullOrEmpty(model.Password))
                    return this.Error3<UserAuthenticationModel>("用户名密码不能为空");

                try
                {

                    User user = null;
                    //验证用户
                    var loginResult = _userRegistrationService.ValidateUser(ref user, model.UserName, model.Password, true, appId: model.AppId);

                    switch (loginResult)
                    {
                        case DCMSStatusCode.Successful when user != null:
                            {

                                _authenticationService.SignIn(user, model.RememberMe);

                                var auth = user.ToModel<UserAuthenticationModel>();

                                var store = _storeService.GetStoreById(user.StoreId);

                                auth.StoreName = store?.Name;
                                auth.StarRate = store?.StarRate ?? 0;

                                auth.DealerNumber = store?.DealerNumber;
                                auth.MarketingCenter = store?.MarketingCenter;
                                auth.MarketingCenterCode = store?.MarketingCenterCode;
                                auth.SalesArea = store?.SalesArea;
                                auth.SalesAreaCode = store?.SalesAreaCode;
                                auth.BusinessDepartment = store?.BusinessDepartment;
                                auth.BusinessDepartmentCode = store?.BusinessDepartmentCode;


                                auth.Roles = user.UserRoles?.Select(s =>
                                {
                                    return new UserRoleQuery()
                                    {
                                        Id = s.Id,
                                        Name = s.Name,
                                        SystemName = s.SystemName
                                    };
                                })?.ToList() ?? new List<UserRoleQuery>();

                                auth.PermissionRecords = _userService.GetUserAPPPermissionRecords(user)?.Select(s =>
                                {
                                    return new PermissionRecordQuery()
                                    {
                                        Id = s.Id,
                                        Name = s.Name,
                                        Code = s.Code
                                    };
                                })?.ToList() ?? new List<PermissionRecordQuery>();


                                auth.Modules = _userService.GetUserModuleRecords(user.StoreId, user, true)?.Select(s =>
                                 {
                                     return new BaseModule()
                                     {
                                         Id = s.Id,
                                         Name = s.Name,
                                         Code = s.Code
                                     };
                                 })?.ToList() ?? new List<BaseModule>();


                                auth.Districts = _userService.GetAllUserDistrictsByUserId(user.StoreId, user.Id)?.Select(s =>
                                {
                                    return new UserDistrictsQuery()
                                    {
                                        Id = s.Id,
                                        DistrictsId = s.DistrictId,
                                    };
                                })?.ToList() ?? new List<UserDistrictsQuery>();


                                //Token 处理
                                var accessToken = GenerateJwtToken(auth);
                                var refreshToken = GenerateRefreshToken(IpAddress());

                                auth.AppId = user.AppId;
                                auth.AccessToken = accessToken;
                                auth.RefreshToken = refreshToken.Token;
                                auth.FaceImage = user?.FaceImage;

                            //保存
                            SetTokenCookie(refreshToken.Token);

                            //保存刷新Token
                            //user.RefreshTokens.Add(refreshToken);

                            refreshToken.UserId = user.Id;
                            _userService.AddToken(refreshToken);

                            _userService.UpdateUser(user);

                                return this.Successful("登录成功", auth);
                            }
                        case DCMSStatusCode.UserNotExist:
                            return this.Error3<UserAuthenticationModel>("用户不存在");
                        case DCMSStatusCode.Deleted:
                            return this.Error3<UserAuthenticationModel>("用户已经被删除");
                        case DCMSStatusCode.NotActive:
                            return this.Error3<UserAuthenticationModel>("账户尚未激活");
                        case DCMSStatusCode.NotRegistered:
                            return this.Error3<UserAuthenticationModel>("账户未注册");
                        case DCMSStatusCode.WrongPassword:
                            return this.Error3<UserAuthenticationModel>("密码错误");
                        case DCMSStatusCode.RepeatDeviceLogin:
                            return this.Error3<UserAuthenticationModel>("当前账户已经在其它设备登录");
                        default:
                            return this.Error3<UserAuthenticationModel>("错误的身份认证");
                    }
                }
                catch (Exception ex)
                {
                    return this.Error3<UserAuthenticationModel>(ex.Message);
                }
            });
        }

        /// <summary>
        /// 请求IP地址
        /// </summary>
        /// <returns></returns>
        private string IpAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"];
            else
                return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }

        /// <summary>
        /// 生成token
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private string GenerateJwtToken(UserAuthenticationModel user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var section = _configuration.GetSection("DCMS:JWT");
            var jwt = section.Get<JwtOptions>();
            var claims = new[]
                {
                   //new Claim(ClaimTypes.Name, user.Id.ToString())
                    new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Sid, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt?.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            //var expires = DateTime.Now.AddSeconds(jwt?.AccessExpireMins ?? 30);
            var expires = DateTime.Now.AddDays(7);
            var token = new JwtSecurityToken(
                jwt?.Issuer,
                jwt?.Issuer,
                claims,
                expires: expires,
                signingCredentials: creds
            );

            //var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// 生成刷新token
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        private RefreshToken GenerateRefreshToken(string ipAddress)
        {
            using var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            var randomBytes = new byte[64];
            rngCryptoServiceProvider.GetBytes(randomBytes);
            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomBytes),
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };
        }

        /// <summary>
        /// 刷新Token
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public IActionResult RefreshToken(string rtoken = "")
        {
            try
            {

                rtoken = CommonHelper.FilterSQLChar(rtoken);

                var token = Request.Cookies["refreshToken"];
                if (string.IsNullOrEmpty(token))
                    token = rtoken;

                var ipAddress = IpAddress();

                var user = _userService.GetUserByToken(token);
                if (user == null)
                    return Unauthorized(new { message = "Invalid token" });

                var auth = user.ToModel<UserAuthenticationModel>();

                var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

                if (!refreshToken.IsActive)
                    return Unauthorized(new { message = "Token is invalid" });

                // 用新令牌替换旧的刷新令牌并保存
                var newRefreshToken = GenerateRefreshToken(ipAddress);
                refreshToken.Revoked = DateTime.UtcNow;
                refreshToken.RevokedByIp = ipAddress;
                refreshToken.ReplacedByToken = newRefreshToken.Token;

                user.RefreshTokens.Add(newRefreshToken);
                _userService.UpdateUser(user);

                //生成新JWT
                var jwtToken = GenerateJwtToken(auth);

                auth.AccessToken = jwtToken;
                auth.RefreshToken = newRefreshToken.Token;

                if (auth == null)
                    return Unauthorized(new { message = "Invalid token" });

                SetTokenCookie(newRefreshToken.Token);

                return Ok(auth);
            }
            catch (Exception ex)
            {
                return Forbid(ex.Message);
            }
        }


        /// <summary>
        /// 吊销Token
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("user/logout/{userid}")]
        [Authorize]
        public IActionResult RevokeToken([FromBody] RevokeTokenRequest model, int? store, int? userid)
        {
            try
            {
                // 接受来自请求主体或cookie的令牌
                var token = model.Token ?? Request.Cookies["refreshToken"];
                var ipAddress = IpAddress();

                if (string.IsNullOrEmpty(token))
                    return BadRequest(new { message = "Token is required" });

                var user = _userService.GetUserByToken(token);
                if (user == null)
                    return NotFound(new { Message = "Token not found" });

                var refreshToken = user.RefreshTokens?.Single(x => x.Token == token);

                if (refreshToken != null)
                {
                    if (!refreshToken.IsActive)
                        return NotFound(new { Message = "Token not found" });

                    // 撤销令牌并保存
                    refreshToken.Revoked = DateTime.UtcNow;
                    refreshToken.RevokedByIp = ipAddress;

                    _userService.UpdateUser(user);
                }

                //Sign Out
                _authenticationService.SignOut();

                return Ok(new { Message = $"Token revoked,{userid} {store}" });
            }
            catch (Exception ex)
            {
                return Forbid(ex.Message);
            }
        }

        /// <summary>
        /// 保存Token到Cookie
        /// </summary>
        /// <param name="token"></param>
        private void SetTokenCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("refreshToken", token, cookieOptions);
        }


        ///// <summary>
        ///// 注销
        ///// </summary>
        ///// <param name="store"></param>
        ///// <param name="userid"></param>
        ///// <returns></returns>
        //[HttpGet("user/logout/{userid}")]
        ////[AuthBaseFilter]
        //public async Task<APIResult<UserModel>> Logout(int? store, int? userid)
        //{
        //    return await Task.Run(() =>
        //    {
        //        try
        //        {
        //            if (userid.HasValue)
        //            {


        //                //_userService.UpdateUUID(store, userid.Value, "");
        //                return this.Successful<UserModel>("注销成功");
        //            }
        //            return this.Error3<UserModel>("注销失败");
        //        }
        //        catch (Exception ex)
        //        {
        //            return this.Error3<UserModel>(ex.Message);
        //        }
        //    });
        //}

        /// <summary>
        /// 检查用户状态
        /// </summary>
        /// <param name="store"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        [HttpGet("user/checkStatus/{store}/{userid}")]
        [SwaggerOperation("checkStatus")]
        //[AuthBaseFilter]
        [Authorize]
        public async Task<APIResult<UserModel>> CheckStatus(int? store, int? userid)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error3<UserModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var result = new UserModel();
                    if (userid.HasValue)
                    {
                        var user = _userService.GetUserById(store ?? 0, userid ?? 0, true);
                        if (user != null)
                        {
                            result = user.ToModel<UserModel>();
                        }
                    }
                    return this.Successful("", result);
                }
                catch (Exception ex)
                {
                    return this.Error3<UserModel>(ex.Message);
                }

            });
        }


        /// <summary>
        /// AuthBearerAsync
        /// </summary>
        /// <param name="store"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        [HttpGet("user/bearer/{store}/{userid}")]
        [SwaggerOperation("authbearerasync")]
        [Authorize]
        public async Task<APIResult<object>> AuthBearerAsync(int? store, int? userid)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error3<object>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    return this.Successful("", new { Userid = userid });
                }
                catch (Exception ex)
                {
                    return this.Error3<object>(ex.Message);
                }
            });
        }


        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="store"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpGet("user/getUserByName/{name}")]
        [SwaggerOperation("getUserByName")]
        //[AuthBaseFilter]
        [Authorize]
        public async Task<APIResult<UserModel>> GetUserByName(int? store, string name)
        {
            name = CommonHelper.FilterSQLChar(name);

            if (!store.HasValue || store.Value == 0)
                return this.Error3<UserModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    if (!string.IsNullOrEmpty(name))
                    {
                        User user;
                        //Email
                        if (CommonHelper.IsValidEmail(name))
                        {
                            user = _userService.GetUserByEmail(store, name);
                        }
                        //MobileNumber
                        else if (CommonHelper.IsValidMobileNumber(name))
                        {
                            user = _userService.GetUserByMobileNamber(store, name);
                        }
                        //Usernames
                        else
                        {
                            user = _userService.GetUserByUsername(store, name);
                        }

                        var result = user.ToModel<UserModel>();

                        return this.Successful("", result);
                    }

                    return this.Error3<UserModel>("用户名不存在");
                }
                catch (Exception ex)
                {
                    return this.Error3<UserModel>(ex.Message);
                }

            });
        }

        /// <summary>
        /// 获取用户权限
        /// </summary>
        /// <param name="store"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        [HttpGet("user/permission/{userid}")]
        [SwaggerOperation("getUserPermission")]
        //[AuthBaseFilter]
        [Authorize]
        public async Task<APIResult<IList<PermissionRecordQuery>>> GetUserPermission(int? store, int? userid)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error2<PermissionRecordQuery>(Resources.ParameterError);

            return await Task.Run(() =>
            {

                try
                {
                    var result = new List<PermissionRecordQuery>();
                    if (userid.HasValue && userid.Value > 0)
                    {
                        var user = _userService.GetUserById(store ?? 0, userid ?? 0);
                        if (user != null)
                        {
                            result = _userService.GetUserAPPPermissionRecords(user).Select(s =>
                            {
                                return new PermissionRecordQuery()
                                {
                                    Id = s.Id,
                                    Name = s.Name,
                                    Code = s.Code
                                };
                            }).ToList();
                        }
                    }

                    return this.Successful2("", result);
                }
                catch (Exception ex)
                {
                    return this.Error2<PermissionRecordQuery>(ex.Message);
                }

            });
        }

        /// <summary>
        /// 更新用户头像
        /// </summary>
        /// <param name="store"></param>
        /// <param name="image"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        [HttpPost("user/profiles/updateFaceImage/{userid}")]
        [SwaggerOperation("upLoadFaceImage")]
        //[AuthBaseFilter]
        [Authorize]
        public async Task<APIResult<string>> UpLoadFaceImage(int? store, string image, int? userid)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error3<string>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    if (userid.HasValue)
                    {
                        var user = _userService.GetUserById(store ?? 0, userid ?? 0);
                        if (user != null)
                        {
                            user.FaceImage = image;
                            _userService.UpdateUser(user);
                            return this.Successful3<string>("更新成功");
                        }
                    }

                    return this.Error3<string>("更新失败");
                }
                catch (Exception ex)
                {
                    return this.Error3<string>(ex.Message);
                }

            });
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="store"></param>
        /// <param name="model"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        [HttpPost("user/profiles/changePassword/{userid}")]
        [SwaggerOperation("changePassword")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        [Authorize]
        public async Task<APIResult<string>> ChangePassword(int? store, [FromBody] PassWordChangeModel model, int? userid)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error3<string>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    if (userid.HasValue)
                    {
                        var user = _userService.GetUserById(store ?? 0, userid ?? 0);
                        if (user == null)
                            return this.Successful3<string>("当前用户不存在");

                        if (string.IsNullOrWhiteSpace(model.NewPassword))
                            return this.Successful3<string>("新密码不能为空");

                        if (model.NewPassword != model.AgainPassword)
                            return this.Successful3<string>("两次输入密码不一致");

                        var changePassRequest = new ChangePasswordRequest(user.Email, true, PasswordFormat.Encrypted, model.NewPassword, model.Password);

                        var changePassResult = _userRegistrationService.ChangePassword(changePassRequest);
                        if (!changePassResult.Success)
                            return this.Error3<string>(string.Join(";", changePassResult.Errors.Select(s => s)));
                        else
                            return this.Successful3<string>("修改成功");
                    }

                    return this.Successful3<string>("修改失败");
                }
                catch (Exception ex)
                {
                    return this.Error3<string>(ex.Message);
                }
            });
        }

        /// <summary>
        /// 扫码登陆
        /// </summary>
        /// <param name="uuid"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost("user/qrlogin")]
        [SwaggerOperation("qrlogin")]
        public async Task<APIResult<object>> QRLgoin(string uuid, int userId)
        {
            uuid = CommonHelper.FilterSQLChar(uuid);

            var cacheKey = new CacheKey($"QRCLIENTINFOLIST_UUID_{uuid}") { CacheTime = 5 };

            if (string.IsNullOrEmpty(uuid))
                return this.Error3<object>("UUID参数错误");

            return await Task.Run(async () =>
            {
                try
                {
                    var user = _userService.GetUserById(userId);

                    var cuuid = _cacheManager.Get<string>(cacheKey, null);

                    if (cuuid.Equals(uuid))
                    {
                        var client = Program.ClientInfoList.Where(u => u.UUID == uuid).SingleOrDefault();

                        //发送用户ID 
                        //await Program.QRHub.SendUserInfo(client.ConnectionId, uuid, user.Id, user?.Password);
                        //_qrHub
                        // await Clients.Client(connectionId).SendAsync("GetUserInfo", uuid, userId, pwd);

                        await _qrHub.Clients
                        .Client(client.ConnectionId)
                        .SendAsync("GetUserInfo", uuid, user.Id, user?.Password);

                        //移除客户端
                        Program.ClientInfoList.Remove(client);

                        return this.Successful("扫码登陆成功", new { Userid = userId });
                    }
                    else
                    {
                        return this.Error3<object>("扫码登陆失败");
                    }
                }
                catch (Exception ex)
                {
                    return this.Error3<object>(ex.Message);
                }
            });
        }



        

    }
}
