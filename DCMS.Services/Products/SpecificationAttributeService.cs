using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Data;
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
    /// 表示规格属性服务
    /// </summary>
    public partial class SpecificationAttributeService : BaseService, ISpecificationAttributeService
    {
        

        public SpecificationAttributeService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
           
            IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {
            
        }


        #region 方法

        #region 规格属性

        /// <summary>
        /// 获取规格属性
        /// </summary>
        /// <param name="specificationAttributeId"></param>
        /// <returns></returns>
        public virtual SpecificationAttribute GetSpecificationAttributeById(int specificationAttributeId)
        {
            if (specificationAttributeId == 0)
            {
                return null;
            }

            return SpecificationAttributesRepository.ToCachedGetById(specificationAttributeId);
        }

        public virtual IList<int> GetpProductIds(int specificationAttributeId)
        {
            if (specificationAttributeId == 0)
            {
                return null;
            }
            var query = ProductsSpecificationAttributeMappingRepository_RO.Table.Where(a => a.SpecificationAttributeOptionId == specificationAttributeId).Select(s => s.Id).ToList();
            return query;
        }
        /// <summary>
        /// 获取规格属性
        /// </summary>
        /// <returns></returns>
        public virtual IList<SpecificationAttribute> GetSpecificationAttributes(string name)
        {
            //var query = from sa in _specificationAttributeRepository.Table
            //            orderby sa.DisplayOrder
            //            select sa;
            var query = SpecificationAttributesRepository.Table;
            if (string.IsNullOrWhiteSpace(name))
            {
                query.Where(x => x.Name.Contains(name));
            }

            var specificationAttributes = query.ToList();
            return specificationAttributes;
        }

        public virtual IList<SpecificationAttribute> GetSpecificationAttributesBtStore(int? store)
        {
            var query = from sa in SpecificationAttributesRepository.Table
                        where sa.StoreId == store.Value
                        orderby sa.DisplayOrder
                        select sa;

            var specificationAttributes = query.ToList();
            return specificationAttributes;
        }


        /// <summary>
        /// 删除规格属性
        /// </summary>
        /// <param name="specificationAttribute"></param>
        public virtual void DeleteSpecificationAttribute(SpecificationAttribute specificationAttribute)
        {
            if (specificationAttribute == null)
            {
                throw new ArgumentNullException("specificationAttribute");
            }

            var uow = SpecificationAttributesRepository.UnitOfWork;
            SpecificationAttributesRepository.Delete(specificationAttribute);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(specificationAttribute);
        }

        /// <summary>
        /// 添加规格属性
        /// </summary>
        /// <param name="specificationAttribute"></param>
        public virtual void InsertSpecificationAttribute(SpecificationAttribute specificationAttribute)
        {
            if (specificationAttribute == null)
            {
                throw new ArgumentNullException("specificationAttribute");
            }

            var uow = SpecificationAttributesRepository.UnitOfWork;
            SpecificationAttributesRepository.Insert(specificationAttribute);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(specificationAttribute);
        }

        /// <summary>
        /// 更新规格属性
        /// </summary>
        /// <param name="specificationAttribute"></param>
        public virtual void UpdateSpecificationAttribute(SpecificationAttribute specificationAttribute)
        {
            if (specificationAttribute == null)
            {
                throw new ArgumentNullException("specificationAttribute");
            }

            var uow = SpecificationAttributesRepository.UnitOfWork;
            SpecificationAttributesRepository.Update(specificationAttribute);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(specificationAttribute);
        }

        #endregion

        #region 规格属性项

        /// <summary>
        /// 获取规格属性项
        /// </summary>
        /// <param name="specificationAttributeOptionId"></param>
        /// <returns></returns>
        public virtual SpecificationAttributeOption GetSpecificationAttributeOptionById(int specificationAttributeOptionId)
        {
            if (specificationAttributeOptionId == 0)
            {
                return new SpecificationAttributeOption();
            }

            var spo = SpecificationAttributeOptionsRepository.ToCachedGetById(specificationAttributeOptionId);
            return spo == null ? new SpecificationAttributeOption() : spo;
        }


        public virtual IList<SpecificationAttributeOption> GetSpecificationAttributeOptionByIds(int? store, List<int> ids, bool platform = false)
        {
            if (ids == null || ids.Count == 0)
            {
                return new List<SpecificationAttributeOption>();
            }

            var query = from c in SpecificationAttributeOptionsRepository.Table
                        where c.StoreId == store && ids.Contains(c.Id)
                        select c;

            if (platform == true)
            {
                query = from c in SpecificationAttributeOptionsRepository_RO.TableNoTracking
                        where c.StoreId == store && ids.Contains(c.Id)
                        select c;
            }

            var specificationAttributeOptions = query.ToList();

            return specificationAttributeOptions;
        }


        public virtual string GetSpecificationAttributeOptionName(int? store, int specificationAttributeOptionId)
        {
            //var option = GetSpecificationAttributeOptionById(specificationAttributeOptionId);
            //return option != null ? option.Name : "";
            if (specificationAttributeOptionId == 0)
            {
                return "";
            }

            var key = DCMSDefaults.SPECIFICATIONATTRIBUTEOPTION_NAME_BY_ID_KEY.FillCacheKey(store ?? 0, specificationAttributeOptionId);
            return _cacheManager.Get(key, () =>
            {
                return SpecificationAttributeOptionsRepository.Table.Where(a => a.Id == specificationAttributeOptionId).Select(a => a.Name).FirstOrDefault();
            });
        }

        public virtual int GetSpecificationAttributeOptionId(int store, string specificationAttributeOptionName)
        {
            var query = SpecificationAttributeOptionsRepository.Table;

            if (string.IsNullOrWhiteSpace(specificationAttributeOptionName))
            {
                return 0;
            }
            return query.Where(s => s.Name == specificationAttributeOptionName && s.SpecificationAttribute.StoreId == store).Select(s => s.Id).FirstOrDefault();
        }

        /// <summary>
        /// 获取规格属性项
        /// </summary>
        /// <param name="specificationAttributeId"></param>
        /// <returns></returns>
        public virtual IList<SpecificationAttributeOption> GetSpecificationAttributeOptionsBySpecificationAttribute(int store, int specificationAttributeId)
        {
            var query = from sao in SpecificationAttributeOptionsRepository.Table
                        orderby sao.DisplayOrder
                        where sao.SpecificationAttributeId == specificationAttributeId && sao.StoreId == store
                        select sao;
            var specificationAttributeOptions = query.ToList();
            return specificationAttributeOptions;
        }

        public virtual IList<SpecificationAttributeOption> GetSpecificationAttributeOptionsBySpecificationAttribute(int specificationAttributeId)
        {
            var query = from sao in SpecificationAttributeOptionsRepository.Table
                        orderby sao.DisplayOrder
                        where sao.SpecificationAttributeId == specificationAttributeId
                        select sao;
            var specificationAttributeOptions = query.ToList();
            return specificationAttributeOptions;
        }


        /// <summary>
        /// 获取全部规格属性项
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        public virtual IList<SpecificationAttributeOption> GetSpecificationAttributeOptionsByStore(int? store)
        {
            var key = DCMSDefaults.GETSPECIFICATIONATTRIBUTEOPTIONSBYSTORE_BY_STORE_KEY.FillCacheKey(store);
            return _cacheManager.Get(key, () =>
            {
                try
                {
                    var query = from sa in SpecificationAttributesRepository.Table
                                where sa.StoreId == store
                                select sa;

                    //var attributes = query.ToList();

                    var query1 = from sa in query
                                 join sao in SpecificationAttributeOptionsRepository.Table on sa.Id equals sao.SpecificationAttributeId
                                 where sa.StoreId == store
                                 group sao by new
                                 {
                                     sao.Id,
                                     sao.SpecificationAttributeId,
                                     sao.Name
                                 } into pGroup
                                 orderby pGroup.Key.SpecificationAttributeId
                                 select pGroup.FirstOrDefault();

                    return query1.ToList();

                }
                catch (Exception)
                {
                    return new List<SpecificationAttributeOption>();
                }
            });
        }


        /// <summary>
        /// 删除规格属性项
        /// </summary>
        /// <param name="specificationAttributeOption"></param>
        public virtual void DeleteSpecificationAttributeOption(SpecificationAttributeOption specificationAttributeOption)
        {
            if (specificationAttributeOption == null)
            {
                throw new ArgumentNullException("specificationAttributeOption");
            }

            var uow = SpecificationAttributeOptionsRepository.UnitOfWork;
            SpecificationAttributeOptionsRepository.Delete(specificationAttributeOption);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(specificationAttributeOption);
        }

        /// <summary>
        /// 添加规格属性项
        /// </summary>
        /// <param name="specificationAttributeOption"></param>
        public virtual void InsertSpecificationAttributeOption(SpecificationAttributeOption specificationAttributeOption)
        {
            if (specificationAttributeOption == null)
            {
                throw new ArgumentNullException("specificationAttributeOption");
            }

            var uow = SpecificationAttributeOptionsRepository.UnitOfWork;
            SpecificationAttributeOptionsRepository.Insert(specificationAttributeOption);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(specificationAttributeOption);
        }

        /// <summary>
        /// 更新规格属性项
        /// </summary>
        /// <param name="specificationAttributeOption"></param>
        public virtual void UpdateSpecificationAttributeOption(SpecificationAttributeOption specificationAttributeOption)
        {
            if (specificationAttributeOption == null)
            {
                throw new ArgumentNullException("specificationAttributeOption");
            }

            var uow = SpecificationAttributeOptionsRepository.UnitOfWork;
            SpecificationAttributeOptionsRepository.Update(specificationAttributeOption);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(specificationAttributeOption);
        }

        #endregion

        #region 商品规格属性


        /// <summary>
        /// 删除商品规格属性
        /// </summary>
        /// <param name="productSpecificationAttribute"></param>
        public virtual void DeleteProductSpecificationAttribute(ProductSpecificationAttribute productSpecificationAttribute)
        {
            if (productSpecificationAttribute == null)
            {
                throw new ArgumentNullException("productSpecificationAttribute");
            }

            var uow = ProductsSpecificationAttributeMappingRepository.UnitOfWork;
            ProductsSpecificationAttributeMappingRepository.Delete(productSpecificationAttribute);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(productSpecificationAttribute);
        }

        /// <summary>
        /// 获取商品规格属性
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public virtual IList<ProductSpecificationAttribute> GetProductSpecificationAttributesByProductId(int? store, int productId)
        {
            return GetProductSpecificationAttributesByProductId(store, productId, null, null, true);
        }

        /// <summary>
        /// 获取当前指定的所有商品的所有规格属性
        /// </summary>
        /// <param name="productIds"></param>
        /// <returns></returns>
        public virtual IList<ProductSpecificationAttribute> GetAllProductSpecificationAttributesByProductIds(int? store, int[] productIds)
        {
            if (productIds == null || productIds.Count() == 0)
            {
                return new List<ProductSpecificationAttribute>();
            }

            var key = DCMSDefaults.PRODUCTSPECIFICATIONATTRIBUTE_ALLBYPRODUCTID_KEY.FillCacheKey(store ?? 0, string.Join("_", productIds.OrderBy(a => a)), null, null);
            return _cacheManager.Get(key, () =>
            {
                var query = ProductsSpecificationAttributeMappingRepository.Table;
                query = query.Where(psa => productIds.Contains(psa.ProductId));

                var productSpecificationAttributes = query.ToList();
                return productSpecificationAttributes;
            });
        }


        /// <summary>
        /// 获取商品规格属性
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="allowFiltering"></param>
        /// <param name="showOnProductPage"></param>
        /// <returns></returns>

        public virtual IList<ProductSpecificationAttribute> GetProductSpecificationAttributesByProductId(int? store, int productId,
            bool? allowFiltering, bool? showOnProductPage, bool noCache = false)
        {
            string allowFilteringCacheStr = "null";
            if (allowFiltering.HasValue)
            {
                allowFilteringCacheStr = allowFiltering.ToString();
            }

            string showOnProductPageCacheStr = "null";
            if (showOnProductPage.HasValue)
            {
                showOnProductPageCacheStr = showOnProductPage.ToString();
            }

            var query = ProductsSpecificationAttributeMappingRepository.TableNoTracking
               .Where(psa => psa.StoreId == store && psa.ProductId == productId);

            if (allowFiltering.HasValue)
            {
                query = query.Where(psa => psa.AllowFiltering == allowFiltering.Value);
            }

            if (showOnProductPage.HasValue)
            {
                query = query.Where(psa => psa.ShowOnProductPage == showOnProductPage.Value);
            }

            query = query.OrderBy(psa => psa.DisplayOrder);

            if (!noCache)
            {
                return query.Distinct().ToList();
            }
            else
            {
                var key = DCMSDefaults.PRODUCTSPECIFICATIONATTRIBUTE_ALLBYPRODUCTID_KEY.FillCacheKey(store, productId, allowFilteringCacheStr, showOnProductPageCacheStr);
                return _cacheManager.Get(key, () => query.ToList());
            }
        }

        /// <summary>
        /// 获取商品规格属性
        /// </summary>
        /// <param name="productSpecificationAttributeId"></param>
        /// <returns></returns>
        public virtual ProductSpecificationAttribute GetProductSpecificationAttributeById(int productSpecificationAttributeId)
        {
            if (productSpecificationAttributeId == 0)
            {
                return null;
            }

            return ProductsSpecificationAttributeMappingRepository.ToCachedGetById(productSpecificationAttributeId);
        }

        /// <summary>
        /// 添加商品规格属性
        /// </summary>
        /// <param name="productSpecificationAttribute"></param>
        public virtual void InsertProductSpecificationAttribute(ProductSpecificationAttribute productSpecificationAttribute)
        {
            if (productSpecificationAttribute == null)
            {
                throw new ArgumentNullException("productSpecificationAttribute");
            }

            var uow = ProductsSpecificationAttributeMappingRepository.UnitOfWork;
            ProductsSpecificationAttributeMappingRepository.Insert(productSpecificationAttribute);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(productSpecificationAttribute);
        }

        public virtual void InsertProductSpecificationAttribute(List<ProductSpecificationAttribute> productSpecificationAttribute)
        {
            if (productSpecificationAttribute == null)
            {
                throw new ArgumentNullException("productSpecificationAttribute");
            }

            var uow = ProductsSpecificationAttributeMappingRepository.UnitOfWork;

            ProductsSpecificationAttributeMappingRepository.Insert(productSpecificationAttribute);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(productSpecificationAttribute[0]);
        }

        public virtual void InsertProductSpecificationAttribute(IUnitOfWork uow, List<ProductSpecificationAttribute> productSpecificationAttribute)
        {
            if (productSpecificationAttribute == null)
            {
                throw new ArgumentNullException("productSpecificationAttribute");
            }

            //var uow = ProductsSpecificationAttributeMappingRepository.UnitOfWork;

            ProductsSpecificationAttributeMappingRepository.Insert(productSpecificationAttribute);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(productSpecificationAttribute[0]);
        }

        /// <summary>
        /// 更新商品规格属性
        /// </summary>
        /// <param name="productSpecificationAttribute"></param>
        public virtual void UpdateProductSpecificationAttribute(ProductSpecificationAttribute productSpecificationAttribute)
        {
            if (productSpecificationAttribute == null)
            {
                throw new ArgumentNullException("productSpecificationAttribute");
            }

            var uow = ProductsSpecificationAttributeMappingRepository.UnitOfWork;
            ProductsSpecificationAttributeMappingRepository.Update(productSpecificationAttribute);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(productSpecificationAttribute);
        }

        public virtual void updateProductSpecificationAttribute(List<ProductSpecificationAttribute> productSpecificationAttributes)
        {
            try
            {
                if (productSpecificationAttributes == null)
                {
                    throw new ArgumentNullException("productSpecificationAttribute");
                }

                var uow = ProductsSpecificationAttributeMappingRepository.UnitOfWork;

                ProductsSpecificationAttributeMappingRepository.Update(productSpecificationAttributes);
                uow.SaveChanges();

                //通知
                _eventPublisher.EntityInserted(productSpecificationAttributes[0]);
            }
            catch (Exception)
            {

            }

        }

        public ProductSpecificationAttribute GetProductSpecAttributeById(int productSpecificationAttributeId,int productId,int specificationAttributeOptionId)
        {
            if (productSpecificationAttributeId == 0)
            {
                return null;
            }

            return ProductsSpecificationAttributeMappingRepository.TableNoTracking.Where(w=>w.Id == productSpecificationAttributeId && w.ProductId == productId && w.SpecificationAttributeOptionId == specificationAttributeOptionId).FirstOrDefault();
        }
        #endregion

        #endregion
    }
}
