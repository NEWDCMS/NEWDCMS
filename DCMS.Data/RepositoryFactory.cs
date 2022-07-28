using DCMS.Core;
using DCMS.Core.Data;


namespace DCMS.Data
{
    public class RepositoryFactory : IRepositoryFactory
    {
        public IRepository<TEntity> GetRepository<TEntity>(IUnitOfWork unitOfWork) where TEntity : BaseEntity
        {
            return new Repository<TEntity>(unitOfWork);
        }

        public IRepositoryReadOnly<TEntity> GetReadOnlyRepository<TEntity>(IUnitOfWork unitOfWork) where TEntity : BaseEntity
        {
            return new RepositoryReadOnly<TEntity>(unitOfWork);
        }
    }
}
