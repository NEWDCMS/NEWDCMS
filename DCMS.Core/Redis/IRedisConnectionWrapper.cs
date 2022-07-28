using StackExchange.Redis;
using System;
using System.Net;
using System.Threading.Tasks;

namespace DCMS.Core.Redis
{
    /// <summary>
    /// 表示redis连接包装器
    /// </summary>
    public interface IRedisConnectionWrapper : IDisposable
    {

        IDatabase GetDatabase(int db);
        string GetConnectionString();
        bool Connected();

        IServer GetServer(EndPoint endPoint);

        EndPoint[] GetEndPoints();

        void FlushDatabase(RedisDatabaseNumber db);

        bool PerformActionWithLock(string resource, TimeSpan expirationTime, Action action);
        Task<bool> PerformActionWithLockAsync(string resource, TimeSpan expirationTime, Func<bool> action);
        bool PerformActionWithLock(string resource, TimeSpan expirationTime, TimeSpan wait, TimeSpan retry, Func<bool> action);
        Task<BaseResult> PerformActionWithLockAsync(string resource, TimeSpan expirationTime, TimeSpan wait, TimeSpan retry, Func<BaseResult> action);
        Task<bool> PerformActionWithLockAsync(string resource, TimeSpan expirationTime, TimeSpan wait, TimeSpan retry, Func<bool> action);
    }
}
