
using DCMS.Core.Data;
using Microsoft.EntityFrameworkCore;


namespace DCMS.Data
{
    public class SqlServerDbContextOptionsBuilderUser : IDbContextOptionsBuilderUser
    {
        public DatabaseType Type => DatabaseType.SqlServer;

        public DbContextOptionsBuilder Use(DbContextOptionsBuilder builder, string connectionString)
        {
            return builder
                .UseLazyLoadingProxies()
                .UseSqlServer(connectionString);
        }
    }
}
