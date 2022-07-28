using DCMS.Core;
using DCMS.Core.Domain.Visit;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Terminals;
using DCMS.Services.Users;
using DCMS.Services.Visit;
using DCMS.ViewModel.Models.Users;
using DCMS.ViewModel.Models.Visit;
using DCMS.Web.Framework.Mvc.Filters;
using DCMS.Web.Infrastructure.Mapper.Extensions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace DCMS.Web.Controllers.Archives
{    /// <summary>
     /// 拜访线路设置
     /// </summary>
    public class LineTierController : BasePublicController
    {
        private readonly IUserService _userService;
        private readonly ILineTierService _lineTierService;
        private readonly ITerminalService _terminalService;
        private readonly IUserActivityService _userActivityService;

        public LineTierController(
            IWorkContext workContext,
            IUserService userService,
            IStoreContext storeContext,
            ILineTierService lineTierService,
            ITerminalService terminalService,
            ILogger loggerService,
            INotificationService notificationService,
            IUserActivityService userActivityService) : base(workContext, loggerService, storeContext, notificationService)
        {
            _userService = userService;
            _lineTierService = lineTierService;
            _terminalService = terminalService;
            _userActivityService = userActivityService;

        }

        // GET: LineTier
        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        [AuthCode((int)AccessGranularityEnum.InventoryProfitlossExport)]
        public IActionResult List(int? lineTierId)
        {
            var lineTierListModel = new LineTierListModel
            {
                LineTiers = _lineTierService.GetLineTiers(curStore?.Id ?? 0).Select(l => l.ToModel<LineTierModel>()).ToList()
            };
            if (lineTierId.HasValue)
            {
                lineTierListModel.LineTierOptions = _lineTierService.GetLineTierOptions(curStore?.Id ?? 0,lineTierId.Value);
            }
            return View(lineTierListModel);
        }


        #region 线路类别
        /// <summary>
        /// 新增线路类别信息
        /// </summary>
        /// <returns></returns>
        public IActionResult AddLineTier()
        {



            return Json(new
            {
                Success = true,
                RenderHtml = RenderPartialViewToString("AddLineTier", new LineTierModel() { Enabled = true })
            });
        }

        [HttpPost]
        public JsonResult AddLineTier(LineTierModel model)
        {
            //经销商
            model.StoreId = curStore?.Id ?? 0;
            model.Enabled = true;
            try
            {
                if(_lineTierService.GetLineTierByName(curStore?.Id ?? 0, model.Name) > 0)
                {
                    //活动日志
                    _userActivityService.InsertActivity("AddLineTier", "线路类别已使用", curUser.Id);
                    _notificationService.SuccessNotification("线路类别已使用");
                    return Error("线路类别已使用！");
                }
                else
                {
                    //添加线路类别表
                    _lineTierService.InsertLineTier(model.ToEntity<LineTier>());
                    //ICollection<LineTierOption> LineTierOption = model.ToEntity<LineTier>().LineTierOptions;
                    //活动日志
                    _userActivityService.InsertActivity("AddLineTier", "添加线路类别成功", curUser.Id);
                    _notificationService.SuccessNotification("添加线路类别成功");
                    return Successful("添加线路类别成功！");
                }
                
            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("AddLineTier", "添加线路类别失败", curUser.Id);
                _notificationService.SuccessNotification("添加线路类别失败");
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// 编辑线路类别信息
        /// </summary>
        /// <returns></returns>
        public JsonResult EditLineTier(int lineTierId)
        {
            _ = new LineTierModel();
            var lineTier = _lineTierService.GetLineTierById(curStore.Id, lineTierId);
            if (lineTier == null)
            {
                return Warning("数据不存在!");
            }
            else if (lineTier.StoreId != curStore.Id)
            {
                return Warning("权限不足!");
            }

            LineTierModel model = lineTier.ToModel<LineTierModel>();

            return Json(new { Success = true, RenderHtml = RenderPartialViewToString("AddLineTier", model) });
        }

        [HttpPost]
        public JsonResult EditLineTier(LineTierModel model)
        {
            try
            {
                var lineTier = _lineTierService.GetLineTierById(curStore.Id, model.Id);
                if (lineTier != null)
                {
                    lineTier = model.ToEntity(lineTier);
                    //编辑线路类别信息
                    _lineTierService.UpdateLineTier(lineTier);
                    //活动日志
                    _userActivityService.InsertActivity("EditLineTier", "修改线路类别成功", curUser.Id);
                    _notificationService.SuccessNotification("修改线路类别成功");
                    return Successful("修改线路类别成功！", model);
                }
                else
                {
                    //活动日志
                    _userActivityService.InsertActivity("EditLineTier", "修改线路类别失败", curUser.Id);
                    _notificationService.SuccessNotification("修改线路类别失败");
                    return Successful("修改线路类别失败！", model);
                }
            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("EditLineTier", "修改线路类别失败", curUser.Id);
                _notificationService.SuccessNotification("修改线路类别失败");
                return Error(ex.Message);
            }

        }

        /// <summary>
        /// 删除线路类别信息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeleteLineTier(int? lineTierId)
        {
            try
            {
                if (lineTierId.HasValue)
                {
                    //保存事务
                    //using (var scope = new TransactionScope())
                    //{
                    //删除线路要访问的终端

                    var TierOptionLineids = _terminalService.GetLineTierOptionLineids(curStore?.Id ?? 0,lineTierId ?? 0);
                    if (TierOptionLineids.Count > 0)
                    {
                        return Warning("该线路已分配过终端用户！");
                    }
                    else
                    {
                        var options = _lineTierService.GetLineTierOptions(curStore?.Id ?? 0, lineTierId.Value);
                        options.ToList().ForEach(option =>
                        {
                            _lineTierService.DeleteLineTierOption(option);
                        });
                        //删除线路
                        LineTier lineTier = _lineTierService.GetLineTierById(curStore.Id, lineTierId.Value);
                        _lineTierService.DeleteLineTier(lineTier);
                        //scope.Complete();
                        //}
                        //活动日志
                        _userActivityService.InsertActivity("DeleteLineTier", "删除线路类别成功", curUser.Id);
                        _notificationService.SuccessNotification("删除线路类别成功");
                        return Successful("删除线路类别成功！");
                    }

                }
                else
                {
                    //活动日志
                    _userActivityService.InsertActivity("DeleteLineTier", "删除线路类别失败", curUser.Id);
                    _notificationService.SuccessNotification("删除线路类别失败");
                    return Successful("删除线路类别失败！");
                }
            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("DeleteLineTier", "删除线路类别失败", curUser.Id);
                _notificationService.SuccessNotification("删除线路类别失败");
                return Error(ex.Message);
            }

        }
        #endregion

        #region 拜访线路

        //?order=asc&offset=0&limit=10&lineTierId=1099&_=1638771676924
        public async Task<JsonResult> LineTierOptionList(int lineTierId, bool macthing = false)
        {
            return await Task.Run(() =>
            {
                try
                {
                    //匹配线路
                    if (macthing)
                    {
                        var linopts = new List<LineTierOption>();
                        var terminals = _terminalService.GetTerminalsByLineId(curStore.Id, lineTierId);
                        if (terminals != null && terminals.Any())
                        {
                            foreach (var t in terminals)
                            {
                                var exists = _lineTierService.LineTierOptionExists(curStore.Id, t.LineId, t.Id);
                                if (!exists)
                                {
                                    linopts.Add(new LineTierOption()
                                    {
                                        StoreId = curStore.Id,
                                        LineTierId = lineTierId,
                                        TerminalId = t.Id,
                                        Order = 0,
                                        CreatedOnUtc = DateTime.Now
                                    });
                                }
                            }
                            if (linopts.Any())
                                _lineTierService.InsertLineTierOptions(linopts);
                        }
                    }

                    var options = _lineTierService.GetLineTierOptions(curStore?.Id ?? 0, lineTierId);
                    var models = options.Select(o =>
                    {
                        var model = o.ToModel<LineTierOptionModel>();
                        var terminal = _terminalService.GetTerminalById(curStore.Id, o.TerminalId);
                        if (terminal != null)
                        {
                            model.TerminalName = terminal.Name;
                            model.BossName = terminal.BossName;
                            model.BossCall = terminal.BossCall;
                            model.TerminalAddress = terminal.Address;
                        }
                        return model;
                    }).ToList();
                    var modelnull = new List<LineTierOptionModel>() { new LineTierOptionModel() { Id = 0, Order = 0, TerminalName = "" } };

                    return Json(new
                    {
                        total = options.Count,
                        rows = options.Count > 0 ? models : modelnull
                    });
                }
                catch (Exception ex) 
                {
                    return Json(new
                    {
                        total = 0,
                        rows = new List<LineTierOptionModel>()
                    });
                }
            });
        }

        [HttpPost]
        public JsonResult UpdateLineTierOptions(LineTierUpdateModel data, int lineTierId)
        {

            try
            {
                string Terminalname = "";
                var lineTier = _lineTierService.GetLineTierById(curStore.Id, lineTierId);
                if (lineTier == null)
                {
                    return Warning("线路类别不存在！");
                }

                if (data != null && data.Items != null)
                {
          
                    #region 更新线路项目

                    data.Items.ForEach(p =>
                    {
                        var lineTierOption = _lineTierService.GetLineTierOptionById(curStore.Id, p.Id);
                        if (lineTierOption == null)
                        {
                            //允许多线路指派
                            lineTierOption = new LineTierOption
                            {
                                StoreId = curStore.Id,
                                LineTierId = lineTierId,
                                TerminalId = p.TerminalId,
                                Order = p.Order,
                                CreatedOnUtc = DateTime.Now
                            };
                            _lineTierService.InsertLineTierOption(lineTierOption);
                        }
                        else
                        {
                            //存在则更新
                            lineTierOption.TerminalId = p.TerminalId;
                            lineTierOption.Order = p.Order;
                            _lineTierService.UpdateLineTierOption(lineTierOption);
                        }
                    });

                    #endregion

 
                    //var lineTierOptions = _lineTierService.GetLineTierOptions(curStore?.Id ?? 0, lineTierId);
                    //#region Grid 移除则从库移除删除项

                    //var dels = new List<LineTierOption>();
                    //lineTierOptions.ToList().ForEach(p =>
                    //{
                    //    if (data.Items.Count(cp => cp.Id == p.Id) == 0)
                    //    {
                    //        dels.Add(p);
                    //    }
                    //});

                    //if (dels.Any())
                    //    _lineTierService.DeleteLineTierOptions(dels);

                    //#endregion

                    //活动日志
                    _userActivityService.InsertActivity("UpdateLineTierOptions", "线路访问更新成功", curUser.Id);
                    _notificationService.SuccessNotification("线路访问更新成功");
                    if (Terminalname == "")
                    {
                        return Successful("线路访问更新成功");
                    }
                    else
                    {
                        return Warning(Terminalname + "已分配");
                    }
                }
                else
                {
                    //活动日志
                    _userActivityService.InsertActivity("UpdateLineTierOptions", "线路访问更新失败", curUser.Id);
                    _notificationService.SuccessNotification("线路访问更新失败");
                    return Successful("线路访问更新失败");
                }

            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("UpdateLineTierOptions", "线路访问更新失败", curUser.Id);
                _notificationService.SuccessNotification("线路访问更新失败");
                return Error(ex.Message);
            }

        }

        /// <summary>
        /// 删除线路访问信息
        /// </summary>
        /// <param name="lineTierId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeleteLineTierOption(int? lineTierOptionId)
        {
            if (!lineTierOptionId.HasValue)
            {
                return Warning("数据不存在!");
            }

            try
            {
                if (lineTierOptionId.HasValue)
                {
                    LineTierOption lineTierOption = _lineTierService.GetLineTierOptionById(curStore.Id, lineTierOptionId.Value);
                    _lineTierService.DeleteLineTierOption(lineTierOption);
                    //活动日志
                    _userActivityService.InsertActivity("DeleteLineTierOption", "删除线路类别成功", curUser.Id);
                    _notificationService.SuccessNotification("删除线路类别成功");
                    return Successful("删除线路类别成功！");
                }
                else
                {
                    //活动日志
                    _userActivityService.InsertActivity("DeleteLineTierOption", "删除线路类别失败", curUser.Id);
                    _notificationService.SuccessNotification("删除线路类别失败");
                    return Successful("删除线路类别失败！");
                }
            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("DeleteLineTierOption", "删除线路类别失败", curUser.Id);
                _notificationService.SuccessNotification("删除线路类别失败");
                return Error(ex.Message);
            }

        }
        #endregion

        #region 业务员拜访线路分配
        [AuthCode((int)AccessGranularityEnum.DistributionLineView)]
        public IActionResult Assign()
        {
            var lineTierListModel = new LineTierListModel
            {
                LineTiers = _lineTierService.GetLineTiers(curStore?.Id ?? 0).Select(l => { var model = l.ToModel<LineTierModel>(); model.Qty = l.LineTierOptions.Count; return model; }).ToList(),
                BusinessUsers = _userService.GetUserBySystemRoleName(curStore?.Id ?? 0, DCMSDefaults.Salesmans).Select(u => { var model = u.ToModel<UserModel>(); model.UserRealName = u.UserRealName; return model; }).ToList()
            };
            return View(lineTierListModel);
        }
        public async Task<JsonResult> UserLineTierAssignList(int userId)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var options = _lineTierService.GetUserLineTierAssigns(userId);
                    var models = options.Select(o =>
                    {
                        var model = o.ToModel<UserLineTierAssignModel>();
                        var lineTier = _lineTierService.GetLineTierById(curStore.Id, o.LineTierId, true);
                        if (lineTier != null)
                        {
                            model.LineTierName = lineTier.Name;
                            model.Quantity = lineTier.LineTierOptions.Count;
                        }
                        return model;
                    });
                    var modelnull = new List<UserLineTierAssignModel>() 
                    {
                        new UserLineTierAssignModel() { Id = 0, Order = 0, LineTierName = "" }
                    };
                    return Json(new
                    {
                        total = options.Count,
                        rows = options.Count > 0 ? models : modelnull
                    });
                }
                catch (Exception ex)
                {
                    return Json(new
                    {
                        total = 0,
                        rows = new List<UserLineTierAssignModel>(),
                        message = ex.Message
                    });
                }
            });
        }

        [HttpPost]
        public JsonResult UpdateUserLineTierAssigns(List<UserLineTierAssignModel> Items, int userId)
        {
            try
            {

                var user = _userService.GetUserById(curStore.Id, userId);
                if (user == null)
                {
                    return Warning("业务员不存在！");
                }

                if (Items != null)
                {
                    //保存事务
                    //using (var scope = new TransactionScope())
                    //{
                    //    scope.Complete();
                    //}
                    var userLineTierAssigns = _lineTierService.GetUserLineTierAssigns(userId);
                    foreach (var ul in Items)
                    {
                        //var userLineTier = userLineTierAssigns.Select(o => o).Where(o => o.Id == ul.Id).FirstOrDefault();
                        var userLineTier = _lineTierService.GetUserLineTierAssignById(curStore.Id, ul.Id);
                        if (userLineTier != null)
                        {
                            userLineTier = _lineTierService.GetUserLineTierAssignById(curStore.Id, userLineTier.Id);
                            //userLineTier = ul.ToEntity(userLineTier);
                            userLineTier.StoreId = curStore.Id;
                            userLineTier.Order = ul.Order;
                            userLineTier.LineTierId = ul.LineTierId;
                            userLineTier.UserId = userId;
                            _lineTierService.UpdateUserLineTierAssign(userLineTier);
                        }
                        else
                        {

                            if (ul.LineTierId != 0)
                            {
                                userLineTier = new UserLineTierAssign
                                {
                                    //Order,UserId,LineTierId,LineTier
                                    //userLineTier = ul.ToEntity(userLineTier);
                                    StoreId = curStore.Id,
                                    Order = ul.Order,
                                    LineTierId = ul.LineTierId,
                                    UserId = userId
                                };
                                _lineTierService.InsertUserLineTierAssign(userLineTier);
                            }
                        }
                    }

                    //活动日志
                    _userActivityService.InsertActivity("UpdateUserLineTierAssigns", "拜访线路分配成功", curUser.Id);
                    _notificationService.SuccessNotification("拜访线路分配成功");
                    return Successful("拜访线路分配成功");
                }
                else
                {
                    //活动日志
                    _userActivityService.InsertActivity("UpdateUserLineTierAssigns", "拜访线路分配失败", curUser.Id);
                    _notificationService.SuccessNotification("拜访线路分配失败");
                    return Successful("拜访线路分配失败");
                }
            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("UpdateUserLineTierAssigns", "拜访线路分配失败", curUser.Id);
                _notificationService.SuccessNotification("拜访线路分配失败");
                return Error(ex.Message);
            }

        }

        /// <summary>
        /// 删除拜访线路信息
        /// </summary>
        /// <param name="lineTierId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeleteUserLineTierAssign(int? userLineTierAssignId)
        {
            if (!userLineTierAssignId.HasValue)
            {
                return Warning("数据不存在!");
            }

            try
            {
                UserLineTierAssign userLineTierAssign = _lineTierService.GetUserLineTierAssignById(curStore.Id, userLineTierAssignId.Value);
                _lineTierService.DeleteUserLineTierAssign(userLineTierAssign);
                //活动日志
                _userActivityService.InsertActivity("DeleteUserLineTierAssign", "删除拜访线路成功", curUser.Id);
                _notificationService.SuccessNotification("删除拜访线路成功");
                return Successful("删除拜访线路成功！");
            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("DeleteUserLineTierAssign", "删除拜访线路失败", curUser.Id);
                _notificationService.SuccessNotification("删除拜访线路失败");
                return Error(ex.Message);
            }

        }
        #endregion
    }
}