namespace DCMS.Core.Data
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;


    public interface IRepository<TEntity> : IRepositoryReadOnly<TEntity> where TEntity : BaseEntity
    {
        void Insert(TEntity entity);
        void Insert(params TEntity[] entities);
        void Insert(IEnumerable<TEntity> entities);
        Task InsertAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken));
        Task InsertAsync(params TEntity[] entities);
        Task InsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default(CancellationToken));


        void Update(TEntity entity);
        void Update(params TEntity[] entities);
        void Update(IEnumerable<TEntity> entities);
        void Detached(TEntity entity);
        void Delete(object id);
        void Delete(TEntity entity);
        void Delete(params TEntity[] entities);
        void Delete(IEnumerable<TEntity> entities);
        int ExecuteSqlScript(string sql);
    }
}