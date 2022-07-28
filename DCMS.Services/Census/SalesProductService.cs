using DCMS.Core.Caching;
using DCMS.Core.Domain.Census;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DCMS.Services.Census
{
    public class SalesProductService : BaseService, ISalesProductService
    {

        public SalesProductService(IServiceGetter getter,
            IEventPublisher eventPublisher,
            IStaticCacheManager cacheManager) : base(getter, cacheManager, eventPublisher)
        {

        }


        /// <summary>
        /// 获取传统/餐饮销售商品信息
        /// </summary>
        /// <param name="traditionId"></param>
        /// <param name="restaurantId"></param>
        /// <returns></returns>
        public virtual IList<SalesProduct> GetSalesProductsByRestaurantId(int restaurantId = 0)
        {
            var lists = new List<SalesProduct>();
            if (restaurantId != 0)
            {
                var query = SalesProductRepository.Table;
                query = query.Where(t => t.RestaurantId.Equals(restaurantId));
                query = query.OrderByDescending(c => c.UpdateDate);
                var saleReservationsProducts = query.ToList();
                lists = saleReservationsProducts;
            }
            else
            {
                lists = null;
            }
            return lists;
        }


        /// <summary>
        /// 获取传统/餐饮销售商品信息
        /// </summary>
        /// <param name="traditionId"></param>
        /// <returns></returns>
        public virtual IList<SalesProduct> GetSalesProductsByTraditionId(int traditionId = 0)
        {
            var query = SalesProductRepository.Table;
            if (traditionId != 0)
            {
                query = query.Where(t => t.TraditionId.Equals(traditionId));
                query = query.OrderByDescending(c => c.UpdateDate);
                var saleReservationsProducts = query.ToList();
                return saleReservationsProducts;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 添加销售商品
        /// </summary>
        /// <param name="saleReservationsProduct"></param>
        /// <returns></returns>
        public int Insert(SalesProduct saleReservationsProduct)
        {
            try
            {
                var uow = SalesProductRepository.UnitOfWork;
                if (saleReservationsProduct != null)
                {
                    SalesProductRepository.Insert(saleReservationsProduct);
                }
                uow.SaveChanges();
                return saleReservationsProduct.Id;
            }
            catch (Exception)
            {
                return 0;
            }
        }

    }
}
