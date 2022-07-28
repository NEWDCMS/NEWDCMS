using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Data;
using DCMS.Core.Domain.CSMS;
using DCMS.Core.Domain.Sales;
using DCMS.Core.Domain.Tasks;
using DCMS.Core.Domain.WareHouses;
using DCMS.Core.Infrastructure;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Caching;
using DCMS.Services.CSMS;
using DCMS.Services.Events;
using DCMS.Services.Finances;
using DCMS.Services.Products;
using DCMS.Services.Tasks;
using DCMS.Services.Terminals;
using DCMS.Services.Users;
using DCMS.Services.WareHouses;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;


namespace DCMS.Services.Sales
{
    public class DispatchBillService : BaseService, IDispatchBillService
    {
        private readonly IUserService _userService;
        private readonly IQueuedMessageService _queuedMessageService;
        private readonly IStockService _stockService;
        private readonly IProductService _productService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IAllocationBillService _allocationBillService;
        private readonly ISaleReservationBillService _saleReservationBillService;
        private readonly IReturnReservationBillService _returnReservationBillService;
        private readonly ICostContractBillService _costContractBillService;
        private readonly IWareHouseService _wareHouseService;
        private readonly ITerminalService _terminalService;
        private readonly IExchangeBillService _exchangeBillService;
        private readonly ITerminalSignReportService _terminalSignReportService;
        private readonly IOrderDetailService _orderDetailService;

        public DispatchBillService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher,
            IUserService userService,
            IQueuedMessageService queuedMessageService,
            IStockService stockService,
            IProductService productService,
            ISpecificationAttributeService specificationAttributeService,
            IAllocationBillService allocationBillService,
            ISaleReservationBillService saleReservationBillService,
            IReturnReservationBillService returnReservationBillService,
            ICostContractBillService costContractBillService,
            IWareHouseService wareHouseService,
            IExchangeBillService exchangeBillService,
            ITerminalService terminalService
            ,
            ITerminalSignReportService terminalSignReportService,
            IOrderDetailService orderDetailService
            ) : base(getter, cacheManager, eventPublisher)
        {
            _userService = userService;
            _queuedMessageService = queuedMessageService;
            _stockService = stockService;
            _productService = productService;
            _specificationAttributeService = specificationAttributeService;
            _allocationBillService = allocationBillService;
            _saleReservationBillService = saleReservationBillService;
            _returnReservationBillService = returnReservationBillService;
            _costContractBillService = costContractBillService;
            _wareHouseService = wareHouseService;
            _terminalService = terminalService;
            _exchangeBillService = exchangeBillService;
            _terminalSignReportService = terminalSignReportService;
            _orderDetailService = orderDetailService;
        }

        #region 装车调度单

        public bool Exists(int billId)
        {
            return DispatchBillsRepository.TableNoTracking.Where(a => a.Id == billId).Count() > 0;
        }

        public DispatchBill GetDispatchBillById(int? store, int dispatchBillId)
        {
            if (dispatchBillId == 0)
            {
                return null;
            }

            var query = DispatchBillsRepository.Table.Include(s => s.Items)
                            .Where(s => s.StoreId == store && s.Id == dispatchBillId)
                            .ToList();

            return query.FirstOrDefault();
        }

        public void InsertDispatchBill(DispatchBill bill)
        {
            var uow = DispatchBillsRepository.UnitOfWork;
            DispatchBillsRepository.Insert(bill);
            uow.SaveChanges();
            //通知
            _eventPublisher.EntityInserted(bill);
        }

        public void UpdateDispatchBill(DispatchBill bill)
        {
            if (bill == null)
            {
                throw new ArgumentNullException("bill");
            }

            var uow = DispatchBillsRepository.UnitOfWork;
            DispatchBillsRepository.Update(bill);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(bill);
        }

        public void DeleteDispatchBill(DispatchBill bill)
        {
            if (bill == null)
            {
                throw new ArgumentNullException("bill");
            }

            var uow = DispatchBillsRepository.UnitOfWork;
            DispatchBillsRepository.Delete(bill);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(bill);
        }
        #endregion

        #region 装车调度单明细

        public DispatchItem GetDispatchItemsById(int? store, int dispatchItemId)
        {
            if (dispatchItemId == 0)
            {
                return null;
            }
            return DispatchItemsRepository.ToCachedGetById(dispatchItemId);
        }

        public void InsertDispatchItem(DispatchItem dispatchItem)
        {
            var uow = DispatchItemsRepository.UnitOfWork;
            DispatchItemsRepository.Insert(dispatchItem);
            uow.SaveChanges();
            //通知
            _eventPublisher.EntityInserted(dispatchItem);
        }

        public void UpdateDispatchItem(DispatchItem dispatchItem)
        {
            try
            {
                if (dispatchItem == null)
                {
                    throw new ArgumentNullException("dispatchItem");
                }

                var uow = DispatchItemsRepository.UnitOfWork;
                DispatchItemsRepository.Update(dispatchItem);
                uow.SaveChanges();
                //通知
                _eventPublisher.EntityUpdated(dispatchItem);
            }
            catch (Exception)
            {
                //var flag = ex.Message;
            }
        }

        public void DeleteDispatchItem(DispatchItem dispatchItem)
        {
            if (dispatchItem == null)
            {
                throw new ArgumentNullException("dispatchItem");
            }

            var uow = DispatchItemsRepository.UnitOfWork;
            DispatchItemsRepository.Delete(dispatchItem);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(dispatchItem);
        }

        /// <summary>
        /// 获取单据(销售订单，退货订单)车辆Id
        /// </summary>
        /// <param name="billType"></param>
        /// <param name="billId"></param>
        /// <returns></returns>
        public int GetCarId(int billTypeId, int billId)
        {
            if (billTypeId == 0 || billId == 0)
            {
                return 0;
            }

            var query = from a in DispatchBillsRepository.Table
                        join b in DispatchItemsRepository.Table on a.Id equals b.DispatchBillId
                        where a.ReversedStatus == false
                        && b.BillTypeId == billTypeId
                        && b.BillId == billId
                        && (b.SignStatus == (int)SignStatusEnum.Done || b.SignStatus == (int)SignStatusEnum.Rejection)
                        select a.CarId;
            return query.FirstOrDefault();

        }


        public virtual DispatchItem GetDispatchItemByDispatchBillIdBillTypeBillId(int dispatchBillId, int billTypeId, int billId)
        {
            var query = from a in DispatchItemsRepository.Table
                        where a.DispatchBillId == dispatchBillId
                        && a.BillTypeId == billTypeId
                        && a.BillId == billId
                        select a;
            return query.FirstOrDefault();
        }

        /// <summary>
        /// 根据销售单获取项目
        /// </summary>
        /// <param name="saleBillId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual IList<DispatchItem> GetDispatchItemByDispatchBillId(int dispatchBillId, int? userId, int? storeId, int pageIndex, int pageSize)
        {
            if (dispatchBillId == 0)
            {
                return new PagedList<DispatchItem>(new List<DispatchItem>(), pageIndex, pageSize);
            }

            var key = DCMSDefaults.DISPATCHBILL_ITEM_ALLBY_SALEID_KEY.FillCacheKey(storeId, dispatchBillId, pageIndex, pageSize, userId);
            return _cacheManager.Get(key, () =>
            {
                var query = from pc in DispatchItemsRepository.Table
                            where pc.DispatchBillId == dispatchBillId
                            orderby pc.Id
                            select pc;
                //var saleItems = new PagedList<DispatchItem>(query.ToList(), pageIndex, pageSize);
                //return saleItems;
                //总页数
                var totalCount = query.Count();
                var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
                return new PagedList<DispatchItem>(plists, pageIndex, pageSize, totalCount);
            });
        }



        #endregion

        /// <summary>
        /// 获取调拨单
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="makeuserId"></param>
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
        public IPagedList<DispatchBill> GetDispatchBillList(int? storeId, 
            int? makeuserId, 
            DateTime? start = null, 
            DateTime? end = null, 
            int? businessUserId = null,
            int? districtId = null,
            int? terminalId = null,
            string billNumber = "",
            int? deliveryUserId = null,
            int? channelId = null, 
            int? rankId = null, 
            int? billTypeId = null, 
            bool? showDispatchReserved = null,
            bool? dispatchStatus = null, 
            int pageIndex = 0, 
            int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;

            DateTime.TryParse(start?.ToString("yyyy-MM-dd 00:00:00"), out DateTime startDate);
            DateTime.TryParse(end?.ToString("yyyy-MM-dd 23:59:59"), out DateTime endDate);

            var query = from pc in DispatchBillsRepository.Table
                       .Include(cr => cr.Items)
                        select pc;

            if (storeId.HasValue && storeId != 0)
            {
                query = query.Where(a => a.StoreId == storeId);
            }

            if (deliveryUserId.HasValue && deliveryUserId != 0)
            {
                query = query.Where(a => a.DeliveryUserId == deliveryUserId);
            }

            //if (makeuserId.HasValue && makeuserId > 0)
            //{
            //    var userIds = _userService.GetSubordinate(storeId, makeuserId ?? 0);
            //    query = query.Where(x => userIds.Contains(x.MakeUserId));
            //}

            //开始时间
            if (start != null)
            {
                query = query.Where(a => a.CreatedOnUtc >= startDate);
            }

            //结束时间
            if (end != null)
            {
                query = query.Where(a => a.CreatedOnUtc <= endDate);
            }

            //单据号
            if (!string.IsNullOrEmpty(billNumber))
            {
                query = query.Where(a => a.BillNumber.Contains(billNumber));
            }

            //显示红冲调度单
            if (showDispatchReserved == true)
            {
                query = query.Where(a => a.ReversedStatus == true);
            }

            query = query.OrderByDescending(a => a.CreatedOnUtc);


            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<DispatchBill>(plists, pageIndex, pageSize, totalCount);
        }


        /// <summary>
        /// 验证销售订单是否签收
        /// </summary>
        /// <param name="srb"></param>
        /// <returns></returns>
        public bool CheckSign(SaleReservationBill srb)
        {
            var query = from a in DispatchBillsRepository.Table
                        join b in DispatchItemsRepository.Table on a.Id equals b.DispatchBillId
                        where a.ReversedStatus == false
                        && b.BillTypeId == (int)BillTypeEnum.SaleReservationBill
                        && b.BillId == srb.Id
                        && b.SignStatus == (int)SignStatusEnum.Done
                        select b;
            return query.Count() > 0;
        }

        public bool CheckSign(ExchangeBill srb)
        {
            var query = from a in DispatchBillsRepository.Table
                        join b in DispatchItemsRepository.Table on a.Id equals b.DispatchBillId
                        where a.ReversedStatus == false
                        && b.BillTypeId == (int)BillTypeEnum.ExchangeBill
                        && b.BillId == srb.Id
                        && b.SignStatus == (int)SignStatusEnum.Done
                        select b;
            return query.Count() > 0;
        }


        /// <summary>
        /// 验证销售订单对应调拨单是否红冲
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="type"></param>
        /// <param name="billId"></param>
        /// <returns></returns>
        public bool CheckReversed(int? SaleReservationBillId)
        {
            if (!SaleReservationBillId.HasValue)
            {
                return false;
            }
            var query = from a in DispatchBillsRepository.Table
                        join b in DispatchItemsRepository.Table on a.Id equals b.DispatchBillId
                        where a.ReversedStatus == false
                        && b.BillTypeId == (int)BillTypeEnum.SaleReservationBill
                        && b.BillId == SaleReservationBillId
                        select b;
            return !(query.Count() > 0);
        }

        /// <summary>
        /// 单据是否已调拨
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="type"></param>
        /// <param name="billId"></param>
        /// <returns></returns>
        private bool CheckDispatch(int storeId, int type, int billId)
        {
            var query = DispatchItemsRepository.Table.Where(s => s.StoreId == storeId && s.BillTypeId == type && s.BillId == billId).ToList();
            return query.Count() > 0;
        }



        /// <summary>
        /// 创建调度单
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="userId"></param>
        /// <param name="bill"></param>
        /// <param name="deliveryId"></param>
        /// <param name="carId"></param>
        /// <param name="selectDatas"></param>
        /// <param name="autoAllocationBill"></param>
        /// <param name="operation"></param>
        /// <param name="allocationBillNumbers"></param>
        /// <param name="dispatchBillId"></param>
        /// <returns></returns>
        public BaseResult CreateBill(int storeId, int userId, DispatchBill bill, int deliveryId, int carId, List<DispatchItem> selectDatas, int autoAllocationBill, int? operation, out string allocationBillNumbers, out int dispatchBillId)
        {
            var uow = DispatchBillsRepository.UnitOfWork;

            ITransaction transaction = null;

            //调度单Id
            dispatchBillId = 0;
            //调拨单 单据号
            allocationBillNumbers = string.Empty;
            try
            {
                transaction = uow.BeginOrUseTransaction();

                bool isRepet = false;
                selectDatas?.ForEach(la =>
                {
                    if (CheckDispatch(storeId, la.BillTypeId, la.BillId))
                    {
                        isRepet = true;
                        return;
                    }
                });

                if (isRepet)
                {
                    transaction?.Rollback();
                    return new BaseResult { Success = false, Message = "重复提交已调度的单据" };
                }

                string allocationBillNumbers2 = string.Empty;

                bill.StoreId = storeId;
                bill.BillType = BillTypeEnum.DispatchBill;
                bill.GenerateNumber();

                bill.DeliveryUserId = deliveryId;
                int? autocarid = carId;

                //自动生成调拨单
                if (autoAllocationBill == (int)DispatchBillAutoAllocationEnum.Auto)
                {
                    //获取默认车仓
                    if (carId == 0)
                        autocarid = _wareHouseService.GetWareHouseIdsBytype(storeId).FirstOrDefault()?.Id;

                    //为空时
                    bill.CarId = (!autocarid.HasValue || autocarid == 0) ? carId : autocarid ?? 0;
                }
                else
                {
                    bill.CarId = carId;
                }

                if (bill.CarId == 0)
                    return new BaseResult { Success = false, Message = "移动仓库未指定" };

                bill.MakeUserId = userId;
                bill.CreatedOnUtc = DateTime.Now;
                //标识操作源（判断来源是PC/APP端）
                bill.Operation = operation;
                bill.BillStatus = true;
                //添加调度单
                InsertDispatchBill(bill);
                dispatchBillId = bill.Id;

                selectDatas.ForEach(la =>
                {
                    //调度明细
                    var dispatchItem = new DispatchItem
                    {
                        DispatchBillId = bill.Id,
                        StoreId = storeId,
                        BillTypeId = la.BillTypeId,
                        BillId = la.BillId,
                        CreatedOnUtc = DateTime.Now,
                        TerminalId= la.TerminalId,
                        //6位随机验证码
                        VerificationCode = CommonHelper.GenerateNumber(6) 
                    };

                    //添加调度明细
                    InsertDispatchItem(dispatchItem);

                    var terminalMobile = _terminalService.GetTerminalById(storeId, la.TerminalId)?.BossCall;
                    var messageContent = "【经销商管家】验证码：" + dispatchItem.VerificationCode + ",该验证码仅用于送货签收，请勿泄露给任何人.";
                    SendMessage(terminalMobile, messageContent); //发送短信

                    //换货
                    var ecb = new ExchangeBill();
                    //销订
                    var srb = new SaleReservationBill();
                    //退订
                    var rrb = new ReturnReservationBill();

                    //修改换货单，调度状态
                    if (la.BillTypeId == (int)BillTypeEnum.ExchangeBill)
                    {
                        ecb = _exchangeBillService.GetExchangeBillById(storeId, la.BillId, true);
                        if (ecb != null && ecb.DispatchedStatus == false)
                        {
                            ecb.DispatchedUserId = userId;
                            ecb.DispatchedDate = DateTime.Now;
                            ecb.DispatchedStatus = true;
                            //送货员
                            ecb.DeliveryUserId = deliveryId;
                            _exchangeBillService.UpdateExchangeBill(ecb);
                        }
                    }
                    //修改销售订单表，调度状态
                    else if (la.BillTypeId == (int)BillTypeEnum.SaleReservationBill)
                    {
                        srb = _saleReservationBillService.GetSaleReservationBillById(storeId, la.BillId, true);
                        if (srb != null && srb.DispatchedStatus == false)
                        {
                            srb.DispatchedUserId = userId;
                            srb.DispatchedDate = DateTime.Now;
                            srb.DispatchedStatus = true;
                            //送货员
                            srb.DeliveryUserId = deliveryId;
                            _saleReservationBillService.UpdateSaleReservationBill(srb);
                        }
                    }
                    //修改退货订单表，调度状态
                    else if (la.BillTypeId == (int)BillTypeEnum.ReturnReservationBill)
                    {
                        rrb = _returnReservationBillService.GetReturnReservationBillById(storeId, la.BillId, true);
                        if (rrb != null && rrb.DispatchedStatus == false)
                        {
                            rrb.DispatchedUserId = userId;
                            rrb.DispatchedDate = DateTime.Now;
                            rrb.DispatchedStatus = true;
                            //送货员
                            rrb.DeliveryUserId = deliveryId;

                            _returnReservationBillService.UpdateReturnReservationBill(rrb);
                        }
                    }


                    #region //注意：以下是调度出库逻辑

                    //如果选择自动生成调拨单，则创建调拨
                    if (autoAllocationBill == (int)DispatchBillAutoAllocationEnum.Auto)
                    {
                        //调度自动生成调拨
                        var allocationBill = new AllocationBill
                        {
                            StoreId = storeId,
                            DispatchBillId = bill.Id,
                            DispatchBillNumber = bill.BillNumber,
                            Remark = "装车调度自动生成调拨单",
                            MakeUserId = userId,
                            AuditedUserId = userId,
                            //默认审核
                            AuditedStatus = true,
                            AuditedDate = DateTime.Now,
                            CreatedOnUtc = DateTime.Now,
                            ReversedUserId = null,
                            ReversedStatus = false,
                            ReversedDate = null,
                            PrintNum = 0,
                        };
                        //换货单 生成调拨单
                        if (la.BillTypeId == (int)BillTypeEnum.ExchangeBill)
                        {
                            #region 生成调拨单

                            allocationBill.BillType = BillTypeEnum.ExchangeBill;
                            allocationBill.GenerateNumber();
                            //出货仓库
                            allocationBill.ShipmentWareHouseId = ecb.WareHouseId;
                            //入货仓库
                            allocationBill.IncomeWareHouseId = bill.CarId;
                            allocationBill.AllocationByMinUnit = ecb.IsMinUnitSale;
                   
                            _allocationBillService.InsertAllocationBill(allocationBill);

                            //调拨单单号
                            allocationBillNumbers2 = allocationBillNumbers2 + (string.IsNullOrEmpty(allocationBillNumbers2) ? "" : ",") + allocationBill.BillNumber;

                            if (ecb != null && ecb.Items != null && ecb.Items.Count > 0)
                            {
                                ecb.Items.ToList().ForEach(sa =>
                                {
                                    var allocationItem = new AllocationItem
                                    {
                                        AllocationBillId = allocationBill.Id,
                                        ProductId = sa.ProductId,
                                        UnitId = sa.UnitId,
                                        Quantity = sa.Quantity,
                                        Remark = sa.Remark,
                                        CreatedOnUtc = DateTime.Now
                                    };
                                    _allocationBillService.InsertAllocationItem(allocationItem);
                                });
                            }

                            #endregion

                            #region （修改库存）自动审核调拨单

                            try
                            {
                                if (allocationBill.AuditedStatus)
                                {
                                    var stockProductsAB = new List<ProductStockItem>();
                                    var allProductsAB = _productService.GetProductsByIds(storeId, allocationBill.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                                    var allOptionsAB = _specificationAttributeService.GetSpecificationAttributeOptionByIds(storeId, allProductsAB.GetProductBigStrokeSmallUnitIds());
                                    foreach (var item in allocationBill.Items)
                                    {
                                        var product = allProductsAB.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                                        if (product != null)
                                        {
                                            var psi = stockProductsAB.Where(a => a.ProductId == item.ProductId).FirstOrDefault();
                                            //商品转化量
                                            var conversionQuantity = product?.GetConversionQuantity(allOptionsAB, item.UnitId) ?? 0;
                                            //库存量增量 = 单位转化量 * 数量
                                            int thisQuantity = item.Quantity * conversionQuantity;
                                            if (psi != null)
                                            {
                                                psi.Quantity += thisQuantity;
                                            }
                                            else
                                            {
                                                psi = new ProductStockItem
                                                {
                                                    ProductId = item.ProductId,
                                                    UnitId = item.UnitId,
                                                    SmallUnitId = product.SmallUnitId,
                                                    BigUnitId = product.BigUnitId ?? 0,
                                                    ProductName = allProductsAB.Where(s => s.Id == item.ProductId).FirstOrDefault()?.Name,
                                                    ProductCode = allProductsAB.Where(s => s.Id == item.ProductId).FirstOrDefault()?.ProductCode,
                                                    Quantity = thisQuantity
                                                };
                                                stockProductsAB.Add(psi);
                                            }
                                        }
                                    }

                                    if (stockProductsAB != null && stockProductsAB.Count > 0)
                                    {
                                        stockProductsAB.ForEach(psi =>
                                        {
                                            psi.Quantity *= (-1);
                                        });
                                    }

                                    //出货仓库 减少现货
                                    _stockService.AdjustStockQty<AllocationBill, AllocationItem>(allocationBill, _productService, _specificationAttributeService, DirectionEnum.Out, StockQuantityType.CurrentQuantity, allocationBill.ShipmentWareHouseId, stockProductsAB, StockFlowChangeTypeEnum.Audited);


                                    //入货仓库 增加现货
                                    _stockService.AdjustStockQty<AllocationBill, AllocationItem>(allocationBill, _productService, _specificationAttributeService, DirectionEnum.In, StockQuantityType.CurrentQuantity, allocationBill.IncomeWareHouseId, stockProductsAB, StockFlowChangeTypeEnum.Audited);
                                }
                                else
                                {
                                    var stockProducts = new List<ProductStockItem>();
                                    var allProducts = _productService.GetProductsByIds(bill.StoreId, allocationBill.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                                    var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(storeId, allProducts.GetProductBigStrokeSmallUnitIds());

                                    foreach (AllocationItem item in allocationBill.Items)
                                    {
                                        var product = allProducts
                                        .Where(ap => ap.Id == item.ProductId)
                                        .FirstOrDefault();

                                        var productStockItem = stockProducts
                                        .Where(ps => ps.ProductId == item.ProductId)
                                        .FirstOrDefault();

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

                                    var productStockItemThiss = new List<ProductStockItem>();
                                    if (stockProducts != null && stockProducts.Count > 0)
                                    {
                                        stockProducts.ForEach(psi =>
                                        {
                                            var productStockItem = new ProductStockItem
                                            {
                                                ProductId = psi.ProductId,
                                                UnitId = psi.UnitId,
                                                SmallUnitId = psi.SmallUnitId,
                                                BigUnitId = psi.BigUnitId,
                                                ProductName = allProducts
                                                .Where(s => s.Id == psi.ProductId)
                                                .FirstOrDefault()?.Name,

                                                ProductCode = allProducts
                                                .Where(s => s.Id == psi.ProductId)
                                                .FirstOrDefault()?.ProductCode,

                                                Quantity = psi.Quantity * (-1)
                                            };
                                            productStockItemThiss.Add(productStockItem);
                                        });
                                    }

                                    //出货仓库 清零预占 换货单
                                    //验证是否有预占
                                    var checkOrderQuantity1 = _stockService.CheckOrderQuantity(storeId, BillTypeEnum.SaleReservationBill, ecb.BillNumber, ecb.WareHouseId);
                                    {
                                        _stockService.AdjustStockQty<ExchangeBill, ExchangeItem>(ecb, _productService, _specificationAttributeService, DirectionEnum.Out, StockQuantityType.OrderQuantity, ecb.WareHouseId, productStockItemThiss, StockFlowChangeTypeEnum.Audited);
                                    }
                                    //出货仓库 减少现货 换货单
                                    _stockService.AdjustStockQty<ExchangeBill, ExchangeItem>(ecb, _productService, _specificationAttributeService, DirectionEnum.Out, StockQuantityType.CurrentQuantity, carId, productStockItemThiss, StockFlowChangeTypeEnum.Audited);

                                    //入货仓库 增加现货 换货单 车仓
                                    _stockService.AdjustStockQty<ExchangeBill, ExchangeItem>(ecb, _productService, _specificationAttributeService, DirectionEnum.In, StockQuantityType.CurrentQuantity, carId, stockProducts, StockFlowChangeTypeEnum.Audited);

                                    //入货仓库 增加预占 换货单 车仓
                                    _stockService.AdjustStockQty<ExchangeBill, ExchangeItem>(ecb, _productService, _specificationAttributeService, DirectionEnum.In, StockQuantityType.OrderQuantity, carId, stockProducts, StockFlowChangeTypeEnum.Audited);
                                }
                            }
                            catch (Exception)
                            {
                            }

                            #endregion
                        }
                        //销售订单 
                        else if (la.BillTypeId == (int)BillTypeEnum.SaleReservationBill)
                        {
                            /* 可用，现货，预占
                             1. 生成调拨单（审核时） 出货仓库 -> 现货减少，预占释放； 入货仓库  ->  现货增加，预占增加
                             2. 生成调拨单（不审核） 出货仓库 -> 现货不变，预占不变（不释放）； 入货仓库  -> 现货不变，预占不变
                            */

                            #region 生成调拨单

                            allocationBill.BillType = BillTypeEnum.AllocationBill;
                            allocationBill.GenerateNumber();
                            //出仓
                            allocationBill.ShipmentWareHouseId = srb.WareHouseId;
                            //入仓（车仓）
                            allocationBill.IncomeWareHouseId = bill.CarId;
                            allocationBill.AllocationByMinUnit = srb.IsMinUnitSale;
                        
                            //添加调拨单
                            _allocationBillService.InsertAllocationBill(allocationBill);

                            //调拨单单号
                            allocationBillNumbers2 = allocationBillNumbers2 + (string.IsNullOrEmpty(allocationBillNumbers2) ? "" : ",") + allocationBill.BillNumber;
                            if ((srb.Items?.Count ?? 0) == 0)
                            {
                                _allocationBillService.DeleteAllocationBill(allocationBill);
                                return;
                            }
                            //调拨明细
                            if (srb != null && srb.Items != null && srb.Items.Count > 0)
                            {
                                srb.Items.ToList().ForEach(sa =>
                                {
                                    var allocationItem = new AllocationItem
                                    {
                                        AllocationBillId = allocationBill.Id,
                                        ProductId = sa.ProductId,
                                        UnitId = sa.UnitId,
                                        Quantity = sa.Quantity,
                                        Remark = sa.Remark,
                                        CreatedOnUtc = DateTime.Now
                                    };
                                    allocationBill.Items.Add(allocationItem);

                                    _allocationBillService.InsertAllocationItem(allocationItem);
                                });
                            }

                            #endregion

                            #region 

                            try
                            {
                                //生成调拨单（审核时） 出货仓库 -> 现货减少，预占释放； 入货仓库  ->  现货增加，预占增加
                                if (allocationBill.AuditedStatus)
                                {
                                    var stockProductsAB = new List<ProductStockItem>();
                                    var allProductsAB = _productService.GetProductsByIds(storeId, allocationBill.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                                    var allOptionsAB = _specificationAttributeService.GetSpecificationAttributeOptionByIds(storeId, allProductsAB.GetProductBigStrokeSmallUnitIds());
                                    foreach (var item in allocationBill.Items)
                                    {
                                        var product = allProductsAB.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                                        if (product != null)
                                        {
                                            var psi = stockProductsAB.Where(a => a.ProductId == item.ProductId).FirstOrDefault();
                                            //商品转化量
                                            var conversionQuantity = product?.GetConversionQuantity(allOptionsAB, item.UnitId) ?? 0;
                                            //库存量增量 = 单位转化量 * 数量
                                            int thisQuantity = item.Quantity * conversionQuantity;
                                            if (psi != null)
                                            {
                                                psi.Quantity += thisQuantity;
                                            }
                                            else
                                            {
                                                psi = new ProductStockItem
                                                {
                                                    ProductId = item.ProductId,
                                                    UnitId = item.UnitId,
                                                    SmallUnitId = product.SmallUnitId,
                                                    BigUnitId = product.BigUnitId ?? 0,
                                                    ProductName = allProductsAB.Where(s => s.Id == item.ProductId).FirstOrDefault()?.Name,
                                                    ProductCode = allProductsAB.Where(s => s.Id == item.ProductId).FirstOrDefault()?.ProductCode,
                                                    Quantity = thisQuantity
                                                };
                                                stockProductsAB.Add(psi);
                                            }
                                        }
                                    }

                                    if (stockProductsAB.Any())
                                    {
                                        stockProductsAB.ForEach(psi =>
                                        {
                                            psi.Quantity *= (-1);
                                        });
                                    }

                                    //出货仓库 现货减少，预占释放
                                    _stockService.AdjustStockQty<AllocationBill, AllocationItem>(allocationBill, _productService, _specificationAttributeService, DirectionEnum.Out, StockQuantityType.CurrentQuantity, allocationBill.ShipmentWareHouseId, stockProductsAB, StockFlowChangeTypeEnum.Audited);
                                    //预占释放
                                    _stockService.AdjustStockQty<AllocationBill, AllocationItem>(allocationBill, _productService, _specificationAttributeService, DirectionEnum.Out, StockQuantityType.OrderQuantity, allocationBill.ShipmentWareHouseId, stockProductsAB, StockFlowChangeTypeEnum.Audited);

                                    if (stockProductsAB.Any())
                                    {
                                        stockProductsAB.ForEach(psi =>
                                        {
                                            psi.Quantity = Math.Abs(psi.Quantity) * 1;
                                        });
                                    }

                                    //入货仓库 现货增加，预占增加
                                    _stockService.AdjustStockQty<AllocationBill, AllocationItem>(allocationBill, _productService, _specificationAttributeService, DirectionEnum.In, StockQuantityType.CurrentQuantity, allocationBill.IncomeWareHouseId, stockProductsAB, StockFlowChangeTypeEnum.Audited);

                                    //预占增加
                                    _stockService.AdjustStockQty<AllocationBill, AllocationItem>(allocationBill, _productService, _specificationAttributeService, DirectionEnum.In, StockQuantityType.OrderQuantity, allocationBill.IncomeWareHouseId, stockProductsAB, StockFlowChangeTypeEnum.Audited);

                                }
                                //生成调拨单（不审核） 主库存->现货不变，预占不变（不释放）； 车库存->现货不变，预占不变
                                else
                                {
                                    var stockProducts = new List<ProductStockItem>();

                                    var allProducts = _productService.GetProductsByIds(bill.StoreId, allocationBill.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                                    var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(storeId, allProducts.GetProductBigStrokeSmallUnitIds());

                                    foreach (var item in allocationBill.Items)
                                    {
                                        var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                                        var productStockItem = stockProducts.Where(ps => ps.ProductId == item.ProductId).FirstOrDefault();
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

                                    //要调拨的商品库存信息
                                    var productStockItemThiss = new List<ProductStockItem>();
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

                                    ////出货仓库 清零预占 销售订单
                                    ////验证是否有预占
                                    //var checkOrderQuantity1 = _stockService.CheckOrderQuantity(storeId, BillTypeEnum.SaleReservationBill, srb.BillNumber, srb.WareHouseId);
                                    //if (checkOrderQuantity1)
                                    //{
                                    //    _stockService.AdjustStockQty<SaleReservationBill, SaleReservationItem>(srb, _productService, _specificationAttributeService, DirectionEnum.Out, StockQuantityType.OrderQuantity, srb.WareHouseId, productStockItemThiss, StockFlowChangeTypeEnum.Audited);
                                    //}

                                    ////出货仓库 减少现货 销售订单
                                    //_stockService.AdjustStockQty<SaleReservationBill, SaleReservationItem>(srb, _productService, _specificationAttributeService, DirectionEnum.Out, StockQuantityType.CurrentQuantity, carId, productStockItemThiss, StockFlowChangeTypeEnum.Audited);

                                    ////入货仓库 增加现货 销售订单 车仓
                                    //_stockService.AdjustStockQty<SaleReservationBill, SaleReservationItem>(srb, _productService, _specificationAttributeService, DirectionEnum.In, StockQuantityType.CurrentQuantity, carId, stockProducts, StockFlowChangeTypeEnum.Audited);

                                    ////入货仓库 增加预占 销售订单 车仓
                                    //_stockService.AdjustStockQty<SaleReservationBill, SaleReservationItem>(srb, _productService, _specificationAttributeService, DirectionEnum.In, StockQuantityType.OrderQuantity, carId, stockProducts, StockFlowChangeTypeEnum.Audited);
                                }
                            }
                            catch (Exception)
                            {

                            }

                            #endregion
                        }
                        //退货订单 生成调拨单
                        else if (la.BillTypeId == (int)BillTypeEnum.ReturnReservationBill)
                        {
                            allocationBill.BillType = BillTypeEnum.AllocationBill;
                            allocationBill.GenerateNumber();
                            allocationBill.ShipmentWareHouseId = rrb.WareHouseId;
                            allocationBill.IncomeWareHouseId = bill.CarId;
                            allocationBill.AllocationByMinUnit = rrb.IsMinUnitSale;
                            allocationBill.Remark = rrb.Remark;
                            allocationBill.AuditedStatus = true;
                            allocationBill.AuditedDate = DateTime.Now;
                            allocationBill.ReversedUserId = null;
                            allocationBill.ReversedStatus = false;
                            allocationBill.ReversedDate = null;
                            allocationBill.PrintNum = 0;
                            _allocationBillService.InsertAllocationBill(allocationBill);

                            //调拨单单号
                            allocationBillNumbers2 = allocationBillNumbers2 + (string.IsNullOrEmpty(allocationBillNumbers2) ? "" : ",") + allocationBill.BillNumber;
                            if (rrb != null && rrb.Items != null && rrb.Items.Count > 0)
                            {
                                rrb.Items.ToList().ForEach(sa =>
                                {
                                    AllocationItem allocationItem = new AllocationItem
                                    {
                                        AllocationBillId = allocationBill.Id,
                                        ProductId = sa.ProductId,
                                        UnitId = sa.UnitId,
                                        Quantity = sa.Quantity,
                                        //allocationItem.TradePrice=s
                                        //allocationItem.WholesaleAmount=sa.who
                                        //allocationItem.OutgoingStock
                                        //allocationItem.WarehousingStock
                                        Remark = sa.Remark,
                                        CreatedOnUtc = DateTime.Now
                                    };

                                    _allocationBillService.InsertAllocationItem(allocationItem);
                                });
                            }
                        }
                    }
                    //这里如果不生成调拨单，但是库存要自动加减
                    else
                    {
                        //换货单
                        if (la.BillTypeId == (int)BillTypeEnum.ExchangeBill)
                        {
                            #region 修改库存
                            /*
                            var stockProducts = new List<ProductStockItem>();
                            var allProducts = _productService.GetProductsByIds(storeId, ecb.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                            var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(storeId, allProducts.GetProductBigStrokeSmallUnitIds());

                            foreach (var item in ecb.Items)
                            {
                                var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                                var productStockItem = stockProducts.Where(ps => ps.ProductId == item.ProductId).FirstOrDefault();
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
                            var productStockItemThiss = new List<ProductStockItem>();
                            if (stockProducts != null && stockProducts.Count > 0)
                            {
                                stockProducts.ForEach(psi =>
                                {
                                    var productStockItem = new ProductStockItem
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

                            //出货仓库 清零预占 换货单
                            //验证是否有预占
                            var checkOrderQuantity1 = _stockService.CheckOrderQuantity(storeId, BillTypeEnum.SaleReservationBill, ecb.BillNumber, ecb.WareHouseId);
                            if (checkOrderQuantity1)
                            {
                                _stockService.AdjustStockQty<ExchangeBill, ExchangeItem>(ecb, _productService, _specificationAttributeService, DirectionEnum.Out, StockQuantityType.OrderQuantity, ecb.WareHouseId, productStockItemThiss, StockFlowChangeTypeEnum.Audited);
                            }
                            //出货仓库 减少现货 换货单
                            _stockService.AdjustStockQty<ExchangeBill, ExchangeItem>(ecb, _productService, _specificationAttributeService, DirectionEnum.Out, StockQuantityType.CurrentQuantity, ecb.WareHouseId, productStockItemThiss, StockFlowChangeTypeEnum.Audited);

                            //入货仓库 增加现货 换货单 车仓
                            _stockService.AdjustStockQty<ExchangeBill, ExchangeItem>(ecb, _productService, _specificationAttributeService, DirectionEnum.In, StockQuantityType.CurrentQuantity, carId, stockProducts, StockFlowChangeTypeEnum.Audited);

                            //入货仓库 增加预占 换货单 车仓
                            _stockService.AdjustStockQty<ExchangeBill, ExchangeItem>(ecb, _productService, _specificationAttributeService, DirectionEnum.In, StockQuantityType.OrderQuantity, carId, stockProducts, StockFlowChangeTypeEnum.Audited);
                            */
                            #endregion
                        }
                        //销售订单
                        else if (la.BillTypeId == (int)BillTypeEnum.SaleReservationBill)
                        {
                            #region 修改库存

                            /*
                            var stockProducts = new List<ProductStockItem>();

                            var allProducts = _productService.GetProductsByIds(storeId, srb.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                            var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(storeId, allProducts.GetProductBigStrokeSmallUnitIds());

                            foreach (var item in srb.Items)
                            {
                                var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                                var productStockItem = stockProducts.Where(ps => ps.ProductId == item.ProductId).FirstOrDefault();
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
                            var productStockItemThiss = new List<ProductStockItem>();
                            if (stockProducts != null && stockProducts.Count > 0)
                            {
                                stockProducts.ForEach(psi =>
                                {
                                    var productStockItem = new ProductStockItem
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

                            //出货仓库 清零预占 销售订单
                            //验证是否有预占
                            var checkOrderQuantity1 = _stockService.CheckOrderQuantity(storeId, BillTypeEnum.SaleReservationBill, srb.BillNumber, srb.WareHouseId);
                            if (checkOrderQuantity1)
                            {
                                _stockService.AdjustStockQty<SaleReservationBill, SaleReservationItem>(srb, _productService, _specificationAttributeService, DirectionEnum.Out, StockQuantityType.OrderQuantity, srb.WareHouseId, productStockItemThiss, StockFlowChangeTypeEnum.Audited);
                            }
                            //出货仓库 减少现货 销售订单
                            _stockService.AdjustStockQty<SaleReservationBill, SaleReservationItem>(srb, _productService, _specificationAttributeService, DirectionEnum.Out, StockQuantityType.CurrentQuantity, srb.WareHouseId, productStockItemThiss, StockFlowChangeTypeEnum.Audited);

                            //入货仓库 增加现货 销售订单 车仓
                            _stockService.AdjustStockQty<SaleReservationBill, SaleReservationItem>(srb, _productService, _specificationAttributeService, DirectionEnum.In, StockQuantityType.CurrentQuantity, carId, stockProducts, StockFlowChangeTypeEnum.Audited);
                            //入货仓库 增加预占 销售订单 车仓
                            _stockService.AdjustStockQty<SaleReservationBill, SaleReservationItem>(srb, _productService, _specificationAttributeService, DirectionEnum.In, StockQuantityType.OrderQuantity, carId, stockProducts, StockFlowChangeTypeEnum.Audited);
                            */
                            #endregion
                        }
                    }

                    #endregion

                });

                #region 发送调度完成通知
                try
                {
                    var adminMobileNumbers = _userService.GetAllUserMobileNumbersByUserIds(new List<int> { deliveryId }).ToList();
                    //调度完成
                    var queuedMessage = new QueuedMessage()
                    {
                        StoreId = storeId,
                        MType = MTypeEnum.Scheduled,
                        Title = CommonHelper.GetEnumDescription<MTypeEnum>(MTypeEnum.Scheduled),
                        Date = bill.CreatedOnUtc,
                        BillType = BillTypeEnum.DispatchBill,
                        BillNumber = bill.BillNumber,
                        BillId = bill.Id,
                        CreatedOnUtc = DateTime.Now
                    };
                    _queuedMessageService.InsertQueuedMessage(adminMobileNumbers,queuedMessage);
                }
                catch (Exception ex)
                {
                    _queuedMessageService.WriteLogs(ex.Message);
                }
                #endregion


                //保存事务
                transaction.Commit();

                allocationBillNumbers = allocationBillNumbers2;

                return new BaseResult { Success = true, Message = Resources.Bill_CreateOrUpdateSuccessful };
            }
            catch (Exception ex)
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

        public BaseResult UpdateBill(int storeId, int userId, DispatchBill bill, int? id, int deliveryId, int carId)
        {
            var uow = DispatchBillsRepository.UnitOfWork;

            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();



                bill.DeliveryUserId = deliveryId;
                bill.CarId = carId;
                UpdateDispatchBill(bill);

                //修改订单送货员
                if (bill != null && bill.Items != null && bill.Items.Count > 0)
                {
                    bill.Items.ToList().ForEach(ds =>
                    {
                        if (ds.BillTypeId == (int)BillTypeEnum.SaleReservationBill)
                        {
                            SaleReservationBill srb = _saleReservationBillService.GetSaleReservationBillById(storeId, ds.BillId);
                            //已审核、未红冲
                            if (srb != null && srb.AuditedStatus == true && srb.ReversedStatus == false)
                            {
                                srb.DeliveryUserId = deliveryId;
                                _saleReservationBillService.UpdateSaleReservationBill(srb);
                            }
                        }
                        else if (ds.BillTypeId == (int)BillTypeEnum.PurchaseReturnBill)
                        {
                            ReturnReservationBill rrb = _returnReservationBillService.GetReturnReservationBillById(storeId, ds.BillId);
                            //已审核、未红冲
                            if (rrb != null && rrb.AuditedStatus == true && rrb.ReversedStatus == false)
                            {
                                rrb.DeliveryUserId = deliveryId;
                                _returnReservationBillService.UpdateReturnReservationBill(rrb);
                            }
                        }

                    });
                }


                //保存事务
                transaction.Commit();

                return new BaseResult { Success = true, Message = Resources.Bill_CreateOrUpdateSuccessful };
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

        public BaseResult Reverse(int userId, DispatchBill bill)
        {
            var uow = DispatchBillsRepository.UnitOfWork;

            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();



                #region 修改单据表状态
                bill.ReversedUserId = userId;
                bill.ReversedDate = DateTime.Now;
                bill.ReversedStatus = true;
                UpdateDispatchBill(bill);
                #endregion

                if (bill != null && bill.Items != null && bill.Items.Count > 0)
                {
                    int carId = bill.CarId;
                    bill.Items.ToList().ForEach(d =>
                    {
                        if (d.BillTypeId == (int)BillTypeEnum.SaleReservationBill)
                        {
                            SaleReservationBill srb = _saleReservationBillService.GetSaleReservationBillById(0, d.BillId);
                            //
                            if (srb != null)
                            {
                                srb.DispatchedDate = null;
                                srb.DispatchedStatus = false;
                                srb.DispatchedUserId = null;
                                _saleReservationBillService.UpdateSaleReservationBill(srb);


                                //预占库存从车仓到主仓
                                #region 修改库存
                                var stockProducts = new List<ProductStockItem>();

                                var allProducts = _productService.GetProductsByIds(bill.StoreId, srb.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                                var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(bill.StoreId, allProducts.GetProductBigStrokeSmallUnitIds());

                                foreach (SaleReservationItem item in srb.Items)
                                {
                                    var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                                    ProductStockItem productStockItem = stockProducts.Where(ps => ps.ProductId == item.ProductId).FirstOrDefault();
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

                                //出货仓库(车仓) 清零预占 销售订单
                                //验证是否有预占
                                var checkOrderQuantity1 = _stockService.CheckOrderQuantity(bill.StoreId, BillTypeEnum.SaleReservationBill, srb.BillNumber, srb.WareHouseId);
                                if (checkOrderQuantity1)
                                {
                                    _stockService.AdjustStockQty<SaleReservationBill, SaleReservationItem>(srb, _productService, _specificationAttributeService, DirectionEnum.Out, StockQuantityType.OrderQuantity, carId, productStockItemThiss, StockFlowChangeTypeEnum.Audited);
                                }
                                //出货仓库(车仓) 减少现货 销售订单

                                _stockService.AdjustStockQty<SaleReservationBill, SaleReservationItem>(srb, _productService, _specificationAttributeService, DirectionEnum.Out, StockQuantityType.CurrentQuantity, carId, productStockItemThiss, StockFlowChangeTypeEnum.Audited);

                                //入货仓库(主仓) 增加现货 销售订单 车仓
                                _stockService.AdjustStockQty<SaleReservationBill, SaleReservationItem>(srb, _productService, _specificationAttributeService, DirectionEnum.In, StockQuantityType.CurrentQuantity, srb.WareHouseId, stockProducts, StockFlowChangeTypeEnum.Audited);

                                //入货仓库(主仓) 增加预占 销售订单 车仓
                                _stockService.AdjustStockQty<SaleReservationBill, SaleReservationItem>(srb, _productService, _specificationAttributeService, DirectionEnum.In, StockQuantityType.OrderQuantity, srb.WareHouseId, stockProducts, StockFlowChangeTypeEnum.Audited);

                                #endregion

                            }

                        }
                        else if (d.BillTypeId == (int)BillTypeEnum.ReturnReservationBill)
                        {
                            ReturnReservationBill rrb = _returnReservationBillService.GetReturnReservationBillById(0, d.BillId);
                            //
                            if (rrb != null)
                            {
                                rrb.DispatchedDate = null;
                                rrb.DispatchedStatus = false;
                                rrb.DispatchedUserId = null;
                                _returnReservationBillService.UpdateReturnReservationBill(rrb);
                            }
                        }
                    });
                }


                //保存事务
                transaction.Commit();

                return new BaseResult { Success = true, Message = "单据红冲成功" };
            }
            catch (Exception)
            {
                //如果事务不存在或者为控则回滚
                transaction?.Rollback();
                return new BaseResult { Success = false, Message = "单据红冲失败" };
            }
            finally
            {
                //不管怎样最后都会关闭掉这个事务
                using (transaction) { }
            }
        }

        /// <summary>
        /// 打印
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="userId"></param>
        /// <param name="bill"></param>
        /// <returns></returns>
        public BaseResult BillPrint(int storeId, int userId, DispatchBill bill, List<string> printTypeList)
        {
            var uow = DispatchBillsRepository.UnitOfWork;

            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();

                #region 修改单据表打印数
                foreach (var d in bill.Items)
                {
                    //换货单
                    if (d.BillTypeId == (int)BillTypeEnum.ExchangeBill)
                    {
                        var ecb = _exchangeBillService.GetExchangeBillById(storeId, d.BillId);
                        if (ecb != null)
                        {
                            //打印整箱拆零合并单
                            foreach (string t in printTypeList)
                            {
                                //打印订单
                                if (t == "4")
                                {
                                    ecb.PrintNum += 1;
                                }
                            }
                        }

                        _exchangeBillService.UpdateExchangeBill(ecb);

                    }
                    //销售订单
                    else if (d.BillTypeId == (int)BillTypeEnum.SaleReservationBill)
                    {
                        var srb = _saleReservationBillService.GetSaleReservationBillById(storeId, d.BillId);
                        if (srb != null)
                        {
                            //打印整箱拆零合并单
                            foreach (string t in printTypeList)
                            {
                                //打印整箱拆零合并单
                                if (t == "1")
                                {
                                    srb.PickingWholeScrapStatus = true;
                                    srb.PickingWholeScrapPrintNum = (srb.PickingWholeScrapPrintNum ?? 0) + 1;
                                    srb.PickingWholeScrapPrintDate = DateTime.Now;
                                }
                                //打印整箱装车单
                                else if (t == "2")
                                {
                                    srb.PickingWholeStatus = true;
                                    srb.PickingWholePrintNum = (srb.PickingWholePrintNum ?? 0) + 1;
                                    srb.PickingWholePrintDate = DateTime.Now;
                                }
                                //为每个客户打印拆零装车单
                                else if (t == "3")
                                {
                                    srb.PickingScrapStatus = true;
                                    srb.PickingScrapPrintNum = (srb.PickingScrapPrintNum ?? 0) + 1;
                                    srb.PickingScrapPrintDate = DateTime.Now;
                                }
                                //打印订单
                                else if (t == "4")
                                {
                                    srb.PrintNum += 1;
                                }
                            }
                        }

                        _saleReservationBillService.UpdateSaleReservationBill(srb);

                    }
                    else
                    //退货订单
                    if (d.BillTypeId == (int)BillTypeEnum.ReturnReservationBill)
                    {
                        var rrb = _returnReservationBillService.GetReturnReservationBillById(storeId, d.BillId);
                        if (rrb != null)
                        {
                            //打印整箱拆零合并单
                            foreach (string t in printTypeList)
                            {
                                //打印整箱拆零合并单
                                if (t == "1")
                                {
                                    rrb.PickingWholeScrapStatus = true;
                                    rrb.PickingWholeScrapPrintNum = (rrb.PickingWholeScrapPrintNum ?? 0) + 1;
                                    rrb.PickingWholeScrapPrintDate = DateTime.Now;
                                }
                                //打印整箱装车单
                                else if (t == "2")
                                {
                                    rrb.PickingWholeStatus = true;
                                    rrb.PickingWholePrintNum = (rrb.PickingWholePrintNum ?? 0) + 1;
                                    rrb.PickingWholePrintDate = DateTime.Now;
                                }
                                //为每个客户打印拆零装车单
                                else if (t == "3")
                                {
                                    rrb.PickingScrapStatus = true;
                                    rrb.PickingScrapPrintNum = (rrb.PickingScrapPrintNum ?? 0) + 1;
                                    rrb.PickingScrapPrintDate = DateTime.Now;
                                }
                                //打印订单
                                else if (t == "4")
                                {
                                    rrb.PrintNum += 1;
                                }
                            }
                        }
                        _returnReservationBillService.UpdateReturnReservationBill(rrb);
                    }

                }
                #endregion


                //保存事务
                transaction.Commit();

                return new BaseResult { Success = true, Message = "单据红冲成功" };
            }
            catch (Exception)
            {
                //如果事务不存在或者为控则回滚
                transaction?.Rollback();
                return new BaseResult { Success = false, Message = "单据红冲失败" };
            }
            finally
            {
                //不管怎样最后都会关闭掉这个事务
                using (transaction) { }
            }
        }


        #region 送货签收

        /// <summary>
        /// 获取已经签收单据
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="userId"></param>
        /// <param name="terminalId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public IPagedList<DeliverySign> GetSignedBills(int? storeId, DateTime? start = null, DateTime? end = null, int? userId = null,int? terminalId = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;

            DateTime.TryParse(start?.ToString("yyyy-MM-dd 00:00:00"), out DateTime startDate);
            DateTime.TryParse(end?.ToString("yyyy-MM-dd 23:59:59"), out DateTime endDate);

            var query = from pc in DeliverySignsRepository.Table
                        .Include(sb => sb.RetainPhotos)
                        select pc;

            if (storeId.HasValue && storeId != 0)
            {
                query = query.Where(a => a.StoreId == storeId);
            }

            //if (businessUserId.HasValue && businessUserId > 0)
            //{
            //   var userIds = _userService.GetSubordinate(storeId, businessUserId ?? 0);
            //   query = query.Where(x => userIds.Contains(x.BusinessUserId));
            //}

            //签收人
            if (userId.HasValue && userId != 0)
            {
                query = query.Where(a => a.SignUserId == userId);
            }

            if (terminalId.HasValue && terminalId != 0)
            {
                query = query.Where(a => a.TerminalId == terminalId);
            }

            //开始时间
            if (start != null)
            {
                query = query.Where(a => a.SignedDate >= startDate);
            }

            //结束时间
            if (end != null)
            {
                query = query.Where(a => a.SignedDate <= endDate);
            }

            query = query.OrderByDescending(a => a.SignedDate);

            return new PagedList<DeliverySign>(query, pageIndex, pageSize);
        }

        public void InsertDeliverySign(DeliverySign deliverySign)
        {
            var uow = DeliverySignsRepository.UnitOfWork;
            DeliverySignsRepository.Insert(deliverySign);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(deliverySign);
        }
        public DeliverySign GetDeliverySignById(int? store, int deliverySignId)
        {
            if (deliverySignId == 0)
            {
                return null;
            }

            return DeliverySignsRepository.ToCachedGetById(deliverySignId);
        }
        public void UpdateDeliverySign(DeliverySign deliverySign)
        {
            if (deliverySign == null)
            {
                throw new ArgumentNullException("deliverySign");
            }

            var uow = DeliverySignsRepository.UnitOfWork;
            DeliverySignsRepository.Update(deliverySign);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(deliverySign);
        }
        public void DeleteDeliverySign(DeliverySign deliverySign)
        {
            if (deliverySign == null)
            {
                throw new ArgumentNullException("deliverySign");
            }

            var uow = DeliverySignsRepository.UnitOfWork;
            DeliverySignsRepository.Delete(deliverySign);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(deliverySign);
        }
        public void InsertRetainPhoto(RetainPhoto retainPhoto)
        {
            var uow = RetainPhotosRepository.UnitOfWork;
            RetainPhotosRepository.Insert(retainPhoto);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(retainPhoto);
        }
        public RetainPhoto GetRetainPhotoById(int? store, int retainPhotoId)
        {
            if (retainPhotoId == 0)
            {
                return null;
            }

            return RetainPhotosRepository.ToCachedGetById(retainPhotoId);
        }
        public void UpdateRetainPhoto(RetainPhoto retainPhoto)
        {
            if (retainPhoto == null)
            {
                throw new ArgumentNullException("retainPhoto");
            }

            var uow = RetainPhotosRepository.UnitOfWork;
            RetainPhotosRepository.Update(retainPhoto);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(retainPhoto);
        }
        public void DeleteRetainPhoto(RetainPhoto retainPhoto)
        {
            if (retainPhoto == null)
            {
                throw new ArgumentNullException("retainPhoto");
            }

            var uow = RetainPhotosRepository.UnitOfWork;
            RetainPhotosRepository.Delete(retainPhoto);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(retainPhoto);
        }

        /// <summary>
        /// 单据拒签逻辑
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="userId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public BaseResult RefusedConfirm(int storeId, int userId, DeliverySignUpdate data, DispatchBill bill, DispatchItem dispatchItem)
        {
            var uow = DispatchBillsRepository.UnitOfWork;

            ITransaction transaction = null;

            try
            {
                transaction = uow.BeginOrUseTransaction();

                if (data != null)
                {
                    //拒签（换货单，退货订单，销售订单）
                    if ((data.BillTypeId == (int)BillTypeEnum.ExchangeBill || data.BillTypeId == (int)BillTypeEnum.SaleReservationBill || data.BillTypeId == (int)BillTypeEnum.ReturnReservationBill) && dispatchItem != null)
                    {
                        #region 更新装车调度单据状态

                        //更新调度项目的签收状态
                        dispatchItem.SignStatus = 2;
                        dispatchItem.SignUserId = userId;
                        dispatchItem.SignDate = DateTime.Now;
                        UpdateDispatchItem(dispatchItem);

                        #endregion

                        //换货单 释放预占
                        if (data.BillTypeId == (int)BillTypeEnum.ExchangeBill)
                        {
                            #region 预占释放

                            var ecb = _exchangeBillService.GetExchangeBillById(storeId, dispatchItem.BillId);
                            var stockProducts = new List<ProductStockItem>();
                            var allProducts = _productService.GetProductsByIds(storeId, ecb.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                            var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(storeId, allProducts.GetProductBigStrokeSmallUnitIds());
                            foreach (var item in ecb.Items)
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

                            var productStockItemThiss = new List<ProductStockItem>();
                            if (stockProducts != null && stockProducts.Count > 0)
                            {
                                stockProducts.ForEach(psi =>
                                {
                                    var productStockItem = new ProductStockItem
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

                            var productStockItemThiss2 = new List<ProductStockItem>();
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
                                    productStockItemThiss2.Add(productStockItem);
                                });
                            }

                            int carId = bill.CarId;

                            //换货单释放预占
                            //验证是否有预占
                            var checkOrderQuantity = _stockService.CheckOrderQuantity(storeId, BillTypeEnum.ExchangeBill, ecb.BillNumber, ecb.WareHouseId);
                            if (checkOrderQuantity)
                            {
                                _stockService.AdjustStockQty<ExchangeBill, ExchangeItem>(ecb, _productService, _specificationAttributeService, DirectionEnum.In, StockQuantityType.OrderQuantity, ecb.WareHouseId, productStockItemThiss, StockFlowChangeTypeEnum.Save);
                            }

                            //出货仓库(车仓) 清零预占 换货单
                            _stockService.AdjustStockQty<ExchangeBill, ExchangeItem>(ecb, _productService, _specificationAttributeService, DirectionEnum.Out, StockQuantityType.OrderQuantity, carId, productStockItemThiss, StockFlowChangeTypeEnum.Audited);
                            //出货仓库(车仓) 增加现货 换货单
                            _stockService.AdjustStockQty<ExchangeBill, ExchangeItem>(ecb, _productService, _specificationAttributeService, DirectionEnum.In, StockQuantityType.CurrentQuantity, carId, productStockItemThiss2, StockFlowChangeTypeEnum.Audited);

                            #endregion
                        }
                        //销售订单 释放预占、释放赠品
                        else if (data.BillTypeId == (int)BillTypeEnum.SaleReservationBill)
                        {
                            #region 预占释放
                            var srb = _saleReservationBillService.GetSaleReservationBillById(storeId, dispatchItem.BillId);

                            //如果此销售订单没有转单，
                            //将一个单据中 相同商品 数量 按最小单位汇总
                            var stockProducts = new List<ProductStockItem>();
                            var allProducts = _productService.GetProductsByIds(storeId, srb.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                            var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(storeId, allProducts.GetProductBigStrokeSmallUnitIds());
                            foreach (SaleReservationItem item in srb.Items)
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

                            List<ProductStockItem> productStockItemThiss2 = new List<ProductStockItem>();
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
                                    productStockItemThiss2.Add(productStockItem);
                                });
                            }

                            int carId = bill.CarId;

                            //销售订单释放预占
                            //验证是否有预占
                            var checkOrderQuantity = _stockService.CheckOrderQuantity(storeId, BillTypeEnum.SaleReservationBill, srb.BillNumber, srb.WareHouseId);
                            if (checkOrderQuantity)
                            {
                                _stockService.AdjustStockQty<SaleReservationBill, SaleReservationItem>(srb, _productService, _specificationAttributeService, DirectionEnum.In, StockQuantityType.OrderQuantity, srb.WareHouseId, productStockItemThiss, StockFlowChangeTypeEnum.Save);

                            }
                            //出货仓库(车仓) 清零预占 销售订单
                            _stockService.AdjustStockQty<SaleReservationBill, SaleReservationItem>(srb, _productService, _specificationAttributeService, DirectionEnum.Out, StockQuantityType.OrderQuantity, carId, productStockItemThiss, StockFlowChangeTypeEnum.Audited);
                            //出货仓库(车仓) 增加现货 销售订单
                            _stockService.AdjustStockQty<SaleReservationBill, SaleReservationItem>(srb, _productService, _specificationAttributeService, DirectionEnum.In, StockQuantityType.CurrentQuantity, carId, productStockItemThiss2, StockFlowChangeTypeEnum.Audited);

                            #endregion

                            #region 赠品信息修改（加）、删除赠送记录
                            if (srb != null && srb.Items.Count > 0)
                            {
                                _costContractBillService.CostContractRecordUpdate(1, srb);
                            }
                            #endregion
                        }


                        //记录签收
                        var sign = new DeliverySign
                        {
                            StoreId = storeId,
                            BillTypeId = data.BillTypeId,
                            BillId = data.BillId,
                            BillNumber = bill.BillNumber,
                            ToBillId = 0,
                            ToBillNumber = "",
                            TerminalId = dispatchItem.TerminalId,
                            BusinessUserId = 0,
                            DeliveryUserId = 0,
                            Latitude = data.Latitude,
                            Longitude = data.Longitude,
                            SignUserId = 0,
                            SignedDate = DateTime.Now,
                            SumAmount = 0,
                            Signature = data.Signature,
                            SignStatus = 2
                        };
                        InsertDeliverySign(sign);
                    }
                }

                //保存事务
                transaction.Commit();

                return new BaseResult { Success = true, Message = "拒签成功" };
            }
            catch (Exception)
            {
                transaction?.Rollback();
                return new BaseResult { Success = false, Message = "拒签失败" };
            }
            finally
            {
                using (transaction) { }
            }
        }

        /// <summary>
        /// 单据签收
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="userId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public BaseResult SignConfirm(int storeId, int userId, DeliverySign data, TerminalSignReport terminalSignReport, List<OrderDetail> orderDetails)
        {
            var uow = DispatchBillsRepository.UnitOfWork;
            ITransaction transaction = null;
            try
            {
                transaction = uow.BeginOrUseTransaction();
                if (data != null)
                {
                    if (data.BillTypeId == (int)BillTypeEnum.SaleBill || data.BillTypeId == (int)BillTypeEnum.CostExpenditureBill)
                    {
                        //记录签收
                        data.SignStatus = 1;
                        InsertDeliverySign(data);
                    }
                    //写入CS签收
                    if (data.BillTypeId == (int)BillTypeEnum.SaleBill && terminalSignReport != null)
                    {
                        TerminalSignReport tsr = _terminalSignReportService.InsertTerminalSignReport(terminalSignReport);
                        if (orderDetails != null)
                        {
                            foreach (OrderDetail orderDetail in orderDetails)
                            {
                                orderDetail.terminalSignReportId = tsr.Id;
                                _orderDetailService.InsertOrderDetail(orderDetail);
                            }
                        }

                    }
                }
                //保存事务
                transaction.Commit();
                return new BaseResult { Success = true, Message = "签收成功" };
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                return new BaseResult { Success = false, Message = "签收失败" };
            }
            finally
            {
                using (transaction) { }
            }
        }



        #endregion

        #region 发送短信

        public bool SendMessage(string mobile, string content)
        {
            try
            {
                string url = EngineContext.GetSendMessageUrl;
                string uid = EngineContext.GetSendMessageUId;
                string password = EngineContext.GetSendMessagePassWord;
                return CommonHelper.SendSMS(url, uid, password, content, mobile);
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion
    }
}
