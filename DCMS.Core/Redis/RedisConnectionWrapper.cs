using DCMS.Core.Caching;
using DCMS.Core.Configuration;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;


namespace DCMS.Core.Redis
{
    /// <summary>
    /// 表示redis连接包装器实现
    /// </summary>
    public class RedisConnectionWrapper : IRedisConnectionWrapper, IRedLocker
    {
        #region Fields

        private readonly DCMSConfig _config;

        private readonly object _lock = new object();
        private volatile ConnectionMultiplexer _connection;
        private readonly Lazy<string> _connectionString;
        private volatile RedLockFactory _redisLockFactory;

        #endregion

        #region Ctor

        public RedisConnectionWrapper(DCMSConfig config)
        {
            _config = config;
            _connectionString = new Lazy<string>(GetConnectionString);
            _redisLockFactory = CreateRedisLockFactory();
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Get connection string to Redis cache from configuration
        /// </summary>
        /// <returns></returns>
        public string GetConnectionString()
        {
            return _config.RedisConnectionString;
        }

        /// <summary>
        /// Get connection to Redis servers
        /// </summary>
        /// <returns></returns>
        protected ConnectionMultiplexer GetConnection()
        {
            if (_connection != null && _connection.IsConnected)
            {
                return _connection;
            }

            lock (_lock)
            {
                if (_connection != null && _connection.IsConnected)
                {
                    return _connection;
                }

                //Connection disconnected. Disposing connection...
                _connection?.Dispose();

                //It was not possible to connect to the redis server(s); to create a disconnected multiplexer, disable AbortOnConnectFail. 
                //Creating new instance of Redis Connection
                _connection = ConnectionMultiplexer.Connect(_connectionString.Value);
            }

            return _connection;
        }


        public bool Connected()
        {
            if (_connection != null && _connection.IsConnected)
            {
                return true;
            }
            else
            {
                return false;
            }
        }



        /// <summary>
        /// Create instance of RedLock factory
        /// </summary>
        /// <returns>RedLock factory</returns>
        protected RedLockFactory CreateRedisLockFactory()
        {
            //get RedLock endpoints
            var configurationOptions = ConfigurationOptions.Parse(_connectionString.Value);
            var redLockEndPoints = GetEndPoints().Select(endPoint => new RedLockEndPoint
            {
                EndPoint = endPoint,
                Password = configurationOptions.Password,
                Ssl = configurationOptions.Ssl,
                RedisDatabase = configurationOptions.DefaultDatabase,
                ConfigCheckSeconds = configurationOptions.ConfigCheckSeconds,
                ConnectionTimeout = configurationOptions.ConnectTimeout,
                SyncTimeout = configurationOptions.SyncTimeout
            }).ToList();

            //create RedLock factory to use RedLock distributed lock algorithm
            return RedLockFactory.Create(redLockEndPoints);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Obtain an interactive connection to a database inside Redis
        /// </summary>
        /// <param name="db">Database number</param>
        /// <returns>Redis cache database</returns>
        public IDatabase GetDatabase(int db)
        {
            return GetConnection().GetDatabase(db);
        }

        /// <summary>
        /// Obtain a configuration API for an individual server
        /// </summary>
        /// <param name="endPoint">The network endpoint</param>
        /// <returns>Redis server</returns>
        public IServer GetServer(EndPoint endPoint)
        {
            return GetConnection().GetServer(endPoint);
        }

        /// <summary>
        /// Gets all endpoints defined on the server
        /// </summary>
        /// <returns>Array of endpoints</returns>
        public EndPoint[] GetEndPoints()
        {
            return GetConnection().GetEndPoints();
        }

        /// <summary>
        /// Delete all the keys of the database
        /// </summary>
        /// <param name="db">Database number</param>
        public void FlushDatabase(RedisDatabaseNumber db)
        {
            var endPoints = GetEndPoints();

            foreach (var endPoint in endPoints)
            {
                GetServer(endPoint).FlushDatabase((int)db);
            }
        }

        /// <summary>
        /// 使用独占锁执行某些操作
        /// </summary>
        /// <param name="resource">The thing we are locking on</param>
        /// <param name="expirationTime">Redis自动将锁过期的时间</param>
        /// <param name="action">Action to be performed with locking</param>
        /// <returns>如果获取了锁并执行了操作，则为True；否则为false</returns>
        public bool PerformActionWithLock(string resource, TimeSpan expirationTime, Action action)
        {
            //use RedLock library
            using (var redisLock = _redisLockFactory.CreateLock(resource, expirationTime))
            {
                //ensure that lock is acquired
                if (!redisLock.IsAcquired)
                {
                    return false;
                }

                //perform action
                action();

                return true;
            }// the lock is automatically released at the end of the using block
        }
        public async Task<bool> PerformActionWithLockAsync(string resource, TimeSpan expirationTime, Func<bool> action)
        {
            //use RedLock library
            using (var redisLock = await _redisLockFactory.CreateLockAsync(resource, expirationTime))
            {
                //ensure that lock is acquired
                if (!redisLock.IsAcquired)
                {
                    return false;
                }

                //perform action
                return action();

                //return true;
            }// the lock is automatically released at the end of the using block
        }

        /*
         var resource = "the-thing-we-are-locking-on";
            var expiry = TimeSpan.FromSeconds(30);
            var wait = TimeSpan.FromSeconds(10);
            var retry = TimeSpan.FromSeconds(1);

            // blocks until acquired or 'wait' timeout
            using (var redLock = await redlockFactory.CreateLockAsync(resource, expiry, wait, retry)) // there are also non async Create() methods
            {
	            // make sure we got the lock
	            if (redLock.IsAcquired)
	            {
		            // do stuff
	            }
            }
        */


        public bool PerformActionWithLock(string resource, TimeSpan expirationTime, TimeSpan wait, TimeSpan retry, Func<bool> action)
        {
            //use RedLock library
            using (var redisLock = _redisLockFactory.CreateLock(resource, expirationTime, wait, retry))
            {
                //ensure that lock is acquired
                if (!redisLock.IsAcquired)
                {
                    return false;
                }

                //perform action
                return action();
            }// the lock is automatically released at the end of the using block
        }
        public async Task<BaseResult> PerformActionWithLockAsync(string resource, TimeSpan expirationTime, TimeSpan wait, TimeSpan retry, Func<BaseResult> action)
        {
            var result = new BaseResult() { Success = false, Message = "" };

            //use RedLock library CancellationToken? cancellationToken = null
            using (var redisLock = await _redisLockFactory.CreateLockAsync(resource, expirationTime, wait, retry))
            {
                //ensure that lock is acquired
                if (!redisLock.IsAcquired)
                {
                    return result;
                }

                //perform action
                return action();

                //return true;
            }// the lock is automatically released at the end of the using block
        }

        public async Task<bool> PerformActionWithLockAsync(string resource, TimeSpan expirationTime, TimeSpan wait, TimeSpan retry, Func<bool> action)
        {


            //use RedLock library CancellationToken? cancellationToken = null
            using (var redisLock = await _redisLockFactory.CreateLockAsync(resource, expirationTime, wait, retry))
            {
                //ensure that lock is acquired
                if (!redisLock.IsAcquired)
                {
                    return false;
                }

                //perform action
                return action();

                //return true;
            }// the lock is automatically released at the end of the using block
        }

        /// <summary>
        /// Release all resources associated with this object
        /// </summary>
        public void Dispose()
        {
            //dispose ConnectionMultiplexer
            _connection?.Dispose();

            //dispose RedLock factory
            _redisLockFactory?.Dispose();
        }

        #endregion
    }
}