using DCMS.Core;
using DCMS.Core.Domain.Products;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Products;
using DCMS.Services.Stores;
using DCMS.ViewModel.Models.Products;
using DCMS.Web.Framework.Mvc.Filters;
using DCMS.Web.Infrastructure.Mapper.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;


namespace DCMS.Web.Controllers
{
    /// <summary>
    /// 用于商品信息管理
    /// </summary>
    public partial class SpecificationAttributeController : BasePublicController
    {
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IUserActivityService _userActivityService;
        private readonly IStoreService _storeService;


        public SpecificationAttributeController(ISpecificationAttributeService specificationAttributeService,
            IUserActivityService userActivityService,
            IWorkContext workContext,
            IStoreContext storeContext,
            IStoreService storeService,
            ILogger loggerService,
            INotificationService notificationService) : base(workContext, loggerService, storeContext, notificationService)
        {
            _storeService = storeService;
            _specificationAttributeService = specificationAttributeService;
            _userActivityService = userActivityService;
        }


        #region 属性管理

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        [AuthCode((int)AccessGranularityEnum.UnitArchivesView)]
        public IActionResult List(string searchKey, int pagenumber = 0)
        {



            var model = new SpecificationAttributeListModel();
            if (pagenumber > 1)
            {
                pagenumber -= 1;
            }

            model.SearchKey = searchKey;

            var query = _specificationAttributeService.GetSpecificationAttributesBtStore(curStore?.Id ?? 0).ToList();

            if (!string.IsNullOrEmpty(searchKey))
            {
                query = query.Where(t => t.Name.Contains(searchKey)).ToList();
            }

            var specificationAttributes = new PagedList<SpecificationAttribute>(query, pagenumber, 10);
            model.PagingFilteringContext.LoadPagedList(specificationAttributes);

            model.SpecificationAttributes = specificationAttributes.Select(x =>
            {
                var store = _storeService.GetStoreById(x.StoreId);
                var m = x.ToModel<SpecificationAttributeModel>();
                m.StoreName = store != null ? store.Name : "";
                return m;
            }).ToList();

            return View(model);
        }

        [AuthCode((int)AccessGranularityEnum.UnitArchivesSave)]
        public IActionResult Create()
        {

            var model = new SpecificationAttributeModel();

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        //[ValidateInput(false)]
        [AuthCode((int)AccessGranularityEnum.UnitArchivesSave)]
        public IActionResult Create(SpecificationAttributeModel model, bool continueEditing)
        {



            if (ModelState.IsValid)
            {
                //
                var specificationAttribute = model.ToEntity<SpecificationAttribute>();
                specificationAttribute.StoreId = curStore?.Id ?? 0;
                _specificationAttributeService.InsertSpecificationAttribute(specificationAttribute);
                //activity log
                _userActivityService.InsertActivity("AddNewSpecAttribute", "添加规格属性成功", curUser, specificationAttribute.Name);
                _notificationService.SuccessNotification("添加规格属性成功");

                return continueEditing ? RedirectToAction("Edit", new { id = specificationAttribute.Id }) : RedirectToAction("List");
            }


            return View(model);
        }

        [AuthCode((int)AccessGranularityEnum.UnitArchivesUpdate)]
        public IActionResult Edit(int id)
        {
            try
            {



                var specificationAttribute = _specificationAttributeService.GetSpecificationAttributeById(id);
                if (specificationAttribute == null)
                {
                    return RedirectToAction("List");
                }
                //只能操作当前经销商数据
                else if (specificationAttribute.StoreId != curStore.Id)
                {
                    return RedirectToAction("List");
                }

                var model = specificationAttribute.ToModel<SpecificationAttributeModel>();

                return View(model);
            }
            catch /*(Exception ex)*/
            {
                return RedirectToAction("List");
            }
        }

        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        //[ValidateInput(false)]
        [AuthCode((int)AccessGranularityEnum.UnitArchivesUpdate)]
        public IActionResult Edit(SpecificationAttributeModel model, bool continueEditing)
        {



            try
            {
                //

                var specificationAttribute = _specificationAttributeService.GetSpecificationAttributeById(model.Id);
                if (specificationAttribute == null)
                {
                    return RedirectToAction("List");
                }

                if (ModelState.IsValid)
                {
                    specificationAttribute = model.ToEntity(specificationAttribute);
                    specificationAttribute.StoreId = curStore?.Id ?? 0;
                    _specificationAttributeService.UpdateSpecificationAttribute(specificationAttribute);

                    //activity log
                    _userActivityService.InsertActivity("EditSpecAttribute", "编辑规格属性", curUser, specificationAttribute.Name);

                    _notificationService.SuccessNotification("编辑规格属性成功");
                    return continueEditing ? RedirectToAction("Edit", specificationAttribute.Id) : RedirectToAction("List");
                }
            }
            catch/* (Exception ex)*/
            {
            }

            return View(model);
        }

        [AuthCode((int)AccessGranularityEnum.UnitArchivesDelete)]
        public IActionResult Delete(int id)
        {

            var productids = _specificationAttributeService.GetpProductIds(id);
            if (productids.Count > 0)
            {
                //return this.Warning("该单位档案不能删除!");
            }
            else
            {
                var specificationAttribute = _specificationAttributeService.GetSpecificationAttributeById(id);
                if (specificationAttribute == null)
                {
                    return RedirectToAction("List");
                }

                _specificationAttributeService.DeleteSpecificationAttribute(specificationAttribute);

                //activity log
                _userActivityService.InsertActivity("DeleteSpecAttribute", "删除规格属性成功", curUser, specificationAttribute.Name);

                _notificationService.SuccessNotification("删除规格属性成功");
                //return this.Warning("删除规格属性成功!");
            }

            return RedirectToAction("List");
        }

        #endregion


        #region 属性项管理

        [HttpGet]
        public JsonResult OptionList(int specificationAttributeId)
        {
            var options = _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttribute(curStore?.Id ?? 0, specificationAttributeId);
            var gridModel = options.Select(x =>
            {
                //var sp = _specificationAttributeService.GetSpecificationAttributeById(x.SpecificationAttributeId);
                var model = x.ToModel<SpecificationAttributeModel>();
                //model.NumberOfAssociatedProducts = x.ProductSpecificationAttributes.Count;
                return model;
            }).ToList();

            return Json(new
            {
                total = gridModel.Count(),
                rows = gridModel
            });
        }


        public JsonResult OptionCreatePopup(int specificationAttributeId)
        {

            var model = new SpecificationAttributeOptionModel
            {
                SpecificationAttributeId = specificationAttributeId
            };

            return Json(new
            {
                Success = true,
                RenderHtml = RenderPartialViewToString("CreateOption", model)
            });
        }

        [HttpPost]
        public JsonResult OptionCreatePopup(IFormCollection form)
        {

            var specificationAttributeId = string.IsNullOrEmpty(form["SpecificationAttributeId"].ToString()) ? "0" : form["SpecificationAttributeId"].ToString();

            var specificationAttribute = _specificationAttributeService.GetSpecificationAttributeById(int.Parse(specificationAttributeId));
            if (specificationAttribute == null)
            {
                return Warning("数据不存在!");
            }

            var model = new SpecificationAttributeOptionModel()
            {
                NumberOfAssociatedProducts = 0,
                Name = string.IsNullOrEmpty(form["Name"].ToString()) ? "0" : form["Name"].ToString(),
                DisplayOrder = int.Parse(string.IsNullOrEmpty(form["DisplayOrder"].ToString()) ? "0" : form["DisplayOrder"].ToString()),
                SpecificationAttributeId = int.Parse(specificationAttributeId),
                StoreId = curStore.Id
            };

            if (ModelState.IsValid)
            {
                var sao = model.ToEntity<SpecificationAttributeOption>();
                _specificationAttributeService.InsertSpecificationAttributeOption(sao);
                return Successful("添加成功");
            }

            return Warning("添加成功");
        }

        public JsonResult OptionEditPopup(int id)
        {

            var sao = _specificationAttributeService.GetSpecificationAttributeOptionById(id);
            if (sao == null)
            {
                return Warning("数据不存在!");
            }

            var model = sao.ToModel<SpecificationAttributeOptionModel>();


            return Json(new
            {
                Success = true,
                RenderHtml = RenderPartialViewToString("EditOption", model)
            });
        }

        [HttpPost]
        public JsonResult OptionEditPopup(IFormCollection form)
        {

            var id = string.IsNullOrEmpty(form["Id"].ToString()) ? "0" : form["Id"].ToString();
            var specificationAttributeId = string.IsNullOrEmpty(form["SpecificationAttributeId"].ToString()) ? "0" : form["SpecificationAttributeId"].ToString();


            var sao = _specificationAttributeService.GetSpecificationAttributeOptionById(int.Parse(id));
            if (sao == null)
            {
                return Warning("数据不存在!");
            }

            if (ModelState.IsValid)
            {
                sao.Name = string.IsNullOrEmpty(form["Name"].ToString()) ? "" : form["Name"].ToString();
                sao.DisplayOrder = int.Parse(string.IsNullOrEmpty(form["DisplayOrder"].ToString()) ? "0" : form["DisplayOrder"].ToString());

                _specificationAttributeService.UpdateSpecificationAttributeOption(sao);

                return Successful("编辑成功");
            }
            return Warning("编辑失败");
        }


        public JsonResult OptionDelete(int optionId = 0, int specificationAttributeId = 0)
        {

            var sao = _specificationAttributeService.GetSpecificationAttributeOptionById(optionId);
            if (sao == null || optionId == 0)
            {
                return Warning("删除失败");
            }

            _specificationAttributeService.DeleteSpecificationAttributeOption(sao);

            return Successful("删除成功");
        }

        public JsonResult GetOptionsByAttributeId(string attributeId)
        {

            if (string.IsNullOrEmpty(attributeId))
            {
                throw new ArgumentNullException("attributeId");
            }

            var options = _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttribute(curStore?.Id ?? 0, Convert.ToInt32(attributeId));
            var result = (from o in options
                          select new { id = o.Id, name = o.Name }).ToList();
            return Json(result);
        }

        #endregion
    }
}
