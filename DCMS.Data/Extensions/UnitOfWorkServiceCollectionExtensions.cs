using DCMS.Core;
using DCMS.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;


namespace DCMS.Data.Extensions
{

    public static class UnitOfWorkServiceCollectionExtensions
    {

        public static IServiceCollection AddUnitOfWork<TContext>(this IServiceCollection services)
            where TContext : DbContext
        {
            //services.AddScoped<IRepositoryFactory, UnitOfWork<TContext>>();
            //services.AddScoped<IUnitOfWork, UnitOfWork<TContext>>();
            //services.AddScoped<IUnitOfWork<TContext>, UnitOfWork<TContext>>();
            return services;
        }


        public static IServiceCollection AddUnitOfWork<TContext1, TContext2>(this IServiceCollection services)
            where TContext1 : DbContext
            where TContext2 : DbContext
        {
            //services.AddScoped<IUnitOfWork<TContext1>, UnitOfWork<TContext1>>();
            //services.AddScoped<IUnitOfWork<TContext2>, UnitOfWork<TContext2>>();

            return services;
        }


        public static IServiceCollection AddUnitOfWork<TContext1, TContext2, TContext3>(this IServiceCollection services)
            where TContext1 : DbContext
            where TContext2 : DbContext
            where TContext3 : DbContext
        {
            //services.AddScoped<IUnitOfWork<TContext1>, UnitOfWork<TContext1>>();
            //services.AddScoped<IUnitOfWork<TContext2>, UnitOfWork<TContext2>>();
            //services.AddScoped<IUnitOfWork<TContext3>, UnitOfWork<TContext3>>();

            return services;
        }


        public static IServiceCollection AddUnitOfWork<TContext1, TContext2, TContext3, TContext4>(this IServiceCollection services)
            where TContext1 : DbContext
            where TContext2 : DbContext
            where TContext3 : DbContext
            where TContext4 : DbContext
        {
            //services.AddScoped<IUnitOfWork<TContext1>, UnitOfWork<TContext1>>();
            //services.AddScoped<IUnitOfWork<TContext2>, UnitOfWork<TContext2>>();
            //services.AddScoped<IUnitOfWork<TContext3>, UnitOfWork<TContext3>>();
            //services.AddScoped<IUnitOfWork<TContext4>, UnitOfWork<TContext4>>();

            return services;
        }


        public static IServiceCollection AddCustomRepository<TEntity, TRepository>(this IServiceCollection services)
            where TEntity : BaseEntity
            where TRepository : BaseEntity, IRepository<TEntity>
        {
            services.AddScoped<IRepository<TEntity>, TRepository>();
            return services;
        }

        public static IServiceCollection AddCustomRepositoryReadOnly<TEntity, TRepository>(this IServiceCollection services)
       where TEntity : BaseEntity
       where TRepository : BaseEntity, IRepositoryReadOnly<TEntity>
        {
            services.AddScoped<IRepositoryReadOnly<TEntity>, TRepository>();
            return services;
        }

        public static IServiceCollection AddRepository<TEntity>(this IServiceCollection services) where TEntity : BaseEntity
        {
            services.AddScoped<IRepository<TEntity>, Repository<TEntity>>();
            services.AddScoped<IRepositoryReadOnly<TEntity>, RepositoryReadOnly<TEntity>>();
            return services;
        }

    }
}
