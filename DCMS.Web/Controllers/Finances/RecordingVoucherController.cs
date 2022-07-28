using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Finances;
using DCMS.Services.Common;
using DCMS.Services.ExportImport;
using DCMS.Services.Finances;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Settings;
using DCMS.Services.Users;
using DCMS.ViewModel.Models.Finances;
using DCMS.Web.Framework.Extensions;
using DCMS.Web.Framework.Mvc.Filters;
using DCMS.Web.Infrastructure.Mapper.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace DCMS.Web.Controllers
{
    /// <summary>
    /// 用于记账凭证管理
    /// </summary>
    public class RecordingVoucherController : BasePublicController
    {
        private readonly IUserService _userService;
        private readonly IAccountingService _accountingService;
        private readonly IUserActivityService _userActivityService;
        private readonly IRecordingVoucherService _recordingVoucherService;
        private readonly IBillConvertService _billConvertService;
        private readonly IRedLocker _locker;
        private readonly IExportManager _exportManager;
        private readonly ICommonBillService _commonBillService;



        public RecordingVoucherController(
            IWorkContext workContext,
            IStoreContext storeContext,
            ILogger loggerService,
            IAccountingService accountingService,
            IUserActivityService userActivityService,
            INotificationService notificationService,
            IRecordingVoucherService recordingVoucherService,
            IBillConvertService billConvertService,
            IUserService userService,
            IRedLocker locker,
            IExportManager exportManager,
            ICommonBillService commonBillService
            ) : base(workContext, loggerService, storeContext, notificationService)
        {
            _accountingService = accountingService;
            _userService = userService;
            _userActivityService = userActivityService;
            _recordingVoucherService = recordingVoucherService;
            _billConvertService = billConvertService;
            _locker = locker;
            _exportManager = exportManager;
            _commonBillService = commonBillService;
        }

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        /// <summary>
        /// 凭证列表
        /// </summary>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.EntryVoucherView)]
        public IActionResult List(string recordName, int? generateModeId, string billNumber = "", string summary = "", int? accountingOptionId = -1, string accountingOptionName = "", bool? auditedStatus = null, DateTime? startTime = null, DateTime? endTime = null, int? billTypeId = null, int pagenumber = 0)
        {

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var billTypes = new[] { 12, 14, 22, 24, 32, 33, 34, 37, 38 };
            var types = CommonHelper.EnumToSelectListItem<BillTypeEnum>("单据类型", "0")
                .Where(s => billTypes.Contains(int.Parse(s.Value)))
                .ToList();

            var model = new RecordingVoucherListModel
            {
                RecordName = recordName,
                GenerateModeId = generateModeId ?? null,
                BillNumber = billNumber,
                Summary = summary,
                AccountingOptionId = accountingOptionId,
                AccountingOptionName = accountingOptionName,
                BillTypeId = billTypeId ?? null,
                StartTime = (startTime == null) ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")) : startTime,
                EndTime = (endTime == null) ? DateTime.Now.AddDays(1) : endTime,
                AuditedStatus = auditedStatus,
                BillTypes = new SelectList(types, "Value", "Text")
            };

            var recordingVouchers = _recordingVoucherService.GetAllRecordingVouchers(curStore?.Id ?? 0,
                 curUser.Id,
                generateModeId,
                billNumber,
                summary,
                auditedStatus,
                model.StartTime, //?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")),
                model.EndTime, //?? DateTime.Now.AddDays(1),
                billTypeId,
                recordName,
                accountingOptionId,
                pagenumber,
                30);

            model.PagingFilteringContext.LoadPagedList(recordingVouchers);

            var recordingVouchers2 = recordingVouchers.ToList();

            #region 查询需要关联其他表的数据

            List<int> userIds = new List<int>();
            if (recordingVouchers != null && recordingVouchers.Count > 0)
            {
                recordingVouchers.ToList().ForEach(rv =>
                {
                    userIds.Add(rv.MakeUserId);
                    userIds.Add(rv.AuditedUserId ?? 0);
                });
            }

            var allUsers = _userService.GetUsersDictsByIds(curStore.Id, userIds.ToArray());

            List<int> accountingOptionIds = new List<int>();
            if (recordingVouchers != null && recordingVouchers.Count > 0)
            {
                recordingVouchers.ToList().ForEach(bill =>
                {
                    if (bill != null && bill.Items != null && bill.Items.Count > 0)
                    {
                        accountingOptionIds.AddRange(bill.Items.Select(ba => ba.AccountingOptionId));
                    }
                });
            }
            var allAccountingOptions = _accountingService.GetAccountingOptionsByIds(curStore.Id, accountingOptionIds.ToArray());

            #endregion

            model.Items = recordingVouchers2.Select(r =>
            {
                var m = r.ToModel<RecordingVoucherModel>();

                m.Vouchers = r.Items.Select(v =>
                {
                    var vm = v.ToModel<VoucherItemModel>();
                    var option = allAccountingOptions.Where(ao => ao.Id == vm.AccountingOptionId).FirstOrDefault();
                    vm.AccountingOptionName = option != null ? option.Name : "";
                    return vm;

                }).ToList();

                //制单人
                m.MakeUserName = allUsers.Where(aw => aw.Key == m.MakeUserId).Select(aw => aw.Value).FirstOrDefault();

                //审核人
                m.AuditedUserName = allUsers.Where(aw => aw.Key == m.AuditedUserId).Select(aw => aw.Value).FirstOrDefault();



                m.GenerateModeName = CommonHelper.GetEnumDescription((GenerateMode)Enum.Parse(typeof(GenerateMode), r.GenerateMode.ToString()));

                m.BillLink = _billConvertService.GenerateBillUrl(m.BillTypeId, m.BillId);

                return m;
            }).OrderByDescending(v => v.RecordTime).ToList();

            return View(model);
        }

        /// <summary>
        /// 添加凭证
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.EntryVoucherSave)]
        public IActionResult Create(int? store)
        {
            var model = new RecordingVoucherModel
            {
                RecordTime = DateTime.Now,
                RecordName = "记"
            };
            var maxRecordingVoucher = _recordingVoucherService.GetAllRecordingVouchers(curStore?.Id ?? 0).OrderByDescending(r => r.Id).FirstOrDefault();
            model.RecordNumber = maxRecordingVoucher != null ? maxRecordingVoucher.Id + 1 : 0;

            return View(model);
        }

        /// <summary>
        /// 编辑凭证
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.EntryVoucherView)]
        public IActionResult Edit(int id = 0)
        {
            var model = new RecordingVoucherModel();
            var recordingVoucher = _recordingVoucherService.GetRecordingVoucherById(curStore.Id, id);
            if (recordingVoucher == null)
            {
                return RedirectToAction("List");
            }

            if (recordingVoucher != null)
            {
                //只能操作当前经销商数据
                if (recordingVoucher.StoreId != curStore.Id)
                {
                    return RedirectToAction("List");
                }

                model = recordingVoucher.ToModel<RecordingVoucherModel>();
                model.Vouchers = recordingVoucher.Items.Select(v => v.ToModel<VoucherItemModel>()).ToList();
            }

            //制单人
            //var mu = _userService.GetUserById(curStore.Id, recordingVoucher.MakeUserId);
            //model.MakeUserName = mu != null ? (mu.UserRealName + " " + recordingVoucher.RecordTime.ToString("yyyy/MM/dd HH:mm:ss")) : "";
            var mu = string.Empty;
            if (recordingVoucher.MakeUserId > 0)
            {
                mu = _userService.GetUserName(curStore.Id, recordingVoucher.MakeUserId);
            }
            model.MakeUserName = mu + " " + recordingVoucher.RecordTime.ToString("yyyy/MM/dd HH:mm:ss");

            //审核人
            //var au = _userService.GetUserById(curStore.Id, recordingVoucher.AuditedUserId ?? 0);
            //model.AuditedUserName = au != null ? (au.UserRealName + " " + (recordingVoucher.AuditedDate.HasValue ? recordingVoucher.AuditedDate.Value.ToString("yyyy/MM/dd HH:mm:ss") : "")) : "";
            var au = string.Empty;
            if (recordingVoucher.AuditedUserId != null && recordingVoucher.AuditedUserId > 0)
            {
                au = _userService.GetUserName(curStore.Id, recordingVoucher.AuditedUserId ?? 0);
            }
            model.AuditedUserName = au + " " + (recordingVoucher.AuditedDate.HasValue ? recordingVoucher.AuditedDate.Value.ToString("yyyy/MM/dd HH:mm:ss") : "");

            return View(model);
        }


        /// <summary>
        /// 凭证
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.EntryVoucherView)]
        public IActionResult View(int id = 0, string returnUrl = "")
        {
            var model = new RecordingVoucherModel();
            var recordingVoucher = _recordingVoucherService.GetRecordingVoucherById(curStore.Id, id, true);
            if (recordingVoucher == null)
            {
                return RedirectToAction("List");
            }

            if (recordingVoucher != null)
            {
                //只能操作当前经销商数据
                if (recordingVoucher.StoreId != curStore.Id)
                {
                    return RedirectToAction("List");
                }

                model = recordingVoucher.ToModel<RecordingVoucherModel>();
                model.Vouchers = recordingVoucher.Items.Select(v => v.ToModel<VoucherItemModel>()).ToList();
            }

            //制单人
            //var mu = _userService.GetUserById(curStore.Id, recordingVoucher.MakeUserId);
            //model.MakeUserName = mu != null ? (mu.UserRealName + " " + recordingVoucher.RecordTime.ToString("yyyy/MM/dd HH:mm:ss")) : "";
            var mu = string.Empty;
            if (recordingVoucher.MakeUserId > 0)
            {
                mu = _userService.GetUserName(curStore.Id, recordingVoucher.MakeUserId);
            }
            model.MakeUserName = mu + " " + recordingVoucher.RecordTime.ToString("yyyy/MM/dd HH:mm:ss");

            //审核人
            //var au = _userService.GetUserById(curStore.Id, recordingVoucher.AuditedUserId ?? 0);
            //model.AuditedUserName = au != null ? (au.UserRealName + " " + (recordingVoucher.AuditedDate.HasValue ? recordingVoucher.AuditedDate.Value.ToString("yyyy/MM/dd HH:mm:ss") : "")) : "";
            var au = string.Empty;
            if (recordingVoucher.AuditedUserId != null && recordingVoucher.AuditedUserId > 0)
            {
                au = _userService.GetUserName(curStore.Id, recordingVoucher.AuditedUserId ?? 0);
            }
            model.AuditedUserName = au + " " + (recordingVoucher.AuditedDate.HasValue ? recordingVoucher.AuditedDate.Value.ToString("yyyy/MM/dd HH:mm:ss") : "");

            return View(model);
        }



        /// <summary>
        /// 审核录入凭证
        /// </summary>
        /// <param name="billId"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.EntryVoucherApproved)]
        public async Task<JsonResult> AsyncAudited(int? billId)
        {
            try
            {
                RecordingVoucher recordingVoucher = new RecordingVoucher();
                #region 验证
                if (!billId.HasValue)
                {
                    return Warning("参数错误.");
                }
                else
                {
                    recordingVoucher = _recordingVoucherService.GetRecordingVoucherById(curStore.Id, billId.Value);
                }

                //公共单据验证
                var commonBillChecking = BillChecking<RecordingVoucher, VoucherItem>(recordingVoucher, BillStates.Audited);
                if (commonBillChecking.Value != null)
                {
                    return commonBillChecking;
                }

                #endregion

                //RedLock
                string lockKey = string.Format(DCMSCachingDefaults.RedisDataReSubmitKey, Request.GetUrl(), curStore.Id, curUser.Id, CommonHelper.MD5(JsonConvert.SerializeObject(billId)));
                var result = await _locker.PerformActionWithLockAsync(lockKey,
                      TimeSpan.FromSeconds(30),
                      TimeSpan.FromSeconds(10),
                      TimeSpan.FromSeconds(1),
                      () => _recordingVoucherService.Auditing(curStore.Id, curUser.Id, recordingVoucher));
                return Json(result);

            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("Auditing", "单据审核失败", curUser.Id);
                _notificationService.SuccessNotification("单据审核失败");
                return Error(ex.Message);
            }
        }

        #region 单据项目

        /// <summary>
        /// 异步获取项目
        /// </summary>
        /// <param name="voucherId"></param>
        /// <returns></returns>
        public async Task<JsonResult> AsyncVoucherItems(int voucherId)
        {
            return await Task.Run(() =>
            {
                var items = _recordingVoucherService.GetVoucherItemsByRecordingVoucherId(voucherId, _workContext.CurrentUser.Id, _storeContext.CurrentStore.Id, 0, 30).Select(o =>
                  {
                      var m = o.ToModel<VoucherItemModel>();
                      var option = _accountingService.GetAccountingOptionById(m.AccountingOptionId);
                      m.AccountingOptionName = option != null ? option.Name : "";
                      return m;

                  }).ToList();

                return Json(new
                {
                    Success = true,
                    total = items.Count,
                    rows = items
                });
            });
        }



        /// <summary>
        /// 创建/更新录入凭证
        /// </summary>
        /// <param name="data"></param>
        /// <param name="voucherId"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthCode((int)AccessGranularityEnum.EntryVoucherSave)]
        public async Task<JsonResult> CreateOrUpdate(RecordingVoucherUpdateModel data, int? voucherId)
        {

            try
            {
                RecordingVoucher recordingVoucher = new RecordingVoucher();

                if (data == null || data.Items == null)
                {
                    return Warning("请录入数据.");
                }

                if (PeriodLocked(DateTime.Now))
                {
                    return Warning("锁账期间,禁止业务操作.");
                }

                if (PeriodClosed(DateTime.Now))
                {
                    return Warning("会计期间已结账,禁止业务操作.");
                }

                #region 单据验证
                if (voucherId.HasValue && voucherId.Value != 0)
                {
                    recordingVoucher = _recordingVoucherService.GetRecordingVoucherById(curStore.Id, voucherId.Value);

                    //公共单据验证
                    var commonBillChecking = BillChecking<RecordingVoucher, VoucherItem>(recordingVoucher, BillStates.Draft);
                    if (commonBillChecking.Value != null)
                    {
                        return commonBillChecking;
                    }
                }

                #endregion

                //业务逻辑
                var dataTo = data.ToEntity<RecordingVoucherUpdate>();
                dataTo.Items = data.Items.Select(it =>
                {
                    return it.ToEntity<VoucherItem>();
                }).ToList();

                //RedLock
                var result = await _locker.PerformActionWithLockAsync(LockKey(data),
                      TimeSpan.FromSeconds(30),
                      TimeSpan.FromSeconds(10),
                      TimeSpan.FromSeconds(1),
                      () => _recordingVoucherService.BillCreateOrUpdate(curStore.Id, curUser.Id, voucherId, recordingVoucher, dataTo, dataTo.Items, _userService.IsAdmin(curStore.Id, curUser.Id)));
                return Json(result);
            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("CreateOrUpdate", "凭证创建/更新失败", curUser.Id);
                _notificationService.SuccessNotification("凭证创建/更新失败");
                return Error(ex.Message);
            }

        }


        #endregion

        //导出
        [AuthCode((int)AccessGranularityEnum.EntryVoucherExport)]
        public FileResult Export(int type, string selectData, string recordName, int? generateModeId, string billNumber = "", string summary = "", int? accountingOptionId = -1, string accountingOptionName = "", bool? auditedStatus = null, DateTime? startTime = null, DateTime? endTime = null, int? billTypeId = -1)
        {

            #region 查询导出数据
            IList<RecordingVoucher> recordingVouchers = new List<RecordingVoucher>();

            if (type == 1)
            {
                if (!string.IsNullOrEmpty(selectData))
                {
                    List<string> ids = selectData.Split(',').ToList();
                    foreach (var id in ids)
                    {
                        RecordingVoucher recordingVoucher = _recordingVoucherService.GetRecordingVoucherById(curStore.Id, int.Parse(id));
                        if (recordingVoucher != null)
                        {
                            recordingVouchers.Add(recordingVoucher);
                        }
                    }
                }
            }
            else if (type == 2)
            {
                recordingVouchers = _recordingVoucherService.GetAllRecordingVouchers(curStore?.Id ?? 0,
                     curUser.Id,
                                    generateModeId,
                                    billNumber,
                                    summary,
                                    auditedStatus,
                                    startTime, //?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")),
                                    endTime, //?? DateTime.Now.AddDays(1),
                                    0);
            }

            #endregion

            #region 导出
            var ms = _exportManager.ExportRecordingVoucherToXlsx(recordingVouchers);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "记账凭证.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "记账凭证.xlsx");
            }
            #endregion

        }

        /// <summary>
        /// 用户欠款金额
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<JsonResult> GetUserOwnCash(int userId = 0)
        {
            if (userId == 0)
            {
                userId = curUser?.Id ?? 0;
            }

            return await Task.Run(() =>
            {
                if (userId > 0)
                {
                    var amount = _commonBillService.GetUserAvailableOweCash(curStore.Id, userId);
                    return Json(new { UserUsedAmount = amount.Item1, UserAvailableOweCash = amount.Item2 });
                }
                else
                {
                    return Json(new { UserUsedAmount = 0, UserAvailableOweCash = 0 });
                }
            });
        }
    }
}