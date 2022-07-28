using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DCMS.Web.Framework.Infrastructure.Extensions
{
    /// <summary>
    /// Represents extensions of DbContextOptionsBuilder
    /// </summary>
    public static class DbContextOptionsBuilderExtensions
    {
        /// <summary>
        /// 用于 SQL Server专用扩展方法 Microsoft.EntityFrameworkCore.DbContextOptionsBuilder
        /// </summary>
        /// <param name="optionsBuilder">Database context options builder</param>
        /// <param name="services">Collection of service descriptors</param>
        //public static void UseSqlServerWithLazyLoading(this DbContextOptionsBuilder optionsBuilder, IServiceCollection services)
        //{
        //    var dcmsConfig = services.BuildServiceProvider().GetRequiredService<DCMSConfig>();

        //    var dataSettings = DataSettingsManager.LoadSettings();
        //    if (!dataSettings?.IsValid ?? true)
        //        return;

        //    var dbContextOptionsBuilder = optionsBuilder.UseLazyLoadingProxies();

        //    if (dcmsConfig.UseRowNumberForPaging)
        //        dbContextOptionsBuilder.UseSqlServer(dataSettings.DataConnectionString, option => option.UseRowNumberForPaging());
        //    else
        //        dbContextOptionsBuilder.UseSqlServer(dataSettings.DataConnectionString);
        //}


        public static void UseMySqlServerWithLazyLoading(this DbContextOptionsBuilder optionsBuilder, IServiceCollection services)
        {
            //var dcmsConfig = services.BuildServiceProvider().GetRequiredService<DCMSConfig>();
            //var dataSettings = DataSettingsManager.LoadSettings();
            //if (dataSettings.DbConnections.Count == 0)
            //    return;
            //var dbContextOptionsBuilder = optionsBuilder.UseLazyLoadingProxies();
            //dbContextOptionsBuilder.UseSqlServer(dataSettings.DataConnectionString);
            //var services = new ServiceCollection()
            //    .AddSingleton<IConfigurationRoot>(config)
            //    .AddSingleton<IConfiguration>(config)
            //    .AddAlasFx()
            //    .AddAlasFxDatabase()
            //    .AddAlasFxSqlServer()
            //    .AddDbBuilderOptions<TestDbContext>(null, builder =>
            //    {
            //    });

            //fileProvider = fileProvider ?? CommonHelper.DefaultFileProvider;
            //filePath = filePath ?? fileProvider.MapPath(DCMSDataSettingsDefaults.FilePath);


            //var config = new ConfigurationBuilder()
            //  .AddJsonFile("appsettings.json")
            //  .Build();

            //services.AddSingleton<IConfigurationRoot>(config)
            //    .AddSingleton<IConfiguration>(config)
            //    .AddDCMS()
            //    .AddDCMSDatabase()
            //    .AddDCMSMySql()
            //    .AddDbBuilderOptions<AUTHObjectContext>(null, builder => { })
            //    .AddDbBuilderOptions<DCMSObjectContext>(null, builder => { })
            //    .AddDbBuilderOptions<CensusObjectContext>(null, builder => { })
            //    .AddDbBuilderOptions<SKDObjectContext>(null, builder => { });

        }

    }
}
