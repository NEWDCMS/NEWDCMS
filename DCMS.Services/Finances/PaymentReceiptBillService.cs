using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Data;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Finances;
using DCMS.Core.Domain.Tasks;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using DCMS.Services.Purchases;
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
    /// 表示付款单据服务
    /// </summary>
    public partial class PaymentReceiptBillService : BaseService, IPaymentReceiptBillService
    {

        #region 构造
        private readonly IUserService _userService;
        private readonly IQueuedMessageService _queuedMessageService;
        private readonly IRecordingVoucherService _recordingVoucherService;
        private readonly IPurchaseBillService _purchaseService;
        private readonly IPurchaseReturnBillService _purchaseReturnService;
        private readonly IFinancialIncomeBillService _financialIncomeBillService;


        public PaymentReceiptBillService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher,
            IUserService userService,
            IQueuedMessageService queuedMessageService,
            IRecordingVoucherService recordingVoucherService,
            IPurchaseBillService purchaseService,
            IFinancialIncomeBillService financialIncomeBillService,
        IPurchaseReturnBillService purchaseReturnService) : base(getter, cacheManager, eventPublisher)
        {
            _userService = userService;
            _queuedMessageService = queuedMessageService;
            _recordingVoucherService = recordingVoucherService;
            _purchaseService = purchaseService;
            _purchaseReturnService = purchaseReturnService;
            _financialIncomeBillService = financialIncomeBillService;
        }

        #endregion

        #region 单据

        public bool Exists(int billId)
        {
            return PaymentReceiptBillsRepository.TableNoTracking.Where(a => a.Id == billId).Count() > 0;
        }

        public virtual IPagedList<PaymentReceiptBill> GetAllPaymentReceiptBills(int? store, int? makeuserId, int? draweer, int? manufacturerId, string billNumber = "", bool? status = null, DateTime? start = null, DateTime? end = null, bool? isShowReverse = null, bool? sortByAuditedTime = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;
            //var query = PaymentReceiptBillsRepository.Table;
            var query = from pc in PaymentReceiptBillsRepository.Table
                          .Include(cr => cr.Items)
                          //.ThenInclude(cr => cr.PaymentReceiptBill)
                          .Include(cr => cr.PaymentReceiptBillAccountings)
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

            if (draweer.HasValue && draweer.Value > 0)
            {
                query = query.Where(c => c.Draweer == draweer);
            }

            if (manufacturerId.HasValue && manufacturerId.Value > 0)
            {
                query = query.Where(c => c.ManufacturerId == manufacturerId);
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
            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<PaymentReceiptBill>(plists, pageIndex, pageSize, totalCount);
        }

        public virtual IList<PaymentReceiptBill> GetAllPaymentReceiptBills()
        {
            var query = from c in PaymentReceiptBillsRepository.Table
                        orderby c.Id
                        select c;

            var categories = query.ToList();
            return categories;
        }

        public virtual PaymentReceiptBill GetPaymentReceiptBillById(int? store, int paymentReceiptBillId)
        {
            if (paymentReceiptBillId == 0)
            {
                return null;
            }

            var key = DCMSDefaults.PAYMENTRECEIPTBILL_BY_ID_KEY.FillCacheKey(store ?? 0, paymentReceiptBillId);
            return _cacheManager.Get(key, () =>
            {
                return PaymentReceiptBillsRepository.ToCachedGetById(paymentReceiptBillId);
            });
        }

        public virtual PaymentReceiptBill GetPaymentReceiptBillById(int? store, int paymentReceiptBillId, bool isInclude = false)
        {
            if (paymentReceiptBillId == 0)
            {
                return null;
            }

            if (isInclude)
            {
                var query = PaymentReceiptBillsRepository.Table
                .Include(f => f.Items)
                //.ThenInclude(f => f.PaymentReceiptBill)
                .Include(pb => pb.PaymentReceiptBillAccountings)
                .ThenInclude(ao => ao.AccountingOption);

                return query.FirstOrDefault(pb => pb.Id == paymentReceiptBillId);
            }
            return PaymentReceiptBillsRepository.ToCachedGetById(paymentReceiptBillId);
        }

        public virtual PaymentReceiptBill GetPaymentReceiptBillNumber(int? store, string billNumber)
        {
            var key = DCMSDefaults.PAYMENTRECEIPTBILL_BY_NUMBER_KEY.FillCacheKey(store ?? 0, billNumber);
            return _cacheManager.Get(key, () =>
            {
                var query = PaymentReceiptBillsRepository.Table;
                var paymentReceiptBill = query.Where(a => a.StoreId == store && a.BillNumber == billNumber).FirstOrDefault();
                return paymentReceiptBill;
            });
        }



        public virtual void InsertPaymentReceiptBill(PaymentReceiptBill paymentReceiptBill)
        {
            if (paymentReceiptBill == null)
            {
                throw new ArgumentNullException("paymentReceiptBill");
            }

            var uow = PaymentReceiptBillsRepository.UnitOfWork;
            PaymentReceiptBillsRepository.Insert(paymentReceiptBill);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(paymentReceiptBill);
        }

        public virtual void UpdatePaymentReceiptBill(PaymentReceiptBill paymentReceiptBill)
        {
            if (paymentReceiptBill == null)
            {
                throw new ArgumentNullException("paymentReceiptBill");
            }

            var uow = PaymentReceiptBillsRepository.UnitOfWork;
            PaymentReceiptBillsRepository.Update(paymentReceiptBill);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(paymentReceiptBill);
        }

        public virtual void DeletePaymentReceiptBill(PaymentReceiptBill paymentReceiptBill)
        {
            if (paymentReceiptBill == null)
            {
                throw new ArgumentNullException("paymentReceiptBill");
            }

            var uow = PaymentReceiptBillsRepository.UnitOfWork;
            PaymentReceiptBillsRepository.Delete(paymentReceiptBill);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityDeleted(paymentReceiptBill);
        }

        #endregion

        #region 单据项目

        public virtual IPagedList<PaymentReceiptItem> GetPaymentReceiptItemsByPaymentReceiptBillId(int paymentReceiptBillId, int? userId, int? storeId, int pageIndex, int pageSize)
        {
            if (pageSize >= 50)
                pageSize = 50;
            if (paymentReceiptBillId == 0)
            {
                return new PagedList<PaymentReceiptItem>(new List<PaymentReceiptItem>(), pageIndex, pageSize);
            }

            var key = DCMSDefaults.PAYMENTRECEIPTBILLITEM_ALL_KEY.FillCacheKey(storeId, paymentReceiptBillId, pageIndex, pageSize, userId);

            return _cacheManager.Get(key, () =>
            {
                var query = from pc in PaymentReceiptItemsRepository.Table
                            where pc.PaymentReceiptBillId == paymentReceiptBillId
                            orderby pc.Id
                            select pc;
                //var productPaymentReceiptBills = new PagedList<PaymentReceiptItem>(query.ToList(), pageIndex, pageSize);
                //return productPaymentReceiptBills;
                //总页数
                var totalCount = query.Count();
                var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
                return new PagedList<PaymentReceiptItem>(plists, pageIndex, pageSize, totalCount);
            });
        }

        public virtual List<PaymentReceiptItem> GetPaymentReceiptItemList(int paymentReceiptBillId)
        {
            List<PaymentReceiptItem> paymentReceiptItems = null;
            var query = PaymentReceiptItemsRepository_RO.Table.Include(s => s.PaymentReceiptBill);
            paymentReceiptItems = query.Where(a => a.PaymentReceiptBillId == paymentReceiptBillId).ToList();
            return paymentReceiptItems;
        }

        public virtual PaymentReceiptItem GetPaymentReceiptItemById(int? store, int paymentReceiptItemId)
        {
            if (paymentReceiptItemId == 0)
            {
                return null;
            }

            return PaymentReceiptItemsRepository.ToCachedGetById(paymentReceiptItemId);
        }

        public virtual void InsertPaymentReceiptItem(PaymentReceiptItem paymentReceiptItem)
        {
            if (paymentReceiptItem == null)
            {
                throw new ArgumentNullException("paymentReceiptItem");
            }

            var uow = PaymentReceiptItemsRepository.UnitOfWork;
            PaymentReceiptItemsRepository.Insert(paymentReceiptItem);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(paymentReceiptItem);
        }

        public virtual void UpdatePaymentReceiptItem(PaymentReceiptItem paymentReceiptItem)
        {
            if (paymentReceiptItem == null)
            {
                throw new ArgumentNullException("paymentReceiptItem");
            }

            var uow = PaymentReceiptItemsRepository.UnitOfWork;
            PaymentReceiptItemsRepository.Update(paymentReceiptItem);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(paymentReceiptItem);
        }

        public virtual void DeletePaymentReceiptItem(PaymentReceiptItem paymentReceiptItem)
        {
            if (paymentReceiptItem == null)
            {
                throw new ArgumentNullException("paymentReceiptItem");
            }

            var uow = PaymentReceiptItemsRepository.UnitOfWork;
            PaymentReceiptItemsRepository.Delete(paymentReceiptItem);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(paymentReceiptItem);
        }


        #endregion

        #region 收款账户映射

        public virtual IPagedList<PaymentReceiptBillAccounting> GetPaymentReceiptBillAccountingsByPaymentReceiptBillId(int storeId, int userId, int paymentReceiptBillId, int pageIndex, int pageSize)
        {
            if (pageSize >= 50)
                pageSize = 50;
            if (paymentReceiptBillId == 0)
            {
                return new PagedList<PaymentReceiptBillAccounting>(new List<PaymentReceiptBillAccounting>(), pageIndex, pageSize);
            }

            //string key = string.Format(PAYMENTRECEIPTBILL_ACCOUNTINGL_BY_BILLID_KEY.FillCacheKey( paymentReceiptBillId, pageIndex, pageSize, _workContext.CurrentUser.Id, _workContext.CurrentStore.Id);
            var key = DCMSDefaults.PAYMENTRECEIPTBILL_ACCOUNTING_ALLBY_BILLID_KEY.FillCacheKey(storeId, paymentReceiptBillId, pageIndex, pageSize, userId);
            return _cacheManager.Get(key, () =>
            {
                var query = from pc in PaymentReceiptBillAccountingMappingRepository.Table
                            join p in AccountingOptionsRepository.Table on pc.AccountingOptionId equals p.Id
                            where pc.BillId == paymentReceiptBillId
                            orderby pc.Id
                            select pc;

                //return saleAccountings;
                //总页数
                var totalCount = query.Count();
                var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
                return new PagedList<PaymentReceiptBillAccounting>(plists, pageIndex, pageSize, totalCount);
            });
        }

        public virtual IList<PaymentReceiptBillAccounting> GetPaymentReceiptBillAccountingsByPaymentReceiptBillId(int? store, int paymentReceiptBillId)
        {

            var key = DCMSDefaults.PAYMENTRECEIPTBILL_ACCOUNTINGL_BY_BILLID_KEY.FillCacheKey(store ?? 0, paymentReceiptBillId);
            return _cacheManager.Get(key, () =>
            {
                var query = from pc in PaymentReceiptBillAccountingMappingRepository.Table
                            join p in AccountingOptionsRepository.Table on pc.AccountingOptionId equals p.Id
                            where pc.BillId == paymentReceiptBillId
                            orderby pc.Id
                            select pc;


                return query.ToList();
            });
        }

        public virtual PaymentReceiptBillAccounting GetPaymentReceiptBillAccountingById(int paymentReceiptBillAccountingId)
        {
            if (paymentReceiptBillAccountingId == 0)
            {
                return null;
            }

            return PaymentReceiptBillAccountingMappingRepository.ToCachedGetById(paymentReceiptBillAccountingId);
        }

        public virtual void InsertPaymentReceiptBillAccounting(PaymentReceiptBillAccounting paymentReceiptBillAccounting)
        {
            if (paymentReceiptBillAccounting == null)
            {
                throw new ArgumentNullException("paymentReceiptBillAccounting");
            }

            var uow = PaymentReceiptBillAccountingMappingRepository.UnitOfWork;
            PaymentReceiptBillAccountingMappingRepository.Insert(paymentReceiptBillAccounting);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(paymentReceiptBillAccounting);
        }

        public virtual void UpdatePaymentReceiptBillAccounting(PaymentReceiptBillAccounting paymentReceiptBillAccounting)
        {
            if (paymentReceiptBillAccounting == null)
            {
                throw new ArgumentNullException("paymentReceiptBillAccounting");
            }

            var uow = PaymentReceiptBillAccountingMappingRepository.UnitOfWork;
            PaymentReceiptBillAccountingMappingRepository.Update(paymentReceiptBillAccounting);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(paymentReceiptBillAccounting);
        }

        public virtual void DeletePaymentReceiptBillAccounting(PaymentReceiptBillAccounting paymentReceiptBillAccounting)
        {
            if (paymentReceiptBillAccounting == null)
            {
                throw new ArgumentNullException("paymentReceiptBillAccounting");
            }

            var uow = PaymentReceiptBillAccountingMappingRepository.UnitOfWork;
            PaymentReceiptBillAccountingMappingRepository.Delete(paymentReceiptBillAccounting);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(paymentReceiptBillAccounting);
        }


        #endregion

        public BaseResult BillCreateOrUpdate(int storeId, int userId, int? billId, PaymentReceiptBill bill, List<PaymentReceiptBillAccounting> accountingOptions, List<AccountingOption> accountings, PaymentReceiptBillUpdate data, List<PaymentReceiptItem> items, bool isAdmin = false,bool doAudit = true)
        {
            var uow = PaymentReceiptBillsRepository.UnitOfWork;

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
                    #region 更新付款单
                    if (bill != null)
                    {

                        //供应商
                        bill.ManufacturerId = data.ManufacturerId;
                        //付款人
                        bill.Draweer = data.Draweer;
                        //剩余金额(收款后尚欠金额总和)
                        bill.AmountOwedAfterReceipt = data.AmountOwedAfterReceipt;
                        //总优惠金额(本次优惠金额总和)
                        bill.DiscountAmount = data.DiscountAmount;
                        //备注
                        bill.Remark = data.Remark;
                        UpdatePaymentReceiptBill(bill);
                    }

                    #endregion
                }
                else
                {
                    #region 添加付款单

                    bill.StoreId = storeId;
                    //供应商
                    bill.ManufacturerId = data.ManufacturerId;
                    //付款人
                    bill.Draweer = data.Draweer;
                    //开单日期
                    bill.CreatedOnUtc = DateTime.Now;
                    //剩余金额(收款后尚欠金额总和)
                    bill.AmountOwedAfterReceipt = data.AmountOwedAfterReceipt;
                    //总优惠金额(本次优惠金额总和)
                    bill.DiscountAmount = data.DiscountAmount;
                    //单据编号
                    bill.BillNumber = string.IsNullOrEmpty(data.BillNumber) ? CommonHelper.GetBillNumber("FK", storeId): data.BillNumber;

                    var sb = GetPaymentReceiptBillNumber(storeId, bill.BillNumber);
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
                    bill.Operation = data.Operation;
                    //标识操作源

                    InsertPaymentReceiptBill(bill);

                    #endregion
                }


                #region 更新付款项目

                data.Items.ForEach(p =>
                {
                    if (!string.IsNullOrEmpty(p.BillNumber) && p.BillTypeId != 0)
                    {
                        var sd = GetPaymentReceiptItemById(storeId, p.Id);
                        if (sd == null)
                        {
                            //追加项
                            if (bill.Items.Count(cp => cp.Id == p.Id) == 0)
                            {
                                var item = p;
                                item.PaymentReceiptBillId = bill.Id;
                                item.CreatedOnUtc = DateTime.Now;
                                item.StoreId = storeId;

                                item.BillId = p.BillId;

                                InsertPaymentReceiptItem(item);

                                //不排除
                                p.Id = item.Id;

                                if (!bill.Items.Select(s => s.Id).Contains(item.Id))
                                {
                                    bill.Items.Add(item);
                                }

                                //已付款
                                if (item.AmountOwedAfterReceipt == decimal.Zero)
                                {
                                    //更新开单状态
                                    switch (item.BillTypeEnum)
                                    {
                                        case BillTypeEnum.PurchaseBill:
                                            _purchaseService.UpdatePaymented(storeId, item.BillId, PayStatus.Paid);
                                            break;
                                        case BillTypeEnum.PurchaseReturnBill:
                                            _purchaseReturnService.UpdatePaymented(storeId, item.BillId, PayStatus.Paid);
                                            break;
                                        case BillTypeEnum.FinancialIncomeBill:
                                            _financialIncomeBillService.UpdatePaymented(storeId, item.BillId, PayStatus.Paid);
                                            break;
                                    }
                                }
                                //部分付款
                                else if (item.AmountOwedAfterReceipt <= item.ArrearsAmount)
                                {
                                    //更新开单状态
                                    switch (item.BillTypeEnum)
                                    {
                                        case BillTypeEnum.PurchaseBill:
                                            _purchaseService.UpdatePaymented(storeId, item.BillId, PayStatus.Part);
                                            break;
                                        case BillTypeEnum.PurchaseReturnBill:
                                            _purchaseReturnService.UpdatePaymented(storeId, item.BillId, PayStatus.Part);
                                            break;
                                        case BillTypeEnum.FinancialIncomeBill:
                                            _financialIncomeBillService.UpdatePaymented(storeId, item.BillId, PayStatus.Part);
                                            break;
                                    }
                                }
                                //如果付款后项目尚欠金额,且尚欠金额 必须大于收款后尚欠金额时
                                else
                                {
                                    ////更新欠款额度
                                    //switch (item.BillTypeEnum)
                                    //{
                                    //    case BillTypeEnum.PurchaseBill:
                                    //        _purchaseService.UpdatePurchaseBillOweCash(item.BillNumber, item.AmountOwedAfterReceipt);
                                    //        break;
                                    //    case BillTypeEnum.PurchaseReturnBill:
                                    //        _purchaseReturnService.UpdatePurchaseReturnBillOweCash(item.BillNumber, item.AmountOwedAfterReceipt);
                                    //        break;
                                    //}
                                }

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
                            sd.PaymentedAmount = p.PaymentedAmount;//已付金额
                            sd.ArrearsAmount = p.ArrearsAmount;//尚欠金额
                            sd.DiscountAmountOnce = p.DiscountAmountOnce;//本次优惠金额
                            sd.ReceivableAmountOnce = p.ReceivableAmountOnce;//本次付款金额
                            sd.AmountOwedAfterReceipt = p.AmountOwedAfterReceipt;//付款后尚欠金额	
                            sd.Remark = p.Remark;//备注
                            sd.BillId = p.BillId;

                            //已付款
                            if (sd.AmountOwedAfterReceipt == decimal.Zero)
                            {
                                //更新开单状态
                                switch (sd.BillTypeEnum)
                                {
                                    case BillTypeEnum.PurchaseBill:
                                        _purchaseService.UpdatePaymented(storeId, sd.BillId, PayStatus.Paid);
                                        break;
                                    case BillTypeEnum.PurchaseReturnBill:
                                        _purchaseReturnService.UpdatePaymented(storeId, sd.BillId, PayStatus.Paid);
                                        break;
                                    case BillTypeEnum.FinancialIncomeBill:
                                        _financialIncomeBillService.UpdatePaymented(storeId, sd.BillId, PayStatus.Paid);
                                        break;
                                }
                            }
                            //部分付款
                            else if (sd.AmountOwedAfterReceipt <= sd.ArrearsAmount)
                            {
                                //更新开单状态
                                switch (sd.BillTypeEnum)
                                {
                                    case BillTypeEnum.PurchaseBill:
                                        _purchaseService.UpdatePaymented(storeId, sd.BillId, PayStatus.Part);
                                        break;
                                    case BillTypeEnum.PurchaseReturnBill:
                                        _purchaseReturnService.UpdatePaymented(storeId, sd.BillId, PayStatus.Part);
                                        break;
                                    case BillTypeEnum.FinancialIncomeBill:
                                        _financialIncomeBillService.UpdatePaymented(storeId, sd.BillId, PayStatus.Part);
                                        break;
                                }
                            }
                            //如果付款后项目尚欠金额,且尚欠金额 必须大于收款后尚欠金额时
                            else
                            {
                                ////更新欠款额度
                                //switch (sd.BillTypeEnum)
                                //{
                                //    case BillTypeEnum.PurchaseBill:
                                //        _purchaseService.UpdatePurchaseBillOweCash(sd.BillNumber, sd.AmountOwedAfterReceipt);
                                //        break;
                                //    case BillTypeEnum.PurchaseReturnBill:
                                //        _purchaseReturnService.UpdatePurchaseReturnBillOweCash(sd.BillNumber, sd.AmountOwedAfterReceipt);
                                //        break;
                                //}
                            }

                            UpdatePaymentReceiptItem(sd);
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
                        var item = GetPaymentReceiptItemById(storeId, p.Id);
                        if (item != null)
                        {
                            DeletePaymentReceiptItem(item);
                        }
                    }
                });

                #endregion

                #region 收款账户映射

                var paymentReceiptBillAccountings = GetPaymentReceiptBillAccountingsByPaymentReceiptBillId(storeId, bill.Id);
                accountings.ToList().ForEach(c =>
                {
                    if (data.Accounting.Select(a => a.AccountingOptionId).Contains(c.Id))
                    {
                        if (!paymentReceiptBillAccountings.Select(cc => cc.AccountingOptionId).Contains(c.Id))
                        {
                            var collection = data.Accounting.Select(a => a).Where(a => a.AccountingOptionId == c.Id).FirstOrDefault();
                            var paymentReceiptBillAccounting = new PaymentReceiptBillAccounting()
                            {
                                //AccountingOption = c,
                                AccountingOptionId = c.Id,
                                CollectionAmount = collection != null ? collection.CollectionAmount : 0,
                                PaymentReceiptBill = bill,
                                BillId = bill.Id,
                                ManufacturerId = data.ManufacturerId,
                                StoreId = storeId
                            };
                            //添加账户
                            InsertPaymentReceiptBillAccounting(paymentReceiptBillAccounting);
                        }
                        else
                        {
                            paymentReceiptBillAccountings.ToList().ForEach(acc =>
                            {
                                var collection = data.Accounting.Select(a => a).Where(a => a.AccountingOptionId == acc.AccountingOptionId).FirstOrDefault();
                                acc.CollectionAmount = collection != null ? collection.CollectionAmount : 0;
                                acc.ManufacturerId = data.ManufacturerId;
                                //更新账户
                                UpdatePaymentReceiptBillAccounting(acc);
                            });
                        }
                    }
                    else
                    {
                        if (paymentReceiptBillAccountings.Select(cc => cc.AccountingOptionId).Contains(c.Id))
                        {
                            var saleaccountings = paymentReceiptBillAccountings.Select(cc => cc).Where(cc => cc.AccountingOptionId == c.Id).ToList();
                            saleaccountings.ForEach(sa =>
                            {
                                DeletePaymentReceiptBillAccounting(sa);
                            });
                        }
                    }
                });

                #endregion

                //管理员创建自动审核
                if (isAdmin && doAudit) //判断当前登录者是否为管理员,若为管理员，开启自动审核
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
                            BillType = BillTypeEnum.PaymentReceiptBill,
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


        public BaseResult Auditing(int storeId, int userId, PaymentReceiptBill bill)
        {
            var uow = CashReceiptBillsRepository.UnitOfWork;

            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();
                bill.StoreId = storeId;


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

        public BaseResult AuditingNoTran(int userId, PaymentReceiptBill bill)
        {
            var successful = new BaseResult { Success = true, Message = "单据审核成功" };
            var failed = new BaseResult { Success = false, Message = "单据审核失败" };

            try
            {
                return _recordingVoucherService.CreateVoucher<PaymentReceiptBill, PaymentReceiptItem>(bill, bill.StoreId, userId, (voucherId) =>
                {
                    #region 修改单据表状态
                    bill.VoucherId = voucherId;
                    bill.AuditedDate = DateTime.Now;
                    bill.AuditedUserId = userId;
                    bill.AuditedStatus = true;
                    UpdatePaymentReceiptBill(bill);
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
                            StoreId = bill.StoreId,
                            MType = MTypeEnum.Audited,
                            Title = CommonHelper.GetEnumDescription<MTypeEnum>(MTypeEnum.Audited),
                            Date = bill.CreatedOnUtc,
                            BillType = BillTypeEnum.PaymentReceiptBill,
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

        public BaseResult Reverse(int userId, PaymentReceiptBill bill)
        {
            var successful = new BaseResult { Success = true, Message = "单据红冲成功" };
            var failed = new BaseResult { Success = false, Message = "单据红冲失败" };

            var uow = PaymentReceiptBillsRepository.UnitOfWork;
            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();


                #region 红冲记账凭证

                _recordingVoucherService.CancleVoucher<PaymentReceiptBill, PaymentReceiptItem>(bill, () =>
                {

                    #region 修改单据付款状态

                    if (bill != null && bill.Items != null && bill.Items.Count > 0)
                    {

                        bill.Items.ToList().ForEach(a =>
                        {
                            //修改采购单 欠款金额
                            //if (a.BillTypeId == (int)BillTypeEnum.PurchaseBill)
                            //{
                            //    var purchaseBill = _purchaseService.GetPurchaseBillByNumber(0, a.BillNumber);
                            //    if (purchaseBill != null)
                            //    {
                            //        //欠款金额 += 本次优惠金额+本次付款金额
                            //        purchaseBill.OweCash += (a.DiscountAmountOnce ?? 0) + (a.ReceivableAmountOnce ?? 0);
                            //        _purchaseService.UpdatePurchaseBill(purchaseBill);
                            //    }
                            //}

                            //部分付款
                            if (a.PaymentedAmount != decimal.Zero) //可能为负数
                            {
                                //更新开单状态
                                switch (a.BillTypeEnum)
                                {
                                    case BillTypeEnum.PurchaseBill:
                                        _purchaseService.UpdatePaymented(bill.StoreId, a.BillId, PayStatus.Part);
                                        break;
                                    case BillTypeEnum.PurchaseReturnBill:
                                        _purchaseReturnService.UpdatePaymented(bill.StoreId, a.BillId, PayStatus.Part);
                                        break;
                                    case BillTypeEnum.FinancialIncomeBill:
                                        _financialIncomeBillService.UpdatePaymented(bill.StoreId, a.BillId, PayStatus.Part);
                                        break;
                                }
                            }
                            //未付款
                            else if (a.PaymentedAmount == decimal.Zero)
                            {
                                //更新开单状态
                                switch (a.BillTypeEnum)
                                {
                                    case BillTypeEnum.PurchaseBill:
                                        _purchaseService.UpdatePaymented(bill.StoreId, a.BillId, PayStatus.None);
                                        break;
                                    case BillTypeEnum.PurchaseReturnBill:
                                        _purchaseReturnService.UpdatePaymented(bill.StoreId, a.BillId, PayStatus.None);
                                        break;
                                    case BillTypeEnum.FinancialIncomeBill:
                                        _financialIncomeBillService.UpdatePaymented(bill.StoreId, a.BillId, PayStatus.None);
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
                    //UpdatePaymentReceiptBill(bill);
                    #endregion

                    bill.VoucherId = 0;
                    UpdatePaymentReceiptBill(bill);
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
        /// 验证单据是否已经付款
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="billTypeId"></param>
        /// <param name="billNumber"></param>
        /// <returns></returns>
        public Tuple<bool, string> CheckBillPaymentReceipt(int storeId, int billTypeId, string billNumber)
        {

            var query = from a in PaymentReceiptBillsRepository.Table
                        join b in PaymentReceiptItemsRepository.Table on a.Id equals b.PaymentReceiptBillId
                        where a.StoreId == storeId
                        && a.AuditedStatus == true
                        && a.ReversedStatus == false
                        && b.BillTypeId == billTypeId
                        && b.BillNumber == billNumber
                        select a.BillNumber;
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
        /// 获取待付款欠款单据
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="payeer"></param>
        /// <param name="manufacturerId"></param>
        /// <param name="billTypeId"></param>
        /// <param name="billNumber"></param>
        /// <param name="remark"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public IList<BillCashReceiptSummary> GetBillPaymentReceiptSummary(int storeId, int? payeer,
            int? manufacturerId,
            int? billTypeId,
            string billNumber = "",
            string remark = "",
            DateTime? startTime = null,
            DateTime? endTime = null)
        {
            billNumber = CommonHelper.FilterSQLChar(billNumber);
            remark = CommonHelper.FilterSQLChar(remark);

            var queryString = @"(SELECT 0 as Id,sb.StoreId,
                                sb.Id AS BillId,
                                sb.BillNumber AS BillNumber,
                                22 AS BillTypeId,'采购单' as BillTypeName,
                                sb.ManufacturerId AS CustomerId,
                                '' AS CustomerPointCode,
                                sb.BusinessUserId,
                                sb.CreatedOnUtc AS MakeBillDate,
                                sb.ReceivableAmount AS Amount,
                                sb.PreferentialAmount AS DiscountAmount,
                                0 AS PaymentedAmount,
                                (CASE sb.PayStatus WHEN 0 then sb.OweCash ELSE 0 END) AS ArrearsAmount,
                                sb.Remark AS Remark
                            FROM
                                dcms.PurchaseBills AS sb
                            WHERE
                                sb.StoreId = " + storeId + " AND sb.auditedStatus = 1 AND sb.ReversedStatus = 0 AND abs(sb.OweCash) > 0 AND (sb.PayStatus = 0 or sb.PayStatus = 1)";

            if (manufacturerId.HasValue && manufacturerId.Value > 0)
            {
                queryString += @" AND sb.ManufacturerId = " + manufacturerId + "";
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
                                24 AS BillTypeId,'采购退货单' as BillTypeName,
                                sb.ManufacturerId AS CustomerId,
                                '' AS CustomerPointCode,
                                sb.BusinessUserId,
                                sb.CreatedOnUtc AS MakeBillDate,
                                sb.ReceivableAmount AS Amount,
                                sb.PreferentialAmount AS DiscountAmount,
                                0 AS PaymentedAmount,
                                (CASE sb.PayStatus WHEN 0 then sb.OweCash ELSE 0 END) AS ArrearsAmount,
                                sb.Remark AS Remark
                            FROM
                                dcms.PurchaseReturnBills AS sb
                            WHERE
                                sb.StoreId = " + storeId + " AND sb.auditedStatus = 1 AND sb.ReversedStatus = 0 AND abs(sb.OweCash) > 0 AND (sb.PayStatus = 0 or sb.PayStatus = 1)";

            if (manufacturerId.HasValue && manufacturerId.Value > 0)
            {
                queryString += @" AND sb.ManufacturerId = " + manufacturerId + "";
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
                                47 AS BillTypeId,'其它收入' as BillTypeName,
                                sb.TerminalId AS CustomerId,
                                '' AS CustomerPointCode,
                                sb.SalesmanId AS BusinessUserId,
                                sb.CreatedOnUtc AS MakeBillDate,
                                sb.SumAmount AS Amount,
                                sb.DiscountAmount AS DiscountAmount,
                                0 AS PaymentedAmount,
                                (CASE sb.PayStatus WHEN 0 then sb.OweCash  ELSE 0 END) AS ArrearsAmount,
                                sb.Remark AS Remark
                            FROM
                                dcms.FinancialIncomeBills AS sb
                            WHERE
                                 sb.StoreId = " + storeId + " AND sb.auditedStatus = 1 AND sb.ReversedStatus = 0 AND abs(sb.OweCash) > 0 AND (sb.PayStatus = 0 or sb.PayStatus = 1)";

            if (manufacturerId.HasValue && manufacturerId.Value > 0)
            {
                queryString += @" AND sb.ManufacturerId = " + manufacturerId + "";
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

            var query = PaymentReceiptBillsRepository.QueryFromSql<BillCashReceiptSummary>(queryString).ToList();

            if (billTypeId.HasValue && billTypeId.Value > 0)
                query = query.Where(s => s.BillTypeId == billTypeId).ToList();

            if (!string.IsNullOrEmpty(billNumber))
                query = query.Where(s => s.BillNumber == billNumber).ToList();

            return query;
        }

        /// <summary>
        /// 判断指定单据是否还有欠款
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="billTypeId"></param>
        /// <param name="billId"></param>
        /// <returns></returns>
        public bool ThereAnyDebt(int storeId, int? billTypeId, int billId)
        {

            //采购单
            if (billTypeId == (int)BillTypeEnum.PurchaseBill)
            {
                return PurchaseBillsRepository.Table
                    .Where(s => s.StoreId == storeId && s.Id == billId && (s.PayStatus == 0 || s.PayStatus == 1))
                    .Count() > 0;
            }
            //采购退货单
            else if (billTypeId == (int)BillTypeEnum.PurchaseReturnBill)
            {
                return PurchaseReturnBillsRepository.Table
                    .Where(s => s.StoreId == storeId && s.Id == billId && (s.PayStatus == 0 || s.PayStatus == 1))
                    .Count() > 0;
            }
            //其它收入
            else if (billTypeId == (int)BillTypeEnum.FinancialIncomeBill)
            {
                return FinancialIncomeBillsRepository.Table.
                    Where(s => s.StoreId == storeId && s.Id == billId && (s.PayStatus == 0 || s.PayStatus == 1))
                    .Count() > 0;
            }
            else
            {
                return true;
            }
        }
    }
}
