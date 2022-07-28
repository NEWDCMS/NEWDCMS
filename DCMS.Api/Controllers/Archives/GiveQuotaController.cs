using DCMS.Api.Infrastructure.Mapper.Extensions;
using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.Users;
using DCMS.Services.Logging;
using DCMS.Services.Products;
using DCMS.Services.Users;
using DCMS.ViewModel.Models.Products;
using DCMS.Web.Framework.Extensions;
using DCMS.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace DCMS.Api.Controllers
{

    /// <summary>
    /// 赠品额度设置
    /// </summary>
   [Authorize]
    [Route("api/v{version:apiVersion}/dcms/archives")]
    public class GiveQuotaController : BaseAPIController
    {
        private readonly IProductService _productService;

        private readonly IGiveQuotaService _giveQuotaService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IUserActivityService _userActivityService;
        private readonly IRedLocker _locker;
        private readonly IUserService _userService;
        

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="productService"></param>
        /// <param name="cacheManager"></param>
        /// <param name="specificationAttributeService"></param>
        /// <param name="giveQuotaService"></param>
        /// <param name="userActivityService"></param>
        /// <param name="locker"></param>
        /// <param name="userService"></param>
        public GiveQuotaController(
            IProductService productService,
            IStaticCacheManager cacheManager,
            ISpecificationAttributeService specificationAttributeService,
            IGiveQuotaService giveQuotaService,
            IUserActivityService userActivityService,
            IRedLocker locker,
            IUserService userService,
            ILogger<BaseAPIController> logger) : base(logger)
        {
            _productService = productService;
            
            _specificationAttributeService = specificationAttributeService;
            _giveQuotaService = giveQuotaService;
            _userActivityService = userActivityService;
            _locker = locker;
            _userService = userService;
        }

        /// <summary>
        /// 获取所有赠品额度配置
        /// </summary>
        /// <param name="store"></param>
        /// <param name="userId"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        [HttpGet("givequota/getAllGiveQuotas/{store}")]
        [SwaggerOperation("getAllGiveQuotas")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<GiveQuotaOptionModel>>> GetAllGiveQuotas(int? store, int year, int userId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error2<GiveQuotaOptionModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var giveQuota = _giveQuotaService.GetGiveQuotaByStoreIdUserIdYear(store ?? 0, userId, year);
                    var options = _giveQuotaService.GetGiveQuotaOptions(year, userId);
                    var allProducts = _productService.GetProductsByIds(store ?? 0, options.Select(pr => pr.ProductId).Distinct().ToArray());
                    var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(store, allProducts.GetProductBigStrokeSmallUnitIds());

                    var result = options.Select(o =>
                    {
                        var m = o.ToModel<GiveQuotaOptionModel>();
                        var product = allProducts.Where(ap => ap.Id == m.ProductId).FirstOrDefault();
                        if (product != null)
                        {
                            m.ProductName = product != null ? product.Name : "";
                            m.ProductSKU = product != null ? product.Sku : "";
                            m.BarCode = product != null ? product.SmallBarCode : "";
                            m.UnitConversion = product.GetProductUnitConversion(allOptions);
                            //m.Units = product.GetProductUnits(_productService, _specificationAttributeService);
                            m.Units = product.GetProductUnits(allOptions);
                            m.UnitName = m.Units.Keys.Select(k => k).ToArray()[2];

                            m.Jan ??= 0;
                            m.Feb ??= 0;
                            m.Mar ??= 0;
                            m.Apr ??= 0;
                            m.May ??= 0;
                            m.Jun ??= 0;
                            m.Jul ??= 0;
                            m.Aug ??= 0;
                            m.Sep ??= 0;
                            m.Oct ??= 0;
                            m.Nov ??= 0;
                            m.Dec ??= 0;
                        }
                        m.Total = ((m.Jan ?? 0) + (m.Feb ?? 0) + (m.Mar ?? 0) + (m.Apr ?? 0) + (m.May ?? 0) + (m.Jun ?? 0) + (m.Jul ?? 0) + (m.Aug ?? 0) + (m.Sep ?? 0) + (m.Oct ?? 0) + (m.Nov) + (m.Dec ?? 0));
                        return m;

                    }).ToList();

                    return this.Successful("", result);
                }
                catch (Exception ex)
                {
                    return this.Error<GiveQuotaOptionModel>(ex.Message);
                }

            });
        }

        /// <summary>
        /// 创建/修改
        /// </summary>
        /// <param name="store"></param>
        /// <param name="data"></param>
        /// <param name="giveQuotaId"></param>
        /// <returns></returns>
        [HttpPost("givequota/createOrUpdate/{store}")]
        [SwaggerOperation("createOrUpdate")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> CreateOrUpdate(GiveQuotaUpdateModel data, int? store, int? giveQuotaId, int? userId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error(Resources.ParameterError);

            return await Task.Run(async () =>
            {
                try
                {
                    User user = _userService.GetUserById(store ?? 0, userId ?? 0);

                    if (data != null)
                    {

                        string errMsg = string.Empty;
                        var giveQuota = new GiveQuota();

                        #region 验证

                        if (data.UserId == null || data.UserId == 0)
                        {
                            return this.Error("请选择主管");

                        }
                        if (data.Year == null || data.Year == 0)
                        {
                            return this.Error("请选择年份");

                        }

                        List<Product> products = new List<Product>();
                        data.Items.ForEach(a =>
                        {
                            if (a.ProductId > 0)
                            {
                                Product product = new Product
                                {
                                    Id = a.ProductId,
                                    Name = a.ProductName
                                };
                                products.Add(product);
                            }
                        });

                        if (products.Count == 0)
                        {
                            return this.Error("没有商品明细");

                        }
                        else
                        {
                            foreach (var product in products)
                            {
                                var count = products.Where(p => p.Id == product.Id).ToList().Count;
                                if (count > 1)
                                {
                                    return this.Error("商品：" + product.Name + "有重复项");
                                }
                            }
                        }

                        giveQuota = _giveQuotaService.GetGiveQuotaById(store, giveQuotaId.Value);
                        if (giveQuotaId.HasValue && giveQuotaId.Value != 0 && giveQuota != null)
                        {
                            //原有额度配置
                            List<GiveQuotaOption> oldGiveQuotaOptions = _giveQuotaService.GetGiveQuotaOptionByQuotaId(giveQuotaId).ToList();
                            if (oldGiveQuotaOptions != null && oldGiveQuotaOptions.Count > 0)
                            {

                                //所有涉及商品，当前配置额度商品+原配置额度商品
                                List<int> productIds = new List<int>();
                                productIds.AddRange(products.Select(p => p.Id));
                                productIds.AddRange(oldGiveQuotaOptions.Select(od => od.ProductId).Distinct());
                                List<Product> allProducts = _productService.GetProductsByIds(store ?? 0, productIds.Distinct().ToArray()).ToList(); ;
                                var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(store, allProducts.GetProductBigStrokeSmallUnitIds());

                                oldGiveQuotaOptions.ForEach(od =>
                                {
                                    var product = allProducts.Where(al => al.Id == od.ProductId).FirstOrDefault();
                                    if (product != null)
                                    {

                                        //原有配置额度有使用
                                        var oldconversionQuantity = product.GetConversionQuantity(allOptions, od.UnitId ?? 0);
                                        var oldJanQuantity = (od.Jan ?? 0) * oldconversionQuantity;
                                        var oldFebQuantity = (od.Feb ?? 0) * oldconversionQuantity;
                                        var oldMarQuantity = (od.Mar ?? 0) * oldconversionQuantity;
                                        var oldAprQuantity = (od.Apr ?? 0) * oldconversionQuantity;
                                        var oldMayQuantity = (od.May ?? 0) * oldconversionQuantity;
                                        var oldJunQuantity = (od.Jun ?? 0) * oldconversionQuantity;
                                        var oldJulQuantity = (od.Jul ?? 0) * oldconversionQuantity;
                                        var oldAugQuantity = (od.Aug ?? 0) * oldconversionQuantity;
                                        var oldSepQuantity = (od.Sep ?? 0) * oldconversionQuantity;
                                        var oldOctQuantity = (od.Oct ?? 0) * oldconversionQuantity;
                                        var oldNovQuantity = (od.Nov ?? 0) * oldconversionQuantity;
                                        var oldDecQuantity = (od.Dec ?? 0) * oldconversionQuantity;

                                        if (oldJanQuantity != od.Jan_Balance || oldFebQuantity != od.Feb_Balance || oldMarQuantity != od.Mar_Balance
                                        || oldAprQuantity != od.Apr_Balance || oldMayQuantity != od.May_Balance || oldJunQuantity != od.Jun_Balance
                                        || oldJulQuantity != od.Jul_Balance || oldAugQuantity != od.Aug_Balance || oldSepQuantity != od.Sep_Balance
                                        || oldOctQuantity != od.Oct_Balance || oldNovQuantity != od.Nov_Balance || oldDecQuantity != od.Dec_Balance
                                        )
                                        {
                                            var newGiveQuotaOptionModel = data.Items.Where(it => it.ProductId == od.ProductId).FirstOrDefault();
                                            //删除验证（当前额度已经使用，不能删除）
                                            if (newGiveQuotaOptionModel == null)
                                            {
                                                errMsg += errMsg + "商品:" + (product != null ? product.Name : "") + "额度已使用，不能删除";
                                            }
                                            //修改验证（当前额度小于已经使用，不能修改）
                                            else
                                            {
                                                var conversionQuantity = product.GetConversionQuantity(allOptions, newGiveQuotaOptionModel.UnitId ?? 0);
                                                //当前额度最小单位数量
                                                var JanQuantity = (newGiveQuotaOptionModel.Jan ?? 0) * conversionQuantity;
                                                var FebQuantity = (newGiveQuotaOptionModel.Feb ?? 0) * conversionQuantity;
                                                var MarQuantity = (newGiveQuotaOptionModel.Mar ?? 0) * conversionQuantity;
                                                var AprQuantity = (newGiveQuotaOptionModel.Apr ?? 0) * conversionQuantity;
                                                var MayQuantity = (newGiveQuotaOptionModel.May ?? 0) * conversionQuantity;
                                                var JunQuantity = (newGiveQuotaOptionModel.Jun ?? 0) * conversionQuantity;
                                                var JulQuantity = (newGiveQuotaOptionModel.Jul ?? 0) * conversionQuantity;
                                                var AugQuantity = (newGiveQuotaOptionModel.Aug ?? 0) * conversionQuantity;
                                                var SepQuantity = (newGiveQuotaOptionModel.Sep ?? 0) * conversionQuantity;
                                                var OctQuantity = (newGiveQuotaOptionModel.Oct ?? 0) * conversionQuantity;
                                                var NovQuantity = (newGiveQuotaOptionModel.Nov ?? 0) * conversionQuantity;
                                                var DecQuantity = (newGiveQuotaOptionModel.Dec ?? 0) * conversionQuantity;

                                                #region 已使用额度大于当前配置额度
                                                //1
                                                if ((oldJanQuantity - od.Jan_Balance ?? 0) > JanQuantity)
                                                {

                                                    int.TryParse((oldJanQuantity - od.Jan_Balance ?? 0).ToString(), out int oldUseQuantity);
                                                    var oldUseQuantityConversionFormat = product.GetConversionFormat(allOptions, product.SmallUnitId, oldUseQuantity);


                                                    int.TryParse(JanQuantity.ToString(), out int newQuantity);
                                                    var newQuantityConversionFormat = product.GetConversionFormat(allOptions, product.SmallUnitId, newQuantity);

                                                    errMsg += errMsg + "商品:" + (product != null ? product.Name : "") + "1月额度已使用：" + oldUseQuantityConversionFormat + "大于当前配置：" + newQuantityConversionFormat + "，不能修改";
                                                }
                                                //2
                                                if ((oldFebQuantity - od.Feb_Balance ?? 0) > FebQuantity)
                                                {

                                                    int.TryParse((oldFebQuantity - od.Feb_Balance ?? 0).ToString(), out int oldUseQuantity);
                                                    var oldUseQuantityConversionFormat = product.GetConversionFormat(allOptions, product.SmallUnitId, oldUseQuantity);


                                                    int.TryParse(FebQuantity.ToString(), out int newQuantity);
                                                    var newQuantityConversionFormat = product.GetConversionFormat(allOptions, product.SmallUnitId, newQuantity);

                                                    errMsg += errMsg + "商品:" + (product != null ? product.Name : "") + "2月额度已使用：" + oldUseQuantityConversionFormat + "大于当前配置：" + newQuantityConversionFormat + "，不能修改";
                                                }
                                                //3
                                                if ((oldMarQuantity - od.Mar_Balance ?? 0) > MarQuantity)
                                                {

                                                    int.TryParse((oldMarQuantity - od.Mar_Balance ?? 0).ToString(), out int oldUseQuantity);
                                                    var oldUseQuantityConversionFormat = product.GetConversionFormat(allOptions, product.SmallUnitId, oldUseQuantity);


                                                    int.TryParse(MarQuantity.ToString(), out int newQuantity);
                                                    var newQuantityConversionFormat = product.GetConversionFormat(allOptions, product.SmallUnitId, newQuantity);

                                                    errMsg += errMsg + "商品:" + (product != null ? product.Name : "") + "3月额度已使用：" + oldUseQuantityConversionFormat + "大于当前配置：" + newQuantityConversionFormat + "，不能修改";
                                                }
                                                //4
                                                if ((oldAprQuantity - od.Apr_Balance ?? 0) > AprQuantity)
                                                {

                                                    int.TryParse((oldAprQuantity - od.Apr_Balance ?? 0).ToString(), out int oldUseQuantity);
                                                    var oldUseQuantityConversionFormat = product.GetConversionFormat(allOptions, product.SmallUnitId, oldUseQuantity);


                                                    int.TryParse(AprQuantity.ToString(), out int newQuantity);
                                                    var newQuantityConversionFormat = product.GetConversionFormat(allOptions, product.SmallUnitId, newQuantity);

                                                    errMsg += errMsg + "商品:" + (product != null ? product.Name : "") + "4月额度已使用：" + oldUseQuantityConversionFormat + "大于当前配置：" + newQuantityConversionFormat + "，不能修改";
                                                }
                                                //5
                                                if ((oldMayQuantity - od.May_Balance ?? 0) > MayQuantity)
                                                {

                                                    int.TryParse((oldMayQuantity - od.May_Balance ?? 0).ToString(), out int oldUseQuantity);
                                                    var oldUseQuantityConversionFormat = product.GetConversionFormat(allOptions, product.SmallUnitId, oldUseQuantity);


                                                    int.TryParse(MayQuantity.ToString(), out int newQuantity);
                                                    var newQuantityConversionFormat = product.GetConversionFormat(allOptions, product.SmallUnitId, newQuantity);

                                                    errMsg += errMsg + "商品:" + (product != null ? product.Name : "") + "5月额度已使用：" + oldUseQuantityConversionFormat + "大于当前配置：" + newQuantityConversionFormat + "，不能修改";
                                                }
                                                //6
                                                if ((oldJunQuantity - od.Jun_Balance ?? 0) > JunQuantity)
                                                {

                                                    int.TryParse((oldJunQuantity - od.Jun_Balance ?? 0).ToString(), out int oldUseQuantity);
                                                    var oldUseQuantityConversionFormat = product.GetConversionFormat(allOptions, product.SmallUnitId, oldUseQuantity);


                                                    int.TryParse(JunQuantity.ToString(), out int newQuantity);
                                                    var newQuantityConversionFormat = product.GetConversionFormat(allOptions, product.SmallUnitId, newQuantity);

                                                    errMsg += errMsg + "商品:" + (product != null ? product.Name : "") + "6月额度已使用：" + oldUseQuantityConversionFormat + "大于当前配置：" + newQuantityConversionFormat + "，不能修改";
                                                }
                                                //7
                                                if ((oldJulQuantity - od.Jul_Balance ?? 0) > JulQuantity)
                                                {

                                                    int.TryParse((oldJulQuantity - od.Jul_Balance ?? 0).ToString(), out int oldUseQuantity);
                                                    var oldUseQuantityConversionFormat = product.GetConversionFormat(allOptions, product.SmallUnitId, oldUseQuantity);


                                                    int.TryParse(JulQuantity.ToString(), out int newQuantity);
                                                    var newQuantityConversionFormat = product.GetConversionFormat(allOptions, product.SmallUnitId, newQuantity);

                                                    errMsg += errMsg + "商品:" + (product != null ? product.Name : "") + "7月额度已使用：" + oldUseQuantityConversionFormat + "大于当前配置：" + newQuantityConversionFormat + "，不能修改";
                                                }
                                                //8
                                                if ((oldAugQuantity - od.Aug_Balance ?? 0) > AugQuantity)
                                                {

                                                    int.TryParse((oldAugQuantity - od.Aug_Balance ?? 0).ToString(), out int oldUseQuantity);
                                                    var oldUseQuantityConversionFormat = product.GetConversionFormat(allOptions, product.SmallUnitId, oldUseQuantity);


                                                    int.TryParse(AugQuantity.ToString(), out int newQuantity);
                                                    var newQuantityConversionFormat = product.GetConversionFormat(allOptions, product.SmallUnitId, newQuantity);

                                                    errMsg += errMsg + "商品:" + (product != null ? product.Name : "") + "8月额度已使用：" + oldUseQuantityConversionFormat + "大于当前配置：" + newQuantityConversionFormat + "，不能修改";
                                                }
                                                //9
                                                if ((oldSepQuantity - od.Sep_Balance ?? 0) > SepQuantity)
                                                {

                                                    int.TryParse((oldSepQuantity - od.Sep_Balance ?? 0).ToString(), out int oldUseQuantity);
                                                    var oldUseQuantityConversionFormat = product.GetConversionFormat(allOptions, product.SmallUnitId, oldUseQuantity);


                                                    int.TryParse(SepQuantity.ToString(), out int newQuantity);
                                                    var newQuantityConversionFormat = product.GetConversionFormat(allOptions, product.SmallUnitId, newQuantity);

                                                    errMsg += errMsg + "商品:" + (product != null ? product.Name : "") + "9月额度已使用：" + oldUseQuantityConversionFormat + "大于当前配置：" + newQuantityConversionFormat + "，不能修改";
                                                }
                                                //10
                                                if ((oldOctQuantity - od.Oct_Balance ?? 0) > OctQuantity)
                                                {
                                                    int.TryParse((oldOctQuantity - od.Oct_Balance ?? 0).ToString(), out int oldUseQuantity);
                                                    var oldUseQuantityConversionFormat = product.GetConversionFormat(allOptions, product.SmallUnitId, oldUseQuantity);

                                                    int.TryParse(OctQuantity.ToString(), out int newQuantity);
                                                    var newQuantityConversionFormat = product.GetConversionFormat(allOptions, product.SmallUnitId, newQuantity);

                                                    errMsg += errMsg + "商品:" + (product != null ? product.Name : "") + "10月额度已使用：" + oldUseQuantityConversionFormat + "大于当前配置：" + newQuantityConversionFormat + "，不能修改";
                                                }
                                                //11
                                                if ((oldNovQuantity - od.Nov_Balance ?? 0) > NovQuantity)
                                                {

                                                    int.TryParse((oldNovQuantity - od.Nov_Balance ?? 0).ToString(), out int oldUseQuantity);
                                                    var oldUseQuantityConversionFormat = product.GetConversionFormat(allOptions, product.SmallUnitId, oldUseQuantity);


                                                    int.TryParse(NovQuantity.ToString(), out int newQuantity);
                                                    var newQuantityConversionFormat = product.GetConversionFormat(allOptions, product.SmallUnitId, newQuantity);

                                                    errMsg += errMsg + "商品:" + (product != null ? product.Name : "") + "11月额度已使用：" + oldUseQuantityConversionFormat + "大于当前配置：" + newQuantityConversionFormat + "，不能修改";
                                                }
                                                //12
                                                if ((oldDecQuantity - od.Dec_Balance ?? 0) > DecQuantity)
                                                {

                                                    int.TryParse((oldDecQuantity - od.Dec_Balance ?? 0).ToString(), out int oldUseQuantity);
                                                    var oldUseQuantityConversionFormat = product.GetConversionFormat(allOptions, product.SmallUnitId, oldUseQuantity);


                                                    int.TryParse(DecQuantity.ToString(), out int newQuantity);
                                                    var newQuantityConversionFormat = product.GetConversionFormat(allOptions, product.SmallUnitId, newQuantity);

                                                    errMsg += errMsg + "商品:" + (product != null ? product.Name : "") + "12月额度已使用：" + oldUseQuantityConversionFormat + "大于当前配置：" + newQuantityConversionFormat + "，不能修改";
                                                }

                                                #endregion


                                            }
                                        }
                                    }
                                });
                            }
                        }
                        else
                        {
                            var give = _giveQuotaService.GetGiveQuotas(data.UserId, data.Year).FirstOrDefault();
                            if (give != null)
                            {
                                return this.Error("配置已经存在");

                            }
                        }

                        if (!string.IsNullOrEmpty(errMsg))
                        {
                            return this.Error(errMsg);
                        }

                        #endregion

                        ////业务逻辑
                        var dataTo = data.ToEntity<GiveQuotaUpdate>();
                        dataTo.Items = data.Items.Select(it =>
                        {
                            return it.ToEntity<GiveQuotaOption>();
                        }).ToList();

                        //RedLock
                        string lockKey = string.Format(DCMSCachingDefaults.RedisDataReSubmitKey, Request.GetUrl(), store ?? 0, userId, CommonHelper.MD5(JsonConvert.SerializeObject(data)));

                        var result = await _locker.PerformActionWithLockAsync(lockKey,
                            TimeSpan.FromSeconds(30),
                            TimeSpan.FromSeconds(10),
                            TimeSpan.FromSeconds(1),
                            () => _giveQuotaService.BillCreateOrUpdate(store ?? 0, userId ?? 0, giveQuotaId, giveQuota, dataTo, dataTo.Items, _userService.IsAdmin(store ?? 0, userId ?? 0)));

                        return this.Successful("", result);
                    }
                    else
                    {
                        return this.Error("没有商品明细！");
                    }
                }
                catch (Exception ex)
                {
                    //活动日志
                    _userActivityService.InsertActivity("CreateOrUpdate", Resources.Bill_CreateOrUpdateFailed, userId ?? 0);
                    return this.Error(ex.Message);
                }
            });
        }
    }
}
