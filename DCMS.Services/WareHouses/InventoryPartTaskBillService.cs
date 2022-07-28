using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Data;
using DCMS.Core.Domain.Tasks;
using DCMS.Core.Domain.WareHouses;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using DCMS.Services.Tasks;
using DCMS.Services.Users;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using DCMS.Services.Caching;

namespace DCMS.Services.WareHouses
{
    /// <summary>
    /// 用于表示盘点任务(部分)服务
    /// </summary>
    public partial class InventoryPartTaskBillService : BaseService, IInventoryPartTaskBillService
    {

        private readonly IUserService _userService;
        private readonly IQueuedMessageService _queuedMessageService;

        public InventoryPartTaskBillService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher,
            IUserService userService,
            IQueuedMessageService queuedMessageService
            ) : base(getter, cacheManager, eventPublisher)
        {
            _userService = userService;
            _queuedMessageService = queuedMessageService;
        }

        #region 单据

        public bool Exists(int billId)
        {
            return InventoryPartTaskBillsRepository.TableNoTracking.Where(a => a.Id == billId).Count() > 0;
        }

        public virtual IPagedList<InventoryPartTaskBill> GetAllInventoryPartTaskBills(int? store, int? makeuserId, int? inventoryPerson, int? wareHouseId, string billNumber = "", bool? status = null, DateTime? start = null, DateTime? end = null, int? inventoryStatus = -1, bool? showReverse = null, bool? sortByCompletedTime = null, string remark = "", bool? deleted = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;

            DateTime.TryParse(start?.ToString("yyyy-MM-dd 00:00:00"), out DateTime startDate);
            DateTime.TryParse(end?.ToString("yyyy-MM-dd 23:59:59"), out DateTime endDate);

            try
            {
                //var query = InventoryPartTaskBillsRepository.Table;
                var query = from ab in InventoryPartTaskBillsRepository.Table
                            .Include(a => a.Items) 
                            select ab;

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

                if (inventoryPerson.HasValue && inventoryPerson.Value > 0)
                {
                    query = query.Where(c => c.InventoryPerson == inventoryPerson);
                }

                if (wareHouseId.HasValue && wareHouseId.Value > 0)
                {
                    query = query.Where(c => c.WareHouseId == wareHouseId);
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

                if (inventoryStatus.HasValue && inventoryStatus > 0)
                {
                    query = query.Where(c => c.InventoryStatus == inventoryStatus);
                }

                if (deleted.HasValue)
                {
                    query = query.Where(c => c.Deleted == deleted);
                }

                //按完成时间排序
                if (sortByCompletedTime.HasValue && sortByCompletedTime.Value == true)
                {
                    query = query.OrderByDescending(c => c.CompletedDate);
                }
                //默认创建时间排序
                else
                {
                    query = query.OrderByDescending(c => c.CreatedOnUtc);
                }


                var totalCount = query.Count();
                var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
                return new PagedList<InventoryPartTaskBill>(plists, pageIndex, pageSize, totalCount);
            }
            catch (Exception)
            {
                return null;
            }

        }

        public virtual IList<InventoryPartTaskBill> GetAllInventoryPartTaskBills()
        {
            var query = from c in InventoryPartTaskBillsRepository.Table
                        orderby c.Id
                        select c;

            var categories = query.ToList();
            return categories;
        }

        public virtual IList<InventoryPartTaskBill> CheckInventory(int? store, int? inventoryPerson, int? wareHouseId)
        {
            var query = from c in InventoryPartTaskBillsRepository.Table
                        where c.StoreId == store && c.InventoryPerson == inventoryPerson && c.WareHouseId == wareHouseId && c.InventoryStatus == (int)InventorysetStatus.Pending
                        orderby c.Id
                        select c;
            return query.ToList();
        }


        public virtual IList<InventoryPartTaskBill> CheckInventory(int? store, int? inventoryPerson, int? wareHouseId, int? productId)
        {
            var query = from ptb in InventoryPartTaskBillsRepository.Table
                        join pti in InventoryPartTaskItemsRepository.Table
                        on ptb.Id equals pti.InventoryPartTaskBillId into ptb_pti
                        let pids = from p in ptb_pti.DefaultIfEmpty() select p.ProductId
                        where ptb.StoreId == store && ptb.InventoryPerson == inventoryPerson && ptb.WareHouseId == wareHouseId && ptb.InventoryStatus == (int)InventorysetStatus.Pending && pids.Contains(productId ?? 0)
                        orderby ptb.Id
                        select ptb;
            return query.ToList();
        }

        public virtual InventoryPartTaskBill GetInventoryPartTaskBillById(int? store, int inventoryPartTaskBillId)
        {
            if (inventoryPartTaskBillId == 0)
            {
                return null;
            }

            var key = DCMSDefaults.INVENTORYPARTTASKBILL_BY_ID_KEY.FillCacheKey(store ?? 0, inventoryPartTaskBillId);
            return _cacheManager.Get(key, () =>
            {
                return InventoryPartTaskBillsRepository.ToCachedGetById(inventoryPartTaskBillId);
            });
        }

        public virtual InventoryPartTaskBill GetInventoryPartTaskBillById(int? store, int inventoryPartTaskBillId, bool isInclude = false)
        {
            if (inventoryPartTaskBillId == 0)
            {
                return null;
            }

            if (isInclude)
            {
                var query = InventoryPartTaskBillsRepository_RO.Table.Include(ip => ip.Items);
                return query.FirstOrDefault(i => i.Id == inventoryPartTaskBillId);
            }
            return InventoryPartTaskBillsRepository.ToCachedGetById(inventoryPartTaskBillId);
        }

        public virtual InventoryPartTaskBill GetInventoryPartTaskBillByNumber(int? store, string billNumber)
        {
            var key = DCMSDefaults.INVENTORYPARTTASKBILL_BY_NUMBER_KEY.FillCacheKey(store ?? 0, billNumber);
            return _cacheManager.Get(key, () =>
            {
                var query = InventoryPartTaskBillsRepository.Table;
                var inventoryPartTaskBill = query.Where(a => a.BillNumber == billNumber).FirstOrDefault();
                return inventoryPartTaskBill;
            });
        }

        public virtual void InsertInventoryPartTaskBill(InventoryPartTaskBill inventoryPartTaskBill)
        {
            if (inventoryPartTaskBill == null)
            {
                throw new ArgumentNullException("inventoryPartTaskBill");
            }

            var uow = InventoryPartTaskBillsRepository.UnitOfWork;
            InventoryPartTaskBillsRepository.Insert(inventoryPartTaskBill);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(inventoryPartTaskBill);
        }

        public virtual void UpdateInventoryPartTaskBill(InventoryPartTaskBill inventoryPartTaskBill)
        {
            if (inventoryPartTaskBill == null)
            {
                throw new ArgumentNullException("inventoryPartTaskBill");
            }

            var uow = InventoryPartTaskBillsRepository.UnitOfWork;
            InventoryPartTaskBillsRepository.Update(inventoryPartTaskBill);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(inventoryPartTaskBill);
        }

        public virtual void DeleteInventoryPartTaskBill(InventoryPartTaskBill inventoryPartTaskBill)
        {
            if (inventoryPartTaskBill == null)
            {
                throw new ArgumentNullException("inventoryPartTaskBill");
            }

            var uow = InventoryPartTaskBillsRepository.UnitOfWork;
            InventoryPartTaskBillsRepository.Delete(inventoryPartTaskBill);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityDeleted(inventoryPartTaskBill);
        }


        #endregion

        #region 单据项目


        public virtual IPagedList<InventoryPartTaskItem> GetInventoryPartTaskItemsByInventoryPartTaskBillId(int inventoryPartTaskBillId, int? userId, int? storeId, int pageIndex, int pageSize)
        {
            if (pageSize >= 50)
                pageSize = 50;
            if (inventoryPartTaskBillId == 0)
            {
                return new PagedList<InventoryPartTaskItem>(new List<InventoryPartTaskItem>(), pageIndex, pageSize);
            }

            var key = DCMSDefaults.INVENTORYPARTTASKBILLITEM_ALL_KEY.FillCacheKey(storeId, inventoryPartTaskBillId, pageIndex, pageSize, userId);
            return _cacheManager.Get(key, () =>
            {
                var query = from pc in InventoryPartTaskItemsRepository.Table
                            where pc.InventoryPartTaskBillId == inventoryPartTaskBillId
                            orderby pc.Id
                            select pc;
                //var productInventoryPartTaskBills = new PagedList<InventoryPartTaskItem>(query.ToList(), pageIndex, pageSize);
                //return productInventoryPartTaskBills;
                //总页数
                var totalCount = query.Count();
                var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
                return new PagedList<InventoryPartTaskItem>(plists, pageIndex, pageSize, totalCount);
            });
        }

        public List<InventoryPartTaskItem> GetInventoryPartTaskItemList(int inventoryPartTaskBillId)
        {
            List<InventoryPartTaskItem> inventoryPartTaskItems = null;
            var query = InventoryPartTaskItemsRepository.Table;
            inventoryPartTaskItems = query.Where(a => a.InventoryPartTaskBillId == inventoryPartTaskBillId).ToList();
            return inventoryPartTaskItems;
        }

        public virtual InventoryPartTaskItem GetInventoryPartTaskItemById(int? store, int inventoryPartTaskItemId)
        {
            if (inventoryPartTaskItemId == 0)
            {
                return null;
            }

            return InventoryPartTaskItemsRepository.ToCachedGetById(inventoryPartTaskItemId);
        }

        public virtual void InsertInventoryPartTaskItem(InventoryPartTaskItem inventoryPartTaskItem)
        {
            if (inventoryPartTaskItem == null)
            {
                throw new ArgumentNullException("inventoryPartTaskItem");
            }

            var uow = InventoryPartTaskItemsRepository.UnitOfWork;
            InventoryPartTaskItemsRepository.Insert(inventoryPartTaskItem);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(inventoryPartTaskItem);
        }

        public virtual void UpdateInventoryPartTaskItem(InventoryPartTaskItem inventoryPartTaskItem)
        {
            if (inventoryPartTaskItem == null)
            {
                throw new ArgumentNullException("inventoryPartTaskItem");
            }

            var uow = InventoryPartTaskItemsRepository.UnitOfWork;
            InventoryPartTaskItemsRepository.Update(inventoryPartTaskItem);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(inventoryPartTaskItem);
        }

        public virtual void DeleteInventoryPartTaskItem(InventoryPartTaskItem inventoryPartTaskItem)
        {
            if (inventoryPartTaskItem == null)
            {
                throw new ArgumentNullException("inventoryPartTaskItem");
            }

            var uow = InventoryPartTaskItemsRepository.UnitOfWork;
            InventoryPartTaskItemsRepository.Delete(inventoryPartTaskItem);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(inventoryPartTaskItem);
        }


        #endregion

        public void UpdateInventoryPartTaskBillActive(int? store, int? billId, int? user)
        {
            var query = InventoryPartTaskBillsRepository.Table.ToList();

            query = query.Where(x => x.StoreId == store && x.MakeUserId == user && x.AuditedStatus == true && (DateTime.Now.Subtract(x.AuditedDate ?? DateTime.Now).Duration().TotalDays > 30)).ToList();

            if (billId.HasValue && billId.Value > 0)
            {
                query = query.Where(x => x.Id == billId).ToList();
            }

            var result = query;

            if (result != null && result.Count > 0)
            {
                var uow = InventoryPartTaskBillsRepository.UnitOfWork;
                foreach (InventoryPartTaskBill bill in result)
                {
                    if ((bill.AuditedStatus && !bill.ReversedStatus) || bill.Deleted) continue;
                    bill.Deleted = true;
                    InventoryPartTaskBillsRepository.Update(bill);
                }
                uow.SaveChanges();
            }

        }

        public BaseResult BillCreateOrUpdate(int storeId, int userId, int? billId, InventoryPartTaskBill inventoryPartTaskBill, InventoryPartTaskBillUpdate data, List<InventoryPartTaskItem> items, out int inventoryPartTaskBillId, bool isAdmin = false)
        {
            var uow = InventoryPartTaskBillsRepository.UnitOfWork;

            ITransaction transaction = null;
            inventoryPartTaskBillId = 0;
            try
            {
                transaction = uow.BeginOrUseTransaction();
                string errMsg = string.Empty;

                if (!string.IsNullOrEmpty(errMsg))
                {
                    return new BaseResult { Success = false, Message = errMsg };
                }
                else
                {
                    if (billId.HasValue && billId.Value != 0)
                    {
                        #region 更新盘点单（部分）
                        if (inventoryPartTaskBill != null)
                        {
                            inventoryPartTaskBill.InventoryPerson = data.InventoryPerson;
                            inventoryPartTaskBill.WareHouseId = data.WareHouseId;
                            inventoryPartTaskBill.InventoryDate = data.InventoryDate;
                            inventoryPartTaskBill.InventoryStatus = (int)InventorysetStatus.Pending;
                            UpdateInventoryPartTaskBill(inventoryPartTaskBill);
                        }

                        #endregion

                        inventoryPartTaskBillId = billId.Value;
                    }
                    else
                    {
                        #region 盘点单（部分） 13015384503

                        inventoryPartTaskBill.InventoryPerson = data.InventoryPerson;
                        inventoryPartTaskBill.WareHouseId = data.WareHouseId;
                        inventoryPartTaskBill.InventoryDate = DateTime.Now;
                        inventoryPartTaskBill.InventoryStatus = (int)InventorysetStatus.Pending;

                        inventoryPartTaskBill.StoreId = storeId;
                        //开单日期
                        inventoryPartTaskBill.CreatedOnUtc = DateTime.Now;
                        //单据编号
                        inventoryPartTaskBill.BillNumber = CommonHelper.GetBillNumber("PDD-PART", storeId);
                        //制单人
                        inventoryPartTaskBill.MakeUserId = userId;
                        //状态(审核)
                        inventoryPartTaskBill.AuditedStatus = false;
                        inventoryPartTaskBill.AuditedDate = null;
                        inventoryPartTaskBill.Operation = data.Operation;//标识操作源

                        //盘点状态
                        inventoryPartTaskBill.InventoryStatus = (int)InventorysetStatus.Pending;

                        InsertInventoryPartTaskBill(inventoryPartTaskBill);

                        #endregion

                        inventoryPartTaskBillId = inventoryPartTaskBill.Id;
                    }

                    #region 更新盘点单（部分）项目

                    data.Items.ForEach(p =>
                    {
                        if (p.ProductId != 0)
                        {
                            var sd = GetInventoryPartTaskItemById(storeId, p.Id);
                            if (sd == null)
                            {
                                //追加项
                                if (inventoryPartTaskBill.Items.Count(cp => cp.Id == p.Id) == 0)
                                {
                                    var item = p;
                                    item.StoreId = storeId;
                                    item.InventoryPartTaskBillId = inventoryPartTaskBill.Id;

                                    item.CurrentStock = p.CurrentStock ?? 0;
                                    item.BigUnitQuantity = p.BigUnitQuantity ?? 0;
                                    item.AmongUnitQuantity = p.AmongUnitQuantity ?? 0;
                                    item.SmallUnitQuantity = p.SmallUnitQuantity ?? 0;
                                    item.VolumeQuantity = p.VolumeQuantity ?? 0;
                                    item.LossesQuantity = p.LossesQuantity ?? 0;
                                    item.VolumeWholesaleAmount = p.VolumeWholesaleAmount ?? 0;
                                    item.LossesWholesaleAmount = p.LossesWholesaleAmount ?? 0;
                                    item.VolumeCostAmount = p.VolumeCostAmount ?? 0;
                                    item.LossesCostAmount = p.LossesCostAmount ?? 0;

                                    item.CreatedOnUtc = DateTime.Now;

                                    InsertInventoryPartTaskItem(item);
                                    //不排除
                                    p.Id = item.Id;
                                    if (!inventoryPartTaskBill.Items.Select(s => s.Id).Contains(item.Id))
                                    {
                                        inventoryPartTaskBill.Items.Add(item);
                                    }
                                }
                            }
                            else
                            {
                                //存在则更新
                                sd.InventoryPartTaskBillId = inventoryPartTaskBill.Id;
                                sd.ProductId = p.ProductId;
                                sd.UnitId = p.UnitId;

                                sd.CurrentStock = p.CurrentStock ?? 0;
                                sd.BigUnitQuantity = p.BigUnitQuantity ?? 0;
                                sd.AmongUnitQuantity = p.AmongUnitQuantity ?? 0;
                                sd.SmallUnitQuantity = p.SmallUnitQuantity ?? 0;
                                sd.VolumeQuantity = p.VolumeQuantity ?? 0;
                                sd.LossesQuantity = p.LossesQuantity ?? 0;
                                sd.VolumeWholesaleAmount = p.VolumeWholesaleAmount ?? 0;
                                sd.LossesWholesaleAmount = p.LossesWholesaleAmount ?? 0;
                                sd.VolumeCostAmount = p.VolumeCostAmount ?? 0;
                                sd.LossesCostAmount = p.LossesCostAmount ?? 0;

                                UpdateInventoryPartTaskItem(sd);
                            }
                        }
                    });

                    #endregion

                    #region Grid 移除则从库移除删除项

                    inventoryPartTaskBill.Items.ToList().ForEach(p =>
                    {
                        if (data.Items.Count(cp => cp.Id == p.Id) == 0)
                        {
                            inventoryPartTaskBill.Items.Remove(p);
                            var item = GetInventoryPartTaskItemById(storeId, p.Id);
                            if (item != null)
                            {
                                DeleteInventoryPartTaskItem(item);
                            }
                        }

                    });

                    #endregion
                }

                //保存事务
                transaction.Commit();

                return new BaseResult { Success = true, Return = inventoryPartTaskBillId, Message = Resources.Bill_CreateOrUpdateSuccessful };
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

        public BaseResult CancelTakeInventory(int storeId, int userId, InventoryPartTaskBill inventoryPartTaskBill)
        {
            var uow = InventoryPartTaskBillsRepository.UnitOfWork;

            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();



                #region 修改单据表状态
                inventoryPartTaskBill.InventoryStatus = (int)InventorysetStatus.Canceled;
                inventoryPartTaskBill.CompletedUserId = userId;
                inventoryPartTaskBill.CompletedDate = DateTime.Now;
                UpdateInventoryPartTaskBill(inventoryPartTaskBill);
                #endregion


                //保存事务
                transaction.Commit();

                return new BaseResult { Success = true, Message = "取消盘点成功" };
            }
            catch (Exception)
            {
                //如果事务不存在或者为控则回滚
                transaction?.Rollback();
                return new BaseResult { Success = false, Message = "取消盘点失败" };
            }
            finally
            {
                //不管怎样最后都会关闭掉这个事务
                using (transaction) { }
            }
        }
        public BaseResult SetInventoryCompleted(int storeId, int userId, InventoryPartTaskBill inventoryPartTaskBill)
        {
            var uow = InventoryPartTaskBillsRepository.UnitOfWork;

            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();



                #region 修改单据表状态
                inventoryPartTaskBill.AuditedUserId = userId;
                inventoryPartTaskBill.AuditedDate = DateTime.Now;
                inventoryPartTaskBill.AuditedStatus = true;
                //完成盘点
                inventoryPartTaskBill.InventoryStatus = (int)InventorysetStatus.Completed;
                inventoryPartTaskBill.CompletedUserId = userId;
                inventoryPartTaskBill.CompletedDate = DateTime.Now;

                UpdateInventoryPartTaskBill(inventoryPartTaskBill);
                #endregion

                #region 发送通知
                try
                {
                    //获取当前用户管理员用户 电话号码 多个用"|"分隔
                    var adminMobileNumbers = _userService.GetAllUserMobileNumbersByUserIds(new List<int> { userId }).ToList();
                    var queuedMessage = new QueuedMessage()
                    {
                        StoreId = storeId,
                        MType = MTypeEnum.InventoryCompleted,
                        Title = CommonHelper.GetEnumDescription<MTypeEnum>(MTypeEnum.InventoryCompleted),
                        Date = inventoryPartTaskBill.CreatedOnUtc,
                        BillType = BillTypeEnum.InventoryPartTaskBill,
                        BillNumber = inventoryPartTaskBill.BillNumber,
                        BillId = inventoryPartTaskBill.Id,
                        CreatedOnUtc = DateTime.Now
                    };
                    _queuedMessageService.InsertQueuedMessage(adminMobileNumbers,queuedMessage);
                }
                catch (Exception ex)
                {
                    _queuedMessageService.WriteLogs(ex.Message);
                }
                #endregion


                //保存事务
                transaction.Commit();

                return new BaseResult { Success = true, Message = "完成盘点成功" };
            }
            catch (Exception)
            {
                //如果事务不存在或者为控则回滚
                transaction?.Rollback();
                return new BaseResult { Success = false, Message = "完成盘点失败" };
            }
            finally
            {
                //不管怎样最后都会关闭掉这个事务
                using (transaction) { }
            }
        }
    }
}
