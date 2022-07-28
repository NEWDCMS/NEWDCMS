using DCMS.Core.Infrastructure;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace DCMS.Web.Framework.Mvc.ModelBinding
{
    /// <summary>
    /// Represents model binder provider for the creating DCMSModelBinder
    /// </summary>
    public class DCMSModelBinderProvider : IModelBinderProvider
    {
        /// <summary>
        /// Creates a dcms model binder based on passed context
        /// </summary>
        /// <param name="context">Model binder provider context</param>
        /// <returns>Model binder</returns>
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var modelType = context.Metadata.ModelType;
            if (!typeof(BaseModel).IsAssignableFrom(modelType))
            {
                return null;
            }

            //use DCMSModelBinder as a ComplexTypeModelBinder for BaseModel
            if (context.Metadata.IsComplexType && !context.Metadata.IsCollectionType)
            {
                //create binders for all model properties
                var propertyBinders = context.Metadata.Properties
                    .ToDictionary(modelProperty => modelProperty, modelProperty => context.CreateBinder(modelProperty));

                return new DCMSModelBinder(propertyBinders, EngineContext.Current.Resolve<ILoggerFactory>());
            }

            //or return null to further search for a suitable binder
            return null;
        }
    }
}
