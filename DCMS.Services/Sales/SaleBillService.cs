using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Data;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.Sales;
using DCMS.Core.Domain.Tasks;
using DCMS.Core.Domain.WareHouses;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Configuration;
using DCMS.Services.Events;
using DCMS.Services.Finances;
using DCMS.Services.Products;
using DCMS.Services.Tasks;
using DCMS.Services.Terminals;
using DCMS.Services.Users;
using DCMS.Services.WareHouses;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using DCMS.Services.Caching;
using DCMS.Services.Stores;
using DCMS.Core.Domain.Stores;
using DCMS.Core.Domain.CSMS;
using DCMS.Services.CSMS;

namespace DCMS.Services.Sales
{
	public class SaleBillService : BaseService, ISaleBillService
	{
		private readonly IUserService _userService;
		private readonly IQueuedMessageService _queuedMessageService;
		private readonly IStockService _stockService;
		private readonly ISettingService _settingService;
		private readonly IRecordingVoucherService _recordingVoucherService;
		private readonly IProductService _productService;
		private readonly ISpecificationAttributeService _specificationAttributeService;
		private readonly ICostContractBillService _costContractBillService;
		private readonly ITerminalService _terminalService;
		private readonly IStoreService _storeService;
		private readonly ITerminalSignReportService _terminalSignReportService;
		private readonly IOrderDetailService _orderDetailService;

		public SaleBillService(IServiceGetter serviceGetter,
			IStaticCacheManager cacheManager,
			IEventPublisher eventPublisher,
			IUserService userService,
			IQueuedMessageService queuedMessageService,
			IStockService stockService,
			ISettingService settingService,
			IRecordingVoucherService recordingVoucherService,
			IProductService productService,
			ISpecificationAttributeService specificationAttributeService,
			ICostContractBillService costContractBillService,
			ITerminalService terminalService,
			IStoreService storeService,
			ITerminalSignReportService terminalSignReportService,
			IOrderDetailService orderDetailService
			) : base(serviceGetter, cacheManager, eventPublisher)
		{
			_userService = userService;
			_settingService = settingService;
			_queuedMessageService = queuedMessageService;
			_stockService = stockService;
			_recordingVoucherService = recordingVoucherService;
			_productService = productService;
			_specificationAttributeService = specificationAttributeService;
			_costContractBillService = costContractBillService;
			_terminalService = terminalService;
			_storeService = storeService;
			_terminalSignReportService=terminalSignReportService  ;
			_orderDetailService=orderDetailService;

		}

		#region 销售单
		public bool Exists(int billId)
		{
			return SaleBillsRepository.TableNoTracking.Where(a => a.Id == billId).Count() > 0;
		}

		/// <summary>
		/// 获取当前经销商销售单
		/// </summary>
		/// <param name="store"></param>
		/// <param name="terminalId"></param>
		/// <param name="terminalName"></param>
		/// <param name="businessUserId"></param>
		/// <param name="billNumber"></param>
		/// <param name="wareHouseId"></param>
		/// <param name="remark"></param>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="districtId"></param>
		/// <param name="auditedStatus"></param>
		/// <param name="sortByAuditedTime"></param>
		/// <param name="showReverse"></param>
		/// <param name="showReturn"></param>
		/// <param name="paymentMethodType"></param>
		/// <param name="billSourceType"></param>
		/// <param name="receipted"></param>
		/// <param name="pageIndex"></param>
		/// <param name="pageSize"></param>
		/// <returns></returns>
		public IPagedList<SaleBill> GetSaleBillList(int? store, int? makeuserId, int? terminalId, string terminalName, int? businessUserId, int? deliveryUserId, string billNumber = "", int? wareHouseId = null, string remark = "", DateTime? start = null, DateTime? end = null, int? districtId = null, bool? auditedStatus = null, bool? sortByAuditedTime = null, bool? showReverse = null, bool? showReturn = null, int? paymentMethodType = null, int? billSourceType = null, bool? receipted = null, bool? deleted = null, bool? handleStatus = null, int? sign = null, int? productId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
		{
			if (pageSize >= 50)
				pageSize = 50;

			DateTime.TryParse(start?.ToString("yyyy-MM-dd 00:00:00"), out DateTime startDate);
			DateTime.TryParse(end?.ToString("yyyy-MM-dd 23:59:59"), out DateTime endDate);

			var query = from pc in SaleBillsRepository.Table
						 .Include(cr => cr.Items)
						 //.ThenInclude(cr => cr.SaleBill)
						 .Include(cr => cr.SaleBillAccountings)
						 .ThenInclude(cr => cr.AccountingOption)
						select pc;

			if (store.HasValue && store != 0)
			{
				query = query.Where(a => a.StoreId == store);
			}

			var isadmin = _userService.IsAdmin(store,makeuserId ?? 0);
			if (!isadmin)
			{
				if (makeuserId.HasValue && makeuserId > 0)
				{
					var userIds = _userService.GetSubordinate(store, makeuserId ?? 0)?.Where(s => s > 0).ToList();
					if (userIds.Count > 0)
						query = query.Where(x => userIds.Contains(x.MakeUserId));
				}
			}

			if (productId.HasValue && productId > 0)
			{
				query = query.Where(a => a.Items.Where(s => s.ProductId == productId).Count() > 0);
			}

			//客户
			if (terminalId.HasValue && terminalId != 0)
			{
				query = query.Where(a => a.TerminalId == terminalId);
			}
			//客户名称检索
			if (!string.IsNullOrEmpty(terminalName))
			{
				var terminalIds = _terminalService.GetTerminalIds(store, terminalName);
				query = query.Where(a => terminalIds.Contains(a.TerminalId));
			}

			//业务员
			if (businessUserId.HasValue && businessUserId != 0)
			{
				query = query.Where(a => a.BusinessUserId == businessUserId);
			}

			//送货员
			if (deliveryUserId.HasValue && deliveryUserId != 0)
			{
				query = query.Where(a => a.DeliveryUserId == deliveryUserId);
			}

			//单据号
			if (!string.IsNullOrEmpty(billNumber))
			{
				query = query.Where(a => a.BillNumber.Contains(billNumber));
			}

			//仓库
			if (wareHouseId.HasValue && wareHouseId != 0)
			{
				query = query.Where(a => a.WareHouseId == wareHouseId);
			}

			//备注
			if (!string.IsNullOrEmpty(remark))
			{
				query = query.Where(a => a.Remark.Contains(remark));
			}

			//开始时间
			if (start != null)
			{
				query = query.Where(a => a.CreatedOnUtc >= startDate);
			}

			//结束时间
			if (end != null)
			{
				query = query.Where(a => a.CreatedOnUtc <= endDate);
			}

			//片区
			if (districtId.HasValue && districtId != 0)
			{
				var terminals = _terminalService.GetDisTerminalIds(store,districtId ?? 0);
				query = query.Where(a => terminals.Contains(a.TerminalId));
			}

			//审核状态
			if (auditedStatus.HasValue)
			{
				query = query.Where(a => a.AuditedStatus == auditedStatus);
			}

			//按审核排序
			if (sortByAuditedTime.HasValue)
			{
				query = query.OrderByDescending(a => a.AuditedDate);
			}

			//红冲状态
			if (showReverse.HasValue)
			{
				query = query.Where(a => a.ReversedStatus == showReverse);
			}

			//显示退货单 showReturn
			//支付方式 paymentMethodType
			if (paymentMethodType == (int)SaleBillPaymentMethodType.AlreadyBill)
			{
				query = query.Where(a => a.ReceiptStatus == 2);
			}

			if (paymentMethodType == (int)SaleBillPaymentMethodType.OweBill)
			{
				query = query.Where(a => a.ReceiptStatus == 0);
			}

			//单据来源 billSourceType
			if (billSourceType == (int)SaleBillSourceType.Order)
			{
				query = query.Where(a => a.SaleReservationBillId > 0);
			}

			if (billSourceType == (int)SaleBillSourceType.UnOrder)
			{
				query = query.Where(a => a.SaleReservationBillId == 0);
			}


			//是否收款
			if (receipted.HasValue)
			{
				query = query.Where(a => a.ReceiptStatus == 2);
			}

			if (deleted.HasValue)
			{
				query = query.Where(a => a.Deleted == deleted);
			}

			//是否上交
			if (handleStatus.HasValue)
			{
				if (handleStatus.Value)
				{
					query = query.Where(c => c.HandInStatus == handleStatus);
				}
				else
				{
					query = query.Where(c => (c.HandInStatus == handleStatus || c.HandInStatus == null) && c.HandInDate == null);
				}
			}

			//部门
			//if (departmentId.HasValue && departmentId != 0)
			//    query = query.Where(a => a.DepartmentId == departmentId);

			//按审核排序
			if (sortByAuditedTime.HasValue && sortByAuditedTime.Value == true)
			{
				query = query.OrderByDescending(c => c.AuditedDate);
			}
			//默认创建时间
			else
			{
				query = query.OrderByDescending(c => c.CreatedOnUtc);
			}

			//签收状态
			if (sign.HasValue)
			{
				query = query.Where(a => a.SignStatus == (sign ?? 0));
			}

			//总页数
			var totalCount = query.Count();
			var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
			return new PagedList<SaleBill>(plists, pageIndex, pageSize, totalCount);
		}

		public IList<SaleBill> GetSaleBillsByStoreId(int? storeId, bool? auditedStatus = null, bool? reversedStatus = null)
		{

			return _cacheManager.Get(DCMSDefaults.SALEBILL_BY_STOREID_KEY.FillCacheKey(storeId, auditedStatus, reversedStatus), () =>
		   {

			   var query = SaleBillsRepository.Table;

			   if (storeId.HasValue && storeId != 0)
			   {
				   query = query.Where(a => a.StoreId == storeId);
			   }

			   //审核
			   if (auditedStatus != null)
			   {
				   query = query.Where(a => a.AuditedStatus == auditedStatus);
			   }

			   //红冲
			   if (reversedStatus != null)
			   {
				   query = query.Where(a => a.ReversedStatus == reversedStatus);
			   }

			   query = query.OrderByDescending(a => a.CreatedOnUtc);

			   return query.ToList();

		   });

		}

		public IList<SaleBill> GetSaleBillsByStoreId(int? storeId, bool? auditedStatus = null, bool? reversedStatus = null, int? businessUserId = null, DateTime? startTime = null, DateTime? endTime = null)
		{
			return _cacheManager.Get(DCMSDefaults.SALEBILL_BY_STOREID_KEY_1.FillCacheKey(storeId, auditedStatus, reversedStatus, businessUserId, startTime, endTime), () =>
			{

				var query = SaleBillsRepository.Table;

				if (storeId.HasValue && storeId != 0)
				{
					query = query.Where(a => a.StoreId == storeId);
				}

				//审核
				if (auditedStatus != null)
				{
					query = query.Where(a => a.AuditedStatus == auditedStatus);
				}

				//红冲
				if (reversedStatus != null)
				{
					query = query.Where(a => a.ReversedStatus == reversedStatus);
				}

				//员工
				if (businessUserId != null && businessUserId != 0)
				{
					query = query.Where(a => a.BusinessUserId == businessUserId);
				}

				//开始时间
				if (startTime != null)
				{
					query = query.Where(a => a.TransactionDate >= startTime);
				}

				//结束时间
				if (endTime != null)
				{
					query = query.Where(a => a.TransactionDate <= endTime);
				}

				query = query.OrderByDescending(a => a.CreatedOnUtc);

				return query.ToList();

			});
		}


		public IList<BaseItem> GetSaleBillsByBusinessUsers(int? storeId, bool? auditedStatus = null, bool? reversedStatus = null, int[] businessUserIds = null, DateTime? startTime = null, DateTime? endTime = null)
		{

			return _cacheManager.Get(DCMSDefaults.GETSALEBILLSBYBUSINESSUSERS.FillCacheKey(storeId, auditedStatus, reversedStatus, string.Join("-", businessUserIds), startTime, endTime), () =>
			{
				var query = from sb in SaleBillsRepository.Table
							join si in SaleItemsRepository.Table on sb.Id equals si.SaleBillId
							join pr in ProductsRepository.Table on si.ProductId equals pr.Id
							where sb.StoreId == storeId && sb.AuditedStatus == auditedStatus && sb.ReversedStatus == reversedStatus && businessUserIds.Contains(sb.BusinessUserId) && sb.CreatedOnUtc >= startTime && sb.CreatedOnUtc <= endTime
							select new BaseItem
							{
								Id = si.Id,
								StoreId = sb.StoreId,
								BusinessUserId = sb.BusinessUserId,
								DeliveryUserId = sb.DeliveryUserId,
								ProductId = si.ProductId,
								Quantity = si.Quantity,
								Price = si.Price,
								Amount = si.Amount,
								ProductName = pr.Name,
								CategoryId = pr.CategoryId,
								PercentageType = 0,
								Profit = si.Profit,
								CostPrice = si.CostPrice,
								CostAmount = si.CostAmount,
								CostProfitRate = si.CostProfitRate
							};
				return query.ToList();
			});
		}

		public IList<BaseItem> GetSaleBillsByDeliveryUsers(int? storeId, bool? auditedStatus = null, bool? reversedStatus = null, int[] deliveryUserIds = null, DateTime? startTime = null, DateTime? endTime = null)
		{

			return _cacheManager.Get(DCMSDefaults.GETSALEBILLSBYDELIVERYUSERS.FillCacheKey(storeId, auditedStatus, reversedStatus, string.Join("-", deliveryUserIds), startTime, endTime), () =>
			{
				var query = from sb in SaleBillsRepository.Table
							join si in SaleItemsRepository.Table on sb.Id equals si.SaleBillId
							join pr in ProductsRepository.Table on si.ProductId equals pr.Id
							where sb.StoreId == storeId && sb.AuditedStatus == auditedStatus && sb.ReversedStatus == reversedStatus && deliveryUserIds.Contains(sb.DeliveryUserId) && sb.CreatedOnUtc >= startTime && sb.CreatedOnUtc <= endTime
							select new BaseItem
							{
								Id = si.Id,
								StoreId = sb.StoreId,
								BusinessUserId = sb.BusinessUserId,
								DeliveryUserId = sb.DeliveryUserId,
								ProductId = si.ProductId,
								Quantity = si.Quantity,
								Price = si.Price,
								Amount = si.Amount,
								ProductName = pr.Name,
								CategoryId = pr.CategoryId,
								PercentageType = 1,
								Profit = si.Profit,
								CostPrice = si.CostPrice,
								CostAmount = si.CostAmount,
								CostProfitRate = si.CostProfitRate
							};
				return query.ToList();

			});
		}



		public IList<SaleBill> GetSaleBillsByStoreId(int? storeId, bool? auditedStatus = null, bool? reversedStatus = null, DateTime? beginDate = null)
		{
			//return _cacheManager.Get(DCMSDefaults.SALEBILL_BY_STOREID_KEY_2.FillCacheKey(storeId, auditedStatus, reversedStatus, beginDate), () =>
			//{
			//});
			var query = SaleBillsRepository.Table;

			if (storeId.HasValue && storeId != 0)
			{
				query = query.Where(a => a.StoreId == storeId);
			}

			//审核
			if (auditedStatus != null)
			{
				query = query.Where(a => a.AuditedStatus == auditedStatus);
			}

			//红冲
			if (reversedStatus != null)
			{
				query = query.Where(a => a.ReversedStatus == reversedStatus);
			}

			//日期
			if (beginDate != null)
			{
				query = query.Where(a => a.TransactionDate >= beginDate);
			}

			query = query.OrderByDescending(a => a.CreatedOnUtc);

			return query.ToList();
		}

		public IList<SaleBill> GetHotSaleRanking(int? store, int? terminalId, int? businessUserId, DateTime? startTime, DateTime? endTime)
		{
			var query = SaleBillsRepository.Table;

			//已审核，未红冲
			query = query.Where(a => a.AuditedStatus == true && a.ReversedStatus == false);

			//经销商
			if (store > 0)
			{
				query = query.Where(a => a.StoreId == store);
			}

			//客户
			if (terminalId > 0)
			{
				query = query.Where(a => a.TerminalId == terminalId);
			}

			//业务员
			if (businessUserId > 0)
			{
				query = query.Where(a => a.BusinessUserId == businessUserId);
			}
			//开始日期
			if (startTime != null)
			{
				query = query.Where(a => a.TransactionDate > startTime);
			}

			//结束日期
			if (endTime != null)
			{
				query = query.Where(a => a.TransactionDate < endTime);
			}

			return query.ToList();
		}

		public IList<SaleBill> GetCostProfitRanking(int? store, int? terminalId, int? businessUserId, DateTime? startTime, DateTime? endTime)
		{
			var query = from sb in SaleBillsRepository.Table.Include(s=>s.Items) select sb;

			//已审核，未红冲
			query = query.Where(a => a.AuditedStatus == true && a.ReversedStatus == false);

			//经销商
			if (store > 0)
			{
				query = query.Where(a => a.StoreId == store);
			}

			//客户
			if (terminalId > 0)
			{
				query = query.Where(a => a.TerminalId == terminalId);
			}

			//业务员
			if (businessUserId > 0)
			{
				var userIds = _userService.GetSubordinate(store, businessUserId ?? 0);
				query = query.Where(a => userIds.Contains(a.BusinessUserId));
			}
			//开始日期
			if (startTime != null)
			{
				query = query.Where(a => a.TransactionDate > startTime);
			}

			//结束日期
			if (endTime != null)
			{
				query = query.Where(a => a.TransactionDate < endTime);
			}

			return query.ToList();
		}

		public IList<SaleBill> GetSaleBillByStoreIdTerminalId(int storeId, int terminalId)
		{
			var query = SaleBillsRepository.Table;

			//已审核，未红冲
			query = query.Where(a => a.AuditedStatus == true && a.ReversedStatus == false);

			//经销商
			query = query.Where(a => a.StoreId == storeId);
			//客户
			query = query.Where(a => a.TerminalId == terminalId);

			return query.ToList();
		}

		public virtual SaleBill GetSaleBillById(int? store, int saleBillId)
		{
			if (saleBillId == 0)
			{
				return null;
			}

			var key = DCMSDefaults.SALEBILL_BY_ID_KEY.FillCacheKey(store ?? 0, saleBillId);
			return _cacheManager.Get(key, () =>
			{
				return SaleBillsRepository.ToCachedGetById(saleBillId);
			});
		}


		public virtual SaleBill GetSaleBillById(int? store, int saleBillId, bool isInclude = false)
		{
			if (saleBillId == 0)
			{
				return null;
			}

			if (isInclude)
			{
				var query = SaleBillsRepository.Table
				.Include(sb => sb.Items)
				//.ThenInclude(sb => sb.SaleBill)
				.Include(sb => sb.SaleBillAccountings)
				.ThenInclude(cr => cr.AccountingOption);

				//var sb = query.FirstOrDefault(s => s.Id == saleBillId);

				//var sbas = SaleBillAccountingMappingRepository.Table.Where(s => s.BillId == saleBillId).ToList();

				//sb.SetSaleBillAccounting(sbas);

				return query.FirstOrDefault(s => s.Id == saleBillId);
			}

			return SaleBillsRepository.ToCachedGetById(saleBillId);
		}

		public virtual IList<SaleBill> GetSaleBillsByIds(int[] sIds)
		{
			if (sIds == null || sIds.Length == 0)
			{
				return new List<SaleBill>();
			}

			var query = from c in SaleBillsRepository.Table
						where sIds.Contains(c.Id)
						select c;
			var list = query.ToList();
			return list;
		}

		public virtual IList<SaleBill> GetSaleBillsBySaleReservationIds(int? store, int[] sIds)
		{
			if (sIds == null || sIds.Length == 0)
			{
				return new List<SaleBill>();
			}

			var query = from c in SaleBillsRepository.Table
						where sIds.Contains(c.SaleReservationBillId ?? 0) && c.StoreId == store
						orderby c.Id
						select c;

			return query.ToList();
		}


		public virtual SaleBill GetSaleBillBySaleReservationBillId(int? store, int saleReservationBillId)
		{
			if (saleReservationBillId == 0)
			{
				return null;
			}

			var key = DCMSDefaults.SALEBILL_BY_RESERVATIONID_KEY.FillCacheKey(store ?? 0, saleReservationBillId);
			return _cacheManager.Get(key, () => { return SaleBillsRepository.Table.Where(a => a.SaleReservationBillId == saleReservationBillId).FirstOrDefault(); });
		}

		public virtual IList<SaleBill> GetSaleBillBySaleReservationsBillId(int saleReservationBillId)
		{
			return SaleBillsRepository.Table.Where(a => a.SaleReservationBillId == saleReservationBillId).ToList();
		}

		public virtual SaleBill GetSaleBillByNumber(int? store, string billNumber)
		{
			var key = DCMSDefaults.SALEBILL_BY_NUMBER_KEY.FillCacheKey(store ?? 0, billNumber);
			return _cacheManager.Get(key, () =>
			{
				var query = SaleBillsRepository.Table;
				var bill = query.Where(a => a.StoreId == store && a.BillNumber == billNumber).FirstOrDefault();
				return bill;
			});
		}

		public int GetBillId(int? store, string billNumber)
		{
			var query = SaleBillsRepository.TableNoTracking;
			var bill = query.Where(a => a.BillNumber == billNumber).Select(s => s.Id).FirstOrDefault();
			return bill;
		}

		public void DeleteSaleBill(SaleBill bill)
		{

			if (bill == null)
			{
				throw new ArgumentNullException("salebill");
			}

			var uow = SaleBillsRepository.UnitOfWork;
			SaleBillsRepository.Delete(bill);
			uow.SaveChanges();

			//通知
			_eventPublisher.EntityDeleted(bill);

		}

		public void InsertSaleBill(SaleBill bill)
		{
			var uow = SaleBillsRepository.UnitOfWork;
			SaleBillsRepository.Insert(bill);
			uow.SaveChanges();

			//通知
			_eventPublisher.EntityInserted(bill);
		}

		public void UpdateSaleBill(SaleBill bill)
		{
			if (bill == null)
			{
				throw new ArgumentNullException("salebill");
			}

			var uow = SaleBillsRepository.UnitOfWork;
			SaleBillsRepository.Update(bill);
			uow.SaveChanges();

			//通知
			_eventPublisher.EntityUpdated(bill);

		}


		/// <summary>
		/// 更新销售单是否开具收款标志位
		/// </summary>
		/// <param name="billNumber"></param>
		public void UpdateSaleBillReceipted(string billNumber)
		{
			var bill = GetSaleBillByNumber(0, billNumber);
			if (bill != null)
			{
				var uow = SaleBillsRepository.UnitOfWork;
				bill.ReceiptStatus = 2;
				SaleBillsRepository.Update(bill);
				uow.SaveChanges();

				_eventPublisher.EntityUpdated(bill);

			}
		}


		/// <summary>
		/// 更新销售单欠款
		/// </summary>
		/// <param name="billNumber"></param>
		public void UpdateSaleBillOweCash(string billNumber, decimal oweCash)
		{
			var bill = GetSaleBillByNumber(0, billNumber);
			if (bill != null)
			{
				bill.OweCash = oweCash;

				//如果欠款为0，则已收款
				if (oweCash == 0)
				{
					bill.ReceiptStatus = 2;
				}

				var uow = SaleBillsRepository.UnitOfWork;
				SaleBillsRepository.Update(bill);
				uow.SaveChanges();

				_eventPublisher.EntityUpdated(bill);
			}
		}

		/// <summary>
		/// 更新单据收款状态
		/// </summary>
		/// <param name="store"></param>
		/// <param name="billId"></param>
		/// <param name="receiptStatus"></param>
		public void UpdateReceived(int? store, int billId, ReceiptStatus receiptStatus)
		{
			var bill = GetSaleBillById(store, billId, false);
			if (bill != null)
			{
				bill.ReceiptStatus = (int)receiptStatus;
				var uow = PurchaseBillsRepository.UnitOfWork;
				SaleBillsRepository.Update(bill);
				uow.SaveChanges();
				//通知
				_eventPublisher.EntityUpdated(bill);
			}
		}



		/// <summary>
		/// 设置销售单价格
		/// </summary>
		/// <param name="saleBillId"></param>
		public void SetSaleBillAmount(int saleBillId)
		{
			SaleBill bill;
			var query = SaleBillsRepository.Table;
			bill = query.Where(a => a.Id == saleBillId).FirstOrDefault();
			if (bill == null)
			{
				throw new ArgumentNullException("salebill");
			}
			List<SaleItem> saleItems = GetSaleItemList(saleBillId);
			if (saleItems != null && saleItems.Count > 0)
			{
				//总金额
				decimal SumAmount = saleItems.Sum(a => a.Amount);
				//已收金额（会计科目金额）
				decimal accounting = 0;
				IList<SaleBillAccounting> saleAccountings = GetSaleBillAccountingsBySaleBillId(0, saleBillId);
				if (saleAccountings != null && saleAccountings.Count > 0)
				{
					accounting = saleAccountings.Sum(a => a.CollectionAmount);
				}
				//总金额
				bill.SumAmount = SumAmount;
				//应收金额=总金额-优惠金额
				//sale.ReceivableAmount = SumAmount - (sale.PreferentialAmount ?? 0);
				//欠款金额=总金额-优惠金额-已收金额
				//sale.OweCash = SumAmount - (sale.PreferentialAmount ?? 0) - accounting;

				//总成本价
				decimal SumCostPrice = saleItems.Sum(a => a.CostPrice);
				bill.SumCostPrice = SumCostPrice;
				//总成本金额
				decimal SumCostAmount = saleItems.Sum(a => a.CostAmount);
				bill.SumCostAmount = SumCostAmount;
				//总利润 = 总金额-总成本金额
				bill.SumProfit = bill.SumAmount - SumCostAmount;
				//成本利润率 = 总利润 / 总成本金额
				var amount = (bill.SumCostAmount == 0) ? bill.SumProfit : bill.SumCostAmount;
				if (amount != 0)
				{
					bill.SumCostProfitRate = (bill.SumProfit / amount) * 100;
				}
				var uow = SaleBillsRepository.UnitOfWork;
				SaleBillsRepository.Update(bill);
				uow.SaveChanges();

				//通知
				_eventPublisher.EntityUpdated(bill);
			}

		}



		#endregion

		#region 销售单明细


		/// <summary>
		/// 根据销售单获取项目
		/// </summary>
		/// <param name="saleBillId"></param>
		/// <param name="pageIndex"></param>
		/// <param name="pageSize"></param>
		/// <returns></returns>
		public virtual IList<SaleItem> GetSaleItemBySaleBillId(int saleBillId, int? storeId, int? userId, int pageIndex, int pageSize)
		{
			if (saleBillId == 0)
			{
				return new PagedList<SaleItem>(new List<SaleItem>(), pageIndex, pageSize);
			}

			var key = DCMSDefaults.SALEBILL_ITEM_ALLBY_SALEID_KEY.FillCacheKey(storeId, saleBillId, pageIndex, pageSize, userId);
			return _cacheManager.Get(key, () =>
			{
				var query = from pc in SaleItemsRepository.Table
							.Include(si => si.SaleBill)
							where pc.SaleBillId == saleBillId
							orderby pc.Id
							select pc;
				//var saleItems = new PagedList<SaleItem>(query.ToList(), pageIndex, pageSize);
				//return saleItems;
				//总页数
				var totalCount = query.Count();
				var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
				return new PagedList<SaleItem>(plists, pageIndex, pageSize, totalCount);
			});
		}

		public List<SaleItem> GetSaleItemList(int saleBillId)
		{
			List<SaleItem> saleItems = null;
			var query = SaleItemsRepository.Table.Include(s=>s.SaleBill);
			saleItems = query.Where(a => a.SaleBillId == saleBillId).ToList();
			return saleItems;
		}

		public int SaleItemQtySum(int storeId, int productId, int saleBillId)
		{
			int qty = 0;
			var query = from sale in SaleBillsRepository.Table
						join saleItem in SaleItemsRepository.Table on sale.Id equals saleItem.SaleBillId
						where sale.AuditedStatus == true && saleItem.ProductId == productId
							  && sale.Id != saleBillId    //排除当前销售单数量
						select saleItem;
			List<SaleItem> saleItems = query.ToList();
			if (saleItems != null && saleItems.Count > 0)
			{
				qty = saleItems.Sum(x => x.Quantity);
			}
			return qty;
		}

		public SaleItem GetSaleItemById(int saleItemId)
		{
			SaleItem saleItem;
			var query = SaleItemsRepository.Table;
			saleItem = query.Where(a => a.Id == saleItemId).FirstOrDefault();
			return saleItem;
		}

		public void DeleteSaleItem(SaleItem saleItem)
		{
			if (saleItem == null)
			{
				throw new ArgumentNullException("saleitem");
			}

			var uow = SaleItemsRepository.UnitOfWork;
			SaleItemsRepository.Delete(saleItem);
			uow.SaveChanges();

			//通知
			_eventPublisher.EntityDeleted(saleItem);
		}

		public void InsertSaleItem(SaleItem saleItem)
		{
			var uow = SaleItemsRepository.UnitOfWork;
			SaleItemsRepository.Insert(saleItem);
			uow.SaveChanges();

			//通知
			_eventPublisher.EntityInserted(saleItem);
		}
		public void InsertSaleItems(List<SaleItem> saleItems)
		{
			var uow = SaleItemsRepository.UnitOfWork;
			SaleItemsRepository.Insert(saleItems);
			uow.SaveChanges();
			//通知
			saleItems.ForEach(s =>
			{
				_eventPublisher.EntityInserted(s);
			});
		}

		public void UpdateSaleItem(SaleItem saleItem)
		{
			if (saleItem == null)
			{
				throw new ArgumentNullException("saleitem");
			}

			var uow = SaleItemsRepository.UnitOfWork;
			SaleItemsRepository.Update(saleItem);
			uow.SaveChanges();

			//通知
			_eventPublisher.EntityUpdated(saleItem);
		}


		public void UpdateSaleItem(int storeId, int billId, int productId, decimal costPrice = 0)
		{
			var uow = SaleItemsRepository.UnitOfWork;

			var items = SaleItemsRepository.Table.Where(s => s.StoreId == storeId && s.SaleBillId == billId && s.ProductId == productId).ToList();

			items?.ForEach(s => { s.CostPrice = costPrice; });

			SaleItemsRepository.Update(items);

			uow.SaveChanges();

			items?.ForEach(s =>
			{
				_eventPublisher.EntityUpdated(s);
			});
		}



		#endregion

		#region 收款账户映射

		public virtual IPagedList<SaleBillAccounting> GetSaleBillAccountingsBySaleId(int storeId, int userId, int saleBillId, int pageIndex, int pageSize)
		{
			if (pageSize >= 50)
				pageSize = 50;
			if (saleBillId == 0)
			{
				return new PagedList<SaleBillAccounting>(new List<SaleBillAccounting>(), pageIndex, pageSize);
			}

			//string key = string.Format(SALEBILL_ACCOUNTING_ALLBY_SALEID_KEY.FillCacheKey( saleBillId, pageIndex, pageSize, _workContext.CurrentUser.Id, _workContext.CurrentStore.Id);
			var key = DCMSDefaults.SALEBILL_ACCOUNTING_ALLBY_SALEID_KEY.FillCacheKey(storeId, saleBillId, pageIndex, pageSize, userId);
			return _cacheManager.Get(key, () =>
			{
				var query = from pc in SaleBillAccountingMappingRepository.Table
							join p in AccountingOptionsRepository.Table on pc.AccountingOptionId equals p.Id
							where pc.BillId == saleBillId
							orderby pc.Id
							select pc;


				//var saleAccountings = new PagedList<SaleBillAccounting>(query.ToList(), pageIndex, pageSize);
				//return saleAccountings;
				//总页数
				var totalCount = query.Count();
				var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
				return new PagedList<SaleBillAccounting>(plists, pageIndex, pageSize, totalCount);
			});
		}

		public virtual IList<SaleBillAccounting> GetSaleBillAccountingsBySaleBillId(int? store, int saleBillId)
		{
			var query = from pc in SaleBillAccountingMappingRepository.Table
						join p in AccountingOptionsRepository.Table on pc.AccountingOptionId equals p.Id
						where pc.StoreId == store && pc.BillId == saleBillId
						orderby pc.Id
						select pc;
			return query.Distinct().ToList();
		}

		public virtual SaleBillAccounting GetSaleBillAccountingById(int saleBillAccountingId)
		{
			if (saleBillAccountingId == 0)
			{
				return null;
			}

			return SaleBillAccountingMappingRepository.ToCachedGetById(saleBillAccountingId);
		}

		public virtual void InsertSaleBillBillAccounting(SaleBillAccounting saleBillAccounting)
		{
			if (saleBillAccounting == null)
			{
				throw new ArgumentNullException("salebillaccounting");
			}

			var uow = SaleBillAccountingMappingRepository.UnitOfWork;
			SaleBillAccountingMappingRepository.Insert(saleBillAccounting);
			uow.SaveChanges();

			//通知
			_eventPublisher.EntityInserted(saleBillAccounting);
		}

		public virtual void UpdateSaleBillAccounting(SaleBillAccounting saleBillAccounting)
		{
			if (saleBillAccounting == null)
			{
				throw new ArgumentNullException("salebillaccounting");
			}

			var uow = SaleBillAccountingMappingRepository.UnitOfWork;
			SaleBillAccountingMappingRepository.Update(saleBillAccounting);
			uow.SaveChanges();

			//通知
			_eventPublisher.EntityUpdated(saleBillAccounting);
		}



		public virtual void DeleteSaleBillAccounting(SaleBillAccounting saleBillAccounting)
		{
			if (saleBillAccounting == null)
			{
				throw new ArgumentNullException("salebillaccounting");
			}

			var uow = SaleBillAccountingMappingRepository.UnitOfWork;
			SaleBillAccountingMappingRepository.Delete(saleBillAccounting);
			uow.SaveChanges();

			//通知
			_eventPublisher.EntityDeleted(saleBillAccounting);
		}

		/// <summary>
		/// 获取当前单据的所有搜款账户(目的:在查询时不依赖延迟加载,由于获的较高查询性能)
		/// </summary>
		/// <returns></returns>
		public virtual IList<SaleBillAccounting> GetAllSaleBillAccountingsByBillIds(int? store, int[] billIds)
		{
			if (billIds == null || billIds.Length == 0)
			{
				return new List<SaleBillAccounting>();
			}

			var key = DCMSDefaults.SALEBILL_ACCOUNTINGL_BY_SALEID_KEY.FillCacheKey(store ?? 0, string.Join("_", billIds.OrderBy(a => a)));
			return _cacheManager.Get(key, () =>
			{
				var query = from pc in SaleBillAccountingMappingRepository.Table
							.Include(sb => sb.AccountingOption)
							where billIds.Contains(pc.BillId)
							select pc;
				return query.ToList();
			});
		}
		#endregion

		public SaleBill GetLastSaleBill(int? storeId, int? terminalId, int? businessUserId)
		{
			var query = SaleBillsRepository.Table;

			//经销商
			if (storeId.HasValue && storeId != 0)
			{
				query = query.Where(a => a.StoreId == storeId);
			}

			if (terminalId.HasValue && terminalId != 0)
			{
				query = query.Where(a => a.TerminalId == terminalId);
			}

			if (businessUserId.HasValue && businessUserId != 0)
			{
				query = query.Where(a => a.BusinessUserId == businessUserId);
			}

			query = query.Where(x => x.CreatedOnUtc < DateTime.Now);

			query = query.OrderByDescending(a => a.CreatedOnUtc);

			return query.ToList().FirstOrDefault();
		}

		public void UpdateSaleBillActive(int? store, int? billId, int? user)
		{
			var query = SaleBillsRepository.Table.ToList();

			query = query.Where(x => x.StoreId == store && x.MakeUserId == user && x.AuditedStatus == true && (DateTime.Now.Subtract(x.AuditedDate ?? DateTime.Now).Duration().TotalDays > 30)).ToList();

			if (billId.HasValue && billId.Value > 0)
			{
				query = query.Where(x => x.Id == billId).ToList();
			}

			var result = query;

			if (result != null && result.Count > 0)
			{
				var uow = SaleBillsRepository.UnitOfWork;
				foreach (SaleBill bill in result)
				{
					if ((bill.AuditedStatus && !bill.ReversedStatus) || bill.Deleted) 
					{
						continue;
					}
					bill.Deleted = true;
					SaleBillsRepository.Update(bill);
				}
				uow.SaveChanges();
			}
		}

		/// <summary>
		/// 获取收款对账单
		/// </summary>
		/// <param name="storeId"></param>
		/// <param name="status"></param>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="businessUserId"></param>
		/// <returns></returns>
		public IList<SaleBill> GetSaleBillListToFinanceReceiveAccount(int? storeId, int? employeeId = null, DateTime? start = null, DateTime? end = null)
		{
			var query = SaleBillsRepository.Table;

			//经销商
			if (storeId.HasValue && storeId != 0)
			{
				query = query.Where(a => a.StoreId == storeId);
			}

			//已审核，未红冲
			query = query.Where(a => a.AuditedStatus == true && a.ReversedStatus == false);

			//开始时间
			if (start != null)
			{
				query = query.Where(a => a.CreatedOnUtc >= start);
			}

			//结束时间
			if (end != null)
			{
				query = query.Where(a => a.CreatedOnUtc <= end);
			}

			//业务员
			if (employeeId.HasValue)
			{
				query = query.Where(a => a.DeliveryUserId == employeeId);
			}

			query = query.OrderByDescending(a => a.CreatedOnUtc);

			return query.ToList();
		}


		/// <summary>
		/// APP/PC 销售单据（订单转单）创建
		/// </summary>
		/// <param name="storeId"></param>
		/// <param name="userId"></param>
		/// <param name="billId"></param>
		/// <param name="bill"></param>
		/// <param name="accountingOptions"></param>
		/// <param name="accountings"></param>
		/// <param name="data"></param>
		/// <param name="items"></param>
		/// <param name="productStockItemThiss"></param>
		/// <param name="isAdmin"></param>
		/// <param name="photos"></param>
		/// <returns></returns>
		public BaseResult BillCreateOrUpdate(int storeId, int userId, int? billId, SaleBill bill, List<SaleBillAccounting> accountingOptions, List<AccountingOption> accountings, SaleBillUpdate data, List<SaleItem> items, List<ProductStockItem> productStockItemThiss, bool isAdmin = false, List<RetainPhoto> photos = null,bool doAudit = true)
		{
			var uow = SaleBillsRepository.UnitOfWork;

			ITransaction transaction = null;
			try
			{
				transaction = uow.BeginOrUseTransaction();

				bill.StoreId = storeId;
				if (!(bill.Id > 0))
				{
					bill.MakeUserId = userId;
				}

				//业务员
				string adminMobileNumbers = _userService.GetMobileNumberByUserId(data.BusinessUserId);
				var companySetting = _settingService.LoadSetting<CompanySetting>(storeId);

				#region 销售单据
				if (billId.HasValue && billId.Value != 0)
				{
					#region 更新销售单据

					bill.TerminalId = data.TerminalId;
					bill.BusinessUserId = data.BusinessUserId;
					bill.WareHouseId = data.WareHouseId;
					bill.DeliveryUserId = data.DeliveryUserId;
					bill.TransactionDate = data.TransactionDate;
					bill.IsMinUnitSale = data.IsMinUnitSale;
					bill.Remark = data.Remark;
					bill.DefaultAmountId = data.DefaultAmountId;

					bill.PreferentialAmount = data.PreferentialAmount;
					bill.ReceivableAmount = data.PreferentialEndAmount;
					bill.OweCash = data.OweCash;

					if (data.OweCash <= 0)
					{
						bill.ReceivedStatus = ReceiptStatus.Received;
					}
					if (data.OweCash > 0 && data.OweCash < data.PreferentialEndAmount)
					{
						bill.ReceivedStatus = ReceiptStatus.Part;
					}
					if (data.OweCash == data.PreferentialEndAmount)
					{
						bill.ReceivedStatus = ReceiptStatus.None;
					}
					//计算金额
					if (data.Items != null && data.Items.Count > 0)
					{
						//总金额
						//decimal SumAmount = data.Items.Sum(a => a.Amount * (1 + a.TaxRate / 100));
						//总金额
						bill.SumAmount = data.Items.Sum(a => a.Amount);
						//总成本价
						decimal SumCostPrice = data.Items.Sum(a => a.CostPrice);
						bill.SumCostPrice = SumCostPrice;
						//总成本金额
						decimal SumCostAmount = data.Items.Sum(a => a.CostAmount);
						bill.SumCostAmount = SumCostAmount;
						//总利润 = 总金额-总成本金额
						bill.SumProfit = bill.SumAmount - SumCostAmount;
						//成本利润率 = 总利润 / 总成本金额
						if (bill.SumCostAmount == 0)
						{
							bill.SumCostProfitRate = 100;
						}
						else
						{
							bill.SumCostProfitRate = (bill.SumProfit / bill.SumCostAmount) * 100;
						}

						if (companySetting.EnableTaxRate)
						{
							//总税额
							bill.TaxAmount = Math.Round(data.Items.Sum(a => a.Amount * (a.TaxRate / 100)), 2, MidpointRounding.AwayFromZero);
						}
						else
						{
							bill.TaxAmount = 0;
						}
					}
					else
					{
						bill.SumAmount = 0;
						bill.SumCostPrice = 0;
						bill.SumCostAmount = 0;
						bill.SumProfit = 0;
						bill.SumCostProfitRate = 0;
					}


					if (data.OrderId > 0)
					{
						bill.SaleReservationBillId = data.OrderId;
					}

					UpdateSaleBill(bill);

					#endregion
				}
				else
				{
					#region 添加销售单据

					bill.BillType = BillTypeEnum.SaleBill;
					bill.BillNumber = string.IsNullOrEmpty(data.BillNumber) ? bill.GenerateNumber() : data.BillNumber;

					var sb = GetSaleBillByNumber(storeId, bill.BillNumber);
					if (sb != null)
					{
						return new BaseResult { Success = false, Message = "操作失败，重复提交" };
					}

					//销售订单Id 默认0
					bill.SaleReservationBillId = 0;

					bill.TerminalId = data.TerminalId;
					bill.BusinessUserId = data.BusinessUserId;
					bill.WareHouseId = data.WareHouseId;
					bill.DeliveryUserId = data.DeliveryUserId;
					bill.TransactionDate = data.TransactionDate == null ? DateTime.Now : data.TransactionDate;
					bill.IsMinUnitSale = data.IsMinUnitSale;
					bill.Remark = data.Remark;
					bill.DefaultAmountId = data.DefaultAmountId;

					bill.SumAmount = 0;
					bill.ReceivableAmount = data.PreferentialEndAmount;
					bill.PreferentialAmount = data.PreferentialAmount;
					bill.OweCash = data.OweCash;
					bill.SumCostPrice = 0;
					bill.SumCostAmount = 0;
					bill.SumProfit = 0;
					bill.SumCostProfitRate = 0;

					bill.MakeUserId = userId;
					bill.CreatedOnUtc = DateTime.Now;
					bill.AuditedStatus = false;
					bill.ReversedStatus = false;
					bill.Operation = data.Operation;//标识操作源
					if (data.OweCash < data.PreferentialEndAmount)
					{
						bill.ReceivedStatus = ReceiptStatus.Part;
					}
					if (data.OweCash == data.PreferentialEndAmount)
					{
						bill.ReceivedStatus = ReceiptStatus.None;
					}
					//计算金额
					if (data.Items != null && data.Items.Count > 0)
					{
						//总金额
						//decimal SumAmount = data.Items.Sum(a => a.Amount * (1 + a.TaxRate / 100));
						//总金额
						bill.SumAmount = data.Items.Sum(a => a.Amount);
						//总成本价
						decimal SumCostPrice = data.Items.Sum(a => a.CostPrice);
						bill.SumCostPrice = SumCostPrice;
						//总成本金额
						decimal SumCostAmount = data.Items.Sum(a => a.CostAmount);
						bill.SumCostAmount = SumCostAmount;
						//总利润 = 总金额-总成本金额
						bill.SumProfit = bill.SumAmount - SumCostAmount;
						//成本利润率 = 总利润 / 总成本金额
						if (bill.SumCostAmount == 0)
						{
							bill.SumCostProfitRate = 100;
						}
						else
						{
							bill.SumCostProfitRate = (bill.SumProfit / bill.SumCostAmount) * 100;
						}

						if (companySetting.EnableTaxRate)
						{
							//总税额
							bill.TaxAmount = Math.Round(data.Items.Sum(a => a.Amount * (a.TaxRate / 100)), 2, MidpointRounding.AwayFromZero);
						}
						else
						{
							bill.TaxAmount = 0;
						}
					}
					else
					{
						bill.SumAmount = 0;
						bill.SumCostPrice = 0;
						bill.SumCostAmount = 0;
						bill.SumProfit = 0;
						bill.SumCostProfitRate = 0;
					}

					if (data.OrderId > 0)
					{
						bill.SaleReservationBillId = data.OrderId;
					}

					InsertSaleBill(bill);
					//主表Id
					billId = bill.Id;

					#endregion

					#region 当前用户判断今天是否交账
					//if (_financeReceiveAccountBillService.CheckTodayReceive(storeId, userId))
					//{
					//    #region 发送通知
					//    try
					//    {
					//        var queuedMessage = new QueuedMessage()
					//        {
					//            StoreId = storeId,
					//            MType = MTypeEnum.LedgerWarning,
					//            Title = CommonHelper.GetEnumDescription<MTypeEnum>(MTypeEnum.LedgerWarning),
					//            Date = bill.CreatedOnUtc,
					//            BillType = BillTypeEnum.SaleBill,
					//            BillNumber = bill.BillNumber,
					//            BillId = bill.Id,
					//            CreatedOnUtc = DateTime.Now,
					//            ToUsers = adminMobileNumbers,
					//            BusinessUser = _userService.GetUserName(storeId, userId)
					//        };
					//        _queuedMessageService.InsertQueuedMessage(queuedMessage);
					//    }
					//    catch (Exception ex)
					//    {
					//        _queuedMessageService.WriteLogs(ex.Message);
					//    }
					//    #endregion
					//}
					#endregion
				}
				#endregion

				#region 更新销售项目

				data.Items.ForEach(p =>
				{
					if (p.ProductId != 0)
					{
						_productService.SetSolded(p.ProductId);

						var saleItem = GetSaleItemById(p.Id);
						if (saleItem == null)
						{
							//追加项
							if (bill.Items.Count(cp => cp.Id == p.Id) == 0)
							{
								var item = p;
								item.SaleBillId = billId.Value;
								item.StoreId = storeId;

								//有税率，则价格=含税价格，金额=含税金额
								if (item.TaxRate > 0 && companySetting.EnableTaxRate)
								{
									item.Price *= (1 + item.TaxRate / 100);
									item.Amount *= (1 + item.TaxRate / 100);
								}

								//利润 = 金额 - 成本金额
								item.Profit = item.Amount - item.CostAmount;
								//成本利润率 = 利润 / 成本金额
								if (item.CostAmount == 0)
								{
									item.CostProfitRate = 100;
								}
								else
								{
									item.CostProfitRate = item.Profit / item.CostAmount * 100;
								}

								item.CreatedOnUtc = DateTime.Now;

								//匹配先进先出
								var fifo = _stockService.GetFIFOProduct(storeId, bill.WareHouseId, item.ProductId);
								item.ProductionBatch = fifo?.ProductionBatch;
								item.DealerStockId = fifo?.DealerStockId ?? 0;
								item.ManufactureDete = fifo?.DateOfManufacture;
							   

								InsertSaleItem(item);
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
							saleItem.ProductId = p.ProductId;
							saleItem.UnitId = p.UnitId;
							saleItem.Quantity = p.Quantity;
							saleItem.RemainderQty = p.RemainderQty;
							saleItem.Price = p.Price;
							saleItem.Amount = p.Amount;
							//有税率，则价格=含税价格，金额=含税金额
							if (saleItem.TaxRate > 0 && companySetting.EnableTaxRate)
							{
								saleItem.Price = p.Price * (1 + p.TaxRate / 100);
								saleItem.Amount = p.Amount * (1 + p.TaxRate / 100);
							}

							//成本价
							saleItem.CostPrice = p.CostPrice;
							//成本金额
							saleItem.CostAmount = p.CostAmount;
							//利润 = 金额 - 成本金额
							saleItem.Profit = saleItem.Amount - saleItem.CostAmount;
							//成本利润率 = 利润 / 成本金额
							if (saleItem.CostAmount == 0)
							{
								saleItem.CostProfitRate = 100;
							}
							else
							{
								saleItem.CostProfitRate = saleItem.Profit / saleItem.CostAmount * 100;
							}

							saleItem.StockQty = p.StockQty;
							saleItem.Remark = p.Remark;
							saleItem.RemarkConfigId = p.RemarkConfigId;
							saleItem.RemainderQty = p.RemainderQty;
							saleItem.ManufactureDete = p.ManufactureDete;

							//2019-07-25
							saleItem.IsGifts = p.IsGifts;
							saleItem.BigGiftQuantity = p.BigGiftQuantity;
							saleItem.SmallGiftQuantity = p.SmallGiftQuantity;

							//赠送商品信息
							saleItem.SaleProductTypeId = p.SaleProductTypeId;
							saleItem.GiveTypeId = p.GiveTypeId;
							saleItem.CampaignId = p.CampaignId;
							saleItem.CampaignBuyProductId = p.CampaignBuyProductId;
							saleItem.CampaignGiveProductId = p.CampaignGiveProductId;
							saleItem.CostContractId = p.CostContractId;
							saleItem.CostContractItemId = p.CostContractItemId;
							saleItem.CostContractMonth = p.CostContractMonth;
							saleItem.CampaignLinkNumber = p.CampaignLinkNumber;

							//匹配先进先出
							var fifo = _stockService.GetFIFOProduct(storeId, bill.WareHouseId, saleItem.ProductId);
							saleItem.ProductionBatch = fifo?.ProductionBatch;
							saleItem.DealerStockId = fifo?.DealerStockId ?? 0;
							saleItem.ManufactureDete = fifo?.DateOfManufacture;

							UpdateSaleItem(saleItem);
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
						var sd = GetSaleItemById(p.Id);
						if (sd != null)
						{
							DeleteSaleItem(sd);
						}
					}
				});

				#endregion

				#region 收款账户映射

				var saleAccountings = GetSaleBillAccountingsBySaleBillId(storeId, bill.Id);
				accountings.ToList().ForEach(c =>
				{
					if (data != null && data.Accounting != null && data.Accounting.Select(a => a.AccountingOptionId).Contains(c.Id))
					{
						if (!saleAccountings.Select(cc => cc.AccountingOptionId).Contains(c.Id))
						{
							var collection = data.Accounting
							.Select(a => a)
							.Where(a => a.AccountingOptionId == c.Id)
							.FirstOrDefault();

							var saleAccounting = new SaleBillAccounting()
							{
								StoreId = storeId,
								//AccountingOption = c,
								AccountingOptionId = c.Id,
								CollectionAmount = collection?.CollectionAmount ?? 0,
								SaleBill = bill,
								TerminalId = data.TerminalId,
								BillId = bill.Id
							};
							//添加账户
							InsertSaleBillBillAccounting(saleAccounting);
						}
						else
						{
							//更新账户
							var sbas = saleAccountings.ToList();
							if (sbas != null && sbas.Any())
							{
								foreach (var sba in sbas)
								{
									var collection = data.Accounting
										  .Select(a => a)
										  .Where(a => a.AccountingOptionId == sba.AccountingOptionId)
										  .FirstOrDefault();
						 
									var cur = SaleBillAccountingMappingRepository.Table.Where(s => s.Id == sba.Id).FirstOrDefault();
									if (cur != null)
									{
										cur.CollectionAmount = collection?.CollectionAmount ?? 0;
										cur.TerminalId = data.TerminalId;
										UpdateSaleBillAccounting(cur);
									}
								}
							}
						}
					}
					else
					{
						if (saleAccountings.Select(cc => cc.AccountingOptionId).Contains(c.Id))
						{
							var saleaccountings = saleAccountings
							.Select(cc => cc)
							.Where(cc => cc.AccountingOptionId == c.Id)
							.ToList();

							saleaccountings.ForEach(sa =>
							{
								DeleteSaleBillAccounting(sa);
							});
						}
					}
				});

				#endregion

				#region 预占库存

				////预占库存
				//if (productStockItemThiss != null && productStockItemThiss.Any())
				//{
				//    //修改库存
				//    var faileds = _stockService.AdjustStockQty<SaleBill, SaleItem>(bill,DirectionEnum.Out, StockQuantityType.OrderQuantity, bill.WareHouseId, productStockItemThiss, StockFlowChangeTypeEnum.Save);

				//    //异常回滚
				//    if (faileds == null)
				//    {
				//        transaction?.Rollback();
				//        return new BaseResult { Success = false, Message = Resources.Bill_CreateOrUpdateFailed };
				//    }

				//    //缺失回滚
				//    if (faileds.Item1.Any())
				//    {
				//        transaction?.Rollback();
				//        return new BaseResult { Success = false, Message = $"保存失败,{faileds.Item1.Count} 件商品库存缺失." };
				//    }

				//}
				#endregion

				#region APP签收转单逻辑

				bool sameWareHouse = false;
				if (bill.SaleReservationBillId > 0 && bill.Operation == (int)OperationEnum.APP && data.dispatchItem != null)
				{
					#region 更新调度项目的签收状态

					var dispatchItem = data.dispatchItem;
					dispatchItem.SignStatus = 1;
					dispatchItem.SignUserId = userId;
					dispatchItem.SignDate = DateTime.Now;
					dispatchItem.TerminalId = data.TerminalId;
					//更新状态
					UpdateDispatchItem(dispatchItem);

					#endregion

					if (dispatchItem.SignStatus == 1)
					{
						#region 释放订单预占库存 -> 增加可用库存

						var order = GetSaleReservationBillById(storeId, dispatchItem.BillId, true);
						//销售订单 
						if (order != null && order.Items != null && order.Items.Count > 0)
						{
							//将一个单据中 相同商品 数量 按最小单位汇总
							var stockProducts = new List<ProductStockItem>();
							var allProducts = _productService.GetProductsByIds(storeId, order.Items.Select(pr => pr.ProductId).Distinct().ToArray());
							var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(storeId, allProducts.GetProductBigStrokeSmallUnitIds());

							foreach (var item in order.Items)
							{
								//当前商品
								var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
								//商品库存
								var productStockItem = stockProducts.Where(b => b.ProductId == item.ProductId).FirstOrDefault();
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

							//合集
							var temps = new List<ProductStockItem>();
							if (stockProducts != null && stockProducts.Count > 0)
							{
								stockProducts.ForEach(psi =>
								{
									var productStockItem = new ProductStockItem
									{
										ProductId = psi.ProductId,
										UnitId = psi.UnitId,
										SmallUnitId = psi.SmallUnitId,
										BigUnitId = psi.BigUnitId,
										ProductName = allProducts.Where(s => s.Id == psi.ProductId).FirstOrDefault()?.Name,
										ProductCode = allProducts.Where(s => s.Id == psi.ProductId).FirstOrDefault()?.ProductCode,
										Quantity = psi.Quantity * (-1)
									};
									temps.Add(productStockItem);
								});
							}

							//车库
							var carId = data?.dispatchBill?.CarId ?? 0;
							//ERROR 验证是否有预占（调度后的订单预占量转移至车舱）
							//var checkOrderQuantity = _stockService.CheckOrderQuantity(storeId, BillTypeEnum.SaleReservationBill, order.BillNumber, carId);
							//if (checkOrderQuantity)
							//{
							//    if (carId > 0)
							//    {
							//        //记录出库，清零预占量
							//        _stockService.AdjustStockQty<SaleReservationBill, SaleReservationItem>(order, _productService, _specificationAttributeService, DirectionEnum.Out, StockQuantityType.OrderQuantity, carId, temps, StockFlowChangeTypeEnum.Audited);
							//    }
							//}

							//转单选择的仓库等于单据的仓库时，释放原来单据预占
							sameWareHouse = data.WareHouseId == order.WareHouseId;
							if (sameWareHouse)
							{
								//记录出库，清零预占量
								_stockService.AdjustStockQty<SaleReservationBill, SaleReservationItem>(order, _productService, _specificationAttributeService, DirectionEnum.Out, StockQuantityType.OrderQuantity, order.WareHouseId, temps, StockFlowChangeTypeEnum.Audited);
							}
							else
							{
								//记录出库，释放车库预占
								if (carId > 0)
								{
									_stockService.AdjustStockQty<SaleReservationBill, SaleReservationItem>(order, _productService, _specificationAttributeService, DirectionEnum.Out, StockQuantityType.OrderQuantity, carId, temps, StockFlowChangeTypeEnum.Audited);
								}
							}
						}

						#endregion

						#region 添加送货签收记录

						var deliverySign = new DeliverySign
						{
							StoreId = storeId,
							BillTypeId = dispatchItem.BillTypeId,
							//订单Id
							BillId = order.Id,
							BillNumber = order.BillNumber,
							//转单后销售单Id
							ToBillId = bill.Id,
							ToBillNumber = bill.BillNumber,
							TerminalId = data.TerminalId,
							BusinessUserId = data.BusinessUserId,
							DeliveryUserId = data.DeliveryUserId,
							Latitude = data.Latitude,
							Longitude = data.Longitude,
							SignUserId = userId,
							SignedDate = DateTime.Now,
							SumAmount = order.SumAmount
						};

						//留存照片
						if (photos != null && photos.Any())
						{
							photos.ForEach(p =>
							{
								p.StoreId = storeId;
								deliverySign.RetainPhotos.Add(p);
							});
						}

						InsertDeliverySign(deliverySign);

						#endregion

						//销售订单签收 添加CS出库
						TerminalSignReport terminalSignReport = null;
						List<OrderDetail> orderDetails = null;
						//获取经销商信息
						Store storeInfo = _storeService.GetStoreById(storeId);
						//判断是否上报CS并构造上报信息
						if (storeInfo!=null&&storeInfo.SslEnabled)
						{
							//构造明细信息
							if (items == null || items.Count == 0)
							{
								  return new BaseResult { Success = false, Message = "上报CS库，单据明细为空" }; 
							}
							orderDetails = new List<OrderDetail>();
							foreach (SaleItem saleItem in items)
							{
								OrderDetail detail = new OrderDetail();
								//获取erp编码
								Product product = _productService.GetProductById(storeId, saleItem.ProductId);
								detail.productCode = product.Sku + product.ManufacturerPartNumber;
								detail.productName = product.Name;
								//默认小单位转换 计算小单位数量
								int convertedQuantity = 1;
								if (saleItem.UnitId == product.BigUnitId)
								{
									if (product.BigQuantity == null || product.BigQuantity == 0)
									{
										return new BaseResult { Success = false, Message = product.Name + "商品大单位信息维护错误，请检查" };
									}
									convertedQuantity = (int)product.BigQuantity;
								}
								else if (saleItem.UnitId == product.StrokeUnitId)
								{
									if (product.StrokeQuantity == null || product.StrokeQuantity == 0)
									{
										return new BaseResult { Success = false, Message = product.Name + "商品中单位信息维护错误，请检查" }; 
									}
									convertedQuantity = (int)product.StrokeQuantity;
								}
								else if (saleItem.UnitId == product.SmallUnitId)
								{
									convertedQuantity = 1;
								}
								else
								{
									return new BaseResult { Success = false, Message = product.Name + "商品单位信息维护错误，请检查" }; 
								}

								detail.productAmount = saleItem.Quantity * convertedQuantity;
								detail.type = 0;
								detail.goodsCategory = 31;
								orderDetails.Add(detail);
							}
							//构造上报信息
							var terminal = _terminalService.FindTerminalById(storeId, bill.TerminalId);
							terminalSignReport = new TerminalSignReport();
							terminalSignReport.source = 5;
							terminalSignReport.signType = 0;
							terminalSignReport.sttsBillNo = "5" + bill.BillNumber;
							//在OC和DC经销商是0142000105  在CS是142000105 所以判断如果第一位是0就截取第一位
							string storeCode = storeInfo.Code;
							if (storeCode.StartsWith("0"))
							{
								storeCode = storeCode.Substring(1, storeCode.Length - 1);
							}
							terminalSignReport.dealerCode = storeCode;
							terminalSignReport.dealerName = storeInfo.Name;
							terminalSignReport.whichCode = terminal.Code;
							terminalSignReport.whichName = terminal.Name;
							terminalSignReport.whichName = terminal.Name;
							terminalSignReport.sttsSignDate = deliverySign.SignedDate?.ToString("yyyy-MM-dd HH:mm:ss");
							terminalSignReport.sttsCreatedDate = deliverySign.SignedDate?.ToString("yyyy-MM-dd HH:mm:ss");
							terminalSignReport.latitude = data.Latitude == null ? "" : data.Latitude + "";
							terminalSignReport.longitude = data.Longitude == null ? "" : data.Longitude + "";
							terminalSignReport.region = storeInfo.QuyuCode;
							terminalSignReport.cimformOnUtc = DateTime.Now;
							terminalSignReport.directType = "1";
							terminalSignReport.orderType = "1";
							terminalSignReport.StoreId = storeId;
							// 写入CS表
							TerminalSignReport tsr = _terminalSignReportService.InsertTerminalSignReport(terminalSignReport);
							if (orderDetails != null)
							{
								foreach (OrderDetail orderDetail in orderDetails)
								{
									orderDetail.terminalSignReportId = tsr.Id;
									_orderDetailService.InsertOrderDetail(orderDetail);
								}
							}
						}
					}
				}

				#endregion

				//判断App开单是否自动审核
				bool appBillAutoAudits = false;
				if (data.Operation == (int)OperationEnum.APP)
				{
					appBillAutoAudits = _settingService.AppBillAutoAudits(storeId, BillTypeEnum.SaleBill);
				}

				//读取配置自动审核、管理员创建自动审核
				if ((isAdmin && doAudit) || appBillAutoAudits) //判断当前登录者是否为管理员,若为管理员，开启自动审核
				{
					AuditingNoTran(storeId, userId, bill, sameWareHouse);
				}
				else
				{
					#region 发送审批通知
					try
					{
						//制单人、管理员
						//string userNumbers = _userService.GetAllAdminMobileNumbersAndThisUsersByUser(bill.MakeUserId, new List<int> { bill.MakeUserId });
						var userNumbers = _userService.GetAllAdminUserMobileNumbersByStore(storeId).ToList();
						QueuedMessage queuedMessage = new QueuedMessage()
						{
							StoreId = storeId,
							MType = MTypeEnum.Message,
							Title = CommonHelper.GetEnumDescription<MTypeEnum>(MTypeEnum.Message),
							Date = bill.CreatedOnUtc,
							BillType = BillTypeEnum.SaleBill,
							BillNumber = bill.BillNumber,
							BillId = bill.Id,
							CreatedOnUtc = DateTime.Now
						};
						_queuedMessageService.InsertQueuedMessage(userNumbers,queuedMessage);
					}
					catch (Exception ex)
					{
						_queuedMessageService.WriteLogs(ex.Message);
					}
					#endregion
				}

				//保存事务
				transaction.Commit();

				return new BaseResult { Success = true, Return = billId ?? 0, Message = "单据创建/更新成功", Code = bill.Id };
			}
			catch (Exception ex)
			{
				transaction?.Rollback();
				return new BaseResult { Success = false, Message = "单据创建/更新失败" };
			}
			finally
			{
				using (transaction) { }
			}
		}

		public BaseResult Auditing(int storeId, int userId, SaleBill bill)
		{
			var uow = SaleBillsRepository.UnitOfWork;

			ITransaction transaction = null;
			try
			{

				transaction = uow.BeginOrUseTransaction();

				bill.StoreId = storeId;

				AuditingNoTran(storeId, userId, bill, false);


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


		public BaseResult AuditingNoTran(int storeId, int userId, SaleBill bill, bool sameWareHouse)
		{
			var successful = new BaseResult { Success = true, Message = "单据审核成功" };
			var failed = new BaseResult { Success = false, Message = "单据审核失败" };

			try
			{
				//历史库存记录
				Tuple<List<ProductStockItem>, Tuple<StockInOutRecord, StockInOutRecord>, Tuple<List<StockFlow>, List<StockFlow>>, Tuple<List<StockInOutRecordStockFlow>, List<StockInOutRecordStockFlow>>, Tuple<List<Stock>, List<Stock>>> historyDatas1 = null;

				//记账凭证
				return _recordingVoucherService.CreateVoucher<SaleBill, SaleItem>(bill, storeId, userId, (voucherId) =>
				{
					#region 修改库存
					try
					{
						//转单使用 
						if (bill.SaleReservationBillId > 0 )
						{
							var order = GetSaleReservationBillById(storeId, bill.SaleReservationBillId ?? 0, true);
							//销售订单 
							if (order != null && order.Items != null && order.Items.Count > 0)
							{
								//将一个单据中 相同商品 数量 按最小单位汇总
								var stockProducts = new List<ProductStockItem>();
								var allProducts = _productService.GetProductsByIds(storeId, order.Items.Select(pr => pr.ProductId).Distinct().ToArray());
								var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(storeId, allProducts.GetProductBigStrokeSmallUnitIds());

								foreach (var item in order.Items)
								{
									//当前商品
									var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
									//商品库存
									var productStockItem = stockProducts.Where(b => b.ProductId == item.ProductId).FirstOrDefault();
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
									//销售单开单修改商品开单状态
									product.HasSold = true;
									_productService.UpdateProduct(product);
								}

								//合集
								var temps = new List<ProductStockItem>();
								if (stockProducts != null && stockProducts.Count > 0)
								{
									stockProducts.ForEach(psi =>
									{
										var productStockItem = new ProductStockItem
										{
											ProductId = psi.ProductId,
											UnitId = psi.UnitId,
											SmallUnitId = psi.SmallUnitId,
											BigUnitId = psi.BigUnitId,
											ProductName = allProducts.Where(s => s.Id == psi.ProductId).FirstOrDefault()?.Name,
											ProductCode = allProducts.Where(s => s.Id == psi.ProductId).FirstOrDefault()?.ProductCode,
											Quantity = psi.Quantity * (-1)
										};
										temps.Add(productStockItem);
									});
								}

								//检查原来订单是否有预占
								var oldTemps = new List<ProductStockItem>();
								foreach (var item in temps)
								{
									var checkOrderQuantity = _stockService.CheckOrderQuantity(storeId, BillTypeEnum.SaleReservationBill, order.BillNumber, order.WareHouseId, item.ProductId);
									if (checkOrderQuantity)
									{
										oldTemps.Add(item);
									}
								}

								if(oldTemps.Any())
								{
									//原来订单释放预占量
									_stockService.AdjustStockQty<SaleReservationBill, SaleReservationItem>(order, _productService, _specificationAttributeService, DirectionEnum.Out, StockQuantityType.OrderQuantity, order.WareHouseId, oldTemps, StockFlowChangeTypeEnum.Audited);
								}

								//当前单据减现货
								historyDatas1 = _stockService.AdjustStockQty<SaleBill, SaleItem>(bill, DirectionEnum.Out, StockQuantityType.CurrentQuantity, bill.WareHouseId, temps, StockFlowChangeTypeEnum.Audited);
							}
						}
						else 
						{
							if (bill != null && bill.Items.Any())
							{
								//将一个单据中 相同商品 数量 按最小单位汇总
								var stockProducts = new List<ProductStockItem>();
								var allProducts = _productService.GetProductsByIds(storeId, bill.Items.Select(pr => pr.ProductId).Distinct().ToArray(), true);
								var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(storeId, allProducts.GetProductBigStrokeSmallUnitIds());

								foreach (var item in bill.Items)
								{
									var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
									//商品库存
									var psItem = stockProducts.Where(a => a.ProductId == item.ProductId).FirstOrDefault();
									//商品转化量
									var conversionQuantity = product?.GetConversionQuantity(allOptions, item.UnitId);
									//库存量增量 = 单位转化量 * 数量
									int thisQuantity = item.Quantity * conversionQuantity ?? 0;

									item.ProductName = allProducts.Where(s => s.Id == item.ProductId).FirstOrDefault()?.Name;

									if (psItem != null)
									{
										psItem.Quantity += thisQuantity;
									}
									else
									{
										var curp = allProducts.Where(s => s.Id == item.ProductId).FirstOrDefault();
										psItem = new ProductStockItem
										{
											ProductId = item.ProductId,
											UnitId = item.UnitId,
											SmallUnitId = product.SmallUnitId,
											BigUnitId = product.BigUnitId ?? 0,
											ProductName = curp?.Name,
											ProductCode = curp?.ProductCode,
											Quantity = thisQuantity,
											BillItem = item
										};
										stockProducts.Add(psItem);
									}
									//销售单开单修改商品开单状态
									product.HasSold = true;
									_productService.UpdateProduct(product);
								}

								var productStockItemThiss = new List<ProductStockItem>();
								if (stockProducts.Any())
								{
									productStockItemThiss = stockProducts.Select(p =>
									{
										var curp = allProducts.Where(s => s.Id == p.ProductId).FirstOrDefault();
										return new ProductStockItem
										{
											ProductId = p.ProductId,
											UnitId = p.UnitId,
											SmallUnitId = p.SmallUnitId,
											BigUnitId = p.BigUnitId,
											ProductName = curp?.Name,
											ProductCode = curp?.ProductCode,
											Quantity = p.Quantity * (-1)
										};
									}).ToList();
								}

								//减现货
								historyDatas1 = _stockService.AdjustStockQty<SaleBill, SaleItem>(bill, DirectionEnum.Out, StockQuantityType.CurrentQuantity, bill.WareHouseId, productStockItemThiss, StockFlowChangeTypeEnum.Audited);
							}
						}
					}
					catch (Exception)
					{
					}

					#endregion

					#region 记录上次售价

					try
					{
						if (bill != null && bill.Items != null && bill.Items.Count > 0 && bill.Items.Where(it => it.Price > 0).Count() > 0)
						{
							List<RecordProductPrice> recordProductPrices = new List<RecordProductPrice>();
							bill.Items.ToList().ForEach(it =>
							{
								recordProductPrices.Add(new RecordProductPrice()
								{
									StoreId = bill.StoreId,
									TerminalId = bill.TerminalId,
									ProductId = it.ProductId,
									UnitId = it.UnitId,
									//Price = it.Price
									Price = bill.TaxAmount > 0 ? it.Price / (1 + it.TaxRate / 100) : it.Price //注意这里记录税前价格，因为税率用户可以修改，有可能不等于配置税率
								});
							});

							_productService.RecordRecentPriceLastPrice(storeId, recordProductPrices);
						}
					}
					catch (Exception)
					{
						//回滚
						if (historyDatas1 != null)
						{
							_stockService.RoolBackChanged(historyDatas1);
						}
					}
					#endregion

					#region 赠品信息修改、记录赠送记录

					try
					{
						if (bill != null && bill.Items.Count > 0)
						{
							_costContractBillService.CostContractRecordUpdate(storeId, -1, bill);
						}
					}
					catch (Exception)
					{
						//回滚
						if (historyDatas1 != null)
						{
							_stockService.RoolBackChanged(historyDatas1);
						}
					}
					#endregion

					#region 修改单据表状态

					bill.VoucherId = voucherId;
					bill.AuditedUserId = userId;
					bill.AuditedDate = DateTime.Now;
					bill.AuditedStatus = true;

					//如果欠款小于等于0，则单据已收款
					if (bill.OweCash <= 0)
					{
						bill.ReceivedStatus = ReceiptStatus.Received;
					}
					UpdateSaleBill(bill);
					#endregion

				},
				() =>
				{
					#region 发送通知
					try
					{
						//制单人、管理员
						var userNumbers = new List<string>() { _userService.GetMobileNumberByUserId(bill.BusinessUserId) };
						if (bill.SaleReservationBillId == 0)
						{
							var queuedMessage = new QueuedMessage()
							{
								StoreId = storeId,
								MType = MTypeEnum.Audited,
								Title = CommonHelper.GetEnumDescription<MTypeEnum>(MTypeEnum.Audited),
								Date = bill.CreatedOnUtc,
								BillType = BillTypeEnum.SaleBill,
								BillNumber = bill.BillNumber,
								BillId = bill.Id,
								CreatedOnUtc = DateTime.Now
							};
							_queuedMessageService.InsertQueuedMessage(userNumbers, queuedMessage);
						}
						else
						{
							var queuedMessage2 = new QueuedMessage() //转单完成消息体
							{
								StoreId = storeId,
								MType = MTypeEnum.TransferCompleted,
								Title = CommonHelper.GetEnumDescription<MTypeEnum>(MTypeEnum.TransferCompleted),
								Date = bill.CreatedOnUtc,
								BillType = BillTypeEnum.SaleBill,
								BillNumber = bill.BillNumber,
								BillId = bill.Id,
								CreatedOnUtc = DateTime.Now
							};
							_queuedMessageService.InsertQueuedMessage(userNumbers, queuedMessage2);
						}
					}
					catch (Exception ex)
					{
						_queuedMessageService.WriteLogs(ex.Message);
					}
					#endregion

					return successful;
				},
				() =>
				{
					//回滚库存
					if (historyDatas1 != null)
					{
						_stockService.RoolBackChanged(historyDatas1);
					}

					return failed;
				});

			}
			catch (Exception)
			{
				return failed;
			}
		}

		public BaseResult Reverse(int userId, SaleBill bill)
		{
			var successful = new BaseResult { Success = true, Message = "单据红冲成功" };
			var failed = new BaseResult { Success = false, Message = "单据红冲失败" };

			var uow = SaleBillsRepository.UnitOfWork;

			ITransaction transaction = null;
			try
			{

				transaction = uow.BeginOrUseTransaction();
				//历史库存记录
				Tuple<List<ProductStockItem>, Tuple<StockInOutRecord, StockInOutRecord>, Tuple<List<StockFlow>, List<StockFlow>>, Tuple<List<StockInOutRecordStockFlow>, List<StockInOutRecordStockFlow>>, Tuple<List<Stock>, List<Stock>>> historyDatas1 = null;

				//记账凭证
				_recordingVoucherService.CancleVoucher<SaleBill, SaleItem>(bill, () =>
				{
					#region 修改库存
					try
					{
						var stockProducts = new List<ProductStockItem>();

						var allProducts = _productService.GetProductsByIds(bill.StoreId, bill.Items.Select(pr => pr.ProductId).Distinct().ToArray());
						var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(bill.StoreId, allProducts.GetProductBigStrokeSmallUnitIds());

						foreach (SaleItem item in bill.Items)
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

						historyDatas1 = _stockService.AdjustStockQty<SaleBill, SaleItem>(bill, _productService, _specificationAttributeService, DirectionEnum.In, StockQuantityType.CurrentQuantity, bill.WareHouseId, stockProducts, StockFlowChangeTypeEnum.Reversed);
					}
					catch (Exception)
					{
					}

					#endregion

					#region 赠品信息修改（加）、删除赠送记录
					try
					{
						if (bill != null && bill.Items.Count > 0)
						{
							_costContractBillService.CostContractRecordUpdate(bill.StoreId, 1, bill);
						}
					}
					catch (Exception)
					{
						//回滚
						if (historyDatas1 != null)
						{
							_stockService.RoolBackChanged(historyDatas1);
						}
					}
					#endregion

					#region 修改单据表状态
					bill.ReversedUserId = userId;
					bill.ReversedDate = DateTime.Now;
					bill.ReversedStatus = true;
					//UpdateSaleBill(bill);
					#endregion

					bill.VoucherId = 0;
					UpdateSaleBill(bill);
				});

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

		public BaseResult Delete(int userId, SaleBill bill)
		{
			var successful = new BaseResult { Success = true, Message = "单据作废成功" };
			var failed = new BaseResult { Success = false, Message = "单据作废失败" };

			var uow = SaleBillsRepository.UnitOfWork;

			ITransaction transaction = null;
			try
			{

				transaction = uow.BeginOrUseTransaction();
				#region 修改单据表状态
				bill.Deleted = true;
				#endregion
				UpdateSaleBill(bill);

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

		public bool CheckReversed(int? BillId)
		{
			var query = from a in AllocationBillsRepository.Table
						join b in DispatchBillsRepository.Table on a.DispatchBillId equals b.Id
						join c in DispatchItemsRepository.Table on b.Id equals c.DispatchBillId
						join d in SaleBillsRepository.Table on c.BillId equals d.SaleReservationBillId
						where d.ReversedStatus == false
						&& c.BillTypeId == (int)BillTypeEnum.SaleReservationBill
						&& a.Id == BillId
						select d;
			return !(query.Count() > 0);
		}

		/// <summary>
		/// 更新单据交账状态
		/// </summary>
		/// <param name="store"></param>
		/// <param name="billId"></param>
		/// <param name="handInStatus"></param>
		public void UpdateHandInStatus(int? store, int billId, bool handInStatus)
		{
			var bill = GetSaleBillById(store, billId, false);
			if (bill != null)
			{
				bill.HandInStatus = handInStatus;
				var uow = SaleBillsRepository.UnitOfWork;
				SaleBillsRepository.Update(bill);
				uow.SaveChanges();
				//通知
				_eventPublisher.EntityUpdated(bill);
			}
		}

		/// <summary>
		/// 更新调度
		/// </summary>
		/// <param name="dispatchItem"></param>
		private void UpdateDispatchItem(DispatchItem dispatchItem)
		{
			try
			{
				if (dispatchItem == null)
				{
					throw new ArgumentNullException("dispatchItem");
				}

				var uow = DispatchItemsRepository.UnitOfWork;
				DispatchItemsRepository.Update(dispatchItem);
				uow.SaveChanges();
				//通知
				_eventPublisher.EntityUpdated(dispatchItem);
			}
			catch (Exception)
			{
			}
		}

		/// <summary>
		/// 提交调度记录
		/// </summary>
		/// <param name="deliverySign"></param>
		private void InsertDeliverySign(DeliverySign deliverySign)
		{
			var uow = DeliverySignsRepository.UnitOfWork;
			DeliverySignsRepository.Insert(deliverySign);
			uow.SaveChanges();
			//通知
			_eventPublisher.EntityInserted(deliverySign);
		}

		/// <summary>
		/// 获取订单
		/// </summary>
		/// <param name="store"></param>
		/// <param name="saleReservationBillId"></param>
		/// <param name="isInclude"></param>
		/// <returns></returns>
		private SaleReservationBill GetSaleReservationBillById(int? store, int saleReservationBillId, bool isInclude = false)
		{
			if (saleReservationBillId == 0)
			{
				return null;
			}

			if (isInclude)
			{
				var query = SaleReservationBillsRepository.Table
				.Include(sb => sb.Items)
				//.ThenInclude(sb => sb.SaleReservationBill)
				.Include(sb => sb.SaleReservationBillAccountings)
				.ThenInclude(sb => sb.AccountingOption);

				return query.FirstOrDefault(s => s.Id == saleReservationBillId);
			}
			return SaleReservationBillsRepository.ToCachedGetById(saleReservationBillId);
		}
	}
}
