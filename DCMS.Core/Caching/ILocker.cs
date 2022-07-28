
using System;
using System.Threading.Tasks;

namespace DCMS.Core.Caching
{

    //[Obsolete("新版已由IRedLocker替代")]
    //public interface ILocker
    //{
    //    /// <summary>
    //    /// 使用独占锁执行某些操作
    //    /// </summary>
    //    /// <param name="resource"></param>
    //    /// <param name="expirationTime"></param>
    //    /// <param name="action"></param>
    //    /// <returns></returns>
    //    bool PerformActionWithLock(string resource, TimeSpan expirationTime, Action action);
    //    bool PerformActionWithLock(string resource, TimeSpan expirationTime, TimeSpan wait, TimeSpan retry, Action action);

    //}


    public interface IRedLocker
    {
        /// <summary>
        /// 使用独占锁执行某些操作
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="expirationTime"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        bool PerformActionWithLock(string resource, TimeSpan expirationTime, Action action);
        bool PerformActionWithLock(string resource, TimeSpan expirationTime, TimeSpan wait, TimeSpan retry, Func<bool> action);
        Task<bool> PerformActionWithLockAsync(string key, TimeSpan expirationTime, Func<bool> action);
        Task<bool> PerformActionWithLockAsync(string key, TimeSpan expirationTime, TimeSpan wait, TimeSpan retry, Func<bool> action);
        Task<BaseResult> PerformActionWithLockAsync(string key, TimeSpan expirationTime, TimeSpan wait, TimeSpan retry, Func<BaseResult> action);
    }

}
