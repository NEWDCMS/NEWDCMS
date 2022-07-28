using DCMS.Core;
using DCMS.Core.Domain.Terminals;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Terminals;
using DCMS.ViewModel.Models.Terminals;
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
    /// 用于等级信息管理
    /// </summary>
    public class RankController : BasePublicController
    {
        private readonly IRankService _rankService;
        private readonly IUserActivityService _userActivityService;
        private readonly ITerminalService _terminalService;

        public RankController(IRankService rankService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ILogger loggerService,
            INotificationService notificationService,
            IUserActivityService userActivityService, ITerminalService terminalService) : base(workContext, loggerService, storeContext, notificationService)
        {
            _rankService = rankService;
            _userActivityService = userActivityService;
            _terminalService = terminalService;
        }

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        [AuthCode((int)AccessGranularityEnum.CustomerLelelView)]
        public IActionResult List()
        {
            return View();
        }

        /// <summary>
        /// 等级信息列表
        /// </summary>
        /// <param name="searchStr"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<JsonResult> RankList(string searchStr = "", int pageIndex = 0, int pageSize = 10)
        {
            return await Task.Run(() =>
            {

                var ranklist = _rankService.GetRanks(searchStr, curStore?.Id ?? 0, pageIndex, pageSize);
                return Json(new
                {
                    total = ranklist.TotalCount,
                    rows = ranklist.Select(r => { return r.ToModel<RankModel>(); })
                });
            });
        }


        /// <summary>
        /// 添加等级
        /// </summary>
        /// <returns></returns>
        public JsonResult AddRank()
        {
            return Json(new { Success = true, RenderHtml = RenderPartialViewToString("AddRank", new RankModel() { }) });
        }

        [HttpPost]
        public JsonResult AddRank(RankModel model)
        {
            try
            {
                if (model != null)
                {
                    //经销商
                    //
                    model.StoreId = curStore?.Id ?? 0;
                    model.Deleted = false;
                    model.CreateDate = DateTime.Now;
                    //添加等级信息表
                    _rankService.InsertRank(model.ToEntity<Rank>());
                    //活动日志
                    _userActivityService.InsertActivity("AddRank", "添加等级成功", curUser.Id);
                    _notificationService.SuccessNotification("添加等级成功");
                    return Successful("添加等级成功！");
                }
                else
                {
                    //活动日志
                    _userActivityService.InsertActivity("AddRank", "添加等级失败", curUser.Id);
                    _notificationService.SuccessNotification("添加等级失败");
                    return Successful("添加等级失败！");
                }
            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("AddRank", "添加等级失败", curUser.Id);
                _notificationService.SuccessNotification("添加等级失败");
                return Error(ex.Message);
            }

        }

        /// <summary>
        /// 编辑等级信息
        /// </summary>
        /// <returns></returns>
        public JsonResult EditRank(int id)
        {
            var rank = _rankService.GetRankById(curStore.Id, id);
            if (rank == null)
            {
                return Warning("数据不存在!");
            }

            //只能操作当前经销商数据
            else if (rank.StoreId != curStore.Id)
            {
                return Warning("权限不足!");
            }

            return Json(new { Success = true, RenderHtml = RenderPartialViewToString("AddRank", rank.ToModel<RankModel>()) });
        }

        [HttpPost]
        public JsonResult EditRank(RankModel model)
        {

            try
            {
                if (model != null)
                {
                    var rank = _rankService.GetRankById(curStore.Id, model.Id);
                    rank = model.ToEntity(rank);
                    //编辑等级表
                    _rankService.UpdateRank(rank);
                    //活动日志
                    _userActivityService.InsertActivity("EditRank", "修改等级成功", curUser.Id);
                    _notificationService.SuccessNotification("修改等级成功");
                    return Successful("修改等级成功！");
                }
                else
                {
                    //活动日志
                    _userActivityService.InsertActivity("EditRank", "修改等级失败", curUser.Id);
                    _notificationService.SuccessNotification("修改等级失败");
                    return Successful("修改等级失败！");
                }
            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("EditRank", "修改等级失败", curUser.Id);
                _notificationService.SuccessNotification("修改等级失败");
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// 删除等级信息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeleteRank(string ids)
        {
            try
            {
                if (!string.IsNullOrEmpty(ids))
                {
                    //保存事务
                    //using (var scope = new TransactionScope())
                    //{
                    var terminals = _terminalService.GetRankTerminalIds(curStore.Id, int.Parse(ids));
                    if (terminals.Count > 0)
                    {
                        return Warning("该等级已分配过终端用户！");
                    }
                    else
                    {
                        int[] idArr = ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(u => int.Parse(u)).ToArray();
                        var list = _rankService.GetRanksByIds(curStore.Id, idArr);

                        foreach (var rank in list)
                        {
                            if (rank != null)
                            {
                                rank.Deleted = true;
                                _rankService.UpdateRank(rank);
                            }
                        }
                        //scope.Complete();
                        //}
                        //活动日志
                        _userActivityService.InsertActivity("DeleteRank", "删除等级成功", curUser.Id);
                        _notificationService.SuccessNotification("删除等级成功");
                        return Successful("删除等级成功！");
                    }
                }
                else
                {
                    //活动日志
                    _userActivityService.InsertActivity("DeleteRank", "删除等级失败", curUser.Id);
                    _notificationService.SuccessNotification("删除等级失败");
                    return Successful("删除等级失败！");
                }
            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("DeleteRank", "删除等级失败", curUser.Id);
                _notificationService.SuccessNotification("删除等级失败");
                return Error(ex.Message);
            }

        }
    }
}