using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Data;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Finances;
using DCMS.Core.Domain.Tasks;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using DCMS.Services.Sales;
using DCMS.Services.Tasks;
using DCMS.Services.Users;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using DCMS.Services.Caching;
using DCMS.Services.Configuration;
using DCMS.Core.Domain.Report;
using DCMS.Services.Common;
using DCMS.Core.Domain.Common;

namespace DCMS.Services.Finances
{
    /// <summary>
    /// 收款单服务
    /// </summary>
    public partial class CashReceiptBillService : BaseService, ICashReceiptBillService
    {
        private readonly IUserService _userService;
        private readonly IQueuedMessageService _queuedMessageService;
        private readonly IRecordingVoucherService _recordingVoucherService;
        private readonly ISaleBillService _saleBillService;
        private readonly IReturnBillService _returnBillService;
        private readonly IAdvanceReceiptBillService _advanceReceiptBillService;
        private readonly ICostExpenditureBillService _costExpenditureBillService;
        private readonly IFinancialIncomeBillService _financialIncomeBillService;
        private readonly ISettingService _settingService;
        private readonly ICommonBillService _commonBillService;


        public CashReceiptBillService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher,
            IUserService userService,
            IQueuedMessageService queuedMessageService,
            IRecordingVoucherService recordingVoucherService,
            ISaleBillService saleBillService,
            IAdvanceReceiptBillService advanceReceiptBillService,
            ICostExpenditureBillService costExpenditureBillService,
            IFinancialIncomeBillService financialIncomeBillService,
            ISettingService settingService,
            ICommonBillService commonBillService,
            IReturnBillService returnBillService) : base(getter, cacheManager, eventPublisher)
        {
            _userService = userService;
            _queuedMessageService = queuedMessageService;
            _recordingVoucherService = recordingVoucherService;
            _saleBillService = saleBillService;
            _returnBillService = returnBillService;
            _advanceReceiptBillService = advanceReceiptBillService;
            _costExpenditureBillService = costExpenditureBillService;
            _financialIncomeBillService = financialIncomeBillService;
            _settingService = settingService;
            _commonBillService = commonBillService;
        }

        #region 单据

        public bool Exists(int billId)
        {
            return CashReceiptBillsRepository.TableNoTracking.Where(a => a.Id == billId).Count() > 0;
        }

        public virtual IPagedList<CashReceiptBill> GetAllCashReceiptBills(int? store, int? makeuserId, int? customerId, int? payeerId, string billNumber = "", bool? status = null, DateTime? start = null, DateTime? end = null, bool? isShowReverse = null, bool? sortByAuditedTime = null, string remark = "", bool? deleted = null, bool? handleStatus = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;
            //var query = CashReceiptBillsRepository.Table;
            var query = from pc in CashReceiptBillsRepository.Table
                        .Include(cr => cr.Items)
                        //.ThenInclude(cr => cr.CashReceiptBill)
                        .Include(cr => cr.CashReceiptBillAccountings)
                        .ThenInclude(cr => cr.AccountingOption)
                        select pc;

            if (store.HasValue)
            {
                query = query.Where(c => c.StoreId == store);
            }
            else
            {
                return null;
            }

            if (makeuserId.HasValue && makeuserId > 0)
            {
                var userIds = _userService.GetSubordinate(store, makeuserId ?? 0)?.Where(s => s > 0).ToList();
                if (userIds.Count > 0)
                    query = query.Where(x => userIds.Contains(x.MakeUserId));
            }

            if (customerId.HasValue && customerId.Value > 0)
            {
                query = query.Where(c => c.CustomerId == customerId);
            }

            if (payeerId.HasValue && payeerId.Value > 0)
            {
                query = query.Where(c => c.Payeer == payeerId);
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
                query = query.Where(o => start.Value <= o.CreatedOnUtc);
            }

            if (end.HasValue)
            {
                query = query.Where(o => end.Value >= o.CreatedOnUtc);
            }

            if (isShowReverse.HasValue)
            {
                query = query.Where(c => c.ReversedStatus == isShowReverse);
            }

            if (!string.IsNullOrWhiteSpace(remark))
            {
                query = query.Where(c => c.Remark.Contains(remark));
            }

            if (deleted.HasValue)
            {
                query = query.Where(c => c.Deleted == deleted);
            }

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

            //var unsortedCashReceiptBills = query.ToList();
            ////分页
            //return new PagedList<CashReceiptBill>(unsortedCashReceiptBills, pageIndex, pageSize);
            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<CashReceiptBill>(plists, pageIndex, pageSize, totalCount);
        }

        public virtual IList<CashReceiptBill> GetAllCashReceiptBills()
        {
            var query = from c in CashReceiptBillsRepository.Table
                        orderby c.Id
                        select c;

            var categories = query.ToList();
            return categories;
        }

        public virtual CashReceiptBill GetCashReceiptBillById(int? store, int cashReceiptBillId)
        {
            if (cashReceiptBillId == 0)
            {
                return null;
            }

            var key = DCMSDefaults.CASHRECEIPTBILL_BY_ID_KEY.FillCacheKey(store ?? 0, cashReceiptBillId);
            return _cacheManager.Get(key, () =>
            {
                return CashReceiptBillsRepository.ToCachedGetById(cashReceiptBillId);
            });
        }

        public virtual CashReceiptBill GetCashReceiptBillById(int? store, int cashReceiptBillId, bool isInclude = false)
        {
            if (cashReceiptBillId == 0)
            {
                return null;
            }

            if (isInclude)
            {
                var query = CashReceiptBillsRepository.Table
                .Include(cr => cr.Items)
                //.ThenInclude(cb => cb.CashReceiptBill)
                .Include(cr => cr.CashReceiptBillAccountings)
                .ThenInclude(ao => ao.AccountingOption);

                return query.FirstOrDefault(c => c.Id == cashReceiptBillId);
            }
            return CashReceiptBillsRepository.ToCachedGetById(cashReceiptBillId);
        }


        public virtual CashReceiptBill GetCashReceiptBillByNumber(int? store, string billNumber)
        {
            var key = DCMSDefaults.CASHRECEIPTBILL_BY_NUMBER_KEY.FillCacheKey(store ?? 0, billNumber);
            return _cacheManager.Get(key, () =>
            {
                var query = CashReceiptBillsRepository.Table;
                var cashReceiptBill = query.Where(a => a.BillNumber == billNumber).FirstOrDefault();
                return cashReceiptBill;
            });
        }



        public virtual void InsertCashReceiptBill(CashReceiptBill cashReceiptBill)
        {
            if (cashReceiptBill == null)
            {
                throw new ArgumentNullException("cashReceiptBill");
            }

            var uow = CashReceiptBillsRepository.UnitOfWork;
            CashReceiptBillsRepository.Insert(cashReceiptBill);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(cashReceiptBill);
        }

        public virtual void UpdateCashReceiptBill(CashReceiptBill cashReceiptBill)
        {
            if (cashReceiptBill == null)
            {
                throw new ArgumentNullException("cashReceiptBill");
            }

            var uow = CashReceiptBillsRepository.UnitOfWork;
            CashReceiptBillsRepository.Update(cashReceiptBill);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(cashReceiptBill);
        }

        public virtual void DeleteCashReceiptBill(CashReceiptBill cashReceiptBill)
        {
            if (cashReceiptBill == null)
            {
                throw new ArgumentNullException("cashReceiptBill");
            }

            var uow = CashReceiptBillsRepository.UnitOfWork;
            CashReceiptBillsRepository.Delete(cashReceiptBill);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityDeleted(cashReceiptBill);
        }


        #endregion

        #region 单据项目


        public virtual IPagedList<CashReceiptItem> GetCashReceiptItemsByCashReceiptBillId(int cashReceiptBillId, int? userId, int? storeId, int pageIndex, int pageSize)
        {
            if (pageSize >= 50)
                pageSize = 50;
            if (cashReceiptBillId == 0)
            {
                return new PagedList<CashReceiptItem>(new List<CashReceiptItem>(), pageIndex, pageSize);
            }

            var key = DCMSDefaults.CASHRECEIPTBILLITEM_ALL_KEY.FillCacheKey(storeId, cashReceiptBillId, pageIndex, pageSize, userId);

            return _cacheManager.Get(key, () =>
            {
                var query = from pc in CashReceiptItemsRepository.Table
                            where pc.CashReceiptBillId == cashReceiptBillId
                            orderby pc.Id
                            select pc;
                //var productCashReceiptBills = new PagedList<CashReceiptItem>(query.ToList(), pageIndex, pageSize);
                //return productCashReceiptBills;
                //总页数
                var totalCount = query.Count();
                var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
                return new PagedList<CashReceiptItem>(plists, pageIndex, pageSize, totalCount);
            });
        }

        public virtual List<CashReceiptItem> GetCashReceiptItemList(int cashReceiptBillId)
        {
            List<CashReceiptItem> cashReceiptItems = null;
            var query = CashReceiptItemsRepository_RO.Table.Include(s => s.CashReceiptBill);
            cashReceiptItems = query.Where(a => a.CashReceiptBillId == cashReceiptBillId).ToList();
            return cashReceiptItems;
        }

        public virtual CashReceiptItem GetCashReceiptItemById(int? store, int cashReceiptItemId)
        {
            if (cashReceiptItemId == 0)
            {
                return null;
            }

            var key = DCMSDefaults.CASHRECEIPTBILLITEM_BY_ID_KEY.FillCacheKey(store ?? 0, cashReceiptItemId);
            return _cacheManager.Get(key, () => { return CashReceiptItemsRepository.ToCachedGetById(cashReceiptItemId); });
        }

        public virtual void InsertCashReceiptItem(CashReceiptItem cashReceiptItem)
        {
            if (cashReceiptItem == null)
            {
                throw new ArgumentNullException("cashReceiptItem");
            }

            var uow = CashReceiptItemsRepository.UnitOfWork;
            CashReceiptItemsRepository.Insert(cashReceiptItem);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(cashReceiptItem);
        }

        public virtual void UpdateCashReceiptItem(CashReceiptItem cashReceiptItem)
        {
            if (cashReceiptItem == null)
            {
                throw new ArgumentNullException("cashReceiptItem");
            }

            var uow = CashReceiptItemsRepository.UnitOfWork;
            CashReceiptItemsRepository.Update(cashReceiptItem);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(cashReceiptItem);
        }

        public virtual void DeleteCashReceiptItem(CashReceiptItem cashReceiptItem)
        {
            if (cashReceiptItem == null)
            {
                throw new ArgumentNullException("cashReceiptItem");
            }

            var uow = CashReceiptItemsRepository.UnitOfWork;
            CashReceiptItemsRepository.Delete(cashReceiptItem);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(cashReceiptItem);
        }


        #endregion

        #region 收款账户映射

        public virtual IPagedList<CashReceiptBillAccounting> GetCashReceiptBillAccountingsByCashReceiptBillId(int storeId, int userId, int cashReceiptBillId, int pageIndex, int pageSize)
        {
            if (pageSize >= 50)
                pageSize = 50;
            if (cashReceiptBillId == 0)
            {
                return new PagedList<CashReceiptBillAccounting>(new List<CashReceiptBillAccounting>(), pageIndex, pageSize);
            }

            //string key = string.Format(CASHRECEIPTBILL_ACCOUNTINGL_BY_BILLID_KEY.FillCacheKey( cashReceiptBillId, pageIndex, pageSize, _workContext.CurrentUser.Id, _workContext.CurrentStore.Id);
            var key = DCMSDefaults.CASHRECEIPTBILL_ACCOUNTING_ALLBY_BILLID_KEY.FillCacheKey(storeId, cashReceiptBillId, pageIndex, pageSize, userId);
            return _cacheManager.Get(key, () =>
            {
                var query = from pc in CashReceiptBillAccountingMappingRepository.Table
                            join p in AccountingOptionsRepository.Table on pc.AccountingOptionId equals p.Id
                            where pc.BillId == cashReceiptBillId
                            orderby pc.Id
                            select pc;

                //var saleAccountings = new PagedList<CashReceiptBillAccounting>(query.ToList(), pageIndex, pageSize);
                //return saleAccountings;
                //总页数
                var totalCount = query.Count();
                var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
                return new PagedList<CashReceiptBillAccounting>(plists, pageIndex, pageSize, totalCount);
            });
        }

        public virtual IList<CashReceiptBillAccounting> GetCashReceiptBillAccountingsByCashReceiptBillId(int? store, int cashReceiptBillId)
        {

            var key = DCMSDefaults.CASHRECEIPTBILL_ACCOUNTINGL_BY_BILLID_KEY.FillCacheKey(store ?? 0, cashReceiptBillId);
            return _cacheManager.Get(key, () =>
            {
                var query = from pc in CashReceiptBillAccountingMappingRepository.Table
                            join p in AccountingOptionsRepository.Table on pc.AccountingOptionId equals p.Id
                            where pc.BillId == cashReceiptBillId
                            orderby pc.Id
                            select pc;

                return query.ToList();
            });
        }

        public virtual CashReceiptBillAccounting GetCashReceiptBillAccountingById(int cashReceiptBillAccountingId)
        {
            if (cashReceiptBillAccountingId == 0)
            {
                return null;
            }

            return CashReceiptBillAccountingMappingRepository.ToCachedGetById(cashReceiptBillAccountingId);
        }

        public virtual void InsertCashReceiptBillAccounting(CashReceiptBillAccounting cashReceiptBillAccounting)
        {
            if (cashReceiptBillAccounting == null)
            {
                throw new ArgumentNullException("cashReceiptBillAccounting");
            }

            var uow = CashReceiptBillAccountingMappingRepository.UnitOfWork;
            CashReceiptBillAccountingMappingRepository.Insert(cashReceiptBillAccounting);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(cashReceiptBillAccounting);
        }

        public virtual void UpdateCashReceiptBillAccounting(CashReceiptBillAccounting cashReceiptBillAccounting)
        {
            if (cashReceiptBillAccounting == null)
            {
                throw new ArgumentNullException("cashReceiptBillAccounting");
            }

            var uow = CashReceiptBillAccountingMappingRepository.UnitOfWork;
            CashReceiptBillAccountingMappingRepository.Update(cashReceiptBillAccounting);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(cashReceiptBillAccounting);
        }

        public virtual void DeleteCashReceiptBillAccounting(CashReceiptBillAccounting cashReceiptBillAccounting)
        {
            if (cashReceiptBillAccounting == null)
            {
                throw new ArgumentNullException("cashReceiptBillAccounting");
            }

            var uow = CashReceiptBillAccountingMappingRepository.UnitOfWork;
            CashReceiptBillAccountingMappingRepository.Delete(cashReceiptBillAccounting);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(cashReceiptBillAccounting);
        }


        #endregion

        /// <summary>
        /// 获取当前单据的所有收款账户(目的:在查询时不依赖延迟加载,由于获的较高查询性能)
        /// </summary>
        /// <returns></returns>
        public IList<CashReceiptBillAccounting> GetAllCashReceiptBillAccountingsByBillIds(int? store, int[] billIds)
        {
            if (billIds.Length == 0)
            {
                return new List<CashReceiptBillAccounting>();
            }

            var key = DCMSDefaults.CASHRECEIPTBILL_ACCOUNTINGL_BY_BILLID_KEY.FillCacheKey(store ?? 0, string.Join("_", billIds));
            return _cacheManager.Get(key, () =>
            {
                var query = from pc in CashReceiptBillAccountingMappingRepository.Table
                            .Include(cr => cr.AccountingOption)
                            where billIds.Contains(pc.BillId)
                            select pc;
                return query.ToList();
            });
        }

        public void UpdateCashReceiptBillActive(int? store, int? billId, int? user)
        {
            var query = CashReceiptBillsRepository.Table.ToList();

            query = query.Where(x => x.StoreId == store && x.MakeUserId == user && x.AuditedStatus == true && (DateTime.Now.Subtract(x.AuditedDate ?? DateTime.Now).Duration().TotalDays > 30)).ToList();

            if (billId.HasValue && billId.Value > 0)
            {
                query = query.Where(x => x.Id == billId).ToList();
            }
            
            var result = query;

            if (result != null && result.Count > 0)
            {
                var uow = CashReceiptBillsRepository.UnitOfWork;
                foreach (CashReceiptBill bill in result)
                {
                    if ((bill.AuditedStatus && !bill.ReversedStatus) || bill.Deleted) continue;
                    bill.Deleted = true;
                    CashReceiptBillsRepository.Update(bill);
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
        public IList<CashReceiptBill> GetCashReceiptBillListToFinanceReceiveAccount(int? storeId, int? employeeId = null, DateTime? start = null, DateTime? end = null)
        {
            var query = CashReceiptBillsRepository.Table;

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
                query = query.Where(a => a.Payeer == employeeId);
            }

            query = query.OrderByDescending(a => a.CreatedOnUtc);

            return query.ToList();
        }


        /// <summary>
        /// 提交收款单
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="userId"></param>
        /// <param name="billId"></param>
        /// <param name="bill"></param>
        /// <param name="accountingOptions"></param>
        /// <param name="accountings"></param>
        /// <param name="data"></param>
        /// <param name="items"></param>
        /// <param name="isAdmin"></param>
        /// <returns></returns>
        public BaseResult BillCreateOrUpdate(int storeId, int userId, int? billId, CashReceiptBill bill, List<CashReceiptBillAccounting> accountingOptions, List<AccountingOption> accountings, CashReceiptBillUpdate data, List<CashReceiptItem> items, bool isAdmin = false, bool doAudit = true)
        {
            var uow = CashReceiptBillsRepository.UnitOfWork;

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
                    #region 更新收款单

                    if (bill != null)
                    {
                        //客户
                        bill.CustomerId = data.CustomerId;
                        //收款人
                        bill.Payeer = data.Payeer ?? 0;
                        //备注
                        bill.Remark = data.Remark;

                        bill.OweCash = data.OweCash;
                        bill.ReceivableAmount = data.ReceivableAmount;
                        bill.PreferentialAmount = data.PreferentialAmount;

                        UpdateCashReceiptBill(bill);
                    }

                    #endregion
                }
                else
                {
                    #region 添加收款单

                    bill.StoreId = storeId;
                    //客户
                    bill.CustomerId = data.CustomerId;
                    //收款人
                    bill.Payeer = data.Payeer ?? 0;
                    //开单日期
                    bill.CreatedOnUtc = DateTime.Now;
                    //单据编号
                    bill.BillNumber = string.IsNullOrEmpty(data.BillNumber) ? CommonHelper.GetBillNumber("SK", storeId) : data.BillNumber;
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

                    bill.OweCash = data.OweCash;
                    bill.ReceivableAmount = data.ReceivableAmount;
                    bill.PreferentialAmount = data.PreferentialAmount;

                    InsertCashReceiptBill(bill);

                    #endregion
                }

                #region 更新收款项目

                data.Items.ForEach(p =>
                {
                    if (!string.IsNullOrEmpty(p.BillNumber) && p.BillTypeId != 0)
                    {
                        var sd = GetCashReceiptItemById(storeId, p.Id);
                        if (sd == null)
                        {
                            //追加项
                            if (bill.Items.Count(cp => cp.Id == p.Id) == 0)
                            {
                                var item = p;
                                item.BillId = p.BillId;
                                item.CashReceiptBillId = bill.Id;
                                item.CreatedOnUtc = DateTime.Now;
                                item.StoreId = storeId;
                                item.BillId = p.BillId;
                                InsertCashReceiptItem(item);

                                //不排除
                                p.Id = item.Id;
                                if (!bill.Items.Select(s => s.Id).Contains(item.Id))
                                {
                                    bill.Items.Add(item);
                                }

                                #region 将修改单据 部分收款/已收款  逻辑移动至审核功能
                                ////已收款
                                //if (item.AmountOwedAfterReceipt == decimal.Zero)
                                //{
                                //    //更新开单状态
                                //    switch (item.BillTypeEnum)
                                //    {
                                //        case BillTypeEnum.SaleBill:
                                //            _saleBillService.UpdateReceived(storeId, item.BillId, ReceiptStatus.Received);
                                //            break;
                                //        case BillTypeEnum.ReturnBill:
                                //            _returnBillService.UpdateReceived(storeId, item.BillId, ReceiptStatus.Received);
                                //            break;
                                //        case BillTypeEnum.AdvanceReceiptBill:
                                //            _advanceReceiptBillService.UpdateReceived(storeId, item.BillId, ReceiptStatus.Received);
                                //            break;
                                //        case BillTypeEnum.CostExpenditureBill:
                                //            _costExpenditureBillService.UpdateReceived(storeId, item.BillId, ReceiptStatus.Received);
                                //            break;
                                //        case BillTypeEnum.FinancialIncomeBill:
                                //            _financialIncomeBillService.UpdateReceived(storeId, item.BillId, ReceiptStatus.Received);
                                //            break;
                                //    }
                                //}
                                ////部分收款
                                //else if (item.AmountOwedAfterReceipt <= item.ArrearsAmount)
                                //{
                                //    //更新开单状态
                                //    switch (item.BillTypeEnum)
                                //    {
                                //        case BillTypeEnum.SaleBill:
                                //            _saleBillService.UpdateReceived(storeId, item.BillId, ReceiptStatus.Part);
                                //            break;
                                //        case BillTypeEnum.ReturnBill:
                                //            _returnBillService.UpdateReceived(storeId, item.BillId, ReceiptStatus.Part);
                                //            break;
                                //        case BillTypeEnum.AdvanceReceiptBill:
                                //            _advanceReceiptBillService.UpdateReceived(storeId, item.BillId, ReceiptStatus.Part);
                                //            break;
                                //        case BillTypeEnum.CostExpenditureBill:
                                //            _costExpenditureBillService.UpdateReceived(storeId, item.BillId, ReceiptStatus.Part);
                                //            break;
                                //        case BillTypeEnum.FinancialIncomeBill:
                                //            _financialIncomeBillService.UpdateReceived(storeId, item.BillId, ReceiptStatus.Part);
                                //            break;
                                //    }
                                //}
                                //else
                                //{

                                //}
                                #endregion
                            }
                        }
                        else
                        {
                            //存在则更新
                            sd.BillNumber = p.BillNumber;
                            sd.BillTypeId = p.BillTypeId;//单据类型
                            sd.Amount = p.Amount;// 单据金额
                            sd.MakeBillDate = sd.MakeBillDate;//开单时间
                            sd.DiscountAmount = p.DiscountAmount;//优惠金额
                            sd.PaymentedAmount = p.PaymentedAmount;//已收金额
                            sd.ArrearsAmount = p.ArrearsAmount;//尚欠金额
                            sd.DiscountAmountOnce = p.DiscountAmountOnce;//本次优惠金额
                            sd.ReceivableAmountOnce = p.ReceivableAmountOnce;//本次收款金额
                            sd.AmountOwedAfterReceipt = p.AmountOwedAfterReceipt;//收款后尚欠金额
                            sd.Remark = p.Remark;//备注
                            sd.BillId = p.BillId;//收款单据

                            #region 将修改单据 部分收款/已收款逻辑移动至 审核功能
                            ////已收款
                            //if (sd.AmountOwedAfterReceipt == decimal.Zero)
                            //{
                            //    //更新开单状态
                            //    switch (sd.BillTypeEnum)
                            //    {
                            //        case BillTypeEnum.SaleBill:
                            //            _saleBillService.UpdateReceived(storeId, sd.BillId, ReceiptStatus.Received);
                            //            break;
                            //        case BillTypeEnum.ReturnBill:
                            //            _returnBillService.UpdateReceived(storeId, sd.BillId, ReceiptStatus.Received);
                            //            break;
                            //        case BillTypeEnum.AdvanceReceiptBill:
                            //            _advanceReceiptBillService.UpdateReceived(storeId, sd.BillId, ReceiptStatus.Received);
                            //            break;
                            //        case BillTypeEnum.CostExpenditureBill:
                            //            _costExpenditureBillService.UpdateReceived(storeId, sd.BillId, ReceiptStatus.Received);
                            //            break;
                            //        case BillTypeEnum.FinancialIncomeBill:
                            //            _financialIncomeBillService.UpdateReceived(storeId, sd.BillId, ReceiptStatus.Received);
                            //            break;
                            //    }
                            //}
                            ////部分收款
                            //else if (sd.AmountOwedAfterReceipt <= sd.ArrearsAmount)
                            //{
                            //    //更新开单状态
                            //    switch (sd.BillTypeEnum)
                            //    {
                            //        case BillTypeEnum.SaleBill:
                            //            _saleBillService.UpdateReceived(storeId, sd.BillId, ReceiptStatus.Part);
                            //            break;
                            //        case BillTypeEnum.ReturnBill:
                            //            _returnBillService.UpdateReceived(storeId, sd.BillId, ReceiptStatus.Part);
                            //            break;
                            //        case BillTypeEnum.AdvanceReceiptBill:
                            //            _advanceReceiptBillService.UpdateReceived(storeId, sd.BillId, ReceiptStatus.Part);
                            //            break;
                            //        case BillTypeEnum.CostExpenditureBill:
                            //            _costExpenditureBillService.UpdateReceived(storeId, sd.BillId, ReceiptStatus.Part);
                            //            break;
                            //        case BillTypeEnum.FinancialIncomeBill:
                            //            _financialIncomeBillService.UpdateReceived(storeId, sd.BillId, ReceiptStatus.Part);
                            //            break;
                            //    }
                            //}
                            //else
                            //{

                            //}
                            #endregion
                            UpdateCashReceiptItem(sd);
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
                        var item = GetCashReceiptItemById(storeId, p.Id);
                        if (item != null)
                        {
                            DeleteCashReceiptItem(item);
                        }
                    }
                });

                #endregion

                #region 收款账户映射

                var cashReceiptBillAccountings = GetCashReceiptBillAccountingsByCashReceiptBillId(storeId, bill.Id);
                accountings.ToList().ForEach(c =>
                {
                    if (data.Accounting.Select(a => a.AccountingOptionId).Contains(c.Id))
                    {
                        if (!cashReceiptBillAccountings.Select(cc => cc.AccountingOptionId).Contains(c.Id))
                        {
                            var collection = data.Accounting.Select(a => a).Where(a => a.AccountingOptionId == c.Id).FirstOrDefault();
                            var cashReceiptBillAccounting = new CashReceiptBillAccounting()
                            {
                                //AccountingOption = c,
                                AccountingOptionId = c.Id,
                                CollectionAmount = collection != null ? collection.CollectionAmount : 0,
                                CashReceiptBill = bill,
                                BillId = bill.Id,
                                TerminalId = data.CustomerId,
                                StoreId = storeId
                            };
                            //添加账户
                            InsertCashReceiptBillAccounting(cashReceiptBillAccounting);
                        }
                        else
                        {
                            cashReceiptBillAccountings.ToList().ForEach(acc =>
                            {
                                var collection = data.Accounting.Select(a => a).Where(a => a.AccountingOptionId == acc.AccountingOptionId).FirstOrDefault();
                                acc.CollectionAmount = collection != null ? collection.CollectionAmount : 0;
                                acc.TerminalId = data.CustomerId;

                                //更新账户
                                UpdateCashReceiptBillAccounting(acc);
                            });
                        }
                    }
                    else
                    {
                        if (cashReceiptBillAccountings.Select(cc => cc.AccountingOptionId).Contains(c.Id))
                        {
                            var saleaccountings = cashReceiptBillAccountings.Select(cc => cc).Where(cc => cc.AccountingOptionId == c.Id).ToList();
                            saleaccountings.ForEach(sa =>
                            {
                                DeleteCashReceiptBillAccounting(sa);
                            });
                        }
                    }
                });

                #endregion

                bool appBillAutoAudits = false;
                if (data.Operation == (int)OperationEnum.APP)
                {
                    appBillAutoAudits = _settingService.AppBillAutoAudits(storeId, BillTypeEnum.CashReceiptBill);
                }

                //管理员创建自动审核
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
                            BillType = BillTypeEnum.CashReceiptBill,
                            BillNumber = bill.BillNumber,
                            BillId = bill.Id,
                            CreatedOnUtc = DateTime.Now
                        };
                        _queuedMessageService.InsertQueuedMessage(adminNumbers, queuedMessage);
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
                return new BaseResult { Success = false, Message = ex.Message };
            }
            finally
            {
                //不管怎样最后都会关闭掉这个事务
                using (transaction) { }
            }
        }

        public BaseResult Auditing(int storeId, int userId, CashReceiptBill bill)
        {
            var uow = CashReceiptBillsRepository.UnitOfWork;

            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();

                bill.StoreId = storeId;
                //bill.MakeUserId = userId;


                //if (!CheckTerminalCashReceiptSettled(storeId, bill.CustomerId, bill.ReceivableAmount))
                //{
                //    return new BaseResult { Success = true, Message = "客户欠款已经结完，审核失败！" };
                //}

                foreach (var item in bill.Items)
                {
                    if (GetBillIsReceipted(storeId, item.BillId, item.BillTypeEnum)) 
                    {
                        return new BaseResult { Success = true, Message = $"审核失败,单据{item.BillNumber}款项已结清！" };
                    }
                }

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

        public BaseResult AuditingNoTran(int storeId, int userId, CashReceiptBill bill)
        {
            var successful = new BaseResult { Success = true, Message = "单据审核成功" };
            var failed = new BaseResult { Success = false, Message = "单据审核失败" };

            try
            {
                return _recordingVoucherService.CreateVoucher<CashReceiptBill, CashReceiptItem>(bill, storeId, userId, (voucherId) =>
                {
                    #region 修改单据已收款/部分收款
                    //获取单据项目
                    bill.Items.ToList().ForEach(i=> 
                    {
                        var sd = GetCashReceiptItemById(storeId, i.Id);
                        if (sd != null)
                        {
                            var receiptStatus = ReceiptStatus.None;
                            if (sd.ArrearsAmount < 0)
                            {
                                if (sd.ReceivableAmountOnce < 0 && sd.AmountOwedAfterReceipt < 0)
                                    receiptStatus = ReceiptStatus.Part;
                                else if(sd.ReceivableAmountOnce<0 && sd.AmountOwedAfterReceipt >= 0)
                                    receiptStatus = ReceiptStatus.Received;
                            }
                            else 
                            {
                                if (sd.ReceivableAmountOnce > 0 && sd.AmountOwedAfterReceipt > 0)
                                    receiptStatus = ReceiptStatus.Part;
                                else if (sd.ReceivableAmountOnce > 0 && sd.AmountOwedAfterReceipt <= 0)
                                    receiptStatus = ReceiptStatus.Received;
                            }
                            //if (sd.AmountOwedAfterReceipt == decimal.Zero)
                            //    receiptStatus = ReceiptStatus.Received; //已收款
                            //else if (sd.AmountOwedAfterReceipt <= sd.ArrearsAmount) //欠款为
                            //    receiptStatus = ReceiptStatus.Part; //部分收款
                            //更新开单状态
                            switch (sd.BillTypeEnum)
                            {
                                case BillTypeEnum.SaleBill:
                                    _saleBillService.UpdateReceived(storeId, sd.BillId, receiptStatus);
                                    break;
                                case BillTypeEnum.ReturnBill:
                                    _returnBillService.UpdateReceived(storeId, sd.BillId, receiptStatus);
                                    break;
                                case BillTypeEnum.AdvanceReceiptBill:
                                    _advanceReceiptBillService.UpdateReceived(storeId, sd.BillId, receiptStatus);
                                    break;
                                case BillTypeEnum.CostExpenditureBill:
                                    _costExpenditureBillService.UpdateReceived(storeId, sd.BillId, receiptStatus);
                                    break;
                                case BillTypeEnum.FinancialIncomeBill:
                                    _financialIncomeBillService.UpdateReceived(storeId, sd.BillId, receiptStatus);
                                    break;
                            }
                        }
                    });
                    #endregion

                    #region 修改单据信息
                    bill.VoucherId = voucherId;
                    bill.AuditedDate = DateTime.Now;
                    bill.AuditedUserId = userId;
                    bill.AuditedStatus = true;
                    UpdateCashReceiptBill(bill);
                    #endregion
                },
                () =>
                {
                    #region 发送通知
                    try
                    {
                        //制单人
                        var userNumbers = new List<string>() { _userService.GetMobileNumberByUserId(bill.MakeUserId) };
                        QueuedMessage queuedMessage = new QueuedMessage()
                        {
                            StoreId = storeId,
                            MType = MTypeEnum.Audited,
                            Title = CommonHelper.GetEnumDescription<MTypeEnum>(MTypeEnum.Audited),
                            Date = bill.CreatedOnUtc,
                            BillType = BillTypeEnum.CashReceiptBill,
                            BillNumber = bill.BillNumber,
                            BillId = bill.Id,
                            CreatedOnUtc = DateTime.Now
                        };
                        _queuedMessageService.InsertQueuedMessage(userNumbers.ToList(), queuedMessage);
                    }
                    catch (Exception ex)
                    {
                        _queuedMessageService.WriteLogs(ex.Message);
                    }
                    #endregion

                    return successful;
                },
                () => { return failed; });
            }
            catch (Exception)
            {
                return failed;
            }

        }
        #region 判断单据是否已收款
        private bool GetBillIsReceipted(int storeId,int billId, BillTypeEnum billType) 
        {
            var isTrue = false;
            switch (billType)
            {
                case BillTypeEnum.SaleBill:
                    var saleBill = _saleBillService.GetSaleBillById(storeId,billId);
                    if (saleBill != null && saleBill.ReceivedStatus == ReceiptStatus.Received) 
                        isTrue = true;
                    break;
                case BillTypeEnum.ReturnBill:
                    var returnBill = _returnBillService.GetReturnBillById(storeId,billId);
                    if (returnBill != null && returnBill.ReceivedStatus == ReceiptStatus.Received)
                        isTrue = true;
                    break;
                case BillTypeEnum.AdvanceReceiptBill:
                    var advanceReceiptBill =_advanceReceiptBillService.GetAdvanceReceiptBillById(storeId,billId);
                    if (advanceReceiptBill != null && advanceReceiptBill.ReceivedStatus == ReceiptStatus.Received)
                        isTrue = true;
                    break;
                case BillTypeEnum.CostExpenditureBill:
                    var costExpenditureBill = _costExpenditureBillService.GetCostExpenditureBillById(storeId,billId);
                    if (costExpenditureBill != null && costExpenditureBill.ReceivedStatus == ReceiptStatus.Received)
                        isTrue = true;
                    break;
                case BillTypeEnum.FinancialIncomeBill:
                    var financialIncomeBill = _financialIncomeBillService.GetFinancialIncomeBillById(storeId,billId);
                    if (financialIncomeBill != null && financialIncomeBill.ReceivedStatus == ReceiptStatus.Received)
                        isTrue = true;
                    break;
            }
            return isTrue;
        }
        #endregion

        public BaseResult Reverse(int userId, CashReceiptBill bill)
        {
            var successful = new BaseResult { Success = true, Message = "单据红冲成功" };
            var failed = new BaseResult { Success = false, Message = "单据红冲失败" };

            var uow = CashReceiptBillsRepository.UnitOfWork;
            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();

                #region 红冲记账凭证

                _recordingVoucherService.CancleVoucher<CashReceiptBill, CashReceiptItem>(bill, () =>
                {
                    #region 修改单据收款状态

                    if (bill != null && bill.Items != null && bill.Items.Count > 0)
                    {

                        bill.Items.ToList().ForEach(a =>
                        {
                            //部分收款
                            if (a.PaymentedAmount != decimal.Zero) //可能为负数
                            {
                                //更新开单状态
                                switch (a.BillTypeEnum)
                                {
                                    case BillTypeEnum.SaleBill:
                                        _saleBillService.UpdateReceived(bill.StoreId, a.BillId, ReceiptStatus.Part);
                                        break;
                                    case BillTypeEnum.ReturnBill:
                                        _returnBillService.UpdateReceived(bill.StoreId, a.BillId, ReceiptStatus.Part);
                                        break;
                                    case BillTypeEnum.AdvanceReceiptBill:
                                        _advanceReceiptBillService.UpdateReceived(bill.StoreId, a.BillId, ReceiptStatus.Part);
                                        break;
                                    case BillTypeEnum.CostExpenditureBill:
                                        _costExpenditureBillService.UpdateReceived(bill.StoreId, a.BillId, ReceiptStatus.Part);
                                        break;
                                    case BillTypeEnum.FinancialIncomeBill:
                                        _financialIncomeBillService.UpdateReceived(bill.StoreId, a.BillId, ReceiptStatus.Part);
                                        break;
                                }
                            }
                            //未收款
                            else if (a.PaymentedAmount == decimal.Zero)
                            {
                                //更新开单状态
                                switch (a.BillTypeEnum)
                                {
                                    case BillTypeEnum.SaleBill:
                                        _saleBillService.UpdateReceived(bill.StoreId, a.BillId, ReceiptStatus.None);
                                        break;
                                    case BillTypeEnum.ReturnBill:
                                        _returnBillService.UpdateReceived(bill.StoreId, a.BillId, ReceiptStatus.None);
                                        break;
                                    case BillTypeEnum.AdvanceReceiptBill:
                                        _advanceReceiptBillService.UpdateReceived(bill.StoreId, a.BillId, ReceiptStatus.None);
                                        break;
                                    case BillTypeEnum.CostExpenditureBill:
                                        _costExpenditureBillService.UpdateReceived(bill.StoreId, a.BillId, ReceiptStatus.None);
                                        break;
                                    case BillTypeEnum.FinancialIncomeBill:
                                        _financialIncomeBillService.UpdateReceived(bill.StoreId, a.BillId, ReceiptStatus.None);
                                        break;
                                }
                            }
                        });
                    }

                    #endregion

                    #region 修改单据表状态
                    bill.ReversedUserId = userId;
                    bill.ReversedDate = DateTime.Now;
                    bill.ReversedStatus = true;
                    //UpdateCashReceiptBill(bill);
                    #endregion

                    bill.VoucherId = 0;
                    UpdateCashReceiptBill(bill);
                });

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


        /// <summary>
        /// 验证终端是否已经结完欠款
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="terminalId"></param>
        /// <param name="oweCaseAmount">金额</param>
        /// <returns></returns>
        public bool CheckTerminalCashReceiptSettled(int storeId, int? terminalId, decimal oweCaseAmount)
        {

            var queryString = @"(SELECT 0 as Id,sb.StoreId,
                                sb.Id AS BillId,
                                sb.BillNumber AS BillNumber,
                                12 AS BillTypeId,'销售单' as BillTypeName,
                                sb.TerminalId AS CustomerId,
                                t.Code AS CustomerPointCode,
                                sb.BusinessUserId,
                                sb.CreatedOnUtc AS MakeBillDate,
                                sb.ReceivableAmount AS Amount,
                                sb.PreferentialAmount AS DiscountAmount,
                                0 AS PaymentedAmount,
                                sb.OweCash AS ArrearsAmount,
                                sb.Remark AS Remark
                            FROM
                                dcms.SaleBills AS sb
                                inner join dcms_crm.CRM_Terminals AS t on sb.TerminalId=t.Id
                            WHERE
                                sb.StoreId = " + storeId + "  AND  sb.auditedStatus = 1 AND sb.ReversedStatus = 0 AND abs(sb.OweCash) > 0 AND (sb.ReceiptStatus = 0 or sb.ReceiptStatus = 1)";

            if (terminalId.HasValue && terminalId.Value > 0)
            {
                queryString += @" AND sb.TerminalId = " + terminalId + "";
            }


            queryString += @" ) UNION ALL (SELECT  0 as Id,sb.StoreId,
                                sb.Id AS BillId,
                                sb.BillNumber AS BillNumber,
                                14 AS BillTypeId,'退货单' as BillTypeName,
                                sb.TerminalId AS CustomerId,
                                t.Code AS CustomerPointCode,
                                sb.BusinessUserId,
                                sb.CreatedOnUtc AS MakeBillDate,
                                sb.ReceivableAmount AS Amount,
                                sb.PreferentialAmount AS DiscountAmount,
                                0 AS PaymentedAmount,
                                sb.OweCash AS ArrearsAmount,
                                sb.Remark AS Remark
                            FROM
                                dcms.ReturnBills AS sb
                                inner join dcms_crm.CRM_Terminals AS t on sb.TerminalId=t.Id
                            WHERE
                                sb.StoreId = " + storeId + "  AND   sb.auditedStatus = 1 AND sb.ReversedStatus = 0 AND abs(sb.OweCash) > 0 AND (sb.ReceiptStatus = 0 or sb.ReceiptStatus = 1)";

            if (terminalId.HasValue && terminalId.Value > 0)
            {
                queryString += @" AND sb.TerminalId = " + terminalId + "";
            }



            queryString += @") UNION ALL (SELECT  0 as Id,sb.StoreId,
                                sb.Id AS BillId,
                                sb.BillNumber AS BillNumber,
                                43 AS BillTypeId,'预收款单' as BillTypeName,
                                sb.CustomerId AS CustomerId,
                                t.Code AS CustomerPointCode,
                                sb.Payeer AS BusinessUserId,
                                sb.CreatedOnUtc AS MakeBillDate,
                                sb.AdvanceAmount AS Amount,
                                sb.DiscountAmount AS DiscountAmount,
                                0 AS PaymentedAmount,
                                sb.OweCash AS ArrearsAmount,
                                sb.Remark AS Remark
                            FROM
                                dcms.AdvanceReceiptBills AS sb
                                inner join dcms_crm.CRM_Terminals AS t on sb.CustomerId=t.Id
                            WHERE
                                 sb.StoreId = " + storeId + "  AND   sb.auditedStatus = 1 AND sb.ReversedStatus = 0 AND abs(sb.OweCash) > 0 AND (sb.ReceiptStatus = 0 or sb.ReceiptStatus = 1)";

            if (terminalId.HasValue && terminalId.Value > 0)
            {
                queryString += @" AND sb.CustomerId = " + terminalId + "";
            }



            queryString += @" ) UNION ALL (SELECT 0 as Id,sb.StoreId,
                                sb.Id AS BillId,
                                sb.BillNumber AS BillNumber,
                                45 AS BillTypeId,'费用支出' as BillTypeName,
                                sb.TerminalId AS CustomerId,
                                t.Code AS CustomerPointCode,
                                sb.EmployeeId AS BusinessUserId,
                                sb.CreatedOnUtc AS MakeBillDate,
                                sb.SumAmount AS Amount,
                                sb.DiscountAmount AS DiscountAmount,
                                0 AS PaymentedAmount,
                                sb.OweCash AS ArrearsAmount,
                                sb.Remark AS Remark
                            FROM
                                dcms.CostExpenditureBills AS sb
                                inner join dcms.CostExpenditureItems cs on sb.Id=cs.CostExpenditureBillId
                                inner join dcms_crm.CRM_Terminals AS t on cs.CustomerId=t.Id
                            WHERE
                                 sb.StoreId = " + storeId + "  AND  sb.auditedStatus = 1 AND sb.ReversedStatus = 0 AND abs(sb.OweCash) > 0 AND (sb.ReceiptStatus = 0 or sb.ReceiptStatus = 1)";

            if (terminalId.HasValue && terminalId.Value > 0)
            {
                queryString += @" AND cs.CustomerId = " + terminalId + "";
            }



            queryString += @" ) UNION ALL (SELECT  0 as Id,sb.StoreId,
                                sb.Id AS BillId,
                                sb.BillNumber AS BillNumber,
                                47 AS BillTypeId,'其它收入' as BillTypeName,
                                sb.TerminalId AS CustomerId,
                                t.Code AS CustomerPointCode,
                                sb.SalesmanId AS BusinessUserId,
                                sb.CreatedOnUtc AS MakeBillDate,
                                sb.SumAmount AS Amount,
                                sb.DiscountAmount AS DiscountAmount,
                                0 AS PaymentedAmount,
                                sb.OweCash AS ArrearsAmount,
                                sb.Remark AS Remark
                            FROM
                                dcms.FinancialIncomeBills AS sb
                                inner join dcms_crm.CRM_Terminals AS t on sb.TerminalId=t.Id
                            WHERE
                                 sb.StoreId = " + storeId + "  AND  sb.auditedStatus = 1 AND sb.ReversedStatus = 0 AND abs(sb.OweCash) > 0 AND (sb.ReceiptStatus = 0 or sb.ReceiptStatus = 1)";

            if (terminalId.HasValue && terminalId.Value > 0)
            {
                queryString += @" AND sb.TerminalId = " + terminalId + "";
            }

            queryString += @" )";

            var bills = CashReceiptBillsRepository.QueryFromSql<BillCashReceiptSummary>(queryString).ToList();
            #region 计算欠款金额逻辑
            //重写计算： 优惠金额	 已收金额  尚欠金额
            foreach (var bill in bills)
            {
                //销售单
                if (bill.BillTypeId == (int)BillTypeEnum.SaleBill)
                {
                    //单据金额
                    decimal calc_billAmount = bill.Amount ?? 0;
                    if (bill.Remark != null && bill.Remark.IndexOf("应收款销售单") != -1)
                    {
                        calc_billAmount = bill.ArrearsAmount ?? 0;
                    }

                    //优惠金额 
                    decimal calc_discountAmount = 0;
                    //已收金额
                    decimal calc_paymentedAmount = 0;
                    //尚欠金额
                    decimal calc_arrearsAmount = 0;

                    #region 计算如下

                    //单已经收款部分的本次优惠合计
                    var discountAmountOnce = _commonBillService.GetBillDiscountAmountOnce(storeId, bill.BillId);

                    //优惠金额 =  单据优惠金额  + （单已经收款部分的本次优惠合计）
                    calc_discountAmount = Convert.ToDecimal(Convert.ToDouble(bill.DiscountAmount ?? 0) + Convert.ToDouble(discountAmountOnce));

                    //单据收款金额（收款账户）
                    var collectionAmount = _commonBillService.GetBillCollectionAmount(storeId, bill.BillId, BillTypeEnum.SaleBill);

                    //单已经收款部分的本次收款合计
                    var receivableAmountOnce = _commonBillService.GetBillReceivableAmountOnce(storeId, bill.BillId);

                    //已收金额 = 单据收款金额（收款账户） + （单已经收款部分的本次收款合计）
                    calc_paymentedAmount = collectionAmount + receivableAmountOnce;

                    //尚欠金额
                    //Convert.ToDouble(bill.ArrearsAmount ?? 0) + 
                    calc_arrearsAmount = Convert.ToDecimal(Convert.ToDouble(calc_billAmount) - Convert.ToDouble(calc_discountAmount) - Convert.ToDouble(calc_paymentedAmount));

                    #endregion

                    //重新赋值
                    bill.Amount = calc_billAmount;
                    bill.DiscountAmount = calc_discountAmount;
                    bill.PaymentedAmount = calc_paymentedAmount;
                    bill.ArrearsAmount = calc_arrearsAmount;

                }
                //退货单
                else if (bill.BillTypeId == (int)BillTypeEnum.ReturnBill)
                {
                    //单据金额
                    decimal calc_billAmount = bill.Amount ?? 0;
                    //优惠金额 
                    decimal calc_discountAmount = 0;
                    //已收金额
                    decimal calc_paymentedAmount = 0;
                    //尚欠金额
                    decimal calc_arrearsAmount = 0;

                    #region 计算如下

                    //单已经收款部分的本次优惠合计
                    var discountAmountOnce = _commonBillService.GetBillDiscountAmountOnce(storeId, bill.BillId);

                    //优惠金额 =  单据优惠金额  + （单已经收款部分的本次优惠合计）
                    calc_discountAmount = bill.DiscountAmount ?? 0 + discountAmountOnce;

                    //单据收款金额（收款账户）
                    var collectionAmount = _commonBillService.GetBillCollectionAmount(storeId, bill.BillId, BillTypeEnum.ReturnBill);

                    //单已经收款部分的本次收款合计
                    var receivableAmountOnce = _commonBillService.GetBillReceivableAmountOnce(storeId, bill.BillId);

                    //已收金额 = 单据收款金额（收款账户） + （单已经收款部分的本次收款合计）
                    calc_paymentedAmount = collectionAmount + receivableAmountOnce;

                    //尚欠金额
                    calc_arrearsAmount = Convert.ToDecimal(Convert.ToDouble(calc_billAmount) - Convert.ToDouble(calc_discountAmount) - Math.Abs(Convert.ToDouble(calc_paymentedAmount)));

                    #endregion

                    //重新赋值
                    bill.Amount = -calc_billAmount;
                    bill.DiscountAmount = -calc_discountAmount;
                    bill.PaymentedAmount = -calc_paymentedAmount;
                    bill.ArrearsAmount = -calc_arrearsAmount;

                }
                //预收款单
                else if (bill.BillTypeId == (int)BillTypeEnum.AdvanceReceiptBill)
                {

                    //单据金额
                    decimal calc_billAmount = bill.Amount ?? 0;
                    //优惠金额 
                    decimal calc_discountAmount = 0;
                    //已收金额
                    decimal calc_paymentedAmount = 0;
                    //尚欠金额
                    decimal calc_arrearsAmount = 0;

                    #region 计算如下

                    //单已经收款部分的本次优惠合计
                    var discountAmountOnce = _commonBillService.GetBillDiscountAmountOnce(storeId, bill.BillId);

                    //优惠金额 =  单据优惠金额  + （已经收款部分的本次优惠合计）
                    calc_discountAmount = bill.DiscountAmount ?? 0 + discountAmountOnce;

                    //单据收款金额（收款账户）
                    var collectionAmount = _commonBillService.GetBillCollectionAmount(storeId, bill.BillId, BillTypeEnum.AdvanceReceiptBill);

                    //单已经收款部分的本次收款合计
                    var receivableAmountOnce = _commonBillService.GetBillReceivableAmountOnce(storeId, bill.BillId);

                    //已收金额 = 单据收款金额（收款账户） + （已经收款部分的本次收款合计）
                    calc_paymentedAmount = collectionAmount + receivableAmountOnce;

                    calc_arrearsAmount = Convert.ToDecimal(Convert.ToDouble(calc_billAmount) - Convert.ToDouble(calc_discountAmount) - Convert.ToDouble(calc_paymentedAmount));

                    #endregion

                    //重新赋值
                    bill.Amount = calc_billAmount;
                    bill.DiscountAmount = calc_discountAmount;
                    bill.PaymentedAmount = calc_paymentedAmount;
                    bill.ArrearsAmount = calc_arrearsAmount;
                }
                //费用支出
                else if (bill.BillTypeId == (int)BillTypeEnum.CostExpenditureBill)
                {
                    //单据金额
                    decimal calc_billAmount = bill.Amount ?? 0;
                    //优惠金额 
                    decimal calc_discountAmount = 0;
                    //已收金额
                    decimal calc_paymentedAmount = 0;
                    //尚欠金额
                    decimal calc_arrearsAmount = 0;

                    #region 计算如下

                    //单已经收款部分的本次优惠合计
                    var discountAmountOnce = _commonBillService.GetBillDiscountAmountOnce(storeId, bill.BillId);

                    //优惠金额 =  单据优惠金额  + （单已经收款部分的本次优惠合计）
                    calc_discountAmount = bill.DiscountAmount ?? 0 + discountAmountOnce;

                    //单据收款金额（收款账户）
                    var collectionAmount = _commonBillService.GetBillCollectionAmount(storeId, bill.BillId, BillTypeEnum.CostExpenditureBill);

                    //单已经收款部分的本次收款合计
                    var receivableAmountOnce = _commonBillService.GetBillReceivableAmountOnce(storeId, bill.BillId);

                    //已收金额 = 单据收款金额（收款账户） + （单已经收款部分的本次收款合计）
                    calc_paymentedAmount = collectionAmount + receivableAmountOnce;

                    //尚欠金额 
                    calc_arrearsAmount = Convert.ToDecimal(Convert.ToDouble(calc_billAmount) - Convert.ToDouble(calc_discountAmount) - Math.Abs(Convert.ToDouble(calc_paymentedAmount)));

                    #endregion

                    //重新赋值
                    bill.Amount = -Math.Abs(calc_billAmount);
                    bill.DiscountAmount = -Math.Abs(calc_discountAmount);
                    bill.PaymentedAmount = -Math.Abs(calc_paymentedAmount);
                    bill.ArrearsAmount = -Math.Abs(calc_arrearsAmount);
                }
                //其它收入
                else if (bill.BillTypeId == (int)BillTypeEnum.FinancialIncomeBill)
                {
                    //单据金额
                    decimal calc_billAmount = bill.Amount ?? 0;
                    //优惠金额 
                    decimal calc_discountAmount = 0;
                    //已收金额
                    decimal calc_paymentedAmount = 0;
                    //尚欠金额
                    decimal calc_arrearsAmount = 0;

                    #region 计算如下

                    //单已经收款部分的本次优惠合计
                    var discountAmountOnce = _commonBillService.GetBillDiscountAmountOnce(storeId, bill.BillId);

                    //优惠金额 =  单据优惠金额  + （单已经收款部分的本次优惠合计）
                    calc_discountAmount = bill.DiscountAmount ?? 0 + discountAmountOnce;

                    //单据收款金额（收款账户）
                    var collectionAmount = _commonBillService.GetBillCollectionAmount(storeId, bill.BillId, BillTypeEnum.FinancialIncomeBill);

                    //单已经收款部分的本次收款合计
                    var receivableAmountOnce = _commonBillService.GetBillReceivableAmountOnce(storeId, bill.BillId);

                    //已收金额 = 单据收款金额（收款账户） + （单已经收款部分的本次收款合计）
                    calc_paymentedAmount = collectionAmount + receivableAmountOnce;

                    //尚欠金额 
                    calc_arrearsAmount = Convert.ToDecimal(Convert.ToDouble(calc_billAmount) - Convert.ToDouble(calc_discountAmount) - Convert.ToDouble(calc_paymentedAmount));

                    #endregion

                    //重新赋值
                    bill.Amount = calc_billAmount;
                    bill.DiscountAmount = calc_discountAmount;
                    bill.PaymentedAmount = calc_paymentedAmount;
                    bill.ArrearsAmount = calc_arrearsAmount;

                }
            }

            var totalArrearsAmount = bills.Sum(s => s.ArrearsAmount);

            if (oweCaseAmount <= totalArrearsAmount)
            {
                return true;
            }
            #endregion
            return false;
        }


        /// <summary>
        /// 验证单据是否已经收款
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="billTypeId"></param>
        /// <param name="billNumber"></param>
        /// <returns></returns>
        public Tuple<bool, string> CheckBillCashReceipt(int storeId, int billTypeId, string billNumber)
        {

            var query = from a in CashReceiptBillsRepository.Table
                        join b in CashReceiptItemsRepository.Table on a.Id equals b.CashReceiptBillId
                        where a.StoreId == storeId
                        && a.AuditedStatus == true
                        && a.ReversedStatus == false
                        && b.BillTypeId == billTypeId
                        && b.BillNumber == billNumber
                        select b;
            var lists = query.ToList();
            bool fg = false;
            string billNumbers = string.Empty;
            if (lists != null && lists.Count > 0)
            {
                fg = true;
                billNumbers = string.Join(",", lists);
            }
            return new Tuple<bool, string>(fg, billNumbers);

        }


        /// <summary>
        /// 获取待收款欠款单据
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="payeer"></param>
        /// <param name="terminalId"></param>
        /// <param name="billTypeId"></param>
        /// <param name="billNumber"></param>
        /// <param name="remark"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public IList<BillCashReceiptSummary> GetBillCashReceiptSummary(int storeId, int? payeer,
            int? terminalId,
            int? billTypeId,
            string billNumber = "",
            string remark = "",
            DateTime? startTime = null,
            DateTime? endTime = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            billNumber = CommonHelper.FilterSQLChar(billNumber);
            remark = CommonHelper.FilterSQLChar(remark);

            // sb.ReceivableAmount AS Amount,
            var queryString = @"(SELECT 0 as Id,sb.StoreId,
                                sb.Id AS BillId,
                                sb.BillNumber AS BillNumber,
                                12 AS BillTypeId,'销售单' as BillTypeName,
                                sb.TerminalId AS CustomerId,
                                t.Code AS CustomerPointCode,
                                sb.BusinessUserId,
                                sb.CreatedOnUtc AS MakeBillDate,
                                sb.SumAmount AS Amount,
                                sb.PreferentialAmount AS DiscountAmount,
                                0 AS PaymentedAmount,
                                sb.OweCash AS ArrearsAmount,
                                sb.Remark AS Remark
                            FROM
                                dcms.SaleBills AS sb
                                inner join dcms_crm.CRM_Terminals AS t on sb.TerminalId=t.Id
                            WHERE
                                sb.StoreId = " + storeId + "  AND  sb.auditedStatus = 1 AND sb.ReversedStatus = 0 AND abs(sb.OweCash) > 0 AND (sb.ReceiptStatus = 0 or sb.ReceiptStatus = 1)";

            if (terminalId.HasValue && terminalId.Value > 0)
            {
                queryString += @" AND sb.TerminalId = " + terminalId + "";
            }

            if (payeer.HasValue && payeer.Value > 0)
            {
                queryString += @" AND sb.BusinessUserId = " + payeer + "";
            }

            if (startTime.HasValue)
            {
                queryString += @" AND sb.CreatedOnUtc >= '" + startTime.Value.ToString("yyyy-MM-dd 00:00:00") + "'";
            }

            if (endTime.HasValue)
            {
                queryString += @" AND sb.CreatedOnUtc <= '" + endTime.Value.ToString("yyyy-MM-dd 23:59:59") + "'";
            }

            queryString += @" ) UNION ALL (SELECT  0 as Id,sb.StoreId,
                                sb.Id AS BillId,
                                sb.BillNumber AS BillNumber,
                                14 AS BillTypeId,'退货单' as BillTypeName,
                                sb.TerminalId AS CustomerId,
                                t.Code AS CustomerPointCode,
                                sb.BusinessUserId,
                                sb.CreatedOnUtc AS MakeBillDate,
                                sb.ReceivableAmount AS Amount,
                                sb.PreferentialAmount AS DiscountAmount,
                                0 AS PaymentedAmount,
                                sb.OweCash AS ArrearsAmount,
                                sb.Remark AS Remark
                            FROM
                                dcms.ReturnBills AS sb
                                inner join dcms_crm.CRM_Terminals AS t on sb.TerminalId=t.Id
                            WHERE
                                sb.StoreId = " + storeId + "  AND   sb.auditedStatus = 1 AND sb.ReversedStatus = 0 AND abs(sb.OweCash) > 0 AND (sb.ReceiptStatus = 0 or sb.ReceiptStatus = 1)";

            if (terminalId.HasValue && terminalId.Value > 0)
            {
                queryString += @" AND sb.TerminalId = " + terminalId + "";
            }

            if (payeer.HasValue && payeer.Value > 0)
            {
                queryString += @" AND sb.BusinessUserId = " + payeer + "";
            }

            if (startTime.HasValue)
            {
                queryString += @" AND sb.CreatedOnUtc >= '" + startTime.Value.ToString("yyyy-MM-dd 00:00:00") + "'";
            }

            if (endTime.HasValue)
            {
                queryString += @" AND sb.CreatedOnUtc <= '" + endTime.Value.ToString("yyyy-MM-dd 23:59:59") + "'";
            }

            queryString += @") UNION ALL (SELECT  0 as Id,sb.StoreId,
                                sb.Id AS BillId,
                                sb.BillNumber AS BillNumber,
                                43 AS BillTypeId,'预收款单' as BillTypeName,
                                sb.CustomerId AS CustomerId,
                                t.Code AS CustomerPointCode,
                                sb.Payeer AS BusinessUserId,
                                sb.CreatedOnUtc AS MakeBillDate,
                                sb.AdvanceAmount AS Amount,
                                sb.DiscountAmount AS DiscountAmount,
                                0 AS PaymentedAmount,
                                sb.OweCash AS ArrearsAmount,
                                sb.Remark AS Remark
                            FROM
                                dcms.AdvanceReceiptBills AS sb
                                inner join dcms_crm.CRM_Terminals AS t on sb.CustomerId=t.Id
                            WHERE
                                 sb.StoreId = " + storeId + "  AND   sb.auditedStatus = 1 AND sb.ReversedStatus = 0 AND abs(sb.OweCash) > 0 AND (sb.ReceiptStatus = 0 or sb.ReceiptStatus = 1)";

            if (terminalId.HasValue && terminalId.Value > 0)
            {
                queryString += @" AND sb.CustomerId = " + terminalId + "";
            }

            if (payeer.HasValue && payeer.Value > 0)
            {
                queryString += @" AND sb.Payeer = " + payeer + "";
            }

            if (startTime.HasValue)
            {
                queryString += @" AND sb.CreatedOnUtc >= '" + startTime.Value.ToString("yyyy-MM-dd 00:00:00") + "'";
            }

            if (endTime.HasValue)
            {
                queryString += @" AND sb.CreatedOnUtc <= '" + endTime.Value.ToString("yyyy-MM-dd 23:59:59") + "'";
            }

            queryString += @" ) UNION ALL (SELECT 0 as Id,sb.StoreId,
                                sb.Id AS BillId,
                                sb.BillNumber AS BillNumber,
                                45 AS BillTypeId,'费用支出' as BillTypeName,
                                sb.TerminalId AS CustomerId,
                                t.Code AS CustomerPointCode,
                                sb.EmployeeId AS BusinessUserId,
                                sb.CreatedOnUtc AS MakeBillDate,
                                sb.SumAmount AS Amount,
                                sb.DiscountAmount AS DiscountAmount,
                                0 AS PaymentedAmount,
                                sb.OweCash AS ArrearsAmount,
                                sb.Remark AS Remark
                            FROM
                                dcms.CostExpenditureBills AS sb
                                inner join dcms.CostExpenditureItems cs on sb.Id=cs.CostExpenditureBillId
                                inner join dcms_crm.CRM_Terminals AS t on cs.CustomerId = t.Id
                            WHERE
                                 sb.StoreId = " + storeId + "  AND  sb.auditedStatus = 1 AND sb.ReversedStatus = 0 AND abs(sb.OweCash) > 0 AND (sb.ReceiptStatus = 0 or sb.ReceiptStatus = 1)";

            if (terminalId.HasValue && terminalId.Value > 0)
            {
                queryString += @" AND cs.CustomerId = " + terminalId + "";
            }

            if (payeer.HasValue && payeer.Value > 0)
            {
                queryString += @" AND sb.EmployeeId = " + payeer + "";
            }

            if (startTime.HasValue)
            {
                queryString += @" AND sb.CreatedOnUtc >= '" + startTime.Value.ToString("yyyy-MM-dd 00:00:00") + "'";
            }

            if (endTime.HasValue)
            {
                queryString += @" AND sb.CreatedOnUtc <= '" + endTime.Value.ToString("yyyy-MM-dd 23:59:59") + "'";
            }

            queryString += @" ) UNION ALL (SELECT  0 as Id,sb.StoreId,
                                sb.Id AS BillId,
                                sb.BillNumber AS BillNumber,
                                47 AS BillTypeId,'其它收入' as BillTypeName,
                                sb.TerminalId AS CustomerId,
                                t.Code AS CustomerPointCode,
                                sb.SalesmanId AS BusinessUserId,
                                sb.CreatedOnUtc AS MakeBillDate,
                                sb.SumAmount AS Amount,
                                sb.DiscountAmount AS DiscountAmount,
                                0 AS PaymentedAmount,
                                sb.OweCash AS ArrearsAmount,
                                sb.Remark AS Remark
                            FROM
                                dcms.FinancialIncomeBills AS sb
                                inner join dcms_crm.CRM_Terminals AS t on sb.TerminalId=t.Id
                            WHERE
                                 sb.StoreId = " + storeId + "  AND  sb.auditedStatus = 1 AND sb.ReversedStatus = 0 AND abs(sb.OweCash) > 0 AND (sb.ReceiptStatus = 0 or sb.ReceiptStatus = 1)";

            if (terminalId.HasValue && terminalId.Value > 0)
            {
                queryString += @" AND sb.TerminalId = " + terminalId + "";
            }

            if (payeer.HasValue && payeer.Value > 0)
            {
                queryString += @" AND sb.SalesmanId = " + payeer + "";
            }

            if (startTime.HasValue)
            {
                queryString += @" AND sb.CreatedOnUtc >= '" + startTime.Value.ToString("yyyy-MM-dd 00:00:00") + "'";
            }

            if (endTime.HasValue)
            {
                queryString += @" AND sb.CreatedOnUtc <= '" + endTime.Value.ToString("yyyy-MM-dd 23:59:59") + "'";
            }

            queryString += @" )";

            //var sbCount = $"SELECT COUNT(1) as `Value` FROM ({queryString}) as alls;";
            //int totalCount = ProductsRepository.QueryFromSql<IntQueryType>(sbCount.ToString()).ToList().FirstOrDefault().Value ?? 0;

            string sbQuery = $"SELECT * FROM(SELECT ROW_NUMBER() OVER(ORDER BY BillId) AS RowNum, alls.* FROM({queryString}) as alls ) AS result  WHERE RowNum >= {pageIndex * pageSize} AND RowNum <= {(pageIndex + 1) * pageSize} ORDER BY BillId asc";

            var query = CashReceiptBillsRepository.QueryFromSql<BillCashReceiptSummary>(sbQuery).ToList();

            if (billTypeId.HasValue && billTypeId.Value > 0)
                query = query.Where(s => s.BillTypeId == billTypeId).ToList();

            if (!string.IsNullOrEmpty(billNumber))
                query = query.Where(s => s.BillNumber == billNumber).ToList();

            return query;
        }


        /// <summary>
        /// 判断指定单据是否尚有欠款(是否已经收完款)
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="billTypeId"></param>
        /// <param name="billId"></param>
        /// <returns></returns>
        public bool ThereAnyDebt(int storeId, int? billTypeId, int billId)
        {

            //销售单
            if (billTypeId == (int)BillTypeEnum.SaleBill)
            {
                return SaleBillsRepository.Table
                    .Where(s => s.StoreId == storeId && s.Id == billId && (s.ReceiptStatus == 0 || s.ReceiptStatus == 1))
                    .Count() > 0;
            }
            //退货单
            else if (billTypeId == (int)BillTypeEnum.ReturnBill)
            {
                return ReturnBillsRepository.Table
                    .Where(s => s.StoreId == storeId && s.Id == billId && (s.ReceiptStatus == 0 || s.ReceiptStatus == 1))
                    .Count() > 0;
            }
            //预收款单
            else if (billTypeId == (int)BillTypeEnum.AdvanceReceiptBill)
            {
                return AdvanceReceiptBillsRepository.Table.
                    Where(s => s.StoreId == storeId && s.Id == billId && (s.ReceiptStatus == 0 || s.ReceiptStatus == 1))
                    .Count() > 0;
            }
            //费用支出
            else if (billTypeId == (int)BillTypeEnum.CostExpenditureBill)
            {
                return CostExpenditureBillsRepository.Table.
                    Where(s => s.StoreId == storeId && s.Id == billId && (s.ReceiptStatus == 0 || s.ReceiptStatus == 1))
                    .Count() > 0;
            }
            //其它收入
            else if (billTypeId == (int)BillTypeEnum.FinancialIncomeBill)
            {
                return FinancialIncomeBillsRepository.Table.
                    Where(s => s.StoreId == storeId && s.Id == billId && (s.ReceiptStatus == 0 || s.ReceiptStatus == 1))
                    .Count() > 0;
            }
            else
            {
                return true;
            }
        }


        /// <summary>
        /// 更新单据交账状态
        /// </summary>
        /// <param name="store"></param>
        /// <param name="billId"></param>
        /// <param name="handInStatus"></param>
        public void UpdateHandInStatus(int? store, int billId, bool handInStatus)
        {
            var bill = GetCashReceiptBillById(store, billId, false);
            if (bill != null)
            {
                bill.HandInStatus = handInStatus;
                var uow = CashReceiptBillsRepository.UnitOfWork;
                CashReceiptBillsRepository.Update(bill);
                uow.SaveChanges();
                //通知
                _eventPublisher.EntityUpdated(bill);
            }
        }

        /// <summary>
        /// 获取待收款欠款单据
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="payeer"></param>
        /// <param name="terminalId"></param>
        /// <param name="billTypeId"></param>
        /// <param name="billNumber"></param>
        /// <param name="remark"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public IList<BillCashReceiptSummary> GetBillCashReceiptList(int storeId, IList<int> userIds,
            int? terminalId,
            int? billTypeId,
            string billNumber = "",
            string remark = "",
            DateTime? startTime = null,
            DateTime? endTime = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            billNumber = CommonHelper.FilterSQLChar(billNumber);
            remark = CommonHelper.FilterSQLChar(remark);

            // sb.ReceivableAmount AS Amount,
            var queryString = @"(SELECT 0 as Id,sb.StoreId,
                                sb.Id AS BillId,
                                sb.BillNumber AS BillNumber,
                                12 AS BillTypeId,'销售单' as BillTypeName,
                                sb.TerminalId AS CustomerId,
                                t.Code AS CustomerPointCode,
                                sb.BusinessUserId,
                                sb.CreatedOnUtc AS MakeBillDate,
                                sb.SumAmount AS Amount,
                                sb.PreferentialAmount AS DiscountAmount,
                                0 AS PaymentedAmount,
                                sb.OweCash AS ArrearsAmount,
                                sb.Remark AS Remark
                            FROM
                                dcms.SaleBills AS sb
                                inner join dcms_crm.CRM_Terminals AS t on sb.TerminalId=t.Id
                            WHERE
                                sb.StoreId = " + storeId + "  AND  sb.auditedStatus = 1 AND sb.ReversedStatus = 0 AND abs(sb.OweCash) > 0 AND (sb.ReceiptStatus = 0 or sb.ReceiptStatus = 1)";

            if (terminalId.HasValue && terminalId.Value > 0)
            {
                queryString += @" AND sb.TerminalId = " + terminalId + "";
            }

            if (userIds != null && userIds.Count>0)
            {
                queryString += @" AND sb.BusinessUserId in("+ string.Join(",", userIds) +")";
            }

            if (startTime.HasValue)
            {
                queryString += @" AND sb.CreatedOnUtc >= '" + startTime.Value.ToString("yyyy-MM-dd 00:00:00") + "'";
            }

            if (endTime.HasValue)
            {
                queryString += @" AND sb.CreatedOnUtc <= '" + endTime.Value.ToString("yyyy-MM-dd 23:59:59") + "'";
            }

            queryString += @" ) UNION ALL (SELECT  0 as Id,sb.StoreId,
                                sb.Id AS BillId,
                                sb.BillNumber AS BillNumber,
                                14 AS BillTypeId,'退货单' as BillTypeName,
                                sb.TerminalId AS CustomerId,
                                t.Code AS CustomerPointCode,
                                sb.BusinessUserId,
                                sb.CreatedOnUtc AS MakeBillDate,
                                sb.ReceivableAmount AS Amount,
                                sb.PreferentialAmount AS DiscountAmount,
                                0 AS PaymentedAmount,
                                sb.OweCash AS ArrearsAmount,
                                sb.Remark AS Remark
                            FROM
                                dcms.ReturnBills AS sb
                                inner join dcms_crm.CRM_Terminals AS t on sb.TerminalId=t.Id
                            WHERE
                                sb.StoreId = " + storeId + "  AND   sb.auditedStatus = 1 AND sb.ReversedStatus = 0 AND abs(sb.OweCash) > 0 AND (sb.ReceiptStatus = 0 or sb.ReceiptStatus = 1)";

            if (terminalId.HasValue && terminalId.Value > 0)
            {
                queryString += @" AND sb.TerminalId = " + terminalId + "";
            }

            if (userIds != null && userIds.Count > 0)
            {
                queryString += @" AND sb.BusinessUserId in("+ string.Join(",",userIds) +")";
            }

            if (startTime.HasValue)
            {
                queryString += @" AND sb.CreatedOnUtc >= '" + startTime.Value.ToString("yyyy-MM-dd 00:00:00") + "'";
            }

            if (endTime.HasValue)
            {
                queryString += @" AND sb.CreatedOnUtc <= '" + endTime.Value.ToString("yyyy-MM-dd 23:59:59") + "'";
            }

            queryString += @") UNION ALL (SELECT  0 as Id,sb.StoreId,
                                sb.Id AS BillId,
                                sb.BillNumber AS BillNumber,
                                43 AS BillTypeId,'预收款单' as BillTypeName,
                                sb.CustomerId AS CustomerId,
                                t.Code AS CustomerPointCode,
                                sb.Payeer AS BusinessUserId,
                                sb.CreatedOnUtc AS MakeBillDate,
                                sb.AdvanceAmount AS Amount,
                                sb.DiscountAmount AS DiscountAmount,
                                0 AS PaymentedAmount,
                                sb.OweCash AS ArrearsAmount,
                                sb.Remark AS Remark
                            FROM
                                dcms.AdvanceReceiptBills AS sb
                                inner join dcms_crm.CRM_Terminals AS t on sb.CustomerId=t.Id
                            WHERE
                                 sb.StoreId = " + storeId + "  AND   sb.auditedStatus = 1 AND sb.ReversedStatus = 0 AND abs(sb.OweCash) > 0 AND (sb.ReceiptStatus = 0 or sb.ReceiptStatus = 1)";

            if (terminalId.HasValue && terminalId.Value > 0)
            {
                queryString += @" AND sb.CustomerId = " + terminalId + "";
            }

            if (userIds != null && userIds.Count > 0)
            {
                queryString += @" AND sb.Payeer in("+ string.Join(",",userIds) +")";
            }

            if (startTime.HasValue)
            {
                queryString += @" AND sb.CreatedOnUtc >= '" + startTime.Value.ToString("yyyy-MM-dd 00:00:00") + "'";
            }

            if (endTime.HasValue)
            {
                queryString += @" AND sb.CreatedOnUtc <= '" + endTime.Value.ToString("yyyy-MM-dd 23:59:59") + "'";
            }

            queryString += @" ) UNION ALL (SELECT 0 as Id,sb.StoreId,
                                sb.Id AS BillId,
                                sb.BillNumber AS BillNumber,
                                45 AS BillTypeId,'费用支出' as BillTypeName,
                                sb.TerminalId AS CustomerId,
                                t.Code AS CustomerPointCode,
                                sb.EmployeeId AS BusinessUserId,
                                sb.CreatedOnUtc AS MakeBillDate,
                                sb.SumAmount AS Amount,
                                sb.DiscountAmount AS DiscountAmount,
                                0 AS PaymentedAmount,
                                sb.OweCash AS ArrearsAmount,
                                sb.Remark AS Remark
                            FROM
                                dcms.CostExpenditureBills AS sb
                                inner join dcms.CostExpenditureItems cs on sb.Id=cs.CostExpenditureBillId
                                inner join dcms_crm.CRM_Terminals AS t on cs.CustomerId = t.Id
                            WHERE
                                 sb.StoreId = " + storeId + "  AND  sb.auditedStatus = 1 AND sb.ReversedStatus = 0 AND abs(sb.OweCash) > 0 AND (sb.ReceiptStatus = 0 or sb.ReceiptStatus = 1)";

            if (terminalId.HasValue && terminalId.Value > 0)
            {
                queryString += @" AND cs.CustomerId = " + terminalId + "";
            }

            if (userIds != null && userIds.Count > 0)
            {
                queryString += @" AND sb.EmployeeId in("+ string.Join(",",userIds) +")";
            }

            if (startTime.HasValue)
            {
                queryString += @" AND sb.CreatedOnUtc >= '" + startTime.Value.ToString("yyyy-MM-dd 00:00:00") + "'";
            }

            if (endTime.HasValue)
            {
                queryString += @" AND sb.CreatedOnUtc <= '" + endTime.Value.ToString("yyyy-MM-dd 23:59:59") + "'";
            }

            queryString += @" ) UNION ALL (SELECT  0 as Id,sb.StoreId,
                                sb.Id AS BillId,
                                sb.BillNumber AS BillNumber,
                                47 AS BillTypeId,'其它收入' as BillTypeName,
                                sb.TerminalId AS CustomerId,
                                t.Code AS CustomerPointCode,
                                sb.SalesmanId AS BusinessUserId,
                                sb.CreatedOnUtc AS MakeBillDate,
                                sb.SumAmount AS Amount,
                                sb.DiscountAmount AS DiscountAmount,
                                0 AS PaymentedAmount,
                                sb.OweCash AS ArrearsAmount,
                                sb.Remark AS Remark
                            FROM
                                dcms.FinancialIncomeBills AS sb
                                inner join dcms_crm.CRM_Terminals AS t on sb.TerminalId=t.Id
                            WHERE
                                 sb.StoreId = " + storeId + "  AND  sb.auditedStatus = 1 AND sb.ReversedStatus = 0 AND abs(sb.OweCash) > 0 AND (sb.ReceiptStatus = 0 or sb.ReceiptStatus = 1)";

            if (terminalId.HasValue && terminalId.Value > 0)
            {
                queryString += @" AND sb.TerminalId = " + terminalId + "";
            }

            if (userIds != null && userIds.Count > 0)
            {
                queryString += @" AND sb.SalesmanId in("+ string.Join(",",userIds) +")";
            }

            if (startTime.HasValue)
            {
                queryString += @" AND sb.CreatedOnUtc >= '" + startTime.Value.ToString("yyyy-MM-dd 00:00:00") + "'";
            }

            if (endTime.HasValue)
            {
                queryString += @" AND sb.CreatedOnUtc <= '" + endTime.Value.ToString("yyyy-MM-dd 23:59:59") + "'";
            }

            queryString += @" )";

            //var sbCount = $"SELECT COUNT(1) as `Value` FROM ({queryString}) as alls;";
            //int totalCount = ProductsRepository.QueryFromSql<IntQueryType>(sbCount.ToString()).ToList().FirstOrDefault().Value ?? 0;

            string sbQuery = $"SELECT * FROM(SELECT ROW_NUMBER() OVER(ORDER BY BillId) AS RowNum, alls.* FROM({queryString}) as alls ) AS result  WHERE RowNum >= {pageIndex * pageSize} AND RowNum <= {(pageIndex + 1) * pageSize} ORDER BY BillId asc";

            var query = CashReceiptBillsRepository.QueryFromSql<BillCashReceiptSummary>(sbQuery).ToList();

            if (billTypeId.HasValue && billTypeId.Value > 0)
                query = query.Where(s => s.BillTypeId == billTypeId).ToList();

            if (!string.IsNullOrEmpty(billNumber))
                query = query.Where(s => s.BillNumber == billNumber).ToList();

            return query;
        }

        public bool ExistsUnAuditedByBillNumber(int storeId, string billNumber,int id)
        {
            try
            {
                var query = from a in CashReceiptBillsRepository.Table
                            join b in CashReceiptItemsRepository.Table on a.Id equals b.CashReceiptBillId
                            where a.StoreId == storeId
                            && a.AuditedStatus == false
                            && b.BillNumber == billNumber
                            select a;
                //修改单据时排除自己
                if (id>0) 
                {
                    query = query.Where(w=>w.Id != id);
                }
                return query.Count() == 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<CashReceiptItem> GetCashReceiptItemListByBillId(int storeId, int billId)
        {
            try
            {
                var query = from a in CashReceiptBillsRepository.Table
                            join b in CashReceiptItemsRepository.Table on a.Id equals b.CashReceiptBillId
                            where a.StoreId == storeId
                            && a.AuditedStatus == true
                            && a.ReversedStatus == false
                            && b.BillId == billId
                            select b;
                return query.ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
