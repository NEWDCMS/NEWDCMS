using DCMS.Core.Infrastructure;
using DCMS.Services.Events;
using DCMS.Web.Framework.Events;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using System.Collections.Generic;

namespace DCMS.Web.Framework.Components
{
    /// <summary>
    /// 视图组件基类
    /// </summary>
    public abstract class DCMSViewComponent : ViewComponent
    {
        private void PublishModelPrepared<TModel>(TModel model)
        {
            //组件不是控制器生命周期的一部分。
            //因此，我们不能再使用动作过滤器拦截返回的模型
            //正如我们在控制器的/DCMS.Web.Framework/Mvc/Filters/PublishModelEventsAttribute.cs 中所做的那样
            if (model is BaseModel)
            {
                var eventPublisher = EngineContext.Current.Resolve<IEventPublisher>();
                //我们将所有模型的ModelPrepared事件发布为BaseModel，
                //您需要实现IConsumer<ModelPrepared<BaseModel>>接口来处理此事件
                eventPublisher.ModelPrepared(model as BaseModel);
            }

            if (model is IEnumerable<BaseModel> modelCollection)
            {
                var eventPublisher = EngineContext.Current.Resolve<IEventPublisher>();

                //我们将集合的ModelPrepared事件发布为IEnumerable<BaseModel>，
                //需要实现IConsumer<ModelPrepared<IEnumerable<BaseModel>>接口来处理此事件
                eventPublisher.ModelPrepared(modelCollection);
            }
        }

        /// <summary>
        /// 返回一个结果，该结果将呈现名为<paramref name="viewName"/>.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="viewName"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public new ViewViewComponentResult View<TModel>(string viewName, TModel model)
        {
            PublishModelPrepared(model);
            //调用基方法
            return base.View<TModel>(viewName, model);
        }

        /// <summary>
        /// 返回将呈现部分视图的结果
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public new ViewViewComponentResult View<TModel>(TModel model)
        {
            PublishModelPrepared(model);
            return base.View<TModel>(model);
        }

        /// <summary>
        /// 返回一个结果，该结果将呈现名为view name的部分视图
        /// </summary>
        /// <param name="viewName"></param>
        /// <returns></returns>
        public new ViewViewComponentResult View(string viewName)
        {
            return base.View(viewName);
        }
    }
}