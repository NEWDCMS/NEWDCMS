using DCMS.Api.Infrastructure.Mapper.Extensions;
using DCMS.Api.Models;
using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.WareHouses;
using DCMS.Services.Logging;
using DCMS.Services.Products;
using DCMS.Services.Users;
using DCMS.Services.WareHouses;
using DCMS.ViewModel.Models.WareHouses;
using DCMS.Web.Framework.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
    /// 用于盘点任务(整仓)管理
    /// </summary>
   [Authorize]
    [Route("api/v{version:apiVersion}/dcms/warehouse")]
    public class InventoryAllTaskController : BaseAPIController
    {

        private readonly IUserService _userService;
        private readonly IWareHouseService _wareHouseService;
        private readonly IStockService _stockService;
        private readonly IInventoryAllTaskBillService _inventoryAllTaskBillService;
        private readonly IProductService _productService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IRedLocker _locker;
        private readonly IUserActivityService _userActivityService;


        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="wareHouseService"></param>
        /// <param name="inventoryAllTaskBillService"></param>
        /// <param name="productService"></param>
        /// <param name="stockService"></param>
        /// <param name="specificationAttributeService"></param>
        /// <param name="userService"></param>
        /// <param name="locker"></param>
        /// <param name="userActivityService"></param>
        /// <param name="logger"></param>
        public InventoryAllTaskController(
            IWareHouseService wareHouseService,
            IInventoryAllTaskBillService inventoryAllTaskBillService,
            IProductService productService,
            IStockService stockService,
            ISpecificationAttributeService specificationAttributeService,
            IUserService userService,
            IRedLocker locker,
            IUserActivityService userActivityService, 
            ILogger<BaseAPIController> logger) : base(logger)
        {
            _userService = userService;
            _wareHouseService = wareHouseService;
            _inventoryAllTaskBillService = inventoryAllTaskBillService;
            _productService = productService;
            _stockService = stockService;
            _specificationAttributeService = specificationAttributeService;
            _locker = locker;
            _userActivityService = userActivityService;
        }

        /// <summary>
        /// 获取盘点单（整仓）
        /// </summary>
        /// <param name="store"></param>
        /// <param name="inventoryPerson"></param>
        /// <param name="wareHouseId"></param>
        /// <param name="billNumber"></param>
        /// <param name="auditedStatus"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="inventoryStatus"></param>
        /// <param name="showReverse"></param>
        /// <param name="sortByCompletedTime"></param>
        /// <param name="remark"></param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet("inventoryAllTask/getbills/{store}/{inventoryPerson}")]
        [SwaggerOperation("getbills")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<InventoryAllTaskBillModel>>> AsyncList(int? store, int? makeuserId, int? inventoryPerson, int? wareHouseId, string billNumber, string remark, bool? auditedStatus = null, DateTime? startTime = null, DateTime? endTime = null, int? inventoryStatus = -1, bool? showReverse = null, bool? sortByCompletedTime = null, int pagenumber = 0, int pageSize = 50)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error<InventoryAllTaskBillModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {

                    //获取分页
                    var bills = _inventoryAllTaskBillService.GetAllInventoryAllTaskBills(
                        store != null ? store : 0,
                        makeuserId,
                        inventoryPerson,
                        wareHouseId,
                        billNumber,
                        auditedStatus,
                        startTime,
                        endTime,
                        inventoryStatus,
                        showReverse,
                        sortByCompletedTime,
                        remark,
                        pagenumber,
                        pageSize);

                    #region 查询需要关联其他表的数据
                    var allUsers = _userService.GetUsersByIds(store ?? 0, bills.Select(b => b.InventoryPerson).Distinct().ToArray());
                    var allWarehouses = _wareHouseService.GetWareHouseByIds(store, bills.Select(b => b.WareHouseId).Distinct().ToArray());
                    #endregion

                    var productIds = new List<int>();
                    bills?.ToList().ForEach(s =>
                    {
                        var pids = s?.Items?.Select(s => s.ProductId).ToList();
                        productIds.AddRange(pids);
                    });
                    var allProducts = _productService.GetProductsByIds(store ?? 0, productIds.Distinct().ToArray());

                    var results = bills.OrderByDescending(b => b.CreatedOnUtc).Select(b =>
                    {
                        var m = b.ToModel<InventoryAllTaskBillModel>();

                        //操作员	
                        var user = allUsers.Where(au => au.Id == m.InventoryPerson).FirstOrDefault();
                        m.InventoryPersonName = user == null ? "" : user.UserRealName;
                        //仓库
                        var warehouse = allWarehouses.Where(aw => aw.Id == b.WareHouseId).FirstOrDefault();
                        m.WareHouseName = warehouse == null ? "" : warehouse.Name;

                        m.Items?.ToList()?.ForEach(item =>
                        {
                            var p = allProducts.Where(s => s.Id == item.ProductId).FirstOrDefault();
                            item.ProductName = p?.Name;
                        });

                        return m;
                    }).ToList();

                    return this.Successful(Resources.Successful, results);
                }
                catch (Exception ex)
                {
                    return this.Error<InventoryAllTaskBillModel>(ex.Message);
                }

            });
        }

        /// <summary>
        /// 检查库存盘点状态
        /// </summary>
        /// <param name="store"></param>
        /// <param name="user"></param>
        /// <param name="wareHouseId"></param>
        /// <returns></returns>
        [HttpGet("inventoryAllTask/CheckInventory/{store}/{user}/{wareHouseId}")]
        [SwaggerOperation("CheckInventory")]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<InventoryAllTaskBillModel>>> CheckInventory(int? store, int? user, int wareHouseId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error<InventoryAllTaskBillModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var pending = _inventoryAllTaskBillService.CheckInventory(store, user, wareHouseId);
                    var results = pending.Select(s => { return s.ToModel<InventoryAllTaskBillModel>(); }).ToList();
                    return this.Successful(Resources.Successful, results);
                }
                catch (Exception ex)
                {
                    return this.Error<InventoryAllTaskBillModel>(ex.Message);
                }
            });
        }


        /// <summary>
        /// 获取盘点单据项目
        /// </summary>
        /// <param name="store"></param>
        /// <param name="user"></param>
        /// <param name="billId"></param>
        /// <returns></returns>
        [HttpGet("inventoryAllTask/getInventoryAllTaskItems/{store}/{user}/{billId}")]
        [SwaggerOperation("getInventoryAllTaskItems")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<InventoryAllTaskItemModel>>> AsyncInventoryAllTaskItems(int? store, int? user, int billId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error<InventoryAllTaskItemModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var inventoryAllTaskBill = _inventoryAllTaskBillService.GetInventoryAllTaskBillById(store, billId, true);

                    //var gridModel = _inventoryAllTaskBillService.GetInventoryAllTaskItemsByInventoryAllTaskBillId(billId, user, store, 0, int.MaxValue);
                    var allProducts = _productService.GetProductsByIds(store ?? 0, inventoryAllTaskBill.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                    var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(store, allProducts.GetProductBigStrokeSmallUnitIds());
                    var allProductTierPrices = _productService.GetProductTierPriceByProductIds(store, allProducts.Select(p => p.Id).Distinct().ToArray());
                    var allProductPrices = _productService.GetProductPricesByProductIds(store, inventoryAllTaskBill.Items.Select(gm => gm.ProductId).Distinct().ToArray());

                    var results = inventoryAllTaskBill.Items.Select(o =>
                    {
                        var m = o.ToModel<InventoryAllTaskItemModel>();

                        var product = allProducts.Where(ap => ap.Id == m.ProductId).FirstOrDefault();
                        if (product != null)
                        {
                            //这里替换成高级用法
                            m = product.InitBaseModel<InventoryAllTaskItemModel>(m, 0, allOptions, allProductPrices, allProductTierPrices, _productService);

                            //商品信息
                            m.BigUnitId = product.BigUnitId;
                            m.StrokeUnitId = product.StrokeUnitId;
                            m.SmallUnitId = product.SmallUnitId;

                            //当前商品实时库存
                            if (product.Stocks != null)
                            {
                                //m.CurrentStock = product.Stocks.Where(s => s.ProductId == product.Id).Sum(s => s.CurrentQuantity ?? 0);
                                m.CurrentStock = product.Stocks.Where(s => s.ProductId == product.Id && s.WareHouseId == (inventoryAllTaskBill == null ? 0 : inventoryAllTaskBill.WareHouseId)).Sum(s => s.CurrentQuantity ?? 0);
                            }
                        }
                        return m;

                    }).ToList();

                    return this.Successful(Resources.Successful, results);
                }
                catch (Exception ex)
                {
                    return this.Error<InventoryAllTaskItemModel>(ex.Message);
                }

            });
        }

        /// <summary>
        /// 获取盘点单
        /// </summary>
        /// <param name="store"></param>
        /// <param name="user"></param>
        /// <param name="billId"></param>
        /// <returns></returns>
        [HttpGet("inventoryAllTask/getInventoryAllTaskBill/{store}/{user}")]
        [SwaggerOperation("getInventoryAllTaskBill")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<InventoryAllTaskBillModel>> GetInventoryAllTaskBill(int? store, int? user, int billId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error3<InventoryAllTaskBillModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {

                try
                {
                    var model = new InventoryAllTaskBillModel();
                    var inventoryAllTaskBill = _inventoryAllTaskBillService.GetInventoryAllTaskBillById(store, billId, true);
                    //var gridModel = _inventoryAllTaskBillService.GetInventoryAllTaskItemsByInventoryAllTaskBillId(billId, user, store, 0, int.MaxValue);
                    var allProducts = _productService.GetProductsByIds(store ?? 0, inventoryAllTaskBill.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                    var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(store, allProducts.GetProductBigStrokeSmallUnitIds());
                    var allProductTierPrices = _productService.GetProductTierPriceByProductIds(store, allProducts.Select(p => p.Id).Distinct().ToArray());
                    var allProductPrices = _productService.GetProductPricesByProductIds(store, inventoryAllTaskBill.Items.Select(gm => gm.ProductId).Distinct().ToArray());

                    var items = inventoryAllTaskBill.Items.Select(o =>
                    {
                        var m = o.ToModel<InventoryAllTaskItemModel>();

                        var product = allProducts.Where(ap => ap.Id == m.ProductId).FirstOrDefault();
                        if (product != null)
                        {
                            //这里替换成高级用法
                            m = product.InitBaseModel<InventoryAllTaskItemModel>(m, 0, allOptions, allProductPrices, allProductTierPrices, _productService);

                            //商品信息
                            m.BigUnitId = product.BigUnitId;
                            m.StrokeUnitId = product.StrokeUnitId;
                            m.SmallUnitId = product.SmallUnitId;

                            //当前商品实时库存
                            if (product.Stocks != null)
                            {
                                //m.CurrentStock = product.Stocks.Where(s => s.ProductId == product.Id).Sum(s => s.CurrentQuantity ?? 0);
                                m.CurrentStock = product.Stocks.Where(s => s.ProductId == product.Id && s.WareHouseId == (inventoryAllTaskBill == null ? 0 : inventoryAllTaskBill.WareHouseId)).Sum(s => s.CurrentQuantity ?? 0);
                            }
                        }
                        return m;

                    }).ToList();
                    model = inventoryAllTaskBill.ToModel<InventoryAllTaskBillModel>();
                    model.Items = items;

                    return this.Successful(Resources.Successful, model);
                }
                catch (Exception ex)
                {
                    return this.Error3<InventoryAllTaskBillModel>(ex.Message);
                }

            });
        }

        /// <summary>
        /// 创建/更新
        /// </summary>
        /// <param name="data"></param>
        /// <param name="billId"></param>
        /// <param name="store"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost("inventoryAllTask/createOrUpdate/{store}/{userId}/{billId}")]
        [SwaggerOperation("createOrUpdate")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> CreateOrUpdate(InventoryAllTaskUpdateModel data, int? billId, int? store, int? userId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error(Resources.ParameterError);

            return await Task.Run(async () =>
            {
                int inventoryAllTaskBillId = 0;

                try
                {
                    var user = _userService.GetUserById(store ?? 0, userId ?? 0);
                    InventoryAllTaskBill inventoryAllTaskBill = new InventoryAllTaskBill();

                    if (data == null || data.Items == null)
                    {
                        return this.Error("请录入数据");
                    }

                    if (PeriodLocked(DateTime.Now, store ?? 0))
                    {
                        return this.Error("锁账期间,禁止业务操作");
                    }

                    if (PeriodClosed(DateTime.Now, store ?? 0))
                    {
                        return this.Error("会计期间已结账,禁止业务操作");
                    }

                    #region 单据验证
                    if (billId.HasValue && billId.Value != 0)
                    {
                        inventoryAllTaskBill = _inventoryAllTaskBillService.GetInventoryAllTaskBillById(store, billId.Value, false);

                        //单据不存在
                        if (inventoryAllTaskBill == null)
                        {
                            return this.Error("单据不存在");
                        }


                        //单开经销商不等于当前经销商
                        if (inventoryAllTaskBill.StoreId != store)
                        {
                            return this.Error("非法操作");
                        }


                        //单据已经完成盘点
                        if (inventoryAllTaskBill.InventoryStatus == (int)InventorysetStatus.Completed || inventoryAllTaskBill.InventoryStatus == (int)InventorysetStatus.Canceled)
                        {
                            return this.Error("单据已经完成盘点");
                        }


                    }
                    #endregion

                    var dataTo = data.ToEntity<InventoryAllTaskBillUpdate>();
                    dataTo.Operation = (int)OperationEnum.APP;
                    dataTo.Items = data.Items.Select(it =>
                    {
                        return it.ToEntity<InventoryAllTaskItem>();
                    }).ToList();

                    //RedLock
                    string lockKey = string.Format(DCMSCachingDefaults.RedisDataReSubmitKey, Request.GetUrl(), store, userId, CommonHelper.MD5(JsonConvert.SerializeObject(data)));
                    var result = await _locker.PerformActionWithLockAsync(lockKey,
                       TimeSpan.FromSeconds(30),
                       TimeSpan.FromSeconds(10),
                       TimeSpan.FromSeconds(1),
                       () => _inventoryAllTaskBillService.BillCreateOrUpdate(store ?? 0, userId ?? 0, billId, inventoryAllTaskBill, dataTo, dataTo.Items, out inventoryAllTaskBillId, _userService.IsAdmin(store ?? 0, userId ?? 0)));

                    return result.To(result);

                }
                catch (Exception ex)
                {
                    //活动日志
                    _userActivityService.InsertActivity("CreateOrUpdate", Resources.Bill_CreateOrUpdateFailed, userId);
                    return this.Error(ex.Message);

                }
            });
        }

        /// <summary>
        /// 取消盘点
        /// </summary>
        /// <param name="store"></param>
        /// <param name="user"></param>
        /// <param name="billId"></param>
        /// <returns></returns>
        [HttpGet("inventoryAllTask/cancelTakeInventory/{store}/{user}/{billId}")]
        [SwaggerOperation("cancelTakeInventory")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> CancelTakeInventory(int? store, int? user, int? billId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error(Resources.ParameterError);

            return await Task.Run(async () =>
            {
                try
                {
                    string errMsg = string.Empty;
                    var inventoryAllTaskBill = new InventoryAllTaskBill();

                    if (!billId.HasValue)
                    {
                        return this.Error(Resources.ParameterError);
                    }
                    else
                        inventoryAllTaskBill = _inventoryAllTaskBillService.GetInventoryAllTaskBillById(store, billId.Value, true);

                    if (inventoryAllTaskBill == null)
                    {
                        return this.Error("单据不存在");
                    }


                    //单开经销商不等于当前经销商
                    if (inventoryAllTaskBill.StoreId != store)
                    {
                        return this.Error("非法操作");
                    }

                    if (inventoryAllTaskBill.InventoryStatus == (int)InventorysetStatus.Completed || inventoryAllTaskBill.InventoryStatus == (int)InventorysetStatus.Canceled)
                    {
                        return this.Error("单据已经完成盘点，不能取消");
                    }

                    //Redis事务锁(防止重复取消盘点)
                    string lockKey = string.Format(DCMSCachingDefaults.RedisDataReSubmitKey, Request.GetUrl(), store ?? 0, user, CommonHelper.MD5(JsonConvert.SerializeObject(billId)));
                    var result = await _locker.PerformActionWithLockAsync(lockKey,
                          TimeSpan.FromSeconds(30),
                          TimeSpan.FromSeconds(10),
                          TimeSpan.FromSeconds(1),
                          () => _inventoryAllTaskBillService.CancelTakeInventory(store ?? 0, user ?? 0, inventoryAllTaskBill));

                    return this.Successful(Resources.Successful, result);
                }
                catch (Exception ex)
                {
                    return this.Error(ex.Message);
                }

            });
        }

        /// <summary>
        /// 完成盘点
        /// </summary>
        /// <param name="store"></param>
        /// <param name="user"></param>
        /// <param name="billId"></param>
        /// <returns></returns>
        [HttpGet("inventoryAllTask/setInventoryCompleted/{store}/{user}/{billId}")]

        [SwaggerOperation("setInventoryCompleted")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        //[Get("/setInventoryCompleted/{storeId}/{userId}/{billId}")]
        public async Task<APIResult<dynamic>> SetInventoryCompleted(int? store, int? user, int? billId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error(Resources.ParameterError);

            return await Task.Run(async () =>
            {
                try
                {

                    bool fg = true;
                    string errMsg = string.Empty;

                    InventoryAllTaskBill inventoryAllTaskBill = new InventoryAllTaskBill();
                    #region 验证
                    if (billId == null)
                    {
                        errMsg += "单据编号不存在";
                    }
                    else
                    {
                        inventoryAllTaskBill = _inventoryAllTaskBillService.GetInventoryAllTaskBillById(store, billId.Value, true);
                        if (inventoryAllTaskBill == null)
                        {
                            errMsg += "单据信息不存在";
                        }
                        else
                        {
                            if (inventoryAllTaskBill.StoreId != store)
                            {
                                errMsg += "只能完成自己单据";
                            }
                            if (inventoryAllTaskBill.AuditedStatus)
                            {
                                errMsg += "单据已经完成盘点";
                            }
                            if (inventoryAllTaskBill.Items == null || inventoryAllTaskBill.Items.Count == 0)
                            {
                                errMsg += "单据没有明细";
                            }
                            else
                            {

                                #region 验证库存 验证当前商品库存(盘亏数量)
                                //将一个单据中 相同商品 数量 按最小单位汇总
                                List<ProductStockItem> productStockItems = new List<ProductStockItem>();

                                var allProducts = _productService.GetProductsByIds(store ?? 0, inventoryAllTaskBill.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                                foreach (InventoryAllTaskItem item in inventoryAllTaskBill.Items)
                                {
                                    var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                                    ProductStockItem productStockItem = productStockItems.Where(a => a.ProductId == item.ProductId).FirstOrDefault();
                                    //盘点时数量为最小单位
                                    int thisQuantity = (item.LossesQuantity ?? 0);
                                    if (productStockItem != null)
                                    {
                                        productStockItem.Quantity += thisQuantity;
                                    }
                                    else
                                    {
                                        productStockItem = new ProductStockItem
                                        {
                                            ProductId = item.ProductId,
                                            //当期选择单位
                                            UnitId = item.UnitId,
                                            //转化单位
                                            SmallUnitId = product.SmallUnitId,
                                            //转化单位
                                            BigUnitId = product.BigUnitId ?? 0,
                                            ProductName = allProducts.Where(s => s.Id == item.ProductId).FirstOrDefault()?.Name,
                                            ProductCode = allProducts.Where(s => s.Id == item.ProductId).FirstOrDefault()?.ProductCode,
                                            Quantity = thisQuantity
                                        };

                                        productStockItems.Add(productStockItem);
                                    }
                                }
                                //验证库存
                                string thisMsg = string.Empty;
                                //验证当前商品库存
                                fg = _stockService.CheckStockQty(_productService, _specificationAttributeService, inventoryAllTaskBill.StoreId, inventoryAllTaskBill.WareHouseId, productStockItems, out thisMsg);
                                if (fg == false)
                                {
                                    errMsg += thisMsg;
                                }
                                #endregion

                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(errMsg))
                    {
                        return this.Error(errMsg);
                    }
                    #endregion

                    //Redis事务锁(防止重复完成盘点)
                    string lockKey = string.Format(DCMSCachingDefaults.RedisDataReSubmitKey, Request.GetUrl(), store, user, CommonHelper.MD5(JsonConvert.SerializeObject(billId)));
                    var result = await _locker.PerformActionWithLockAsync(lockKey,
                          TimeSpan.FromSeconds(30),
                          TimeSpan.FromSeconds(10),
                          TimeSpan.FromSeconds(1),
                          () => _inventoryAllTaskBillService.SetInventoryCompleted(store ?? 0, user ?? 0, inventoryAllTaskBill));

                    return this.Successful(Resources.Successful, result);
                }
                catch (Exception ex)
                {
                    return this.Error(ex.Message);
                }

            });
        }


    }
}
