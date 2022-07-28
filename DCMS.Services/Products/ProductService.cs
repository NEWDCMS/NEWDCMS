using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Data;
using DCMS.Core.Domain.Common;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.WareHouses;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Core.Infrastructure.Mapper;
using DCMS.Services.Events;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbF = Microsoft.EntityFrameworkCore.EF;
using DCMS.Services.Caching;

namespace DCMS.Services.Products
{
    /// <summary>
    /// 产品服务
    /// </summary>
    public partial class ProductService : BaseService, IProductService
    {

        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        
        private readonly IProductFlavorService _productFlavorService;


        public ProductService(
            IServiceGetter getter,
            IStaticCacheManager cacheManager,
           
            IProductAttributeService productAttributeService,
            IProductAttributeParser productAttributeParser,
            ISpecificationAttributeService specificationAttributeService,
            IEventPublisher eventPublisher,
            IProductFlavorService productFlavorService
         ) : base(getter, cacheManager, eventPublisher)
        {
            _productAttributeService = productAttributeService;
            _productAttributeParser = productAttributeParser;
            _specificationAttributeService = specificationAttributeService;
            
            _productFlavorService = productFlavorService;
        }


        #region 商品管理

        /// <summary>
        /// 验证商品是否开单
        /// </summary>
        /// <param name="districtId"></param>
        /// <returns></returns>
        public virtual IList<int> GetHasSoldProductIds(int storId, int ProductId)
        {
            if (ProductId == 0)
            {
                return new List<int>();
            }
            var query = (from sa in ProductsRepository.Table
                         where sa.StoreId == storId && sa.Id == ProductId && sa.HasSold == true
                         select sa.Id).ToList();

            return query;
        }

        /// <summary>
        /// 根据终商品分类获取商品IDS
        /// </summary>
        /// <param name="districtId"></param>
        /// <returns></returns>
        public virtual IList<int> GetProductIds(int Categoryid)
        {
            if (Categoryid == 0)
            {
                return new List<int>();
            }
            var query = from sa in ProductsRepository.Table
                        where sa.CategoryId == Categoryid
                        orderby sa.Id
                        select sa.Id;
            var productids = query.ToList();
            return productids;
        }

        /// <summary>
        /// 根据品牌ID获取商品IDS
        /// </summary>
        /// <param name="districtId"></param>
        /// <returns></returns>
        public virtual IList<int> GetProductBrandIds(int Brandid)
        {
            if (Brandid == 0)
            {
                return new List<int>();
            }
            var query = from sa in ProductsRepository.Table
                        where sa.BrandId == Brandid
                        orderby sa.Id
                        select sa.Id;
            var productids = query.ToList();
            return productids;
        }

        /// <summary>
        /// 根据统计类别ID获取商品IDS
        /// </summary>
        /// <param name="districtId"></param>
        /// <returns></returns>
        public virtual IList<int> GetStatisticalTypeid(int StatisticalTypeid)
        {
            if (StatisticalTypeid == 0)
            {
                return new List<int>();
            }
            var query = from sa in ProductsRepository.Table
                        where sa.StatisticalType == StatisticalTypeid
                        select sa.Id;
            var productids = query.ToList();
            return productids;
        }


        /// <summary>
        /// 删除商品
        /// </summary>
        public virtual void DeleteProduct(Product product)
        {
            if (product == null)
            {
                throw new ArgumentNullException("product");
            }

            product.Deleted = true;
            UpdateProduct(product);
        }

        /// <summary>
        /// 获取显示商品
        /// </summary>
        public virtual IList<Product> GetAllProductsDisplayed(int pagesize = 0)
        {

            var query = from p in ProductsRepository.Table
                        orderby p.UpdatedOnUtc, p.Name
                        where p.Published &&
                        !p.Deleted
                        select p;

            if (pagesize == 0)
            {
                pagesize = query.Count();
            }

            var products = query.Take(pagesize).ToList();
            return products;
        }

        /// <summary>
        /// 获取商品
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public virtual Product GetProductById(int? store, int productId)
        {
            if (productId == 0)
            {
                return null;
            }
            return ProductsRepository.TableNoTracking.FirstOrDefault(p => p.StoreId == store && p.Id == productId);
        }

        public virtual IList<Product> GetProductByIds(int? store, int[] productIds)
        {
            return ProductsRepository.TableNoTracking.Where(p => p.StoreId == store &&  productIds.Contains(p.Id)).ToList();
        }


        public virtual string GetProductName(int? store, int productId)
        {
            var product = GetProductById(store, productId);
            return product != null ? product.Name : "";
        }

        /// <summary>
        /// 根据商品名称获取终端Ids
        /// </summary>
        /// <param name="productName"></param>
        /// <returns></returns>
        public virtual IList<int> GetProductIds(int? store, string productName)
        {
            if (string.IsNullOrEmpty(productName))
            {
                return new List<int>();
            }
            var key = DCMSDefaults.PRODUCTS_IDS_BY_NAME_KEY.FillCacheKey(store ?? 0, productName);
            return _cacheManager.Get(key, () =>
            {
                return ProductsRepository.Table.Where(a => a.Name.Contains(productName)).Select(a => a.Id).ToList();
            });

        }

        /// <summary>
        /// 获取商品标识
        /// </summary>
        /// <param name="productIds"></param>
        /// <returns></returns>
        public virtual IList<Product> GetProductsByIds(int storeId, int[] productIds, bool platform = false)
        {
            try
            {
                if (productIds == null || productIds.Length == 0)
                {
                    return new List<Product>();
                }

                var query = from p in ProductsRepository.Table
                            where p.StoreId == storeId && productIds.Contains(p.Id)
                            orderby p.Id
                            select p;

                var products = query.ToList();

                if (products.Any())
                {
                    products.ForEach(p => 
                    {
                        p.Name = string.IsNullOrEmpty(p.MnemonicCode) ? p.Name : p.MnemonicCode;
                    });
                }
                return products;

            }
            catch (Exception)
            {
                return null;
            }

        }


        public virtual IList<Product> GetProductsByCatagoryId(int? store, int catagoryId)
        {
            var key = DCMSDefaults.PRODUCTS_BY_CATAGORYID_KEY.FillCacheKey(store ?? 0, catagoryId);
            return _cacheManager.Get(key, () =>
            {
                var query = from p in ProductsRepository.Table
                            where p.CategoryId == catagoryId
                            select p;
                var products = query.ToList();
                return products;
            });
        }

        public virtual Product GetProductByName(int store, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            return ProductsRepository.TableNoTracking.FirstOrDefault(p => p.StoreId == store && p.Name == name);
        }

        /// <summary>
        /// 添加商品
        /// </summary>
        /// <param name="product"></param>
        public virtual void InsertProduct(Product product)
        {
            if (product == null)
            {
                throw new ArgumentNullException("product");
            }

            var uow = ProductsRepository.UnitOfWork;

            ProductsRepository.Insert(product);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(product);
        }

        public virtual void InsertProduct(IUnitOfWork uow, Product product)
        {
            if (product == null)
            {
                throw new ArgumentNullException("product");
            }

            ProductsRepository.Insert(product);

            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(product);
        }

        /// <summary>
        /// 批量插入
        /// </summary>
        /// <param name="products"></param>
        public virtual void InsertProducts(List<Product> products)
        {
            if (products == null)
            {
                throw new ArgumentNullException("product");
            }

            var uow = ProductsRepository.UnitOfWork;

            ProductsRepository.Insert(products);
            uow.SaveChanges();

            //通知
            products.ForEach(s => { _eventPublisher.EntityInserted(s); });
        }

        /// <summary>
        /// 修改商品
        /// </summary>
        /// <param name="product"></param>
        public virtual void UpdateProduct(Product product)
        {
            if (product == null)
            {
                throw new ArgumentNullException("product");
            }

            var uow = ProductsRepository.UnitOfWork;
            ProductsRepository.Update(product);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(product);
        }


        /// <summary>
        /// 修改商品
        /// </summary>
        /// <param name="product"></param>
        public virtual void UpdateProducts(List<Product> products)
        {
            if ((products?.Count ?? 0) > 0)
            {
                var uow = ProductsRepository.UnitOfWork;
                ProductsRepository.Update(products);
                uow.SaveChanges();
                products.ForEach(s =>
                {
                    _eventPublisher.EntityUpdated(s);
                });
            }
        }



        /// <summary>
        /// 搜索商品
        /// </summary>
        /// <param name="storeId">经销商</param>
        /// <param name="includes">排除商品</param>
        /// <param name="categoryIds">检索类别</param>
        /// <param name="usablequantity">是否库存量限制条件</param>
        /// <param name="published">是否发布</param>
        /// <param name="hassold">是否开单</param>
        /// <param name="includeCategories">是否包含类别记录</param>
        /// <param name="includeManufacturers">是否包含供应商记录</param>
        /// <param name="includeSpecificationAttributes">是否包含规格属性记录</param>
        /// <param name="includeVariantAttributes">是否包含变体记录</param>
        /// <param name="includeVariantAttributeCombinations">是否包含组合记录</param>
        /// <param name="includePrices">是否包含价格记录</param>
        /// <param name="includeTierPrices">是否包含层次价格记录</param>
        /// <param name="includePictures">是否包含图片记录</param>
        /// <param name="includeStocks">是否包含库存记录</param>
        /// <param name="includeFlavor">是否包含口味记录</param>
        /// <param name="wareHouseId">仓库</param>
        /// <param name="key">搜索关键字</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual IPagedList<Product> SearchProducts(int? storeId,
            int[] includes,
            int[] categoryIds,
            bool? usablequantity,
            bool? published,
            bool? hassold,
            bool includeCategories = true,
            bool includeManufacturers = true,
            bool includeSpecificationAttributes = true,
            bool includeVariantAttributes = true,
            bool includeVariantAttributeCombinations = true,
            bool includePrices = true,
            bool includeTierPrices = true,
            bool includePictures = true,
            bool includeStocks = true,
            bool includeFlavor = true,
            bool includeBrand = true,
            //
            int wareHouseId = 0,
            string key = "",
            int productStatus = 2,
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            int brandId=0)
        {
            if (pageSize >= 50)
                pageSize = 50;
            List<ProductCategory> allCategories = null;
            List<ProductManufacturer> allManufacturers = null;
            List<ProductSpecificationAttribute> allSpecificationAttributes = null;
            List<ProductVariantAttribute> allVariantAttributes = null;
            List<ProductVariantAttributeCombination> allVariantAttributeCombinations = null;
            List<ProductPrice> allPrices = null;
            List<ProductTierPrice> allTierPrices = null;
            List<ProductPicture> allPictures = null;
            List<ProductFlavor> allFlavors = null;
            List<Stock> allStocks = null;
            List<Brand> allBrands = null;

            key = CommonHelper.FilterSQLChar(key);

            #region Where

            try
            {
                string queryString = @"select distinct {0}
	                            from dcms.Products  as p  
	                            left join dcms.Categories as c on  p.CategoryId = c.Id
	                            left join dcms.Stocks as st on p.Id = st.ProductId
                            where p.Deleted = 0";

                if (storeId.HasValue && storeId.Value > 0)
                {
                    queryString += @" and p.StoreId = " + storeId + " ";
                }

                if (hassold.HasValue)
                {
                    queryString += @" and p.HasSold = " + hassold + " ";
                }

                if (published.HasValue)
                {
                    queryString += @" and p.Published = " + published + " ";
                }

                if (includes?.Count() > 0)
                {
                    queryString += @" and (p.Id not in (" + string.Join(",", includes) + ")) ";
                }

                if (!string.IsNullOrEmpty(key))
                {
                    queryString += @" and (p.Name like '%" + key + "%' or p.MnemonicCode like '%" + key + "%' or p.SmallBarCode like '%"+key+ "%' or p.StrokeBarCode like '%" + key + "%' or p.BigBarCode like '%" + key + "%') ";
                }

                if (categoryIds?.Count() > 0)
                {
                    queryString += @" and (c.StoreId = " + storeId + " and c.Id in (" + string.Join(",", categoryIds) + ")) ";
                }

                if (wareHouseId > 0)
                {
                    queryString += @" and st.StoreId = " + storeId + " and st.WareHouseId = " + wareHouseId + " ";
                }

                if (usablequantity.HasValue && usablequantity.Value == true)
                {
                    queryString += @" and st.UsableQuantity > 0 ";
                }


                if (productStatus < 2)
                {
                    queryString += @" and p.Status=" + productStatus + "";
                }

                if (brandId > 0) 
                {
                    queryString += @" and p.BrandId=" + brandId + "";
                }

                queryString += @" order by p.Id";

                var sbCount = new StringBuilder().AppendFormat(@"SELECT COUNT(1) as `Value` FROM ({0}) as alls", string.Format(queryString, "p.Id"));
                int totalCount = ProductsRepository.QueryFromSql<IntQueryType>(sbCount.ToString()).ToList().FirstOrDefault().Value ?? 0;

                var sbQuery = new StringBuilder().AppendFormat(@"SELECT * FROM(SELECT ROW_NUMBER() OVER(ORDER BY Name) AS RowNum, alls.* 
                                FROM({0}) as alls ) AS result  
                                WHERE RowNum >= " + pageIndex * pageSize + " AND RowNum <= " + (pageIndex + 1) * pageSize + " ORDER BY Name", string.Format(queryString, "p.*"));

                var query = ProductsRepository.QueryFromSql<ProductView>(sbQuery.ToString()).ToList();

                #endregion

                #region  按需加载

                var allPids = query.Select(s => s.Id).ToArray();

                if (includeCategories)
                {
                    allCategories = GetProductCategories(storeId ?? 0, allPids).ToList();
                }

                if (includeManufacturers)
                {
                    allManufacturers = GetProductManufacturers(storeId ?? 0, allPids).ToList();
                }

                if (includeSpecificationAttributes)
                {
                    allSpecificationAttributes = GetProductSpecificationAttributes(storeId ?? 0, allPids).ToList();
                }

                if (includeVariantAttributes)
                {
                    allVariantAttributes = GetProductVariantAttributes(storeId ?? 0, allPids).ToList();
                }

                if (includeVariantAttributeCombinations)
                {
                    allVariantAttributeCombinations = GetProductVariantAttributeCombinations(storeId ?? 0, allPids).ToList();
                }

                if (includePrices)
                {
                    allPrices = GetProductPrices(storeId ?? 0, allPids).ToList();
                }

                if (includeTierPrices)
                {
                    allTierPrices = GetProductTierPrices(storeId ?? 0, allPids).ToList();
                }

                if (includePictures)
                {
                    allPictures = GetProductPictures(storeId ?? 0, allPids).ToList();
                }

                if (includeFlavor)
                {
                    allFlavors = GetProductFlavors(storeId ?? 0, allPids).ToList();
                }

                if (includeStocks)
                {
                    allStocks = GetStocks(storeId ?? 0, allPids).ToList();
                }

                if (includeBrand)
                {
                    allBrands = GetAllBrands(storeId ?? 0, query.Select(s => s.BrandId).ToArray());
                }


                //GetProductBigStrokeSmallUnitIds

                //GetAllBrandsNames
                var products = query.Select(pv =>
                {
                    //AutoMapperConfiguration.Mapper.Map
                    var product = AutoMapperConfiguration.Mapper.Map<Product>(pv);
                    product.Name = (string.IsNullOrWhiteSpace(product.MnemonicCode) || product.MnemonicCode.StartsWith("ERP_")) ? product.Name : product.MnemonicCode;
                    product.Brand = allBrands?.Where(s => s.Id == pv.BrandId).FirstOrDefault();
                    product.ProductCategories = allCategories?.Where(p => p.ProductId == pv.Id).ToList();
                    product.ProductManufacturers = allManufacturers?.Where(p => p.ProductId == pv.Id).ToList();
                    product.ProductSpecificationAttributes = allSpecificationAttributes?.Where(p => p.ProductId == pv.Id).ToList();
                    product.ProductVariantAttributes = allVariantAttributes?.Where(p => p.ProductId == pv.Id).ToList();
                    product.ProductVariantAttributeCombinations = allVariantAttributeCombinations?.Where(p => p.ProductId == pv.Id).ToList();
                    product.ProductPrices = allPrices?.Where(p => p.ProductId == pv.Id).ToList();
                    product.ProductTierPrices = allTierPrices?.Where(p => p.ProductId == pv.Id).ToList();
                    product.ProductPictures = allPictures?.Where(p => p.ProductId == pv.Id).ToList();
                    product.Stocks = allStocks?.Where(p => p.ProductId == pv.Id).ToList();
                    product.ProductFlavors = allFlavors?.Where(p => p.ProductId == pv.Id).ToList();

                    return product;

                }).ToList();
                return new PagedList<Product>(products, pageIndex, pageSize, totalCount);

            }
            catch (Exception)
            {
                return null;
            }

            #endregion


        }



        /// <summary>
        /// 获取调拨商品
        /// </summary>
        /// <param name="store">经销商</param>
        /// <param name="type"></param>
        /// 0: 获取仓库今天销售的商品
        /// 1：获取仓库昨天的销售商品
        /// 2：获取仓库近2天的销售商品
        /// 3：获取仓库上次调拨的销售商品
        /// 4：获取仓库今天退货的商品
        /// 5：获取仓库昨天退货的商品
        /// 6：获取仓库前天退货的商品
        /// 7: 获取库存所在指定类别的商品
        /// <param name="categoryIds">商品类别</param>
        /// <param name="wareHouseId">仓库</param>
        /// <returns></returns>
        public virtual IList<Product> GetAllocationProducts(int store, int type, int[] categoryIds, int wareHouseId)
        {

            try
            {
                List<Product> list = new List<Product>();

                string queryString1 = $"SELECT DISTINCT p.* FROM Products p " +
                           $" LEFT JOIN SaleItems si ON p.Id = si.ProductId " +
                           $" LEFT JOIN SaleBills sb ON si.SaleBillId = sb.Id " +
                           $" WHERE sb.StoreId = " + store + " and sb.AuditedStatus = 1 and sb.ReversedStatus = 0 and " +
                           $" sb.WareHouseId = " + (wareHouseId > 0 ? wareHouseId : 1) + "";

                string queryString2 = $"SELECT DISTINCT p.* FROM Products p " +
                           $" LEFT JOIN ReturnItems ri ON p.Id = ri.ProductId " +
                           $" LEFT JOIN ReturnBills rb ON ri.SaleBillId = rb.Id " +
                           $" WHERE rb.StoreId = " + store + " and rb.AuditedStatus = 1 and rb.ReversedStatus = 0 and " +
                           $" rb.WareHouseId = " + (wareHouseId > 0 ? wareHouseId : 1) + "";

                switch (type)
                {
                    //0: 获取仓库今天销售的商品
                    case 0:
                        queryString1 += $" and to_days(sb.CreatedOnUtc) = to_days(now());";
                        var query0 = ProductsRepository.EntityFromSql<Product>(queryString1).ToList();
                        if (categoryIds.Where(s => s > 0).Count() > 0)
                        {
                            query0 = query0.Where(q => categoryIds.Contains(q.CategoryId)).ToList();
                        }
                        list = query0.ToList();
                        break;
                    //1：获取仓库昨天的销售商品
                    case 1:
                        queryString1 += $" and to_days(now()) - to_days(sb.CreatedOnUtc) =1;";
                        var query1 = ProductsRepository.EntityFromSql<Product>(queryString1).ToList();
                        if (categoryIds.Where(s => s > 0).Count() > 0)
                        {
                            query1 = query1.Where(q => categoryIds.Contains(q.CategoryId)).ToList();
                        }
                        list = query1.ToList();
                        break;
                    //2：获取仓库近2天的销售商品
                    case 2:
                        queryString1 += $" and to_days(now()) - to_days(sb.CreatedOnUtc) >=1;";
                        var query2 = ProductsRepository.EntityFromSql<Product>(queryString1).ToList();
                        if (categoryIds.Where(s => s > 0).Count() > 0)
                        {
                            query2 = query2.Where(q => categoryIds.Contains(q.CategoryId)).ToList();
                        }
                        list = query2.ToList();
                        break;
                    //3：获取仓库上次调拨的销售商品
                    case 3:
                        var qu = AllocationBillsRepository.Table;
                        qu = qu.Where(q => q.AuditedStatus == true);
                        qu = qu.Where(q => q.ReversedStatus == false);
                        if (wareHouseId > 0)
                        {
                            qu = qu.Where(q => q.IncomeWareHouseId == wareHouseId || q.ShipmentWareHouseId == wareHouseId);
                        }
                        int id = qu.OrderByDescending(q => q.Id).Select(q => q.Id).First();
                        var query3 =
                                     from b in AllocationItemsRepository.Table
                                     join c in ProductsRepository.Table on b.ProductId equals c.Id
                                     where b.Id == id
                                     select c;
                        if (categoryIds.Where(s => s > 0).Count() > 0)
                        {
                            query3 = query3.Where(q => categoryIds.Contains(q.CategoryId));
                        }
                        list = query3.Distinct().ToList();
                        break;
                    //4：获取仓库今天退货的商品
                    case 4:
                        queryString2 += $" and to_days(rb.CreatedOnUtc) = to_days(now());";
                        var query4 = ProductsRepository.EntityFromSql<Product>(queryString2).ToList();
                        if (categoryIds.Where(s => s > 0).Count() > 0)
                        {
                            query4 = query4.Where(q => categoryIds.Contains(q.CategoryId)).ToList();
                        }
                        list = query4.ToList();
                        break;
                    //5：获取仓库昨天退货的商品
                    case 5:
                        queryString2 += $" and to_days(now()) - to_days(rb.CreatedOnUtc) =1;";
                        var query5 = ProductsRepository.EntityFromSql<Product>(queryString2).ToList();
                        if (categoryIds.Where(s => s > 0).Count() > 0)
                        {
                            query5 = query5.Where(q => categoryIds.Contains(q.CategoryId)).ToList();
                        }
                        list = query5.ToList();
                        break;
                    //6：获取仓库前天退货的商品
                    case 6:
                        queryString2 += $" and to_days(now()) - to_days(rb.CreatedOnUtc) = 2;";
                        var query6 = ProductsRepository.EntityFromSql<Product>(queryString2).ToList();
                        if (categoryIds.Where(s => s > 0).Count() > 0)
                        {
                            query6 = query6.Where(q => categoryIds.Contains(q.CategoryId)).ToList();
                        }
                        list = query6.ToList();
                        break;
                    //7: 获取库存所在指定类别的商品
                    case 7:
                        var query7 = from a in StocksRepository.Table
                                     join b in ProductsRepository.Table on a.ProductId equals b.Id
                                     where a.StoreId == store
                                     && (wareHouseId > 0 ? a.WareHouseId == wareHouseId : 1 == 1)
                                     select b;
                        if (categoryIds.Where(s => s > 0).Count() > 0)
                        {
                            query7 = query7.Where(q => categoryIds.Contains(q.CategoryId));
                        }
                        list = query7.Distinct().ToList();
                        break;
                    default:
                        break;
                }
                return list;
            }
            catch (Exception)
            {
                return new List<Product>();
            }
        }

        /// <summary>
        /// 通常经销商商品不会太多，可以直接取，如果有必要应该分页（）
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public virtual IList<Product> GetAllProducts(int storeId)
        {
            var query = ProductsRepository.TableNoTracking;

            if (storeId != 0)
            {
                query = query.Where(p => p.StoreId == storeId);
            }

            query = query.Where(p => !p.Deleted && p.Published == true);

            //去重

            query = query.OrderBy(p => p.CreatedOnUtc);

            return query.ToList();

        }


        public virtual IList<Product> GetProducts(
            out IList<int> filterableSpecificationAttributeOptionIds,
            bool loadFilterableSpecificationAttributeOptionIds = false,
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            IList<int> categoryIds = null,
            int manufacturerId = 0,
            int storeId = 0,
            int supplierId = 0,
            int parentGroupedProductId = 0,
            ProductType? productType = null,
            bool visibleIndividuallyOnly = false,
            bool? featuredProducts = null,
            decimal? priceMin = null,
            decimal? priceMax = null,
            int productTagId = 0,
            string keywords = null,
            bool searchDescriptions = false,
            bool searchSku = true,
            bool searchProductTags = false,
            IList<int> filteredSpecs = null,
            ProductSortingEnum orderBy = ProductSortingEnum.Position,
            bool showHidden = false,
            bool specialprice = false)
        {
            filterableSpecificationAttributeOptionIds = new List<int>();

            if (categoryIds != null && categoryIds.Contains(0))
            {
                categoryIds.Remove(0);
            }


            #region 搜索

            //条件在一下自己追加
            var query = ProductsRepository.TableNoTracking;
            query = query.Where(p => !p.Deleted);
            if (!showHidden)
            {
                query = query.Where(p => p.Published);
            }

            var nowUtc = DateTime.UtcNow;
            if (priceMin.HasValue)
            {
                //最小价格在这里添加...

            }
            if (priceMax.HasValue)
            {
                //最大价格在这里添加...

            }

            //关键字
            if (!string.IsNullOrWhiteSpace(keywords))
            {
                //这里可以添加其他字段
                query = from p in query
                        where (p.Name.Contains(keywords))
                        select p;
            }

            if (!showHidden)
            {
                //访问控制在这里...
            }

            if (storeId > 0)
            {
                //经销商映射
                query = from p in query
                        join sm in StoreMappingRepository.Table
                        on new { c1 = p.Id, c2 = "Product" } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into p_sm
                        from sm in p_sm.DefaultIfEmpty()
                        where storeId == sm.StoreId
                        select p;
            }

            //搜索规格属性
            if (filteredSpecs != null && filteredSpecs.Count > 0)
            {
                query = from p in query
                        where !filteredSpecs
                                   .Except(
                                       p.ProductSpecificationAttributes.Where(psa => psa.AllowFiltering).Select(
                                           psa => psa.SpecificationAttributeOptionId))
                                   .Any()
                        select p;
            }

            //分类过滤
            if (categoryIds != null && categoryIds.Count > 0)
            {
                //search in subcategories
                query = from p in query
                        from pc in p.ProductCategories.Where(pc => categoryIds.Contains(pc.CategoryId))
                        where (!featuredProducts.HasValue)
                        select p;
            }

            //提供商过滤
            if (manufacturerId > 0)
            {
                query = from p in query
                        from pm in p.ProductManufacturers.Where(pm => pm.ManufacturerId == manufacturerId)
                        where (!featuredProducts.HasValue || featuredProducts.Value == pm.IsFeaturedProduct)
                        select p;
            }

            //供应商过滤
            if (supplierId > 0)
            {
                query = query.Where(p => p.Supplier == supplierId);
            }

            //去重
            query = from p in query
                    group p by p.Id
                        into pGroup
                    orderby pGroup.Key
                    select pGroup.FirstOrDefault();

            //排序
            if (orderBy == ProductSortingEnum.Position && categoryIds != null && categoryIds.Count > 0)
            {
                var firstCategoryId = categoryIds[0];
                query = query.OrderBy(p => p.ProductCategories.FirstOrDefault(pc => pc.CategoryId == firstCategoryId).DisplayOrder);
            }
            else if (orderBy == ProductSortingEnum.Position && manufacturerId > 0)
            {
                query =
                    query.OrderBy(p => p.ProductManufacturers.FirstOrDefault(pm => pm.ManufacturerId == manufacturerId).DisplayOrder);
            }
            else if (orderBy == ProductSortingEnum.Position && parentGroupedProductId > 0)
            {
                query = query.OrderBy(p => p.DisplayOrder);
            }
            else if (orderBy == ProductSortingEnum.Position)
            {
                //按名称
                query = query.OrderBy(p => p.Name);
            }
            else if (orderBy == ProductSortingEnum.NameAsc)
            {
                //按名称从 A 到 Z
                query = query.OrderBy(p => p.Name);
            }
            else if (orderBy == ProductSortingEnum.NameDesc)
            {
                //按名称从: Z 到 A
                query = query.OrderByDescending(p => p.Name);
            }
            else if (orderBy == ProductSortingEnum.CreatedOn)
            {
                //按创建时间
                query = query.OrderByDescending(p => p.CreatedOnUtc);
            }
            else
            {
                query = query.OrderBy(p => p.Name);
            }

            var products = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();

            //获取可过滤规范属性选项标识符
            if (loadFilterableSpecificationAttributeOptionIds)
            {
                var querySpecs = from p in query
                                 join psa in ProductsSpecificationAttributeMappingRepository.Table on p.Id equals psa.ProductId
                                 where psa.AllowFiltering
                                 select psa.SpecificationAttributeOptionId;
                //去重
                filterableSpecificationAttributeOptionIds = querySpecs
                    .Distinct()
                    .ToList();
            }

            //返回产品列表
            return products;

            #endregion

        }


        /// <summary>
        /// 获取子商品类别列
        /// </summary>
        /// <param name="store"></param>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public virtual IList<int> GetSubCategoryIds(int? store, int categoryId)
        {
            var categoryIds = CategoriesRepository_RO.QueryFromSql<IntQueryType>($"SELECT id FROM (SELECT t1.id,IF ( find_in_set( ParentId, @pids ) > 0, @pids := concat( @pids, ',', id ), 0 ) AS ischild FROM ( SELECT  id, ParentId FROM dcms.Categories where StoreId = {store} and ParentId = {categoryId} ) t1,( SELECT @pids := 1 ) t2 ) t3 WHERE ischild != 0;").Select(s => s.Value ?? 0).ToList();
            return categoryIds;
        }



        /// <summary>
        /// 获取低库存商品
        /// </summary>
        /// <param name="SupplierId">供应商标识，0 加载所有</param>
        /// <returns>Result</returns>
        public virtual IList<Product> GetLowStockProducts(int supplierId)
        {
            //跟踪商品库存
            var query1 = from p in ProductsRepository.Table
                         orderby p.MinStockQuantity
                         where !p.Deleted &&
                         p.ManageInventoryMethodId == (int)ManageInventoryMethod.ManageStock &&
                         p.MinStockQuantity >= p.StockQuantity &&
                         (supplierId == 0 || p.Supplier == supplierId)
                         select p;
            var products1 = query1.ToList();

            //按产品属性跟踪产品库存
            var query2 = from p in ProductsRepository.Table
                         from pvac in p.ProductVariantAttributeCombinations
                         where !p.Deleted &&
                         p.ManageInventoryMethodId == (int)ManageInventoryMethod.ManageStockByAttributes &&
                         pvac.StockQuantity <= 0 &&
                         (supplierId == 0 || p.Supplier == supplierId)
                         select p;

            //去重
            query2 = from p in query2
                     group p by p.Id into pGroup
                     orderby pGroup.Key
                     select pGroup.FirstOrDefault();
            var products2 = query2.ToList();

            var result = new List<Product>();
            result.AddRange(products1);
            result.AddRange(products2);
            return result;
        }

        /// <summary>
        /// 获取商品SKU
        /// </summary>
        /// <param name="sku">SKU</param>
        /// <returns></returns>
        public virtual Product GetProductBySku(string sku)
        {
            if (string.IsNullOrEmpty(sku))
            {
                return null;
            }

            sku = sku.Trim();

            var query = from p in ProductsRepository.Table
                        orderby p.Id
                        where !p.Deleted &&
                        p.Sku == sku
                        select p;
            var product = query.FirstOrDefault();
            return product;
        }


        /// <summary>
        /// 调整库存
        /// </summary>
        /// <param name="product">商品</param>
        /// <param name="decrease">指示是否增加或减少产品库存量</param>
        /// <param name="quantity">数量</param>
        /// <param name="attributesXml">属性XML</param>
        public virtual void AdjustInventory(Product product, bool decrease,
            int quantity, string attributesXml)
        {
            if (product == null)
            {
                throw new ArgumentNullException("product");
            }

            //var prevStockQuantity = product.StockQuantity;

            switch (product.ManageInventoryMethod)
            {
                case ManageInventoryMethod.DontManageStock:
                    {
                        //其他逻辑处理...
                        return;
                    }
                case ManageInventoryMethod.ManageStock:
                    {
                        int newStockQuantity;
                        if (decrease)
                        {
                            newStockQuantity = product.StockQuantity - quantity;
                        }
                        else
                        {
                            newStockQuantity = product.StockQuantity + quantity;
                        }

                        bool newPublished = product.Published;
                        bool newDisablePlaceButton = product.DisablePlaceButton;

                        //检查最小库存量
                        if (decrease)
                        {
                            if (product.MinStockQuantity >= newStockQuantity)
                            {
                                switch (product.LowStockActivity)
                                {
                                    case LowStockActivity.DisablePlaceButton:
                                        newDisablePlaceButton = true;
                                        break;
                                    case LowStockActivity.Unpublish:
                                        newPublished = false;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }

                        product.StockQuantity = newStockQuantity;
                        product.DisablePlaceButton = newDisablePlaceButton;

                        product.Published = newPublished;
                        UpdateProduct(product);

                        //发送邮件通知
                        if (decrease && product.NotifyAdminForQuantityBelow > newStockQuantity)
                        {
                            //注意（在这里发送库存预警）
                        }
                    }
                    break;
                case ManageInventoryMethod.ManageStockByAttributes:
                    {
                        var combination = _productAttributeParser.FindProductVariantAttributeCombination(product, attributesXml);
                        if (combination != null)
                        {
                            int newStockQuantity;
                            if (decrease)
                            {
                                newStockQuantity = combination.StockQuantity - quantity;
                            }
                            else
                            {
                                newStockQuantity = combination.StockQuantity + quantity;
                            }

                            combination.StockQuantity = newStockQuantity;
                            _productAttributeService.UpdateProductVariantAttributeCombination(combination);
                        }
                    }
                    break;
                default:
                    break;
            }


            //捆绑商品
            var pvaValues = _productAttributeParser.ParseProductVariantAttributeValues(attributesXml);
            foreach (var pvaValue in pvaValues)
            {
                if (pvaValue.AttributeValueType == AttributeValueType.AssociatedToProduct)
                {
                    //关联商品 (bundle)
                    var associatedProduct = GetProductById(0, pvaValue.AssociatedProductId);
                    if (associatedProduct != null)
                    {
                        AdjustInventory(associatedProduct, decrease, quantity, "");
                    }
                }
            }

        }


        /// <summary>
        /// 设置开单状态
        /// </summary>
        /// <param name="productId"></param>
        public virtual void SetSolded(int productId)
        {
            var product = GetProductById(0, productId);
            if (product != null && product.HasSold != true)
            {
                product.HasSold = true;
                //更新
                UpdateProduct(product);
            }
        }


        public virtual List<Brand> GetAllBrands(int? store, int[] ids)
        {
            var brands = BrandsRepository_RO.Table
                        .Where(c => c.StoreId == store && ids.Contains(c.Id))
                        .Select(k => k);
            return brands.ToList();
        }


        public IList<ProductCategory> GetProductCategories(int stroId, int[] productIds)
        {
            return GetProductCategoriesAsync(stroId, productIds).Result;
        }
        public async Task<IList<ProductCategory>> GetProductCategoriesAsync(int stroId, int[] productIds)
        {
            return await Task.Run(() =>
            {
                var query = ProductsCategoryMappingRepository_RO.Table.Where(s => s.StoreId == stroId && productIds.Contains(s.ProductId));
                return query.ToList();
            });
        }

        public IList<ProductManufacturer> GetProductManufacturers(int stroId, int[] productIds)
        {
            return GetProductManufacturersAsync(stroId, productIds).Result;
        }
        public async Task<IList<ProductManufacturer>> GetProductManufacturersAsync(int stroId, int[] productIds)
        {
            return await Task.Run(() =>
            {
                var query = ProductManufacturersRepository_RO.Table.Where(s => s.StoreId == stroId && productIds.Contains(s.ProductId));
                return query.ToList();
            });
        }

        public IList<ProductSpecificationAttribute> GetProductSpecificationAttributes(int stroId, int[] productIds)
        {
            return GetProductSpecificationAttributesAsync(stroId, productIds).Result;
        }
        public async Task<IList<ProductSpecificationAttribute>> GetProductSpecificationAttributesAsync(int stroId, int[] productIds)
        {
            return await Task.Run(() =>
            {
                var query = ProductsSpecificationAttributeMappingRepository_RO.Table
                .Include(s => s.SpecificationAttributeOption)
                .Where(s => s.StoreId == stroId && productIds.Contains(s.ProductId));

                return query.ToList();
            });
        }

        public IList<ProductVariantAttribute> GetProductVariantAttributes(int stroId, int[] productIds)
        {
            return GetProductVariantAttributesAsync(stroId, productIds).Result;
        }
        public async Task<IList<ProductVariantAttribute>> GetProductVariantAttributesAsync(int stroId, int[] productIds)
        {
            return await Task.Run(() =>
            {
                var query = ProductsProductAttributeMappingRepository_RO.Table.Where(s => s.StoreId == stroId && productIds.Contains(s.ProductId));
                return query.ToList();
            });
        }

        public IList<ProductVariantAttributeCombination> GetProductVariantAttributeCombinations(int stroId, int[] productIds)
        {
            return GetProductVariantAttributeCombinationsAsync(stroId, productIds).Result;
        }
        public async Task<IList<ProductVariantAttributeCombination>> GetProductVariantAttributeCombinationsAsync(int stroId, int[] productIds)
        {
            return await Task.Run(() =>
            {
                var query = ProductVariantAttributeCombinationRepository_RO.Table.Where(s => s.StoreId == stroId && productIds.Contains(s.ProductId));
                return query.ToList();
            });
        }

        public IList<ProductPrice> GetProductPrices(int stroId, int[] productIds)
        {
            return GetProductPricesAsync(stroId, productIds).Result;
        }
        public async Task<IList<ProductPrice>> GetProductPricesAsync(int stroId, int[] productIds)
        {
            return await Task.Run(() =>
            {
                var query = ProductPricesRepository.Table.Where(s => s.StoreId == stroId && productIds.Contains(s.ProductId));
                return query.ToList();
            });
        }

        public IList<ProductTierPrice> GetProductTierPrices(int stroId, int[] productIds)
        {
            return GetProductTierPricesAsync(stroId, productIds).Result;
        }
        public async Task<IList<ProductTierPrice>> GetProductTierPricesAsync(int stroId, int[] productIds)
        {
            return await Task.Run(() =>
            {
                var query = ProductTierPricesRepository_RO.Table.Where(s => s.StoreId == stroId && productIds.Contains(s.ProductId));
                return query.ToList();
            });
        }

        public IList<ProductPicture> GetProductPictures(int stroId, int[] productIds)
        {
            return GetProductPicturesAsync(stroId, productIds).Result;
        }
        public async Task<IList<ProductPicture>> GetProductPicturesAsync(int stroId, int[] productIds)
        {
            return await Task.Run(() =>
            {
                var query = ProductsPicturesMappingRepository_RO.Table.Where(s => s.StoreId == stroId && productIds.Contains(s.ProductId));
                return query.ToList();
            });
        }

        public IList<Stock> GetStocks(int stroId, int[] productIds)
        {
            return GetStocksAsync(stroId, productIds).Result;
        }
        public async Task<IList<Stock>> GetStocksAsync(int stroId, int[] productIds)
        {
            return await Task.Run(() =>
            {
                var query = StocksRepository.Table.Where(s => s.StoreId == stroId && productIds.Contains(s.ProductId));
                return query.ToList();
            });
        }

        public IList<ProductFlavor> GetProductFlavors(int stroId, int[] productIds)
        {
            return GetProductFlavorsAsync(stroId, productIds).Result;
        }
        public async Task<IList<ProductFlavor>> GetProductFlavorsAsync(int stroId, int[] productIds)
        {
            return await Task.Run(() =>
            {
                var query = ProductFlavorsRepository.Table.Where(s => s.StoreId == stroId && productIds.Contains(s.ProductId));
                return query.ToList();
            });
        }

        #endregion


        #region 商品价格

        /// <summary>
        /// 删除商品价格
        /// </summary>
        public virtual void DeleteProductPrice(ProductPrice productPrice)
        {
            if (productPrice == null)
            {
                throw new ArgumentNullException("productPrice");
            }
            //delete product
            //UpdateProductPrice(productPrice);
            if (productPrice == null)
            {
                throw new ArgumentNullException("productPrice");
            }

            var uow = ProductPricesRepository.UnitOfWork;

            ProductPricesRepository.Delete(productPrice);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(productPrice);
        }

        /// <summary>
        /// 获取显示商品价格
        /// </summary>
        public virtual IList<ProductPrice> GetAllProductPricesDisplayed(int pagesize = 0)
        {

            var query = from p in ProductPricesRepository.Table
                        orderby p.Id
                        select p;

            if (pagesize == 0)
            {
                pagesize = query.Count();
            }

            var products = query.Take(pagesize).ToList();
            return products;
        }

        /// <summary>
        /// 获取商品价格
        /// </summary>
        /// <param name="productPriceId"></param>
        /// <returns></returns>
        public virtual ProductPrice GetProductPriceById(int? store, int productPriceId)
        {
            if (productPriceId == 0)
            {
                return null;
            }

            return ProductPricesRepository.ToCachedGetById(productPriceId);
        }

        /// <summary>
        /// 根据商品单位获取商品价格
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="unitId"></param>
        /// <returns></returns>

        public virtual ProductPrice GetProductPriceByProductIdAndUnitId(int? store, int productId, int unitId)
        {
            if (productId == 0)
            {
                return null;
            }

            var key = DCMSDefaults.PRODUCTSPRICES_UNITS_BY_ID_KEY.FillCacheKey(store ?? 0, productId, unitId);
            return _cacheManager.Get(key, () =>
            {

                //var query = from p in _productPriceRepository.TableNoTracking
                //修改商品价格时会用到此调用
                var query = from p in ProductPricesRepository.Table
                            where p.ProductId == productId && p.UnitId == unitId && p.StoreId == store
                            select p;
                return query.AsNoTracking().FirstOrDefault();

            });
        }


        /// <summary>
        /// 获取商品价格
        /// </summary>
        /// <param name="productIds"></param>
        /// <returns></returns>
        public virtual IList<ProductPrice> GetProductPricesByProductIds(int? store, int[] productIds, bool platform = false)
        {
            if (productIds == null || productIds.Length == 0)
            {
                return new List<ProductPrice>();
            }

            var query = from p in ProductPricesRepository.Table
                        where p.StoreId == store && productIds.Contains(p.ProductId)
                        select p;

            if (platform == true)
            {
                query = from p in ProductPricesRepository_RO.TableNoTracking
                        where p.StoreId == store && productIds.Contains(p.ProductId)
                        select p;
            }

            var productPrice = query.ToList();
            return productPrice;

        }

        public virtual IList<ProductPrice> GetAllProductPrices(int storeId)
        {
            var key = DCMSDefaults.PRODUCTPRICES_GETALLPRODUCTPRICES_BY_STORE_KEY.FillCacheKey(storeId);
            var lists = _cacheManager.Get(key, () =>
             {
                 //select pr.* from  ProductPrices as pr left join Products  as p on pr.ProductId = p.Id  where p.StoreId = 2;
                 var query = from p in ProductsRepository.TableNoTracking
                             join pr in ProductPricesRepository.TableNoTracking on p.Id equals pr.ProductId into dc
                             from dci in dc.DefaultIfEmpty()
                             where p.StoreId == storeId
                             select new ProductPrice
                             {
                                 Id = dci != null ? dci.Id : 0,
                                 ProductId = dci != null ? dci.ProductId : 0,
                                 UnitId = dci != null ? dci.UnitId : 0,
                                 TradePrice = dci != null ? dci.TradePrice ?? 0 : 0,
                                 RetailPrice = dci != null ? dci.RetailPrice ?? 0 : 0,
                                 FloorPrice = dci != null ? dci.FloorPrice ?? 0 : 0,
                                 PurchasePrice = dci != null ? dci.PurchasePrice ?? 0 : 0,
                                 CostPrice = dci != null ? dci.CostPrice ?? 0 : 0,
                                 SALE1 = dci != null ? dci.SALE1 ?? 0 : 0,
                                 SALE2 = dci != null ? dci.SALE2 ?? 0 : 0,
                                 SALE3 = dci != null ? dci.SALE3 ?? 0 : 0
                             };
                 var productPrice = query.ToList();

                 return productPrice;
             });

            return lists ?? new List<ProductPrice>();
        }


        /// <summary>
        /// 获取商品价格
        /// </summary>
        /// <param name="productIds"></param>
        /// <returns></returns>
        public virtual IList<ProductPrice> GetAllProductPrices()
        {
            var query = from p in ProductPricesRepository.TableNoTracking
                        select p;
            var productPrice = query.ToList();
            return productPrice;
        }


        /// <summary>
        /// 根据商品单位获取对应商品价格
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        public virtual Tuple<ProductPrice, ProductPrice, ProductPrice> GetProductPriceByUnit(Product product)
        {
            return GetProductPriceByUnit(product.StoreId, product.Id, product.SmallUnitId, product.StrokeUnitId ?? 0, product.BigUnitId ?? 0);
        }

        /// <summary>
        /// 根据商品单位获取对应商品价格
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <param name="smallUnitId">小单位</param>
        /// <param name="strokeUnitId">中单位</param>
        /// <param name="bigUnitId">大单位</param>
        /// <returns></returns>
        public virtual Tuple<ProductPrice, ProductPrice, ProductPrice> GetProductPriceByUnit(int storeId, int productId, int smallUnitId, int strokeUnitId, int bigUnitId)
        {
            var smallProductPrices = GetProductPriceByProductIdAndUnitId(storeId, productId, smallUnitId);
            var strokeProductPrices = GetProductPriceByProductIdAndUnitId(storeId, productId, strokeUnitId);
            var bigProductPrices = GetProductPriceByProductIdAndUnitId(storeId, productId, bigUnitId);
            return new Tuple<ProductPrice, ProductPrice, ProductPrice>(smallProductPrices, strokeProductPrices, bigProductPrices);
        }


        /// <summary>
        /// 添加商品价格
        /// </summary>
        /// <param name="productPrice"></param>
        public virtual void InsertProductPrice(List<ProductPrice> productPrice)
        {
            if (productPrice == null)
            {
                throw new ArgumentNullException("productPrice");
            }

            var uow = ProductPricesRepository.UnitOfWork;

            ProductPricesRepository.Insert(productPrice);
            uow.SaveChanges();

            //通知
            productPrice.ForEach(s => { _eventPublisher.EntityInserted(s); });
        }

        public virtual void InsertProductPrice(ProductPrice productPrice)
        {
            if (productPrice == null)
            {
                throw new ArgumentNullException("productPrice");
            }

            var uow = ProductPricesRepository.UnitOfWork;

            ProductPricesRepository.Insert(productPrice);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(productPrice);
        }

        /// <summary>
        /// 修改商品价格
        /// </summary>
        /// <param name="productPrice"></param>
        public virtual void UpdateProductPrice(ProductPrice productPrice)
        {
            if (productPrice == null)
            {
                throw new ArgumentNullException("productPrice");
            }

            var uow = ProductPricesRepository.UnitOfWork;
            ProductPricesRepository.Update(productPrice);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(productPrice);
        }

        public virtual void UpdateProductPrice(List<ProductPrice> productPrice)
        {
            if (productPrice == null)
            {
                throw new ArgumentNullException("productPrice");
            }

            var uow = ProductPricesRepository.UnitOfWork;

            ProductPricesRepository.Update(productPrice);
            uow.SaveChanges();

            //通知
            productPrice.ForEach(s => { _eventPublisher.EntityUpdated(s); });
        }

        /// <summary>
        /// 根据商品Id获取商品单位价目列表
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public virtual IList<ProductPrice> GetProductPricesByProductId(int? store, int productId)
        {
            if (productId == 0)
            {
                return null;
            }

            var key = DCMSDefaults.PRODUCTSPRICES_BY_PRODUCTID_KEY.FillCacheKey(store ?? 0, productId);
            return _cacheManager.Get(key, () =>
            {
                var query = from pp in ProductPricesRepository.Table
                            orderby pp.Id
                            where pp.ProductId == productId
                            select pp;
                return query.AsNoTracking().ToList();
            });
        }
        #endregion

        #region 商品层次价格

        /// <summary>
        /// 删除层次价格
        /// </summary>
        /// <param name="tierPrice"></param>
        public virtual void DeleteProductTierPrice(ProductTierPrice tierPrice)
        {
            if (tierPrice == null)
            {
                throw new ArgumentNullException("tierPrice");
            }

            var uow = ProductTierPricesRepository.UnitOfWork;
            ProductTierPricesRepository.Delete(tierPrice);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityDeleted(tierPrice);
        }

        public virtual void DeleteProductTierPrice(List<ProductTierPrice> tierPrices)
        {
            if (tierPrices == null)
            {
                throw new ArgumentNullException("tierPrice");
            }

            var uow = ProductTierPricesRepository.UnitOfWork;
            ProductTierPricesRepository.Delete(tierPrices);
            uow.SaveChanges();

            tierPrices.ForEach(s => { _eventPublisher.EntityDeleted(s); });
        }

        /// <summary>
        /// 获取层次价格
        /// </summary>
        /// <param name="tierPriceId"></param>
        /// <returns></returns>
        public virtual ProductTierPrice GetProductTierPriceById(int tierPriceId)
        {
            if (tierPriceId == 0)
            {
                return null;
            }

            return ProductTierPricesRepository.ToCachedGetById(tierPriceId);
        }



        /// <summary>
        /// 获取商品层次价格
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="planId"></param>
        /// <param name="planType"></param>
        /// <returns></returns>
        public virtual ProductTierPrice GetProductTierPriceById(int productId, int planId, int planType)
        {
            if (productId == 0)
            {
                return null;
            }

            var query = from p in ProductTierPricesRepository.Table
                        where p.ProductId == productId && p.PricesPlanId == planId && p.PriceTypeId == planType
                        select p;

            return query.FirstOrDefault();
        }

        public virtual ProductTierPrice GetProductTierPriceById(int store, int productId, int planId, int planType)
        {
            if (store == 0)
            {
                return null;
            }

            if (productId == 0)
            {
                return null;
            }

            var query = from p in ProductTierPricesRepository.Table
                        where p.ProductId == productId && p.PricesPlanId == planId && p.PriceTypeId == planType
                        select p;

            return query.FirstOrDefault();
        }



        /// <summary>
        /// 获取商品全部层次价格
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public virtual IList<ProductTierPrice> GetProductTierPriceByProductId(int productId)
        {
            if (productId == 0)
            {
                return null;
            }

            var query = from p in ProductTierPricesRepository.Table
                        where p.ProductId == productId
                        select p;
            var productTierPrices = query.ToList();

            return productTierPrices;
        }

        //
        public virtual IList<ProductTierPrice> GetProductTierPriceByProductIds(int? store, int[] productIds, bool platform = false)
        {
            if (productIds == null)
            {
                return null;
            }

            var query = from p in ProductTierPricesRepository.Table
                        where p.StoreId == store && productIds.Contains(p.ProductId)
                        select p;
            if (platform == true)
            {
                query = from p in ProductTierPricesRepository_RO.TableNoTracking
                        where p.StoreId == store && productIds.Contains(p.ProductId)
                        select p;
            }
            var productTierPrices = query.ToList();

            return productTierPrices;
        }

        /// <summary>
        /// 添加层次价格
        /// </summary>
        /// <param name="tierPrice"></param>
        public virtual void InsertProductTierPrice(List<ProductTierPrice> tierPrice)
        {
            if (tierPrice == null)
            {
                throw new ArgumentNullException("tierPrice");
            }

            var uow = ProductTierPricesRepository.UnitOfWork;
            ProductTierPricesRepository.Insert(tierPrice);
            uow.SaveChanges();

            //event notification
            tierPrice.ForEach(s => { _eventPublisher.EntityInserted(s); }); ;
        }

        /// <summary>
        /// 添加层次价格
        /// </summary>
        /// <param name="tierPrice"></param>
        public virtual void InsertProductTierPrice(ProductTierPrice tierPrice)
        {
            if (tierPrice == null)
            {
                throw new ArgumentNullException("tierPrice");
            }

            var uow = ProductTierPricesRepository.UnitOfWork;
            ProductTierPricesRepository.Insert(tierPrice);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityInserted(tierPrice);
        }
        /// <summary>
        /// 更新层次价格
        /// </summary>
        /// <param name="tierPrice"></param>
        public virtual void UpdateProductTierPrice(ProductTierPrice tierPrice)
        {
            if (tierPrice == null)
            {
                throw new ArgumentNullException("tierPrice");
            }

            var uow = ProductTierPricesRepository.UnitOfWork;
            ProductTierPricesRepository.Update(tierPrice);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityUpdated(tierPrice);
        }


        #endregion

        #region 商品层次价格方案

        /// <summary>
        /// 获取层次价格方案
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual IPagedList<ProductTierPricePlan> GetAllPlans(int storeId, string name = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;
            if (storeId == 0)
            {
                return null;
            }

            var query = ProductTierPricePlansRepository.Table;
            query = query.Where(q => q.StoreId == storeId);

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(c => c.Name.Contains(name));
            }

            query = query.OrderByDescending(c => c.Name);
            //var plans = new PagedList<ProductTierPricePlan>(query.ToList(), pageIndex, pageSize);
            //return plans;

            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<ProductTierPricePlan>(plists, pageIndex, pageSize, totalCount);

        }


        public virtual IList<ProductTierPricePlan> GetAllPlansByStore(int? store)
        {
            var query = ProductTierPricePlansRepository.Table;

            if (store.HasValue)
            {
                query = query.Where(c => c.StoreId == store.Value);
            }

            query = query.OrderByDescending(c => c.Name);
            var plans = query.ToList();
            return plans;
        }

        /// <summary>
        /// 删除层次价格方案
        /// </summary>
        /// <param name="tierPrice"></param>
        public virtual void DeleteProductTierPricePlan(ProductTierPricePlan tierPricePlan)
        {
            if (tierPricePlan == null)
            {
                throw new ArgumentNullException("tierPricePlan");
            }

            var uow = ProductTierPricePlansRepository.UnitOfWork;
            ProductTierPricePlansRepository.Delete(tierPricePlan);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityDeleted(tierPricePlan);
        }

        /// <summary>
        /// 获取层次价格方案
        /// </summary>
        /// <param name="tierPriceId"></param>
        /// <returns></returns>
        public virtual ProductTierPricePlan GetProductTierPricePlanById(int tierPriceId)
        {
            if (tierPriceId == 0)
            {
                return null;
            }

            return ProductTierPricePlansRepository.ToCachedGetById(tierPriceId);
        }

        /// <summary>
        /// 添加层次价格方案
        /// </summary>
        /// <param name="tierPrice"></param>
        public virtual void InsertProductTierPrice(ProductTierPricePlan tierPricePlan)
        {
            if (tierPricePlan == null)
            {
                throw new ArgumentNullException("tierPricePlan");
            }

            var uow = ProductTierPricePlansRepository.UnitOfWork;
            ProductTierPricePlansRepository.Insert(tierPricePlan);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityInserted(tierPricePlan);
        }

        /// <summary>
        /// 更新层次价格方案
        /// </summary>
        /// <param name="tierPrice"></param>
        public virtual void UpdateProductTierPricePlan(ProductTierPricePlan tierPricePlan)
        {
            if (tierPricePlan == null)
            {
                throw new ArgumentNullException("tierPricePlan");
            }

            var uow = ProductTierPricePlansRepository.UnitOfWork;
            ProductTierPricePlansRepository.Update(tierPricePlan);
            uow.SaveChanges();


            //event notification
            _eventPublisher.EntityUpdated(tierPricePlan);
        }


        #endregion

        #region (商品)组合

        public virtual IPagedList<Combination> GetAllCombinations(int? store, string name = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;
            var query = CombinationsRepository.Table;

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(c => c.ProductName.Contains(name));
            }

            if (store.HasValue)
            {
                query = query.Where(c => c.StoreId == store.Value);
            }

            query = query.OrderByDescending(c => c.DisplayOrder);
            //var combinations = new PagedList<Combination>(query.ToList(), pageIndex, pageSize);
            //return combinations;

            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<Combination>(plists, pageIndex, pageSize, totalCount);

        }
        public virtual IList<Combination> GetAllCombinationsByStore(int? store)
        {
            var query = CombinationsRepository.Table;

            if (store.HasValue)
            {
                query = query.Where(c => c.StoreId == store.Value);
            }

            query = query.OrderByDescending(c => c.DisplayOrder);
            var combinations = query.ToList();
            return combinations;
        }

        public virtual IList<Combination> GetAllHaveSubProductsCombinationsByStore(int? store)
        {
            var query = CombinationsRepository.Table;

            if (store.HasValue)
            {
                query = query.Where(c => c.StoreId == store.Value);
            }

            //子商品过滤
            query = query.Where(c => c.ProductCombinations.Count > 0);

            query = query.OrderByDescending(c => c.DisplayOrder);
            var combinations = query.Include(cb => cb.ProductCombinations).ToList();
            return combinations;
        }


        public virtual bool CombinationHasExists(int? productId)
        {
            var query = CombinationsRepository.Table;
            if (productId.HasValue)
            {
                query = query.Where(c => c.ProductId == productId.Value);
            }

            return query.ToList().Count > 0;
        }
        public virtual void DeleteCombination(Combination combination)
        {
            if (combination == null)
            {
                throw new ArgumentNullException("combination");
            }

            var uow = CombinationsRepository.UnitOfWork;
            CombinationsRepository.Delete(combination);
            uow.SaveChanges();

            _eventPublisher.EntityDeleted(combination);
        }
        public virtual Combination GetCombinationById(int combinationId)
        {
            if (combinationId == 0)
            {
                return null;
            }

            return CombinationsRepository.ToCachedGetById(combinationId);
        }
        public virtual Combination GetCombinationByProductId(int? productId)
        {
            if (productId == 0)
            {
                return null;
            }

            var query = CombinationsRepository.Table;
            if (productId.HasValue)
            {
                query = query.Where(c => c.ProductId == productId.Value);
            }

            return query.FirstOrDefault();
        }
        public virtual void InsertCombination(Combination combination)
        {
            if (combination == null)
            {
                throw new ArgumentNullException("combination");
            }

            var uow = CombinationsRepository.UnitOfWork;
            CombinationsRepository.Insert(combination);
            uow.SaveChanges();

            _eventPublisher.EntityInserted(combination);
        }
        public virtual void UpdateCombination(Combination combination)
        {
            if (combination == null)
            {
                throw new ArgumentNullException("combination");
            }

            var uow = CombinationsRepository.UnitOfWork;
            CombinationsRepository.Update(combination);
            uow.SaveChanges();

            _eventPublisher.EntityUpdated(combination);
        }

        #endregion

        #region 组合商品

        public virtual IPagedList<ProductCombination> GetAllProductCombinations(int? store, string name = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;
            var query = ProductCombinationsRepository.Table;

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(c => c.ProductName.Contains(name));
            }

            if (store.HasValue)
            {
                query = query.Where(c => c.StoreId == store.Value);
            }

            query = query.OrderByDescending(c => c.DisplayOrder);
            //var combinations = new PagedList<ProductCombination>(query.ToList(), pageIndex, pageSize);
            //return combinations;

            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<ProductCombination>(plists, pageIndex, pageSize, totalCount);

        }
        public virtual IList<ProductCombination> GetAllProductCombinationsByCombinationId(int? store, int? combinationId)
        {
            var query = ProductCombinationsRepository.Table;

            if (store.HasValue)
            {
                query = query.Where(c => c.StoreId == store.Value);
            }

            if (combinationId.HasValue)
            {
                query = query.Where(c => c.CombinationId == combinationId.Value);
            }

            query = query.OrderByDescending(c => c.DisplayOrder);
            var productCombinations = query.ToList();
            return productCombinations;
        }
        public virtual IList<ProductCombination> GetAllProductCombinationsByStore(int? store)
        {
            var query = ProductCombinationsRepository.Table;

            if (store.HasValue)
            {
                query = query.Where(c => c.StoreId == store.Value);
            }

            query = query.OrderByDescending(c => c.DisplayOrder);
            var productCombinations = query.ToList();
            return productCombinations;
        }
        public virtual void DeleteProductCombination(ProductCombination productCombination)
        {
            if (productCombination == null)
            {
                throw new ArgumentNullException("productCombination");
            }

            var uow = ProductCombinationsRepository.UnitOfWork;
            ProductCombinationsRepository.Delete(productCombination);
            uow.SaveChanges();

            _eventPublisher.EntityDeleted(productCombination);
        }
        public virtual ProductCombination GetProductCombinationById(int productCombinationId)
        {
            if (productCombinationId == 0)
            {
                return null;
            }

            return ProductCombinationsRepository.ToCachedGetById(productCombinationId);
        }

        public virtual void InsertProductCombination(List<ProductCombination> productCombination)
        {
            if (productCombination == null)
            {
                throw new ArgumentNullException("productCombination");
            }

            var uow = ProductCombinationsRepository.UnitOfWork;
            ProductCombinationsRepository.Insert(productCombination);
            uow.SaveChanges();

            //_eventPublisher.EntityInserted(productCombination);
        }

        public virtual void InsertProductCombination(ProductCombination productCombination)
        {
            if (productCombination == null)
            {
                throw new ArgumentNullException("productCombination");
            }

            var uow = ProductCombinationsRepository.UnitOfWork;
            ProductCombinationsRepository.Insert(productCombination);
            uow.SaveChanges();

            _eventPublisher.EntityInserted(productCombination);
        }
        public virtual void UpdateProductCombination(ProductCombination productCombination)
        {
            if (productCombination == null)
            {
                throw new ArgumentNullException("productCombination");
            }

            var uow = ProductCombinationsRepository.UnitOfWork;
            ProductCombinationsRepository.Update(productCombination);
            uow.SaveChanges();
            _eventPublisher.EntityUpdated(productCombination);
        }
        public virtual void InsertProductFlavor(List<ProductFlavor> productFlavors)
        {
            if (productFlavors == null)
            {
                throw new ArgumentNullException("productFlavors");
            }

            var uow = ProductFlavorsRepository.UnitOfWork;
            ProductFlavorsRepository.Insert(productFlavors);
            uow.SaveChanges();

            //_eventPublisher.EntityInserted(productFlavors);
        }
        #endregion

        #region 最近售价

        /// <summary>
        /// 获取最近售价
        /// </summary>
        /// <param name="store"></param>
        /// <param name="productName"></param>
        /// <param name="customerName"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual IPagedList<RecentPrice> GetAllRecentPrices(int? store, string productName = null, string customerName = "", int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;

            //商品过滤
            var query1 = ProductsRepository.Table;
            if (!string.IsNullOrEmpty(productName))
            {
                query1 = query1.Where(q => q.Name.Contains(productName));
            }
            List<int> productIds = query1.Select(q => q.Id).ToList();

            //客户过滤
            var query2 = TerminalsRepository.Table;
            if (!string.IsNullOrEmpty(customerName))
            {
                query2 = query2.Where(q => q.Name.Contains(customerName));
            }
            List<int> terminalIds = query2.Select(q => q.Id).ToList();

            var query = RecentPricesRepository.Table;
            query = query.Where(q => q.StoreId == (store ?? 0));
            if (!string.IsNullOrEmpty(productName))
            {
                query = query.Where(q => productIds.Contains(q.ProductId));
            }
            if (!string.IsNullOrEmpty(customerName))
            {
                query = query.Where(q => terminalIds.Contains(q.CustomerId));
            }

            //var query2 = from p in query
            //             join rp in RecentPricesRepository.Table on p equals rp.ProductId
            //             join tr in query1 on rp.CustomerId equals tr
            //             where rp.StoreId == store
            //             select rp;

            query = query.OrderByDescending(c => c.UpdateTime);

            //var prices = new PagedList<RecentPrice>(query2.ToList(), pageIndex, pageSize);

            //return prices;

            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<RecentPrice>(plists, pageIndex, pageSize, totalCount);

        }

        /// <summary>
        /// 最近售价
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        public virtual IList<RecentPrice> GetAllRecentPricesByStore(int? store)
        {
            var query = RecentPricesRepository.Table;

            if (store.HasValue)
            {
                query = query.Where(c => c.StoreId == store.Value);
            }

            query = query.OrderByDescending(c => c.UpdateTime);

            var prices = query.ToList();

            return prices;
        }

        /// <summary>
        /// 删除更新最近售价
        /// </summary>
        /// <param name="recentPrice"></param>
        public virtual void DeleteRecentPrice(RecentPrice recentPrice)
        {
            if (recentPrice == null)
            {
                throw new ArgumentNullException("recentPrice");
            }

            var uow = RecentPricesRepository.UnitOfWork;
            RecentPricesRepository.Delete(recentPrice);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityDeleted(recentPrice);
        }

        /// <summary>
        /// 获取更新最近售价
        /// </summary>
        /// <param name="recentPriceId"></param>
        /// <returns></returns>
        public virtual RecentPrice GetRecentPriceById(int recentPriceId)
        {
            if (recentPriceId == 0)
            {
                return null;
            }

            return RecentPricesRepository.ToCachedGetById(recentPriceId);
        }


        public virtual RecentPrice GetRecentPrice(int store, int customerId, int productId)
        {
            if (store == 0 || customerId == 0 || productId == 0)
            {
                return null;
            }

            var query = RecentPricesRepository.Table;
            query = query.Where(q => q.StoreId == store);
            query = query.Where(q => q.CustomerId == customerId);
            query = query.Where(q => q.ProductId == productId);
            query = query.OrderByDescending(q => q.Id);
            return query.FirstOrDefault();

        }

        /// <summary>
        /// 添加更新最近售价
        /// </summary>
        /// <param name="recentPrice"></param>
        public virtual void InsertRecentPrice(RecentPrice recentPrice)
        {
            if (recentPrice == null)
            {
                throw new ArgumentNullException("recentPrice");
            }

            var uow = RecentPricesRepository.UnitOfWork;
            RecentPricesRepository.Insert(recentPrice);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityInserted(recentPrice);

            //if (!CheckExists(recentPrice))
            //{
            //    _recentPriceRepository.Insert(recentPrice);
            //    //event notification
            //    _eventPublisher.EntityInserted(recentPrice);
            //}
        }

        /// <summary>
        /// 终端商品最近价格是否存在
        /// </summary>
        /// <param name="recentPrice"></param>
        /// <returns></returns>
        public virtual bool CheckExists(RecentPrice recentPrice)
        {
            var query = RecentPricesRepository.Table;
            query = query.Where(p => p.ProductId == recentPrice.ProductId && p.CustomerId == recentPrice.CustomerId);
            return (query.ToList().Count > 0);
        }

        /// <summary>
        /// 更新最近售价
        /// </summary>
        /// <param name="recentPrice"></param>
        public virtual void UpdateRecentPrice(RecentPrice recentPrice)
        {
            if (recentPrice == null)
            {
                throw new ArgumentNullException("recentPrice");
            }

            var uow = RecentPricesRepository.UnitOfWork;
            RecentPricesRepository.Update(recentPrice);
            uow.SaveChanges();
            //event notification
            _eventPublisher.EntityUpdated(recentPrice);
        }

        public virtual BaseResult UpdateRecentPrice(int storeId, int userId, List<RecentPrice> recentPrices)
        {
            var uow = RecentPricesRepository.UnitOfWork;

            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();



                List<RecordProductPrice> recordProductPrices = new List<RecordProductPrice>();

                #region 更新RecentPrice
                //更新RecentPrice
                if (recentPrices != null && recentPrices.Count > 0)
                {
                    var allProducts = GetProductsByIds(storeId, recentPrices.Select(rp => rp.ProductId).ToArray());

                    recentPrices.ForEach(rp =>
                    {
                        //更新RecentPrice
                        UpdateRecentPrice(rp);

                        //添加大、中、小单位价格
                        var product = allProducts.Where(ap => ap.Id == rp.ProductId).FirstOrDefault();
                        if (product != null)
                        {
                            if (product.BigUnitId != null && product.BigUnitId != 0)
                            {
                                recordProductPrices.Add(new RecordProductPrice()
                                {
                                    StoreId = storeId,
                                    ProductId = rp.ProductId,
                                    UnitId = product.BigUnitId ?? 0,
                                    Price = rp.BigUnitPrice ?? 0
                                });
                            }
                            if (product.StrokeUnitId != null && product.StrokeUnitId != 0)
                            {
                                recordProductPrices.Add(new RecordProductPrice()
                                {
                                    StoreId = storeId,
                                    ProductId = rp.ProductId,
                                    UnitId = product.StrokeUnitId ?? 0,
                                    Price = rp.StrokeUnitPrice ?? 0
                                });
                            }
                            if (product.SmallUnitId != 0)
                            {
                                recordProductPrices.Add(new RecordProductPrice()
                                {
                                    StoreId = storeId,
                                    ProductId = rp.ProductId,
                                    UnitId = product.SmallUnitId,
                                    Price = rp.SmallUnitPrice ?? 0
                                });
                            }
                        }
                    });
                }
                #endregion

                #region 更新ProductTierPrice
                //更新ProductTierPrice
                RecordProductTierPriceLastPrice(storeId, recordProductPrices);
                #endregion


                //保存事务
                transaction.Commit();

                return new BaseResult { Success = true, Message = "上次售价更新成功" };
            }
            catch (Exception)
            {
                //如果事务不存在或者为控则回滚
                transaction?.Rollback();
                return new BaseResult { Success = false, Message = "上次售价更新失败" };
            }
            finally
            {
                //不管怎样最后都会关闭掉这个事务
                using (transaction) { }
            }
        }



        #endregion


        public ProductUnitOption UnitConversion(int? storeId, Product product, int[] producIds)
        {
            try
            {
                //单位转化
                var allProducts = GetProductsByIds(storeId??0, producIds.Distinct().ToArray());
                var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(storeId, allProducts.GetProductBigStrokeSmallUnitIds());
                var allProductPrices = GetProductPricesByProductIds(storeId, producIds.Distinct().ToArray());
                var option = product.GetProductUnit(allOptions, allProductPrices);

                return option;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Print(ex.Message);
                return null;
            }
        }
        public List<ProductUnitOptions> UnitConversions(int? storeId, List<Product> products, int[] producIds)
        {
            try
            {
                var options = new List<ProductUnitOptions>();

                products.ForEach(p =>
                {
                    var uct = UnitConversion(storeId, p, producIds);
                    options.Add(new ProductUnitOptions()
                    {
                        Option = uct,
                        Product = p
                    });
                });
                
                return options;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Print(ex.Message);
                return null;
            }
        }


        /// <summary>
        /// 获取商品现在加权平均价 (以历史5次进货价的平均价为参考成本价)
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public decimal GetNowWeightedAveragePrice(int pid, int num)
        {
            var query = from pi in PurchaseItemsRepository.Table
                        where pi.ProductId == pid
                        orderby pi.CreatedOnUtc descending
                        select pi.Price;
            return query.Take(num).Average();
        }

        /// <summary>
        /// 记录上次售价（ProductTierPrice）
        /// </summary>
        /// <param name="recentPrice"></param>
        public virtual void RecordProductTierPriceLastPrice(int storeId, List<RecordProductPrice> recordProductPrices)
        {
            if (recordProductPrices != null && recordProductPrices.Count > 0)
            {
                var allProducts = GetProductsByIds(storeId, recordProductPrices.Select(pr => pr.ProductId).Distinct().ToArray());
                recordProductPrices.ToList().ForEach(s =>
                {
                    //只记录金额大于0的
                    if (s.Price > 0)
                    {
                        ProductTierPrice productTierPrice = GetProductTierPriceById(s.StoreId, s.ProductId, 0, (int)PriceType.LastedPrice);
                        var product = allProducts.Where(ap => ap.Id == s.ProductId).FirstOrDefault();

                        if (productTierPrice == null)
                        {
                            //插入
                            productTierPrice = new ProductTierPrice
                            {
                                StoreId = s.StoreId,
                                ProductId = s.ProductId,
                                PricesPlanId = 0,
                                PriceTypeId = (int)PriceType.LastedPrice,
                                SmallUnitPrice = 0,
                                StrokeUnitPrice = 0,
                                BigUnitPrice = 0
                            };
                            if (product != null)
                            {
                                if (product.SmallUnitId == s.UnitId)
                                {
                                    productTierPrice.SmallUnitPrice = s.Price;
                                }

                                if (product.StrokeUnitId == s.UnitId)
                                {
                                    productTierPrice.StrokeUnitPrice = s.Price;
                                }

                                if (product.BigUnitId == s.UnitId)
                                {
                                    productTierPrice.BigUnitPrice = s.Price;
                                }
                            }
                            InsertProductTierPrice(productTierPrice);
                        }
                        else
                        {
                            //修改
                            if (product != null)
                            {
                                if (product.SmallUnitId == s.UnitId)
                                {
                                    productTierPrice.SmallUnitPrice = s.Price;
                                }

                                if (product.StrokeUnitId == s.UnitId)
                                {
                                    productTierPrice.StrokeUnitPrice = s.Price;
                                }

                                if (product.BigUnitId == s.UnitId)
                                {
                                    productTierPrice.BigUnitPrice = s.Price;
                                }
                            }
                            UpdateProductTierPrice(productTierPrice);
                        }
                    }
                });
            }
        }

        /// <summary>
        /// 记录上次售价（RecentPrice）
        /// </summary>
        /// <param name="recordProductPrices"></param>
        public virtual void RecordRecentPriceLastPrice(int storeId, List<RecordProductPrice> recordProductPrices)
        {
            if (recordProductPrices != null && recordProductPrices.Count > 0)
            {
                var allProducts = GetProductsByIds(storeId, recordProductPrices.Select(rp => rp.ProductId).ToArray());

                recordProductPrices.ForEach(rp =>
                {
                    var product = allProducts.Where(ap => ap.Id == rp.ProductId).FirstOrDefault();
                    if (product != null)
                    {
                        RecentPrice recentPrice = GetRecentPrice(rp.StoreId, rp.TerminalId, rp.ProductId);
                        if (recentPrice != null)
                        {
                            if (rp.UnitId == product.BigUnitId)
                            {
                                recentPrice.BigUnitPrice = rp.Price;
                            }
                            else if (rp.UnitId == product.StrokeUnitId)
                            {
                                recentPrice.StrokeUnitPrice = rp.Price;
                            }
                            else if (rp.UnitId == product.SmallUnitId)
                            {
                                recentPrice.SmallUnitPrice = rp.Price;
                            }

                            recentPrice.UpdateTime = DateTime.Now;

                            UpdateRecentPrice(recentPrice);
                        }
                        else
                        {
                            recentPrice = new RecentPrice
                            {
                                StoreId = product.StoreId,
                                CustomerId = rp.TerminalId,
                                ProductId = rp.ProductId,
                                BigUnitPrice = 0,
                                StrokeUnitPrice = 0,
                                SmallUnitPrice = 0,
                                UpdateTime = DateTime.Now
                            };
                            if (rp.UnitId == product.BigUnitId)
                            {
                                recentPrice.BigUnitPrice = rp.Price;
                            }
                            else if (rp.UnitId == product.StrokeUnitId)
                            {
                                recentPrice.StrokeUnitPrice = rp.Price;
                            }
                            else if (rp.UnitId == product.SmallUnitId)
                            {
                                recentPrice.SmallUnitPrice = rp.Price;
                            }
                            InsertRecentPrice(recentPrice);
                        }
                    }
                });
            }
        }

        /// <summary>
        /// 获取生产日期
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="wareHouseId"></param>
        /// <returns></returns>
        public List<string> GetProductDates(int store, int productId, int wareHouseId)
        {

            var query = StockInOutDetailsRepository.Table
                .Where(s => s.StoreId == store && s.WareHouseId == wareHouseId && s.ProductId == productId && s.DateOfManufacture != null).Select(s => s.DateOfManufacture)
                .OrderBy(s => s);

            return query.Select(s => s.ToString("yyyy-MM-dd")).ToList();
        }


        /// <summary>
        /// 商品新增、编辑
        /// </summary>
        /// <param name="storeId">经销商Id</param>
        /// <param name="userId">用户Id</param>
        /// <param name="productId">商品Id</param>
        /// <param name="product">商品</param>
        /// <param name="productSpecificationAttributes">商品规格属性</param>
        /// <param name="productPrices">价格信息</param>
        /// <param name="productTierPrices">价格方案</param>
        /// <param name="productFlavors">口味信息</param>
        /// <param name="combination">组合商品主表</param>
        /// <param name="productCombinations">组合商品明细</param>
        /// <returns></returns>
        public BaseResult ProductCreateOrUpdate(int storeId, int userId, int productId, Product product, List<ProductSpecificationAttribute> productSpecificationAttributes, List<ProductPrice> productPrices, List<ProductTierPrice> productTierPrices, List<ProductFlavor> productFlavors, Combination combination, List<ProductCombination> productCombinations)
        {
            var uow = ProductsRepository.UnitOfWork;

            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();

                //新增
                if (productId == 0)
                {

                    #region 基本信息、其他信息
                    InsertProduct(product);
                    productId = product.Id;

                    #endregion

                    #region 规格属性
                    if (productSpecificationAttributes != null && productSpecificationAttributes.Count > 0)
                    {
                        productSpecificationAttributes.ForEach(ps =>
                        {
                            ps.ProductId = productId;
                        });

                        _specificationAttributeService.InsertProductSpecificationAttribute(productSpecificationAttributes);
                    }
                    #endregion

                    #region 价格信息
                    if (productPrices != null && productPrices.Count > 0)
                    {
                        productPrices.ForEach(pp =>
                        {
                            pp.StoreId = storeId;
                            pp.ProductId = productId;

                        });
                        InsertProductPrice(productPrices);
                    }
                    #endregion

                    #region 价格方案
                    if (productTierPrices != null && productTierPrices.Count > 0)
                    {
                        productTierPrices.ForEach(pt =>
                        {
                            pt.ProductId = productId;

                        });
                        InsertProductTierPrice(productTierPrices);
                    }
                    #endregion

                    #region 口味信息
                    if (productFlavors != null && productFlavors.Count > 0)
                    {
                        productFlavors.ForEach(pt =>
                        {
                            pt.ProductId = productId;

                        });

                        InsertProductFlavor(productFlavors);
                    }
                    #endregion

                    #region 组合商品

                    combination.ProductId = productId;
                    InsertCombination(combination);

                    if (productCombinations != null && productCombinations.Count > 0)
                    {
                        productCombinations.ForEach(pt =>
                        {
                            pt.CombinationId = combination.Id;

                        });
                        InsertProductCombination(productCombinations);
                    }
                    #endregion
                }
                //编辑
                else
                {

                    #region 基本信息、其他信息
                    UpdateProduct(product);
                    #endregion

                    #region 规格属性

                    if (productSpecificationAttributes != null && productSpecificationAttributes.Count > 0)
                    {
                        productSpecificationAttributes.ForEach(ps =>
                        {
                            ps.ProductId = productId;
                            ps.StoreId = storeId;

                            //var temp = listSpecAttr?.Where(s => s.CustomValue == ps.CustomValue).FirstOrDefault(); //该方法未跟踪实体

                            //var temp = _specificationAttributeService.GetProductSpecificationAttributeById(ps.Id);
                            var temp = _specificationAttributeService.GetProductSpecAttributeById(ps.Id,ps.ProductId,ps.SpecificationAttributeOptionId);
                            if (temp != null)
                            {
                                //ps.Id = temp?.Id ?? 0;
                                _specificationAttributeService.UpdateProductSpecificationAttribute(ps);
                            }
                            else
                            {
                                if (ps.SpecificationAttributeOptionId > 0)
                                    _specificationAttributeService.InsertProductSpecificationAttribute(ps);
                            }
                        });
                    }
                    #endregion

                    #region 价格信息
                    if (product.HasSold == false)
                    {
                        var oldProductPrices = GetProductPricesByProductId(storeId, productId);
                        if (productPrices != null && productPrices.Count > 0)
                        {
                            productPrices.ToList().ForEach(pp =>
                            {
                                pp.StoreId = storeId;
                                ProductPrice productPrice = null;
                                if (oldProductPrices != null && oldProductPrices.Count > 0)
                                {
                                    productPrice = oldProductPrices.Where(op => op.Id == pp.Id).FirstOrDefault();
                                }
                                if (productPrice == null)
                                {
                                    pp.ProductId = productId;
                                    InsertProductPrice(pp);
                                }
                                else
                                {
                                    UpdateProductPrice(pp);
                                }
                            });
                        }
                        if (oldProductPrices != null && oldProductPrices.Count > 0)
                        {
                            oldProductPrices.ToList().ForEach(op =>
                            {
                                if (productPrices != null && productPrices.Count > 0)
                                {
                                    if (productPrices.Count(pp => pp.Id == op.Id) == 0)
                                    {
                                        DeleteProductPrice(op);
                                    }
                                }
                                else
                                {
                                    DeleteProductPrice(op);
                                }
                            });
                        }
                    }

                    #endregion

                    #region 价格方案
                    if (product.HasSold == false)
                    {
                        var oldProductTierPrices = GetProductTierPriceByProductId(productId);
                        if (productTierPrices != null && productTierPrices.Count > 0)
                        {
                            productTierPrices.ForEach(pp =>
                            {
                                if (pp.Id == 0)
                                {
                                    InsertProductTierPrice(pp);
                                }
                                else
                                {
                                    UpdateProductTierPrice(pp);
                                }
                            });
                        }
                    }

                    #endregion

                    if (combination.Id == 0)
                    {
                        InsertCombination(combination);
                    }
                }

                //保存事务
                transaction.Commit();

                return new BaseResult { Success = true, Message = "商品创建/更新成功" };
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                return new BaseResult { Success = false, Message = "商品创建/更新失败" };
            }
            finally
            {
                using (transaction) { }
            }
        }




        /// <summary>
        /// 获取商品成本价（注意，此方法不用，使用PurchaseBillService 中的获取成本价方法）
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="referenceCostPrice">预设进价，平均进价</param>
        /// <param name="averagePurchasePriceCalcNumber">平均计算次数</param>
        /// <param name="allProducts">商品</param>
        /// <returns></returns>
        public IList<ProductTierPrice> GetProductCostPrice(int storeId, int referenceCostPrice, int averagePurchasePriceCalcNumber, IList<Product> allProducts)
        {
            //预设进价
            if (referenceCostPrice == 1)
            {
                var productIds = allProducts.Select(ap => ap.Id).Distinct().ToList();
                var query = from p in ProductTierPricesRepository.Table
                            where productIds.Contains(p.Id) && p.PricesPlanId == 0 && p.PriceTypeId == 0
                            select p;
                var productTierPrices = query.ToList();
                return productTierPrices;
            }
            //平均进价
            else if (referenceCostPrice == 2)
            {
                List<ProductTierPrice> productTierPrices = new List<ProductTierPrice>();

                //默认5次
                if (averagePurchasePriceCalcNumber <= 0)
                {
                    averagePurchasePriceCalcNumber = 5;
                }

                allProducts.ToList().ForEach(p =>
                {
                    ProductTierPrice productTierPrice = new ProductTierPrice
                    {
                        StoreId = storeId,
                        ProductId = p.Id,
                        PricesPlanId = 0,
                        PriceTypeId = 0
                    };
                    decimal averagePrice = 0;
                    //小单位
                    if (p.SmallUnitId > 0)
                    {
                        var query = from a in PurchaseBillsRepository.Table
                                    join b in PurchaseItemsRepository.Table on a.Id equals b.PurchaseBillId
                                    where a.AuditedStatus == true && a.ReversedStatus == false
                                    && b.ProductId == p.Id
                                    && b.UnitId == p.SmallUnitId
                                    select b;
                        query = query.OrderByDescending(q => q.Id).Take(averagePurchasePriceCalcNumber);
                        //实际查询行数
                        int count = query.Count();
                        averagePrice = query.Sum(q => q.Price) / (count == 0 ? 1 : count);
                        productTierPrice.SmallUnitPrice = averagePrice;
                    }
                    //中单位
                    if (p.StrokeUnitId > 0)
                    {
                        var query = from a in PurchaseBillsRepository.Table
                                    join b in PurchaseItemsRepository.Table on a.Id equals b.PurchaseBillId
                                    where a.AuditedStatus == true && a.ReversedStatus == false
                                    && b.ProductId == p.Id
                                    && b.UnitId == p.StrokeUnitId
                                    select b;
                        query = query.OrderByDescending(q => q.Id).Take(averagePurchasePriceCalcNumber);
                        //实际查询行数
                        int count = query.Count();
                        averagePrice = query.Sum(q => q.Price) / (count == 0 ? 1 : count);
                        productTierPrice.StrokeUnitPrice = averagePrice;
                    }
                    //大单位
                    if (p.BigUnitId > 0)
                    {
                        var query = from a in PurchaseBillsRepository.Table
                                    join b in PurchaseItemsRepository.Table on a.Id equals b.PurchaseBillId
                                    where a.AuditedStatus == true && a.ReversedStatus == false
                                    && b.ProductId == p.Id
                                    && b.UnitId == p.BigUnitId
                                    select b;
                        query = query.OrderByDescending(q => q.Id).Take(averagePurchasePriceCalcNumber);
                        //实际查询行数
                        int count = query.Count();
                        averagePrice = query.Sum(q => q.Price) / (count == 0 ? 1 : count);
                        productTierPrice.BigUnitPrice = averagePrice;
                    }
                    productTierPrices.Add(productTierPrice);
                });

                return productTierPrices;
            }
            else
            {
                return new List<ProductTierPrice>();
            }
        }

        public void BatchSetMnemonicCode(int storeId)
        {
            var sql = $"UPDATE dcms.Products t1 INNER JOIN dcms_ocms.OCMS_Products t2  ON t1.ProductCode = t2.PRODUCT_CODE SET t1.MnemonicCode = (CASE t2.PRODUCT_SHORT_NAME WHEN NULL THEN t2.PRODUCT_NAME ELSE t2.PRODUCT_SHORT_NAME END) WHERE t1.StoreId =  {storeId}";
            ProductsRepository.ExecuteSqlScript(sql);
        }
    }

}
