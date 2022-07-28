using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Linq;

namespace DCMS.Web.Framework.Mvc.ModelBinding
{
    /// <summary>
    /// ModelState 扩展
    /// </summary>
    public static class ModelStateExtensions
    {
        //2.2
        //private static Dictionary<string, object> SerializeModelState(ModelStateEntry modelState)
        //{
        //    var errors = new List<string>();
        //    for (var i = 0; i < modelState.Errors.Count; i++)
        //    {
        //        var modelError = modelState.Errors[i];
        //        var errorText = ValidationHelpers.GetModelErrorMessageOrDefault(modelError);

        //        if (!string.IsNullOrEmpty(errorText))
        //        {
        //            errors.Add(errorText);
        //        }
        //    }

        //    var dictionary = new Dictionary<string, object>
        //    {
        //        ["errors"] = errors.ToArray()
        //    };
        //    return dictionary;
        //}
        //public static object SerializeErrors(this ModelStateDictionary modelStateDictionary)
        //{
        //    return modelStateDictionary.Where(entry => entry.Value.Errors.Any())
        //        .ToDictionary(entry => entry.Key, entry => SerializeModelState(entry.Value));
        //}

        //3.1
        private static Dictionary<string, object> SerializeModelState(ModelStateEntry modelState)
        {
            var errors = new List<string>();
            for (var i = 0; i < modelState.Errors.Count; i++)
            {
                var modelError = modelState.Errors[i];

                if (!string.IsNullOrEmpty(modelError.ErrorMessage))
                {
                    errors.Add(modelError.ErrorMessage);
                }
            }

            var dictionary = new Dictionary<string, object>
            {
                ["errors"] = errors.ToArray()
            };
            return dictionary;
        }

        public static object SerializeErrors(this ModelStateDictionary modelStateDictionary)
        {
            return modelStateDictionary.Where(entry => entry.Value.Errors.Any())
                .ToDictionary(entry => entry.Key, entry => SerializeModelState(entry.Value));
        }
    }
}