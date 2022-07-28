using Microsoft.EntityFrameworkCore;

namespace DCMS.Core.Data
{
    /// <summary>
    /// 配置DbContextOptionsBuilder的数据库连接
    /// </summary>
    public interface IDbContextConnectionConfigure
    {
        DbContextOptionsBuilder Configure(DbContextOptionsBuilder builder);
    }
}