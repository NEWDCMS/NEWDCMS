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
    /// 用于表示盘点任务(整仓)服务
    /// </summary>
    public partial class InventoryAllTaskBillService : BaseService, IInventoryAllTaskBillService
    {
        private readonly IUserService _userService;
        private readonly IQueuedMessageService _queuedMessageService;


        public InventoryAllTaskBillService(IServiceGetter getter,
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
            return InventoryAllTaskBillsRepository.TableNoTracking.Where(a => a.Id == billId).Count() > 0;
        }

        public virtual IPagedList<InventoryAllTaskBill> GetAllInventoryAllTaskBills(int? store, int? makeuserId, int? inventoryPerson, int? wareHouseId, string billNumber = "", bool? status = null, DateTime? start = null, DateTime? end = null, int? inventoryStatus = -1, bool? showReverse = null, bool? sortByCompletedTime = null, string remark = "", int pageIndex = 0, int pageSize = int.MaxValue)
        {
            try
            {
                var query = InventoryAllTaskBillsRepository.Table;


                DateTime.TryParse(start?.ToString("yyyy-MM-dd 00:00:00"), out DateTime startDate);
                DateTime.TryParse(end?.ToString("yyyy-MM-dd 23:59:59"), out DateTime endDate);

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

                if (inventoryStatus.HasValue && inventoryStatus != -1)
                {
                    query = query.Where(c => c.InventoryStatus == inventoryStatus);
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
                return new PagedList<InventoryAllTaskBill>(plists, pageIndex, pageSize, totalCount);
            }
            catch (Exception)
            {
                return null;
            }

        }

        public virtual IList<InventoryAllTaskBill> GetAllInventoryAllTaskBills()
        {
            var query = from c in InventoryAllTaskBillsRepository.Table
                        orderby c.Id
                        select c;
            return query.ToList();
        }

        public virtual IList<InventoryAllTaskBill> CheckInventory(int? store, int? inventoryPerson, int? wareHouseId)
        {
            var query = from c in InventoryAllTaskBillsRepository.Table
                        where c.StoreId == store && c.InventoryPerson == inventoryPerson && c.WareHouseId == wareHouseId && c.InventoryStatus == (int)InventorysetStatus.Pending
                        orderby c.Id
                        select c;
            return query.ToList();
        }

        public virtual IList<InventoryAllTaskBill> CheckInventory(int? store, int? inventoryPerson, int? wareHouseId, int? productId)
        {
            var query = from ptb in InventoryAllTaskBillsRepository.Table
                        join pti in InventoryAllTaskItemsRepository.Table
                        on ptb.Id equals pti.InventoryAllTaskBillId into ptb_pti
                        let pids = from p in ptb_pti.DefaultIfEmpty() select p.ProductId
                        where ptb.StoreId == store && ptb.InventoryPerson == inventoryPerson && ptb.WareHouseId == wareHouseId && ptb.InventoryStatus == (int)InventorysetStatus.Pending && pids.Contains(productId ?? 0)
                        orderby ptb.Id
                        select ptb;
            return query.ToList();
        }

        public virtual InventoryAllTaskBill GetInventoryAllTaskBillById(int? store, int inventoryAllTaskBillId)
        {
            if (inventoryAllTaskBillId == 0)
            {
                return null;
            }

            var key = DCMSDefaults.INVENTORYALLTASKBILL_BY_ID_KEY.FillCacheKey(store ?? 0, inventoryAllTaskBillId);
            return _cacheManager.Get(key, () =>
            {
                return InventoryAllTaskBillsRepository.ToCachedGetById(inventoryAllTaskBillId);
            });
        }

        public virtual InventoryAllTaskBill GetInventoryAllTaskBillById(int? store, int inventoryAllTaskBillId, bool isInclude = false)
        {
            if (inventoryAllTaskBillId == 0)
            {
                return null;
            }

            if (isInclude)
            {
                var query = InventoryAllTaskBillsRepository_RO.Table.Include(ia => ia.Items);
                return query.FirstOrDefault(s => s.Id == inventoryAllTaskBillId);
            }

            return InventoryAllTaskBillsRepository.ToCachedGetById(inventoryAllTaskBillId);
        }


        public virtual InventoryAllTaskBill GetInventoryAllTaskBillByNumber(int? store, string billNumber)
        {
            var key = DCMSDefaults.INVENTORYALLTASKBILL_BY_NUMBER_KEY.FillCacheKey(store ?? 0, billNumber);
            return _cacheManager.Get(key, () =>
            {
                var query = InventoryAllTaskBillsRepository.Table;
                var inventoryAllTaskBill = query.Where(a => a.BillNumber == billNumber).FirstOrDefault();
                return inventoryAllTaskBill;
            });
        }



        public virtual void InsertInventoryAllTaskBill(InventoryAllTaskBill inventoryAllTaskBill)
        {
            if (inventoryAllTaskBill == null)
            {
                throw new ArgumentNullException("inventoryAllTaskBill");
            }

            var uow = InventoryAllTaskBillsRepository.UnitOfWork;
            InventoryAllTaskBillsRepository.Insert(inventoryAllTaskBill);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(inventoryAllTaskBill);
        }

        public virtual void UpdateInventoryAllTaskBill(InventoryAllTaskBill inventoryAllTaskBill)
        {
            if (inventoryAllTaskBill == null)
            {
                throw new ArgumentNullException("inventoryAllTaskBill");
            }

            var uow = InventoryAllTaskBillsRepository.UnitOfWork;
            InventoryAllTaskBillsRepository.Update(inventoryAllTaskBill);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(inventoryAllTaskBill);
        }

        public virtual void DeleteInventoryAllTaskBill(InventoryAllTaskBill inventoryAllTaskBill)
        {
            if (inventoryAllTaskBill == null)
            {
                throw new ArgumentNullException("inventoryAllTaskBill");
            }

            var uow = InventoryAllTaskBillsRepository.UnitOfWork;
            InventoryAllTaskBillsRepository.Delete(inventoryAllTaskBill);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityDeleted(inventoryAllTaskBill);
        }


        #endregion

        #region 单据项目


        public virtual IPagedList<InventoryAllTaskItem> GetInventoryAllTaskItemsByInventoryAllTaskBillId(int inventoryAllTaskBillId, int? userId, int? storeId, int pageIndex, int pageSize)
        {
            if (inventoryAllTaskBillId == 0)
            {
                return new PagedList<InventoryAllTaskItem>(new List<InventoryAllTaskItem>(), pageIndex, pageSize);
            }

            var key = DCMSDefaults.INVENTORYALLTASKBILLITEM_ALL_KEY.FillCacheKey(storeId, inventoryAllTaskBillId, pageIndex, pageSize, userId);

            return _cacheManager.Get(key, () =>
            {
                var query = from pc in InventoryAllTaskItemsRepository.Table
                        where pc.InventoryAllTaskBillId == inventoryAllTaskBillId
                            orderby pc.Id
                            select pc;
                //var productInventoryAllTaskBills = new PagedList<InventoryAllTaskItem>(query.ToList(), pageIndex, pageSize);
                //return productInventoryAllTaskBills;
                //总页数
               
                var totalCount = query.Count();
                var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
                return new PagedList<InventoryAllTaskItem>(plists, pageIndex, pageSize, totalCount);
            });
        }

        public List<InventoryAllTaskItem> GetInventoryAllTaskItemList(int inventoryAllTaskBillId)
        {
            List<InventoryAllTaskItem> inventoryAllTaskItems = null;
            var query = InventoryAllTaskItemsRepository.Table;
            inventoryAllTaskItems = query.Where(a => a.InventoryAllTaskBillId == inventoryAllTaskBillId).ToList();
            return inventoryAllTaskItems;
        }

        public virtual InventoryAllTaskItem GetInventoryAllTaskItemById(int? store, int inventoryAllTaskItemId)
        {
            if (inventoryAllTaskItemId == 0)
            {
                return null;
            }
            return InventoryAllTaskItemsRepository.ToCachedGetById(inventoryAllTaskItemId);
        }

        public virtual void InsertInventoryAllTaskItem(InventoryAllTaskItem inventoryAllTaskItem)
        {
            if (inventoryAllTaskItem == null)
            {
                throw new ArgumentNullException("inventoryAllTaskItem");
            }

            var uow = InventoryAllTaskItemsRepository.UnitOfWork;
            InventoryAllTaskItemsRepository.Insert(inventoryAllTaskItem);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(inventoryAllTaskItem);
        }

        public virtual void UpdateInventoryAllTaskItem(InventoryAllTaskItem inventoryAllTaskItem)
        {
            if (inventoryAllTaskItem == null)
            {
                throw new ArgumentNullException("inventoryAllTaskItem");
            }

            var uow = InventoryAllTaskItemsRepository.UnitOfWork;
            InventoryAllTaskItemsRepository.Update(inventoryAllTaskItem);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(inventoryAllTaskItem);
        }

        public virtual void DeleteInventoryAllTaskItem(InventoryAllTaskItem inventoryAllTaskItem)
        {
            if (inventoryAllTaskItem == null)
            {
                throw new ArgumentNullException("inventoryAllTaskItem");
            }

            var uow = InventoryAllTaskItemsRepository.UnitOfWork;
            InventoryAllTaskItemsRepository.Delete(inventoryAllTaskItem);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(inventoryAllTaskItem);
        }


        #endregion

        public BaseResult BillCreateOrUpdate(int storeId, int userId, int? billId, InventoryAllTaskBill inventoryAllTaskBill, InventoryAllTaskBillUpdate data, List<InventoryAllTaskItem> items, out int inventoryAllTaskBillId, bool isAdmin = false)
        {
            var uow = InventoryAllTaskBillsRepository.UnitOfWork;

            ITransaction transaction = null;
            inventoryAllTaskBillId = 0;
            try
            {

                transaction = uow.BeginOrUseTransaction();



                //bool fg = true;
                string errMsg = string.Empty;

                if (!string.IsNullOrEmpty(errMsg))
                {
                    return new BaseResult { Success = false, Message = errMsg };
                }
                else
                {

                    if (billId.HasValue && billId.Value != 0)
                    {
                        #region 更新盘点单（整仓）

                        if (inventoryAllTaskBill != null)
                        {
                            inventoryAllTaskBill.InventoryPerson = data.InventoryPerson;
                            inventoryAllTaskBill.WareHouseId = data.WareHouseId;
                            inventoryAllTaskBill.InventoryDate = data.InventoryDate;
                            inventoryAllTaskBill.InventoryStatus = (int)InventorysetStatus.Pending;
                            UpdateInventoryAllTaskBill(inventoryAllTaskBill);
                        }

                        #endregion

                        inventoryAllTaskBillId = billId.Value;
                    }
                    else
                    {
                        #region 添加盘点单（整仓）

                        inventoryAllTaskBill.InventoryPerson = data.InventoryPerson;
                        inventoryAllTaskBill.WareHouseId = data.WareHouseId;
                        inventoryAllTaskBill.InventoryDate = DateTime.Now;
                        inventoryAllTaskBill.InventoryStatus = (int)InventorysetStatus.Pending;

                        inventoryAllTaskBill.StoreId = storeId;
                        //开单日期
                        inventoryAllTaskBill.CreatedOnUtc = DateTime.Now;
                        //单据编号
                        inventoryAllTaskBill.BillNumber = CommonHelper.GetBillNumber("PDD-ALL", storeId);
                        //制单人
                        inventoryAllTaskBill.MakeUserId = userId;
                        //状态(审核)
                        inventoryAllTaskBill.AuditedStatus = false;
                        inventoryAllTaskBill.AuditedDate = null;

                        //盘点状态
                        inventoryAllTaskBill.InventoryStatus = (int)InventorysetStatus.Pending;

                        inventoryAllTaskBill.Operation = data.Operation;//标识操作源

                        InsertInventoryAllTaskBill(inventoryAllTaskBill);

                        #endregion

                        inventoryAllTaskBillId = inventoryAllTaskBill.Id;
                    }

                    #region 更新盘点单（整仓）项目

                    data.Items.ForEach(p =>
                    {
                        if (p.ProductId != 0)
                        {
                            var sd = GetInventoryAllTaskItemById(storeId, p.Id);
                            if (sd == null)
                            {
                                //追加项
                                if (inventoryAllTaskBill.Items.Count(cp => cp.Id == p.Id) == 0)
                                {
                                    var item = p;
                                    item.StoreId = storeId;
                                    item.InventoryAllTaskBillId = inventoryAllTaskBill.Id;

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

                                    InsertInventoryAllTaskItem(item);
                                    //不排除
                                    p.Id = item.Id;
                                    if (!inventoryAllTaskBill.Items.Select(s => s.Id).Contains(item.Id))
                                    {
                                        inventoryAllTaskBill.Items.Add(item);
                                    }
                                }
                            }
                            else
                            {
                                //存在则更新
                                sd.InventoryAllTaskBillId = inventoryAllTaskBill.Id;
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

                                UpdateInventoryAllTaskItem(sd);
                            }
                        }
                    });

                    #endregion

                    #region Grid 移除则从库移除删除项

                    inventoryAllTaskBill.Items.ToList().ForEach(p =>
                    {
                        if (data.Items.Count(cp => cp.Id == p.Id) == 0)
                        {
                            inventoryAllTaskBill.Items.Remove(p);
                            var item = GetInventoryAllTaskItemById(storeId, p.Id);
                            if (item != null)
                            {
                                DeleteInventoryAllTaskItem(item);
                            }
                        }
                    });

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

        public BaseResult CancelTakeInventory(int storeId, int userId, InventoryAllTaskBill inventoryAllTaskBill)
        {
            var uow = InventoryAllTaskBillsRepository.UnitOfWork;

            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();



                #region 修改单据表状态
                inventoryAllTaskBill.InventoryStatus = (int)InventorysetStatus.Canceled;
                inventoryAllTaskBill.CompletedUserId = userId;
                inventoryAllTaskBill.CompletedDate = DateTime.Now;
                UpdateInventoryAllTaskBill(inventoryAllTaskBill);
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

        public BaseResult SetInventoryCompleted(int storeId, int userId, InventoryAllTaskBill inventoryAllTaskBill)
        {
            var uow = InventoryAllTaskBillsRepository.UnitOfWork;

            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();



                #region 修改单据表状态
                inventoryAllTaskBill.AuditedUserId = userId;
                inventoryAllTaskBill.AuditedDate = DateTime.Now;
                inventoryAllTaskBill.AuditedStatus = true;
                //完成盘点
                inventoryAllTaskBill.InventoryStatus = (int)InventorysetStatus.Completed;
                inventoryAllTaskBill.CompletedUserId = userId;
                inventoryAllTaskBill.CompletedDate = DateTime.Now;

                UpdateInventoryAllTaskBill(inventoryAllTaskBill);
                #endregion

                #region 发送通知
                try
                {
                    //获取当前用户管理员用户 电话号码 多个用"|"分隔
                    var adminMobileNumbers = _userService.GetAllAdminUserMobileNumbersByStore(storeId).ToList();
                    var queuedMessage = new QueuedMessage()
                    {
                        StoreId = storeId,
                        MType = MTypeEnum.InventoryCompleted,
                        Title = CommonHelper.GetEnumDescription<MTypeEnum>(MTypeEnum.InventoryCompleted),
                        Date = inventoryAllTaskBill.CreatedOnUtc,
                        BillType = BillTypeEnum.InventoryAllTaskBill,
                        BillNumber = inventoryAllTaskBill.BillNumber,
                        BillId = inventoryAllTaskBill.Id,
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
