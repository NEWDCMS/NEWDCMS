using DCMS.Services.Events;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DCMS.Web.Framework.Events
{

    public static class EventPublisherExtensions
    {
        /// <summary>
        /// 发布ModelPrepared事件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="eventPublisher"></param>
        /// <param name="model"></param>
        public static void ModelPrepared<T>(this IEventPublisher eventPublisher, T model)
        {
            eventPublisher.Publish(new ModelPreparedEvent<T>(model));
        }

        /// <summary>
        /// 发布ModelReceived事件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="eventPublisher"></param>
        /// <param name="model"></param>
        /// <param name="modelState"></param>
        public static void ModelReceived<T>(this IEventPublisher eventPublisher, T model, ModelStateDictionary modelState)
        {
            eventPublisher.Publish(new ModelReceivedEvent<T>(model, modelState));
        }
    }
}