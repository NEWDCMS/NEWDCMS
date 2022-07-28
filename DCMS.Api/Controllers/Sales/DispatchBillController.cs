using DCMS.Api.Infrastructure.Mapper.Extensions;
using DCMS.Api.Models;
using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Sales;
using DCMS.Core.Domain.Users;
using DCMS.Services.Logging;
using DCMS.Services.Products;
using DCMS.Services.Sales;
using DCMS.Services.Settings;
using DCMS.Services.Terminals;
using DCMS.Services.Users;
using DCMS.Services.WareHouses;
using DCMS.Services.Finances;
using DCMS.ViewModel.Models.Sales;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using Microsoft.AspNetCore.Authorization;
using System.Configuration;
using DCMS.ViewModel.Models.Finances;
using DCMS.Services.CSMS;
using DCMS.Core.Domain.CSMS;
using DCMS.Core.Domain.Terminals;
using DCMS.Services.Stores;
using DCMS.Core.Domain.Stores;
using DCMS.Core.Domain.Products;

namespace DCMS.Api.Controllers
{
    /// <summary>
    /// 装车调度
    /// </summary>
    [Authorize]
    [Route("api/v{version:apiVersion}/dcms/sales")]
    public class DispatchBillController : BaseAPIController
    {
        private readonly IUserActivityService _userActivityService;
        private readonly IUserService _userService;
        private readonly IProductService _productService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly ITerminalService _terminalService;
        private readonly IWareHouseService _wareHouseService;
        private readonly IDispatchBillService _dispatchBillService;
        private readonly ISaleReservationBillService _saleReservationBillService;
        private readonly IReturnReservationBillService _returnReservationBillService;
        private readonly IAccountingService _accountingService;
        private readonly IRedLocker _locker;
        private readonly ISaleBillService _saleBillService;
        private readonly IReturnBillService  _returnBillService;
        private readonly IExchangeBillService _exchangeBillService;
        private readonly ICostExpenditureBillService _costExpenditureBillService;
        private readonly ITerminalSignReportService _terminalSignReportService;
        private readonly IStoreService _storeService;


        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="userActivityService"></param>
        /// <param name="userService"></param>
        /// <param name="productService"></param>
        /// <param name="specificationAttributeService"></param>
        /// <param name="terminalService"></param>
        /// <param name="wareHouseService"></param>
        /// <param name="dispatchBillService"></param>
        /// <param name="saleReservationBillService"></param>
        /// <param name="returnReservationBillService"></param>
        /// <param name="accountingService"></param>
        /// <param name="saleBillService"></param>
        /// <param name="returnBillService"></param>
        /// <param name="locker"></param>
        /// <param name="logger"></param>
        public DispatchBillController(
            IUserActivityService userActivityService,
            IUserService userService,
            IProductService productService,
            ISpecificationAttributeService specificationAttributeService,
            ITerminalService terminalService,
            IWareHouseService wareHouseService,
            IDispatchBillService dispatchBillService,
            ISaleReservationBillService saleReservationBillService,
            IReturnReservationBillService returnReservationBillService,
            IAccountingService accountingService,
            ISaleBillService saleBillService,
            IReturnBillService returnBillService,
            IExchangeBillService exchangeBillService,
            ICostExpenditureBillService costExpenditureBillService,
            ITerminalSignReportService terminalSignReportService,
            IStoreService storeService,
        IRedLocker locker, 
            ILogger<BaseAPIController> logger) : base(logger)
        {
            _userActivityService = userActivityService;
            _userService = userService;
            _productService = productService;
            _specificationAttributeService = specificationAttributeService;
            _terminalService = terminalService;
            _wareHouseService = wareHouseService;
            _dispatchBillService = dispatchBillService;
            _saleReservationBillService = saleReservationBillService;
            _returnReservationBillService = returnReservationBillService;
            _accountingService = accountingService;
            _locker = locker;
            _saleBillService = saleBillService;
            _returnBillService = returnBillService;
            _exchangeBillService = exchangeBillService;
            _costExpenditureBillService = costExpenditureBillService;
            _terminalSignReportService = terminalSignReportService;
            _storeService = storeService;
        }

        /// <summary>
        /// 获取装车调度单列表
        /// </summary>
        /// <param name="store"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="businessUserId"></param>
        /// <param name="districtId"></param>
        /// <param name="terminalId"></param>
        /// <param name="billNumber"></param>
        /// <param name="deliveryUserId"></param>
        /// <param name="channelId"></param>
        /// <param name="rankId"></param>
        /// <param name="billTypeId"></param>
        /// <param name="showDispatchReserved"></param>
        /// <param name="dispatchStatus"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet("salebill/getAllDispatchBill/{store}/{terminalId}/{businessUserId}")]
        [SwaggerOperation("getAllDispatchBill")]
        //[ValidateActionParameters]
        public async Task<APIResult<IList<DispatchItemModel>>> GetAllDispatchBills(int? store, int? makeuserId, DateTime? start = null, DateTime? end = null, int? businessUserId = 0, int? districtId = 0, int? terminalId = 0, string billNumber = "", int? deliveryUserId = 0, int? channelId = 0, int? rankId = 0, int? billTypeId = 0, bool? showDispatchReserved = null, bool? dispatchStatus = null, int pageIndex = 0, int pageSize = 30)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error<DispatchItemModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var model = new DispatchBillModel();
                    //待调度
                    List<DispatchItemModel> dispatchItemModels = new List<DispatchItemModel>();
                    //销售订单
                    if (billTypeId == null || billTypeId == (int)BillTypeEnum.SaleReservationBill)
                    {
                        //销售订单
                        var gridModelSale = _saleReservationBillService.GetSaleReservationBillToDispatch(store ?? 0, makeuserId ?? 0, start, end, businessUserId, districtId, terminalId,
                            billNumber, deliveryUserId, channelId, rankId, billTypeId,
                            showDispatchReserved, dispatchStatus);

                        //将查询的销售订单转换装车调度数据
                        if (gridModelSale != null && gridModelSale.Count > 0)
                        {
                            gridModelSale.ToList().ForEach(a =>
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
                                    OrderAmount = a.ReceivableAmount,
                                    WareHouseId = a.WareHouseId,
                                    Remark = a.Remark
                                };
                                //销售商品数量
                                if (a.Items != null && a.Items.Count > 0)
                                {
                                    List<DispatchBillProductItem> dispatchBillProductItems = new List<DispatchBillProductItem>();
                                    var allProducts = _productService.GetProductsByIds(store ?? 0, a.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                                    var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(store ?? 0, allProducts.GetProductBigStrokeSmallUnitIds());

                                    a.Items.ToList().ForEach(s =>
                                    {
                                        var product = allProducts.Where(ap => ap.Id == s.ProductId).FirstOrDefault();
                                        if (product != null)
                                        {
                                            //商品转化量
                                            var conversionQuantity = product.GetConversionQuantity(allOptions, s.UnitId);
                                            //库存量增量 = 单位转化量 * 数量
                                            int thisQuantity = s.Quantity * conversionQuantity;
                                            if (dispatchBillProductItems != null && dispatchBillProductItems.Where(d => d.ProductId == s.ProductId).FirstOrDefault() != null)
                                            {
                                                dispatchBillProductItems.Where(d => d.ProductId == s.ProductId).FirstOrDefault().Quantity += thisQuantity;
                                            }
                                            else
                                            {
                                                dispatchBillProductItems.Add(new DispatchBillProductItem() { ProductId = s.ProductId, Quantity = thisQuantity, BigQuantity = product.BigQuantity ?? 1 });
                                            }
                                            dispatchItemModel.OrderQuantitySum += thisQuantity;
                                        }
                                    });
                                    //计算XXX大XXX小
                                    if (dispatchBillProductItems != null && dispatchBillProductItems.Count > 0)
                                    {
                                        int bigQuantity = 0;
                                        int smallQuantity = 0;
                                        dispatchBillProductItems.ForEach(d =>
                                        {
                                            bigQuantity += d.Quantity / d.BigQuantity;
                                            smallQuantity += d.Quantity % d.BigQuantity;
                                        });

                                        dispatchItemModel.OrderQuantityView = bigQuantity + "大" + smallQuantity + "小";
                                    }
                                }
                                dispatchItemModels.Add(dispatchItemModel);
                            });
                        }
                    }

                    //退货订单
                    if (billTypeId == null || billTypeId == (int)BillTypeEnum.ReturnReservationBill)
                    {
                        //退货订单
                        var gridModelReturn = _returnReservationBillService.GetReturnReservationBillToDispatch(store ?? 0, makeuserId ?? 0, start, end, businessUserId, districtId, terminalId,
                            billNumber, deliveryUserId, channelId, rankId, billTypeId,
                            showDispatchReserved, dispatchStatus);

                        //将查询的销售订单转换装车调度数据
                        if (gridModelReturn != null && gridModelReturn.Count > 0)
                        {
                            gridModelReturn.ToList().ForEach(a =>
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
                                    OrderAmount = a.SumAmount,
                                    WareHouseId = a.WareHouseId,
                                    Remark = a.Remark
                                };
                                //销售商品数量
                                if (a.Items != null && a.Items.Count > 0)
                                {
                                    List<DispatchBillProductItem> dispatchBillProductItems = new List<DispatchBillProductItem>();
                                    var allProducts = _productService.GetProductsByIds(store ?? 0, a.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                                    var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(store ?? 0, allProducts.GetProductBigStrokeSmallUnitIds());

                                    a.Items.ToList().ForEach(s =>
                                    {
                                        var product = allProducts.Where(ap => ap.Id == s.ProductId).FirstOrDefault();
                                        if (product != null)
                                        {
                                            //商品转化量
                                            var conversionQuantity = product.GetConversionQuantity(allOptions, s.UnitId);
                                            //库存量增量 = 单位转化量 * 数量
                                            int thisQuantity = s.Quantity * conversionQuantity;
                                            if (dispatchBillProductItems != null && dispatchBillProductItems.Where(d => d.ProductId == s.ProductId).FirstOrDefault() != null)
                                            {
                                                dispatchBillProductItems.Where(d => d.ProductId == s.ProductId).FirstOrDefault().Quantity += thisQuantity;
                                            }
                                            else
                                            {
                                                dispatchBillProductItems.Add(new DispatchBillProductItem() { ProductId = s.ProductId, Quantity = thisQuantity, BigQuantity = product.BigQuantity ?? 1 });
                                            }
                                            dispatchItemModel.OrderQuantitySum += thisQuantity;
                                        }
                                    });
                                    //计算XXX大XXX小
                                    if (dispatchBillProductItems != null && dispatchBillProductItems.Count > 0)
                                    {
                                        int bigQuantity = 0;
                                        int smallQuantity = 0;
                                        dispatchBillProductItems.ForEach(d =>
                                        {
                                            bigQuantity += d.Quantity / d.BigQuantity;
                                            smallQuantity += d.Quantity % d.BigQuantity;
                                        });

                                        dispatchItemModel.OrderQuantityView = bigQuantity + "大" + smallQuantity + "小";
                                    }
                                }
                                dispatchItemModels.Add(dispatchItemModel);
                            });
                        }
                    }

                    //分页
                    var results = dispatchItemModels.Skip(pageIndex * pageSize).Take(pageSize).ToList();

                    return this.Successful(Resources.Successful, results);
                }
                catch (Exception ex)
                {
                    return this.Error<DispatchItemModel>(ex.Message);
                }
            });
        }



        /// <summary>
        /// 获取待签收单据（销售订单，退货订单，换货单）
        /// </summary>
        /// <param name="store"></param>
        /// <param name="userId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="businessUserId"></param>
        /// <param name="districtId"></param>
        /// <param name="terminalId"></param>
        /// <param name="terminalName"></param>
        /// <param name="billNumber"></param>
        /// <param name="deliveryUserId"></param>
        /// <param name="channelId"></param>
        /// <param name="rankId"></param>
        /// <param name="billTypeId"></param>
        /// <param name="showDispatchReserved"></param>
        /// <param name="dispatchStatus"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet("salebill/getUndeliveredSigns/{store}/{userId}")]
        [SwaggerOperation("getUndeliveredSigns")]
        public async Task<APIResult<IList<DispatchItemModel>>> GetUndeliveredSigns(int? store, int? userId, DateTime? start = null, DateTime? end = null, int? businessUserId = null, int? districtId = null, int? terminalId = null, string terminalName = "", string billNumber = "", int? deliveryUserId = null, int? channelId = null, int? rankId = null, int? billTypeId = null, bool? showDispatchReserved = null, bool? dispatchStatus = null, int pageIndex = 0, int pageSize = 20)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error<DispatchItemModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                var dispatchItems = new List<DispatchItemModel>();
                try
                {
                    var gridModelDispatch = _dispatchBillService.GetDispatchBillList(store,
                        userId,
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

                    gridModelDispatch?.ToList().ForEach(bill =>
                    {
                        bill.Items?.ToList().ForEach(item =>
                        {
                            //待签收
                            if (item != null && item.SignStatus == 0)
                            {
                                var dispatch = item.ToModel<DispatchItemModel>();
                                //调度项目ID
                                dispatch.Id = item.Id;
                                //调度单ID
                                dispatch.DispatchBillId = item.DispatchBillId;
                                dispatch.CarId = bill.CarId;
                                dispatch.TransactionDate = item.CreatedOnUtc;

                                //换货单
                                if (item.BillTypeId == (int)BillTypeEnum.ExchangeBill)
                                {
                                    var ecb = _exchangeBillService.GetExchangeBillById(store, item.BillId, true);
                                    dispatch.BillNumber = ecb?.BillNumber;
                                    dispatch.BillTypeName = "换货单";
                                    dispatch.BusinessUserId = ecb?.BusinessUserId;
                                    dispatch.BusinessUserName = _userService.GetUserName(store, ecb?.BusinessUserId ?? 0);
                                    dispatch.OrderAmount = ecb?.SumAmount;
                                    dispatch.DeliveryUserId = ecb?.DeliveryUserId;
                                    dispatch.DeliveryUserName = _userService.GetUserName(store, ecb?.DeliveryUserId ?? 0);
                                    dispatch.TerminalId = ecb?.TerminalId ?? 0;
                                    dispatch.TerminalName = _terminalService.GetTerminalName(store, ecb?.TerminalId ?? 0);
                                    //dispatch.WareHouseName = _wareHouseService.GetWareHouseName(store ?? 0, srb?.WareHouseId ?? 0);
                                    dispatch.WareHouseName = _wareHouseService.GetWareHouseName(store ?? 0, bill.CarId);

                                    if (ecb != null)
                                    {
                                        var model = ecb.ToModel<ExchangeBillModel>();
                                        var gridModel = _exchangeBillService.GetExchangeItemList(ecb.Id);
                                        var allProductPrices = _productService.GetProductPricesByProductIds(store, gridModel.Select(p => p.ProductId).ToArray());
                                        var allProducts = _productService.GetProductsByIds(store ?? 0, gridModel.Select(pr => pr.ProductId).Distinct().ToArray());
                                        var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(store, allProducts.GetProductBigStrokeSmallUnitIds());
                                        var allProductTierPrices = _productService.GetProductTierPriceByProductIds(store ?? 0, gridModel.Select(pr => pr.ProductId).Distinct().ToArray());

                                        model.Items = gridModel.Select(o =>
                                        {
                                            var m = o.ToModel<ExchangeItemModel>();
                                            var product = allProducts.Where(ap => ap.Id == m.ProductId).FirstOrDefault();
                                            if (product != null)
                                            {
                                                //这里替换成高级用法
                                                m = product.InitBaseModel<ExchangeItemModel>(m, o.ExchangeBill.WareHouseId, allOptions, allProductPrices, allProductTierPrices, _productService);

                                                //商品信息
                                                m.BigUnitId = product.BigUnitId;
                                                m.StrokeUnitId = product.StrokeUnitId;
                                                m.SmallUnitId = product.SmallUnitId;
                                                //是否开启生产日期功能
                                                m.IsManufactureDete = product.IsManufactureDete; 
                                                m.ProductTimes = _productService.GetProductDates(store ?? 0, product.Id, o.ExchangeBill.WareHouseId);
                                                m.UnitId = o.UnitId;
                                                m.UnitName = m.Units.Where(q => q.Value == m.UnitId).Select(q => q.Key)?.FirstOrDefault();

                                                //税价总计
                                                m.TaxPriceAmount = m.Amount;

                                                //if (o.ExchangeBill.TaxAmount > 0)
                                                //{
                                                //    //含税价格
                                                //    m.ContainTaxPrice = m.Price;
                                                //    //税额
                                                //    m.TaxPrice = m.Amount - m.Amount / (1 + m.TaxRate / 100);

                                                //    m.Price /= (1 + m.TaxRate / 100);
                                                //    m.Amount /= (1 + m.TaxRate / 100);
                                                //}

                                            }
                                            return m;
                                        }).ToList();

                                        model.TerminalName = dispatch.TerminalName;
                                        model.WareHouseName = dispatch.WareHouseName;
                                        model.DeliveryUserName = dispatch.DeliveryUserName;
                                        model.WareHouseId = dispatch.CarId;

                                        dispatch.ExchangeBill = model;
                                    }
                                }
                                //销售订单
                                else if (item.BillTypeId == (int)BillTypeEnum.SaleReservationBill)
                                {
                                    var srb = _saleReservationBillService.GetSaleReservationBillById(store, item.BillId, true);

                                    dispatch.BillNumber = srb?.BillNumber;
                                    dispatch.BillTypeName = "销售订单";
                                    dispatch.BusinessUserId = srb?.BusinessUserId;
                                    dispatch.BusinessUserName = _userService.GetUserName(store, srb?.BusinessUserId ?? 0);
                                    dispatch.OrderAmount = srb?.SumAmount;
                                    dispatch.DeliveryUserId = srb?.DeliveryUserId;
                                    dispatch.DeliveryUserName = _userService.GetUserName(store, srb?.DeliveryUserId ?? 0);
                                    dispatch.TerminalId = srb?.TerminalId ?? 0;
                                    dispatch.TerminalName = _terminalService.GetTerminalName(store, srb?.TerminalId??0);
                                    //dispatch.WareHouseName = _wareHouseService.GetWareHouseName(store ?? 0, srb?.WareHouseId ?? 0);
                                    dispatch.WareHouseName= _wareHouseService.GetWareHouseName(store ?? 0, bill.CarId);

                                    if (srb != null)
                                    {
                                        var model = srb.ToModel<SaleReservationBillModel>();
                                        model.SaleReservationBillAccountings = srb.SaleReservationBillAccountings.Select(s =>
                                        {
                                            var m = s.ToAccountModel<SaleReservationBillAccountingModel>();
                                            m.Name = _accountingService.GetAccountingOptionName(store, s.AccountingOptionId);
                                            m.AccountingOptionName= _accountingService.GetAccountingOptionName(store, s.AccountingOptionId);
                                            return m;

                                        }).ToList();

                                        var gridModel = _saleReservationBillService.GetSaleReservationItemList(srb.Id);
                                        var allProductPrices = _productService.GetProductPricesByProductIds(store, gridModel.Select(p => p.ProductId).ToArray());
                                        var allProducts = _productService.GetProductsByIds(store ?? 0, gridModel.Select(pr => pr.ProductId).Distinct().ToArray());
                                        var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(store, allProducts.GetProductBigStrokeSmallUnitIds());
                                        var allProductTierPrices = _productService.GetProductTierPriceByProductIds(store ?? 0, gridModel.Select(pr => pr.ProductId).Distinct().ToArray());

                                        model.Items = gridModel.Select(o =>
                                        {
                                            var m = o.ToModel<SaleReservationItemModel>();
                                            var product = allProducts.Where(ap => ap.Id == m.ProductId).FirstOrDefault();
                                            if (product != null)
                                            {
                                                //这里替换成高级用法
                                                m = product.InitBaseModel<SaleReservationItemModel>(m, o.SaleReservationBill.WareHouseId, allOptions, allProductPrices, allProductTierPrices, _productService);

                                                //商品信息
                                                m.BigUnitId = product.BigUnitId;
                                                m.StrokeUnitId = product.StrokeUnitId;
                                                m.SmallUnitId = product.SmallUnitId;
                                                m.IsManufactureDete = product.IsManufactureDete; //是否开启生产日期功能
                                                m.ProductTimes = _productService.GetProductDates(store ?? 0, product.Id, o.SaleReservationBill.WareHouseId);
                                                m.UnitId = o.UnitId;
                                                m.UnitName = m.Units.Where(q => q.Value == m.UnitId).Select(q => q.Key)?.FirstOrDefault();

                                                //税价总计
                                                m.TaxPriceAmount = m.Amount;

                                                if (o.SaleReservationBill.TaxAmount > 0)
                                                {
                                                    //含税价格
                                                    m.ContainTaxPrice = m.Price;
                                                    //税额
                                                    m.TaxPrice = m.Amount - m.Amount / (1 + m.TaxRate / 100);

                                                    m.Price /= (1 + m.TaxRate / 100);
                                                    m.Amount /= (1 + m.TaxRate / 100);
                                                }

                                            }
                                            return m;
                                        }).ToList();

                                        model.TerminalName = dispatch.TerminalName;
                                        model.WareHouseName = dispatch.WareHouseName;
                                        model.DeliveryUserName = dispatch.DeliveryUserName;
                                        model.WareHouseId = dispatch.CarId;

                                        dispatch.SaleReservationBill = model;
                                    }
                                }
                                //退货订单
                                else if (item.BillTypeId == (int)BillTypeEnum.ReturnReservationBill)
                                {
                                    var rrb = _returnReservationBillService.GetReturnReservationBillById(store, item.BillId, true);
                                    dispatch.BillNumber = rrb?.BillNumber;
                                    dispatch.BillTypeName = "退货订单";
                                    dispatch.BusinessUserId = rrb?.BusinessUserId;
                                    dispatch.BusinessUserName = _userService.GetUserName(store, rrb?.BusinessUserId ?? 0);
                                    dispatch.OrderAmount = rrb?.SumAmount;
                                    dispatch.DeliveryUserId = rrb?.DeliveryUserId;
                                    dispatch.DeliveryUserName = _userService.GetUserName(store, rrb?.DeliveryUserId ?? 0);
                                    dispatch.TerminalId = rrb?.TerminalId ?? 0;
                                    dispatch.TerminalName = _terminalService.GetTerminalName(store, rrb?.TerminalId ?? 0);
                                    dispatch.WareHouseName = _wareHouseService.GetWareHouseName(store ?? 0, bill.CarId);

                                    if (rrb != null)
                                    {
                                        var model = rrb.ToModel<ReturnReservationBillModel>();
                                        model.ReturnReservationBillAccountings = rrb.ReturnReservationBillAccountings.Select(s =>
                                        {
                                            var m = s.ToAccountModel<ReturnReservationBillAccountingModel>();
                                            m.Name = _accountingService.GetAccountingOptionName(store, s.AccountingOptionId);
                                            m.AccountingOptionName = _accountingService.GetAccountingOptionName(store, s.AccountingOptionId);

                                            return m;
                                        }).ToList();

                                        var gridModel = _returnReservationBillService.GetReturnReservationItemList(rrb.Id);
                                        var allProductPrices = _productService.GetProductPricesByProductIds(store, gridModel.Select(p => p.ProductId).ToArray());
                                        var allProducts = _productService.GetProductsByIds(store ?? 0, gridModel.Select(pr => pr.ProductId).Distinct().ToArray());
                                        var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(store, allProducts.GetProductBigStrokeSmallUnitIds());
                                        var allProductTierPrices = _productService.GetProductTierPriceByProductIds(store ?? 0, gridModel.Select(pr => pr.ProductId).Distinct().ToArray());

                                        model.Items = gridModel.Select(o =>
                                        {
                                            var m = o.ToModel<ReturnReservationItemModel>();
                                            var product = allProducts.Where(ap => ap.Id == m.ProductId).FirstOrDefault();
                                            if (product != null)
                                            {
                                                //这里替换成高级用法
                                                m = product.InitBaseModel<ReturnReservationItemModel>(m, o.ReturnReservationBill.WareHouseId, allOptions, allProductPrices, allProductTierPrices, _productService);

                                                //商品信息
                                                m.BigUnitId = product.BigUnitId;
                                                m.StrokeUnitId = product.StrokeUnitId;
                                                m.SmallUnitId = product.SmallUnitId;
                                                m.IsManufactureDete = product.IsManufactureDete; //是否开启生产日期功能
                                                m.ProductTimes = _productService.GetProductDates(store ?? 0, product.Id, o.ReturnReservationBill.WareHouseId);
                                                m.UnitId = o.UnitId;
                                                m.UnitName = m.Units.Where(q => q.Value == m.UnitId).Select(q => q.Key)?.FirstOrDefault();

                                                //税价总计
                                                m.TaxPriceAmount = m.Amount;

                                                if (o.ReturnReservationBill.TaxAmount > 0)
                                                {
                                                    //含税价格
                                                    m.ContainTaxPrice = m.Price;
                                                    //税额
                                                    m.TaxPrice = m.Amount - m.Amount / (1 + m.TaxRate / 100);

                                                    m.Price /= (1 + m.TaxRate / 100);
                                                    m.Amount /= (1 + m.TaxRate / 100);
                                                }

                                            }
                                            return m;
                                        }).ToList();

                                        model.TerminalName = dispatch.TerminalName;
                                        model.WareHouseName = dispatch.WareHouseName;
                                        model.DeliveryUserName = dispatch.DeliveryUserName;
                                        model.WareHouseId = dispatch.CarId;

                                        dispatch.ReturnReservationBill = model;
                                    }
                                }

                                dispatchItems.Add(dispatch);
                            }
                        });

                    });

                    return this.Successful(Resources.Successful, dispatchItems);
                }
                catch (Exception ex)
                {
                    return this.Error<DispatchItemModel>(ex.Message);
                }
            });
        }


        /// <summary>
        /// 获取已签收单据
        /// </summary>
        /// <param name="store"></param>
        /// <param name="userId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="businessUserId"></param>
        /// <param name="terminalId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet("salebill/getDeliveriedSigns/{store}/{userId}")]
        [SwaggerOperation("getDeliveriedSigns")]
        public async Task<APIResult<IList<DeliverySignModel>>> GetDeliveriedSigns(int? store, int? userId, DateTime? start = null, DateTime? end = null, int? businessUserId = null, int? terminalId = null, int pageIndex = 0, int pageSize = 20)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error<DeliverySignModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                var dispatchItems = new List<DeliverySignModel>();
                try
                {
                    var bills = _dispatchBillService.GetSignedBills(store, start, end, userId, terminalId, pageIndex, pageSize);
                    var allTerminals = _terminalService.GetTerminalsByIds(store, bills.Select(s => s.TerminalId).ToArray()).ToList();
                    dispatchItems = bills?.Select(s => 
                    {
                        var m = s.ToModel<DeliverySignModel>();
                        //var t = _terminalService.GetTerminalById(store, s?.TerminalId ?? 0);
                        var t = allTerminals.Where(t => t.Id == s.TerminalId).FirstOrDefault();

                        m.TerminalName = t?.Name;
                        m.Address = t?.Address;
                        m.BossCall = t?.BossCall;

                        m.BusinessUserName = _userService.GetUserName(store, s?.BusinessUserId??0);
                        m.DeliveryUserName = _userService.GetUserName(store, s?.DeliveryUserId ?? 0);

                        //换货单
                        if (m.BillTypeId == (int)BillTypeEnum.ExchangeBill)
                        {
                            var ecb = _exchangeBillService.GetExchangeBillById(store, s.ToBillId, true);
                            if (ecb != null)
                            {
                                var model = ecb.ToModel<ExchangeBillModel>();
                                model.BillNumber = s.ToBillNumber;
                                var gridModel = _exchangeBillService.GetExchangeItemList(ecb.Id);
                                var allProductPrices = _productService.GetProductPricesByProductIds(store, gridModel.Select(p => p.ProductId).ToArray());
                                var allProducts = _productService.GetProductsByIds(store ?? 0, gridModel.Select(pr => pr.ProductId).Distinct().ToArray());
                                var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(store, allProducts.GetProductBigStrokeSmallUnitIds());
                                var allProductTierPrices = _productService.GetProductTierPriceByProductIds(store ?? 0, gridModel.Select(pr => pr.ProductId).Distinct().ToArray());

                                model.Items = gridModel.Select(o =>
                                {
                                    var m = o.ToModel<ExchangeItemModel>();
                                    var product = allProducts.Where(ap => ap.Id == m.ProductId).FirstOrDefault();
                                    if (product != null)
                                    {
                                        //这里替换成高级用法
                                        m = product.InitBaseModel(m, o.ExchangeBill.WareHouseId, allOptions, allProductPrices, allProductTierPrices, _productService);

                                        //商品信息
                                        m.BigUnitId = product.BigUnitId;
                                        m.StrokeUnitId = product.StrokeUnitId;
                                        m.SmallUnitId = product.SmallUnitId;
                                        m.IsManufactureDete = product.IsManufactureDete; //是否开启生产日期功能
                                        m.ProductTimes = _productService.GetProductDates(store ?? 0, product.Id, o.ExchangeBill.WareHouseId);

                                        //税价总计
                                        m.TaxPriceAmount = m.Amount;

                                    }
                                    return m;
                                }).ToList();

                                model.TerminalName = m.TerminalName;
                                model.DeliveryUserName = m.DeliveryUserName;
                                model.WareHouseName = _wareHouseService.GetWareHouseName(store, ecb?.WareHouseId ?? 0);

                                m.ExchangeBill = model;
                            }
                        }
                        //销售订单
                        else if (m.BillTypeId == (int)BillTypeEnum.SaleReservationBill)
                        {
                            var srb = _saleBillService.GetSaleBillById(store, s.ToBillId, true);
                            if (srb != null)
                            {
                                var model = srb.ToModel<SaleBillModel>();
                                model.BillNumber = s.ToBillNumber;
                                model.SaleBillAccountings = srb.SaleBillAccountings.Select(s =>
                                {
                                    var m = s.ToAccountModel<SaleBillAccountingModel>();
                                    m.Name = _accountingService.GetAccountingOptionName(store, s.AccountingOptionId);
                                    m.AccountingOptionName = _accountingService.GetAccountingOptionName(store, s.AccountingOptionId);

                                    return m;
                                }).ToList();

                                var gridModel = _saleBillService.GetSaleItemList(srb.Id);
                                var allProductPrices = _productService.GetProductPricesByProductIds(store, gridModel.Select(p => p.ProductId).ToArray());
                                var allProducts = _productService.GetProductsByIds(store ?? 0, gridModel.Select(pr => pr.ProductId).Distinct().ToArray());
                                var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(store, allProducts.GetProductBigStrokeSmallUnitIds());
                                var allProductTierPrices = _productService.GetProductTierPriceByProductIds(store ?? 0, gridModel.Select(pr => pr.ProductId).Distinct().ToArray());

                                model.Items = gridModel.Select(o =>
                                {
                                    var m = o.ToModel<SaleItemModel>();
                                    var product = allProducts.Where(ap => ap.Id == m.ProductId).FirstOrDefault();
                                    if (product != null)
                                    {
                                        //这里替换成高级用法
                                        m = product.InitBaseModel(m, o.SaleBill.WareHouseId, allOptions, allProductPrices, allProductTierPrices, _productService);

                                        //商品信息
                                        m.BigUnitId = product.BigUnitId;
                                        m.StrokeUnitId = product.StrokeUnitId;
                                        m.SmallUnitId = product.SmallUnitId;
                                        m.IsManufactureDete = product.IsManufactureDete; //是否开启生产日期功能
                                        m.ProductTimes = _productService.GetProductDates(store ?? 0, product.Id, o.SaleBill.WareHouseId);

                                        //税价总计
                                        m.TaxPriceAmount = m.Amount;

                                        if (o.SaleBill.TaxAmount > 0)
                                        {
                                            //含税价格
                                            m.ContainTaxPrice = m.Price;
                                            //税额
                                            m.TaxPrice = m.Amount - m.Amount / (1 + m.TaxRate / 100);

                                            m.Price /= (1 + m.TaxRate / 100);
                                            m.Amount /= (1 + m.TaxRate / 100);
                                        }
                                    }
                                    return m;
                                }).ToList();

                                model.TerminalName = m.TerminalName;
                                model.DeliveryUserName = m.DeliveryUserName;
                                model.WareHouseName = _wareHouseService.GetWareHouseName(store, srb?.WareHouseId ?? 0);

                                m.SaleBill = model;
                            }
                        }
                        //销售单
                        else if (m.BillTypeId == (int)BillTypeEnum.SaleBill)
                        {
                            var sb = _saleBillService.GetSaleBillById(store, s.BillId, true);
                            if (sb != null)
                            {
                                var model = sb.ToModel<SaleBillModel>();
                                model.BillNumber = s.ToBillNumber;
                                model.SaleBillAccountings = sb.SaleBillAccountings.Select(s =>
                                {
                                    var m = s.ToAccountModel<SaleBillAccountingModel>();
                                    m.Name = _accountingService.GetAccountingOptionName(store, s.AccountingOptionId);
                                    m.AccountingOptionName = _accountingService.GetAccountingOptionName(store, s.AccountingOptionId);

                                    return m;
                                }).ToList();

                                var gridModel = _saleBillService.GetSaleItemList(sb.Id);
                                var allProductPrices = _productService.GetProductPricesByProductIds(store, gridModel.Select(p => p.ProductId).ToArray());
                                var allProducts = _productService.GetProductsByIds(store ?? 0, gridModel.Select(pr => pr.ProductId).Distinct().ToArray());
                                var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(store, allProducts.GetProductBigStrokeSmallUnitIds());
                                var allProductTierPrices = _productService.GetProductTierPriceByProductIds(store ?? 0, gridModel.Select(pr => pr.ProductId).Distinct().ToArray());
                                model.Items = gridModel.Select(o =>
                                {
                                    var m = o.ToModel<SaleItemModel>();
                                    var product = allProducts.Where(ap => ap.Id == m.ProductId).FirstOrDefault();
                                    if (product != null)
                                    {
                                        //这里替换成高级用法
                                        m = product.InitBaseModel(m, o.SaleBill.WareHouseId, allOptions, allProductPrices, allProductTierPrices, _productService);

                                        //商品信息
                                        m.BigUnitId = product.BigUnitId;
                                        m.StrokeUnitId = product.StrokeUnitId;
                                        m.SmallUnitId = product.SmallUnitId;
                                        m.IsManufactureDete = product.IsManufactureDete; //是否开启生产日期功能
                                        m.ProductTimes = _productService.GetProductDates(store ?? 0, product.Id, o.SaleBill.WareHouseId);

                                        //税价总计
                                        m.TaxPriceAmount = m.Amount;
                                        if (o.SaleBill.TaxAmount > 0)
                                        {
                                            //含税价格
                                            m.ContainTaxPrice = m.Price;
                                            //税额
                                            m.TaxPrice = m.Amount - m.Amount / (1 + m.TaxRate / 100);

                                            m.Price /= (1 + m.TaxRate / 100);
                                            m.Amount /= (1 + m.TaxRate / 100);
                                        }
                                    }
                                    return m;
                                }).ToList();

                                model.TerminalName = m.TerminalName;
                                model.DeliveryUserName = m.DeliveryUserName;
                                model.WareHouseName = _wareHouseService.GetWareHouseName(store, sb?.WareHouseId ?? 0);

                                m.SaleBill = model;
                            }
                        }
                        //退货订单
                        else if (m.BillTypeId == (int)BillTypeEnum.ReturnReservationBill)
                        {
                            var rrb = _returnBillService.GetReturnBillById(store, s.ToBillId, true);
                            if (rrb != null)
                            {
                                var model = rrb.ToModel<ReturnBillModel>();
                                model.BillNumber = s.ToBillNumber;
                                model.ReturnBillAccountings = rrb.ReturnBillAccountings.Select(s =>
                                {
                                    var m = s.ToAccountModel<ReturnBillAccountingModel>();
                                    m.Name = _accountingService.GetAccountingOptionName(store, s.AccountingOptionId);
                                    m.AccountingOptionName = _accountingService.GetAccountingOptionName(store, s.AccountingOptionId);

                                    return m;
                                }).ToList();

                                var gridModel = _returnBillService.GetReturnItemList(rrb.Id);
                                var allProductPrices = _productService.GetProductPricesByProductIds(store, gridModel.Select(p => p.ProductId).ToArray());
                                var allProducts = _productService.GetProductsByIds(store ?? 0, gridModel.Select(pr => pr.ProductId).Distinct().ToArray());
                                var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(store, allProducts.GetProductBigStrokeSmallUnitIds());
                                var allProductTierPrices = _productService.GetProductTierPriceByProductIds(store ?? 0, gridModel.Select(pr => pr.ProductId).Distinct().ToArray());

                                model.Items = gridModel.Select(o =>
                                {
                                    var m = o.ToModel<ReturnItemModel>();
                                    var product = allProducts.Where(ap => ap.Id == m.ProductId).FirstOrDefault();
                                    if (product != null)
                                    {
                                        //这里替换成高级用法
                                        m = product.InitBaseModel(m, o.ReturnBill.WareHouseId, allOptions, allProductPrices, allProductTierPrices, _productService);

                                        //商品信息
                                        m.BigUnitId = product.BigUnitId;
                                        m.StrokeUnitId = product.StrokeUnitId;
                                        m.SmallUnitId = product.SmallUnitId;
                                        m.IsManufactureDete = product.IsManufactureDete; //是否开启生产日期功能
                                        m.ProductTimes = _productService.GetProductDates(store ?? 0, product.Id, o.ReturnBill.WareHouseId);

                                        //税价总计
                                        m.TaxPriceAmount = m.Amount;

                                        if (o.ReturnBill.TaxAmount > 0)
                                        {
                                            //含税价格
                                            m.ContainTaxPrice = m.Price;
                                            //税额
                                            m.TaxPrice = m.Amount - m.Amount / (1 + m.TaxRate / 100);

                                            m.Price /= (1 + m.TaxRate / 100);
                                            m.Amount /= (1 + m.TaxRate / 100);
                                        }

                                    }
                                    return m;
                                }).ToList();

                                model.TerminalName = m.TerminalName;
                                model.DeliveryUserName = m.DeliveryUserName;
                                model.WareHouseName = _wareHouseService.GetWareHouseName(store, rrb?.WareHouseId ?? 0);

                                m.ReturnBill = model;
                            }
                        }
                        //费用支出
                        else if (m.BillTypeId == (int)BillTypeEnum.CostExpenditureBill)
                        {
                            var ceb = _costExpenditureBillService.GetCostExpenditureBillById(store, s.BillId);
                            if (ceb != null)
                            {
                                var model = ceb.ToModel<CostExpenditureBillModel>();

                                model.BillNumber = s.BillNumber;
                                model.CostExpenditureBillAccountings = ceb.CostExpenditureBillAccountings.Select(sb =>
                                {
                                     var m = sb.ToAccountModel<CostExpenditureBillAccountingModel>();
                                    m.Name = _accountingService.GetAccountingOptionName(store, sb.AccountingOptionId);
                                    m.AccountingOptionName = _accountingService.GetAccountingOptionName(store, sb.AccountingOptionId);
                                    return m;

                                }).Where(ao => ao.AccountingOptionId > 0).ToList();

                                var gridModel = _costExpenditureBillService.GetCostExpenditureItemList(ceb.Id);
                                var custorerIds = gridModel.Select(s => s.CustomerId).ToList();
                                var allTerminal = _terminalService.GetTerminalsByIds(store ?? 0, custorerIds.Distinct().ToArray());
                                model.Items = gridModel.Select(sb =>
                                {
                                    var it = sb.ToModel<CostExpenditureItemModel>();
                                    var terminal = allTerminal.Where(t => t.Id == it.CustomerId).FirstOrDefault();
                                    it.CustomerName = terminal == null ? "" : terminal.Name;
                                    return it;
                                }).ToList();

                                var allUsers = _userService.GetUsersDictsByIds(store ?? 0, bills.Select(b => ceb.EmployeeId).Distinct().ToArray());
                                model.EmployeeName = allUsers.Where(aw => aw.Key == ceb.EmployeeId).Select(aw => aw.Value).FirstOrDefault();

                                model.TotalAmount = model.Items?.Sum(t => t.Amount) ?? 0;
                                model.CustomerId = m.TerminalId;
                                model.CustomerName = m.TerminalName;
                                m.CostExpenditureBill = model;
                            }
                        }

                        return m;

                    }).ToList();
                    return this.Successful(Resources.Successful, dispatchItems);
                }
                catch (Exception ex)
                {
                    return this.Error<DeliverySignModel>(ex.Message);
                }
            });
        }

        /// <summary>
        /// 用于订单拒签
        /// </summary>
        /// <param name="data"></param>
        /// <param name="store"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost("salebill/refusedConfirm/{store}/{userId}")]
        [SwaggerOperation("refusedConfirm")]
        public async Task<APIResult<dynamic>> RefusedConfirm(DeliverySignUpdateModel data, int? store, int? userId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error(Resources.ParameterError);

            return await Task.Run(async () =>
            {
                User user = null;
                try
                {
                    if (data == null)
                        return this.Error("拒签失败！");


                    user = _userService.GetUserById(store ?? 0, userId ?? 0);
                    //有调度项目时
                    if (data.BillTypeId == (int)BillTypeEnum.ExchangeBill || data.BillTypeId == (int)BillTypeEnum.SaleReservationBill || data.BillTypeId == (int)BillTypeEnum.ReturnReservationBill)
                    {
                        //获取调度项目
                        var dispatchItem = _dispatchBillService.GetDispatchItemsById(store, data.DispatchItemId);
                        //获取调度单
                        var dispatchBill = _dispatchBillService.GetDispatchBillById(store, dispatchItem?.DispatchBillId ?? 0);
                        if (dispatchItem == null || dispatchBill == null)
                        {
                            return this.Error("调度单据不存在");
                        }
                        else
                        {
                            //待签收时
                            if (dispatchItem.SignStatus == 0)
                            {
                                //RedLock
                                var results = await _locker.PerformActionWithLockAsync(this.LockKey(data, store ?? 0, userId ?? 0),
                                    TimeSpan.FromSeconds(30),
                                    TimeSpan.FromSeconds(10),
                                    TimeSpan.FromSeconds(1),
                                    () => _dispatchBillService.RefusedConfirm(store ?? 0, userId ?? 0, data.ToEntity<DeliverySignUpdate>(), dispatchBill, dispatchItem));


                                return results.To(results);
                            }
                            else
                            {
                                return this.Error("单据已签收");
                            }
                        }
                    }
                    else if (data.BillTypeId == (int)BillTypeEnum.SaleBill)
                    {
                        var sign = new DeliverySign();
                        var bill = _saleBillService.GetSaleBillById(store, data.BillId);
                        if (bill != null)
                        {
                            bill.SignStatus = 1;
                            _saleBillService.UpdateSaleBill(bill);
                        }

                        //记录
                        sign = new DeliverySign
                        {
                            StoreId = store ?? 0,
                            BillTypeId = (int)BillTypeEnum.SaleBill,
                            BillId = data.BillId,
                            BillNumber = bill.BillNumber,
                            ToBillId = 0,
                            ToBillNumber = "",
                            TerminalId = bill.TerminalId,
                            BusinessUserId = bill.BusinessUserId,
                            DeliveryUserId = bill.DeliveryUserId,
                            Latitude = data.Latitude,
                            Longitude = data.Longitude,
                            SignUserId = userId ?? 0,
                            SignedDate = DateTime.Now,
                            SumAmount = bill.SumAmount,
                            Signature = data.Signature,
                            SignStatus = 2
                        };
                        _dispatchBillService.InsertDeliverySign(sign);
                    }

                    return this.Successful("单据已签收");
                }
                catch (Exception ex)
                {
                    _userActivityService.InsertActivity("DeliverySignConfirm", ex.Message, user, userId ?? 0);
                    return this.Error(ex.Message);
                }
            });
        }

        /// <summary>
        /// 用于单据签收确认(销售单，费用支出单)适用，（销售订单，换货单，退货订单）走转单逻辑
        /// </summary>
        /// <param name="data"></param>
        /// <param name="store"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost("salebill/deliverySignConfirm/{store}/{userId}")]
        [SwaggerOperation("deliverySignConfirm")]
        public async Task<APIResult<dynamic>> DeliverySignConfirm(DeliverySignUpdateModel data, int? store, int? userId, int billId = 0)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error(Resources.ParameterError);

            return await Task.Run(async () =>
            {
                User user = null;
                try
                {
                    user = _userService.GetUserById(store ?? 0, userId ?? 0);
                    if (data != null)
                    {
                        var sign = new DeliverySign();

                        TerminalSignReport terminalSignReport = null;
                        List<OrderDetail> orderDetails = null;
                        //销售单
                        if (data.BillTypeId == (int)BillTypeEnum.SaleBill)
                        {
                            var bill = _saleBillService.GetSaleBillById(store, data.BillId);
                            if (bill != null)
                            {
                                bill.SignStatus = 1;
                                _saleBillService.UpdateSaleBill(bill);
                            }
                            sign = new DeliverySign
                            {
                                StoreId = store ?? 0,
                                BillTypeId = (int)BillTypeEnum.SaleBill,
                                BillId = data.BillId,
                                BillNumber = bill.BillNumber,
                                ToBillId = 0,
                                ToBillNumber = "",
                                TerminalId = bill.TerminalId,
                                BusinessUserId = bill.BusinessUserId,
                                DeliveryUserId = bill.DeliveryUserId,
                                Latitude = data.Latitude,
                                Longitude = data.Longitude,
                                SignUserId = userId ?? 0,
                                SignedDate = DateTime.Now,
                                SumAmount = bill.SumAmount,
                                Signature= data.Signature,
                                SignStatus = 1
                            };

                            //留存照片
                            if (data.RetainPhotos != null && data.RetainPhotos.Any())
                            {
                                data.RetainPhotos.ForEach(p =>
                                {
                                    var rp = p.ToEntity<RetainPhoto>();
                                    rp.StoreId = store ?? 0;
                                    sign.RetainPhotos.Add(rp);
                                });
                            }


                            //获取经销商信息
                            Store storeInfo = _storeService.GetStoreById((int)store);
                            //判断是否上报CS并构造上报信息
                            if (storeInfo.SslEnabled)
                            {
                                //构造明细信息
                                var sb = _saleBillService.GetSaleBillById(store, data.BillId, true);
                                if (sb.Items == null || sb.Items.Count == 0)
                                {
                                    return this.Error("单据明细为空");
                                }
                                orderDetails = new List<OrderDetail>();
                                foreach (SaleItem saleItem in sb.Items)
                                {
                                    OrderDetail detail = new OrderDetail();
                                    //获取erp编码
                                    Product product= _productService.GetProductById(store, saleItem.ProductId);
                                    detail.productCode = product.Sku+ product.ManufacturerPartNumber;
                                    detail.productName = product.Name;
                                    //默认小单位转换 计算小单位数量
                                    int convertedQuantity = 1;
                                    if (saleItem.UnitId == product.BigUnitId)
                                    {
                                        if (product.BigQuantity==null|| product.BigQuantity ==0)
                                        {
                                            return this.Error(product.Name + "商品大单位信息维护错误，请检查");
                                        }
                                        convertedQuantity = (int)product.BigQuantity;
                                    }else if(saleItem.UnitId == product.StrokeUnitId)
                                    {
                                        if (product.StrokeQuantity == null || product.StrokeQuantity == 0)
                                        {
                                            return this.Error(product.Name + "商品中单位信息维护错误，请检查");
                                        }
                                        convertedQuantity = (int)product.StrokeQuantity;
                                    }else if (saleItem.UnitId == product.SmallUnitId)
                                    {
                                        convertedQuantity = 1;
                                    }
                                    else
                                    {
                                        return this.Error(product.Name + "商品单位信息维护错误，请检查");
                                    }

                                    detail.productAmount = saleItem.Quantity * convertedQuantity;
                                    detail.type = 0;
                                    detail.goodsCategory = 31;
                                    orderDetails.Add(detail);
                                }
                                //构造上报信息
                                Terminal terminal = _terminalService.FindTerminalById(store, bill.TerminalId);
                                terminalSignReport = new TerminalSignReport();
                                terminalSignReport.source = 5;
                                terminalSignReport.signType = 0;
                                terminalSignReport.sttsBillNo = "5" + bill.BillNumber;
                                //在OC和DC经销商是0142000105  在CS是142000105 所以判断如果第一位是0就截取第一位
                                string storeCode= storeInfo.Code;
                                if (storeCode.StartsWith("0"))
                                {
                                    storeCode = storeCode.Substring(1, storeCode.Length - 1);
                                }
                                terminalSignReport.dealerCode = storeCode;
                                terminalSignReport.dealerName = storeInfo.Name;
                                terminalSignReport.whichCode = terminal.Code;
                                terminalSignReport.whichName = terminal.Name;
                                terminalSignReport.whichName = terminal.Name;
                                terminalSignReport.sttsSignDate = sign.SignedDate?.ToString("yyyy-MM-dd HH:mm:ss");
                                terminalSignReport.sttsCreatedDate = sign.SignedDate?.ToString("yyyy-MM-dd HH:mm:ss");
                                terminalSignReport.latitude = data.Latitude == null ? "" : data.Latitude + "";
                                terminalSignReport.longitude = data.Longitude == null ? "" : data.Longitude + "";
                                terminalSignReport.region = storeInfo.QuyuCode;
                                terminalSignReport.cimformOnUtc = DateTime.Now;
                                terminalSignReport.directType = "1";
                                terminalSignReport.orderType = "1";
                                terminalSignReport.StoreId = store ?? 0;

                            }
                        }

                        //费用支出
                        if (data.BillTypeId == (int)BillTypeEnum.CostExpenditureBill)
                        {
                            var bill = _costExpenditureBillService.GetCostExpenditureBillById(store, data.BillId);
                            if (bill != null)
                            {
                                bill.SignStatus = 1;
                                _costExpenditureBillService.UpdateCostExpenditureBill(bill);
                            }
                            sign = new DeliverySign
                            {
                                StoreId = store ?? 0,
                                BillTypeId = (int)BillTypeEnum.CostExpenditureBill,
                                BillId = data.BillId,
                                BillNumber = bill.BillNumber,
                                ToBillId = 0,
                                ToBillNumber = "",
                                TerminalId = bill.TerminalId,
                                BusinessUserId = bill.EmployeeId,
                                DeliveryUserId = 0,
                                Latitude = data.Latitude,
                                Longitude = data.Longitude,
                                SignUserId = userId ?? 0,
                                SignedDate = DateTime.Now,
                                SumAmount = bill.SumAmount,
                                Signature = data.Signature,
                                SignStatus = 1
                            };

                            //留存照片
                            if (data.RetainPhotos != null && data.RetainPhotos.Any())
                            {
                                data.RetainPhotos.ForEach(p =>
                                {
                                    var rp = p.ToEntity<RetainPhoto>();
                                    rp.StoreId = store ?? 0;
                                    sign.RetainPhotos.Add(rp);
                                });
                            }
                        }

                        //RedLock 
                        var results = await _locker.PerformActionWithLockAsync(this.LockKey(data, store ?? 0, userId ?? 0),
                        TimeSpan.FromSeconds(30),
                        TimeSpan.FromSeconds(10),
                        TimeSpan.FromSeconds(1),
                        () => _dispatchBillService.SignConfirm(store ?? 0, userId ?? 0, sign,terminalSignReport,orderDetails));
                        return results.To(results);
                    }

                    return this.Successful("单据已签收");
                }
                catch (Exception ex)
                {
                    _userActivityService.InsertActivity("DeliverySignConfirm", ex.Message, user, userId ?? 0);
                    return this.Error(ex.Message);
                }
            });
        }


        /// <summary>
        /// 用于换货单换货确认
        /// </summary>
        [HttpPost("exchangebill/exchangeSignIn/{store}/{userId}")]
        [SwaggerOperation("exchangeSignIn")]
        public async Task<APIResult<dynamic>> ExchangeSignIn(DeliverySignUpdateModel data, int? store, int? userId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error(Resources.ParameterError);

            return await Task.Run(() =>
            {
                User user = null;
                try
                {
                    if (data == null)
                        return this.Error("调度单据不存在");

                    //获取调度项目
                    var dispatchItem = _dispatchBillService.GetDispatchItemsById(store, data.DispatchItemId);
                    //获取调度单
                    var dispatchBill = _dispatchBillService.GetDispatchBillById(store, dispatchItem?.DispatchBillId ?? 0);
                    if (dispatchItem == null || dispatchBill == null)
                    {
                        return this.Error("调度单据不存在");
                    }

                    var exchange = _exchangeBillService.GetExchangeBillById(store, dispatchItem.BillId, true);
                    exchange.Operation = (int)OperationEnum.APP;

                    if (exchange == null)
                    {
                        return this.Error("换货单据不存在");
                    }

                    var photos = data.RetainPhotos.Select(s => s.ToEntity<RetainPhoto>()).ToList();
                    var result = _exchangeBillService.ExchangeSignIn(store ?? 0, userId ?? 0, exchange, dispatchItem, photos, data.Signature);

                    return result.To(result);
                }
                catch (Exception ex)
                {
                    _userActivityService.InsertActivity("ExchangeSignIn", ex.Message, user, userId ?? 0);
                    return this.Error(ex.Message);
                }
            });
        }
    }
}