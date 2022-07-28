using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Products;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Products;
using DCMS.ViewModel.Models.Products;
using DCMS.Web.Framework.Mvc.Filters;
using DCMS.Web.Infrastructure.Mapper.Extensions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace DCMS.Web.Controllers
{


    public class BrandController : BasePublicController
    {
        private readonly IUserActivityService _userActivityService;
        
        private readonly IBrandService _brandService;
        private readonly IProductService _productService;



        public BrandController(IUserActivityService userActivityService,
            IWorkContext workContext,
            IStaticCacheManager cacheManager,
            IStoreContext storeContext,
            IBrandService brandService,
            ILogger loggerService,
            INotificationService notificationService, IProductService productService) : base(workContext, loggerService, storeContext, notificationService)
        {
            _userActivityService = userActivityService;
            _brandService = brandService;
            
            _productService = productService;
        }


        public IActionResult Index()
        {
            return RedirectToAction("List");
        }


        [AuthCode((int)AccessGranularityEnum.BrandArchivesView)]
        public IActionResult List(string name, int pagenumber = 0)
        {

            var listModel = new BrandListModel();

            if (pagenumber > 1)
            {
                pagenumber -= 1;
            }

            var brands = _brandService.GetAllBrands(curStore?.Id ?? 0, name, pageIndex: pagenumber, pageSize: 10);
            listModel.PagingFilteringContext.LoadPagedList(brands);
            listModel.Items = brands.Select(s => s.ToModel<BrandModel>()).ToList();

            return View(listModel);
        }

        /// <summary>
        /// 异步品牌搜索
        /// </summary>
        /// <param name="key"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public JsonResult AsyncSearch(string key, int pageIndex = 0)
        {
            var model = new BrandListModel
            {
                Key = key
            };

            return Json(new
            {
                Success = true,
                RenderHtml = RenderPartialViewToString("AsyncSearch", model)
            });
        }

        public JsonResult AsyncList(string key, int pageIndex = 0, int pageSize = 10)
        {

            //当前登录者
            var model = new BrandListModel();

            var lists = _brandService.GetAllBrands(curStore?.Id ?? 0, key, pageIndex: pageIndex, pageSize: pageSize);

            model.PagingFilteringContext.LoadPagedList(lists);
            model.Items = lists.Select(s => s.ToModel<BrandModel>()).ToList();


            return Json(new
            {
                Success = true,
                total = model.Items.Count(),
                rows = model.Items
            });
        }

        [AuthCode((int)AccessGranularityEnum.BrandArchivesSave)]
        public JsonResult Create()
        {
            var model = new BrandModel
            {
                Status = true
            };
            return Json(new
            {
                Success = true,
                RenderHtml = RenderPartialViewToString("Create", model)
            });
        }


        [HttpPost]
        //[ValidateInput(false)]
        [AuthCode((int)AccessGranularityEnum.BrandArchivesSave)]
        public JsonResult Create(BrandModel model, bool continueEditing = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Name))
                {
                    ModelState.AddModelError("", "名称不能为空");
                }

                if (ModelState.IsValid)
                {

                    var brand = model.ToEntity<Brand>();
                    brand.StoreId = curStore?.Id ?? 0;
                    brand.CreatedOnUtc = DateTime.Now;
                    if(_brandService.GetBrandId(curStore?.Id ?? 0, model.Name) > 0)
                    {
                        _userActivityService.InsertActivity("InsertBrands", "该品牌已被使用", curUser.Id);
                        return Warning("该品牌已被使用");
                    }
                    else
                    {
                        _brandService.InsertBrand(brand);

                        //活动日志
                        _userActivityService.InsertActivity("InsertBrands", "创建品牌", curUser.Id);
                        return Successful("添加成功");
                    }
                    

                }
                else
                {
                    return Warning("数据验证失败");
                }
            }
            catch (Exception ex)
            {

                _userActivityService.InsertActivity("InsertBrands", "创建品牌", curUser.Id);
                _notificationService.SuccessNotification(ex.Message);
                return Warning(ex.ToString());
            }

        }

        [AuthCode((int)AccessGranularityEnum.BrandArchivesUpdate)]
        public JsonResult Edit(int? id)
        {
            var model = new BrandModel();

            if (id.HasValue)
            {
                var brand = _brandService.GetBrandById(curStore.Id, id.Value);
                if (brand != null)
                {
                    //只能操作当前经销商数据
                    if (brand.StoreId != curStore.Id)
                    {
                        return Warning("权限不足!");
                    }

                    model = brand.ToModel<BrandModel>();
                }
            }

            return Json(new
            {
                Success = true,
                RenderHtml = RenderPartialViewToString("Edit", model)
            });

        }

        [HttpPost]
        //[ValidateInput(false)]
        [AuthCode((int)AccessGranularityEnum.BrandArchivesUpdate)]
        public JsonResult Edit(BrandModel model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Name))
                {
                    ModelState.AddModelError("", "名称不能为空");
                }

                if (ModelState.IsValid)
                {

                    var brand = _brandService.GetBrandById(curStore.Id, model.Id);
                    if (brand != null)
                    {
                        brand.Name = model.Name;
                        brand.Status = model.Status;
                        brand.DisplayOrder = model.DisplayOrder;
                        _brandService.UpdateBrand(brand);
                        //活动日志
                        _userActivityService.InsertActivity("UpdateBrands", "编辑品牌", curUser.Id);
                    }

                    return Successful("编辑成功");
                }
                else
                {
                    return Warning("数据验证失败");
                }
            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("UpdateBrands", "编辑品牌", curUser.Id);
                _notificationService.SuccessNotification(ex.Message);
                return Warning(ex.ToString());
            }

        }

        [AuthCode((int)AccessGranularityEnum.BrandArchivesDelete)]
        public IActionResult Delete(string ids)
        {
            try
            {
                if (!string.IsNullOrEmpty(ids))
                {
                    var productsId = _productService.GetProductIds(curStore.Id, ids);
                    if (productsId.Count > 0)
                    {
                        //return Json(new { Success = false, Message = "该品牌下已有商品！" });
                    }
                    else
                    {
                        int[] sids = ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(u => int.Parse(u)).ToArray();
                        var brands = _brandService.GetBrandsByIds(curStore.Id, sids);
                        foreach (var brand in brands)
                        {
                            if (brand != null)
                            {
                                _brandService.DeleteBrand(brand);
                            }
                        }
                        //活动日志
                        _userActivityService.InsertActivity("DeleteBrands", "删除品牌", curUser, ids);
                    }

                }
            }
            catch (Exception ex)
            {
                _userActivityService.InsertActivity("DeleteBrands", "删除品牌", curUser, ids);
                _notificationService.SuccessNotification(ex.Message);
            }

            return RedirectToAction("List");
        }


    }
}
