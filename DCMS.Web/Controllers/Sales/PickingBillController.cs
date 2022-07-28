using DCMS.Core;
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
    /// 仓库分拣
    /// </summary>
    public class PickingBillController : BasePublicController
    {
        private readonly IPrintTemplateService _printTemplateService;
        private readonly IUserService _userService;
        private readonly ITerminalService _terminalService;
        private readonly IWareHouseService _wareHouseService;
        private readonly ISaleReservationBillService _saleReservationBillService;
        private readonly IProductService _productService;
        private readonly ISpecificationAttributeService _specificationAttributeService;

        public PickingBillController(
            IWorkContext workContext,
            IStoreContext storeContext,
            IPrintTemplateService printTemplateService,
            IUserService userService,
            ITerminalService terminalService,
            IWareHouseService wareHouseService,
            ISaleReservationBillService saleReservationBillService,
            IProductService productService,
            ISpecificationAttributeService specificationAttributeService,
            ILogger loggerService,
            INotificationService notificationService
            ) : base(workContext, loggerService, storeContext, notificationService)
        {
            _printTemplateService = printTemplateService;
            _userService = userService;
            _terminalService = terminalService;
            _wareHouseService = wareHouseService;
            _saleReservationBillService = saleReservationBillService;
            _productService = productService;
            _specificationAttributeService = specificationAttributeService;
        }


        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        /// <summary>
        /// 销售订单列表
        /// </summary>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.StockSortingView)]
        public IActionResult List()
        {


            var model = new PickingBillListModel();

            #region 绑定数据源

            //业务员
            var userlists = new List<SelectListItem>();
            var users = _userService.GetUserBySystemRoleName(curStore.Id, DCMSDefaults.Salesmans).ToList();
            users.ForEach(u =>
            {
                userlists.Add(new SelectListItem() { Text = u.UserRealName, Value = u.Id.ToString() });
            });
            model.BusinessUsers = new SelectList(userlists, "Value", "Text");
            model.BusinessUserId = (model.BusinessUserId ?? null);

            //过滤
            model.PickingFilters = from filter in Enum.GetValues(typeof(PickingFilter)).Cast<PickingFilter>()
                                   select new SelectListItem
                                   {
                                       Text = CommonHelper.GetEnumDescription(filter),
                                       Value = ((int)filter).ToString(),
                                       Selected = filter == 0 ? true : false
                                   };
            model.PickingFilterSelectedIds = model.PickingFilters.Select(b => int.Parse(b.Value)).ToArray();

            #endregion

            return View(model);
        }

        /// <summary>
        /// 获取销售订单列表
        /// </summary>
        /// <param name="terminalId"></param>
        /// <param name="businessUserId"></param>
        /// <param name="wareHouseId"></param>
        /// <param name="auditingStatus"></param>
        /// <param name="districtId"></param>
        /// <param name="departmentId"></param>
        /// <param name="remark"></param>
        /// <param name="billNumber"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.StockSortingView)]
        public async Task<JsonResult> AsyncList(DateTime? start = null, DateTime? end = null, int businessUserId = 0, string remark = "", int pickingFilter = 0, int wholeScrapStatus = 0, int scrapStatus = 0, int pageIndex = 0, int pageSize = 20)
        {
            return await Task.Run(() =>
              {



                  var gridModelSale = _saleReservationBillService.GetSaleReservationBillsToPicking(curStore?.Id ?? 0, curUser.Id, start, end, businessUserId, remark, pickingFilter, wholeScrapStatus, scrapStatus, pageIndex, pageSize);

                  //将查询的销售订单转换成仓库分拣数据
                  List<PickingBillModel> pickingModels = new List<PickingBillModel>();
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

                  return Json(new
                  {
                      Success = true,
                      total = gridModel.TotalCount,
                      rows = gridModel.Select(s =>
                      {
                          //业务员名称
                          s.BusinessUserName = _userService.GetUserName(curStore.Id, s.BusinessUserId);
                          //客户名称
                          var terminal = _terminalService.GetTerminalById(curStore.Id, s.TerminalId);
                          s.TerminalName = terminal == null ? "" : terminal.Name;
                          s.TerminalPointCode = terminal == null ? "" : terminal.Code;
                          return s;
                      }).ToList()
                  });

              });
        }

        /// <summary>
        /// 打印
        /// </summary>
        /// <param name="selectData"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.StockSortingPrint)]
        public JsonResult Print(int pickingFilter, int wholeScrapStatus, int scrapStatus, string selectData)
        {
            try
            {

                bool fg = true;
                string errMsg = string.Empty;

                #region 验证

                List<SaleReservationBill> saleReservationBills = new List<SaleReservationBill>();
                string datas = string.Empty;

                if (string.IsNullOrEmpty(selectData))
                {
                    errMsg += "没有选择单号";
                }
                else
                {
                    List<string> ids = selectData.Split(',').ToList();
                    foreach (var id in ids)
                    {
                        SaleReservationBill saleReservationBill = _saleReservationBillService.GetSaleReservationBillById(curStore.Id, int.Parse(id));
                        if (saleReservationBill == null)
                        {
                            errMsg += "单据信息不存在";
                        }
                        else
                        {
                            if (saleReservationBill.StoreId != curStore.Id)
                            {
                                errMsg += "只能打印自己单据";
                            }
                            else
                            {
                                saleReservationBills.Add(saleReservationBill);
                            }
                        }
                    }
                }

                //获取打印模板
                var printTemplates = _printTemplateService.GetAllPrintTemplates(curStore.Id).ToList();

                if (printTemplates == null || printTemplates.Count == 0)
                {
                    errMsg += "没有可供选择的打印模板";
                }

                #endregion

                #region 修改数据
                if (!string.IsNullOrEmpty(errMsg))
                {
                    return Warning(errMsg);
                }
                else
                {
                    //using (var scope = new TransactionScope())
                    //{
                    //    scope.Complete();
                    //}
                    #region 修改单据表打印数
                    foreach (var saleReservationBill in saleReservationBills)
                    {
                        //整箱拆零
                        if (pickingFilter == 1)
                        {
                            saleReservationBill.PickingWholeScrapPrintNum = ((saleReservationBill.PickingWholeScrapPrintNum == null) ? 0 : saleReservationBill.PickingWholeScrapPrintNum) + 1;
                            saleReservationBill.PickingWholeScrapStatus = true;
                        }
                        else if (pickingFilter == 0)
                        {
                            //整箱
                            if (scrapStatus == 1 || scrapStatus == 3)
                            {
                                saleReservationBill.PickingScrapPrintNum = ((saleReservationBill.PickingScrapPrintNum == null) ? 0 : saleReservationBill.PickingScrapPrintNum) + 1;
                                saleReservationBill.PickingScrapStatus = true;
                            }
                            //拆零
                            else if (scrapStatus == 2 || scrapStatus == 4)
                            {
                                saleReservationBill.PickingWholePrintNum = ((saleReservationBill.PickingWholePrintNum == null) ? 0 : saleReservationBill.PickingWholePrintNum) + 1;
                                saleReservationBill.PickingWholeStatus = true;
                            }
                        }

                        _saleReservationBillService.UpdateSaleReservationBill(saleReservationBill);
                    }
                    #endregion
                }


                //整箱拆零
                if (pickingFilter == 1)
                {
                    var content = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.AllZeroMergerBill).Select(a => a.Content).FirstOrDefault();

                    //填充打印数据
                    foreach (var d in saleReservationBills)
                    {

                        StringBuilder sb = new StringBuilder();
                        sb.Append(content);

                        #region theadid
                        sb.Replace("@商铺名称", curStore.Name);
                        Terminal terminal = _terminalService.GetTerminalById(curStore.Id, d.TerminalId);
                        if (terminal != null)
                        {
                            sb.Replace("@客户名称", terminal.Name);
                        }
                        WareHouse wareHouse = _wareHouseService.GetWareHouseById(curStore.Id, d.WareHouseId);
                        if (wareHouse != null)
                        {
                            sb.Replace("@仓库", wareHouse.Name);
                        }
                        sb.Replace("@车辆", "");
                        User businessUser = _userService.GetUserById(curStore.Id, d.BusinessUserId);
                        if (businessUser != null)
                        {
                            sb.Replace("@业务员", businessUser.UserRealName);
                            sb.Replace("@业务电话", businessUser.MobileNumber);
                        }
                        sb.Replace("@订单编号", d.BillNumber);

                        #endregion

                        #region tbodyid
                        //明细
                        //获取 tbody 中的行
                        int beginTbody = sb.ToString().IndexOf("<tbody>") + 7;
                        int endTbody = sb.ToString().IndexOf("</tbody>");
                        string tbodytr = sb.ToString()[beginTbody..endTbody];
                        sb.Remove(beginTbody, endTbody - beginTbody);

                        if (d.Items != null && d.Items.Count > 0)
                        {
                            //1.先删除明细第一行
                            
                            int i = 0;

                            int bigQuantity = 0;
                            int strokeQuantity = 0;
                            int smallQuantity = 0;

                            var allProducts = _productService.GetProductsByIds(curStore.Id, d.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                            var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());
                            foreach (var item in d.Items)
                            {
                                int index = sb.ToString().IndexOf("</tbody>");
                                i++;
                                StringBuilder sb2 = new StringBuilder();
                                sb2.Append(tbodytr);

                                sb2.Replace("#序号", i.ToString());
                                var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                                if (product != null)
                                {
                                    sb2.Replace("#商品名称", product.Name);
                                    if (item.UnitId == product.BigUnitId)
                                    {
                                        bigQuantity += item.Quantity;
                                    }
                                    else if (item.UnitId == product.StrokeUnitId)
                                    {
                                        strokeQuantity += item.Quantity;
                                    }
                                    else if (item.UnitId == product.SmallUnitId)
                                    {
                                        smallQuantity += item.Quantity;
                                    }

                                }
                                sb2.Replace("#数量", item.Quantity.ToString());

                                sb.Insert(index, sb2);
                            }

                            sb.Replace("数量(大中小):###", bigQuantity + "大" + strokeQuantity + "中" + smallQuantity + "小");
                        }
                        #endregion

                        #region tfootid
                        #endregion

                        datas += sb;
                    }

                }
                else if (pickingFilter == 0)
                {
                    //整箱
                    if (scrapStatus == 1 || scrapStatus == 3)
                    {
                        var content = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.AllZeroMergerBill).Select(a => a.Content).FirstOrDefault();

                        //填充打印数据
                        foreach (var d in saleReservationBills)
                        {

                            StringBuilder sb = new StringBuilder();
                            sb.Append(content);

                            #region theadid
                            sb.Replace("@商铺名称", curStore.Name);
                            Terminal terminal = _terminalService.GetTerminalById(curStore.Id, d.TerminalId);
                            if (terminal != null)
                            {
                                sb.Replace("@客户名称", terminal.Name);
                            }
                            WareHouse wareHouse = _wareHouseService.GetWareHouseById(curStore.Id, d.WareHouseId);
                            if (wareHouse != null)
                            {
                                sb.Replace("@仓库", wareHouse.Name);
                            }
                            sb.Replace("@车辆", "");
                            User businessUser = _userService.GetUserById(curStore.Id, d.BusinessUserId);
                            if (businessUser != null)
                            {
                                sb.Replace("@业务员", businessUser.UserRealName);
                                sb.Replace("@业务电话", businessUser.MobileNumber);
                            }
                            sb.Replace("@订单编号", d.BillNumber);

                            #endregion

                            #region tbodyid
                            //明细
                            //获取 tbody 中的行
                            int beginTbody = sb.ToString().IndexOf("<tbody>") + 7;
                            int endTbody = sb.ToString().IndexOf("</tbody>");
                            string tbodytr = sb.ToString()[beginTbody..endTbody];
                            //1.先删除明细第一行
                            sb.Remove(beginTbody, endTbody - beginTbody);
                            if (d.Items != null && d.Items.Count > 0)
                            {
                                
                                int i = 0;
                                int bigQuantity = 0;
                                int strokeQuantity = 0;
                                int smallQuantity = 0;

                                var allProducts = _productService.GetProductsByIds(curStore.Id, d.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                                var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());
                                foreach (var item in d.Items)
                                {
                                    int index = sb.ToString().IndexOf("</tbody>");
                                    i++;
                                    StringBuilder sb2 = new StringBuilder();
                                    sb2.Append(tbodytr);

                                    sb2.Replace("#序号", i.ToString());
                                    var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                                    if (product != null)
                                    {
                                        sb2.Replace("#商品名称", product.Name);
                                        int thisProductSmallQuantity = 0;
                                        string thisProductSmallUnitName = "";
                                        var option = allOptions.Where(sf => sf.Id == product.SmallUnitId).FirstOrDefault();
                                        thisProductSmallUnitName = option == null ? "" : option.Name;

                                        if (item.UnitId == product.BigUnitId)
                                        {
                                            bigQuantity += item.Quantity;
                                            thisProductSmallQuantity += item.Quantity * (product.BigQuantity ?? 1);
                                            //sb2.Replace("#数量", item.Quantity.ToString());
                                        }
                                        else if (item.UnitId == product.StrokeUnitId)
                                        {
                                            strokeQuantity += item.Quantity;
                                            thisProductSmallQuantity += item.Quantity * (product.StrokeQuantity ?? 1);
                                        }
                                        else if (item.UnitId == product.SmallUnitId)
                                        {
                                            smallQuantity += item.Quantity;
                                            thisProductSmallQuantity += item.Quantity;
                                        }
                                        sb2.Replace("#数量", thisProductSmallQuantity.ToString() + thisProductSmallUnitName);
                                        sb.Insert(index, sb2);
                                    }
                                }

                                sb.Replace("数量(大中小):###", bigQuantity + "大" + strokeQuantity + "中" + smallQuantity + "小");
                            }
                            #endregion

                            #region tfootid
                            #endregion

                            datas += sb;
                        }
                    }
                    //拆零
                    else if (scrapStatus == 2 || scrapStatus == 4)
                    {
                        var content = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.AllZeroMergerBill).Select(a => a.Content).FirstOrDefault();

                        //填充打印数据
                        foreach (var d in saleReservationBills)
                        {

                            StringBuilder sb = new StringBuilder();
                            sb.Append(content);

                            #region theadid
                            sb.Replace("@商铺名称", curStore.Name);
                            Terminal terminal = _terminalService.GetTerminalById(curStore.Id, d.TerminalId);
                            if (terminal != null)
                            {
                                sb.Replace("@客户名称", terminal.Name);
                            }
                            WareHouse wareHouse = _wareHouseService.GetWareHouseById(curStore.Id, d.WareHouseId);
                            if (wareHouse != null)
                            {
                                sb.Replace("@仓库", wareHouse.Name);
                            }
                            sb.Replace("@车辆", "");
                            User businessUser = _userService.GetUserById(curStore.Id, d.BusinessUserId);
                            if (businessUser != null)
                            {
                                sb.Replace("@业务员", businessUser.UserRealName);
                                sb.Replace("@业务电话", businessUser.MobileNumber);
                            }
                            sb.Replace("@订单编号", d.BillNumber);

                            #endregion

                            #region tbodyid
                            //明细
                            //获取 tbody 中的行
                            int beginTbody = sb.ToString().IndexOf("<tbody>") + 7;
                            int endTbody = sb.ToString().IndexOf("</tbody>");
                            string tbodytr = sb.ToString()[beginTbody..endTbody];
                            //1.先删除明细第一行
                            sb.Remove(beginTbody, endTbody - beginTbody);

                            if (d.Items != null && d.Items.Count > 0)
                            {
                                
                                int i = 0;

                                var allProducts = _productService.GetProductsByIds(curStore.Id, d.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                                foreach (var item in d.Items)
                                {
                                    int index = sb.ToString().IndexOf("</tbody>");
                                    i++;
                                    StringBuilder sb2 = new StringBuilder();
                                    sb2.Append(tbodytr);

                                    sb2.Replace("#序号", i.ToString());
                                    var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                                    if (product != null)
                                    {
                                        sb2.Replace("#商品名称", product.Name);
                                        if (item.UnitId == product.SmallUnitId)
                                        {
                                            sb2.Replace("#数量", item.Quantity.ToString());
                                        }
                                        else
                                        {
                                            sb2.Replace("#数量", "");
                                        }
                                    }

                                    sb.Insert(index, sb2);
                                }

                            }
                            #endregion

                            #region tfootid
                            #endregion

                            datas += sb;
                        }
                    }
                }

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