using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DCMS.Services.Common
{
    public class BillCheckService : BaseService, IBillCheckService
    {

        public BillCheckService(IStaticCacheManager cacheManager,
            IServiceGetter getter,
            IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {
        }

        public IList<string> CheckAudited(int storeId, BillTypeEnum billTypeEnum, Tuple<DateTime, DateTime> period)
        {
            switch (billTypeEnum)
            {
                case BillTypeEnum.SaleReservationBill:
                    {
                        var query = SaleReservationBillsRepository.TableNoTracking.Where(s => s.StoreId == storeId && (s.CreatedOnUtc >= period.Item1 && s.CreatedOnUtc <= period.Item2) && s.AuditedStatus == false);
                        return query.Select(s => s.BillNumber).ToList();
                    }
                case BillTypeEnum.SaleBill:
                    {
                        var query = SaleBillsRepository.TableNoTracking.Where(s => s.StoreId == storeId && (s.CreatedOnUtc >= period.Item1 && s.CreatedOnUtc <= period.Item2) && s.AuditedStatus == false);
                        return query.Select(s => s.BillNumber).ToList();
                    }
                case BillTypeEnum.ReturnReservationBill:
                    {
                        var query = ReturnReservationBillsRepository.TableNoTracking.Where(s => s.StoreId == storeId && (s.CreatedOnUtc >= period.Item1 && s.CreatedOnUtc <= period.Item2) && s.AuditedStatus == false);
                        return query.Select(s => s.BillNumber).ToList();
                    }
                case BillTypeEnum.ReturnBill:
                    {
                        var query = ReturnBillsRepository.TableNoTracking.Where(s => s.StoreId == storeId && (s.CreatedOnUtc >= period.Item1 && s.CreatedOnUtc <= period.Item2) && s.AuditedStatus == false);
                        return query.Select(s => s.BillNumber).ToList();
                    }
                case BillTypeEnum.PurchaseBill:
                    {
                        var query = PurchaseBillsRepository.TableNoTracking.Where(s => s.StoreId == storeId && (s.CreatedOnUtc >= period.Item1 && s.CreatedOnUtc <= period.Item2) && s.AuditedStatus == false);
                        return query.Select(s => s.BillNumber).ToList();
                    }
                case BillTypeEnum.PurchaseReturnBill:
                    {
                        var query = PurchaseReturnBillsRepository.TableNoTracking.Where(s => s.StoreId == storeId && (s.CreatedOnUtc >= period.Item1 && s.CreatedOnUtc <= period.Item2) && s.AuditedStatus == false);
                        return query.Select(s => s.BillNumber).ToList();
                    }
                case BillTypeEnum.AllocationBill:
                    {
                        var query = AllocationBillsRepository.TableNoTracking.Where(s => s.StoreId == storeId && (s.CreatedOnUtc >= period.Item1 && s.CreatedOnUtc <= period.Item2) && s.AuditedStatus == false);
                        return query.Select(s => s.BillNumber).ToList();
                    }
                case BillTypeEnum.InventoryProfitLossBill:
                    {
                        var query = InventoryProfitLossBillsRepository.TableNoTracking.Where(s => s.StoreId == storeId && (s.CreatedOnUtc >= period.Item1 && s.CreatedOnUtc <= period.Item2) && s.AuditedStatus == false);
                        return query.Select(s => s.BillNumber).ToList();
                    }
                case BillTypeEnum.CostAdjustmentBill:
                    {
                        var query = CostAdjustmentBillsRepository.TableNoTracking.Where(s => s.StoreId == storeId && (s.CreatedOnUtc >= period.Item1 && s.CreatedOnUtc <= period.Item2) && s.AuditedStatus == false);
                        return query.Select(s => s.BillNumber).ToList();
                    }
                case BillTypeEnum.ScrapProductBill:
                    {
                        var query = ScrapProductBillsRepository.TableNoTracking.Where(s => s.StoreId == storeId && (s.CreatedOnUtc >= period.Item1 && s.CreatedOnUtc <= period.Item2) && s.AuditedStatus == false);
                        return query.Select(s => s.BillNumber).ToList();
                    }
                case BillTypeEnum.InventoryAllTaskBill:
                    {
                        var query = InventoryAllTaskBillsRepository.TableNoTracking.Where(s => s.StoreId == storeId && (s.CreatedOnUtc >= period.Item1 && s.CreatedOnUtc <= period.Item2) && s.InventoryStatus == 1);
                        return query.Select(s => s.BillNumber).ToList();
                    }
                case BillTypeEnum.InventoryPartTaskBill:
                    {
                        var query = InventoryPartTaskBillsRepository.TableNoTracking.Where(s => s.StoreId == storeId && (s.CreatedOnUtc >= period.Item1 && s.CreatedOnUtc <= period.Item2) && s.InventoryStatus == 1);
                        return query.Select(s => s.BillNumber).ToList();
                    }
                case BillTypeEnum.CombinationProductBill:
                    {
                        var query = CombinationProductBillsRepository.TableNoTracking.Where(s => s.StoreId == storeId && (s.CreatedOnUtc >= period.Item1 && s.CreatedOnUtc <= period.Item2) && s.AuditedStatus == false);
                        return query.Select(s => s.BillNumber).ToList();
                    }
                case BillTypeEnum.SplitProductBill:
                    {
                        var query = SplitProductBillsRepository.TableNoTracking.Where(s => s.StoreId == storeId && (s.CreatedOnUtc >= period.Item1 && s.CreatedOnUtc <= period.Item2) && s.AuditedStatus == false);
                        return query.Select(s => s.BillNumber).ToList();
                    }
                case BillTypeEnum.CashReceiptBill:
                    {
                        var query = CashReceiptBillsRepository.TableNoTracking.Where(s => s.StoreId == storeId && (s.CreatedOnUtc >= period.Item1 && s.CreatedOnUtc <= period.Item2) && s.AuditedStatus == false);
                        return query.Select(s => s.BillNumber).ToList();
                    }
                case BillTypeEnum.PaymentReceiptBill:
                    {
                        var query = PaymentReceiptBillsRepository.TableNoTracking.Where(s => s.StoreId == storeId && (s.CreatedOnUtc >= period.Item1 && s.CreatedOnUtc <= period.Item2) && s.AuditedStatus == false);
                        return query.Select(s => s.BillNumber).ToList();
                    }
                case BillTypeEnum.AdvanceReceiptBill:
                    {
                        var query = AdvanceReceiptBillsRepository.TableNoTracking.Where(s => s.StoreId == storeId && (s.CreatedOnUtc >= period.Item1 && s.CreatedOnUtc <= period.Item2) && s.AuditedStatus == false);
                        return query.Select(s => s.BillNumber).ToList();
                    }
                case BillTypeEnum.AdvancePaymentBill:
                    {
                        var query = AdvancePaymentBillsRepository.TableNoTracking.Where(s => s.StoreId == storeId && (s.CreatedOnUtc >= period.Item1 && s.CreatedOnUtc <= period.Item2) && s.AuditedStatus == false);
                        return query.Select(s => s.BillNumber).ToList();
                    }
                case BillTypeEnum.CostExpenditureBill:
                    {
                        var query = CostExpenditureBillsRepository.TableNoTracking.Where(s => s.StoreId == storeId && (s.CreatedOnUtc >= period.Item1 && s.CreatedOnUtc <= period.Item2) && s.AuditedStatus == false);
                        return query.Select(s => s.BillNumber).ToList();
                    }
                case BillTypeEnum.CostContractBill:
                    {
                        var query = CostContractBillsRepository.TableNoTracking.Where(s => s.StoreId == storeId && (s.CreatedOnUtc >= period.Item1 && s.CreatedOnUtc <= period.Item2) && s.AuditedStatus == false);
                        return query.Select(s => s.BillNumber).ToList();
                    }
                case BillTypeEnum.FinancialIncomeBill:
                    {
                        var query = FinancialIncomeBillsRepository.TableNoTracking.Where(s => s.StoreId == storeId && (s.CreatedOnUtc >= period.Item1 && s.CreatedOnUtc <= period.Item2) && s.AuditedStatus == false);
                        return query.Select(s => s.BillNumber).ToList();
                    }
                default:
                    return null;

            }
        }


        public IList<string> CheckAllBills(int storeId, Tuple<DateTime, DateTime> tuple)
        {
            var bills = new List<string>();
            bills.AddRange(CheckAudited(storeId, BillTypeEnum.SaleReservationBill, tuple));
            bills.AddRange(CheckAudited(storeId, BillTypeEnum.SaleBill, tuple));
            bills.AddRange(CheckAudited(storeId, BillTypeEnum.ReturnReservationBill, tuple));
            bills.AddRange(CheckAudited(storeId, BillTypeEnum.ReturnBill, tuple));
            bills.AddRange(CheckAudited(storeId, BillTypeEnum.PurchaseBill, tuple));
            bills.AddRange(CheckAudited(storeId, BillTypeEnum.PurchaseReturnBill, tuple));
            bills.AddRange(CheckAudited(storeId, BillTypeEnum.AllocationBill, tuple));
            bills.AddRange(CheckAudited(storeId, BillTypeEnum.InventoryProfitLossBill, tuple));
            bills.AddRange(CheckAudited(storeId, BillTypeEnum.CostAdjustmentBill, tuple));
            bills.AddRange(CheckAudited(storeId, BillTypeEnum.ScrapProductBill, tuple));
            bills.AddRange(CheckAudited(storeId, BillTypeEnum.InventoryAllTaskBill, tuple));
            bills.AddRange(CheckAudited(storeId, BillTypeEnum.InventoryPartTaskBill, tuple));
            bills.AddRange(CheckAudited(storeId, BillTypeEnum.CombinationProductBill, tuple));
            bills.AddRange(CheckAudited(storeId, BillTypeEnum.SplitProductBill, tuple));
            bills.AddRange(CheckAudited(storeId, BillTypeEnum.CashReceiptBill, tuple));
            bills.AddRange(CheckAudited(storeId, BillTypeEnum.PaymentReceiptBill, tuple));
            bills.AddRange(CheckAudited(storeId, BillTypeEnum.AdvanceReceiptBill, tuple));
            bills.AddRange(CheckAudited(storeId, BillTypeEnum.AdvancePaymentBill, tuple));
            bills.AddRange(CheckAudited(storeId, BillTypeEnum.CostExpenditureBill, tuple));
            bills.AddRange(CheckAudited(storeId, BillTypeEnum.CostContractBill, tuple));
            bills.AddRange(CheckAudited(storeId, BillTypeEnum.FinancialIncomeBill, tuple));
            return bills;
        }
    }
}
