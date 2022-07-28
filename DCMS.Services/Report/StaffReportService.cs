using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Common;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Report;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Configuration;
using DCMS.Services.Events;
using DCMS.Services.Products;
using DCMS.Services.Sales;
using DCMS.Services.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCMS.Services.Report
{
	/// <summary>
	/// 员工报表
	/// </summary>
	public class StaffReportService : BaseService, IStaffReportService
	{
		#region 构造

		private readonly IUserService _userService;
		private readonly ISaleBillService _saleBillService;
		private readonly IReturnBillService _returnBillService;
		private readonly IProductService _productService;
		private readonly ISettingService _settingService;
		private readonly ICategoryService _categoryService;

		public StaffReportService(IServiceGetter getter,
			IStaticCacheManager cacheManager,
			IUserService userService,
			ISaleBillService saleBillService,
			IReturnBillService returnBillService,
			IProductService productService,
			ISettingService settingService,
			IEventPublisher eventPublisher,
			ICategoryService categoryService
			) : base(getter, cacheManager, eventPublisher)
		{
			_userService = userService;
			_saleBillService = saleBillService;
			_returnBillService = returnBillService;
			_productService = productService;
			_settingService = settingService;
			_categoryService = categoryService;
		}

		#endregion


		/// <summary>
		/// 业务员业绩
		/// </summary>
		/// <param name="storeId">经销商Id</param>
		/// <param name="categoryId">商品类别Id</param>
		/// <param name="wareHouseId">仓库Id</param>
		/// <param name="terminalId">客户Id</param>
		/// <param name="startTime">开始日期</param>
		/// <param name="endTime">结束日期</param>
		/// <returns></returns>
		public IList<StaffReportBusinessUserAchievement> GetStaffReportBusinessUserAchievement(int? storeId, int? categoryId, int? wareHouseId, int? terminalId, int? topNumber, DateTime? startTime, DateTime? endTime, bool status = false)
		{

			var reporting = new List<StaffReportBusinessUserAchievement>();
			try
			{
				if (!storeId.HasValue)
				{
					return null;
				}

				string whereQuery = "";

				if (categoryId.HasValue && categoryId.Value != 0)
				{
					//递归商品类别查询
					var categoryIds = _categoryService.GetSubCategoryIds(storeId ?? 0, categoryId ?? 0);
					if (categoryIds != null && categoryIds.Count > 0)
					{
						string incategoryIds = string.Join("','", categoryIds);
						whereQuery += $" and p.CategoryId in ('{incategoryIds}') ";

					}
					else
					{
						whereQuery += $" and p.CategoryId = {categoryId} ";
					}
				}


				if (wareHouseId.HasValue && wareHouseId.Value != 0)
				{
					whereQuery += $" and b.WareHouseId = {wareHouseId} ";

				}

				if (terminalId.HasValue && terminalId.Value != 0)
				{
					whereQuery += $" and b.TerminalId = {terminalId} ";

				}

				if (startTime.HasValue)
				{
					whereQuery += $" and b.CreatedOnUtc >= '{startTime?.ToString("yyyy-MM-dd 00:00:00")}' ";

				}

				if (endTime.HasValue)
				{
					whereQuery += $" and b.CreatedOnUtc <='{endTime?.ToString("yyyy-MM-dd 23:59:59")}' ";
				}

				//MYSQL
				string sqlString = @"SELECT 
									u.Id AS BusinessUserId,
									u.UserRealName AS BusinessUserName,
									IFNULL((SELECT 
													SUM(IFNULL(alls.ReceivableAmount, 0.00))
												FROM
													(SELECT 
														b.ReceivableAmount
													FROM
														dcms.SaleBills AS b
													LEFT JOIN SaleItems AS sb ON b.Id = sb.SaleBillId
													LEFT JOIN Products AS p ON sb.ProductId = p.Id
													WHERE
														b.StoreId = {0}
															AND b.BusinessUserId = u.Id {1}
													GROUP BY b.Id) AS alls),
											0.00) AS SaleAmount,
									IFNULL((SELECT 
													SUM(IFNULL(alls.ReceivableAmount, 0.00))
												FROM
													(SELECT 
														b.ReceivableAmount
													FROM
														dcms.SaleBills AS b
													LEFT JOIN SaleItems AS sb ON b.Id = sb.SaleBillId
													LEFT JOIN Products AS p ON sb.ProductId = p.Id
													WHERE
														b.StoreId = {0}
															AND b.BusinessUserId = u.Id
															AND b.AuditedStatus = 1
															AND b.ReversedStatus = 0 {1}
													GROUP BY b.Id) AS alls),
											0.00) AS AdSaleAmount,
									IFNULL((SELECT 
													SUM(IFNULL(alls.ReceivableAmount, 0.00))
												FROM
													(SELECT 
														b.ReceivableAmount
													FROM
														dcms.ReturnBills AS b
													LEFT JOIN SaleItems AS sb ON b.Id = sb.SaleBillId
													LEFT JOIN Products AS p ON sb.ProductId = p.Id
													WHERE
														b.StoreId = {0}
															AND b.BusinessUserId = u.Id {1}
													GROUP BY b.Id) AS alls),
											0.00) AS ReturnAmount,
									IFNULL((SELECT 
													SUM(IFNULL(alls.ReceivableAmount, 0.00))
												FROM
													(SELECT 
														b.ReceivableAmount
													FROM
														dcms.ReturnBills AS b
													LEFT JOIN SaleItems AS sb ON b.Id = sb.SaleBillId
													LEFT JOIN Products AS p ON sb.ProductId = p.Id
													WHERE
														b.StoreId = {0}
															AND b.BusinessUserId = u.Id
															AND b.AuditedStatus = 1
															AND b.ReversedStatus = 0  {1}
													GROUP BY b.Id) AS alls),
											0.00) AS AdReturnAmount,
									0.00 AS NetAmount
								FROM
									auth.User_UserRole_Mapping AS urm
										LEFT JOIN
									auth.Users AS u ON u.id = urm.UserId
										LEFT JOIN
									auth.UserRoles AS ur ON urm.UserRoleId = ur.id
								WHERE
									u.StoreId = {0}
										AND ur.SystemName = 'Salesmans' 
								GROUP BY u.Id , u.UserRealName";

				var aa = string.Format(sqlString, storeId ?? 0, whereQuery);

				var reports = SaleBillsRepository_RO.QueryFromSql<StaffReportBusinessUserAchievement>(string.Format(sqlString, storeId ?? 0, whereQuery)).ToList();


				foreach (var u in reports)
				{
					var r = new StaffReportBusinessUserAchievement()
					{
						BusinessUserId = u.BusinessUserId,
						BusinessUserName = u.BusinessUserName,
						SaleAmount = u.SaleAmount,
						ReturnAmount = u.ReturnAmount,
						AdSaleAmount = u.AdSaleAmount,
						AdReturnAmount = u.AdReturnAmount,
						NetAmount = (u.SaleAmount - u.ReturnAmount),
						ReceiptAmount = this.GetBusinessUserReceipt(storeId ?? 0, u.BusinessUserId ?? 0, startTime, endTime),
						OweAmount = this.GetBusinessUserSaleOweCash(storeId ?? 0, u.BusinessUserId ?? 0, startTime, endTime)
					};

					reporting.Add(r);
				}


				return reporting.OrderByDescending(r => r.NetAmount).ToList();

			}
			catch (Exception ex)
			{
				//:“Unable to cast object of type 'System.Int64' to type 'System.Decimal'.”
				/*
				 "   在 MySqlConnector.Core.Row.GetDecimal(Int32 ordinal) 在 /_/src/MySqlConnector/Core/Row.cs 中: 第 391 行\r\n   在 MySqlConnector.MySqlDataReader.GetDecimal(Int32 ordinal) 在 /_/src/MySqlConnector/MySqlDataReader.cs 中: 第 290 行\r\n   在 Microsoft.EntityFrameworkCore.Query.Internal.FromSqlQueryingEnumerable`1.Enumerator.MoveNext()\r\n   在 System.Collections.Generic.List`1..ctor(IEnumerable`1 collection)\r\n   在 System.Linq.Enumerable.ToList[TSource](IEnumerable`1 source)\r\n   在 DCMS.Services.Report.StaffReportService.GetStaffReportBusinessUserAchievement(Nullable`1 storeId, Nullable`1 categoryId, Nullable`1 wareHouseId, Nullable`1 terminalId, Nullable`1 topNumber, Nullable`1 startTime, Nullable`1 endTime, Boolean status) 在 D:\\Git\\DCMS.Studio.Core\\DCMS.Core.V6\\DCMS.Services\\Report\\StaffReportService.cs 中: 第 196 行"
				 */
				return new List<StaffReportBusinessUserAchievement>();
			}

		}

		/// <summary>
		/// 员工提成汇总表
		/// </summary>
		/// <param name="storeId">经销商Id</param>
		/// <param name="startTime">开始日期</param>
		/// <param name="endTime">结束日期</param>
		/// <param name="staffUserId">员工Id</param>
		/// <param name="categoryId">商品类别Id</param>
		/// <param name="productId">商品Id</param>
		/// <returns></returns>
		public IList<StaffReportPercentageSummary> GetStaffReportPercentageSummary(int? storeId, DateTime? startTime, DateTime? endTime, int? staffUserId, int? categoryId, int? productId)
		{
			try
			{
				return _cacheManager.Get(DCMSDefaults.GETSTAFFREPORT_PERCENTAGE_SUMMARY_KEY.FillCacheKey(storeId, startTime, endTime, staffUserId, categoryId, productId), () =>
				   {

					   var summaries = new List<StaffReportPercentageSummary>();

					   try
					   {

						   var companySetting = _settingService.LoadSetting<CompanySetting>(storeId ?? 0);

						   if (startTime.HasValue)
						   {
							   startTime = DateTime.Parse(((DateTime)startTime).ToString("yyyy-MM-dd 00:00:00"));
						   }

						   if (endTime.HasValue)
						   {
							   endTime = DateTime.Parse(((DateTime)endTime).ToString("yyyy-MM-dd 23:59:59"));
						   }

						   //1. 获取所有员工（提成方案）
						   var users = _userService.GetAllUserPercentages(storeId ?? 0);
						   if (users == null)
						   {
							   return null;
						   }

						   //2.获取所业务员指定时间的销售单的所有商品
						   var bSales = _saleBillService.GetSaleBillsByBusinessUsers(storeId, true, false, users.Select(s => s.User.Id).ToArray(), startTime, endTime);
						   //3.获取所配送员指定时间的销售单的所有商品
						   var dSales = _saleBillService.GetSaleBillsByDeliveryUsers(storeId, true, false, users.Select(s => s.User.Id).ToArray(), startTime, endTime);
						   //4.获取所业务员指定时间的退货单的所有商品
						   var bReturns = _returnBillService.GetReturnBillsByBusinessUsers(storeId, true, false, users.Select(s => s.User.Id).ToArray(), startTime, endTime);
						   //5.获取所有配送员指定时间的退货单的所有商品
						   var dReturns = _returnBillService.GetReturnBillsByDeliveryUsers(storeId, true, false, users.Select(s => s.User.Id).ToArray(), startTime, endTime);

						   //汇总
						   foreach (var u in users)
						   {
							   var summery = new StaffReportPercentageSummary
							   {
								   StaffUserId = u.User.Id,
								   StaffUserName = u.User.UserRealName,
								   BusinessPercentage = 0,
								   DeliveryPercentage = 0,
								   PercentageTotal = 0
							   };
							   summaries.Add(summery);
						   }

						   //汇总明细
						   var items = new List<StaffReportPercentageItem>();
						   foreach (var summ in summaries)
						   {
							   var userId = users.Where(s => s.User.Id == summ.StaffUserId).FirstOrDefault();
							   if (userId == null)
							   {
								   break;
							   }

							   //业务提成计算规则
							   var spts = userId.SPercentages;
							   //送货提成计算规则
							   var dpts = userId.DPercentages;

							   //当前员工销售商品
							   var uSaleProducts = bSales.Where(s => s.BusinessUserId == summ.StaffUserId).ToList();
							   //当前员工退货商品
							   var uReturnProducts = bReturns.Where(s => s.BusinessUserId == summ.StaffUserId).ToList();

							   //当前员工配送销售商品
							   var dSaleProducts = dSales.Where(s => s.DeliveryUserId == summ.StaffUserId).ToList();
							   //当前员工配送退货商品
							   var dReturnProducts = dReturns.Where(s => s.DeliveryUserId == summ.StaffUserId).ToList();

							   //按分类
							   if (categoryId.HasValue && categoryId != 0)
							   {
								   spts = spts.Where(s => s.CatagoryId == categoryId).ToList();
								   dpts = dpts.Where(s => s.CatagoryId == categoryId).ToList();

								   uSaleProducts = uSaleProducts.Where(s => s.CategoryId == categoryId).ToList();
								   uReturnProducts = uReturnProducts.Where(s => s.CategoryId == categoryId).ToList();

								   dSaleProducts = dSaleProducts.Where(s => s.CategoryId == categoryId).ToList();
								   dReturnProducts = dReturnProducts.Where(s => s.CategoryId == categoryId).ToList();
							   }

							   //按商品
							   if (productId.HasValue && productId != 0)
							   {
								   spts = spts.Where(s => s.ProductId == productId).ToList();
								   dpts = dpts.Where(s => s.ProductId == productId).ToList();

								   uSaleProducts = uSaleProducts.Where(s => s.ProductId == productId).ToList();
								   uReturnProducts = uReturnProducts.Where(s => s.ProductId == productId).ToList();

								   dSaleProducts = dSaleProducts.Where(s => s.ProductId == productId).ToList();
								   dReturnProducts = dReturnProducts.Where(s => s.ProductId == productId).ToList();
							   }


							   #region  //业务提成

							   //业务合集（销售+退货）
							   var allBProducts = uSaleProducts.Union(uReturnProducts);
							   foreach (var product in allBProducts)
							   {
								   var pid = product.ProductId;
								   //提成明细
								   var item = new StaffReportPercentageItem
								   {
									   //商品名称
									   ProductId = pid,
									   ProductName = product.ProductName,
									   SaleFragment = ""
								   };

								   //获取当前商品分类的提成计算方式
								   var curcatagorySpt = spts.Where(s => s.CatagoryId == product.CategoryId).FirstOrDefault();
								   //获取当前商品的提成计算方式
								   var curSpt = spts.Where(s => s.ProductId == pid).FirstOrDefault();
								   //当前商品的计算方案不存在时，判断分类是否存在，分类存在则使用分类计算规则，否则商品不计算提成
								   if (curSpt == null)
								   {
									   if (curcatagorySpt != null)
									   {
										   curSpt = curcatagorySpt;
									   }
								   }

								   if (curSpt != null)
								   {
									   //提成方式
									   item.CalCulateMethodId = curSpt.CalCulateMethodId;
									   //提成方式枚举名称
									   item.CalCulateMethodName = CommonHelper.GetEnumDescription<PercentageCalCulateMethod>(curSpt.CalCulateMethod);

									   //是否赠品参与提成
									   if (!curSpt.IsGiftCalCulated)
									   {
										   if (product.IsGifts)
										   {
											   break;
										   }
									   }

									   #region  

									   //当前销售商品数量
									   var curSPQty = 0;
									   //当前销售商品价格
									   decimal curSPprice = 0;
									   //当前销售商品金额 
									   decimal curSPAmount = 0;
									   //当前销售商品利润
									   decimal curSPProfit = 0;
									   //当前销售商品成本金额
									   decimal curSPCostAmount = 0;
									   //当前销售商品历史成本价
									   decimal curSPCostPrice = 0;
									   //当前销售商品成本利润率
									   decimal curSPCostProfitRate = 0;

									   //当前退货商品数量
									   var curRPQty = 0;
									   //当前退货商品价格
									   decimal curRPprice = 0;
									   //当前退货商品金额 
									   decimal curRPAmount = 0;
									   //当前销售商品成本金额
									   decimal curRPCostAmount = 0;
									   //当前退货商品利润
									   decimal curRPProfit = 0;
									   //当前退货商品历史成本价
									   decimal curRPCostPrice = 0;
									   //当前退货商品成本利润率
									   decimal curRPCostProfitRate = 0;

									   if (product.PercentageType == 0)//销售
									   {
										   curSPQty = product.Quantity;
										   curSPprice = product.Price;
										   curSPAmount = product.Amount;
										   curSPProfit = product.Profit;
										   curSPCostAmount = product.CostAmount;
										   curSPCostPrice = product.CostPrice;
										   curSPCostProfitRate = product.CostProfitRate;
									   }
									   else if (product.PercentageType == 1)//退货
									   {
										   curRPQty = product.Quantity;
										   curRPprice = product.Price;
										   curRPAmount = product.Amount;
										   curRPProfit = product.Profit;
										   curRPCostAmount = product.CostAmount;
										   curRPCostPrice = product.CostPrice;
										   curRPCostProfitRate = product.CostProfitRate;
									   }

									   #endregion

									   //提成方式
									   switch (curSpt.CalCulateMethod)
									   {
										   //销售额百分比
										   case PercentageCalCulateMethod.PercentageOfSales:
											   {
												   //销售提成比例
												   var sp = curSpt.SalesPercent ?? 0;
												   //退货提成比例
												   var rp = curSpt.ReturnPercent ?? 0;
												   //销售提成 = 当前销售商品金额 * 销售提成比例 / 100;
												   item.PercentageSale = curSPAmount * (sp / 100);

												   //是否退货参与提成
												   if (curSpt.IsReturnCalCulated)
												   {
													   //退货提成 = 当前退货商品金额 * 退货提成比例 / 100;
													   item.PercentageReturn = curRPAmount * (rp / 100);
													   //提成合计
													   item.PercentageTotal = item.PercentageSale + item.PercentageReturn;
												   }
												   else
												   {
													   item.PercentageReturn = 0;
													   item.PercentageTotal = item.PercentageSale;
												   }

											   }
											   break;
										   //销售额变化百分比
										   case PercentageCalCulateMethod.PercentageChangeInSales:
											   {
												   //取区间 PercentageRangeOptions
												   //按利润区间范围计算提成
												   var rangs = curSpt.PercentageRangeOptions;

												   //净销售额区间范围
												   var amountSaleNet = curSPAmount - curRPAmount;
												   //销售提成比例
												   decimal sp = 0;
												   //退货提成比例
												   decimal rp = 0;
												   foreach (var ra in rangs)
												   {
													   if (ra.NetSalesRange <= amountSaleNet)
													   {
														   //销售提成(%)
														   sp = ra.SalesPercent ?? 0;
														   //退货提成(%)
														   rp = ra.ReturnPercent ?? 0;
													   }

													   item.SaleFragment = $"{(ra.NetSalesRange ?? 0).ToString("0.00")} 元以上";
												   }

												   //销售提成 = 当前销售商品金额 * 销售提成比例 / 100;
												   item.PercentageSale = curSPAmount * (sp / 100);

												   //是否退货参与提成
												   if (curSpt.IsReturnCalCulated)
												   {
													   //退货提成 = 当前退货商品金额 * 退货提成比例 / 100;
													   item.PercentageReturn = curRPAmount * (rp / 100);
													   //提成合计
													   item.PercentageTotal = item.PercentageSale + item.PercentageReturn;
												   }
												   else
												   {
													   item.PercentageReturn = 0;
													   item.PercentageTotal = item.PercentageSale;
												   }
											   }
											   break;
										   //销售额分段变化百分比
										   case PercentageCalCulateMethod.SalesSegmentChangePercentage:
											   {
												   //取区间 PercentageRangeOptions
												   var rangs = curSpt.PercentageRangeOptions;

												   //净销售额区间范围
												   var amountSaleNet = curSPAmount - curRPAmount;
												   //销售提成比例
												   decimal sp = 0;
												   //退货提成比例
												   decimal rp = 0;
												   foreach (var ra in rangs)
												   {
													   if (ra.NetSalesRange <= amountSaleNet)
													   {
														   //销售提成(%)
														   sp = ra.SalesPercent ?? 0;
														   //退货提成(%)
														   rp = ra.ReturnPercent ?? 0;
													   }
													   item.SaleFragment = $"{(ra.NetSalesRange ?? 0).ToString("0.00")} 元以上";
												   }

												   //销售提成 = 当前销售商品金额 * 销售提成比例 / 100;
												   item.PercentageSale = curSPAmount * (sp / 100);

												   //是否退货参与提成
												   if (curSpt.IsReturnCalCulated)
												   {
													   //退货提成 = 当前退货商品金额 * 退货提成比例 / 100;
													   item.PercentageReturn = curRPAmount * (rp / 100);
													   //提成合计
													   item.PercentageTotal = item.PercentageSale + item.PercentageReturn;
												   }
												   else
												   {
													   item.PercentageReturn = 0;
													   item.PercentageTotal = item.PercentageSale;
												   }
											   }
											   break;
										   //销售数量每件固定额
										   case PercentageCalCulateMethod.SalesVolumeFixedAmount:
											   {
												   var curProduct = _productService.GetProductById(storeId, pid);
												   //销售提成金额 SalesAmount
												   //退货提成金额 ReturnAmount
												   //成本计算方式 curSpt.CostingCalCulateMethod

												   decimal tmpSQuantity = 0;
												   //使用基本单位核算数量
												   decimal tmpRQuantity = 0;
												   if (curSpt.QuantityCalCulateMethod == QuantityCalCulateMethod.UsingBasicUnitsCalculateQuantity)
												   {
													   tmpSQuantity = curSPQty;
													   tmpRQuantity = curRPQty;
												   }
												   //使用大包单位核算数量
												   else if (curSpt.QuantityCalCulateMethod == QuantityCalCulateMethod.UselargePackageUnitsCalculateQuantity)
												   {
													   var bigU = curProduct != null ? curProduct.BigQuantity ?? 1 : 1;
													   tmpSQuantity = curSPQty / bigU;
													   tmpRQuantity = curRPQty / bigU;
												   }

												   //销售提成 = 销售提成金额 *  销售量
												   item.PercentageSale = curSpt.SalesAmount * tmpSQuantity;
												   //是否退货参与提成
												   if (curSpt.IsReturnCalCulated)
												   {
													   //退货提成 = 退货提成金额 * 退货量
													   item.PercentageReturn = curSpt.ReturnAmount * tmpRQuantity;
													   //提成合计
													   item.PercentageTotal = item.PercentageSale + item.PercentageReturn;
												   }
												   else
												   {
													   item.PercentageReturn = 0;
													   item.PercentageTotal = item.PercentageSale;
												   }
											   }
											   break;
										   //按销售数量变化每件提成金额
										   case PercentageCalCulateMethod.ChangesInSalesVolume:
											   {
												   var curProduct = _productService.GetProductById(storeId, pid);
												   //取区间 PercentageRangeOptions
												   //成本计算方式 CostingCalCulateMethod

												   decimal tmpSQuantity = 0;
												   //使用基本单位核算数量
												   decimal tmpRQuantity = 0;
												   if (curSpt.QuantityCalCulateMethod == QuantityCalCulateMethod.UsingBasicUnitsCalculateQuantity)
												   {
													   tmpSQuantity = curSPQty;
													   tmpRQuantity = curRPQty;
												   }
												   //使用大包单位核算数量
												   else if (curSpt.QuantityCalCulateMethod == QuantityCalCulateMethod.UselargePackageUnitsCalculateQuantity)
												   {
													   var bigU = curProduct != null ? curProduct.BigQuantity ?? 1 : 1;
													   tmpSQuantity = curSPQty / bigU;
													   tmpRQuantity = curRPQty / bigU;
												   }

												   var rangs = curSpt.PercentageRangeOptions;

												   //净销售量区间范围
												   var quantitySaleNet = tmpSQuantity - tmpRQuantity;
												   //销售提成
												   decimal sp = 0;
												   //退货提成
												   decimal rp = 0;
												   foreach (var ra in rangs)
												   {
													   if (ra.NetSalesRange <= quantitySaleNet)
													   {
														   //销售提成(元)
														   sp = ra.SalesPercent ?? 0;
														   //退货提成(元)
														   rp = ra.ReturnPercent ?? 0;
													   }
													   item.SaleFragment = $"{(ra.NetSalesRange ?? 0).ToString("0.00")} 件以上";
												   }

												   //销售提成 = 销售提成金额 *  销售量
												   item.PercentageSale = sp * tmpSQuantity;
												   //是否退货参与提成
												   if (curSpt.IsReturnCalCulated)
												   {
													   //退货提成 = 退货提成金额 * 退货量
													   item.PercentageReturn = rp * tmpRQuantity;
													   //提成合计
													   item.PercentageTotal = item.PercentageSale + item.PercentageReturn;
												   }
												   else
												   {
													   item.PercentageReturn = 0;
													   item.PercentageTotal = item.PercentageSale;
												   }
											   }
											   break;
										   //按销售数量分段变化每件提成金额
										   case PercentageCalCulateMethod.AccordingToSalesVolume:
											   {
												   var curProduct = _productService.GetProductById(storeId, pid);
												   //取区间 PercentageRangeOptions
												   //成本计算方式 CostingCalCulateMethod
												   decimal tmpSQuantity = 0;
												   //使用基本单位核算数量
												   decimal tmpRQuantity = 0;
												   if (curSpt.QuantityCalCulateMethod == QuantityCalCulateMethod.UsingBasicUnitsCalculateQuantity)
												   {
													   tmpSQuantity = curSPQty;
													   tmpRQuantity = curRPQty;
												   }
												   //使用大包单位核算数量
												   else if (curSpt.QuantityCalCulateMethod == QuantityCalCulateMethod.UselargePackageUnitsCalculateQuantity)
												   {
													   var bigU = curProduct != null ? curProduct.BigQuantity ?? 1 : 1;
													   tmpSQuantity = curSPQty / bigU;
													   tmpRQuantity = curRPQty / bigU;
												   }

												   var rangs = curSpt.PercentageRangeOptions;

												   //净销售量区间范围
												   var quantitySaleNet = tmpSQuantity - tmpRQuantity;
												   //销售提成
												   decimal sp = 0;
												   //退货提成
												   decimal rp = 0;
												   foreach (var ra in rangs)
												   {
													   if (ra.NetSalesRange <= quantitySaleNet)
													   {
														   //销售提成(元)
														   sp = ra.SalesPercent ?? 0;
														   //退货提成(元)
														   rp = ra.ReturnPercent ?? 0;
													   }
													   item.SaleFragment = $"{(ra.NetSalesRange ?? 0).ToString("0.00")} 件以上";
												   }

												   //销售提成 = 销售提成金额 *  销售量
												   item.PercentageSale = sp * tmpSQuantity;
												   //是否退货参与提成
												   if (curSpt.IsReturnCalCulated)
												   {
													   //退货提成 = 退货提成金额 * 退货量
													   item.PercentageReturn = rp * tmpRQuantity;
													   //提成合计
													   item.PercentageTotal = item.PercentageSale + item.PercentageReturn;
												   }
												   else
												   {
													   item.PercentageReturn = 0;
													   item.PercentageTotal = item.PercentageSale;
												   }
											   }
											   break;
										   //利润额百分比
										   case PercentageCalCulateMethod.ProfitPercentage:
											   {
												   //销售提成比例 SalesPercent
												   //退货提成比例 ReturnPercent
												   //成本计算方式 CostingCalCulateMethod
												   //利润 = 金额 - 成本金额

												   curSPAmount = 0;
												   curRPAmount = 0;

												   //重新计算成本
												   //当时加权平均价(历史成本价)
												   if (curSpt.CostingCalCulateMethod == CostingCalCulateMethod.WeightedAveragePriceTime)
												   {
													   var averagePrice = curSPCostPrice;
													   //重新计算利润
													   curSPProfit = curSPAmount - averagePrice;
													   curRPProfit = curRPAmount - averagePrice;
												   }
												   //现在加权平均价 (以历史5次进货价的平均价为参考成本价)
												   else if (curSpt.CostingCalCulateMethod == CostingCalCulateMethod.WeightedAveragePriceNow)
												   {
													   //现在加权平均价
													   var num = companySetting.AveragePurchasePriceCalcNumber;
													   var averagePrice = _productService.GetNowWeightedAveragePrice(pid, num == 0 ? 5 : num);
													   //重新计算利润
													   curSPProfit = curSPAmount - averagePrice;
													   curRPProfit = curRPAmount - averagePrice;

												   }
												   //现在预设进价（现在商品预设进价）
												   else if (curSpt.CostingCalCulateMethod == CostingCalCulateMethod.PresetPurchasePriceNow)
												   {
													   var curProduct = _productService.GetProductById(storeId, pid);
													   var averagePrice = curProduct.ProductPrices.Where(s => s.UnitId == curProduct.SmallUnitId).Select(s => s.PurchasePrice).FirstOrDefault() ?? 0;
													   //重新计算利润
													   curSPProfit = curSPAmount - averagePrice;
													   curRPProfit = curRPAmount - averagePrice;
												   }

												   //========================================

												   //销售提成比例(利润额百分比)
												   var sp = curSpt.SalesPercent ?? 0;
												   //退货提成比例(利润额百分比)
												   var rp = curSpt.ReturnPercent ?? 0;
												   //销售提成 = 当前销售商品利润额 * 销售提成比例 / 100;
												   item.PercentageSale = curSPProfit * (sp / 100);

												   //是否退货参与提成
												   if (curSpt.IsReturnCalCulated)
												   {
													   //退货提成 = 当前退货商品利润额 * 退货提成比例 / 100;
													   item.PercentageReturn = curRPProfit * (rp / 100);
													   //提成合计
													   item.PercentageTotal = item.PercentageSale + item.PercentageReturn;
												   }
												   else
												   {
													   item.PercentageReturn = 0;
													   item.PercentageTotal = item.PercentageSale;
												   }
											   }
											   break;
										   //利润额变化百分比
										   case PercentageCalCulateMethod.ProfitChangePercentage:
											   {
												   //取区间 PercentageRangeOptions
												   //成本计算方式 CostingCalCulateMethod

												   curSPAmount = 0;
												   curRPAmount = 0;

												   //重新计算成本
												   //当时加权平均价(历史成本价)
												   if (curSpt.CostingCalCulateMethod == CostingCalCulateMethod.WeightedAveragePriceTime)
												   {
													   var averagePrice = curSPCostPrice;
													   //重新计算利润
													   curSPProfit = curSPAmount - averagePrice;
													   curRPProfit = curRPAmount - averagePrice;
												   }
												   //现在加权平均价 (以历史5次进货价的平均价为参考成本价)
												   else if (curSpt.CostingCalCulateMethod == CostingCalCulateMethod.WeightedAveragePriceNow)
												   {
													   //现在加权平均价
													   var num = companySetting.AveragePurchasePriceCalcNumber;
													   var averagePrice = _productService.GetNowWeightedAveragePrice(pid, num == 0 ? 5 : num);
													   //重新计算利润
													   curSPProfit = curSPAmount - averagePrice;
													   curRPProfit = curRPAmount - averagePrice;

												   }
												   //现在预设进价（现在商品预设进价）
												   else if (curSpt.CostingCalCulateMethod == CostingCalCulateMethod.PresetPurchasePriceNow)
												   {
													   var curProduct = _productService.GetProductById(storeId, pid);
													   var averagePrice = curProduct.ProductPrices.Where(s => s.UnitId == curProduct.SmallUnitId).Select(s => s.PurchasePrice).FirstOrDefault() ?? 0;
													   //重新计算利润
													   curSPProfit = curSPAmount - averagePrice;
													   curRPProfit = curRPAmount - averagePrice;
												   }

												   //========================================
												   var rangs = curSpt.PercentageRangeOptions;

												   //净销售额区间范围
												   var quantitySaleNet = curSPAmount - curRPAmount;
												   //销售提成比例(利润额百分比)
												   decimal sp = 0;
												   //退货提成比例(利润额百分比)
												   decimal rp = 0;
												   foreach (var ra in rangs)
												   {
													   if (ra.NetSalesRange <= quantitySaleNet)
													   {
														   //销售提成(元)
														   sp = ra.SalesPercent ?? 0;
														   //退货提成(元)
														   rp = ra.ReturnPercent ?? 0;
													   }
													   item.SaleFragment = $"{(ra.NetSalesRange ?? 0).ToString("0.00")} 元以上";
												   }

												   //========================================

												   //销售提成 = 当前销售商品利润额 * 销售提成比例 / 100;
												   item.PercentageSale = curSPProfit * (sp / 100);

												   //是否退货参与提成
												   if (curSpt.IsReturnCalCulated)
												   {
													   //退货提成 = 当前退货商品利润额 * 退货提成比例 / 100;
													   item.PercentageReturn = curRPProfit * (rp / 100);
													   //提成合计
													   item.PercentageTotal = item.PercentageSale + item.PercentageReturn;
												   }
												   else
												   {
													   item.PercentageReturn = 0;
													   item.PercentageTotal = item.PercentageSale;
												   }
											   }
											   break;
										   //利润额分段变化百分比
										   case PercentageCalCulateMethod.PercentageChangeInProfit:
											   {
												   //取区间 PercentageRangeOptions
												   //成本计算方式 CostingCalCulateMethod

												   curSPAmount = 0;
												   curRPAmount = 0;

												   //重新计算成本
												   //当时加权平均价(历史成本价)
												   if (curSpt.CostingCalCulateMethod == CostingCalCulateMethod.WeightedAveragePriceTime)
												   {
													   var averagePrice = curSPCostPrice;
													   //重新计算利润
													   curSPProfit = curSPAmount - averagePrice;
													   curRPProfit = curRPAmount - averagePrice;
												   }
												   //现在加权平均价 (以历史5次进货价的平均价为参考成本价)
												   else if (curSpt.CostingCalCulateMethod == CostingCalCulateMethod.WeightedAveragePriceNow)
												   {
													   //现在加权平均价
													   var num = companySetting.AveragePurchasePriceCalcNumber;
													   var averagePrice = _productService.GetNowWeightedAveragePrice(pid, num == 0 ? 5 : num);
													   //重新计算利润
													   curSPProfit = curSPAmount - averagePrice;
													   curRPProfit = curRPAmount - averagePrice;

												   }
												   //现在预设进价（现在商品预设进价）
												   else if (curSpt.CostingCalCulateMethod == CostingCalCulateMethod.PresetPurchasePriceNow)
												   {
													   var curProduct = _productService.GetProductById(storeId, pid);
													   var averagePrice = curProduct.ProductPrices.Where(s => s.UnitId == curProduct.SmallUnitId).Select(s => s.PurchasePrice).FirstOrDefault() ?? 0;
													   //重新计算利润
													   curSPProfit = curSPAmount - averagePrice;
													   curRPProfit = curRPAmount - averagePrice;
												   }

												   //========================================
												   var rangs = curSpt.PercentageRangeOptions;

												   //净销售额区间范围
												   var quantitySaleNet = curSPAmount - curRPAmount;
												   //销售提成比例(利润额百分比)
												   decimal sp = 0;
												   //退货提成比例(利润额百分比)
												   decimal rp = 0;
												   foreach (var ra in rangs)
												   {
													   if (ra.NetSalesRange <= quantitySaleNet)
													   {
														   //销售提成(元)
														   sp = ra.SalesPercent ?? 0;
														   //退货提成(元)
														   rp = ra.ReturnPercent ?? 0;
													   }
													   item.SaleFragment = $"{(ra.NetSalesRange ?? 0).ToString("0.00")} 元以上";
												   }

												   //========================================

												   //销售提成 = 当前销售商品利润额 * 销售提成比例 / 100;
												   item.PercentageSale = curSPProfit * (sp / 100);

												   //是否退货参与提成
												   if (curSpt.IsReturnCalCulated)
												   {
													   //退货提成 = 当前退货商品利润额 * 退货提成比例 / 100;
													   item.PercentageReturn = curRPProfit * (rp / 100);
													   //提成合计
													   item.PercentageTotal = item.PercentageSale + item.PercentageReturn;
												   }
												   else
												   {
													   item.PercentageReturn = 0;
													   item.PercentageTotal = item.PercentageSale;
												   }
											   }
											   break;
										   default:
											   break;
									   }


									   //净销售量
									   item.QuantityNetSaleQuantity = curSPQty - curRPQty;
									   //销售量
									   item.QuantitySale = curSPQty;
									   //退货量
									   item.QuantityReturn = curRPQty;

									   //销售净额
									   item.AmountSaleNet = curSPAmount - curRPAmount;
									   //销售额
									   item.AmountSale = curSPAmount;
									   //退货额
									   item.AmountReturn = curRPAmount;

									   //净利润
									   item.ProfitNet = curSPProfit - curRPProfit;
									   //销售利润
									   item.ProfitSale = curSPProfit;
									   //退货利润
									   item.ProfitReturn = curRPProfit;
								   }

								   summ.SItems.Add(item);
							   }

							   #endregion


							   #region  //送货提成

							   //送货合集（销售+退货）
							   var allDProducts = dSaleProducts.Union(dReturnProducts);
							   foreach (var product in allDProducts)
							   {
								   var pid = product.ProductId;
								   //提成明细
								   var item = new StaffReportPercentageItem
								   {

									   //商品名称
									   ProductId = pid,
									   ProductName = product.ProductName,
									   SaleFragment = ""
								   };

								   //获取当前商品分类的提成计算方式
								   var curcatagorySpt = spts.Where(s => s.CatagoryId == product.CategoryId).FirstOrDefault();
								   //获取当前商品的提成计算方式
								   var curSpt = spts.Where(s => s.ProductId == pid).FirstOrDefault();
								   //当前商品的计算方案不存在时，判断分类是否存在，分类存在则使用分类计算规则，否则商品不计算提成
								   if (curSpt == null)
								   {
									   if (curcatagorySpt != null)
									   {
										   curSpt = curcatagorySpt;
									   }
								   }

								   if (curSpt != null)
								   {
									   //提成方式
									   item.CalCulateMethodId = curSpt.CalCulateMethodId;
									   item.CalCulateMethodName = CommonHelper.GetEnumDescription<PercentageCalCulateMethod>(curSpt.CalCulateMethod);

									   //是否赠品参与提成
									   if (!curSpt.IsGiftCalCulated)
									   {
										   if (product.IsGifts)
										   {
											   break;
										   }
									   }

									   #region  

									   //当前销售商品数量
									   var curSPQty = 0;
									   //当前销售商品价格
									   decimal curSPprice = 0;
									   //当前销售商品金额 
									   decimal curSPAmount = 0;
									   //当前销售商品利润
									   decimal curSPProfit = 0;
									   //当前销售商品成本金额
									   decimal curSPCostAmount = 0;
									   //当前销售商品历史成本价
									   decimal curSPCostPrice = 0;
									   //当前销售商品成本利润率
									   decimal curSPCostProfitRate = 0;

									   //当前退货商品数量
									   var curRPQty = 0;
									   //当前退货商品价格
									   decimal curRPprice = 0;
									   //当前退货商品金额 
									   decimal curRPAmount = 0;
									   //当前销售商品成本金额
									   decimal curRPCostAmount = 0;
									   //当前退货商品利润
									   decimal curRPProfit = 0;
									   //当前退货商品历史成本价
									   decimal curRPCostPrice = 0;
									   //当前退货商品成本利润率
									   decimal curRPCostProfitRate = 0;

									   if (product.PercentageType == 0)//销售
									   {
										   curSPQty = product.Quantity;
										   curSPprice = product.Price;
										   curSPAmount = product.Amount;
										   curSPProfit = product.Profit;
										   curSPCostAmount = product.CostAmount;
										   curSPCostPrice = product.CostPrice;
										   curSPCostProfitRate = product.CostProfitRate;
									   }
									   else if (product.PercentageType == 1)//退货
									   {
										   curRPQty = product.Quantity;
										   curRPprice = product.Price;
										   curRPAmount = product.Amount;
										   curRPProfit = product.Profit;
										   curRPCostAmount = product.CostAmount;
										   curRPCostPrice = product.CostPrice;
										   curRPCostProfitRate = product.CostProfitRate;
									   }

									   #endregion

									   //提成方式
									   switch (curSpt.CalCulateMethod)
									   {
										   //销售额百分比
										   case PercentageCalCulateMethod.PercentageOfSales:
											   {
												   //销售提成比例
												   var sp = curSpt.SalesPercent ?? 0;
												   //退货提成比例
												   var rp = curSpt.ReturnPercent ?? 0;
												   //销售提成 = 当前销售商品金额 * 销售提成比例 / 100;
												   item.PercentageSale = curSPAmount * (sp / 100);

												   //是否退货参与提成
												   if (curSpt.IsReturnCalCulated)
												   {
													   //退货提成 = 当前退货商品金额 * 退货提成比例 / 100;
													   item.PercentageReturn = curRPAmount * (rp / 100);
													   //提成合计
													   item.PercentageTotal = item.PercentageSale + item.PercentageReturn;
												   }
												   else
												   {
													   item.PercentageReturn = 0;
													   item.PercentageTotal = item.PercentageSale;
												   }

											   }
											   break;
										   //销售额变化百分比
										   case PercentageCalCulateMethod.PercentageChangeInSales:
											   {
												   //取区间 PercentageRangeOptions
												   //按利润区间范围计算提成
												   var rangs = curSpt.PercentageRangeOptions;

												   //净销售额区间范围
												   var amountSaleNet = curSPAmount - curRPAmount;
												   //销售提成比例
												   decimal sp = 0;
												   //退货提成比例
												   decimal rp = 0;
												   foreach (var ra in rangs)
												   {
													   if (ra.NetSalesRange <= amountSaleNet)
													   {
														   //销售提成(%)
														   sp = ra.SalesPercent ?? 0;
														   //退货提成(%)
														   rp = ra.ReturnPercent ?? 0;
													   }
													   item.SaleFragment = $"{(ra.NetSalesRange ?? 0).ToString("0.00")} 元以上";
												   }

												   //销售提成 = 当前销售商品金额 * 销售提成比例 / 100;
												   item.PercentageSale = curSPAmount * (sp / 100);

												   //是否退货参与提成
												   if (curSpt.IsReturnCalCulated)
												   {
													   //退货提成 = 当前退货商品金额 * 退货提成比例 / 100;
													   item.PercentageReturn = curRPAmount * (rp / 100);
													   //提成合计
													   item.PercentageTotal = item.PercentageSale + item.PercentageReturn;
												   }
												   else
												   {
													   item.PercentageReturn = 0;
													   item.PercentageTotal = item.PercentageSale;
												   }
											   }
											   break;
										   //销售额分段变化百分比
										   case PercentageCalCulateMethod.SalesSegmentChangePercentage:
											   {
												   //取区间 PercentageRangeOptions
												   var rangs = curSpt.PercentageRangeOptions;

												   //净销售额区间范围
												   var amountSaleNet = curSPAmount - curRPAmount;
												   //销售提成比例
												   decimal sp = 0;
												   //退货提成比例
												   decimal rp = 0;
												   foreach (var ra in rangs)
												   {
													   if (ra.NetSalesRange <= amountSaleNet)
													   {
														   //销售提成(%)
														   sp = ra.SalesPercent ?? 0;
														   //退货提成(%)
														   rp = ra.ReturnPercent ?? 0;
													   }
													   item.SaleFragment = $"{(ra.NetSalesRange ?? 0).ToString("0.00")} 元以上";
												   }

												   //销售提成 = 当前销售商品金额 * 销售提成比例 / 100;
												   item.PercentageSale = curSPAmount * (sp / 100);

												   //是否退货参与提成
												   if (curSpt.IsReturnCalCulated)
												   {
													   //退货提成 = 当前退货商品金额 * 退货提成比例 / 100;
													   item.PercentageReturn = curRPAmount * (rp / 100);
													   //提成合计
													   item.PercentageTotal = item.PercentageSale + item.PercentageReturn;
												   }
												   else
												   {
													   item.PercentageReturn = 0;
													   item.PercentageTotal = item.PercentageSale;
												   }
											   }
											   break;
										   //销售数量每件固定额
										   case PercentageCalCulateMethod.SalesVolumeFixedAmount:
											   {
												   var curProduct = _productService.GetProductById(storeId, pid);
												   //销售提成金额 SalesAmount
												   //退货提成金额 ReturnAmount
												   //成本计算方式 curSpt.CostingCalCulateMethod

												   decimal tmpSQuantity = 0;
												   //使用基本单位核算数量
												   decimal tmpRQuantity = 0;
												   if (curSpt.QuantityCalCulateMethod == QuantityCalCulateMethod.UsingBasicUnitsCalculateQuantity)
												   {
													   tmpSQuantity = curSPQty;
													   tmpRQuantity = curRPQty;
												   }
												   //使用大包单位核算数量
												   else if (curSpt.QuantityCalCulateMethod == QuantityCalCulateMethod.UselargePackageUnitsCalculateQuantity)
												   {
													   var bigU = curProduct != null ? curProduct.BigQuantity ?? 1 : 1;
													   tmpSQuantity = curSPQty / bigU;
													   tmpRQuantity = curRPQty / bigU;
												   }

												   //销售提成 = 销售提成金额 *  销售量
												   item.PercentageSale = curSpt.SalesAmount * tmpSQuantity;
												   //是否退货参与提成
												   if (curSpt.IsReturnCalCulated)
												   {
													   //退货提成 = 退货提成金额 * 退货量
													   item.PercentageReturn = curSpt.ReturnAmount * tmpRQuantity;
													   //提成合计
													   item.PercentageTotal = item.PercentageSale + item.PercentageReturn;
												   }
												   else
												   {
													   item.PercentageReturn = 0;
													   item.PercentageTotal = item.PercentageSale;
												   }
											   }
											   break;
										   //按销售数量变化每件提成金额
										   case PercentageCalCulateMethod.ChangesInSalesVolume:
											   {
												   var curProduct = _productService.GetProductById(storeId, pid);
												   //取区间 PercentageRangeOptions
												   //成本计算方式 CostingCalCulateMethod

												   decimal tmpSQuantity = 0;
												   //使用基本单位核算数量
												   decimal tmpRQuantity = 0;
												   if (curSpt.QuantityCalCulateMethod == QuantityCalCulateMethod.UsingBasicUnitsCalculateQuantity)
												   {
													   tmpSQuantity = curSPQty;
													   tmpRQuantity = curRPQty;
												   }
												   //使用大包单位核算数量
												   else if (curSpt.QuantityCalCulateMethod == QuantityCalCulateMethod.UselargePackageUnitsCalculateQuantity)
												   {
													   var bigU = curProduct != null ? curProduct.BigQuantity ?? 1 : 1;
													   tmpSQuantity = curSPQty / bigU;
													   tmpRQuantity = curRPQty / bigU;
												   }

												   var rangs = curSpt.PercentageRangeOptions;

												   //净销售量区间范围
												   var quantitySaleNet = tmpSQuantity - tmpRQuantity;
												   //销售提成
												   decimal sp = 0;
												   //退货提成
												   decimal rp = 0;
												   foreach (var ra in rangs)
												   {
													   if (ra.NetSalesRange <= quantitySaleNet)
													   {
														   //销售提成(元)
														   sp = ra.SalesPercent ?? 0;
														   //退货提成(元)
														   rp = ra.ReturnPercent ?? 0;
													   }
													   item.SaleFragment = $"{(ra.NetSalesRange ?? 0).ToString("0.00")} 件以上";
												   }

												   //销售提成 = 销售提成金额 *  销售量
												   item.PercentageSale = sp * tmpSQuantity;
												   //是否退货参与提成
												   if (curSpt.IsReturnCalCulated)
												   {
													   //退货提成 = 退货提成金额 * 退货量
													   item.PercentageReturn = rp * tmpRQuantity;
													   //提成合计
													   item.PercentageTotal = item.PercentageSale + item.PercentageReturn;
												   }
												   else
												   {
													   item.PercentageReturn = 0;
													   item.PercentageTotal = item.PercentageSale;
												   }
											   }
											   break;
										   //按销售数量分段变化每件提成金额
										   case PercentageCalCulateMethod.AccordingToSalesVolume:
											   {
												   var curProduct = _productService.GetProductById(storeId, pid);
												   //取区间 PercentageRangeOptions
												   //成本计算方式 CostingCalCulateMethod
												   decimal tmpSQuantity = 0;
												   //使用基本单位核算数量
												   decimal tmpRQuantity = 0;
												   if (curSpt.QuantityCalCulateMethod == QuantityCalCulateMethod.UsingBasicUnitsCalculateQuantity)
												   {
													   tmpSQuantity = curSPQty;
													   tmpRQuantity = curRPQty;
												   }
												   //使用大包单位核算数量
												   else if (curSpt.QuantityCalCulateMethod == QuantityCalCulateMethod.UselargePackageUnitsCalculateQuantity)
												   {
													   var bigU = curProduct != null ? curProduct.BigQuantity ?? 1 : 1;
													   tmpSQuantity = curSPQty / bigU;
													   tmpRQuantity = curRPQty / bigU;
												   }

												   var rangs = curSpt.PercentageRangeOptions;

												   //净销售量区间范围
												   var quantitySaleNet = tmpSQuantity - tmpRQuantity;
												   //销售提成
												   decimal sp = 0;
												   //退货提成
												   decimal rp = 0;
												   foreach (var ra in rangs)
												   {
													   if (ra.NetSalesRange <= quantitySaleNet)
													   {
														   //销售提成(元)
														   sp = ra.SalesPercent ?? 0;
														   //退货提成(元)
														   rp = ra.ReturnPercent ?? 0;
													   }
													   item.SaleFragment = $"{(ra.NetSalesRange ?? 0).ToString("0.00")} 件以上";
												   }

												   //销售提成 = 销售提成金额 *  销售量
												   item.PercentageSale = sp * tmpSQuantity;
												   //是否退货参与提成
												   if (curSpt.IsReturnCalCulated)
												   {
													   //退货提成 = 退货提成金额 * 退货量
													   item.PercentageReturn = rp * tmpRQuantity;
													   //提成合计
													   item.PercentageTotal = item.PercentageSale + item.PercentageReturn;
												   }
												   else
												   {
													   item.PercentageReturn = 0;
													   item.PercentageTotal = item.PercentageSale;
												   }
											   }
											   break;
										   //利润额百分比
										   case PercentageCalCulateMethod.ProfitPercentage:
											   {
												   //销售提成比例 SalesPercent
												   //退货提成比例 ReturnPercent
												   //成本计算方式 CostingCalCulateMethod
												   //利润 = 金额 - 成本金额

												   curSPAmount = 0;
												   curRPAmount = 0;

												   //重新计算成本
												   //当时加权平均价(历史成本价)
												   if (curSpt.CostingCalCulateMethod == CostingCalCulateMethod.WeightedAveragePriceTime)
												   {
													   var averagePrice = curSPCostPrice;
													   //重新计算利润
													   curSPProfit = curSPAmount - averagePrice;
													   curRPProfit = curRPAmount - averagePrice;
												   }
												   //现在加权平均价 (以历史5次进货价的平均价为参考成本价)
												   else if (curSpt.CostingCalCulateMethod == CostingCalCulateMethod.WeightedAveragePriceNow)
												   {
													   //现在加权平均价
													   var num = companySetting.AveragePurchasePriceCalcNumber;
													   var averagePrice = _productService.GetNowWeightedAveragePrice(pid, num == 0 ? 5 : num);
													   //重新计算利润
													   curSPProfit = curSPAmount - averagePrice;
													   curRPProfit = curRPAmount - averagePrice;

												   }
												   //现在预设进价（现在商品预设进价）
												   else if (curSpt.CostingCalCulateMethod == CostingCalCulateMethod.PresetPurchasePriceNow)
												   {
													   var curProduct = _productService.GetProductById(storeId, pid);
													   var averagePrice = curProduct.ProductPrices.Where(s => s.UnitId == curProduct.SmallUnitId).Select(s => s.PurchasePrice).FirstOrDefault() ?? 0;
													   //重新计算利润
													   curSPProfit = curSPAmount - averagePrice;
													   curRPProfit = curRPAmount - averagePrice;
												   }

												   //========================================

												   //销售提成比例(利润额百分比)
												   var sp = curSpt.SalesPercent ?? 0;
												   //退货提成比例(利润额百分比)
												   var rp = curSpt.ReturnPercent ?? 0;
												   //销售提成 = 当前销售商品利润额 * 销售提成比例 / 100;
												   item.PercentageSale = curSPProfit * (sp / 100);

												   //是否退货参与提成
												   if (curSpt.IsReturnCalCulated)
												   {
													   //退货提成 = 当前退货商品利润额 * 退货提成比例 / 100;
													   item.PercentageReturn = curRPProfit * (rp / 100);
													   //提成合计
													   item.PercentageTotal = item.PercentageSale + item.PercentageReturn;
												   }
												   else
												   {
													   item.PercentageReturn = 0;
													   item.PercentageTotal = item.PercentageSale;
												   }
											   }
											   break;
										   //利润额变化百分比
										   case PercentageCalCulateMethod.ProfitChangePercentage:
											   {
												   //取区间 PercentageRangeOptions
												   //成本计算方式 CostingCalCulateMethod

												   curSPAmount = 0;
												   curRPAmount = 0;

												   //重新计算成本
												   //当时加权平均价(历史成本价)
												   if (curSpt.CostingCalCulateMethod == CostingCalCulateMethod.WeightedAveragePriceTime)
												   {
													   var averagePrice = curSPCostPrice;
													   //重新计算利润
													   curSPProfit = curSPAmount - averagePrice;
													   curRPProfit = curRPAmount - averagePrice;
												   }
												   //现在加权平均价 (以历史5次进货价的平均价为参考成本价)
												   else if (curSpt.CostingCalCulateMethod == CostingCalCulateMethod.WeightedAveragePriceNow)
												   {
													   //现在加权平均价
													   var num = companySetting.AveragePurchasePriceCalcNumber;
													   var averagePrice = _productService.GetNowWeightedAveragePrice(pid, num == 0 ? 5 : num);
													   //重新计算利润
													   curSPProfit = curSPAmount - averagePrice;
													   curRPProfit = curRPAmount - averagePrice;

												   }
												   //现在预设进价（现在商品预设进价）
												   else if (curSpt.CostingCalCulateMethod == CostingCalCulateMethod.PresetPurchasePriceNow)
												   {
													   var curProduct = _productService.GetProductById(storeId, pid);
													   var averagePrice = curProduct.ProductPrices.Where(s => s.UnitId == curProduct.SmallUnitId).Select(s => s.PurchasePrice).FirstOrDefault() ?? 0;
													   //重新计算利润
													   curSPProfit = curSPAmount - averagePrice;
													   curRPProfit = curRPAmount - averagePrice;
												   }

												   //========================================
												   var rangs = curSpt.PercentageRangeOptions;

												   //净销售额区间范围
												   var quantitySaleNet = curSPAmount - curRPAmount;
												   //销售提成比例(利润额百分比)
												   decimal sp = 0;
												   //退货提成比例(利润额百分比)
												   decimal rp = 0;
												   foreach (var ra in rangs)
												   {
													   if (ra.NetSalesRange <= quantitySaleNet)
													   {
														   //销售提成(元)
														   sp = ra.SalesPercent ?? 0;
														   //退货提成(元)
														   rp = ra.ReturnPercent ?? 0;
													   }
													   item.SaleFragment = $"{(ra.NetSalesRange ?? 0).ToString("0.00")} 元以上";
												   }

												   //========================================

												   //销售提成 = 当前销售商品利润额 * 销售提成比例 / 100;
												   item.PercentageSale = curSPProfit * (sp / 100);

												   //是否退货参与提成
												   if (curSpt.IsReturnCalCulated)
												   {
													   //退货提成 = 当前退货商品利润额 * 退货提成比例 / 100;
													   item.PercentageReturn = curRPProfit * (rp / 100);
													   //提成合计
													   item.PercentageTotal = item.PercentageSale + item.PercentageReturn;
												   }
												   else
												   {
													   item.PercentageReturn = 0;
													   item.PercentageTotal = item.PercentageSale;
												   }
											   }
											   break;
										   //利润额分段变化百分比
										   case PercentageCalCulateMethod.PercentageChangeInProfit:
											   {
												   //取区间 PercentageRangeOptions
												   //成本计算方式 CostingCalCulateMethod

												   curSPAmount = 0;
												   curRPAmount = 0;

												   //重新计算成本
												   //当时加权平均价(历史成本价)
												   if (curSpt.CostingCalCulateMethod == CostingCalCulateMethod.WeightedAveragePriceTime)
												   {
													   var averagePrice = curSPCostPrice;
													   //重新计算利润
													   curSPProfit = curSPAmount - averagePrice;
													   curRPProfit = curRPAmount - averagePrice;
												   }
												   //现在加权平均价 (以历史5次进货价的平均价为参考成本价)
												   else if (curSpt.CostingCalCulateMethod == CostingCalCulateMethod.WeightedAveragePriceNow)
												   {
													   //现在加权平均价
													   var num = companySetting.AveragePurchasePriceCalcNumber;
													   var averagePrice = _productService.GetNowWeightedAveragePrice(pid, num == 0 ? 5 : num);
													   //重新计算利润
													   curSPProfit = curSPAmount - averagePrice;
													   curRPProfit = curRPAmount - averagePrice;

												   }
												   //现在预设进价（现在商品预设进价）
												   else if (curSpt.CostingCalCulateMethod == CostingCalCulateMethod.PresetPurchasePriceNow)
												   {
													   var curProduct = _productService.GetProductById(storeId, pid);
													   var averagePrice = curProduct.ProductPrices.Where(s => s.UnitId == curProduct.SmallUnitId).Select(s => s.PurchasePrice).FirstOrDefault() ?? 0;
													   //重新计算利润
													   curSPProfit = curSPAmount - averagePrice;
													   curRPProfit = curRPAmount - averagePrice;
												   }

												   //========================================
												   var rangs = curSpt.PercentageRangeOptions;

												   //净销售额区间范围
												   var quantitySaleNet = curSPAmount - curRPAmount;
												   //销售提成比例(利润额百分比)
												   decimal sp = 0;
												   //退货提成比例(利润额百分比)
												   decimal rp = 0;
												   foreach (var ra in rangs)
												   {
													   if (ra.NetSalesRange <= quantitySaleNet)
													   {
														   //销售提成(元)
														   sp = ra.SalesPercent ?? 0;
														   //退货提成(元)
														   rp = ra.ReturnPercent ?? 0;
													   }
													   item.SaleFragment = $"{(ra.NetSalesRange ?? 0).ToString("0.00")} 元以上";
												   }

												   //========================================

												   //销售提成 = 当前销售商品利润额 * 销售提成比例 / 100;
												   item.PercentageSale = curSPProfit * (sp / 100);

												   //是否退货参与提成
												   if (curSpt.IsReturnCalCulated)
												   {
													   //退货提成 = 当前退货商品利润额 * 退货提成比例 / 100;
													   item.PercentageReturn = curRPProfit * (rp / 100);
													   //提成合计
													   item.PercentageTotal = item.PercentageSale + item.PercentageReturn;
												   }
												   else
												   {
													   item.PercentageReturn = 0;
													   item.PercentageTotal = item.PercentageSale;
												   }
											   }
											   break;
										   default:
											   break;
									   }


									   //净销售量
									   item.QuantityNetSaleQuantity = curSPQty - curRPQty;
									   //销售量
									   item.QuantitySale = curSPQty;
									   //退货量
									   item.QuantityReturn = curRPQty;

									   //销售净额
									   item.AmountSaleNet = curSPAmount - curRPAmount;
									   //销售额
									   item.AmountSale = curSPAmount;
									   //退货额
									   item.AmountReturn = curRPAmount;

									   //净利润
									   item.ProfitNet = curSPProfit - curRPProfit;
									   //销售利润
									   item.ProfitSale = curSPProfit;
									   //退货利润
									   item.ProfitReturn = curRPProfit;
								   }

								   summ.DItems.Add(item);
							   }

							   #endregion


							   //业务提成
							   summ.BusinessPercentage = summ.SItems.Select(s => s.PercentageTotal).Sum();
							   //送货提成
							   summ.DeliveryPercentage = summ.DItems.Select(s => s.PercentageTotal).Sum();
							   //提成合计
							   summ.PercentageTotal = summ.BusinessPercentage + summ.DeliveryPercentage;
						   }

					   }
					   catch (Exception ex)
					   {
						   System.Diagnostics.Debug.Print(ex.Message);
					   }

					   return summaries;

				   });
			}
			catch (Exception)
			{
				return new List<StaffReportPercentageSummary>();
			}
		}


		/// <summary>
		/// 提成明细表
		/// </summary>
		/// <param name="storeId">经销商Id</param>
		/// <param name="userType">用户类型（业务员、送货员）</param>
		/// <param name="startTime">开始日期</param>
		/// <param name="endTime">结束日期</param>
		/// <param name="staffUserId">员工Id</param>
		/// <param name="categoryId">商品类别Id</param>
		/// <param name="productId">商品Id</param>
		/// <returns></returns>
		public IList<StaffReportPercentageItem> GetStaffReportPercentageItem(int? storeId, int userType, DateTime? startTime, DateTime? endTime, int? staffUserId, int? categoryId, int? productId)
		{
			try
			{
				var reporting = new List<StaffReportPercentageItem>();
				var summery = GetStaffReportPercentageSummary(storeId, startTime, endTime, staffUserId, categoryId, productId);
				if (summery != null)
				{
					var cursetect = summery.Where(s => s.StaffUserId == staffUserId).FirstOrDefault();
					if (cursetect != null)
					{
						if (userType == 1)
						{
							reporting = cursetect.SItems.ToList();
						}
						else if (userType == 2)
						{
							reporting = cursetect.DItems.ToList();
						}
					}
				}

				return _cacheManager.Get(DCMSDefaults.GETSTAFFREPORT_PERCENTAGE_ITEM_KEY.FillCacheKey(storeId, userType, startTime, endTime, staffUserId, categoryId, productId), () => reporting);
			}
			catch (Exception)
			{
				return new List<StaffReportPercentageItem>();
			}
		}


		/// <summary>
		/// 业务员业绩
		/// </summary>
		/// <param name="storeId"></param>
		/// <param name="businessUserId"></param>
		/// <param name="startTime"></param>
		/// <param name="endTime"></param>
		/// <returns></returns>
		public IList<StaffSaleQuery> StaffSaleQuery(int? storeId, int? businessUserId, DateTime? startTime = null, DateTime? endTime = null)
		{
			try
			{
				string whereQuery = $"sb.StoreId = {storeId} AND sao.StoreId = {storeId} AND sbi.IsGifts = 0 AND sb.AuditedStatus = 1 AND sb.ReversedStatus = 0 ";

				if (businessUserId.HasValue && businessUserId.Value > 0)
				{
					whereQuery += $" AND sb.BusinessUserId = '{businessUserId}' ";
				}

				if (startTime.HasValue)
				{
					string startTimedata = startTime.Value.ToString("yyyy-MM-dd 00:00:00");
					whereQuery += $" AND sb.CreatedOnUtc >= '{startTimedata}'";
				}

				if (endTime.HasValue)
				{
					string endTimedata = endTime.Value.ToString("yyyy-MM-dd 23:59:59");
					whereQuery += $" AND sb.CreatedOnUtc <= '{endTimedata}'";
				}

				string sqlString = @"(SELECT 
					sb.BusinessUserId,
					pr.Name,
					CONCAT('1*',pr.BigQuantity) as BigQuantity,
					sbi.UnitId,
					sao.Name as UnitName,
					CONCAT('',sbi.Quantity) as Quantity,
					sbi.Price,
					sbi.Amount,
					sb.CreatedOnUtc
				FROM
					dcms.SaleBills AS sb
						LEFT JOIN
					dcms.SaleItems AS sbi ON sb.Id = sbi.SaleBillId
						LEFT JOIN
					dcms.SpecificationAttributeOptions AS sao ON sbi.UnitId = sao.Id
						LEFT JOIN
					dcms.Products AS pr ON sbi.ProductId = pr.Id
				WHERE {0} ) union (
						SELECT 
					sb.BusinessUserId,
					pr.Name,
					CONCAT('1*',pr.BigQuantity) as BigQuantity,
					sbi.UnitId,
					sao.Name as UnitName,
					CONCAT('-',sbi.Quantity) as Quantity,
					sbi.Price,
					sbi.Amount,
					sb.CreatedOnUtc
				FROM
					dcms.ReturnBills AS sb
						LEFT JOIN
					dcms.ReturnItems AS sbi ON sb.Id = sbi.ReturnBillId
						LEFT JOIN
					dcms.SpecificationAttributeOptions AS sao ON sbi.UnitId = sao.Id
						LEFT JOIN
					dcms.Products AS pr ON sbi.ProductId = pr.Id
				WHERE {0} )";

				var query = SaleBillsRepository_RO.QueryFromSql<StaffSaleQuery>(string.Format(sqlString, whereQuery)).ToList();

				return query;
			}
			catch (Exception)
			{
				return new List<StaffSaleQuery>();
			}
		}


		/// <summary>
		/// 拜访汇总
		/// </summary>
		/// <param name="type"></param>visitsummery
		/// <param name="storeId"></param>
		/// <param name="businessUserId"></param>
		/// <returns></returns>
		public IList<VisitSummeryQuery> GetVisitSummeryQuery(int type, int? storeId, int? businessUserId, DateTime? start = null, DateTime? end = null)
		{
			try
			{
				var s1 = $" and to_days(now()) = to_days(v.SigninDateTime) ";
				var s2 = $" and to_days(now()) = to_days(ct.CreatedOnUtc) ";

				if (type == 0 && start.HasValue && end.HasValue)
				{
					s1 = $" and v.SigninDateTime >= '" + start?.ToString("yyyy-MM-dd 00:00:00") + "' and v.SigninDateTime <= '" + end?.ToString("yyyy-MM-dd 23:59:59") + "' ";
					s2 = $"  and ct.CreatedOnUtc >= '" + start?.ToString("yyyy-MM-dd 00:00:00") + "' and ct.CreatedOnUtc <= '" + end?.ToString("yyyy-MM-dd 23:59:59") + "' ";
				}
				else
				{
					switch (type)
					{
						//今日
						case 1:
							{
								s1 = $" and to_days(now()) = to_days(v.SigninDateTime) ";
								s2 = $" and to_days(now()) = to_days(ct.CreatedOnUtc) ";
							}
							break;
						//昨天
						case 3:
							{
								s1 = $" and to_days(now()) - to_days(v.SigninDateTime) = 1 ";
								s2 = $" and to_days(now()) - to_days(ct.CreatedOnUtc) = 1 ";
							}
							break;
						//前天
						case 4:
							{
								s1 = $" and to_days(now()) - to_days(v.SigninDateTime) = 2 ";
								s2 = $" and to_days(now()) - to_days(ct.CreatedOnUtc) = 2 ";
							}
							break;
						//上周
						case 5:
							{
								s1 = $" and YEARWEEK(date_format(v.SigninDateTime,'%Y-%m-%d')) = YEARWEEK(now())-1 ";
								s2 = $" and YEARWEEK(date_format(ct.CreatedOnUtc,'%Y-%m-%d')) = YEARWEEK(now())-1 ";
							}
							break;
						//本周
						case 6:
							{
								s1 = $" and YEARWEEK(date_format(v.SigninDateTime,'%Y-%m-%d')) = YEARWEEK(now()) ";
								s2 = $" and YEARWEEK(date_format(ct.CreatedOnUtc,'%Y-%m-%d')) = YEARWEEK(now()) ";
							}
							break;
						//上月
						case 7:
							{
								s1 = $" and date_format(v.SigninDateTime,'%Y-%m')=date_format(DATE_SUB(curdate(), INTERVAL 1 MONTH),'%Y-%m') ";
								s2 = $" and date_format(ct.CreatedOnUtc,'%Y-%m')=date_format(DATE_SUB(curdate(), INTERVAL 1 MONTH),'%Y-%m') ";
							}
							break;
						//本月
						case 8:
							{
								s1 = $" and date_format(v.SigninDateTime,'%Y-%m')=date_format(now(),'%Y-%m') ";
								s2 = $" and date_format(ct.CreatedOnUtc,'%Y-%m')=date_format(now(),'%Y-%m') ";
							}
							break;
						//本年
						case 9:
							{
								s1 = $" and YEAR(v.SigninDateTime)=YEAR(NOW()) ";
								s2 = $" and YEAR(ct.CreatedOnUtc)=YEAR(NOW()) ";
							}
							break;
					}
				}

				string sqlString = @"SELECT 
						u.Id AS UserId,
						u.UserRealName AS UserName,
						(SELECT 
								COUNT(1)
							FROM
								dcms.VisitStore AS v
							WHERE
								v.StoreId = {2}
									AND v.BusinessUserId = u.id
									{0}) AS VistCount,
						(SELECT 
								COUNT(1)
							FROM
								dcms_crm.CRM_Terminals AS ct
							WHERE
								ct.StoreId = {2}
									AND (ct.CreatedUserId = u.id and ct.IsNewAdd = 1)
									{1} ) AS NewAddCount,
						(SELECT 
								COUNT(1)
							FROM
								census.DoorheadPhoto AS sp
							WHERE
								sp.StoreId = {2}
									AND sp.VisitStoreId IN (SELECT 
										v.Id
									FROM
										dcms.VisitStore AS v
									WHERE
										v.StoreId = {2}
											AND v.BusinessUserId = u.id
											{0} )) AS DoorheadPhotoCount,
						(SELECT 
								COUNT(1)
							FROM
								census.DisplayPhoto AS sp
							WHERE
								sp.StoreId = {2}
									AND sp.VisitStoreId IN (SELECT 
										v.Id
									FROM
										dcms.VisitStore AS v
									WHERE
										v.StoreId = {2}
											AND v.BusinessUserId = u.id
											{0} )) AS DisplayPhotoCount
					FROM
						auth.User_UserRole_Mapping AS urm
							LEFT JOIN
						auth.Users AS u ON u.id = urm.UserId
							LEFT JOIN
						auth.UserRoles AS ur ON urm.UserRoleId = ur.id
					WHERE
						u.StoreId = {2} AND ur.SystemName = 'Salesmans' ";


				if (businessUserId.HasValue && businessUserId>0)
				{
					sqlString += $" AND userId = {businessUserId} ";
				}

				 sqlString +="  GROUP BY u.Id, u.UserRealName , VistCount , NewAddCount , DoorheadPhotoCount , DisplayPhotoCount";


				var result = SaleBillsRepository
					.QueryFromSql<VisitSummeryQuery>(string.Format(sqlString, s1, s2, storeId))
					.ToList();

				return result;
			}
			catch (Exception)
			{
				return new List<VisitSummeryQuery>();
			}
		}

		public IList<BusinessUserVisitOfYear> GetBusinessUserVisitOfYearList(int? storeId,int year,int month) 
		{
			try
			{
				//获取月份天数
				var daysCount = DateTime.DaysInMonth(year, month);
				StringBuilder sb = new StringBuilder();
				sb.Append("SELECT u.Id AS UserId,u.UserRealName AS UserName");
				for (int i = 1; i <= daysCount; i++)
				{
					sb.Append(",(SELECT COUNT(1) FROM dcms.VisitStore AS v WHERE ");
					sb.AppendFormat(" v.StoreId = {0} AND v.BusinessUserId = u.id and to_days(v.SigninDateTime) = to_days('{1}-{2}-{3}')", storeId,year,month,i);
					sb.Append($") AS Days{i}");
				}
				for (int i = daysCount+1; i <= 31; i++)
				{
					sb.Append($",0 AS Days{i}");
				}
				sb.Append(",(SELECT COUNT(1) FROM dcms.VisitStore AS v WHERE ");
				sb.AppendFormat(" v.StoreId = {0} AND v.BusinessUserId = u.id and DATE_FORMAT(v.SigninDateTime,'%Y%m')=DATE_FORMAT('{1}-{2}-01','%Y%m')) AS Total", storeId,year,month);
				sb.Append(" FROM auth.User_UserRole_Mapping AS urm");
				sb.Append(" LEFT JOIN auth.Users AS u ON u.id = urm.UserId");
				sb.Append(" LEFT JOIN auth.UserRoles AS ur ON urm.UserRoleId = ur.id");
				sb.AppendFormat(" WHERE u.StoreId = {0} AND ur.SystemName = 'Salesmans'",storeId);
				var result = SaleBillsRepository
					.QueryFromSql<BusinessUserVisitOfYear>(sb.ToString())
					.OrderByDescending(o=>o.Total)
					.ToList();
				return result;
			}
			catch (Exception ex)
			{
				return new List<BusinessUserVisitOfYear>();
			}
		}

		private decimal GetBusinessUserReceipt(int storeId,int businessUserId,DateTime? startDate,DateTime? endDate) 
		{
			try
			{
				decimal receiptAmount = 0;
				//获取销售单收款
				var saleBills = SaleBillsRepository_RO.Table.Where(w => w.StoreId == storeId && w.BusinessUserId == businessUserId && w.AuditedStatus == true && w.ReversedStatus == false);
				//获取退货单
				var returnBills = ReturnBillsRepository_RO.Table.Where(w=>w.StoreId==storeId && w.BusinessUserId == businessUserId && w.AuditedStatus == true && w.ReversedStatus == false);
				//获取收款单收款
				var receiptBills = CashReceiptBillsRepository_RO.Table.Where(w => w.StoreId == storeId && w.MakeUserId == businessUserId && w.AuditedStatus == true && w.ReversedStatus == false);
				//获取预收款单
				var advanceBills = AdvancePaymentBillsRepository_RO.Table.Where(w => w.StoreId == storeId && w.MakeUserId == businessUserId && w.AuditedStatus == true && w.ReversedStatus == false);
				if (startDate.HasValue) 
				{
					saleBills = saleBills.Where(w=>w.AuditedDate >= startDate);
					returnBills = returnBills.Where(w => w.AuditedDate >= startDate);
					receiptBills = receiptBills.Where(w=>w.AuditedDate >= startDate);
					advanceBills = advanceBills.Where(w => w.AuditedDate >= startDate);
				}
				if (endDate.HasValue) 
				{
					saleBills = saleBills.Where(w => w.AuditedDate <= Convert.ToDateTime(endDate.Value.ToString("yyyy-MM-dd 23:59:59")));
					returnBills = returnBills.Where(w => w.AuditedDate <= Convert.ToDateTime(endDate.Value.ToString("yyyy-MM-dd 23:59:59")));
					receiptBills = receiptBills.Where(w => w.AuditedDate <= Convert.ToDateTime(endDate.Value.ToString("yyyy-MM-dd 23:59:59")));
					advanceBills = advanceBills.Where(w => w.AuditedDate <= Convert.ToDateTime(endDate.Value.ToString("yyyy-MM-dd 23:59:59")));
				}
				receiptAmount += saleBills.Select(s => s.ReceivableAmount - s.OweCash).Sum();
				receiptAmount -= returnBills.Select(s=>s.SumAmount).Sum();
				receiptAmount += receiptBills.Select(s=>s.ReceivableAmount).Sum();
				receiptAmount += advanceBills.Select(s => s.AdvanceAmount).Sum()??0;
				//获取费用支出

				//获取其他收入

				return receiptAmount;
			}
			catch (Exception ex)
			{
				return 0;
			}
		}

		private decimal GetBusinessUserSaleOweCash(int storeId,int businessUserId,DateTime? startDate,DateTime? endDate) 
		{
			try
			{
				decimal OweCash = 0;
				//获取销售单收款
				var saleBills = SaleBillsRepository_RO.Table.Where(w => w.StoreId == storeId && w.BusinessUserId == businessUserId && w.AuditedStatus == true && w.ReversedStatus == false && w.ReceiptStatus < 2);
				//获取单据收款
				var receptItems = from a in CashReceiptItemsRepository_RO.Table
								  join b in CashReceiptBillsRepository_RO.Table on a.CashReceiptBillId equals b.Id
								  where b.StoreId == storeId && b.MakeUserId == businessUserId && b.AuditedStatus == true && b.ReversedStatus == false && saleBills.Select(s => s.Id).Contains(a.BillId)
								  select new { b.CreatedOnUtc, a.ReceivableAmountOnce };
				if (startDate.HasValue) 
				{
					saleBills = saleBills.Where(w=>w.CreatedOnUtc >= startDate);
					receptItems = receptItems.Where(w=>w.CreatedOnUtc >= startDate);
				}
				if (endDate.HasValue)
				{
					saleBills = saleBills.Where(w => w.CreatedOnUtc <= Convert.ToDateTime(endDate.Value.ToString("yyyy-MM-dd 23:59:59")));
					receptItems = receptItems.Where(w => w.CreatedOnUtc <= Convert.ToDateTime(endDate.Value.ToString("yyyy-MM-dd 23:59:59")));
				}
				OweCash += saleBills.Select(s => s.OweCash).Sum();
				OweCash -= receptItems.Select(s => s.ReceivableAmountOnce).Sum() ?? 0;
				return OweCash;
			}
			catch (Exception ex)
			{
				return 0;
			}
		}
	}
}
