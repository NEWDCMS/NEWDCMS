using DCMS.Core;
using DCMS.Core.Domain.Census;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Visit;
using DCMS.Services.Census;
using DCMS.Services.Configuration;
using DCMS.Services.ExportImport;
using DCMS.Services.Global.Common;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Sales;
using DCMS.Services.Terminals;
using DCMS.Services.Users;
using DCMS.Services.Visit;
using DCMS.ViewModel.Models.Report;
using DCMS.Web.Framework.Mvc.Filters;
using DCMS.Web.Infrastructure.Mapper.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using DCMS.Web.Framework.UI.Paging;
using DCMS.Core.Infrastructure;

namespace DCMS.Web.Controllers.Report
{
    /// <summary>
    /// 业务员拜访
    /// </summary>
    public class VisitStoreController : BasePublicController
    {
        private readonly IUserService _userService;
        private readonly ISettingService _settingService;
        private readonly IDistrictService _districtService;
        private readonly ITerminalService _terminalService;
        private readonly IChannelService _channelService;
        private readonly IVisitStoreService _visitStoreService;
        private readonly ILineTierService _lineTierService;
        private readonly IExportService _exportService;
        private readonly ISaleBillService _saleBillService;
        private readonly ISaleReservationBillService _saleReservationBillService;
        private readonly IReturnBillService _returnBillService;
        private readonly IReturnReservationBillService _returnReservationBillService;
        private readonly IExportManager _exportManager;
        private readonly IWebHelper _webHelper;

        private string resourceServer = EngineContext.GetStaticResourceServer; //获取静态资源服务器

        public VisitStoreController(
            INotificationService notificationService,
            IStoreContext storeContext,
            IUserService userService,
            ISettingService settingService,
            IDistrictService districtService,
            ITerminalService terminalService,
            IChannelService channelService,
            IVisitStoreService visitStoreService,
            ILineTierService lineTierService,
            IExportService exportService,
            ISaleBillService saleBillService,
            ISaleReservationBillService saleReservationBillService,
            IReturnBillService returnBillService,
            IReturnReservationBillService returnReservationBillService,
            ILogger loggerService,
            IExportManager exportManager,
            IWorkContext workContext,
            IWebHelper webHelper
            ) : base(workContext, loggerService, storeContext, notificationService)
        {
            _userService = userService;
            _settingService = settingService;
            _districtService = districtService;
            _terminalService = terminalService;
            _channelService = channelService;
            _visitStoreService = visitStoreService;
            _lineTierService = lineTierService;
            _exportService = exportService;
            _saleBillService = saleBillService;
            _returnBillService = returnBillService;
            _saleReservationBillService = saleReservationBillService;
            _returnReservationBillService = returnReservationBillService;
            _exportManager = exportManager;
            _webHelper = webHelper;
        }

        #region 业务员拜访记录

        /// <summary>
        /// 业务员拜访记录
        /// </summary>
        /// <param name="store"></param>
        /// <param name="businessUserId"></param>
        /// <param name="terminalId"></param>
        /// <param name="terminalName"></param>
        /// <param name="districtId"></param>
        /// <param name="channelId"></param>
        /// <param name="visitTypeId"></param>
        /// <param name="signinDateTime"></param>
        /// <param name="signOutDateTime"></param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.VisitingRecordsView)]
        public IActionResult BusinessUserVisitingRecord(int? store, int? businessUserId, int? terminalId, string terminalName, int? districtId, int? channelId, int? visitTypeId, DateTime? signinDateTime = null, DateTime? signOutDateTime = null, int pagenumber = 0)
        {
            var models = new VisitStoreListModel
            {
                //业务员
                BusinessUsers = BindUserSelection(new Func<int, string,int, bool, bool,List<Core.Domain.Users.User>>(_userService.BindUserList), curStore, DCMSDefaults.Salesmans,curUser.Id,true, _userService.IsAdmin(curStore.Id, curUser.Id))
            };
            models.BusinessUserId = businessUserId ?? null;

            //客户渠道
            models.Channels = BindChanneSelection(_channelService.BindChannelList, curStore);
            models.ChannelId = channelId ?? null;

            //客户片区
            models.Districts = BindDistrictSelection(_districtService.BindDistrictList, curStore);
            models.DistrictId = districtId ?? null;

            //状态
            models.VisitTypes = new SelectList(from a in Enum.GetValues(typeof(VisitTypeEnum)).Cast<VisitTypeEnum>()
                                               select new SelectListItem
                                               {
                                                   Text = CommonHelper.GetEnumDescription(a),
                                                   Value = ((int)a).ToString()
                                               }, "Value", "Text");
            models.VisitTypeId = null;

            models.TerminalName = terminalName;

            models.SigninDateTime = signinDateTime ?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01"));
            models.SignOutDateTime = signOutDateTime ?? DateTime.Now.AddDays(1);

            if (pagenumber > 0) pagenumber -= 1;

            //分页获取
            var lists = _visitStoreService.GetAllVisitRecords(
                curStore?.Id ?? 0,
                businessUserId ?? 0,
                terminalId ?? 0,
                terminalName,
                districtId ?? 0,
                channelId ?? 0,
                visitTypeId ?? 0,
                models.SigninDateTime,
                models.SignOutDateTime,
                pageIndex: pagenumber,
                pageSize: 30);

            models.PagingFilteringContext.LoadPagedList(lists);

            var items = lists.Select(m =>
            {
                var model = m.ToModel<VisitStoreModel>();

                if (model.VisitTypeId == 0)
                {
                    model.VisitTypeName = "";
                }
                else
                {
                    //状态
                    model.VisitTypeName = CommonHelper.GetEnumDescription((VisitTypeEnum)Enum.Parse(typeof(VisitTypeEnum), m.VisitTypeId.ToString()));
                }


                //未拜访天数
                var lastRecord = _visitStoreService.GetLastRecord(store != null ? store : 0, m.Id, 0, 0);
                //model.NoVisitedDays = lastRecord != null && lastRecord.SignOutDateTime != null ? m.SigninDateTime.Subtract(lastRecord.SignOutDateTime.Value).Days : 0;
                model.NoVisitedDays = (lastRecord != null && lastRecord.SignOutDateTime != null) ? m.SigninDateTime.Subtract(lastRecord.SignOutDateTime.Value).Days : DateTime.Now.Subtract(m.SigninDateTime).Days;

                //达成金额计算
                //if (m.SaleBillId != null && m.SaleBillId != 0)
                //{
                //    model.SaleAmount = _saleBillService.GetSaleBillById(store, m.SaleBillId.Value).SumAmount;
                //}

                //if (m.SaleReservationBillId != null && m.SaleReservationBillId != 0)
                //{
                //    model.SaleOrderAmount = _saleReservationBillService.GetSaleReservationBillById(curStore.Id, m.SaleReservationBillId.Value).SumAmount;
                //}

                //if (m.ReturnBillId != null && m.ReturnBillId != 0)
                //{
                //    model.ReturnAmount = _returnBillService.GetReturnBillById(curStore.Id, m.ReturnBillId.Value).SumAmount;
                //}

                //if (m.ReturnReservationBillId != null && m.ReturnReservationBillId != 0)
                //{
                //    model.ReturnOrderAmount = _returnReservationBillService.GetReturnReservationBillById(curStore.Id, m.ReturnReservationBillId.Value).SumAmount;
                //}

                //判断是否下单
                if ((m.SaleBillId != null && m.SaleBillId != 0) 
                || (m.SaleReservationBillId != null && m.SaleReservationBillId != 0) 
                || (m.ReturnBillId != null && m.ReturnBillId != 0)
                || (m.ReturnReservationBillId != null && m.ReturnReservationBillId != 0)) 
                {
                    model.IsCreateOrder = true;
                }
                //路线
                model.LineName = _lineTierService.GetLineTierById(curStore.Id, m.LineId == null ? 0 : m.LineId.Value)?.Name;
                //修改图片路径
                var staticResourceUrl = $"{resourceServer}/HRXHJS/document/image/"; //静态资源路径
                //var domainUrl = _webHelper.GetStoreLocation();
                //var photoUrl = $"{domainUrl}visitstore/showimage?url={staticResourceUrl}";

                model.DoorheadPhotos = _visitStoreService.GetDoorheadPhotoByVisitId(model.Id)?
                .Select(x => new DoorheadPhoto
                {
                    StoragePath = x.StoragePath,
                    RestaurantId = 0,
                    TraditionId = 0,
                    VisitStoreId = x.VisitStoreId
                }).ToList();
                model.DoorheadPhotos.ForEach(p => {
                    var imageId = "";
                    if (p.StoragePath.StartsWith("https://") || p.StoragePath.StartsWith("http://"))
                    {
                        var displayPath = p.StoragePath.TrimEnd('/');
                        if (displayPath.Contains("/"))
                            imageId = displayPath.Substring(displayPath.LastIndexOf("/") + 1);
                        else
                            imageId = displayPath;
                    }
                    else
                        imageId = p.StoragePath;
                    if (imageId.ToLower() == "image")
                        p.StoragePath = $"{resourceServer}/vant/empty-image-error.png";
                    else
                        p.StoragePath = $"{staticResourceUrl}{imageId}";
                });
                model.DisplayPhotos = _visitStoreService.GetDisplayPhotoByVisitId(model.Id)?
                .Select(x => new DisplayPhoto
                {
                    DisplayPath = x.DisplayPath,
                    RestaurantId = 0,
                    TraditionId = 0,
                    VisitStoreId = x.VisitStoreId
                }).ToList();
                
                model.DisplayPhotos.ForEach(p=> {
                    var imageId = "";
                    if (p.DisplayPath.StartsWith("https://") || p.DisplayPath.StartsWith("http://")) 
                    {
                        var displayPath = p.DisplayPath.TrimEnd('/');
                        if (displayPath.Contains("/"))
                            imageId = displayPath.Substring(displayPath.LastIndexOf("/") + 1);
                        else 
                            imageId = displayPath;
                    }
                    else
                        imageId = p.DisplayPath;
                    if (imageId.ToLower() == "image") 
                        p.DisplayPath = $"{resourceServer}/vant/empty-image-error.png";
                    else
                        p.DisplayPath = $"{staticResourceUrl}{imageId}";
                });
                return m == null ? new VisitStoreModel() : model;
            }).ToList();

            models.Items = items;

            return View(models);
        }

        /// <summary>
        /// 业务员拜访记录导出
        /// </summary>
        /// <param name="businessUserId"></param>
        /// <param name="terminalId"></param>
        /// <param name="terminalName"></param>
        /// <param name="districtId"></param>
        /// <param name="channelId"></param>
        /// <param name="visitTypeId"></param>
        /// <param name="signinDateTime"></param>
        /// <param name="signOutDateTime"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.VisitingRecordsExport)]
        public FileResult ExportBusinessUserVisitingRecord(int? businessUserId, int? terminalId, string terminalName, int? districtId, int? channelId, int? visitTypeId, DateTime? signinDateTime = null, DateTime? signOutDateTime = null)
        {

            #region 查询导出数据

            var sqlDatas = _visitStoreService.GetVisitRecords(
                curStore?.Id ?? 0,
                businessUserId ?? 0,
                terminalId ?? 0,
                terminalName,
                districtId ?? 0,
                channelId ?? 0,
                visitTypeId ?? 0,
                (signinDateTime == null) ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")) : signinDateTime,
                (signOutDateTime == null) ? DateTime.Now.AddDays(1) : signOutDateTime).ToList();

            #endregion

            #region 导出
            var ms = _exportManager.ExportBusinessUserVisitingRecordToXlsx(sqlDatas);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "业务员拜访记录.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "业务员拜访记录.xlsx");
            }
            #endregion

        }

        #endregion

        #region 业务员拜访达成表

        /// <summary>
        /// 业务员拜访达成表
        /// </summary>
        /// <param name="store"></param>
        /// <param name="businessUserId"></param>
        /// <param name="lineId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.VisitingScheduleView)]
        public IActionResult BusinessUserVisitReached(int? store, int? businessUserId, int? lineId, DateTime? startTime = null, DateTime? endTime = null, int pagenumber = 0)
        {

            var models = new VisitReachedListModel();
            var tempdatas = new List<VisitReachedModel>();

            try
            {
                //业务员
                models.BusinessUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Salesmans, curUser.Id, true, _userService.IsAdmin(curStore.Id, curUser.Id));
                models.BusinessUserId = businessUserId ?? null;

                //线路
                var lst_line = _lineTierService.BindLineTier(curStore.Id);
                //判断是否启用业务员线路/是否超级管理员
                var companySetting = _settingService.LoadSetting<CompanySetting>(curStore.Id);
                if (companySetting.EnableBusinessVisitLine && !_userService.IsAdmin(store ?? 0, businessUserId ?? 0)) //启用业务员线路并且不是超级管理员
                {
                    //获取用户指定线路
                    var lst = _lineTierService.GetUserLineTier(curStore.Id, businessUserId ?? 0);
                    lst_line = lst_line.Where(w => lst.Contains(w.Id)).ToList();
                }
                //models.Lines = BindLineSelection(new Func<int?, IList<LineTier>>(_lineTierService.BindLineTier), curStore);
                if (lst_line?.Count > 0)
                {
                    models.Lines = BindLineSelection(lst_line, lst_line.FirstOrDefault().Id);
                }
                else
                {
                    models.Lines = new SelectList(lst_line);
                }
                models.LineId = lineId ?? null;

                models.StartTime = startTime ?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01"));
                models.EndTime = endTime ?? DateTime.Now.AddDays(1);

                var lineIds = lst_line.Select(s => s.Id).ToList();
                if (lineId.HasValue && lineId.Value > 0)
                {
                    lineIds = lineIds.Where(w => w == lineId).ToList();
                    if (lineIds?.Count == 0) lineIds.Add(0);
                }
                if (pagenumber > 0) pagenumber -= 1;

                //查询出的数据为每一笔拜访记录
                var lists = _visitStoreService.GetAllVisitReacheds(curStore?.Id ?? 0,
                    businessUserId ?? 0,
                    lineIds,
                    models.StartTime,
                    models.EndTime,
                    pageIndex: pagenumber,
                    pageSize: 30);
                lists.ToList().ForEach(d =>
                {
                    d.LineId = _lineTierService.GetTerminalLineId(d.BusinessUserId, d.TerminalId);
                });
                models.PagingFilteringContext.LoadPagedList(lists);

                //先根据业务员分组、再根据时间分组、再根据路线分组
                var items = lists.GroupBy(x => x.SigninDateTime.ToString("yyyy-MM-dd")).ToList();
                items.ForEach(date =>
                {
                    //获取当前业务员拜访数据
                    var dateData = lists.Where(x => x.SigninDateTime.ToString("yyyy-MM-dd") == date.Key).ToList();
                    dateData.GroupBy(s => s.BusinessUserId).ToList().ForEach(user =>
                    {
                        //某个业务员当天的拜访数据
                        dateData.GroupBy(x => x.LineId).ToList().ForEach(line =>
                        {
                            var model = new VisitReachedModel();
                            var lineData = dateData.Where(x => x.LineId == line.Key).ToList();

                            model.BusinessUserId = user.Key;
                            model.BusinessUserName = _userService.GetUserName(curStore.Id, user.Key);
                            model.LineId = line.Key ?? 0;

                            //路线
                            model.LineName = _lineTierService.GetLineTierById(curStore.Id, line.Key == null ? 0 : line.Key.Value)?.Name;

                            //日期
                            model.SigninDateTime = DateTime.Parse(date.Key);
                            //停留时间
                            model.OnStoreStopSeconds = GetTime(lineData.Where(w => w.BusinessUserId == user.Key).Sum(x => x.OnStoreStopSeconds) ?? 0);

                            var result = _visitStoreService.GetVisitRecordsByUserid(curStore.Id, user.Key, DateTime.Parse(date.Key));
                            result.ToList().ForEach(v =>
                        {
                            var visitStore = lists.Where(w => w.BusinessUserId == v.BusinessUserId && w.SigninDateTime.ToString("yyyy-MM-dd") == date.Key && w.TerminalId == v.TerminalId).FirstOrDefault();
                            v.LineId = visitStore == null ? 0 : visitStore.LineId;
                        });
                            //计划拜访数即拜访线路的客户数
                            model.PlanVisitCount = GetUserPlanVisitCount(curStore.Id, user.Key, line.Key ?? 0);

                            //实际拜访数
                            model.ActualVisitCount = result.Count(x => x.LineId == line.Key);



                            //获取业务员当天该条路线的所有开单拜访记录
                            result = result.Where(c => (c.SaleBillId != null && c.SaleBillId != 0) || (c.SaleReservationBillId != null && c.SaleReservationBillId != 0) || (c.ReturnBillId != null && c.ReturnBillId != 0) || (c.ReturnReservationBillId != null && c.ReturnReservationBillId != 0) && c.LineId == line.Key).ToList();
                            var lst_terminal = new List<int>();
                            if (companySetting.EnableBusinessVisitLine)
                            {
                                lst_terminal = _terminalService.GetTerminalsByLineId(curStore.Id, line.Key ?? 0).Select(s => s.Id).ToList();
                            }
                            else
                            {
                                var lst_dis = _districtService.GetUserDistrict(curStore.Id, user.Key);
                                lst_terminal = _terminalService.GetTerminalsByDistrictIds(curStore.Id, lst_dis.Select(s => s.Id).ToList(), lineId ?? 0).Select(s => s.Id).ToList();
                            }
                            model.TerminalCount = lst_terminal.Count();  //终端数

                            model.UnVisitCount = lst_terminal.Where(w => !lineData.Select(s => s.TerminalId).Contains(w)).Count();

                            //拜访成功率(实际拜访数量/计划拜访数量)
                            double visitRate = 0;
                            if (model.TerminalCount >= 0)
                            {
                                visitRate = model.TerminalCount == 0 ? 100 : ((double)model.ActualVisitCount / (double)model.TerminalCount);
                            }
                            model.VisitSuccessRate = string.Format("{0:N2}", visitRate * 100) + "%";

                            //计算开单数
                            var billCount = 0;
                            if (result != null && result.Count > 0)
                            {
                                foreach (var vm in result)
                                {
                                    if (vm.SaleBillId != 0 && vm.SaleBillId != null)
                                    {
                                        billCount++;
                                    }

                                    if (vm.SaleReservationBillId != null && vm.SaleReservationBillId != 0)
                                    {
                                        billCount++;
                                    }

                                    if (vm.ReturnBillId != null && vm.ReturnBillId != 0)
                                    {
                                        billCount++;
                                    }

                                    if (vm.ReturnReservationBillId != null && vm.ReturnReservationBillId != 0)
                                    {
                                        billCount++;
                                    }
                                }
                            }

                            //成交单数
                            model.ReachedBillCount = billCount;

                            //成交店数
                            model.ReachedStoreCount = result.Count;

                            //成交率(成交单数/实际拜访数量)
                            if (model.ActualVisitCount == 0)
                                model.CloseRate = "0%";
                            else
                                model.CloseRate = (model.ReachedBillCount / model.ActualVisitCount) * 100 + "%";

                            tempdatas.Add(model);

                        });
                    });
                });

                models.Items = tempdatas;

            }
            catch (Exception)
            { }

            return View(models);
        }

        //业务员拜访达成表导出
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.SaleDetailsExport)]
        public FileResult ExportBusinessUserVisitReached(int? businessUserId, int? lineId, DateTime? startTime = null, DateTime? endTime = null)
        {



            #region 表头
            DataTable dt = new DataTable("ExportBusinessUserVisitReached");
            dt.Columns.Add("业务员", typeof(string));
            dt.Columns.Add("日期", typeof(string));
            dt.Columns.Add("拜访线路", typeof(string));
            dt.Columns.Add("计划拜访", typeof(string));
            dt.Columns.Add("实际拜访", typeof(string));
            dt.Columns.Add("成交店数", typeof(string));
            dt.Columns.Add("成交单数", typeof(string));
            dt.Columns.Add("拜访成功率", typeof(string));
            dt.Columns.Add("成交率", typeof(string));
            dt.Columns.Add("定位时长", typeof(string));

            #endregion

            #region 查询导出数据

            var tempdatas = new List<VisitReachedModel>();
            //查询出的数据为每一笔拜访记录
            var sqlDatas = _visitStoreService.GetVisitReacheds(curStore?.Id ?? 0,
                businessUserId ?? 0,
                lineId ?? 0,
                (startTime == null) ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")) : startTime,
                (endTime == null) ? DateTime.Now.AddDays(1) : endTime);

            //先根据业务员分组、再根据时间分组、再根据路线分组
            sqlDatas.GroupBy(x => x.SigninDateTime.ToString("yyyy-MM-dd")).ToList().ForEach(date =>
            {
                //获取当前业务员拜访数据
                var dateData = sqlDatas.Where(x => x.SigninDateTime.ToString("yyyy-MM-dd") == date.Key).ToList();
                dateData.GroupBy(s => s.BusinessUserId).ToList().ForEach(user =>
                {
                    //某个业务员当天的拜访数据
                    dateData.GroupBy(x => x.LineId).ToList().ForEach(line =>
                    {
                        var model = new VisitReachedModel();
                        var lineData = dateData.Where(x => x.LineId == line.Key).ToList();

                        model.BusinessUserId = user.Key;
                        model.BusinessUserName = _userService.GetUserName(curStore.Id, user.Key);
                        model.LineId = line.Key ?? 0;
                        //路线
                        model.LineName = _lineTierService.GetLineTierById(curStore.Id, line.Key == null ? 0 : line.Key.Value)?.Name;

                        //日期
                        model.SigninDateTime = DateTime.Parse(date.Key);
                        //停留时间
                        model.OnStoreStopSeconds = GetTime(lineData.Sum(x => x.OnStoreStopSeconds) ?? 0);

                        var result = _visitStoreService.GetVisitRecordsByUserid(curStore?.Id ?? 0, user.Key, DateTime.Parse(date.Key));

                        //计划拜访数即拜访线路的客户数
                        model.PlanVisitCount = GetUserPlanVisitCount(curStore?.Id ?? 0, user.Key, line.Key ?? 0);

                        //实际拜访数
                        model.ActualVisitCount = result.Count(x => x.LineId == line.Key);

                        //拜访成功率(实际拜访数量/计划拜访数量)
                        decimal visitRate = model.PlanVisitCount == 0 ? 100 : (model.ActualVisitCount / model.PlanVisitCount);
                        model.VisitSuccessRate = visitRate + "%";

                        //获取业务员当天该条路线的所有开单拜访记录
                        result = result.Where(c => (c.SaleBillId != null && c.SaleBillId != 0) || (c.SaleReservationBillId != null && c.SaleReservationBillId != 0) || (c.ReturnBillId != null && c.ReturnBillId != 0) || (c.ReturnReservationBillId != null && c.ReturnReservationBillId != 0) && c.LineId == line.Key).ToList();

                        //计算开单数
                        var billCount = 0;
                        if (result != null && result.Count > 0)
                        {
                            foreach (var vm in result)
                            {
                                if (vm.SaleBillId != 0 && vm.SaleBillId != null)
                                {
                                    billCount++;
                                }

                                if (vm.SaleReservationBillId != null && vm.SaleReservationBillId != 0)
                                {
                                    billCount++;
                                }

                                if (vm.ReturnBillId != null && vm.ReturnBillId != 0)
                                {
                                    billCount++;
                                }

                                if (vm.ReturnReservationBillId != null && vm.ReturnReservationBillId != 0)
                                {
                                    billCount++;
                                }

                            }
                        }

                        //成交单数
                        model.ReachedBillCount = billCount;

                        //成交店数
                        model.ReachedStoreCount = result.Count;

                        //成交率(成交单数/实际拜访数量)
                        model.CloseRate = (model.ReachedBillCount / model.ActualVisitCount) * 100 + "%";

                        tempdatas.Add(model);
                    });
                });
            });

            #region 格式化
            tempdatas.ToList().ForEach(s =>
            {
                DataRow dr = dt.NewRow();
                dr["业务员"] = s.BusinessUserName;
                dr["日期"] = s.SigninDateTime;
                dr["拜访线路"] = s.LineName;
                dr["计划拜访"] = s.PlanVisitCount;
                dr["实际拜访"] = s.ActualVisitCount;
                dr["成交店数"] = s.ReachedStoreCount;
                dr["成交单数"] = s.ReachedBillCount;
                dr["拜访成功率"] = s.VisitSuccessRate;
                dr["成交率"] = s.CloseRate;
                dr["定位时长"] = s.OnStoreStopSeconds;

                dt.Rows.Add(dr);
            });

            #endregion

            #endregion

            #region 导出
            var ms = _exportService.ExportTableToExcel("sheet1", dt);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "业务员拜访达成表.xls");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "业务员拜访达成表.xls");
            }
            #endregion

        }

        /// <summary>
        /// 业务员拜访达成表(按线路)
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.VisitingScheduleOnlineView)]
        public IActionResult BusinessUserVisitReachedOnline(int? userid,DateTime? date)
        {
            var models = new VisitOnlineModel();
            try
            {
                if (date == null)
                {
                    date = DateTime.Parse(DateTime.Now.ToShortDateString());
                }

                models.BusinessUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Salesmans, curUser.Id, true, _userService.IsAdmin(curStore.Id, curUser.Id));

                var defaultUser = models.BusinessUsers.FirstOrDefault();
                int uid = 0;
                if (userid.HasValue && userid > 0)
                {
                    uid = userid.Value;
                }
                else
                {
                    uid = int.Parse(defaultUser.Value);
                }

                //拜访记录
                var records = _visitStoreService
                    .GetVisitRecordsByUserid(curStore?.Id ?? 0, uid, date)
                    .Select(x => x.ToModel<VisitStoreModel>())
                    .ToList();

                var lines = _visitStoreService.GetLineReachs(curStore?.Id ?? 0, uid);
                foreach (IGrouping<int, ReachOnlineQuery> lgroup in lines.GroupBy(s => s.LineId))
                {
                    int lid = lgroup.Key;
                    var l = lines.Where(s => s.LineId == lid).FirstOrDefault();
                    models.Lines.Add(new LineVisitRecord
                    {
                        LineId = lid,
                        LineName = l.LineName,

                        VisitRecords = lgroup.Select(s =>
                        {
                            var curt = records.Where(r => r.TerminalId == s.TerminalId).FirstOrDefault();

                            return new VisitStoreModel()
                            {
                                TerminalId = s?.TerminalId ?? 0,
                                TerminalName = s?.TerminalName ?? "",

                                OnStoreStopSeconds = curt?.OnStoreStopSeconds ?? 0,
                                SigninDateTime = curt?.SigninDateTime ?? DateTime.Now,
                                SignOutDateTime = curt?.SignOutDateTime ?? DateTime.Now,
                                SignTypeId = curt?.SignTypeId ?? 0
                            };

                        }).ToList()
                    }); ;
                }
            }
            catch (Exception)
            {
            }
            return View(models);
        }

        #endregion


        //外勤轨迹跟踪
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.SalesmanTrackView)]
        public IActionResult BusinessUserFieldTrack(int? store, DateTime? signinDateTime)
        {

            var models = new BusinessVisitModel();
            return View(models);
        }

        public async Task<JsonResult> UserVisitDetail(int userId)
        {

            return await Task.Run(() =>
            {
                var records = _visitStoreService.GetVisitRecordsByUserid(curStore?.Id ?? 0, userId).Select(x => x.ToModel<VisitStoreModel>()).ToList();
                return Json(new
                {
                    total = records.Count,
                    rows = records.Count > 0 ? records : null
                });
            });
        }

        [Route("visitStore/getBusinessUserTracks")]
        public async Task<JsonResult> GetBusinessUserTracks(DateTime? signinDateTime)
        {
            if (signinDateTime == null)
            {
                signinDateTime = DateTime.Parse(DateTime.Now.ToShortDateString());
            }

            return await Task.Run(() =>
            {
                //获取在店时间配置
                var onStoreStopSecondsSetting = _settingService.LoadSetting<CompanySetting>(curStore?.Id ?? 0)?.OnStoreStopSeconds;

                //获取经销商所有业务员
                var users = _userService.GetUserBySystemRoleName(curStore?.Id ?? 0, DCMSDefaults.Salesmans)
                .Select(u =>
                {
                    var model = new BusinessVisitListModel();

                    var terminalsCount = GetUserTerminals(curStore?.Id ?? 0, u.Id);

                    //拜访轨迹
                    var records = _visitStoreService
                      .GetVisitRecordsByUserid(curStore?.Id ?? 0, u.Id, signinDateTime)
                      .Select(x => x.ToModel<VisitStoreModel>())
                      .ToList();

                    //实时跟踪轨迹
                    var tracks = _visitStoreService.GetQueryVisitStoreAndTrackingForWeb(curStore?.Id ?? 0, u.Id, signinDateTime, null)?.ToList();


                    return new BusinessVisitListModel()
                    {
                      BusinessUserId = u.Id,
                      BusinessUserName = u.UserRealName,
                      VisitedCount = records.Count,
                      NoVisitedCount = terminalsCount - records.Count,
                      ExceptVisitCount = records.Select(r => r.SignTypeId == 1 && (r.SignOutDateTime.Value.Subtract(r.SigninDateTime).TotalMinutes < onStoreStopSecondsSetting)).Count(),
                      VisitRecords = records,
                      RealTimeTracks = tracks
                  };
              }).OrderByDescending(g => g.VisitedCount).ToList();

                return Json(new { Data = users });
            });
        }


        private int GetUserTerminals(int? store, int userId)
        {
            int count = 0;
            var items = _lineTierService.GetUserLineTierAssigns(userId)
                .Select(l =>
                {
                    //获取业务员客户
                    var tids = l.LineTier.LineTierOptions.Select(o => o.TerminalId).ToArray();
                    //获取客户实体
                    return _terminalService.GetTerminalsByIds(curStore.Id, tids, true).ToList();
                }).ToList();
            var all = _terminalService.GetAllTerminal(store).ToList().Count();
            count = items.Count;
            if ((count == 0 || count < all) && all != 0)
            {
                count = all;
            }

            return count;
        }

        private int GetUserPlanVisitCount(int? store, int userId, int lineId)
        {
            int count = 0;
            if (lineId != 0)
            {
                var items = _lineTierService.GetUserLineTierAssigns(userId).Where(x => x.LineTierId == lineId)
               .Select(l =>
               {
                   //获取业务员客户
                   var tids = l.LineTier.LineTierOptions.Select(o => o.TerminalId).ToArray();
                   //获取客户实体
                   return _terminalService.GetTerminalsByIds(curStore.Id, tids).ToList();
               }).ToList();

                count = items.Count;
            }
            return count;
        }

        private string GetTime(int time)
        {
            //小时计算
            int hours = (time) % (24 * 3600) / 3600;
            //分钟计算
            int minutes = (time) % 3600 / 60;

            return hours + "时" + minutes + "分";
        }


        [HttpGet]
        [CheckAccessPublicStore(true)]
        [RequestSizeLimit(52428800)]
        public IActionResult ShowImage(string url)
        {
            try
            {
                if(string.IsNullOrEmpty(url))
                {
                    url = $"{resourceServer}/content/images/loading.gif";
                }

                var request = (HttpWebRequest)WebRequest.Create(url);
                var response = (HttpWebResponse)request.GetResponse();
                var responseStream = response.GetResponseStream();
                List<byte> btlst = new List<byte>();
                int b = responseStream.ReadByte();
                while (b > -1)
                {
                    btlst.Add((byte)b);
                    b = responseStream.ReadByte();
                }
                byte[] bts = btlst.ToArray();

                if (response != null)
                    response.Dispose();

                if (responseStream != null)
                    responseStream.Dispose();

                return new FileContentResult(bts, "image/jpeg");
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [HttpGet]
        public IActionResult GetLineTierByUserId(int userId) 
        {
            try
            {
                //线路
                var lst_line = _lineTierService.BindLineTier(curStore.Id);
                //判断是否启用业务员线路/是否超级管理员
                var companySetting = _settingService.LoadSetting<CompanySetting>(curStore.Id);
                if (companySetting.EnableBusinessVisitLine && !_userService.IsAdmin(curStore.Id, userId)) //启用业务员线路并且不是超级管理员
                {
                    //获取用户指定线路
                    var lst = _lineTierService.GetUserLineTier(curStore.Id, userId);
                    lst_line = lst_line.Where(w => lst.Contains(w.Id)).ToList();
                }
                return Json(new { Success=true, Data = lst_line });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Data = ex.Message });
            }
        }
    }
}