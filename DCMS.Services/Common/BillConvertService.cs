using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using DCMS.Services.Finances;
using DCMS.Services.Purchases;
using DCMS.Services.Sales;
using DCMS.Services.WareHouses;
using System;
using System.Linq;

namespace DCMS.Services.Common
{
    public class BillConvertService : BaseService, IBillConvertService
    {


        private readonly IPaymentReceiptBillService _paymentReceiptBillService;
        private readonly ISaleReservationBillService _saleReservationService;
        private readonly ISaleBillService _saleService;
        private readonly IReturnReservationBillService _returnReservationService;
        private readonly IReturnBillService _returnService;
        private readonly IPurchaseBillService _purchaseService;
        private readonly IPurchaseReturnBillService _purchaseReturnService;
        private readonly ICashReceiptBillService _cashReceiptBillService;

        private readonly ICostExpenditureBillService  _costExpenditureBillService;
        private readonly IAdvanceReceiptBillService  _advanceReceiptBillService;

        private readonly IAllocationBillService _allocationBillService;
        private readonly ICombinationProductBillService _combinationProductBillService;
        private readonly ICostAdjustmentBillService _costAdjustmentBillService;
        private readonly IInventoryAllTaskBillService _inventoryAllTaskBillService;
        private readonly IInventoryPartTaskBillService _inventoryPartTaskBillService;
        private readonly IInventoryProfitLossBillService _inventoryProfitLossBillService;
        private readonly IScrapProductBillService _scrapProductBillService;
        private readonly ISplitProductBillService _splitProductBillService;

        public BillConvertService(IStaticCacheManager cacheManager,
            IPaymentReceiptBillService paymentReceiptBillService,
            IRecordingVoucherService recordingVoucherService,
            ISaleReservationBillService saleReservationService,
            ISaleBillService saleService,
            IReturnReservationBillService returnReservationService,
            IReturnBillService returnService,
            IPurchaseBillService purchaseService,
            IPurchaseReturnBillService purchaseReturnService,
            ICashReceiptBillService cashReceiptBillService,
            IServiceGetter getter,
            IEventPublisher eventPublisher,
            IAllocationBillService allocationBillService,
            ICombinationProductBillService combinationProductBillService,
            ICostAdjustmentBillService costAdjustmentBillService,
            IInventoryAllTaskBillService inventoryAllTaskBillService,
            IInventoryPartTaskBillService inventoryPartTaskBillService,
            IInventoryProfitLossBillService inventoryProfitLossBillService,
            IScrapProductBillService scrapProductBillService,

            ICostExpenditureBillService costExpenditureBillService,
            IAdvanceReceiptBillService advanceReceiptBillService,

            ISplitProductBillService splitProductBillService) : base(getter, cacheManager, eventPublisher)
        {
            _paymentReceiptBillService = paymentReceiptBillService;
            _saleReservationService = saleReservationService;
            _saleService = saleService;
            _returnReservationService = returnReservationService;
            _returnService = returnService;
            _purchaseService = purchaseService;
            _purchaseReturnService = purchaseReturnService;
            _cashReceiptBillService = cashReceiptBillService;

            _allocationBillService = allocationBillService;
            _combinationProductBillService = combinationProductBillService;
            _costAdjustmentBillService = costAdjustmentBillService;
            _inventoryAllTaskBillService = inventoryAllTaskBillService;
            _inventoryPartTaskBillService = inventoryPartTaskBillService;
            _inventoryProfitLossBillService = inventoryProfitLossBillService;
            _scrapProductBillService = scrapProductBillService;
            _splitProductBillService = splitProductBillService;

            _costExpenditureBillService = costExpenditureBillService;
            _advanceReceiptBillService = advanceReceiptBillService;

        }

        public string GenerateBillUrl(int billType, int billId)
        {
            string urlLink = "";
            switch (billType)
            {
                case (int)BillTypeEnum.SaleReservationBill://销售订单
                    {

                        urlLink = $"/salereservationbill/edit?id={billId}";
                    }
                    break;
                case (int)BillTypeEnum.SaleBill://销售单
                    {
                        urlLink = $"/salebill/edit?id={billId}";
                    }
                    break;
                case (int)BillTypeEnum.ReturnReservationBill://退货订单
                    {
                        urlLink = $"/returnreservationbill/edit?id={billId}";
                    }
                    break;
                case (int)BillTypeEnum.ReturnBill://退货单
                    {
                        urlLink = $"/returnbill/edit?id={billId}";
                    }
                    break;
                case (int)BillTypeEnum.PurchaseReservationBill://采购订单

                    break;
                case (int)BillTypeEnum.PurchaseBill://采购单
                    {
                        urlLink = $"/purchasebill/edit?id={billId}";
                    }
                    break;
                case (int)BillTypeEnum.PurchaseReturnReservationBill://采购退货订单

                    break;
                case (int)BillTypeEnum.PurchaseReturnBill://采购退货单
                    {
                        urlLink = $"/purchasereturnbill/edit?id={billId}";
                    }
                    break;
                case (int)BillTypeEnum.CashReceiptBill://收款单
                    {
                        urlLink = $"/receiptcash/edit?id={billId}";
                    }
                    break;
                case (int)BillTypeEnum.AdvanceReceiptBill://预收款单
                    {
                        urlLink = $"/advancereceipt/edit?id={billId}";
                    }
                    break;
                case (int)BillTypeEnum.PaymentReceiptBill://付款单
                    {
                        urlLink = $"/paymentreceipt/edit?id={billId}";
                    }
                    break;
                case (int)BillTypeEnum.AdvancePaymentBill://预付款单
                    {
                        urlLink = $"/advancepayment/edit?id={billId}";
                    }
                    break;
                case (int)BillTypeEnum.AllocationBill://调拨单
                    {
                        urlLink = $"/allocation/edit?id={billId}";
                    }
                    break;
                case (int)BillTypeReportEnum.Input://调拨单调入
                    {
                        urlLink = $"/allocation/edit?id={billId}";
                    }
                    break;
                case (int)BillTypeReportEnum.Callout://调拨单调出
                    {
                        urlLink = $"/allocation/edit?id={billId}";
                    }
                    break;
                case (int)BillTypeEnum.InventoryProfitLossBill://盘点盈亏单
                    {
                        urlLink = $"/inventoryprofitloss/edit?id={billId}";
                    }
                    break;
                case (int)BillTypeEnum.CostAdjustmentBill://成本调价单
                    {
                        urlLink = $"/costadjustment/edit?id={billId}";
                    }
                    break;
                case (int)BillTypeEnum.ScrapProductBill://报损单
                    {
                        urlLink = $"/scrapproduct/edit?id={billId}";
                    }
                    break;
                case (int)BillTypeEnum.InventoryAllTaskBill://盘点单（整仓）
                    {
                        urlLink = $"/inventoryalltask/edit?id={billId}";
                    }
                    break;
                case (int)BillTypeEnum.InventoryPartTaskBill://盘点单（部分）
                    {
                        urlLink = $"/inventoryparttask/edit?id={billId}";
                    }
                    break;
                case (int)BillTypeEnum.CombinationProductBill://组合单
                    {
                        urlLink = $"/combinationproduct/edit?id={billId}";
                    }
                    break;
                case (int)BillTypeEnum.SplitProductBill://拆分单
                    {
                        urlLink = $"/splitproduct/edit?id={billId}";
                    }
                    break;
                case (int)BillTypeEnum.CostExpenditureBill://费用支出
                    {
                        urlLink = $"/costexpenditure/Edit?id={billId}";
                    }
                    break;
                case (int)BillTypeEnum.CostContractBill://费用合同
                    {
                        urlLink = $"/costcontract/Edit?id={billId}";
                    }
                    break;
                case (int)BillTypeEnum.FinancialIncomeBill://其它收入
                    {
                        urlLink = $"/financialincome/Edit?id={billId}";
                    }
                    break;
                case 100:
                case 101:
                case 102:
                case 103:
                case 104:
                case 105:
                case 0:
                    {
                        urlLink = $"javascript:;";
                    }
                    break;
            }

            return urlLink.ToLower();
        }

        public bool CheckWareHouse(int wareHouseId)
        {
            if (CombinationProductBillsRepository_RO.TableNoTracking.Where(w => w.WareHouseId == wareHouseId).Count() > 0)
            {
                return true;
            }
            else if (SplitProductBillsRepository_RO.TableNoTracking.Where(w => w.WareHouseId == wareHouseId).Count() > 0)
            {
                return true;
            }
            else if (AllocationBillsRepository_RO.TableNoTracking.Where(w => w.IncomeWareHouseId == wareHouseId).Count() > 0)
            {
                return true;
            }
            else if (AllocationBillsRepository_RO.TableNoTracking.Where(w => w.ShipmentWareHouseId == wareHouseId).Count() > 0)
            {
                return true;
            }
            else if (InventoryAllTaskBillsRepository_RO.TableNoTracking.Where(w => w.WareHouseId == wareHouseId).Count() > 0)
            {
                return true;
            }
            else if (InventoryPartTaskBillsRepository_RO.TableNoTracking.Where(w => w.WareHouseId == wareHouseId).Count() > 0)
            {
                return true;
            }
            else if (InventoryProfitLossBillsRepository_RO.TableNoTracking.Where(w => w.WareHouseId == wareHouseId).Count() > 0)
            {
                return true;
            }
            else if (PurchaseBillsRepository_RO.TableNoTracking.Where(w => w.WareHouseId == wareHouseId).Count() > 0)
            {
                return true;
            }
            else if (PurchaseReturnBillsRepository_RO.TableNoTracking.Where(w => w.WareHouseId == wareHouseId).Count() > 0)
            {
                return true;
            }
            else if (ReturnBillsRepository_RO.TableNoTracking.Where(w => w.WareHouseId == wareHouseId).Count() > 0)
            {
                return true;
            }
            else if (ReturnReservationBillsRepository_RO.TableNoTracking.Where(w => w.WareHouseId == wareHouseId).Count() > 0)
            {
                return true;
            }
            else if (SaleBillsRepository_RO.TableNoTracking.Where(w => w.WareHouseId == wareHouseId).Count() > 0)
            {
                return true;
            }
            else if (SaleReservationBillsRepository_RO.TableNoTracking.Where(w => w.WareHouseId == wareHouseId).Count() > 0)
            {
                return true;
            }
            else if (ScrapProductBillsRepository_RO.TableNoTracking.Where(w => w.WareHouseId == wareHouseId).Count() > 0)
            {
                return true;
            }
            else if (SplitProductBillsRepository_RO.TableNoTracking.Where(w => w.WareHouseId == wareHouseId).Count() > 0)
            {
                return true;
            }
            else if (StockEarlyWarningsRepository_RO.TableNoTracking.Where(w => w.WareHouseId == wareHouseId).Count() > 0)
            {
                return true;
            }
            else if (StockInOutRecordsRepository_RO.TableNoTracking.Where(w => w.InWareHouseId == wareHouseId || w.OutWareHouseId == wareHouseId).Count() > 0)
            {
                return true;
            }
            else if (StocksRepository_RO.TableNoTracking.Where(w => w.WareHouseId == wareHouseId).Count() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool CheckStore(int StoreId)
        {
            if (BranchRepository_RO.TableNoTracking.Where(w => w.StoreId == StoreId).Count() > 0)
            {
                return true;
            }
            else if (DataChannelPermissionsRepository_RO.TableNoTracking.Where(w => w.StoreId == StoreId).Count() > 0)
            {
                return true;
            }
            else if (ModuleRepository_RO.TableNoTracking.Where(w => w.StoreId == StoreId).Count() > 0)
            {
                return true;
            }
            else if (PermissionRecordRepository_RO.TableNoTracking.Where(w => w.StoreId == StoreId).Count() > 0)
            {
                return true;
            }
            else if (PermissionRecordRolesMappingRepository_RO.TableNoTracking.Where(w => w.StoreId == StoreId).Count() > 0)
            {
                return true;
            }
            else if (PrivateMessageRepository_RO.TableNoTracking.Where(w => w.StoreId == StoreId).Count() > 0)
            {
                return true;
            }
            else if (UserGroupRepository_RO.TableNoTracking.Where(w => w.StoreId == StoreId).Count() > 0)
            {
                return true;
            }
            else if (ReturnBillsRepository_RO.TableNoTracking.Where(w => w.StoreId == StoreId).Count() > 0)
            {
                return true;
            }
            else if (UserRoleRepository_RO.TableNoTracking.Where(w => w.StoreId == StoreId).Count() > 0)
            {
                return true;
            }
            else if (UserRepository_RO.TableNoTracking.Where(w => w.StoreId == StoreId).Count() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// 调整替换商品全月加权平均价（结转成本后，已审核业务单据中的成本价，将会被替换成结转后的全月平均价）
        /// 单据： 销售单  12 退货单 14  采购单  22 采购退货单  24  盘点盈亏单 32  成本调价单 33 报损单 34 组合单 37  拆分单 38
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="billTypeEnum"></param>
        /// <param name="billId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        public bool AdjustBillCost(int storeId, int billTypeEnum, int billId, int productId, decimal costPrice = 0)
        {
            try
            {
                switch (billTypeEnum)
                {
                    case (int)BillTypeEnum.SaleBill:
                        UpdateSaleItem(storeId, billId, productId, costPrice);
                        break;
                    case (int)BillTypeEnum.ReturnBill:
                        UpdateReturnItem(storeId, billId, productId, costPrice);
                        break;
                    case (int)BillTypeEnum.PurchaseBill:
                        UpdatePurchaseItem(storeId, billId, productId, costPrice);
                        break;
                    case (int)BillTypeEnum.PurchaseReturnBill:
                        UpdatePurchaseReturnItem(storeId, billId, productId, costPrice);
                        break;
                    case (int)BillTypeEnum.InventoryProfitLossBill:
                        UpdateInventoryProfitLossItem(storeId, billId, productId, costPrice);
                        break;
                    case (int)BillTypeEnum.CostAdjustmentBill:
                        UpdateCostAdjustmentItem(storeId, billId, productId, costPrice);
                        break;
                    case (int)BillTypeEnum.ScrapProductBill:
                        UpdateScrapProductItem(storeId, billId, productId, costPrice);
                        break;
                    case (int)BillTypeEnum.CombinationProductBill:
                        UpdateCombinationProductItem(storeId, billId, productId, costPrice);
                        break;
                    case (int)BillTypeEnum.SplitProductBill:
                        UpdateSplitProductItem(storeId, billId, productId, costPrice);
                        break;
                }
                return true;
            }
            catch (Exception)
            {
                return false;

            }
        }

        /// <summary>
        /// 销售单
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="billId"></param>
        /// <param name="productId"></param>
        /// <param name="costPrice"></param>
        public void UpdateSaleItem(int storeId, int billId, int productId, decimal costPrice = 0)
        {
            var uow = SaleItemsRepository.UnitOfWork;

            var items = SaleItemsRepository.Table.Where(s => s.StoreId == storeId && s.SaleBillId == billId && s.ProductId == productId).ToList();

            items?.ForEach(s =>
            {
                if (costPrice != 0)
                {
                    s.CostPrice = costPrice;
                }
            });

            SaleItemsRepository.Update(items);

            uow.SaveChanges();

            items?.ForEach(s =>
            {
                _eventPublisher.EntityUpdated(s);
            });
        }


        /// <summary>
        /// 退货单
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="billId"></param>
        /// <param name="productId"></param>
        /// <param name="costPrice"></param>
        public void UpdateReturnItem(int storeId, int billId, int productId, decimal costPrice = 0)
        {
            var uow = ReturnItemsRepository.UnitOfWork;

            var items = ReturnItemsRepository.Table.Where(s => s.StoreId == storeId && s.ReturnBillId == billId && s.ProductId == productId).ToList();

            items?.ForEach(s =>
            {
                if (costPrice != 0)
                {
                    s.CostPrice = costPrice;
                }
            });

            ReturnItemsRepository.Update(items);

            uow.SaveChanges();

            items?.ForEach(s =>
            {
                _eventPublisher.EntityUpdated(s);
            });
        }

        /// <summary>
        /// 采购单
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="billId"></param>
        /// <param name="productId"></param>
        /// <param name="costPrice"></param>
        public void UpdatePurchaseItem(int storeId, int billId, int productId, decimal costPrice = 0)
        {
            var uow = PurchaseItemsRepository.UnitOfWork;

            var items = PurchaseItemsRepository.Table.Where(s => s.StoreId == storeId && s.PurchaseBillId == billId && s.ProductId == productId).ToList();

            items?.ForEach(s =>
            {
                if (costPrice != 0)
                {
                    s.CostPrice = costPrice;
                }
            });

            PurchaseItemsRepository.Update(items);

            uow.SaveChanges();

            items?.ForEach(s =>
            {
                _eventPublisher.EntityUpdated(s);
            });
        }

        /// <summary>
        /// 采购退货单
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="billId"></param>
        /// <param name="productId"></param>
        /// <param name="costPrice"></param>
        public void UpdatePurchaseReturnItem(int storeId, int billId, int productId, decimal costPrice = 0)
        {
            var uow = PurchaseReturnItemsRepository.UnitOfWork;

            var items = PurchaseReturnItemsRepository.Table.Where(s => s.StoreId == storeId && s.PurchaseReturnBillId == billId && s.ProductId == productId).ToList();

            items?.ForEach(s =>
            {
                if (costPrice != 0)
                {
                    s.CostPrice = costPrice;
                }
            });

            PurchaseReturnItemsRepository.Update(items);

            uow.SaveChanges();

            items?.ForEach(s =>
            {
                _eventPublisher.EntityUpdated(s);
            });
        }

        /// <summary>
        /// 盘点盈亏单
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="billId"></param>
        /// <param name="productId"></param>
        /// <param name="costPrice"></param>
        public void UpdateInventoryProfitLossItem(int storeId, int billId, int productId, decimal costPrice = 0)
        {
            var uow = InventoryProfitLossItemsRepository.UnitOfWork;

            var items = InventoryProfitLossItemsRepository.Table.Where(s => s.StoreId == storeId && s.InventoryProfitLossBillId == billId && s.ProductId == productId).ToList();

            items?.ForEach(s =>
            {
                if (costPrice != 0)
                {
                    s.CostPrice = costPrice;
                }
            });

            InventoryProfitLossItemsRepository.Update(items);

            uow.SaveChanges();

            items?.ForEach(s =>
            {
                _eventPublisher.EntityUpdated(s);
            });
        }

        /// <summary>
        /// 成本调价单
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="billId"></param>
        /// <param name="productId"></param>
        /// <param name="costPrice"></param>
        public void UpdateCostAdjustmentItem(int storeId, int billId, int productId, decimal costPrice = 0)
        {
            var uow = CostAdjustmentItemsRepository.UnitOfWork;

            var items = CostAdjustmentItemsRepository.Table.Where(s => s.StoreId == storeId && s.CostAdjustmentBillId == billId && s.ProductId == productId).ToList();

            items?.ForEach(s =>
            {
                if (costPrice != 0)
                {
                    s.AdjustedPrice = costPrice;
                }
            });

            CostAdjustmentItemsRepository.Update(items);

            uow.SaveChanges();

            items?.ForEach(s =>
            {
                _eventPublisher.EntityUpdated(s);
            });
        }


        /// <summary>
        /// 报损单
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="billId"></param>
        /// <param name="productId"></param>
        /// <param name="costPrice"></param>
        public void UpdateScrapProductItem(int storeId, int billId, int productId, decimal costPrice = 0)
        {
            var uow = ScrapProductItemsRepository.UnitOfWork;

            var items = ScrapProductItemsRepository.Table.Where(s => s.StoreId == storeId && s.ScrapProductBillId == billId && s.ProductId == productId).ToList();

            items?.ForEach(s =>
            {
                if (costPrice != 0)
                {
                    s.CostPrice = costPrice;
                }
            });

            ScrapProductItemsRepository.Update(items);

            uow.SaveChanges();

            items?.ForEach(s =>
            {
                _eventPublisher.EntityUpdated(s);
            });
        }

        /// <summary>
        /// 组合单
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="billId"></param>
        /// <param name="productId"></param>
        /// <param name="costPrice"></param>
        public void UpdateCombinationProductItem(int storeId, int billId, int productId, decimal costPrice = 0)
        {
            var uow = CombinationProductItemsRepository.UnitOfWork;

            var items = CombinationProductItemsRepository.Table.Where(s => s.StoreId == storeId && s.CombinationProductBillId == billId && s.ProductId == productId).ToList();

            items?.ForEach(s =>
            {
                if (costPrice != 0)
                {
                    s.CostPrice = costPrice;
                }
            });

            CombinationProductItemsRepository.Update(items);

            uow.SaveChanges();

            items?.ForEach(s =>
            {
                _eventPublisher.EntityUpdated(s);
            });
        }


        /// <summary>
        /// 拆分单
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="billId"></param>
        /// <param name="productId"></param>
        /// <param name="costPrice"></param>
        public void UpdateSplitProductItem(int storeId, int billId, int productId, decimal costPrice = 0)
        {
            var uow = SplitProductItemsRepository.UnitOfWork;

            var items = SplitProductItemsRepository.Table.Where(s => s.StoreId == storeId && s.SplitProductBillId == billId && s.ProductId == productId).ToList();

            items?.ForEach(s =>
            {
                if (costPrice != 0)
                {
                    s.CostPrice = costPrice;
                }
            });

            SplitProductItemsRepository.Update(items);

            uow.SaveChanges();

            items?.ForEach(s =>
            {
                _eventPublisher.EntityUpdated(s);
            });
        }


        /// <summary>
        /// 根据明细账的借贷 金额 判断出本条记录的余额 及方向，balance 为上一条记录余额
        /// </summary>
        /// <param name="balance">余额</param>
        /// <param name="balance_debit">借方</param>
        /// <param name="balance_credit">贷方</param>
        /// <returns></returns>
        public Tuple<string, decimal> JudgmentLending(decimal balance, decimal balance_debit, decimal balance_credit)
        {
            string direction;

            balance += balance_debit - balance_credit;

            if (balance > 0)
            {
                direction = "借";
            }
            else if (balance < 0)
            {
                direction = "贷";
            }
            else
            {
                direction = "平";
            }

            return new Tuple<string, decimal>(direction, balance);
        }

        public Tuple<string, decimal> JudgmentLending(decimal balance_debit, decimal balance_credit)
        {
            string direction;

            var balance = balance_debit - balance_credit;

            if (balance > 0)
            {
                direction = "借";
            }
            else if (balance < 0)
            {
                direction = "贷";
            }
            else
            {
                direction = "平";
            }

            return new Tuple<string, decimal>(direction, balance);
        }

        public Tuple<string, decimal> JudgmentLending(Tuple<decimal, decimal, decimal> tuple)
        {
            string direction;

            var balance = tuple.Item1;
            var balance_debit = tuple.Item2;
            var balance_credit = tuple.Item3;

            balance += balance_debit - balance_credit;

            if (balance > 0)
            {
                direction = "借";
            }
            else if (balance < 0)
            {
                direction = "贷";
            }
            else
            {
                direction = "平";
            }

            return new Tuple<string, decimal>(direction, balance);
        }



        /// <summary>
        /// 更新单据交账状态
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="billId"></param>
        /// <param name="billTypeEnum"></param>
        /// <returns></returns>
        public bool UpdataBillAccountStatement(int storeId, int billId, BillTypeEnum billTypeEnum)
        {
            try
            {
                switch (billTypeEnum)
                {
                    //销售单
                    case BillTypeEnum.SaleBill:
                        _saleService.UpdateHandInStatus(storeId, billId, true);
                        break;
                    //退货单
                    case BillTypeEnum.ReturnBill:
                        _returnService.UpdateHandInStatus(storeId, billId, true);
                        break;
                    //收款单
                    case BillTypeEnum.CashReceiptBill:
                        _cashReceiptBillService.UpdateHandInStatus(storeId, billId, true);
                        break;
                    //预收款单
                    case BillTypeEnum.AdvanceReceiptBill:
                        _advanceReceiptBillService.UpdateHandInStatus(storeId, billId, true);
                        break;
                    //费用支出
                    case BillTypeEnum.CostExpenditureBill:
                        _costExpenditureBillService.UpdateHandInStatus(storeId, billId, true);
                        break;
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        public bool Cy(int change, int storeId, int billId,int productId)
        {
            //22, 24, 12, 14, 37, 38, 34, 31, 32
            if (change == 12)
            {
                //销售单
                var query = from pc in SaleItemsRepository.Table
                            where pc.StoreId == storeId && pc.SaleBillId == billId && pc.ProductId == productId
                            orderby pc.Id
                            select pc;

                return query.Count() > 0;
            }
            else if (change == 14)
            {
                var query = from pc in ReturnItemsRepository.Table
                            where pc.StoreId == storeId && pc.ReturnBillId == billId && pc.ProductId == productId
                            orderby pc.Id
                            select pc;

                return query.Count() > 0;
            }
            else if (change == 22)
            {
                var query = from pc in PurchaseItemsRepository.Table
                            where pc.StoreId == storeId && pc.PurchaseBillId == billId && pc.ProductId == productId
                            orderby pc.Id
                            select pc;

                return query.Count() > 0;
            }
            else if (change == 24)
            {
                var query = from pc in PurchaseReturnItemsRepository.Table
                            where pc.StoreId == storeId && pc.PurchaseReturnBillId == billId && pc.ProductId == productId
                            orderby pc.Id
                            select pc;

                return query.Count() > 0;
            }
            else if (change == 31)
            {
                var query = from pc in AllocationItemsRepository.Table
                            where pc.StoreId == storeId && pc.AllocationBillId == billId && pc.ProductId == productId
                            orderby pc.Id
                            select pc;

                return query.Count() > 0;
            }
            else if (change == 32)
            {
                var query = from pc in InventoryProfitLossItemsRepository.Table
                            where pc.StoreId == storeId && pc.InventoryProfitLossBillId == billId && pc.ProductId == productId
                            orderby pc.Id
                            select pc;

                return query.Count() > 0;
            }
            else if (change == 34)
            {
                var query = from pc in ScrapProductItemsRepository.Table
                            where pc.StoreId == storeId && pc.ScrapProductBillId == billId && pc.ProductId == productId
                            orderby pc.Id
                            select pc;

                return query.Count() > 0;
            }
            else if (change == 38)
            {
                var query = from pc in SplitProductItemsRepository.Table
                            where pc.StoreId == storeId && pc.SplitProductBillId == billId && pc.ProductId == productId
                            orderby pc.Id
                            select pc;

                return query.Count() > 0;
            }

            return true;
        }

    }
}
