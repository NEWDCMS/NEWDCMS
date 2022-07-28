using DCMS.Core;
using DCMS.Services.Finances;
using DCMS.Services.Purchases;
using DCMS.Services.Sales;
using DCMS.Services.WareHouses;
using DCMS.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace DCMS.Api.Controllers
{
    /// <summary>
    /// 用于单据操作
    /// </summary>
   [Authorize]
    [Route("api/v{version:apiVersion}/dcms/global")]
    public class GlobalController : BaseAPIController
    {

        //销售订单
        private readonly ISaleReservationBillService _saleReservationBillService;
        //销售单
        private readonly ISaleBillService _saleBillService;
        //退货订单
        private readonly IReturnReservationBillService _returnReservationBillService;
        //退货单
        private readonly IReturnBillService _returnBillService;
        //收款单
        private readonly ICashReceiptBillService _cashReceiptBillService;
        //调拨单
        private readonly IAllocationBillService _allocationBillService;
        //费用支出
        private readonly ICostExpenditureBillService _costExpenditureBillService;
        //预收款单
        private readonly IAdvanceReceiptBillService _advanceReceiptBillService;
        //费用合同
        private readonly ICostContractBillService _costContractBillService;
        //采购单
        private readonly IPurchaseBillService _purchaseBillService;
        //盘点单
        private readonly IInventoryPartTaskBillService _inventoryPartTaskBillService;
        //库存上报
        private readonly IInventoryReportBillService _inventoryReportBillService;


        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="saleReservationBillService"></param>
        /// <param name="saleBillService"></param>
        /// <param name="returnReservationBillService"></param>
        /// <param name="returnBillService"></param>
        /// <param name="cashReceiptBillService"></param>
        /// <param name="allocationBillService"></param>
        /// <param name="costExpenditureBillService"></param>
        /// <param name="advanceReceiptBillService"></param>
        /// <param name="costContractBillService"></param>
        /// <param name="purchaseBillService"></param>
        /// <param name="inventoryPartTaskBillService"></param>
        /// <param name="inventoryReportBillService"></param>
        public GlobalController(
            IWorkContext workContext,
            IStoreContext storeContext,
            ISaleReservationBillService saleReservationBillService,
            ISaleBillService saleBillService,
            IReturnReservationBillService returnReservationBillService,
            IReturnBillService returnBillService,
            ICashReceiptBillService cashReceiptBillService,
            IAllocationBillService allocationBillService,
            ICostExpenditureBillService costExpenditureBillService,
            IAdvanceReceiptBillService advanceReceiptBillService,
            ICostContractBillService costContractBillService,
            IPurchaseBillService purchaseBillService,
            IInventoryPartTaskBillService inventoryPartTaskBillService,
            IInventoryReportBillService inventoryReportBillService
           , ILogger<BaseAPIController> logger) : base(logger)
        {
            _saleReservationBillService = saleReservationBillService;
            _saleBillService = saleBillService;
            _returnReservationBillService = returnReservationBillService;
            _returnBillService = returnBillService;
            _cashReceiptBillService = cashReceiptBillService;
            _allocationBillService = allocationBillService;
            _costExpenditureBillService = costExpenditureBillService;
            _advanceReceiptBillService = advanceReceiptBillService;
            _costContractBillService = costContractBillService;
            _purchaseBillService = purchaseBillService;
            _inventoryPartTaskBillService = inventoryPartTaskBillService;
            _inventoryReportBillService = inventoryReportBillService;


        }


        /// <summary>
        /// 更新历史单据状态
        /// </summary>
        /// <param name="store"></param>
        /// <param name="user"></param>
        /// <param name="billType"></param>
        /// <param name="billId"></param>
        /// <returns></returns>
        [HttpGet("updateHistoryBillStatus/{store}/{user}")]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> UpdateHistoryBillStatus(int? store, int? user, int? billType, int? billId = 0)
        {
            if (!store.HasValue || store.Value == 0)
            {
                return null;
            }

            return await Task.Run(() =>
            {

                //1. 给12种单据 实体和表 增加bool 字段  Deleted (已完成)
                //2. 修改API 上述单据获取的分页查询地方， 加上限制条件 Deleted 为 false
                //3. 请求 UpdateHistoryBillStatus 时更新指定的单据的 Deleted 为 true. 更新规则为（中更新一个月以前完成（AuditedStatus= true）的单据） （已完成）
                //4. 检查测试

                var result = new APIResult<dynamic>();
                try
                {
                    result.Code = (int)DCMSStatusCode.Successful;
                    switch ((BillTypeEnum)billType)
                    {
                        case BillTypeEnum.SaleReservationBill:
                            _saleReservationBillService.UpdateSaleReservationBillActive(store, billId, user);
                            break;
                        case BillTypeEnum.SaleBill:
                            _saleBillService.UpdateSaleBillActive(store, billId, user);
                            break;
                        case BillTypeEnum.ReturnReservationBill:
                            _returnReservationBillService.UpdateReturnReservationBillActive(store, billId, user);
                            break;
                        case BillTypeEnum.ReturnBill:
                            _returnBillService.UpdateReturnBillActive(store, billId, user);
                            break;
                        case BillTypeEnum.CarGoodBill:
                            break;
                        case BillTypeEnum.FinanceReceiveAccount:

                            break;
                        case BillTypeEnum.PickingBill:

                            break;
                        case BillTypeEnum.DispatchBill:

                            break;
                        case BillTypeEnum.ChangeReservation:

                            break;
                        case BillTypeEnum.PurchaseReservationBill:

                            break;
                        case BillTypeEnum.PurchaseBill:
                            _purchaseBillService.UpdatePurchaseBillActive(store, billId, user);
                            break;
                        case BillTypeEnum.PurchaseReturnReservationBill:

                            break;
                        case BillTypeEnum.PurchaseReturnBill:

                            break;
                        case BillTypeEnum.AllocationBill:
                            _allocationBillService.UpdateAllocationBillActive(store, billId, user);
                            break;
                        case BillTypeEnum.InventoryProfitLossBill:

                            break;
                        case BillTypeEnum.CostAdjustmentBill:

                            break;
                        case BillTypeEnum.ScrapProductBill:

                            break;
                        case BillTypeEnum.InventoryAllTaskBill:

                            break;
                        case BillTypeEnum.InventoryPartTaskBill:
                            _inventoryPartTaskBillService.UpdateInventoryPartTaskBillActive(store, billId, user);
                            break;
                        case BillTypeEnum.CombinationProductBill:

                            break;
                        case BillTypeEnum.SplitProductBill:

                            break;
                        case BillTypeEnum.InventoryReportBill:
                            _inventoryReportBillService.UpdateInventoryReportBillActive(store, billId, user);
                            break;
                        case BillTypeEnum.CashReceiptBill:
                            _cashReceiptBillService.UpdateCashReceiptBillActive(store, billId, user);
                            break;
                        case BillTypeEnum.PaymentReceiptBill:

                            break;
                        case BillTypeEnum.AdvanceReceiptBill:
                            _advanceReceiptBillService.UpdateAdvanceReceiptBillActive(store, billId, user);
                            break;
                        case BillTypeEnum.AdvancePaymentBill:

                            break;
                        case BillTypeEnum.CostExpenditureBill:
                            _costExpenditureBillService.UpdateCostExpenditureBillActive(store, billId, user);
                            break;
                        case BillTypeEnum.CostContractBill:
                            _costContractBillService.UpdateCostContractBillActive(store, billId, user);
                            break;
                        case BillTypeEnum.FinancialIncomeBill:

                            break;
                        case BillTypeEnum.AllLoadBill:

                            break;
                        case BillTypeEnum.ZeroLoadBill:

                            break;
                        case BillTypeEnum.AllZeroMergerBill:

                            break;
                        case BillTypeEnum.AccountingVoucher:

                            break;
                        case BillTypeEnum.StockReport:

                            break;
                        case BillTypeEnum.SaleSummeryReport:

                            break;
                        case BillTypeEnum.TransferSummaryReport:

                            break;
                        case BillTypeEnum.SaleSummeryProductReport:

                            break;
                        case BillTypeEnum.RecordingVoucher:

                            break;
                        default:
                            break;
                    }

                    return this.Successful("清理成功");
                }
                catch (Exception ex)
                {
                    return this.Error(ex.Message);

                }

            });
        }

    }
}