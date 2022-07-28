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

    /// <summary>
    /// 自定义价格方案
    /// </summary>
    public partial class ProductTierPricePlanService : BaseService, IProductTierPricePlanService
    {

        #region 构造
        public ProductTierPricePlanService(IStaticCacheManager cacheManager,
            IServiceGetter getter,
            IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {

        }

        #endregion

        #region 方法


        /// <summary>
        /// 获取经销商所有价格方案
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        public virtual IList<ProductPricePlan> GetAllPricePlan(int? store)
        {
            //获取价格方案
            var plans = GetAllProductTierPricePlans(store).Select(p =>
            {
                return new ProductPricePlan()
                {
                    StoreId = p.StoreId,
                    Name = p.Name,
                    PricesPlanId = p.Id,
                    PriceTypeId = 88,
                };
            }).ToList();

            var values = Enum.GetValues(typeof(PriceType)).Cast<PriceType>();
            foreach (var item in values)
            {
                var value = ((int)Enum.Parse(typeof(PriceType), item.ToString()));
                var otherName = item.ToString();
                if (value == 88)
                {
                    break;
                }

                plans.Add(new ProductPricePlan()
                {

                    StoreId = store.Value,
                    Name = CommonHelper.GetEnumDescription<PriceType>(item),
                    PricesPlanId = 0,
                    PriceTypeId = (byte)value
                });
            }
            return plans.OrderBy(p => p.PriceTypeId).ToList();
        }


        /// <summary>
        ///  删除
        /// </summary>
        /// <param name="productTierPricePlans"></param>
        public virtual void DeleteProductTierPricePlan(ProductTierPricePlan productTierPricePlans)
        {
            if (productTierPricePlans == null)
            {
                throw new ArgumentNullException("productTierPricePlans");
            }

            var uow = ProductTierPricePlansRepository.UnitOfWork;
            ProductTierPricePlansRepository.Delete(productTierPricePlans);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityDeleted(productTierPricePlans);
        }

        /// <summary>
        /// 获取全部
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual IPagedList<ProductTierPricePlan> GetAllProductTierPricePlans(string name = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;
            var query = ProductTierPricePlansRepository.Table;

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(c => c.Name.Contains(name));
            }

            query = query.Where(c => c.Name.Contains(name));

            query = query.OrderByDescending(c => c.Name);
            //var users = new PagedList<ProductTierPricePlan>(query.ToList(), pageIndex, pageSize);
            //return users;

            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<ProductTierPricePlan>(plists, pageIndex, pageSize, totalCount);

        }


        /// <summary>
        /// 获取全部
        /// </summary>
        /// <returns></returns>
        public virtual IList<ProductTierPricePlan> GetAllProductTierPricePlans(int? store)
        {
            var key = DCMSDefaults.PRICEPLAN_ALL_KEY.FillCacheKey(store ?? 0);
            return _cacheManager.Get(key, () =>
            {
                var query = from s in ProductTierPricePlansRepository.Table
                            where s.StoreId == store.Value
                            orderby s.Name
                            select s;
                var productTierPricePlan = query.ToList();
                return productTierPricePlan;
            });
        }

        /// <summary>
        /// 获取全部
        /// </summary>
        /// <returns></returns>
        public virtual IList<ProductTierPricePlan> GetAllProductTierPricePlans()
        {
            var key = DCMSDefaults.PRICEPLAN_ALL_KEY.FillCacheKey(0);
            return _cacheManager.Get(key, () =>
            {
                var query = from s in ProductTierPricePlansRepository.Table
                            orderby s.Name
                            select s;
                var productTierPricePlan = query.ToList();
                return productTierPricePlan;
            });
        }

        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="productTierPricePlansId"></param>
        /// <returns></returns>
        public virtual ProductTierPricePlan GetProductTierPricePlanById(int? store, int productTierPricePlansId)
        {
            if (productTierPricePlansId == 0)
            {
                return null;
            }

            return ProductTierPricePlansRepository.ToCachedGetById(productTierPricePlansId);
        }


        public virtual IList<ProductTierPricePlan> GetProductTierPricePlansByIds(int[] sIds)
        {
            if (sIds == null || sIds.Length == 0)
            {
                return new List<ProductTierPricePlan>();
            }

            var query = from c in ProductTierPricePlansRepository.Table
                        where sIds.Contains(c.Id)
                        select c;
            var productTierPricePlan = query.ToList();

            var sortedProductTierPricePlans = new List<ProductTierPricePlan>();
            foreach (int id in sIds)
            {
                var productTierPricePlans = productTierPricePlan.Find(x => x.Id == id);
                if (productTierPricePlans != null)
                {
                    sortedProductTierPricePlans.Add(productTierPricePlans);
                }
            }
            return sortedProductTierPricePlans;
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="productTierPricePlans"></param>
        public virtual void InsertProductTierPricePlan(ProductTierPricePlan productTierPricePlans)
        {
            if (productTierPricePlans == null)
            {
                throw new ArgumentNullException("productTierPricePlans");
            }

            var uow = ProductTierPricePlansRepository.UnitOfWork;
            ProductTierPricePlansRepository.Insert(productTierPricePlans);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityInserted(productTierPricePlans);
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="productTierPricePlans"></param>
        public virtual void UpdateProductTierPricePlan(ProductTierPricePlan productTierPricePlans)
        {
            if (productTierPricePlans == null)
            {
                throw new ArgumentNullException("productTierPricePlans");
            }

            var uow = ProductTierPricePlansRepository.UnitOfWork;
            ProductTierPricePlansRepository.Update(productTierPricePlans);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityUpdated(productTierPricePlans);
        }

        /// <summary>
        /// 获取全部
        /// </summary>
        /// <returns></returns>
        public virtual IList<ProductTierPrice> GetProductTierPricePlans(int priceplanid)
        {
            var key = DCMSDefaults.PRICEPLAN_ALL_KEY;
            return _cacheManager.Get(key, () =>
            {
                var query = from s in ProductTierPricesRepository.Table
                            where s.PricesPlanId == priceplanid
                            select s;
                var productTierPricePlan = query.ToList();
                return productTierPricePlan;
            });
        }

        public virtual int ProductTierPricePlansId(int store, string Name)
        {
            var query = ProductTierPricePlansRepository.Table;

            if (string.IsNullOrWhiteSpace(Name))
            {
                return 0;
            }

            return query.Where(s => s.StoreId == store && s.Name == Name).Select(s => s.Id).FirstOrDefault();
        }
        #endregion
    }
}