using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Products;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Products;
using DCMS.Services.Users;
using DCMS.ViewModel.Models.Products;
using DCMS.ViewModel.Models.Users;
using DCMS.Web.Framework.Mvc.Filters;
using DCMS.Web.Infrastructure.Mapper.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace DCMS.Web.Controllers
{

    /// <summary>
    /// 赠品额度设置
    /// </summary>
    public class GiveQuotaController : BasePublicController
    {
        private readonly IProductService _productService;
        private readonly IUserActivityService _userActivityService;
        private readonly IUserService _userService;
        private readonly IGiveQuotaService _giveQuotaService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IRedLocker _locker;

        public GiveQuotaController(
            IProductService productService,
            IUserActivityService userActivityService,
            IWorkContext workContext,
            IUserService userService,
            IStoreContext storeContext,
            ILogger loggerService,
            INotificationService notificationService,
            ISpecificationAttributeService specificationAttributeService,
            IGiveQuotaService giveQuotaService,
            IRedLocker locker
            ) : base(workContext, loggerService, storeContext, notificationService)
        {
            _userActivityService = userActivityService;
            _productService = productService;
            _userService = userService;
            _specificationAttributeService = specificationAttributeService;
            _giveQuotaService = giveQuotaService;
            _locker = locker;
        }

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        /// <summary>
        /// 配置列表
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="year"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.GiftAmountView)]
        public IActionResult List(int? userId, int? year, string remark)
        {
            var listModel = new GiveQuotaListModel();

            if (year.HasValue)
            {
                listModel.Year = year.Value;
            }
            else
            {
                listModel.Year = DateTime.Now.Year;
            }

            //备注
            listModel.Remark = remark;

            var giveQuota = _giveQuotaService.GetGiveQuotas(userId.HasValue ? userId : curUser.Id, listModel.Year).FirstOrDefault();
            if (giveQuota != null)
            {
                listModel.GiveQuotaId = giveQuota.Id;
            }
            else
            {
                listModel.GiveQuotaId = 0;
            }

            listModel.Items = _giveQuotaService.GetGiveQuotas(userId.HasValue ? userId : curUser.Id, listModel.Year).Select(g => g.ToModel<GiveQuotaModel>()).ToList();
            listModel.Managers = _userService.GetStoreManagers(curStore?.Id ?? 0)
                .Select(u =>
                {
                    var m = u.ToModel<UserModel>();
                    m.UserRealName = u.UserRealName;
                    return m;
                })
                .ToList();

            if (userId.HasValue)
            {
                listModel.UserId = userId;
            }
            else
            {
                listModel.UserId = listModel.Managers.Count > 0 ? listModel.Managers.First().Id : 0;
            }

            return View(listModel);
        }

        /// <summary>
        /// 异步获取配置
        /// </summary>
        /// <param name="year"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<JsonResult> AsyncList(int year, int userId)
        {
            return await Task.Run(() =>
            {
                var giveQuota = _giveQuotaService.GetGiveQuotaByStoreIdUserIdYear(curStore == null ? 0 : curStore.Id, userId, year);
                //var options = _giveQuotaService.GetGiveQuotaOptions(year, userId);
                var options = _giveQuotaService.GetGiveQuotaBalances(curStore.Id, year, userId, giveQuota.Id);
                var allProducts = _productService.GetProductsByIds(curStore.Id, options.Select(pr => pr.ProductId).Distinct().ToArray());
                var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());

                var optionModels = options.Select(o =>
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
                          var option = allOptions.Where(ao => ao.Id == m.UnitId).FirstOrDefault();
                          m.UnitName = option != null ? option.Name : "";
                          m.Jan = m.Jan ?? 0;
                          m.Feb = m.Feb ?? 0;
                          m.Mar = m.Mar ?? 0;
                          m.Apr = m.Apr ?? 0;
                          m.May = m.May ?? 0;
                          m.Jun = m.Jun ?? 0;
                          m.Jul = m.Jul ?? 0;
                          m.Aug = m.Aug ?? 0;
                          m.Sep = m.Sep ?? 0;
                          m.Oct = m.Oct ?? 0;
                          m.Nov = m.Nov ?? 0;
                          m.Dec = m.Dec ?? 0;
                      }

                      m.SmallUnitName = m.Units.Keys.Select(k => k).ToArray()[0];

                      m.Total = ((m.Jan ?? 0) + (m.Feb ?? 0) + (m.Mar ?? 0) + (m.Apr ?? 0) + (m.May ?? 0) + (m.Jun ?? 0) + (m.Jul ?? 0) + (m.Aug ?? 0) + (m.Sep ?? 0) + (m.Oct ?? 0) + (m.Nov) + (m.Dec ?? 0));
                      return m;

                  }).ToList();

                //10行增加默认值
                if (optionModels == null || optionModels.Count == 0)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        GiveQuotaOptionModel giveQuotaOptionModel = new GiveQuotaOptionModel
                        {
                            ProductName = ""
                        };
                        optionModels.Add(giveQuotaOptionModel);
                    }
                    return Json(new
                    {
                        Success = true,
                        total = 10,
                        rows = optionModels,
                        remark = giveQuota == null ? "" : (giveQuota.Remark ?? "")
                    });
                }

                return Json(new
                {
                    Success = true,
                    total = options.Count,
                    rows = optionModels,
                    remark = giveQuota == null ? "" : (giveQuota.Remark ?? "")
                });
            });
        }

        [HttpPost]
        [AuthCode((int)AccessGranularityEnum.GiftAmountUpdateSave)]
        public async Task<JsonResult> CreateOrUpdate(GiveQuotaUpdateModel data, int? giveQuotaId)
        {
            try
            {
                if (data != null)
                {

                    string errMsg = string.Empty;
                    var giveQuota = new GiveQuota();

                    if (PeriodLocked(DateTime.Now))
                    {
                        return Warning("锁账期间,禁止业务操作.");
                    }

                    if (PeriodClosed(DateTime.Now))
                    {
                        return Warning("会计期间已结账,禁止业务操作.");
                    }


                    #region 验证

                    if (data.UserId == null || data.UserId == 0)
                    {
                        return Warning("请选择主管");
                    }
                    if (data.Year == null || data.Year == 0)
                    {
                        return Warning("请选择年份");
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
                        return Warning("没有商品明细");
                    }
                    else
                    {
                        foreach (var product in products)
                        {
                            var count = products.Count(p => p.Id == product.Id);
                            if (count > 1)
                            {
                                return Warning("商品有重复项");
                            }
                        }
                    }

                    giveQuota = _giveQuotaService.GetGiveQuotaById(curStore.Id, giveQuotaId.Value);
                    if (giveQuotaId.HasValue && giveQuotaId.Value != 0 && giveQuota != null)
                    {
                        //原有额度配置
                        //List<GiveQuotaOption> oldGiveQuotaOptions = _giveQuotaService.GetGiveQuotaOptionByQuotaId(giveQuotaId).ToList();
                        List<GiveQuotaOption> oldGiveQuotaOptions = _giveQuotaService.GetGiveQuotaBalances(curStore.Id, data.Year??0, data.UserId??0, giveQuotaId).ToList();
                        if (oldGiveQuotaOptions != null && oldGiveQuotaOptions.Count > 0)
                        {

                            //所有涉及商品，当前配置额度商品+原配置额度商品
                            List<int> productIds = new List<int>();
                            productIds.AddRange(products.Select(p => p.Id));
                            productIds.AddRange(oldGiveQuotaOptions.Select(od => od.ProductId).Distinct());
                            List<Product> allProducts = _productService.GetProductsByIds(curStore.Id, productIds.Distinct().ToArray()).ToList();
                            var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());

                            oldGiveQuotaOptions.ForEach(od =>
                            {
                                var product = allProducts.Where(al => al.Id == od.ProductId).FirstOrDefault();
                                if (product != null)
                                {
                                    decimal? ojan = od.Jan, ofeb = od.Feb, omar = od.Mar, oapr = od.Apr, omay = od.May, ojun = od.Jun, ojul = od.Jul, oaug = od.Aug, osep = od.Sep, ooct = od.Oct, onov = od.Nov, odec = od.Dec;

                                    if (product.SmallUnitId != od.UnitId) //转换双精度量
                                    {
                                        var oldConversionQuantity = product.GetConversionQuantity(allOptions, od.UnitId ?? 0);

                                        ojan = CalcDoubleQuality(od.Jan == null ? "0" : od.Jan.Value.ToString("0.0"), oldConversionQuantity);
                                        ofeb = CalcDoubleQuality(od.Feb == null ? "0" : od.Feb.Value.ToString("0.0"), oldConversionQuantity);
                                        omar = CalcDoubleQuality(od.Mar == null ? "0" : od.Mar.Value.ToString("0.0"), oldConversionQuantity);
                                        oapr = CalcDoubleQuality(od.Apr == null ? "0" : od.Apr.Value.ToString("0.0"), oldConversionQuantity);
                                        omay = CalcDoubleQuality(od.May == null ? "0" : od.May.Value.ToString("0.0"), oldConversionQuantity);
                                        ojun = CalcDoubleQuality(od.Jun == null ? "0" : od.Jun.Value.ToString("0.0"), oldConversionQuantity);
                                        ojul = CalcDoubleQuality(od.Jul == null ? "0" : od.Jul.Value.ToString("0.0"), oldConversionQuantity);
                                        oaug = CalcDoubleQuality(od.Aug == null ? "0" : od.Aug.Value.ToString("0.0"), oldConversionQuantity);
                                        osep = CalcDoubleQuality(od.Sep == null ? "0" : od.Sep.Value.ToString("0.0"), oldConversionQuantity);
                                        ooct = CalcDoubleQuality(od.Oct == null ? "0" : od.Oct.Value.ToString("0.0"), oldConversionQuantity);
                                        onov = CalcDoubleQuality(od.Nov == null ? "0" : od.Nov.Value.ToString("0.0"), oldConversionQuantity);
                                        odec = CalcDoubleQuality(od.Dec == null ? "0" : od.Dec.Value.ToString("0.0"), oldConversionQuantity);
                                    }

                                    var newOption = data.Items.Where(it => it.ProductId == od.ProductId).FirstOrDefault();
                                    //删除验证（当前额度已经使用，不能删除）
                                    if (newOption == null && (ojan != od.Jan_Balance || ofeb !=od.Feb_Balance || omar != od.Mar_Balance || oapr != od.Apr_Balance || omay != od.May_Balance || ojun != od.Jun_Balance || ojul != od.Jul_Balance || oaug != od.Aug_Balance || osep != od.Sep_Balance || ooct != od.Oct_Balance || onov != od.Nov_Balance || odec != od.Dec_Balance))
                                    {
                                        errMsg = "商品额度已使用，不能删除";
                                    }
                                    //修改验证（当前额度小于已经使用，不能修改）
                                    else
                                    {
                                        var conversionQuantity = product.GetConversionQuantity(allOptions, newOption.UnitId ?? 0);
                                        decimal? njan = newOption.Jan, nfeb = newOption.Feb, nmar = newOption.Mar, napr = newOption.Apr, nmay = newOption.May, njun = newOption.Jun, njul = newOption.Jul, naug = newOption.Aug, nsep = newOption.Sep, noct = newOption.Oct, nnov = newOption.Nov, ndec = newOption.Dec;
                                        //转换双精度量
                                        if (product.SmallUnitId != newOption.UnitId)
                                        {
                                            njan= CalcDoubleQuality(newOption.Jan == null ? "0" : newOption.Jan.Value.ToString("0.0"), conversionQuantity);
                                            nfeb = CalcDoubleQuality(newOption.Feb == null ? "0" : newOption.Feb.Value.ToString("0.0"), conversionQuantity);
                                            nmar = CalcDoubleQuality(newOption.Mar == null ? "0" : newOption.Mar.Value.ToString("0.0"), conversionQuantity);
                                            napr = CalcDoubleQuality(newOption.Apr == null ? "0" : newOption.Apr.Value.ToString("0.0"), conversionQuantity);
                                            nmay = CalcDoubleQuality(newOption.May == null ? "0" : newOption.May.Value.ToString("0.0"), conversionQuantity);
                                            njun = CalcDoubleQuality(newOption.Jun == null ? "0" : newOption.Jun.Value.ToString("0.0"), conversionQuantity);
                                            njul = CalcDoubleQuality(newOption.Jul == null ? "0" : newOption.Jul.Value.ToString("0.0"), conversionQuantity);
                                            naug = CalcDoubleQuality(newOption.Aug == null ? "0" : newOption.Aug.Value.ToString("0.0"), conversionQuantity);
                                            nsep = CalcDoubleQuality(newOption.Sep == null ? "0" : newOption.Sep.Value.ToString("0.0"), conversionQuantity);
                                            noct = CalcDoubleQuality(newOption.Oct == null ? "0" : newOption.Oct.Value.ToString("0.0"), conversionQuantity);
                                            nnov = CalcDoubleQuality(newOption.Nov == null ? "0" : newOption.Nov.Value.ToString("0.0"), conversionQuantity);
                                            ndec = CalcDoubleQuality(newOption.Dec == null ? "0" : newOption.Dec.Value.ToString("0.0"), conversionQuantity);
                                        }

                                        #region 已使用额度大于当前配置额度
                                        //1
                                        if ((ojan - od.Jan_Balance) > njan)
                                        {
                                            errMsg = "商品已使用额度大于当前配额，不能修改";
                                        }
                                        //2
                                        if ((ofeb - od.Feb_Balance ?? 0) > nfeb)
                                        {
                                            errMsg = "商品已使用额度大于当前配额，不能修改";
                                        }
                                        //3
                                        if ((omar - od.Mar_Balance ?? 0) > nmar)
                                        {
                                            errMsg = "商品已使用额度大于当前配额，不能修改";
                                        }
                                        //4
                                        if ((oapr - od.Apr_Balance ?? 0) > napr)
                                        {
                                            errMsg = "商品已使用额度大于当前配额，不能修改";
                                        }
                                        //5
                                        if ((omay - od.May_Balance ?? 0) > nmay)
                                        {
                                            errMsg = "商品已使用额度大于当前配额，不能修改";
                                        }
                                        //6
                                        if ((ojun - od.Jun_Balance ?? 0) > njun)
                                        {
                                            errMsg = "商品已使用额度大于当前配额，不能修改";
                                        }
                                        //7
                                        if ((ojul - od.Jul_Balance ?? 0) > njul)
                                        {
                                            errMsg = "商品已使用额度大于当前配额，不能修改";
                                        }
                                        //8
                                        if ((oaug - od.Aug_Balance ?? 0) > naug)
                                        {
                                            errMsg = "商品已使用额度大于当前配额，不能修改";
                                        }
                                        //9
                                        if ((osep - od.Sep_Balance ?? 0) > nsep)
                                        {
                                            errMsg = "商品已使用额度大于当前配额，不能修改";
                                        }
                                        //10
                                        if ((ooct - od.Oct_Balance ?? 0) > noct)
                                        {
                                            errMsg = "商品已使用额度大于当前配额，不能修改";
                                        }
                                        //11
                                        if ((onov - od.Nov_Balance ?? 0) > nnov)
                                        {
                                            errMsg = "商品已使用额度大于当前配额，不能修改";
                                        }
                                        //12
                                        if ((odec - od.Dec_Balance ?? 0) > ndec)
                                        {
                                            errMsg = "商品已使用额度大于当前配额，不能修改";
                                        }
                                        #endregion
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
                            return Warning("配置已经存在");
                        }
                    }

                    if (!string.IsNullOrEmpty(errMsg))
                    {
                        return Warning(errMsg);
                    }


                    #endregion

                    //业务逻辑
                    var dataTo = data.ToEntity<GiveQuotaUpdate>();
                    dataTo.Items = data.Items.Select(it =>
                    {
                        return it.ToEntity<GiveQuotaOption>();
                    }).ToList();

                    //RedLock
                    var result = await _locker.PerformActionWithLockAsync(LockKey(data),
                        TimeSpan.FromSeconds(30),
                        TimeSpan.FromSeconds(10),
                        TimeSpan.FromSeconds(1),
                        () => _giveQuotaService.BillCreateOrUpdate(curStore.Id, curUser.Id, giveQuotaId, giveQuota, dataTo, dataTo.Items, _userService.IsAdmin(curStore.Id, curUser.Id)));
                }
            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("CreateOrUpdate", Resources.Bill_CreateOrUpdateFailed, curUser.Id);
                _notificationService.SuccessNotification(Resources.Bill_CreateOrUpdateFailed);
                return Error(ex.Message);
            }

            //活动日志
            _userActivityService.InsertActivity("UpdateGiveQuota", "配置更新", curUser.Id);
            _notificationService.SuccessNotification("配置更新成功");
            return Successful("配置更新成功");
        }


        public JsonResult LoadGiveQuotData(int employeeId, int year, int month, int leaderId)
        {
            try
            {
                bool fg = true;
                string errMsg = string.Empty;

                List<GiveQuotaOption> datas = new List<GiveQuotaOption>();
                datas = _giveQuotaService.GetGiveQuotaOptions(year, leaderId).ToList();
                var allPorducts = _productService.GetProductsByIds(curStore.Id, datas.Select(d => d.ProductId).Distinct().ToArray());
                var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allPorducts.GetProductBigStrokeSmallUnitIds());
                List<GiveQuotaOptionModel> datas2 = datas.Select(d =>
                {
                    var m = d.ToModel<GiveQuotaOptionModel>();
                    Product product = allPorducts.Where(ap => ap.Id == d.ProductId).FirstOrDefault();
                    m.ProductName = product == null ? "" : product.Name;
                    m.BarCode = product != null ? product.SmallBarCode : "";
                    //m.UnitName = allOptions.Where(s => s.Id == m.UnitId).Select(s => s.Name).FirstOrDefault();
                    var option = allOptions.Where(al => al.Id == (m.UnitId ?? 0)).FirstOrDefault();
                    m.UnitName = option == null ? "" : option.Name;
                    m.UnitConversion = product.GetProductUnitConversion(allOptions);
                    //m.Units = product.GetProductUnits(_productService, _specificationAttributeService);
                    m.Units = product.GetProductUnits(allOptions);

                    //具体到月
                    switch (month)
                    {
                        case 1:
                            m.Total = d.Jan_Balance ?? 0;
                            break;
                        case 2:
                            m.Total = d.Feb_Balance ?? 0;
                            break;
                        case 3:
                            m.Total = d.Mar_Balance ?? 0;
                            break;
                        case 4:
                            m.Total = d.Apr_Balance ?? 0;
                            break;
                        case 5:
                            m.Total = d.May_Balance ?? 0;
                            break;
                        case 6:
                            m.Total = d.Jun_Balance ?? 0;
                            break;
                        case 7:
                            m.Total = d.Jul_Balance ?? 0;
                            break;
                        case 8:
                            m.Total = d.Aug_Balance ?? 0;
                            break;
                        case 9:
                            m.Total = d.Sep_Balance ?? 0;
                            break;
                        case 10:
                            m.Total = d.Oct_Balance ?? 0;
                            break;
                        case 11:
                            m.Total = d.Nov_Balance ?? 0;
                            break;
                        case 12:
                            m.Total = d.Dec_Balance ?? 0;
                            break;
                        default:
                            m.Total = 0;
                            break;
                    }
                    return m;
                }).ToList();

                //可用数量大于0过滤
                datas2 = datas2.Where(da => da.Total > 0).ToList();

                if (fg)
                {
                    return Successful("加载主管赠品数据成功", datas2);
                }
                else
                {
                    return Warning(errMsg);
                }

            }
            catch (Exception ex)
            {
                return Warning(ex.ToString());
            }
        }

        protected decimal CalcDoubleQuality(string num, int conversionQuantity)
        {
            if (num.IndexOf(".") > -1)
            {
                string[] idArray = num.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                double[] idsDoubles = Array.ConvertAll<string, double>(idArray, s => Convert.ToDouble(s));
                int[] idInts = Array.ConvertAll<double, int>(idsDoubles, s => Convert.ToInt32(s));

                return idInts[0] * conversionQuantity + idInts[1];
            }

            return int.Parse(num) * conversionQuantity;
        }
    }
}
