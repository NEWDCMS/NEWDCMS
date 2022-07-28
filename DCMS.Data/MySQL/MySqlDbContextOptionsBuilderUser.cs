
using DCMS.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System;


namespace DCMS.Data
{
	//public class MySqlDbContextOptionsBuilderUser : IDbContextOptionsBuilderUser
	//{
	//    public DatabaseType Type => DatabaseType.MySql;
	//    public DbContextOptionsBuilder Use(DbContextOptionsBuilder builder, string connectionString)
	//    {
	//        if (!builder.IsConfigured)
	//            //An item with the same key has already been added. Key
	//            return builder.UseMySql(connectionString);
	//        else
	//            return builder;
	//    }
	//}

	public class EFCoreLoggerProvider : ILoggerProvider
	{
		public ILogger CreateLogger(string categoryName) => new EFCoreLogger(categoryName);
		public void Dispose() { }
	}

	public class EFCoreLogger : ILogger
	{
		private readonly string categoryName;

		public EFCoreLogger(string categoryName) => this.categoryName = categoryName;

		public bool IsEnabled(LogLevel logLevel) => true;

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{

			Console.WriteLine($"categoryName===========================>:{categoryName}");
			//EF Core执行SQL语句时的categoryName为Microsoft.EntityFrameworkCore.Database.Command,日志级别为Information
			if (categoryName == "Microsoft.EntityFrameworkCore.Database.Command" && logLevel == LogLevel.Information)
			{
				var logContent = formatter(state, exception);
				Console.WriteLine(logContent);
			}
		}

		public IDisposable BeginScope<TState>(TState state) => null;
	}

	public class MySqlDbContextOptionsBuilderUser : IDbContextOptionsBuilderUser
	{
	
		public DatabaseType Type => DatabaseType.MySql;

		public DbContextOptionsBuilder Use(DbContextOptionsBuilder builder, string connectionString)
		{
			if (!builder.IsConfigured)
			{

				var loggerFactory = new LoggerFactory();
				loggerFactory.AddProvider(new EFCoreLoggerProvider());

				//3.1
				//return builder.UseLazyLoadingProxies(false)
				//    //.UseLoggerFactory(loggerFactory)
				//    .UseMySQL(connectionString)
				//    //.EnableSensitiveDataLogging()
				//    .ConfigureWarnings(warnnings => warnnings.Log(CoreEventId.DetachedLazyLoadingWarning));

				//optionsBuilder.ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.RedundantIndexRemoved);

				//6.0
				return builder.UseLazyLoadingProxies(false)
					.UseLoggerFactory(loggerFactory)
					.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 28)))
					//添加EnableSensitiveDataLogging，启用敏感数据记录
					.EnableSensitiveDataLogging()
					.ConfigureWarnings(warnnings => 
					{
						//RedundantIndexRemoved 忽略
						//https://stackoverflow.com/questions/63898245/ef-core-keeps-complaining-about-non-existent-indexes
						warnnings
						.Ignore(CoreEventId.RedundantIndexRemoved)
						.Log(CoreEventId.DetachedLazyLoadingWarning);

					});

			}
			else
			{
				return builder;
			}
		}

	}
}


/*
 ory and Windows DPAPI to encrypt keys at rest.
categoryName===========================>:Microsoft.EntityFrameworkCore.Infrastructure
An 'IServiceProvider' was created for internal use by Entity Framework.
categoryName===========================>:Microsoft.EntityFrameworkCore.Model
No relationship from 'FinanceReceiveAccountBillAccounting' to 'FinanceReceiveAccountBill' has been configured by convention because there are multiple properties on one entity type - {'FinanceReceiveAccountBill'} that could be matched with the properties on the other entity type - {'Accounts', 'FinanceReceiveAccountBillAccountings'}. This message can be disregarded if explicit configuration has been specified in 'OnModelCreating'.
categoryName===========================>:Microsoft.EntityFrameworkCore.Model
No relationship from 'FinanceReceiveAccountBillAccounting' to 'FinanceReceiveAccountBill' has been configured by convention because there are multiple properties on one entity type - {'FinanceReceiveAccountBill'} that could be matched with the properties on the other entity type - {'Accounts', 'FinanceReceiveAccountBillAccountings'}. This message can be disregarded if explicit configuration has been specified in 'OnModelCreating'.
categoryName===========================>:Microsoft.EntityFrameworkCore.Model
No relationship from 'FinanceReceiveAccountBill' to 'FinanceReceiveAccountBillAccounting' has been configured by convention because there are multiple properties on one entity type - {'Accounts', 'FinanceReceiveAccountBillAccountings'} that could be matched with the properties on the other entity type - {'FinanceReceiveAccountBill'}. This message can be disregarded if explicit configuration has been specified in 'OnModelCreating'.
categoryName===========================>:Microsoft.EntityFrameworkCore.Model
The index {'CampaignId'} was not created on entity type 'CampaignChannel' as the properties are already covered by the index {'CampaignId', 'ChannelId'}.
categoryName===========================>:Microsoft.EntityFrameworkCore.Model
The index {'BillId'} was not created on entity type 'AdvancePaymentBillAccounting' as the properties are already covered by the index {'BillId', 'AccountingOptionId'}.
categoryName===========================>:Microsoft.EntityFrameworkCore.Model
The index {'BillId'} was not created on entity type 'AdvanceReceiptBillAccounting' as the properties are already covered by the index {'BillId', 'AccountingOptionId'}.
categoryName===========================>:Microsoft.EntityFrameworkCore.Model
The index {'BillId'} was not created on entity type 'CashReceiptBillAccounting' as the properties are already covered by the index {'BillId', 'AccountingOptionId'}.
categoryName===========================>:Microsoft.EntityFrameworkCore.Model
The index {'BillId'} was not created on entity type 'CostExpenditureBillAccounting' as the properties are already covered by the index {'BillId', 'AccountingOptionId'}.
categoryName===========================>:Microsoft.EntityFrameworkCore.Model
The index {'BillId'} was not created on entity type 'FinancialIncomeBillAccounting' as the properties are already covered by the index {'BillId', 'AccountingOptionId'}.
categoryName===========================>:Microsoft.EntityFrameworkCore.Model
The index {'BillId'} was not created on entity type 'PaymentReceiptBillAccounting' as the properties are already covered by the index {'BillId', 'AccountingOptionId'}.
categoryName===========================>:Microsoft.EntityFrameworkCore.Model
The index {'CategoryId'} was not created on entity type 'ProductCategory' as the properties are already covered by the index {'CategoryId', 'ProductId'}.
categoryName===========================>:Microsoft.EntityFrameworkCore.Model
The index {'ProductId'} was not created on entity type 'ProductManufacturer' as the properties are already covered by the index {'ProductId', 'ManufacturerId'}.
categoryName===========================>:Microsoft.EntityFrameworkCore.Model
The index {'ProductId'} was not created on entity type 'ProductPicture' as the properties are already covered by the index {'ProductId', 'PictureId'}.
categoryName===========================>:Microsoft.EntityFrameworkCore.Model
The index {'BillId'} was not created on entity type 'PurchaseBillAccounting' as the properties are already covered by the index {'BillId', 'AccountingOptionId'}.
categoryName===========================>:Microsoft.EntityFrameworkCore.Model
The index {'BillId'} was not created on entity type 'PurchaseReturnBillAccounting' as the properties are already covered by the index {'BillId', 'AccountingOptionId'}.
categoryName===========================>:Microsoft.EntityFrameworkCore.Model
The index {'BillId'} was not created on entity type 'FinanceReceiveAccountBillAccounting' as the properties are already covered by the index {'BillId', 'AccountingOptionId'}.
categoryName===========================>:Microsoft.EntityFrameworkCore.Model
The index {'BillId'} was not created on entity type 'ReturnBillAccounting' as the properties are already covered by the index {'BillId', 'AccountingOptionId'}.
categoryName===========================>:Microsoft.EntityFrameworkCore.Model
The index {'BillId'} was not created on entity type 'ReturnReservationBillAccounting' as the properties are already covered by the index {'BillId', 'AccountingOptionId'}.
categoryName===========================>:Microsoft.EntityFrameworkCore.Model
The index {'BillId'} was not created on entity type 'SaleBillAccounting' as the properties are already covered by the index {'BillId', 'AccountingOptionId'}.
categoryName===========================>:Microsoft.EntityFrameworkCore.Model
The index {'BillId'} was not created on entity type 'SaleReservationBillAccounting' as the properties are already covered by the index {'BillId', 'AccountingOptionId'}.
categoryName===========================>:Microsoft.EntityFrameworkCore.Model
The index {'Module_Id'} was not created on entity type 'ModuleRole' as the properties are already covered by the index {'Module_Id', 'UserRole_Id'}.
categoryName===========================>:Microsoft.EntityFrameworkCore.Model
The index {'PermissionRecord_Id'} was not created on entity type 'PermissionRecordRoles' as the properties are already covered by the index {'PermissionRecord_Id', 'UserRole_Id', 'Platform'}.
categoryName===========================>:Microsoft.EntityFrameworkCore.Model
The index {'UserGroup_Id'} was not created on entity type 'UserGroupUser' as the properties are already covered by the index {'UserGroup_Id', 'User_Id'}.
categoryName===========================>:Microsoft.EntityFrameworkCore.Model
The index {'UserGroup_Id'} was not created on entity type 'UserGroupUserRole' as the properties are already covered by the index {'UserGroup_Id', 'UserRole_Id'}.
categoryName===========================>:Microsoft.EntityFrameworkCore.Model
The index {'UserId'} was not created on entity type 'UserUserRole' as the properties are already covered by the index {'UserId', 'UserRoleId'}.
categoryName===========================>:Microsoft.EntityFrameworkCore.Model
The index {'StockInOutRecordId'} was not created on entity type 'StockInOutRecordStockFlow' as the properties are already covered by the index {'StockInOutRecordId', 'StockFlowId'}.
categoryName===========================>:Microsoft.EntityFrameworkCore.Model.Validation
Sensitive data logging is enabled. Log entries and exception messages may include sensitive application data; this mode should only be enabled during development.
categoryName===========================>:Microsoft.EntityFrameworkCore.Model.Validation
The property 'PickingItem.PickingBillId' was created in shadow state because there are no eligible CLR members with a matching name.
categoryName===========================>:Microsoft.EntityFrameworkCore.Infrastructure
Entity Framework Core 6.0.2 initialized 'DbContextBase' using provider 'Pomelo.EntityFrameworkCore.MySql:6.0.1' with options: SensitiveDataLoggingEnabled ServerVersion 8.0.28-mysql
 */