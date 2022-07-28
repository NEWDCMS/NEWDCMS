using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Common;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Terminals;
using DCMS.Core.Domain.Visit;
using DCMS.Core.Infrastructure;
using DCMS.Services.Common;
using DCMS.Services.Configuration;
using DCMS.Services.ExportImport;
using DCMS.Services.Finances;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Terminals;
using DCMS.Services.Visit;
using DCMS.ViewModel.Models.Terminals;
using DCMS.Web.Framework.Mvc.Filters;
using DCMS.Web.Framework.UI;
using DCMS.Web.Infrastructure.Mapper.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using DCMS.Core.Domain.CRM;
namespace DCMS.Web.Controllers
{
    /// <summary>
    /// 用于终端信息管理
    /// </summary>
    public class TerminalController : BasePublicController
    {
        private readonly IExportManager _exportManager;
        private readonly IUserActivityService _userActivityService;
        private readonly ITerminalService _terminalService;
        private readonly IDistrictService _districtService;
        private readonly IChannelService _channelService;
        private readonly IRankService _rankService;
        private readonly ILineTierService _lineTierService;
        private readonly ISettingService _settingService;
        private readonly IRecordingVoucherService _recordingVoucherService;
        private readonly IImportManager _importManager;
        private readonly ICommonBillService _commonBillService;
        private readonly IDCMSFileProvider _dCMSFileProvider;



        public TerminalController(
            IUserActivityService userActivityService,
            IWorkContext workContext,
            IStaticCacheManager cacheManager,
            ITerminalService terminalService,
            IDistrictService districtService,
            IChannelService channelService,
            IRankService rankService,
            ILineTierService lineTierService,
            ISettingService settingService,
            IStoreContext storeContext,
            ILogger loggerService,
            INotificationService notificationService,
            IRecordingVoucherService recordingVoucherService,
            IExportManager exportManager,
            IImportManager importManager,
            IDCMSFileProvider dCMSFileProvider,
            ICommonBillService commonBillService) : base(workContext, loggerService, storeContext, notificationService)
        {
            _userActivityService = userActivityService;
            _terminalService = terminalService;
            _districtService = districtService;
            _channelService = channelService;
            _rankService = rankService;
            _lineTierService = lineTierService;
            _settingService = settingService;
            _recordingVoucherService = recordingVoucherService;
            _exportManager = exportManager;
            _importManager = importManager;
            _commonBillService = commonBillService;
            _dCMSFileProvider = dCMSFileProvider;
        }

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public IActionResult AddMap()
        {
            return PartialView();
        }

        #region 终端
        [AuthCode((int)AccessGranularityEnum.EndPointListView)]
        public IActionResult List()
        {
            var model = new TerminalListModel()
            {
                DistrictList = _districtService.GetAll(curStore?.Id ?? 0).ToList(),
                ChannelList = _channelService.GetAll(curStore?.Id ?? 0).Select(c => { return new SelectListItem() { Text = c.Name, Value = c.Id.ToString() }; }),
                RankList = _rankService.GetAll(curStore?.Id ?? 0).Select(r => { return new SelectListItem() { Text = r.Name, Value = r.Id.ToString() }; })
            };
            return View("List", model);
        }

        /// <summary>
        /// 终端列表
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> TerminalList(int? districtId = null, string searchStr = "", int? channelId = null, int? rankId = null, int? lineId = 0, bool? status = true, int pageIndex = 0, int pageSize = 10)
        {
            return await Task.Run(() =>
            {
                var terminalList = _terminalService.GetTerminals(curStore?.Id ?? 0, 0, new List<int> { districtId ?? 0 }, searchStr, channelId, rankId, lineId, status, true, pageIndex, pageSize);

                return Json(new
                {
                    Success = true,
                    total = terminalList.TotalCount,
                    rows = terminalList.Select(t =>
                    {
                        var district = _districtService.GetDistrictById(curStore.Id, t.DistrictId);
                        var channel = _channelService.GetChannelById(curStore.Id, t.ChannelId);
                        var rank = _rankService.GetRankById(curStore.Id, t.RankId);
                        var line = _lineTierService.GetLineTierById(curStore.Id, t.LineId);
                        var model = t.ToModel<TerminalModel>();

                        if (!string.IsNullOrEmpty(t.MnemonicName))
                            model.MnemonicName = t.MnemonicName.Equals("NULL") ? "" : t.MnemonicName;
                        if (!string.IsNullOrEmpty(t.BossCall))
                            model.BossCall = t.BossCall.Equals("NULL") ? "" : t.BossCall;
                        if (!string.IsNullOrEmpty(t.BossName))
                            model.BossName = t.BossName.Equals("NULL") ? "" : t.BossName;
                        if (!string.IsNullOrEmpty(t.BusinessNo))
                            model.BusinessNo = t.BusinessNo.Equals("NULL") ? "" : t.BusinessNo;
                        if (!string.IsNullOrEmpty(t.Code))
                            model.Code = t.Code.Equals("NULL") ? "" : t.Code;

                        model.DistrictName = district == null ? "" : district.Name;//片区
                        model.ChannelName = channel == null ? "" : channel.Name;//渠道
                        model.LineName = line == null ? "" : line.Name;//线路
                        model.RankName = rank == null ? "" : rank.Name;//客户等级
                        model.PaymentMethod = t.PaymentMethod;
                        return model;
                    })
                });
            });
        }

        /// <summary>
        /// 添加终端
        /// </summary>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.EndPointListSave)]
        public JsonResult AddTerminal(int districtId)
        {
            return Json(new
            {
                Success = true,
                RenderHtml = RenderPartialViewToString("AddTerminal", new TerminalModel()
                {
                    IsNewAdd = true,
                    DistrictList = new SelectList(_districtService.GetAll(curStore?.Id ?? 0).Select(d => { return new SelectListItem() { Text = d.Name, Value = d.Id.ToString() }; }), "Value", "Text"),
                    ChannelList = new SelectList(_channelService.GetAll(curStore?.Id ?? 0).Select(c => { return new SelectListItem() { Text = c.Name, Value = c.Id.ToString() }; }), "Value", "Text"),
                    RankList = new SelectList(_rankService.GetAll(curStore?.Id ?? 0).Select(r => { return new SelectListItem() { Text = r.Name, Value = r.Id.ToString() }; }), "Value", "Text"),
                    PaymentMethodType = new SelectList(from a in Enum.GetValues(typeof(PaymentMethodType)).Cast<PaymentMethodType>()
                                                       select new SelectListItem
                                                       {
                                                           Text = CommonHelper.GetEnumDescription(a),
                                                           Value = ((int)a).ToString(),
                                                           Selected = a == 0 ? true : false
                                                       }, "Value", "Text"),
                    LineList = new SelectList(_lineTierService.GetAll(curStore?.Id ?? 0).Select(l => { return new SelectListItem() { Text = l.Name, Value = l.Id.ToString() }; }), "Value", "Text"),

                    //默认经纬度大地原点
                    Location_Lng = 108.5525,
                    Location_Lat = 34.3227
                })
            });

        }

        [HttpPost]
        [AuthCode((int)AccessGranularityEnum.EndPointListSave)]
        public JsonResult AddTerminal(TerminalModel model)
        {
            try
            {
                if (model != null)
                {
                    bool fg = true;
                    string errMsg = string.Empty;

                    if (model.Location_Lng == null || model.Location_Lat == null)
                    {
                        //fg = false;
                        //errMsg += (errMsg == "" ? "" : ",") + "请在地图信息中选择终端位置";
                        //默认经纬度大地原点
                        model.Location_Lng = 108.5525;
                        model.Location_Lat = 34.3227;
                    }
                    if (Regex.IsMatch(model.BossCall, @"^1(3|4|5|7|8)\d{9}$") == false)
                    {
                        fg = false;
                        errMsg += (errMsg == "" ? "" : ",") + "号码格式错误";
                    }
                    if (fg)
                    {
                        //经销商
                        var terminal = model.ToEntity<Terminal>();
                        terminal.StoreId = curStore?.Id ?? 0;
                        terminal.CreatedUserId = curUser.Id;
                        terminal.CreatedOnUtc = DateTime.Now;
                        terminal.Deleted = false;
                        terminal.IsNewAdd = model.IsNewAdd;

                        //添加终端表
                        _terminalService.InsertTerminal(terminal, curStore.Code);

                        //添加映射
                        var relation = new CRM_RELATION()
                        {
                            TerminalId = terminal.Id,
                            StoreId = terminal.StoreId,
                            CreatedOnUtc = DateTime.Now,
                            //DCMS新增终端编码规则(经销商编码+自增ID)
                            PARTNER1 = $"{curStore.Code}{terminal.Id}",
                            //经销商编码
                            PARTNER2 = curStore.Code,
                            RELTYP = "",
                            ZUPDMODE = "",
                            ZDATE = DateTime.Now
                        };
                        _terminalService.InsertRelation(relation);

                        //上报
                        if (terminal.IsNewAdd)
                        {
                            var nw = new NewTerminal()
                            {
                                StoreId = terminal.StoreId,
                                CreatedUserId = terminal.CreatedUserId,
                                Status = 1,
                                TerminalId = terminal.Id,
                                CreatedOnUtc = DateTime.Now
                            };
                            _terminalService.InsertNewTerminal(nw);
                        }


                        //片区
                        var lineTierOptions = _lineTierService.GetLineTierOptionOnlyOne(curStore.Id, terminal.Id);
                        if (lineTierOptions != null)
                        {
                            _lineTierService.DeleteLineTierOption(lineTierOptions);
                        }

                        var lineTierOption = new LineTierOption
                        {
                            StoreId = curStore.Id,
                            LineTierId = model.LineId ?? 0,
                            TerminalId = terminal.Id,
                            Order = 1,
                            CreatedOnUtc = DateTime.Now
                        };

                        _lineTierService.InsertLineTierOption(lineTierOption);

                        //活动日志
                        _userActivityService.InsertActivity("AddTerminal", "添加终端成功", curUser.Id);
                        _notificationService.SuccessNotification("添加终端成功");
                        return Successful("添加终端成功！");
                    }
                    else
                    {
                        //活动日志
                        _userActivityService.InsertActivity("AddTerminal", "添加终端失败", curUser.Id);
                        _notificationService.SuccessNotification("添加终端失败");
                        return Warning(errMsg);
                    }
                }
                else
                {
                    //活动日志
                    _userActivityService.InsertActivity("AddTerminal", "添加终端失败", curUser.Id);
                    _notificationService.SuccessNotification("添加终端失败");
                    return Warning("添加终端失败！");
                }
            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("AddTerminal", "添加终端失败", curUser.Id);
                _notificationService.SuccessNotification("添加终端失败");
                return Error(ex.Message);
            }

        }


        /// <summary>
        /// 上传门头照片
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult FileUpload(IFormFile file)
        {
            if (file != null)
            {
                var doorwayPhoto = _dCMSFileProvider.UploadFile(file);
                return Successful("上传成功", new { Url = LayoutExtensions.ResourceServerUrl(":9100/HRXHJS/document/image/" + doorwayPhoto) });
            }
            else
            {
                return Error("上传失败");
            }
        }

        /// <summary>
        /// 编辑终端
        /// </summary>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.EndPointListSave)]
        public JsonResult EditTerminal(int id)
        {

            var terminal = _terminalService.GetTerminalById(curStore.Id, id);
            if (terminal == null)
            {
                return Warning("数据不存在!");
            }
            //只能操作当前经销商数据
            else if (terminal.StoreId != curStore.Id)
            {
                return Warning("权限不足!");
            }

            var model = terminal.ToModel<TerminalModel>();


            model.DistrictList = new SelectList(_districtService.GetAll(curStore?.Id ?? 0).Select(d => { return new SelectListItem() { Text = d.Name, Value = d.Id.ToString() }; }), "Value", "Text");
            model.ChannelList = new SelectList(_channelService.GetAll(curStore?.Id ?? 0).Select(c => { return new SelectListItem() { Text = c.Name, Value = c.Id.ToString() }; }), "Value", "Text");
            model.RankList = new SelectList(_rankService.GetAll(curStore?.Id ?? 0).Select(r => { return new SelectListItem() { Text = r.Name, Value = r.Id.ToString() }; }), "Value", "Text");
            model.PaymentMethod = (model.PaymentMethod == 0 || model.PaymentMethod == 2) ? 2 : model.PaymentMethod;
            model.PaymentMethodType = new SelectList(from a in Enum.GetValues(typeof(PaymentMethodType)).Cast<PaymentMethodType>()
                                                     select new SelectListItem
                                                     {
                                                         Text = CommonHelper.GetEnumDescription(a),
                                                         Value = ((int)a).ToString(),
                                                         Selected = (int)a == 0
                                                     }, "Value", "Text");
            model.LineList = new SelectList(_lineTierService.GetAll(curStore?.Id ?? 0).Select(l => { return new SelectListItem() { Text = l.Name, Value = l.Id.ToString() }; }), "Value", "Text");
            return Json(new { Success = true, RenderHtml = RenderPartialViewToString("AddTerminal", model) });
        }

        [HttpPost]
        //[ValidateInput(false)]
        [AuthCode((int)AccessGranularityEnum.EndPointListSave)]
        public JsonResult EditTerminal(TerminalModel model)
        {
            try
            {
                if (model != null)
                {
                    //if (Regex.IsMatch(model.BossCall, @"^1(3|4|5|7|8)\d{9}$") ==false)
                    //{
                    //    return Warning("号码格式错误！");
                    //}
                    //if (string.IsNullOrWhiteSpace(model.BusinessNo))
                    //{
                    //    return Warning("营业编号不能为空！");
                    //}
                    //if (string.IsNullOrWhiteSpace(model.EnterpriseRegNo))
                    //{
                    //    return Warning("企业注册号不能为空！");
                    //}
                    //if (string.IsNullOrWhiteSpace(model.FoodBusinessLicenseNo))
                    //{
                    //    return Warning("食品经营许可证号不能为空！");
                    //}

                    var terminal = _terminalService.GetTerminalById(curStore.Id, model.Id);
                    terminal = model.ToEntity(terminal);
                    terminal.CreatedOnUtc = DateTime.Now;
                    terminal.StoreId = curStore?.Id ?? 0;
                    terminal.DoorwayPhoto = string.IsNullOrWhiteSpace(terminal.DoorwayPhoto) ? "" : terminal.DoorwayPhoto;
                    terminal.Remark = string.IsNullOrWhiteSpace(terminal.Remark) ? "" : terminal.Remark;
                    //编辑终端表
                    _terminalService.UpdateTerminal(terminal);

                    var lineTierOptions = _lineTierService.GetLineTierOptionOnlyOne(curStore.Id, model.Id);
                    if (lineTierOptions != null)
                    {
                        _lineTierService.DeleteLineTierOption(lineTierOptions);
                    }
                    var lineTierOption = new LineTierOption
                    {
                        StoreId = curStore.Id,
                        LineTierId = model.LineId ?? 0,
                        TerminalId = model.Id,
                        Order = 1,
                        CreatedOnUtc = DateTime.Now
                    };
                    _lineTierService.InsertLineTierOption(lineTierOption);

                    //活动日志
                    _userActivityService.InsertActivity("EditTerminal", "编辑终端成功", curUser.Id);
                    _notificationService.SuccessNotification("编辑终端成功");
                    return Successful("编辑终端成功！");
                }
                else
                {
                    //活动日志
                    _userActivityService.InsertActivity("EditTerminal", "编辑终端失败", curUser.Id);
                    _notificationService.SuccessNotification("编辑终端失败");
                    return Successful("编辑终端失败！");
                }
            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("EditTerminal", "编辑终端失败", curUser.Id);
                _notificationService.SuccessNotification("编辑终端失败");
                return Error(ex.Message);
            }

        }


        /// <summary>
        /// 批量禁用
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.EndPointListSave)]
        public JsonResult BatchDisable(int[] ids, bool type = false)
        {
            try
            {
                var terminals = _terminalService.GetTerminalsByIds(curStore.StoreId, ids, true).ToList();
                if (terminals != null && terminals.Any())
                {
                    foreach (var t in terminals)
                    {
                        t.Status = type;
                    }
                }
                _terminalService.UpdateTerminals(terminals);

                return Json(new { Successful = true });
            }
            catch (Exception ex)
            {
                return Json(new { Successful = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// 删除终端信息
        /// </summary>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.EndPointListSave)]
        public JsonResult DeleteTerminal(string ids)
        {
            try
            {
                if (!string.IsNullOrEmpty(ids))
                {
                    //验证终端是否生成单据
                    if (_terminalService.CheckRelated(int.Parse(ids)) == true)
                    {
                        var terminalmodule = new TerminalModel();
                        var terminal = _terminalService.GetTerminalById(curStore.StoreId, int.Parse(ids));
                        terminal = terminalmodule.ToEntity(terminal);
                        terminal.CreatedOnUtc = DateTime.Now;
                        terminal.Related = true;
                        terminal.StoreId = curStore.StoreId;

                        //编辑终端表
                        _terminalService.UpdateTerminal(terminal);
                        return Warning("该终端客户已开单不能删除！");
                    }
                    //var TerminalsId = _terminalService.GetTerminalsId(int.Parse(ids));
                    //if (TerminalsId.Count > 0)
                    //{
                    //    return this.Warning("该终端客户已开单不能删除！");
                    //}
                    else
                    {
                        int[] sids = ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(u => int.Parse(u)).ToArray();
                        var list = _terminalService.GetTerminalsByIds(curStore.Id, sids);

                        foreach (var terminal in list)
                        {
                            if (terminal != null)
                            {
                                terminal.Deleted = true;
                                _terminalService.UpdateTerminal(terminal);
                            }
                        }
                        //活动日志
                        _userActivityService.InsertActivity("DeleteTerminal", "删除终端成功", curUser.Id);
                        _notificationService.SuccessNotification("删除终端成功");
                        return Successful("删除终端成功！");
                    }

                }
                else
                {
                    //活动日志
                    _userActivityService.InsertActivity("DeleteTerminal", "删除终端失败", curUser.Id);
                    _notificationService.SuccessNotification("删除终端失败");
                    return Successful("删除终端失败！");
                }
            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("DeleteTerminal", "删除渠道失败", curUser.Id);
                _notificationService.SuccessNotification("删除渠道失败");
                return Error(ex.Message);
            }

        }

        /// <summary>
        /// 异步终端搜索
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<JsonResult> AsyncSearch(string key = "", string excludeIds = null, int pageIndex = 0, int pageSize = 10)
        {
            return await Task.Run(() =>
            {

                TerminalListModel model = new TerminalListModel();
                int[] ids = excludeIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(u => int.Parse(u)).ToArray();
                var list = _terminalService.GetTerminals(curStore.Id, ids, key, pageIndex, pageSize);
                model.PagingFilteringContext.LoadPagedList(list);

                model.Items = list.Select(s =>
                {
                    var m = s.ToModel<TerminalModel>();
                    return m;
                }).ToList();

                return Json(new
                {
                    total = model.Items.Count(),
                    rows = model.Items
                });
            });
        }

        [HttpGet]
        public async Task<JsonResult> CheckTerminalHasGives(int terminalId = 0, int businessUserId = 0)
        {
            return await Task.Run(() =>
            {
                try
                {
                    bool fg = false;
                    //促销活动
                    bool fg1 = _terminalService.CheckTerminalHasCampaignGives(curStore?.Id ?? 0, terminalId);
                    //费用合同
                    bool fg2 = _terminalService.CheckTerminalHasCostContractGives(curStore?.Id ?? 0, terminalId, businessUserId);
                    if (fg1 || fg2)
                    {
                        fg = true;
                    }
                    return Json(new { HasGives = fg });
                }
                catch (Exception)
                {
                    return Json(new { HasGives = false });
                }
            });
        }

        /// <summary>
        /// 获取终端账户余额 
        /// </summary>
        /// <param name="terminalId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<JsonResult> GetTerminalBalance(int? storeId, int terminalId = 0)
        {
            if (!storeId.HasValue)
            {
                storeId = curStore?.Id ?? 0;
            }

            return await Task.Run(() =>
            {
                try
                {
                    var terminalBalance = _commonBillService.CalcTerminalBalance(storeId ?? 0, terminalId);
                    return Json(terminalBalance);
                }
                catch (Exception)
                {
                    return Json(new TerminalBalance());
                }
            });
        }

        /// <summary>
        /// 终端客户选择弹出
        /// </summary>
        /// <param name="key"></param>
        /// <param name="pageIndex"></param>
        /// <param name="index"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult AsyncSearchSelectPopup(string key, int pageIndex = 0, int index = 0, string target = "")
        {
            return Json(new
            {
                Success = true,
                RenderHtml = RenderPartialViewToString("AsyncSearch", new TerminalListModel
                {
                    Key = key,
                    RowIndex = index,
                    Target = target
                })
            });
        }

        /// <summary>
        /// 终端客户列表
        /// </summary>
        /// <param name="key"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<JsonResult> AsyncList(string key, int pageIndex = 0, int pageSize = 10, bool? status = null)
        {
            return await Task.Run(() =>
            {
                if (!string.IsNullOrEmpty(key))
                {
                    key = key.Trim();
                }
                var model = new TerminalListModel();
                var lists = new List<TerminalModel>();

                var companySetting = _settingService.LoadSetting<CompanySetting>(curStore.Id);//获取配置信息
                var terminals = _terminalService.GetPopupTerminals(curStore?.Id ?? 0, key, status, curUser, companySetting.SalesmanOnlySeeHisCustomer, pageIndex, pageSize);

                lists.AddRange(terminals.Select(t =>
                {
                    return new TerminalModel()
                    {
                        Id = t.Id,
                        Name = t.Name,
                        BossName = t.BossName,
                        BossCall = t.BossCall,
                        ChannelId = t.ChannelId,
                        Address = t.Address,
                        MaxAmountOwed = t.MaxAmountOwed
                    };
                }).ToList());


                return Json(new
                {
                    Success = true,
                    total = terminals.TotalCount,
                    rows = lists.ToList()
                });
            });
        }

        #endregion

        #region 渠道
        /// <summary>
        /// 异步获取渠道列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<JsonResult> AsyncBatchAssignChannel()
        {
            return await Task.Run(() =>
            {
                var model = new ChannelListModel
                {
                    Items = new SelectList(_channelService.GetAll(curStore?.Id ?? 0).Select(c => { return new SelectListItem() { Text = c.Name, Value = c.Id.ToString() }; }), "Value", "Text")
                };
                return Json(new { RenderHtml = RenderPartialViewToString("BatchAssignChannel", model), Success = true });
            });
        }
        [HttpPost]
        [AuthCode((int)AccessGranularityEnum.EndPointListSave)]
        public JsonResult BatchAssignChannel(int[] tids, int cid, bool isAll)
        {
            try
            {
                if (isAll == false && tids.Length == 0)
                {
                    return Warning("请选择终端！");
                }
                if (cid == 0)
                {
                    return Warning("请选择渠道！");
                }
                var terminals = new List<Terminal>();
                if (isAll)
                {
                    terminals = _terminalService.GetAllTerminal(curStore.Id).ToList();
                }
                else
                {
                    terminals = _terminalService.GetTerminalsByIds(curStore.Id, tids).ToList();
                }
                terminals.ForEach(t => t.ChannelId = cid);
                _terminalService.UpdateTerminals(terminals);
                //活动日志
                _userActivityService.InsertActivity("EditTerminal", "批量编辑终端成功", curUser.Id);
                _notificationService.SuccessNotification("批量编辑终端成功");
                return Successful("批量编辑终端成功！");
            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("EditTerminal", "批量编辑终端失败", curUser.Id);
                _notificationService.SuccessNotification("批量编辑终端失败");
                return Error(ex.Message);
            }
        }

        #endregion

        #region 片区
        /// <summary>
        /// 异步获取片区ZTree
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<JsonResult> AsyncDistricts()
        {
            return await Task.Run(() =>
            {

                IList<ZTree> nodes = _districtService.GetListZTreeVM(curStore?.Id ?? 0);
                return Json(new { data = nodes, Success = true });
                //return Json(nodes);
            });
        }

        /// <summary>
        /// 异步会计科目弹出选择
        /// </summary>
        public async Task<JsonResult> AsyncDistrictsSelectPopup(string style = "", string mode = "all")
        {
            return await Task.Run(() =>
            {
                return Json(new { Success = true, RenderHtml = RenderPartialViewToString("AsyncSearchDistrict") });
            });
        }

        /// <summary>
        /// 新增片区信息
        /// </summary>
        /// <returns></returns>
        public JsonResult AddDistrict(int districtId)
        {
            var districts = BindDistrictSelection(_districtService.BindDistrictList, curStore);
            if (districts.Count() == 0)
            {
                districts = new SelectList(new List<SelectListItem>() {
                    new SelectListItem() { Text = "根", Value = "0", Selected = true } }, "Value", "Text");
            }

            return Json(new
            {
                Success = true,
                RenderHtml = RenderPartialViewToString("AddDistrict", new DistrictModel()
                {
                    // //
                    ParentId = districtId,
                    DistrictList = districts
                    //DistrictList =new SelectList(_districtService.GetAll(curStore?.Id??0).Select(d => { return new SelectListItem() { Text = d.Name, Value = d.Id.ToString(), Selected = true }; })),
                })
            });
        }

        [HttpPost]
        public JsonResult AddDistrict(DistrictModel model)
        {
            try
            {
                if (model != null)
                {

                    //如果父节点为0
                    //if (model.ParentId == 0)
                    //{
                    //    //如果片区已经存在父节点
                    //    if (_districtService.CheckDistrictRoot(curStore.Id))
                    //    {
                    //        return Warning("片区只能存在一个根节点");
                    //    }
                    //}

                    //经销商
                    //
                    model.StoreId = curStore?.Id ?? 0;
                    model.Deleted = false;
                    if (_districtService.GetDistrictByName(curStore?.Id ?? 0, model.Name) > 0)
                    {
                        //活动日志
                        _userActivityService.InsertActivity("AddDistrict", "片区名称已使用", curUser.Id);
                        _notificationService.SuccessNotification("片区名称已使用");
                        return Error("片区名称已使用！");
                    }
                    else
                    {
                        //添加片区表
                        _districtService.InsertDistrict(model.ToEntity<District>());
                        //活动日志
                        _userActivityService.InsertActivity("AddDistrict", "添加片区成功", curUser.Id);
                        _notificationService.SuccessNotification("添加片区成功");
                        return Successful("添加片区成功！");
                    }

                }
                else
                {
                    //活动日志
                    _userActivityService.InsertActivity("AddDistrict", "添加片区失败", curUser.Id);
                    _notificationService.SuccessNotification("添加片区失败");
                    return Successful("添加片区失败！");
                }
            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("AddDistrict", "添加片区失败", curUser.Id);
                _notificationService.SuccessNotification("添加片区失败");
                return Error(ex.Message);
            }

        }

        /// <summary>
        /// 编辑片区信息
        /// </summary>
        /// <returns></returns>
        public JsonResult EditDistrict(int districtId)
        {

            var model = _districtService.GetDistrictById(curStore.Id, districtId).ToModel<DistrictModel>();

            model.DistrictList = BindDistrictSelection(_districtService.BindDistrictList, curStore);

            return Json(new { Success = true, RenderHtml = RenderPartialViewToString("AddDistrict", model) });
        }

        [HttpPost]
        public JsonResult EditDistrict(DistrictModel model)
        {
            try
            {
                if (model != null)
                {
                    var district = _districtService.GetDistrictById(curStore.Id, model.Id);
                    if (district != null)
                    {

                        //如果当前节点是跟节点
                        //if (district.ParentId == 0)
                        //{
                        //    if (model.ParentId != district.Id)
                        //    {
                        //        return Warning("片区根节点不能修改父节点");
                        //    }
                        //}

                        if (model.Id == model.ParentId)
                        {
                            return Warning("片区节点的父节点不能为当前节点");
                        }

                        district = model.ToEntity(district);
                        //编辑片区信息
                        _districtService.UpdateDistrict(district);
                        //活动日志
                        _userActivityService.InsertActivity("EditDistrict", "编辑片区成功", curUser.Id);
                        _notificationService.SuccessNotification("编辑片区成功");
                        return Successful("编辑片区成功！", model);
                    }
                    else
                    {
                        //活动日志
                        _userActivityService.InsertActivity("EditDistrict", "编辑片区失败", curUser.Id);
                        _notificationService.SuccessNotification("编辑片区失败");
                        return Successful("编辑片区失败！", model);
                    }
                }
                else
                {
                    //活动日志
                    _userActivityService.InsertActivity("EditDistrict", "编辑片区失败", curUser.Id);
                    _notificationService.SuccessNotification("编辑片区失败");
                    return Successful("编辑片区失败！", model);
                }
            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("EditDistrict", "编辑片区失败", curUser.Id);
                _notificationService.SuccessNotification("编辑片区失败");
                return Error(ex.Message);
            }

        }


        /// <summary>
        /// 判断片区是否关联终端
        /// </summary>
        /// <param name="store">经销商</param>
        /// <param name="Id"></param>
        /// <returns></returns>
        public JsonResult CheckDisTerminal(int districtId)
        {
            var include = _terminalService.GetDisTerminalIds(curStore.Id, districtId).Count() > 0;
            return Json(new
            {
                Success = true,
                IncludeTerminal = include
            });
        }

        /// <summary>
        /// 删除片区信息
        /// </summary>
        /// <returns></returns>
        public JsonResult DeleteDistrict(int? districtId)
        {
            try
            {
                if (districtId.HasValue)
                {
                    //保存事务
                    //using (var scope = new TransactionScope())
                    //{
                    //取消该片区下所有终端
                    var terminals = _terminalService.GetTerminalsByDistrictId(curStore?.Id ?? 0, districtId.Value);
                    var terminalIds = _districtService.GetSonDistrictIds(curStore.Id, districtId ?? 0);
                    if (terminalIds.Count > 0)
                    {
                        return Warning("该片区有子片区，不能删除！");
                    }
                    if (terminals.Count > 0)
                    {
                        return Warning("该片区已分配终端用户！");
                    }
                    District district = _districtService.GetDistrictById(curStore.Id, districtId.Value);
                    //删除当前选中片区
                    district.Deleted = true;
                    _districtService.UpdateDistrict(district);
                    //scope.Complete();
                    //}
                    //活动日志
                    _userActivityService.InsertActivity("DeleteDistrict", "删除片区成功", curUser.Id);
                    _notificationService.SuccessNotification("删除片区成功");
                    return Successful("删除片区成功！");
                    //terminals.ToList().ForEach(t =>
                    //{
                    //    t.DistrictId = 0;
                    //    _terminalService.UpdateTerminal(t);
                    //});
                    ////删除子孙片区
                    //List<District> childDistrict = new List<District>();
                    //childDistrict.AddRange(_districtService.GetDistrictByParentId(curStore?.Id??0, districtId.Value));
                    //if (childDistrict != null && childDistrict.Count > 0)
                    //{
                    //    childDistrict.ForEach(d =>
                    //    {
                    //        d.Deleted = true;
                    //        _districtService.UpdateDistrict(d);
                    //    });
                    //} 
                }
                else
                {
                    //活动日志
                    _userActivityService.InsertActivity("DeleteDistrict", "删除片区失败", curUser.Id);
                    _notificationService.SuccessNotification("删除片区失败");
                    return Successful("删除片区失败！");
                }
            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("DeleteDistrict", "删除片区失败", curUser.Id);
                _notificationService.SuccessNotification("删除片区失败");
                return Error(ex.Message);
            }

        }


        /// <summary>
        /// 获取片区树
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<JsonResult> GetDistrictFancyTree()
        {

            return await Task.Run(() =>
            {
                var trees = GetDistrictList(curStore?.Id ?? 0, 0);
                return Json(trees);
            });
        }

        /// <summary>
        /// 递归获取类别树
        /// </summary>
        /// <param name="store">经销商</param>
        /// <param name="Id"></param>
        /// <returns></returns>
        [NonAction]
        public List<FancyTree> GetDistrictList(int? store, int Id)
        {
            List<FancyTree> fancyTrees = new List<FancyTree>();
            var perentList = _districtService.GetDistrictByParentId(store.Value, Id);
            if (perentList != null && perentList.Count > 0)
            {
                foreach (var b in perentList)
                {
                    List<FancyTree> tempList = GetDistrictList(store.Value, b.Id);
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

        /// <summary>
        /// 批量匹配片区
        /// </summary>
        /// <param name="districtId"></param>
        /// <returns></returns>
        public JsonResult BatchAssignDistrict()
        {
            return Json(new
            {
                Success = true,
                RenderHtml = RenderPartialViewToString("BatchAssignDistrict")
            });
        }
        [HttpPost]
        [AuthCode((int)AccessGranularityEnum.EndPointListSave)]
        public JsonResult BatchAssignDistrict(int[] tids, int did, bool isAll)
        {
            try
            {
                if (isAll == false && tids.Length == 0)
                {
                    return Warning("请选择终端！");
                }
                if (did == 0)
                {
                    return Warning("请选择片区！");
                }
                var terminals = new List<Terminal>();
                if (isAll)
                {
                    terminals = _terminalService.GetAllTerminal(curStore.Id).ToList();
                }
                else
                {
                    terminals = _terminalService.GetTerminalsByIds(curStore.Id, tids).ToList();
                }
                terminals.ForEach(t => t.DistrictId = did);
                _terminalService.UpdateTerminals(terminals);
                //活动日志
                _userActivityService.InsertActivity("EditTerminal", "批量编辑终端成功", curUser.Id);
                _notificationService.SuccessNotification("批量编辑终端成功");
                return Successful("批量编辑终端成功！");
            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("EditTerminal", "批量编辑终端失败", curUser.Id);
                _notificationService.SuccessNotification("批量编辑终端失败");
                return Error(ex.Message);
            }
        }


        #endregion

        #region 导入数据
        [HttpPost]
        public IActionResult Import(IFormCollection form)
        {
            try
            {
                var file = form.Files["file"];
                var distId = form["hidDistrictId"];
                var fileName = file.FileName.Trim('"');//获取文件名
                string fileExt = Path.GetExtension(fileName).ToLower(); //获取文件扩展名
                if (fileExt != ".xls" && fileExt != ".xlsx")
                {
                    return Warning("请选择Excel文件");
                }
                //string sFilePath = AppDomain.CurrentDomain.BaseDirectory.Replace("\\", "/");

                string filepath = AppContext.BaseDirectory + $"\\App_Data\\TempUploads\\{curStore.Id}";
                //if (!Directory.Exists(filepath))
                //{
                //    Directory.CreateDirectory(filepath);
                //}
                ///string filepath = sFilePath.Substring(0, sFilePath.Length - sFilePath.IndexOf("bin")) + "App_Data/TempUploads/";
                if (!Directory.Exists(filepath))
                {
                    Directory.CreateDirectory(filepath);
                }

                fileName = filepath + $@"\{fileName}";//指定文件上传的路径
                List<TerminalModel> models = null;
                using (FileStream fs = new FileStream(fileName, FileMode.Create))//创建文件流
                {
                    file.CopyTo(fs);
                    //int.TryParse(distId, out int a);
                    //models = ImportDataHandler(fs, fileName, curStore.Id, curUser.Id, a);
                    fs.Flush();
                }
                //获取表格数据
                var dtTable = ExcelHelper.ExcelToDataTable(fileName);
                if (dtTable == null || dtTable.Rows.Count == 0)
                    return Warning("没有可以导入的数据");
                //将DataTable转为List
                models = ExcelHelper.TableToList<TerminalModel>(dtTable);
                int.TryParse(distId, out int districtId);  //片区
                if (models == null)
                {
                    return Warning("上传文件格式不正确");
                }

                //遍历数据根据名称获取ID
                models.ForEach(t =>
                {
                    t.StoreId = curStore.Id;
                    t.IsNewAdd = false;

                    if (!string.IsNullOrEmpty(t.DistrictName))
                    {
                        var disId = _districtService.GetDistrictByName(curStore.Id, t.DistrictName); //获取片区ID
                        t.DistrictId = disId == 0 ? districtId : disId;
                    }
                    else
                        t.DistrictId = districtId;
                    if (!string.IsNullOrEmpty(t.ChannelName))
                        t.ChannelId = _channelService.GetChannelByName(curStore.Id, t.ChannelName);//获取渠道ID
                    if (!string.IsNullOrEmpty(t.LineName))
                        t.LineId = _lineTierService.GetLineTierByName(curStore.Id, t.LineName); //获取线路ID
                    if (!string.IsNullOrEmpty(t.RankName))
                        t.RankId = _rankService.GetRankByName(curStore.Id, t.RankName); //获取等级ID
                });
                var ents = models.Select(m => m.ToEntity<Terminal>()).ToList();
                //插入、更新关系，插入终端
                _importManager.ImportTerminals(ents, curStore.Code, curStore.Name);
                //更新关系
                //_importManager.ImportTerminals(ents, curStore.Code, curStore.Name);
                return Successful("终端导入成功");
                //return RedirectToAction("List");
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }

        }

        public FileResult UploadDown()
        {
            try
            {
                byte[] ms = null;
                var terminals = _terminalService.GetAllTerminal(2).Take(1);
                ms = _exportManager.ExportTerminalsToXlsx(terminals);
                if (ms != null)
                {
                    return File(ms, "application/vnd.ms-excel", "终端档案.xlsx");
                }
                else
                {
                    return File(new MemoryStream(), "application/vnd.ms-excel", "终端档案.xlsx");
                }
            }
            catch (Exception)
            {
                return null;
            }
        }


        private List<TerminalModel> ImportDataHandler(Stream stream, string fileName, int store, int userId, int distrctid)
        {
            static string GetCellValue(ISheet excelWorksheet, int rowNum, int colNum)
            {
                var row = excelWorksheet.GetRow(rowNum);
                if (row == null) return null;
                var cell = row.GetCell(colNum);
                if (cell == null) return null;
                return cell.StringCellValue;
            }

            static bool CheckExcelTemplate(ISheet excelWorksheet, int colNum, string firstColName)
            {
                var cell = excelWorksheet.GetRow(0).Cells.Count();
                if (cell != colNum)
                {
                    return false;
                }

                if (GetCellValue(excelWorksheet, 0, 0) != firstColName)
                {
                    return false;
                }

                return true;
            }

            List<TerminalModel> terminals = new List<TerminalModel>();

            string fileExt = Path.GetExtension(fileName).ToLower();
            IWorkbook workbook;

            ////////////////////////////2021-10-09 mu 修改/////////////////////////////
            //if (fileExt == ".xlsx")
            //{
            //    stream.Position = 0;
            //    workbook = new XSSFWorkbook(stream);
            //}
            //else if (fileExt == ".xls")
            //{
            //    workbook = new HSSFWorkbook(stream);
            //}
            //else
            //{
            //    workbook = null;
            //}

            //if (workbook == null) 
            //{ 
            //    return terminals;
            //}
            try
            {
                stream.Position = 0;
                workbook = new XSSFWorkbook(stream);
            }
            catch (Exception)
            {
                workbook = new HSSFWorkbook(stream);
            }
            /////////////////////////////////////////////////////////

            ISheet worksheet = workbook.GetSheetAt(0);
            if (worksheet == null)
            {
                throw new DCMSException("No worksheet found");
            }

            if (CheckExcelTemplate(worksheet, 19, "名称"))
            {
                int rowCount = worksheet.PhysicalNumberOfRows; //行数
                int ColCount = worksheet.GetRow(0).Cells.Count(); //列数
                if (ColCount < 19) { }
                for (int row = 1; row < rowCount; row++)
                {
                    var currentRow = worksheet.GetRow(row);
                    var terminal = new TerminalModel
                    {
                        StoreId = store
                    };
                    for (int col = 0; col <= ColCount; col++)
                    {
                        switch (col)
                        {
                            case 0:
                            terminal.Name = currentRow.GetCell(col)?.ToString().Trim();
                            break;
                            case 1:
                            terminal.MnemonicName = (currentRow.GetCell(col) == null) ? "NULL" : currentRow.GetCell(col)?.ToString().Trim();
                            break;
                            case 2:
                            terminal.BossName = (currentRow.GetCell(col) == null) ? "" : currentRow.GetCell(col)?.ToString().Trim();
                            break;
                            case 3:
                            terminal.BossCall = (currentRow.GetCell(col) == null) ? "" : currentRow.GetCell(col)?.ToString().Trim();
                            break;
                            case 4:
                            terminal.Status = true;
                            break;
                            case 5:
                            int.TryParse(currentRow.GetCell(col)?.ToString().Trim(), out int a);
                            terminal.MaxAmountOwed = a;
                            break;
                            case 6:
                            terminal.Code = (currentRow.GetCell(col) == null) ? "NULL" : currentRow.GetCell(col)?.ToString().Trim();
                            break;
                            case 7:
                            terminal.Address = (currentRow.GetCell(col) == null) ? "" : currentRow.GetCell(col)?.ToString().Trim();
                            break;
                            case 8:
                            terminal.Remark = (currentRow.GetCell(col) == null) ? "NULL" : currentRow.GetCell(col)?.ToString().Trim();
                            break;
                            case 9:
                            terminal.DistrictId = _districtService.GetDistrictByName(store, currentRow.GetCell(col)?.ToString().Trim()) == 0 ? distrctid : _districtService.GetDistrictByName(store, currentRow.GetCell(col)?.ToString().Trim());
                            break;
                            case 10:
                            terminal.ChannelId = _channelService.GetChannelByName(store, currentRow.GetCell(col)?.ToString().Trim());
                            break;
                            case 11:
                            terminal.LineId = _lineTierService.GetLineTierByName(store, currentRow.GetCell(col)?.ToString().Trim());
                            break;
                            case 12:
                            terminal.RankId = _rankService.GetRankByName(store, currentRow.GetCell(col)?.ToString().Trim());
                            break;
                            case 13:
                            terminal.PaymentMethod = currentRow.GetCell(col)?.ToString().Trim() == "1" ? 1 : 2;
                            break;
                            case 14:
                            double.TryParse(currentRow.GetCell(col)?.ToString().Trim(), out double lng);
                            terminal.Location_Lng = lng;
                            break;
                            case 15:
                            double.TryParse(currentRow.GetCell(col)?.ToString().Trim(), out double lat);
                            terminal.Location_Lat = lat;
                            break;
                            case 16:
                            terminal.BusinessNo = currentRow.GetCell(col)?.ToString().Trim();
                            break;
                            case 17:
                            terminal.FoodBusinessLicenseNo = currentRow.GetCell(col)?.ToString().Trim();
                            break;
                            case 18:
                            terminal.EnterpriseRegNo = currentRow.GetCell(col)?.ToString().Trim();
                            break;
                        }
                    }

                    terminal.CreatedUserId = userId;
                    terminal.CreatedOnUtc = DateTime.Now;
                    terminal.Deleted = false;
                    terminal.Related = false;


                    if (!terminals.Select(s => s.Code).Contains(terminal.Code))
                    {
                        terminals.Add(terminal);
                    }
                }
            }
            else
            {
                return null;
            }
            return terminals;
        }
        #endregion

    }
}