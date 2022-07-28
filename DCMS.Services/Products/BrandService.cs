using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Products;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using DCMS.Services.Caching;

namespace DCMS.Services.Products
{

    public partial class BrandService : BaseService, IBrandService
    {
        

        public BrandService(IStaticCacheManager cacheManager,
            IServiceGetter getter,
           
            IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {
            
        }



        /// <summary>
        ///  删除
        /// </summary>
        /// <param name="brands"></param>
        public virtual void DeleteBrand(Brand brands)
        {
            if (brands == null)
            {
                throw new ArgumentNullException("brands");
            }

            if (!brands.IsPreset)
            {
                var uow = BrandsRepository.UnitOfWork;
                BrandsRepository.Delete(brands);
                uow.SaveChanges();
            }

            //event notification
            _eventPublisher.EntityDeleted(brands);
        }

        /// <summary>
        /// 获取全部品牌
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual IPagedList<Brand> GetAllBrands(int? store, string name = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;
            var key = DCMSDefaults.BRAND_GETALLBRANDS_BY_KEY.FillCacheKey(store, name, pageIndex, pageSize);
            var plists = _cacheManager.Get(key, () =>
             {
                 var query = BrandsRepository.Table;

                 if (!string.IsNullOrWhiteSpace(name))
                 {
                     query = query.Where(c => c.Name.Contains(name));
                 }

                 if (store.HasValue && store.Value != 0)
                 {
                     query = query.Where(c => c.StoreId == store);
                 }

                 query = query.OrderByDescending(c => c.CreatedOnUtc);
                 return query.ToList();
             });
            return new PagedList<Brand>(plists, pageIndex, pageSize);
        }


        /// <summary>
        /// 获取全部
        /// </summary>
        /// <returns></returns>
        public virtual IList<Brand> GetAllBrands(int? store)
        {
            var key = DCMSDefaults.BRAND_ALL_KEY.FillCacheKey(store);
            return _cacheManager.Get(key, () =>
            {
                try
                {
                    var query = from s in BrandsRepository_RO.TableNoTracking
                                where s.StoreId == store.Value
                                select s;
                    query = query.OrderBy(c => c.CreatedOnUtc).ThenBy(c => c.Name);

                    return query.ToList();
                }
                catch (Exception)
                {
                    return null;
                }
            });
        }

        public virtual IList<Brand> BindBrandList(int? store)
        {
            var key = DCMSDefaults.BINDBRAND_ALL_STORE_KEY.FillCacheKey(store);
            return _cacheManager.Get(key, () =>
            {
                var query = from s in BrandsRepository.Table
                            where s.StoreId == store.Value
                            orderby s.CreatedOnUtc, s.Name
                            select s;
                var result = query.Select(q => new { q.Id, q.Name })
                .ToList()
                .Select(x => new Brand { Id = x.Id, Name = x.Name }).ToList();
                return result;
            });
        }


        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="brandsId"></param>
        /// <returns></returns>
        public virtual Brand GetBrandById(int? store, int brandsId)
        {
            if (brandsId == 0)
            {
                return null;
            }

            return BrandsRepository.ToCachedGetById(brandsId);
        }


        public virtual string GetBrandName(int? store, int brandId)
        {
            //var brand = GetBrandById(brandId);
            //return brand == null ? "" : brand.Name;
            if (brandId == 0)
            {
                return "";
            }

            var key = DCMSDefaults.BRAND_NAME_BY_ID_KEY.FillCacheKey(store ?? 0, brandId);
            return _cacheManager.Get(key, () =>
            {
                return BrandsRepository.Table.Where(a => a.Id == brandId).Select(b => b.Name).FirstOrDefault();
            });
        }


        public virtual int GetBrandId(int store, string brandName)
        {
            var query = BrandsRepository.Table;

            if (string.IsNullOrWhiteSpace(brandName))
            {
                return 0;
            }

            return query.Where(s => s.StoreId == store && s.Name == brandName).Select(s => s.Id).FirstOrDefault();
        }


        public virtual IList<Brand> GetBrandsByIds(int? store, int[] idArr)
        {
            if (idArr == null || idArr.Length == 0)
            {
                return new List<Brand>();
            }

            var key = DCMSDefaults.BRAND_BY_IDS_KEY.FillCacheKey(store ?? 0, string.Join("_", idArr.OrderBy(a => a)));
            return _cacheManager.Get(key, () =>
            {

                var query = from c in BrandsRepository.Table
                            where idArr.Contains(c.Id)
                            select c;
                var list = query.ToList();
                return list;
            });
        }

        public virtual IList<Brand> GetBrandsByBrandIds(int? store, int[] ids)
        {

            if (ids == null || ids.Length == 0)
            {
                return new List<Brand>();
            }

            var key = DCMSDefaults.BRANDS_BY_STORE_IDS_KEY.FillCacheKey(store, string.Join("_", ids.OrderBy(a => a)));
            return _cacheManager.Get(key, () =>
            {

                var query = from c in BrandsRepository.Table
                            where c.StoreId == store &&
                             ids.Contains(c.Id)
                            select c;
                var categories = query.ToList();
                return categories;
            });

        }



        public virtual Dictionary<int, string> GetAllBrandsNames(int? store, int[] ids)
        {
            var categories = BrandsRepository_RO.TableNoTracking
                        .Where(c => c.StoreId == store && ids.Contains(c.Id))
                        .ToDictionary(k => k.Id, v => v.Name);
            return categories;
        }




        public virtual IList<Brand> GetBrandsIdsByBrandIds(int? store, int[] ids)
        {

            if (ids == null || ids.Length == 0)
            {
                return new List<Brand>();
            }

            var key = DCMSDefaults.BRANDS_NOTRACK_BY_STORE_IDS_KEY.FillCacheKey(store, string.Join("_", ids.OrderBy(a => a)));
            return _cacheManager.Get(key, () =>
            {

                var query = from c in BrandsRepository_RO.TableNoTracking
                            where c.StoreId == store &&
                             ids.Contains(c.Id)
                            select new Brand { Id = c.Id, Name = c.Name };
                var categories = query.ToList();
                return categories;
            });

        }


        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="brands"></param>
        public virtual void InsertBrand(Brand brands)
        {
            if (brands == null)
            {
                throw new ArgumentNullException("brands");
            }

            var uow = BrandsRepository.UnitOfWork;
            BrandsRepository.Insert(brands);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityInserted(brands);
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="brands"></param>
        public virtual void UpdateBrand(Brand brands)
        {
            if (brands == null)
            {
                throw new ArgumentNullException("brands");
            }

            var uow = BrandsRepository.UnitOfWork;
            BrandsRepository.Update(brands);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityUpdated(brands);
        }

    }
}