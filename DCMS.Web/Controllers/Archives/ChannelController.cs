using DCMS.Core;
using DCMS.Core.Domain.Terminals;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Terminals;
using DCMS.ViewModel.Models.Terminals;
using DCMS.Web.Framework.Mvc.Filters;
using DCMS.Web.Infrastructure.Mapper.Extensions;
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
    /// 用于渠道信息管理
    /// </summary>
    public class ChannelController : BasePublicController
    {
        private readonly IChannelService _channelService;
        private readonly IUserActivityService _userActivityService;
        private readonly ITerminalService _terminalService;

        public ChannelController(IChannelService channelService, IWorkContext workContext,
            ILogger loggerService,
            INotificationService notificationService,
            IStoreContext storeContext,
            IUserActivityService userActivityService,
            ITerminalService terminalService) : base(workContext, loggerService, storeContext, notificationService)
        {
            _channelService = channelService;
            _userActivityService = userActivityService;
            _terminalService = terminalService;
        }

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        [AuthCode((int)AccessGranularityEnum.ChannelListView)]
        public IActionResult List()
        {
            return View();
        }

        /// <summary>
        /// 渠道列表
        /// </summary>
        /// <param name="searchStr"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<JsonResult> ChannelList(string searchStr = "", int pageIndex = 0, int pageSize = 10)
        {
            return await Task.Run(() =>
            {

                var channelList = _channelService.GetChannels(searchStr, curStore?.Id ?? 0, pageIndex, pageSize);
                return Json(new
                {
                    Success = true,
                    total = channelList.TotalCount,
                    rows = channelList.Select(c =>
                    {
                        var model = c.ToModel<ChannelModel>();
                        if (Enum.IsDefined(typeof(ChannelAttributeType), c.Attribute))
                        {
                            model.AttributeName = CommonHelper.GetEnumDescription((ChannelAttributeType)Enum.Parse(typeof(ChannelAttributeType), c.Attribute.ToString()));
                        }
                        return model;
                    })
                });
            });
        }


        /// <summary>
        /// 添加渠道
        /// </summary>
        /// <returns></returns>
        public IActionResult AddChannel()
        {
            var model = new ChannelModel();
            IEnumerable<ChannelAttributeType> attributes = Enum.GetValues(typeof(ChannelAttributeType)).Cast<ChannelAttributeType>();
            model.Attributes = new SelectList(from purchase in attributes
                                              select new SelectListItem
                                              {
                                                  Text = CommonHelper.GetEnumDescription(purchase),
                                                  Value = ((int)purchase).ToString(),
                                                  Selected = purchase == 0 ? true : false
                                              }, "Value", "Text");
            return Json(new { Success = true, RenderHtml = RenderPartialViewToString("AddChannel", model) });
        }
        [HttpPost]
        //[ValidateInput(false)]
        public JsonResult AddChannel(ChannelModel model)
        {
            //经销商
            try
            {
                if (model != null)
                {
                    if(_channelService.GetChannelByName(curStore?.Id ?? 0, model.Name) == 0)
                    {
                        model.StoreId = curStore?.Id ?? 0;
                        model.Deleted = false;
                        model.CreateDate = DateTime.Now;
                        //添加渠道信息表
                        _channelService.InsertChannel(model.ToEntity<Channel>());
                        //活动日志
                        _userActivityService.InsertActivity("AddChannel", "渠道创建成功", curUser.Id);
                        //_notificationService.SuccessNotification("渠道创建成功");
                        return Successful("渠道创建成功！");
                    }
                    else
                    {
                        return Successful("渠道名称已存在！");
                    }
                }
                else
                {
                    //活动日志
                    _userActivityService.InsertActivity("AddChannel", "渠道创建失败", curUser.Id);
                    _notificationService.SuccessNotification("渠道创建失败");
                    return Successful("渠道创建失败！");
                }
            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("AddChannel", "渠道创建失败", curUser.Id);
                _notificationService.SuccessNotification("渠道创建失败");
                return Error(ex.Message);
            }

        }

        /// <summary>
        /// 编辑渠道信息
        /// </summary>
        /// <returns></returns>
        public JsonResult EditChannel(int id)
        {
            var channel = _channelService.GetChannelById(curStore.Id, id);
            if (channel == null)
            {
                return Warning("数据不存在!");
            }
            //只能操作当前经销商数据
            else if (channel.StoreId != curStore.Id)
            {
                return Warning("权限不足!");
            }

            var model = channel.ToModel<ChannelModel>();
            IEnumerable<ChannelAttributeType> attributes = Enum.GetValues(typeof(ChannelAttributeType)).Cast<ChannelAttributeType>();
            model.Attributes = new SelectList(from purchase in attributes
                                              select new SelectListItem
                                              {
                                                  Text = CommonHelper.GetEnumDescription(purchase),
                                                  Value = ((int)purchase).ToString(),
                                                  Selected = purchase == 0 ? true : false
                                              }, "Value", "Text");
            return Json(new { Success = true, RenderHtml = RenderPartialViewToString("AddChannel", model) });
        }

        [HttpPost]
        //[ValidateInput(false)]
        public JsonResult EditChannel(ChannelModel model)
        {
            try
            {
                if (model != null)
                {
                    var channel = _channelService.GetChannelById(curStore.Id, model.Id);
                    channel = model.ToEntity(channel);
                    //编辑渠道表
                    _channelService.UpdateChannel(channel);
                    //活动日志
                    _userActivityService.InsertActivity("EditChannel", "渠道修改成功", curUser.Id);
                    _notificationService.SuccessNotification("渠道修改成功");
                    return Successful("渠道修改成功！");
                }
                else
                {
                    //活动日志
                    _userActivityService.InsertActivity("EditChannel", "渠道修改失败", curUser.Id);
                    _notificationService.SuccessNotification("渠道修改失败");
                    return Successful("渠道修改失败！");
                }
            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("EditChannel", "渠道修改失败", curUser.Id);
                _notificationService.SuccessNotification("渠道修改失败");
                return Error(ex.Message);
            }

        }

        /// <summary>
        /// 删除渠道信息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeleteChannel(string ids)
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
                    var terminals = _terminalService.GetTerminalsByChannelid(curStore?.Id ?? 0,int.Parse(ids));
                    if (terminals.Count > 0)
                    {
                        return Warning("该渠道已分配过终端用户！");
                    }
                    else
                    {
                        int[] sids = ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(u => int.Parse(u)).ToArray();
                        var list = _channelService.GetChannelsByIds(curStore?.Id ?? 0, sids);

                        foreach (var wareHouse in list)
                        {
                            if (wareHouse != null)
                            {
                                _channelService.DeleteChannel(wareHouse);
                            }
                        }

                        //活动日志
                        _userActivityService.InsertActivity("DeleteChannel", "渠道删除成功", curUser.Id);
                        _notificationService.SuccessNotification("渠道删除成功");
                        return Successful("渠道删除成功！");
                    }

                }
                else
                {
                    //活动日志
                    _userActivityService.InsertActivity("DeleteChannel", "渠道删除失败", curUser.Id);
                    _notificationService.SuccessNotification("渠道删除失败");
                    return Successful("渠道删除失败！");
                }
            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("DeleteChannel", "渠道删除失败", curUser.Id);
                _notificationService.SuccessNotification("渠道删除失败");
                return Error(ex.Message);
            }

        }
    }
}