using DCMS.Core;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.Sales;
using DCMS.Core.Domain.Terminals;
using DCMS.Core.Domain.Users;
using DCMS.Core.Domain.WareHouses;
using DCMS.Services.Configuration;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Products;
using DCMS.Services.Sales;
using DCMS.Services.Terminals;
using DCMS.Services.Users;
using DCMS.Services.WareHouses;
using DCMS.ViewModel.Models.Sales;
using DCMS.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace DCMS.Web.Controllers
{
    /// <summary>
    /// 车辆对货单
    /// </summary>
    public class CarGoodBillController : BasePublicController
    {
        private readonly ISettingService _settingService;
        private readonly IPrintTemplateService _printTemplateService;
        private readonly IUserService _userService;
        private readonly ISaleReservationBillService _saleReservationService;
        private readonly ISaleBillService _saleBillService;
        private readonly IReturnReservationBillService _returnReservationService;
        private readonly IReturnBillService _returnBillService;
        public readonly IProductService _productService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IWareHouseService _wareHouseService;
        private readonly ITerminalService _terminalService;
        private readonly IDispatchBillService _dispatchBillService;

        public CarGoodBillController(
            IWorkContext workContext,
            IStoreContext storeContext,
            ISettingService settingService,
            IPrintTemplateService printTemplateService,
            IUserService userService,
            ISaleReservationBillService saleReservationService,
            ISaleBillService saleBillService,
            IReturnReservationBillService returnReservationService,
            IReturnBillService returnBillService,
            IProductService productService,
            ISpecificationAttributeService specificationAttributeService,
            IWareHouseService wareHouseService,
            ITerminalService terminalService,
            IDispatchBillService dispatchBillService,
            ILogger loggerService,
            INotificationService notificationService
            ) : base(workContext, loggerService, storeContext, notificationService)
        {
            _settingService = settingService;
            _printTemplateService = printTemplateService;
            _userService = userService;
            _saleReservationService = saleReservationService;
            _saleBillService = saleBillService;
            _returnReservationService = returnReservationService;
            _returnBillService = returnBillService;
            _productService = productService;
            _specificationAttributeService = specificationAttributeService;
            _wareHouseService = wareHouseService;
            _terminalService = terminalService;
            _dispatchBillService = dispatchBillService;

        }

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        [AuthCode((int)AccessGranularityEnum.VehicleToGoodsView)]
        public IActionResult List(int? deliveryUserId, DateTime? startTime = null, DateTime? endTime = null, int pagenumber = 0)
        {
            CarGoodBillListModel model = new CarGoodBillListModel();

            #region 绑定数据源
            //送货员
            var userlists = new List<SelectListItem>();
            var users = _userService.BindUserList(curStore.Id, DCMSDefaults.Delivers).ToList();
            users.ForEach(u =>
            {
                userlists.Add(new SelectListItem() { Text = u.UserRealName, Value = u.Id.ToString() });
            });
            model.DeliveryUsers = new SelectList(userlists, "Value", "Text");
            model.DeliveryUserId = deliveryUserId ?? null;

            model.StartTime = startTime ?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01"));
            model.EndTime = endTime ?? DateTime.Now.AddDays(1);
            #endregion

            List<CarGoodBillModel> carGoodModels = new List<CarGoodBillModel>();

            //销售订单
            IList<SaleReservationBill> saleReservations = new List<SaleReservationBill>();
            saleReservations = _saleReservationService.GetSaleReservationBillsToCarGood(curStore.Id, curUser.Id, deliveryUserId ?? 0, model.StartTime, model.EndTime);
            //退货订单
            IList<ReturnReservationBill> returnReservations = new List<ReturnReservationBill>();
            returnReservations = _returnReservationService.GetReturnReservationBillsToCarGood(curStore.Id, curUser.Id, deliveryUserId ?? 0, model.StartTime, model.EndTime);

            //查询关联的商品
            List<int> allProductIds = new List<int>();
            //销售订单关联商品、关联销售单
            IList<SaleBill> allSaleBills = new List<SaleBill>();
            if (saleReservations != null && saleReservations.Count > 0)
            {
                saleReservations.ToList().ForEach(sr =>
                {
                    if (sr.Items != null && sr.Items.Count > 0)
                    {
                        allProductIds.AddRange(sr.Items.Select(it => it.ProductId).Distinct().ToList());
                    }
                    //去重
                    allProductIds = allProductIds.Distinct().ToList();
                });
                allSaleBills = _saleBillService.GetSaleBillsBySaleReservationIds(curStore.Id, saleReservations.Select(rr => rr.Id).ToArray());
            }
            //退货订单关联商品、关联退货单
            IList<ReturnBill> allReturnBills = new List<ReturnBill>();
            if (returnReservations != null && returnReservations.Count > 0)
            {
                returnReservations.ToList().ForEach(rr =>
                {
                    if (rr.Items != null && rr.Items.Count > 0)
                    {
                        allProductIds.AddRange(rr.Items.Select(it => it.ProductId).Distinct().ToList());
                    }
                    //去重
                    allProductIds = allProductIds.Distinct().ToList();
                });

                allReturnBills = _returnBillService.GetReturnBillsByReturnReservationIds(curStore.Id, returnReservations.Select(rr => rr.Id).ToArray());
            }
            var allProducts = _productService.GetProductsByIds(curStore.Id, allProductIds.ToArray());
            var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());

            if (saleReservations != null && saleReservations.Count > 0)
            {
                //销售订单主表
                saleReservations.ToList().ForEach(a =>
                {
                    if (a.Items != null && a.Items.Count > 0)
                    {
                        //销售订单商品
                        var allSaleReservationProducts = allProducts.Where(ap => a.Items.Select(pr => pr.ProductId).Distinct().Contains(ap.Id)).ToList();

                        if (allSaleReservationProducts != null && allSaleReservationProducts.Count > 0)
                        {

                            //销售订单对应的销售单:获取拒收商品数量
                            IList<SaleBill> saleBills = allSaleBills.Where(asb => asb.Id == a.Id).ToList();
                            List<SaleItem> saleItems = new List<SaleItem>();
                            if (saleBills != null && saleBills.Count > 0)
                            {
                                saleBills.ToList().ForEach(sb =>
                                {
                                    if (sb.Items != null && sb.Items.Count > 0)
                                    {
                                        saleItems.AddRange(sb.Items);
                                    }
                                });
                            }

                            foreach (var product in allSaleReservationProducts)
                            {
                                List<SaleReservationItem> items = a.Items.Where(q => q.ProductId == product.Id).ToList();
                                if (items != null && items.Count > 0)
                                {
                                    int thisQuantity = 0;
                                    items.ForEach(it =>
                                    {
                                        //商品转化量
                                        var conversionQuantity = product.GetConversionQuantity(allOptions, it.UnitId);
                                        //最小单位数量
                                        thisQuantity += it.Quantity * conversionQuantity;
                                    });

                                    CarGoodBillModel carGoodModel = new CarGoodBillModel
                                    {
                                        StoreId = a.StoreId,
                                        BillId = a.Id,
                                        BillNumber = a.BillNumber,
                                        BillType = (int)BillTypeEnum.SaleReservationBill,
                                        BillTypeName = CommonHelper.GetEnumDescription<BillTypeEnum>(BillTypeEnum.SaleReservationBill),
                                        BusinessUserId = a.BusinessUserId,
                                        BusinessUserName = "",
                                        DeliveryUserId = (saleBills != null && saleBills.Count > 0) ? saleBills[0].DeliveryUserId : 0,
                                        DeliveryUserName = "",
                                        TerminalId = a.TerminalId,
                                        TerminalName = "",
                                        ChangeDate = a.CreatedOnUtc,
                                        WareHouseId = a.WareHouseId,
                                        ProductId = product.Id,
                                        ProductName = product.Name,

                                        SaleReservationBillQty = thisQuantity
                                    };
                                    //先初始 拒收数量等于销售数量
                                    carGoodModel.RefuseQty = carGoodModel.SaleReservationBillQty;
                                    carGoodModel.SaleBillQty = 0;

                                    #region 拒收数量
                                    if (saleItems != null && saleItems.Count > 0)
                                    {
                                        List<SaleItem> thisSaleItems = saleItems.Where(sa => sa.ProductId == product.Id).ToList();
                                        if (thisSaleItems != null && thisSaleItems.Count > 0)
                                        {
                                            int thisSaleQuantity = 0;
                                            thisSaleItems.ForEach(tsi =>
                                            {
                                                //商品转化量
                                                var conversionQuantity = product.GetConversionQuantity(allOptions, tsi.UnitId);
                                                //最小单位数量
                                                thisSaleQuantity += tsi.Quantity * conversionQuantity;
                                            });
                                            //如果：销售单数量 < 销售订单数量 拒收数量 = 销售订单数量 - 销售单数量
                                            if (thisSaleQuantity < carGoodModel.SaleReservationBillQty)
                                            {
                                                carGoodModel.RefuseQty = carGoodModel.SaleReservationBillQty - thisSaleQuantity;
                                            }
                                            //如果：销售单数量 >= 销售订单数量 拒收数量 = 0
                                            else
                                            {
                                                carGoodModel.RefuseQty = 0;
                                            }

                                        }
                                    }
                                    #endregion

                                    carGoodModel.ReturnReservationBillQty = 0;
                                    carGoodModel.ReturnRealQty = 0;

                                    carGoodModels.Add(carGoodModel);
                                }

                            }
                        }

                    }

                });
            }
            if (returnReservations != null && returnReservations.Count > 0)
            {
                //退货订单主表
                returnReservations.ToList().ForEach(a =>
                {
                    if (a.Items != null && a.Items.Count > 0)
                    {
                        //退货订单商品
                        var allReturnReservationProducts = allProducts.Where(ap => a.Items.Select(pr => pr.ProductId).Distinct().Contains(ap.Id)).ToList();

                        if (allReturnReservationProducts != null && allReturnReservationProducts.Count > 0)
                        {

                            //退货订单对应的退货单:获取退货商品数量
                            ReturnBill returnBill = allReturnBills.Where(arb => arb.Id == a.Id).FirstOrDefault();
                            List<ReturnItem> returnItems = new List<ReturnItem>();
                            if (returnBill != null)
                            {
                                returnItems.AddRange(returnBill.Items);
                            }

                            foreach (var product in allReturnReservationProducts)
                            {
                                List<ReturnReservationItem> items = a.Items.Where(q => q.ProductId == product.Id).ToList();
                                if (items != null && items.Count > 0)
                                {
                                    int thisQuantity = 0;
                                    items.ForEach(it =>
                                    {
                                        //商品转化量
                                        var conversionQuantity = product.GetConversionQuantity(allOptions, it.UnitId);
                                        //最小单位数量
                                        thisQuantity += it.Quantity * conversionQuantity;
                                    });

                                    CarGoodBillModel carGoodModel = new CarGoodBillModel
                                    {
                                        StoreId = a.StoreId,
                                        BillId = a.Id,
                                        BillNumber = a.BillNumber,
                                        BillType = (int)BillTypeEnum.ReturnReservationBill,
                                        BillTypeName = CommonHelper.GetEnumDescription<BillTypeEnum>(BillTypeEnum.ReturnReservationBill),
                                        BusinessUserId = a.BusinessUserId,
                                        BusinessUserName = "",
                                        DeliveryUserId = returnBill != null ? returnBill.DeliveryUserId : 0,
                                        DeliveryUserName = "",
                                        TerminalId = a.TerminalId,
                                        TerminalName = "",
                                        ChangeDate = a.CreatedOnUtc,
                                        WareHouseId = a.WareHouseId,
                                        ProductId = product.Id,
                                        ProductName = product.Name,

                                        SaleReservationBillQty = 0,
                                        SaleBillQty = 0,
                                        RefuseQty = 0,

                                        ReturnReservationBillQty = thisQuantity,

                                        ReturnRealQty = 0
                                    };

                                    #region 退货单数量、退货数量 都为实际退货单数量
                                    if (returnItems != null && returnItems.Count > 0)
                                    {
                                        List<ReturnItem> thisReturnItems = returnItems.Where(sa => sa.ProductId == product.Id).ToList();
                                        if (thisReturnItems != null && thisReturnItems.Count > 0)
                                        {
                                            int thisReturnQuantity = 0;
                                            thisReturnItems.ForEach(tsi =>
                                            {
                                                //商品转化量
                                                var conversionQuantity = product.GetConversionQuantity(allOptions, tsi.UnitId);
                                                //最小单位数量
                                                thisReturnQuantity += tsi.Quantity * conversionQuantity;
                                            });
                                            carGoodModel.ReturnBillQty = thisReturnQuantity;
                                            carGoodModel.ReturnRealQty = thisReturnQuantity;

                                        }
                                    }
                                    #endregion

                                    carGoodModels.Add(carGoodModel);
                                }

                            }
                        }
                    }
                });
            }

            //拒收、退货大于0 暂时都显示
            //carGoodModels = carGoodModels.Where(q => q.RefuseQty > 0 || q.ReturnRealQty > 0).ToList();

            //分页
            IPagedList<CarGoodBillModel> plist = new PagedList<CarGoodBillModel>(carGoodModels, pagenumber, 30);
            model.PagingFilteringContext.LoadPagedList(plist);

            var allTerminals = _terminalService.GetTerminalsByIds(curStore.Id, plist.Select(p => p.TerminalId).Distinct().ToArray());
            var allWareHouses = _wareHouseService.GetWareHouseByIds(curStore.Id, plist.Select(p => p.WareHouseId).ToArray());
            var userIds = plist.Select(p => p.BusinessUserId ?? 0).ToList();
            userIds.AddRange(plist.Select(p => p.BusinessUserId ?? 0).ToList());

            var allUsers = _userService.GetUsersDictsByIds(curStore.Id, userIds.Distinct().ToArray());

            model.Lists = plist.Select(p =>
            {
                var terminal = allTerminals.Where(at => at.Id == p.TerminalId).FirstOrDefault();
                p.TerminalName = terminal == null ? "" : terminal.Name;
                var wareHouse = allWareHouses.Where(aw => aw.Id == p.WareHouseId).FirstOrDefault();
                p.WareHouseName = wareHouse == null ? "" : wareHouse.Name;

                //业务员名称
                p.BusinessUserName = allUsers.Where(aw => aw.Key == p.BusinessUserId).Select(aw => aw.Value).FirstOrDefault();
                //送货员名称
                p.DeliveryUserName = allUsers.Where(aw => aw.Key == p.DeliveryUserId).Select(aw => aw.Value).FirstOrDefault();


                p.CarId = _dispatchBillService.GetCarId(p.BillType, p.BillId);
                if (p.CarId != 0)
                {
                    p.CarName = _wareHouseService.GetWareHouseName(curStore.Id, p.CarId);
                }

                return p;
            }).ToList();

            return View(model);
        }

        /// <summary>
        /// 查询车辆对货单
        /// </summary>
        /// <param name="deliveryUserId">送货员</param>
        /// <param name="start">开始日期</param>
        /// <param name="end">结束日期</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.VehicleToGoodsView)]
        public async Task<JsonResult> AsyncList(int? deliveryUserId = null, DateTime? start = null, DateTime? end = null, int pageIndex = 0, int pageSize = 20)
        {

            return await Task.Run(() =>
            {

                List<CarGoodBillModel> carGoodModels = new List<CarGoodBillModel>();

                //销售订单
                IList<SaleReservationBill> saleReservations = new List<SaleReservationBill>();
                saleReservations = _saleReservationService.GetSaleReservationBillsToCarGood(curStore.Id, curUser.Id, deliveryUserId ?? 0, start, end);

                //退货订单
                IList<ReturnReservationBill> returnReservations = new List<ReturnReservationBill>();
                returnReservations = _returnReservationService.GetReturnReservationBillsToCarGood(curStore.Id, curUser.Id, deliveryUserId ?? 0, start, end);

                //查询关联的商品
                List<int> allProductIds = new List<int>();
                //销售订单关联商品
                if (saleReservations != null && saleReservations.Count > 0)
                {
                    saleReservations.ToList().ForEach(sr =>
                    {
                        if (sr.Items != null && sr.Items.Count > 0)
                        {
                            allProductIds.AddRange(sr.Items.Select(it => it.ProductId).Distinct().ToList());
                        }
                        //去重
                        allProductIds = allProductIds.Distinct().ToList();
                    });
                }
                //退货订单关联商品
                if (returnReservations != null && returnReservations.Count > 0)
                {
                    returnReservations.ToList().ForEach(rr =>
                    {
                        if (rr.Items != null && rr.Items.Count > 0)
                        {
                            allProductIds.AddRange(rr.Items.Select(it => it.ProductId).Distinct().ToList());
                        }
                        //去重
                        allProductIds = allProductIds.Distinct().ToList();
                    });
                }
                var allProducts = _productService.GetProductsByIds(curStore.Id, allProductIds.ToArray());
                var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());

                if (saleReservations != null && saleReservations.Count > 0)
                {
                    //销售订单主表
                    saleReservations.ToList().ForEach(a =>
                    {
                        if (a.Items != null && a.Items.Count > 0)
                        {
                            foreach (SaleReservationItem item in a.Items)
                            {
                                //这里要按 商品Id 分组
                                var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                                CarGoodBillModel carGoodModel = carGoodModels.Where(c => c.ProductId == item.ProductId && c.BillNumber == a.BillNumber).FirstOrDefault();
                                //商品转化量
                                var conversionQuantity = product.GetConversionQuantity(allOptions, item.UnitId);
                                //库存量增量 = 单位转化量 * 数量
                                int thisQuantity = item.Quantity * conversionQuantity;
                                if (carGoodModel != null)
                                {
                                    carGoodModel.SaleReservationBillQty += thisQuantity;
                                }
                                else
                                {
                                    carGoodModel = new CarGoodBillModel
                                    {

                                        StoreId = a.StoreId,
                                        BillId = a.Id,
                                        BillNumber = a.BillNumber,
                                        BillType = (int)BillTypeEnum.SaleReservationBill,
                                        BillTypeName = CommonHelper.GetEnumDescription<BillTypeEnum>(BillTypeEnum.SaleReservationBill),
                                        DeliveryUserId = a.BusinessUserId,
                                        DeliveryUserName = "",
                                        TerminalId = a.TerminalId,
                                        TerminalName = "",
                                        ChangeDate = a.CreatedOnUtc,
                                        WareHouseId = a.WareHouseId,
                                        ProductId = item.ProductId,
                                        ProductName = product.Name,
                                        SaleReservationBillQty = thisQuantity,
                                        ReturnReservationBillQty = 0,

                                        ReturnRealQty = 0
                                    };

                                    //获取当前 销售订单 对应的 销售单
                                    SaleBill saleBill = _saleBillService.GetSaleBillBySaleReservationBillId(curStore.Id, a.Id);
                                    if (saleBill != null && saleBill.AuditedStatus == true && saleBill.ReversedStatus == false)
                                    {
                                        carGoodModel.Id = saleBill.Id;
                                        carGoodModel.BillNumber = saleBill.BillNumber;
                                        //销售单数量 根据商品分组
                                        carGoodModel.SaleBillQty = 0;
                                        if (saleBill.Items != null && saleBill.Items.Count > 0)
                                        {
                                            foreach (SaleItem saleItem in saleBill.Items)
                                            {
                                                if (saleItem.ProductId == item.ProductId)
                                                {
                                                    //商品转化量
                                                    var conversionQuantity2 = product.GetConversionQuantity(allOptions, saleItem.UnitId);
                                                    //库存量增量 = 单位转化量 * 数量
                                                    int thisQuantity2 = saleItem.Quantity * conversionQuantity2;
                                                    carGoodModel.SaleBillQty += thisQuantity2;
                                                }
                                            }
                                        }

                                        carGoodModels.Add(carGoodModel);
                                    }
                                }

                            }
                        }

                    });
                }
                if (returnReservations != null && returnReservations.Count > 0)
                {
                    //退货订单主表
                    returnReservations.ToList().ForEach(a =>
                    {
                        if (a.Items != null && a.Items.Count > 0)
                        {

                            foreach (ReturnReservationItem item in a.Items)
                            {
                                //这里要按 商品Id 分组
                                var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                                CarGoodBillModel carGoodModel = carGoodModels.Where(c => c.ProductId == item.ProductId && c.BillNumber == a.BillNumber).FirstOrDefault();
                                //商品转化量
                                var conversionQuantity = product.GetConversionQuantity(allOptions, item.UnitId);
                                //库存量增量 = 单位转化量 * 数量
                                int thisQuantity = item.Quantity * conversionQuantity;
                                if (carGoodModel != null)
                                {
                                    carGoodModel.SaleReservationBillQty += thisQuantity;
                                }
                                else
                                {
                                    carGoodModel = new CarGoodBillModel
                                    {
                                        StoreId = a.StoreId,
                                        BillId = a.Id,
                                        BillNumber = a.BillNumber,
                                        BillType = (int)BillTypeEnum.ReturnReservationBill,
                                        BillTypeName = CommonHelper.GetEnumDescription<BillTypeEnum>(BillTypeEnum.ReturnReservationBill),
                                        DeliveryUserId = a.BusinessUserId,
                                        DeliveryUserName = "",
                                        TerminalId = a.TerminalId,
                                        TerminalName = "",
                                        ChangeDate = a.CreatedOnUtc,
                                        WareHouseId = a.WareHouseId,
                                        ProductId = item.ProductId,
                                        ProductName = product.Name,
                                        SaleReservationBillQty = 0,
                                        ReturnReservationBillQty = thisQuantity
                                    };

                                    //获取当前 退货订单 对应的 退货单
                                    ReturnBill returnBill = _returnBillService.GetReturnBillByReturnReservationBillId(curStore.Id, a.Id);
                                    if (returnBill != null && returnBill.AuditedStatus == true && returnBill.ReversedStatus == false)
                                    {
                                        carGoodModel.Id = returnBill.Id;
                                        carGoodModel.BillNumber = returnBill.BillNumber;
                                        //退货单数量 根据商品分组
                                        carGoodModel.SaleBillQty = 0;
                                        if (returnBill.Items != null && returnBill.Items.Count > 0)
                                        {
                                            foreach (ReturnItem returnItem in returnBill.Items)
                                            {
                                                if (returnItem.ProductId == item.ProductId)
                                                {
                                                    //商品转化量
                                                    var conversionQuantity2 = product.GetConversionQuantity(allOptions, returnItem.UnitId);
                                                    //库存量增量 = 单位转化量 * 数量
                                                    int thisQuantity2 = returnItem.Quantity * conversionQuantity2;
                                                    carGoodModel.ReturnBillQty += thisQuantity2;
                                                }
                                            }
                                        }

                                        carGoodModels.Add(carGoodModel);
                                    }
                                }

                            }
                        }
                    });
                }

                //分页
                IPagedList<CarGoodBillModel> plist = new PagedList<CarGoodBillModel>(carGoodModels, pageIndex, pageSize);
                var Products = _productService.GetProductsByIds(curStore.Id, plist.Select(pr => pr.ProductId).Distinct().ToArray());

                return Json(new
                {
                    Success = true,
                    total = plist.TotalCount,
                    rows = plist.Select(s =>
                    {
                        var m = s;
                        User user = _userService.GetUserById(curStore.Id, m.DeliveryUserId ?? 0);
                        if (user != null)
                        {
                            m.DeliveryUserName = user.UserRealName;
                        }
                        Terminal terminal = _terminalService.GetTerminalById(curStore.Id, m.TerminalId);
                        if (terminal != null)
                        {
                            m.TerminalName = terminal.Name;
                        }
                        WareHouse wareHouse = _wareHouseService.GetWareHouseById(curStore.Id, m.WareHouseId);
                        if (wareHouse != null)
                        {
                            m.WareHouseName = wareHouse.Name;
                        }

                        var product = Products.Where(ap => ap.Id == m.ProductId).FirstOrDefault();
                        if (product != null)
                        {
                            m.ProductName = product.Name;
                        }

                        //销售订单 
                        if (m.BillType == (int)BillTypeEnum.SaleReservationBill)
                        {
                            //拒收 = 销售订单数量
                            //拒收数量是实际要返回给仓库的
                            m.RefuseQty = m.SaleReservationBillQty;
                        }
                        //退货单的数量是实际返回给仓库的
                        m.ReturnRealQty = m.ReturnBillQty;
                        return m;
                    })

                });

            });

        }


        /// <summary>
        /// 获取用户选择单据的商品信息
        /// </summary>
        /// <param name="selectDatas"></param>
        /// <returns></returns>
        public JsonResult AsyncProductList(string selectDatas)
        {
            //[0] 单据类型
            //[1] 主表 ID
            //[2] 商品 ID
            //1_25_13,1_43_73,1_43_74,1_50_84,1_51_77,1_51_80

            List<CarGoodDetailModel> carGoodDetailModels = new List<CarGoodDetailModel>();

            if (!string.IsNullOrEmpty(selectDatas))
            {
                List<string> lists = selectDatas.Split(',').ToList();
                if (lists != null && lists.Count > 0)
                {
                    lists.ForEach(a =>
                    {
                        List<string> lists2 = a.Split('_').ToList();
                        if (lists2 != null && lists2.Count == 3)
                        {
                            int billTypeId = int.Parse(lists2[0]);
                            int reservationId = int.Parse(lists2[1]);
                            int reservationDetailId = int.Parse(lists2[2]);
                            //销售订单
                            if (billTypeId == (int)BillTypeEnum.SaleReservationBill)
                            {
                                SaleReservationItem saleReservationDetail = _saleReservationService.GetSaleReservationItemById(reservationDetailId);
                                if (saleReservationDetail != null)
                                {
                                    CarGoodDetailModel carGoodDetailModel = new CarGoodDetailModel
                                    {
                                        ProductId = saleReservationDetail.ProductId
                                    };
                                    Product product = _productService.GetProductById(curStore.Id, saleReservationDetail.ProductId);
                                    if (product != null)
                                    {
                                        carGoodDetailModel.ProductName = product.Name;
                                    }
                                    carGoodDetailModel.RefuseQty = saleReservationDetail.Quantity;
                                    carGoodDetailModel.ReturnRealQty = 0;
                                    carGoodDetailModel.Total = saleReservationDetail.Quantity;

                                    carGoodDetailModels.Add(carGoodDetailModel);
                                }

                            }
                            //退货订单
                            if (billTypeId == (int)BillTypeEnum.ReturnReservationBill)
                            {
                                ReturnReservationItem returnReservationDetail = _returnReservationService.GetReturnReservationItemById(reservationDetailId);
                                //
                                if (returnReservationDetail != null)
                                {
                                    CarGoodDetailModel carGoodDetailModel = new CarGoodDetailModel
                                    {
                                        ProductId = returnReservationDetail.ProductId
                                    };
                                    Product product = _productService.GetProductById(curStore.Id, returnReservationDetail.ProductId);
                                    if (product != null)
                                    {
                                        carGoodDetailModel.ProductName = product.Name;
                                    }
                                    carGoodDetailModel.RefuseQty = 0;
                                    carGoodDetailModel.ReturnRealQty = returnReservationDetail.Quantity;
                                    carGoodDetailModel.Total = returnReservationDetail.Quantity;

                                    carGoodDetailModels.Add(carGoodDetailModel);
                                }
                            }
                        }
                    });
                }
            }

            return Json(new
            {
                total = carGoodDetailModels.Count,
                rows = carGoodDetailModels
            });
        }


        /// <summary>
        /// 打印
        /// </summary>
        /// <param name="selectData"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.VehicleToGoodsPrint)]
        public JsonResult Print(string selectData)
        {
            try
            {

                bool fg = true;
                string errMsg = string.Empty;

                #region 验证

                string datas = string.Empty;

                if (string.IsNullOrEmpty(selectData))
                {
                    errMsg += "没有选择单号";
                }
                else
                {
                }

                #endregion

                #region 修改数据


                //获取打印模板
                var printTemplates = _printTemplateService.GetAllPrintTemplates(curStore.Id).ToList();
                var content = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.CarGoodBill).Select(a => a.Content).FirstOrDefault();

                //获取打印设置
                var pCPrintSetting = _settingService.LoadSetting<PCPrintSetting>(_storeContext.CurrentStore.Id);

                //填充打印数据
                StringBuilder sb = new StringBuilder();
                sb.Append(content);

                #region theadid
                //sb.Replace("@商铺名称", curStore.Name);
                if (pCPrintSetting != null)
                {
                    sb.Replace("@商铺名称", pCPrintSetting.StoreName);
                }

                #endregion

                #region tbodyid
                //明细
                //获取 tbody 中的行
                int beginTbody = sb.ToString().IndexOf("<tbody>") + 7;
                int endTbody = sb.ToString().IndexOf("</tbody>");
                string tbodytr = sb.ToString()[beginTbody..endTbody];

                List<string> lst = selectData.Split(',').ToList();
                if (lst != null && lst.Count > 0)
                {
                    //1.先删除明细第一行
                    sb.Remove(beginTbody, endTbody - beginTbody);
                    int i = 0;

                    int sumSaleReservationBillQty = 0;
                    int sumReturnReservationQty = 0;
                    int sumRefuseQty = 0;
                    int sumReturnRealQty = 0;

                    foreach (var item in lst)
                    {
                        int index = sb.ToString().IndexOf("</tbody>");
                        i++;
                        StringBuilder sb2 = new StringBuilder();
                        sb2.Append(tbodytr);

                        sb2.Replace("#序号", i.ToString());

                        sb2.Replace("#单据编号", item.Split('|')[0] + ",</br>" + item.Split('|')[1]);
                        sb2.Replace("#单据类型", item.Split('|')[2] + "," + item.Split('|')[3]);
                        sb2.Replace("#客户", item.Split('|')[4]);
                        sb2.Replace("#转单时间", item.Split('|')[5]);
                        sb2.Replace("#仓库", item.Split('|')[6]);
                        sb2.Replace("#商品名称", item.Split('|')[7]);
                        sb2.Replace("#销订数量", item.Split('|')[8]);
                        sb2.Replace("#退订数量", item.Split('|')[9]);
                        sb2.Replace("#拒收", item.Split('|')[10]);
                        sb2.Replace("#退货", item.Split('|')[11]);

                        sb.Insert(index, sb2);

                        sumSaleReservationBillQty += int.Parse(item.Split('|')[8]);
                        sumReturnReservationQty += int.Parse(item.Split('|')[9]);
                        sumRefuseQty += int.Parse(item.Split('|')[10]);
                        sumReturnRealQty += int.Parse(item.Split('|')[11]);
                    }

                    sb.Replace("销订数量:###", sumSaleReservationBillQty.ToString());
                    sb.Replace("退订数量:###", sumReturnReservationQty.ToString());
                    sb.Replace("拒收:###", sumRefuseQty.ToString());
                    sb.Replace("退货:###", sumReturnRealQty.ToString());
                }
                #endregion

                #region tfootid

                #endregion

                datas += sb;


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
    }
}