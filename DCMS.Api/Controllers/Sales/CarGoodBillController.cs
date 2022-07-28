using DCMS.Core;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.Sales;
using DCMS.Services.Products;
using DCMS.Services.Sales;
using DCMS.Services.Terminals;
using DCMS.Services.Users;
using DCMS.Services.WareHouses;
using DCMS.ViewModel.Models.Sales;
using DCMS.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using Microsoft.AspNetCore.Authorization;

namespace DCMS.Api.Controllers
{
    /// <summary>
    /// 车辆对货单
    /// </summary>
   [Authorize]
    [Route("api/v{version:apiVersion}/dcms/sales")]
    public class CarGoodBillController : BaseAPIController
    {
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


        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="userService"></param>
        /// <param name="saleReservationService"></param>
        /// <param name="saleBillService"></param>
        /// <param name="returnReservationService"></param>
        /// <param name="returnBillService"></param>
        /// <param name="productService"></param>
        /// <param name="specificationAttributeService"></param>
        /// <param name="wareHouseService"></param>
        /// <param name="terminalService"></param>
        public CarGoodBillController(
            IUserService userService,
            ISaleReservationBillService saleReservationService,
            ISaleBillService saleBillService,
            IReturnReservationBillService returnReservationService,
            IReturnBillService returnBillService,
            IProductService productService,
            ISpecificationAttributeService specificationAttributeService,
            IWareHouseService wareHouseService,
            IDispatchBillService dispatchBillService,
            ITerminalService terminalService
            , ILogger<BaseAPIController> logger) : base(logger)
        {
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

        /// <summary>
        /// 获取车辆对货单列表
        /// </summary>
        /// <param name="store"></param>
        /// <param name="deliveryUserId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet("cargood/getbills/{store}/{deliveryUserId}")]
        [SwaggerOperation("getbills")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<CarGoodBillModel>>> AsyncList(int? store, int? makeuserId, int? deliveryUserId = 0, DateTime? start = null, DateTime? end = null, int pageIndex = 0, int pageSize = 50)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error<CarGoodBillModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var carGoodModels = new List<CarGoodBillModel>();
                    //查询关联的商品
                    var allProductIds = new List<int>();
                    //销售订单关联商品、关联销售单
                    var allSaleBills = new List<SaleBill>();
                    //退货订单关联商品、关联退货单
                    var allReturnBills = new List<ReturnBill>();

                    //销售订单
                    var saleReservations = _saleReservationService.GetSaleReservationBillsToCarGood(store ?? 0, makeuserId ?? 0, deliveryUserId ?? 0, start, end);

                    //退货订单
                    var returnReservations = _returnReservationService.GetReturnReservationBillsToCarGood(store ?? 0, makeuserId ?? 0, deliveryUserId ?? 0, start, end);

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
                        allSaleBills = _saleBillService.GetSaleBillsBySaleReservationIds(store ?? 0, saleReservations.Select(rr => rr.Id).ToArray()).ToList();
                    }
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

                        allReturnBills = _returnBillService.GetReturnBillsByReturnReservationIds(store ?? 0, returnReservations.Select(rr => rr.Id).ToArray()).ToList();
                    }
                    var allProducts = _productService.GetProductsByIds(store ?? 0, allProductIds.ToArray());
                    var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(store ?? 0, allProducts.GetProductBigStrokeSmallUnitIds());

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
                    var plist = new PagedList<CarGoodBillModel>(carGoodModels, pageIndex, pageSize);

                    var allTerminals = _terminalService.GetTerminalsByIds(store ?? 0, plist.Select(p => p.TerminalId)
                        .Distinct().ToArray());
                    var allWareHouses = _wareHouseService.GetWareHouseByIds(store ?? 0, plist.Select(p => p.WareHouseId).ToArray());
                    var userIds = plist.Select(p => p.BusinessUserId ?? 0).ToList();
                    userIds.AddRange(plist.Select(p => p.BusinessUserId ?? 0).ToList());

                    var allUsers = _userService.GetUsersDictsByIds(store ?? 0, userIds.Distinct().ToArray());

                    var results = plist.Select(p =>
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
                            p.CarName = _wareHouseService.GetWareHouseName(store ?? 0, p.CarId);
                        }

                        return p;
                    }).ToList();

                    return this.Successful(Resources.Successful, results);
                }
                catch (Exception ex)
                {
                    return this.Error<CarGoodBillModel>(ex.Message);
                }
            });
        }

        /// <summary>
        ///  获取用户选择单据的商品信息
        /// </summary>
        /// <param name="store"></param>
        /// <param name="selectDatas"></param>
        /// <returns></returns>
        [HttpGet("cargood/asyncProductList/{store}")]
        [SwaggerOperation("asyncProductList")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<CarGoodDetailModel>>> AsyncProductList(int? store, string selectDatas)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error<CarGoodDetailModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var carGoodDetailModels = new List<CarGoodDetailModel>();

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
                                            Product product = _productService.GetProductById(store ?? 0, saleReservationDetail.ProductId);
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
                                            Product product = _productService.GetProductById(store ?? 0, returnReservationDetail.ProductId);
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

                    return this.Successful(Resources.Successful, carGoodDetailModels);
                }
                catch (Exception ex)
                {
                    return this.Error<CarGoodDetailModel>(ex.Message);
                }

            });
        }
    }
}