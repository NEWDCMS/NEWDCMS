using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DCMS.Web.Framework.Events
{
    /// <summary>
    /// 表示从视图接收模型后发生的事件
    /// </summary>
    /// <typeparam name="T">Type of the model</typeparam>
    public class ModelReceivedEvent<T>
    {

        public ModelReceivedEvent(T model, ModelStateDictionary modelState)
        {
            Model = model;
            ModelState = modelState;
        }



        /// <summary>
        /// 获取Model
        /// </summary>
        public T Model { get; private set; }

        /// <summary>
        /// 获取Model状态
        /// </summary>
        public ModelStateDictionary ModelState { get; private set; }


    }
}
