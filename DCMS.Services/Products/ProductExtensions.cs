using DCMS.Core.Domain.Products;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DCMS.Services.Products
{
    /// <summary>
    /// 产品价格和规格扩展
    /// </summary>
    public static class Pexts
    {

        /// <summary>
        /// 获取商品单位规格
        /// </summary>
        /// <param name="product"></param>
        /// <param name="_specificationAttributeService"></param>
        /// <returns>各单位规格属性</returns>
        public static ProductUnitOption GetProductUnit(this Product product,
            ISpecificationAttributeService _specificationAttributeService, IProductService _productService)
        {

            if (product == null)
            {
                throw new ArgumentNullException("product");
            }

            try
            {
                var price = _productService.GetProductPriceByUnit(product);

                var option = new ProductUnitOption()
                {
                    smallOption = _specificationAttributeService.GetSpecificationAttributeOptionById(product.SmallUnitId),
                    smallPrice = price != null ? price.Item1 : null,
                    strokOption = _specificationAttributeService.GetSpecificationAttributeOptionById(product.StrokeUnitId.HasValue ? product.StrokeUnitId.Value : 0),
                    strokPrice = price != null ? price.Item2 : null,
                    bigOption = _specificationAttributeService.GetSpecificationAttributeOptionById(product.BigUnitId.HasValue ? product.BigUnitId.Value : 0),
                    bigPrice = price != null ? price.Item3 : null
                };


                if (option.smallOption == null)
                {
                    option.smallOption = new SpecificationAttributeOption();
                }

                if (option.strokOption == null)
                {
                    option.strokOption = new SpecificationAttributeOption();
                }

                if (option.bigOption == null)
                {
                    option.bigOption = new SpecificationAttributeOption();
                }

                option.smallOption.ConvertedQuantity = 1;
                option.strokOption.ConvertedQuantity = product.StrokeQuantity ?? 0;
                option.bigOption.ConvertedQuantity = product.BigQuantity ?? 0;

                option.smallOption.UnitConversion = "";
                option.strokOption.UnitConversion = string.Format("1{0} = {1}{2}", option.strokOption != null ? option.strokOption.Name : "/", product.StrokeQuantity ?? 0, option.smallOption != null ? option.smallOption.Name : "/");
                option.bigOption.UnitConversion = string.Format("1{0} = {1}{2}", option.bigOption != null ? option.bigOption.Name : "/", product.BigQuantity ?? 0, option.smallOption != null ? option.smallOption.Name : "/");

                return option;
            }
            catch (Exception ex)
            {
                throw new ArgumentNullException("获取商品单位规格异常:" + ex.Message);
            }
        }


        /// <summary>
        /// 获取商品单位规格(高效获取方式)
        /// </summary>
        /// <returns>各单位规格属性</returns>
        public static ProductUnitOption GetProductUnit(this Product product, IList<SpecificationAttributeOption> specificationAttributeOptions, IList<ProductPrice> prices)
        {
            if (product == null)
            {
                throw new ArgumentNullException("product");
            }

            try
            {
                var smallProductPrices = new ProductPrice();
                var strokeProductPrices = new ProductPrice();
                var bigProductPrices = new ProductPrice();

                try
                {
                    smallProductPrices = prices.FirstOrDefault(p => p.ProductId == product.Id && p.UnitId == product.SmallUnitId);
                }
                catch (Exception)
                {

                }

                try
                {
                    strokeProductPrices = prices.FirstOrDefault(p => p.ProductId == product.Id && p.UnitId == (product.StrokeUnitId ?? 0));
                }
                catch (Exception)
                {
                }

                try
                {
                    bigProductPrices = prices.FirstOrDefault(p => p.ProductId == product.Id && p.UnitId == (product.BigUnitId ?? 0));
                }
                catch (Exception)
                {
                }

                var option = new ProductUnitOption()
                {
                    smallOption = specificationAttributeOptions.Where(sao => sao.Id == product.SmallUnitId).FirstOrDefault(),
                    smallPrice = (smallProductPrices ?? new ProductPrice()),
                    strokOption = specificationAttributeOptions.Where(sao => sao.Id == (product.StrokeUnitId ?? 0)).FirstOrDefault(),
                    strokPrice = strokeProductPrices ?? new ProductPrice(),
                    bigOption = specificationAttributeOptions.Where(sao => sao.Id == (product.BigUnitId ?? 0)).FirstOrDefault(),
                    bigPrice = (bigProductPrices ?? new ProductPrice())
                };

                if (option.smallOption == null)
                {
                    option.smallOption = new SpecificationAttributeOption();
                }

                if (option.strokOption == null)
                {
                    option.strokOption = new SpecificationAttributeOption();
                }

                if (option.bigOption == null)
                {
                    option.bigOption = new SpecificationAttributeOption();
                }

                option.smallOption.ConvertedQuantity = 1;
                option.strokOption.ConvertedQuantity = product.StrokeQuantity ?? 0;
                option.bigOption.ConvertedQuantity = product.BigQuantity ?? 0;

                option.smallOption.UnitConversion = string.Format("1{0} = {1}{2}", option.bigOption != null ? option.bigOption.Name : "/", product.BigQuantity, option.smallOption != null ? option.smallOption.Name : "/");
                option.strokOption.UnitConversion = string.Format("1{0} = {1}{2}", option.strokOption != null ? option.strokOption.Name : "/", product.StrokeQuantity ?? 0, option.smallOption != null ? option.smallOption.Name : "/");
                option.bigOption.UnitConversion = string.Format("1{0} = {1}{2}", option.bigOption != null ? option.bigOption.Name : "/", product.BigQuantity ?? 0, option.smallOption != null ? option.smallOption.Name : "/");

                return option;
            }
            catch (Exception ex)
            {
                throw new ArgumentNullException("获取商品单位规格异常:" + ex.Message);
            }
        }



        /// <summary>
        /// 获取商品单位转化量
        /// </summary>
        /// <param name="product"></param>
        /// <param name="_specificationAttributeService"></param>
        /// <param name="_productService"></param>
        /// <returns></returns>
        public static List<SpecificationAttributeOption> GetUnitConversions(this Product product,
    ISpecificationAttributeService _specificationAttributeService, IProductService _productService)
        {
            var conversions = new List<SpecificationAttributeOption>();

            if (product == null)
            {
                throw new ArgumentNullException("product");
            }

            try
            {
                var price = _productService.GetProductPriceByUnit(product);

                var option = new ProductUnitOption()
                {
                    smallOption = _specificationAttributeService.GetSpecificationAttributeOptionById(product.SmallUnitId),
                    strokOption = _specificationAttributeService.GetSpecificationAttributeOptionById(product.StrokeUnitId.HasValue ? product.StrokeUnitId.Value : 0),
                    bigOption = _specificationAttributeService.GetSpecificationAttributeOptionById(product.BigUnitId.HasValue ? product.BigUnitId.Value : 0),
                };

                if (option.smallOption == null)
                {
                    option.smallOption = new SpecificationAttributeOption();
                }

                if (option.strokOption == null)
                {
                    option.strokOption = new SpecificationAttributeOption();
                }

                if (option.bigOption == null)
                {
                    option.bigOption = new SpecificationAttributeOption();
                }

                option.smallOption.ConvertedQuantity = 1;
                option.strokOption.ConvertedQuantity = product.StrokeQuantity;
                option.bigOption.ConvertedQuantity = product.BigQuantity;

                conversions.Add(option.smallOption);
                conversions.Add(option.strokOption);
                conversions.Add(option.bigOption);

                return conversions;
            }
            catch (Exception ex)
            {
                throw new ArgumentNullException("获取商品单位转化量:" + ex.Message);
            }
        }


        /// <summary>
        /// 获取商品单位转化量
        /// </summary>
        /// <param name="product"></param>
        /// <param name="_specificationAttributeService"></param>
        /// <param name="_productService"></param>
        /// <returns></returns>
        public static List<SpecificationAttributeOption> GetUnitConversions(this Product product, IList<SpecificationAttributeOption> allOptions)
        {
            var conversions = new List<SpecificationAttributeOption>();

            if (product == null)
            {
                throw new ArgumentNullException("product");
            }

            try
            {

                var option = new ProductUnitOption()
                {
                    smallOption = allOptions.Where(al => al != null && al.Id == product.SmallUnitId).FirstOrDefault(),
                    strokOption = allOptions.Where(al => al != null && al.Id == (product.StrokeUnitId.HasValue ? product.StrokeUnitId.Value : 0)).FirstOrDefault(),
                    bigOption = allOptions.Where(al => al != null && al.Id == (product.BigUnitId.HasValue ? product.BigUnitId.Value : 0)).FirstOrDefault(),
                };

                if (option.smallOption == null)
                {
                    option.smallOption = new SpecificationAttributeOption();
                }

                if (option.strokOption == null)
                {
                    option.strokOption = new SpecificationAttributeOption();
                }

                if (option.bigOption == null)
                {
                    option.bigOption = new SpecificationAttributeOption();
                }

                option.smallOption.ConvertedQuantity = 1;
                option.strokOption.ConvertedQuantity = product.StrokeQuantity;
                option.bigOption.ConvertedQuantity = product.BigQuantity;

                conversions.Add(option.smallOption);
                conversions.Add(option.strokOption);
                conversions.Add(option.bigOption);

                return conversions;
            }
            catch (Exception ex)
            {
                throw new ArgumentNullException("获取商品单位转化量:" + ex.Message);
            }
        }


        /// <summary>
        /// 获取商品单位转化量
        /// </summary>
        /// <param name="product"></param>
        /// <param name="unitId"></param>
        /// <param name="_specificationAttributeService"></param>
        /// <param name="_productService"></param>
        /// <returns></returns>
        public static int GetConversionQuantity(this Product product, int unitId,
 ISpecificationAttributeService _specificationAttributeService, IProductService _productService)
        {
            try
            {
                var conversions = product.GetUnitConversions(_specificationAttributeService, _productService);
                var quantity = conversions.Where(c => c.Id == unitId).Select(c => c.ConvertedQuantity ?? 0).First();
                return quantity;
            }
            catch (Exception)
            {
                return 1;
            }
        }

        /// <summary>
        /// 获取商品单位转化量
        /// </summary>
        /// <param name="product"></param>
        /// <param name="allOptions">单位</param> 
        /// <param name="unitId"></param>
        /// <returns></returns>
        public static int GetConversionQuantity(this Product product, IList<SpecificationAttributeOption> allOptions, int unitId)
        {
            try
            {
                var conversions = product.GetUnitConversions(allOptions);
                var quantity = conversions.Where(c => c.Id == unitId).Select(c => c.ConvertedQuantity ?? 0).First();
                return quantity;
            }
            catch (Exception)
            {
                return 1;
            }
        }

        /// <summary>
        /// 将商品数量转换成 大单位数量+大单位名称+中单位数量+中单位名称+小单位数量+小单位名称
        /// </summary>
        /// <param name="product"></param>
        /// <param name="unitId"></param>
        /// <param name="quantity"></param>
        /// <param name="_specificationAttributeService"></param>
        /// <param name="_productService"></param>
        /// <returns></returns>
        public static string GetConversionFormat(this Product product, int unitId, int quantity, ISpecificationAttributeService _specificationAttributeService, IProductService _productService)
        {
            try
            {
                string prefix = "";
                if (quantity < 0)
                {
                    quantity = quantity * (-1);
                    prefix = "-";
                }

                //1.将当期单位转换成最小单位数量
                int conversionQuantity = product.GetConversionQuantity(unitId, _specificationAttributeService, _productService);
                int thisQuantity = conversionQuantity * quantity;

                string result = string.Empty;

                var bigUnitId = product.BigUnitId ?? 0;
                var bigUnitName = "";
                var bigQuantity = product.BigQuantity ?? 0;

                var strokeUnitId = product.StrokeUnitId ?? 0;
                var strokeUnitName = "";
                var strokeQuantity = product.StrokeQuantity ?? 0;

                var smallUnitId = product.SmallUnitId;
                var smallUnitName = "";

                //商品单位
                var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(0, product.GetProductBigStrokeSmallUnitIds());
                Dictionary<string, int> dic = product.GetProductUnits(allOptions);

                List<string> dicKeys = dic.Keys.ToList();
                dicKeys.ForEach(u =>
                {
                    int id = dic[u];
                    if (id == bigUnitId)
                    {
                        bigUnitName = u;
                    }

                    if (id == strokeUnitId)
                    {
                        strokeUnitName = u;
                    }

                    if (id == smallUnitId)
                    {
                        smallUnitName = u;
                    }
                });

                int big = 0;
                int stroke = 0;
                int small = 0;

                //大
                if (bigUnitId > 0 && bigQuantity > 0)
                {
                    big = thisQuantity / bigQuantity;
                    thisQuantity = thisQuantity - big * bigQuantity;
                }
                //中
                if (strokeUnitId > 0 && strokeQuantity > 0)
                {
                    stroke = thisQuantity / strokeQuantity;
                    thisQuantity = thisQuantity - stroke * strokeQuantity;
                }

                //小
                small = thisQuantity;

                if (big > 0)
                {
                    result += big.ToString() + bigUnitName;
                }

                if (stroke > 0)
                {
                    result += stroke.ToString() + strokeUnitName;
                }

                if (small > 0)
                {
                    result += small.ToString() + smallUnitName;
                }

                return prefix + result;
            }
            catch /*(Exception ex)*/
            {
                return quantity.ToString();
            }
        }

        /// <summary>
        /// 将商品数量转换成 大单位数量+大单位名称+中单位数量+中单位名称+小单位数量+小单位名称
        /// </summary>
        /// <param name="product"></param>
        /// <param name="allOptions">单位</param> 
        /// <param name="unitId"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public static string GetConversionFormat(this Product product, IList<SpecificationAttributeOption> allOptions, int unitId, int quantity)
        {
            try
            {
                string prefix = "";
                if (quantity < 0)
                {
                    quantity = quantity * (-1);
                    prefix = "-";
                }

                //1.将当期单位转换成最小单位数量
                int conversionQuantity = product.GetConversionQuantity(allOptions, unitId);
                int thisQuantity = conversionQuantity * quantity;

                string result = string.Empty;

                var bigUnitId = product.BigUnitId ?? 0;
                var bigUnitName = "";
                var bigQuantity = product.BigQuantity ?? 0;

                var strokeUnitId = product.StrokeUnitId ?? 0;
                var strokeUnitName = "";
                var strokeQuantity = product.StrokeQuantity ?? 0;

                var smallUnitId = product.SmallUnitId;
                var smallUnitName = "";

                //商品单位
                Dictionary<string, int> dic = product.GetProductUnits(allOptions);

                List<string> dicKeys = dic.Keys.ToList();
                dicKeys.ForEach(u =>
                {
                    int id = dic[u];
                    if (id == bigUnitId)
                    {
                        bigUnitName = u;
                    }

                    if (id == strokeUnitId)
                    {
                        strokeUnitName = u;
                    }

                    if (id == smallUnitId)
                    {
                        smallUnitName = u;
                    }
                });

                int big = 0;
                int stroke = 0;
                int small = 0;

                //大
                if (bigUnitId > 0 && bigQuantity > 0)
                {
                    big = thisQuantity / bigQuantity;
                    thisQuantity = thisQuantity - big * bigQuantity;
                }
                //中
                if (strokeUnitId > 0 && strokeQuantity > 0)
                {
                    stroke = thisQuantity / strokeQuantity;
                    thisQuantity = thisQuantity - stroke * strokeQuantity;
                }

                //小
                small = thisQuantity;

                if (big > 0)
                {
                    result += big.ToString() + bigUnitName;
                }

                if (stroke > 0)
                {
                    result += stroke.ToString() + strokeUnitName;
                }

                if (small > 0)
                {
                    result += small.ToString() + smallUnitName;
                }

                return prefix + result;
            }
            catch /*(Exception ex)*/
            {
                return quantity.ToString();
            }
        }

        /// <summary>
        /// 将商品数量转换成 大单位数量+大单位名称+中单位数量+中单位名称+小单位数量+小单位名称
        /// </summary>
        /// <param name="product"></param>
        /// <param name="dic">当前商品 大、中、小 单位</param>
        /// <param name="unitId"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public static string GetConversionFormat(this Product product, Dictionary<string, int> dic, int unitId, int quantity)
        {
            try
            {
                string prefix = "";
                if (quantity < 0)
                {
                    quantity = quantity * (-1);
                    prefix = "-";
                }
                //1.将当期单位转换成最小单位数量
                int conversionQuantity = 1;
                if (product.BigQuantity > 0 && product.BigUnitId == unitId)
                {
                    conversionQuantity = product.BigQuantity ?? 0;
                }
                else if (product.StockQuantity > 0 && product.StrokeUnitId == unitId)
                {
                    conversionQuantity = product.StockQuantity;
                }
                int thisQuantity = conversionQuantity * quantity;

                string result = string.Empty;

                var bigUnitId = product.BigUnitId ?? 0;
                var bigUnitName = "";
                var bigQuantity = product.BigQuantity ?? 0;

                var strokeUnitId = product.StrokeUnitId ?? 0;
                var strokeUnitName = "";
                var strokeQuantity = product.StrokeQuantity ?? 0;

                var smallUnitId = product.SmallUnitId;
                var smallUnitName = "";

                List<string> dicKeys = dic.Keys.ToList();
                dicKeys.ForEach(u =>
                {
                    int id = dic[u];
                    if (id == bigUnitId)
                    {
                        bigUnitName = u;
                    }

                    if (id == strokeUnitId)
                    {
                        strokeUnitName = u;
                    }

                    if (id == smallUnitId)
                    {
                        smallUnitName = u;
                    }
                });

                int big = 0;
                int stroke = 0;
                int small = 0;

                //大
                if (bigUnitId > 0 && bigQuantity > 0)
                {
                    big = thisQuantity / bigQuantity;
                    thisQuantity = thisQuantity - big * bigQuantity;
                }
                //中
                if (strokeUnitId > 0 && strokeQuantity > 0)
                {
                    stroke = thisQuantity / strokeQuantity;
                    thisQuantity = thisQuantity - stroke * strokeQuantity;
                }

                //小
                small = thisQuantity;

                if (big > 0)
                {
                    result += big.ToString() + bigUnitName;
                }

                if (stroke > 0)
                {
                    result += stroke.ToString() + strokeUnitName;
                }

                if (small > 0)
                {
                    result += small.ToString() + smallUnitName;
                }

                return prefix + result;
            }
            catch /*(Exception ex)*/
            {
                return quantity.ToString();
            }
        }

        /// <summary>
        /// 获取商品各单位的价格
        /// </summary>
        /// <param name="product"></param>
        /// <param name="_productService"></param>
        /// <returns>Item1(小单位价格)， Item2(中单位价格)，Item3(大单位价格)，</returns>
        public static Tuple<ProductPrice, ProductPrice, ProductPrice> GetProductPriceByUnit(this Product product, IProductService _productService)
        {

            if (product == null)
            {
                throw new ArgumentNullException("product");
            }

            try
            {
                return _productService.GetProductPriceByUnit(product);
            }
            catch (Exception ex)
            {
                throw new ArgumentNullException("获取商品各单位的价格:" + ex.Message);
            }
        }


        /// <summary>
        /// 获取商品各单位的价格
        /// </summary>
        /// <param name="product"></param>
        /// <param name="smallUnitId"></param>
        /// <param name="strokeUnitId"></param>
        /// <param name="bigUnitId"></param>
        /// <param name="_productService"></param>
        /// <returns>Item1(小单位价格)， Item2(中单位价格)，Item3(大单位价格)，</returns>
        public static Tuple<ProductPrice, ProductPrice, ProductPrice> GetProductPriceByUnit(this Product product, int smallUnitId, int strokeUnitId, int bigUnitId, IProductService _productService)
        {

            if (product == null)
            {
                throw new ArgumentNullException("product");
            }

            try
            {
                return _productService.GetProductPriceByUnit(product.StoreId, product.Id, smallUnitId, strokeUnitId, bigUnitId);
            }
            catch (Exception ex)
            {
                throw new ArgumentNullException("获取商品各单位的价格:" + ex.Message);
            }
        }

        /// <summary>
        /// 获取商品各单位的价格
        /// </summary>
        /// <param name="product"></param>
        /// <param name="unitId"></param>
        /// <param name="_productService"></param>
        /// <returns></returns>
        public static ProductPrice GetProductPriceByUnitId(this Product product, int unitId, IProductService _productService)
        {
            var productPrice = new ProductPrice();

            if (product == null)
            {
                throw new ArgumentNullException("product");
            }

            try
            {
                var price = _productService.GetProductPriceByUnit(product);
                if (price.Item1.Id == unitId)
                {
                    return price.Item1;
                }
                else if (price.Item2.Id == unitId)
                {
                    return price.Item2;
                }
                else if (price.Item3.Id == unitId)
                {
                    return price.Item2;
                }
                else
                {
                    return price.Item1;
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentNullException("获取商品各单位的价格:" + ex.Message);
            }

        }

        /// <summary>
        /// 获取商品各单位的价格
        /// </summary>
        /// <param name="product"></param>
        /// <param name="unitId"></param>
        /// <param name="_productService"></param>
        /// <param name="_specificationAttributeService"></param>
        /// <returns>Item1(单位价格ProductPrice)， Item2(单位规格SpecificationAttributeOption)</returns>
        public static Tuple<ProductPrice, SpecificationAttributeOption> GetProductPriceByUnitId(this Product product, int unitId, IProductService _productService, ISpecificationAttributeService _specificationAttributeService)
        {
            if (product == null)
            {
                throw new ArgumentNullException("product");
            }

            try
            {
                var price = _productService.GetProductPriceByUnit(product);
                var option = _specificationAttributeService.GetSpecificationAttributeOptionById(unitId);

                if (price.Item1.Id == unitId)
                {
                    return new Tuple<ProductPrice, SpecificationAttributeOption>(price.Item1, option);
                }
                else if (price.Item2.Id == unitId)
                {
                    return new Tuple<ProductPrice, SpecificationAttributeOption>(price.Item2, option);
                }
                else if (price.Item3.Id == unitId)
                {
                    return new Tuple<ProductPrice, SpecificationAttributeOption>(price.Item3, option);
                }
                else
                {
                    return new Tuple<ProductPrice, SpecificationAttributeOption>(price.Item1, option);
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentNullException("获取商品各单位的价格:" + ex.Message);
            }

        }


        /// <summary>
        /// 获取商品层次价格
        /// </summary>
        /// <param name="product">商品</param>
        /// <param name="planId">方案（自定义方案取值为方案ID，否则取0）</param>
        /// <param name="planType">方案类型（自定义方案取值88，否则取（进价(成本价)：0,批发价格：1,零售价格：2,最低售价：3））</param>
        /// <param name="_productService"></param>
        /// <returns></returns>
        public static TierPrice GetProductTierPriceByPlan(this Product product, int planId, int planType, IProductService _productService, ISpecificationAttributeService _specificationAttributeService)
        {
            var productTierPrice = new TierPrice();

            if (product == null)
            {
                throw new ArgumentNullException("product");
            }

            try
            {
                var price = _productService.GetProductTierPriceById(product.Id, planId, planType);

                var tierPrice = new TierPrice()
                {
                    SmallOption = _specificationAttributeService.GetSpecificationAttributeOptionById(product.SmallUnitId),
                    SmallUnitPrice = price != null ? price.SmallUnitPrice : null,
                    StrokOption = _specificationAttributeService.GetSpecificationAttributeOptionById(product.StrokeUnitId.HasValue ? product.StrokeUnitId.Value : 0),
                    StrokeUnitPrice = price != null ? price.StrokeUnitPrice : null,
                    BigOption = _specificationAttributeService.GetSpecificationAttributeOptionById(product.BigUnitId.HasValue ? product.BigUnitId.Value : 0),
                    BigUnitPrice = price != null ? price.BigUnitPrice : null
                };

                return tierPrice;
            }
            catch (Exception ex)
            {
                throw new ArgumentNullException("获取商品层次价格:" + ex.Message);
            }
        }

        /// <summary>
        /// 获取商品所有层次价格
        /// </summary>
        /// <param name="product">商品</param>
        /// <param name="_productService"></param>
        /// <returns></returns>
        public static List<ProductTierPrice> GetAllProductTierPrices(this Product product, IProductService _productService)
        {
            if (product == null)
            {
                throw new ArgumentNullException("product");
            }

            try
            {
                var productTierPrices = _productService.GetProductTierPriceByProductId(product.Id);
                return productTierPrices.ToList();
            }
            catch (Exception ex)
            {
                throw new ArgumentNullException("获取商品所有层次价格:" + ex.Message);
            }
        }


        /// <summary>
        /// 获取商品单位换算
        /// </summary>
        /// <param name="product"></param>
        /// <param name="_productService"></param>
        /// <param name="_specificationAttributeService"></param>
        /// <returns>1箱 = 10瓶</returns>
        public static string GetProductUnitConversion(this Product product, IProductService _productService, ISpecificationAttributeService _specificationAttributeService)
        {
            if (product == null)
            {
                throw new ArgumentNullException("product");
            }

            //获取规格
            var smallOption = _specificationAttributeService.GetSpecificationAttributeOptionById(product.SmallUnitId);
            var strokOption = _specificationAttributeService.GetSpecificationAttributeOptionById(product.StrokeUnitId.HasValue ? product.StrokeUnitId.Value : 0);
            var bigOption = _specificationAttributeService.GetSpecificationAttributeOptionById(product.BigUnitId.HasValue ? product.BigUnitId.Value : 0);

            //单位换算
            var unitName = string.Format("1{0} = {1}{2}", bigOption != null ? bigOption.Name : "/", product.BigQuantity, smallOption != null ? smallOption.Name : "/");

            return unitName;
        }

        /// <summary>
        /// 获取商品单位换算(高效获取方式)
        /// </summary>
        /// <param name="product"></param>
        /// <param name="specificationAttributeOptions"></param>
        /// <returns></returns>
        public static string GetProductUnitConversion(this Product product, IList<SpecificationAttributeOption> specificationAttributeOptions)
        {
            if (product == null)
            {
                throw new ArgumentNullException("product");
            }

            //获取规格
            var smallOption = specificationAttributeOptions.Where(sao => sao != null && sao.Id == product.SmallUnitId).FirstOrDefault();
            var strokOption = specificationAttributeOptions.Where(sao => sao != null && sao.Id == (product.StrokeUnitId.HasValue ? product.StrokeUnitId.Value : 0)).FirstOrDefault();
            var bigOption = specificationAttributeOptions.Where(sao => sao != null && sao.Id == (product.BigUnitId.HasValue ? product.BigUnitId.Value : 0)).FirstOrDefault();

            //单位换算
            var unitName = string.Format("1{0} = {1}{2}", bigOption != null ? bigOption.Name : "/", product.BigQuantity, smallOption != null ? smallOption.Name : "/");

            return unitName;
        }


        /// <summary>
        /// 获取商品单位和对应规格
        /// </summary>
        /// <param name="product"></param>
        /// <param name="_productService"></param>
        /// <param name="_specificationAttributeService"></param>
        /// <returns>{"瓶":11,"包":20,"箱":17}</returns>
        public static Dictionary<string, int> GetProductUnits(this Product product, IProductService _productService, ISpecificationAttributeService _specificationAttributeService)
        {
            if (product == null)
            {
                throw new ArgumentNullException("product");
            }

            //获取规格
            var smallOption = _specificationAttributeService.GetSpecificationAttributeOptionById(product.SmallUnitId);
            var strokOption = _specificationAttributeService.GetSpecificationAttributeOptionById(product.StrokeUnitId.HasValue ? product.StrokeUnitId.Value : 0);
            var bigOption = _specificationAttributeService.GetSpecificationAttributeOptionById(product.BigUnitId.HasValue ? product.BigUnitId.Value : 0);

            return PrepareUnits(smallOption, strokOption, bigOption);
        }

        /// <summary>
        /// 获取商品单位和对应规格
        /// </summary>
        /// <param name="product"></param>
        /// <param name="productSpecificationAttributes"></param>
        /// <returns></returns>
        public static Dictionary<string, int> GetProductUnits(this Product product, IList<ProductSpecificationAttribute> productSpecificationAttributes)
        {
            if (product == null)
            {
                throw new ArgumentNullException("product");
            }

            //获取规格
            var smallSPEC = productSpecificationAttributes.Where(ps => ps.ProductId == product.Id && ps.SpecificationAttributeOptionId == product.SmallUnitId).FirstOrDefault();
            var strokOptionSPEC = productSpecificationAttributes.Where(ps => ps.ProductId == product.Id && ps.SpecificationAttributeOptionId == (product.StrokeUnitId ?? 0)).FirstOrDefault();
            var bigOptionSPEC = productSpecificationAttributes.Where(ps => ps.ProductId == product.Id && ps.SpecificationAttributeOptionId == (product.BigUnitId ?? 0)).FirstOrDefault();

            var smallOption = smallSPEC != null ? smallSPEC.SpecificationAttributeOption : new SpecificationAttributeOption();
            var strokOption = strokOptionSPEC != null ? strokOptionSPEC.SpecificationAttributeOption : new SpecificationAttributeOption();
            var bigOption = bigOptionSPEC != null ? bigOptionSPEC.SpecificationAttributeOption : new SpecificationAttributeOption();

            return PrepareUnits(smallOption, strokOption, bigOption);
        }

        /// <summary>
        /// 获取商品单位和对应规格
        /// </summary>
        /// <param name="product"></param>
        /// <param name="specificationAttributeOptions"></param>
        /// <returns></returns>
        public static Dictionary<string, int> GetProductUnits(this Product product, IList<SpecificationAttributeOption> specificationAttributeOptions)
        {
            if (product == null)
            {
                throw new ArgumentNullException("product");
            }

            //获取规格
            var smallOption = specificationAttributeOptions.Where(sao => sao != null && sao.Id == product.SmallUnitId).FirstOrDefault();
            var strokOption = specificationAttributeOptions.Where(sao => sao != null && sao.Id == (product.StrokeUnitId ?? 0)).FirstOrDefault();
            var bigOption = specificationAttributeOptions.Where(sao => sao != null && sao.Id == (product.BigUnitId ?? 0)).FirstOrDefault();
            return PrepareUnits(smallOption, strokOption, bigOption);
        }

        public static Dictionary<int, string> GetFlavors(this Product product)
        {
            Dictionary<int, string> dic = new Dictionary<int, string>();
            if (product.ProductFlavors != null && product.ProductFlavors.Count > 0)
            {
                product.ProductFlavors.ToList().ForEach(pf =>
                {
                    dic.Add(pf.Id, pf.Name);
                });
            }
            return dic;
        }

        public static Dictionary<string, int> PrepareUnits(SpecificationAttributeOption smallOption, SpecificationAttributeOption strokOption, SpecificationAttributeOption bigOption)
        {
            var units = new Dictionary<string, int>();
            var ks = smallOption != null ? (string.IsNullOrEmpty(smallOption.Name) ? "SMALL" : smallOption.Name) : "SMALL";
            var km = strokOption != null ? (string.IsNullOrEmpty(strokOption.Name) ? "STROK" : strokOption.Name) : "STROK";
            var kb = bigOption != null ? (string.IsNullOrEmpty(bigOption.Name) ? "BIG" : bigOption.Name) : "BIG";

            var vs = smallOption != null ? smallOption.Id : 0;
            var vm = strokOption != null ? strokOption.Id : 0;
            var vb = bigOption != null ? bigOption.Id : 0;

            units.Add(ks, vs);

            if (!units.ContainsKey(km))
            {
                units.Add(km, vm);
            }
            else if (!units.ContainsKey($"{km}-R"))
            {
                units.Add($"{km}-R", vm);
            }

            if (!units.ContainsKey(kb))
            {
                units.Add(kb, vb);
            }
            else if (!units.ContainsKey($"{km}-B"))
            {
                units.Add($"{kb}-B", vb);
            }

            return units;
        }

        public static Dictionary<string, int> GetProductUnits(int bigUnitId = 0, string bigUnitName = "", int strokeUnitId = 0, string strokeUnitName = "", int smallUnitId = 0, string smallUnitName = "")
        {
            var units = new Dictionary<string, int>();
            var ks = string.IsNullOrEmpty(smallUnitName) ? "SMALL" : smallUnitName;
            var km = string.IsNullOrEmpty(strokeUnitName) ? "STROK" : strokeUnitName;
            var kb = string.IsNullOrEmpty(bigUnitName) ? "BIG" : bigUnitName;

            var vs = smallUnitId;
            var vm = strokeUnitId;
            var vb = bigUnitId;

            units.Add(ks, vs);

            if (!units.ContainsKey(km))
            {
                units.Add(km, vm);
            }
            else if (!units.ContainsKey($"{km}-R"))
            {
                units.Add($"{km}-R", vm);
            }

            if (!units.ContainsKey(kb))
            {
                units.Add(kb, vb);
            }
            else if (!units.ContainsKey($"{km}-B"))
            {
                units.Add($"{kb}-B", vb);
            }

            return units;
        }


        #region 商品库存数量（库用、现货、预占、锁定）
        /// <summary>
        /// 获取商品可用库存
        /// </summary>
        /// <param name="product">商品</param>
        /// <param name="wareHouseId">仓库Id</param>
        /// <returns></returns>
        public static int GetProductUsableQuantity(this Product product, int wareHouseId)
        {
            if (wareHouseId == 0)
            {
                return product.Stocks.Sum(s => s.UsableQuantity ?? 0);
            }
            else
            {
                return product.Stocks.Where(s => s.WareHouseId == wareHouseId).Sum(s => s.UsableQuantity ?? 0);
            }
        }

        /// <summary>
        /// 获取商品现货库存
        /// </summary>
        /// <param name="product">商品</param>
        /// <param name="wareHouseId">仓库Id</param>
        /// <returns></returns>
        public static int GetProductCurrentQuantity(this Product product, int wareHouseId)
        {
            if (wareHouseId == 0)
            {
                return product.Stocks.Sum(s => s.CurrentQuantity ?? 0);
            }
            else
            {
                return product.Stocks.Where(s => s.WareHouseId == wareHouseId).Sum(s => s.CurrentQuantity ?? 0);
            }

        }

        /// <summary>
        /// 获取商品预占库存
        /// </summary>
        /// <param name="product">商品</param>
        /// <param name="wareHouseId">仓库Id</param>
        /// <returns></returns>
        public static int GetProductOrderQuantity(this Product product, int wareHouseId)
        {
            if (wareHouseId == 0)
            {
                return product.Stocks.Sum(s => s.OrderQuantity ?? 0);
            }
            else
            {
                return product.Stocks.Where(s => s.WareHouseId == wareHouseId).Sum(s => s.OrderQuantity ?? 0);
            }
        }

        /// <summary>
        /// 获取商品锁定库存
        /// </summary>
        /// <param name="product">商品</param>
        /// <param name="wareHouseId">仓库Id</param>
        /// <returns></returns>
        public static int GetProductLockQuantity(this Product product, int wareHouseId)
        {
            if (wareHouseId == 0)
            {
                return product.Stocks.Sum(s => s.LockQuantity ?? 0);
            }
            else
            {
                return product.Stocks.Where(s => s.WareHouseId == wareHouseId).Sum(s => s.LockQuantity ?? 0);
            }
        }

        #endregion


        /// <summary>
        /// 格式化转化量
        /// </summary>
        /// <param name="product"></param>
        /// <param name="option"></param>
        /// <param name="StockQuantity"></param>
        /// <returns></returns>
        public static string StockQuantityFormat(this Product product, ProductUnitOption option, int StockQuantity)
        {
            var big = StockQuantity > 0 ? (StockQuantity / product.BigQuantity) : 0;
            var sma = StockQuantity > 0 ? (StockQuantity % product.BigQuantity) : 0;
            return $"{big}{option.bigOption.Name}{sma}{option.smallOption.Name}";
        }

        /// <summary>
        /// 格式化转化量
        /// </summary>
        /// <param name="product">商品</param>
        /// <param name="totalQuantity">小单位总量</param>
        /// <param name="_specificationAttributeService"></param>
        /// <param name="_productService"></param>
        /// <returns></returns>
        public static string StockQuantityFormat(this Product product, int totalQuantity, ISpecificationAttributeService _specificationAttributeService, IProductService _productService)
        {
            try
            {
                int thisQuantity = totalQuantity;
                string result = string.Empty;

                var bigUnitId = product.BigUnitId ?? 0;
                var bigUnitName = "";
                var bigQuantity = product.BigQuantity ?? 0;

                var strokeUnitId = product.StrokeUnitId ?? 0;
                var strokeUnitName = "";
                var strokeQuantity = product.StrokeQuantity ?? 0;

                var smallUnitId = product.SmallUnitId;
                var smallUnitName = "";

                //商品单位
                var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(0, product.GetProductBigStrokeSmallUnitIds());
                Dictionary<string, int> dic = product.GetProductUnits(allOptions);

                List<string> dicKeys = dic.Keys.ToList();
                dicKeys.ForEach(u =>
                {
                    int id = dic[u];
                    if (id == bigUnitId)
                    {
                        bigUnitName = u;
                    }

                    if (id == strokeUnitId)
                    {
                        strokeUnitName = u;
                    }

                    if (id == smallUnitId)
                    {
                        smallUnitName = u;
                    }
                });

                int big = 0;
                int stroke = 0;
                int small = 0;

                //大
                if (bigUnitId > 0 && bigQuantity > 0)
                {
                    big = thisQuantity / bigQuantity;
                    thisQuantity = thisQuantity - big * bigQuantity;
                }
                //中
                if (strokeUnitId > 0 && strokeQuantity > 0)
                {
                    stroke = thisQuantity / bigQuantity;
                    thisQuantity = thisQuantity - stroke * strokeQuantity;
                }

                //小
                small = thisQuantity;

                if (big > 0)
                {
                    result += big.ToString() + bigUnitName;
                }

                if (stroke > 0)
                {
                    result += stroke.ToString() + strokeUnitName;
                }

                if (small > 0)
                {
                    result += small.ToString() + smallUnitName;
                }

                return result;
            }
            catch
            {
                return "";
            }
        }


        /// <summary>
        /// 格式化转化量
        /// </summary>
        /// <param name="totalQuantity">总小单位量</param>
        /// <param name="mCQuantity">中单位转化量</param>
        /// <param name="bCQuantity">大单位转化量</param>
        /// <param name="sName">小单位</param>
        /// <param name="mName">中单位</param>
        /// <param name="bName">大单位</param>
        /// <returns></returns>
        public static string StockQuantityFormat(int totalQuantity, int mCQuantity, int bCQuantity, string sName, string mName, string bName)
        {
            try
            {
                int thisQuantity = totalQuantity;
                string result = string.Empty;

                var bigUnitName = bName;
                var bigQuantity = bCQuantity;

                var strokeUnitName = mName;
                var strokeQuantity = mCQuantity;

                var smallUnitName = sName;


                int big = 0;
                int stroke = 0;
                int small = 0;

                //大
                if (bigQuantity > 0)
                {
                    big = thisQuantity / bigQuantity;
                    thisQuantity = thisQuantity - big * bigQuantity;
                }
                //中
                if (strokeQuantity > 0)
                {
                    stroke = thisQuantity / bigQuantity;
                    thisQuantity = thisQuantity - stroke * strokeQuantity;
                }

                //小
                small = thisQuantity;

                if (big > 0)
                {
                    result += big.ToString() + bigUnitName;
                }

                if (stroke > 0)
                {
                    result += stroke.ToString() + strokeUnitName;
                }

                if (small > 0)
                {
                    result += small.ToString() + smallUnitName;
                }

                return result;
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// 构建XX大XX中XX小
        /// </summary>
        /// <param name="totalQuantity"></param>
        /// <param name="mCQuantity"></param>
        /// <param name="bCQuantity"></param>
        /// <returns></returns>
        public static Tuple<int, int, int> StockQuantityFormat(int totalQuantity, int mCQuantity, int bCQuantity)
        {
            try
            {
                int thisQuantity = totalQuantity;
                string result = string.Empty;

                var bigQuantity = bCQuantity;
                var strokeQuantity = mCQuantity;


                int big = 0;
                int stroke = 0;
                int small = 0;

                //大
                if (bigQuantity > 0)
                {
                    big = thisQuantity / bigQuantity;
                    thisQuantity = thisQuantity - big * bigQuantity;
                }
                //中
                if (strokeQuantity > 0)
                {
                    stroke = thisQuantity / bigQuantity;
                    thisQuantity = thisQuantity - stroke * strokeQuantity;
                }

                //小
                small = thisQuantity;

                return new Tuple<int, int, int>(big, stroke, small);
            }
            catch
            {
                return new Tuple<int, int, int>(0, 0, 0);
            }
        }


        /// <summary>
        /// 获取商品对应单位条形码
        /// </summary>
        /// <param name="product"></param>
        /// <param name="unitId"></param>
        /// <returns></returns>
        public static string GetProductBarCode(this Product product, int unitId)
        {
            try
            {
                string barCode = "";
                if (product.SmallUnitId == unitId)
                {
                    barCode = product.SmallBarCode;
                }
                else if (product.StrokeUnitId == unitId)
                {
                    barCode = product.StrokeBarCode;
                }
                else if (product.BigUnitId == unitId)
                {
                    barCode = product.BigBarCode;
                }
                return barCode;
            }
            catch
            {
                return "";
            }
        }


        /// <summary>
        /// 获取商品大单位、中单位、小单位
        /// </summary>
        /// <param name="allProducts"></param>
        /// <returns></returns>
        public static List<int> GetProductBigStrokeSmallUnitIds(this Product product)
        {
            List<int> optionIds = new List<int>();
            if (product != null)
            {
                optionIds.Add(product.BigUnitId ?? 0);
                optionIds.Add(product.StrokeUnitId ?? 0);
                optionIds.Add(product.SmallUnitId);
            }
            return optionIds;
        }

        /// <summary>
        /// 获取商品大单位、中单位、小单位
        /// </summary>
        /// <param name="allProducts"></param>
        /// <returns></returns>
        public static List<int> GetProductBigStrokeSmallUnitIds(this IList<Product> allProducts)
        {
            List<int> optionIds = new List<int>();
            if (allProducts != null && allProducts.Count > 0)
            {
                List<int> smallIds = allProducts.Select(sd => sd.SmallUnitId).Distinct().ToList();
                List<int> strokeIds = allProducts.Select(sd => sd.StrokeUnitId ?? 0).Distinct().ToList();
                List<int> bigIds = allProducts.Select(sd => sd.BigUnitId ?? 0).Distinct().ToList();
                optionIds.AddRange(smallIds);
                optionIds.AddRange(strokeIds);
                optionIds.AddRange(bigIds);

                optionIds = optionIds.Distinct().ToList();
            }
            return optionIds;
        }

        /// <summary>
        /// 获取商品成本价
        /// </summary>
        /// <param name="product"></param>
        /// <param name="productTierPrice"></param>
        /// <param name="unitId"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public static decimal GetProductCostPrice(this Product product, ProductTierPrice productTierPrice, int unitId, int quantity)
        {
            decimal result = 0;
            if (product != null && productTierPrice != null)
            {
                decimal costAmount = 0;
                if (unitId == product.SmallUnitId)
                {
                    costAmount = productTierPrice.SmallUnitPrice ?? 0;
                }
                else if (unitId == product.StrokeUnitId)
                {
                    costAmount = productTierPrice.StrokeUnitPrice ?? 0;
                }
                else if (unitId == product.BigUnitId)
                {
                    costAmount = productTierPrice.BigUnitPrice ?? 0;
                }
                result = costAmount * quantity;
            }
            return result;
        }


    }

}
