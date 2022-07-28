using DCMS.Core;
using DCMS.Core.Domain.WareHouses;
using DCMS.Services.Common;
using DCMS.Services.Installation;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Users;
using DCMS.Services.WareHouses;
using DCMS.ViewModel.Models.WareHouses;
using DCMS.Web.Framework.Mvc.Filters;
using DCMS.Web.Infrastructure.Mapper.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace DCMS.Web.Controllers
{
    /// <summary>
    /// 用于仓库管理
    /// </summary>
    public class WareHouseController : BasePublicController
    {
        private readonly IWareHouseService _wareHouseService;
        private readonly IUserActivityService _userActivityService;
        private readonly IBillConvertService _billConvertService;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IInstallationService _installationService;
        private readonly IUserService _userService;

        public WareHouseController(IWareHouseService wareHouseService,
            IWorkContext workContext,
            IStoreContext storeContext,
            IUserActivityService userActivityService,
            ILogger loggerService,
            INotificationService notificationService, IBillConvertService billConvertService,
            IWebHostEnvironment hostingEnvironment,
            IInstallationService installationService,
            IUserService userService
            ) : base(workContext, loggerService, storeContext, notificationService)
        {
            _wareHouseService = wareHouseService;
            _userActivityService = userActivityService;
            _billConvertService = billConvertService;
            _hostingEnvironment = hostingEnvironment;
            _installationService = installationService;
            _userService = userService;
        }

        // GET: WareHouse
        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        [AuthCode((int)AccessGranularityEnum.StockListView)]
        public IActionResult List()
        {
            return View();
        }

        /// <summary>
        /// 异步获取列表数据
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<JsonResult> WareHouseList(string searchStr = "", int pageIndex = 0, int pageSize = 10)
        {
            return await Task.Run(() =>
            {
                var list = _wareHouseService.GetWareHouseList(searchStr, curStore?.Id ?? 0, 0, pageIndex, pageSize);
                return Json(new
                {
                    total = list.TotalCount,
                    rows = list.Select(w => w.ToModel<WareHouseModel>())
                });
            });
        }

        /// <summary>
        /// 添加仓库
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public JsonResult AddWareHouse()
        {
            var model = new WareHouseModel() { AllowNegativeInventoryPreSale = true };
            var users = _userService.GetAllUsers(curStore?.Id ?? 0);
            if (users != null && users.Any())
            {
                foreach (var user in users)
                {
                    var wha = new WareHouseAccessModel()
                    {
                        WareHouseId = 0,
                        WareHouseName = "",
                        UserId = user.Id,
                        UserName = user.UserRealName,
                    };
                    //销售单	退货单	销售订单	退货订单	调拨单	采购单	采购退货单	盘点单	借货单	还货单
                    var types = Enum.GetValues(typeof(WHAEnum)).Cast<WHAEnum>();
                    foreach (var u in types)
                    {
                        wha.BillTypes.Add(new WareHouseAccessBillTypeModel()
                        {
                            BillTypeId = (int)u,
                            BillTypeName = CommonHelper.GetEnumDescription(u),
                            Selected = false
                        });
                    }
                    model.WareHouseAccess.Add(wha);
                }
            }

            return Json(new
            {
                Success = true,
                RenderHtml = RenderPartialViewToString("AddWareHouse",model)
            });
        }

        [HttpPost]
        public JsonResult AddWareHouse(WareHouseModel model, IFormCollection form)
        {
            if (model == null)
            {
                return Warning("数据不存在!");
            }

            try
            {
                var users = _userService.GetAllUsers(curStore?.Id ?? 0);
                if (users != null && users.Any())
                {
                    foreach (var u in users)
                    {
                        var wha = new WareHouseAccessModel()
                        {
                            WareHouseId = model.Id,
                            WareHouseName = model.Name,
                            UserId = u.Id,
                            UserName = u.UserRealName,
                        };
                        var types = Enum.GetValues(typeof(WHAEnum)).Cast<WHAEnum>();
                        foreach (var t in types)
                        {
                            var select = form.ContainsKey($"model[makebill_{(int)t}_{u.Id}]");
                            wha.BillTypes.Add(new WareHouseAccessBillTypeModel()
                            {
                                BillTypeId = (int)t,
                                BillTypeName = CommonHelper.GetEnumDescription(t),
                                Selected = select
                            });
                        }

                        var query = form.ContainsKey($"model[stockquery_{u.Id}]");
                        wha.StockQuery = query;
                        model.WareHouseAccess.Add(wha);
                    }
                }

                var wareHouse = model.ToEntity<WareHouse>();
                wareHouse.StoreId = curStore?.Id ?? 0;
                wareHouse.CreatedOnUtc = DateTime.Now;
                wareHouse.AllowNegativeInventoryPreSale = model.AllowNegativeInventoryPreSale;
                wareHouse.WareHouseAccessSettings = JsonConvert.SerializeObject(model.WareHouseAccess);


                if (_wareHouseService.GetWareHouseByName(curStore?.Id ?? 0, wareHouse.Name)==null)
                {
                    if (wareHouse.Name.Equals("主仓库"))
                    {
                        wareHouse.IsSystem = true;
                    }
                    _wareHouseService.InsertWareHouse(wareHouse);
                    //活动日志
                    _userActivityService.InsertActivity("AddWareHouse", "添加仓库成功", curUser.Id);
                    _notificationService.SuccessNotification("添加仓库成功");
                    return Successful("添加仓库成功！");
                }
                else
                {
                    //活动日志
                    _userActivityService.InsertActivity("AddWareHouse", "仓库名称已使用", curUser.Id);

                    _notificationService.ErrorNotification("仓库名称已使用");
                    return Error("仓库名称已使用");
                }
            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("AddWareHouse", "添加仓库失败", curUser.Id);

                _notificationService.ErrorNotification("添加仓库失败");
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// 编辑仓库
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult EditWareHouse(int? id)
        {
            if (!id.HasValue)
            {
                return Warning("数据不存在!");
            }

            var model = new WareHouseModel();
            var wareHouse = _wareHouseService.GetWareHouseById(curStore.Id, id.Value);

            if (wareHouse == null)
            {
                return Warning("数据不存在!");
            }
            else
            {
                model = wareHouse.ToModel<WareHouseModel>();
                model.AllowNegativeInventoryPreSale = wareHouse.AllowNegativeInventory;

                var temps = new List<WareHouseAccessModel>();
                if (!string.IsNullOrEmpty(wareHouse.WareHouseAccessSettings))
                    temps = JsonConvert.DeserializeObject<List<WareHouseAccessModel>>(wareHouse.WareHouseAccessSettings);

                var users = _userService.GetAllUsers(curStore?.Id ?? 0);
                if (users != null && users.Any())
                {
                    foreach (var user in users)
                    {
                        var curuser = temps.Where(s => s.UserId == user.Id).FirstOrDefault();
                        var wha = new WareHouseAccessModel()
                        {
                            WareHouseId = model.Id,
                            WareHouseName = model.Name,
                            UserId = user.Id,
                            UserName = user.UserRealName,
                            StockQuery = curuser?.StockQuery ?? false
                        };

                        //销售单	退货单	销售订单	退货订单	调拨单	采购单	采购退货单	盘点单	借货单	还货单
                        var types = Enum.GetValues(typeof(WHAEnum)).Cast<WHAEnum>();
                        foreach (var u in types)
                        {
                            var selt = curuser?.BillTypes.Where(s => s.BillTypeId == (int)u).FirstOrDefault();
                            wha.BillTypes.Add(new WareHouseAccessBillTypeModel()
                            {
                                BillTypeId = (int)u,
                                BillTypeName = CommonHelper.GetEnumDescription(u),
                                Selected = selt?.Selected ?? false
                            }); 
                        }
                        model.WareHouseAccess.Add(wha);
                    }
                }
            }

            return Json(new { Success = true, RenderHtml = RenderPartialViewToString("AddWareHouse", model) });
        }

        [HttpPost]
        public JsonResult EditWareHouse(WareHouseModel model, IFormCollection form)
        {
            if (model == null)
            {
                return Warning("数据不存在!");
            }
            if (string.IsNullOrEmpty(model.Name))
            {
                return Warning("仓库名称不能为空!");
            }

            try
            {
                var users = _userService.GetAllUsers(curStore?.Id ?? 0);
                if (users != null && users.Any())
                {
                    foreach (var u in users)
                    {
                        var wha = new WareHouseAccessModel()
                        {
                            WareHouseId = model.Id,
                            WareHouseName = model.Name,
                            UserId = u.Id,
                            UserName = u.UserRealName,
                        };
                        var types = Enum.GetValues(typeof(WHAEnum)).Cast<WHAEnum>();
                        foreach (var t in types)
                        {
                            var select = form.ContainsKey($"model[makebill_{(int)t}_{u.Id}]");
                            wha.BillTypes.Add(new WareHouseAccessBillTypeModel()
                            {
                                BillTypeId = (int)t,
                                BillTypeName = CommonHelper.GetEnumDescription(t),
                                Selected = select
                            });
                        }
                        var query = form.ContainsKey($"model[stockquery_{u.Id}]");
                        wha.StockQuery = query;
                        model.WareHouseAccess.Add(wha);
                    }
                }

                model.StoreId = curStore.Id;
                //var wareHouse = _wareHouseService.GetWareHouseById(curStore.Id, model.Id);
                var wareHouse = model.ToEntity<WareHouse>(); ;
                if (wareHouse.Name.Equals("主仓库"))
                {
                    wareHouse.IsSystem = true;
                }
                wareHouse.AllowNegativeInventoryPreSale = model.AllowNegativeInventoryPreSale;
                wareHouse.WareHouseAccessSettings = JsonConvert.SerializeObject(model.WareHouseAccess);

                _wareHouseService.UpdateWareHouse(wareHouse);
                //活动日志
                _userActivityService.InsertActivity("EditWareHouse", "编辑仓库成功", curUser.Id);

                _notificationService.SuccessNotification("编辑仓库成功");
                return Successful("编辑仓库成功！");
            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("EditWareHouse", "编辑仓库失败", curUser.Id);
                _notificationService.ErrorNotification("编辑仓库失败");
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// 删除仓库
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeleteWareHouse(string ids)
        {

            try
            {
                if (!string.IsNullOrEmpty(ids))
                {

                    int[] sids = ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(u => int.Parse(u)).ToArray();
                    var list = _wareHouseService.GetWareHouseByIds(curStore.Id, sids);
                    bool checkWareHouse = _billConvertService.CheckWareHouse(int.Parse(ids));
                    if (checkWareHouse == true)
                    {
                        return Warning("该仓库不能被删除！");
                    }

                    foreach (var item in list)
                    {
                        _wareHouseService.DeleteWareHouse(item);
                    }

                    //活动日志
                    _userActivityService.InsertActivity("DeleteWareHouse", "删除仓库成功", curUser.Id);
                    _notificationService.SuccessNotification("删除仓库成功");
                    return Successful("删除仓库成功！");
                }
                else
                {
                    //活动日志
                    _userActivityService.InsertActivity("DeleteWareHouse", "删除仓库失败", curUser, curUser != null ? _workContext.CurrentUser.Id : 0);

                    _notificationService.ErrorNotification("删除仓库失败");
                    return Successful("删除仓库失败！");
                }
            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("DeleteWareHouse", "删除仓库失败", curUser.Id);

                _notificationService.ErrorNotification("删除仓库失败");
                return Error(ex.Message);
            }
        }

        /// 异步获取所有仓库
        /// </summary>
        /// <returns></returns>
        public JsonResult GetWareHouseAll()
        {
            List<WareHouseModel> list = new List<WareHouseModel>();
            try
            {
                IList<WareHouse> wareHouses = _wareHouseService.GetWareHouseList(_storeContext.CurrentStore != null ? _storeContext.CurrentStore.Id : 0);
                List<WareHouse> entityList = new List<WareHouse>();
                entityList.AddRange(wareHouses);
                entityList.ForEach(w =>
                {
                    list.Add(w.ToModel<WareHouseModel>());
                });
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }

            return Json(new { Success = true, rows = list });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public JsonResult InitWareHouse()
        {
            try
            {
                _installationService.InstallWarehouses(curStore.Id);
                _userActivityService.InsertActivity("InitWareHouse", "仓库初始化成功", curUser.Id);
                _notificationService.SuccessNotification("仓库初始化成功");
                return Successful("仓库初始化成功！");
            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("InitWareHouse", "仓库初始化失败", curUser.Id);
                _notificationService.ErrorNotification("仓库初始化失败");
                return Error(ex.Message);
            }
        }
    }
}