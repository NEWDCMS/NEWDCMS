using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Common;
using DCMS.Core.Domain.Plan;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.plan;
using DCMS.Services.Products;
using DCMS.ViewModel.Models.Plan;
using DCMS.ViewModel.Models.Products;
using DCMS.Web.Framework.Extensions;
using DCMS.Web.Framework.Mvc.Filters;
using DCMS.Web.Infrastructure.Mapper.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace DCMS.Web.Controllers
{

    /// <summary>
    /// 用于提成管理
    /// </summary>
    public class PercentageController : BasePublicController
    {
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IBrandService _brandService;
        private readonly IPercentagePlanService _percentagePlanService;
        private readonly IPercentageService _percentageService;
        
        private readonly ICategoryService _productCategoryService;
        private readonly IRedLocker _locker;

        public PercentageController(ICategoryService categoryService,
            IProductService productService,
            IWorkContext workContext,
            IBrandService brandService,
            ISpecificationAttributeService specificationAttributeService,
            IPercentagePlanService percentagePlanService,
            IPercentageService percentageService,
            IStaticCacheManager cacheManager,
            ICategoryService productCategoryService,
            ILogger loggerService,
            INotificationService notificationService,
            IStoreContext storeContext,
            IRedLocker locker
            ) : base(workContext, loggerService, storeContext, notificationService)
        {
            _categoryService = categoryService;
            _productService = productService;
            _brandService = brandService;
            _specificationAttributeService = specificationAttributeService;
            _percentagePlanService = percentagePlanService;
            _percentageService = percentageService;
            
            _productCategoryService = productCategoryService;
            _locker = locker;
        }


        public IActionResult Index()
        {
            return RedirectToAction("List");
        }


        /// <summary>
        /// 提成列表
        /// </summary>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.PercentageListView)]
        public IActionResult List(int planId = 0)
        {
            var model = new ProductListModel
            {
                CurrentPercentagePlan = planId
            };

            var plans = _percentagePlanService.GetAllPercentagePlans(curStore?.Id ?? 0);
            model.PercentagePlans = plans.Select(p => p.ToModel<PercentagePlanModel>()).ToList();

            return View(model);
        }


        /// <summary>
        /// 异步获取FancyTree
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<JsonResult> GetFancyTree(int plan = 0)
        {

            return await Task.Run(() =>
            {
                var trees = GetCategoryList(curStore?.Id ?? 0, plan, 0);
                return Json(trees);
            });
        }



        /// <summary>
        /// 递归获取类别树
        /// </summary>
        /// <param name="store">经销商</param>
        /// <param name="Id"></param>
        /// <returns></returns>
        [NonAction]
        public List<FancyTreeExt> GetCategoryList(int? store, int plan, int Id)
        {
            List<FancyTreeExt> fancyTrees = new List<FancyTreeExt>();
            var perentList = _productCategoryService.GetAllCategoriesByParentCategoryId(store.Value, Id);
            if (perentList != null && perentList.Count > 0)
            {
                foreach (var b in perentList)
                {
                    var percentage = _percentageService.GetPercentageByCatagory(store ?? 0, plan, b.Id);
                    var tempList = GetCategoryList(store.Value, plan, b.Id);

                    var title2 = new List<string>();
                    var title3 = new List<string>();

                    if (percentage != null)
                    {
                        switch (percentage.CalCulateMethodId)
                        {
                            //销售额百分比
                            case 1:
                                {
                                    decimal salesPercent = percentage.SalesPercent ?? 0;
                                    decimal returnPercent = percentage.ReturnPercent ?? 0;

                                    if (salesPercent != 0)
                                    {
                                        title2.Add($"{salesPercent:0.00}%");
                                    }

                                    if (returnPercent != 0)
                                    {
                                        title3.Add($"{returnPercent:0.00}%");
                                    }
                                }
                                break;
                            //销售额变化百分比
                            case 2:
                                {
                                    var rangs = _percentageService.GetAllPercentageRangeOptionsByPercentageId(store ?? 0, percentage.Id);
                                    rangs.ToList().ForEach(r =>
                                    {

                                        decimal salesPercent = r.SalesPercent ?? 0;
                                        decimal returnPercent = r.ReturnPercent ?? 0;

                                        if (salesPercent != 0)
                                        {
                                            title2.Add($"{salesPercent:0.00}%");
                                        }

                                        if (returnPercent != 0)
                                        {
                                            title3.Add($"{returnPercent:0.00}%");
                                        }
                                    });

                                }
                                break;
                            //销售额分段变化百分比
                            case 3:
                                {
                                    var rangs = _percentageService.GetAllPercentageRangeOptionsByPercentageId(store ?? 0, percentage.Id);
                                    rangs.ToList().ForEach(r =>
                                    {

                                        decimal salesPercent = r.SalesPercent ?? 0;
                                        decimal returnPercent = r.ReturnPercent ?? 0;

                                        if (salesPercent != 0)
                                        {
                                            title2.Add($"{salesPercent:0.00}%");
                                        }

                                        if (returnPercent != 0)
                                        {
                                            title3.Add($"{returnPercent:0.00}%");
                                        }
                                    });

                                }
                                break;
                            //销售数量每件固定额
                            case 4:
                                {
                                    decimal salesAmount = percentage.SalesAmount ?? 0;
                                    decimal returnAmount = percentage.ReturnAmount ?? 0;

                                    if (salesAmount != 0)
                                    {
                                        title2.Add($"{salesAmount:0.00}元");
                                    }

                                    if (returnAmount != 0)
                                    {
                                        title3.Add($"{returnAmount:0.00}元");
                                    }
                                }
                                break;
                            //按销售数量变化每件提成金额
                            case 5:
                                {
                                    var rangs = _percentageService.GetAllPercentageRangeOptionsByPercentageId(store ?? 0, percentage.Id);
                                    rangs.ToList().ForEach(r =>
                                    {

                                        decimal salesPercent = r.SalesPercent ?? 0;
                                        decimal returnPercent = r.ReturnPercent ?? 0;

                                        if (salesPercent != 0)
                                        {
                                            title2.Add($"{salesPercent:0.00}元");
                                        }

                                        if (returnPercent != 0)
                                        {
                                            title3.Add($"{returnPercent:0.00}元");
                                        }
                                    });
                                }
                                break;
                            //按销售数量分段变化每件提成金额
                            case 6:
                                {
                                    var rangs = _percentageService.GetAllPercentageRangeOptionsByPercentageId(store ?? 0, percentage.Id);
                                    rangs.ToList().ForEach(r =>
                                    {

                                        decimal salesPercent = r.SalesPercent ?? 0;
                                        decimal returnPercent = r.ReturnPercent ?? 0;

                                        if (salesPercent != 0)
                                        {
                                            title2.Add($"{salesPercent:0.00}元");
                                        }

                                        if (returnPercent != 0)
                                        {
                                            title3.Add($"{returnPercent:0.00}元");
                                        }
                                    });
                                }
                                break;
                            //利润额百分比
                            case 7:
                                {
                                    decimal salesPercent = percentage.SalesPercent ?? 0;
                                    decimal returnPercent = percentage.ReturnPercent ?? 0;

                                    if (salesPercent != 0)
                                    {
                                        title2.Add($"{salesPercent:0.00}%");
                                    }

                                    if (returnPercent != 0)
                                    {
                                        title3.Add($"{returnPercent:0.00}%");
                                    }
                                }
                                break;
                            //利润额变化百分比
                            case 8:
                                {
                                    var rangs = _percentageService.GetAllPercentageRangeOptionsByPercentageId(store ?? 0, percentage.Id);
                                    rangs.ToList().ForEach(r =>
                                    {

                                        decimal salesPercent = r.SalesPercent ?? 0;
                                        decimal returnPercent = r.ReturnPercent ?? 0;

                                        if (salesPercent != 0)
                                        {
                                            title2.Add($"{salesPercent:0.00}元");
                                        }

                                        if (returnPercent != 0)
                                        {
                                            title3.Add($"{returnPercent:0.00}元");
                                        }
                                    });
                                }
                                break;
                            //利润额分段变化百分比
                            case 9:
                                {
                                    var rangs = _percentageService.GetAllPercentageRangeOptionsByPercentageId(store ?? 0, percentage.Id);
                                    rangs.ToList().ForEach(r =>
                                    {

                                        decimal salesPercent = r.SalesPercent ?? 0;
                                        decimal returnPercent = r.ReturnPercent ?? 0;

                                        if (salesPercent != 0)
                                        {
                                            title2.Add($"{salesPercent:0.00}元");
                                        }

                                        if (returnPercent != 0)
                                        {
                                            title3.Add($"{returnPercent:0.00}元");
                                        }
                                    });
                                }
                                break;
                        }
                    }

                    var node = new FancyTreeExt
                    {
                        id = b.Id,
                        id1 = b.PercentageId ?? 0,
                        title = b.Name,
                        expanded = true,
                        title1 = percentage != null ? CommonHelper.GetEnumDescription<PercentageCalCulateMethod>(percentage.CalCulateMethod) : "",
                        title2 = string.Join(",", title2.Where(t => !string.IsNullOrEmpty(t)).ToArray()),
                        title3 = string.Join(",", title3.Where(t => !string.IsNullOrEmpty(t)).ToArray()),
                        children = new List<FancyTreeExt>()
                    };

                    if (tempList != null && tempList.Count > 0)
                    {
                        node.folder = true;
                        node.children = tempList;
                    }
                    fancyTrees.Add(node);
                }
            }
            return fancyTrees;
        }


        /// <summary>
        /// 异步获取商品列表
        /// </summary>
        /// <param name="key">搜索关键字</param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        public async Task<JsonResult> AsyncList(int? store, int[] excludeIds, int plan, string key = "", int categoryId = 0, int pagesize = 30, int pageindex = 0)
        {
            return await Task.Run(() =>
            {
                var gridModel = _productService.SearchProducts(
                    curStore.Id,
                    excludeIds,
                    categoryId == 0 ? new int[] { } : new int[] { categoryId },
                    null,
                    null,
                    null,
                    includeManufacturers: false,
                    includeSpecificationAttributes: false,
                    includeVariantAttributes: false,
                    includeVariantAttributeCombinations: false,
                    includePrices: false,
                    includeTierPrices: false,
                    includePictures: false,
                    includeStocks: false,
                    includeFlavor: false,
                    includeBrand: false,
                    key: key,
                    pageIndex: pageindex,
                    pageSize: pagesize);

                return Json(new
                {
                    Success = true,
                    total = gridModel.TotalCount,
                    rows = gridModel.Select(m =>
                    {
                        var cat = _categoryService.GetCategoryById(curStore.Id, m.CategoryId);
                        var brand = _brandService.GetBrandById(curStore.Id, m.BrandId);
                        var smallOption = _specificationAttributeService.GetSpecificationAttributeOptionById(m.SmallUnitId);
                        var bigOption = _specificationAttributeService.GetSpecificationAttributeOptionById(m.BigUnitId ?? 0);
                        var p = m.ToModel<ProductModel>();

                        p.CategoryName = cat != null ? cat.Name : "";
                        p.BrandName = brand != null ? brand.Name : "";
                        p.UnitName = string.Format("1{0} = {1}{2}", bigOption != null ? bigOption.Name : "/", m.BigQuantity, smallOption != null ? smallOption.Name : "/");

                        var percentage = _percentageService.GetPercentageByProduct(curStore?.Id ?? 0, plan, m.Id);

                        var title2 = new List<string>();
                        var title3 = new List<string>();

                        if (percentage != null)
                        {
                            switch (percentage.CalCulateMethodId)
                            {
                                //销售额百分比
                                case 1:
                                    {
                                        decimal salesPercent = percentage.SalesPercent ?? 0;
                                        decimal returnPercent = percentage.ReturnPercent ?? 0;

                                        if (salesPercent != 0)
                                        {
                                            title2.Add($"{salesPercent:0.00}%");
                                        }

                                        if (returnPercent != 0)
                                        {
                                            title3.Add($"{returnPercent:0.00}%");
                                        }
                                    }
                                    break;
                                //销售额变化百分比
                                case 2:
                                    {
                                        var rangs = _percentageService.GetAllPercentageRangeOptionsByPercentageId(store ?? 0, percentage.Id);
                                        rangs.ToList().ForEach(r =>
                                        {

                                            decimal salesPercent = r.SalesPercent ?? 0;
                                            decimal returnPercent = r.ReturnPercent ?? 0;

                                            if (salesPercent != 0)
                                            {
                                                title2.Add($"{salesPercent:0.00}%");
                                            }

                                            if (returnPercent != 0)
                                            {
                                                title3.Add($"{returnPercent:0.00}%");
                                            }
                                        });

                                    }
                                    break;
                                //销售额分段变化百分比
                                case 3:
                                    {
                                        var rangs = _percentageService.GetAllPercentageRangeOptionsByPercentageId(store ?? 0, percentage.Id);
                                        rangs.ToList().ForEach(r =>
                                        {

                                            decimal salesPercent = r.SalesPercent ?? 0;
                                            decimal returnPercent = r.ReturnPercent ?? 0;

                                            if (salesPercent != 0)
                                            {
                                                title2.Add($"{salesPercent:0.00}%");
                                            }

                                            if (returnPercent != 0)
                                            {
                                                title3.Add($"{returnPercent:0.00}%");
                                            }
                                        });

                                    }
                                    break;
                                //销售数量每件固定额
                                case 4:
                                    {
                                        decimal salesAmount = percentage.SalesAmount ?? 0;
                                        decimal returnAmount = percentage.ReturnAmount ?? 0;

                                        if (salesAmount != 0)
                                        {
                                            title2.Add($"{salesAmount:0.00}元");
                                        }

                                        if (returnAmount != 0)
                                        {
                                            title3.Add($"{returnAmount:0.00}元");
                                        }
                                    }
                                    break;
                                //按销售数量变化每件提成金额
                                case 5:
                                    {
                                        var rangs = _percentageService.GetAllPercentageRangeOptionsByPercentageId(store ?? 0, percentage.Id);
                                        rangs.ToList().ForEach(r =>
                                        {

                                            decimal salesPercent = r.SalesPercent ?? 0;
                                            decimal returnPercent = r.ReturnPercent ?? 0;

                                            if (salesPercent != 0)
                                            {
                                                title2.Add($"{salesPercent:0.00}元");
                                            }

                                            if (returnPercent != 0)
                                            {
                                                title3.Add($"{returnPercent:0.00}元");
                                            }
                                        });
                                    }
                                    break;
                                //按销售数量分段变化每件提成金额
                                case 6:
                                    {
                                        var rangs = _percentageService.GetAllPercentageRangeOptionsByPercentageId(store ?? 0, percentage.Id);
                                        rangs.ToList().ForEach(r =>
                                        {

                                            decimal salesPercent = r.SalesPercent ?? 0;
                                            decimal returnPercent = r.ReturnPercent ?? 0;

                                            if (salesPercent != 0)
                                            {
                                                title2.Add($"{salesPercent:0.00}元");
                                            }

                                            if (returnPercent != 0)
                                            {
                                                title3.Add($"{returnPercent:0.00}元");
                                            }
                                        });
                                    }
                                    break;
                                //利润额百分比
                                case 7:
                                    {
                                        decimal salesPercent = percentage.SalesPercent ?? 0;
                                        decimal returnPercent = percentage.ReturnPercent ?? 0;

                                        if (salesPercent != 0)
                                        {
                                            title2.Add($"{salesPercent:0.00}%");
                                        }

                                        if (returnPercent != 0)
                                        {
                                            title3.Add($"{returnPercent:0.00}%");
                                        }
                                    }
                                    break;
                                //利润额变化百分比
                                case 8:
                                    {
                                        var rangs = _percentageService.GetAllPercentageRangeOptionsByPercentageId(store ?? 0, percentage.Id);
                                        rangs.ToList().ForEach(r =>
                                        {

                                            decimal salesPercent = r.SalesPercent ?? 0;
                                            decimal returnPercent = r.ReturnPercent ?? 0;

                                            if (salesPercent != 0)
                                            {
                                                title2.Add($"{salesPercent:0.00}元");
                                            }

                                            if (returnPercent != 0)
                                            {
                                                title3.Add($"{returnPercent:0.00}元");
                                            }
                                        });
                                    }
                                    break;
                                //利润额分段变化百分比
                                case 9:
                                    {
                                        var rangs = _percentageService.GetAllPercentageRangeOptionsByPercentageId(store ?? 0, percentage.Id);
                                        rangs.ToList().ForEach(r =>
                                        {

                                            decimal salesPercent = r.SalesPercent ?? 0;
                                            decimal returnPercent = r.ReturnPercent ?? 0;

                                            if (salesPercent != 0)
                                            {
                                                title2.Add($"{salesPercent:0.00}元");
                                            }

                                            if (returnPercent != 0)
                                            {
                                                title3.Add($"{returnPercent:0.00}元");
                                            }
                                        });
                                    }
                                    break;
                            }
                        }

                        p.PercentageCalCulateMethods = percentage != null ? CommonHelper.GetEnumDescription<PercentageCalCulateMethod>(percentage.CalCulateMethod) : "";
                        p.PercentageSales = string.Join(",", title2.Where(t => !string.IsNullOrEmpty(t)).ToArray());
                        p.PercentageReturns = string.Join(",", title3.Where(t => !string.IsNullOrEmpty(t)).ToArray());

                        p.PercentageId = m.PercentageId;
                        return p;
                    }).ToList()
                });
            });
        }


        /// <summary>
        /// 异步获取分段项目
        /// </summary>
        /// <param name="percentageId"></param>
        /// <returns></returns>
        public async Task<JsonResult> AsyncOptionList(int percentageId = 0)
        {
            return await Task.Run(() =>
            {
                var options = _percentageService.GetAllPercentageRangeOptionsByPercentageId(curStore.Id, percentageId);
                return Json(new
                {
                    total = options.Count,
                    rows = options.Select(o => { return o.ToModel<PercentageRangeOptionModel>(); }).ToList()
                });
            });
        }

        /// <summary>
        /// 新增提成设置
        /// </summary>
        /// <param name="catagoryId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult Create(int? catagoryId, int? productId, int percentagePlanId)
        {
            var model = new PercentageModel
            {
                IsReturnCalCulated = true,
                IsGiftCalCulated = true
            };

            if (catagoryId.HasValue)
            {
                var catagory = _categoryService.GetCategoryById(curStore.Id, catagoryId.Value);
                if (catagory != null)
                {
                    model.CatagoryName = catagory.Name;
                }

                var percentage = _percentageService.GetPercentageByCatagoryId(curStore?.Id ?? 0, percentagePlanId, catagoryId.Value);
                if (percentage != null)
                {
                    model = percentage.ToModel<PercentageModel>();
                    var options = _percentageService.GetAllPercentageRangeOptionsByPercentageId(curStore.Id, percentage.Id);
                    model.Rangs = options.Select(o => { return o.ToModel<PercentageRangeOptionModel>(); }).ToList();
                }
            }

            if (productId.HasValue)
            {
                var product = _productService.GetProductById(curStore.Id, productId.Value);
                if (product != null)
                {
                    model.ProductName = product.Name;
                }

                var percentage = _percentageService.GetPercentageByProductId(curStore?.Id ?? 0, percentagePlanId, productId.Value);
                if (percentage != null)
                {
                    model = percentage.ToModel<PercentageModel>();
                    var options = _percentageService.GetAllPercentageRangeOptionsByPercentageId(curStore.Id, percentage.Id);
                    model.Rangs = options.Select(o => { return o.ToModel<PercentageRangeOptionModel>(); }).ToList();
                }
            }


            if (!string.IsNullOrEmpty(model.CatagoryName))
            {
                model.Name = model.CatagoryName;
            }
            else if (!string.IsNullOrEmpty(model.ProductName))
            {
                model.Name = model.ProductName;
            }

            #region 绑定数据源
            model.CalCulateMethods = new SelectList(from a in Enum.GetValues(typeof(PercentageCalCulateMethod)).Cast<PercentageCalCulateMethod>()
                                                    select new SelectListItem
                                                    {
                                                        Text = CommonHelper.GetEnumDescription(a),
                                                        Value = ((int)a).ToString()
                                                    }, "Value", "Text");
            model.QuantityCalCulateMethods = new SelectList(from a in Enum.GetValues(typeof(QuantityCalCulateMethod)).Cast<QuantityCalCulateMethod>()
                                                            select new SelectListItem
                                                            {
                                                                Text = CommonHelper.GetEnumDescription(a),
                                                                Value = ((int)a).ToString()
                                                            }, "Value", "Text");

            model.CostingCalCulateMethods = new SelectList(from a in Enum.GetValues(typeof(CostingCalCulateMethod)).Cast<CostingCalCulateMethod>()
                                                           select new SelectListItem
                                                           {
                                                               Text = CommonHelper.GetEnumDescription(a),
                                                               Value = ((int)a).ToString()
                                                           }, "Value", "Text");
            #endregion


            return Json(new
            {
                Success = true,
                RenderHtml = RenderPartialViewToString("Create", model)
            });
        }

        /// <summary>
        /// 保存新增提成设置
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> Create(PercentageSerializeModel data, int? percentageId)
        {
            try
            {
                if (data != null && data.Percentage != null)
                {
                    if (PeriodClosed(DateTime.Now))
                    {
                        return Warning("会计期间已经锁定,禁止业务操作.");
                    }

                    #region 数据验证
                    var percentage = data.Percentage;
                    if (percentage.PercentagePlanId == 0)
                    {
                        return Warning("请选择方案");
                    }
                    #endregion

                    var dataTo = data.Percentage.ToEntity<Percentage>();
                    var percentageRangeOptions = data.Percentage.Rangs.Select(r => r.ToEntity<PercentageRangeOption>()).ToList();

                    //Redis事务锁(防止重复保存)
                    var result = await _locker.PerformActionWithLockAsync(LockKey(data),
                        TimeSpan.FromSeconds(30),
                        TimeSpan.FromSeconds(10),
                        TimeSpan.FromSeconds(1),
                        () => _percentageService.CreateOrUpdate(curStore.Id, curUser.Id, percentageId, dataTo, percentageRangeOptions));

                    return Successful("配置成功");
                }
                else
                {
                    return Warning("配置失败,没有找到数据");
                }
            }
            catch (Exception ex)
            {
                return Warning(ex.Message);
            }
        }

        [HttpPost]
        public async Task<JsonResult> Reset(int? percentageId)
        {
            try
            {
                if (percentageId.HasValue && percentageId != 0)
                {
                    var percentage = _percentageService.GetPercentageById(curStore.Id, percentageId.Value);

                    if (percentage == null)
                    {
                        return Warning("提成信息不存在");
                    }

                    //Redis事务锁(防止重复保存)
                    string lockKey = string.Format(DCMSCachingDefaults.RedisDataReSubmitKey, Request.GetUrl(), curStore.Id, curUser.Id, CommonHelper.MD5(JsonConvert.SerializeObject(percentageId)));
                    var result = await _locker.PerformActionWithLockAsync(lockKey,
                        TimeSpan.FromSeconds(30),
                        TimeSpan.FromSeconds(10),
                        TimeSpan.FromSeconds(1),
                        () => _percentageService.Reset(curStore.Id, curUser.Id, percentage));

                    return Successful("重置提成成功");
                }
                else
                {
                    return Warning("提成信息不存在");
                }
            }
            catch (Exception ex)
            {

                return Warning(ex.Message);
            }
        }



    }
}
