using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Data;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Finances;
using DCMS.Core.Domain.Tasks;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Configuration;
using DCMS.Services.Events;
using DCMS.Services.Settings;
using DCMS.Services.Tasks;
using DCMS.Services.Terminals;
using DCMS.Services.Users;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using DCMS.Services.Caching;

namespace DCMS.Services.Finances
{
    /// <summary>
    /// 表示预收款单据服务
    /// </summary>
    public partial class AdvanceReceiptBillService : BaseService, IAdvanceReceiptBillService
    {
        private readonly IUserService _userService;
        private readonly IQueuedMessageService _queuedMessageService;
        private readonly IRecordingVoucherService _recordingVoucherService;
        private readonly ITerminalService _terminalService;

        public AdvanceReceiptBillService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher,
            IUserService userService,
            IQueuedMessageService queuedMessageService,
            ISettingService settingService,
            IRecordingVoucherService recordingVoucherService,
            IAccountingService accountingService,
            ITerminalService terminalService
            ) : base(getter, cacheManager, eventPublisher)
        {
            _userService = userService;
            _queuedMessageService = queuedMessageService;
            _recordingVoucherService = recordingVoucherService;
            _terminalService = terminalService;
        }

        #region 单据
        public bool Exists(int billId)
        {
            return AdvanceReceiptBillsRepository.TableNoTracking.Where(a => a.Id == billId).Count() > 0;
        }

        public virtual IPagedList<AdvanceReceiptBill> GetAllAdvanceReceiptBills(int? store, int? makeuserId, int? customerId, string customerName, int? payeerId, string billNumber = "", bool? status = null, DateTime? start = null, DateTime? end = null, bool? isShowReverse = null, bool? sortByAuditedTime = null, int? accountingOptionId = null, bool? deleted = null, bool? handleStatus = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;


            DateTime.TryParse(start?.ToString("yyyy-MM-dd 00:00:00"), out DateTime startDate);
            DateTime.TryParse(end?.ToString("yyyy-MM-dd 23:59:59"), out DateTime endDate);

            var query = from pc in AdvanceReceiptBillsRepository.Table
                          .Include(cr => cr.Items)
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
            //客户名称检索
            if (!string.IsNullOrEmpty(customerName))
            {
                var terminalIds = _terminalService.GetTerminalIds(store, customerName);
                query = query.Where(a => terminalIds.Contains(a.CustomerId));
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

            if (accountingOptionId.HasValue && accountingOptionId.Value > 0)
            {
                query = query.Where(c => c.AccountingOptionId == accountingOptionId);
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


            //var unsortedAdvanceReceiptBills = query.ToList();
            ////分页
            //return new PagedList<AdvanceReceiptBill>(unsortedAdvanceReceiptBills, pageIndex, pageSize);
            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<AdvanceReceiptBill>(plists, pageIndex, pageSize, totalCount);
        }

        public virtual IList<AdvanceReceiptBill> GetAllAdvanceReceiptBills()
        {
            var query = from c in AdvanceReceiptBillsRepository.Table
                        orderby c.Id
                        select c;

            var categories = query.ToList();
            return categories;
        }

        public IList<AdvanceReceiptBill> GetAdvanceReceiptBillByStoreIdTerminalId(int storeId, int terminalId)
        {
            var query = AdvanceReceiptBillsRepository.Table;

            query = query.Where(a => a.AuditedStatus == true);
            query = query.Where(a => a.ReversedStatus == false);

            query = query.Where(a => a.StoreId == storeId);
            query = query.Where(a => a.CustomerId == terminalId);

            return query.ToList();

        }

        public virtual AdvanceReceiptBill GetAdvanceReceiptBillById(int? store, int advanceReceiptBillId)
        {
            if (advanceReceiptBillId == 0)
            {
                return null;
            }

            var key = DCMSDefaults.ADVANCEPRCEIPTBILL_BY_ID_KEY.FillCacheKey(store ?? 0, advanceReceiptBillId);
            return _cacheManager.Get(key, () =>
            {
                return AdvanceReceiptBillsRepository.ToCachedGetById(advanceReceiptBillId);
            });
        }

        public virtual AdvanceReceiptBill GetAdvanceReceiptBillById(int? store, int advanceReceiptBillId, bool isInclude = false)
        {
            if (advanceReceiptBillId == 0)
            {
                return null;
            }

            if (isInclude)
            {
                var query = AdvanceReceiptBillsRepository.Table
                .Include(ar => ar.Items)
                .ThenInclude(ao => ao.AccountingOption);

                return query.FirstOrDefault(a => a.Id == advanceReceiptBillId);
            }
            return AdvanceReceiptBillsRepository.ToCachedGetById(advanceReceiptBillId);
        }

        public virtual AdvanceReceiptBill GetAdvanceReceiptBillByNumber(int? store, string billNumber)
        {
            var key = DCMSDefaults.ADVANCEPRCEIPTBILL_BY_NUMBER_KEY.FillCacheKey(store ?? 0, billNumber);
            return _cacheManager.Get(key, () =>
            {
                var query = AdvanceReceiptBillsRepository.Table;
                var bill = query.Where(a => a.StoreId == store && a.BillNumber == billNumber).FirstOrDefault();
                return bill;
            });
        }

        public virtual void InsertAdvanceReceiptBill(AdvanceReceiptBill bill)
        {
            if (bill == null)
            {
                throw new ArgumentNullException("bill");
            }

            var uow = AdvanceReceiptBillsRepository.UnitOfWork;
            AdvanceReceiptBillsRepository.Insert(bill);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(bill);
        }

        public virtual void UpdateAdvanceReceiptBill(AdvanceReceiptBill bill)
        {
            if (bill == null)
            {
                throw new ArgumentNullException("bill");
            }

            var uow = AdvanceReceiptBillsRepository.UnitOfWork;
            AdvanceReceiptBillsRepository.Update(bill);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(bill);
        }

        public virtual void DeleteAdvanceReceiptBill(AdvanceReceiptBill bill)
        {
            if (bill == null)
            {
                throw new ArgumentNullException("bill");
            }

            var uow = AdvanceReceiptBillsRepository.UnitOfWork;
            AdvanceReceiptBillsRepository.Delete(bill);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityDeleted(bill);
        }


        #endregion


        #region 收款账户映射

        public virtual IPagedList<AdvanceReceiptBillAccounting> GetAdvanceReceiptBillAccountingsByAdvanceReceiptBillId(int storeId, int userId, int advanceReceiptBillId, int pageIndex, int pageSize)
        {
            if (pageSize >= 50)
                pageSize = 50;
            if (advanceReceiptBillId == 0)
            {
                return new PagedList<AdvanceReceiptBillAccounting>(new List<AdvanceReceiptBillAccounting>(), pageIndex, pageSize);
            }

            //string key = string.Format(ADVANCEPRCEIPTBILL_ACCOUNTINGL_BY_BILLID_KEY.FillCacheKey( advanceReceiptBillId, pageIndex, pageSize, _workContext.CurrentUser.Id, _workContext.CurrentStore.Id);
            var key = DCMSDefaults.ADVANCEPRCEIPTBILL_ACCOUNTING_ALLBY_BILLID_KEY.FillCacheKey(storeId, advanceReceiptBillId, pageIndex, pageSize, userId);
            return _cacheManager.Get(key, () =>
            {
                var query = from pc in AdvanceReceiptBillAccountingMappingRepository.Table
                            join p in AccountingOptionsRepository.Table on pc.AccountingOptionId equals p.Id
                            where pc.BillId == advanceReceiptBillId
                            orderby pc.Id
                            select pc;

                //var saleAccountings = new PagedList<AdvanceReceiptBillAccounting>(query.ToList(), pageIndex, pageSize);
                //return saleAccountings;
                //总页数
                var totalCount = query.Count();
                var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
                return new PagedList<AdvanceReceiptBillAccounting>(plists, pageIndex, pageSize, totalCount);
            });
        }

        public virtual IList<AdvanceReceiptBillAccounting> GetAdvanceReceiptBillAccountingsByAdvanceReceiptBillId(int? store, int advanceReceiptBillId)
        {

            var key = DCMSDefaults.ADVANCEPRCEIPTBILL_ACCOUNTINGL_BY_BILLID_KEY.FillCacheKey(store ?? 0, advanceReceiptBillId);
            return _cacheManager.Get(key, () =>
            {
                var query = from pc in AdvanceReceiptBillAccountingMappingRepository.Table
                            join p in AccountingOptionsRepository.Table on pc.AccountingOptionId equals p.Id
                            where pc.BillId == advanceReceiptBillId
                            orderby pc.Id
                            select pc;


                return query.ToList();
            });
        }

        public virtual AdvanceReceiptBillAccounting GetAdvanceReceiptBillAccountingById(int advanceReceiptBillAccountingId)
        {
            if (advanceReceiptBillAccountingId == 0)
            {
                return null;
            }

            return AdvanceReceiptBillAccountingMappingRepository.ToCachedGetById(advanceReceiptBillAccountingId);
        }

        public virtual void InsertAdvanceReceiptBillAccounting(AdvanceReceiptBillAccounting advanceReceiptBillAccounting)
        {
            if (advanceReceiptBillAccounting == null)
            {
                throw new ArgumentNullException("advanceReceiptBillAccounting");
            }

            var uow = AdvanceReceiptBillAccountingMappingRepository.UnitOfWork;
            AdvanceReceiptBillAccountingMappingRepository.Insert(advanceReceiptBillAccounting);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(advanceReceiptBillAccounting);
        }

        public virtual void UpdateAdvanceReceiptBillAccounting(AdvanceReceiptBillAccounting advanceReceiptBillAccounting)
        {
            if (advanceReceiptBillAccounting == null)
            {
                throw new ArgumentNullException("advanceReceiptBillAccounting");
            }

            var uow = AdvanceReceiptBillAccountingMappingRepository.UnitOfWork;
            AdvanceReceiptBillAccountingMappingRepository.Update(advanceReceiptBillAccounting);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(advanceReceiptBillAccounting);
        }

        public virtual void DeleteAdvanceReceiptBillAccounting(AdvanceReceiptBillAccounting advanceReceiptBillAccounting)
        {
            if (advanceReceiptBillAccounting == null)
            {
                throw new ArgumentNullException("advanceReceiptBillAccounting");
            }

            var uow = AdvanceReceiptBillAccountingMappingRepository.UnitOfWork;
            AdvanceReceiptBillAccountingMappingRepository.Delete(advanceReceiptBillAccounting);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(advanceReceiptBillAccounting);
        }


        #endregion

        public void UpdateAdvanceReceiptBillActive(int? store, int? billId, int? user)
        {
            var query = AdvanceReceiptBillsRepository.Table.ToList();

            query = query.Where(x => x.StoreId == store && x.MakeUserId == user && x.AuditedStatus == true && (DateTime.Now.Subtract(x.AuditedDate ?? DateTime.Now).Duration().TotalDays > 30)).ToList();

            if (billId.HasValue && billId.Value > 0)
            {
                query = query.Where(x => x.Id == billId).ToList();
            }

            var result = query;

            if (result != null && result.Count > 0)
            {
                var uow = AdvanceReceiptBillsRepository.UnitOfWork;
                foreach (AdvanceReceiptBill bill in result)
                {
                    if ((bill.AuditedStatus && !bill.ReversedStatus) || bill.Deleted) continue;
                    bill.Deleted = true;
                    AdvanceReceiptBillsRepository.Update(bill);
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
        public IList<AdvanceReceiptBill> GetAdvanceReceiptBillListToFinanceReceiveAccount(int? storeId, int? employeeId = null, DateTime? start = null, DateTime? end = null)
        {
            var query = AdvanceReceiptBillsRepository.Table;

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

        public BaseResult BillCreateOrUpdate(int storeId, int userId, int? billId, AdvanceReceiptBill bill, List<AdvanceReceiptBillAccounting> accountingOptions, List<AccountingOption> accountings, AdvanceReceiptBillUpdate data, bool isAdmin = false,bool doAudit = true)
        {
            var uow = AdvanceReceiptBillsRepository.UnitOfWork;

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
                    #region 更新预收款单
                    if (bill != null)
                    {
                        //客户
                        bill.CustomerId = data.CustomerId;
                        //收款人
                        bill.Payeer = data.Payeer ?? 0;
                        bill.DiscountAmount = data.DiscountAmount;
                        bill.OweCash = data.OweCash;

                        //预收款金额 = 优惠金额 + 科目金额
                        if (data.Accounting != null && data.Accounting.Count > 0)
                        {
                            bill.AdvanceAmount = bill.DiscountAmount + data.Accounting
                                .Where(s => s.Copy == false)
                                .Sum(ac => ac.CollectionAmount ) + bill.OweCash;
                        }
                        else
                        {
                            bill.AdvanceAmount = bill.DiscountAmount + bill.OweCash;
                        }

                        bill.AccountingOptionId = data.AccountingOptionId;
                        //备注
                        bill.Remark = data.Remark;

                        UpdateAdvanceReceiptBill(bill);
                    }

                    #endregion
                }
                else
                {
                    #region 添加预收款单

                    bill.StoreId = storeId;
                    //客户
                    bill.CustomerId = data.CustomerId;
                    //收款人
                    bill.Payeer = data.Payeer ?? 0;
                    //开单日期
                    bill.CreatedOnUtc = DateTime.Now;
                    bill.DiscountAmount = data.DiscountAmount;
                    bill.OweCash = data.OweCash;
                    //预收款金额 = 优惠金额 + 科目金额 + 欠款金额
                    if (data.Accounting != null && data.Accounting.Count > 0)
                    {
                        bill.AdvanceAmount = bill.DiscountAmount + data.Accounting.Where(s => s.Copy == false)
                            .Sum(ac => ac.CollectionAmount ) + bill.OweCash;
                    }
                    else
                    {
                        bill.AdvanceAmount = bill.DiscountAmount + bill.OweCash;
                    }
                    bill.AccountingOptionId = data.AccountingOptionId;
                    //单据编号
                    bill.BillNumber = string.IsNullOrEmpty(data.BillNumber) ? CommonHelper.GetBillNumber("YSK", storeId) : data.BillNumber;

                    var sb = GetAdvanceReceiptBillByNumber(storeId, bill.BillNumber);
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
                    if (accountingOptions.Sum(s => s.CollectionAmount) > 0 && data.OweCash == 0)
                    {
                        bill.ReceivedStatus = ReceiptStatus.Received;
                    }
                    if (accountingOptions.Sum(s => s.CollectionAmount )==0)
                    {
                        bill.ReceivedStatus = ReceiptStatus.None;
                    }
                    if(accountingOptions.Sum(s => s.CollectionAmount ) > 0 && data.OweCash > 0)
                    {
                        bill.ReceivedStatus = ReceiptStatus.Part;
                    }
                    InsertAdvanceReceiptBill(bill);

                    #endregion
                }

                #region 收款账户映射

                var advanceReceiptBillAccountings = GetAdvanceReceiptBillAccountingsByAdvanceReceiptBillId(storeId, bill.Id);
                accountings.ToList().ForEach(c =>
                {
                    if (data.Accounting != null && data.Accounting.Select(a => a.AccountingOptionId).Contains(c.Id))
                    {
                        if (!advanceReceiptBillAccountings.Select(cc => cc.AccountingOptionId).Contains(c.Id))
                        {
                            var collection = data.Accounting.Select(a => a).Where(a => a.AccountingOptionId == c.Id).FirstOrDefault();
                            var advanceReceiptBillAccounting = new AdvanceReceiptBillAccounting()
                            {
                                //AccountingOption = c,
                                AccountingOptionId = c.Id,
                                CollectionAmount = collection != null ? collection.CollectionAmount : 0,
                                AdvanceReceiptBill = bill,
                                BillId = bill.Id,
                                TerminalId = data.CustomerId,
                                StoreId = storeId,
                                Copy = collection.Copy
                            };
                            //添加账户
                            InsertAdvanceReceiptBillAccounting(advanceReceiptBillAccounting);
                        }
                        else
                        {
                            advanceReceiptBillAccountings.ToList().ForEach(acc =>
                            {
                                var collection = data.Accounting.Select(a => a).Where(a => a.AccountingOptionId == acc.AccountingOptionId).FirstOrDefault();
                                acc.CollectionAmount = collection != null ? collection.CollectionAmount : 0;
                                acc.TerminalId = data.CustomerId;
                                acc.Copy = collection.Copy;
                                //更新账户
                                UpdateAdvanceReceiptBillAccounting(acc);
                            });
                        }
                    }
                    else
                    {
                        if (advanceReceiptBillAccountings.Select(cc => cc.AccountingOptionId).Contains(c.Id))
                        {
                            var saleaccountings = advanceReceiptBillAccountings.Select(cc => cc).Where(cc => cc.AccountingOptionId == c.Id).ToList();
                            saleaccountings.ForEach(sa =>
                            {
                                DeleteAdvanceReceiptBillAccounting(sa);
                            });
                        }
                    }
                });

                #endregion

                //管理员创建自动审核
                if (isAdmin && doAudit) //判断当前登录者是否为管理员,若为管理员，开启自动审核
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
                            BillType = BillTypeEnum.AdvanceReceiptBill,
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

        public BaseResult Auditing(int storeId, int userId, AdvanceReceiptBill bill)
        {
            var uow = AdvanceReceiptBillsRepository.UnitOfWork;

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

        public BaseResult AuditingNoTran(int storeId, int userId, AdvanceReceiptBill bill)
        {
            var successful = new BaseResult { Success = true, Message = "单据审核成功" };
            var failed = new BaseResult { Success = false, Message = "单据审核失败" };

            try
            {
                return _recordingVoucherService.CreateVoucher<AdvanceReceiptBill, AdvanceReceiptBillAccounting>(bill, storeId, userId, (voucherId) =>
                {
                    bill.VoucherId = voucherId;
                    bill.AuditedUserId = userId;
                    bill.AuditedDate = DateTime.Now;
                    bill.AuditedStatus = true;
                    UpdateAdvanceReceiptBill(bill);
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
                            BillType = BillTypeEnum.AdvanceReceiptBill,
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

        public BaseResult Reverse(int userId, AdvanceReceiptBill bill)
        {
            var successful = new BaseResult { Success = true, Message = "单据红冲成功" };
            var failed = new BaseResult { Success = false, Message = "单据红冲失败" };

            var uow = AdvanceReceiptBillsRepository.UnitOfWork;
            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();

                #region 红冲记账凭证

                _recordingVoucherService.CancleVoucher<AdvanceReceiptBill, AdvanceReceiptBillAccounting>(bill, () =>
                {
                    #region 修改单据表状态
                    bill.ReversedUserId = userId;
                    bill.ReversedDate = DateTime.Now;
                    bill.ReversedStatus = true;
                    //UpdateAdvanceReceiptBill(bill);
                    #endregion

                    bill.VoucherId = 0;
                    UpdateAdvanceReceiptBill(bill);
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
            var bill = GetAdvanceReceiptBillById(store, billId, false);
            if (bill != null)
            {
                bill.ReceiptStatus = (int)receiptStatus;
                var uow = AdvanceReceiptBillsRepository.UnitOfWork;
                AdvanceReceiptBillsRepository.Update(bill);
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
            var bill = GetAdvanceReceiptBillById(store, billId, false);
            if (bill != null)
            {
                bill.HandInStatus = handInStatus;
                var uow = AdvanceReceiptBillsRepository.UnitOfWork;
                AdvanceReceiptBillsRepository.Update(bill);
                uow.SaveChanges();
                //通知
                _eventPublisher.EntityUpdated(bill);
            }
        }
    }
}
