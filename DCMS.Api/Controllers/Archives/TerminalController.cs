using DCMS.Api.Infrastructure.Mapper.Extensions;
using DCMS.Core;
using DCMS.Core.Domain.Terminals;
using DCMS.Services.Common;
using DCMS.Services.Terminals;
using DCMS.Services.Visit;
using DCMS.ViewModel.Models.Terminals;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Text.RegularExpressions;
using DCMS.Services.Stores;
using DCMS.Core.Domain.CRM;
using DCMS.Core.Domain.Configuration;
using DCMS.Services.Users;
using DCMS.Services.Configuration;

namespace DCMS.Api.Controllers
{
    /// <summary>
    /// 用于终端信息管理
    /// </summary>
    [Authorize]
    [Route("api/v{version:apiVersion}/dcms/archives")]
    public class TerminalController : BaseAPIController
    {
        private readonly ITerminalService _terminalService;
        private readonly IDistrictService _districtService;
        private readonly IChannelService _channelService;
        private readonly IRankService _rankService;
        private readonly ILineTierService _lineTierService;
        private readonly IStoreService _storeService;
        private readonly ICommonBillService _commonBillService;
        private readonly IUserService _userService;
        private readonly ISettingService _settingService;


        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="terminalService"></param>
        /// <param name="districtService"></param>
        /// <param name="channelService"></param>
        /// <param name="rankService"></param>
        /// <param name="lineTierService"></param>
        public TerminalController(
            ITerminalService terminalService,
            IDistrictService districtService,
            IChannelService channelService,
            IRankService rankService,
            IStoreService storeService,
            ILineTierService lineTierService,
            ICommonBillService commonBillService,
            IUserService userService,
            ISettingService settingService,
            ILogger<BaseAPIController> logger) : base(logger)
        {
            _terminalService = terminalService;
            _districtService = districtService;
            _channelService = channelService;
            _rankService = rankService;
            _lineTierService = lineTierService;
            _storeService = storeService;
            _commonBillService = commonBillService;
            _userService = userService;
            _settingService = settingService;
        }


        /// <summary>
        /// 根据客户片区检索客户信息
        /// </summary>
        /// <param name="store"></param>
        /// <param name="searchStr"></param>
        /// <param name="districtId"></param>
        /// <returns></returns>
        [HttpGet("terminal/getTerminals/{store}/{userId}")]
        [SwaggerOperation("getTerminals")]
        //[AuthBaseFilter]
        public async Task<APIResult<IPagedList<TerminalModel>>> GetTerminals(int? store, int? userId ,int? districtId, string searchStr, int? channelId, int? rankId, int? lineId = 0, bool? status = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (!store.HasValue || store.Value == 0)
                return new APIResult<IPagedList<TerminalModel>>() { Message = Resources.ParameterError };

            return await Task.Run(() =>
            {
                try
                {
                    IList<int> districtIds = new List<int>();
                    //if (districtId.HasValue && districtId.Value != 0)
                    //    districtIds = _districtService.GetSubDistrictIds(store ?? 0, districtId ?? 0);
                    districtIds.Add(districtId??0);
                    var terminalList = _terminalService.GetTerminals(store ?? 0,
                         userId,
                         districtIds,
                         searchStr,
                         channelId,
                         rankId,
                         lineId,
                         status,
                         pageIndex,
                         pageSize);

                    var all_districts = _districtService.GetDistrictByIds(store ?? 0, terminalList.Select(s => s.DistrictId).ToArray());
                    var all_dchannels = _channelService.GetChannelsByIds(store ?? 0, terminalList.Select(s => s.ChannelId).ToArray());
                    var all_dranks = _rankService.GetRanksByIds(store ?? 0, terminalList.Select(s => s.RankId).ToArray());
                    var all_dlines = _lineTierService.GetLineTiersByIds(store ?? 0, terminalList.Select(s => s.LineId).ToArray());

                    var result = terminalList.Select(t =>
                    {
                        var model = t.ToModel<TerminalModel>();
                        model.DistrictName = all_districts?.Where(s => s.Id == t.DistrictId).FirstOrDefault()?.Name;
                        model.ChannelName = all_dchannels?.Where(s => s.Id == t.ChannelId).FirstOrDefault()?.Name;
                        model.LineName = all_dlines?.Where(s => s.Id == t.LineId).FirstOrDefault()?.Name;
                        model.RankName = all_dranks?.Where(s => s.Id == t.RankId).FirstOrDefault()?.Name;

                        return model;
                    }).ToList();

                    var pgs = new PagedList<TerminalModel>(result, pageIndex, terminalList?.TotalPages??0, terminalList?.TotalCount ?? 0);
                    return this.Successful8("", pgs);
                }
                catch (Exception ex)
                {
                    return new APIResult<IPagedList<TerminalModel>>() { Message = ex.Message };
                }
            });
        }

        /// <summary>
        /// 根据客户片区检索客户信息
        /// </summary>
        /// <param name="store"></param>
        /// <param name="searchStr"></param>
        /// <param name="districtId"></param>
        /// <returns></returns>
        [HttpGet("terminal/getAllTerminals/{store}/{userId}/{lat}/{lng}/{range}")]
        [SwaggerOperation("getAllTerminals")]
        //[AuthBaseFilter]
        public async Task<APIResult<IPagedList<TerminalModel>>> GetAllTerminals(int? store, int? userId, int? districtId, string searchStr, int? channelId, int? rankId, double? lat = 0, double? lng = 0, double? range = 0.5, int? lineId = 0, bool? status = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (!store.HasValue || store.Value == 0)
                return new APIResult<IPagedList<TerminalModel>>() { Message = Resources.ParameterError };

            return await Task.Run(() =>
            {
                try
                {
                    //获取片区
                    var disIds = new List<int>();
                    var lst = _districtService.GetUserDistrict(store ?? 0, userId ?? 0);
                    if (districtId.HasValue && districtId > 0)  //指定片区
                    {
                        if (lst != null && lst.Count != 0 && lst.Where(w => w.Id == districtId).Count() == 0)  //传递的片区不是用户所属片区则只能查看未设置片区的终端
                        {
                            disIds.Add(0);
                        }
                        else
                        {
                            disIds.Add(districtId ?? 0);
                            var child_lst = _districtService.GetChildDistrict(store ?? 0, districtId.Value);
                            if (child_lst?.Count > 0)  //districtId为父节点
                            {
                                //获取父级区域的所有子区域
                                var lst1 = child_lst.Select(s => s.Id).ToList();
                                disIds.AddRange(lst1);
                            }
                        }
                    }
                    else //未指定片区
                    {
                        if (lst?.Count > 0)
                            disIds.AddRange(lst.Select(s => s.Id).ToList());
                    }
                    //线路
                    var lst_line = new List<int>();
                    if (lineId.HasValue && lineId.Value > 0) 
                        lst_line.Add(lineId??0);
                    //判断是否启用业务员线路/是否超级管理员
                    var companySetting = _settingService.LoadSetting<CompanySetting>(store ?? 0);
                    if (companySetting.EnableBusinessVisitLine && !_userService.IsAdmin(store ?? 0, userId ?? 0))
                    {
                        if (lst_line?.Count == 0) 
                        {
                            //获取用户分配的线路
                            var lst_lineId = _lineTierService.GetUserLineTier(store ?? 0, userId ?? 0);
                            if(lst_lineId != null) lst_line.AddRange(lst_lineId);
                            if (lst_line?.Count == 0) lst_line.Add(0);
                        }
                    }
                    
                    var terminalList = _terminalService.GetAllTerminals(store ?? 0,
                         userId,
                         disIds,
                         searchStr,
                         channelId,
                         rankId,
                         lat ?? 0,
                         lng ?? 0,
                         range ?? 0,
                         lst_line,
                         status,
                         pageIndex,
                         pageSize);

                    var all_districts = _districtService.GetDistrictByIds(store ?? 0, terminalList.Select(s => s.DistrictId).ToArray());
                    var all_dchannels = _channelService.GetChannelsByIds(store ?? 0, terminalList.Select(s => s.ChannelId).ToArray());
                    var all_dranks = _rankService.GetRanksByIds(store ?? 0, terminalList.Select(s => s.RankId).ToArray());
                    var all_dlines = _lineTierService.GetLineTiersByIds(store ?? 0, terminalList.Select(s => s.LineId).ToArray());

                    var result = terminalList.Select(t =>
                    {
                        var model = t.ToModel<TerminalModel>();
                        model.DistrictName = all_districts?.Where(s => s.Id == t.DistrictId).FirstOrDefault()?.Name;
                        model.ChannelName = all_dchannels?.Where(s => s.Id == t.ChannelId).FirstOrDefault()?.Name;
                        model.LineName = all_dlines?.Where(s => s.Id == t.LineId).FirstOrDefault()?.Name;
                        model.RankName = all_dranks?.Where(s => s.Id == t.RankId).FirstOrDefault()?.Name;

                        return model;
                    }).ToList();

                    var pgs = new PagedList<TerminalModel>(result, pageIndex, terminalList?.PageSize ?? 0, terminalList?.TotalCount ?? 0);
                    return this.Successful8("", pgs);
                }
                catch (Exception ex)
                {
                    return new APIResult<IPagedList<TerminalModel>>() { Message = ex.Message };
                }
            });
        }


        /// <summary>
        /// 根据客户片区检索客户信息
        /// </summary>
        /// <param name="store"></param>
        /// <param name="searchStr"></param>
        /// <param name="districtId"></param>
        /// <returns></returns>
        [HttpGet("terminal/getTerminals/{store}/{userId}/{lat}/{lng}/{range}")]
        [SwaggerOperation("getTerminals")]
        //[AuthBaseFilter]
        public async Task<APIResult<IPagedList<TerminalModel>>> GetTerminalsByLatLng(int? store, int? userId, int? districtId, string searchStr, int? channelId, int? rankId,double lat=0,double lng =0, double range = 0.5, int? lineId = 0, bool? status = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (!store.HasValue || store.Value == 0)
                return new APIResult<IPagedList<TerminalModel>>() { Message = Resources.ParameterError };

            return await Task.Run(() =>
            {
                try
                {
                    IList<int> districtIds = null;
                    if (districtId.HasValue && districtId.Value != 0)
                        districtIds = _districtService.GetSubDistrictIds(store ?? 0, districtId ?? 0);

                    var terminalList = _terminalService.GetTerminals(store ?? 0,
                         userId,
                         districtIds,
                         searchStr,
                         channelId,
                         rankId,
                         lineId,
                         status,
                         lat, 
                         lng, 
                         range,
                         pageIndex,
                         pageSize);

                    var all_districts = _districtService.GetDistrictByIds(store ?? 0, terminalList.Select(s => s.DistrictId).ToArray());
                    var all_dchannels = _channelService.GetChannelsByIds(store ?? 0, terminalList.Select(s => s.ChannelId).ToArray());
                    var all_dranks = _rankService.GetRanksByIds(store ?? 0, terminalList.Select(s => s.RankId).ToArray());
                    var all_dlines = _lineTierService.GetLineTiersByIds(store ?? 0, terminalList.Select(s => s.LineId).ToArray());

                    var result = terminalList.Select(t =>
                    {
                        var model = t.ToModel<TerminalModel>();

                        model.DistrictName = all_districts?.Where(s => s.Id == t.DistrictId).FirstOrDefault()?.Name;
                        model.ChannelName = all_dchannels?.Where(s => s.Id == t.ChannelId).FirstOrDefault()?.Name;
                        model.LineName = all_dlines?.Where(s => s.Id == t.LineId).FirstOrDefault()?.Name;
                        model.RankName = all_dranks?.Where(s => s.Id == t.RankId).FirstOrDefault()?.Name;

                        if (lat > 0 && lng > 0)
                        {
                            model.Distance = CalcDistance(t, lat, lng);
                        }

                        return model;

                    }).ToList();

                    if (lat > 0 && lng > 0)
                    {
                        result = result.OrderBy(s => s.Distance).ToList();
                    }

                    var pgs = new PagedList<TerminalModel>(result, pageIndex, terminalList?.TotalPages ?? 0, terminalList?.TotalCount ?? 0);

                    return this.Successful8("", pgs);
                }
                catch (Exception ex)
                {
                    return new APIResult<IPagedList<TerminalModel>>() { Message = ex.Message };
                }
            });
        }


        private double CalcDistance(Terminal t, double? latitude, double? longitude)
        {
            try
            {
                double radLat1 = (latitude ?? 0) * Math.PI / 180d;
                double radLng1 = (longitude ?? 0) * Math.PI / 180d;
                double radLat2 = (t.Location_Lat) * Math.PI / 180d;
                double radLng2 = (t.Location_Lng) * Math.PI / 180d;
                double a = radLat1 - radLat2;
                double b = radLng1 - radLng2;
                double result = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) + Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Pow(Math.Sin(b / 2), 2))) * 6378137;
                return result;
            }
#pragma warning disable CS0168 // 声明了变量“ex”，但从未使用过
            catch (Exception ex)
#pragma warning restore CS0168 // 声明了变量“ex”，但从未使用过
            {
                return 0;
            }
        }

        /// <summary>
        /// 获取经销商
        /// </summary>
        /// <param name="store"></param>
        /// <param name="terminalId"></param>
        /// <returns></returns>
        [HttpGet("terminal/{store}")]
        [SwaggerOperation("getTerminal")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<TerminalModel>> GetTerminal(int? store, int terminalId = 0)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error3<TerminalModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var t = _terminalService.FindTerminalById(store, terminalId);

                    var allDistricts = _districtService.GetDistrictByIds(store, new int[] { t?.DistrictId ?? 0 });
                    var allChannels = _channelService.GetChannelsByIds(store ?? 0, new int[] { t?.ChannelId ?? 0 });
                    var allRanks = _rankService.GetRanksByIds(store, new int[] { t?.RankId ?? 0 });
                    var allLines = _lineTierService.GetLineTiersByIds(store, new int[] { t?.LineId ?? 0 });

                    var district = allDistricts.Where(ad => ad.Id == (t?.DistrictId ?? 0)).FirstOrDefault();
                    var channel = allChannels.Where(ac => ac.Id == (t?.ChannelId ?? 0)).FirstOrDefault();
                    var rank = allRanks.Where(ar => ar.Id == (t?.RankId ?? 0)).FirstOrDefault();
                    var line = allLines.Where(al => al.Id == (t?.LineId ?? 0)).FirstOrDefault();

                    var model = t?.ToModel<TerminalModel>() ?? new TerminalModel();
                    model.DistrictName = district == null ? "" : district.Name;//片区
                    model.ChannelName = channel == null ? "" : channel.Name;//渠道
                    model.LineName = line == null ? "" : line.Name;//线路
                    model.RankName = rank == null ? "" : rank.Name;//客户等级

                    return this.Successful3("", model);
                }
                catch (Exception ex)
                {
                    return this.Error3<TerminalModel>(ex.Message);
                }
            });
        }


        /// <summary>
        /// 检查经销商
        /// </summary>
        /// <param name="store"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpGet("terminal/checkTerminal/{store}")]
        [SwaggerOperation("checkTerminal")]
        public async Task<APIResult<dynamic>> CheckTerminal(int? store, string name)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var model = _terminalService.CheckTerminal(store, name);
                    return this.Successful(Resources.Successful, model);
                }
                catch (Exception ex)
                {
                    return this.Error(ex.Message);
                }
            });
        }

        /// <summary>
        /// 获取经销商片区
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        [HttpGet("terminal/getAllDistricts/{store}/{userId}")]
        [SwaggerOperation("getAllDistricts")]
        public async Task<APIResult<IList<DistrictModel>>> GetAllDistricts(int? store,int? userId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error2<DistrictModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    //var districts = _districtService.GetAllDistrictByStoreId(store ?? 0);
                    var districts = _districtService.GetAllDistrictByStoreId(store ?? 0, userId ?? 0).ToList();
                    var praent_lst = new List<District>();
                    //获取父级片区
                    foreach (var item in districts)
                    {
                        if (item.ParentId != 0) 
                        {
                            var lst = _districtService.GetParentDistrict(store ?? 0, item.Id).ToList();
                            praent_lst.AddRange(lst);
                        }
                    }
                    districts.AddRange(praent_lst);
                    var result = districts.Distinct()?.Select(t =>
                    {
                        return t.ToModel<DistrictModel>();
                    })?.ToList();

                    return this.Successful2("", result);
                }
                catch (Exception ex)
                {
                    return this.Error2<DistrictModel>(ex.Message);
                }
            });
        }

        /// <summary>
        /// 创建经销商终端客户信息
        /// </summary>
        /// <param name="model"></param>
        /// <param name="store"></param>
        /// <returns></returns>
        [HttpPost("terminal/createTerminal/{store}/{userId}")]
        [SwaggerOperation("createTerminal")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> CreateTerminal(TerminalModel model, int? store, int? userId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    if (model != null)
                    {
                        var results = new BaseResult() { Success = true, Message = "添加成功" };
                        var terminal = new Terminal();

                        if (Regex.IsMatch(model.BossCall, @"^1(3|4|5|7|8|9)\d{9}$") == false)
                        {
                            this.Warning("号码格式错误.");
                        }

                        if (model.Location_Lng == null || model.Location_Lat == null)
                        {
                            model.Location_Lng = 0;//zsh
                            model.Location_Lat = 0;//zsh
                        }

                        if (model.Id > 0)
                        {
                            terminal = _terminalService.GetTerminalById(store, model.Id);
                            terminal = model.ToEntity(terminal);
                            terminal.CreatedOnUtc = DateTime.Now;
                            terminal.StoreId = store ?? 0;
                            terminal.CreatedUserId = userId ?? 0;
                            terminal.MnemonicName = "";
                            _terminalService.UpdateTerminal(terminal);
                        }
                        else
                        {
                            var st = _storeService.GetStoreById(store ?? 0);
                            terminal = model.ToEntity<Terminal>();
                            terminal.StoreId = store ?? 0;
                            terminal.MnemonicName = "";
                            terminal.CreatedUserId = userId ?? 0;
                            terminal.CreatedOnUtc = DateTime.Now;
                            terminal.Deleted = false;
                            terminal.IsNewAdd = true;

                            //添加终端表
                            _terminalService.InsertTerminal(terminal, st?.Code);

                            //添加映射
                            var relation = new CRM_RELATION()
                            {
                                TerminalId = terminal.Id,
                                StoreId = terminal.StoreId,
                                CreatedOnUtc = DateTime.Now,
                                //DCMS新增终端编码规则(经销商编码+自增ID)
                                PARTNER1 = $"{st?.Code}{terminal.Id}",
                                //经销商编码
                                PARTNER2 = st?.Code,
                                RELTYP = "",
                                ZUPDMODE = "",
                                ZDATE = DateTime.Now,
                            };
                            _terminalService.InsertRelation(relation);

                        }

                        return this.Successful(Resources.Successful, results);
                    }
                    else
                    {
                        return this.Error("添加失败");
                    }
                }
                catch (Exception ex)
                {
                    return this.Error(ex.Message);
                }
            });
        }

        /// <summary>
        /// 获取终端账户余额 
        /// </summary>
        /// <param name="terminalId"></param>
        /// <returns></returns>
        [HttpGet("terminal/getTerminalBalance/{store}")]
        [SwaggerOperation("getTerminalBalance")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> GetTerminalBalance(int? store, int terminalId = 0)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var terminalBalance = _commonBillService.CalcTerminalBalance(store ?? 0, terminalId);
                    return this.Successful("", terminalBalance);
                }
                catch (Exception ex)
                {
                    return this.Error(ex.Message);
                }
            });
        }


        /// <summary>
        /// 更新终端位置
        /// </summary>
        /// <param name="store"></param>
        /// <param name="terminalId"></param>
        /// <param name="location_lat"></param>
        /// <param name="location_lng"></param>
        /// <returns></returns>
        [HttpPost("terminal/UpdateTerminal/{store}")]
        [SwaggerOperation("updateterminal")]
        public async Task<APIResult<dynamic>> UpdateTerminal(int? store, int? terminalId, double location_lat, double location_lng)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    if (terminalId != null)
                    {
                        var results = new BaseResult() { Success = true, Message = "更新成功" };
                        var terminal = _terminalService.GetTerminalById(store, terminalId ?? 0);
                        terminal.Location_Lat = location_lat;
                        terminal.Location_Lng = location_lng;
                        _terminalService.UpdateTerminal(terminal);
                        return this.Successful(Resources.Successful, results);
                    }
                    else
                    {
                        return this.Error("更新失败");
                    }
                }
                catch (Exception ex)
                {
                    return this.Error(ex.Message);
                }
            });
        }
    }
}