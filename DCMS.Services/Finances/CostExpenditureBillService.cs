using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Data;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Finances;
using DCMS.Core.Domain.Tasks;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Configuration;
using DCMS.Services.Events;
using DCMS.Services.Tasks;
using DCMS.Services.Users;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using DCMS.Services.Caching;

namespace DCMS.Services.Finances
{
    /// <summary>
    /// 表示费用支出单据服务
    /// </summary>
    public partial class CostExpenditureBillService : BaseService, ICostExpenditureBillService
    {

        #region 构造

        private readonly IUserService _userService;
        private readonly IQueuedMessageService _queuedMessageService;
        private readonly ISettingService _settingService;
        private readonly IRecordingVoucherService _recordingVoucherService;

        public CostExpenditureBillService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher,
            IUserService userService,
            IQueuedMessageService queuedMessageService,
            ISettingService settingService,
            IRecordingVoucherService recordingVoucherService
            ) : base(getter, cacheManager, eventPublisher)
        {
            _userService = userService;
            _queuedMessageService = queuedMessageService;
            _settingService = settingService;
            _recordingVoucherService = recordingVoucherService;
        }

        #endregion

        #region 单据
        public bool Exists(int billId)
        {
            return CostExpenditureBillsRepository.TableNoTracking.Where(a => a.Id == billId).Count() > 0;
        }
        public virtual IPagedList<CostExpenditureBill> GetAllCostExpenditureBills(int? store, int? makeuserId, int? employeeId, int? terminalId, string billNumber = "", bool? status = null, DateTime? start = null, DateTime? end = null, bool? isShowReverse = null, bool? sortByAuditedTime = null, bool? deleted = null, bool? handleStatus = null, int? sign = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;

            DateTime.TryParse(start?.ToString("yyyy-MM-dd 00:00:00"), out DateTime startDate);
            DateTime.TryParse(end?.ToString("yyyy-MM-dd 23:59:59"), out DateTime endDate);

            var query = from pc in CostExpenditureBillsRepository.Table
                          .Include(cr => cr.Items)
                          //.ThenInclude(cr => cr.CostExpenditureBill)
                          .Include(cr => cr.CostExpenditureBillAccountings)
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

            //query = from d in _costExpenditureItemRepository.Table
            //        join k in _costExpenditureItemRepository.Table on  d.CustomerId equals k.

            if (terminalId.HasValue && terminalId.Value > 0)
            {
                query = query.Where(c => c.Items.Select(d => d.CustomerId).Contains(terminalId.Value));
            }

            if (employeeId.HasValue && employeeId.Value > 0)
            {
                query = query.Where(c => c.EmployeeId == employeeId);
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

            //签收状态
            if (sign.HasValue)
            {
                query = query.Where(a => a.SignStatus == (sign ?? 0));
            }

            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<CostExpenditureBill>(plists, pageIndex, pageSize, totalCount);
        }

        public virtual IList<CostExpenditureBill> GetAllCostExpenditureBills()
        {
            var query = from c in CostExpenditureBillsRepository.Table
                        orderby c.Id
                        select c;

            var categories = query.ToList();
            return categories;
        }

        public virtual CostExpenditureBill GetCostExpenditureBillById(int? store, int costExpenditureBillId)
        {
            if (costExpenditureBillId == 0)
            {
                return null;
            }

            var key = DCMSDefaults.COSTEXPENDITUREBILLITEM_BY_ID_KEY.FillCacheKey(store ?? 0, costExpenditureBillId);
            return _cacheManager.Get(key, () =>
            {
                return CostExpenditureBillsRepository.ToCachedGetById(costExpenditureBillId);
            });
        }

        public virtual CostExpenditureBill GetCostExpenditureBillById(int? store, int costExpenditureBillId, bool isInclude = false)
        {
            if (costExpenditureBillId == 0)
            {
                return null;
            }

            if (isInclude)
            {
                var query = CostExpenditureBillsRepository.Table
                .Include(ce => ce.Items)
                //.ThenInclude(ce => ce.CostExpenditureBill)
                .Include(ce => ce.CostExpenditureBillAccountings)
                .ThenInclude(ce => ce.AccountingOption);

                return query.FirstOrDefault(c => c.Id == costExpenditureBillId);
            }
            return CostExpenditureBillsRepository.ToCachedGetById(costExpenditureBillId);
        }

        public virtual CostExpenditureBill GetCostExpenditureBillByNumber(int? store, string billNumber)
        {
            var query = CostExpenditureBillsRepository.Table;
            var bill = query.Where(a => a.StoreId == store && a.BillNumber == billNumber).FirstOrDefault();
            return bill;
        }

        public virtual void InsertCostExpenditureBill(CostExpenditureBill costExpenditureBill)
        {
            if (costExpenditureBill == null)
            {
                throw new ArgumentNullException("costExpenditureBill");
            }

            var uow = CostExpenditureBillsRepository.UnitOfWork;
            CostExpenditureBillsRepository.Insert(costExpenditureBill);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(costExpenditureBill);
        }

        public virtual void UpdateCostExpenditureBill(CostExpenditureBill costExpenditureBill)
        {
            if (costExpenditureBill == null)
            {
                throw new ArgumentNullException("costExpenditureBill");
            }

            var uow = CostExpenditureBillsRepository.UnitOfWork;
            CostExpenditureBillsRepository.Update(costExpenditureBill);
            uow.SaveChanges();
            //通知
            _eventPublisher.EntityUpdated(costExpenditureBill);
        }

        public virtual void DeleteCostExpenditureBill(CostExpenditureBill costExpenditureBill)
        {
            if (costExpenditureBill == null)
            {
                throw new ArgumentNullException("costExpenditureBill");
            }

            var uow = CostExpenditureBillsRepository.UnitOfWork;
            CostExpenditureBillsRepository.Delete(costExpenditureBill);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityDeleted(costExpenditureBill);
        }


        #endregion

        #region 单据项目

        public virtual IPagedList<CostExpenditureItem> GetCostExpenditureItemsByCostExpenditureBillId(int costExpenditureBillId, int? userId, int? storeId, int pageIndex, int pageSize)
        {
            if (pageSize >= 50)
                pageSize = 50;
            if (costExpenditureBillId == 0)
            {
                return new PagedList<CostExpenditureItem>(new List<CostExpenditureItem>(), pageIndex, pageSize);
            }

            var key = DCMSDefaults.COSTEXPENDITUREBILLITEM_ALL_KEY.FillCacheKey(storeId, costExpenditureBillId, pageIndex, pageSize, userId);

            return _cacheManager.Get(key, () =>
            {
                var query = from pc in CostExpenditureItemsRepository.Table
                            where pc.CostExpenditureBillId == costExpenditureBillId
                            orderby pc.Id
                            select pc;
                //var productCostExpenditureBills = new PagedList<CostExpenditureItem>(query.ToList(), pageIndex, pageSize);
                //return productCostExpenditureBills;
                //总页数
                var totalCount = query.Count();
                var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
                return new PagedList<CostExpenditureItem>(plists, pageIndex, pageSize, totalCount);
            });
        }

        public virtual List<CostExpenditureItem> GetCostExpenditureItemList(int costExpenditureBillId)
        {
            List<CostExpenditureItem> costExpenditureItems = null;
            var query = CostExpenditureItemsRepository_RO.Table.Include(s => s.CostExpenditureBill);
            costExpenditureItems = query.Where(a => a.CostExpenditureBillId == costExpenditureBillId).ToList();
            return costExpenditureItems;
        }

        public virtual CostExpenditureItem GetCostExpenditureItemById(int? store, int costExpenditureItemId)
        {
            if (costExpenditureItemId == 0)
            {
                return null;
            }

            var key = DCMSDefaults.COSTEXPENDITUREBILLITEM_BY_ID_KEY.FillCacheKey(store ?? 0, costExpenditureItemId);
            return _cacheManager.Get(key, () => { return CostExpenditureItemsRepository.ToCachedGetById(costExpenditureItemId); });
        }

        public IList<CostExpenditureItem> GetCostExpenditureItemByCostContractId(int? store, int? costContractId)
        {
            var query = CostExpenditureItemsRepository_RO.Table;

            query = query.Where(ce => ce.CostContractId > 0); //支出类型为合同费用兑付

            if (store.HasValue && store.Value > 0)
            {
                query = query.Where(ce => ce.StoreId == store);
            }

            if (costContractId.HasValue && costContractId.Value > 0)
            {
                query = query.Where(ce => ce.CostContractId == costContractId);
            }

            return query.ToList();
        }

        public virtual void InsertCostExpenditureItem(CostExpenditureItem costExpenditureItem)
        {
            if (costExpenditureItem == null)
            {
                throw new ArgumentNullException("costExpenditureItem");
            }

            var uow = CostExpenditureItemsRepository.UnitOfWork;
            CostExpenditureItemsRepository.Insert(costExpenditureItem);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(costExpenditureItem);
        }

        public virtual void UpdateCostExpenditureItem(CostExpenditureItem costExpenditureItem)
        {
            if (costExpenditureItem == null)
            {
                throw new ArgumentNullException("costExpenditureItem");
            }

            var uow = CostExpenditureItemsRepository.UnitOfWork;
            CostExpenditureItemsRepository.Update(costExpenditureItem);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(costExpenditureItem);
        }

        public virtual void DeleteCostExpenditureItem(CostExpenditureItem costExpenditureItem)
        {
            if (costExpenditureItem == null)
            {
                throw new ArgumentNullException("costExpenditureItem");
            }

            var uow = CostExpenditureItemsRepository.UnitOfWork;
            CostExpenditureItemsRepository.Delete(costExpenditureItem);
            uow.SaveChanges();
            //通知
            _eventPublisher.EntityDeleted(costExpenditureItem);
        }


        #endregion

        #region 收款账户映射

        public virtual IPagedList<CostExpenditureBillAccounting> GetCostExpenditureBillAccountingsByCostExpenditureBillId(int storeId, int userId, int costExpenditureBillId, int pageIndex, int pageSize)
        {
            if (pageSize >= 50)
                pageSize = 50;
            if (costExpenditureBillId == 0)
            {
                return new PagedList<CostExpenditureBillAccounting>(new List<CostExpenditureBillAccounting>(), pageIndex, pageSize);
            }

            //var key = DCMSDefaults.DCMSDefaults.COSTEXPENDITUREBILLITEM_ACCOUNTINGL_BY_BILLID_KEY.FillCacheKey( costExpenditureBillId, pageIndex, pageSize, _workContext.CurrentUser.Id, _workContext.CurrentStore.Id);
            var key = DCMSDefaults.COSTEXPENDITUREBILLITEM_ALL_KEY.FillCacheKey(storeId, costExpenditureBillId, pageIndex, pageSize, userId);
            return _cacheManager.Get(key, () =>
            {
                var query = from pc in CostExpenditureBillAccountingMappingRepository.Table
                            join p in AccountingOptionsRepository.Table on pc.AccountingOptionId equals p.Id
                            where pc.BillId == costExpenditureBillId
                            orderby pc.Id
                            select pc;


                //var saleAccountings = new PagedList<CostExpenditureBillAccounting>(query.ToList(), pageIndex, pageSize);
                //return saleAccountings;
                //总页数
                var totalCount = query.Count();
                var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
                return new PagedList<CostExpenditureBillAccounting>(plists, pageIndex, pageSize, totalCount);
            });
        }

        public virtual IList<CostExpenditureBillAccounting> GetCostExpenditureBillAccountingsByCostExpenditureBillId(int costExpenditureBillId)
        {

            var query = from pc in CostExpenditureBillAccountingMappingRepository.Table
                        join p in AccountingOptionsRepository.Table on pc.AccountingOptionId equals p.Id
                        where pc.BillId == costExpenditureBillId
                        orderby pc.Id
                        select pc;


            return query.ToList();
        }


        /// <summary>
        /// 获取当前单据的所有搜款账户(目的:在查询时不依赖延迟加载,由于获的较高查询性能)
        /// </summary>
        /// <returns></returns>
        public virtual IList<CostExpenditureBillAccounting> GetAllCostExpenditureBillAccountingsByBillIds(int[] billIds)
        {
            if (billIds == null || billIds.Length == 0)
            {
                return new List<CostExpenditureBillAccounting>();
            }

            var query = from pc in CostExpenditureBillAccountingMappingRepository.Table
                        where billIds.Contains(pc.BillId)
                        select pc;
            return query.ToList();
        }

        public virtual CostExpenditureBillAccounting GetCostExpenditureBillAccountingById(int costExpenditureBillAccountingId)
        {
            if (costExpenditureBillAccountingId == 0)
            {
                return null;
            }

            return CostExpenditureBillAccountingMappingRepository.ToCachedGetById(costExpenditureBillAccountingId);
        }

        public virtual void InsertCostExpenditureBillAccounting(CostExpenditureBillAccounting costExpenditureBillAccounting)
        {
            if (costExpenditureBillAccounting == null)
            {
                throw new ArgumentNullException("costExpenditureBillAccounting");
            }

            var uow = CostExpenditureBillAccountingMappingRepository.UnitOfWork;
            CostExpenditureBillAccountingMappingRepository.Insert(costExpenditureBillAccounting);
            uow.SaveChanges();


            //通知
            _eventPublisher.EntityInserted(costExpenditureBillAccounting);
        }

        public virtual void UpdateCostExpenditureBillAccounting(CostExpenditureBillAccounting costExpenditureBillAccounting)
        {
            if (costExpenditureBillAccounting == null)
            {
                throw new ArgumentNullException("costExpenditureBillAccounting");
            }

            var uow = CostExpenditureBillAccountingMappingRepository.UnitOfWork;
            CostExpenditureBillAccountingMappingRepository.Update(costExpenditureBillAccounting);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(costExpenditureBillAccounting);
        }

        public virtual void DeleteCostExpenditureBillAccounting(CostExpenditureBillAccounting costExpenditureBillAccounting)
        {
            if (costExpenditureBillAccounting == null)
            {
                throw new ArgumentNullException("costExpenditureBillAccounting");
            }

            var uow = CostExpenditureBillAccountingMappingRepository.UnitOfWork;
            CostExpenditureBillAccountingMappingRepository.Delete(costExpenditureBillAccounting);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(costExpenditureBillAccounting);
        }


        #endregion

        public void UpdateCostExpenditureBillActive(int? store, int? billId, int? user)
        {
            var query = CostExpenditureBillsRepository.Table.ToList();

            query = query.Where(x => x.StoreId == store && x.MakeUserId == user && x.AuditedStatus == true && (DateTime.Now.Subtract(x.AuditedDate ?? DateTime.Now).Duration().TotalDays > 30)).ToList();

            if (billId.HasValue && billId.Value > 0)
            {
                query = query.Where(x => x.Id == billId).ToList();
            }

            var result = query;

            if (result != null && result.Count > 0)
            {
                var uow = CostExpenditureBillsRepository.UnitOfWork;
                foreach (CostExpenditureBill bill in result)
                {
                    if ((bill.AuditedStatus && !bill.ReversedStatus) || bill.Deleted) continue;
                    bill.Deleted = true;
                    CostExpenditureBillsRepository.Update(bill);
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
        public IList<CostExpenditureBill> GetCostExpenditureBillListToFinanceReceiveAccount(int? storeId, int? employeeId = null, DateTime? start = null, DateTime? end = null)
        {
            var query = CostExpenditureBillsRepository.Table;

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
                query = query.Where(a => a.EmployeeId == employeeId);
            }

            query = query.OrderByDescending(a => a.CreatedOnUtc);

            return query.ToList();
        }

        public BaseResult BillCreateOrUpdate(int storeId, int userId, int? billId, CostExpenditureBill bill, List<CostExpenditureBillAccounting> accountingOptions, List<AccountingOption> accountings, CostExpenditureBillUpdate data, List<CostExpenditureItem> items, bool isAdmin = false,bool doAudit = true)
        {
            var uow = CostExpenditureBillsRepository.UnitOfWork;

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
                    #region 更新费用支出单
                    if (bill != null)
                    {
                        bill.EmployeeId = data.EmployeeId;
                        bill.TerminalId = data.CustomerId;
                        bill.Remark = data.Remark;
                        //开单日期
                        bill.PayDate = DateTime.Now;
                        bill.OweCash = data.OweCash;
                        bill.SumAmount = data.Items.Sum(c => c.Amount)??0;

                        UpdateCostExpenditureBill(bill);
                    }

                    #endregion
                }
                else
                {
                    #region 添加费用支出单

                    bill.StoreId = storeId;
                    bill.EmployeeId = data.EmployeeId;
                    bill.TerminalId = data.CustomerId;

                    //开单日期
                    bill.PayDate = DateTime.Now;

                    bill.CreatedOnUtc = DateTime.Now;

                    //单据编号
                    bill.BillNumber = string.IsNullOrEmpty(data.BillNumber) ? CommonHelper.GetBillNumber("FYZC", storeId): data.BillNumber;

                    var sb = GetCostExpenditureBillByNumber(storeId, bill.BillNumber);
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
                    bill.OweCash = data.OweCash;
                    bill.SumAmount = data.Items.Sum(c => c.Amount) ?? 0;

                    if (accountingOptions.Sum(s => s.CollectionAmount) >0 && data.OweCash>0)
                    {
                        bill.ReceivedStatus = ReceiptStatus.Part;
                    }
                    if (accountingOptions.Sum(s => s.CollectionAmount ) > 0 && data.OweCash==0)
                    {
                        bill.ReceivedStatus = ReceiptStatus.Received;
                    }
                    if (accountingOptions.Sum(s => s.CollectionAmount) == 0)
                    {
                        bill.ReceivedStatus = ReceiptStatus.None;
                    }
                    InsertCostExpenditureBill(bill);

                    #endregion
                }

                #region 更新收款项目

                data.Items.ForEach(p =>
                {
                    if (p.AccountingOptionId != 0)
                    {
                        var sd = GetCostExpenditureItemById(storeId, p.Id);
                        if (sd == null)
                        {
                            //追加项
                            if (bill.Items.Count(cp => cp.Id == p.Id) == 0)
                            {
                                var item = p;
                                item.CostExpenditureBillId = bill.Id;
                                item.CreatedOnUtc = DateTime.Now;
                                item.StoreId = storeId;
                                InsertCostExpenditureItem(item);
                                //不排除
                                p.Id = item.Id;
                                //costExpenditureBill.Items.Add(item);
                                if (!bill.Items.Select(s => s.Id).Contains(item.Id))
                                {
                                    bill.Items.Add(item);
                                }
                            }
                        }
                        else
                        {
                            //存在则更新
                            sd.AccountingOptionId = p.AccountingOptionId;//费用类别
                            sd.CustomerId = p.CustomerId;//客户
                            sd.Amount = p.Amount;//金额
                            sd.CostContractId = sd.CostContractId;//费用合同
                            sd.Remark = p.Remark;//备注
                            UpdateCostExpenditureItem(sd);
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
                        var item = GetCostExpenditureItemById(storeId, p.Id);
                        if (item != null)
                        {
                            DeleteCostExpenditureItem(item);
                        }
                    }
                });

                #endregion

                #region 收款账户映射

                var costExpenditureBillAccountings = GetCostExpenditureBillAccountingsByCostExpenditureBillId(bill.Id);
                accountings.ToList().ForEach(c =>
                {
                    if (data.Accounting.Select(a => a.AccountingOptionId).Contains(c.Id))
                    {
                        if (!costExpenditureBillAccountings.Select(cc => cc.AccountingOptionId).Contains(c.Id))
                        {
                            var collection = data.Accounting.Select(a => a).Where(a => a.AccountingOptionId == c.Id).FirstOrDefault();
                            var cashReceiptBillAccounting = new CostExpenditureBillAccounting()
                            {
                                //AccountingOption = c,
                                AccountingOptionId = c.Id,
                                CollectionAmount = collection != null ? collection.CollectionAmount : 0,
                                CostExpenditureBill = bill,
                                BillId = bill.Id,
                                StoreId = storeId
                            };
                            //添加账户
                            InsertCostExpenditureBillAccounting(cashReceiptBillAccounting);
                        }
                        else
                        {
                            costExpenditureBillAccountings.ToList().ForEach(acc =>
                            {
                                var collection = data.Accounting.Select(a => a).Where(a => a.AccountingOptionId == acc.AccountingOptionId).FirstOrDefault();
                                acc.CollectionAmount = collection != null ? collection.CollectionAmount : 0;
                                //更新账户
                                UpdateCostExpenditureBillAccounting(acc);
                            });
                        }
                    }
                    else
                    {
                        if (costExpenditureBillAccountings.Select(cc => cc.AccountingOptionId).Contains(c.Id))
                        {
                            var saleaccountings = costExpenditureBillAccountings.Select(cc => cc).Where(cc => cc.AccountingOptionId == c.Id).ToList();
                            saleaccountings.ForEach(sa =>
                            {
                                DeleteCostExpenditureBillAccounting(sa);
                            });
                        }
                    }
                });

                #endregion

                //判断App开单是否自动审核
                bool appBillAutoAudits = false;
                if (data.Operation == (int)OperationEnum.APP)
                {
                    appBillAutoAudits = _settingService.AppBillAutoAudits(storeId, BillTypeEnum.CostExpenditureBill);
                }
                //读取配置自动审核、管理员创建自动审核
                if ((isAdmin && doAudit) || appBillAutoAudits) //判断当前登录者是否为管理员,若为管理员，开启自动审核
                {
                    AuditingNoTran(userId, bill);
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
                            BillType = BillTypeEnum.CostExpenditureBill,
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
            catch (Exception)
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

        public BaseResult Auditing(int userId, CostExpenditureBill bill)
        {
            var uow = CostExpenditureBillsRepository.UnitOfWork;

            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();
                bill.StoreId = bill.StoreId;


                AuditingNoTran(userId, bill);


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

        public BaseResult AuditingNoTran(int userId, CostExpenditureBill bill)
        {
            var successful = new BaseResult { Success = true, Message = "单据审核成功" };
            var failed = new BaseResult { Success = false, Message = "单据审核失败" };

            try
            {
                return _recordingVoucherService.CreateVoucher<CostExpenditureBill, CostExpenditureItem>(bill, bill.StoreId, userId, (voucherId) =>
                {
                    bill.VoucherId = voucherId;
                    bill.AuditedUserId = userId;
                    bill.AuditedDate = DateTime.Now;
                    bill.AuditedStatus = true;
                    UpdateCostExpenditureBill(bill);
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
                            StoreId = bill.StoreId,
                            MType = MTypeEnum.Audited,
                            Title = CommonHelper.GetEnumDescription<MTypeEnum>(MTypeEnum.Audited),
                            Date = bill.CreatedOnUtc,
                            BillType = BillTypeEnum.CostExpenditureBill,
                            BillNumber = bill.BillNumber,
                            BillId = bill.Id,
                            CreatedOnUtc = DateTime.Now
                        };
                        _queuedMessageService.InsertQueuedMessage(userNumbers.ToList(),queuedMessage);
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

        public BaseResult Reverse(int userId, CostExpenditureBill bill)
        {
            var successful = new BaseResult { Success = true, Message = "单据红冲成功" };
            var failed = new BaseResult { Success = false, Message = "单据红冲失败" };

            var uow = CostExpenditureBillsRepository.UnitOfWork;
            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();

                #region 红冲记账凭证

                _recordingVoucherService.CancleVoucher<CostExpenditureBill, CostExpenditureItem>(bill, () =>
                {
                    #region 修改单据表状态
                    bill.ReversedUserId = userId;
                    bill.ReversedDate = DateTime.Now;
                    bill.ReversedStatus = true;
                    //UpdateCostExpenditureBill(bill);
                    #endregion

                    bill.VoucherId = 0;
                    UpdateCostExpenditureBill(bill);
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
        /// 更新单据收款状态
        /// </summary>
        /// <param name="store"></param>
        /// <param name="billId"></param>
        /// <param name="receiptStatus"></param>
        public void UpdateReceived(int? store, int billId, ReceiptStatus receiptStatus)
        {
            var bill = GetCostExpenditureBillById(store, billId, false);
            if (bill != null)
            {
                bill.ReceiptStatus = (int)receiptStatus;
                var uow = CostExpenditureBillsRepository.UnitOfWork;
                CostExpenditureBillsRepository.Update(bill);
                uow.SaveChanges();
                //通知
                _eventPublisher.EntityUpdated(bill);
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
            var bill = GetCostExpenditureBillById(store, billId, false);
            if (bill != null)
            {
                bill.HandInStatus = handInStatus;
                var uow = CostExpenditureBillsRepository.UnitOfWork;
                CostExpenditureBillsRepository.Update(bill);
                uow.SaveChanges();
                //通知
                _eventPublisher.EntityUpdated(bill);
            }
        }
    }
}
