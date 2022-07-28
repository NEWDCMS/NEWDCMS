using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.Users;
using DCMS.Core.Domain.WareHouses;
using DCMS.Services.Common;
using DCMS.Services.Configuration;
using DCMS.Services.ExportImport;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Products;
using DCMS.Services.Users;
using DCMS.Services.WareHouses;
using DCMS.ViewModel.Models.WareHouses;
using DCMS.Web.Framework.Mvc.Filters;
using DCMS.Web.Infrastructure.Mapper.Extensions;
using DCMS.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCMS.Web.Controllers
{
    /// <summary>
    /// 用于成本调价管理
    /// </summary>
    public class CostAdjustmentController : BasePublicController
    {
        private readonly IPrintTemplateService _printTemplateService;
        private readonly IUserService _userService;
        private readonly IMediaService _mediaService;
        private readonly IUserActivityService _userActivityService;
        private readonly ISettingService _settingService;
        private readonly ICostAdjustmentBillService _costAdjustmentBillService;
        private readonly IProductService _productService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IRedLocker _locker;
        private readonly IExportManager _exportManager;

        public CostAdjustmentController(
            IWorkContext workContext,
            IStoreContext storeContext,
            IPrintTemplateService printTemplateService,
            IMediaService mediaService,
            IUserActivityService userActivityService,
            ISettingService settingService,
            ICostAdjustmentBillService costAdjustmentBillService,
             IProductService productService,
            ISpecificationAttributeService specificationAttributeService,
            IUserService userService,
            ILogger loggerService,
            INotificationService notificationService,
            IRedLocker locker,
            IExportManager exportManager
            ) : base(workContext, loggerService, storeContext, notificationService)
        {
            _printTemplateService = printTemplateService;
            _mediaService = mediaService;
            _userService = userService;
            _userActivityService = userActivityService;
            _settingService = settingService;
            _costAdjustmentBillService = costAdjustmentBillService;
            _productService = productService;
            _specificationAttributeService = specificationAttributeService;
            _locker = locker;
            _exportManager = exportManager;
        }

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        /// <summary>
        /// 列表
        /// </summary>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.CostAdjustmentView)]
        public IActionResult List(int? operatorId, string billNumber = "", bool? auditedStatus = null, DateTime? startTime = null, DateTime? endTime = null, bool? PrintedStatus = null, bool? showReverse = null, bool? sortByAuditedTime = null, string remark = "", int pagenumber = 0)
        {


            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var model = new CostAdjustmentBillListModel
            {
                Operators = BindUserSelection(_userService.BindUserList, curStore, ""),
                OperatorId = null,

                BillNumber = billNumber,
                AuditedStatus = auditedStatus,
                StartTime = (startTime == null) ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")) : startTime,
                EndTime = (endTime == null) ? DateTime.Now.AddDays(1) : endTime,
                ShowReverse = showReverse,
                SortByAuditedTime = sortByAuditedTime
            };

            //获取分页
            var bills = _costAdjustmentBillService.GetAllCostAdjustmentBills(
                curStore?.Id ?? 0,
                operatorId,
                billNumber,
                auditedStatus,
                model.StartTime,
                model.EndTime,
                showReverse,
                sortByAuditedTime,
                pagenumber,
                30);
            model.PagingFilteringContext.LoadPagedList(bills);

            #region 查询需要关联其他表的数据
            var allUsers = _userService.GetUsersDictsByIds(curStore.Id, bills.Select(b => b.MakeUserId).Distinct().ToArray());
            #endregion

            model.Items = bills.OrderByDescending(b => b.CreatedOnUtc).Select(b =>
            {
                var m = b.ToModel<CostAdjustmentBillModel>();
                //操作员
                m.MakeUserName = allUsers.Where(au => au.Key == b.MakeUserId).Select(au => au.Value).FirstOrDefault();
                return m;
            }).ToList();

            return View(model);
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.CostAdjustmentSave)]
        public IActionResult Create(int? store)
        {
            var model = new CostAdjustmentBillModel
            {
                CreatedOnUtc = DateTime.Now,
                AdjustmentDate = DateTime.Now
            };

            return View(model);
        }

        /// <summary>
        /// 编辑
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.CostAdjustmentView)]
        public IActionResult Edit(int id = 0)
        {
            var model = new CostAdjustmentBillModel();
            var costAdjustmentBill = _costAdjustmentBillService.GetCostAdjustmentBillById(curStore.Id, id, true);
            if (costAdjustmentBill == null)
            {
                return RedirectToAction("List");
            }

            if (costAdjustmentBill != null)
            {
                if (costAdjustmentBill.StoreId != curStore.Id)
                {
                    return RedirectToAction("List");
                }

                model = costAdjustmentBill.ToModel<CostAdjustmentBillModel>();
                model.BillBarCode = _mediaService.GenerateBarCodeForBase64(costAdjustmentBill.BillNumber, 150, 50);
                model.Items = costAdjustmentBill.Items.Select(a => a.ToModel<CostAdjustmentItemModel>()).ToList();
            }

            var mu = string.Empty;
            if (costAdjustmentBill.MakeUserId > 0)
            {
                mu = _userService.GetUserName(curStore.Id, costAdjustmentBill.MakeUserId);
            }
            model.MakeUserName = mu + " " + costAdjustmentBill.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss");

            var au = string.Empty;
            if (costAdjustmentBill.AuditedUserId != null && costAdjustmentBill.AuditedUserId > 0)
            {
                au = _userService.GetUserName(curStore.Id, costAdjustmentBill.AuditedUserId ?? 0);
            }
            model.AuditedUserName = au + " " + (costAdjustmentBill.AuditedDate.HasValue ? costAdjustmentBill.AuditedDate.Value.ToString("yyyy/MM/dd HH:mm:ss") : "");

            return View(model);
        }

        #region 单据项目

        /// <summary>
        /// 异步获取项目
        /// </summary>
        /// <param name="billId"></param>
        /// <returns></returns>
        public JsonResult AsyncCostAdjustmentItems(int billId)
        {

            var gridModel =_costAdjustmentBillService.GetCostAdjustmentItemsByCostAdjustmentBillId(billId, curUser.Id, curStore.Id, 0, 30);
            var allProducts = _productService.GetProductsByIds(curStore.Id, gridModel.Select(pr => pr.ProductId).Distinct().ToArray());
            var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());
            var allProductPrices = _productService.GetProductPricesByProductIds(curStore.Id, gridModel.Select(pr => pr.ProductId).Distinct().ToArray());
            var allProductTierPrices = _productService.GetProductTierPriceByProductIds(curStore.Id, gridModel.Select(pr => pr.ProductId).Distinct().ToArray());

            var items = gridModel.Select(o =>
            {
                var m = o.ToModel<CostAdjustmentItemModel>();

                var product = allProducts.Where(ap => ap.Id == m.ProductId).FirstOrDefault();
                if (product != null)
                {
                    //这里替换成高级用法
                    m = product.InitBaseModel<CostAdjustmentItemModel>(m, 0, allOptions, allProductPrices, allProductTierPrices, _productService);
                }

                return m;

            }).ToList();

            return Json(new
            {
                Success = true,
                total = items.Count,
                rows = items
            });
        }

        /// <summary>
        /// 创建/更新
        /// </summary>
        /// <param name="data"></param>
        /// <param name="billId"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthCode((int)AccessGranularityEnum.CostAdjustmentSave)]
        public async Task<JsonResult> CreateOrUpdate(CostAdjustmentUpdateModel data, int? billId)
        {
            try
            {
                CostAdjustmentBill costAdjustmentBill = new CostAdjustmentBill();

                if (data == null || data.Items == null)
                {
                    return Warning("请录入数据.");
                }

                if (PeriodLocked(DateTime.Now))
                {
                    return Warning("锁账期间,禁止业务操作.");
                }

                if (PeriodClosed(DateTime.Now))
                {
                    return Warning("会计期间已结账,禁止业务操作.");
                }

                #region 单据验证
                if (billId.HasValue && billId.Value != 0)
                {
                    costAdjustmentBill = _costAdjustmentBillService.GetCostAdjustmentBillById(curStore.Id, billId.Value, true);

                    //公共单据验证
                    var commonBillChecking = BillChecking<CostAdjustmentBill, CostAdjustmentItem>(costAdjustmentBill, BillStates.Draft);
                    if (commonBillChecking.Value != null)
                    {
                        return commonBillChecking;
                    }
                }
                #endregion

                //业务逻辑
                var dataTo = data.ToEntity<CostAdjustmentBillUpdate>();
                dataTo.Operation = (int)OperationEnum.PC;
                dataTo.Items = data.Items.Select(it =>
                {
                    return it.ToEntity<CostAdjustmentItem>();
                }).ToList();

                //RedLock
                var result = await _locker.PerformActionWithLockAsync(LockKey(data),
                     TimeSpan.FromSeconds(30),
                     TimeSpan.FromSeconds(10),
                     TimeSpan.FromSeconds(1),
                     () => _costAdjustmentBillService.BillCreateOrUpdate(curStore.Id, curUser.Id, billId, costAdjustmentBill, dataTo, dataTo.Items, _userService.IsAdmin(curStore.Id, curUser.Id)));

                return Json(result);
            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("CreateOrUpdate", Resources.Bill_CreateOrUpdateFailed, curUser.Id);
                _notificationService.SuccessNotification(Resources.Bill_CreateOrUpdateFailed);
                return Error(ex.Message);
            }

        }

        #endregion

        /// <summary>
        /// 审核
        /// </summary>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.CostAdjustmentApproved)]
        public async Task<JsonResult> Auditing(int? id)
        {
            try
            {
                var bill = new CostAdjustmentBill();

                #region 验证

                if (!id.HasValue)
                {
                    return Warning("参数错误.");
                }
                else
                {
                    bill = _costAdjustmentBillService.GetCostAdjustmentBillById(curStore.Id, id.Value, true);
                    if (bill.AuditedStatus)
                    {
                        return Warning("单据已审核，请刷新页面.");
                    }
                }

                //公共单据验证
                var commonBillChecking = BillChecking<CostAdjustmentBill, CostAdjustmentItem>(bill, BillStates.Audited);
                if (commonBillChecking.Value != null)
                {
                    return commonBillChecking;
                }
                #endregion

                //RedLock
                var result = await _locker.PerformActionWithLockAsync(RedLockKey(bill),
                      TimeSpan.FromSeconds(30),
                      TimeSpan.FromSeconds(10),
                      TimeSpan.FromSeconds(1),
                      () => _costAdjustmentBillService.Auditing(curStore.Id, curUser.Id, bill));
                return Json(result);

            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("Auditing", "单据审核失败", curUser.Id);
                _notificationService.SuccessNotification("单据审核失败");
                return Error(ex.Message);
            }

        }

        /// <summary>
        /// 红冲
        /// </summary>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.CostAdjustmentReverse)]
        public async Task<JsonResult> Reverse(int? id)
        {
            try
            {
                var bill = new CostAdjustmentBill() { StoreId = curStore?.Id ?? 0 };

                #region 验证

                if (PeriodClosed(DateTime.Now))
                {
                    return Warning("系统当月已经结转，不允许红冲");
                }

                if (!id.HasValue)
                {
                    return Warning("参数错误.");
                }
                else
                {
                    bill = _costAdjustmentBillService.GetCostAdjustmentBillById(curStore.Id, id.Value, true);
                    if (bill.ReversedStatus)
                    {
                        return Warning("单据已红冲，请刷新页面.");
                    }
                }

                //公共单据验证
                var commonBillChecking = BillChecking<CostAdjustmentBill, CostAdjustmentItem>(bill, BillStates.Reversed);
                if (commonBillChecking.Value != null)
                {
                    return commonBillChecking;
                }
                #endregion

                //RedLock
                var result = await _locker.PerformActionWithLockAsync(RedLockKey(bill),
                      TimeSpan.FromSeconds(30),
                      TimeSpan.FromSeconds(10),
                      TimeSpan.FromSeconds(1),
                      () => _costAdjustmentBillService.Reverse(curUser.Id, bill));
                return Json(result);

            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("Reverse", "单据红冲失败", curUser.Id);
                _notificationService.SuccessNotification("单据红冲失败");
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="operatorId"></param>
        /// <param name="billNumber"></param>
        /// <param name="auditedStatus"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="PrintedStatus"></param>
        /// <param name="showReverse"></param>
        /// <param name="sortByAuditedTime"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.CostAdjustmentExport)]
        public FileResult Export(int type, string selectData, int? operatorId, string billNumber = "", bool? auditedStatus = null, DateTime? startTime = null, DateTime? endTime = null, bool? PrintedStatus = null, bool? showReverse = null, bool? sortByAuditedTime = null, string remark = "")
        {

            #region 查询导出数据

            IList<CostAdjustmentBill> costAdjustmentBills = new List<CostAdjustmentBill>();
            if (type == 1)
            {
                if (!string.IsNullOrEmpty(selectData))
                {
                    List<string> ids = selectData.Split(',').ToList();
                    foreach (var id in ids)
                    {
                        CostAdjustmentBill costAdjustmentBill = _costAdjustmentBillService.GetCostAdjustmentBillById(curStore.Id, int.Parse(id), true);
                        if (costAdjustmentBill != null)
                        {
                            costAdjustmentBills.Add(costAdjustmentBill);
                        }
                    }
                }
            }
            else if (type == 2)
            {
                costAdjustmentBills = _costAdjustmentBillService.GetAllCostAdjustmentBills(
                        curStore?.Id ?? 0,
                        operatorId,
                        billNumber,
                        auditedStatus,
                        startTime,
                        endTime,
                        showReverse,
                        sortByAuditedTime);
            }

            #endregion

            #region 导出
            var ms = _exportManager.ExportCostAdjustmentBillToXlsx(costAdjustmentBills);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "成本调价单.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "成本调价单.xlsx");
            }
            #endregion
        }
        [AuthCode((int)AccessGranularityEnum.CostAdjustmentPrint)]
        public JsonResult PrintSetting()
        {
            var printTemplates = _printTemplateService.GetAllPrintTemplates(curStore.Id).ToList();
            var printTemplate = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.CostAdjustmentBill).FirstOrDefault();
            //获取打印设置
            var pCPrintSetting = _settingService.LoadSetting<PCPrintSetting>(_storeContext.CurrentStore.Id);
            var settings = new object();
            if (pCPrintSetting != null)
            {
                settings = new
                {
                    PaperWidth = (printTemplate?.PaperWidth == 0 || printTemplate?.PaperHeight == 0) ? pCPrintSetting.PaperWidth : printTemplate.PaperWidth,
                    PaperHeight = (printTemplate?.PaperWidth == 0 || printTemplate?.PaperHeight == 0) ? pCPrintSetting.PaperHeight : printTemplate.PaperHeight,
                    BorderType = pCPrintSetting.BorderType,
                    MarginTop = pCPrintSetting.MarginTop,
                    MarginBottom = pCPrintSetting.MarginBottom,
                    MarginLeft = pCPrintSetting.MarginLeft,
                    MarginRight = pCPrintSetting.MarginRight,
                    IsPrintPageNumber = pCPrintSetting.IsPrintPageNumber,
                    PrintHeader = pCPrintSetting.PrintHeader,
                    PrintFooter = pCPrintSetting.PrintFooter,
                    FixedRowNumber = pCPrintSetting.FixedRowNumber,
                    PrintSubtotal = pCPrintSetting.PrintSubtotal,
                    PrintPort = pCPrintSetting.PrintPort
                };
                return Successful("", settings);
            }
            return Successful("", null);

        }
        /// <summary>
        /// 打印
        /// </summary>
        /// <param name="selectData"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.CostAdjustmentPrint)]
        public JsonResult Print(int type, string selectData, int? operatorId, string billNumber = "", bool? auditedStatus = null, DateTime? startTime = null, DateTime? endTime = null, bool? PrintedStatus = null, bool? showReverse = null, bool? sortByAuditedTime = null, string remark = "")
        {
            try
            {

                bool fg = true;
                string errMsg = string.Empty;

                #region 查询打印数据

                IList<CostAdjustmentBill> costAdjustmentBills = new List<CostAdjustmentBill>();
                var datas = new List<string>();
                //默认选择
                type = type == 0 ? 1 : type;
                if (type == 1)
                {
                    if (!string.IsNullOrEmpty(selectData))
                    {
                        List<string> ids = selectData.Split(',').ToList();
                        foreach (var id in ids)
                        {
                            CostAdjustmentBill costAdjustmentBill = _costAdjustmentBillService.GetCostAdjustmentBillById(curStore.Id, int.Parse(id), true);
                            if (costAdjustmentBill != null)
                            {
                                costAdjustmentBills.Add(costAdjustmentBill);
                            }
                        }
                    }
                }
                else if (type == 2)
                {
                    costAdjustmentBills = _costAdjustmentBillService.GetAllCostAdjustmentBills(
                            curStore?.Id ?? 0,
                            operatorId,
                            billNumber,
                            auditedStatus,
                            startTime,
                            endTime,
                            showReverse,
                            sortByAuditedTime);
                }

                #endregion

                #region 修改数据
                if (costAdjustmentBills != null && costAdjustmentBills.Count > 0)
                {
                    //using (var scope = new TransactionScope())
                    //{

                    //    scope.Complete();
                    //}

                    #region 修改单据表打印数
                    foreach (var d in costAdjustmentBills)
                    {
                        d.PrintNum = (d.PrintNum ?? 0) + 1;
                        _costAdjustmentBillService.UpdateCostAdjustmentBill(d);
                    }
                    #endregion
                }

                //获取打印模板
                var printTemplates = _printTemplateService.GetAllPrintTemplates(curStore.Id).ToList();
                var content = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.CostAdjustmentBill).Select(a => a.Content).FirstOrDefault();

                //获取打印设置
                var pCPrintSetting = _settingService.LoadSetting<PCPrintSetting>(_storeContext.CurrentStore.Id);

                //填充打印数据
                foreach (var d in costAdjustmentBills)
                {

                    StringBuilder sb = new StringBuilder();
                    sb.Append(content);

                    #region theadid
                    //sb.Replace("@商铺名称", curStore.Name);
                    if (pCPrintSetting != null)
                    {
                        sb.Replace("@商铺名称", string.IsNullOrWhiteSpace(pCPrintSetting.StoreName) ? "&nbsp;" : pCPrintSetting.StoreName);
                    }

                    sb.Replace("@单据编号", d.BillNumber);

                    #endregion

                    #region tbodyid
                    //明细
                    //获取 tbody 中的行
                    int beginTbody = sb.ToString().IndexOf(@"<tbody id=""tbody"">") + @"<tbody id=""tbody"">".Length;
                    if (beginTbody == 17)
                    {
                        beginTbody = sb.ToString().IndexOf(@"<tbody id='tbody'>") + @"<tbody id='tbody'>".Length;
                    }
                    int endTbody = sb.ToString().IndexOf("</tbody>", beginTbody);
                    string tbodytr = sb.ToString()[beginTbody..endTbody];

                    if (d.Items != null && d.Items.Count > 0)
                    {
                        //1.先删除明细第一行
                        sb.Remove(beginTbody, endTbody - beginTbody);
                        int i = 0;
                        var allProducts = _productService.GetProductsByIds(curStore.Id, d.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                        var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());
                        foreach (var item in d.Items)
                        {
                            int index = sb.ToString().IndexOf("</tbody>", beginTbody);
                            i++;
                            StringBuilder sb2 = new StringBuilder();
                            sb2.Append(tbodytr);

                            sb2.Replace("#序号", i.ToString());
                            var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                            if (product != null)
                            {
                                sb2.Replace("#商品名称", product.Name);
                                ProductUnitOption productUnitOption = product.GetProductUnit(_specificationAttributeService, _productService);
                                if (item.UnitId == product.SmallUnitId)
                                {
                                    sb2.Replace("#条形码", product.SmallBarCode);
                                    if (productUnitOption != null && productUnitOption.smallOption != null)
                                    {
                                        sb2.Replace("#商品单位", productUnitOption.smallOption.Name);
                                    }

                                }
                                else if (item.UnitId == product.StrokeUnitId)
                                {
                                    sb2.Replace("#条形码", product.StrokeBarCode);
                                    if (productUnitOption != null && productUnitOption.strokOption != null)
                                    {
                                        sb2.Replace("#商品单位", productUnitOption.strokOption.Name);
                                    }
                                }
                                else if (item.UnitId == product.BigUnitId)
                                {
                                    sb2.Replace("#条形码", product.BigBarCode);
                                    if (productUnitOption != null && productUnitOption.bigOption != null)
                                    {
                                        sb2.Replace("#商品单位", productUnitOption.bigOption.Name);
                                    }
                                }
                            }
                            sb2.Replace("#调整前价格", item.AdjustmentPriceBefore == null ? "0.00" : item.AdjustmentPriceBefore?.ToString("0.00"));
                            sb2.Replace("#调整后价格", item.AdjustedPrice == null ? "0.00" : item.AdjustedPrice?.ToString("0.00"));
                            sb2.Replace("#单位换算", product.GetProductUnitConversion(allOptions));
                            sb2.Replace("#保质期", (product.ExpirationDays ?? 0).ToString());
                            sb2.Replace("#备注", "");
                            sb.Insert(index, sb2);

                        }

                    }
                    #endregion

                    #region tfootid
                    User makeUser = _userService.GetUserById(curStore.Id, d.MakeUserId);
                    if (makeUser != null)
                    {
                        sb.Replace("@制单", makeUser.UserRealName);
                    }
                    sb.Replace("@日期", d.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss"));
                    sb.Replace("@打印日期", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                    //sb.Replace("@公司地址", "");
                    if (pCPrintSetting != null)
                    {
                        sb.Replace("@公司地址", pCPrintSetting.Address);
                    }

                    sb.Replace("@备注", d.Remark);
                    //sb.Replace("@订货电话", "");
                    if (pCPrintSetting != null)
                    {
                        sb.Replace("@订货电话", pCPrintSetting.PlaceOrderTelphone);
                    }

                    #endregion

                    datas.Add(sb.ToString());
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