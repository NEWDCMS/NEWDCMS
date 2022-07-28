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
    /// ProductFlavorService
    /// </summary>
    public partial class ProductFlavorService : BaseService, IProductFlavorService
    {

        public ProductFlavorService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {
        }

        #region 

        /// <summary>
        /// 获取规格属性
        /// </summary>
        /// <param name="productFlavorId"></param>
        /// <returns></returns>
        public virtual ProductFlavor GetProductFlavorById(int productFlavorId)
        {
            if (productFlavorId == 0)
            {
                return null;
            }

            return ProductFlavorsRepository.ToCachedGetById(productFlavorId);
        }

        /// <summary>
        /// 获取规格属性
        /// </summary>
        /// <returns></returns>
        public virtual IList<ProductFlavor> GetProductFlavors()
        {
            var query = from sa in ProductFlavorsRepository.Table
                        orderby sa.Id
                        select sa;
            var productFlavors = query.ToList();
            return productFlavors;
        }

        public virtual IList<ProductFlavor> GetProductFlavorsByProductId(int? pid)
        {
            var query = from sa in ProductFlavorsRepository.Table
                        where sa.ProductId == pid.Value
                        orderby sa.Id
                        select sa;
            var productFlavors = query.ToList();
            return productFlavors;
        }

        public virtual IList<ProductFlavor> GetProductFlavorsByParentId(int? pid)
        {
            var query = from sa in ProductFlavorsRepository.Table
                        where sa.ParentId == pid.Value
                        orderby sa.Id
                        select sa;
            var productFlavors = query.ToList();
            return productFlavors;
        }

        /// <summary>
        /// 删除规格属性
        /// </summary>
        /// <param name="productFlavor"></param>
        public virtual void DeleteProductFlavor(ProductFlavor productFlavor)
        {
            if (productFlavor == null)
            {
                throw new ArgumentNullException("productFlavor");
            }

            var uow = ProductFlavorsRepository.UnitOfWork;
            ProductFlavorsRepository.Delete(productFlavor);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(productFlavor);
        }

        /// <summary>
        /// 添加规格属性
        /// </summary>
        /// <param name="productFlavor"></param>
        public virtual void InsertProductFlavor(ProductFlavor productFlavor)
        {
            if (productFlavor == null)
            {
                throw new ArgumentNullException("productFlavor");
            }

            var uow = ProductFlavorsRepository.UnitOfWork;
            ProductFlavorsRepository.Insert(productFlavor);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(productFlavor);
        }

        /// <summary>
        /// 更新规格属性
        /// </summary>
        /// <param name="productFlavor"></param>
        public virtual void UpdateProductFlavor(ProductFlavor productFlavor)
        {
            if (productFlavor == null)
            {
                throw new ArgumentNullException("productFlavor");
            }

            var uow = ProductFlavorsRepository.UnitOfWork;
            ProductFlavorsRepository.Update(productFlavor);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(productFlavor);
        }


        #endregion

        public virtual int GetProductId(int flavorId)
        {
            if (flavorId == 0)
            {
                return 0;
            }
            else
            {
                return ProductFlavorsRepository.Table.Where(p => p.Id == flavorId).Select(p => p.ProductId).FirstOrDefault();
            }
        }

        /// <summary>
        /// 根据口味获取商品Id
        /// </summary>
        /// <param name="flavorIds"></param>
        /// <returns></returns>
        public virtual IList<int> GetProductIds(int[] flavorIds)
        {
            if (flavorIds == null || flavorIds.Length == 0)
            {
                return new List<int>();
            }
            else
            {
                var query = from p in ProductFlavorsRepository.Table
                            where flavorIds.Contains(p.Id)
                            select p.ProductId;
                return query.ToList();
            }
        }

        public virtual IPagedList<ProductFlavor> GetProductFlavors(string key = null, int parentId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;
            var query = ProductFlavorsRepository.Table;
            if (parentId != 0)
            {
                query = query.Where(q => q.ParentId == parentId);
            }
            if (!string.IsNullOrWhiteSpace(key))
            {
                query = query.Where(c => c.Name.Contains(key));
            }

            query = query.OrderByDescending(c => c.Id);
            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<ProductFlavor>(plists, pageIndex, pageSize, totalCount);
        }

    }
}
