namespace DCMS.Core.Data
{

    public interface IRepositoryFactory
    {
        IRepository<TEntity> GetRepository<TEntity>(IUnitOfWork unitOfWork) where TEntity : BaseEntity;

        IRepositoryReadOnly<TEntity> GetReadOnlyRepository<TEntity>(IUnitOfWork unitOfWork) where TEntity : BaseEntity;
    }

}