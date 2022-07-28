using DCMS.Core;
using DCMS.Core.Data;
using DCMS.Core.Infrastructure.DependencyManagement;

namespace DCMS.Data
{
    public class ServiceGetter : IServiceGetter
    {
        private readonly IDbProvider _dbProvider;


        public ServiceGetter(IDbProvider dbProvider)
        {
            _dbProvider = dbProvider;
        }

        public IUnitOfWork UOW(string dbName)
        {
            return _dbProvider.GetUnitOfWork<DbContextBase>($"{dbName}_RW");
        }

        public IRepository<T> RW<T>(string dbName) where T : BaseEntity
        {
            return UOW(dbName).GetRepository<T>();
        }

        public IRepositoryReadOnly<T> RO<T>(string dbName) where T : BaseEntity
        {
            return _dbProvider.GetUnitOfWork<DbContextBase>($"{dbName}_RO").GetReadOnlyRepository<T>();
        }
    }
}
