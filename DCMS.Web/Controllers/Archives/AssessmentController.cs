using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Users;
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
{    
    
    /// <summary>
     /// 业绩考核
     /// </summary>
    public class AssessmentController : BasePublicController
    {
        private readonly IUserService _userService;
        private readonly ILineTierService _lineTierService;
        private readonly ITerminalService _terminalService;
        private readonly IUserActivityService _userActivityService;
        private readonly IUserAssessmentService _userAssessmentService;
        private readonly IRedLocker _locker;

        public AssessmentController(
            IWorkContext workContext,
            IUserService userService,
            IStoreContext storeContext,
            ILineTierService lineTierService,
            ITerminalService terminalService,
            ILogger loggerService,
            INotificationService notificationService,
            IUserActivityService userActivityService, 
            IUserAssessmentService userAssessmentService, IRedLocker locker) : base(workContext, loggerService, storeContext, notificationService)
        {
            _userService = userService;
            _lineTierService = lineTierService;
            _terminalService = terminalService;
            _userActivityService = userActivityService;
            _userAssessmentService = userAssessmentService;
            _locker = locker;
        }

        public IActionResult List(int? year, string name, int pagenumber = 0)
        {
            if (!year.HasValue)
            {
                year = DateTime.Now.Year;
            }
            UserAssessmentModel model = _userAssessmentService.GetUserAssessmentByStoreId(curStore.Id, year ?? 0)?.ToModel<UserAssessmentModel>();
            if (model == null)
            {
                model = new UserAssessmentModel();
                model.Items = new List<UserAssessmentItemModel>();
            }
            return View(model);
        }

        public JsonResult GetUserAssessmentByYear(int? year)
        {
            if (!year.HasValue)
            {
                year = DateTime.Now.Year;
            }
            UserAssessmentModel model = _userAssessmentService.GetUserAssessmentByStoreId(curStore.Id, year ?? 0)?.ToModel<UserAssessmentModel>();
            if (model == null)
            {
                model = new UserAssessmentModel();
                model.Year = year.Value;
                model.StoreId = curStore.Id;
                model.Name = "";
            }
            return Successful("", model);
        }

        [AuthCode((int)AccessGranularityEnum.AssessmentView)]
        public async Task<JsonResult> AsyncLists(int? AssessmentId)
        {
            return await Task.Run(() =>
            {
                var bussinessUsers = _userService.BindUserList(curStore.Id, DCMSDefaults.Salesmans, curUser.Id,false, _userService.IsAdmin(curStore.Id, curUser.Id));
                var items = new List<UserAssessmentItemModel>();
                if (AssessmentId > 0)
                {
                    items = _userAssessmentService.GetUserAssessmentItems(curStore.Id, AssessmentId ?? 0,bussinessUsers.Select(u => u.Id).ToList()).Select(item => item.ToModel<UserAssessmentItemModel>()).ToList();
                }
                else
                {
                    bussinessUsers.ForEach(user =>
                    {
                        var item = new UserAssessmentItemModel();
                        item.UserId = user.Id;
                        item.UserName = user.UserRealName;
                        item.StoreId = user.StoreId;
                        item.Id = 0;
                        items.Add(item);
                    });
                }
                return Json(new {
                    Success = true,
                    rows = items
                });
            });
        }

        [HttpPost]
        public async Task<JsonResult> UpdateUserAssessment(UserAssessmentModel data)
        {
            
            try
            {
                data.StoreId = curStore.Id;

                var result = await _locker.PerformActionWithLockAsync(LockKey(data),
                    TimeSpan.FromSeconds(30),
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromSeconds(1),
                    () => _userAssessmentService.UserAssessmentCreateOrUpdate(data.ToEntity<UserAssessment>(), data.Items.Select(item => item.ToEntity<UserAssessmentsItems>()).ToList()));
                
                _userActivityService.InsertActivity("UpdateUserAssessment", "业绩设置成功", curUser.Id);
                _notificationService.SuccessNotification("业绩设置成功");
                return Successful("业绩设置成功！");

            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("UpdateUserAssessment", "业绩设置失败", curUser.Id);
                _notificationService.SuccessNotification("业绩设置失败");
                return Error(ex.Message);
            }
        }
    }
}