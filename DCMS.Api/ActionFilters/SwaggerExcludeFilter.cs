using DCMS.Api.Helpers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Linq;
using static System.Char;

namespace DCMS.Api.ActionFilters
{
    /// <summary>
    /// 从请求忽略属性
    /// </summary>
    public class SwaggerExcludeFilter : ISchemaFilter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="context"></param>
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema.Properties == null)
            {
                return;
            }

            foreach (var property in schema.Properties)
            {
                if (property.Value.Default != null && property.Value.Example == null)
                {
                    property.Value.Example = property.Value.Default;
                }
            }

            var excludeProperties = context.Type?.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(SwaggerExcludeAttribute)));
            if (excludeProperties != null)
            {
                foreach (var property in excludeProperties)
                {
                    // Because swagger uses camel casing
                    var propertyName = $"{ToLowerInvariant(property.Name[0])}{property.Name.Substring(1)}";
                    if (schema.Properties.ContainsKey(propertyName))
                    {
                        schema.Properties.Remove(propertyName);
                    }
                }
            }
        }

    }
}
