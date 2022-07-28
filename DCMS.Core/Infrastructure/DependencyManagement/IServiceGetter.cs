using DCMS.Core.Data;

namespace DCMS.Core.Infrastructure.DependencyManagement
{
    public interface IServiceGetter
    {
        IUnitOfWork UOW(string dbName);
        IRepository<T> RW<T>(string dbName) where T : BaseEntity;
        IRepositoryReadOnly<T> RO<T>(string dbName) where T : BaseEntity;
    }

}
