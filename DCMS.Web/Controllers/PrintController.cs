using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DCMS.Core;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Sales;
using DCMS.Core.Domain.Terminals;
using DCMS.Core.Domain.Users;
using DCMS.Core.Domain.WareHouses;
using DCMS.Services.Configuration;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Products;
using DCMS.Services.Sales;
using DCMS.Services.Settings;
using DCMS.Services.Terminals;
using DCMS.Services.Users;
using DCMS.Services.WareHouses;
using DCMS.ViewModel.Models.Products;
using DCMS.ViewModel.Models.Sales;
using DCMS.Web.Infrastructure.Mapper.Extensions;
using Microsoft.AspNetCore.Mvc;
using DCMS.Web.Models;

namespace DCMS.Web.Controllers
{
    public class PrintController : BasePublicController
    {
        private readonly ISaleReservationBillService _saleReservationBillService;
        private readonly IAccountingService _accountingService;
        private readonly ISaleBillService _saleBillService;
        private readonly IProductService _productService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IUserService _userService;
        private readonly ISettingService _settingService;
        private readonly IWareHouseService _wareHouseService;
        private readonly ITerminalService _terminalService; 
        public PrintController(ISaleReservationBillService saleReservationBillService, ISaleBillService saleBillService, IAccountingService accountingService, IProductService productService, ISpecificationAttributeService specificationAttributeService, IUserService userService, ISettingService settingService, IWareHouseService wareHouseService, ITerminalService terminalService, IStoreContext storeContext, IWorkContext workContext, ILogger loggerService, INotificationService notificationService) : base(workContext, loggerService, storeContext, notificationService)
        {
            _saleReservationBillService = saleReservationBillService;
            _saleBillService = saleBillService;
            _accountingService = accountingService;
            _productService = productService;
            _specificationAttributeService = specificationAttributeService;
            _userService = userService;
            _settingService = settingService;
            _wareHouseService = wareHouseService;
            _terminalService = terminalService;
        }

        public JsonResult Print(int? type, int? billType, int[] billIds, int? terminalId, string terminalName, int? businessUserId, int? deliveryUserId = null, string billNumber = "", int? wareHouseId = null, string remark = "", DateTime? startTime = null, DateTime? endTime = null, int? districtId = null, bool? auditedStatus = null, bool? sortByAuditedTime = null, bool? showReverse = null, bool? showReturn = null, bool? alreadyChange = false, int? paymentMethodType = null, int? billSourceType = null)
        {
            
            string RenderHtml = "";
            if (billType.HasValue)
            {
                var pCPrintSetting = _settingService.LoadSetting<PCPrintSetting>(_storeContext.CurrentStore.Id);
                switch (billType.Value)
                {
                    case (int)BillTypeEnum.PurchaseReservationBill:
                        getSaleReservationBills(type, billIds, terminalId, terminalName, businessUserId, deliveryUserId, billNumber, wareHouseId, remark, startTime, endTime, districtId, auditedStatus, sortByAuditedTime, showReverse, showReturn, alreadyChange, paymentMethodType, billSourceType).ForEach(model =>
                        {
                            ViewBag.StoreName = curStore.Name;
                            ViewBag.StoreAddress = pCPrintSetting?.Address;
                            ViewBag.PlaceOrderTelphone = pCPrintSetting?.PlaceOrderTelphone;
                            model.PrintDate = DateTime.Now;
                            RenderHtml += RenderPartialViewToString("AsyncPurchaseReservationBillPrint", model);
                        });
                        break;
                    case (int)BillTypeEnum.PurchaseBill:
                        getSaleReservationBills(type, billIds, terminalId, terminalName, businessUserId, deliveryUserId, billNumber, wareHouseId, remark, startTime, endTime, districtId, auditedStatus, sortByAuditedTime, showReverse, showReturn, alreadyChange, paymentMethodType, billSourceType).ForEach(model =>
                        {
                            ViewBag.StoreName = curStore.Name;
                            ViewBag.StoreAddress = pCPrintSetting?.Address;
                            ViewBag.PlaceOrderTelphone = pCPrintSetting?.PlaceOrderTelphone;
                            model.PrintDate = DateTime.Now;
                            RenderHtml += RenderPartialViewToString("AsyncPurchaseBillPrint", model);
                        });
                        break;
                    case (int)BillTypeEnum.PurchaseReturnReservationBill:
                        getSaleReservationBills(type, billIds, terminalId, terminalName, businessUserId, deliveryUserId, billNumber, wareHouseId, remark, startTime, endTime, districtId, auditedStatus, sortByAuditedTime, showReverse, showReturn, alreadyChange, paymentMethodType, billSourceType).ForEach(model =>
                        {
                            ViewBag.StoreName = curStore.Name;
                            ViewBag.StoreAddress = pCPrintSetting?.Address;
                            ViewBag.PlaceOrderTelphone = pCPrintSetting?.PlaceOrderTelphone;
                            model.PrintDate = DateTime.Now;
                            RenderHtml += RenderPartialViewToString("AsyncPurchaseReturnReservationBillPrint", model);
                        });
                        break;
                    case (int)BillTypeEnum.PurchaseReturnBill:
                        getSaleReservationBills(type, billIds, terminalId, terminalName, businessUserId, deliveryUserId, billNumber, wareHouseId, remark, startTime, endTime, districtId, auditedStatus, sortByAuditedTime, showReverse, showReturn, alreadyChange, paymentMethodType, billSourceType).ForEach(model =>
                        {
                            ViewBag.StoreName = curStore.Name;
                            ViewBag.StoreAddress = pCPrintSetting?.Address;
                            ViewBag.PlaceOrderTelphone = pCPrintSetting?.PlaceOrderTelphone;
                            model.PrintDate = DateTime.Now;
                            RenderHtml += RenderPartialViewToString("AsyncPurchaseReturnBillPrint", model);
                        });
                        break;
                    case (int)BillTypeEnum.SaleReservationBill:
                        getSaleReservationBills(type, billIds, terminalId, terminalName, businessUserId, deliveryUserId, billNumber, wareHouseId, remark, startTime, endTime, districtId, auditedStatus, sortByAuditedTime, showReverse, showReturn, alreadyChange, paymentMethodType, billSourceType).ForEach(model =>
                        {
                            ViewBag.StoreName = curStore.Name;
                            ViewBag.StoreAddress = pCPrintSetting?.Address;
                            ViewBag.PlaceOrderTelphone = pCPrintSetting?.PlaceOrderTelphone;
                            model.PrintDate = DateTime.Now;
                            RenderHtml += RenderPartialViewToString("AsyncSaleReservationBillPrint", model);
                        });
                        break;
                    case (int)BillTypeEnum.SaleBill:
                        getSaleBills(type, billIds, terminalId, terminalName, businessUserId, deliveryUserId, billNumber, wareHouseId, remark, startTime, endTime, districtId, auditedStatus, sortByAuditedTime, showReverse, showReturn, alreadyChange, paymentMethodType, billSourceType).ForEach(model
                            =>
                        {
                            ViewBag.StoreName = curStore.Name;
                            ViewBag.StoreAddress = pCPrintSetting?.Address;
                            ViewBag.PlaceOrderTelphone = pCPrintSetting?.PlaceOrderTelphone;
                            model.PrintDate = DateTime.Now;
                            RenderHtml += RenderPartialViewToString("AsyncSaleBillPrint", model);
                        });
                        break;
                    case (int)BillTypeEnum.ReturnReservationBill:
                        getSaleReservationBills(type, billIds, terminalId, terminalName, businessUserId, deliveryUserId, billNumber, wareHouseId, remark, startTime, endTime, districtId, auditedStatus, sortByAuditedTime, showReverse, showReturn, alreadyChange, paymentMethodType, billSourceType).ForEach(model =>
                        {
                            ViewBag.StoreName = curStore.Name;
                            ViewBag.StoreAddress = pCPrintSetting?.Address;
                            ViewBag.PlaceOrderTelphone = pCPrintSetting?.PlaceOrderTelphone;
                            model.PrintDate = DateTime.Now;
                            RenderHtml += RenderPartialViewToString("AsyncReturnReservationBillPrint", model);
                        });
                        break;
                    case (int)BillTypeEnum.ReturnBill:
                        getSaleBills(type, billIds, terminalId, terminalName, businessUserId, deliveryUserId, billNumber, wareHouseId, remark, startTime, endTime, districtId, auditedStatus, sortByAuditedTime, showReverse, showReturn, alreadyChange, paymentMethodType, billSourceType).ForEach(model
                            =>
                        {
                            ViewBag.StoreName = curStore.Name;
                            ViewBag.StoreAddress = pCPrintSetting?.Address;
                            ViewBag.PlaceOrderTelphone = pCPrintSetting?.PlaceOrderTelphone;
                            model.PrintDate = DateTime.Now;
                            RenderHtml += RenderPartialViewToString("AsyncReturnBillPrint", model);
                        });
                        break;
                    case (int)BillTypeEnum.AllocationBill:
                        getSaleBills(type, billIds, terminalId, terminalName, businessUserId, deliveryUserId, billNumber, wareHouseId, remark, startTime, endTime, districtId, auditedStatus, sortByAuditedTime, showReverse, showReturn, alreadyChange, paymentMethodType, billSourceType).ForEach(model
                            =>
                        {
                            ViewBag.StoreName = curStore.Name;
                            ViewBag.StoreAddress = pCPrintSetting?.Address;
                            ViewBag.PlaceOrderTelphone = pCPrintSetting?.PlaceOrderTelphone;
                            model.PrintDate = DateTime.Now;
                            RenderHtml += RenderPartialViewToString("AsyncAllocationBillPrint", model);
                        });
                        break;
                }
            }
            return Json(new
            {
                Success = true,
                RenderHtml = RenderHtml
            });
        }

        private List<SaleReservationBillModel> getSaleReservationBills(int? type, int[] billIds, int? terminalId, string terminalName, int? businessUserId, int? deliveryUserId = null, string billNumber = "", int? wareHouseId = null, string remark = "", DateTime? startTime = null, DateTime? endTime = null, int? districtId = null, bool? auditedStatus = null, bool? sortByAuditedTime = null, bool? showReverse = null, bool? showReturn = null, bool? alreadyChange = false, int? paymentMethodType = null, int? billSourceType = null)
        {
            var models = new List<SaleReservationBillModel>();
            //默认选择
            type ??= 1;
            if (type == 1)
            {
                if (billIds != null && billIds.Any())
                {
                    foreach (var id in billIds)
                    {
                        var saleReservationBill = _saleReservationBillService.GetSaleReservationBillById(curStore.Id, id, true);
                        if (saleReservationBill != null)
                        {
                            saleReservationBill.SaleReservationBillAccountings.Select(s =>
                            {
                                s.AccountingOptionName = _accountingService.GetAccountingOptionName(curStore.Id, s.AccountingOptionId);
                                return s;
                            }).ToList();
                            saleReservationBill.PrintNum += 1;
                            _saleReservationBillService.UpdateSaleReservationBill(saleReservationBill);
                            var model = saleReservationBill.ToModel<SaleReservationBillModel>();
                            model.Items = _saleReservationBillService.GetSaleReservationItemList(model.Id).Select(s => s.ToModel<SaleReservationItemModel>()).ToList();
                            model.TerminalName = _terminalService.GetTerminalById(curStore.Id, model.TerminalId)?.Name;
                            model.MakeUserName = _userService.GetUserById(curStore.Id, model.MakeUserId)?.UserRealName;
                            model.BusinessUserName = _userService.GetUserById(curStore.Id, model.BusinessUserId ?? 0)?.UserRealName;
                            var allProducts = _productService.GetProductsByIds(curStore.Id, model.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                            var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());
                            var allProductPrices = _productService.GetProductPricesByProductIds(curStore.Id, model.Items.Select(gm => gm.ProductId).Distinct().ToArray());
                            var allProductTierPrices = _productService.GetProductTierPriceByProductIds(curStore.Id, model.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                            model.Items = model.Items.Select(m =>
                            {
                                var product = allProducts.Where(ap => ap.Id == m.ProductId).FirstOrDefault();
                                if (product != null)
                                {
                                    if (m.UnitId == product.SmallUnitId)
                                    {
                                        m.Quantity = m.Quantity;
                                        m.Price = m.Price;
                                    }
                                    else if (m.UnitId == product.StrokeUnitId)
                                    {
                                        m.Price = m.Price / (product.StrokeQuantity ?? 1);
                                        m.Quantity = m.Quantity * (product.StrokeQuantity ?? 1);
                                        m.UnitId = product.SmallUnitId;
                                    }
                                    else if (m.UnitId == product.BigUnitId)
                                    {
                                        m.Price = m.Price / (product.BigQuantity ?? 1);
                                        m.Quantity = m.Quantity * (product.BigQuantity ?? 1);
                                        m.UnitId = product.SmallUnitId;
                                    }

                                    //这里替换成高级用法
                                    m = product.InitBaseModel<SaleReservationItemModel>(m, model.WareHouseId, allOptions, allProductPrices, allProductTierPrices, _productService);

                                    var qty = Pexts.StockQuantityFormat(m.Quantity, product.StrokeQuantity ?? 0, product.BigQuantity ?? 0);
                                    m.BigQty = qty.Item1;
                                    m.StrokeQty = qty.Item2;
                                    m.SmallQty = qty.Item3;
                                    //商品信息
                                    m.BigUnitId = product.BigUnitId;
                                    m.StrokeUnitId = product.StrokeUnitId;
                                    m.SmallUnitId = product.SmallUnitId;
                                    m.SmallUnitName = allOptions.Where(ao => ao.Id == product.SmallUnitId).FirstOrDefault()?.Name;
                                    m.IsManufactureDete = product.IsManufactureDete;
                                    m.ExpirationDays = product.ExpirationDays;
                                    m.ProductTimes = _productService.GetProductDates(curStore.Id, product.Id, model.WareHouseId);
                                    m.SaleProductTypeName = CommonHelper.GetEnumDescription<SaleProductTypeEnum>(m.SaleProductTypeId ?? 0);
                                    //税价总计
                                    m.TaxPriceAmount = m.Amount;
                                    if (saleReservationBill.TaxAmount > 0 && m.TaxRate > 0)
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

                            models.Add(model);
                        }
                    }
                }
            }
            else if (type == 2)
            {
                _saleReservationBillService.GetSaleReservationBillList(curStore?.Id ?? 0, curUser.Id, terminalId, terminalName, businessUserId, deliveryUserId, billNumber, wareHouseId, remark, startTime, endTime, districtId, auditedStatus, sortByAuditedTime, showReverse, showReturn, alreadyChange, false, 0).ToList()?.ForEach(bill => {
                    bill.SaleReservationBillAccountings.Select(s =>
                    {
                        s.AccountingOptionName = _accountingService.GetAccountingOptionName(curStore.Id, s.AccountingOptionId);
                        return s;
                    }).ToList();
                    bill.PrintNum += 1;
                    _saleReservationBillService.UpdateSaleReservationBill(bill);
                    var model = bill.ToModel<SaleReservationBillModel>();
                    model.Items = _saleReservationBillService.GetSaleReservationItemList(model.Id).Select(s => s.ToModel<SaleReservationItemModel>()).ToList();
                    model.TerminalName = _terminalService.GetTerminalById(curStore.Id, model.TerminalId)?.Name;
                    model.MakeUserName = _userService.GetUserById(curStore.Id, model.MakeUserId)?.UserRealName;
                    model.BusinessUserName = _userService.GetUserById(curStore.Id, model.BusinessUserId ?? 0)?.UserRealName;
                    var allProducts = _productService.GetProductsByIds(curStore.Id, model.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                    var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());
                    var allProductPrices = _productService.GetProductPricesByProductIds(curStore.Id, model.Items.Select(gm => gm.ProductId).Distinct().ToArray());
                    var allProductTierPrices = _productService.GetProductTierPriceByProductIds(curStore.Id, model.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                    model.Items = model.Items.Select(m =>
                    {
                        var product = allProducts.Where(ap => ap.Id == m.ProductId).FirstOrDefault();
                        if (product != null)
                        {
                            if (m.UnitId == product.SmallUnitId)
                            {
                                m.Quantity = m.Quantity;
                                m.Price = m.Price;
                            }
                            else if (m.UnitId == product.StrokeUnitId)
                            {
                                m.Price = m.Price / (product.StrokeQuantity ?? 1);
                                m.Quantity = m.Quantity * (product.StrokeQuantity ?? 1);
                                m.UnitId = product.SmallUnitId;
                            }
                            else if (m.UnitId == product.BigUnitId)
                            {
                                m.Price = m.Price / (product.BigQuantity ?? 1);
                                m.Quantity = m.Quantity * (product.BigQuantity ?? 1);
                                m.UnitId = product.SmallUnitId;
                            }

                            //这里替换成高级用法
                            m = product.InitBaseModel<SaleReservationItemModel>(m, model.WareHouseId, allOptions, allProductPrices, allProductTierPrices, _productService);

                            var qty = Pexts.StockQuantityFormat(m.Quantity, product.StrokeQuantity ?? 0, product.BigQuantity ?? 0);
                            m.BigQty = qty.Item1;
                            m.StrokeQty = qty.Item2;
                            m.SmallQty = qty.Item3;
                            //商品信息
                            m.BigUnitId = product.BigUnitId;
                            m.StrokeUnitId = product.StrokeUnitId;
                            m.SmallUnitId = product.SmallUnitId;
                            m.SmallUnitName = allOptions.Where(ao => ao.Id == product.SmallUnitId).FirstOrDefault()?.Name;
                            m.IsManufactureDete = product.IsManufactureDete;
                            m.ExpirationDays = product.ExpirationDays;
                            m.ProductTimes = _productService.GetProductDates(curStore.Id, product.Id, model.WareHouseId);
                            m.SaleProductTypeName = CommonHelper.GetEnumDescription<SaleProductTypeEnum>(m.SaleProductTypeId ?? 0);
                            //税价总计
                            m.TaxPriceAmount = m.Amount;
                            if (bill.TaxAmount > 0 && m.TaxRate > 0)
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
                    models.Add(model);
                });
            }
            return models;
        }

        private List<SaleBillModel> getSaleBills(int? type, int[] billIds, int? terminalId, string terminalName, int? businessUserId, int? deliveryUserId = null, string billNumber = "", int? wareHouseId = null, string remark = "", DateTime? startTime = null, DateTime? endTime = null, int? districtId = null, bool? auditedStatus = null, bool? sortByAuditedTime = null, bool? showReverse = null, bool? showReturn = null, bool? alreadyChange = false, int? paymentMethodType = null, int? billSourceType = null)
        {
            var models = new List<SaleBillModel>();
            //默认选择
            type ??= 1;
            if (type == 1)
            {
                if (billIds != null && billIds.Any())
                {
                    foreach (var id in billIds)
                    {
                        var saleBill = _saleBillService.GetSaleBillById(curStore.Id, id, true);
                        if (saleBill != null)
                        {
                            //获取默认收款账户
                            saleBill.SaleBillAccountings.Select(s =>
                            {
                                s.AccountingOptionName = _accountingService.GetAccountingOptionName(curStore.Id, s.AccountingOptionId);
                                return s;
                            }).ToList();
                            saleBill.PrintNum += 1;
                            _saleBillService.UpdateSaleBill(saleBill);
                            var model = saleBill.ToModel<SaleBillModel>();
                            model.Items = _saleBillService.GetSaleItemList(model.Id).Select(s => s.ToModel<SaleItemModel>()).ToList();
                            model.WareHouseName = _wareHouseService.GetWareHouseById(curStore.Id, model.WareHouseId)?.Name;
                            model.TerminalName = _terminalService.GetTerminalById(curStore.Id, model.TerminalId)?.Name;
                            model.MakeUserName = _userService.GetUserById(curStore.Id, model.MakeUserId)?.UserRealName;
                            model.BusinessUserName = _userService.GetUserById(curStore.Id, model.BusinessUserId ?? 0)?.UserRealName;
                            model.DeliveryUserName = _userService.GetUserById(curStore.Id, model.DeliveryUserId ?? 0)?.UserRealName;
                            var allProducts = _productService.GetProductsByIds(curStore.Id, model.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                            var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());
                            var allProductPrices = _productService.GetProductPricesByProductIds(curStore.Id, model.Items.Select(gm => gm.ProductId).Distinct().ToArray());
                            var allProductTierPrices = _productService.GetProductTierPriceByProductIds(curStore.Id, model.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                            model.Items = model.Items.Select(m =>
                            {
                                var product = allProducts.Where(ap => ap.Id == m.ProductId).FirstOrDefault();
                                if (product != null)
                                {
                                    if (m.UnitId == product.SmallUnitId)
                                    {
                                        m.Quantity = m.Quantity;
                                        m.Price = m.Price;
                                    } 
                                    else if (m.UnitId == product.StrokeUnitId) 
                                    {
                                        m.Price = m.Price / (product.StrokeQuantity ?? 1);
                                        m.Quantity = m.Quantity * (product.StrokeQuantity ?? 1);
                                        m.UnitId = product.SmallUnitId;
                                    }
                                    else if (m.UnitId == product.BigUnitId)
                                    {
                                        m.Price = m.Price / (product.BigQuantity ?? 1);
                                        m.Quantity = m.Quantity * (product.BigQuantity ?? 1);
                                        m.UnitId = product.SmallUnitId;
                                    }

                                    //这里替换成高级用法
                                    m = product.InitBaseModel<SaleItemModel>(m, model.WareHouseId, allOptions, allProductPrices, allProductTierPrices, _productService);
                                    
                                    var qty = Pexts.StockQuantityFormat(m.Quantity, product.StrokeQuantity ?? 0, product.BigQuantity ?? 0);
                                    m.BigQty = qty.Item1;
                                    m.StrokeQty = qty.Item2;
                                    m.SmallQty = qty.Item3;
                                    //商品信息
                                    m.BigUnitId = product.BigUnitId;
                                    m.StrokeUnitId = product.StrokeUnitId;
                                    m.SmallUnitId = product.SmallUnitId;
                                    m.SmallUnitName = allOptions.Where(ao => ao.Id == product.SmallUnitId).FirstOrDefault()?.Name;
                                    m.IsManufactureDete = product.IsManufactureDete;
                                    m.ExpirationDays = product.ExpirationDays;
                                    m.ProductTimes = _productService.GetProductDates(curStore.Id, product.Id, model.WareHouseId);
                                    m.SaleProductTypeName = CommonHelper.GetEnumDescription<SaleProductTypeEnum>(m.SaleProductTypeId ?? 0);
                                    //税价总计
                                    m.TaxPriceAmount = m.Amount;
                                    if (saleBill.TaxAmount > 0 && m.TaxRate > 0)
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

                            models.Add(model);
                        }
                    }
                }
            }
            else if (type == 2)
            {
                _saleBillService.GetSaleBillList(curStore?.Id ?? 0, curUser.Id, terminalId, terminalName, businessUserId, deliveryUserId, billNumber, wareHouseId, remark, startTime, endTime, districtId, auditedStatus, sortByAuditedTime, showReverse, showReturn, paymentMethodType, billSourceType, null, false, null, null, 0).ToList()?.ForEach(bill => {
                    //获取默认收款账户
                    bill.SaleBillAccountings.Select(s =>
                    {
                        s.AccountingOptionName = _accountingService.GetAccountingOptionName(curStore.Id, s.AccountingOptionId);
                        return s;
                    }).ToList();
                    bill.PrintNum += 1;
                    _saleBillService.UpdateSaleBill(bill);
                    var model = bill.ToModel<SaleBillModel>();
                    model.Items = _saleBillService.GetSaleItemList(model.Id).Select(s => s.ToModel<SaleItemModel>()).ToList();
                    model.WareHouseName = _wareHouseService.GetWareHouseById(curStore.Id, model.WareHouseId)?.Name;
                    model.TerminalName = _terminalService.GetTerminalById(curStore.Id, model.TerminalId)?.Name;
                    var allProducts = _productService.GetProductsByIds(curStore.Id, model.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                    model.MakeUserName = _userService.GetUserById(curStore.Id, model.MakeUserId)?.UserRealName;
                    model.BusinessUserName = _userService.GetUserById(curStore.Id, model.BusinessUserId ?? 0)?.UserRealName;
                    model.DeliveryUserName = _userService.GetUserById(curStore.Id, model.DeliveryUserId ?? 0)?.UserRealName;
                   
                    var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());
                    var allProductPrices = _productService.GetProductPricesByProductIds(curStore.Id, model.Items.Select(gm => gm.ProductId).Distinct().ToArray());
                    var allProductTierPrices = _productService.GetProductTierPriceByProductIds(curStore.Id, model.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                    model.Items = model.Items.Select(m =>
                    {
                        var product = allProducts.Where(ap => ap.Id == m.ProductId).FirstOrDefault();
                        if (product != null)
                        {
                            if (m.UnitId == product.SmallUnitId)
                            {
                                m.Quantity = m.Quantity;
                                m.Price = m.Price;
                            }
                            else if (m.UnitId == product.StrokeUnitId)
                            {
                                m.Price = m.Price / (product.StrokeQuantity ?? 1);
                                m.Quantity = m.Quantity * (product.StrokeQuantity ?? 1);
                                m.UnitId = product.SmallUnitId;
                            }
                            else if (m.UnitId == product.BigUnitId)
                            {
                                m.Price = m.Price / (product.BigQuantity ?? 1);
                                m.Quantity = m.Quantity * (product.BigQuantity ?? 1);
                                m.UnitId = product.SmallUnitId;
                            }
                            //这里替换成高级用法
                            m = product.InitBaseModel<SaleItemModel>(m, model.WareHouseId, allOptions, allProductPrices, allProductTierPrices, _productService);
                            var qty = Pexts.StockQuantityFormat(m.Quantity, product.StrokeQuantity ?? 0, product.BigQuantity ?? 0);
                            m.SmallQty = qty.Item1;
                            m.StrokeQty = qty.Item2;
                            m.BigQty = qty.Item3;
                            //商品信息
                            m.BigUnitId = product.BigUnitId;
                            m.StrokeUnitId = product.StrokeUnitId;
                            m.SmallUnitId = product.SmallUnitId;
                            m.SmallUnitName = allOptions.Where(ao => ao.Id == product.SmallUnitId).FirstOrDefault()?.Name;
                            m.IsManufactureDete = product.IsManufactureDete;
                            m.ExpirationDays = product.ExpirationDays;
                            m.ProductTimes = _productService.GetProductDates(curStore.Id, product.Id, model.WareHouseId);
                            m.SaleProductTypeName = CommonHelper.GetEnumDescription<SaleProductTypeEnum>(m.SaleProductTypeId ?? 0);
                            //税价总计
                            m.TaxPriceAmount = m.Amount;
                            if (bill.TaxAmount > 0 && m.TaxRate > 0)
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
                    models.Add(model);
                });
            }
            return models;
        }
    }
}
