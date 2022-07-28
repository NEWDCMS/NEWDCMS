using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.WareHouses;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using DCMS.Services.Caching;
using Newtonsoft.Json;


namespace DCMS.Services.WareHouses
{
    /// <summary>
    /// 仓库服务
    /// </summary>
    public class WareHouseService : BaseService, IWareHouseService
    {
        
        public WareHouseService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
           
            IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {
            
        }

        /// <summary>
        /// 获取所有仓库数据
        /// </summary>
        /// <returns></returns>
        public virtual IList<WareHouse> GetWareHouseList(int? storeId)
        {
            return _cacheManager.Get(DCMSDefaults.WAREHOUSE_ALL_STORE_KEY.FillCacheKey(storeId), () =>
           {
               var query = from c in WareHousesRepository.Table
                           where !c.Deleted
                           select c;
               if (storeId != null)
               {
                   query = query.Where(c => c.StoreId == storeId);
               }

               return query.ToList();
           });
        }

        /// <summary>
        /// 绑定仓库信息
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public virtual IList<WareHouse> BindWareHouseList(int? storeId, WHAEnum? wHA, int userId = 0)
        {
            var query = from c in WareHousesRepository_RO.Table
                        where !c.Deleted
                        select c;

            if (storeId != null)
                query = query.Where(c => c.StoreId == storeId);

            var result = query.Select(q => new
            {
                q.Id,
                q.Name,
                q.WareHouseAccessSettings,
                q.AllowNegativeInventory,
                q.AllowNegativeInventoryPreSale
            })
            .ToList()
            .Select(x => new WareHouse
            {
                Id = x.Id,
                Name = x.Name,
                AllowNegativeInventory = x.AllowNegativeInventory,
                AllowNegativeInventoryPreSale = x.AllowNegativeInventoryPreSale,
                WareHouseAccessSettings = x.WareHouseAccessSettings
            }).ToList();

            result.ForEach(s =>
            {
                s.WareHouseAccess = JsonConvert.DeserializeObject<List<WareHouseAccess>>(s.WareHouseAccessSettings);
            });

            var ws = new List<WareHouse>();
            if (wHA != null && (int)wHA > 0 && userId > 0)
            {
                result.ForEach(s =>
                {

                    var bs = s.WareHouseAccess?.Where(b => b.UserId == userId).Select(b => b.BillTypes).FirstOrDefault();
                    if (bs?.Where(bts => bts.Selected)?.Select(bts => bts.BillTypeId).Contains((int)wHA) ?? false)
                    {
                        ws.Add(s);
                    }

                });
                return ws;
            }
            else if (wHA == null && userId > 0)
            {
                result.ForEach(s =>
                {
                    var bs = s.WareHouseAccess?.Where(b => b.UserId == userId && b.StockQuery == true).FirstOrDefault();
                    if (bs != null)
                    {
                        ws.Add(s);
                    }
                });
                return ws;
            }
            else
            {
                return result;
            }
        }
        public virtual IList<WareHouse> BindWareHouseList(int? storeId, int? type, WHAEnum? wHA, int userId = 0)
        {
            return _cacheManager.Get(DCMSDefaults.BINDWAREHOUSE_ALL_STORE_KEY_TYPE.FillCacheKey(storeId, type), () =>
            {
                 var query = from c in WareHousesRepository.Table
                             where !c.Deleted
                             select c;
                 if (storeId != null)
                 {
                     query = query.Where(c => c.StoreId == storeId);
                 }

                 //仓库、车辆
                 if (type == (int)WareHouseType.Normal || type == (int)WareHouseType.Car)
                 {
                     query = query.Where(c => c.Type == type);
                 }

                 var result = query.Select(q => new
                 {
                     q.Id,
                     q.Name,
                     q.WareHouseAccessSettings,
                     q.AllowNegativeInventory,
                     q.AllowNegativeInventoryPreSale
                 })
                   .ToList()
                   .Select(x => new WareHouse
                   {
                       Id = x.Id,
                       Name = x.Name,
                       AllowNegativeInventory = x.AllowNegativeInventory,
                       AllowNegativeInventoryPreSale = x.AllowNegativeInventoryPreSale,
                       WareHouseAccessSettings = x.WareHouseAccessSettings
                   }).ToList();

                 result.ForEach(s =>
                 {
                     s.WareHouseAccess = JsonConvert.DeserializeObject<List<WareHouseAccess>>(s.WareHouseAccessSettings);
                 });

                var ws = new List<WareHouse>();
                if (wHA != null && userId > 0)
                {
                    result.ForEach(s =>
                    {
                        var bs = s.WareHouseAccess?.Where(b => b.UserId == userId).Select(b => b.BillTypes).FirstOrDefault();
                        if (bs?.Where(bts => bts.Selected)?.Select(bts => bts.BillTypeId).Contains((int)wHA) ?? false)
                        {
                            ws.Add(s);
                        }
                    });

                    return ws;
                }
                else if (wHA == null && userId > 0)
                {
                    result.ForEach(s =>
                    {
                        var bs = s.WareHouseAccess?.Where(b => b.UserId == userId && b.StockQuery == true).FirstOrDefault();
                        if (bs != null)
                        {
                            ws.Add(s);
                        }
                    });
                    return ws;
                }
                else
                {
                    return result;
                }
            });
        }

        /// <summary>
        /// 分页获取仓库数据
        /// </summary>
        /// <param name="searchStr"></param>
        /// <param name="totalCount"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual IPagedList<WareHouse> GetWareHouseList(string searchStr, int? storeId, int? type, int pageIndex, int pageSize)
        {
            if (pageSize >= 50)
                pageSize = 50;
            var query = from p in WareHousesRepository.Table
                        orderby p.CreatedOnUtc descending
                        where !p.Deleted
                        select p;
            if (!string.IsNullOrEmpty(searchStr))
            {
                query = query.Where(t => t.Name.Contains(searchStr));
            }

            if (storeId != null)
            {
                query = query.Where(c => c.StoreId == storeId);
            }

            //仓库、车辆
            if (type == (int)WareHouseType.Normal || type == (int)WareHouseType.Car)
            {
                query = query.Where(c => c.Type == type);
            }

            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<WareHouse>(plists, pageIndex, pageSize, totalCount);
        }
        /// <summary>
        /// 根据主键Id获取仓库
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual WareHouse GetWareHouseById(int? store, int id)
        {
            if (id == 0)
                return null;

            return WareHousesRepository.GetById(id);
        }


        public virtual string GetWareHouseName(int? store, int id)
        {
            //var horse = GetWareHouseById(id);
            //return horse != null ? horse.Name : "";
            if (id == 0)
            {
                return "";
            }

            var key = DCMSDefaults.WAREHOUSE_NAME_BY_ID_KEY.FillCacheKey(store ?? 0, id);
            return _cacheManager.Get(key, () =>
            {
                return WareHousesRepository_RO.TableNoTracking.FirstOrDefault(a => a.Id == id)?.Name;
            });
        }

        public virtual WareHouse GetWareHouseByName(int store, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            return WareHousesRepository.Table.FirstOrDefault(a => a.StoreId == store && a.Name == name);
        }

        /// <summary>
        /// 添加仓库
        /// </summary>
        /// <param name="wareHouse"></param>
        public virtual void InsertWareHouse(WareHouse wareHouse)
        {


            if (wareHouse == null)
            {
                throw new ArgumentNullException("wareHouse");
            }

            var uow = WareHousesRepository.UnitOfWork;
            WareHousesRepository.Insert(wareHouse);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(wareHouse);
        }
        /// <summary>
        /// 删除仓库
        /// </summary>
        /// <param name="wareHouse"></param>
        public virtual void DeleteWareHouse(WareHouse wareHouse)
        {
            if (wareHouse == null)
            {
                throw new ArgumentNullException("wareHouse");
            }

            var uow = WareHousesRepository.UnitOfWork;
            WareHousesRepository.Delete(wareHouse);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(wareHouse);
        }
        /// <summary>
        /// 修改仓库
        /// </summary>
        /// <param name="wareHouse"></param>
        public virtual void UpdateWareHouse(WareHouse wareHouse)
        {
            if (wareHouse == null)
            {
                throw new ArgumentNullException("wareHouse");
            }

            var uow = WareHousesRepository.UnitOfWork;

            WareHousesRepository.Update(wareHouse);

            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(wareHouse);
        }

        public virtual IList<WareHouse> GetWareHouseByIds(int? store, int[] idArr, bool platform = false)
        {
            if (idArr == null || idArr.Length == 0)
            {
                return new List<WareHouse>();
            }
            try
            {
                var key = DCMSDefaults.WAREHOUSE_BY_IDS_KEY.FillCacheKey(store ?? 0, string.Join("_", idArr.OrderBy(a => a)));
                return _cacheManager.Get(key, () =>
                {
                    var query = from c in WareHousesRepository.Table
                                where idArr.Contains(c.Id)
                                select c;
                    if (platform == true)
                    {
                        query = from c in WareHousesRepository_RO.TableNoTracking
                                where idArr.Contains(c.Id)
                                select c;
                    }
                    return query.ToList();
                });
            }
            catch (Exception)
            {
                return new List<WareHouse>();
            }
        }

        public virtual IList<WareHouse> GetWareHouseIdsByWareHouseIds(int[] idArr)
        {
            if (idArr == null || idArr.Length == 0)
            {
                return new List<WareHouse>();
            }

            var key = DCMSDefaults.WAREHOUSE_BY_IDS_KEY.FillCacheKey(string.Join("_", idArr.OrderBy(a => a)));
            return _cacheManager.Get(key, () =>
            {
                var query = from c in WareHousesRepository_RO.TableNoTracking
                            where idArr.Contains(c.Id)
                            select new WareHouse { Id = c.Id, Name = c.Name };
                var list = query.ToList();
                return list;
            });

        }


        public virtual IList<WareHouse> GetWareHouseIdsBytype(int store, int type = 2)
        {
            var query = (from dis in DispatchBillsRepository.Table
                         join
 disitem in DispatchItemsRepository.Table on dis.Id equals disitem.DispatchBillId
                         where disitem.SignStatus != 1 && dis.StoreId == store
                         select new { dis.CarId }).ToList().Distinct();
            List<int> arrint = query.ToList().ConvertAll<int>(x => x.CarId);
            return WareHousesRepository.Table.Where(a => a.StoreId == store && a.Type == 2 && !arrint.Contains(a.Id)).ToList();
        }
        /// <summary>
        /// 验证当前商品是否正在盘点
        /// </summary>
        /// <param name="storeId">经销商Id</param>
        /// <param name="wareHouseId">仓库Id</param>
        /// <param name="productIds">商品Ids</param>
        /// <param name="errMsg">错误信息</param>
        /// <returns></returns>
        public bool CheckProductInventory(int storeId, int wareHouseId, int[] productIds, out string errMsg)
        {

            errMsg = string.Empty;
            List<string> list = new List<string>();
            //盘点单 整仓
            var query1 = from a in InventoryAllTaskBillsRepository.Table
                         join b in InventoryAllTaskItemsRepository.Table on a.Id equals b.InventoryAllTaskBillId
                         join c in ProductsRepository.Table on b.ProductId equals c.Id
                         where a.StoreId == storeId
                         && a.InventoryStatus == (int)InventorysetStatus.Pending
                         && productIds.Contains(b.ProductId)
                         select new { a.WareHouseId, c.Name };
            if (wareHouseId > 0)
            {
                list.AddRange(query1.Where(q => q.WareHouseId == wareHouseId).Select(q => q.Name).ToList());
            }
            else
            {
                list.AddRange(query1.Select(q => q.Name).ToList());
            }
            //盘点单 部分
            var query2 = from a in InventoryPartTaskBillsRepository.Table
                         join b in InventoryPartTaskItemsRepository.Table on a.Id equals b.InventoryPartTaskBillId
                         join c in ProductsRepository.Table on b.ProductId equals c.Id
                         where a.StoreId == storeId
                         && a.InventoryStatus == (int)InventorysetStatus.Pending
                         && productIds.Contains(b.ProductId)
                         && ((wareHouseId > 0) ? a.WareHouseId == wareHouseId : 1 == 1)
                         select new { a.WareHouseId, c.Name };
            if (wareHouseId > 0)
            {
                list.AddRange(query2.Where(q => q.WareHouseId == wareHouseId).Select(q => q.Name).ToList());
            }
            else
            {
                list.AddRange(query2.Select(q => q.Name).ToList());
            }

            list = list.Distinct().ToList();
            if (list != null && list.Count > 0)
            {
                errMsg = "商品：" + string.Join(",", list) + ".正在盘点中";
            }

            return list.Count > 0;

        }
    }
}
