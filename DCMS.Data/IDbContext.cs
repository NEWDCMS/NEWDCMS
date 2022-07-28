using System.Linq;
using Microsoft.EntityFrameworkCore;
using DCMS.Core;
using System;
using System.Threading;
using System.Threading.Tasks;


namespace DCMS.Data
{
    /// <summary>
    /// 数据上下文接口(不支持读写分离) v1.0
    /// </summary>
    public partial interface IDbContext : IDisposable
    {
        #region Methods

        /// <summary>
        /// 创建可用于查询和保存实体实例的 DbSet
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        DbSet<TEntity> Set<TEntity>() where TEntity : BaseEntity;

        /// <summary>
        /// 保存数据变更
        /// </summary>
        /// <returns></returns>
        int SaveChanges();

        /// <summary>
        /// 异步保存数据变更
        /// </summary>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        Task<int> SaveChangesAsync(CancellationToken cancelToken = default);

        /// <summary>
        /// 从当前模型的所有表生成创建脚本
        /// </summary>
        /// <returns></returns>
        string GenerateCreateScript();

        /// <summary>
        /// 创建LINQ查询基于原始SQL查询为查询类型
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        IQueryable<TEntity> EntityFromSql<TEntity>(string sql, params object[] parameters) where TEntity : BaseEntity;

        /// <summary>
        /// 创建LINQ查询基于原始SQL查询为查询类型
        /// </summary>
        /// <typeparam name="TQuery"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        IQueryable<TQuery> QueryFromSql<TQuery>(string sql, params object[] parameters) where TQuery : class;


        /// <summary>
        /// 上下文中实体的详细信息
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        void Detach<TEntity>(TEntity entity) where TEntity : BaseEntity;


        #endregion
    }
}