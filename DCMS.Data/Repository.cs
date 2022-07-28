using DCMS.Core;
using DCMS.Core.Data;
using DCMS.Data.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace DCMS.Data
{
    public class Repository<TEntity> : IRepository<TEntity>, IRepositoryReadOnly<TEntity> where TEntity : BaseEntity
    {
        protected readonly DbContext _dbContext;
        protected readonly DbSet<TEntity> _dbSet;
        protected readonly IUnitOfWork _unitOfWork;


        public Repository(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _dbContext = unitOfWork.DbContext;
            _dbSet = _dbContext.Set<TEntity>();
        }

        public DbContext DbContext
        {
            get
            {
                if (_dbContext == null)
                {
                    throw new Exception("无法解析数据库上下文实例，UnitOfWork 已被释放");
                }
                //_dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
                return _dbContext;
            }
        }


        public IUnitOfWork UnitOfWork
        {
            get
            {
                if (_unitOfWork == null)
                {
                    throw new Exception("无法解析实例，因为它已被释放.请确保 RepositoryFactory 注册为 Scoped");
                }
                return _unitOfWork;
            }
        }


        ///// <summary>
        ///// 通过添加传递的参数修改输入sql查询
        ///// </summary>
        ///// <param name="sql">The raw SQL query</param>
        ///// <param name="parameters">The values to be assigned to parameters</param>
        ///// <returns>Modified raw SQL query</returns>
        //protected virtual string CreateSqlWithParameters(string sql, params object[] parameters)
        //{
        //    //add parameters to sql
        //    for (var i = 0; i <= (parameters?.Length ?? 0) - 1; i++)
        //    {
        //        if (!(parameters[i] is DbParameter parameter))
        //            continue;

        //        sql = $"{sql}{(i > 0 ? "," : string.Empty)} @{parameter.ParameterName}";

        //        //whether parameter is output
        //        if (parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Output)
        //            sql = $"{sql} output";
        //    }

        //    return sql;
        //}


        //public virtual void ChangeTable(string table)
        //{
        //    if (_dbContext.Model.FindEntityType(typeof(TEntity)).Relational() is RelationalEntityTypeAnnotations relational)
        //    {
        //        relational.TableName = table;
        //    }
        //}

        #region RO

        /// <summary>
        /// Gets all entities. This method is not recommended
        /// </summary>
        /// <returns>The <see cref="IQueryable{TEntity}"/>.</returns>
        [Obsolete("This method is not recommended, please use GetPagedList or GetPagedListAsync methods")]
        public IQueryable<TEntity> GetAll()
        {
            return _dbSet;
        }


        /// <summary>
        /// Gets the <see cref="IPagedList{TEntity}"/> based on a predicate, orderby delegate and page information. This method default no-tracking query.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="orderBy">A function to order elements.</param>
        /// <param name="include">A function to include navigation properties</param>
        /// <param name="pageIndex">The index of page.</param>
        /// <param name="pageSize">The size of the page.</param>
        /// <param name="disableTracking"><c>True</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
        /// <param name="ignoreQueryFilters">Ignore query filters</param>
        /// <returns>An <see cref="IPagedList{TEntity}"/> that contains elements that satisfy the condition specified by <paramref name="predicate"/>.</returns>
        /// <remarks>This method default no-tracking query.</remarks>
        public virtual IPagedList<TEntity> GetPagedList(Expression<Func<TEntity, bool>> predicate = null,
                                                Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
                                                Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
                                                int pageIndex = 0,
                                                int pageSize = 20,
                                                bool disableTracking = true,
                                                bool ignoreQueryFilters = false)
        {
            IQueryable<TEntity> query = _dbSet;

            if (disableTracking)
            {
                query = query.AsNoTracking();
            }

            if (include != null)
            {
                query = include(query);
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (ignoreQueryFilters)
            {
                query = query.IgnoreQueryFilters();
            }

            if (orderBy != null)
            {
                return orderBy(query).ToPagedList(pageIndex, pageSize);
            }
            else
            {
                return query.ToPagedList(pageIndex, pageSize);
            }
        }

        /// <summary>
        /// Gets the <see cref="IPagedList{TEntity}"/> based on a predicate, orderby delegate and page information. This method default no-tracking query.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="orderBy">A function to order elements.</param>
        /// <param name="include">A function to include navigation properties</param>
        /// <param name="pageIndex">The index of page.</param>
        /// <param name="pageSize">The size of the page.</param>
        /// <param name="disableTracking"><c>True</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <param name="ignoreQueryFilters">Ignore query filters</param>
        /// <returns>An <see cref="IPagedList{TEntity}"/> that contains elements that satisfy the condition specified by <paramref name="predicate"/>.</returns>
        /// <remarks>This method default no-tracking query.</remarks>
        public virtual Task<IPagedList<TEntity>> GetPagedListAsync(Expression<Func<TEntity, bool>> predicate = null,
                                                           Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
                                                           Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
                                                           int pageIndex = 0,
                                                           int pageSize = 20,
                                                           bool disableTracking = true,
                                                           CancellationToken cancellationToken = default(CancellationToken),
                                                           bool ignoreQueryFilters = false)
        {
            IQueryable<TEntity> query = _dbSet;

            if (disableTracking)
            {
                query = query.AsNoTracking();
            }

            if (include != null)
            {
                query = include(query);
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (ignoreQueryFilters)
            {
                query = query.IgnoreQueryFilters();
            }

            if (orderBy != null)
            {
                return orderBy(query).ToPagedListAsync(pageIndex, pageSize, 0, cancellationToken);
            }
            else
            {
                return query.ToPagedListAsync(pageIndex, pageSize, 0, cancellationToken);
            }
        }

        /// <summary>
        /// Gets the <see cref="IPagedList{TResult}"/> based on a predicate, orderby delegate and page information. This method default no-tracking query.
        /// </summary>
        /// <param name="selector">The selector for projection.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="orderBy">A function to order elements.</param>
        /// <param name="include">A function to include navigation properties</param>
        /// <param name="pageIndex">The index of page.</param>
        /// <param name="pageSize">The size of the page.</param>
        /// <param name="disableTracking"><c>True</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
        /// <param name="ignoreQueryFilters">Ignore query filters</param>
        /// <returns>An <see cref="IPagedList{TResult}"/> that contains elements that satisfy the condition specified by <paramref name="predicate"/>.</returns>
        /// <remarks>This method default no-tracking query.</remarks>
        public virtual IPagedList<TResult> GetPagedList<TResult>(Expression<Func<TEntity, TResult>> selector,
                                                         Expression<Func<TEntity, bool>> predicate = null,
                                                         Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
                                                         Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
                                                         int pageIndex = 0,
                                                         int pageSize = 20,
                                                         bool disableTracking = true,
                                                         bool ignoreQueryFilters = false)
            where TResult : class
        {
            IQueryable<TEntity> query = _dbSet;

            if (disableTracking)
            {
                query = query.AsNoTracking();
            }

            if (include != null)
            {
                query = include(query);
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (ignoreQueryFilters)
            {
                query = query.IgnoreQueryFilters();
            }

            if (orderBy != null)
            {
                return orderBy(query).Select(selector).ToPagedList(pageIndex, pageSize);
            }
            else
            {
                return query.Select(selector).ToPagedList(pageIndex, pageSize);
            }
        }

        /// <summary>
        /// Gets the <see cref="IPagedList{TEntity}"/> based on a predicate, orderby delegate and page information. This method default no-tracking query.
        /// </summary>
        /// <param name="selector">The selector for projection.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="orderBy">A function to order elements.</param>
        /// <param name="include">A function to include navigation properties</param>
        /// <param name="pageIndex">The index of page.</param>
        /// <param name="pageSize">The size of the page.</param>
        /// <param name="disableTracking"><c>True</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <param name="ignoreQueryFilters">Ignore query filters</param>
        /// <returns>An <see cref="IPagedList{TEntity}"/> that contains elements that satisfy the condition specified by <paramref name="predicate"/>.</returns>
        /// <remarks>This method default no-tracking query.</remarks>
        public virtual Task<IPagedList<TResult>> GetPagedListAsync<TResult>(Expression<Func<TEntity, TResult>> selector,
                                                                    Expression<Func<TEntity, bool>> predicate = null,
                                                                    Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
                                                                    Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
                                                                    int pageIndex = 0,
                                                                    int pageSize = 20,
                                                                    bool disableTracking = true,
                                                                    CancellationToken cancellationToken = default(CancellationToken),
                                                                    bool ignoreQueryFilters = false)
            where TResult : class
        {
            IQueryable<TEntity> query = _dbSet;

            if (disableTracking)
            {
                query = query.AsNoTracking();
            }

            if (include != null)
            {
                query = include(query);
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (ignoreQueryFilters)
            {
                query = query.IgnoreQueryFilters();
            }

            if (orderBy != null)
            {
                return orderBy(query).Select(selector).ToPagedListAsync(pageIndex, pageSize, 0, cancellationToken);
            }
            else
            {
                return query.Select(selector).ToPagedListAsync(pageIndex, pageSize, 0, cancellationToken);
            }
        }

        /// <summary>
        /// Gets the first or default entity based on a predicate, orderby delegate and include delegate. This method default no-tracking query.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="orderBy">A function to order elements.</param>
        /// <param name="include">A function to include navigation properties</param>
        /// <param name="disableTracking"><c>True</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
        /// <param name="ignoreQueryFilters">Ignore query filters</param>
        /// <returns>An <see cref="IPagedList{TEntity}"/> that contains elements that satisfy the condition specified by <paramref name="predicate"/>.</returns>
        /// <remarks>This method default no-tracking query.</remarks>
        public virtual TEntity GetFirstOrDefault(Expression<Func<TEntity, bool>> predicate = null,
                                         Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
                                         Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
                                         bool disableTracking = true,
                                         bool ignoreQueryFilters = false)
        {
            IQueryable<TEntity> query = _dbSet;

            if (disableTracking)
            {
                query = query.AsNoTracking();
            }

            if (include != null)
            {
                query = include(query);
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (ignoreQueryFilters)
            {
                query = query.IgnoreQueryFilters();
            }

            if (orderBy != null)
            {
                return orderBy(query).FirstOrDefault();
            }
            else
            {
                return query.FirstOrDefault();
            }
        }


        /// <inheritdoc />
        public virtual async Task<TEntity> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            bool disableTracking = true,
            bool ignoreQueryFilters = false)
        {
            IQueryable<TEntity> query = _dbSet;

            if (disableTracking)
            {
                query = query.AsNoTracking();
            }

            if (include != null)
            {
                query = include(query);
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (ignoreQueryFilters)
            {
                query = query.IgnoreQueryFilters();
            }

            if (orderBy != null)
            {
                return await orderBy(query).FirstOrDefaultAsync();
            }
            else
            {
                return await query.FirstOrDefaultAsync();
            }
        }

        /// <summary>
        /// Gets the first or default entity based on a predicate, orderby delegate and include delegate. This method default no-tracking query.
        /// </summary>
        /// <param name="selector">The selector for projection.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="orderBy">A function to order elements.</param>
        /// <param name="include">A function to include navigation properties</param>
        /// <param name="disableTracking"><c>True</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
        /// <param name="ignoreQueryFilters">Ignore query filters</param>
        /// <returns>An <see cref="IPagedList{TEntity}"/> that contains elements that satisfy the condition specified by <paramref name="predicate"/>.</returns>
        /// <remarks>This method default no-tracking query.</remarks>
        public virtual TResult GetFirstOrDefault<TResult>(Expression<Func<TEntity, TResult>> selector,
                                                  Expression<Func<TEntity, bool>> predicate = null,
                                                  Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
                                                  Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
                                                  bool disableTracking = true,
                                                  bool ignoreQueryFilters = false)
        {
            IQueryable<TEntity> query = _dbSet;

            if (disableTracking)
            {
                query = query.AsNoTracking();
            }

            if (include != null)
            {
                query = include(query);
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (ignoreQueryFilters)
            {
                query = query.IgnoreQueryFilters();
            }

            if (orderBy != null)
            {
                return orderBy(query).Select(selector).FirstOrDefault();
            }
            else
            {
                return query.Select(selector).FirstOrDefault();
            }
        }

        /// <inheritdoc />
        public virtual async Task<TResult> GetFirstOrDefaultAsync<TResult>(Expression<Func<TEntity, TResult>> selector,
                                                  Expression<Func<TEntity, bool>> predicate = null,
                                                  Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
                                                  Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
                                                  bool disableTracking = true, bool ignoreQueryFilters = false)
        {
            IQueryable<TEntity> query = _dbSet;

            if (disableTracking)
            {
                query = query.AsNoTracking();
            }

            if (include != null)
            {
                query = include(query);
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (ignoreQueryFilters)
            {
                query = query.IgnoreQueryFilters();
            }

            if (orderBy != null)
            {
                return await orderBy(query).Select(selector).FirstOrDefaultAsync();
            }
            else
            {
                return await query.Select(selector).FirstOrDefaultAsync();
            }
        }

        ///// <summary>
        ///// Uses raw SQL queries to fetch the specified <typeparamref name="TEntity" /> data.
        ///// </summary>
        ///// <param name="sql">The raw SQL.</param>
        ///// <param name="parameters">The parameters.</param>
        ///// <returns>An <see cref="IQueryable{TEntity}" /> that contains elements that satisfy the condition specified by raw SQL.</returns>
        //public virtual IQueryable<TEntity> EntityFromSql(string sql, params object[] parameters)
        //{
        //    return _dbSet.FromSql<TEntity>(sql, parameters);
        //}


        public IQueryable<T> EntityFromSql<T>(string sql, params object[] parameters) where T : BaseEntity
        {
            //return _dbContext.Set<T>().FromSql(sql, parameters).AsNoTracking();
            return _dbContext.AsDbContext().Set<T>().FromSqlRaw(sql, parameters).AsNoTracking();
        }


        ///// <summary>
        ///// 创建LINQ查询基于原始SQL查询为查询类型
        ///// </summary>
        ///// <typeparam name="TEntity"></typeparam>
        ///// <param name="sql"></param>
        ///// <param name="parameters"></param>
        ///// <returns></returns>
        //public IQueryable<T> EntityFromSql<T>(string sql, params object[] parameters) where T : BaseEntity
        //{
        //    //3.0 FromSqlRaw
        //    return _context.AsDbContext().Set<T>().FromSqlRaw(CreateSqlWithParameters(sql, parameters), parameters);
        //}

        /// <summary>
        /// 创建LINQ查询基于原始SQL查询为查询类型
        /// Cannot create a DbSet for 'IntQueryType' because it is a query type. Use the DbContext.Query method to create a DbQuery instead.”
        /// </summary>
        /// <typeparam name="TQuery"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IQueryable<TQuery> QueryFromSql<TQuery>(string sql, params object[] parameters) where TQuery : class
        {
            //:“Cannot create a DbQuery for 'xx' because it is not a query type. Use the DbContext.Set method to create a DbSet instead.”
            //return _dbContext.Query<TQuery>().FromSql(sql, parameters).AsNoTracking();
            return _dbContext.AsDbContext().Set<TQuery>().FromSqlRaw(sql, parameters).IgnoreQueryFilters();
        }

        /// <summary>
        /// Finds an entity with the given primary key values. If found, is attached to the context and returned. If no entity is found, then null is returned.
        /// </summary>
        /// <param name="keyValues">The values of the primary key for the entity to be found.</param>
        /// <returns>The found entity or null.</returns>
        public virtual TEntity Find(params object[] keyValues) => _dbSet.Find(keyValues);

        /// <summary>
        /// Finds an entity with the given primary key values. If found, is attached to the context and returned. If no entity is found, then null is returned.
        /// </summary>
        /// <param name="keyValues">The values of the primary key for the entity to be found.</param>
        /// <returns>A <see cref="Task{TEntity}" /> that represents the asynchronous insert operation.</returns>
        public virtual async Task<TEntity> FindAsync(params object[] keyValues) => await _dbSet.FindAsync(keyValues);

        /// <summary>
        /// Finds an entity with the given primary key values. If found, is attached to the context and returned. If no entity is found, then null is returned.
        /// </summary>
        /// <param name="keyValues">The values of the primary key for the entity to be found.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task{TEntity}"/> that represents the asynchronous find operation. The task result contains the found entity or null.</returns>
        public virtual async Task<TEntity> FindAsync(object[] keyValues, CancellationToken cancellationToken) => await _dbSet.FindAsync(keyValues, cancellationToken);

        /// <summary>
        /// Gets the count based on a predicate.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public virtual int Count(Expression<Func<TEntity, bool>> predicate = null)
        {
            if (predicate == null)
            {
                return _dbSet.Count();
            }
            else
            {
                return _dbSet.Count(predicate);
            }
        }


        public virtual IQueryable<TEntity> Table => _dbSet;

        /// <summary>
        /// 不跟踪是上下文实体当只为只读操作加载记录时才使用该表
        /// </summary>
        public virtual IQueryable<TEntity> TableNoTracking => _dbSet.AsNoTracking();

        public TEntity GetById(object id)
        {
            return _dbSet.Find(id);
        }

        #endregion

        #region RW


        public virtual void Insert(TEntity entity)
        {
            _dbSet.Add(entity);
            //if (_unitOfWork != null)
            //    _unitOfWork.SaveChanges();
        }
        public virtual void Insert(params TEntity[] entities)
        {
            _dbSet.AddRange(entities);
            //if (_unitOfWork != null)
            //    _unitOfWork.SaveChanges();
        }
        public virtual void Insert(IEnumerable<TEntity> entities)
        {
            _dbSet.AddRange(entities);
            //if (_unitOfWork != null)
            //    _unitOfWork.SaveChanges();
        }
        public virtual Task InsertAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken))
        {
            _dbSet.AddAsync(entity, cancellationToken);

            //if (_unitOfWork != null)
            //    return _unitOfWork.SaveChangesAsync();
            //else
            //    return Task.FromResult(false);

            return Task.FromResult(false);
            // Shadow properties?
            //var property = _dbContext.Entry(entity).Property("Created");
            //if (property != null) {
            //property.CurrentValue = DateTime.Now;
            //}
        }
        public virtual Task InsertAsync(params TEntity[] entities)
        {

            _dbSet.AddRangeAsync(entities);
            //if (_unitOfWork != null)
            //    return _unitOfWork.SaveChangesAsync();
            //else
            //    return Task.FromResult(false);
            return Task.FromResult(false);
        }
        public virtual Task InsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default(CancellationToken))
        {
            _dbSet.AddRangeAsync(entities, cancellationToken);
            //if (_unitOfWork != null)
            //    return _unitOfWork.SaveChangesAsync();
            //else
            //    return Task.FromResult(false);
            return Task.FromResult(false);
        }


        public virtual void Update(TEntity entity)
        {
            _dbSet.Update(entity);
            //if (_unitOfWork != null)
            //    _unitOfWork.SaveChanges();
        }
        public virtual void UpdateAsync(TEntity entity)
        {
            _dbSet.Update(entity);
            //if (_unitOfWork != null)
            //    _unitOfWork.SaveChangesAsync();

        }
        public virtual void Update(params TEntity[] entities)
        {
            _dbSet.UpdateRange(entities);
            //if (_unitOfWork != null)
            //    _unitOfWork.SaveChanges();
        }
        public virtual void Update(IEnumerable<TEntity> entities)
        {

            _dbSet.UpdateRange(entities);
            //if (_unitOfWork != null)
            //    _unitOfWork.SaveChanges();
        }
      
        public virtual void Detached(TEntity entity)
        {
            _dbContext.Entry(entity).State = EntityState.Detached;
        }

        public virtual void Delete(TEntity entity)
        {
            _dbSet.Remove(entity);
            //if (_unitOfWork != null)
            //    _unitOfWork.SaveChanges();
        }
        public virtual void Delete(object id)
        {
            // using a stub entity to mark for deletion
            var typeInfo = typeof(TEntity).GetTypeInfo();
            var key = _dbContext.Model.FindEntityType(typeInfo).FindPrimaryKey().Properties.FirstOrDefault();
            var property = typeInfo.GetProperty(key?.Name);
            if (property != null)
            {
                var entity = Activator.CreateInstance<TEntity>();
                property.SetValue(entity, id);
                _dbContext.Entry(entity).State = EntityState.Deleted;
            }
            else
            {
                var entity = _dbSet.Find(id);
                if (entity != null)
                {
                    Delete(entity);
                }
            }

            //if (_unitOfWork != null)
            //    _unitOfWork.SaveChanges();
        }
        public virtual void Delete(params TEntity[] entities)
        {
            _dbSet.RemoveRange(entities);
            //if (_unitOfWork != null)
            //    _unitOfWork.SaveChanges();
        }
        public virtual void Delete(IEnumerable<TEntity> entities)
        {

            _dbSet.RemoveRange(entities);
            //if (_unitOfWork != null)
            //    _unitOfWork.SaveChanges();
        }


        public virtual int ExecuteSqlScript(string sql)
        {
            if (_dbContext == null)
            {
                throw new ArgumentNullException(nameof(DbContext));
            }

            return _dbContext.ExecuteSqlCommand(sql);
        }

        #endregion
    }
}
