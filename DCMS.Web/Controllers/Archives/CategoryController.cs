using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Common;
using DCMS.Core.Domain.Products;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Products;
using DCMS.ViewModel.Models.Products;
using DCMS.Web.Framework.Mvc.Filters;
using DCMS.Web.Infrastructure.Mapper.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;



namespace DCMS.Web.Controllers
{

    /// <summary>
    /// 用于商品类别信息管理
    /// </summary>
    public class CategoryController : BasePublicController
    {
        private readonly ICategoryService _productCategoryService;
        private readonly IUserActivityService _userActivityService;
        
        private readonly IStatisticalTypeService _statisticalTypeService;
        private readonly IProductService _productService;

        public CategoryController(ICategoryService productCategoryService,
            IUserActivityService userActivityService,
            IWorkContext workContext,
            IStoreContext storeContext,
            IStaticCacheManager cacheManager,
            ILogger loggerService,
            INotificationService notificationService,
            IStatisticalTypeService statisticalTypeService,
            IProductService productService
            ) : base(workContext, loggerService, storeContext, notificationService)
        {
            _productCategoryService = productCategoryService;
            _userActivityService = userActivityService;
            _statisticalTypeService = statisticalTypeService;
            
            _productService = productService;
        }


        public IActionResult Index()
        {
            return RedirectToAction("List");
        }


        /// <summary>
        /// 示例获取分类
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.StatisticalTypeView)]
        public IActionResult List()
        {
            var model = new CategoryListModel();
            return View();
        }

        /// <summary>
        /// 获取分类分页
        /// </summary>
        /// <param name="model"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthCode((int)AccessGranularityEnum.StatisticalTypeView)]

        public IActionResult List(CategoryListModel model, int page = 0, int pageSize = 30)
        {
            return null;
        }

        /// <summary>
        /// AJAX获取分类
        /// </summary>
        /// <param name="text"></param>
        /// <param name="selectedId"></param>
        /// <returns></returns>
        public JsonResult AllCategories(string text, int selectedId)
        {

            var categories = _productCategoryService.GetAllCategories(curStore?.Id ?? 0, showHidden: true);
            categories.Insert(0, new Category { Name = "[None]", Id = 0 });
            var selectList = new List<SelectListItem>();
            foreach (var c in categories)
            {
                selectList.Add(new SelectListItem()
                {
                    Value = c.Id.ToString(),
                    Text = c.GetFormattedBreadCrumb(_productCategoryService),
                    Selected = c.Id == selectedId
                });
            }

            return Json(selectList);
        }


        /// <summary>
        /// 异步获取FancyTree
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<JsonResult> GetFancyTree()
        {

            return await Task.Run(() =>
            {
                var trees = GetCategoryList(curStore?.Id ?? 0, 0);
                return Json(trees);
            });
        }


        /// <summary>
        /// 获取分类树
        /// </summary>
        /// <returns></returns>
        public IActionResult Tree()
        {

            var rootCategories = _productCategoryService.GetAllCategoriesByParentCategoryId(curStore?.Id ?? 0, 0, true);
            return View(rootCategories);
        }


        /// <summary>
        /// 示例创建分类
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        //[AuthCode((int)AccessGranularityEnum.StatisticalTypeSave)]
        public JsonResult Create()
        {
            var model = new CategoryModel();

            //分级
            model = BindCategoryDropDownList<CategoryModel>(model, new Func<int?, int, bool, IList<Category>>(_productCategoryService.GetAllCategoriesByParentCategoryId), curStore?.Id ?? 0, 0);
            model.Published = true;
            //统计类别 
            var statisticalTypes = _statisticalTypeService.GetAllStatisticalTypess(curStore?.Id ?? 0);
            var types = new List<SelectListItem> { new SelectListItem() { Text = "-请选择-", Value = "0" } };
            statisticalTypes.ToList().ForEach(t =>
            {
                types.Add(new SelectListItem
                {
                    Text = t.Name,
                    Value = t.Id.ToString(),
                });
            });
            model.StatisticalTypes = new SelectList(types, "Value", "Text");

            return Json(new
            {
                Success = true,
                RenderHtml = RenderPartialViewToString("Create", model)
            });
        }

        /// <summary>
        /// 创建分类保存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        //[AuthCode((int)AccessGranularityEnum.StatisticalTypeSave)]
        public JsonResult Create(IFormCollection form)
        {
            try
            {



                var name = !string.IsNullOrEmpty(form["Name"]) ? form["Name"].ToString() : "";
                var parentId = !string.IsNullOrEmpty(form["ParentId"]) ? form["ParentId"].ToString() : "0";
                var orderNo = !string.IsNullOrEmpty(form["OrderNo"]) ? form["OrderNo"].ToString() : "0";
                var statisticalType = !string.IsNullOrEmpty(form["StatisticalType"]) ? form["StatisticalType"].ToString() : "0";
                var brandId = !string.IsNullOrEmpty(form["BrandId"]) ? form["BrandId"].ToString() : "0";
                var brandName = !string.IsNullOrEmpty(form["BrandName"]) ? form["BrandName"].ToString() : "";
                var published = !string.IsNullOrEmpty(form["Published"]) ? form["Published"].Contains("true").ToString() : "false";

                if (_productCategoryService.GetAllCategories(curStore.Id).Count(c => c.Name == name) > 0)
                {
                    return Warning("该类别已经存在");
                }

                var model = new CategoryModel()
                {
                    Name = name,
                    ParentId = int.Parse(parentId),
                    OrderNo = int.Parse(orderNo),
                    StatisticalType = int.Parse(statisticalType),
                    BrandId = int.Parse(brandId),
                    BrandName = brandName,
                    Published = Convert.ToBoolean(published),
                    StoreId = curStore?.Id ?? 0
                };

                _productCategoryService.InsertCategory(model.ToEntity<Category>());

                //移除缓存
                //_cacheManager.RemoveByPrefix("DCMS.Web.Controllers.GetFancyTree.");

                _userActivityService.InsertActivity("InsertCategory", "创建分类", curUser, curUser.Id);
                _notificationService.SuccessNotification("创建分类成功");

                return Successful("创建分类成功");

            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }


        /// <summary>
        /// 编辑分类
        /// </summary>
        /// <returns></returns>
        //[AuthCode((int)AccessGranularityEnum.StatisticalTypeUpdate)]   
        public JsonResult Edit(int? id)
        {
            var model = new CategoryModel();
            if (id.HasValue)
            {
                var category = _productCategoryService.GetCategoryById(curStore.Id, id.Value);
                if (category != null)
                {
                    model = category.ToModel<CategoryModel>();
                }
                //只能操作当前经销商数据
                else if (category.StoreId != curStore.Id)
                {
                    return Successful("权限不足!");
                }

                //分级
                model = BindCategoryDropDownList<CategoryModel>(model, new Func<int?, int, bool, IList<Category>>(_productCategoryService.GetAllCategoriesByParentCategoryId), curStore?.Id ?? 0, 0);

                //统计类别 
                var statisticalTypes = _statisticalTypeService.GetAllStatisticalTypess(curStore?.Id ?? 0);
                var types = new List<SelectListItem> { new SelectListItem() { Text = "-请选择-", Value = "0" } };
                statisticalTypes.ToList().ForEach(t =>
                {
                    types.Add(new SelectListItem
                    {
                        Text = t.Name,
                        Value = t.Id.ToString(),
                    });
                });

                model.StatisticalTypes = new SelectList(types, "Value", "Text");
            }

            return Json(new
            {
                Success = true,
                RenderHtml = RenderPartialViewToString("Edit", model)
            });
        }

        /// <summary>
        /// 编辑分类保存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        //[AuthCode((int)AccessGranularityEnum.StatisticalTypeUpdate)]

        public JsonResult Edit(IFormCollection form)
        {
            try
            {



                var id = !string.IsNullOrEmpty(form["Id"]) ? form["Id"].ToString() : "0";
                var name = !string.IsNullOrEmpty(form["Name"]) ? form["Name"].ToString() : "";
                var parentId = !string.IsNullOrEmpty(form["ParentId"]) ? form["ParentId"].ToString() : "0";
                var orderNo = !string.IsNullOrEmpty(form["OrderNo"]) ? form["OrderNo"].ToString() : "0";
                var statisticalType = !string.IsNullOrEmpty(form["StatisticalType"]) ? form["StatisticalType"].ToString() : "0";
                var brandId = !string.IsNullOrEmpty(form["BrandId"]) ? form["BrandId"].ToString() : "0";
                var brandName = !string.IsNullOrEmpty(form["BrandName"]) ? form["BrandName"].ToString() : "";
                var published = !string.IsNullOrEmpty(form["Published"]) ? form["Published"].Contains("true").ToString() : "false";

                if (_productCategoryService.GetAllCategories(curStore.Id).Count(c => c.Name == name) > 1)
                {
                    return Warning("该类别已经存在");
                }

                var category = _productCategoryService.GetCategoryById(curStore.Id, int.Parse(id));
                if (category != null)
                {
                    category.Name = name;
                    category.ParentId = int.Parse(parentId);
                    category.OrderNo = int.Parse(orderNo);
                    category.StatisticalType = int.Parse(statisticalType);
                    category.BrandId = int.Parse(brandId);
                    category.BrandName = brandName;
                    category.Published = Convert.ToBoolean(published);
                    category.StoreId = curStore?.Id ?? 0;

                    _productCategoryService.UpdateCategory(category);

                    //移除缓存
                    //_cacheManager.RemoveByPrefix("DCMS.Web.Controllers.GetFancyTree.");

                    _userActivityService.InsertActivity("UpdateCategory", "编辑分类", curUser, curUser.Id);
                    _notificationService.SuccessNotification("编辑分类成功");

                    return Successful("编辑分类成功");
                }
                else
                {
                    return Warning("更新失败");
                }
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }


        /// <summary>
        /// 删除商品分类
        /// </summary>
        /// <param name="selections"></param>
        /// <returns></returns>
        [HttpPost]
        //[AuthCode((int)AccessGranularityEnum.StatisticalTypeDelete)]
        public JsonResult Delete(int[] selections)
        {
            try
            {

                foreach (var n in selections)
                {
                    var category = _productCategoryService.GetCategoryById(curStore.Id, n);
                    //category1 查看是否有子类别
                    var category1 = _productCategoryService.GetAllCategoriesByParentCategoryId(curStore.Id, n);
                    var productids = _productCategoryService.GetProductedId(curStore.Id, n);
                    var productids1 = _productCategoryService.GetProductedId1(curStore.Id, n);
                    if (category != null)
                    {
                        if (productids != null && productids.Count > 0)
                        {
                            return Warning("该商品类别下的商品已开单，不能删除");
                        }
                        if (productids1 != null && productids1.Count > 0)
                        {
                            return Warning("该商品类别下有商品商品，不能删除");
                        }
                        if (category1 != null && category1.Count > 0)
                        {
                            return Warning("该商品类别下有子类别，不能删除");
                        }
                        _productCategoryService.DeleteCategory(category);
                        //移除缓存
                        //_cacheManager.RemoveByPrefix("CATEGORIES_BY_PARENT_CATEGORY_ID_KEY");
                        //日志
                        _userActivityService.InsertActivity("DeleteCategory", "删除分类成功", curUser, category.Name);
                    }
                }
                return Successful("删除分类成功");

            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }





        #region 统计类别

        [AuthCode((int)AccessGranularityEnum.StatisticalTypeView)]
        public IActionResult StatisticalTypeList(string name, int pagenumber = 0)
        {

            var listModel = new StatisticalTypeListModel();

            if (pagenumber > 1)
            {
                pagenumber -= 1;
            }

            var items = _statisticalTypeService.GetAllStatisticalTypess(curStore.Id, name, pageIndex: pagenumber, pageSize: 30);
            listModel.PagingFilteringContext.LoadPagedList(items);
            listModel.Items = items.Select(s => s.ToModel<StatisticalTypeModel>()).ToList();

            return View(listModel);
        }

        [AuthCode((int)AccessGranularityEnum.StatisticalTypeSave)]
        public JsonResult StatisticalTypeCreate()
        {
            var model = new StatisticalTypeModel();
            return Json(new
            {
                Success = true,
                RenderHtml = RenderPartialViewToString("StatisticalTypeCreate", model)
            });
        }


        [HttpPost]
        //[ValidateInput(false)]
        [AuthCode((int)AccessGranularityEnum.StatisticalTypeSave)]
        public JsonResult StatisticalTypeCreate(StatisticalTypeModel model, bool continueEditing = false)
        {

            if (string.IsNullOrWhiteSpace(model.Value))
            {
                ModelState.AddModelError("", "键值不能为空");
            }

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError("", "名称不能为空");
            }

            if (ModelState.IsValid)
            {
                var statisticalType = model.ToEntity<StatisticalTypes>();
                statisticalType.StoreId = curStore?.Id ?? 0;
                statisticalType.CreatedOnUtc = DateTime.Now;

                _statisticalTypeService.InsertStatisticalTypes(statisticalType);

                //活动日志
                _userActivityService.InsertActivity("InsertStatisticalTypes", "创建统计类别", curUser.Id);
                _notificationService.SuccessNotification("创建统计类别成功");

            }

            return Successful("添加成功");
        }

        [AuthCode((int)AccessGranularityEnum.StatisticalTypeUpdate)]
        public JsonResult StatisticalTypeEdit(int? id)
        {
            var model = new StatisticalTypeModel();

            if (id.HasValue)
            {
                var statisticalType = _statisticalTypeService.GetStatisticalTypesById(curStore.Id, id.Value);
                if (statisticalType != null)
                {
                    model = statisticalType.ToModel<StatisticalTypeModel>();
                }
            }
            return Json(new
            {
                Success = true,
                RenderHtml = RenderPartialViewToString("StatisticalTypeEdit", model)
            });
        }

        [HttpPost]
        //[ValidateInput(false)]
        [AuthCode((int)AccessGranularityEnum.StatisticalTypeUpdate)]
        public JsonResult StatisticalTypeEdit(StatisticalTypeModel model)
        {

            if (string.IsNullOrWhiteSpace(model.Value))
            {
                ModelState.AddModelError("", "键值不能为空");
            }

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError("", "名称不能为空");
            }

            if (ModelState.IsValid)
            {

                var statisticalType = _statisticalTypeService.GetStatisticalTypesById(curStore.Id, model.Id);
                if (statisticalType != null)
                {
                    statisticalType.Name = model.Name;
                    statisticalType.Value = model.Value;
                    _statisticalTypeService.UpdateStatisticalTypes(statisticalType);
                    //活动日志
                    _userActivityService.InsertActivity("UpdateStatisticalTypes", "编辑统计类别", curUser.Id);
                    _notificationService.SuccessNotification("编辑统计类别成功");
                }


                return Successful("编辑成功");
            }

            return Successful("编辑成功");
        }

        [AuthCode((int)AccessGranularityEnum.StatisticalTypeDelete)]
        public IActionResult StatisticalTypeDelete(string ids)
        {

            if (!string.IsNullOrEmpty(ids))
            {
                var productsId = _productService.GetStatisticalTypeid(int.Parse(ids));
                if (productsId.Count > 0)
                {
                    //活动日志
                }
                else
                {
                    int[] sids = ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(u => int.Parse(u)).ToArray();
                    var statisticalTypes = _statisticalTypeService.GetStatisticalTypessByIds(sids);
                    foreach (var statisticalType in statisticalTypes)
                    {
                        if (statisticalType != null)
                        {
                            _statisticalTypeService.DeleteStatisticalTypes(statisticalType);
                        }
                    }
                    //活动日志
                    _userActivityService.InsertActivity("DeleteStatisticalTypes", "删除统计类别", curUser, ids);
                    _notificationService.SuccessNotification("删除统计类别成功");
                }
            }

            return RedirectToAction("StatisticalTypeList");
        }


        #endregion



        #region Utility


        /// <summary>
        /// 递归获取类别树
        /// </summary>
        /// <param name="store">经销商</param>
        /// <param name="Id"></param>
        /// <returns></returns>
        [NonAction]
        public List<FancyTree> GetCategoryList(int? store, int Id)
        {
            List<FancyTree> fancyTrees = new List<FancyTree>();
            var perentList = _productCategoryService.GetAllCategoriesByParentCategoryId(store.Value, Id, true);
            if (perentList != null && perentList.Count > 0)
            {
                foreach (var b in perentList)
                {
                    List<FancyTree> tempList = GetCategoryList(store.Value, b.Id);
                    var node = new FancyTree
                    {
                        id = b.Id,
                        title = b.Name,
                        expanded = true,
                        children = new List<FancyTree>()
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

        #endregion

    }
}
