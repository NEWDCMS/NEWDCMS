using System;
using System.Threading;
using System.Threading.Tasks;

namespace DCMS.Core.Data
{
    /// <summary>
    /// 数据库事务接口
    /// </summary>
    public interface ITransaction : IDisposable
    {
        /// <summary>
        /// 保存事务
        /// </summary>
        void Commit();

        /// <summary>
        /// 回滚事务
        /// </summary>
        void Rollback();

        /// <summary>
        /// 保存事务
        /// </summary>
        Task CommitAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 回滚事务
        /// </summary>
        Task RollbackAsync(CancellationToken cancellationToken = default);
    }
}
