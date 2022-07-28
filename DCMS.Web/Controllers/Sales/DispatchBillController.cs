using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.Sales;
using DCMS.Core.Domain.Terminals;
using DCMS.Core.Domain.Users;
using DCMS.Core.Domain.WareHouses;
using DCMS.Services.Common;
using DCMS.Services.Configuration;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Products;
using DCMS.Services.Sales;
using DCMS.Services.Settings;
using DCMS.Services.Terminals;
using DCMS.Services.Users;
using DCMS.Services.WareHouses;
using DCMS.ViewModel.Models.Sales;
using DCMS.Web.Framework.Extensions;
using DCMS.Web.Framework.Mvc.Filters;
using DCMS.Web.Infrastructure.Mapper.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;



namespace DCMS.Web.Controllers
{
    /// <summary>
    /// 装车调度
    /// </summary>
    public class DispatchBillController : BasePublicController
    {
        private readonly IPrintTemplateService _printTemplateService;
        private readonly IUserActivityService _userActivityService;
        private readonly IUserService _userService;
        private readonly IProductService _productService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IDistrictService _districtService;
        private readonly ITerminalService _terminalService;
        private readonly IChannelService _channelService;
        private readonly IRankService _rankService;
        private readonly IWareHouseService _wareHouseService;
        private readonly IDispatchBillService _dispatchBillService;
        private readonly ISaleReservationBillService _saleReservationBillService;
        private readonly ISaleBillService _saleBillService;
        private readonly IReturnReservationBillService _returnReservationBillService;
        private readonly IReturnBillService _returnBillService;
        private readonly IMediaService _mediaService;
        private readonly IRedLocker _locker;
        private readonly IExchangeBillService _exchangeBillService;
        private readonly IAccountingService _accountingService;
        private readonly ISettingService _settingService;

        public DispatchBillController(
            IWorkContext workContext,
            IStoreContext storeContext,
            ILogger loggerService,
            IPrintTemplateService printTemplateService,
            IUserActivityService userActivityService,
            IUserService userService,
            IProductService productService,
            ISpecificationAttributeService specificationAttributeService,
            IDistrictService districtService,
            ITerminalService terminalService,
            IChannelService channelService,
            IRankService rankService,
            IWareHouseService wareHouseService,
            IDispatchBillService dispatchBillService,
            ISaleReservationBillService saleReservationBillService,
            ISaleBillService saleBillService,
            IReturnReservationBillService returnReservationBillService,
            IReturnBillService returnBillService,
            IMediaService mediaService,
            IAccountingService accountingService,
            INotificationService notificationService,
            IExchangeBillService exchangeBillService,
            ISettingService settingService,
            IRedLocker locker
            ) : base(workContext, loggerService, storeContext, notificationService)
        {
            _printTemplateService = printTemplateService;
            _userActivityService = userActivityService;
            _userService = userService;
            _productService = productService;
            _specificationAttributeService = specificationAttributeService;
            _districtService = districtService;
            _terminalService = terminalService;
            _channelService = channelService;
            _rankService = rankService;
            _wareHouseService = wareHouseService;
            _dispatchBillService = dispatchBillService;
            _saleReservationBillService = saleReservationBillService;
            _saleBillService = saleBillService;
            _returnReservationBillService = returnReservationBillService;
            _returnBillService = returnBillService;
            _mediaService = mediaService;
            _locker = locker;
            _settingService = settingService;
            _accountingService = accountingService;
            _exchangeBillService = exchangeBillService;
        }


        public IActionResult Index()
        {

            return RedirectToAction("List");
        }

        /// <summary>
        /// 装车调度列表
        /// </summary>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.TrackSchedulingView)]
        public IActionResult List()
        {

            var model = new DispatchBillListModel();

            #region 绑定数据源

            model.StartTime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01"));
            model.EndTime = DateTime.Now.AddDays(1);

            //业务员
            model.BusinessUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Salesmans,curUser.Id,true, _userService.IsAdmin(curStore.Id, curUser.Id));
            model.BusinessUserId = null;

            //片区
            model.Districts = BindDistrictSelection(_districtService.BindDistrictList, curStore);
            model.DistrictId = null;

            //送货员
            model.DeliveryUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Delivers, curUser.Id, true, _userService.IsAdmin(curStore.Id, curUser.Id));
            model.DeliveryUserId = null;

            //客户渠道
            model.Channels = BindChanneSelection(_channelService.BindChannelList, curStore);
            model.ChannelId = null;

            //客户等级
            model.Ranks = BindRankSelection(_rankService.BindRankList, curStore);
            model.RankId = null;

            //订单类型
            var billTypeList = new List<SelectListItem>
            {
                //换货单
                new SelectListItem { Text = CommonHelper.GetEnumDescription(BillTypeEnum.ExchangeBill), Value = ((int)BillTypeEnum.ExchangeBill).ToString() },
                //销售订单
                new SelectListItem { Text = CommonHelper.GetEnumDescription(BillTypeEnum.SaleReservationBill), Value = ((int)BillTypeEnum.SaleReservationBill).ToString() },
                //退货订单
                new SelectListItem { Text = CommonHelper.GetEnumDescription(BillTypeEnum.ReturnReservationBill), Value = ((int)BillTypeEnum.ReturnReservationBill).ToString() }
            };
            model.BillTypes = new SelectList(billTypeList, "Value", "Text");
            model.BillTypeId = null;

            #endregion

            return View(model);
        }

        /// <summary>
        /// 获取装车调度单列表（待调度）
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="businessUserId"></param>
        /// <param name="remark"></param>
        /// <param name="districtId"></param>
        /// <param name="dispatchFilter"></param>
        /// <param name="status"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.TrackSchedulingView)]
        public async Task<JsonResult> AsyncPending(DateTime? start = null, DateTime? end = null, int? businessUserId = null, int? districtId = null, int? terminalId = null, string terminalName = "", string billNumber = "", int? deliveryUserId = null, int? channelId = null, int? rankId = null, int? billTypeId = null, bool? showDispatchReserved = null, bool? dispatchStatus = null, int pageIndex = 0, int pageSize = 20)
        {
            return await Task.Run(() =>
            {
                //待调度
                var dispatchItemModels = new List<DispatchItemModel>();

                
                //换货单
                if (billTypeId == null || billTypeId == 0 || billTypeId == (int)BillTypeEnum.ExchangeBill)
                {
                    //换货单
                    var gridDatas = _exchangeBillService.GetExchangeBillToDispatch(curStore?.Id ?? 0, curUser.Id, start, end, businessUserId, districtId, terminalId,
                        billNumber, deliveryUserId, channelId, rankId, billTypeId,
                        showDispatchReserved, dispatchStatus);

                    //将查询的换货单转换装车调度数据
                    if (gridDatas != null && gridDatas.Count > 0)
                    {
                        gridDatas.ToList().ForEach(a =>
                        {
                            var dispatchItemModel = new DispatchItemModel
                            {
                                BillId = a.Id,
                                BillNumber = a.BillNumber,
                                BillTypeId = (int)BillTypeEnum.ExchangeBill,
                                BillTypeName = CommonHelper.GetEnumDescription(BillTypeEnum.ExchangeBill),
                                TransactionDate = a.TransactionDate,
                                BusinessUserId = a.BusinessUserId,
                                TerminalId = a.TerminalId,
                                TerminalName = _terminalService.GetTerminalName(curStore?.Id ?? 0, a.TerminalId),
                                OrderAmount = a.ReceivableAmount,
                                WareHouseId = a.WareHouseId,
                                Remark = a.Remark,
                                DeliveryUserId = a.DeliveryUserId,
                            };

                            //换货商品数量
                            int bigQuantity = 0;
                            int strokeQuantity = 0;
                            int smallQuantity = 0;
                            if (a.Items != null && a.Items.Count > 0)
                            {
                                var allProducts = _productService.GetProductsByIds(curStore.Id, a.Items.Select(pr => pr.ProductId).Distinct().ToArray());

                                a.Items.ToList().ForEach(s =>
                                {
                                    var product = allProducts.Where(ap => ap.Id == s.ProductId).FirstOrDefault();
                                    if (product != null)
                                    {
                                        if (s.UnitId == product?.SmallUnitId)
                                            smallQuantity += s.Quantity;
                                        else if (s.UnitId == product?.StrokeUnitId)
                                            strokeQuantity += s.Quantity;
                                        else if (s.UnitId == product?.BigUnitId)
                                            bigQuantity += s.Quantity;
                                    }
                                });

                                dispatchItemModel.OrderQuantityView = bigQuantity + "大" + strokeQuantity + "中" + smallQuantity + "小";
                            }

                            dispatchItemModels.Add(dispatchItemModel);
                        });
                    }
                }

                //销售订单
                if (billTypeId == null || billTypeId == 0 || billTypeId == (int)BillTypeEnum.SaleReservationBill)
                {
                    //销售订单
                    var gridDatas = _saleReservationBillService.GetSaleReservationBillToDispatch(curStore?.Id ?? 0, curUser.Id, start, end, businessUserId, districtId, terminalId,
                        billNumber, deliveryUserId, channelId, rankId, billTypeId,
                        showDispatchReserved, dispatchStatus);

                    //将查询的销售订单转换装车调度数据
                    if (gridDatas != null && gridDatas.Count > 0)
                    {
                        gridDatas.ToList().ForEach(a =>
                        {
                            DispatchItemModel dispatchItemModel = new DispatchItemModel
                            {
                                BillId = a.Id,
                                BillNumber = a.BillNumber,
                                BillTypeId = (int)BillTypeEnum.SaleReservationBill,
                                BillTypeName = CommonHelper.GetEnumDescription(BillTypeEnum.SaleReservationBill),
                                TransactionDate = a.TransactionDate,
                                BusinessUserId = a.BusinessUserId,
                                TerminalId = a.TerminalId,
                                TerminalName = _terminalService.GetTerminalName(curStore?.Id ?? 0, a.TerminalId),
                                OrderAmount = a.ReceivableAmount,
                                WareHouseId = a.WareHouseId,
                                Remark = a.Remark,
                                DeliveryUserId = a.DeliveryUserId
                            };

                            //销售商品数量
                            int bigQuantity = 0;
                            int strokeQuantity = 0;
                            int smallQuantity = 0;
                            if (a.Items != null && a.Items.Count > 0)
                            {
                                var allProducts = _productService.GetProductsByIds(curStore.Id, a.Items.Select(pr => pr.ProductId).Distinct().ToArray());

                                a.Items.ToList().ForEach(s =>
                                {
                                    var product = allProducts.Where(ap => ap.Id == s.ProductId).FirstOrDefault();
                                    if (product != null)
                                    {
                                        if (s.UnitId == product?.SmallUnitId)
                                            smallQuantity += s.Quantity;
                                        else if (s.UnitId == product?.StrokeUnitId)
                                            strokeQuantity += s.Quantity;
                                        else if (s.UnitId == product?.BigUnitId)
                                            bigQuantity += s.Quantity;
                                    }
                                });
                                dispatchItemModel.OrderQuantityView = bigQuantity + "大" + strokeQuantity + "中" + smallQuantity + "小";
                            }
                            dispatchItemModels.Add(dispatchItemModel);
                        });
                    }
                }

                //退货订单
                if (billTypeId == null || billTypeId == 0 || billTypeId == (int)BillTypeEnum.ReturnReservationBill)
                {
                    //退货订单
                    var gridDatas = _returnReservationBillService.GetReturnReservationBillToDispatch(curStore?.Id ?? 0, curUser.Id, start, end, businessUserId, districtId, terminalId,
                        billNumber, deliveryUserId, channelId, rankId, billTypeId,
                        showDispatchReserved, dispatchStatus);

                    //将查询的销售订单转换装车调度数据
                    if (gridDatas != null && gridDatas.Count > 0)
                    {
                        gridDatas.ToList().ForEach(a =>
                        {
                            DispatchItemModel dispatchItemModel = new DispatchItemModel
                            {
                                BillId = a.Id,
                                BillNumber = a.BillNumber,
                                BillTypeId = (int)BillTypeEnum.ReturnReservationBill,
                                BillTypeName = CommonHelper.GetEnumDescription(BillTypeEnum.ReturnReservationBill),
                                TransactionDate = a.TransactionDate,
                                BusinessUserId = a.BusinessUserId,
                                TerminalId = a.TerminalId,
                                TerminalName = _terminalService.GetTerminalName(curStore?.Id ?? 0, a.TerminalId),
                                OrderAmount = -a.SumAmount,
                                WareHouseId = a.WareHouseId,
                                Remark = a.Remark,
                                DeliveryUserId = a.DeliveryUserId
                            };

                            //销售商品数量
                            int bigQuantity = 0;
                            int strokeQuantity = 0;
                            int smallQuantity = 0;
                            if (a.Items != null && a.Items.Count > 0)
                            {
                                var allProducts = _productService.GetProductsByIds(curStore.Id, a.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                                var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());

                                a.Items.ToList().ForEach(s =>
                                {
                                    var product = allProducts.Where(ap => ap.Id == s.ProductId).FirstOrDefault();
                                    if (product != null)
                                    {
                                        if (s.UnitId == product?.SmallUnitId)
                                            smallQuantity += s.Quantity;
                                        else if (s.UnitId == product?.StrokeUnitId)
                                            strokeQuantity += s.Quantity;
                                        else if (s.UnitId == product?.BigUnitId)
                                            bigQuantity += s.Quantity;
                                    }
                                });
                                dispatchItemModel.OrderQuantityView = bigQuantity + "大" + strokeQuantity + "中" + smallQuantity + "小";
                            }
                            dispatchItemModels.Add(dispatchItemModel);
                        });
                    }
                }

                //分页
                var gridModel = new PagedList<DispatchItemModel>(dispatchItemModels, pageIndex, pageSize);
                var deliveryUsers = _userService.BindUserList(curStore.Id, DCMSDefaults.Delivers);
                return Json(new
                {
                    Success = true,
                    total = gridModel.TotalCount,
                    rows = gridModel.Select(s =>
                    {
                        //业务员名称
                        s.BusinessUserName = _userService.GetUserName(curStore.Id, s.BusinessUserId ?? 0);
                        s.DeliveryUserName = deliveryUsers.Where(d => d.Id == s.DeliveryUserId).FirstOrDefault()?.UserRealName;
                        //客户名称
                        Terminal terminal = _terminalService.GetTerminalById(curStore.Id, s.TerminalId ?? 0);
                        if (terminal != null)
                        {
                            s.TerminalName = terminal.Name;
                            s.TerminalPointCode = terminal.Code;
                            s.TerminalAddress = terminal.Address;
                        }
                        //仓库名称
                        s.WareHouseName = _wareHouseService.GetWareHouseName(curStore.Id, s.WareHouseId);

                        return s;
                    }).OrderByDescending(c=>c.TransactionDate).ToList()
                });
            });
        }

        /// <summary>
        /// 获取装车调度单列表（已调度）
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="businessUserId"></param>
        /// <param name="remark"></param>
        /// <param name="districtId"></param>
        /// <param name="dispatchFilter"></param>
        /// <param name="status"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.TrackSchedulingView)]
        public async Task<JsonResult> AsyncCompleted(DateTime? start = null, DateTime? end = null, int? businessUserId = null, int? districtId = null, int? terminalId = null, string terminalName = "", string billNumber = "", int? deliveryUserId = null, int? channelId = null, int? rankId = null, int? billTypeId = null, bool? showDispatchReserved = null, bool? dispatchStatus = null, int pageIndex = 0, int pageSize = 20)
        {
            return await Task.Run(() =>
            {
                //调度单
                var dispatchBillModels = new List<DispatchBillModel>();

                var gridDatas = _dispatchBillService.GetDispatchBillList(curStore?.Id ?? 0, 
                    curUser.Id, 
                    start, 
                    end, 
                    businessUserId, 
                    districtId, 
                    terminalId,
                    billNumber, 
                    deliveryUserId, 
                    channelId, 
                    rankId, 
                    billTypeId,
                    showDispatchReserved, 
                    dispatchStatus, 
                    pageIndex, 
                    pageSize);

                return Json(new
                {
                    Success = true,
                    total = gridDatas.TotalCount,
                    rows = gridDatas.Select(s =>
                    {
                        var m = s.ToModel<DispatchBillModel>();
                        m.BillTypeId = (int)BillTypeEnum.DispatchBill;
                        m.DeliveryUserName = _userService.GetUserName(curStore.Id, s.DeliveryUserId);
                        m.CarNO = _wareHouseService.GetWareHouseName(curStore.Id, m.CarId);

                        int bigQuantity = 0; //大单位量
                        int smallQuantity = 0; //中单位量
                        int strokeQuantity = 0; //小单位量
                        if (s.Items != null && s.Items.Count > 0)
                        {
                            s.Items.ToList().ForEach(db =>
                            {
                                var dispatchItemModel = new DispatchItemModel();
                                //换货单
                                if (db.BillTypeId == (int)BillTypeEnum.ExchangeBill)
                                {
                                    var bill = _exchangeBillService
                                    .GetExchangeBillById(curStore.Id, db.BillId, true);

                                    if (bill != null)
                                    {
                                        //订单金额
                                        m.OrderAmount += bill.ReceivableAmount;

                                        //换货商品数量
                                        if (bill.Items != null && bill.Items.Count > 0)
                                        {
                                            var allProducts = _productService
                                            .GetProductsByIds(curStore.Id, bill.Items.Select(pr => pr.ProductId)
                                            .Distinct()
                                            .ToArray());

                                            bill.Items.ToList().ForEach(sb =>
                                            {
                                                var product = allProducts
                                                .Where(ap => ap.Id == sb.ProductId)
                                                .FirstOrDefault();

                                                if (product != null)
                                                {
                                                    if (sb.UnitId == product.SmallUnitId)
                                                        smallQuantity += sb.Quantity;
                                                    else if (sb.UnitId == product.StrokeUnitId)
                                                        strokeQuantity += sb.Quantity;
                                                    else if (sb.UnitId == product.BigUnitId)
                                                        bigQuantity += sb.Quantity;
                                                }
                                            });
                                        }
                                    }
                                }
                                //销售订单
                                else if(db.BillTypeId == (int)BillTypeEnum.SaleReservationBill)
                                {
                                    var bill = _saleReservationBillService
                                    .GetSaleReservationBillById(curStore.Id, db.BillId, true);

                                    if (bill != null)
                                    {
                                        //订单金额
                                        m.OrderAmount += bill.ReceivableAmount;

                                        //销售商品数量
                                        if (bill.Items != null && bill.Items.Count > 0)
                                        {
                                            var allProducts = _productService
                                            .GetProductsByIds(curStore.Id, bill.Items.Select(pr => pr.ProductId)
                                            .Distinct()
                                            .ToArray());

                                            bill.Items.ToList().ForEach(sb =>
                                            {
                                                var product = allProducts
                                                .Where(ap => ap.Id == sb.ProductId)
                                                .FirstOrDefault();

                                                if (product != null)
                                                {
                                                    if (sb.UnitId == product.SmallUnitId)
                                                        smallQuantity += sb.Quantity;
                                                    else if (sb.UnitId == product.StrokeUnitId)
                                                        strokeQuantity += sb.Quantity;
                                                    else if (sb.UnitId == product.BigUnitId)
                                                        bigQuantity += sb.Quantity;
                                                }
                                            });
                                        }
                                    }
                                }
                                //退货订单
                                else if (db.BillTypeId == (int)BillTypeEnum.ReturnReservationBill)
                                {
                                    var bill = _returnReservationBillService
                                    .GetReturnReservationBillById(curStore.Id, db.BillId, true);
                                    {
                                        if (bill != null)
                                        {
                                            //订单金额
                                            m.OrderAmount += -bill.ReceivableAmount;

                                            //退货商品数量
                                            if (bill.Items != null && bill.Items.Count > 0)
                                            {
                                                var allProducts = _productService
                                                .GetProductsByIds(curStore.Id, bill.Items.Select(pr => pr.ProductId)
                                                .Distinct()
                                                .ToArray());

                                                bill.Items.ToList().ForEach(sb =>
                                                {
                                                    var product = allProducts
                                                    .Where(ap => ap.Id == sb.ProductId)
                                                    .FirstOrDefault();

                                                    if (product != null)
                                                    {
                                                        if (sb.UnitId == product?.SmallUnitId)
                                                            smallQuantity += sb.Quantity;
                                                        else if (sb.UnitId == product?.StrokeUnitId)
                                                            strokeQuantity += sb.Quantity;
                                                        else if (sb.UnitId == product?.BigUnitId)
                                                            bigQuantity += sb.Quantity;
                                                    }
                                                });
                                            }
                                        }
                                    }
                                }
                            });
                        }
                        m.OrderQuantityView = bigQuantity + "大" + strokeQuantity + "中" + smallQuantity + "小";
                        return m;
                    }).OrderByDescending(c => c.CreatedOnUtc).ToList()
                });

            });
        }


        /// <summary>
        /// 调度弹出窗体
        /// </summary>
        /// <returns></returns>
        //[AuthCode((int)AccessGranularityEnum.TrackSchedulingScheduling)]
        [HttpPost]
        public IActionResult Dispatch(List<DispatchItemModel> datas, int dispatchBillId = 0, bool billStatus = false)
        {
            try
            {
                var model = new DispatchBillModel
                {
                    //送货员
                    DeliveryUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Delivers)
                };
                int da = 0;
                int xiao = 0;
                decimal OrderAmount = 0;
                if (datas.Count > 0 && datas != null)
                {
                    foreach (var data in datas)
                    {
                        da += Int32.Parse(data.OrderQuantityView.Split('大')[0]);
                        xiao += Int32.Parse((data.OrderQuantityView.Split('中')[1]).Split('小')[0]);
                        OrderAmount += data.OrderAmount ?? 0;
                    }
                }
                model.DeliveryUserId = (model.DeliveryUserId);
                model.BillStatus = billStatus;
                model.Id = dispatchBillId;

                //车辆(车仓)
                model.Cars = BindWareHouseByTypeSelection(_wareHouseService.BindWareHouseList, curStore, (int)WareHouseType.Car, null, 0);
                model.CarId = (model.CarId);
                model.OrderQuantityView = da + "大" + xiao + "小";
                model.OrderAmount = OrderAmount;
                //自动生成调拨单
                IEnumerable<DispatchBillAutoAllocationEnum> autoAllocationEnums = Enum.GetValues(typeof(DispatchBillAutoAllocationEnum)).Cast<DispatchBillAutoAllocationEnum>();
                var autoAllocationModes = from a in autoAllocationEnums
                                          select new SelectListItem
                                          {
                                              Text = CommonHelper.GetEnumDescription(a),
                                              Value = ((int)a).ToString()
                                          };
                model.DispatchBillAutoAllocationBills = new SelectList(autoAllocationModes, "Value", "Text");
                model.DispatchBillAutoAllocationBill = (int)DispatchBillAutoAllocationEnum.Auto;  //修改为自动生成调拨单 ---duzhuang 2021-1-16 11:00
                //打印
                IEnumerable<DispatchBillCreatePrintEnum> createPrintEnums = Enum.GetValues(typeof(DispatchBillCreatePrintEnum)).Cast<DispatchBillCreatePrintEnum>();
                var createPrintModes = from a in createPrintEnums
                                       select new SelectListItem
                                       {
                                           Text = CommonHelper.GetEnumDescription(a),
                                           Value = ((int)a).ToString()
                                       };
                model.DispatchBillCreatePrints = new SelectList(createPrintModes, "Value", "Text");

                model.SelectDatas = JsonConvert.SerializeObject(datas);
                return Json(new
                {
                    Success = true,
                    RenderHtml = RenderPartialViewToString("AsyncDispatch", model)
                });
            }
            catch(Exception ex)
            {
                return Json(new
                {
                    Success = true,
                    ex.Message
                });
            }
        }


        /// <summary>
        /// 添加装车调度单保存
        /// </summary>
        /// <param name="model"></param>
        /// <param name="continueEditing"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.TrackSchedulingScheduling)]
        [HttpPost]
        public async Task<JsonResult> CreateBill(List<DispatchItem> selectDatas, [FromQuery] int deliveryId, [FromQuery]int carId, [FromQuery]int autoAllocationBill)
        {
            return await Task.Run(async () =>
            {
                try
                {
                    string errMsg = string.Empty;

                    #region 验证
                    if (deliveryId == 0)
                    {
                        errMsg += "没有选择送货员";
                    }
                    else
                    {
                        if (carId == 0)
                        {
                            errMsg += "没有选择车辆";
                        }
                        else
                        {
                            if (selectDatas?.Count == 0)
                            {
                                errMsg += "没有选择需要调拨的单据";
                            }
                            else
                            {

                                selectDatas.ForEach(a =>
                                {
                                    //int billType = int.Parse(a.Split('_')[0]);
                                    //int billId = int.Parse(a.Split('_')[1]);

                                    //换货单
                                    if (a.BillTypeId == (int)BillTypeEnum.ExchangeBill)
                                    {
                                        var bill = _exchangeBillService
                                        .GetExchangeBillById(curStore.Id, a.BillId);

                                        if (bill == null)
                                        {
                                            errMsg += "单据Id为：" + a.BillId + "的换货单不存在";
                                        }
                                        else if (bill.ReversedStatus)
                                        {
                                            errMsg += "单据Id为：" + a.BillId + "的换货单已经红冲";
                                        }
                                        else if (bill.DispatchedStatus)
                                        {
                                            errMsg += "单据Id为：" + a.BillId + "的换货单已经调度";
                                        }
                                        else if (bill.ChangedStatus)
                                        {
                                            errMsg += "单据Id为：" + a.BillId + "的换货单已经转为销售单";
                                        }
                                    }
                                    //销售订单
                                    else if (a.BillTypeId == (int)BillTypeEnum.SaleReservationBill)
                                    {
                                        var bill = _saleReservationBillService
                                        .GetSaleReservationBillById(curStore.Id, a.BillId);

                                        if (bill == null)
                                        {
                                            errMsg += "单据Id为：" + a.BillId + "的销售订单不存在";
                                        }
                                        else if (bill.ReversedStatus)
                                        {
                                            errMsg += "单据Id为：" + a.BillId + "的销售订单已经红冲";
                                        }
                                        else if (bill.DispatchedStatus)
                                        {
                                            errMsg += "单据Id为：" + a.BillId + "的销售订单已经调度";
                                        }
                                        else if (bill.ChangedStatus)
                                        {
                                            errMsg += "单据Id为：" + a.BillId + "的销售订单已经转为销售单";
                                        }
                                    }
                                    //退货订单
                                    else if (a.BillTypeId == (int)BillTypeEnum.ReturnReservationBill)
                                    {
                                        var bill = _returnReservationBillService
                                        .GetReturnReservationBillById(curStore.Id, a.BillId);

                                        if (bill == null)
                                        {
                                            errMsg += "单据Id为：" + a.BillId + "的退货订单不存在";
                                        }
                                        else if (bill.ReversedStatus)
                                        {
                                            errMsg += "单据Id为：" + a.BillId + "的退货订单已经红冲";
                                        }
                                        else if (bill.DispatchedStatus)
                                        {
                                            errMsg += "单据Id为：" + a.BillId + "的退货订单已经调度";
                                        }
                                        else if (bill.ChangedStatus)
                                        {
                                            errMsg += "单据Id为：" + a.BillId + "的退货订单已经转为退货单";
                                        }
                                    }
                                });
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(errMsg))
                    {
                        return Warning(errMsg);
                    }

                    #endregion

                    DispatchBillModel model = new DispatchBillModel();

                    #region 绑定数据源
                    //送货员
                    model.DeliveryUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Delivers, curUser.Id, true, _userService.IsAdmin(curStore.Id, curUser.Id));
                    model.DeliveryUserId = (model.DeliveryUserId);

                    //车辆(车仓)
                    model.Cars = BindWareHouseByTypeSelection(_wareHouseService.BindWareHouseList, curStore, (int)WareHouseType.Car, null, 0);
                    model.CarId = (model.CarId);

                    //自动生成调拨单
                    IEnumerable<DispatchBillAutoAllocationEnum> autoAllocationEnums = Enum.GetValues(typeof(DispatchBillAutoAllocationEnum)).Cast<DispatchBillAutoAllocationEnum>();
                    var autoAllocationModes = from a in autoAllocationEnums
                                              select new SelectListItem
                                              {
                                                  Text = CommonHelper.GetEnumDescription(a),
                                                  Value = ((int)a).ToString()
                                              };
                    model.DispatchBillAutoAllocationBills = new SelectList(autoAllocationModes, "Value", "Text");
                    model.DispatchBillAutoAllocationBill = (int)DispatchBillAutoAllocationEnum.UnAuto;

                    //打印
                    IEnumerable<DispatchBillCreatePrintEnum> createPrintEnums = Enum.GetValues(typeof(DispatchBillCreatePrintEnum)).Cast<DispatchBillCreatePrintEnum>();
                    var createPrintModes = from a in createPrintEnums
                                           select new SelectListItem
                                           {
                                               Text = CommonHelper.GetEnumDescription(a),
                                               Value = ((int)a).ToString()
                                           };
                    model.DispatchBillCreatePrints = new SelectList(createPrintModes, "Value", "Text");

                    #endregion

                    //调度单Id
                    int dispatchBillId = 0;

                    string allocationBillNumbers = string.Empty;

                    //RedLock
                    string lockKey = string.Format(DCMSCachingDefaults.RedisDataReSubmitKey,
                        Request.GetUrl(),
                        curStore.Id,
                        curUser.Id, CommonHelper.MD5(string.Join("_", selectDatas.Select(s => s.BillId))));

                    var result = await _locker.PerformActionWithLockAsync(lockKey,
                          TimeSpan.FromSeconds(30),
                          TimeSpan.FromSeconds(10),
                          TimeSpan.FromSeconds(1),
                          () => _dispatchBillService.CreateBill(curStore.Id,
                          curUser.Id,
                          model.ToEntity<DispatchBill>(),
                          deliveryId,
                          carId,
                          selectDatas,
                          autoAllocationBill,
                          (int)OperationEnum.PC,
                          out allocationBillNumbers,
                          out dispatchBillId));

                    if (result.Success)
                    {
                        return Successful("调度成功 " + (string.IsNullOrEmpty(allocationBillNumbers) ? "" : "调拨单号：" + allocationBillNumbers), dispatchBillId);
                    }
                    else
                    {
                        return Successful(result.Message, result);
                    }
                }
                catch (Exception ex)
                {
                    //活动日志
                    _userActivityService.InsertActivity("CreateOrUpdate", Resources.Bill_CreateOrUpdateFailed, curUser.Id);
                    _notificationService.ErrorNotification(Resources.Bill_CreateOrUpdateFailed);
                    return Error(ex.Message);
                }
            });
        }

        /// <summary>
        /// 冲改
        /// </summary>
        /// <param name="dispatchBillId"></param>
        /// <param name="deliveryId">送货员</param>
        /// <param name="carId">送货车辆</param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.TrackSchedulingScheduling)]
        public async Task<JsonResult> UpdateBill(int? id, int deliveryId, int carId)
        {
            try
            {
                string errMsg = string.Empty;
                var bill = new DispatchBill();
                #region 验证
                if (id == null)
                {
                    errMsg += "单据编号不存在";
                }
                else
                {
                    bill = _dispatchBillService.GetDispatchBillById(curStore.Id, id.Value);
                    if (bill == null)
                    {
                        errMsg += "单据信息不存在";
                    }
                    else
                    {
                        if (bill.StoreId != curStore.Id)
                        {
                            errMsg += "只能冲改自己单据";
                        }
                        if (bill.ReversedStatus != false)
                        {
                            errMsg += "单据状态必须为未红冲";
                        }

                        if (deliveryId == 0)
                        {
                            errMsg += "没有选择送货员";
                        }
                        if (carId == 0)
                        {
                            errMsg += "没有选择车辆";
                        }

                        //***已转单据不能冲改和红冲
                        if (bill.Items != null && bill.Items.Count > 0)
                        {
                            bill.Items.ToList().ForEach(di =>
                            {
                                if (di.BillTypeId == (int)BillTypeEnum.SaleReservationBill)
                                {
                                    SaleBill saleBill = _saleBillService.GetSaleBillBySaleReservationBillId(curStore.Id, di.BillId);
                                    if (saleBill != null)
                                    {
                                        errMsg += "已有订单被转成销售单或退货单，不允许红冲或冲改。";
                                    }
                                }
                                if (di.BillTypeId == (int)BillTypeEnum.ReturnReservationBill)
                                {
                                    ReturnBill returnBill = _returnBillService.GetReturnBillByReturnReservationBillId(curStore.Id, di.BillId);
                                    if (returnBill != null)
                                    {
                                        errMsg += "已有订单被转成销售单或退货单，不允许红冲或冲改。";
                                    }
                                }

                                //存在签收不能红冲，修改
                                if (di.SignStatus == (int)SignStatusEnum.Done)
                                {
                                    errMsg += $"调度单存在已签收单据，不允许红冲或冲改。";
                                }

                            });
                        }

                    }
                }
                if (!string.IsNullOrEmpty(errMsg))
                {
                    return Warning(errMsg);
                }
                #endregion

                //Redis事务锁(防止重复冲改)

                var result = await _locker.PerformActionWithLockAsync(RedLockKey(bill),
                      TimeSpan.FromSeconds(30),
                      TimeSpan.FromSeconds(10),
                      TimeSpan.FromSeconds(1),
                      () => _dispatchBillService.UpdateBill(curStore.Id, curUser.Id, bill, id, deliveryId, carId));
                return Json(result);

            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("Reverse", "单据冲改失败", curUser.Id);
                _notificationService.SuccessNotification("单据冲改失败");
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// 红冲
        /// </summary>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.TrackSchedulingReverse)]
        public async Task<JsonResult> Reverse(int? id)
        {
            try
            {

                var bill = new DispatchBill() { StoreId = curStore?.Id ?? 0 };

                #region 验证


                if (PeriodClosed(DateTime.Now))
                {
                    return Warning("系统当月已经结转，不允许红冲");
                }

                if (!id.HasValue)
                {
                    return Warning("参数错误.");
                }
                else
                {
                    bill = _dispatchBillService.GetDispatchBillById(curStore.Id, id.Value);
                    if (bill.ReversedStatus)
                    {
                        return Warning("单据已红冲，请刷新页面.");
                    }
                }

                if (bill == null)
                {
                    return Warning("单据信息不存在.");
                }

                if (bill.StoreId != curStore.Id)
                {
                    return Warning("非法操作.");
                }

                if (!bill.AuditedStatus || bill.ReversedStatus)
                {
                    return Warning("非法操作，单据未审核或者重复操作.");
                };

                if (bill.Items == null || !bill.Items.Any())
                {
                    return Warning("单据没有明细.");
                }

                if (DateTime.Now.Subtract(bill.AuditedDate ?? DateTime.Now).TotalSeconds > 86400)
                {
                    return Warning("已经审核的单据超过24小时，不允许红冲.");
                }

                //***已转单据不能冲改和红冲
                if (bill.Items.Any())
                {

                    foreach (var di in bill.Items)
                    {
                        if (di.BillTypeId == (int)BillTypeEnum.SaleReservationBill)
                        {
                            if (_saleBillService.Exists(di.BillId))
                            {
                                return Warning("已有订单被转成销售单或退货单，不允许红冲或冲改.");
                            }
                        }

                        if (di.BillTypeId == (int)BillTypeEnum.ReturnReservationBill)
                        {
                            if (_returnBillService.Exists(di.BillId))
                            {
                                return Warning("已有订单被转成销售单或退货单，不允许红冲或冲改.");
                            }
                        }

                        if (di.SignStatus == (int)SignStatusEnum.Done)
                        {
                            return Warning("调度单存在已签收单据，不允许红冲或冲改.");
                        }
                    }
                }

                #endregion

                //RedLock

                var result = await _locker.PerformActionWithLockAsync(RedLockKey(bill),
                      TimeSpan.FromSeconds(30),
                      TimeSpan.FromSeconds(10),
                      TimeSpan.FromSeconds(1),
                      () => _dispatchBillService.Reverse(curUser.Id, bill));
                return Json(result);

            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("Reverse", "单据红冲失败", curUser.Id);
                _notificationService.SuccessNotification("单据红冲失败");
                return Error(ex.Message);
            }
        }


        [AuthCode((int)AccessGranularityEnum.TrackSchedulingPrint)]
        public JsonResult PrintSetting()
        {
            var printTemplates = _printTemplateService.GetAllPrintTemplates(curStore.Id).ToList();
            var printTemplate = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.SaleBill).FirstOrDefault();
            //获取打印设置
            var pCPrintSetting = _settingService.LoadSetting<PCPrintSetting>(_storeContext.CurrentStore.Id);
            var settings = new object();
            if (pCPrintSetting != null)
            {
                settings = new
                {
                    PaperWidth = (printTemplate?.PaperWidth == 0 || printTemplate?.PaperHeight == 0) ? pCPrintSetting.PaperWidth : printTemplate.PaperWidth,
                    PaperHeight = (printTemplate?.PaperWidth == 0 || printTemplate?.PaperHeight == 0) ? pCPrintSetting.PaperHeight : printTemplate.PaperHeight,
                    BorderType = pCPrintSetting.BorderType,
                    MarginTop = pCPrintSetting.MarginTop,
                    MarginBottom = pCPrintSetting.MarginBottom,
                    MarginLeft = pCPrintSetting.MarginLeft,
                    MarginRight = pCPrintSetting.MarginRight,
                    IsPrintPageNumber = pCPrintSetting.IsPrintPageNumber,
                    PrintHeader = pCPrintSetting.PrintHeader,
                    PrintFooter = pCPrintSetting.PrintFooter,
                    FixedRowNumber = pCPrintSetting.FixedRowNumber,
                    PrintSubtotal = pCPrintSetting.PrintSubtotal,
                    PrintPort = pCPrintSetting.PrintPort
                };
                return Successful("", settings);
            }
            return Successful("", null);

        }
        /// <summary>
        /// 单据打印
        /// </summary>
        /// <param name="pickingFilter"></param>
        /// <param name="wholeScrapStatus"></param>
        /// <param name="scrapStatus"></param>
        /// <param name="selectData"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.TrackSchedulingPrint)]
        public JsonResult BillPrint(string printTypes, int dispatchBillId = 0)
        {
            //验证单据
            try
            {
                bool fg = true;
                string errMsg = string.Empty;

                List<string> lst = new List<string>();
                var dispatchBill = _dispatchBillService.GetDispatchBillById(curStore.Id, dispatchBillId);
                //获取打印模板
                var printTemplates = _printTemplateService.GetAllPrintTemplates(curStore.Id).ToList();
                var datas = new List<string>();

                if (dispatchBill != null)
                {
                    if (dispatchBill.StoreId != curStore.Id)
                    {
                        errMsg += (errMsg != "" ? "," : "") + "单据:" + dispatchBill.BillNumber + "不为当前经销商";
                    }
                    else
                    {
                        if (dispatchBill.Items == null && dispatchBill.Items.Count == 0)
                        {
                            errMsg += (errMsg != "" ? "," : "") + "单据:" + dispatchBill.BillNumber + "没明细";
                        }
                    }
                }
                else
                {
                    errMsg += (errMsg != "" ? "," : "") + "装车调度单不存在";
                }

                List<string> printTypeList = new List<string>();
                if (string.IsNullOrEmpty(printTypes))
                {
                    errMsg += (errMsg != "" ? "," : "") + "没有选择打印类型";
                }
                else
                {
                    printTypeList = printTypes.Split(',').ToList();
                }

                if (printTemplates == null || printTemplates.Count == 0)
                {
                    errMsg += (errMsg != "" ? "," : "") + "没有可供选择的打印模板";
                }

                if (fg == false)
                {
                    return Warning(errMsg);
                }

                if (!string.IsNullOrEmpty(errMsg))
                {
                    return Warning(errMsg);
                }

                //获取打印设置
                var pCPrintSetting = _settingService.LoadSetting<PCPrintSetting>(_storeContext.CurrentStore.Id);

                #region 修改数据

                _dispatchBillService.BillPrint(curStore.Id, curUser.Id, dispatchBill, printTypeList);

                //车辆
                WareHouse car = _wareHouseService.GetWareHouseById(curStore.Id, dispatchBill.CarId);
                string carNo = "";
                if (car != null)
                {
                    carNo = car.Name;
                }

                foreach (string t in printTypeList)
                {
                    if (t == "1")
                    {
                        List<WareHouse> wareHouses = new List<WareHouse>();
                        List<User> businessUsers = new List<User>();
                        List<Terminal> terminals = new List<Terminal>();
                        List<string> billNums = new List<string>();

                        int bigQuantity = 0;
                        int strokeQuantity = 0;
                        int smallQuantity = 0;
                        List<Tuple<int, string, int, string>> allProducts = new List<Tuple<int, string, int, string>>();

                        foreach (var d in dispatchBill.Items)
                        {
                            if (d.BillTypeId == (int)BillTypeEnum.ExchangeBill)
                            {
                                //换货单列表
                                var ebl = _exchangeBillService.GetExchangeBillById(curStore.Id, d.BillId, true);
                                if (ebl != null)
                                {
                                    var terminal = _terminalService.GetTerminalById(curStore.Id, ebl.TerminalId);
                                    var wareHouse = _wareHouseService.GetWareHouseById(curStore.Id, ebl.WareHouseId);
                                    var businessUser = _userService.GetUserById(curStore.Id, ebl.BusinessUserId);

                                    List<int> productIds = new List<int>();
                                    if (ebl.Items != null && ebl.Items.Count > 0)
                                    {
                                        productIds.AddRange(ebl.Items.Select(it => it.ProductId).Distinct().ToList());
                                    }
                                    //去重
                                    productIds = productIds.Distinct().ToList();

                                    var products = _productService.GetProductsByIds(curStore.Id, productIds.ToArray());
                                    var options = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, products.GetProductBigStrokeSmallUnitIds());

                                    #region theadid
                                    if (terminal != null)
                                    {
                                        terminals.Add(terminal);
                                    }
                                    if (wareHouse != null)
                                    {
                                        wareHouses.Add(wareHouse);
                                    }
                                    if (businessUser != null)
                                    {
                                        businessUsers.Add(businessUser);
                                    }
                                    billNums.Add(ebl.BillNumber);
                                    #endregion

                                    #region tbodyid
                                    //明细
                                    //获取 tbody 中的行
                                    if (ebl.Items != null && ebl.Items.Count > 0)
                                    {
                                        ebl.Items.Select(si => si.ProductId).Distinct().ToList().ForEach(pi =>
                                       {
                                           var items = ebl.Items.Where(it => it.ProductId == pi).ToList();
                                           Product product = products.Where(ap => ap.Id == pi).FirstOrDefault();
                                           if (items != null && items.Count > 0 && product != null)
                                           {
                                               int thisProductSmallQuantity = 0;
                                               string thisProductSmallUnitName = "";
                                               var option = options.Where(sf => sf.Id == product.SmallUnitId).FirstOrDefault();
                                               thisProductSmallUnitName = option == null ? "" : option.Name;
                                               items.ForEach(it =>
                                               {
                                                   if (it.UnitId == product.BigUnitId)
                                                   {
                                                       bigQuantity += it.Quantity;
                                                       thisProductSmallQuantity += it.Quantity * (product.BigQuantity ?? 1);
                                                   }
                                                   else if (it.UnitId == product.StrokeUnitId)
                                                   {
                                                       strokeQuantity += it.Quantity;
                                                       thisProductSmallQuantity += it.Quantity * (product.StrokeQuantity ?? 1);
                                                   }
                                                   else if (it.UnitId == product.SmallUnitId)
                                                   {
                                                       smallQuantity += it.Quantity;
                                                       thisProductSmallQuantity += it.Quantity;
                                                   }
                                               });

                                               Tuple<int, string, int, string> p = new Tuple<int, string, int, string>(product.Id, product.Name, thisProductSmallQuantity, thisProductSmallUnitName);
                                               allProducts.Add(p);
                                           }
                                       });
                                    }
                                    #endregion

                                    #region tfootid
                                    #endregion
                                }
                            }
                            else if (d.BillTypeId == (int)BillTypeEnum.SaleReservationBill)
                            {
                                //销售订单列表
                                var sbl = _saleReservationBillService.GetSaleReservationBillById(curStore.Id, d.BillId, true);
                                if (sbl != null)
                                {
                                    var terminal = _terminalService.GetTerminalById(curStore.Id, sbl.TerminalId);
                                    var wareHouse = _wareHouseService.GetWareHouseById(curStore.Id, sbl.WareHouseId);
                                    var businessUser = _userService.GetUserById(curStore.Id, sbl.BusinessUserId);

                                    List<int> productIds = new List<int>();
                                    if (sbl.Items != null && sbl.Items.Count > 0)
                                    {
                                        productIds.AddRange(sbl.Items.Select(it => it.ProductId).Distinct().ToList());
                                    }
                                    //去重
                                    productIds = productIds.Distinct().ToList();

                                    var products = _productService.GetProductsByIds(curStore.Id, productIds.ToArray());
                                    var options = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, products.GetProductBigStrokeSmallUnitIds());

                                    #region theadid
                                    if (terminal != null)
                                    {
                                        terminals.Add(terminal);
                                    }
                                    if (wareHouse != null)
                                    {
                                        wareHouses.Add(wareHouse);
                                    }
                                    if (businessUser != null)
                                    {
                                        businessUsers.Add(businessUser);
                                    }
                                    billNums.Add(sbl.BillNumber);
                                    #endregion

                                    #region tbodyid
                                    //明细
                                    //获取 tbody 中的行
                                    if (sbl.Items != null && sbl.Items.Count > 0)
                                    {
                                        sbl.Items.Select(si => si.ProductId).Distinct()?.ToList().ForEach(pi =>
                                        {
                                            var items = sbl.Items.Where(it => it.ProductId == pi).ToList();
                                            Product product = products.Where(ap => ap.Id == pi).FirstOrDefault();
                                            if (items != null && items.Count > 0 && product != null)
                                            {
                                                int thisProductSmallQuantity = 0;
                                                string thisProductSmallUnitName = "";
                                                var option = options.Where(sf => sf.Id == product.SmallUnitId).FirstOrDefault();
                                                thisProductSmallUnitName = option == null ? "" : option.Name;
                                                items.ForEach(it =>
                                                {
                                                    if (it.UnitId == product.BigUnitId)
                                                    {
                                                        bigQuantity += it.Quantity;
                                                        thisProductSmallQuantity += it.Quantity * (product.BigQuantity ?? 1);
                                                    }
                                                    else if (it.UnitId == product.StrokeUnitId)
                                                    {
                                                        strokeQuantity += it.Quantity;
                                                        thisProductSmallQuantity += it.Quantity * (product.StrokeQuantity ?? 1);
                                                    }
                                                    else if (it.UnitId == product.SmallUnitId)
                                                    {
                                                        smallQuantity += it.Quantity;
                                                        thisProductSmallQuantity += it.Quantity;
                                                    }
                                                });

                                                Tuple<int, string, int, string> p = new Tuple<int, string, int, string>(product.Id, product.Name, thisProductSmallQuantity, thisProductSmallUnitName);
                                                allProducts.Add(p);
                                            }
                                        });
                                    }
                                    #endregion

                                    #region tfootid
                                    #endregion
                                }
                            }
                            else if (d.BillTypeId == (int)BillTypeEnum.ReturnReservationBill)
                            {
                                //退货订单列表
                                var rrl = _returnReservationBillService.GetReturnReservationBillById(curStore.Id, d.BillId, true);
                                if (rrl != null)
                                {
                                    var terminal = _terminalService.GetTerminalById(curStore.Id, rrl.TerminalId);
                                    var wareHouse = _wareHouseService.GetWareHouseById(curStore.Id, rrl.WareHouseId);
                                    var businessUser = _userService.GetUserById(curStore.Id, rrl.BusinessUserId);

                                    List<int> productIds = new List<int>();
                                    if (rrl.Items != null && rrl.Items.Count > 0)
                                    {
                                        productIds.AddRange(rrl.Items.Select(it => it.ProductId).Distinct().ToList());
                                    }
                                    //去重
                                    productIds = productIds.Distinct().ToList();

                                    var products = _productService.GetProductsByIds(curStore.Id, productIds.ToArray());
                                    var options = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, products.GetProductBigStrokeSmallUnitIds());

                                    #region theadid
                                    if (terminal != null)
                                    {
                                        terminals.Add(terminal);
                                    }
                                    if (wareHouse != null)
                                    {
                                        wareHouses.Add(wareHouse);
                                    }
                                    if (businessUser != null)
                                    {
                                        businessUsers.Add(businessUser);
                                    }
                                    billNums.Add(rrl.BillNumber);
                                    #endregion

                                    #region tbodyid
                                    if (rrl.Items != null && rrl.Items.Count > 0)
                                    {
                                        rrl.Items.Select(si => si.ProductId).Distinct().ToList().ForEach(pi =>
                                        {
                                            var items = rrl.Items.Where(it => it.ProductId == pi).ToList();
                                            Product product = products.Where(ap => ap.Id == pi).FirstOrDefault();
                                            if (items != null && items.Count > 0 && product != null)
                                            {
                                                int thisProductSmallQuantity = 0;
                                                string thisProductSmallUnitName = "";
                                                var option = options.Where(sf => sf.Id == product.SmallUnitId).FirstOrDefault();
                                                thisProductSmallUnitName = option == null ? "" : option.Name;
                                                items.ForEach(it =>
                                                {
                                                    if (it.UnitId == product.BigUnitId)
                                                    {
                                                        bigQuantity += it.Quantity;
                                                        thisProductSmallQuantity += it.Quantity * (product.BigQuantity ?? 1);
                                                    }
                                                    else if (it.UnitId == product.StrokeUnitId)
                                                    {
                                                        strokeQuantity += it.Quantity;
                                                        thisProductSmallQuantity += it.Quantity * (product.StrokeQuantity ?? 1);
                                                    }
                                                    else if (it.UnitId == product.SmallUnitId)
                                                    {
                                                        smallQuantity += it.Quantity;
                                                        thisProductSmallQuantity += it.Quantity;
                                                    }
                                                });

                                                Tuple<int, string, int, string> p = new Tuple<int, string, int, string>(product.Id, product.Name, thisProductSmallQuantity, thisProductSmallUnitName);
                                                allProducts.Add(p);
                                            }
                                        });
                                    }
                                    #endregion

                                    #region tfootid
                                    #endregion
                                }
                            }
                        }

                        var content = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.AllZeroMergerBill).Select(a => a.Content).FirstOrDefault();
                        //填充打印数据
                        StringBuilder sb = new StringBuilder();
                        sb.Append(content);

                        #region theadid
                        sb.Replace("@商铺名称", $@"{curStore.Name}整箱拆零合并单");
                        sb.Replace("@客户名称", string.Join(",", terminals.Distinct().Select(w => w.Name).ToList()));
                        sb.Replace("@仓库", string.Join(",", wareHouses.Distinct().Select(w => w.Name).ToList()));
                        sb.Replace("@车辆", carNo);
                        sb.Replace("@业务员", string.Join(",", businessUsers.Distinct().Select(u => u.UserRealName).ToList()));
                        sb.Replace("@业务电话", string.Join(",", businessUsers.Distinct().Select(u => u.MobileNumber).ToList()));
                        sb.Replace("@订单编号", string.Join(",", billNums.Distinct().ToList()));
                        #endregion

                        #region tbodyid
                        //明细
                        //获取 tbody 中的行
                        int beginTbody = sb.ToString().IndexOf("<tbody>") + 7;
                        int endTbody = sb.ToString().IndexOf("</tbody>");
                        string tbodytr = sb.ToString()[beginTbody..endTbody];

                        if (allProducts != null && allProducts.Count > 0)
                        {
                            //1.先删除明细第一行
                            sb.Remove(beginTbody, endTbody - beginTbody);
                            int i = 0;

                            var ps = allProducts.GroupBy(p => p.Item1).Select(p => new Tuple<int, string, int, string>(p.Key, p.Select(pi => pi.Item2).FirstOrDefault(), p.Sum(pi => pi.Item3), p.Select(pi => pi.Item4).FirstOrDefault())).ToList();

                            ps.ForEach(product =>
                            {
                                int index = sb.ToString().IndexOf("</tbody>");
                                i++;
                                StringBuilder sb2 = new StringBuilder();
                                sb2.Append(tbodytr);
                                sb2.Replace("#序号", i.ToString());
                                sb2.Replace("#商品名称", product.Item2);
                                sb2.Replace("#数量", product.Item3.ToString() + product.Item4);
                                sb.Insert(index, sb2);
                            });

                            sb.Replace("数量(大中小):###", bigQuantity + "大" + strokeQuantity + "中" + smallQuantity + "小");
                        }
                        #endregion

                        #region tfootid
                        #endregion

                        datas.Add(sb.ToString());
                    }
                    if (t == "2")
                    {
                        List<WareHouse> wareHouses = new List<WareHouse>();
                        List<User> businessUsers = new List<User>();
                        List<Terminal> terminals = new List<Terminal>();
                        List<string> billNums = new List<string>();

                        List<Tuple<int, string, int, string>> allProducts = new List<Tuple<int, string, int, string>>();

                        foreach (var d in dispatchBill.Items)
                        {
                            if (d.BillTypeId == (int)BillTypeEnum.ExchangeBill)
                            {
                                //换货单列表
                                var ebl = _exchangeBillService.GetExchangeBillById(curStore.Id, d.BillId, true);
                                if (ebl != null)
                                {
                                    var terminal = _terminalService.GetTerminalById(curStore.Id, ebl.TerminalId);
                                    var wareHouse = _wareHouseService.GetWareHouseById(curStore.Id, ebl.WareHouseId);
                                    var businessUser = _userService.GetUserById(curStore.Id, ebl.BusinessUserId);

                                    List<int> productIds = new List<int>();
                                    if (ebl.Items != null && ebl.Items.Count > 0)
                                    {
                                        productIds.AddRange(ebl.Items.Select(it => it.ProductId).Distinct().ToList());
                                    }
                                    //去重
                                    productIds = productIds.Distinct().ToList();

                                    var products = _productService.GetProductsByIds(curStore.Id, productIds.ToArray());
                                    var options = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, products.GetProductBigStrokeSmallUnitIds());


                                    #region theadid
                                    if (terminal != null)
                                    {
                                        terminals.Add(terminal);
                                    }
                                    if (wareHouse != null)
                                    {
                                        wareHouses.Add(wareHouse);
                                    }
                                    if (businessUser != null)
                                    {
                                        businessUsers.Add(businessUser);
                                    }
                                    billNums.Add(ebl.BillNumber);
                                    #endregion

                                    #region tbodyid
                                    //明细
                                    //获取 tbody 中的行
                                    if (ebl.Items != null && ebl.Items.Count > 0)
                                    {
                                        ebl.Items.Select(si => si.ProductId).Distinct().ToList().ForEach(pi =>
                                        {
                                            var items = ebl.Items.Where(it => it.ProductId == pi).ToList();
                                            Product product = products.Where(ap => ap.Id == pi).FirstOrDefault();
                                            if (items != null && items.Count > 0 && product != null)
                                            {
                                                int thisProductQuantity = 0;
                                                string thisProductUnitName = "";
                                                var option = options.Where(sf => sf.Id == product.BigUnitId).FirstOrDefault();
                                                thisProductUnitName = option == null ? "" : option.Name;
                                                items.ForEach(it =>
                                                {
                                                    if (it.UnitId == product.BigUnitId)
                                                    {
                                                        thisProductQuantity += it.Quantity;
                                                    }
                                                    else if (it.UnitId == product.StrokeUnitId)
                                                    {
                                                        thisProductQuantity += it.Quantity / (product.StrokeQuantity ?? 1);
                                                    }
                                                    else if (it.UnitId == product.SmallUnitId)
                                                    {
                                                        thisProductQuantity += it.Quantity;
                                                    }
                                                });

                                                Tuple<int, string, int, string> p = new Tuple<int, string, int, string>(product.Id, product.Name, thisProductQuantity, thisProductUnitName);
                                                allProducts.Add(p);
                                            }
                                        });
                                    }
                                    #endregion

                                    #region tfootid
                                    #endregion
                                }
                            }
                            else if (d.BillTypeId == (int)BillTypeEnum.SaleReservationBill)
                            {
                                //销售订单列表
                                var sbl = _saleReservationBillService.GetSaleReservationBillById(curStore.Id, d.BillId, true);
                                if (sbl != null)
                                {
                                    var terminal = _terminalService.GetTerminalById(curStore.Id, sbl.TerminalId);
                                    var wareHouse = _wareHouseService.GetWareHouseById(curStore.Id, sbl.WareHouseId);
                                    var businessUser = _userService.GetUserById(curStore.Id, sbl.BusinessUserId);

                                    List<int> productIds = new List<int>();
                                    if (sbl.Items != null && sbl.Items.Count > 0)
                                    {
                                        productIds.AddRange(sbl.Items.Select(it => it.ProductId).Distinct().ToList());
                                    }
                                    //去重
                                    productIds = productIds.Distinct().ToList();

                                    var products = _productService.GetProductsByIds(curStore.Id, productIds.ToArray());
                                    var options = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, products.GetProductBigStrokeSmallUnitIds());

                                    #region theadid
                                    if (terminal != null)
                                    {
                                        terminals.Add(terminal);
                                    }
                                    if (wareHouse != null)
                                    {
                                        wareHouses.Add(wareHouse);
                                    }
                                    if (businessUser != null)
                                    {
                                        businessUsers.Add(businessUser);
                                    }
                                    billNums.Add(sbl.BillNumber);
                                    #endregion

                                    #region tbodyid
                                    //明细
                                    //获取 tbody 中的行
                                    if (sbl.Items != null && sbl.Items.Count > 0)
                                    {
                                        sbl.Items.Select(si => si.ProductId).Distinct()?.ToList().ForEach(pi =>
                                        {
                                            var items = sbl.Items.Where(it => it.ProductId == pi).ToList();
                                            Product product = products.Where(ap => ap.Id == pi).FirstOrDefault();
                                            if (items != null && items.Count > 0 && product != null)
                                            {
                                                int thisQuantity = 0;
                                                string thisProductUnitName = "";
                                                var option = options.Where(sf => sf.Id == product.BigUnitId).FirstOrDefault();
                                                thisProductUnitName = option == null ? "" : option.Name;
                                                items.ForEach(it =>
                                                {
                                                    if (it.UnitId == product.BigUnitId)
                                                    {
                                                        thisQuantity += it.Quantity;
                                                    }
                                                    else if (it.UnitId == product.StrokeUnitId)
                                                    {
                                                        thisQuantity += it.Quantity / (product.StrokeQuantity ?? 1);
                                                    }
                                                    else if (it.UnitId == product.SmallUnitId)
                                                    {
                                                        thisQuantity += it.Quantity;
                                                    }
                                                });

                                                Tuple<int, string, int, string> p = new Tuple<int, string, int, string>(product.Id, product.Name, thisQuantity, thisProductUnitName);
                                                allProducts.Add(p);
                                            }
                                        });
                                    }
                                    #endregion

                                    #region tfootid
                                    #endregion
                                }
                            }
                            else if (d.BillTypeId == (int)BillTypeEnum.ReturnReservationBill)
                            {
                                //退货订单列表
                                var rrl = _returnReservationBillService.GetReturnReservationBillById(curStore.Id, d.BillId, true);
                                if (rrl != null)
                                {
                                    var terminal = _terminalService.GetTerminalById(curStore.Id, rrl.TerminalId);
                                    var wareHouse = _wareHouseService.GetWareHouseById(curStore.Id, rrl.WareHouseId);
                                    var businessUser = _userService.GetUserById(curStore.Id, rrl.BusinessUserId);

                                    List<int> productIds = new List<int>();
                                    if (rrl.Items != null && rrl.Items.Count > 0)
                                    {
                                        productIds.AddRange(rrl.Items.Select(it => it.ProductId).Distinct().ToList());
                                    }
                                    //去重
                                    productIds = productIds.Distinct().ToList();

                                    var products = _productService.GetProductsByIds(curStore.Id, productIds.ToArray());
                                    var options = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, products.GetProductBigStrokeSmallUnitIds());

                                    #region theadid
                                    if (terminal != null)
                                    {
                                        terminals.Add(terminal);
                                    }
                                    if (wareHouse != null)
                                    {
                                        wareHouses.Add(wareHouse);
                                    }
                                    if (businessUser != null)
                                    {
                                        businessUsers.Add(businessUser);
                                    }
                                    billNums.Add(rrl.BillNumber);
                                    #endregion

                                    #region tbodyid
                                    if (rrl.Items != null && rrl.Items.Count > 0)
                                    {
                                        rrl.Items.Select(si => si.ProductId).Distinct().ToList().ForEach(pi =>
                                        {
                                            var items = rrl.Items.Where(it => it.ProductId == pi).ToList();
                                            Product product = products.Where(ap => ap.Id == pi).FirstOrDefault();
                                            if (items != null && items.Count > 0 && product != null)
                                            {
                                                int thisQuantity = 0;
                                                string thisProductUnitName = "";
                                                var option = options.Where(sf => sf.Id == product.BigUnitId).FirstOrDefault();
                                                thisProductUnitName = option == null ? "" : option.Name;
                                                items.ForEach(it =>
                                                {
                                                    if (it.UnitId == product.BigUnitId)
                                                    {
                                                        thisQuantity += it.Quantity;
                                                    }
                                                    else if (it.UnitId == product.StrokeUnitId)
                                                    {
                                                        thisQuantity += it.Quantity / (product.StrokeQuantity ?? 1);
                                                    }
                                                    else if (it.UnitId == product.SmallUnitId)
                                                    {
                                                        thisQuantity += it.Quantity;
                                                    }
                                                });

                                                Tuple<int, string, int, string> p = new Tuple<int, string, int, string>(product.Id, product.Name, thisQuantity, thisProductUnitName);
                                                allProducts.Add(p);
                                            }
                                        });
                                    }
                                    #endregion

                                    #region tfootid
                                    #endregion
                                }
                            }
                        }

                        var content = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.AllLoadBill).Select(a => a.Content).FirstOrDefault();
                        //填充打印数据
                        StringBuilder sb = new StringBuilder();
                        sb.Append(content);

                        #region theadid
                        sb.Replace("@商铺名称", $@"{curStore.Name}整箱装车单");
                        sb.Replace("@客户名称", string.Join(",", terminals.Distinct().Select(u => u.Name).ToList()));
                        sb.Replace("@仓库", string.Join(",", wareHouses.Distinct().Select(w => w.Name).ToList()));
                        sb.Replace("@车辆", carNo);
                        sb.Replace("@业务员", string.Join(",", businessUsers.Distinct().Select(u => u.UserRealName).ToList()));
                        sb.Replace("@业务电话", string.Join(",", businessUsers.Distinct().Select(u => u.MobileNumber).ToList()));
                        sb.Replace("@订单编号", string.Join(",", billNums.Distinct().ToList()));
                        #endregion

                        #region tbodyid
                        //明细
                        //获取 tbody 中的行
                        int beginTbody = sb.ToString().IndexOf("<tbody>") + 7;
                        int endTbody = sb.ToString().IndexOf("</tbody>");
                        string tbodytr = sb.ToString()[beginTbody..endTbody];

                        if (allProducts != null && allProducts.Count > 0)
                        {
                            //1.先删除明细第一行
                            sb.Remove(beginTbody, endTbody - beginTbody);
                            int i = 0;

                            var ps = allProducts.GroupBy(p => p.Item1).Select(p => new Tuple<int, string, int, string>(p.Key, p.Select(pi => pi.Item2).FirstOrDefault(), p.Sum(pi => pi.Item3), p.Select(pi => pi.Item4).FirstOrDefault())).ToList();

                            ps.ForEach(product =>
                            {
                                int index = sb.ToString().IndexOf("</tbody>");
                                i++;
                                StringBuilder sb2 = new StringBuilder();
                                sb2.Append(tbodytr);
                                sb2.Replace("#序号", i.ToString());
                                sb2.Replace("#商品名称", product.Item2);
                                sb2.Replace("#数量", product.Item3.ToString() + product.Item4);
                                sb.Insert(index, sb2);
                            });
                        }
                        #endregion

                        #region tfootid
                        #endregion

                        datas.Add(sb.ToString());
                    }
                    if (t == "3")
                    {
                        foreach (var d in dispatchBill.Items)
                        {
                            if (d.BillTypeId == (int)BillTypeEnum.ExchangeBill)
                            {
                                //换货单列表
                                var ebl = _exchangeBillService.GetExchangeBillById(curStore.Id, d.BillId, true);
                                if (ebl != null)
                                {
                                    List<int> allProductIds = new List<int>();
                                    if (ebl.Items != null && ebl.Items.Count > 0)
                                    {
                                        allProductIds.AddRange(ebl.Items.Select(it => it.ProductId).Distinct().ToList());
                                    }
                                    //去重
                                    allProductIds = allProductIds.Distinct().ToList();

                                    var allProducts = _productService.GetProductsByIds(curStore.Id, allProductIds.ToArray());
                                    var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());


                                    var terminal = _terminalService.GetTerminalById(curStore.Id, ebl.TerminalId);
                                    var wareHouse = _wareHouseService.GetWareHouseById(curStore.Id, ebl.WareHouseId);
                                    var businessUser = _userService.GetUserById(ebl.BusinessUserId);
                                    var content = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.ZeroLoadBill).Select(a => a.Content).FirstOrDefault();
                                    if (terminal != null)
                                    {
                                        //填充打印数据
                                        StringBuilder sb = new StringBuilder();
                                        sb.Append(content);

                                        #region theadid
                                        sb.Replace("@商铺名称", curStore.Name);
                                        if (terminal != null)
                                        {
                                            sb.Replace("@客户名称", terminal.Name);
                                            sb.Replace("@老板姓名", terminal.BossName);
                                        }
                                        if (wareHouse != null)
                                        {
                                            sb.Replace("@仓库", wareHouse.Name);
                                        }
                                        sb.Replace("@车辆", carNo);
                                        if (businessUser != null)
                                        {
                                            sb.Replace("@业务员", businessUser.UserRealName);
                                            sb.Replace("@业务电话", businessUser.MobileNumber);
                                        }
                                        sb.Replace("@订单编号", ebl.BillNumber);

                                        #endregion

                                        #region tbodyid
                                        //明细
                                        //获取 tbody 中的行
                                        int beginTbody = sb.ToString().IndexOf("<tbody>") + 7;
                                        int endTbody = sb.ToString().IndexOf("</tbody>");
                                        string tbodytr = sb.ToString()[beginTbody..endTbody];

                                        if (ebl.Items != null && ebl.Items.Count > 0)
                                        {
                                            //1.先删除明细第一行
                                            sb.Remove(beginTbody, endTbody - beginTbody);
                                            int i = 0;
                                            int bigQuantity = 0;
                                            int strokeQuantity = 0;
                                            int smallQuantity = 0;

                                            ebl.Items.Select(si => si.ProductId).Distinct().ToList().ForEach(pi =>
                                            {
                                                var items = ebl.Items.Where(it => it.ProductId == pi).ToList();
                                                Product product = allProducts.Where(ap => ap.Id == pi).FirstOrDefault();
                                                if (items != null && items.Count > 0 && product != null)
                                                {
                                                    int index = sb.ToString().IndexOf("</tbody>");
                                                    i++;
                                                    StringBuilder sb2 = new StringBuilder();
                                                    sb2.Append(tbodytr);

                                                    sb2.Replace("#序号", i.ToString());
                                                    sb2.Replace("#商品名称", product.Name);
                                                    int thisProductSmallQuantity = 0;
                                                    string thisProductSmallUnitName = "";
                                                    var option = allOptions.Where(sf => sf.Id == product.SmallUnitId).FirstOrDefault();
                                                    thisProductSmallUnitName = option == null ? "" : option.Name;
                                                    items.ForEach(it =>
                                                    {
                                                        if (it.UnitId == product.BigUnitId)
                                                        {
                                                            bigQuantity += it.Quantity;
                                                            thisProductSmallQuantity += it.Quantity * (product.BigQuantity ?? 1);
                                                        }
                                                        else if (it.UnitId == product.StrokeUnitId)
                                                        {
                                                            strokeQuantity += it.Quantity;
                                                            thisProductSmallQuantity += it.Quantity * (product.StrokeQuantity ?? 1);
                                                        }
                                                        else if (it.UnitId == product.SmallUnitId)
                                                        {
                                                            smallQuantity += it.Quantity;
                                                            thisProductSmallQuantity += it.Quantity;
                                                        }
                                                    });

                                                    sb2.Replace("#数量", thisProductSmallQuantity.ToString() + thisProductSmallUnitName);
                                                    sb.Insert(index, sb2);
                                                }
                                            });
                                            sb.Replace("数量(大中小):###", bigQuantity + "大" + strokeQuantity + "中" + smallQuantity + "小");
                                        }
                                        #endregion

                                        #region tfootid
                                        #endregion

                                        datas.Add(sb.ToString());
                                    }
                                }
                            }
                            else if (d.BillTypeId == (int)BillTypeEnum.SaleReservationBill)
                            {
                                //销售订单列表
                                var sbl = _saleReservationBillService.GetSaleReservationBillById(curStore.Id,d.BillId,true);
                                if (sbl != null)
                                {
                                    List<int> allProductIds = new List<int>();
                                    if (sbl.Items != null && sbl.Items.Count > 0)
                                    {
                                        allProductIds.AddRange(sbl.Items.Select(it => it.ProductId).Distinct().ToList());
                                    }
                                    //去重
                                    allProductIds = allProductIds.Distinct().ToList();

                                    var allProducts = _productService.GetProductsByIds(curStore.Id, allProductIds.ToArray());
                                    var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());

                                    var terminal = _terminalService.GetTerminalById(curStore.Id, sbl.TerminalId);
                                    var wareHouse = _wareHouseService.GetWareHouseById(curStore.Id, sbl.WareHouseId);
                                    var businessUser = _userService.GetUserById(curStore.Id, sbl.BusinessUserId);
                                    var content = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.ZeroLoadBill).Select(a => a.Content).FirstOrDefault();
                                    if (terminal != null)
                                    {
                                        StringBuilder sb = new StringBuilder();
                                        sb.Append(content);

                                        #region theadid
                                        sb.Replace("@商铺名称", curStore.Name);
                                        if (terminal != null)
                                        {
                                            sb.Replace("@客户名称", terminal.Name);
                                        }
                                        if (wareHouse != null)
                                        {
                                            sb.Replace("@仓库", wareHouse.Name);
                                        }
                                        sb.Replace("@车辆", carNo);
                                        if (businessUser != null)
                                        {
                                            sb.Replace("@业务员", businessUser.UserRealName);
                                            sb.Replace("@业务电话", businessUser.MobileNumber);
                                        }
                                        sb.Replace("@订单编号", sbl.BillNumber);

                                        #endregion

                                        #region tbodyid
                                        //明细
                                        //获取 tbody 中的行
                                        int beginTbody = sb.ToString().IndexOf("<tbody>") + 7;
                                        int endTbody = sb.ToString().IndexOf("</tbody>");
                                        string tbodytr = sb.ToString()[beginTbody..endTbody];

                                        if (sbl.Items != null && sbl.Items.Count > 0)
                                        {
                                            //1.先删除明细第一行
                                            sb.Remove(beginTbody, endTbody - beginTbody);
                                            int i = 0;
                                            int bigQuantity = 0;
                                            int strokeQuantity = 0;
                                            int smallQuantity = 0;

                                            sbl.Items.Select(si => si.ProductId).Distinct().ToList().ForEach(pi =>
                                            {
                                                var items = sbl.Items.Where(it => it.ProductId == pi).ToList();
                                                Product product = allProducts.Where(ap => ap.Id == pi).FirstOrDefault();
                                                if (items != null && items.Count > 0 && product != null)
                                                {
                                                    int index = sb.ToString().IndexOf("</tbody>");
                                                    i++;
                                                    StringBuilder sb2 = new StringBuilder();
                                                    sb2.Append(tbodytr);

                                                    sb2.Replace("#序号", i.ToString());
                                                    sb2.Replace("#商品名称", product.Name);
                                                    int thisProductSmallQuantity = 0;
                                                    string thisProductSmallUnitName = "";
                                                    var option = allOptions.Where(sf => sf.Id == product.SmallUnitId).FirstOrDefault();
                                                    thisProductSmallUnitName = option == null ? "" : option.Name;
                                                    items.ForEach(it =>
                                                    {
                                                        if (it.UnitId == product.BigUnitId)
                                                        {
                                                            bigQuantity += it.Quantity;
                                                            thisProductSmallQuantity += it.Quantity * (product.BigQuantity ?? 1);
                                                        }
                                                        else if (it.UnitId == product.StrokeUnitId)
                                                        {
                                                            strokeQuantity += it.Quantity;
                                                            thisProductSmallQuantity += it.Quantity * (product.StrokeQuantity ?? 1);
                                                        }
                                                        else if (it.UnitId == product.SmallUnitId)
                                                        {
                                                            smallQuantity += it.Quantity;
                                                            thisProductSmallQuantity += it.Quantity;
                                                        }
                                                    });

                                                    sb2.Replace("#数量", thisProductSmallQuantity.ToString() + thisProductSmallUnitName);
                                                    sb.Insert(index, sb2);
                                                }
                                            });
                                            sb.Replace("数量(大中小):###", bigQuantity + "大" + strokeQuantity + "中" + smallQuantity + "小");
                                        }
                                        #endregion

                                        #region tfootid
                                        #endregion

                                        datas.Add(sb.ToString());
                                    }
                                }
                            }
                            else if (d.BillTypeId == (int)BillTypeEnum.ReturnReservationBill)
                            {
                                //退货订单列表
                                var rrl = _returnReservationBillService
                                .GetReturnReservationBillById(curStore.Id,d.BillId, true);
                                if (rrl != null)
                                {
                                    List<int> allProductIds = new List<int>();
                                    if (rrl.Items != null && rrl.Items.Count > 0)
                                    {
                                        allProductIds.AddRange(rrl.Items.Select(it => it.ProductId).Distinct().ToList());
                                    }
                                    //去重
                                    allProductIds = allProductIds.Distinct().ToList();

                                    var allProducts = _productService.GetProductsByIds(curStore.Id, allProductIds.ToArray());
                                    var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());


                                    var terminal = _terminalService.GetTerminalById(curStore.Id, rrl.TerminalId);
                                    var wareHouse = _wareHouseService.GetWareHouseById(curStore.Id, rrl.WareHouseId);
                                    var businessUser = _userService.GetUserById(curStore.Id, rrl.BusinessUserId);

                                    var content = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.ZeroLoadBill).Select(a => a.Content).FirstOrDefault();
                                    if (terminal != null)
                                    {
                                        //填充打印数据
                                        StringBuilder sb = new StringBuilder();
                                        sb.Append(content);

                                        #region theadid
                                        sb.Replace("@商铺名称", curStore.Name);
                                        if (terminal != null)
                                        {
                                            sb.Replace("@客户名称", terminal.Name);
                                        }
                                        if (wareHouse != null)
                                        {
                                            sb.Replace("@仓库", wareHouse.Name);
                                        }
                                        sb.Replace("@车辆", carNo);
                                        if (businessUser != null)
                                        {
                                            sb.Replace("@业务员", businessUser.UserRealName);
                                            sb.Replace("@业务电话", businessUser.MobileNumber);
                                        }
                                        sb.Replace("@订单编号", rrl.BillNumber);

                                        #endregion

                                        #region tbodyid
                                        //明细
                                        //获取 tbody 中的行
                                        int beginTbody = sb.ToString().IndexOf("<tbody>") + 7;
                                        int endTbody = sb.ToString().IndexOf("</tbody>");
                                        string tbodytr = sb.ToString()[beginTbody..endTbody];

                                        if (rrl.Items != null && rrl.Items.Count > 0)
                                        {
                                            //1.先删除明细第一行
                                            sb.Remove(beginTbody, endTbody - beginTbody);
                                            int i = 0;
                                            int bigQuantity = 0;
                                            int strokeQuantity = 0;
                                            int smallQuantity = 0;

                                            rrl.Items.Select(si => si.ProductId).Distinct().ToList().ForEach(pi =>
                                            {
                                                var items = rrl.Items.Where(it => it.ProductId == pi).ToList();
                                                Product product = allProducts.Where(ap => ap.Id == pi).FirstOrDefault();
                                                if (items != null && items.Count > 0 && product != null)
                                                {
                                                    int index = sb.ToString().IndexOf("</tbody>");
                                                    i++;
                                                    StringBuilder sb2 = new StringBuilder();
                                                    sb2.Append(tbodytr);

                                                    sb2.Replace("#序号", i.ToString());
                                                    sb2.Replace("#商品名称", product.Name);
                                                    int thisProductSmallQuantity = 0;
                                                    string thisProductSmallUnitName = "";
                                                    var option = allOptions.Where(sf => sf.Id == product.SmallUnitId).FirstOrDefault();
                                                    thisProductSmallUnitName = option == null ? "" : option.Name;
                                                    items.ForEach(it =>
                                                    {
                                                        if (it.UnitId == product.BigUnitId)
                                                        {
                                                            bigQuantity += it.Quantity;
                                                            thisProductSmallQuantity += it.Quantity * (product.BigQuantity ?? 1);
                                                        }
                                                        else if (it.UnitId == product.StrokeUnitId)
                                                        {
                                                            strokeQuantity += it.Quantity;
                                                            thisProductSmallQuantity += it.Quantity * (product.StrokeQuantity ?? 1);
                                                        }
                                                        else if (it.UnitId == product.SmallUnitId)
                                                        {
                                                            smallQuantity += it.Quantity;
                                                            thisProductSmallQuantity += it.Quantity;
                                                        }
                                                    });

                                                    sb2.Replace("#数量", thisProductSmallQuantity.ToString() + thisProductSmallUnitName);
                                                    sb.Insert(index, sb2);
                                                }
                                            });
                                            sb.Replace("数量(大中小):###", bigQuantity + "大" + strokeQuantity + "中" + smallQuantity + "小");
                                        }
                                        #endregion

                                        #region tfootid
                                        #endregion

                                        datas.Add(sb.ToString());
                                    }
                                }
                            }
                        }
                    }
                    if (t == "4")
                    {
                        foreach (var d in dispatchBill.Items)
                        {
                            if (d.BillTypeId == (int)BillTypeEnum.ExchangeBill)
                            {
                                // 换货单列表
                                var ebl = _exchangeBillService.GetExchangeBillById(curStore.Id,d.BillId,true);
                                if (ebl != null)
                                {
                                    List<int> allProductIds = new List<int>();
                                    if (ebl.Items != null && ebl.Items.Count > 0)
                                    {
                                        allProductIds.AddRange(ebl.Items.Select(it => it.ProductId).Distinct().ToList());
                                    }
                                    //去重
                                    allProductIds = allProductIds.Distinct().ToList();

                                    var allProducts = _productService.GetProductsByIds(curStore.Id, allProductIds.ToArray());
                                    var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());


                                    var terminal = _terminalService.GetTerminalById(curStore.Id, ebl.TerminalId);
                                    var wareHouse = _wareHouseService.GetWareHouseById(curStore.Id, ebl.WareHouseId);
                                    var businessUser = _userService.GetUserById(curStore.Id, ebl.BusinessUserId);
                                    var content = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.ExchangeBill).Select(a => a.Content).FirstOrDefault();
                                    var makeUser = _userService.GetUserById(curStore.Id, ebl.MakeUserId);
                                    //填充打印数据
                                    StringBuilder sb = new StringBuilder();
                                    sb.Append(content);

                                    #region theadid
                                    sb.Replace("@商铺名称", curStore.Name);
                                    if (terminal != null)
                                    {
                                        sb.Replace("@客户名称", terminal.Name);
                                        sb.Replace("@客户电话", terminal.BossCall);
                                        sb.Replace("@客户地址", terminal.Address);
                                    }
                                    sb.Replace("@单据编号", ebl.BillNumber);
                                    if (makeUser != null)
                                    {
                                        sb.Replace("@制单", makeUser.UserRealName);
                                    }

                                    sb.Replace("@日期", ebl.TransactionDate == null ? "" : ((DateTime)ebl.TransactionDate).ToString("yyyy/MM/dd HH:mm:ss"));
                                    if (businessUser != null)
                                    {
                                        sb.Replace("@业务员", businessUser.UserRealName);
                                        sb.Replace("@业务电话", businessUser.MobileNumber);
                                    }
                                    #endregion

                                    #region tbodyid
                                    //明细
                                    //获取 tbody 中的行
                                    int beginTbody = sb.ToString().IndexOf("<tbody>") + 7;
                                    int endTbody = sb.ToString().IndexOf("</tbody>");
                                    string tbodytr = sb.ToString()[beginTbody..endTbody];

                                    if (ebl.Items != null && ebl.Items.Count > 0)
                                    {
                                        //1.先删除明细第一行
                                        sb.Remove(beginTbody, endTbody - beginTbody);
                                        int i = 0;
                                        foreach (var item in ebl.Items)
                                        {
                                            int index = sb.ToString().IndexOf("</tbody>");
                                            i++;
                                            StringBuilder sb2 = new StringBuilder();
                                            sb2.Append(tbodytr);

                                            sb2.Replace("#序号", i.ToString());
                                            var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                                            if (product != null)
                                            {
                                                sb2.Replace("#商品名称", product.Name);
                                                ProductUnitOption productUnitOption = product.GetProductUnit(_specificationAttributeService, _productService);
                                                if (item.UnitId == product.SmallUnitId)
                                                {
                                                    sb2.Replace("#条形码", product.SmallBarCode);
                                                    if (productUnitOption != null && productUnitOption.smallOption != null)
                                                    {
                                                        sb2.Replace("#商品单位", productUnitOption.smallOption.Name);
                                                    }

                                                }
                                                else if (item.UnitId == product.StrokeUnitId)
                                                {
                                                    sb2.Replace("#条形码", product.StrokeBarCode);
                                                    if (productUnitOption != null && productUnitOption.strokOption != null)
                                                    {
                                                        sb2.Replace("#商品单位", productUnitOption.strokOption.Name);
                                                    }
                                                }
                                                else if (item.UnitId == product.BigUnitId)
                                                {
                                                    sb2.Replace("#条形码", product.BigBarCode);
                                                    if (productUnitOption != null && productUnitOption.bigOption != null)
                                                    {
                                                        sb2.Replace("#商品单位", productUnitOption.bigOption.Name);
                                                    }
                                                }

                                                sb2.Replace("#单位换算", product.GetProductUnitConversion(allOptions));

                                            }
                                            sb2.Replace("#数量", item.Quantity.ToString());
                                            sb2.Replace("#价格", item.Price.ToString());
                                            sb2.Replace("#金额", item.Amount.ToString());
                                            sb2.Replace("#备注", item.Remark);

                                            sb.Insert(index, sb2);

                                        }

                                        sb.Replace("数量:###", ebl.Items.Sum(s => s.Quantity).ToString());
                                        sb.Replace("金额:###", ebl.Items.Sum(s => s.Amount).ToString());
                                    }
                                    #endregion

                                    #region tfootid
                                    sb.Replace("@公司地址", "");

                                    #endregion

                                    datas.Add(sb.ToString());
                                }
                            }
                            else if (d.BillTypeId == (int)BillTypeEnum.SaleReservationBill)
                            {
                                //销售订单列表
                                var sbl = _saleReservationBillService.GetSaleReservationBillById(curStore.Id,d.BillId, true);
                                if (sbl != null)
                                {
                                    List<int> allProductIds = new List<int>();
                                    if (sbl.Items != null && sbl.Items.Count > 0)
                                    {
                                        allProductIds.AddRange(sbl.Items.Select(it => it.ProductId).Distinct().ToList());
                                    }
                                    //去重
                                    allProductIds = allProductIds.Distinct().ToList();

                                    var allProducts = _productService.GetProductsByIds(curStore.Id, allProductIds.ToArray());
                                    var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());

                                    var terminal = _terminalService.GetTerminalById(curStore.Id, sbl.TerminalId);
                                    var wareHouse = _wareHouseService.GetWareHouseById(curStore.Id, sbl.WareHouseId);
                                    var businessUser = _userService.GetUserById(curStore.Id, sbl.BusinessUserId);

                                    var content = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.SaleReservationBill).Select(a => a.Content).FirstOrDefault();
                                    var makeUser = _userService.GetUserById(curStore.Id, sbl.MakeUserId);
                                    //获取默认收款账户
                                    sbl.SaleReservationBillAccountings.Select(s =>
                                    {
                                        s.AccountingOptionName = _accountingService.GetAccountingOptionName(curStore.Id, s.AccountingOptionId);
                                        return s;
                                    }).ToList();

                                    //填充打印数据
                                    StringBuilder sb = new StringBuilder();
                                    sb.Append(content);

                                    #region theadid
                                    sb.Replace("@商铺名称", curStore.Name);
                                    if (terminal != null)
                                    {
                                        sb.Replace("@客户名称", terminal.Name);
                                        sb.Replace("@客户电话", terminal.BossCall);
                                        sb.Replace("@客户地址", terminal.Address);
                                    }
                                    sb.Replace("@单据编号", sbl.BillNumber);
                                    if (makeUser != null)
                                    {
                                        sb.Replace("@制单", makeUser.UserRealName);
                                    }

                                    sb.Replace("@日期", sbl.TransactionDate == null ? "" : ((DateTime)sbl.TransactionDate).ToString("yyyy/MM/dd HH:mm:ss"));
                                    if (businessUser != null)
                                    {
                                        sb.Replace("@业务员", businessUser.UserRealName);
                                        sb.Replace("@业务电话", businessUser.MobileNumber);
                                    }
                                    #endregion

                                    #region tbodyid
                                    //明细
                                    //获取 tbody 中的行
                                    int beginTbody = sb.ToString().IndexOf("<tbody>") + 7;
                                    int endTbody = sb.ToString().IndexOf("</tbody>");
                                    string tbodytr = sb.ToString()[beginTbody..endTbody];

                                    if (sbl.Items != null && sbl.Items.Count > 0)
                                    {
                                        //1.先删除明细第一行
                                        sb.Remove(beginTbody, endTbody - beginTbody);
                                        int i = 0;
                                        foreach (var item in sbl.Items)
                                        {
                                            int index = sb.ToString().IndexOf("</tbody>");
                                            i++;
                                            StringBuilder sb2 = new StringBuilder();
                                            sb2.Append(tbodytr);

                                            sb2.Replace("#序号", i.ToString());
                                            var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                                            if (product != null)
                                            {
                                                sb2.Replace("#商品名称", product.Name);
                                                ProductUnitOption productUnitOption = product.GetProductUnit(_specificationAttributeService, _productService);
                                                if (item.UnitId == product.SmallUnitId)
                                                {
                                                    sb2.Replace("#条形码", product.SmallBarCode);
                                                    if (productUnitOption != null && productUnitOption.smallOption != null)
                                                    {
                                                        sb2.Replace("#商品单位", productUnitOption.smallOption.Name);
                                                    }

                                                }
                                                else if (item.UnitId == product.StrokeUnitId)
                                                {
                                                    sb2.Replace("#条形码", product.StrokeBarCode);
                                                    if (productUnitOption != null && productUnitOption.strokOption != null)
                                                    {
                                                        sb2.Replace("#商品单位", productUnitOption.strokOption.Name);
                                                    }
                                                }
                                                else if (item.UnitId == product.BigUnitId)
                                                {
                                                    sb2.Replace("#条形码", product.BigBarCode);
                                                    if (productUnitOption != null && productUnitOption.bigOption != null)
                                                    {
                                                        sb2.Replace("#商品单位", productUnitOption.bigOption.Name);
                                                    }
                                                }

                                                sb2.Replace("#单位换算", product.GetProductUnitConversion(allOptions));

                                            }
                                            sb2.Replace("#数量", item.Quantity.ToString());
                                            sb2.Replace("#价格", item.Price.ToString());
                                            sb2.Replace("#金额", item.Amount.ToString());
                                            sb2.Replace("#备注", item.Remark);

                                            sb.Insert(index, sb2);

                                        }

                                        sb.Replace("数量:###", sbl.Items.Sum(s => s.Quantity).ToString());
                                        sb.Replace("金额:###", sbl.Items.Sum(s => s.Amount).ToString());
                                    }
                                    #endregion

                                    #region tfootid

                                    sb.Replace("@公司地址", pCPrintSetting?.Address);
                                    sb.Replace("@订货电话", pCPrintSetting?.PlaceOrderTelphone);

                                    //收/付款方式
                                    var accounts = new StringBuilder();
                                    foreach (var acc in sbl?.SaleReservationBillAccountings)
                                    {
                                        accounts.Append($"{acc?.AccountingOptionName}[{acc?.CollectionAmount ?? 0}]&nbsp;&nbsp;");
                                    }
                                    sb.Replace("@收付款方式", accounts.ToString());

                                    #endregion

                                    datas.Add(sb.ToString());
                                }
                            }
                            else if (d.BillTypeId == (int)BillTypeEnum.ReturnReservationBill)
                            {
                                //退货订单列表
                                var rrl = _returnReservationBillService
                                .GetReturnReservationBillById(curStore.Id,d.BillId, true);
                                if (rrl != null)
                                {
                                    List<int> allProductIds = new List<int>();
                                    if (rrl.Items != null && rrl.Items.Count > 0)
                                    {
                                        allProductIds.AddRange(rrl.Items.Select(it => it.ProductId).Distinct().ToList());
                                    }
                                    //去重
                                    allProductIds = allProductIds.Distinct().ToList();

                                    var allProducts = _productService.GetProductsByIds(curStore.Id, allProductIds.ToArray());
                                    var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());


                                    var terminal = _terminalService.GetTerminalById(curStore.Id, rrl.TerminalId);
                                    var wareHouse = _wareHouseService.GetWareHouseById(curStore.Id, rrl.WareHouseId);
                                    var businessUser = _userService.GetUserById(curStore.Id, rrl.BusinessUserId);

                                    var content = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.ReturnReservationBill).Select(a => a.Content).FirstOrDefault();
                                    var makeUser = _userService.GetUserById(curStore.Id, rrl.MakeUserId);
                                    //填充打印数据
                                    StringBuilder sb = new StringBuilder();
                                    sb.Append(content);

                                    #region theadid
                                    sb.Replace("@商铺名称", curStore.Name);
                                    if (terminal != null)
                                    {
                                        sb.Replace("@客户名称", terminal.Name);
                                    }
                                    if (businessUser != null)
                                    {
                                        sb.Replace("@业务员", businessUser.UserRealName);
                                        sb.Replace("@业务电话", businessUser.MobileNumber);
                                    }
                                    sb.Replace("@单据编号", rrl.BillNumber);
                                    sb.Replace("@交易日期", rrl.TransactionDate == null ? "" : ((DateTime)rrl.TransactionDate).ToString("yyyy/MM/dd HH:mm:ss"));

                                    #endregion

                                    #region tbodyid
                                    //明细
                                    //获取 tbody 中的行
                                    int beginTbody = sb.ToString().IndexOf("<tbody>") + 7;
                                    int endTbody = sb.ToString().IndexOf("</tbody>");
                                    string tbodytr = sb.ToString()[beginTbody..endTbody];

                                    if (rrl.Items != null && rrl.Items.Count > 0)
                                    {
                                        //1.先删除明细第一行
                                        sb.Remove(beginTbody, endTbody - beginTbody);
                                        int i = 0;
                                        foreach (var item in rrl.Items)
                                        {
                                            int index = sb.ToString().IndexOf("</tbody>");
                                            i++;
                                            StringBuilder sb2 = new StringBuilder();
                                            sb2.Append(tbodytr);

                                            sb2.Replace("#序号", i.ToString());
                                            var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                                            if (product != null)
                                            {
                                                sb2.Replace("#商品名称", product.Name);
                                                ProductUnitOption productUnitOption = product.GetProductUnit(_specificationAttributeService, _productService);
                                                if (item.UnitId == product.SmallUnitId)
                                                {
                                                    sb2.Replace("#条形码", product.SmallBarCode);
                                                    if (productUnitOption != null && productUnitOption.smallOption != null)
                                                    {
                                                        sb2.Replace("#商品单位", productUnitOption.smallOption.Name);
                                                    }

                                                }
                                                else if (item.UnitId == product.StrokeUnitId)
                                                {
                                                    sb2.Replace("#条形码", product.StrokeBarCode);
                                                    if (productUnitOption != null && productUnitOption.strokOption != null)
                                                    {
                                                        sb2.Replace("#商品单位", productUnitOption.strokOption.Name);
                                                    }
                                                }
                                                else if (item.UnitId == product.BigUnitId)
                                                {
                                                    sb2.Replace("#条形码", product.BigBarCode);
                                                    if (productUnitOption != null && productUnitOption.bigOption != null)
                                                    {
                                                        sb2.Replace("#商品单位", productUnitOption.bigOption.Name);
                                                    }
                                                }

                                                sb2.Replace("#单位换算", product.GetProductUnitConversion(allOptions));

                                            }
                                            sb2.Replace("#数量", item.Quantity.ToString());
                                            sb2.Replace("#价格", item.Price.ToString());
                                            sb2.Replace("#金额", item.Amount.ToString());
                                            sb2.Replace("#备注", item.Remark);

                                            sb.Insert(index, sb2);

                                        }

                                        sb.Replace("数量:###", rrl.Items.Sum(s => s.Quantity).ToString());
                                        sb.Replace("金额:###", rrl.Items.Sum(s => s.Amount).ToString());
                                    }
                                    #endregion

                                    #region tfootid

                                    if (makeUser != null)
                                    {
                                        sb.Replace("@制单", makeUser.UserRealName);
                                    }
                                    sb.Replace("@日期", rrl.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss"));
                                    sb.Replace("@公司地址", "");
                                    sb.Replace("@订货电话", "");
                                    sb.Replace("@备注", rrl.Remark);

                                    #endregion

                                    datas.Add(sb.ToString());
                                }
                            }
                        }
                    }
                }
                    #region
                    //foreach (var d in dispatchBill.Items)
                    //{
                    //    if (d.BillTypeId == (int)BillTypeEnum.ExchangeBill)
                    //    {
                    //        //换货单列表
                    //        var ebls = _exchangeBillService
                    //            .GetExchangeBillsByIds(dispatchBill.Items.Where(di => di.BillTypeId == (int)BillTypeEnum.ExchangeBill)
                    //            .Select(di => di.BillId).ToArray(), true)?.ToList();
                    //        if (ebls != null && ebls.Count > 0)
                    //        {
                    //            List<int> allProductIds = new List<int>();
                    //            ebls.ToList().ForEach(sr =>
                    //            {
                    //                if (sr.Items != null && sr.Items.Count > 0)
                    //                {
                    //                    allProductIds.AddRange(sr.Items.Select(it => it.ProductId).Distinct().ToList());
                    //                }
                    //            //去重
                    //            allProductIds = allProductIds.Distinct().ToList();
                    //            });

                    //            var allProducts = _productService.GetProductsByIds(curStore.Id, allProductIds.ToArray());
                    //            var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());


                    //            var allTerminals = _terminalService.GetTerminalsByIds(curStore.Id, ebls.Select(sr => sr.TerminalId).Distinct().ToArray());
                    //            var allWareHouses = _wareHouseService.GetWareHouseByIds(curStore.Id, ebls.Select(sr => sr.WareHouseId).Distinct().ToArray());
                    //            var allBusinessUsers = _userService.GetUsersByIds(curStore.Id, ebls.Select(sr => sr.BusinessUserId).Distinct().ToArray());

                    //            foreach (string t in printTypeList)
                    //            {
                    //                //打印整箱拆零合并单 （最小单位打印）
                    //                if (t == "1")
                    //                {
                    //                    var content = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.AllZeroMergerBill).Select(a => a.Content).FirstOrDefault();
                    //                    foreach (var eb in ebls)
                    //                    {
                    //                        //填充打印数据
                    //                        StringBuilder sb = new StringBuilder();
                    //                        sb.Append(content);

                    //                        #region theadid
                    //                        sb.Replace("@商铺名称", curStore.Name);
                    //                        Terminal terminal = allTerminals.Where(at => at.Id == eb.TerminalId).FirstOrDefault();
                    //                        if (terminal != null)
                    //                        {
                    //                            sb.Replace("@客户名称", terminal.Name);
                    //                        }
                    //                        WareHouse wareHouse = allWareHouses.Where(aw => aw.Id == eb.WareHouseId).FirstOrDefault();
                    //                        if (wareHouse != null)
                    //                        {
                    //                            sb.Replace("@仓库", wareHouse.Name);
                    //                        }

                    //                        sb.Replace("@车辆", carNo);
                    //                        User businessUser = allBusinessUsers.Where(ab => ab.Id == eb.BusinessUserId).FirstOrDefault();
                    //                        if (businessUser != null)
                    //                        {
                    //                            sb.Replace("@业务员", businessUser.UserRealName);
                    //                            sb.Replace("@业务电话", businessUser.MobileNumber);
                    //                        }
                    //                        sb.Replace("@订单编号", eb.BillNumber);

                    //                        #endregion

                    //                        #region tbodyid
                    //                        //明细
                    //                        //获取 tbody 中的行
                    //                        int beginTbody = sb.ToString().IndexOf("<tbody>") + 7;
                    //                        int endTbody = sb.ToString().IndexOf("</tbody>");
                    //                        string tbodytr = sb.ToString()[beginTbody..endTbody];

                    //                        if (eb.Items != null && eb.Items.Count > 0)
                    //                        {
                    //                            //1.先删除明细第一行
                    //                            sb.Remove(beginTbody, endTbody - beginTbody);
                    //                            int i = 0;

                    //                            int bigQuantity = 0;
                    //                            int strokeQuantity = 0;
                    //                            int smallQuantity = 0;

                    //                            eb.Items.Select(si => si.ProductId).Distinct().ToList().ForEach(pi =>
                    //                            {
                    //                                var items = eb.Items.Where(it => it.ProductId == pi).ToList();
                    //                                Product product = allProducts.Where(ap => ap.Id == pi).FirstOrDefault();
                    //                                if (items != null && items.Count > 0 && product != null)
                    //                                {
                    //                                    int index = sb.ToString().IndexOf("</tbody>");
                    //                                    i++;
                    //                                    StringBuilder sb2 = new StringBuilder();
                    //                                    sb2.Append(tbodytr);
                    //                                    sb2.Replace("#序号", i.ToString());
                    //                                    sb2.Replace("#商品名称", product.Name);
                    //                                    int thisProductSmallQuantity = 0;
                    //                                    string thisProductSmallUnitName = "";
                    //                                    var option = allOptions.Where(sf => sf.Id == product.SmallUnitId).FirstOrDefault();
                    //                                    thisProductSmallUnitName = option == null ? "" : option.Name;
                    //                                    items.ForEach(it =>
                    //                                    {
                    //                                        if (it.UnitId == product.BigUnitId)
                    //                                        {
                    //                                            bigQuantity += it.Quantity;
                    //                                            thisProductSmallQuantity += it.Quantity * (product.BigQuantity ?? 1);
                    //                                        }
                    //                                        else if (it.UnitId == product.StrokeUnitId)
                    //                                        {
                    //                                            strokeQuantity += it.Quantity;
                    //                                            thisProductSmallQuantity += it.Quantity * (product.StrokeQuantity ?? 1);
                    //                                        }
                    //                                        else if (it.UnitId == product.SmallUnitId)
                    //                                        {
                    //                                            smallQuantity += it.Quantity;
                    //                                            thisProductSmallQuantity += it.Quantity;
                    //                                        }
                    //                                    });

                    //                                    sb2.Replace("#数量", thisProductSmallQuantity.ToString() + thisProductSmallUnitName);
                    //                                    sb.Insert(index, sb2);
                    //                                }
                    //                            });

                    //                            sb.Replace("数量(大中小):###", bigQuantity + "大" + strokeQuantity + "中" + smallQuantity + "小");
                    //                        }
                    //                        #endregion

                    //                        #region tfootid
                    //                        #endregion

                    //                        datas += sb;
                    //                    }
                    //                }
                    //                //打印整箱装车单
                    //                else if (t == "2")
                    //                {
                    //                    var content = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.AllLoadBill).Select(a => a.Content).FirstOrDefault();
                    //                    foreach (var eb in ebls)
                    //                    {
                    //                        //填充打印数据
                    //                        StringBuilder sb = new StringBuilder();
                    //                        sb.Append(content);

                    //                        #region theadid
                    //                        sb.Replace("@商铺名称", curStore.Name);
                    //                        Terminal terminal = allTerminals.Where(at => at.Id == eb.TerminalId).FirstOrDefault();
                    //                        if (terminal != null)
                    //                        {
                    //                            sb.Replace("@客户名称", terminal.Name);
                    //                        }
                    //                        WareHouse wareHouse = allWareHouses.Where(aw => aw.Id == eb.WareHouseId).FirstOrDefault();
                    //                        if (wareHouse != null)
                    //                        {
                    //                            sb.Replace("@仓库", wareHouse.Name);
                    //                        }
                    //                        sb.Replace("@车辆", carNo);
                    //                        User businessUser = allBusinessUsers.Where(ab => ab.Id == eb.BusinessUserId).FirstOrDefault();
                    //                        if (businessUser != null)
                    //                        {
                    //                            sb.Replace("@业务员", businessUser.UserRealName);
                    //                            sb.Replace("@业务电话", businessUser.MobileNumber);
                    //                        }
                    //                        sb.Replace("@订单编号", eb.BillNumber);

                    //                        #endregion

                    //                        #region tbodyid
                    //                        //明细
                    //                        //获取 tbody 中的行
                    //                        int beginTbody = sb.ToString().IndexOf("<tbody>") + 7;
                    //                        int endTbody = sb.ToString().IndexOf("</tbody>");
                    //                        string tbodytr = sb.ToString()[beginTbody..endTbody];

                    //                        if (eb.Items != null && eb.Items.Count > 0)
                    //                        {
                    //                            //1.先删除明细第一行
                    //                            sb.Remove(beginTbody, endTbody - beginTbody);
                    //                            int i = 0;

                    //                            foreach (var item in eb.Items)
                    //                            {
                    //                                int index = sb.ToString().IndexOf("</tbody>");
                    //                                i++;
                    //                                StringBuilder sb2 = new StringBuilder();
                    //                                sb2.Append(tbodytr);

                    //                                sb2.Replace("#序号", i.ToString());
                    //                                var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                    //                                if (product != null)
                    //                                {
                    //                                    sb2.Replace("#商品名称", product.Name);
                    //                                    var option = allOptions.Where(sf => sf.Id == product.SmallUnitId).FirstOrDefault();
                    //                                    sb2.Replace("#数量", item.Quantity.ToString() + (option == null ? "" : option.Name));
                    //                                }
                    //                                sb.Insert(index, sb2);
                    //                            }

                    //                        }
                    //                        #endregion

                    //                        #region tfootid
                    //                        #endregion

                    //                        datas += sb;
                    //                    }
                    //                }
                    //                //为每个客户打印拆零装车单
                    //                else if (t == "3")
                    //                {
                    //                    var content = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.ZeroLoadBill).Select(a => a.Content).FirstOrDefault();
                    //                    if (allTerminals != null && allTerminals.Count > 0)
                    //                    {
                    //                        ebls.Select(sr => sr.TerminalId).Distinct().ToList().ForEach(tid =>
                    //                        {
                    //                            Terminal terminal = allTerminals.Where(at => at.Id == tid).FirstOrDefault();
                    //                            if (terminal != null)
                    //                            {
                    //                                var saleReservations = ebls.Where(ss => ss.TerminalId == tid).ToList();
                    //                                if (saleReservations != null && saleReservations.Count > 0)
                    //                                {

                    //                                    foreach (var saleReservationBill in saleReservations)
                    //                                    {
                    //                                    //填充打印数据
                    //                                    StringBuilder sb = new StringBuilder();
                    //                                        sb.Append(content);

                    //                                    #region theadid
                    //                                    sb.Replace("@商铺名称", curStore.Name);
                    //                                        if (terminal != null)
                    //                                        {
                    //                                            sb.Replace("@客户名称", terminal.Name);
                    //                                        }
                    //                                        WareHouse wareHouse = allWareHouses.Where(aw => aw.Id == saleReservationBill.WareHouseId).FirstOrDefault();
                    //                                        if (wareHouse != null)
                    //                                        {
                    //                                            sb.Replace("@仓库", wareHouse.Name);
                    //                                        }
                    //                                        sb.Replace("@车辆", carNo);
                    //                                        User businessUser = allBusinessUsers.Where(ab => ab.Id == saleReservationBill.BusinessUserId).FirstOrDefault();
                    //                                        if (businessUser != null)
                    //                                        {
                    //                                            sb.Replace("@业务员", businessUser.UserRealName);
                    //                                            sb.Replace("@业务电话", businessUser.MobileNumber);
                    //                                        }
                    //                                        sb.Replace("@订单编号", saleReservationBill.BillNumber);

                    //                                    #endregion

                    //                                    #region tbodyid
                    //                                    //明细
                    //                                    //获取 tbody 中的行
                    //                                    int beginTbody = sb.ToString().IndexOf("<tbody>") + 7;
                    //                                        int endTbody = sb.ToString().IndexOf("</tbody>");
                    //                                        string tbodytr = sb.ToString()[beginTbody..endTbody];

                    //                                        if (saleReservationBill.Items != null && saleReservationBill.Items.Count > 0)
                    //                                        {
                    //                                        //1.先删除明细第一行
                    //                                        sb.Remove(beginTbody, endTbody - beginTbody);
                    //                                            int i = 0;
                    //                                            int bigQuantity = 0;
                    //                                            int strokeQuantity = 0;
                    //                                            int smallQuantity = 0;

                    //                                            saleReservationBill.Items.Select(si => si.ProductId).Distinct().ToList().ForEach(pi =>
                    //                                            {
                    //                                                var items = saleReservationBill.Items.Where(it => it.ProductId == pi).ToList();
                    //                                                Product product = allProducts.Where(ap => ap.Id == pi).FirstOrDefault();
                    //                                                if (items != null && items.Count > 0 && product != null)
                    //                                                {
                    //                                                    int index = sb.ToString().IndexOf("</tbody>");
                    //                                                    i++;
                    //                                                    StringBuilder sb2 = new StringBuilder();
                    //                                                    sb2.Append(tbodytr);

                    //                                                    sb2.Replace("#序号", i.ToString());
                    //                                                    sb2.Replace("#商品名称", product.Name);
                    //                                                    int thisProductSmallQuantity = 0;
                    //                                                    string thisProductSmallUnitName = "";
                    //                                                    var option = allOptions.Where(sf => sf.Id == product.SmallUnitId).FirstOrDefault();
                    //                                                    thisProductSmallUnitName = option == null ? "" : option.Name;
                    //                                                    items.ForEach(it =>
                    //                                                    {
                    //                                                        if (it.UnitId == product.BigUnitId)
                    //                                                        {
                    //                                                            bigQuantity += it.Quantity;
                    //                                                            thisProductSmallQuantity += it.Quantity * (product.BigQuantity ?? 1);
                    //                                                        }
                    //                                                        else if (it.UnitId == product.StrokeUnitId)
                    //                                                        {
                    //                                                            strokeQuantity += it.Quantity;
                    //                                                            thisProductSmallQuantity += it.Quantity * (product.StrokeQuantity ?? 1);
                    //                                                        }
                    //                                                        else if (it.UnitId == product.SmallUnitId)
                    //                                                        {
                    //                                                            smallQuantity += it.Quantity;
                    //                                                            thisProductSmallQuantity += it.Quantity;
                    //                                                        }
                    //                                                    });

                    //                                                    sb2.Replace("#数量", thisProductSmallQuantity.ToString() + thisProductSmallUnitName);
                    //                                                    sb.Insert(index, sb2);
                    //                                                }
                    //                                            });
                    //                                            sb.Replace("数量(大中小):###", bigQuantity + "大" + strokeQuantity + "中" + smallQuantity + "小");
                    //                                        }
                    //                                    #endregion

                    //                                    #region tfootid
                    //                                    #endregion

                    //                                    datas += sb;
                    //                                    }
                    //                                }
                    //                            }
                    //                        });
                    //                    }
                    //                }
                    //                //打印订单
                    //                else if (t == "4")
                    //                {
                    //                    var content = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.ExchangeBill).Select(a => a.Content).FirstOrDefault();
                    //                    var allMakeUsers = _userService.GetUsersByIds(curStore.Id, ebls.Select(sr => sr.MakeUserId).Distinct().ToArray());
                    //                    foreach (var eb in ebls)
                    //                    {
                    //                        //填充打印数据
                    //                        StringBuilder sb = new StringBuilder();
                    //                        sb.Append(content);

                    //                        #region theadid
                    //                        sb.Replace("@商铺名称", curStore.Name);
                    //                        Terminal terminal = allTerminals.Where(at => at.Id == eb.TerminalId).FirstOrDefault();
                    //                        if (terminal != null)
                    //                        {
                    //                            sb.Replace("@客户名称", terminal.Name);
                    //                            sb.Replace("@客户电话", terminal.BossCall);
                    //                            sb.Replace("@客户地址", terminal.Address);
                    //                        }
                    //                        sb.Replace("@单据编号", eb.BillNumber);
                    //                        User makeUser = allMakeUsers.Where(am => am.Id == eb.MakeUserId).FirstOrDefault();
                    //                        if (makeUser != null)
                    //                        {
                    //                            sb.Replace("@制单", makeUser.UserRealName);
                    //                        }

                    //                        sb.Replace("@日期", eb.TransactionDate == null ? "" : ((DateTime)eb.TransactionDate).ToString("yyyy/MM/dd HH:mm:ss"));
                    //                        User businessUser = allBusinessUsers.Where(ab => ab.Id == eb.BusinessUserId).FirstOrDefault();
                    //                        if (businessUser != null)
                    //                        {
                    //                            sb.Replace("@业务员", businessUser.UserRealName);
                    //                            sb.Replace("@业务电话", businessUser.MobileNumber);
                    //                        }
                    //                        #endregion

                    //                        #region tbodyid
                    //                        //明细
                    //                        //获取 tbody 中的行
                    //                        int beginTbody = sb.ToString().IndexOf("<tbody>") + 7;
                    //                        int endTbody = sb.ToString().IndexOf("</tbody>");
                    //                        string tbodytr = sb.ToString()[beginTbody..endTbody];

                    //                        if (eb.Items != null && eb.Items.Count > 0)
                    //                        {
                    //                            //1.先删除明细第一行
                    //                            sb.Remove(beginTbody, endTbody - beginTbody);
                    //                            int i = 0;
                    //                            foreach (var item in eb.Items)
                    //                            {
                    //                                int index = sb.ToString().IndexOf("</tbody>");
                    //                                i++;
                    //                                StringBuilder sb2 = new StringBuilder();
                    //                                sb2.Append(tbodytr);

                    //                                sb2.Replace("#序号", i.ToString());
                    //                                var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                    //                                if (product != null)
                    //                                {
                    //                                    sb2.Replace("#商品名称", product.Name);
                    //                                    ProductUnitOption productUnitOption = product.GetProductUnit(_specificationAttributeService, _productService);
                    //                                    if (item.UnitId == product.SmallUnitId)
                    //                                    {
                    //                                        sb2.Replace("#条形码", product.SmallBarCode);
                    //                                        if (productUnitOption != null && productUnitOption.smallOption != null)
                    //                                        {
                    //                                            sb2.Replace("#商品单位", productUnitOption.smallOption.Name);
                    //                                        }

                    //                                    }
                    //                                    else if (item.UnitId == product.StrokeUnitId)
                    //                                    {
                    //                                        sb2.Replace("#条形码", product.StrokeBarCode);
                    //                                        if (productUnitOption != null && productUnitOption.strokOption != null)
                    //                                        {
                    //                                            sb2.Replace("#商品单位", productUnitOption.strokOption.Name);
                    //                                        }
                    //                                    }
                    //                                    else if (item.UnitId == product.BigUnitId)
                    //                                    {
                    //                                        sb2.Replace("#条形码", product.BigBarCode);
                    //                                        if (productUnitOption != null && productUnitOption.bigOption != null)
                    //                                        {
                    //                                            sb2.Replace("#商品单位", productUnitOption.bigOption.Name);
                    //                                        }
                    //                                    }

                    //                                    sb2.Replace("#单位换算", product.GetProductUnitConversion(allOptions));

                    //                                }
                    //                                sb2.Replace("#数量", item.Quantity.ToString());
                    //                                sb2.Replace("#价格", item.Price.ToString());
                    //                                sb2.Replace("#金额", item.Amount.ToString());
                    //                                sb2.Replace("#备注", item.Remark);

                    //                                sb.Insert(index, sb2);

                    //                            }

                    //                            sb.Replace("数量:###", eb.Items.Sum(s => s.Quantity).ToString());
                    //                            sb.Replace("金额:###", eb.Items.Sum(s => s.Amount).ToString());
                    //                        }
                    //                        #endregion

                    //                        #region tfootid
                    //                        sb.Replace("@公司地址", "");

                    //                        #endregion

                    //                        datas += sb;
                    //                    }

                    //                }
                    //            }
                    //        }
                    //    }
                    //    else if (d.BillTypeId == (int)BillTypeEnum.SaleReservationBill)
                    //    {

                    //        //销售订单列表
                    //        var sbls = _saleReservationBillService
                    //         .GetSaleReservationBillsByIds(dispatchBill.Items.Where(di => di.BillTypeId == (int)BillTypeEnum.SaleReservationBill)
                    //         .Select(di => di.BillId).ToArray(), true)?.ToList();
                    //        if (sbls != null && sbls.Count > 0)
                    //        {
                    //            List<int> allProductIds = new List<int>();
                    //            sbls.ToList().ForEach(sr =>
                    //            {
                    //                if (sr.Items != null && sr.Items.Count > 0)
                    //                {
                    //                    allProductIds.AddRange(sr.Items.Select(it => it.ProductId).Distinct().ToList());
                    //                }
                    //            //去重
                    //            allProductIds = allProductIds.Distinct().ToList();
                    //            });

                    //            var allProducts = _productService.GetProductsByIds(curStore.Id, allProductIds.ToArray());
                    //            var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());

                    //            var allTerminals = _terminalService.GetTerminalsByIds(curStore.Id, sbls.Select(sr => sr.TerminalId).Distinct().ToArray());

                    //            var allWareHouses = _wareHouseService.GetWareHouseByIds(curStore.Id, sbls.Select(sr => sr.WareHouseId).Distinct().ToArray());

                    //            var allBusinessUsers = _userService.GetUsersByIds(curStore.Id, sbls.Select(sr => sr.BusinessUserId).Distinct().ToArray());

                    //            foreach (string t in printTypeList)
                    //            {
                    //                //打印整箱拆零合并单 （最小单位打印）
                    //                if (t == "1")
                    //                {
                    //                    var content = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.AllZeroMergerBill).Select(a => a.Content).FirstOrDefault();
                    //                    foreach (var srb in sbls)
                    //                    {
                    //                        //填充打印数据
                    //                        StringBuilder sb = new StringBuilder();
                    //                        sb.Append(content);

                    //                        #region theadid
                    //                        sb.Replace("@商铺名称", curStore.Name);
                    //                        var terminal = allTerminals.Where(at => at.Id == srb.TerminalId).FirstOrDefault();
                    //                        if (terminal != null)
                    //                        {
                    //                            sb.Replace("@客户名称", terminal.Name);
                    //                        }
                    //                        WareHouse wareHouse = allWareHouses.Where(aw => aw.Id == srb.WareHouseId).FirstOrDefault();
                    //                        if (wareHouse != null)
                    //                        {
                    //                            sb.Replace("@仓库", wareHouse.Name);
                    //                        }

                    //                        sb.Replace("@车辆", carNo);
                    //                        User businessUser = allBusinessUsers.Where(ab => ab.Id == srb.BusinessUserId).FirstOrDefault();
                    //                        if (businessUser != null)
                    //                        {
                    //                            sb.Replace("@业务员", businessUser.UserRealName);
                    //                            sb.Replace("@业务电话", businessUser.MobileNumber);
                    //                        }
                    //                        sb.Replace("@订单编号", srb.BillNumber);

                    //                        #endregion

                    //                        #region tbodyid
                    //                        //明细
                    //                        //获取 tbody 中的行
                    //                        int beginTbody = sb.ToString().IndexOf("<tbody>") + 7;
                    //                        int endTbody = sb.ToString().IndexOf("</tbody>");
                    //                        string tbodytr = sb.ToString()[beginTbody..endTbody];

                    //                        if (srb.Items != null && srb.Items.Count > 0)
                    //                        {
                    //                            //1.先删除明细第一行
                    //                            sb.Remove(beginTbody, endTbody - beginTbody);
                    //                            int i = 0;

                    //                            int bigQuantity = 0;
                    //                            int strokeQuantity = 0;
                    //                            int smallQuantity = 0;

                    //                            srb.Items.Select(si => si.ProductId).Distinct()?.ToList().ForEach(pi =>
                    //                            {
                    //                                var items = srb.Items.Where(it => it.ProductId == pi).ToList();
                    //                                Product product = allProducts.Where(ap => ap.Id == pi).FirstOrDefault();
                    //                                if (items != null && items.Count > 0 && product != null)
                    //                                {
                    //                                    int index = sb.ToString().IndexOf("</tbody>");
                    //                                    i++;
                    //                                    StringBuilder sb2 = new StringBuilder();
                    //                                    sb2.Append(tbodytr);
                    //                                    sb2.Replace("#序号", i.ToString());
                    //                                    sb2.Replace("#商品名称", product.Name);
                    //                                    int thisProductSmallQuantity = 0;
                    //                                    string thisProductSmallUnitName = "";
                    //                                    var option = allOptions.Where(sf => sf.Id == product.SmallUnitId).FirstOrDefault();
                    //                                    thisProductSmallUnitName = option == null ? "" : option.Name;
                    //                                    items.ForEach(it =>
                    //                                    {
                    //                                        if (it.UnitId == product.BigUnitId)
                    //                                        {
                    //                                            bigQuantity += it.Quantity;
                    //                                            thisProductSmallQuantity += it.Quantity * (product.BigQuantity ?? 1);
                    //                                        }
                    //                                        else if (it.UnitId == product.StrokeUnitId)
                    //                                        {
                    //                                            strokeQuantity += it.Quantity;
                    //                                            thisProductSmallQuantity += it.Quantity * (product.StrokeQuantity ?? 1);
                    //                                        }
                    //                                        else if (it.UnitId == product.SmallUnitId)
                    //                                        {
                    //                                            smallQuantity += it.Quantity;
                    //                                            thisProductSmallQuantity += it.Quantity;
                    //                                        }
                    //                                    });

                    //                                    sb2.Replace("#数量", thisProductSmallQuantity.ToString() + thisProductSmallUnitName);
                    //                                    sb.Insert(index, sb2);
                    //                                }
                    //                            });

                    //                            sb.Replace("数量(大中小):###", bigQuantity + "大" + strokeQuantity + "中" + smallQuantity + "小");
                    //                        }
                    //                        #endregion

                    //                        #region tfootid
                    //                        #endregion

                    //                        datas += sb;
                    //                    }
                    //                }
                    //                //打印整箱装车单
                    //                else if (t == "2")
                    //                {
                    //                    var content = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.AllLoadBill).Select(a => a.Content).FirstOrDefault();
                    //                    foreach (var srb in sbls)
                    //                    {
                    //                        //填充打印数据
                    //                        StringBuilder sb = new StringBuilder();
                    //                        sb.Append(content);

                    //                        #region theadid
                    //                        sb.Replace("@商铺名称", curStore.Name);
                    //                        Terminal terminal = allTerminals.Where(at => at.Id == srb.TerminalId).FirstOrDefault();
                    //                        if (terminal != null)
                    //                        {
                    //                            sb.Replace("@客户名称", terminal.Name);
                    //                        }

                    //                        var wn = _wareHouseService.GetWareHouseName(srb.StoreId, srb.WareHouseId);
                    //                        sb.Replace("@仓库", wn);

                    //                        sb.Replace("@车辆", carNo);
                    //                        User businessUser = allBusinessUsers.Where(ab => ab.Id == srb.BusinessUserId).FirstOrDefault();
                    //                        if (businessUser != null)
                    //                        {
                    //                            sb.Replace("@业务员", businessUser.UserRealName);
                    //                            sb.Replace("@业务电话", businessUser.MobileNumber);
                    //                        }
                    //                        sb.Replace("@订单编号", srb.BillNumber);

                    //                        #endregion

                    //                        #region tbodyid
                    //                        //明细
                    //                        //获取 tbody 中的行
                    //                        int beginTbody = sb.ToString().IndexOf("<tbody>") + 7;
                    //                        int endTbody = sb.ToString().IndexOf("</tbody>");
                    //                        string tbodytr = sb.ToString()[beginTbody..endTbody];

                    //                        if (srb.Items != null && srb.Items.Count > 0)
                    //                        {
                    //                            //1.先删除明细第一行
                    //                            sb.Remove(beginTbody, endTbody - beginTbody);
                    //                            int i = 0;

                    //                            foreach (var item in srb.Items)
                    //                            {
                    //                                int index = sb.ToString().IndexOf("</tbody>");
                    //                                i++;
                    //                                StringBuilder sb2 = new StringBuilder();
                    //                                sb2.Append(tbodytr);

                    //                                sb2.Replace("#序号", i.ToString());
                    //                                var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                    //                                if (product != null)
                    //                                {
                    //                                    sb2.Replace("#商品名称", product.Name);
                    //                                    var option = allOptions.Where(sf => sf.Id == product.SmallUnitId).FirstOrDefault();
                    //                                    sb2.Replace("#数量", item.Quantity.ToString() + (option == null ? "" : option.Name));
                    //                                }
                    //                                sb.Insert(index, sb2);
                    //                            }

                    //                        }
                    //                        #endregion

                    //                        #region tfootid
                    //                        #endregion

                    //                        datas += sb;
                    //                    }
                    //                }
                    //                //为每个客户打印拆零装车单
                    //                else if (t == "3")
                    //                {
                    //                    var content = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.ZeroLoadBill).Select(a => a.Content).FirstOrDefault();
                    //                    if (allTerminals != null && allTerminals.Count > 0)
                    //                    {
                    //                        sbls.Select(sr => sr.TerminalId).Distinct().ToList().ForEach(tid =>
                    //                        {
                    //                            Terminal terminal = allTerminals.Where(at => at.Id == tid).FirstOrDefault();
                    //                            if (terminal != null)
                    //                            {
                    //                                var saleReservations = sbls.Where(ss => ss.TerminalId == tid).ToList();
                    //                                if (saleReservations != null && saleReservations.Count > 0)
                    //                                {

                    //                                    foreach (var srb in saleReservations)
                    //                                    {
                    //                                    //填充打印数据
                    //                                    StringBuilder sb = new StringBuilder();
                    //                                        sb.Append(content);

                    //                                    #region theadid
                    //                                    sb.Replace("@商铺名称", curStore.Name);
                    //                                        if (terminal != null)
                    //                                        {
                    //                                            sb.Replace("@客户名称", terminal.Name);
                    //                                        }
                    //                                        WareHouse wareHouse = allWareHouses.Where(aw => aw.Id == srb.WareHouseId).FirstOrDefault();
                    //                                        if (wareHouse != null)
                    //                                        {
                    //                                            sb.Replace("@仓库", wareHouse.Name);
                    //                                        }
                    //                                        sb.Replace("@车辆", carNo);
                    //                                        User businessUser = allBusinessUsers.Where(ab => ab.Id == srb.BusinessUserId).FirstOrDefault();
                    //                                        if (businessUser != null)
                    //                                        {
                    //                                            sb.Replace("@业务员", businessUser.UserRealName);
                    //                                            sb.Replace("@业务电话", businessUser.MobileNumber);
                    //                                        }
                    //                                        sb.Replace("@订单编号", srb.BillNumber);

                    //                                    #endregion

                    //                                    #region tbodyid
                    //                                    //明细
                    //                                    //获取 tbody 中的行
                    //                                    int beginTbody = sb.ToString().IndexOf("<tbody>") + 7;
                    //                                        int endTbody = sb.ToString().IndexOf("</tbody>");
                    //                                        string tbodytr = sb.ToString()[beginTbody..endTbody];

                    //                                        if (srb.Items != null && srb.Items.Count > 0)
                    //                                        {
                    //                                        //1.先删除明细第一行
                    //                                        sb.Remove(beginTbody, endTbody - beginTbody);
                    //                                            int i = 0;
                    //                                            int bigQuantity = 0;
                    //                                            int strokeQuantity = 0;
                    //                                            int smallQuantity = 0;

                    //                                            srb.Items.Select(si => si.ProductId).Distinct().ToList().ForEach(pi =>
                    //                                            {
                    //                                                var items = srb.Items.Where(it => it.ProductId == pi).ToList();
                    //                                                Product product = allProducts.Where(ap => ap.Id == pi).FirstOrDefault();
                    //                                                if (items != null && items.Count > 0 && product != null)
                    //                                                {
                    //                                                    int index = sb.ToString().IndexOf("</tbody>");
                    //                                                    i++;
                    //                                                    StringBuilder sb2 = new StringBuilder();
                    //                                                    sb2.Append(tbodytr);

                    //                                                    sb2.Replace("#序号", i.ToString());
                    //                                                    sb2.Replace("#商品名称", product.Name);
                    //                                                    int thisProductSmallQuantity = 0;
                    //                                                    string thisProductSmallUnitName = "";
                    //                                                    var option = allOptions.Where(sf => sf.Id == product.SmallUnitId).FirstOrDefault();
                    //                                                    thisProductSmallUnitName = option == null ? "" : option.Name;
                    //                                                    items.ForEach(it =>
                    //                                                    {
                    //                                                        if (it.UnitId == product.BigUnitId)
                    //                                                        {
                    //                                                            bigQuantity += it.Quantity;
                    //                                                            thisProductSmallQuantity += it.Quantity * (product.BigQuantity ?? 1);
                    //                                                        }
                    //                                                        else if (it.UnitId == product.StrokeUnitId)
                    //                                                        {
                    //                                                            strokeQuantity += it.Quantity;
                    //                                                            thisProductSmallQuantity += it.Quantity * (product.StrokeQuantity ?? 1);
                    //                                                        }
                    //                                                        else if (it.UnitId == product.SmallUnitId)
                    //                                                        {
                    //                                                            smallQuantity += it.Quantity;
                    //                                                            thisProductSmallQuantity += it.Quantity;
                    //                                                        }
                    //                                                    });

                    //                                                    sb2.Replace("#数量", thisProductSmallQuantity.ToString() + thisProductSmallUnitName);
                    //                                                    sb.Insert(index, sb2);
                    //                                                }
                    //                                            });
                    //                                            sb.Replace("数量(大中小):###", bigQuantity + "大" + strokeQuantity + "中" + smallQuantity + "小");
                    //                                        }
                    //                                    #endregion

                    //                                    #region tfootid
                    //                                    #endregion

                    //                                    datas += sb;
                    //                                    }
                    //                                }
                    //                            }
                    //                        });
                    //                    }
                    //                }
                    //                //打印订单
                    //                else if (t == "4")
                    //                {
                    //                    var content = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.SaleReservationBill).Select(a => a.Content).FirstOrDefault();
                    //                    var allMakeUsers = _userService.GetUsersByIds(curStore.Id, sbls.Select(sr => sr.MakeUserId).Distinct().ToArray());
                    //                    foreach (var srb in sbls)
                    //                    {
                    //                        //获取默认收款账户
                    //                        srb.SaleReservationBillAccountings.Select(s =>
                    //                        {
                    //                            s.AccountingOptionName = _accountingService.GetAccountingOptionName(curStore.Id, s.AccountingOptionId);
                    //                            return s;
                    //                        }).ToList();

                    //                        //填充打印数据
                    //                        StringBuilder sb = new StringBuilder();
                    //                        sb.Append(content);

                    //                        #region theadid
                    //                        sb.Replace("@商铺名称", curStore.Name);
                    //                        Terminal terminal = allTerminals.Where(at => at.Id == srb.TerminalId).FirstOrDefault();
                    //                        if (terminal != null)
                    //                        {
                    //                            sb.Replace("@客户名称", terminal.Name);
                    //                            sb.Replace("@客户电话", terminal.BossCall);
                    //                            sb.Replace("@客户地址", terminal.Address);
                    //                        }
                    //                        sb.Replace("@单据编号", srb.BillNumber);
                    //                        User makeUser = allMakeUsers.Where(am => am.Id == srb.MakeUserId).FirstOrDefault();
                    //                        if (makeUser != null)
                    //                        {
                    //                            sb.Replace("@制单", makeUser.UserRealName);
                    //                        }

                    //                        sb.Replace("@日期", srb.TransactionDate == null ? "" : ((DateTime)srb.TransactionDate).ToString("yyyy/MM/dd HH:mm:ss"));
                    //                        User businessUser = allBusinessUsers.Where(ab => ab.Id == srb.BusinessUserId).FirstOrDefault();
                    //                        if (businessUser != null)
                    //                        {
                    //                            sb.Replace("@业务员", businessUser.UserRealName);
                    //                            sb.Replace("@业务电话", businessUser.MobileNumber);
                    //                        }
                    //                        #endregion

                    //                        #region tbodyid
                    //                        //明细
                    //                        //获取 tbody 中的行
                    //                        int beginTbody = sb.ToString().IndexOf("<tbody>") + 7;
                    //                        int endTbody = sb.ToString().IndexOf("</tbody>");
                    //                        string tbodytr = sb.ToString()[beginTbody..endTbody];

                    //                        if (srb.Items != null && srb.Items.Count > 0)
                    //                        {
                    //                            //1.先删除明细第一行
                    //                            sb.Remove(beginTbody, endTbody - beginTbody);
                    //                            int i = 0;
                    //                            foreach (var item in srb.Items)
                    //                            {
                    //                                int index = sb.ToString().IndexOf("</tbody>");
                    //                                i++;
                    //                                StringBuilder sb2 = new StringBuilder();
                    //                                sb2.Append(tbodytr);

                    //                                sb2.Replace("#序号", i.ToString());
                    //                                var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                    //                                if (product != null)
                    //                                {
                    //                                    sb2.Replace("#商品名称", product.Name);
                    //                                    ProductUnitOption productUnitOption = product.GetProductUnit(_specificationAttributeService, _productService);
                    //                                    if (item.UnitId == product.SmallUnitId)
                    //                                    {
                    //                                        sb2.Replace("#条形码", product.SmallBarCode);
                    //                                        if (productUnitOption != null && productUnitOption.smallOption != null)
                    //                                        {
                    //                                            sb2.Replace("#商品单位", productUnitOption.smallOption.Name);
                    //                                        }

                    //                                    }
                    //                                    else if (item.UnitId == product.StrokeUnitId)
                    //                                    {
                    //                                        sb2.Replace("#条形码", product.StrokeBarCode);
                    //                                        if (productUnitOption != null && productUnitOption.strokOption != null)
                    //                                        {
                    //                                            sb2.Replace("#商品单位", productUnitOption.strokOption.Name);
                    //                                        }
                    //                                    }
                    //                                    else if (item.UnitId == product.BigUnitId)
                    //                                    {
                    //                                        sb2.Replace("#条形码", product.BigBarCode);
                    //                                        if (productUnitOption != null && productUnitOption.bigOption != null)
                    //                                        {
                    //                                            sb2.Replace("#商品单位", productUnitOption.bigOption.Name);
                    //                                        }
                    //                                    }

                    //                                    sb2.Replace("#单位换算", product.GetProductUnitConversion(allOptions));

                    //                                }
                    //                                sb2.Replace("#数量", item.Quantity.ToString());
                    //                                sb2.Replace("#价格", item.Price.ToString());
                    //                                sb2.Replace("#金额", item.Amount.ToString());
                    //                                sb2.Replace("#备注", item.Remark);

                    //                                sb.Insert(index, sb2);

                    //                            }

                    //                            sb.Replace("数量:###", srb.Items.Sum(s => s.Quantity).ToString());
                    //                            sb.Replace("金额:###", srb.Items.Sum(s => s.Amount).ToString());
                    //                        }
                    //                        #endregion

                    //                        #region tfootid

                    //                        sb.Replace("@公司地址", pCPrintSetting?.Address);
                    //                        sb.Replace("@订货电话", pCPrintSetting?.PlaceOrderTelphone);

                    //                        //收/付款方式
                    //                        var accounts = new StringBuilder();
                    //                        foreach (var acc in srb?.SaleReservationBillAccountings)
                    //                        {
                    //                            accounts.Append($"{acc?.AccountingOptionName}[{acc?.CollectionAmount ?? 0}]&nbsp;&nbsp;");
                    //                        }
                    //                        sb.Replace("@收付款方式", accounts.ToString());

                    //                        #endregion

                    //                        datas += sb;
                    //                    }
                    //                }
                    //            }
                    //        }
                    //    }
                    //    else if (d.BillTypeId == (int)BillTypeEnum.ReturnReservationBill)
                    //    {
                    //        //退货订单列表
                    //        var rrls = _returnReservationBillService
                    //        .GetReturnReservationBillsByIds(dispatchBill.Items.Where(di => di.BillTypeId == (int)BillTypeEnum.ReturnReservationBill)
                    //        .Select(di => di.BillId).ToArray(), true)?.ToList(); ;
                    //        if (rrls != null && rrls.Count > 0)
                    //        {
                    //            List<int> allProductIds = new List<int>();
                    //            rrls.ToList().ForEach(sr =>
                    //            {
                    //                if (sr.Items != null && sr.Items.Count > 0)
                    //                {
                    //                    allProductIds.AddRange(sr.Items.Select(it => it.ProductId).Distinct().ToList());
                    //                }
                    //            //去重
                    //            allProductIds = allProductIds.Distinct().ToList();
                    //            });

                    //            var allProducts = _productService.GetProductsByIds(curStore.Id, allProductIds.ToArray());
                    //            var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());


                    //            var allTerminals = _terminalService.GetTerminalsByIds(curStore.Id, rrls.Select(sr => sr.TerminalId).Distinct().ToArray());
                    //            var allWareHouses = _wareHouseService.GetWareHouseByIds(curStore.Id, rrls.Select(sr => sr.WareHouseId).Distinct().ToArray());
                    //            var allBusinessUsers = _userService.GetUsersByIds(curStore.Id, rrls.Select(sr => sr.BusinessUserId).Distinct().ToArray());

                    //            foreach (string t in printTypeList)
                    //            {
                    //                //打印整箱拆零合并单 （最小单位打印）
                    //                if (t == "1")
                    //                {
                    //                    var content = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.AllZeroMergerBill).Select(a => a.Content).FirstOrDefault();
                    //                    foreach (var rrb in rrls)
                    //                    {
                    //                        //填充打印数据
                    //                        StringBuilder sb = new StringBuilder();
                    //                        sb.Append(content);

                    //                        #region theadid
                    //                        sb.Replace("@商铺名称", curStore.Name);
                    //                        Terminal terminal = allTerminals.Where(at => at.Id == rrb.TerminalId).FirstOrDefault();
                    //                        if (terminal != null)
                    //                        {
                    //                            sb.Replace("@客户名称", terminal.Name);
                    //                        }
                    //                        WareHouse wareHouse = allWareHouses.Where(aw => aw.Id == rrb.WareHouseId).FirstOrDefault();
                    //                        if (wareHouse != null)
                    //                        {
                    //                            sb.Replace("@仓库", wareHouse.Name);
                    //                        }

                    //                        sb.Replace("@车辆", carNo);
                    //                        User businessUser = allBusinessUsers.Where(ab => ab.Id == rrb.BusinessUserId).FirstOrDefault();
                    //                        if (businessUser != null)
                    //                        {
                    //                            sb.Replace("@业务员", businessUser.UserRealName);
                    //                            sb.Replace("@业务电话", businessUser.MobileNumber);
                    //                        }
                    //                        sb.Replace("@订单编号", rrb.BillNumber);

                    //                        #endregion

                    //                        #region tbodyid
                    //                        //明细
                    //                        //获取 tbody 中的行
                    //                        int beginTbody = sb.ToString().IndexOf("<tbody>") + 7;
                    //                        int endTbody = sb.ToString().IndexOf("</tbody>");
                    //                        string tbodytr = sb.ToString()[beginTbody..endTbody];

                    //                        if (rrb.Items != null && rrb.Items.Count > 0)
                    //                        {
                    //                            //1.先删除明细第一行
                    //                            sb.Remove(beginTbody, endTbody - beginTbody);
                    //                            int i = 0;

                    //                            int bigQuantity = 0;
                    //                            int strokeQuantity = 0;
                    //                            int smallQuantity = 0;

                    //                            rrb.Items.Select(si => si.ProductId).Distinct().ToList().ForEach(pi =>
                    //                            {
                    //                                var items = rrb.Items.Where(it => it.ProductId == pi).ToList();
                    //                                Product product = allProducts.Where(ap => ap.Id == pi).FirstOrDefault();
                    //                                if (items != null && items.Count > 0 && product != null)
                    //                                {
                    //                                    int index = sb.ToString().IndexOf("</tbody>");
                    //                                    i++;
                    //                                    StringBuilder sb2 = new StringBuilder();
                    //                                    sb2.Append(tbodytr);
                    //                                    sb2.Replace("#序号", i.ToString());
                    //                                    sb2.Replace("#商品名称", product.Name);
                    //                                    int thisProductSmallQuantity = 0;
                    //                                    string thisProductSmallUnitName = "";
                    //                                    var option = allOptions.Where(sf => sf.Id == product.SmallUnitId).FirstOrDefault();
                    //                                    thisProductSmallUnitName = option == null ? "" : option.Name;
                    //                                    items.ForEach(it =>
                    //                                    {
                    //                                        if (it.UnitId == product.BigUnitId)
                    //                                        {
                    //                                            bigQuantity += it.Quantity;
                    //                                            thisProductSmallQuantity += it.Quantity * (product.BigQuantity ?? 1);
                    //                                        }
                    //                                        else if (it.UnitId == product.StrokeUnitId)
                    //                                        {
                    //                                            strokeQuantity += it.Quantity;
                    //                                            thisProductSmallQuantity += it.Quantity * (product.StrokeQuantity ?? 1);
                    //                                        }
                    //                                        else if (it.UnitId == product.SmallUnitId)
                    //                                        {
                    //                                            smallQuantity += it.Quantity;
                    //                                            thisProductSmallQuantity += it.Quantity;
                    //                                        }
                    //                                    });

                    //                                    sb2.Replace("#数量", thisProductSmallQuantity.ToString() + thisProductSmallUnitName);
                    //                                    sb.Insert(index, sb2);
                    //                                }
                    //                            });

                    //                            sb.Replace("数量(大中小):###", bigQuantity + "大" + strokeQuantity + "中" + smallQuantity + "小");
                    //                        }
                    //                        #endregion

                    //                        #region tfootid
                    //                        #endregion

                    //                        datas += sb;
                    //                    }
                    //                }
                    //                //打印整箱装车单
                    //                else if (t == "2")
                    //                {
                    //                    var content = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.AllLoadBill).Select(a => a.Content).FirstOrDefault();
                    //                    foreach (var rrb in rrls)
                    //                    {
                    //                        //填充打印数据
                    //                        StringBuilder sb = new StringBuilder();
                    //                        sb.Append(content);

                    //                        #region theadid
                    //                        sb.Replace("@商铺名称", curStore.Name);
                    //                        Terminal terminal = allTerminals.Where(at => at.Id == rrb.TerminalId).FirstOrDefault();
                    //                        if (terminal != null)
                    //                        {
                    //                            sb.Replace("@客户名称", terminal.Name);
                    //                        }
                    //                        WareHouse wareHouse = allWareHouses.Where(aw => aw.Id == rrb.WareHouseId).FirstOrDefault();
                    //                        if (wareHouse != null)
                    //                        {
                    //                            sb.Replace("@仓库", wareHouse.Name);
                    //                        }
                    //                        sb.Replace("@车辆", carNo);
                    //                        User businessUser = allBusinessUsers.Where(ab => ab.Id == rrb.BusinessUserId).FirstOrDefault();
                    //                        if (businessUser != null)
                    //                        {
                    //                            sb.Replace("@业务员", businessUser.UserRealName);
                    //                            sb.Replace("@业务电话", businessUser.MobileNumber);
                    //                        }
                    //                        sb.Replace("@订单编号", rrb.BillNumber);

                    //                        #endregion

                    //                        #region tbodyid
                    //                        //明细
                    //                        //获取 tbody 中的行
                    //                        int beginTbody = sb.ToString().IndexOf("<tbody>") + 7;
                    //                        int endTbody = sb.ToString().IndexOf("</tbody>");
                    //                        string tbodytr = sb.ToString()[beginTbody..endTbody];

                    //                        if (rrb.Items != null && rrb.Items.Count > 0)
                    //                        {
                    //                            //1.先删除明细第一行
                    //                            sb.Remove(beginTbody, endTbody - beginTbody);
                    //                            int i = 0;

                    //                            foreach (var item in rrb.Items)
                    //                            {
                    //                                int index = sb.ToString().IndexOf("</tbody>");
                    //                                i++;
                    //                                StringBuilder sb2 = new StringBuilder();
                    //                                sb2.Append(tbodytr);

                    //                                sb2.Replace("#序号", i.ToString());
                    //                                var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                    //                                if (product != null)
                    //                                {
                    //                                    sb2.Replace("#商品名称", product.Name);
                    //                                    var option = allOptions.Where(sf => sf.Id == product.SmallUnitId).FirstOrDefault();
                    //                                    sb2.Replace("#数量", item.Quantity.ToString() + (option == null ? "" : option.Name));
                    //                                }
                    //                                sb.Insert(index, sb2);
                    //                            }

                    //                        }
                    //                        #endregion

                    //                        #region tfootid
                    //                        #endregion

                    //                        datas += sb;
                    //                    }
                    //                }
                    //                //为每个客户打印拆零装车单
                    //                else if (t == "3")
                    //                {
                    //                    var content = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.ZeroLoadBill).Select(a => a.Content).FirstOrDefault();
                    //                    if (allTerminals != null && allTerminals.Count > 0)
                    //                    {
                    //                        rrls.Select(sr => sr.TerminalId).Distinct().ToList().ForEach(tid =>
                    //                        {
                    //                            Terminal terminal = allTerminals.Where(at => at.Id == tid).FirstOrDefault();
                    //                            if (terminal != null)
                    //                            {
                    //                                var returnReservations = rrls.Where(ss => ss.TerminalId == tid).ToList();
                    //                                if (returnReservations != null && returnReservations.Count > 0)
                    //                                {

                    //                                    foreach (var rrb in returnReservations)
                    //                                    {
                    //                                    //填充打印数据
                    //                                    StringBuilder sb = new StringBuilder();
                    //                                        sb.Append(content);

                    //                                    #region theadid
                    //                                    sb.Replace("@商铺名称", curStore.Name);
                    //                                        if (terminal != null)
                    //                                        {
                    //                                            sb.Replace("@客户名称", terminal.Name);
                    //                                        }
                    //                                        WareHouse wareHouse = allWareHouses.Where(aw => aw.Id == rrb.WareHouseId).FirstOrDefault();
                    //                                        if (wareHouse != null)
                    //                                        {
                    //                                            sb.Replace("@仓库", wareHouse.Name);
                    //                                        }
                    //                                        sb.Replace("@车辆", carNo);
                    //                                        User businessUser = allBusinessUsers.Where(ab => ab.Id == rrb.BusinessUserId).FirstOrDefault();
                    //                                        if (businessUser != null)
                    //                                        {
                    //                                            sb.Replace("@业务员", businessUser.UserRealName);
                    //                                            sb.Replace("@业务电话", businessUser.MobileNumber);
                    //                                        }
                    //                                        sb.Replace("@订单编号", rrb.BillNumber);

                    //                                    #endregion

                    //                                    #region tbodyid
                    //                                    //明细
                    //                                    //获取 tbody 中的行
                    //                                    int beginTbody = sb.ToString().IndexOf("<tbody>") + 7;
                    //                                        int endTbody = sb.ToString().IndexOf("</tbody>");
                    //                                        string tbodytr = sb.ToString()[beginTbody..endTbody];

                    //                                        if (rrb.Items != null && rrb.Items.Count > 0)
                    //                                        {
                    //                                        //1.先删除明细第一行
                    //                                        sb.Remove(beginTbody, endTbody - beginTbody);
                    //                                            int i = 0;
                    //                                            int bigQuantity = 0;
                    //                                            int strokeQuantity = 0;
                    //                                            int smallQuantity = 0;

                    //                                            rrb.Items.Select(si => si.ProductId).Distinct().ToList().ForEach(pi =>
                    //                                            {
                    //                                                var items = rrb.Items.Where(it => it.ProductId == pi).ToList();
                    //                                                Product product = allProducts.Where(ap => ap.Id == pi).FirstOrDefault();
                    //                                                if (items != null && items.Count > 0 && product != null)
                    //                                                {
                    //                                                    int index = sb.ToString().IndexOf("</tbody>");
                    //                                                    i++;
                    //                                                    StringBuilder sb2 = new StringBuilder();
                    //                                                    sb2.Append(tbodytr);

                    //                                                    sb2.Replace("#序号", i.ToString());
                    //                                                    sb2.Replace("#商品名称", product.Name);
                    //                                                    int thisProductSmallQuantity = 0;
                    //                                                    string thisProductSmallUnitName = "";
                    //                                                    var option = allOptions.Where(sf => sf.Id == product.SmallUnitId).FirstOrDefault();
                    //                                                    thisProductSmallUnitName = option == null ? "" : option.Name;
                    //                                                    items.ForEach(it =>
                    //                                                    {
                    //                                                        if (it.UnitId == product.BigUnitId)
                    //                                                        {
                    //                                                            bigQuantity += it.Quantity;
                    //                                                            thisProductSmallQuantity += it.Quantity * (product.BigQuantity ?? 1);
                    //                                                        }
                    //                                                        else if (it.UnitId == product.StrokeUnitId)
                    //                                                        {
                    //                                                            strokeQuantity += it.Quantity;
                    //                                                            thisProductSmallQuantity += it.Quantity * (product.StrokeQuantity ?? 1);
                    //                                                        }
                    //                                                        else if (it.UnitId == product.SmallUnitId)
                    //                                                        {
                    //                                                            smallQuantity += it.Quantity;
                    //                                                            thisProductSmallQuantity += it.Quantity;
                    //                                                        }
                    //                                                    });

                    //                                                    sb2.Replace("#数量", thisProductSmallQuantity.ToString() + thisProductSmallUnitName);
                    //                                                    sb.Insert(index, sb2);
                    //                                                }
                    //                                            });
                    //                                            sb.Replace("数量(大中小):###", bigQuantity + "大" + strokeQuantity + "中" + smallQuantity + "小");
                    //                                        }
                    //                                    #endregion

                    //                                    #region tfootid
                    //                                    #endregion

                    //                                    datas += sb;
                    //                                    }
                    //                                }
                    //                            }
                    //                        });
                    //                    }

                    //                }
                    //                //打印订单
                    //                else if (t == "4")
                    //                {
                    //                    var content = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.ReturnReservationBill).Select(a => a.Content).FirstOrDefault();
                    //                    var allMakeUsers = _userService.GetUsersByIds(curStore.Id, rrls.Select(sr => sr.MakeUserId).Distinct().ToArray());
                    //                    foreach (var rrb in rrls)
                    //                    {
                    //                        //填充打印数据
                    //                        StringBuilder sb = new StringBuilder();
                    //                        sb.Append(content);

                    //                        #region theadid
                    //                        sb.Replace("@商铺名称", curStore.Name);
                    //                        Terminal terminal = allTerminals.Where(at => at.Id == rrb.TerminalId).FirstOrDefault();
                    //                        if (terminal != null)
                    //                        {
                    //                            sb.Replace("@客户名称", terminal.Name);
                    //                        }
                    //                        User businessUser = allBusinessUsers.Where(ab => ab.Id == rrb.BusinessUserId).FirstOrDefault();
                    //                        if (businessUser != null)
                    //                        {
                    //                            sb.Replace("@业务员", businessUser.UserRealName);
                    //                            sb.Replace("@业务电话", businessUser.MobileNumber);
                    //                        }
                    //                        sb.Replace("@单据编号", rrb.BillNumber);
                    //                        sb.Replace("@交易日期", rrb.TransactionDate == null ? "" : ((DateTime)rrb.TransactionDate).ToString("yyyy/MM/dd HH:mm:ss"));

                    //                        #endregion

                    //                        #region tbodyid
                    //                        //明细
                    //                        //获取 tbody 中的行
                    //                        int beginTbody = sb.ToString().IndexOf("<tbody>") + 7;
                    //                        int endTbody = sb.ToString().IndexOf("</tbody>");
                    //                        string tbodytr = sb.ToString()[beginTbody..endTbody];

                    //                        if (rrb.Items != null && rrb.Items.Count > 0)
                    //                        {
                    //                            //1.先删除明细第一行
                    //                            sb.Remove(beginTbody, endTbody - beginTbody);
                    //                            int i = 0;
                    //                            foreach (var item in rrb.Items)
                    //                            {
                    //                                int index = sb.ToString().IndexOf("</tbody>");
                    //                                i++;
                    //                                StringBuilder sb2 = new StringBuilder();
                    //                                sb2.Append(tbodytr);

                    //                                sb2.Replace("#序号", i.ToString());
                    //                                var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                    //                                if (product != null)
                    //                                {
                    //                                    sb2.Replace("#商品名称", product.Name);
                    //                                    ProductUnitOption productUnitOption = product.GetProductUnit(_specificationAttributeService, _productService);
                    //                                    if (item.UnitId == product.SmallUnitId)
                    //                                    {
                    //                                        sb2.Replace("#条形码", product.SmallBarCode);
                    //                                        if (productUnitOption != null && productUnitOption.smallOption != null)
                    //                                        {
                    //                                            sb2.Replace("#商品单位", productUnitOption.smallOption.Name);
                    //                                        }

                    //                                    }
                    //                                    else if (item.UnitId == product.StrokeUnitId)
                    //                                    {
                    //                                        sb2.Replace("#条形码", product.StrokeBarCode);
                    //                                        if (productUnitOption != null && productUnitOption.strokOption != null)
                    //                                        {
                    //                                            sb2.Replace("#商品单位", productUnitOption.strokOption.Name);
                    //                                        }
                    //                                    }
                    //                                    else if (item.UnitId == product.BigUnitId)
                    //                                    {
                    //                                        sb2.Replace("#条形码", product.BigBarCode);
                    //                                        if (productUnitOption != null && productUnitOption.bigOption != null)
                    //                                        {
                    //                                            sb2.Replace("#商品单位", productUnitOption.bigOption.Name);
                    //                                        }
                    //                                    }

                    //                                    sb2.Replace("#单位换算", product.GetProductUnitConversion(allOptions));

                    //                                }
                    //                                sb2.Replace("#数量", item.Quantity.ToString());
                    //                                sb2.Replace("#价格", item.Price.ToString());
                    //                                sb2.Replace("#金额", item.Amount.ToString());
                    //                                sb2.Replace("#备注", item.Remark);

                    //                                sb.Insert(index, sb2);

                    //                            }

                    //                            sb.Replace("数量:###", rrb.Items.Sum(s => s.Quantity).ToString());
                    //                            sb.Replace("金额:###", rrb.Items.Sum(s => s.Amount).ToString());
                    //                        }
                    //                        #endregion

                    //                        #region tfootid

                    //                        User makeUser = _userService.GetUserById(curStore.Id, rrb.MakeUserId);
                    //                        if (makeUser != null)
                    //                        {
                    //                            sb.Replace("@制单", makeUser.UserRealName);
                    //                        }
                    //                        sb.Replace("@日期", rrb.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss"));
                    //                        sb.Replace("@公司地址", "");
                    //                        sb.Replace("@订货电话", "");
                    //                        sb.Replace("@备注", rrb.Remark);

                    //                        #endregion

                    //                        datas += sb;
                    //                    }

                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                    #endregion
                if (fg)
                {
                    return Successful("打印成功", datas);
                }
                else
                {
                    return Warning(errMsg);
                }
                #endregion

            }
            catch (Exception ex)
            {
                return Warning(ex.ToString());
            }
        }

        /// <summary>
        /// 没有汇总的明细
        /// </summary>
        /// <param name="status"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="businessUserId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<JsonResult> AsyncDetail(int? storeId, int? billId, int pageIndex = 0, int pageSize = 20)
        {
            if (!storeId.HasValue)
            {
                storeId = curStore.Id;
            }

            return await Task.Run(() =>
            {
                var dispatchItemModels = new List<DispatchItemModel>();

                var dispatchBill = _dispatchBillService.GetDispatchBillById(storeId, billId ?? 0);
                if (dispatchBill != null && dispatchBill.Items != null && dispatchBill.Items.Count > 0)
                {
                    dispatchBill.Items?.ToList()?.ForEach(db =>
                    {
                        var dim = new DispatchItemModel();

                        int bigQuantity = 0; //大单位量
                        int smallQuantity = 0; //中单位量
                        int strokeQuantity = 0; //小单位量

                        if (db.BillTypeId == (int)BillTypeEnum.ExchangeBill)
                        {
                            var bill = _exchangeBillService.GetExchangeBillById(storeId, db.BillId, true);
                            {
                                if (bill != null)
                                {
                                    dim.BillId = bill.Id;
                                    dim.BillNumber = bill.BillNumber;
                                    dim.BillTypeId = (int)BillTypeEnum.ExchangeBill;
                                    dim.BillTypeName = CommonHelper.GetEnumDescription(BillTypeEnum.ExchangeBill);
                                    dim.TransactionDate = bill.TransactionDate;
                                    dim.BusinessUserId = bill.BusinessUserId;
                                    dim.DeliveryUserId = dispatchBill.DeliveryUserId;

                                    dim.TerminalId = bill.TerminalId;

                                    var terminal = _terminalService.GetTerminalById(curStore.Id, bill.TerminalId);
                                    if (terminal != null)
                                    {
                                        dim.TerminalName = terminal.Name;
                                        dim.TerminalPointCode = terminal.Code;
                                        dim.TerminalAddress = terminal.Address;
                                    }

                                    dim.OrderAmount = bill.ReceivableAmount;
                                    dim.WareHouseId = bill.WareHouseId;
                                    dim.Remark = bill.Remark;
                                    dim.TransactionDate = bill.CreatedOnUtc;

                                    //销售商品数量
                                    if (bill.Items != null && bill.Items.Count > 0)
                                    {
                                        var allProducts = _productService
                                        .GetProductsByIds(curStore.Id, bill.Items.Select(pr => pr.ProductId)
                                        .Distinct()
                                        .ToArray());

                                        bill.Items.ToList().ForEach(s =>
                                        {
                                            var product = allProducts.Where(ap => ap.Id == s.ProductId).FirstOrDefault();
                                            if (product != null)
                                            {
                                                if (s.UnitId == product?.SmallUnitId)
                                                    smallQuantity += s.Quantity;
                                                else if (s.UnitId == product?.StrokeUnitId)
                                                    strokeQuantity += s.Quantity;
                                                else if (s.UnitId == product?.BigUnitId)
                                                    bigQuantity += s.Quantity;
                                            }
                                        });
                                    }
                                    //签收状态
                                    dim.SignStatus = db.SignStatus;

                                    dispatchItemModels.Add(dim);
                                }
                            }
                        }
                        else if (db.BillTypeId == (int)BillTypeEnum.SaleReservationBill)
                        {
                            var bill = _saleReservationBillService.GetSaleReservationBillById(storeId, db.BillId,true);
                            {
                                if (bill != null)
                                {
                                    dim.BillId = bill.Id;
                                    dim.BillNumber = bill.BillNumber;
                                    dim.BillTypeId = (int)BillTypeEnum.SaleReservationBill;
                                    dim.BillTypeName = CommonHelper.GetEnumDescription(BillTypeEnum.SaleReservationBill);
                                    dim.TransactionDate = bill.TransactionDate;
                                    dim.BusinessUserId = bill.BusinessUserId;

                                    dim.DeliveryUserId = dispatchBill.DeliveryUserId;

                                    dim.TerminalId = bill.TerminalId;

                                    Terminal terminal = _terminalService.GetTerminalById(curStore.Id, bill.TerminalId);
                                    if (terminal != null)
                                    {
                                        dim.TerminalName = terminal.Name;
                                        dim.TerminalPointCode = terminal.Code;
                                        dim.TerminalAddress = terminal.Address;
                                    }

                                    dim.OrderAmount = bill.ReceivableAmount;
                                    dim.WareHouseId = bill.WareHouseId;
                                    dim.Remark = bill.Remark;
                                    dim.TransactionDate = bill.CreatedOnUtc;
                                    
                                    //销售商品数量
                                    if (bill.Items != null && bill.Items.Count > 0)
                                    {
                                        var allProducts = _productService.GetProductsByIds(curStore.Id, bill.Items.Select(pr => pr.ProductId).Distinct().ToArray());

                                        bill.Items.ToList().ForEach(s =>
                                        {
                                            var product = allProducts.Where(ap => ap.Id == s.ProductId).FirstOrDefault();
                                            if (product != null)
                                            {
                                                if (s.UnitId == product?.SmallUnitId)
                                                    smallQuantity += s.Quantity;
                                                else if (s.UnitId == product?.StrokeUnitId)
                                                    strokeQuantity += s.Quantity;
                                                else if (s.UnitId == product?.BigUnitId)
                                                    bigQuantity += s.Quantity;
                                            }
                                        });
                                    }
                                    //签收状态
                                    dim.SignStatus = db.SignStatus;

                                    dispatchItemModels.Add(dim);
                                }
                            }
                        }
                        else if (db.BillTypeId == (int)BillTypeEnum.ReturnReservationBill)
                        {
                            var bill = _returnReservationBillService.GetReturnReservationBillById(storeId, db.BillId,true);
                            {
                                if (bill != null)
                                {
                                    dim.BillId = bill.Id;
                                    dim.BillNumber = bill.BillNumber;
                                    dim.BillTypeId = (int)BillTypeEnum.ReturnReservationBill;
                                    dim.BillTypeName = CommonHelper.GetEnumDescription(BillTypeEnum.ReturnReservationBill);
                                    dim.TransactionDate = bill.TransactionDate;
                                    dim.BusinessUserId = bill.BusinessUserId;

                                    dim.DeliveryUserId = dispatchBill.DeliveryUserId;

                                    dim.TerminalId = bill.TerminalId;

                                    var terminal = _terminalService.GetTerminalById(curStore.Id, bill.TerminalId);
                                    if (terminal != null)
                                    {
                                        dim.TerminalName = terminal.Name;
                                        dim.TerminalPointCode = terminal.Code;
                                        dim.TerminalAddress = terminal.Address;
                                    }

                                    dim.OrderAmount = bill.ReceivableAmount;
                                    dim.WareHouseId = bill.WareHouseId;
                                    dim.Remark = bill.Remark;
                                    dim.TransactionDate = bill.CreatedOnUtc;

                                    //销售商品数量
                                    if (bill.Items != null && bill.Items.Count > 0)
                                    {
                                        var allProducts = _productService.GetProductsByIds(curStore.Id, bill.Items.Select(pr => pr.ProductId).Distinct().ToArray());

                                        bill.Items.ToList().ForEach(s =>
                                        {
                                            var product = allProducts.Where(ap => ap.Id == s.ProductId).FirstOrDefault();
                                            if (product != null)
                                            {
                                                if (s.UnitId == product?.SmallUnitId)
                                                    smallQuantity += s.Quantity;
                                                else if (s.UnitId == product?.StrokeUnitId)
                                                    strokeQuantity += s.Quantity;
                                                else if (s.UnitId == product?.BigUnitId)
                                                    bigQuantity += s.Quantity;
                                            }
                                        });
                                    }

                                    //签收状态
                                    dim.SignStatus = db.SignStatus;

                                    dispatchItemModels.Add(dim);
                                }
                            }
                        }

                        dim.Id = db.Id;
                        dim.VerificationCode = db.VerificationCode; //随机验证码
                        dim.OrderQuantityView = bigQuantity + "大" + strokeQuantity + "中" + smallQuantity + "小";
                    });
                }

                //分页
                var plist = new PagedList<DispatchItemModel>(dispatchItemModels, pageIndex, pageSize);

                return Json(new
                {
                    total = plist.TotalCount,
                    rows = plist.Select(s =>
                    {
                        var m = s;
                        m.BusinessUserName = _userService.GetUserName(curStore.Id, m.BusinessUserId ?? 0);
                        m.DeliveryUserName = _userService.GetUserName(curStore.Id, m.DeliveryUserId ?? 0);
                        return m;
                    })
                });

            });
        }

        /// <summary>
        /// 编辑销售订单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Edit(int? id)
        {


            //没有值跳转到列表
            if (id == null)
            {
                return RedirectToAction("List");
            }
            var model = new DispatchBillModel();

            DispatchBill dispatchBill = _dispatchBillService.GetDispatchBillById(curStore.Id, id.Value);
            //没有值跳转到列表
            if (dispatchBill == null)
            {
                return RedirectToAction("List");
            }
            if (dispatchBill.StoreId != curStore.Id)
            {
                return RedirectToAction("List");
            }

            model = dispatchBill.ToModel<DispatchBillModel>();

            model.BillBarCode = _mediaService.GenerateBarCodeForBase64(dispatchBill.BillNumber, 150, 50);

            #region 绑定数据源

            //送货员
            model.DeliveryUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Delivers, curUser.Id, true, _userService.IsAdmin(curStore.Id, curUser.Id));
            model.DeliveryUserId = (model.DeliveryUserId);

            //车辆(车仓)
            model.Cars = BindWareHouseByTypeSelection(_wareHouseService.BindWareHouseList, curStore, (int)WareHouseType.Car, null, 0);
            model.CarId = (model.CarId);

            //制单人
            model.MakeUserName = _userService.GetUserName(curStore.Id, dispatchBill.MakeUserId) + " " + dispatchBill.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss");


            //打印
            IEnumerable<DispatchBillCreatePrintEnum> createPrintEnums = Enum.GetValues(typeof(DispatchBillCreatePrintEnum)).Cast<DispatchBillCreatePrintEnum>();
            var createPrintModes = from a in createPrintEnums
                                   select new SelectListItem
                                   {
                                       Text = CommonHelper.GetEnumDescription(a),
                                       Value = ((int)a).ToString()
                                   };
            model.DispatchBillCreatePrints = new SelectList(createPrintModes, "Value", "Text");

            #endregion

            //是否存在签收单据，存在 则不能修改、不能红冲
            model.ExistSign = false;
            if (dispatchBill != null && dispatchBill.Items != null && dispatchBill.Items.Where(di => di.SignStatus == (int)SignStatusEnum.Done).Count() > 0)
            {
                model.ExistSign = true;
            }

            return View(model);
        }

        /// <summary>
        /// 异步获取销售单项目
        /// </summary>
        /// <param name="saleId"></param>
        /// <returns></returns>
        public JsonResult AsyncDispatchItems(int dispatchBillId)
        {
            var dispatchBill = _dispatchBillService.GetDispatchBillById(curStore.Id, dispatchBillId);
            var gridModel = dispatchBill.Items.ToList();
            var details = gridModel.Select(o =>
            {
                var m = o.ToModel<DispatchItemModel>();

                //换货
                if (m.BillTypeId == (int)BillTypeEnum.ExchangeBill)
                {
                    var bill = _exchangeBillService.GetExchangeBillById(curStore.Id, m.BillId);
                    {
                        if (bill != null)
                        {
                            m.BillId = bill.Id;
                            m.BillNumber = bill.BillNumber;
                            m.BillTypeId = (int)BillTypeEnum.ExchangeBill;
                            m.BillTypeName = CommonHelper.GetEnumDescription(BillTypeEnum.ExchangeBill);
                            m.TransactionDate = bill.TransactionDate;
                            m.BusinessUserId = bill.BusinessUserId;
                            m.BusinessUserName = "";
                            //User user= _userService.GetUserById(curStore.Id, m.BusinessUserId ?? 0);

                            m.DeliveryUserId = dispatchBill.DeliveryUserId;

                            m.TerminalId = bill.TerminalId;
                            m.OrderAmount = bill.SumAmount;
                            m.WareHouseId = bill.WareHouseId;
                            m.Remark = bill.Remark;
                            m.DispatchedStatus = bill.DispatchedStatus;

                            //销售商品数量
                            if (bill.Items != null && bill.Items.Count > 0)
                            {
                                var dbpi = new List<DispatchBillProductItem>();
                                var allProducts = _productService.GetProductsByIds(curStore.Id, bill.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                                var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());

                                bill.Items.ToList().ForEach(s =>
                                {
                                    var product = allProducts.Where(ap => ap.Id == s.ProductId).FirstOrDefault();
                                    if (product != null)
                                    {
                                        //商品转化量
                                        var conversionQuantity = product.GetConversionQuantity(allOptions, s.UnitId);
                                        //库存量增量 = 单位转化量 * 数量
                                        int thisQuantity = s.Quantity * conversionQuantity;
                                        if (dbpi != null && dbpi.Where(d => d.ProductId == s.ProductId).FirstOrDefault() != null)
                                        {
                                            dbpi.Where(d => d.ProductId == s.ProductId).FirstOrDefault().Quantity += thisQuantity;
                                        }
                                        else
                                        {
                                            dbpi.Add(new DispatchBillProductItem() { ProductId = s.ProductId, Quantity = thisQuantity, BigQuantity = product.BigQuantity ?? 1 });
                                        }
                                        m.OrderQuantitySum += thisQuantity;
                                    }
                                });
                                //计算XXX大XXX小
                                if (dbpi != null && dbpi.Count > 0)
                                {
                                    int bigQuantity = 0;
                                    int smallQuantity = 0;
                                    dbpi.ForEach(d =>
                                    {
                                        bigQuantity += d.Quantity / d.BigQuantity;
                                        smallQuantity += d.Quantity % d.BigQuantity;
                                    });

                                    m.OrderQuantityView = bigQuantity + "大" + smallQuantity + "小";
                                }
                            }
                        }
                    }
                }
                //订单
                else if (m.BillTypeId == (int)BillTypeEnum.SaleReservationBill)
                {
                    var bill = _saleReservationBillService.GetSaleReservationBillById(curStore.Id, m.BillId);
                    {
                        if (bill != null)
                        {
                            m.BillId = bill.Id;
                            m.BillNumber = bill.BillNumber;
                            m.BillTypeId = (int)BillTypeEnum.SaleReservationBill;
                            m.BillTypeName = CommonHelper.GetEnumDescription(BillTypeEnum.SaleReservationBill);
                            m.TransactionDate = bill.TransactionDate;
                            m.BusinessUserId = bill.BusinessUserId;
                            m.BusinessUserName = "";
                            //User user= _userService.GetUserById(curStore.Id, m.BusinessUserId ?? 0);

                            m.DeliveryUserId = dispatchBill.DeliveryUserId;

                            m.TerminalId = bill.TerminalId;
                            m.OrderAmount = bill.SumAmount;
                            m.WareHouseId = bill.WareHouseId;
                            m.Remark = bill.Remark;
                            m.DispatchedStatus = bill.DispatchedStatus;

                            //销售商品数量
                            if (bill.Items != null && bill.Items.Count > 0)
                            {
                               var dbpi = new List<DispatchBillProductItem>();
                                var allProducts = _productService.GetProductsByIds(curStore.Id, bill.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                                var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());

                                bill.Items.ToList().ForEach(s =>
                                {
                                    var product = allProducts.Where(ap => ap.Id == s.ProductId).FirstOrDefault();
                                    if (product != null)
                                    {
                                        //商品转化量
                                        var conversionQuantity = product.GetConversionQuantity(allOptions, s.UnitId);
                                        //库存量增量 = 单位转化量 * 数量
                                        int thisQuantity = s.Quantity * conversionQuantity;
                                        if (dbpi != null && dbpi.Where(d => d.ProductId == s.ProductId).FirstOrDefault() != null)
                                        {
                                            dbpi.Where(d => d.ProductId == s.ProductId).FirstOrDefault().Quantity += thisQuantity;
                                        }
                                        else
                                        {
                                            dbpi.Add(new DispatchBillProductItem() { ProductId = s.ProductId, Quantity = thisQuantity, BigQuantity = product.BigQuantity ?? 1 });
                                        }
                                        m.OrderQuantitySum += thisQuantity;
                                    }
                                });
                                //计算XXX大XXX小
                                if (dbpi != null && dbpi.Count > 0)
                                {
                                    int bigQuantity = 0;
                                    int smallQuantity = 0;
                                    dbpi.ForEach(d =>
                                    {
                                        bigQuantity += d.Quantity / d.BigQuantity;
                                        smallQuantity += d.Quantity % d.BigQuantity;
                                    });

                                    m.OrderQuantityView = bigQuantity + "大" + smallQuantity + "小";
                                }
                            }
                        }
                    }
                }
                //退订
                else if (m.BillTypeId == (int)BillTypeEnum.ReturnReservationBill)
                {
                    var bill = _returnReservationBillService.GetReturnReservationBillById(curStore.Id, m.BillId);
                    {
                        if (bill != null)
                        {
                            m.BillId = bill.Id;
                            m.BillNumber = bill.BillNumber;
                            m.BillTypeId = (int)BillTypeEnum.ReturnReservationBill;
                            m.BillTypeName = CommonHelper.GetEnumDescription(BillTypeEnum.ReturnReservationBill);
                            m.TransactionDate = bill.TransactionDate;
                            m.BusinessUserId = bill.BusinessUserId;

                            m.DeliveryUserId = dispatchBill.DeliveryUserId;

                            m.TerminalId = bill.TerminalId;
                            m.OrderAmount = bill.SumAmount;
                            m.WareHouseId = bill.WareHouseId;
                            m.Remark = bill.Remark;
                            m.DispatchedStatus = bill.DispatchedStatus;

                            //销售商品数量
                            if (bill.Items != null && bill.Items.Count > 0)
                            {
                               var dbpi = new List<DispatchBillProductItem>();
                                var allProducts = _productService.GetProductsByIds(curStore.Id, bill.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                                var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());

                                bill.Items.ToList().ForEach(s =>
                                {
                                    var product = allProducts.Where(ap => ap.Id == s.ProductId).FirstOrDefault();
                                    if (product != null)
                                    {
                                        //商品转化量
                                        var conversionQuantity = product.GetConversionQuantity(allOptions, s.UnitId);
                                        //库存量增量 = 单位转化量 * 数量
                                        int thisQuantity = s.Quantity * conversionQuantity;
                                        if (dbpi != null && dbpi.Where(d => d.ProductId == s.ProductId).FirstOrDefault() != null)
                                        {
                                            dbpi.Where(d => d.ProductId == s.ProductId).FirstOrDefault().Quantity += thisQuantity;
                                        }
                                        else
                                        {
                                            dbpi.Add(new DispatchBillProductItem() { ProductId = s.ProductId, Quantity = thisQuantity, BigQuantity = product.BigQuantity ?? 1 });
                                        }
                                        m.OrderQuantitySum += thisQuantity;
                                    }
                                });
                                //计算XXX大XXX小
                                if (dbpi != null && dbpi.Count > 0)
                                {
                                    int bigQuantity = 0;
                                    int smallQuantity = 0;
                                    dbpi.ForEach(d =>
                                    {
                                        bigQuantity += d.Quantity / d.BigQuantity;
                                        smallQuantity += d.Quantity % d.BigQuantity;
                                    });

                                    m.OrderQuantityView = bigQuantity + "大" + smallQuantity + "小";
                                }
                            }
                        }
                    }
                }

                return m;

            }).ToList();

            return Json(new
            {
                //total = details.Count,
                //rows = details
                total = details.Count,
                rows = details.Select(s =>
                {
                    s.BusinessUserName = _userService.GetUserName(curStore.Id, s.BusinessUserId ?? 0);
                    //客户名称
                    Terminal terminal = _terminalService.GetTerminalById(curStore.Id, s.TerminalId ?? 0);
                    if (terminal != null)
                    {
                        s.TerminalName = terminal.Name;
                        s.TerminalAddress = terminal.Address;
                    }
                    return s;
                })
            });
        }

        /// <summary>
        /// 设置仓库
        /// </summary>
        /// <returns></returns>
        public JsonResult SetWareHouse(string data)
        {

            DispatchSetWareHouseModel model = new DispatchSetWareHouseModel();

            #region 绑定数据源

            //所有仓库
            model.WareHouses = BindWareHouseSelection(_wareHouseService.BindWareHouseList, curStore,null,0);
            model.WareHouseId = (model.WareHouseId);

            #endregion

            model.SelectDatas = data;

            return Json(new
            {
                Success = true,
                RenderHtml = RenderPartialViewToString("SetWareHouse", model)
            });
        }

        [HttpPost]
        public JsonResult SetWareHouse(DispatchSetWareHouseModel model)
        {

            try
            {

                string errMsg = string.Empty;

                #region 验证

                if (model.WareHouseId == 0)
                {
                    errMsg = "没有选择仓库时";
                }

                #endregion

                if (!string.IsNullOrEmpty(errMsg))
                {
                    return Warning(errMsg);
                }
                else
                {
                    //using (var scope = new TransactionScope())
                    //{
                    //当前经销商的销售订单
                    IList<SaleReservationBill> saleReservationBills = _saleReservationBillService.GetSaleReservationBillsNullWareHouseByStoreId(curStore.Id);
                    if (saleReservationBills != null && saleReservationBills.Count > 0)
                    {
                        saleReservationBills.ToList().ForEach(sb =>
                        {
                            sb.WareHouseId = model.WareHouseId;
                            _saleReservationBillService.UpdateSaleReservationBill(sb);
                        });
                    }

                    //当前经销商的退货订单
                    IList<ReturnReservationBill> returnReservationBills = _returnReservationBillService.GetReturnReservationBillsNullWareHouseByStoreId(curStore.Id);
                    if (returnReservationBills != null && returnReservationBills.Count > 0)
                    {
                        returnReservationBills.ToList().ForEach(sb =>
                        {
                            sb.WareHouseId = model.WareHouseId;
                            _returnReservationBillService.UpdateReturnReservationBill(sb);
                        });
                    }

                    //scope.Complete();
                    //}
                }

            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("CreateOrUpdate", Resources.Bill_CreateOrUpdateFailed, curUser.Id);
                _notificationService.SuccessNotification(Resources.Bill_CreateOrUpdateFailed);
                return Error(ex.Message);
            }

            _userActivityService.InsertActivity("CreateOrUpdate", Resources.Bill_CreateOrUpdateSuccessful, curUser.Id);
            _notificationService.SuccessNotification(Resources.Bill_CreateOrUpdateSuccessful);
            return Json(new { Success = true, Message = Resources.Bill_CreateOrUpdateSuccessful });
        }

        /// <summary>
        /// 重置验证码
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Reset(int? id)
        {
            //没有值跳转到列表
            if (id == null)
            {
                return RedirectToAction("List");
            }

            var dispatchItem = _dispatchBillService.GetDispatchItemsById(curStore.Id, id ?? 0);

            if (dispatchItem != null)
            {
                dispatchItem.VerificationCode = CommonHelper.GenerateNumber(6); //6位随机验证码
                _dispatchBillService.UpdateDispatchItem(dispatchItem);
            }

            return RedirectToAction("List");
        }

        /// <summary>
        /// 发送验证码
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Send(int? id)
        {
            //没有值跳转到列表
            if (id == null)
            {
                return RedirectToAction("List");
            }

            var dispatchItem = _dispatchBillService.GetDispatchItemsById(curStore.Id, id ?? 0);

            if (dispatchItem != null)
            {
                var terminalMobile = _terminalService.GetTerminalById(curStore.Id, dispatchItem.TerminalId)?.BossCall;
                var messageContent = "【经销商管家】验证码：" + dispatchItem.VerificationCode + ",该验证码仅用于送货签收，请勿泄露给任何人.";
                _dispatchBillService.SendMessage(terminalMobile, messageContent);
            }

            return RedirectToAction("List");
        }

    }
}