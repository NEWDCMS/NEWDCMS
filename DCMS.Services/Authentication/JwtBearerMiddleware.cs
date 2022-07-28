using DCMS.Services.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;


namespace DCMS.Services.Authentication
{
    //public class TokenProviderOptions
    //{
    //    /// <summary>
    //    /// 请求路径 
    //    /// </summary>
    //    public string Path { get; set; } = "/api/v3/dcms/auth/user/login";
    //    public string Issuer { get; set; }
    //    public string Audience { get; set; }
    //    /// <summary>
    //    /// 过期时间
    //    /// </summary>
    //    public TimeSpan Expiration { get; set; } = TimeSpan.FromMinutes(5000);
    //    public SigningCredentials SigningCredentials { get; set; }
    //}

    /// <summary>
    /// 表示JWT身份认证中间件
    /// </summary>
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        //private readonly TokenProviderOptions _options;

        public JwtMiddleware(RequestDelegate next,
            //IOptions<TokenProviderOptions> options,
            IAuthenticationSchemeProvider schemes,
            IConfiguration configuration, 
            IUserService userService)
        {
            _next = next;
            //_options = options.Value;
            _configuration = configuration;
            _userService = userService;
            Schemes = schemes ?? throw new ArgumentNullException(nameof(schemes));

        }

        /// <summary>
        /// 
        /// </summary>
        public IAuthenticationSchemeProvider Schemes { get; set; }


        public async Task Invoke(HttpContext context, IAuthenticationService authenticationService)
        {

            context.Features.Set<IAuthenticationFeature>(new AuthenticationFeature
            {
                OriginalPath = context.Request.Path,
                OriginalPathBase = context.Request.PathBase
            });


            //获取默认Scheme（或者AuthorizeAttribute指定的Scheme）的AuthenticationHandler
            var handlers = context.RequestServices.GetRequiredService<IAuthenticationHandlerProvider>();
            foreach (var scheme in await Schemes.GetRequestHandlerSchemesAsync())
            {
                if (await handlers.GetHandlerAsync(context, scheme.Name) is IAuthenticationRequestHandler handler && await handler.HandleRequestAsync())
                {
                    return;
                }
            }

            //获取默认Scheme
            var defaultAuthenticate = await Schemes.GetDefaultAuthenticateSchemeAsync();
            if (defaultAuthenticate != null)
            {
                var result = await context.AuthenticateAsync(defaultAuthenticate.Name);
                if (result?.Principal != null)
                {
                    context.User = result.Principal;
                }
            }

            //if (!context.Request.Path.Equals(_options.Path, StringComparison.Ordinal))
            //{
            //    await _next(context);
            //    return;
            //}

            //if (!context.Request.Method.Equals("POST") || !context.Request.HasFormContentType)
            //{
            //    await ReturnBadRequest(context);
            //    return;
            //}


            //{[Authorization, {Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNDIwMDEwMDIiLCJlbWFpbCI6IjE1MDIxMTQ0NDYxQGpzZGNtcy5jb20iLCJzaWQiOiIxNzUyIiwianRpIjoiOTM5MTEyODgtY2FiMy00Y2VkLWI0MWQtZjM1ZTI5ODdkY2Q1IiwiZXhwIjoxNTk3MDI2NDc3LCJpc3MiOiJodHRwOi8vYXBpLmpzZGNtcy5jb20iLCJhdWQiOiJodHRwOi8vYXBpLmpzZGNtcy5jb20ifQ.jY4KWCafGJIQa5tGof0aihb3Qmi6qCK_lkm2T-aRZlY}]}
            //var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            //if (token != null)
            //    await AttachUserToContext(context, authenticationService, token);

            await _next(context);

            //await GenerateAuthorizedResult(context);
        }


        ///// <summary>
        ///// 验证结果并得到token
        ///// </summary>
        ///// <param name="context"></param>
        ///// <returns></returns>
        //private async Task GenerateAuthorizedResult(HttpContext context)
        //{
        //    var username = context.Request.Form["username"];
        //    var password = context.Request.Form["password"];

        //    var identity = await GetIdentity(username, password);
        //    if (identity == null)
        //    {
        //        await ReturnBadRequest(context);
        //        return;
        //    }

        //    // Serialize and return the response
        //    context.Response.ContentType = "application/json";
        //    await context.Response.WriteAsync(GetJwt(username));
        //}

        ///// <summary>
        ///// 验证用户
        ///// </summary>
        ///// <param name="username"></param>
        ///// <param name="password"></param>
        ///// <returns></returns>
        //private Task<ClaimsIdentity> GetIdentity(string username, string password)
        //{
        //    var isValidated = _userService.Auth(username, password);
        //    if (isValidated)
        //    {
        //        return Task.FromResult(new ClaimsIdentity(new System.Security.Principal.GenericIdentity(username, "Token"), new Claim[] { }));

        //    }
        //    return Task.FromResult<ClaimsIdentity>(null);
        //}


        /// <summary>
        /// return the bad request (200)
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task ReturnBadRequest(HttpContext context)
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync(JsonConvert.SerializeObject(new
            {
                Status = false,
                Message = "认证失败"
            }));
        }


        //private string GetJwt(string username)
        //{
        //    var now = DateTime.UtcNow;

        //    var claims = new Claim[]
        //    {
        //        new Claim(JwtRegisteredClaimNames.Sub, username),
        //        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        //        new Claim(JwtRegisteredClaimNames.Iat, now.ToUniversalTime().ToString(),
        //                  ClaimValueTypes.Integer64),
        //        //用户名
        //        new Claim(ClaimTypes.Name,username),
        //        //角色
        //        new Claim(ClaimTypes.Role,"a")
        //    };

        //    var jwt = new JwtSecurityToken(
        //        issuer: _options.Issuer,
        //        audience: _options.Audience,
        //        claims: claims,
        //        notBefore: now,
        //        expires: now.Add(_options.Expiration),
        //        signingCredentials: _options.SigningCredentials
        //    );
        //    var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

        //    var response = new
        //    {
        //        Status = true,
        //        access_token = encodedJwt,
        //        expires_in = (int)_options.Expiration.TotalSeconds,
        //        token_type = "Bearer"
        //    };
        //    return JsonConvert.SerializeObject(response, new JsonSerializerSettings { Formatting = Formatting.Indented });
        //}


        private async Task AttachUserToContext(HttpContext context, IAuthenticationService authenticationService, string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                //var key = Encoding.ASCII.GetBytes(_configuration["DCMS:JWT:Secret"]);
                var key = Encoding.UTF8.GetBytes(_configuration["DCMS:JWT:Secret"]);

                //var tokenValidationParameters = new TokenValidationParameters
                //{
                //    ValidIssuer = _configuration["DCMS:JWT:Issuer"],
                //    ValidAudience = _configuration["DCMS:JWT:Issuer"],
                //    ValidateLifetime = true,
                //    RequireExpirationTime = true,
                //    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["DCMS:JWT:Secret"])),
                //    ClockSkew = TimeSpan.Zero
                //};

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidIssuer = _configuration["DCMS:JWT:Issuer"],
                    ValidAudience = _configuration["DCMS:JWT:Issuer"],
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    //ValidateIssuer = true,
                    //ValidateAudience = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                //{{"alg":"HS256","typ":"JWT"}.{"sub":"142001002","email":"15021144461@jsdcms.com","sid":"1752","jti":"93911288-cab3-4ced-b41d-f35e2987dcd5","exp":1597026477,"iss":"http://api.jsdcms.com","aud":"http://api.jsdcms.com"}}
                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = int.Parse(jwtToken.Claims.First(x => x.Type.ToLower() == "sid").Value);
                var user = _userService.GetUserById(userId);
                if (user != null)
                {
                    context.Items["User"] = user;
                    //var result = new ClaimsIdentity(new System.Security.Principal.GenericIdentity(userId.ToString(), "sid"), jwtToken.Claims);
                }
            }
            catch
            {
                //如果jwt验证失败，则不执行任何操作用户未附加到上下文，因此请求将无法访问安全路由
                await ReturnBadRequest(context);
                return;
            }
        }
    }
}