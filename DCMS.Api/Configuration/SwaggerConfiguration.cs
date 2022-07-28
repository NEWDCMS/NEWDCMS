using DCMS.Api.ActionFilters;
using DCMS.Api.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Collections.Generic;
using System.IO;

namespace DCMS.Api.Configuration
{

    public static class SwaggerConfiguration
    {
        /// <summary>
        /// 配置服务
        /// </summary>
        /// <param name="services"></param>
        public static void ConfigureService(IServiceCollection services)
        {
            //services.AddApiVersioning((o) =>
            //{
            //    o.ReportApiVersions = true;//可选配置，设置为true时，header返回版本信息
            //    o.DefaultApiVersion = new ApiVersion(1, 0);//默认版本，请求未指明版本的求默认认执行版本1.0的API
            //    o.AssumeDefaultVersionWhenUnspecified = true;//是否启用未指明版本API，指向默认版本
            //});

            // services.AddVersionedApiExplorer(option =>
            //{
            //    option.GroupNameFormat = "'v'VVVV";//api组名格式
            //    option.AssumeDefaultVersionWhenUnspecified = true;//是否提供API版本服务
            //});

            services.AddApiVersioning(o =>
            {
                o.ReportApiVersions = true;
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(3, 0);
                o.ApiVersionReader = new HeaderApiVersionReader("api-version");
                //o.Conventions.Controller<DevicesController>().HasApiVersion(new ApiVersion(2, 0));
            });


            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v3", new OpenApiInfo
                {
                    Version = "v3.0.1",
                    Title = $"DCMS.Api",
                    Description = "v3 API",
                    TermsOfService = new Uri("https://licenses.jsdcms.com"),
                    Contact = new OpenApiContact
                    {
                        Name = "MSCHEN",
                        Email = "czhcom@163.com",
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Apache-2.0",
                        Url = new Uri("https://www.apache.org/licenses/LICENSE-2.0.html")
                    }
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    }
                });

                //模拟租户
                //c.OperationFilter<TenantHeaderOperationFilter>();

                //c.OperationFilter<LocalizationQueryOperationFilter>();

                c.CustomSchemaIds((type) => type.FullName);

                c.SchemaFilter<SwaggerExcludeFilter>();

                //BaseAPIController
                //"DCMS.Api.xml"
                //var xmlFile = $"{typeof(BaseController<>).Assembly.GetName().Name}.xml";
                var xmlFile = $"{typeof(BaseAPIController).Assembly.GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            services.AddControllers();


        }



        /// <summary>
        /// Configures the specified application.
        /// </summary>
        /// <param name="app">The application.</param>
        public static void Configure(IApplicationBuilder app, string cors)
        {

            // This will redirect default url to Swagger url
            var option = new RewriteOptions();
            option.AddRedirect("^$", "swagger");
            app.UseRewriter(option);

            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //    app.UseDatabaseErrorPage();
            //}
            //else
            //{
                //app.UseExceptionHandler("/Home/Error");
                //app.UseHsts();
            //}

            app.UseSwagger();


            app.UseSwaggerUI(c =>
            {
                //app.UseSwagger(c => { c.RouteTemplate = "swagger/{documentName}/swagger.json"; });
                //app.UseSwaggerUI(c => {
                //    c.SwaggerEndpoint("v1/swagger.json", "API"); c.RoutePrefix = "swagger";
                //    c.DocExpansion(DocExpansion.None);
                //    c.DefaultModelsExpandDepth(-1);
                //});

                c.DocExpansion(DocExpansion.None);

                c.SwaggerEndpoint("/swagger/v3/swagger.json", "DCMS Api v3.0");

                c.DefaultModelsExpandDepth(-1);
            });

            //app.UseHttpsRedirection();
            //app.UseStaticFiles();

            app.UseRouting();

            app.UseCors(cors);

            // Unhandled error catch middleware
            app.Use(async (context, next) =>
            {
                try
                {
                    await next();
                }
                catch (Exception ex)
                {
                    await context.Response.WriteAsync(ex.Message);
                }
            });

            // Middleware gets called when response status code is between 400 - 599
            app.UseStatusCodePages(async statusCodeContext =>
            {
                if (statusCodeContext.HttpContext.Response.StatusCode == 401)
                {
                    await statusCodeContext.HttpContext.Response.WriteAsync("UnAuthorized !!!");
                }

                if (statusCodeContext.HttpContext.Response.StatusCode == 403)
                {
                    await statusCodeContext.HttpContext.Response.WriteAsync("Forbidden !!!");
                }
            });


            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

        }
    }
}