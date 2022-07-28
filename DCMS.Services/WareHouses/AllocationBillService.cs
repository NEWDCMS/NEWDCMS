using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Data;
using DCMS.Core.Domain.Sales;
using DCMS.Core.Domain.Tasks;
using DCMS.Core.Domain.WareHouses;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Configuration;
using DCMS.Services.Events;
using DCMS.Services.Products;
using DCMS.Services.Tasks;
using DCMS.Services.Users;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using DbF = Microsoft.EntityFrameworkCore.EF;
using DCMS.Services.Caching;

namespace DCMS.Services.WareHouses
{
	/// <summary>
	/// 用于表示调拨单服务
	/// </summary>
	public partial class AllocationBillService : BaseService, IAllocationBillService
	{

		private readonly IStockService _stockService;
		private readonly IProductService _productService;
		private readonly ISpecificationAttributeService _specificationAttributeService;
		private readonly ISettingService _settingService;
		private readonly IUserService _userService;
		private readonly IQueuedMessageService _queuedMessageService;


		public AllocationBillService(IServiceGetter getter,
			IStaticCacheManager cacheManager,
			IEventPublisher eventPublisher,
			IStockService stockService,
			IProductService productService,
			ISpecificationAttributeService specificationAttributeService,
			ISettingService settingService,
			IUserService userService,
			IQueuedMessageService queuedMessageService
			) : base(getter, cacheManager, eventPublisher)
		{
			_stockService = stockService;
			_productService = productService;
			_specificationAttributeService = specificationAttributeService;
			_settingService = settingService;
			_userService = userService;
			_queuedMessageService = queuedMessageService;
		}


		#region 单据

		public bool Exists(int billId)
		{
			return AllocationBillsRepository.TableNoTracking.Where(a => a.Id == billId).Count() > 0;
		}

		public virtual IPagedList<AllocationBill> GetAllAllocationBills(int? store, int? makeuserId, int? businessUserId, int? shipmentWareHouseId, int? IncomeWareHouseId, string billNumber = "", bool? status = null, DateTime? start = null, DateTime? end = null, bool? isShowReverse = null, bool? deleted = null, int? productId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
		{
			if (pageSize >= 50)
				pageSize = 50;

			DateTime.TryParse(start?.ToString("yyyy-MM-dd 00:00:00"), out DateTime startDate);
			DateTime.TryParse(end?.ToString("yyyy-MM-dd 23:59:59"), out DateTime endDate);


			var query = from ab in AllocationBillsRepository.Table
						join b in AllocationItemsRepository.Table on ab.Id equals b.AllocationBillId
						select ab;

			if (store.HasValue && store != 0)
			{
				query = query.Where(c => c.StoreId == store);
			}

			if (productId.HasValue && productId > 0)
			{
				query = query.Where(a => a.Items.Where(s => s.ProductId == productId).Any());
			}

			if (makeuserId.HasValue && makeuserId > 0)
			{
				var userIds = _userService.GetSubordinate(store, makeuserId ?? 0);
				if (userIds.Count > 0)
				{
					query = query.Where(x => userIds.Contains(x.MakeUserId));
				}
			}

			//业务员为制单人
			if (businessUserId.HasValue && businessUserId > 0)
			{
				query = query.Where(c => c.MakeUserId == businessUserId);
			}

			if (shipmentWareHouseId.HasValue && shipmentWareHouseId.Value > 0)
			{
				query = query.Where(c => c.ShipmentWareHouseId == shipmentWareHouseId);
			}

			if (IncomeWareHouseId.HasValue && IncomeWareHouseId.Value > 0)
			{
				query = query.Where(c => c.IncomeWareHouseId == IncomeWareHouseId);
			}

			if (!string.IsNullOrWhiteSpace(billNumber))
			{
				query = query.Where(c => c.BillNumber.Contains(billNumber));
			}

			if (status.HasValue)
			{
				query = query.Where(c => c.AuditedStatus == status);
			}

			if (start.HasValue)
			{
				query = query.Where(o => startDate <= o.CreatedOnUtc);
			}

			if (end.HasValue)
			{
				query = query.Where(o => endDate >= o.CreatedOnUtc);
			}

			if (isShowReverse.HasValue)
			{
				query = query.Where(c => c.ReversedStatus == isShowReverse);
			}

			if (deleted.HasValue)
			{
				query = query.Where(c => c.Deleted == deleted);
			}

			query = query.OrderByDescending(c => c.CreatedOnUtc);

			var totalCount = query.Count();
			var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
			return new PagedList<AllocationBill>(plists, pageIndex, pageSize, totalCount);
		}

		public virtual IList<AllocationBill> GetAllAllocationBills()
		{
			var query = from c in AllocationBillsRepository.Table
						orderby c.Id
						select c;

			var categories = query.ToList();
			return categories;
		}

		public virtual AllocationBill GetAllocationBillById(int? store, int allocationBillId)
		{
			if (allocationBillId == 0)
			{
				return null;
			}

			var key = DCMSDefaults.ALLOCATIONBILL_BY_ID_KEY.FillCacheKey(store ?? 0, allocationBillId);
			return _cacheManager.Get(key, () =>
			{
				return AllocationBillsRepository.ToCachedGetById(allocationBillId);
			});
		}

		public virtual AllocationBill GetAllocationBillById(int? store, int allocationBillId, bool isInclude = false)
		{
			if (allocationBillId == 0)
			{
				return null;
			}

			if (isInclude)
			{
				var query = AllocationBillsRepository.TableNoTracking.Include(ab => ab.Items);
				return query.FirstOrDefault(a => a.Id == allocationBillId);
			}
			return AllocationBillsRepository.ToCachedGetById(allocationBillId);
		}


		public virtual AllocationBill GetAllocationBillByNumber(int? store, string billNumber)
		{
			var key = DCMSDefaults.ALLOCATIONBILL_BY_NUMBER_KEY.FillCacheKey(store ?? 0, billNumber);
			return _cacheManager.Get(key, () =>
			{
				var query = AllocationBillsRepository.Table;
				var bill = query.Where(a => a.StoreId == store && a.BillNumber == billNumber).FirstOrDefault();
				return bill;
			});
		}



		public virtual void InsertAllocationBill(AllocationBill bill)
		{
			if (bill == null)
			{
				throw new ArgumentNullException("bill");
			}

			var uow = AllocationBillsRepository.UnitOfWork;
			AllocationBillsRepository.Insert(bill);
			uow.SaveChanges();

			//通知
			_eventPublisher.EntityInserted(bill);
		}

		public virtual void UpdateAllocationBill(AllocationBill bill)
		{
			if (bill == null)
			{
				throw new ArgumentNullException("bill");
			}

			var uow = AllocationBillsRepository.UnitOfWork;
			AllocationBillsRepository.Update(bill);
			uow.SaveChanges();

			//通知
			_eventPublisher.EntityUpdated(bill);
		}

		public virtual void DeleteAllocationBill(AllocationBill bill)
		{
			if (bill == null)
			{
				throw new ArgumentNullException("bill");
			}

			var uow = AllocationBillsRepository.UnitOfWork;
			AllocationBillsRepository.Delete(bill);
			uow.SaveChanges();

			//event notification
			_eventPublisher.EntityDeleted(bill);
		}


		#endregion

		#region 单据项目


		public virtual IPagedList<AllocationItem> GetAllocationItemsByAllocationBillId(int allocationBillId, int? userId, int? storeId, int pageIndex, int pageSize)
		{
			if (pageSize >= 50)
				pageSize = 50;
			if (allocationBillId == 0)
			{
				return new PagedList<AllocationItem>(new List<AllocationItem>(), pageIndex, pageSize);
			}

			var key = DCMSDefaults.ALLOCATIONBILLITEM_ALL_KEY.FillCacheKey(storeId, allocationBillId, pageIndex, pageSize, userId);

			return _cacheManager.Get(key, () =>
			{
				var query = from pc in AllocationItemsRepository.Table
							.Include(a => a.AllocationBill)
							where pc.AllocationBillId == allocationBillId
							orderby pc.Id
							select pc;
				//var productAllocationBills = new PagedList<AllocationItem>(query.ToList(), pageIndex, pageSize);
				//return productAllocationBills;
				//总页数
				var totalCount = query.Count();
				var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
				return new PagedList<AllocationItem>(plists, pageIndex, pageSize, totalCount);
			});
		}

		public virtual List<AllocationItem> GetAllocationItemList(int allocationBillId)
		{
			List<AllocationItem> allocationItems = null;
			var query = AllocationItemsRepository.Table.Include(a=>a.AllocationBill);
			allocationItems = query.Where(a => a.AllocationBillId == allocationBillId).ToList();
			return allocationItems;
		}


		public virtual AllocationItem GetAllocationItemById(int? store, int allocationItemId)
		{
			if (allocationItemId == 0)
			{
				return null;
			}
			return AllocationItemsRepository.ToCachedGetById(allocationItemId);
		}

		public virtual void InsertAllocationItem(AllocationItem allocationItem)
		{
			if (allocationItem == null)
			{
				throw new ArgumentNullException("allocationItem");
			}

			var uow = AllocationItemsRepository.UnitOfWork;
			AllocationItemsRepository.Insert(allocationItem);
			uow.SaveChanges();

			//通知
			_eventPublisher.EntityInserted(allocationItem);
		}

		public virtual void UpdateAllocationItem(AllocationItem allocationItem)
		{
			if (allocationItem == null)
			{
				throw new ArgumentNullException("allocationItem");
			}

			var uow = AllocationItemsRepository.UnitOfWork;
			AllocationItemsRepository.Update(allocationItem);
			uow.SaveChanges();

			//通知
			_eventPublisher.EntityUpdated(allocationItem);
		}

		public virtual void DeleteAllocationItem(AllocationItem allocationItem)
		{
			if (allocationItem == null)
			{
				throw new ArgumentNullException("allocationItem");
			}

			var uow = AllocationItemsRepository.UnitOfWork;
			AllocationItemsRepository.Delete(allocationItem);
			uow.SaveChanges();

			//通知
			_eventPublisher.EntityDeleted(allocationItem);
		}


		#endregion


		/// <summary>
		/// 快速调拨
		/// </summary>
		/// <param name="storeId"></param>
		/// <param name="allocationType">快速调拨类型</param>
		/// <param name="wareHouseId"></param>
		/// <param name="deliveryUserId"></param>
		/// <param name="categoryIds"></param>
		/// <param name="loadDataNameIds">快速调拨获取方式</param>
		/// <returns></returns>
		public IList<QuickAllocationItem> GetQuickAllocation(int? storeId, int allocationType, int wareHouseId, int deliveryUserId, string categoryIds, string loadDataNameIds)
		{

			#region
			// var parameters = new SqlParameter[]
			//{
			//     new SqlParameter{ ParameterName = "@storeId",DbType = System.Data.DbType.Int32,Value = StoreId },
			//     new SqlParameter{ ParameterName = "@allocationType",DbType = System.Data.DbType.Int32,  Value = allocationType },
			//     new SqlParameter{ ParameterName = "@wareHouseId",DbType = System.Data.DbType.Int32,Value = wareHouseId },
			//     new SqlParameter{ ParameterName = "@deliveryUserId",DbType = System.Data.DbType.Int32, Value = deliveryUserId },
			//     new SqlParameter{ ParameterName = "@categoryIds",DbType = System.Data.DbType.String, Value = (categoryIds==null?"":categoryIds) },
			//     new SqlParameter{ ParameterName = "@loadDataNameIds",DbType = System.Data.DbType.String, Value = (loadDataNameIds==null?"":loadDataNameIds) },
			//};
			// var items = AllocationBillsRepository.ExecuteStoredProcedure<QuickAllocationItem>("getQuickAllocation " +
			//     "@storeId,@allocationType,@wareHouseId,@deliveryUserId,@categoryIds,@loadDataNameIds", parameters);
			// return items.ToList();
			#endregion

			var products = new List<QuickAllocationItem>();
			//var loads = loadDataNameIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => int.Parse(s)).ToList();

			var loads = new List<string>();
			if (!string.IsNullOrEmpty(loadDataNameIds))
			{
				loads = loadDataNameIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s).ToList();
			}
			var caIds = new List<int>();
			if (!string.IsNullOrEmpty(categoryIds))
			{
				caIds = categoryIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => int.Parse(s)).ToList();
			}

			switch ((AllocationTypeEnum)allocationType)
			{
				//按拒收商品调拨
				case AllocationTypeEnum.ByRejection:
					{
						List<SaleReservationItem> items = new List<SaleReservationItem>();
						List<SaleReservationItem> items1 = new List<SaleReservationItem>();
						List<SaleReservationItem> items2 = new List<SaleReservationItem>();
						List<SaleReservationItem> items3 = new List<SaleReservationItem>();

						//加载今天拒收的商品
						if (loads.Contains(QuickAllocationEnum.LoadRejectionToday.ToString()))
						{
							var query1 = from a in DispatchItemsRepository.Table
										 join b in SaleReservationBillsRepository.Table on a.BillId equals b.Id
										 join c in SaleReservationItemsRepository.Table on b.Id equals c.SaleReservationBillId
										 where a.StoreId == storeId && b.DeliveryUserId == deliveryUserId && b.WareHouseId == wareHouseId && a.BillTypeId == (int)BillTypeEnum.SaleReservationBill && a.SignStatus == (int)SignStatusEnum.Rejection
										 select c;
							//query = query.Where(s => SqlFunctions.DateDiff("dd", s.CreatedOnUtc, DateTime.Now).Value == 0);
							query1 = query1.AsEnumerable().Where(s => MySqlDbFunctionsExtensions.DateDiffDay(DbF.Functions, s.CreatedOnUtc, DateTime.Now) == 0).AsQueryable();
							items1 = query1.ToList();
						}
						//加载昨天拒收的商品
						if (loads.Contains(QuickAllocationEnum.LoadRejectionYestDay.ToString()))
						{
							var query2 = from a in DispatchItemsRepository.Table
										 join b in SaleReservationBillsRepository.Table on a.BillId equals b.Id
										 join c in SaleReservationItemsRepository.Table on b.Id equals c.SaleReservationBillId
										 where a.StoreId == storeId && b.DeliveryUserId == deliveryUserId && b.WareHouseId == wareHouseId && a.BillTypeId == (int)BillTypeEnum.SaleReservationBill && a.SignStatus == (int)SignStatusEnum.Rejection
										 select c;
							//query = query.Where(s => SqlFunctions.DateDiff("dd", s.CreatedOnUtc, DateTime.Now).Value == 1);
							query2 = query2.AsEnumerable().Where(s => MySqlDbFunctionsExtensions.DateDiffDay(DbF.Functions, s.CreatedOnUtc, DateTime.Now) == 1).AsQueryable();
							items2 = query2.ToList();
						}
						//加载前天拒收的商品
						if (loads.Contains(QuickAllocationEnum.LoadRejectionBeforeYestday.ToString()))
						{
							var query3 = from a in DispatchItemsRepository.Table
										 join b in SaleReservationBillsRepository.Table on a.BillId equals b.Id
										 join c in SaleReservationItemsRepository.Table on b.Id equals c.SaleReservationBillId
										 where a.StoreId == storeId && b.DeliveryUserId == deliveryUserId && b.WareHouseId == wareHouseId && a.BillTypeId == (int)BillTypeEnum.SaleReservationBill && a.SignStatus == (int)SignStatusEnum.Rejection
										 select c;
							//query = query.Where(s => SqlFunctions.DateDiff("dd", s.CreatedOnUtc, DateTime.Now).Value == 2);
							query3 = query3.AsEnumerable().Where(s => MySqlDbFunctionsExtensions.DateDiffDay(DbF.Functions, s.CreatedOnUtc, DateTime.Now) == 2).AsQueryable();
							items3 = query3.ToList();
						}

						//去重
						items = items1.Union(items2).Union(items3).Distinct().ToList();
						if (items != null && items.Count > 0)
						{
							var allProducts = _productService.GetProductsByIds(storeId ?? 0, items.Select(it => it.ProductId).Distinct().ToArray());
							var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(storeId ?? 0, allProducts.GetProductBigStrokeSmallUnitIds());

							//将商品按最小单位合并
							foreach (var item in items)
							{
								if (item.ProductId != 0)
								{
									var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
									if (product != null)
									{
										QuickAllocationItem quickAllocationItem = products.Where(a => a.ProductId == item.ProductId).FirstOrDefault();
										//商品转化量
										var conversionQuantity = product.GetConversionQuantity(allOptions, item.UnitId);
										//库存量增量 = 单位转化量 * 数量
										int thisQuantity = item.Quantity * conversionQuantity;
										if (quickAllocationItem != null)
										{
											quickAllocationItem.Quantity += thisQuantity;
										}
										else
										{
											quickAllocationItem = new QuickAllocationItem
											{
												ProductId = item.ProductId,
												Quantity = thisQuantity
											};
											products.Add(quickAllocationItem);
										}
									}
								}
							}
						}

					}
					break;
				//按销补货
				case AllocationTypeEnum.BySaleAdd:
					{

						List<SaleItem> items = new List<SaleItem>();
						List<SaleItem> items1 = new List<SaleItem>();
						List<SaleItem> items2 = new List<SaleItem>();
						List<SaleItem> items3 = new List<SaleItem>();
						//加载今天销售的商品
						if (loads.Contains(QuickAllocationEnum.LoadSaleToday.ToString()))
						{
							var query1 = from a in SaleBillsRepository.Table
										 join b in SaleItemsRepository.Table on a.Id equals b.SaleBillId
										 join c in ProductsRepository.Table on b.ProductId equals c.Id
										 where a.StoreId == storeId && caIds.Contains(c.CategoryId) && a.WareHouseId == wareHouseId && a.AuditedStatus == true && a.ReversedStatus == false
										 select b;

							//query = query.Where(s => SqlFunctions.DateDiff("dd", s.CreatedOnUtc, DateTime.Now).Value == 0);
							query1 = query1.AsEnumerable().Where(s => MySqlDbFunctionsExtensions.DateDiffDay(DbF.Functions, s.CreatedOnUtc, DateTime.Now) == 0).AsQueryable();
							items1 = query1.ToList();
						}

						//加载昨天销售的商品
						if (loads.Contains(QuickAllocationEnum.LoadSaleYestDay.ToString()))
						{
							var query2 = from a in SaleBillsRepository.Table
										 join b in SaleItemsRepository.Table on a.Id equals b.SaleBillId
										 join c in ProductsRepository.Table on b.ProductId equals c.Id
										 where a.StoreId == storeId && caIds.Contains(c.CategoryId) && a.WareHouseId == wareHouseId && a.AuditedStatus == true && a.ReversedStatus == false
										 select b;
							//query = query.Where(s => SqlFunctions.DateDiff("dd", s.CreatedOnUtc, DateTime.Now).Value == 1);
							query2 = query2.AsEnumerable().Where(s => MySqlDbFunctionsExtensions.DateDiffDay(DbF.Functions, s.CreatedOnUtc, DateTime.Now) == 1).AsQueryable();
							items2 = query2.ToList();
						}

						//加载近三天销售的商品
						if (loads.Contains(QuickAllocationEnum.LoadSaleNearlyThreeDays.ToString()))
						{
							var query3 = from a in SaleBillsRepository.Table
										 join b in SaleItemsRepository.Table on a.Id equals b.SaleBillId
										 join c in ProductsRepository.Table on b.ProductId equals c.Id
										 where a.StoreId == storeId && caIds.Contains(c.CategoryId) && a.WareHouseId == wareHouseId && a.AuditedStatus == true && a.ReversedStatus == false
										 select b;
							//query = query.Where(s => SqlFunctions.DateDiff("dd", s.CreatedOnUtc, DateTime.Now).Value == 0 || SqlFunctions.DateDiff("dd", s.CreatedOnUtc, DateTime.Now).Value == 1 || SqlFunctions.DateDiff("dd", s.CreatedOnUtc, DateTime.Now).Value == 2);
							query3 = query3.AsEnumerable().Where(s => MySqlDbFunctionsExtensions.DateDiffDay(DbF.Functions, s.CreatedOnUtc, DateTime.Now) == 0 || MySqlDbFunctionsExtensions.DateDiffDay(DbF.Functions, s.CreatedOnUtc, DateTime.Now) == 1 || MySqlDbFunctionsExtensions.DateDiffDay(DbF.Functions, s.CreatedOnUtc, DateTime.Now) == 2).AsQueryable();
							items3 = query3.ToList();
						}
						//去重
						items = items1.Union(items2).Union(items3).Distinct().ToList();
						if (items != null && items.Count > 0)
						{
							var allProducts = _productService.GetProductsByIds(storeId ?? 0, items.Select(it => it.ProductId).Distinct().ToArray());
							var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(storeId ?? 0, allProducts.GetProductBigStrokeSmallUnitIds());

							//将商品按最小单位合并
							foreach (var item in items)
							{
								if (item.ProductId != 0)
								{
									var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
									if (product != null)
									{
										QuickAllocationItem quickAllocationItem = products.Where(a => a.ProductId == item.ProductId).FirstOrDefault();
										//商品转化量
										var conversionQuantity = product.GetConversionQuantity(allOptions, item.UnitId);
										//库存量增量 = 单位转化量 * 数量
										int thisQuantity = item.Quantity * conversionQuantity;
										if (quickAllocationItem != null)
										{
											quickAllocationItem.Quantity += thisQuantity;
										}
										else
										{
											quickAllocationItem = new QuickAllocationItem
											{
												ProductId = item.ProductId,
												Quantity = thisQuantity
											};
											products.Add(quickAllocationItem);
										}
									}
								}
							}
						}
						//加载上次调拨后销售的商品
						if (loads.Contains(QuickAllocationEnum.LoadSaleLast.ToString()))
						{
							//最近调拨已经签收过的商品
							var query4 = from a in DispatchItemsRepository.Table
										 join b in SaleReservationBillsRepository.Table on a.BillId equals b.Id
										 join c in SaleReservationItemsRepository.Table on b.Id equals c.SaleReservationBillId
										 join d in ProductsRepository.Table on c.ProductId equals d.Id
										 where a.StoreId == storeId && caIds.Contains(d.CategoryId) && b.DeliveryUserId == deliveryUserId && b.WareHouseId == wareHouseId && a.BillTypeId == (int)BillTypeEnum.SaleReservationBill && a.SignStatus == (int)SignStatusEnum.Done
										 orderby a.SignDate descending
										 select c;

							var items4 = query4.ToList();
							if (items4.Count > 0)
							{
								var allProducts2 = _productService.GetProductsByIds(storeId ?? 0, items4.Select(it => it.ProductId).Distinct().ToArray());
								var allOptions2 = _specificationAttributeService.GetSpecificationAttributeOptionByIds(storeId ?? 0, allProducts2.GetProductBigStrokeSmallUnitIds());

								//将商品按最小单位合并
								foreach (var item in items4)
								{
									if (item.ProductId != 0)
									{
										var product = allProducts2.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
										if (product != null)
										{
											QuickAllocationItem quickAllocationItem = products.Where(a => a.ProductId == item.ProductId).FirstOrDefault();
											//商品转化量
											var conversionQuantity = product.GetConversionQuantity(allOptions2, item.UnitId);
											//库存量增量 = 单位转化量 * 数量
											int thisQuantity = item.Quantity * conversionQuantity;
											if (quickAllocationItem != null)
											{
												quickAllocationItem.Quantity += thisQuantity;
											}
											else
											{
												quickAllocationItem = new QuickAllocationItem
												{
													ProductId = item.ProductId,
													Quantity = thisQuantity
												};
												products.Add(quickAllocationItem);
											}
										}
									}
								}

							}

						}

					}
					break;
				//按退调拨
				case AllocationTypeEnum.ByReturn:
					{

						List<ReturnItem> items = new List<ReturnItem>();
						List<ReturnItem> items1 = new List<ReturnItem>();
						List<ReturnItem> items2 = new List<ReturnItem>();
						List<ReturnItem> items3 = new List<ReturnItem>();

						//加载今天退货的商品
						if (loads.Contains(QuickAllocationEnum.LoadReturnToday.ToString()))
						{
							var query1 = from a in ReturnBillsRepository.Table
										 join b in ReturnItemsRepository.Table on a.Id equals b.ReturnBillId
										 where a.StoreId == storeId && a.WareHouseId == wareHouseId && a.AuditedStatus == true
										 && a.ReversedStatus == false
										 select b;
							//query = query.Where(s => SqlFunctions.DateDiff("dd", s.CreatedOnUtc, DateTime.Now).Value == 0);
							query1 = query1.AsEnumerable().Where(s => MySqlDbFunctionsExtensions.DateDiffDay(DbF.Functions, s.CreatedOnUtc, DateTime.Now) == 0).AsQueryable();
							items1 = query1.ToList();
						}
						//加载昨天退货的商品
						if (loads.Contains(QuickAllocationEnum.LoadReturnYestDay.ToString()))
						{
							var query2 = from a in ReturnBillsRepository.Table
										 join b in ReturnItemsRepository.Table on a.Id equals b.ReturnBillId
										 where a.StoreId == storeId && a.WareHouseId == wareHouseId && a.AuditedStatus == true && a.ReversedStatus == false
										 select b;
							//query = query.Where(s => SqlFunctions.DateDiff("dd", s.CreatedOnUtc, DateTime.Now).Value == 1);
							query2 = query2.AsEnumerable().Where(s => MySqlDbFunctionsExtensions.DateDiffDay(DbF.Functions, s.CreatedOnUtc, DateTime.Now) == 1).AsQueryable();
							items2 = query2.ToList();
						}
						//加载前天退货的商品
						if (loads.Contains(QuickAllocationEnum.LoadReturnBeforeYestday.ToString()))
						{
							var query3 = from a in ReturnBillsRepository.Table
										 join b in ReturnItemsRepository.Table on a.Id equals b.ReturnBillId
										 where a.StoreId == storeId && a.WareHouseId == wareHouseId && a.AuditedStatus == true && a.ReversedStatus == false
										 select b;
							//query = query.Where(s => SqlFunctions.DateDiff("dd", s.CreatedOnUtc, DateTime.Now).Value == 2);
							query3 = query3.AsEnumerable().Where(s => MySqlDbFunctionsExtensions.DateDiffDay(DbF.Functions, s.CreatedOnUtc, DateTime.Now) == 2).AsQueryable();
							items3 = query3.ToList();
						}

						//去重
						items = items1.Union(items2).Union(items3).Distinct().ToList();
						if (items != null && items.Count > 0)
						{
							var allProducts = _productService.GetProductsByIds(storeId ?? 0, items.Select(it => it.ProductId).Distinct().ToArray());
							var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(storeId ?? 0, allProducts.GetProductBigStrokeSmallUnitIds());

							//将商品按最小单位合并
							foreach (var item in items)
							{
								if (item.ProductId != 0)
								{
									var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
									if (product != null)
									{
										QuickAllocationItem quickAllocationItem = products.Where(a => a.ProductId == item.ProductId).FirstOrDefault();
										//商品转化量
										var conversionQuantity = product.GetConversionQuantity(allOptions, item.UnitId);
										//库存量增量 = 单位转化量 * 数量
										int thisQuantity = item.Quantity * conversionQuantity;
										if (quickAllocationItem != null)
										{
											quickAllocationItem.Quantity += thisQuantity;
										}
										else
										{
											quickAllocationItem = new QuickAllocationItem
											{
												ProductId = item.ProductId,
												Quantity = thisQuantity
											};
											products.Add(quickAllocationItem);
										}
									}
								}
							}
						}

					}
					break;
				//按库存调拨
				case AllocationTypeEnum.ByStock:
					{
						var query = from a in StocksRepository.Table
									join b in ProductsRepository.Table on a.ProductId equals b.Id
									where a.StoreId == storeId && caIds.Contains(b.CategoryId) && a.UsableQuantity > 0 && a.WareHouseId == wareHouseId
									select a;

						products = query.ToList().Select(s =>
						{
							return new QuickAllocationItem
							{
								ProductId = s.ProductId,
								Quantity = s.UsableQuantity ?? 0
							};
						}).ToList();
					}
					break;
				default:
					break;
			}

			//注意暂时按最小单位调拨
			return products;
		}

		public void UpdateAllocationBillActive(int? store, int? billId, int? user)
		{
			var query = AllocationBillsRepository.Table.ToList();

			query = query.Where(x => x.StoreId == store && x.MakeUserId == user && x.AuditedStatus == true && (DateTime.Now.Subtract(x.AuditedDate ?? DateTime.Now).Duration().TotalDays > 30)).ToList();

			if (billId.HasValue && billId.Value > 0)
			{
				query = query.Where(x => x.Id == billId).ToList();
			}

			var result = query;

			if (result != null && result.Count > 0)
			{
				var uow = AllocationBillsRepository.UnitOfWork;
				foreach (AllocationBill bill in result)
				{
					if ((bill.AuditedStatus && !bill.ReversedStatus) || bill.Deleted) continue;
					bill.Deleted = true;
					AllocationBillsRepository.Update(bill);
				}
				uow.SaveChanges();
			}
		}

		public BaseResult BillCreateOrUpdate(int storeId, int userId, int? billId, AllocationBill bill, AllocationBillUpdate data, List<AllocationItem> items, List<ProductStockItem> productStockItemThiss, bool isAdmin = false,bool doAudit = true)
		{
			var uow = AllocationBillsRepository.UnitOfWork;

			ITransaction transaction = null;
			try
			{

				transaction = uow.BeginOrUseTransaction();
				bill.StoreId = storeId;
				if (!(bill.Id > 0))
				{ 
					bill.MakeUserId = userId;
				}


				if (billId.HasValue && billId.Value != 0)
				{
					#region 更新调拨单

					bill = GetAllocationBillById(storeId, billId.Value, false);
					if (bill != null)
					{
						if (bill.AuditedStatus || bill.ReversedStatus || bill.Deleted) throw new Exception("单据状态异常，无法修改");
						bill.ShipmentWareHouseId = data.ShipmentWareHouseId;
						bill.IncomeWareHouseId = data.IncomeWareHouseId;
						bill.AllocationByMinUnit = data.AllocationByMinUnit;
						bill.Remark = data.Remark;
						UpdateAllocationBill(bill);
					}

					#endregion
				}
				else
				{
					#region 添加调拨单

					bill.ShipmentWareHouseId = data.ShipmentWareHouseId;
					bill.IncomeWareHouseId = data.IncomeWareHouseId;
					bill.AllocationByMinUnit = data.AllocationByMinUnit;

					bill.StoreId = storeId;
					//开单日期
					bill.CreatedOnUtc = DateTime.Now;
					//单据编号
					bill.BillNumber = string.IsNullOrEmpty(data.BillNumber) ? CommonHelper.GetBillNumber("DBD", storeId) : data.BillNumber;

					var sb = GetAllocationBillByNumber(storeId, bill.BillNumber);
					if (sb != null)
					{
						return new BaseResult { Success = false, Message = "操作失败，重复提交" };
					}

					//制单人
					bill.MakeUserId = userId;
					//状态(审核)
					bill.AuditedStatus = false;
					bill.AuditedDate = null;
					//红冲状态
					bill.ReversedStatus = false;
					bill.ReversedDate = null;
					//备注
					bill.Remark = data.Remark;
					bill.Operation = data.Operation;//标识操作源

					InsertAllocationBill(bill);

					#endregion
				}

				#region 更新调拨项目

				data.Items.ForEach(p =>
				{
					if (p.ProductId != 0)
					{
						var sd = GetAllocationItemById(storeId, p.Id);
						if (sd == null)
						{
							//追加项
							if (bill.Items.Count(cp => cp.Id == p.Id) == 0)
							{
								var item = p;
								item.StoreId = storeId;
								item.AllocationBillId = bill.Id;
								item.TradePrice = p.TradePrice ?? 0;
								item.WholesaleAmount = p.WholesaleAmount ?? 0;
								item.CreatedOnUtc = DateTime.Now;

								InsertAllocationItem(item);
								//不排除
								p.Id = item.Id;
								if (!bill.Items.Select(s => s.Id).Contains(item.Id))
								{
									bill.Items.Add(item);
								}
							}
						}
						else
						{
							//存在则更新
							sd.AllocationBillId = bill.Id;
							sd.ProductId = p.ProductId;
							sd.UnitId = p.UnitId;
							sd.Quantity = p.Quantity;
							sd.TradePrice = p.TradePrice ?? 0;
							sd.WholesaleAmount = p.WholesaleAmount ?? 0;
							sd.OutgoingStock = p.OutgoingStock;
							sd.WarehousingStock = p.WarehousingStock;
							sd.Remark = p.Remark;

							UpdateAllocationItem(sd);
						}
					}
				});

				#endregion

				#region Grid 移除则从库移除删除项

				bill.Items.ToList().ForEach(p =>
				{
					if (data.Items.Count(cp => cp.Id == p.Id) == 0)
					{
						bill.Items.Remove(p);
						var item = GetAllocationItemById(storeId, p.Id);
						if (item != null)
						{
							DeleteAllocationItem(item);
						}
					}
				});

				#endregion

				//判断App开单是否自动审核
				bool appBillAutoAudits = false;
				if (data.Operation == (int)OperationEnum.APP)
				{
					appBillAutoAudits = _settingService.AppBillAutoAudits(storeId, BillTypeEnum.AllocationBill);
				}
				//读取配置自动审核、管理员创建自动审核
				if ((isAdmin && doAudit) || appBillAutoAudits) //判断当前登录者是否为管理员,若为管理员，开启自动审核
				{
					AuditingNoTran(storeId, userId, bill);
				}
				else
				{
					#region 发送通知 管理员
					try
					{
						//管理员
						var adminNumbers = _userService.GetAllAdminUserMobileNumbersByStore(storeId).ToList();
						QueuedMessage queuedMessage = new QueuedMessage()
						{
							StoreId = storeId,
							MType = MTypeEnum.Message,
							Title = CommonHelper.GetEnumDescription<MTypeEnum>(MTypeEnum.Message),
							Date = bill.CreatedOnUtc,
							BillType = BillTypeEnum.AllocationBill,
							BillNumber = bill.BillNumber,
							BillId = bill.Id,
							CreatedOnUtc = DateTime.Now
						};
						_queuedMessageService.InsertQueuedMessage(adminNumbers,queuedMessage);
					}
					catch (Exception ex)
					{
						_queuedMessageService.WriteLogs(ex.Message);
					}
					#endregion
				}


				//保存事务
				transaction.Commit();

				return new BaseResult { Success = true, Return = billId ?? 0, Message = Resources.Bill_CreateOrUpdateSuccessful };
			}
			catch (Exception ex)
			{
				//如果事务不存在或者为控则回滚
				transaction?.Rollback();
				return new BaseResult { Success = false, Message = Resources.Bill_CreateOrUpdateFailed };
			}
			finally
			{
				//不管怎样最后都会关闭掉这个事务
				using (transaction) { }
			}
		}

		public BaseResult Auditing(int storeId, int userId, AllocationBill bill)
		{
			var uow = AllocationBillsRepository.UnitOfWork;

			ITransaction transaction = null;
			try
			{

				transaction = uow.BeginOrUseTransaction();

				bill.StoreId = storeId;


				AuditingNoTran(storeId, userId, bill);


				//保存事务
				transaction.Commit();

				return new BaseResult { Success = true, Message = "单据审核成功" };
			}
			catch (Exception)
			{
				//如果事务不存在或者为控则回滚
				transaction?.Rollback();
				return new BaseResult { Success = false, Message = "单据审核失败" };
			}
			finally
			{
				//不管怎样最后都会关闭掉这个事务
				using (transaction) { }
			}
		}

		public BaseResult AuditingNoTran(int storeId, int userId, AllocationBill bill)
		{
			var successful = new BaseResult { Success = true, Message = "单据审核成功" };
			var failed = new BaseResult { Success = false, Message = "单据审核失败" };

			try
			{
				//历史库存记录
				Tuple<List<ProductStockItem>, Tuple<StockInOutRecord, StockInOutRecord>, Tuple<List<StockFlow>, List<StockFlow>>, Tuple<List<StockInOutRecordStockFlow>, List<StockInOutRecordStockFlow>>, Tuple<List<Stock>, List<Stock>>> historyDatas1 = null;
				Tuple<List<ProductStockItem>, Tuple<StockInOutRecord, StockInOutRecord>, Tuple<List<StockFlow>, List<StockFlow>>, Tuple<List<StockInOutRecordStockFlow>, List<StockInOutRecordStockFlow>>, Tuple<List<Stock>, List<Stock>>> historyDatas2 = null;

				#region 修改库存

				try
				{
					var stockProducts = new List<ProductStockItem>();

					var allProducts = _productService.GetProductsByIds(storeId, bill.Items.Select(pr => pr.ProductId).Distinct().ToArray());
					var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(storeId, allProducts.GetProductBigStrokeSmallUnitIds());
					foreach (AllocationItem item in bill.Items)
					{
						var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
						ProductStockItem productStockItem = stockProducts.Where(a => a.ProductId == item.ProductId).FirstOrDefault();
						//商品转化量
						var conversionQuantity = product.GetConversionQuantity(allOptions, item.UnitId);
						//库存量增量 = 单位转化量 * 数量
						int thisQuantity = item.Quantity * conversionQuantity;
						if (productStockItem != null)
						{
							productStockItem.Quantity += thisQuantity;
						}
						else
						{
							productStockItem = new ProductStockItem
							{
								ProductId = item.ProductId,
								UnitId = item.UnitId,
								SmallUnitId = product.SmallUnitId,
								BigUnitId = product.BigUnitId ?? 0,
								ProductName = allProducts.Where(s => s.Id == item.ProductId).FirstOrDefault()?.Name,
								ProductCode = allProducts.Where(s => s.Id == item.ProductId).FirstOrDefault()?.ProductCode,
								Quantity = thisQuantity
							};

							stockProducts.Add(productStockItem);
						}
					}

					List<ProductStockItem> productStockItemThiss = new List<ProductStockItem>();
					if (stockProducts != null && stockProducts.Count > 0)
					{
						stockProducts.ForEach(psi =>
						{
							ProductStockItem productStockItem = new ProductStockItem
							{
								ProductId = psi.ProductId,
								UnitId = psi.UnitId,
								SmallUnitId = psi.SmallUnitId,
								BigUnitId = psi.BigUnitId,
								ProductName = allProducts.Where(s => s.Id == psi.ProductId).FirstOrDefault()?.Name,
								ProductCode = allProducts.Where(s => s.Id == psi.ProductId).FirstOrDefault()?.ProductCode,
								Quantity = psi.Quantity * (-1)
							};
							productStockItemThiss.Add(productStockItem);
						});
					}

					//出货仓库 减少现货
					historyDatas1 = _stockService.AdjustStockQty<AllocationBill, AllocationItem>(bill, _productService, _specificationAttributeService, DirectionEnum.Out, StockQuantityType.CurrentQuantity, bill.ShipmentWareHouseId, productStockItemThiss, StockFlowChangeTypeEnum.Audited);

					//入货仓库 增加现货
					historyDatas2 = _stockService.AdjustStockQty<AllocationBill, AllocationItem>(bill, _productService, _specificationAttributeService, DirectionEnum.In, StockQuantityType.CurrentQuantity, bill.IncomeWareHouseId, stockProducts, StockFlowChangeTypeEnum.Audited);
				}
				catch (Exception)
				{
				}

				#endregion

				#region 修改单据表状态

				bill.AuditedUserId = userId;
				bill.AuditedDate = DateTime.Now;
				bill.AuditedStatus = true;
				UpdateAllocationBill(bill);

				#endregion

				#region 发送通知
				try
				{
					//制单人、管理员
					var userNumbers = new List<string>() { _userService.GetMobileNumberByUserId(bill.MakeUserId) };
					var queuedMessage = new QueuedMessage() 
					{
						StoreId = storeId,
						MType = MTypeEnum.Audited,
						Title = CommonHelper.GetEnumDescription<MTypeEnum>(MTypeEnum.Audited),
						Date = bill.CreatedOnUtc,
						BillType = BillTypeEnum.AllocationBill,
						BillNumber = bill.BillNumber,
						BillId = bill.Id,
						CreatedOnUtc = DateTime.Now
					};
					_queuedMessageService.InsertQueuedMessage(userNumbers, queuedMessage);
				}
				catch (Exception ex)
				{
					_queuedMessageService.WriteLogs(ex.Message);
				}
				#endregion

				return successful;
			}
			catch (Exception)
			{
				return failed;
			}

		}

		public BaseResult Reverse(int userId, AllocationBill bill)
		{
			var successful = new BaseResult { Success = true, Message = "单据红冲成功" };
			var failed = new BaseResult { Success = false, Message = "单据红冲失败" };

			var uow = AllocationBillsRepository.UnitOfWork;
			ITransaction transaction = null;
			try
			{

				transaction = uow.BeginOrUseTransaction();
				//历史库存记录
				Tuple<List<ProductStockItem>, Tuple<StockInOutRecord, StockInOutRecord>, Tuple<List<StockFlow>, List<StockFlow>>, Tuple<List<StockInOutRecordStockFlow>, List<StockInOutRecordStockFlow>>, Tuple<List<Stock>, List<Stock>>> historyDatas1 = null;
				Tuple<List<ProductStockItem>, Tuple<StockInOutRecord, StockInOutRecord>, Tuple<List<StockFlow>, List<StockFlow>>, Tuple<List<StockInOutRecordStockFlow>, List<StockInOutRecordStockFlow>>, Tuple<List<Stock>, List<Stock>>> historyDatas2 = null;

				#region 修改库存
				var stockProducts = new List<ProductStockItem>();

				var allProducts = _productService.GetProductsByIds(bill.StoreId, bill.Items.Select(pr => pr.ProductId).Distinct().ToArray());
				var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(bill.StoreId, allProducts.GetProductBigStrokeSmallUnitIds());
				foreach (AllocationItem item in bill.Items)
				{
					var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
					ProductStockItem productStockItem = stockProducts.Where(a => a.ProductId == item.ProductId).FirstOrDefault();
					//商品转化量
					var conversionQuantity = product.GetConversionQuantity(allOptions, item.UnitId);
					//库存量增量 = 单位转化量 * 数量
					int thisQuantity = item.Quantity * conversionQuantity;
					if (productStockItem != null)
					{
						productStockItem.Quantity += thisQuantity;
					}
					else
					{
						productStockItem = new ProductStockItem
						{
							ProductId = item.ProductId,
							UnitId = item.UnitId,
							SmallUnitId = product.SmallUnitId,
							BigUnitId = product.BigUnitId ?? 0,
							ProductName = allProducts.Where(s => s.Id == item.ProductId).FirstOrDefault()?.Name,
							ProductCode = allProducts.Where(s => s.Id == item.ProductId).FirstOrDefault()?.ProductCode,
							Quantity = thisQuantity
						};

						stockProducts.Add(productStockItem);
					}
				}

				List<ProductStockItem> productStockItemThiss = new List<ProductStockItem>();
				if (stockProducts != null && stockProducts.Count > 0)
				{
					stockProducts.ForEach(psi =>
					{
						ProductStockItem productStockItem = new ProductStockItem
						{
							ProductId = psi.ProductId,
							UnitId = psi.UnitId,
							SmallUnitId = psi.SmallUnitId,
							BigUnitId = psi.BigUnitId,
							ProductName = allProducts.Where(s => s.Id == psi.ProductId).FirstOrDefault()?.Name,
							ProductCode = allProducts.Where(s => s.Id == psi.ProductId).FirstOrDefault()?.ProductCode,
							Quantity = psi.Quantity * (-1)
						};
						productStockItemThiss.Add(productStockItem);
					});
				}

				//出货仓库 增加现货

				historyDatas1 = _stockService.AdjustStockQty<AllocationBill, AllocationItem>(bill, _productService, _specificationAttributeService, DirectionEnum.In, StockQuantityType.CurrentQuantity, bill.ShipmentWareHouseId, stockProducts, StockFlowChangeTypeEnum.Reversed);

				//入货仓库 减少现货
				historyDatas2 = _stockService.AdjustStockQty<AllocationBill, AllocationItem>(bill, _productService, _specificationAttributeService, DirectionEnum.Out, StockQuantityType.CurrentQuantity, bill.IncomeWareHouseId, productStockItemThiss, StockFlowChangeTypeEnum.Reversed);

				#endregion

				#region 修改单据表状态
				bill.ReversedUserId = userId;
				bill.ReversedDate = DateTime.Now;
				bill.ReversedStatus = true;

				UpdateAllocationBill(bill);

				#endregion

				//保存事务
				transaction.Commit();

				return successful;
			}
			catch (Exception)
			{
				//如果事务不存在或者为控则回滚
				transaction?.Rollback();
				return failed;
			}
			finally
			{
				//不管怎样最后都会关闭掉这个事务
				using (transaction) { }
			}
		}

		public BaseResult Delete(int userId, AllocationBill bill)
		{
			var successful = new BaseResult { Success = true, Message = "单据作废成功" };
			var failed = new BaseResult { Success = false, Message = "单据作废失败" };

			var uow = AllocationBillsRepository.UnitOfWork;

			ITransaction transaction = null;
			try
			{

				transaction = uow.BeginOrUseTransaction();
				#region 修改单据表状态
				bill.Deleted = true;
				#endregion
				UpdateAllocationBill(bill);

				//保存事务
				transaction.Commit();

				return successful;
			}
			catch (Exception)
			{
				//如果事务不存在或者为控则回滚
				transaction?.Rollback();
				//return new BaseResult { Success = false, Message = Resources.Bill_CreateOrUpdateFailed };
				return failed;
			}
			finally
			{
				//不管怎样最后都会关闭掉这个事务
				using (transaction) { }
			}
		}
	}
}
