using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;



namespace DCMS.Api.ActionFilters
{
    /// <summary>
    /// 添加租户Id字段到API终结点
    /// </summary>
    /// <seealso cref="Swashbuckle.AspNetCore.SwaggerGen.IOperationFilter" />
    public class TenantHeaderOperationFilter : IOperationFilter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="context"></param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();

            //if (context.ApiDescription.ActionDescriptor is ControllerActionDescriptor descriptor && !descriptor.ControllerName.StartsWith(""))
            //{
            //    operation.Parameters.Add(new OpenApiParameter()
            //    {
            //        Name = "sign",
            //        In = ParameterLocation.Query,
            //        Description = "The signature",
            //        Required = true
            //    });
            //}


            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Partner",
                In = ParameterLocation.Query,
                Description = "合作者账户",
                Required = true,
                Example = new OpenApiString("testapi")
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "PartnerKey",
                In = ParameterLocation.Query,
                Description = "合作者账户接入身份验证密钥",
                Required = false,
                Example = new OpenApiString("OFBCX55ApISiL0XbGn2doQJ3yxwX57P7rilCA5wKiiE=")
            });


        }

    }
}
