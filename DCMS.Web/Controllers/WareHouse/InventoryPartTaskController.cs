using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.WareHouses;
using DCMS.Services.Common;
using DCMS.Services.ExportImport;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Products;
using DCMS.Services.Users;
using DCMS.Services.WareHouses;
using DCMS.ViewModel.Models.WareHouses;
using DCMS.Web.Framework.Extensions;
using DCMS.Web.Framework.Mvc.Filters;
using DCMS.Web.Infrastructure.Mapper.Extensions;
using DCMS.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DCMS.Web.Controllers
{
    /// <summary>
    /// 用于盘点任务(部分)管理
    /// </summary>
    public class InventoryPartTaskController : BasePublicController
    {
        private readonly IUserService _userService;
        private readonly IMediaService _mediaService;
        private readonly IUserActivityService _userActivityService;
        private readonly IWareHouseService _wareHouseService;
        private readonly IInventoryPartTaskBillService _inventoryPartTaskBillService;
        private readonly IProductService _productService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IRedLocker _locker;
        private readonly IExportManager _exportManager;

        public InventoryPartTaskController(
            IWorkContext workContext,
            IStoreContext storeContext,
            IMediaService mediaService,
            IUserActivityService userActivityService,
            IWareHouseService wareHouseService,
            IInventoryPartTaskBillService inventoryPartTaskBillService,
            IProductService productService,
            ISpecificationAttributeService specificationAttributeService,
            IUserService userService,
            ILogger loggerService,
            INotificationService notificationService,
            IRedLocker locker,
            IExportManager exportManager
            ) : base(workContext, loggerService, storeContext, notificationService)
        {
            _mediaService = mediaService;
            _userService = userService;
            _userActivityService = userActivityService;
            _wareHouseService = wareHouseService;
            _inventoryPartTaskBillService = inventoryPartTaskBillService;
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
        [AuthCode((int)AccessGranularityEnum.InventorySingleView)]
        public IActionResult List(int? inventoryPerson, int? wareHouseId, string billNumber = "", bool? auditedStatus = null, DateTime? startTime = null, DateTime? endTime = null, int? inventoryStatus = null, bool? showReverse = null, bool? sortByCompletedTime = null, string remark = "", int pagenumber = 0)
        {

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var model = new InventoryPartTaskBillListModel
            {

                //操作员
                InventoryPersons = BindUserSelection(_userService.BindUserList, curStore, ""),
                InventoryPerson = inventoryPerson ?? null,

                WareHouses = BindWareHouseSelection(_wareHouseService.BindWareHouseList, curStore,null,0),
                WareHouseId = wareHouseId ?? null,

                BillNumber = billNumber,
                AuditedStatus = auditedStatus,
                StartTime = (startTime == null) ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")) : startTime,
                EndTime = (endTime == null) ? DateTime.Now.AddDays(1) : endTime,
                InventoryStatus = inventoryStatus ?? null,
                ShowReverse = showReverse,
                SortByCompletedTime = sortByCompletedTime,
                Remark = remark,

                //盘点状态
                InventoryStatuss = new SelectList(from inventory in Enum.GetValues(typeof(InventorysetStatus)).Cast<InventorysetStatus>()
                                                  select new SelectListItem
                                                  {
                                                      Text = CommonHelper.GetEnumDescription(inventory),
                                                      Value = ((int)inventory).ToString(),
                                                      Selected = inventory == 0 ? true : false
                                                  }, "Value", "Text")
            };

            //获取分页
            var bills = _inventoryPartTaskBillService.GetAllInventoryPartTaskBills(
                curStore?.Id ?? 0,
                 curUser.Id,
                inventoryPerson,
                wareHouseId,
                billNumber,
                auditedStatus,
                model.StartTime,
                model.EndTime,
                inventoryStatus,
                showReverse,
                sortByCompletedTime,
                remark,
                null,
                pagenumber,
                30);
            model.PagingFilteringContext.LoadPagedList(bills);

            #region 查询需要关联其他表的数据
            var allUsers = _userService.GetUsersDictsByIds(curStore.Id, bills.Select(b => b.InventoryPerson).Distinct().ToArray());
            var allWarehouses = _wareHouseService.GetWareHouseByIds(curStore.Id, bills.Select(b => b.WareHouseId).Distinct().ToArray());
            #endregion

            model.Items = bills.OrderByDescending(b => b.CreatedOnUtc).Select(b =>
            {
                var m = b.ToModel<InventoryPartTaskBillModel>();

                //操作员	
                m.InventoryPersonName = allUsers.Where(au => au.Key == b.InventoryPerson).Select(au => au.Value).FirstOrDefault();
                //仓库
                var warehouse = allWarehouses.Where(aw => aw.Id == b.WareHouseId).FirstOrDefault();
                m.WareHouseName = warehouse == null ? "" : warehouse.Name;

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
        [AuthCode((int)AccessGranularityEnum.InventorySingleSave)]
        public IActionResult Create(int? store)
        {

            var model = new InventoryPartTaskBillModel
            {
                CreatedOnUtc = DateTime.Now,

                //操作员
                InventoryPersons = BindUserSelection(_userService.BindUserList, curStore, ""),
                InventoryPerson = -1,

                WareHouses = BindWareHouseSelection(_wareHouseService.BindWareHouseList, curStore,null,0),
                WareHouseId = -1
            };

            return View(model);
        }

        /// <summary>
        /// 编辑
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.InventorySingleView)]
        public IActionResult Edit(int id = 0)
        {

            var model = new InventoryPartTaskBillModel();
            var inventoryPartTaskBill = _inventoryPartTaskBillService.GetInventoryPartTaskBillById(curStore.Id, id, true);
            if (inventoryPartTaskBill == null)
            {
                return RedirectToAction("List");
            }

            if (inventoryPartTaskBill != null)
            {
                if (inventoryPartTaskBill.StoreId != curStore.Id)
                {
                    return RedirectToAction("List");
                }

                model = inventoryPartTaskBill.ToModel<InventoryPartTaskBillModel>();
                model.BillBarCode = _mediaService.GenerateBarCodeForBase64(inventoryPartTaskBill.BillNumber, 150, 50);
                model.Items = inventoryPartTaskBill.Items.Select(a => a.ToModel<InventoryPartTaskItemModel>()).ToList();
            }

            //操作员
            model.InventoryPersons = BindUserSelection(_userService.BindUserList, curStore, "");

            model.WareHouses = BindWareHouseSelection(_wareHouseService.BindWareHouseList, curStore, WHAEnum.InventoryAllTaskBill, curUser.Id);

            var mu = string.Empty;
            if (inventoryPartTaskBill.MakeUserId > 0)
            {
                mu = _userService.GetUserName(curStore.Id, inventoryPartTaskBill.MakeUserId);
            }
            model.MakeUserName = mu + " " + inventoryPartTaskBill.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss");

            var au = string.Empty;
            if (inventoryPartTaskBill.AuditedUserId != null && inventoryPartTaskBill.AuditedUserId > 0)
            {
                au = _userService.GetUserName(curStore.Id, inventoryPartTaskBill.AuditedUserId ?? 0);
            }
            model.AuditedUserName = au + " " + (inventoryPartTaskBill.AuditedDate.HasValue ? inventoryPartTaskBill.AuditedDate.Value.ToString("yyyy/MM/dd HH:mm:ss") : "");

            return View(model);
        }

        #region 单据项目

        /// <summary>
        /// 异步获取项目
        /// </summary>
        /// <param name="billId"></param>
        /// <returns></returns>
        public JsonResult AsyncInventoryPartTaskItems(int billId)
        {

            InventoryPartTaskBill inventoryPartTaskBill = _inventoryPartTaskBillService.GetInventoryPartTaskBillById(curStore.Id, billId, true);

            var allProducts = _productService.GetProductsByIds(curStore.Id, inventoryPartTaskBill.Items.Select(pr => pr.ProductId).Distinct().ToArray());
            var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());
            var allProductPrices = _productService.GetProductPricesByProductIds(curStore.Id, inventoryPartTaskBill.Items.Select(pr => pr.ProductId).Distinct().ToArray());
            var allProductTierPrices = _productService.GetProductTierPriceByProductIds(curStore.Id, inventoryPartTaskBill.Items.Select(pr => pr.ProductId).Distinct().ToArray());

            var items = inventoryPartTaskBill.Items.Select(o =>
            {
                var m = o.ToModel<InventoryPartTaskItemModel>();

                var product = allProducts.Where(ap => ap.Id == m.ProductId).FirstOrDefault();
                if (product != null)
                {
                    //这里替换成高级用法
                    m = product.InitBaseModel<InventoryPartTaskItemModel>(m, 0, allOptions, allProductPrices, allProductTierPrices, _productService);

                    //商品信息
                    m.BigUnitId = product.BigUnitId;
                    m.StrokeUnitId = product.StrokeUnitId;
                    m.SmallUnitId = product.SmallUnitId;

                    //当前商品实时库存
                    //if (product.Stocks != null)
                    //{
                    //    //m.CurrentStock = product.Stocks.Where(s => s.ProductId == product.Id).Sum(s => s.CurrentQuantity ?? 0);
                    //    m.CurrentStock = product.Stocks.Where(s => s.ProductId == product.Id && s.WareHouseId == (inventoryPartTaskBill == null ? 0 : inventoryPartTaskBill.WareHouseId)).Sum(s => s.CurrentQuantity ?? 0);
                    //}

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
        [AuthCode((int)AccessGranularityEnum.InventorySingleSave)]
        public async Task<JsonResult> CreateOrUpdate(InventoryPartTaskUpdateModel data, int? billId)
        {

            int inventoryPartTaskBillId = 0;
            try
            {
                if (data != null)
                {

                    InventoryPartTaskBill inventoryPartTaskBill = new InventoryPartTaskBill();

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
                        inventoryPartTaskBill = _inventoryPartTaskBillService.GetInventoryPartTaskBillById(curStore.Id, billId.Value, true);

                        //公共单据验证
                        var commonBillChecking = BillChecking<InventoryPartTaskBill, InventoryPartTaskItem>(inventoryPartTaskBill, BillStates.Draft);
                        if (commonBillChecking.Value != null)
                        {
                            return commonBillChecking;
                        }

                        //单据已经完成盘点
                        if (inventoryPartTaskBill.InventoryStatus == (int)InventorysetStatus.Completed || inventoryPartTaskBill.InventoryStatus == (int)InventorysetStatus.Canceled)
                        {
                            return Warning("单据已经完成盘点.");
                        }
                    }
                    #endregion

                    //业务逻辑
                    var dataTo = data.ToEntity<InventoryPartTaskBillUpdate>();
                    dataTo.InventoryDate = DateTime.Now;
                    dataTo.Operation = (int)OperationEnum.PC;
                    dataTo.Items = data.Items.Select(it =>
                    {
                        return it.ToEntity<InventoryPartTaskItem>();
                    }).ToList();

                    //RedLock
                    var result = await _locker.PerformActionWithLockAsync(LockKey(data),
                        TimeSpan.FromSeconds(30),
                        TimeSpan.FromSeconds(10),
                        TimeSpan.FromSeconds(1),
                        () => _inventoryPartTaskBillService.BillCreateOrUpdate(curStore.Id, curUser.Id, billId, inventoryPartTaskBill, dataTo, dataTo.Items, out inventoryPartTaskBillId, _userService.IsAdmin(curStore.Id, curUser.Id)));
                    return Json(new { Success = true, BillId = inventoryPartTaskBillId, Message = Resources.Bill_CreateOrUpdateSuccessful });
                }
            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("CreateOrUpdate", Resources.Bill_CreateOrUpdateFailed, curUser.Id);

                _notificationService.ErrorNotification(Resources.Bill_CreateOrUpdateFailed);
                return Json(new { Success = false, billId = 0, ex.Message });
            }

            _userActivityService.InsertActivity("CreateOrUpdate", Resources.CreateOrUpdate, curUser.Id);
            _notificationService.SuccessNotification(Resources.Bill_CreateOrUpdateSuccessful);

            return Json(new { Success = true, billId = inventoryPartTaskBillId, Message = Resources.Bill_CreateOrUpdateSuccessful });
        }

        /// <summary>
        /// 取消盘点
        /// </summary>
        /// <param name="billId"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthCode((int)AccessGranularityEnum.InventorySingleReverse)]
        public async Task<JsonResult> CancelTakeInventory(int? billId)
        {
            try
            {
                string errMsg = string.Empty;

                var inventoryPartTaskBill = new InventoryPartTaskBill();
                #region 验证
                if (!billId.HasValue)
                {
                    return Warning("参数错误.");
                }
                else
                {
                    inventoryPartTaskBill = _inventoryPartTaskBillService.GetInventoryPartTaskBillById(curStore.Id, billId.Value, true);
                }

                //公共单据验证
                var commonBillChecking = BillChecking<InventoryPartTaskBill, InventoryPartTaskItem>(inventoryPartTaskBill, BillStates.Draft);
                if (commonBillChecking.Value != null)
                {
                    return commonBillChecking;
                }

                if (inventoryPartTaskBill.InventoryStatus == (int)InventorysetStatus.Completed || inventoryPartTaskBill.InventoryStatus == (int)InventorysetStatus.Canceled)
                {
                    return Warning("单据已经完成盘点，不能取消.");
                }

                #endregion

                //Redis事务锁(防止重复取消盘点)
                string lockKey = string.Format(DCMSCachingDefaults.RedisDataReSubmitKey, Request.GetUrl(), curStore.Id, curUser.Id, CommonHelper.MD5(JsonConvert.SerializeObject(billId)));
                var result = await _locker.PerformActionWithLockAsync(lockKey,
                      TimeSpan.FromSeconds(30),
                      TimeSpan.FromSeconds(10),
                      TimeSpan.FromSeconds(1),
                      () => _inventoryPartTaskBillService.CancelTakeInventory(curStore.Id, curUser.Id, inventoryPartTaskBill));
                return Json(result);

            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("CancelTakeInventory", "取消盘点失败", curUser.Id);
                _notificationService.SuccessNotification("取消盘点失败");
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// 完成盘点
        /// </summary>
        /// <param name="billId"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.InventorySingleApproved)]
        public async Task<JsonResult> SetInventoryCompleted(int? id)
        {
            try
            {
                var bill = new InventoryPartTaskBill();

                #region 验证
                if (!id.HasValue)
                {
                    return Warning("参数错误.");
                }
                else
                {
                    bill = _inventoryPartTaskBillService.GetInventoryPartTaskBillById(curStore.Id, id.Value, true);
                }

                //公共单据验证
                var commonBillChecking = BillChecking<InventoryPartTaskBill, InventoryPartTaskItem>(bill, BillStates.Draft);
                if (commonBillChecking.Value != null)
                {
                    return commonBillChecking;
                }
                #endregion

                //Redis事务锁(防止重复完成盘点)
                var result = await _locker.PerformActionWithLockAsync(RedLockKey(bill),
                      TimeSpan.FromSeconds(30),
                      TimeSpan.FromSeconds(10),
                      TimeSpan.FromSeconds(1),
                      () => _inventoryPartTaskBillService.SetInventoryCompleted(curStore.Id, curUser.Id, bill));
                return Json(result);

            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("SetInventoryCompleted", "完成盘点失败", curUser.Id);
                _notificationService.SuccessNotification("完成盘点失败");
                return Error(ex.Message);
            }
        }

        #endregion

        //导出
        [AuthCode((int)AccessGranularityEnum.InventorySingleExport)]
        public FileResult Export(int type, string selectData, int? inventoryPerson, int? wareHouseId, string billNumber = "", bool? auditedStatus = null, DateTime? startTime = null, DateTime? endTime = null, int? inventoryStatus = -1, bool? showReverse = null, bool? sortByAuditedTime = null, string remark = "")
        {

            #region 查询导出数据

            IList<InventoryPartTaskBill> inventoryPartTaskBills = new List<InventoryPartTaskBill>();
            if (type == 1)
            {
                if (!string.IsNullOrEmpty(selectData))
                {
                    List<string> ids = selectData.Split(',').ToList();
                    foreach (var id in ids)
                    {
                        InventoryPartTaskBill inventoryPartTaskBill = _inventoryPartTaskBillService.GetInventoryPartTaskBillById(curStore.Id, int.Parse(id), true);
                        if (inventoryPartTaskBill != null)
                        {
                            inventoryPartTaskBills.Add(inventoryPartTaskBill);
                        }
                    }
                }
            }
            else if (type == 2)
            {
                inventoryPartTaskBills = _inventoryPartTaskBillService.GetAllInventoryPartTaskBills(
                                curStore?.Id ?? 0,
                                 curUser.Id,
                                inventoryPerson,
                                wareHouseId,
                                billNumber,
                                auditedStatus,
                                startTime,
                                endTime,
                                inventoryStatus,
                                showReverse,
                                sortByAuditedTime,
                                remark, null);
            }

            #endregion

            #region 导出
            var ms = _exportManager.ExportInventoryPartTaskBillToXlsx(inventoryPartTaskBills);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "盘点单（部分）.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "盘点单（部分）.xlsx");
            }
            #endregion
        }
    }
}