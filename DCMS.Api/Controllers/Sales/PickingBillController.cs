using DCMS.Core;
using DCMS.Services.Sales;
using DCMS.Services.Terminals;
using DCMS.Services.Users;
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
    /// 仓库分拣
    /// </summary>
   [Authorize]
    [Route("api/v{version:apiVersion}/dcms/sales")]
    public class PickingBillController : BaseAPIController
    {
        private readonly IUserService _userService;
        private readonly ITerminalService _terminalService;
        private readonly ISaleReservationBillService _saleReservationBillService;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="userService"></param>
        /// <param name="terminalService"></param>
        /// <param name="saleReservationBillService"></param>
        public PickingBillController(
            IUserService userService,
            ITerminalService terminalService,
            ISaleReservationBillService saleReservationBillService, ILogger<BaseAPIController> logger) : base(logger)
        {
            _userService = userService;
            _terminalService = terminalService;
            _saleReservationBillService = saleReservationBillService;

        }

        /// <summary>
        /// 获取仓库分拣数据
        /// </summary>
        /// <param name="store"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="businessUserId"></param>
        /// <param name="remark"></param>
        /// <param name="pickingFilter"></param>
        /// <param name="wholeScrapStatus"></param>
        /// <param name="scrapStatus"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet("pickingbill/getbills/{store}")]
        [SwaggerOperation("getbills")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<PickingBillModel>>> AsyncList(int? store, int? makeuserId, DateTime? start = null, DateTime? end = null, int businessUserId = 0, string remark = "", int pickingFilter = 0, int wholeScrapStatus = 0, int scrapStatus = 0, int pageIndex = 0, int pageSize = 50)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error<PickingBillModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                var model = new List<PickingBillModel>();
                try
                {
                    var gridModelSale = _saleReservationBillService.GetSaleReservationBillsToPicking(store ?? 0, makeuserId ?? 0, start, end, businessUserId, remark, pickingFilter, wholeScrapStatus, scrapStatus, pageIndex, pageSize);

                    //将查询的销售订单转换成仓库分拣数据
                    var pickingModels = new List<PickingBillModel>();
                    if (gridModelSale != null && gridModelSale.Count > 0)
                    {
                        gridModelSale.ToList().ForEach(a =>
                        {
                            PickingBillModel pickingModel = new PickingBillModel
                            {
                                BillId = a.Id,
                                BillNumber = a.BillNumber,
                                BillType = (int)BillTypeEnum.SaleReservationBill,
                                BillTypeName = CommonHelper.GetEnumDescription(BillTypeEnum.SaleReservationBill),
                                TransactionDate = a.CreatedOnUtc,
                                BusinessUserId = a.BusinessUserId,
                                TerminalId = a.TerminalId,
                                OrderAmount = a.SumAmount,
                                Remark = a.Remark
                            };

                            //打印次数，打印时间
                            //整箱拆零
                            if (pickingFilter == 1)
                            {
                                pickingModel.PrintNum = a.PickingWholeScrapPrintNum ?? 0;
                                pickingModel.PrintData = a.PickingWholeScrapPrintDate;
                            }
                            else
                            {
                                //拆零
                                if (scrapStatus == 1 || scrapStatus == 3)
                                {
                                    pickingModel.PrintNum = a.PickingScrapPrintNum ?? 0;
                                    pickingModel.PrintData = a.PickingScrapPrintDate;
                                }
                                //整箱
                                else if (scrapStatus == 2 || scrapStatus == 4)
                                {
                                    pickingModel.PrintNum = a.PickingWholePrintNum ?? 0;
                                    pickingModel.PrintData = a.PickingWholePrintDate;
                                }
                            }

                            pickingModels.Add(pickingModel);
                        });
                    }

                    //分页
                    var gridModel = new PagedList<PickingBillModel>(pickingModels, pageIndex, pageSize);
                    var results = gridModel.Select(s =>
                      {
                          //业务员名称
                          s.BusinessUserName = _userService.GetUserName(store ?? 0, s.BusinessUserId);
                          //客户名称
                          var terminal = _terminalService.GetTerminalById(store ?? 0, s.TerminalId);
                          s.TerminalName = terminal == null ? "" : terminal.Name;
                          s.TerminalPointCode = terminal == null ? "" : terminal.Code;
                          return s;
                      }).ToList();


                    return this.Successful(Resources.Successful, results);
                }
                catch (Exception ex)
                {
                    return this.Error<PickingBillModel>(ex.Message);
                }
            });
        }
    }
}