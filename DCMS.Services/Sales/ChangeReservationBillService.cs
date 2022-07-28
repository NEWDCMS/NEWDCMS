using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Data;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.Sales;
using DCMS.Core.Domain.Tasks;
using DCMS.Core.Domain.WareHouses;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using DCMS.Services.Finances;
using DCMS.Services.Products;
using DCMS.Services.Tasks;
using DCMS.Services.Users;
using DCMS.Services.WareHouses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DCMS.Services.Sales
{
    public class ChangeReservationBillService : BaseService, IChangeReservationBillService
    {

        private readonly IUserService _userService;
        private readonly IQueuedMessageService _queuedMessageService;
        private readonly IStockService _stockService;
        private readonly IWareHouseService _wareHouseService;
        private readonly IProductService _productService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly ICostContractBillService _costContractBillService;
        private readonly ISaleReservationBillService _saleReservationBillService;
        private readonly ISaleBillService _saleBillService;
        private readonly IReturnReservationBillService _returnReservationBillService;
        private readonly IReturnBillService _returnBillService;

        public ChangeReservationBillService(IServiceGetter serviceGetter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher,
            IUserService userService,
            IQueuedMessageService queuedMessageService,
            IStockService stockService,
            IWareHouseService wareHouseService,
            IProductService productService,
            ISpecificationAttributeService specificationAttributeService,
            ICostContractBillService costContractBillService,
            ISaleReservationBillService saleReservationBillService,
            ISaleBillService saleBillService,
            IReturnReservationBillService returnReservationBillService,
            IReturnBillService returnBillService
            ) : base(serviceGetter, cacheManager, eventPublisher)
        {
            _userService = userService;
            _queuedMessageService = queuedMessageService;
            _stockService = stockService;
            _wareHouseService = wareHouseService;
            _productService = productService;
            _specificationAttributeService = specificationAttributeService;
            _costContractBillService = costContractBillService;
            _saleReservationBillService = saleReservationBillService;
            _saleBillService = saleBillService;
            _returnReservationBillService = returnReservationBillService;
            _returnBillService = returnBillService;
        }


        public BaseResult BillCreateOrUpdate(int storeId, int userId, ChangeReservationBillUpdate data, bool isAdmin = false)
        {
            var uow = SaleReservationBillsRepository.UnitOfWork;

            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();



                //获取当前用户管理员用户 电话号码 多个用"|"分隔
                var adminMobileNumbers = _userService.GetAllAdminUserMobileNumbersByStore(storeId).ToList();

                List<string> idList = data.Ids.Split(',').ToList();

                string errMsg = string.Empty;
                bool fg;

                //销售订单
                if (data.BillType == (int)BillTypeEnum.SaleReservationBill)
                {
                    //一次查询所有涉及单据
                    List<int> ids = new List<int>();
                    idList.ForEach(i =>
                    {
                        ids.Add(int.Parse(i));
                    });
                    var saleReservations = _saleReservationBillService.GetSaleReservationBillsByIds(ids.ToArray()).ToList();

                    #region 验证盘点 注意这里要验证 销售订单 仓库，和转单后 仓库 2个仓库是否存在盘点
                    string thisMsg = string.Empty;
                    foreach (var a in idList)
                    {
                        int id = int.Parse(a);
                        SaleReservationBill saleReservation = saleReservations.Where(sr => sr.Id == id).FirstOrDefault();
                        if (saleReservation != null && saleReservation.Items != null && saleReservation.Items.Count > 0)
                        {
                            //销售订单 仓库
                            fg = _wareHouseService.CheckProductInventory(storeId, saleReservation.WareHouseId, saleReservation.Items.Select(it => it.ProductId).Distinct().ToArray(), out thisMsg);
                            //新仓库 如果不等于 销售订单仓库
                            if (saleReservation.WareHouseId != data.WareHouseId)
                            {
                                fg = _wareHouseService.CheckProductInventory(storeId, data.WareHouseId, saleReservation.Items.Select(it => it.ProductId).Distinct().ToArray(), out thisMsg);
                            }
                        }
                        if (!string.IsNullOrEmpty(thisMsg))
                        {
                            return new BaseResult { Success = false, Message = thisMsg };
                        }

                    }
                    #endregion

                    #region 验证库存

                    foreach (var a in idList)
                    {
                        int id = int.Parse(a);
                        SaleReservationBill saleReservation = saleReservations.Where(sr => sr.Id == id).FirstOrDefault();
                        if (saleReservation != null && saleReservation.Items != null && saleReservation.Items.Count > 0)
                        {
                            //将一个单据中 相同商品 数量 按最小单位汇总
                            List<ProductStockItem> stockProducts = new List<ProductStockItem>();
                            var allProducts = _productService.GetProductsByIds(storeId, saleReservation.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                            var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(storeId, allProducts.GetProductBigStrokeSmallUnitIds());

                            foreach (SaleReservationItem item in saleReservation.Items)
                            {
                                if (item.ProductId != 0)
                                {
                                    var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                                    if (product != null)
                                    {
                                        ProductStockItem productStockItem = stockProducts.Where(b => b.ProductId == item.ProductId).FirstOrDefault();
                                        //商品转化量
                                        var conversionQuantity = product.GetConversionQuantity(allOptions, item.UnitId);
                                        //库存量增量 = 单位转化量 * 数量
                                        int thisQuantity = item.Quantity * conversionQuantity;
                                        if (productStockItem != null)
                                        {
                                            productStockItem.Quantity += thisQuantity;
                                        }
                                        else
                                        {
                                            productStockItem = new ProductStockItem
                                            {
                                                ProductId = item.ProductId,
                                                //UnitId = item.UnitId,
                                                UnitId = product.SmallUnitId,
                                                SmallUnitId = product.SmallUnitId,
                                                BigUnitId = product.BigUnitId ?? 0,
                                                ProductName = allProducts.Where(s => s.Id == item.ProductId).FirstOrDefault()?.Name,
                                                ProductCode = allProducts.Where(s => s.Id == item.ProductId).FirstOrDefault()?.ProductCode,
                                                Quantity = thisQuantity
                                            };

                                            stockProducts.Add(productStockItem);
                                        }
                                    }
                                }

                            }
                            //验证库存
                            //验证当前商品库存 注意是新仓库
                            fg = _stockService.CheckStockQty(_productService, _specificationAttributeService, storeId, data.WareHouseId, stockProducts, out errMsg);
                            if (fg == false)
                            {
                                return new BaseResult { Success = false, Message = errMsg };
                            }
                        }
                    }

                    #endregion

                    #region 验证收款账户
                    foreach (var a in idList)
                    {
                        int id = int.Parse(a);
                        SaleReservationBill saleReservation = saleReservations.Where(sr => sr.Id == id).FirstOrDefault();
                        if (saleReservation == null && (saleReservation.SaleReservationBillAccountings != null || saleReservation.SaleReservationBillAccountings.Count == 0))
                        {
                            return new BaseResult { Success = false, Message = $"单据：{saleReservation.BillNumber}收款账户未指定!" };
                        }
                    }
                    #endregion


                    idList.ForEach(a =>
                    {
                        int id = int.Parse(a);
                        SaleReservationBill saleReservation = saleReservations.Where(sr => sr.Id == id).FirstOrDefault();
                        if (saleReservation != null && saleReservation.StoreId == storeId && saleReservation.ChangedStatus == false)
                        {
                            //销售订单 转 销售单 公共方法
                            //_saleReservationBillService.ChangeReservation(userId, storeId, id, data.WareHouseId, data.DeliveryUserId, data.TransactionDate ?? DateTime.Now, data.Operation);

                            #region 修改库存
                            var bill = _saleBillService.GetSaleBillBySaleReservationBillId(storeId, id);
                            //预占库存
                            if (bill != null && bill.Items != null && bill.Items.Count > 0)
                            {
                                //将一个单据中 相同商品 数量 按最小单位汇总
                                var stockProducts = new List<ProductStockItem>();
                                var allProducts = _productService.GetProductsByIds(bill.StoreId, bill.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                                var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(storeId, allProducts.GetProductBigStrokeSmallUnitIds());

                                foreach (SaleItem item in bill.Items)
                                {
                                    var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                                    ProductStockItem productStockItem = stockProducts.Where(b => b.ProductId == item.ProductId).FirstOrDefault();
                                    //商品转化量
                                    var conversionQuantity = product.GetConversionQuantity(allOptions, item.UnitId);
                                    //库存量增量 = 单位转化量 * 数量
                                    int thisQuantity = item.Quantity * conversionQuantity;
                                    if (productStockItem != null)
                                    {
                                        productStockItem.Quantity += thisQuantity;
                                    }
                                    else
                                    {
                                        productStockItem = new ProductStockItem
                                        {
                                            ProductId = item.ProductId,
                                            UnitId = item.UnitId,
                                            SmallUnitId = product.SmallUnitId,
                                            BigUnitId = product.BigUnitId ?? 0,
                                            ProductName = allProducts.Where(s => s.Id == item.ProductId).FirstOrDefault()?.Name,
                                            ProductCode = allProducts.Where(s => s.Id == item.ProductId).FirstOrDefault()?.ProductCode,
                                            Quantity = thisQuantity
                                        };

                                        stockProducts.Add(productStockItem);
                                    }
                                }

                                List<ProductStockItem> productStockItemThiss = new List<ProductStockItem>();
                                if (stockProducts != null && stockProducts.Count > 0)
                                {
                                    stockProducts.ForEach(psi =>
                                    {
                                        ProductStockItem productStockItem = new ProductStockItem
                                        {
                                            ProductId = psi.ProductId,
                                            UnitId = psi.UnitId,
                                            SmallUnitId = psi.SmallUnitId,
                                            BigUnitId = psi.BigUnitId,
                                            ProductName = allProducts.Where(s => s.Id == psi.ProductId).FirstOrDefault()?.Name,
                                            ProductCode = allProducts.Where(s => s.Id == psi.ProductId).FirstOrDefault()?.ProductCode,
                                            Quantity = psi.Quantity * (-1)
                                        };
                                        productStockItemThiss.Add(productStockItem);
                                    });
                                }

                                //验证是否有预占
                                var checkOrderQuantity = _stockService.CheckOrderQuantity(storeId, BillTypeEnum.SaleReservationBill, saleReservation.BillNumber, saleReservation.WareHouseId);
                                if (checkOrderQuantity)
                                {
                                    //销售订单释放预占
                                    _stockService.AdjustStockQty<SaleReservationBill, SaleReservationItem>(saleReservation, _productService, _specificationAttributeService, DirectionEnum.In, StockQuantityType.OrderQuantity, saleReservation.WareHouseId, productStockItemThiss, StockFlowChangeTypeEnum.Audited);
                                }
                                //销售单减少现货
                                _stockService.AdjustStockQty<SaleBill, SaleItem>(bill, _productService, _specificationAttributeService, DirectionEnum.Out, StockQuantityType.CurrentQuantity, saleReservation.WareHouseId, productStockItemThiss, StockFlowChangeTypeEnum.Audited);

                            }

                            #endregion

                            #region 记录上次售价
                            if (bill != null && bill.Items != null && bill.Items.Count > 0 && bill.Items.Where(it => it.Price > 0).Count() > 0)
                            {
                                List<RecordProductPrice> recordProductPrices = new List<RecordProductPrice>();
                                bill.Items.ToList().ForEach(it =>
                                {
                                    recordProductPrices.Add(new RecordProductPrice()
                                    {
                                        StoreId = bill.StoreId,
                                        TerminalId = bill.TerminalId,
                                        ProductId = it.ProductId,
                                        UnitId = it.UnitId,
                                        //Price = it.Price
                                        Price = bill.TaxAmount >0 ? it.Price / (1 + it.TaxRate / 100) : it.Price //注意这里记录税前价格，因为税率用户可以修改，有可能不等于配置税率	
                                    });
                                });
                                //暂时不用这个
                                //_productService.RecordProductTierPriceLastPrice(recordProductPrices);
                                _productService.RecordRecentPriceLastPrice(bill.StoreId, recordProductPrices);
                            }
                            #endregion

                            #region 赠品信息修改（减）、记录赠送记录
                            if (bill != null && bill.Items.Count > 0)
                            {
                                _costContractBillService.CostContractRecordUpdate(storeId, -1, bill);
                            }
                            #endregion

                            #region 发送通知
                            try
                            {
                                if (bill != null)
                                {
                                    QueuedMessage queuedMessage = new QueuedMessage()
                                    {
                                        StoreId = storeId,
                                        MType = MTypeEnum.TransferCompleted,
                                        Title = CommonHelper.GetEnumDescription<MTypeEnum>(MTypeEnum.TransferCompleted),
                                        Date = bill.CreatedOnUtc,
                                        BillType = BillTypeEnum.SaleBill,
                                        BillNumber = bill.BillNumber,
                                        BillId = bill.Id,
                                        CreatedOnUtc = DateTime.Now
                                    };
                                    _queuedMessageService.InsertQueuedMessage(adminMobileNumbers,queuedMessage);

                                }
                            }
                            catch (Exception ex)
                            {
                                _queuedMessageService.WriteLogs(ex.Message);
                            }
                            #endregion

                        }

                    });
                }

                //退货订单
                if (data.BillType == (int)BillTypeEnum.ReturnReservationBill)
                {

                    #region 验证盘点 这里只验证 转单后的仓库
                    string thisMsg = string.Empty;
                    foreach (var a in idList)
                    {
                        int id = int.Parse(a);
                        ReturnReservationBill returnReservation = _returnReservationBillService.GetReturnReservationBillById(storeId, id);
                        if (returnReservation != null && returnReservation.Items != null && returnReservation.Items.Count > 0)
                        {
                            fg = _wareHouseService.CheckProductInventory(storeId, data.WareHouseId, returnReservation.Items.Select(it => it.ProductId).Distinct().ToArray(), out thisMsg);
                        }
                        if (!string.IsNullOrEmpty(thisMsg))
                        {
                            return new BaseResult { Success = false, Message = thisMsg };
                        }

                    }
                    #endregion


                    idList.ForEach(a =>
                    {
                        int id = int.Parse(a);
                        ReturnReservationBill returnReservation = _returnReservationBillService.GetReturnReservationBillById(storeId, id);
                        if (returnReservation != null && returnReservation.StoreId == storeId && returnReservation.ChangedStatus == false)
                        {

                            //退货订单 转 退货单 公共方法
                            //_returnReservationBillService.ChangeReservation(userId, storeId, id, 0, data.DeliveryUserId, data.TransactionDate ?? DateTime.Now, data.Operation);

                            #region 修改库存
                            var returnBill = _returnBillService.GetReturnBillByReturnReservationBillId(storeId, id);
                            var stockProducts = new List<ProductStockItem>();
                            if (returnBill != null && returnBill.Items != null && returnBill.Items.Count > 0)
                            {
                                var allProducts = _productService.GetProductsByIds(returnBill.StoreId, returnBill.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                                var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(storeId, allProducts.GetProductBigStrokeSmallUnitIds());

                                foreach (ReturnItem item in returnBill.Items)
                                {
                                    var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                                    ProductStockItem productStockItem = stockProducts.Where(b => b.ProductId == item.ProductId).FirstOrDefault();
                                    //商品转化量
                                    var conversionQuantity = product.GetConversionQuantity(allOptions, item.UnitId);
                                    //库存量增量 = 单位转化量 * 数量
                                    int thisQuantity = item.Quantity * conversionQuantity;
                                    if (productStockItem != null)
                                    {
                                        productStockItem.Quantity += thisQuantity;
                                    }
                                    else
                                    {
                                        productStockItem = new ProductStockItem
                                        {
                                            ProductId = item.ProductId,
                                            UnitId = item.UnitId,
                                            SmallUnitId = product.SmallUnitId,
                                            BigUnitId = product.BigUnitId ?? 0,
                                            ProductName = allProducts.Where(s => s.Id == item.ProductId).FirstOrDefault()?.Name,
                                            ProductCode = allProducts.Where(s => s.Id == item.ProductId).FirstOrDefault()?.ProductCode,
                                            Quantity = thisQuantity
                                        };

                                        stockProducts.Add(productStockItem);
                                    }
                                }
                            }


                            _stockService.AdjustStockQty<ReturnBill, ReturnItem>(returnBill, _productService, _specificationAttributeService, DirectionEnum.In, StockQuantityType.CurrentQuantity, returnBill.WareHouseId, stockProducts, StockFlowChangeTypeEnum.Audited);

                            #endregion

                            #region 发送通知
                            try
                            {
                                if (returnBill != null)
                                {
                                    QueuedMessage queuedMessage = new QueuedMessage()
                                    {
                                        StoreId = storeId,
                                        MType = MTypeEnum.TransferCompleted,
                                        Title = CommonHelper.GetEnumDescription<MTypeEnum>(MTypeEnum.TransferCompleted),
                                        Date = returnBill.CreatedOnUtc,
                                        BillType = BillTypeEnum.ReturnBill,
                                        BillNumber = returnBill.BillNumber,
                                        BillId = returnBill.Id,
                                        CreatedOnUtc = DateTime.Now
                                    };
                                    _queuedMessageService.InsertQueuedMessage(adminMobileNumbers,queuedMessage);

                                }
                            }
                            catch (Exception ex)
                            {
                                _queuedMessageService.WriteLogs(ex.Message);
                            }
                            #endregion

                        }

                    });
                }


                //保存事务
                transaction.Commit();

                return new BaseResult { Success = true, Return = 0, Message = Resources.Bill_CreateOrUpdateSuccessful };
            }
            catch (Exception)
            {
                //如果事务不存在或者为控则回滚
                transaction?.Rollback();
                return new BaseResult { Success = false, Message = Resources.Bill_CreateOrUpdateFailed };
            }
            finally
            {
                //不管怎样最后都会关闭掉这个事务
                using (transaction) { }
            }
        }


    }
}
