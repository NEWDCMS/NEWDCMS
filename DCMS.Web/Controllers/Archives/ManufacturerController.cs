using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Products;
using DCMS.Services.Common;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Products;
using DCMS.ViewModel.Models.Products;
using DCMS.Web.Framework.Mvc.Filters;
using DCMS.Web.Infrastructure.Mapper.Extensions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace DCMS.Web.Controllers
{
    /// <summary>
    /// 用于供应商信息管理
    /// </summary>
    public class ManufacturerController : BasePublicController
    {
        private readonly IUserActivityService _userActivityService;
        
        private readonly IManufacturerService _manufacturerService;
        private readonly ICommonBillService _commonBillService;


        public ManufacturerController(
            IUserActivityService userActivityService,
            IWorkContext workContext,
            IStaticCacheManager cacheManager,
            IStoreContext storeContext,
            ILogger loggerService,
            INotificationService notificationService,
            ICommonBillService commonBillService,
            IManufacturerService manufacturerService
            ) : base(workContext, loggerService, storeContext, notificationService)
        {
            _userActivityService = userActivityService;
            
            _manufacturerService = manufacturerService;
            _commonBillService = commonBillService;
        }

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        [AuthCode((int)AccessGranularityEnum.SupplierView)]
        public IActionResult List()
        {
            return View();
        }

        /// <summary>
        /// 供应商列表
        /// </summary>
        /// <param name="searchStr"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<JsonResult> ManufacturerList(string searchStr = "", int pageIndex = 0, int pageSize = 10)
        {
            return await Task.Run(() =>
            {

                var model = new ManufacturerListModel();
                var manufacturers = _manufacturerService.GetAllManufactureies(searchStr, curStore?.Id ?? 0, pageIndex, pageSize);
                return Json(new
                {
                    total = manufacturers.TotalCount,
                    rows = manufacturers.Select(m =>
                    {
                        return m.ToModel<ManufacturerModel>();
                    }).ToList()
                });
            });
        }


        /// <summary>
        /// 添加供应商
        /// </summary>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.SupplierSave)]
        public JsonResult AddManufacturer()
        {
            var model = new ManufacturerModel
            {
                Status = true
            };
            return Json(new { Success = true, RenderHtml = RenderPartialViewToString("AddManufacturer", model) });
        }

        [HttpPost]
        //[ValidateInput(false)]
        [AuthCode((int)AccessGranularityEnum.SupplierSave)]
        public JsonResult AddManufacturer(ManufacturerModel model)
        {
            try
            {
                if (model != null)
                {
                    //经销商

                    model.StoreId = curStore?.Id ?? 0;
                    model.DisplayOrder = 0;
                    model.Deleted = false;
                    if(_manufacturerService.ManufacturerByName(curStore?.Id ?? 0, model.Name) > 0)
                    {
                        _userActivityService.InsertActivity("AddManufacturer", "供应商名称已使用", curUser.Id);
                        _notificationService.SuccessNotification("供应商名称已使用");
                        return Successful("供应商名称已使用！");
                    }
                    else
                    {
                        //添加供应商信息表
                        _manufacturerService.InsertManufacturer(model.ToEntity<Manufacturer>());
                        //活动日志
                        _userActivityService.InsertActivity("AddManufacturer", "添加供应商成功", curUser.Id);
                        _notificationService.SuccessNotification("添加供应商成功");
                        return Successful("添加供应商成功！");
                    }  
                }
                else
                {
                    //活动日志
                    _userActivityService.InsertActivity("AddManufacturer", "添加供应商失败", curUser.Id);
                    _notificationService.SuccessNotification("添加供应商失败");
                    return Successful("添加供应商失败！");
                }
            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("AddManufacturer", "添加供应商失败", curUser.Id);
                _notificationService.SuccessNotification("添加供应商失败");
                return Error(ex.Message);
            }

        }

        /// <summary>
        /// 编辑供应商信息
        /// </summary>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.SupplierSave)]
        public JsonResult EditManufacturer(int id)
        {
            var manufacturer = _manufacturerService.GetManufacturerById(curStore.Id, id);
            if (manufacturer == null)
            {
                return Warning("数据不存在!");
            }
            //只能操作当前经销商数据
            else if (manufacturer.StoreId != curStore.Id)
            {
                return Warning("权限不足!");
            }

            return Json(new { Success = true, RenderHtml = RenderPartialViewToString("AddManufacturer", manufacturer.ToModel<ManufacturerModel>()) });
        }

        [HttpPost]
        //[ValidateInput(false)]
        [AuthCode((int)AccessGranularityEnum.SupplierSave)]
        public JsonResult EditManufacturer(ManufacturerModel model)
        {
            try
            {
                if (model != null)
                {
                    if (model.Name == "")
                    {
                        return Error("供应商名称不能为空");
                    }
                    if (model.MnemonicName == null)
                    {
                        return Error("供应商助记名不能为空");
                    }
                    if (model.ContactName == null)
                    {
                        return Error("供应商联系人不能为空");
                    }
                    if (model.ContactPhone == null)
                    {
                        return Error("供应商联系电话不能为空");
                    }
                    if (model.Address == null)
                    {
                        return Error("供应商地址不能为空");
                    }
                    var manufacturer = _manufacturerService.GetManufacturerById(curStore.Id, model.Id);
                    manufacturer = model.ToEntity(manufacturer);
                    manufacturer.UpdatedOnUtc = DateTime.Now;

                    //编辑供应商表
                    _manufacturerService.UpdateManufacturer(manufacturer);
                    //活动日志
                    _userActivityService.InsertActivity("EditManufacturer", "编辑供应商成功", curUser.Id);
                    _notificationService.SuccessNotification("编辑供应商成功");
                    return Successful("编辑供应商成功！");
                }
                else
                {
                    //活动日志
                    _userActivityService.InsertActivity("EditManufacturer", "编辑供应商失败", curUser.Id);
                    _notificationService.SuccessNotification("编辑供应商失败");
                    return Successful("编辑供应商失败！");
                }
            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("EditManufacturer", "编辑供应商失败", curUser.Id);
                _notificationService.SuccessNotification("编辑供应商失败");
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// 删除供应商信息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [AuthCode((int)AccessGranularityEnum.SupplierSave)]
        public JsonResult DeleteManufacturer(string ids)
        {
            try
            {
                if (!string.IsNullOrEmpty(ids))
                {
                    //保存事务
                    //using (var scope = new TransactionScope())
                    //{
                    //    scope.Complete();
                    //}

                    var SaleReservationBillId = _manufacturerService.GetManufacturerId(int.Parse(ids));
                    if (SaleReservationBillId.Count > 0)
                    {
                        return Warning("该供应商不可删除！");
                    }
                    else
                    {
                        int[] idArr = ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(u => int.Parse(u)).ToArray();
                        var list = _manufacturerService.GetManufacturersByIds(curStore.Id, idArr);

                        foreach (var manufacturer in list)
                        {
                            if (manufacturer != null)
                            {
                                _manufacturerService.DeleteManufacturer(manufacturer);
                            }
                        }
                        //活动日志
                        _userActivityService.InsertActivity("DeleteManufacturer", "删除供应商成功", curUser.Id);
                        _notificationService.SuccessNotification("删除供应商成功");
                        return Successful("删除供应商成功！");
                    }
                }
                else
                {
                    //活动日志
                    _userActivityService.InsertActivity("DeleteManufacturer", "删除供应商失败", curUser.Id);
                    _notificationService.SuccessNotification("删除供应商失败");
                    return Successful("删除供应商失败！");
                }
            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("DeleteManufacturer", "删除供应商失败", curUser.Id);
                _notificationService.SuccessNotification("删除供应商失败");
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// 获取供应商信息
        /// </summary>
        /// <param name="key"></param>
        /// <param name="pageIndex"></param>
        /// <param name="index"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult AsyncSearchSelectPopup(string key, int pageIndex = 0, int index = 0, string target = "")
        {
            var model = new ManufacturerListModel
            {
                Key = key,

                //需要获取当前行索引值
                RowIndex = index,
                Target = target
            };

            return Json(new
            {
                Success = true,
                RenderHtml = RenderPartialViewToString("AsyncSearch", model)
            });
        }

        public async Task<JsonResult> AsyncList(string key, int pageIndex = 0, int pageSize = 10)
        {
            //当前登录者
            var model = new ManufacturerListModel();
            model = await Task.Run(() =>
             {
                 var lists = _manufacturerService.GetAllManufactureies(key, curStore?.Id ?? 0, pageIndex: pageIndex, pageSize: pageSize);
                 model.PagingFilteringContext.LoadPagedList(lists);
                 model.Items = lists.Select(s => s.ToModel<ManufacturerModel>()).ToList();
                 return model;
             });

            return Json(new
            {
                Success = true,
                total = model.Items.Count(),
                rows = model.Items
            });
        }




        /// <summary>
        /// 获取供应商余额
        /// </summary>
        /// <param name="manufacturerId"></param>
        /// <returns></returns>
        public async Task<JsonResult> GetManufacturerBalance(int? storeId, int? manufacturerId)
        {
            if (!storeId.HasValue)
            {
                storeId = curStore?.Id ?? 0;
            }

            return await Task.Run(() =>
            {
                var terminalBalance = _commonBillService.CalcManufacturerBalance(storeId ?? 0, manufacturerId ?? 0);
                return Json(terminalBalance);
            });

        }

    }
}