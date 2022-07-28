
namespace DCMS.Web.Framework.Events
{
    /// <summary>
    /// 表示模型准备好供查看后发生的事件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ModelPreparedEvent<T>
    {

        public ModelPreparedEvent(T model)
        {
            Model = model;
        }

        /// <summary>
        /// 获取Model
        /// </summary>
        public T Model { get; private set; }


    }
}
