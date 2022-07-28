using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Finances;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.Users;
using DCMS.Services.Common;
using DCMS.Services.ExportImport;
using DCMS.Services.Finances;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Products;
using DCMS.Services.Purchases;
using DCMS.Services.Settings;
using DCMS.Services.Terminals;
using DCMS.Services.Users;
using DCMS.ViewModel.Models.Configuration;
using DCMS.ViewModel.Models.Finances;
using DCMS.ViewModel.Models.Products;
using DCMS.Web.Framework.Mvc.Filters;
using DCMS.Web.Infrastructure.Mapper.Extensions;
using DCMS.Web.Models;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NPOI.SS.Formula.Functions;
using NUglify.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace DCMS.Web.Controllers
{
    /// <summary>
    /// 用于费用合同管理
    /// </summary>
    public class CostContractController : BasePublicController
    {
        private readonly ITerminalService _terminalService;
        private readonly IUserService _userService;
        private readonly IMediaService _mediaService;
        private readonly IAccountingService _accountingService;
        private readonly IUserActivityService _userActivityService;
        private readonly ICostContractBillService _costContractBillService;
        private readonly IProductService _productService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IGiveQuotaService _giveQuotaService;
        private readonly IPurchaseBillService _purchaseBillService;
        private readonly IRedLocker _locker;
        private readonly IExportManager _exportManager;

        public CostContractController(
            IWorkContext workContext,
            IStoreContext storeContext,
            ILogger loggerService,
            ITerminalService terminalService,
            IMediaService mediaService,
            IAccountingService accountingService,
            IUserActivityService userActivityService,
            ICostContractBillService costContractBillService,
            IProductService productService,
            ISpecificationAttributeService specificationAttributeService,
            IUserService userService,
            INotificationService notificationService,
            IGiveQuotaService giveQuotaService,
            IPurchaseBillService purchaseBillService,
            IRedLocker locker,
            IExportManager exportManager
            ) : base(workContext, loggerService, storeContext, notificationService)
        {
            _terminalService = terminalService;
            _mediaService = mediaService;
            _accountingService = accountingService;
            _userService = userService;
            _userActivityService = userActivityService;
            _costContractBillService = costContractBillService;
            _productService = productService;
            _specificationAttributeService = specificationAttributeService;
            _giveQuotaService = giveQuotaService;
            _purchaseBillService = purchaseBillService;
            _locker = locker;
            _exportManager = exportManager;
        }

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        /// <summary>
        /// 费用合同列表
        /// </summary>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.CostContractView)]
        public IActionResult List(int? customerId, string customerName, int? employeeId, string billNumber = "", string remark = "", DateTime? startTime = null, DateTime? endTime = null, int pagenumber = 0)
        {

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var model = new CostContractBillListModel
            {
                //客户
                CustomerId = customerId ?? 0,
                CustomerName = customerName,

                //员工
                Employees = BindUserSelection(_userService.BindUserList, curStore, ""),
                EmployeeId = employeeId ?? null,

                BillNumber = billNumber,
                Remark = remark,
                //StartTime = (startTime == null) ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")) : startTime,
                EndTime = (endTime == null) ? DateTime.Now.AddDays(1) : endTime
            };

            //获取分页
            var bills = _costContractBillService.GetAllCostContractBills(curStore?.Id ?? 0,
                 curUser.Id,
                customerId,
                customerName,
                employeeId,
                billNumber,
                remark,
                model.StartTime, //?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")),
                model.EndTime, //?? DateTime.Now.AddDays(1),
                null,
                null,
                null,
                null,
                pageIndex:pagenumber,
                pageSize:30);

            //AccountingOptionName
            model.PagingFilteringContext.LoadPagedList(bills);

            #region 查询需要关联其他表的数据

            var allUsers = _userService.GetUsersDictsByIds(curStore.Id, bills.Select(b => b.EmployeeId).Distinct().ToArray());
            var allTerminals = _terminalService.GetTerminalsDictsByIds(curStore.Id, bills.Select(b => b.CustomerId).Distinct().ToArray());
            var alllAccounting = _accountingService.GetAccountingOptionsByIds(curStore.Id, bills.Select(b => b.AccountingOptionId).Distinct().ToArray());

            #endregion

            model.Items = bills.OrderByDescending(b => b.CreatedOnUtc).Select(b =>
              {
                  var m = b.ToModel<CostContractBillModel>();

                  var account = alllAccounting.Where(a => a.Id == m.AccountingOptionId).FirstOrDefault();

                  m.AccountingOptionName = account != null ? account.Name : "";
                  m.EmployeeName = allUsers.Where(u => u.Key == m.EmployeeId).Select(u => u.Value).FirstOrDefault();
                  m.CustomerName = allTerminals.Where(u => u.Key == m.CustomerId).Select(u => u.Value).FirstOrDefault();
                  m.AuditedStatusName = m.AuditedStatus ? "已审核" : "未审核";
                  return m;
              }).ToList();

            return View(model);
        }

        /// <summary>
        /// 添加费用合同
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.CostContractSave)]
        public IActionResult Create(int? store)
        {
            var model = new CostContractBillModel
            {
                BillTypeEnumId = (int)BillTypeEnum.CostContractBill,
                CreatedOnUtc = DateTime.Now,
                Year = DateTime.Now.Year,
                Month = DateTime.Now.Month,
                //员工
                Employees = BindUserSelection(_userService.BindUserList, curStore, ""),
                //暂时规定，员工为开单人
                EmployeeId = curUser == null ? 0 : curUser.Id,
                Leaders = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Administrators),
                LeaderId = null
            };

            //var accountungs = _accountingService.GetSubAccountingOptionsByAccountCodeTypeIds(curStore?.Id ?? 0, new[] { (int)AccountingCodeEnum.SaleFees, (int)AccountingCodeEnum.Preferential, (int)AccountingCodeEnum.CardFees, (int)AccountingCodeEnum.DisplayFees }, true);
            //if (accountungs != null)
            //{
            //    model.AccountingOptionSelects = accountungs.Select(s =>
            //    {
            //        var m = s.ToModel<AccountingOptionModel>();
            //        m.ParentName = _accountingService.GetAccountingOptionName(store, s.ParentId ?? 0);
            //        return m;
            //    }).ToList();
            //}

            return View(model);
        }

        /// <summary>
        /// 编辑费用合同
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.CostContractView)]
        public IActionResult Edit(int id = 0)
        {

            var model = new CostContractBillModel
            {
                BillTypeEnumId = (int)BillTypeEnum.CostContractBill
            };

            var costContractBill = _costContractBillService.GetCostContractBillById(curStore.Id, id, true);
            if (costContractBill == null)
            {
                return RedirectToAction("List");
            }

            if (costContractBill != null)
            {
                //只能操作当前经销商数据
                if (costContractBill.StoreId != curStore.Id)
                {
                    return RedirectToAction("List");
                }

                model = costContractBill.ToModel<CostContractBillModel>();
                model.BillBarCode = _mediaService.GenerateBarCodeForBase64(costContractBill.BillNumber, 150, 50);
                model.Items = costContractBill.Items.Select(c => c.ToModel<CostContractItemModel>()).ToList();
                model.AccountingOptionName = _accountingService.GetAccountingOptionName(curStore.Id, costContractBill.AccountingOptionId);
            }

            model.LeaderName = _userService.GetUserName(curStore.Id, costContractBill.LeaderId ?? 0);
            //主管
            model.Leaders = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Administrators);
            model.LeaderId = model.LeaderId == 0 ? null : model.LeaderId;

            //员工
            model.Employees = BindUserSelection(_userService.BindUserList, curStore, "");
            model.EmployeeId = model.EmployeeId == 0 ? null : model.EmployeeId;

            //客户
            model.CustomerName = _terminalService.GetTerminalName(curStore.Id, costContractBill.CustomerId);

            //制单人
            model.MakeUserName = _userService.GetUserName(curStore.Id, costContractBill.MakeUserId) + " " + costContractBill.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss");

            //审核人
            model.AuditedUserName = _userService.GetUserName(curStore.Id, costContractBill.AuditedUserId ?? 0) + " " + (costContractBill.AuditedDate.HasValue ? costContractBill.AuditedDate.Value.ToString("yyyy/MM/dd HH:mm:ss") : "");


            //var accountungs = _accountingService.GetSubAccountingOptionsByAccountCodeTypeIds(curStore?.Id ?? 0, new[] { (int)AccountingCodeEnum.SaleFees, (int)AccountingCodeEnum.Preferential, (int)AccountingCodeEnum.CardFees, (int)AccountingCodeEnum.DisplayFees }, true);
            //if (accountungs != null)
            //{
            //    model.AccountingOptionSelects = accountungs.Select(s =>
            //    {
            //        var m = s.ToModel<AccountingOptionModel>();
            //        m.ParentName = _accountingService.GetAccountingOptionName(curStore.Id, s.ParentId ?? 0);
            //        return m;
            //    }).ToList();
            //}

            return View(model);
        }

        public JsonResult AsyncSearchSelectPopup(int index, int? customerId, int? accountOptionId)
        {
            var model = new CostContractBillModel();

            model.RowIndex = index;
            model.CustomerId = customerId??0;
            model.AccountingOptionId = accountOptionId ?? 0;

            return Json(new { Success = true, RenderHtml = RenderPartialViewToString("AsyncSearch", model) });
        }

        public async Task<JsonResult> AsyncSearcCostContracts(int? customerId, int? accountOptionId, int pageIndex = 0, int pageSize = 10)
        {
            return await Task.Run(() => 
            {
                var model = new List<CostContractBindingModel>();

                var bills = _costContractBillService.GetAllCostContractBills(curStore?.Id ?? 0, curUser.Id, customerId ?? 0, accountOptionId: accountOptionId, 0, DateTime.Now.Year, 0, pageIndex: pageIndex, pageSize: pageSize).Where(b=>b.ContractType!=2).ToList();

                foreach (var bill in bills)
                {
                    var items = _costContractBillService.CalcCostContractBalances(curStore.Id, bill.CustomerId, bill).Where(c=>c.CType==1).ToList();

                    //客户名称
                    string customerName = _terminalService.GetTerminalName(curStore.Id, bill.CustomerId);

                    if (bill.ContractType == 0)
                    {
                        items.ForEach(i =>
                        {
                            if (i.Jan_Balance > 0)
                            {
                                var m = PrepareCostContractBindModel(bill, i, (int)i.Parse("Jan"), i.Jan, i.Jan_Balance);

                                //客户名称
                                m.CustomerName = customerName;

                                model.Add(m);
                            }
                            //2月
                            if (i.Feb_Balance > 0)
                            {
                                var m = PrepareCostContractBindModel(bill, i, (int)i.Parse("Feb"), i.Feb, i.Feb_Balance);

                                //客户名称
                                m.CustomerName = customerName;

                                model.Add(m);
                            }
                            //3月
                            if (i.Mar_Balance > 0)
                            {
                                var m = PrepareCostContractBindModel(bill, i, (int)i.Parse("Mar"), i.Mar, i.Mar_Balance);

                                //客户名称
                                m.CustomerName = customerName;

                                model.Add(m);
                            }
                            //4月
                            if (i.Apr_Balance > 0)
                            {
                                var m = PrepareCostContractBindModel(bill, i, (int)i.Parse("Apr"), i.Apr, i.Apr_Balance);

                                //客户名称
                                m.CustomerName = customerName;

                                model.Add(m);
                            }
                            //5月
                            if (i.May_Balance > 0)
                            {
                                var m = PrepareCostContractBindModel(bill, i, (int)i.Parse("May"), i.May, i.May_Balance);

                                //客户名称
                                m.CustomerName = customerName;

                                model.Add(m);
                            }
                            //6月
                            if (i.Jun_Balance > 0)
                            {
                                var m = PrepareCostContractBindModel(bill, i, (int)i.Parse("Jun"), i.Jun, i.Jun_Balance);

                                //客户名称
                                m.CustomerName = customerName;

                                model.Add(m);
                            }
                            //7月
                            if (i.Jul_Balance > 0)
                            {
                                var m = PrepareCostContractBindModel(bill, i, (int)i.Parse("Jul"), i.Jul, i.Jul_Balance);

                                //客户名称
                                m.CustomerName = customerName;

                                model.Add(m);
                            }
                            //8月
                            if (i.Aug_Balance > 0)
                            {
                                var m = PrepareCostContractBindModel(bill, i, (int)i.Parse("Aug"), i.Aug, i.Aug_Balance);

                                //客户名称
                                m.CustomerName = customerName;

                                model.Add(m);
                            }
                            //9月
                            if (i.Sep_Balance > 0)
                            {

                                var m = PrepareCostContractBindModel(bill, i, (int)i.Parse("Sep"), i.Sep, i.Sep_Balance);

                                //客户名称
                                m.CustomerName = customerName;

                                model.Add(m);
                            }
                            //10月
                            if (i.Oct_Balance > 0)
                            {

                                var m = PrepareCostContractBindModel(bill, i, (int)i.Parse("Oct"), i.Oct, i.Oct_Balance);

                                //客户名称
                                m.CustomerName = customerName;

                                model.Add(m);
                            }
                            //11月
                            if (i.Nov_Balance > 0)
                            {
                                var m = PrepareCostContractBindModel(bill, i, (int)i.Parse("Nov"), i.Nov, i.Nov_Balance);

                                //客户名称
                                m.CustomerName = customerName;

                                model.Add(m);
                            }
                            //12月
                            if (i.Dec_Balance > 0)
                            {

                                var m = PrepareCostContractBindModel(bill, i, (int)i.Parse("Dec"), i.Dec, i.Dec_Balance);

                                //客户名称
                                m.CustomerName = customerName;

                                model.Add(m);
                            }
                        });
                    }
                    else
                    {
                        items.ForEach(i =>
                        {
                            if (i.Total_Balance > 0)
                            {
                                var m = PrepareCostContractBindModel(bill, i, null, i.Total, i.Total_Balance);

                                //客户名称
                                m.CustomerName = customerName;

                                model.Add(m);
                            }
                        });
                    }
                }

                return Json(new 
                {
                    total = model.Count(),
                    rows = model
                });
            });
        }

        #region 单据项目

        /// <summary>
        /// 异步获取费用合同项目
        /// </summary>
        /// <param name="billId"></param>
        /// <returns></returns>
        public JsonResult AsyncCostContractItems(int billId)
        {
            var bill = _costContractBillService.GetCostContractBillById(curStore.Id, billId, true);
            //var gridModel = _costContractBillService.GetCostContractItemsByCostContractBillId(billId, curUser.Id, curStore.Id, 0, 30);
            var gridModel = _costContractBillService.CalcCostContractBalances(curStore.Id, bill.CustomerId, bill);

            var allProducts = _productService.GetProductsByIds(curStore.Id, gridModel.Select(pr => pr.ProductId).Distinct().ToArray());
            var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());
            var allProductPrices = _productService.GetProductPricesByProductIds(curStore.Id, gridModel.Select(gm => gm.Id).Distinct().ToArray());
            var allProductTierPrices = _productService.GetProductTierPriceByProductIds(curStore.Id, gridModel.Select(p => p.ProductId).Distinct().ToArray());

            var items = gridModel.Select(o =>
            {
                var m = o.ToModel<CostContractItemModel>();

                var product = allProducts.Where(ap => ap.Id == m.ProductId).FirstOrDefault();
                if (product != null)
                {
                    product.InitBaseModel<CostContractItemModel>(m, 0, allOptions, allProductPrices, allProductTierPrices, _productService);
                    m.SmallUnitName = m.smallOption?.Name;
                    m.Name = product.Name;
                }
                else
                {
                    m.SmallUnitName = "元";
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
        /// 创建/更新费用合同
        /// </summary>
        /// <param name="data"></param>
        /// <param name="billId"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthCode((int)AccessGranularityEnum.CostContractSave)]
        public async Task<JsonResult> CreateOrUpdate(CostContractUpdateModel data, int? billId,bool doAudit = true)
        {

            try
            {
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
                if (!string.IsNullOrWhiteSpace(data.ProtocolNum))
                {
                    data.ProtocolNum = data.ProtocolNum.ToUpper();
                    if (!Regex.IsMatch(data.ProtocolNum, @"^(PZC){1}[0-9]{11}$"))
                    {
                        return Warning("请填入正确TPM协议编码");
                    }
                }

                //验证从主管赠品扣除
                if (data.ContractType == (int)ContractTypeEnum.ManageGift)
                {
                    //var giveQuotaOption = _giveQuotaService.GetGiveQuotaOptionsByIds(int[] idArr)
                    if (data.Items != null && data.Items.Count > 0)
                    {
                        //var allGiveQuotaOption = _giveQuotaService.GetGiveQuotaOptionsByIds(data.Items.Select(di => di.GiveQuotaOptionId).Distinct().ToArray());
                        var allGiveQuotaOption = _giveQuotaService.GetGiveQuotaBalances(curStore.Id, data.Year, data.LeaderId); //获取主管的赠品及赠品额度余额(余额已根据最小单位转换计算)
                        var allProducts = _productService.GetProductsByIds(curStore.Id, data.Items.Select(di => di.ProductId).Distinct().ToArray());
                        var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());

                        foreach (var item in data.Items)
                        {
                            if (item.ProductId > 0)
                            {
                                if (allGiveQuotaOption.Count(c => c.ProductId == item.ProductId) == 0)
                                    return Warning("主管赠品中查无此商品!");

                                //转换成相同单位
                                var giveQuotaOption = allGiveQuotaOption.FirstOrDefault(ag => ag.ProductId == item.ProductId);

                                var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();

                                if (giveQuotaOption != null)
                                {
                                    //添加主管额度主表 Id
                                    item.GiveQuotaId = giveQuotaOption.GiveQuotaId;
                                    item.GiveQuotaOptionId = giveQuotaOption.Id;

                                    int quantity = 0;
                                    //具体到月
                                    switch (data.Month)
                                    {
                                        case 1:
                                            quantity = giveQuotaOption.Jan_Balance == null ? 0 : (int)giveQuotaOption.Jan_Balance;
                                            break;
                                        case 2:
                                            quantity = giveQuotaOption.Feb_Balance == null ? 0 : (int)giveQuotaOption.Feb_Balance;
                                            break;
                                        case 3:
                                            quantity = giveQuotaOption.Mar_Balance == null ? 0 : (int)giveQuotaOption.Mar_Balance;
                                            break;
                                        case 4:
                                            quantity = giveQuotaOption.Apr_Balance == null ? 0 : (int)giveQuotaOption.Apr_Balance;
                                            break;
                                        case 5:
                                            quantity = giveQuotaOption.May_Balance == null ? 0 : (int)giveQuotaOption.May_Balance;
                                            break;
                                        case 6:
                                            quantity = giveQuotaOption.Jun_Balance == null ? 0 : (int)giveQuotaOption.Jun_Balance;
                                            break;
                                        case 7:
                                            quantity = giveQuotaOption.Jul_Balance == null ? 0 : (int)giveQuotaOption.Jul_Balance;
                                            break;
                                        case 8:
                                            quantity = giveQuotaOption.Aug_Balance == null ? 0 : (int)giveQuotaOption.Aug_Balance;
                                            break;
                                        case 9:
                                            quantity = giveQuotaOption.Sep_Balance == null ? 0 : (int)giveQuotaOption.Sep_Balance;
                                            break;
                                        case 10:
                                            quantity = giveQuotaOption.Oct_Balance == null ? 0 : (int)giveQuotaOption.Oct_Balance;
                                            break;
                                        case 11:
                                            quantity = giveQuotaOption.Nov_Balance == null ? 0 : (int)giveQuotaOption.Nov_Balance;
                                            break;
                                        case 12:
                                            quantity = giveQuotaOption.Dec_Balance == null ? 0 : (int)giveQuotaOption.Dec_Balance;
                                            break;
                                        default:
                                            break;
                                    }

                                    //当前用户输入数量：注意防止用户复制明细，所以根据 GiveQuotaOptionId汇总
                                    List<CostContractItemModel> itemModels = data.Items.Where(di => di.ProductId == item.ProductId).ToList();
                                    if (itemModels != null && itemModels.Count > 0)
                                    {
                                        int thisQuantity = 0;
                                        foreach (var item2 in itemModels)
                                        {
                                            //商品转化量
                                            var conversionQuantity2 = product.GetConversionQuantity(allOptions, item.UnitId);
                                            int thisTotal = 0;
                                            thisTotal = item2.Total == null ? 0 : (int)item2.Total;
                                            //最小单位数量
                                            thisQuantity += thisTotal * conversionQuantity2;
                                        }

                                        if (thisQuantity > quantity)
                                        {
                                            return Warning("主管赠品余额不足!");
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        return Warning("费用合同没有明细");
                    }
                }

                //按月兑付时，判断当月是否已有合同
                if (data.ContractType == (int)ContractTypeEnum.ByMonth)
                {
                    if(!_costContractBillService.CheckContract(curStore.Id, data.Year, data.CustomerId, data.AccountingOptionId, data.Items.Select(it =>
                    {
                        return it.ToEntity<CostContractItem>();
                    }).ToList(), out string errMsg))
                        return Warning(errMsg);
                }

                //业务逻辑
                var accountings = _accountingService.GetAllAccountingOptions(curStore.Id, 0, true);
                var dataTo = data.ToEntity<CostContractBillUpdate>();
                dataTo.Operation = (int)OperationEnum.PC;
                dataTo.Items = data.Items.Select(it =>
                {
                    return it.ToEntity<CostContractItem>();
                }).ToList();

                //RedLock
                var result = await _locker.PerformActionWithLockAsync(LockKey(data),
                   TimeSpan.FromSeconds(30),
                   TimeSpan.FromSeconds(10),
                   TimeSpan.FromSeconds(1),
                   () => _costContractBillService.BillCreateOrUpdate(curStore.Id, curUser.Id, billId, accountings, dataTo, dataTo.Items, _userService.IsAdmin(curStore.Id, curUser.Id), doAudit));
                return Json(result);
            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("CreateOrUpdate", Resources.Bill_CreateOrUpdateFailed, curUser.Id);
                //_notificationService.SuccessNotification(Resources.Bill_CreateOrUpdateFailed);
                return Error(ex.Message);
            }

        }

        #endregion

        /// <summary>
        /// 审核费用合同
        /// </summary>
        /// <param name="billId"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthCode((int)AccessGranularityEnum.CostContractApproved)]
        public async Task<JsonResult> Auditing(int? id)
        {
            try
            {
                CostContractBill bill = new CostContractBill();

                #region 验证

                if (!id.HasValue)
                {
                    return Warning("参数错误.");
                }
                else
                {
                    bill = _costContractBillService.GetCostContractBillById(curStore.Id, id.Value, true);
                    if (bill.AuditedStatus)
                    {
                        return Warning("单据已审核，请刷新页面.");
                    }
                }

                //公共单据验证
                var commonBillChecking = BillChecking<CostContractBill, CostContractItem>(bill, BillStates.Audited);
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
                      () => _costContractBillService.Auditing(curStore.Id, curUser.Id, bill));
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
        /// 驳回
        /// </summary>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.CostContractReject)]
        public async Task<JsonResult> Rejected(int? id)
        {
            try
            {
                var bill = new CostContractBill();
                #region 验证

                if (!id.HasValue)
                {
                    return Warning("参数错误.");
                }
                else
                {
                    bill = _costContractBillService.GetCostContractBillById(curStore.Id, id.Value, true);
                    if (bill.RejectedStatus)
                    {
                        return Warning("单据已驳回，请刷新页面.");
                    }
                }

                //公共单据验证
                var commonBillChecking = BillChecking<CostContractBill, CostContractItem>(bill, BillStates.Reversed );
                if (commonBillChecking.Value != null)
                {
                    return commonBillChecking;
                }

                if (bill.AuditedStatus != true || bill.RejectedStatus != false || bill.AbandonedStatus != false)
                {
                    return Warning("重复操作.");
                }
                #endregion

                //RedLock

                var result = await _locker.PerformActionWithLockAsync(RedLockKey(bill),
                      TimeSpan.FromSeconds(30),
                      TimeSpan.FromSeconds(10),
                      TimeSpan.FromSeconds(1),
                      () => _costContractBillService.Rejected(curStore.Id, curUser.Id, bill));
                return Json(result);

            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("Auditing", "单据驳回失败", curUser.Id);
                _notificationService.SuccessNotification("单据驳回失败");
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// 终止
        /// </summary>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.CostContractReverse)]
        public async Task<JsonResult> Abandoned(int? id)
        {
            try
            {
                var bill = new CostContractBill();
                #region 验证
                if (!id.HasValue)
                {
                    return Warning("参数错误.");
                }
                else
                {
                    bill = _costContractBillService.GetCostContractBillById(curStore.Id, id.Value, true);
                }

                var commonBillChecking = BillChecking<CostContractBill, CostContractItem>(bill, BillStates.Reversed);
                if (commonBillChecking.Value != null)
                {
                    return commonBillChecking;
                }

                if (bill.AuditedStatus != true || bill.RejectedStatus != true || bill.AbandonedStatus != false)
                {
                    return Warning("重复操作.");
                }

                #endregion

                //RedLock
                var result = await _locker.PerformActionWithLockAsync(RedLockKey(bill),
                      TimeSpan.FromSeconds(30),
                      TimeSpan.FromSeconds(10),
                      TimeSpan.FromSeconds(1),
                      () => _costContractBillService.Auditing(curStore.Id, curUser.Id, bill));
                return Json(result);

            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("Auditing", "单据终止失败", curUser.Id);
                _notificationService.SuccessNotification("单据终止失败");
                return Error(ex.Message);
            }
        }

        //导出
        [AuthCode((int)AccessGranularityEnum.CostContractExport)]

        public FileResult Export(int type, string selectData, int? customerId, string customerName, int? employeeId, string billNumber = "", string remark = "", DateTime? startTime = null, DateTime? endTime = null)
        {

            #region 查询导出数据
            IList<CostContractBill> costContractBills = new List<CostContractBill>();

            if (type == 1)
            {
                if (!string.IsNullOrEmpty(selectData))
                {
                    List<string> ids = selectData.Split(',').ToList();
                    foreach (var id in ids)
                    {
                        CostContractBill costContractBill = _costContractBillService.GetCostContractBillById(curStore.Id, int.Parse(id), true);
                        if (costContractBill != null)
                        {
                            costContractBills.Add(costContractBill);
                        }
                    }
                }
            }
            else if (type == 2)
            {
                costContractBills = _costContractBillService.GetAllCostContractBills(
                                    curStore?.Id ?? 0,
                                     curUser.Id,
                                    customerId,
                                    customerName,
                                    employeeId,
                                    billNumber,
                                    remark,
                                    startTime,
                                    endTime, null, null, null);
            }

            #endregion

            #region 导出
            var ms = _exportManager.ExportCostContractBillToXlsx(costContractBills);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "费用合同单.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "费用合同单.xlsx");
            }
            #endregion

        }

        /// <summary>
        /// 异步获取所有可用合同的赠送商品 Campaign/AsyncAllGiveProducts?customerId=1
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public async Task<JsonResult> AsyncAllGiveProducts(string key, int customerId = 0, int businessUserId = 0, int wareHouseId = 0, int pageSize = 10, int pageIndex = 0)
        {

            return await Task.Run(() =>
            {

                //费用合同
                var costContracts = _costContractBillService.GetAvailableCostContracts(curStore?.Id ?? 0, customerId, businessUserId);

                //主管Ids
                List<int> leaderIds = costContracts.Select(cc => cc.LeaderId ?? 0).Distinct().ToList();
                var allLeaders = _userService.GetUsersByIds(curStore.Id, leaderIds.ToArray());

                var list = new List<CostContractItemModel>();

                IList<Product> allProducts = new List<Product>();
                IList<SpecificationAttributeOption> allOptions = new List<SpecificationAttributeOption>();
                IList<ProductPrice> allProductPrices = new List<ProductPrice>();
                IList<ProductTierPrice> allProductTierPrices = new List<ProductTierPrice>();

                if (costContracts != null && costContracts.Count > 0)
                {
                    // 获取涉及商品
                    List<int> productIds = new List<int>();
                    foreach (var costContract in costContracts)
                    {
                        if (costContract.Items != null && costContract.Items.Count > 0)
                        {
                            productIds.AddRange(costContract.Items.Select(cc => cc.ProductId).Distinct().ToList());
                        }
                    }

                    allProducts = _productService.GetProductsByIds(curStore.Id, productIds.Distinct().ToArray());
                    allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());
                    allProductPrices = _productService.GetProductPricesByProductIds(curStore.Id, productIds.Distinct().ToArray());
                    allProductTierPrices = _productService.GetProductTierPriceByProductIds(curStore.Id, productIds.Distinct().ToArray());

                    foreach (var costContract in costContracts)
                    {
                        if (costContract.Items != null && costContract.Items.Count > 0)
                        {
                            var items = _costContractBillService.CalcCostContractBalances(curStore.Id, costContract.CustomerId, costContract);
                            foreach (var item in items)
                            {
                                Product product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();

                                int smallUnitId= product?.GetProductUnit(_specificationAttributeService, _productService).smallOption.Id??0;
                                string smallUnitName = product?.GetProductUnit(_specificationAttributeService, _productService).smallOption.Name;

                                //按月兑付
                                if (costContract.ContractType == (int)ContractTypeEnum.ByMonth)
                                {
                                    if (item.CType == 0)
                                    {
                                        item.Name = product.Name;
                                        //1月
                                        if (item.Jan_Balance > 0)
                                        {
                                            var model= PrepareCostContractItemModel(item, (int)item.Parse("Jan"), costContract.ContractType ?? 0, smallUnitId, smallUnitName, item.Jan_Balance);

                                            list.Add(model);
                                        }
                                        //2月
                                        if (item.Feb_Balance > 0)
                                        {
                                            var model = PrepareCostContractItemModel(item, (int)item.Parse("Feb"), costContract.ContractType ?? 0, smallUnitId, smallUnitName, item.Feb_Balance);

                                            list.Add(model);
                                        }
                                        //3月
                                        if (item.Mar_Balance > 0)
                                        {
                                            var model = PrepareCostContractItemModel(item, (int)item.Parse("Mar"), costContract.ContractType ?? 0, smallUnitId, smallUnitName, item.Mar_Balance);

                                            list.Add(model);
                                        }
                                        //4月
                                        if (item.Apr_Balance > 0)
                                        {
                                            var model = PrepareCostContractItemModel(item, (int)item.Parse("Apr"), costContract.ContractType ?? 0, smallUnitId, smallUnitName, item.Apr_Balance);

                                            list.Add(model);
                                        }
                                        //5月
                                        if (item.May_Balance > 0)
                                        {
                                            var model = PrepareCostContractItemModel(item, (int)item.Parse("May"), costContract.ContractType ?? 0, smallUnitId, smallUnitName, item.May_Balance);

                                            list.Add(model);
                                        }
                                        //6月
                                        if (item.Jun_Balance > 0)
                                        {
                                            var model = PrepareCostContractItemModel(item, (int)item.Parse("Jun"), costContract.ContractType ?? 0, smallUnitId, smallUnitName, item.Jun_Balance);

                                            list.Add(model);
                                        }
                                        //7月
                                        if (item.Jul_Balance > 0)
                                        {
                                            var model = PrepareCostContractItemModel(item, (int)item.Parse("Jul"), costContract.ContractType ?? 0, smallUnitId, smallUnitName, item.Jul_Balance);

                                            list.Add(model);
                                        }
                                        //8月
                                        if (item.Aug_Balance > 0)
                                        {
                                            var model = PrepareCostContractItemModel(item, (int)item.Parse("Aug"), costContract.ContractType ?? 0, smallUnitId, smallUnitName, item.Aug_Balance);

                                            list.Add(model);
                                        }
                                        //9月
                                        if (item.Sep_Balance > 0)
                                        {
                                            var model = PrepareCostContractItemModel(item, (int)item.Parse("Sep"), costContract.ContractType ?? 0, smallUnitId, smallUnitName, item.Sep_Balance);

                                            list.Add(model);
                                        }
                                        //10月
                                        if (item.Oct_Balance > 0)
                                        {
                                            var model = PrepareCostContractItemModel(item, (int)item.Parse("Oct"), costContract.ContractType ?? 0, smallUnitId, smallUnitName, item.Oct_Balance);

                                            list.Add(model);
                                        }
                                        //11月
                                        if (item.Nov_Balance > 0)
                                        {
                                            var model = PrepareCostContractItemModel(item, (int)item.Parse("Nov"), costContract.ContractType ?? 0, smallUnitId, smallUnitName, item.Nov_Balance);

                                            list.Add(model);
                                        }
                                        //12月
                                        if (item.Dec_Balance > 0)
                                        {
                                            var model = PrepareCostContractItemModel(item, (int)item.Parse("Dec"), costContract.ContractType ?? 0, smallUnitId, smallUnitName, item.Dec_Balance);

                                            list.Add(model);
                                        }
                                    }
                                }
                                //按单位量总计兑付
                                else if (costContract.ContractType == (int)ContractTypeEnum.UnitQuantity)
                                {
                                    if (item.CType==0 && item.Total_Balance > 0)
                                    {
                                        item.Name = product.Name;
                                        var model = PrepareCostContractItemModel(item, null, costContract.ContractType ?? 0, smallUnitId, smallUnitName, item.Total_Balance);

                                        list.Add(model);
                                    }
                                }
                                //从主管赠品扣减
                                else if (costContract.ContractType == (int)ContractTypeEnum.ManageGift)
                                {

                                    if (item.Total_Balance > 0)
                                    {
                                        item.Name = product.Name;
                                        var model = PrepareCostContractItemModel(item, costContract.Month, costContract.ContractType ?? 0, smallUnitId, smallUnitName, item.Total_Balance);

                                        //主管名称
                                        var leader = allLeaders.Where(al => al.Id == costContract.LeaderId).FirstOrDefault();
                                        string leaderName = leader == null ? "" : leader.UserRealName;
                                        model.ContractTypeName += $" ({leaderName})";

                                        model.GiveQuotaId = item.GiveQuotaId;
                                        model.GiveQuotaOptionId = item.GiveQuotaOptionId;

                                        list.Add(model);
                                    }
                                }
                            }
                        }
                    }
                }

                //过滤条件
                if (!string.IsNullOrEmpty(key))
                {
                    list = list.Where(li => li.ProductName.Contains(key)).ToList();
                }

                var rows = new PagedList<CostContractItemModel>(list, pageIndex, pageSize);

                //增加商品属性,只查询当前页面的商品属性
                if (rows != null && rows.Count > 0)
                {
                    foreach (var row in rows)
                    {
                        Product product = allProducts.Where(ap => ap.Id == row.ProductId).FirstOrDefault();
                        if (product != null)
                        {
                            //row.Units = product.GetProductUnits(_productService, _specificationAttributeService);
                            row.Units = product.GetProductUnits(allOptions);
                            if (product.ProductPrices != null && product.ProductPrices.Count > 0)
                            {
                                //暂时批发价
                                ProductPrice productPrice = product.ProductPrices.Where(pp => pp.UnitId == row.UnitId).FirstOrDefault();
                                if (productPrice != null)
                                {
                                    row.Price = productPrice.TradePrice ?? 0;
                                    row.CostPrice = productPrice.CostPrice ?? 0;
                                }
                            }
                            //这里替换成高级用法
                            var p = product.ToModel<ProductModel>();
                            p = product.InitBaseModel<ProductModel>(p, wareHouseId, allOptions, allProductPrices, allProductTierPrices, _productService, customerId);

                            row.BigQuantity = product.BigQuantity;
                            row.StrokeQuantity = product.StrokeQuantity;
                            row.Prices = p.Prices;
                            row.ProductTierPrices = p.ProductTierPrices;
                            row.StockQuantities = p.StockQuantities;
                            row.UnitConversion = product.GetProductUnitConversion(allOptions);
                            //成本价
                            row.CostPrices = _purchaseBillService.GetReferenceCostPrice(product);

                        }

                    }
                }

                return Json(new
                {
                    total = list.Count,
                    rows
                });
            });
        }

        /// <summary>
        /// 获取业务员的直接上级，绑定主管
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        public JsonResult AsyncLoadDirLeader(int employeeId)
        {
            var user = _userService.GetUserById(employeeId);

            if(user.Dirtleader==0 || user.Dirtleader==user.Id) //没有直接上级
            {
                return Json(new
                {
                    Success = true,
                    Data = user
                });
            }
            else
            {
                return Json(new
                {
                    Success = true,
                    Data = _userService.GetUserById(user.Dirtleader ?? 0)
                });
            }
        }

        [NonAction]
        private CostContractItemModel PrepareCostContractItemModel(CostContractItem item, int? month, int contractType, int unitId, string unitName, decimal? monthBalance)
        {
            SaleProductTypeEnum saleProductTypeId = 0;
            ContractTypeEnum contractTypeEnum = 0;
            switch (contractType)
            {
                case (int)ContractTypeEnum.ByMonth:
                    saleProductTypeId = SaleProductTypeEnum.CostContractByMonth;
                    contractTypeEnum = ContractTypeEnum.ByMonth;
                    break;
                case (int)ContractTypeEnum.UnitQuantity:
                    saleProductTypeId = SaleProductTypeEnum.CostContractUnitQuantity;
                    contractTypeEnum = ContractTypeEnum.UnitQuantity;
                    break;
                case (int)ContractTypeEnum.ManageGift:
                    saleProductTypeId = SaleProductTypeEnum.CostContractManageGift;
                    contractTypeEnum = ContractTypeEnum.ManageGift;
                    break;
            }

            return new CostContractItemModel
            {
                Id = item.Id,
                CostContractBillId = item.CostContractBillId,
                ContractType = contractType,
                ContractTypeName = CommonHelper.GetEnumDescription(contractTypeEnum),
                CType = item.CType ?? 0,
                Name = item.Name,
                BigUnitId = item.BigUnitId,
                SmallUnitId = item.SmallUnitId,
                BigUnitQuantity = item.BigUnitQuantity,
                SmallUnitQuantity = item.SmallUnitQuantity,

                ProductId = item.ProductId,
                ProductName = item.Name,
                UnitId = unitId,
                UnitName = unitName,

                Month = month.HasValue ? month ?? 0 : 0,
                MonthName = month.HasValue ? $"{month ?? 0}月" : "",

                AvailableQuantityOrAmount = monthBalance ?? 0,

                SaleProductTypeId = (int)saleProductTypeId,
                SaleProductTypeName = CommonHelper.GetEnumDescription(saleProductTypeId),
                GiveTypeId = (int)GiveTypeEnum.Contract
            };
        }


        [NonAction]
        private CostContractBindingModel PrepareCostContractBindModel(CostContractBill bill, CostContractItem item, int? month, decimal? monthAmount, decimal? monthBalance)
        {

            return new CostContractBindingModel
            {
                Id = bill.Id,
                StoreId = bill.StoreId,
                BillNumber = bill.BillNumber,
                Year = bill.Year,
                Month = month.HasValue ? month ?? 0 : 0,
                MonthName = month.HasValue ? $"{month ?? 0}月" : "",
                Amount = monthAmount,
                Balance = monthBalance,
            };
        }

    }
}