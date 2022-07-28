using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Configuration;
using DCMS.Services.Common;
using DCMS.Services.Finances;
using DCMS.Services.Installation;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Settings;
using DCMS.ViewModel.Models.Configuration;
using DCMS.Web.Framework.Extensions;
using DCMS.Web.Framework.Mvc.Filters;
using DCMS.Web.Infrastructure.Mapper.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace DCMS.Web.Controllers.Setting
{

    /// <summary>
    /// 会计科目设置
    /// </summary>
    public class AccountingController : BasePublicController
    {
        private readonly IUserActivityService _userActivityService;
        private readonly IAccountingService _accountingService;
        
        private readonly IRecordingVoucherService _recordingVoucherService;
        private readonly IRedLocker _locker;
        private readonly ICommonBillService _commonBillService;
        private readonly IInstallationService _installationService;


        public AccountingController(IUserActivityService userActivityService,
            IWorkContext workContext,
            IStoreContext storeContext,
            IAccountingService accountingService,
            IStaticCacheManager cacheManager,
            IRecordingVoucherService recordingVoucherService,
            ILogger loggerService,
            INotificationService notificationService,
            IRedLocker locker,
            ICommonBillService commonBillService,
            IInstallationService installationService
            ) : base(workContext, loggerService, storeContext, notificationService)
        {
            _userActivityService = userActivityService;
            _accountingService = accountingService;
            
            _recordingVoucherService = recordingVoucherService;
            _locker = locker;
            _commonBillService = commonBillService;
            _installationService = installationService;
        }


        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        [AuthCode((int)AccessGranularityEnum.AccountingSubjectsSettingView)]
        public IActionResult Item()
        {
            return RedirectToAction("List");
        }

        [AuthCode((int)AccessGranularityEnum.AccountingSubjectsSettingView)]
        public IActionResult List(int typeid = 0)
        {
            var model = new AccountingTypeListModel();

            var accountings = _accountingService.GetAccountingTypes();
            model.AccountingTypes = accountings.Select(a =>
            {
                return a.ToModel<AccountingTypeModel>();
            }).ToList();

            model.AccountingTypeId = typeid != 0 ? typeid : 0;

            return View(model);
        }


        /// <summary>
        /// 异步获取列表
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public JsonResult AsyncGetList(int? store, int typeid = 0)
        {
            if (!store.HasValue)
            {
                store = curStore?.Id ?? 0;
            }
            var options = _accountingService.GetAccountingOptionsByAccountingType(store, typeid);
            //level
            return Json(new
            {
                total = options.Count(),
                rows = options.Select(m => m.ToModel<AccountingOptionModel>())
                .OrderBy(m => m.ParentId)
                .ThenBy(m => m.DisplayOrder).ToList()
            });
        }

        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.AccountingSubjectsSettingUpdate)]
        public JsonResult Create(int typeid = 0, int id = 0)
        {
            var model = new AccountingOptionModel
            {
                AccountCodeTypes = new SelectList(from a in Enum.GetValues(typeof(AccountingCodeEnum)).Cast<AccountingCodeEnum>()
                                                  select new SelectListItem
                                                  {
                                                      Text = CommonHelper.GetEnumDescription(a),
                                                      Value = ((int)a).ToString()
                                                  }, "Value", "Text"),
                //父级科目
                PartentAccounts = BindAccountSelection(new Func<int?, int, List<AccountingOption>>(_accountingService.GetAccountingOptionsByAccountingType), curStore, typeid)
            };

            //经销商
            var storeList = new List<SelectListItem>();
            model.StoreId = curStore?.Id ?? 0;
            //分级
            var curNode = _accountingService.GetAccountingOptionById(id);
            var childCount = _accountingService.ChildCount(curStore.Id, typeid, id);
            model.Code = (id == 0 ? (typeid * 10).ToString("00") : curNode.Code) + "" + (childCount + 1).ToString("00");

            model.ParentId = id == 0 ? 0 : curNode.Number;
            model.Enabled = true;
            model.IsDefault = false;
            model.InitBalance = curNode?.InitBalance;
            return Json(new
            {
                Success = true,
                RenderHtml = RenderPartialViewToString("Create", model)
            });
        }

        [HttpPost]
        [AuthCode((int)AccessGranularityEnum.AccountingSubjectsSettingUpdate)]
        public async Task<JsonResult> Create(IFormCollection form, int typeid = 0)
        {
            try
            {
                var Id = !string.IsNullOrEmpty(form["Id"]) ? form["Id"].ToString() : "0";
                var StoreId = !string.IsNullOrEmpty(form["StoreId"]) ? form["StoreId"].ToString() : "0";
                var Code = !string.IsNullOrEmpty(form["Code"]) ? form["Code"].ToString() : "0";
                var Name = !string.IsNullOrEmpty(form["Name"]) ? form["Name"].ToString() : "";
                var ParentId = !string.IsNullOrEmpty(form["ParentId"]) && form["ParentId"] != "-请选择父级科目-" ? form["ParentId"].ToString() : "0";
                var DisplayOrder = !string.IsNullOrEmpty(form["DisplayOrder"]) ? form["DisplayOrder"].ToString() : "0";
                var Enabled = !string.IsNullOrEmpty(form["Enabled"]) ? form["Enabled"].Contains("true").ToString() : "false";
                var IsDefault = !string.IsNullOrEmpty(form["IsDefault"]) ? form["IsDefault"].Contains("true").ToString() : "false";
                var InitBalance = !string.IsNullOrEmpty(form["InitBalance"]) ? form["InitBalance"].ToString() : "0"; 
                //分级
                var curNode = _accountingService.GetAccountingOptionById(curStore.Id, typeid, int.Parse(ParentId));
                var childCount = _accountingService.ChildCount(curStore.Id, typeid, int.Parse(ParentId));

                Code = ParentId == "0" ? Code : curNode.Code + "" + (childCount + 1).ToString("00");

                //判断code是否存在
                if (_accountingService.CodeExist(curStore.Id, Code))
                {
                    return Warning("科目代码已经存在，请刷新页面");
                }

                var data = new AccountingOption()
                {
                    AccountingTypeId = typeid,
                    StoreId = curStore?.Id ?? 0,
                    Code = Code,
                    Name = Name.Trim(),
                    ParentId = curNode.Number,
                    DisplayOrder = int.Parse(DisplayOrder),
                    Enabled = bool.Parse(Enabled),
                    IsDefault = bool.Parse(IsDefault),
                    IsLeaf = true,
                    IsCustom = true, //用户自定义会计科目
                    InitBalance = decimal.Parse(InitBalance)
                };

                //RedLock
                string lockKey = string.Format(DCMSCachingDefaults.RedisDataReSubmitKey, Request.GetUrl(), curStore.Id, curUser.Id, CommonHelper.MD5(JsonConvert.SerializeObject(form)));
                var result = await _locker.PerformActionWithLockAsync(lockKey,
                    TimeSpan.FromSeconds(30),
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromSeconds(1),
                    () => _accountingService.CreateOrUpdate(curStore.Id, curUser.Id, typeid, 0, data, null));

                _userActivityService.InsertActivity("InsertAccountingOption", "创建科目", curUser, curUser.Id);
                _notificationService.SuccessNotification("创建科目成功");

                return Successful("创建科目成功");

            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.AccountingSubjectsSettingUpdate)]
        public JsonResult Edit(int typeid = 0, int id = 0)
        {
            var model = new AccountingOptionModel();

            //分级
            var option = _accountingService.GetAccountingOptionById(id);
            if (option != null)
            {
                model = option.ToModel<AccountingOptionModel>();
            }

            if (_accountingService.GetAccountingOptionsByParentId(curStore.Id, id).Count <= 0)
            {
                model.IsEndPoint = true;
            }
            else
            {
                model.IsEndPoint = false;
            }

            //父级科目
            model.PartentAccounts = BindAccountSelection(new Func<int?, int, List<AccountingOption>>(_accountingService.GetAccountingOptionsByAccountingType), curStore, typeid);

            model.AccountCodeTypes = new SelectList(from a in Enum.GetValues(typeof(AccountingCodeEnum)).Cast<AccountingCodeEnum>()
                                                    select new SelectListItem
                                                    {
                                                        Text = CommonHelper.GetEnumDescription(a),
                                                        Value = ((int)a).ToString()
                                                    }, "Value", "Text");

            model.StoreId = curStore?.Id ?? 0;
            model.ParentId = option.ParentId;
            model.AccountCodeTypeId = option.AccountCodeTypeId;
            if (model.AccountCodeTypeId == 0)
            {
                model.AccountCodeTypeId = null;
            }
            //model.PartentAccounts.SelectedValue = option.ParentId;

            return Json(new
            {
                Success = true,
                RenderHtml = RenderPartialViewToString("Edit", model)
            });
        }

        [HttpPost]
        [AuthCode((int)AccessGranularityEnum.AccountingSubjectsSettingUpdate)]
        public async Task<JsonResult> Edit(IFormCollection form, int typeid = 0)
        {
            try
            {
                var Id = !string.IsNullOrEmpty(form["Id"]) ? form["Id"].ToString() : "0";
                var StoreId = !string.IsNullOrEmpty(form["StoreId"]) ? form["StoreId"].ToString() : "0";
                var Code = !string.IsNullOrEmpty(form["Code"]) ? form["Code"].ToString() : "0";
                var Name = !string.IsNullOrEmpty(form["Name"]) ? form["Name"].ToString() : "";
                var ParentId = !string.IsNullOrEmpty(form["ParentId"]) ? form["ParentId"].ToString() : "0";
                var DisplayOrder = !string.IsNullOrEmpty(form["DisplayOrder"]) ? form["DisplayOrder"].ToString() : "0";
                var Enabled = !string.IsNullOrEmpty(form["Enabled"]) ? form["Enabled"].Contains("true").ToString() : "false";
                var IsDefault = !string.IsNullOrEmpty(form["IsDefault"]) ? form["IsDefault"].Contains("true").ToString() : "false";
                var accountCodeType = !string.IsNullOrEmpty(form["AccountCodeTypeId"]) ? form["AccountCodeTypeId"].ToString() : "0";
                var InitBalance = !string.IsNullOrEmpty(form["InitBalance"]) ? form["InitBalance"].ToString() : "0";
                int.TryParse(accountCodeType, out int accountCodeTypeId);

                var option = _accountingService.GetAccountingOptionById(int.Parse(Id));

                if (option == null)
                {
                    return Warning("科目不存在，请刷新页面");
                }

                var data = new AccountingOption()
                {
                    AccountingTypeId = typeid,
                    StoreId = curStore?.Id ?? 0,
                    Code = Code,
                    Name = Name.Trim(),
                    ParentId = int.Parse(ParentId),
                    DisplayOrder = int.Parse(DisplayOrder),
                    Enabled = bool.Parse(Enabled),
                    IsDefault = bool.Parse(IsDefault),
                    AccountCodeTypeId = accountCodeTypeId,
                    InitBalance = decimal.Parse(InitBalance)
                };

                //RedLock
                string lockKey = string.Format(DCMSCachingDefaults.RedisDataReSubmitKey, Request.GetUrl(), curStore.Id, curUser.Id, CommonHelper.MD5(JsonConvert.SerializeObject(form)));
                var result = await _locker.PerformActionWithLockAsync(lockKey,
                    TimeSpan.FromSeconds(30),
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromSeconds(1),
                    () => _accountingService.CreateOrUpdate(curStore.Id, curUser.Id, typeid, option.Id, data, option));

                _userActivityService.InsertActivity("UpdateAccountingOption", "更新科目", curUser, curUser.Id);
                _notificationService.SuccessNotification("更新科目成功");

                return Successful("更新科目成功");
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [HttpPost]
        [AuthCode((int)AccessGranularityEnum.AccountingSubjectsSettingUpdate)]
        public JsonResult Delete(int[] selections, int id)
        {
            try
            {
                bool hasChilds = false;
                //foreach (var m in selections)
                //{
                //    var option = _accountingService.GetAccountingOptionById(m);
                //    if (option != null)
                //    {
                //        hasChilds = _accountingService.HasChilds(option);
                //        if (hasChilds)
                //        {
                //            break;
                //        }

                //        //_accountingService.DeleteAccountingOption(option);
                //        option.Enabled = false;
                //        _accountingService.UpdateAccountingOption(option);

                //    }
                //}

                var option = _accountingService.GetAccountingOptionById(id);
                if (option != null)
                {
                    hasChilds = _accountingService.HasChilds(curStore.Id, option);
                    if (!hasChilds)
                    {
                        option.Enabled = false;
                        _accountingService.UpdateAccountingOption(option);
                    }
                }
                else
                {
                    return Warning("删除失败，科目不存在");
                }

                if (!hasChilds)
                {
                    _userActivityService.InsertActivity("DeleteAccountingOption", "删除科目", curUser, curUser.Id);
                    _notificationService.SuccessNotification("科目删除成功");

                    return Successful("科目删除成功");
                }
                else
                {
                    return Warning("删除失败，存在子科目");
                }

            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// 异步获取会计科目列表
        /// </summary>
        /// <param name="key">搜索关键字</param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        public async Task<JsonResult> AsyncList(string accountingType, string key = "", int categoryId = 0, int pagenumber = 0)
        {

            int accountingTypeId = 1;

            return await Task.Run(() =>
            {
                if (pagenumber > 0)
                {
                    pagenumber -= 1;
                }

                var gridModel = _accountingService.GetAccountingOptionsByAccountingType(curStore.Id, accountingTypeId);
                //过滤科目类型
                if (!string.IsNullOrEmpty(accountingType) && int.TryParse(accountingType, out int iAccountingType))
                {
                    gridModel = gridModel.Where(a => a.AccountingTypeId == iAccountingType).ToList();
                }
                //过滤关键字
                if (!string.IsNullOrEmpty(key))
                {
                    gridModel = gridModel.Where(a => a.Name.Contains(key) || a.Code.Contains(key)).ToList();
                }

                return Json(new
                {
                    Success = true,
                    total = gridModel.Count,
                    rows = gridModel.Select(m =>
                    {
                        var p = m.ToModel<AccountingOptionModel>();
                        return p;
                    }).ToList()
                });
            });
        }

        /// <summary>
        /// 异步会计科目弹出选择的收款方式
        /// </summary>
        /// <param name="manufacturerId"></param>
        /// <param name="terminalId"></param>
        /// <param name="multi">是否支持多选，默认多选</param>
        /// <param name="billTypeId"></param>
        /// <returns></returns>
        public async Task<JsonResult> AsyncSearchSelectPopup(int manufacturerId = 0, int terminalId = 0, int multi = 1, int ifcheck = 0, int selectIndex=0,string PageTable=null, int billTypeId = 0, int self = 0)
        {
            //获取收款方式
            var codeTypeIds = new List<int>();
            switch ((BillTypeEnum)billTypeId)
            {
                case BillTypeEnum.SaleReservationBill:
                    //资产类：预收账款
                    //type.(int)AccountingEnum.Assets;
                    codeTypeIds.Add((int)AccountingCodeEnum.AdvanceReceipt);
                    break;
                case BillTypeEnum.SaleBill:
                    //资产类：库存现金,银行存款,其他账户,预收账款
                    codeTypeIds.Add((int)AccountingCodeEnum.HandCash);
                    codeTypeIds.Add((int)AccountingCodeEnum.BankDeposits);
                    codeTypeIds.Add((int)AccountingCodeEnum.OtherAccount);
                    codeTypeIds.Add((int)AccountingCodeEnum.AdvanceReceipt);
                    break;
                case BillTypeEnum.ReturnReservationBill:
                    //资产类：预收账款
                    codeTypeIds.Add((int)AccountingCodeEnum.AdvanceReceipt);
                    break;
                case BillTypeEnum.ReturnBill:
                    //资产类：库存现金,银行存款,其他账户,预收账款
                    codeTypeIds.Add((int)AccountingCodeEnum.HandCash);
                    codeTypeIds.Add((int)AccountingCodeEnum.BankDeposits);
                    codeTypeIds.Add((int)AccountingCodeEnum.OtherAccount);
                    codeTypeIds.Add((int)AccountingCodeEnum.AdvanceReceipt);
                    break;
                case BillTypeEnum.PurchaseReturnBill:
                    //资产类：库存现金,银行存款,其他账户,预付账款
                    codeTypeIds.Add((int)AccountingCodeEnum.HandCash);
                    codeTypeIds.Add((int)AccountingCodeEnum.BankDeposits);
                    codeTypeIds.Add((int)AccountingCodeEnum.OtherAccount);
                    codeTypeIds.Add((int)AccountingCodeEnum.AdvancePayment);
                    break;
                case BillTypeEnum.PurchaseBill:
                    //资产类：库存现金,银行存款,其他账户,预付账款
                    codeTypeIds.Add((int)AccountingCodeEnum.HandCash);
                    codeTypeIds.Add((int)AccountingCodeEnum.BankDeposits);
                    codeTypeIds.Add((int)AccountingCodeEnum.OtherAccount);
                    codeTypeIds.Add((int)AccountingCodeEnum.AdvancePayment);
                    break;
                case BillTypeEnum.CashReceiptBill:
                    //资产类：库存现金,银行存款,其他账户,预收账款
                    codeTypeIds.Add((int)AccountingCodeEnum.HandCash);
                    codeTypeIds.Add((int)AccountingCodeEnum.BankDeposits);
                    codeTypeIds.Add((int)AccountingCodeEnum.OtherAccount);
                    codeTypeIds.Add((int)AccountingCodeEnum.AdvanceReceipt);
                    break;
                case BillTypeEnum.PaymentReceiptBill:
                    //资产类：库存现金,银行存款,其他账户,预付账款
                    codeTypeIds.Add((int)AccountingCodeEnum.HandCash);
                    codeTypeIds.Add((int)AccountingCodeEnum.BankDeposits);
                    codeTypeIds.Add((int)AccountingCodeEnum.OtherAccount);
                    codeTypeIds.Add((int)AccountingCodeEnum.AdvancePayment);
                    break;
                case BillTypeEnum.AdvanceReceiptBill:
                    //资产类：库存现金,银行存款,其他账户
                    codeTypeIds.Add((int)AccountingCodeEnum.HandCash);
                    codeTypeIds.Add((int)AccountingCodeEnum.BankDeposits);
                    codeTypeIds.Add((int)AccountingCodeEnum.OtherAccount);
                    break;
                case BillTypeEnum.AdvancePaymentBill:
                    //资产类：库存现金,银行存款,其他账户
                    codeTypeIds.Add((int)AccountingCodeEnum.HandCash);
                    codeTypeIds.Add((int)AccountingCodeEnum.BankDeposits);
                    codeTypeIds.Add((int)AccountingCodeEnum.OtherAccount);
                    break;
                case BillTypeEnum.CostExpenditureBill:
                    //资产类：库存现金,银行存款,其他账户,
                    if (self == 1)
                    {
                        //费用支出
                        codeTypeIds.Add((int)AccountingCodeEnum.FinanceFees);
                        codeTypeIds.Add((int)AccountingCodeEnum.SaleFees);
                        codeTypeIds.Add((int)AccountingCodeEnum.ManageFees);
                    }
                    else
                    {
                        //资产类：库存现金,银行存款,其他账户,
                        codeTypeIds.Add((int)AccountingCodeEnum.HandCash);
                        codeTypeIds.Add((int)AccountingCodeEnum.BankDeposits);
                        codeTypeIds.Add((int)AccountingCodeEnum.OtherAccount);
                    }
                    break;
                case BillTypeEnum.CostContractBill:
                    //(费用类别)
                    //损益类（支出）：销售费用,管理费用,财务费用
                    codeTypeIds.Add((int)AccountingCodeEnum.SaleFees);
                    codeTypeIds.Add((int)AccountingCodeEnum.ManageFees);
                    codeTypeIds.Add((int)AccountingCodeEnum.FinanceFees);
                    break;
                case BillTypeEnum.FinancialIncomeBill:
                    //资产类：库存现金,银行存款,其他账户,
                    if (self == 1)
                    {
                        //其它业务收入
                        codeTypeIds.Add((int)AccountingCodeEnum.OtherIncome);
                    }
                    else
                    {
                        codeTypeIds.Add((int)AccountingCodeEnum.HandCash);
                        codeTypeIds.Add((int)AccountingCodeEnum.BankDeposits);
                        codeTypeIds.Add((int)AccountingCodeEnum.OtherAccount);
                    }
                    break;
                default:
                    break;
            }

            return await Task.Run(() =>
            {
                var model = new AccountingOptionListModel
                {
                    StoreId = curStore?.Id ?? 0,
                    AccountingTypeId = billTypeId,
                    ManufacturerId = manufacturerId,
                    TerminalId = terminalId,
                    Multi = multi,
                    Self = self,
                    ifcheck= ifcheck,
                    selectIndex= selectIndex,
                    PageTable= PageTable,
                    AccountCodeTypeIds = string.Join(",", codeTypeIds)
                };
                return Json(new { Success = true, RenderHtml = RenderPartialViewToString("AsyncSearch", model) });
            });
        }



        /// <summary>
        /// 异步获取支付科目
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="typeId"></param>
        /// <param name="manufacturerId"></param>
        /// <param name="terminalId"></param>
        /// <param name="accountCodeTypeIds"></param>
        /// <returns></returns>
        public async Task<JsonResult> AsyncAccountingOptions(int? storeId, int? typeId, string accountCodeTypeIds, int manufacturerId = 0, int terminalId = 0, int multi = 1, int self = 0)
        {
            if (!storeId.HasValue)
            {
                storeId = curStore?.Id ?? 0;
            }

            int[] ids = null;
            if (!string.IsNullOrEmpty(accountCodeTypeIds))
            {
                ids = accountCodeTypeIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(u => int.Parse(u)).ToArray();
            }

            return await Task.Run(() =>
            {
                var options = _accountingService.GetAllAccounts(storeId, typeId, ids).ToList();

                var numbers = options.Where(s => ids?.Contains(s.AccountCodeTypeId ?? 0) ?? true).Select(s => s.Number).ToList();
                return Json(new
                {
                    total = options.Count(),
                    rows = options
                    .Where(m => (numbers?.Contains(m.ParentId ?? 0) ?? true) || (numbers?.Contains(m.Number) ?? true))
                    .OrderBy(m => m.ParentId)
                    .ThenBy(m => m.DisplayOrder)
                    .Select(m =>
                    {
                        var p = m.ToModel<AccountingOptionModel>();
                        //预付款
                        if (m.AccountCodeTypeId == (int)AccountingCodeEnum.AdvancePayment)
                        {
                            if (manufacturerId > 0)
                            {
                                p.Balance = _commonBillService.CalcManufacturerBalance(curStore?.Id ?? 0, manufacturerId).AdvanceAmountBalance;
                            }
                        }
                        //预收款
                        /* else if (m.AccountCodeTypeId == (int)AccountingCodeEnum.AdvanceReceipt || m.AccountCodeTypeId== (int)*/
                        else if (m.AccountCodeTypeId == (int)AccountingCodeEnum.AdvanceReceipt)
                        {
                            if (terminalId > 0)
                            {
                                p.Balance = _commonBillService.CalcTerminalBalance(curStore?.Id ?? 0, terminalId).AdvanceAmountBalance;
                            }
                        }

                        return p;
                    }).ToList()
                });
            });

        }

        [HttpGet]
        public IActionResult InitOption()
        {
            _installationService.InstallAccounting(curStore.Id);

            return RedirectToAction("List");
        }
    }
}