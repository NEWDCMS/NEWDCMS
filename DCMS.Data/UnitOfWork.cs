using DCMS.Core;
using DCMS.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;


namespace DCMS.Data
{
    /// <summary>
    /// UnitOfWork默认实现类
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {

        //private Dictionary<Type, object> repositories;
        private bool disposed = false;
        private readonly IServiceProvider _serviceProvider;
        private readonly DbContext _context;
        private readonly IRepositoryFactory _repositoryFactory;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="context"></param>
        public UnitOfWork(IServiceProvider serviceProvider, DbContext context)
        {
            _serviceProvider = serviceProvider;
            _context = context;
            _repositoryFactory = serviceProvider.GetRequiredService<IRepositoryFactory>();
        }


        public DbContext DbContext
        {
            get
            {
                if (_context == null)
                {
                    throw new Exception("无法解析数据库上下文实例，UnitOfWork 已被释放");
                }
                //_context.ChangeTracker.AutoDetectChangesEnabled = false;
                return _context;
            }
        }


        //public void ChangeDatabase(string database, string port)
        //{
        //    var connection = _context.Database.GetDbConnection();
        //    if (connection.State.HasFlag(ConnectionState.Open))
        //    {
        //        connection.ChangeDatabase(database);
        //        var connectionString = Regex.Replace(connection.ConnectionString.Replace(" ", ""), @"(?<=[Pp]ort=)\w+(?=;)", port, RegexOptions.Singleline);
        //        connection.ConnectionString = connectionString;
        //    }
        //    else
        //    {
        //        //"Server=192.168.1.14;User Id=root;Password=racing.1;Port=3310;Persist Security Info=True;Database=skd;Convert Zero Datetime=True;Treat Tiny As Boolean=true;AllowUserVariables=True;Use Affected Rows=False"
        //        var connectionString = Regex.Replace(connection.ConnectionString.Replace(" ", ""), @"(?<=[Dd]atabase=)\w+(?=;)", database, RegexOptions.Singleline);
        //        connectionString = Regex.Replace(connection.ConnectionString.Replace(" ", ""), @"(?<=[Pp]ort=)\w+(?=;)", port, RegexOptions.Singleline);
        //        connection.ConnectionString = connectionString;
        //    }

        //    // Following code only working for mysql.
        //    var items = _context.Model.GetEntityTypes();
        //    foreach (var item in items)
        //    {
        //        if (item.Relational() is RelationalEntityTypeAnnotations extensions)
        //        {
        //            extensions.Schema = database;
        //        }
        //    }
        //}

        public IRepository<TEntity> GetRepository<TEntity>(bool hasCustomRepository = false) where TEntity : BaseEntity
        {
            return _repositoryFactory.GetRepository<TEntity>(this);
        }

        public IRepositoryReadOnly<TEntity> GetReadOnlyRepository<TEntity>(bool hasCustomRepository = false) where TEntity : BaseEntity
        {
            return _repositoryFactory.GetReadOnlyRepository<TEntity>(this);
        }


        public ITransaction BeginOrUseTransaction(System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.ReadCommitted, DbTransaction transaction = null)
        {
            if (_context == null)
            {
                return null;
            }

            if (transaction != null)
            {
                return new EFCoreTransaction(_context.Database.UseTransaction(transaction));
            }
            else
            {
                return new EFCoreTransaction(_context.Database.BeginTransaction(isolationLevel));
            }
        }

        //public async Task<ITransaction> BeginOrUseTransactionAsync(System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.ReadCommitted, DbTransaction transaction = null, CancellationToken cancelToken = default)
        //{
        //    if (_context == null)
        //        return null;

        //    if (transaction != null)
        //    {
        //        return new EFCoreTransaction(await _context.Database.UseTransactionAsync(transaction, cancelToken));
        //    }
        //    else
        //    {
        //        return new EFCoreTransaction(await _context.Database.BeginTransactionAsync(isolationLevel, cancelToken));
        //    }
        //}

        /// <summary>
        /// Executes the specified raw SQL command.
        /// "For the execution of SQL queries using plain strings, use ExecuteSqlRaw instead. For the execution of SQL queries using interpolated string syntax to create parameters, use ExecuteSqlInterpolated instead."
        /// </summary>
        /// <param name="sql">The raw SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The number of state entities written to database.</returns>
        public int ExecuteSqlCommand(string sql, params object[] parameters) => _context.Database.ExecuteSqlRaw(sql, parameters);

        /// <summary>
        /// Uses raw SQL queries to fetch the specified <typeparamref name="TEntity" /> data.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="sql">The raw SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>An <see cref="IQueryable{T}" /> that contains elements that satisfy the condition specified by raw SQL.</returns>
        public IQueryable<TEntity> FromSql<TEntity>(string sql, params object[] parameters) where TEntity : BaseEntity => _context.Set<TEntity>().FromSqlRaw(sql, parameters);

        /// <summary>
        /// Saves all changes made in this context to the database.
        /// </summary>
        /// <param name="ensureAutoHistory"><c>True</c> if save changes ensure auto record the change history.</param>
        /// <returns>The number of state entries written to the database.</returns>
        public int SaveChanges(bool ensureAutoHistory = false)
        {
            try
            {
                //Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AutoDetectChangesEnabled
                //如果插入频率过高可能导致 Duplicate entry 'xx' for key 'PRIMARY'问题
                if (ensureAutoHistory)
                {
                    _context.EnsureAutoHistory();
                }

                if (_context == null)
                {
                    throw new Exception($"严重错误，DbContext 在其它地方被释放,请检查你的程序逻辑。");
                }

                return _context.SaveChanges();

                //return 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"对上下文中所做的所有更改保存到数据库时出错:{ex.InnerException?.Message} : {ex.Message} : {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Asynchronously saves all changes made in this unit of work to the database.
        /// </summary>
        /// <param name="ensureAutoHistory"><c>True</c> if save changes ensure auto record the change history.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous save operation. The task result contains the number of state entities written to database.</returns>
        public async Task<int> SaveChangesAsync(bool ensureAutoHistory = false)
        {
            if (ensureAutoHistory)
            {
                _context.EnsureAutoHistory();
            }
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Saves all changes made in this context to the database with distributed transaction.
        /// </summary>
        /// <param name="ensureAutoHistory"><c>True</c> if save changes ensure auto record the change history.</param>
        /// <param name="unitOfWorks">An optional <see cref="IUnitOfWork"/> array.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous save operation. The task result contains the number of state entities written to database.</returns>
        public async Task<int> SaveChangesAsync(bool ensureAutoHistory = false, params IUnitOfWork[] unitOfWorks)
        {
            using (var ts = new TransactionScope())
            {
                var count = 0;
                foreach (var unitOfWork in unitOfWorks)
                {
                    count += await unitOfWork.SaveChangesAsync(ensureAutoHistory);
                }

                count += await SaveChangesAsync(ensureAutoHistory);

                ts.Complete();

                return count;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">The disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // clear repositories
                    //if (repositories != null)
                    //{
                    //    repositories.Clear();
                    //}

                    // dispose the db context.
                    _context?.Dispose();
                }
            }
            disposed = true;
        }

        public void TrackGraph(object rootEntity, Action<EntityEntryGraphNode> callback)
        {
            _context.ChangeTracker.TrackGraph(rootEntity, callback);
        }

        public void CleanChanges()
        {
            var entries = _context.ChangeTracker.Entries().ToArray();
            foreach (var entry in entries)
            {
                entry.State = EntityState.Detached;
            }
        }

        public void CleanChanges<TEntity>() where TEntity : class
        {
            var entries = _context.ChangeTracker.Entries<TEntity>().ToArray();
            foreach (var entry in entries)
            {
                entry.State = EntityState.Detached;
            }
        }
    }
}
