using System;
using System.Threading;

namespace DCMS.Core.ComponentModel
{
    /// <summary>
    /// 为实现对资源的锁定访问提供了一种方便的方法
    /// </summary>
    public class ReaderWriteLockDisposable : IDisposable
    {
        private bool _disposed = false;
        private readonly ReaderWriterLockSlim _rwLock;
        private readonly ReaderWriteLockType _readerWriteLockType;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReaderWriteLockDisposable"/> class.
        /// </summary>
        /// <param name="rwLock">The readers–writer lock</param>
        /// <param name="readerWriteLockType">Lock type</param>
        public ReaderWriteLockDisposable(ReaderWriterLockSlim rwLock, ReaderWriteLockType readerWriteLockType = ReaderWriteLockType.Write)
        {
            _rwLock = rwLock;
            _readerWriteLockType = readerWriteLockType;

            switch (_readerWriteLockType)
            {
                case ReaderWriteLockType.Read:
                    _rwLock.EnterReadLock();
                    break;
                case ReaderWriteLockType.Write:
                    _rwLock.EnterWriteLock();
                    break;
                case ReaderWriteLockType.UpgradeableRead:
                    _rwLock.EnterUpgradeableReadLock();
                    break;
            }
        }

        /// <summary>
        /// 可由使用者调用的Dispose模式的公共实现
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose模式的受保护实现
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                switch (_readerWriteLockType)
                {
                    case ReaderWriteLockType.Read:
                        _rwLock.ExitReadLock();
                        break;
                    case ReaderWriteLockType.Write:
                        _rwLock.ExitWriteLock();
                        break;
                    case ReaderWriteLockType.UpgradeableRead:
                        _rwLock.ExitUpgradeableReadLock();
                        break;
                }
            }
            _disposed = true;
        }
    }
}
