using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Common;
using DCMS.Core.Domain.Products;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using DCMS.Services.Caching;

namespace DCMS.Services.Products
{
    /// <summary>
    /// 渠道服务
    /// </summary>
    public partial class ManufacturerService : BaseService, IManufacturerService
    {
        #region 构造
        public ManufacturerService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {

        }
        #endregion

        #region 方法


        public virtual IList<Manufacturer> GetAllManufacturers(int? storeId)
        {
            return _cacheManager.Get(DCMSDefaults.MANUFACTURER_ALL_STORE_KEY.FillCacheKey(storeId), () =>
           {
               var query = from c in ManufacturerRepository.Table
                           orderby c.Name, c.CreatedOnUtc
                           where !c.Deleted && c.StoreId == storeId
                           select c;

               var terminalList = query.ToList();
               return terminalList;
           });
        }

        /// <summary>
        /// 绑定供应商信息
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public virtual IList<Manufacturer> BindManufacturerList(int? storeId)
        {
            return _cacheManager.Get(DCMSDefaults.BINDMANUFACTURER_ALL_STORE_KEY.FillCacheKey(storeId), () =>
           {
               var query = from c in ManufacturerRepository.Table
                           orderby c.Name, c.CreatedOnUtc
                           where !c.Deleted && c.StoreId == storeId
                           select c;
               var result = query.Select(q => new { Id = q.Id, Name = q.Name }).ToList().Select(x => new Manufacturer { Id = x.Id, Name = x.Name }).ToList();
               return result;
           });
        }

        /// <summary>
        /// 分页获取供应商信息
        /// </summary>
        /// <param name="searchStr"></param>
        /// <param name="totalCount"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual IPagedList<Manufacturer> GetAllManufactureies(string searchStr, int? storeId, int pageIndex, int pageSize)
        {
            if (pageSize >= 50)
                pageSize = 50;
            var query = from p in ManufacturerRepository.Table
                        orderby p.Name, p.CreatedOnUtc
                        where !p.Deleted && p.StoreId == storeId
                        select p;

            if (!string.IsNullOrEmpty(searchStr))
            {
                query = query.Where(t => t.Name.Contains(searchStr));
            }

            //return new PagedList<Manufacturer>(query.ToList(), pageIndex, pageSize);

            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<Manufacturer>(plists, pageIndex, pageSize, totalCount);

        }


        /// <summary>
        /// 根据主键Id获取供应商信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual Manufacturer GetManufacturerById(int? store, int id)
        {
            if (id == 0)
            {
                return null;
            }

            return ManufacturerRepository.ToCachedGetById(id);
        }


        public virtual string GetManufacturerName(int? store, int id)
        {
            //var manufacturer = GetManufacturerById(id);
            //return manufacturer != null ? manufacturer.Name : "";
            if (id == 0)
            {
                return "";
            }

            var key = DCMSDefaults.MANUFACTURER_NAME_BY_ID_KEY.FillCacheKey(store ?? 0, id);
            return _cacheManager.Get(key, () =>
            {
                return ManufacturerRepository.Table.Where(a => a.Id == id).Select(a => a.Name).FirstOrDefault();
            });
        }


        public virtual IList<Manufacturer> GetManufacturersByIds(int? store, int[] idArr)
        {
            if (idArr == null || idArr.Length == 0)
            {
                return new List<Manufacturer>();
            }

            var key = DCMSDefaults.MANUFACTURER_BY_IDS_KEY.FillCacheKey(store ?? 0, string.Join("_", idArr.OrderBy(a => a)));
            return _cacheManager.Get(key, () =>
            {

                var query = from c in ManufacturerRepository.Table
                            where idArr.Contains(c.Id)
                            select c;
                var list = query.ToList();
                return list;
            });

        }

        public virtual Dictionary<int, string> GetManufacturerDictsByIds(int storeId, int[] ids)
        {
            var categoryIds = new Dictionary<int, string>();
            if (ids.Count() > 0)
            {
                categoryIds = ManufacturerRepository.QueryFromSql<DictType>($"SELECT Id,Name as Name FROM dcms.Manufacturer where StoreId = " + storeId + " and id in(" + string.Join(",", ids) + ");").ToDictionary(k => k.Id, v => v.Name);
            }
            return categoryIds;
        }

        /// <summary>
        /// 新增供应商信息
        /// </summary>
        /// <param name="manufacturer"></param>
        public virtual void InsertManufacturer(Manufacturer manufacturer)
        {
            if (manufacturer == null)
            {
                throw new ArgumentNullException("manufacturer");
            }

            var uow = ManufacturerRepository.UnitOfWork;
            ManufacturerRepository.Insert(manufacturer);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(manufacturer);
        }
        /// <summary>
        /// 删除供应商
        /// </summary>
        /// <param name="manufacturer"></param>
        public virtual void DeleteManufacturer(Manufacturer manufacturer)
        {
            if (manufacturer == null)
            {
                throw new ArgumentNullException("manufacturer");
            }

            var uow = ManufacturerRepository.UnitOfWork;
            ManufacturerRepository.Delete(manufacturer);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(manufacturer);
        }
        /// <summary>
        /// 修改供应商
        /// </summary>
        /// <param name="manufacturer"></param>
        public virtual void UpdateManufacturer(Manufacturer manufacturer)
        {
            if (manufacturer == null)
            {
                throw new ArgumentNullException("manufacturer");
            }

            var uow = ManufacturerRepository.UnitOfWork;
            ManufacturerRepository.Update(manufacturer);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(manufacturer);
        }

        /// <summary>
        /// 根据供应商Id获取该供应商的所有采购单信息
        /// </summary>
        /// <param name="districtId"></param>
        /// <returns></returns>
        public virtual IList<int> GetManufacturerId(int ManufacturerId)
        {
            if (ManufacturerId == 0)
            {
                return new List<int>();
            }

            var query = from c in PurchaseBillsRepository.Table
                        where c.ManufacturerId == ManufacturerId
                        select c.Id;
            return query.ToList();
        }
        public virtual int ManufacturerByName(int store, string name)
        {
            var query = ManufacturerRepository.Table;
            if (string.IsNullOrWhiteSpace(name))
            {
                return 0;
            }

            return query.Where(s => s.StoreId == store && s.Name == name).Select(s => s.Id).FirstOrDefault();
        }
        #endregion
    }
}
