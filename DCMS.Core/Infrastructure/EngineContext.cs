using System.Runtime.CompilerServices;

namespace DCMS.Core.Infrastructure
{
    /// <summary>
    /// 提供对DCMS引擎的单例实例的访问
    /// </summary>
    public class EngineContext
    {
        #region Methods

        /// <summary>
        /// 创建静态DCMS实例引擎
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static IEngine Create()
        {
            return Singleton<IEngine>.Instance ?? (Singleton<IEngine>.Instance = new DCMSEngine());
        }

        /// <summary>
        /// 将静态引擎实例设置为提供的引擎。使用此方法提供替换您自己的引擎实现
        /// </summary>
        /// <param name="engine"></param>
        public static void Replace(IEngine engine)
        {
            Singleton<IEngine>.Instance = engine;
        }


        public static string GetStaticResourceServer
        {
            get
            {
                return ConfigurationManager.AppSettings["StaticResourceServer"];
            }
        }

        public static string GetSendMessageUrl
        {
            get
            {
                return ConfigurationManager.AppSettings["SendMessageUrl"];
            }
        }

        public static string GetSendMessageUId
        {
            get
            {
                return ConfigurationManager.AppSettings["SendMessageUId"];
            }
        }

        public static string GetSendMessagePassWord
        {
            get
            {
                return ConfigurationManager.AppSettings["SendMessagePassWord"];
            }
        }

        public static string GetWebApiServer 
        {
            get 
            {
                return ConfigurationManager.AppSettings["WebApiServer"];
            }
        }
        #endregion

        #region Properties

        /// <summary>
        /// 获取用于访问DCMS服务的单例DCMS引擎
        /// </summary>
        public static IEngine Current
        {
            get
            {
                if (Singleton<IEngine>.Instance == null)
                {
                    Create();
                }

                return Singleton<IEngine>.Instance;
            }
        }

        #endregion
    }
}
