using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;

namespace DCMS.Web.Framework.Mvc.Filters
{
    /// <summary>
    /// Represents a filter attribute that check existence of passed form key and return result as an action parameter 
    /// </summary>
    public class ParameterBasedOnFormNameAttribute : TypeFilterAttribute
    {
        #region Ctor


        public ParameterBasedOnFormNameAttribute(string formKeyName, string actionParameterName) : base(typeof(ParameterBasedOnFormNameFilter))
        {
            Arguments = new object[] { formKeyName, actionParameterName };
        }

        #endregion

        #region Nested filter


        private class ParameterBasedOnFormNameFilter : IActionFilter
        {
            #region Fields

            private readonly string _formKeyName;
            private readonly string _actionParameterName;

            #endregion

            #region Ctor

            public ParameterBasedOnFormNameFilter(string formKeyName, string actionParameterName)
            {
                _formKeyName = formKeyName;
                _actionParameterName = actionParameterName;
            }

            #endregion

            #region Methods

            public void OnActionExecuting(ActionExecutingContext context)
            {
                if (context == null)
                {
                    throw new ArgumentNullException(nameof(context));
                }

                if (context.HttpContext.Request == null)
                {
                    return;
                }

                //if form key with '_formKeyName' exists, then set specified '_actionParameterName' to true
                context.ActionArguments[_actionParameterName] = context.HttpContext.Request.Form.Keys.Any(key => key.Equals(_formKeyName));

                //we check whether form key with '_formKeyName' exists only
                //uncomment the code below if you want to check whether form value is specified
                //context.ActionArguments[_actionParameterName] = !string.IsNullOrEmpty(context.HttpContext.Request.Form[_formKeyName]);
            }

            /// <summary>
            /// Called after the action executes, before the action result
            /// </summary>
            /// <param name="context">A context for action filters</param>
            public void OnActionExecuted(ActionExecutedContext context)
            {
                //do nothing
            }

            #endregion
        }

        #endregion
    }
}