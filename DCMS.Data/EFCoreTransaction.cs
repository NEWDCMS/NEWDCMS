using DCMS.Core.Data;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace DCMS.Data
{
    /// <summary>
    /// 事务
    /// </summary>
    public class EFCoreTransaction : ITransaction
    {
        private DbTransaction _dbTransaction;
        private IDbContextTransaction _dbContextTransaction;

        public EFCoreTransaction(DbTransaction dbTransaction)
        {
            _dbTransaction = dbTransaction;
        }

        public EFCoreTransaction(IDbContextTransaction dbContextTransaction)
        {
            _dbContextTransaction = dbContextTransaction;
        }

        public void Commit()
        {
            _dbContextTransaction?.Commit();
            _dbTransaction?.Commit();
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            var tokenSource = new CancellationTokenSource();
            await Task.Run(() =>
            {
                _dbContextTransaction?.Commit();
                _dbTransaction?.Commit();
            }, tokenSource.Token);

            if (cancellationToken.CanBeCanceled)
            {
                //发出取消的请求
                tokenSource.Cancel();
            }
        }

        public void Rollback()
        {
            _dbContextTransaction?.Rollback();
            _dbTransaction?.Rollback();
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            var tokenSource = new CancellationTokenSource();
            await Task.Run(() =>
            {
                _dbContextTransaction?.Rollback();
                _dbTransaction?.Rollback();
            }, tokenSource.Token);

            if (cancellationToken.CanBeCanceled)
            {
                //发出取消的请求
                tokenSource.Cancel();
            }
        }

        public void Dispose()
        {
            _dbContextTransaction?.Dispose();
            _dbTransaction?.Dispose();
        }
    }
}
