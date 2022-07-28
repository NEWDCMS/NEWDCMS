using DCMS.Api.Infrastructure.Mapper.Extensions;
using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Census;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Tasks;
using DCMS.Services.Census;
using DCMS.Services.Configuration;
using DCMS.Services.Sales;
using DCMS.Services.Tasks;
using DCMS.Services.Terminals;
using DCMS.Services.Users;
using DCMS.Services.Visit;
using DCMS.ViewModel.Models.Report;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NUglify.Helpers;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

using DCMS.Api.Helpers;
using DCMS.Services.Logging;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Net.Http;
using System.IO;


namespace DCMS.Api.Controllers
{
    /// <summary>
    /// 用于终端拜访管理
    /// </summary>
    [Authorize]
    [Route("api/v{version:apiVersion}/dcms/census")]
    public class VisitStoreController : BaseAPIController
    {
        private readonly IUserService _userService;
        private readonly ISettingService _settingService;
        private readonly IQueuedMessageService _queuedMessageService;
        private readonly ITerminalService _terminalService;
        private readonly ILineTierService _lineTierService;
        private readonly ISaleBillService _saleBillService;
        private readonly ISaleReservationBillService _saleReservationBillService;
        private readonly IReturnBillService _returnBillService;
        private readonly IReturnReservationBillService _returnReservationBillService;

        private readonly IVisitStoreService _visitStoreService;
        private readonly ITrackingService _trackingService;
        private readonly IRedLocker _locker;
        private readonly IDistrictService _districtService;
        private readonly static object _MyLock = new object();

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="userService"></param>
        /// <param name="settingService"></param>
        /// <param name="queuedMessageService"></param>
        /// <param name="terminalService"></param>
        /// <param name="saleBillService"></param>
        /// <param name="saleReservationBillService"></param>
        /// <param name="returnBillService"></param>
        /// <param name="returnReservationBillService"></param>
        /// <param name="lineTierService"></param>
        /// <param name="visitStoreService"></param>
        /// <param name="trackingService"></param>
        public VisitStoreController(
            IUserService userService,
            ISettingService settingService,
            IQueuedMessageService queuedMessageService,
            ITerminalService terminalService,
            ISaleBillService saleBillService,
            ISaleReservationBillService saleReservationBillService,
            IReturnBillService returnBillService,
            IReturnReservationBillService returnReservationBillService,

            ILineTierService lineTierService,
            IVisitStoreService visitStoreService,
            ITrackingService trackingService,
            IDistrictService districtService,
            IRedLocker locker, ILogger<BaseAPIController> logger) : base(logger)
        {
            _userService = userService;
            _settingService = settingService;
            _queuedMessageService = queuedMessageService;
            _terminalService = terminalService;
            _lineTierService = lineTierService;
            _saleBillService = saleBillService;
            _returnBillService = returnBillService;
            _saleReservationBillService = saleReservationBillService;
            _returnReservationBillService = returnReservationBillService;

            _visitStoreService = visitStoreService;
            _trackingService = trackingService;
            _districtService = districtService;
            _locker = locker;
        }


        /// <summary>
        /// 获取业务员门店拜访记录
        /// </summary>
        /// <param name="store">经销商</param>
        /// <param name="terminalId">客户</param>
        /// <param name="districtId">片区</param>
        /// <param name="businessUserId">业务员</param>
        /// <param name="start">开始时间</param>
        /// <param name="end">结束时间</param>
        /// <returns></returns>
        [HttpGet("vist/getVisitStores/{store}")]
        [SwaggerOperation("getVisitStores")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<VisitStoreModel>>> GetVisitStores(int? store, int? terminalId = 0, int? districtId = 0, int? businessUserId = 0, DateTime? start = null, DateTime? end = null, int pageIndex = 0, int pageSize = 50)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error2<VisitStoreModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var records = _visitStoreService.GetVisitRecords(store, businessUserId, terminalId, null, districtId, 0, 0, start, end, pageIndex, pageSize);
                    var allLines = _lineTierService.GetLineTiersByIds(store, records.Select(r => r.LineId ?? 0).ToArray());

                    var results = records.Select(m =>
                    {
                        var model = m.ToModel<VisitStoreModel>();

                        model.FaceImage = _userService.GetUserById(store, businessUserId ?? 0)?.FaceImage;

                        //状态
                        model.VisitTypeName = CommonHelper.GetEnumDescription((VisitTypeEnum)Enum.Parse(typeof(VisitTypeEnum), m.VisitTypeId.ToString()));

                        //未拜访天数
                        var lastRecord = _visitStoreService.GetLastRecord(store != null ? store : 0, m.Id, 0, 0);
                        model.NoVisitedDays = lastRecord != null && lastRecord.SignOutDateTime != null ? (m.SigninDateTime.Subtract(lastRecord.SignOutDateTime.Value).Days) : 0;

                        //达成金额计算
                        if (m.SaleBillId != null && m.SaleBillId != 0)
                        {
                            var saleBill = _saleBillService.GetSaleBillById(store, m.SaleBillId.Value);
                            model.SaleAmount = saleBill == null ? 0 : saleBill.SumAmount;
                        }

                        if (m.SaleReservationBillId != null && m.SaleReservationBillId != 0)
                        {
                            var saleReservationBill = _saleReservationBillService.GetSaleReservationBillById(store, m.SaleReservationBillId.Value);
                            model.SaleOrderAmount = saleReservationBill == null ? 0 : saleReservationBill.SumAmount;
                        }

                        if (m.ReturnBillId != null && m.ReturnBillId != 0)
                        {
                            var returnBill = _returnBillService.GetReturnBillById(store, m.ReturnBillId.Value);
                            model.ReturnAmount = returnBill == null ? 0 : returnBill.SumAmount;
                        }

                        if (m.ReturnReservationBillId != null && m.ReturnReservationBillId != 0)
                        {
                            var returnReservationBill = _returnReservationBillService.GetReturnReservationBillById(store, m.ReturnReservationBillId.Value);
                            model.ReturnOrderAmount = returnReservationBill == null ? 0 : returnReservationBill.SumAmount;
                        }

                        //路线
                        var lineTier = allLines.Where(al => al.Id == (m.LineId == null ? 0 : m.LineId.Value)).FirstOrDefault();
                        m.LineName = lineTier == null ? "" : lineTier.Name;

                        model.DoorheadPhotos = _visitStoreService.GetDoorheadPhotoByVisitId(model.Id)?.Select(x => new DoorheadPhoto
                        {
                            StoragePath = x.StoragePath,
                            RestaurantId = 0,
                            TraditionId = 0,
                            VisitStoreId = x.VisitStoreId
                        }).ToList();

                        model.DisplayPhotos = _visitStoreService.GetDisplayPhotoByVisitId(model.Id)?.Select(x => new DisplayPhoto
                        {
                            DisplayPath = x.DisplayPath,
                            RestaurantId = 0,
                            TraditionId = 0,
                            VisitStoreId = x.VisitStoreId
                        }).ToList();

                        return m == null ? new VisitStoreModel() : model;
                    }).ToList();


                    return this.Successful2("", results);
                }
                catch (Exception ex)
                {
                    return this.Error2<VisitStoreModel>(ex.Message);
                }

            });
        }

        /// <summary>
        /// 拜访签到
        /// </summary>
        /// <param name="model"></param>
        /// <param name="store"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost("vist/signInVisitStore/{store}")]
        [SwaggerOperation("signInVisitStore")]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> SignInVisitStore(VisitStoreModel model, int? store, int? userId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error(Resources.ParameterError);

            return await Task.Run(() =>
            {

                try
                {
                    var user = _userService.GetUserById(store ?? 0, model.BusinessUserId);
                    if (model != null)
                    {
                        //上次拜访
                        var lastVisited = new VisitStore();
                        var visitStore = model.ToEntity<VisitStore>();

                        #region 添加拜访记录
                        if (visitStore?.StoreId != 0 && visitStore?.TerminalId != 0 && visitStore?.BusinessUserId != 0)
                        {
                            var isOut = _visitStoreService.CheckOut(visitStore.StoreId, visitStore.TerminalId, visitStore.BusinessUserId);
                            if (isOut != null)
                            {
                                return this.Successful("尚未签退", isOut);
                            }

                            lock (_MyLock)
                            {

                                visitStore.SignTypeId = 1;
                                visitStore.SigninDateTime = DateTime.Now;
                                visitStore.SignType = SignEnum.CheckIn;
                                visitStore.DistrictId = model.DistrictId ?? 0;
                                visitStore.ChannelId = model.ChannelId ?? 0;
                                visitStore.NextOrderDays = model.NextOrderDays ?? 0;
                                visitStore.LastSigninDateTime = 0;
                                visitStore.LastPurchaseDateTime = 0;
                                visitStore.OnStoreStopSeconds = model.OnStoreStopSeconds ?? 0;
                                visitStore.LineId = model.LineId ?? 0;
                                visitStore.Latitude = model.Latitude ?? 0;
                                visitStore.Longitude = model.Longitude ?? 0;
                                visitStore.SaleBillId = model.SaleBillId ?? 0;
                                visitStore.SaleReservationBillId = model.SaleReservationBillId ?? 0;
                                visitStore.ReturnBillId = model.ReturnBillId ?? 0;
                                visitStore.ReturnReservationBillId = model.ReturnReservationBillId ?? 0;
                                visitStore.SaleAmount = model.SaleAmount ?? 0;
                                visitStore.SaleOrderAmount = model.SaleOrderAmount ?? 0;
                                visitStore.ReturnAmount = model.ReturnAmount ?? 0;
                                visitStore.ReturnOrderAmount = model.ReturnOrderAmount ?? 0;
                                visitStore.NoVisitedDays = model.NoVisitedDays ?? 0;

                                _visitStoreService.InsertVisitStore(visitStore);
                            }
                        }
                        #endregion

                        if (visitStore.Id > 0)
                        {
                            //这里获取上一次 签到记录  
                            lastVisited = _visitStoreService.GetLastRecord(store, 0, model.TerminalId, model.BusinessUserId);

                            if (lastVisited != null)
                            {
                                //上次签到时间
                                lastVisited.LastSigninDateTime = model.SigninDateTime.Subtract(lastVisited.SigninDateTime).Days;

                                //这里获取上一次开单记录 SaleBillId
                                // 条件为： TerminalId BusinessUserId
                                var sales = _saleBillService.GetLastSaleBill(store, model.TerminalId,
                                model.BusinessUserId);

                                if (sales != null)
                                {
                                    //上次采购时间
                                    lastVisited.LastPurchaseDateTime = model.SigninDateTime.Subtract(sales.CreatedOnUtc).Days;
                                }
                            }
                        }

                        //发送通知
                        try
                        {
                            //获取在店时间配置
                            var distanceSetting = _settingService.LoadSetting<CompanySetting>(store ?? 0).SalesmanDeliveryDistance;
                            if (model.Distance > distanceSetting)
                            {
                                //获取当前用户管理员用户 电话号码
                                var adminMobileNumbers = _userService.GetAllUserMobileNumbersByUserIds(new List<int> { model.BusinessUserId }).ToList();
                                QueuedMessage queuedMessage = new QueuedMessage()
                                {
                                    StoreId = store ?? 0,
                                    MType = MTypeEnum.CheckException,
                                    Title = CommonHelper.GetEnumDescription<MTypeEnum>(MTypeEnum.CheckException),
                                    BillType = BillTypeEnum.Other,
                                    BillId = visitStore.Id,
                                    CreatedOnUtc = DateTime.Now,
                                    BusinessUser = (user == null) ? "" : user.UserRealName,
                                    TerminalName = model.TerminalName,
                                    Distance = model.Distance
                                };
                                _queuedMessageService.InsertQueuedMessage(adminMobileNumbers, queuedMessage);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(0, ex, ex.Message);
                        }

                        return this.Successful("签到成功", visitStore);
                    }

                    return this.Error("签到失败");
                }
                catch (Exception ex)
                {
                    return this.Error(ex.StackTrace);
                }
            });
        }


        /// <summary>
        /// 拜访签退
        /// </summary>
        /// <param name="model"></param>
        /// <param name="store"></param>
        /// <returns></returns>
        [HttpPost("vist/signOutVisitStore/{store}")]
        [SwaggerOperation("signOutVisitStore")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> SignOutVisitStore(VisitStoreModel model, int? store, int? userId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error(Resources.ParameterError);

            return await Task.Run(() =>
           {
               try
               {
                   var user = _userService.GetUserById(store ?? 0, userId ?? 0);
                   if (model != null && model.Id > 0)
                   {
                       lock (_MyLock)
                       {
                           //更新签退状态
                           var visitStore = _visitStoreService.GetRecordById(store, model.Id);
                           if (visitStore != null)
                           {
                               visitStore.SignOutDateTime = DateTime.Now;
                               visitStore.SignTypeId = 2;
                               visitStore.SignType = SignEnum.Signed;
                               visitStore.OnStoreStopSeconds = (int)(visitStore.SignOutDateTime - visitStore.SigninDateTime).Value.TotalSeconds;
                               visitStore.Remark = model.Remark;
                               //更改拜访开单信息
                               if (model.SaleReservationBillId > 0)
                               {
                                   visitStore.SaleReservationBillId = model.SaleReservationBillId;
                                   visitStore.SaleOrderAmount = model.SaleOrderAmount;
                               }
                               if (model.SaleBillId > 0)
                               {
                                   visitStore.SaleBillId = model.SaleBillId;
                                   visitStore.SaleAmount = model.SaleAmount;
                               }
                               if (model.ReturnReservationBillId > 0)
                               {
                                   visitStore.ReturnReservationBillId = model.ReturnReservationBillId;
                                   visitStore.ReturnOrderAmount = model.ReturnOrderAmount;
                               }
                               if (model.ReturnBillId > 0)
                               {
                                   visitStore.ReturnBillId = model.ReturnBillId;
                                   visitStore.ReturnAmount = model.ReturnAmount;
                               }
                               _visitStoreService.UpdateVisitStore(visitStore);

                               #region 添加图片
                               foreach (var photo in model.DisplayPhotos)
                               {
                                   var displayPhoto = new DisplayPhoto
                                   {
                                       StoreId = store ?? 0,
                                       DisplayPath = photo.DisplayPath,
                                       RestaurantId = 0,
                                       TraditionId = 0,
                                       UpdateDate = DateTime.Now,
                                       VisitStoreId = visitStore.Id
                                   };
                                   _visitStoreService.InsertDisplayPhoto(displayPhoto);
                               }

                               foreach (var photo in model.DoorheadPhotos)
                               {
                                   var doorPhoto = new DoorheadPhoto
                                   {
                                       StoreId = store ?? 0,
                                       StoragePath = photo.StoragePath,
                                       RestaurantId = 0,
                                       TraditionId = 0,
                                       UpdateDate = DateTime.Now,
                                       VisitStoreId = visitStore.Id
                                   };
                                   _visitStoreService.InsertDoorheadPhoto(doorPhoto);
                               }
                               #endregion

                               return this.Successful("签退成功", visitStore);
                           }
                       }

                       return this.Error("签退失败");
                   }
                   else
                   {
                       return this.Error("签退失败");
                   }
               }
               catch (Exception ex)
               {
                   return this.Error(ex.Message);
               }
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
                 return _terminalService.GetTerminalsByIds(store, tids).ToList();
             }).ToList();

            var all = _terminalService.GetAllTerminal(store).ToList().Count();
            count = items.Count;
            if ((count == 0 || count < all) && all != 0)
            {
                count = all;
            }

            return count;
        }




        /// <summary>
        /// 获取业务员最近签到
        /// </summary>
        /// <param name="store"></param>
        /// <param name="terminalId"></param>
        /// <param name="businessUserId"></param>
        /// <returns></returns>
        [HttpGet("vist/getLastVisitStore/{store}")]
        [SwaggerOperation("getLastVisitStore")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<VisitStoreModel>> GetLastVisitStore(int? store, int? terminalId = 0, int? businessUserId = 0)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error3<VisitStoreModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var result = new VisitStoreModel();
                    var sale = _saleBillService.GetLastSaleBill(store, terminalId, businessUserId);
                    var record = _visitStoreService.GetUserLastRecord(store, terminalId, businessUserId);
                    if (record != null)
                    {
                        result = record.ToModel<VisitStoreModel>();
                        if (sale != null)
                        {
                            result.LastPurchaseDate = sale.CreatedOnUtc;
                        }
                    }

                    return this.Successful3("", result);

                }
                catch (Exception ex)
                {
                    return this.Error3<VisitStoreModel>(ex.Message);
                }

            });
        }


        /// <summary>
        /// 获取最后一次签到未签退记录
        /// </summary>
        /// <param name="store"></param>
        /// <param name="businessUserId"></param>
        /// <returns></returns>
        [HttpGet("vist/getOutVisitStore/{store}")]
        [SwaggerOperation("getOutVisitStore")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<VisitStoreModel>> GetOutVisitStore(int? store, int? businessUserId = 0)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error3<VisitStoreModel>(Resources.ParameterError);

            return await Task.Run(async () =>
           {

               try
               {
                   var result = new VisitStoreModel();
                   var record = _visitStoreService.GetUserLastOutRecord(store, businessUserId);
                   if (record != null)
                   {
                       result = record.ToModel<VisitStoreModel>();
                       var sale = await Task.Run(() =>
                       {
                           return _saleBillService.GetLastSaleBill(store, record.TerminalId, businessUserId);
                       });
                       if (sale != null)
                       {
                           result.LastPurchaseDate = sale.CreatedOnUtc;
                       }
                   }

                   return this.Successful3("", result);

               }
               catch (Exception ex)
               {
                   return this.Error3<VisitStoreModel>(ex.Message);
               }

           });
        }




        /// <summary>
        /// 获取经销商所有业务员拜访统计
        /// </summary>
        /// <param name="store"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        [HttpGet("vist/getAllUserVisitedList/{store}")]
        [SwaggerOperation("getAllUserVisitedList")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<BusinessVisitListModel>>> GetAllUserVisitedList(int? store, DateTime? date)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error2<BusinessVisitListModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var lists = new List<BusinessVisitListModel>();
                    //获取在店时间配置
                    var onStoreStopSecondsSetting = _settingService.LoadSetting<CompanySetting>(store ?? 0).OnStoreStopSeconds;

                    //获取经销商所有业务员
                    var users = _userService.GetUserBySystemRoleName(store ?? 0, DCMSDefaults.Salesmans).ToList();
                    users.ForEach(u =>
                    {
                        var salesman = new BusinessVisitListModel
                        {
                            BusinessUserId = u.Id,
                            BusinessUserName = u.UserRealName
                        };

                        //拜访轨迹
                        var records = _visitStoreService.GetVisitRecordsByUserid(store, u.Id, date);
                        if (records != null && records.Count > 0)
                        {
                            salesman.VisitRecords = records.Select(r => r.ToModel<VisitStoreModel>()).ToList();
                        }

                        //跟踪轨迹
                        var realTimRrecords = _visitStoreService.GetQueryVisitStoreAndTracking(store, u.Id, date, date).ToList();
                        if (realTimRrecords != null)
                        {
                            salesman.RealTimeTracks = realTimRrecords;
                        }

                        salesman.VisitedCount = records.Count;
                        salesman.NoVisitedCount = GetUserTerminals(store, u.Id) - records.Count;
                        //已经签到，没有签退的客户数,停留事件小与2分钟则为异常
                        salesman.ExceptVisitCount = records.Select(r => r.SignTypeId == 1 && (r.SignOutDateTime.Value.Subtract(r.SigninDateTime).TotalMinutes < onStoreStopSecondsSetting)).Count();
                        lists.Add(salesman);
                    });

                    return this.Successful2("", lists);
                }
                catch (Exception ex)
                {
                    return this.Error2<BusinessVisitListModel>(ex.Message);
                }

            });
        }


        /// <summary>
        /// 获取业务员拜访排行
        /// </summary>
        /// <param name="store"></param>
        /// <param name="businessUserId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        [HttpGet("vist/getVisitedRanking/{store}")]
        [SwaggerOperation("getVisitedRanking")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<BusinessVisitRankModel>>> GetVisitedRanking(int? store, int? businessUserId = 0, DateTime? start = null, DateTime? end = null)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error2<BusinessVisitRankModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {

                try
                {
                    var lists = new List<BusinessVisitRankModel>();
                    //获取在店时间配置
                    var onStoreStopSecondsSetting = _settingService.LoadSetting<CompanySetting>(store ?? 0).OnStoreStopSeconds;

                    //获取经销商所有业务员
                    var users = _userService.GetUserBySystemRoleName(store ?? 0, DCMSDefaults.Salesmans).ToList();
                    if (businessUserId != 0)
                    {
                        users = users.Where(u => u.Id == businessUserId).ToList();
                    }

                    users.ForEach(u =>
                    {
                        var salesman = new BusinessVisitRankModel
                        {
                            BusinessUserId = u.Id,
                            BusinessUserName = u.UserRealName
                        };

                        var records = _visitStoreService.GetVisitRecordsByUserid(store, u.Id, start, end);
                        salesman.CustomerCount = GetUserTerminals(store, u.Id);
                        salesman.VisitedCount = records.Count;
                        salesman.NoVisitedCount = salesman.CustomerCount - records.Count;
                        //已经签到，没有签退的客户数,停留事件小与2分钟则为异常
                        salesman.ExceptVisitCount = records.Select(r => r.SignTypeId == 1 && (r.SignOutDateTime.Value.Subtract(r.SigninDateTime).TotalMinutes < onStoreStopSecondsSetting)).Count();
                        lists.Add(salesman);
                    });

                    return this.Successful2("", lists);
                }
                catch (Exception ex)
                {
                    return this.Error2<BusinessVisitRankModel>(ex.Message);
                }

            });
        }


        /// <summary>
        /// 获取业务员轨迹跟踪
        /// </summary>
        /// <param name="store"></param>
        /// <param name="businessUserId">用户</param>
        /// <param name="start">开始时间</param>
        /// <param name="end">结束时间</param>
        /// <returns></returns>
        [HttpGet("vist/getUserTracking/{store}")]
        [SwaggerOperation("getUserTracking")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<TrackingModel>>> GetUserTracking(int? store, int? businessUserId = 0, DateTime? start = null, DateTime? end = null)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error2<TrackingModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var records = _trackingService.GetTrackings(
                            store, businessUserId, start, end)
                            .Select(m =>
                            {
                                var model = m.ToModel<TrackingModel>();

                                return m == null ? new TrackingModel() : model;
                            }).ToList();

                    return this.Successful2("", records);
                }
                catch (Exception ex)
                {
                    return this.Error2<TrackingModel>(ex.Message);
                }

            });
        }


        /// <summary>
        /// 上报业务员轨迹
        /// </summary>
        /// <param name="data"></param>
        /// <param name="store"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        //http://api.jsdcms.com:9998/api/v3/dcms/census/vist/reportingTrack/797?userId=1752
        [HttpPost("vist/reportingTrack/{store}")]
        [SwaggerOperation("reportingTrack")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> ReportingTrack(List<TrackingModel> data, int? store, int? userId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error(Resources.ParameterError);

            return await Task.Run(() =>
           {
               try
               {
                   var trackings = data?.Select(s => s.ToEntity<Tracking>()).ToList();
                   trackings.ForEach(s =>
                   {
                       s.StoreId = store ?? 0;
                       s.BusinessUserId = userId ?? 0;
                       s.CreateDateTime = s.CreateDateTime == null ? DateTime.Now : s.CreateDateTime;
                   });

                   var result = _trackingService.InsertTrackings(trackings);

                   return this.Successful("上报成功", result.To(result));
               }
               catch (Exception ex)
               {
                   return this.Error(ex.Message);
               }

           });
        }



        /// <summary>
        /// 获取业务员实时轨迹跟踪
        /// </summary>
        /// <param name="store"></param>
        /// <param name="businessUserId">用户</param>
        /// <param name="start">开始时间</param>
        /// <param name="end">结束时间</param>
        /// <returns></returns>
        [HttpGet("vist/getUserRealTimeTracking/{store}")]
        [SwaggerOperation("getUserRealTimeTracking")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<QueryVisitStoreAndTracking>>> GetUserRealTimeTracking(int? store, int? businessUserId = 0, DateTime? start = null, DateTime? end = null)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error2<QueryVisitStoreAndTracking>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                var result = new APIResult<IList<QueryVisitStoreAndTracking>>();
                try
                {
                    var records = _visitStoreService.GetQueryVisitStoreAndTracking(store, businessUserId, start, end).ToList();
                    if (records != null)
                    {
                        result.Data = records;
                    }

                    return this.Successful2("", records);
                }
                catch (Exception ex)
                {
                    return this.Error2<QueryVisitStoreAndTracking>(ex.Message);
                }

            });
        }



        /// <summary>
        /// 获取客户拜访活跃度排行榜
        /// </summary>
        /// <param name="store"></param>
        /// <param name="businessUserId"></param>
        /// <param name="terminalId"></param>
        /// <returns></returns>
        [HttpGet("vist/getCustomerActivityRanking/{store}")]
        [SwaggerOperation("getCustomerActivityRanking")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<UserActivityRanking>>> GetCustomerActivityRanking(int? store, int? businessUserId = 0, int? terminalId = 0)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error2<UserActivityRanking>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var lists = new List<UserActivityRanking>();
                    //获取经销商所有客户未拜访天数排行（从 VisitStore 取最近上一次 SigninDateTime，计算和当前天数差） 按降序排列
                    var records = _visitStoreService.GetCustomerActivityRanking(store, businessUserId, terminalId).ToList();

                    records.ForEach(u =>
                    {
                        var cord = new UserActivityRanking
                        {
                            TerminalId = u.TerminalId,
                            TerminalName = u.TerminalName,
                            VisitDaySum = (int)DateTime.Now.Subtract(u.SigninDateTime).TotalDays
                        };

                        lists.Add(cord);
                    });

                    var result = lists.OrderByDescending(c => c.VisitDaySum).ToList();

                    return this.Successful2("", result);
                }
                catch (Exception ex)
                {
                    return this.Error2<UserActivityRanking>(ex.Message);
                }

            });
        }


        /// <summary>
        /// 用于拜访照片上传
        /// </summary>
        public const string FileCenterEndpoint = "http://resources.jsdcms.com:9100/";
        [HttpPost("vist/uploadPhotograph/{store}")]
        [SwaggerOperation("uploadPhotograph")]
        //[AuthBaseFilter]
        public async Task<APIResult<UploadResult>> UploadPhotograph(int? store, IFormFile file)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error3<UploadResult>(Resources.ParameterError);

            return await Task.Run(async () =>
            {
                try
                {
                    var httpClientHelper = new HttpClientHelper();
                    var uploadResult = new UploadResult();

                    //保存文件
                    using (var stream = file.OpenReadStream())
                    {
                        if (stream != null)
                        {
                            var scb = new StreamContent(stream);
                            var content = new MultipartFormDataContent { { scb, "\"file\"", $"\"takephotograph.jpg\"" } };
                            var url = $"{FileCenterEndpoint}document/reomte/fileupload/HRXHJS";
                            var result = await httpClientHelper.PostAsync(url, content);

                            if (!string.IsNullOrEmpty(result))
                            {
                                uploadResult = JsonConvert.DeserializeObject<UploadResult>(result);
                            }

                            if (content != null)
                                content.Dispose();

                            if (scb != null)
                                scb.Dispose();
                        }
                    }

                    /*
                     *POST multipart/form-data  https://api.jsdcms.com/api/v3/dcms/census/vist/uploadphotograph/{store}
                     *GET https://www.jsdcms.com/visitstore/showimage?url=http://resources.jsdcms.com:9100/HRXHJS/document/image/{Id}
                     {
                      "code": 1,
                      "data": {
                        "Id": "4807f4c764614789a7451553e8a47b30",
                        "Success": "上传成功!",
                        "MD5": "220744f6766e4c8aa50f0d6d5b2dc829,4807f4c764614789a7451553e8a47b30"
                      },
                      "success": true,
                      "message": "上传成功"
                    }
                    */
                    return this.Successful3("上传成功", uploadResult);
                }
                catch (Exception ex)
                {
                    return this.Error3<UploadResult>(ex.Message);
                }
            });
        }


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
        [HttpGet("vist/getBusinessUserVisitReached/{store}/{businessUserId}/{lineId}")]
        [SwaggerOperation("getBusinessUserVisitReached")]
        public async Task<APIResult<IList<VisitReachedModel>>> GetBusinessUserVisitReached(int? store, int? businessUserId, int? lineId, DateTime? startTime = null, DateTime? endTime = null, int pagenumber = 0 , int pageSize= 2147483647)
        {
            static string GetTime(int time)
            {
                //小时计算
                int hours = (time) % (24 * 3600) / 3600;
                //分钟计算
                int minutes = (time) % 3600 / 60;

                return hours + "时" + minutes + "分";
            }

            if (!store.HasValue || store.Value == 0 || !businessUserId.HasValue || businessUserId.Value == 0 || !lineId.HasValue || lineId.Value == 0)
                return this.Error2<VisitReachedModel>(Resources.ParameterError);
            return await Task.Run(() =>
            {
                try
                {
                    //var models = new VisitReachedListModel();
                    var tempdatas = new List<VisitReachedModel>();

                    var startDate = startTime ?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01"));
                    var endDate = endTime ?? DateTime.Now.AddDays(1);

                    if (pagenumber > 0) pagenumber -= 1;
                    //线路
                    var lst_line = _lineTierService.BindLineTier(store??0);
                    //判断是否启用业务员线路/是否超级管理员
                    var companySetting = _settingService.LoadSetting<CompanySetting>(store ?? 0);
                    if (companySetting.EnableBusinessVisitLine && !_userService.IsAdmin(store ?? 0, businessUserId ?? 0)) //启用业务员线路并且不是超级管理员
                    {
                        //获取用户指定线路
                        var lst = _lineTierService.GetUserLineTier(store??0, businessUserId ?? 0);
                        lst_line = lst_line.Where(w => lst.Contains(w.Id)).ToList();
                    }
                    var lineIds = lst_line.Select(s => s.Id).ToList();
                    if (lineId.HasValue && lineId.Value > 0)
                    {
                        lineIds = lineIds.Where(w => w == lineId).ToList();
                        if (lineIds?.Count == 0) lineIds.Add(0);
                    }
                    //查询出的数据为每一笔拜访记录
                    var lists = _visitStoreService.GetAllVisitReacheds(store ?? 0,
                        businessUserId ?? 0,
                        lineIds,
                        startDate,
                        endDate,
                        pageIndex: pagenumber,
                        pageSize: pageSize);
                    lists.ToList().ForEach(d =>
                    {
                        d.LineId = _lineTierService.GetTerminalLineId(d.BusinessUserId, d.TerminalId);
                    });
                    //models.PagingFilteringContext.LoadPagedList(lists);
                    
                    //先根据业务员分组、再根据时间分组、再根据路线分组
                    var items = lists.GroupBy(x => x.SigninDateTime.ToString("yyyy-MM-dd")).ToList();
                    TimeSpan ts1 = endDate.Subtract(startDate).Duration();
                    var days = ts1.Days;
                    for (int i = 0; i < days; i++)
                    {
                        var day = startDate.AddDays(i).ToString("yyyy-MM-dd");
                        var count = items.Select(s => s.Key).Where(w => w == day).Count();
                        var model = new VisitReachedModel();
                        if (count > 0)
                        {
                            //获取当前业务员拜访数据
                            var dateData = lists.Where(x => x.SigninDateTime.ToString("yyyy-MM-dd") == day).ToList();
                            //日期
                            model.SigninDateTime = DateTime.Parse(day);
                            //停留时间
                            model.OnStoreStopSeconds = GetTime(dateData.Sum(x => x.OnStoreStopSeconds) ?? 0);
                            //实际拜访数
                            model.ActualVisitCount = dateData.Count();

                            var lst_terminal = new List<int>();
                            if (companySetting.EnableBusinessVisitLine)
                            {
                                lst_terminal = _terminalService.GetTerminalsByLineId(store ?? 0, lineId ?? 0).Select(s => s.Id).ToList();
                            }
                            else
                            {
                                var lst_dis = _districtService.GetUserDistrict(store ?? 0, businessUserId ?? 0);
                                lst_terminal = _terminalService.GetTerminalsByDistrictIds(store ?? 0, lst_dis.Select(s => s.Id).ToList(), lineId ?? 0).Select(s => s.Id).ToList();
                            }
                            model.TerminalCount = lst_terminal.Count();  //终端数
                            model.UnVisitCount = lst_terminal.Where(w => !dateData.Select(s => s.TerminalId).Contains(w)).Count();
                            tempdatas.Add(model);
                        }
                        else 
                        {
                            var lst_terminal = new List<int>();
                            if (companySetting.EnableBusinessVisitLine)
                            {
                                lst_terminal = _terminalService.GetTerminalsByLineId(store ?? 0, lineId ?? 0).Select(s => s.Id).ToList();
                            }
                            else
                            {
                                var lst_dis = _districtService.GetUserDistrict(store ?? 0, businessUserId ?? 0);
                                lst_terminal = _terminalService.GetTerminalsByDistrictIds(store ?? 0, lst_dis.Select(s => s.Id).ToList(), lineId ?? 0).Select(s => s.Id).ToList();
                            }
                            model.SigninDateTime = DateTime.Parse(day);//停留时间
                            model.OnStoreStopSeconds = "0时0分";
                            model.TerminalCount = lst_terminal.Count();  //终端数
                            model.UnVisitCount = lst_terminal.Count();  //终端数
                            //实际拜访数
                            model.ActualVisitCount = 0;
                            tempdatas.Add(model);
                        }
                    }
                    return this.Successful2("", tempdatas);
                }
                catch (Exception ex)
                {
                    return this.Error2<VisitReachedModel>(ex.Message);
                }
            });
        }



        /// <summary>
        /// 获取用户线路达成情况
        /// </summary>
        /// <param name="store"></param>
        /// <param name="userId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        [HttpGet("vist/getReachDatas/{store}")]
        [SwaggerOperation("getReachDatas")]
        //[AllowAnonymous]
        public async Task<APIResult<IList<Reach>>> GetReachDatas(int? store, int? userId = 0, DateTime? start = null, DateTime? end = null)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error2<Reach>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var lists = new List<Reach>();
                    var records = _visitStoreService.GetReachs(store, userId, start, end).ToList();
   
                    foreach (IGrouping<int, ReachQuery> tgroup in records.GroupBy(s => s.UserId))
                    {
                        var u = tgroup.FirstOrDefault();

                        foreach (IGrouping<int, ReachQuery> pgroup in tgroup.GroupBy(s => s.LineId))
                        {
                            var l = tgroup.Where(s => s.LineId == pgroup.Key).FirstOrDefault();
                            lists.Add(new Reach()
                            {
                                UserId = u.UserId,
                                UserName = u.UserName,
                                LineId = pgroup.Key,
                                LineName = l.LineName,
                                RecordDatas = pgroup.Select(s => s).ToList()
                            });
                        }
                    }

                    return this.Successful2("", lists);
                }
                catch (Exception ex)
                {
                    return this.Error2<Reach>(ex.Message);
                }

            });
        }



        #endregion

        public class UploadResult
        {
            public string Id { get; set; }
            public string Success { get; set; }
            public string MD5 { get; set; }
        }
    }
}