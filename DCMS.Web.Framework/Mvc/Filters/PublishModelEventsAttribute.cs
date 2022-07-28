using DCMS.Services.Events;
using DCMS.Web.Framework.Events;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace DCMS.Web.Framework.Mvc.Filters
{
    /// <summary>
    /// 表示在操作执行之前、模型绑定完成之后以及在操作执行之后、在操作结果之前发布ModelPrepared事件的筛选器属性。
    /// </summary>
    public class PublishModelEventsAttribute : TypeFilterAttribute
    {
        #region Fields

        private readonly bool _ignoreFilter;

        #endregion

        #region Ctor

        /// <summary>
        /// Create instance of the filter attribute
        /// </summary>
        /// <param name="ignore">Whether to ignore the execution of filter actions</param>
        public PublishModelEventsAttribute(bool ignore = false) : base(typeof(PublishModelEventsFilter))
        {
            _ignoreFilter = ignore;
            Arguments = new object[] { ignore };
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether to ignore the execution of filter actions
        /// </summary>
        public bool IgnoreFilter => _ignoreFilter;

        #endregion

        #region Nested filter

        /// <summary>
        /// Represents filter that publish ModelReceived event before the action executes, after model binding is complete
        /// and publish ModelPrepared event after the action executes, before the action result
        /// </summary>
        private class PublishModelEventsFilter : IActionFilter
        {
            #region Fields

            private readonly bool _ignoreFilter;
            private readonly IEventPublisher _eventPublisher;

            #endregion

            #region Ctor

            public PublishModelEventsFilter(bool ignoreFilter,
                IEventPublisher eventPublisher)
            {
                _ignoreFilter = ignoreFilter;
                _eventPublisher = eventPublisher;
            }

            #endregion

            #region Methods

            /// <summary>
            /// Called before the action executes, after model binding is complete
            /// </summary>
            /// <param name="context">A context for action filters</param>
            public void OnActionExecuting(ActionExecutingContext context)
            {
                if (context == null)
                {
                    throw new ArgumentNullException(nameof(context));
                }

                //check whether this filter has been overridden for the Action
                var actionFilter = context.ActionDescriptor.FilterDescriptors
                    .Where(filterDescriptor => filterDescriptor.Scope == FilterScope.Action)
                    .Select(filterDescriptor => filterDescriptor.Filter).OfType<PublishModelEventsAttribute>().FirstOrDefault();

                //whether to ignore this filter
                if (actionFilter?.IgnoreFilter ?? _ignoreFilter)
                {
                    return;
                }

                if (context.HttpContext.Request == null)
                {
                    return;
                }

                //only in POST requests
                if (!context.HttpContext.Request.Method.Equals(WebRequestMethods.Http.Post, StringComparison.InvariantCultureIgnoreCase))
                {
                    return;
                }

                //model received event
                foreach (var model in context.ActionArguments.Values.OfType<BaseModel>())
                {
                    //we publish the ModelReceived event for all models as the BaseModel, 
                    //so you need to implement IConsumer<ModelReceived<BaseModel>> interface to handle this event
                    _eventPublisher.ModelReceived(model, context.ModelState);
                }
            }

            /// <summary>
            /// Called after the action executes, before the action result
            /// </summary>
            /// <param name="context">A context for action filters</param>
            public void OnActionExecuted(ActionExecutedContext context)
            {
                if (context == null)
                {
                    throw new ArgumentNullException(nameof(context));
                }

                //check whether this filter has been overridden for the Action
                var actionFilter = context.ActionDescriptor.FilterDescriptors
                    .Where(filterDescriptor => filterDescriptor.Scope == FilterScope.Action)
                    .Select(filterDescriptor => filterDescriptor.Filter).OfType<PublishModelEventsAttribute>().FirstOrDefault();

                //whether to ignore this filter
                if (actionFilter?.IgnoreFilter ?? _ignoreFilter)
                {
                    return;
                }

                if (context.HttpContext.Request == null)
                {
                    return;
                }

                //model prepared event
                if (context.Controller is Controller controller)
                {
                    if (controller.ViewData.Model is BaseModel model)
                    {
                        //we publish the ModelPrepared event for all models as the BaseModel, 
                        //so you need to implement IConsumer<ModelPrepared<BaseModel>> interface to handle this event
                        _eventPublisher.ModelPrepared(model);
                    }

                    if (controller.ViewData.Model is IEnumerable<BaseModel> modelCollection)
                    {
                        //we publish the ModelPrepared event for collection as the IEnumerable<BaseModel>, 
                        //so you need to implement IConsumer<ModelPrepared<IEnumerable<BaseModel>>> interface to handle this event
                        _eventPublisher.ModelPrepared(modelCollection);
                    }
                }
            }

            #endregion
        }

        #endregion
    }
}