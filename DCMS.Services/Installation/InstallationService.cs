using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Data;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Plan;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.Security;
using DCMS.Core.Domain.Stores;
using DCMS.Core.Domain.Terminals;
using DCMS.Core.Domain.Users;
using DCMS.Core.Domain.WareHouses;
using DCMS.Core.Infrastructure;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Common;
using DCMS.Services.Configuration;
using DCMS.Services.Events;
using DCMS.Services.Security;
using DCMS.Services.Settings;
using DCMS.Services.Stores;
using DCMS.Services.Users;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DCMS.Services.Installation
{
    /// <summary>
    /// 用于经销商（租户）快速初始化安装
    /// </summary>
    public partial class InstallationService : BaseService, IInstallationService
    {
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IDCMSFileProvider _fileProvider;
        private readonly IWebHelper _webHelper;

        public InstallationService(
          IStaticCacheManager cacheManager,
          IServiceGetter getter,
          IGenericAttributeService genericAttributeService,
          IDCMSFileProvider fileProvider,
          IWebHelper webHelper,
          IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {
            _genericAttributeService = genericAttributeService;
            _fileProvider = fileProvider;
            _webHelper = webHelper;
        }

        /// <summary>
        /// 安装初始经销商（租户）
        /// </summary>
        /// <param name="storeName"></param>
        public virtual ProcessStatus InstallStores(string storeName, out int storeId)
        {
            storeId = 0;

            try
            {


                return new ProcessStatus { Result = true };
            }
            catch (Exception ex)
            {
                var message = string.Empty;
                for (var inner = ex; inner != null; inner = inner.InnerException)
                {
                    message = $"{message}{inner.Message}{Environment.NewLine}";
                }

                return new ProcessStatus { Result = false, Errors = message };
            }
        }

        /// <summary>
        /// 安装用户并初始角色权限
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="defaultUserName"></param>
        /// <param name="defaultUserEmail"></param>
        /// <param name="defaultUserPassword"></param>
        /// <param name="defaultMobileNumber"></param>
        /// <returns></returns>
        public virtual ProcessStatus InstallUsers(int storeId, string defaultUserName, string defaultUserEmail, string defaultUserPassword, string defaultMobileNumber)
        {
            if (storeId == 0)
            {
                return new ProcessStatus { Result = false, Errors = "指定经销商不存在" };
            }

            try
            {

                var crAdministrators = new UserRole
                {
                    StoreId = storeId,
                    Name = "超级管理员",
                    Active = true,
                    IsSystemRole = true,
                    Description = "超级管理员",
                    SystemName = DCMSDefaults.Administrators
                };

                var crMarketManagers = new UserRole
                {
                    StoreId = storeId,
                    Name = "营销总经理",
                    Active = true,
                    IsSystemRole = true,
                    Description = "营销总经理",
                    SystemName = DCMSDefaults.MarketManagers
                };

                var crBusinessManagers = new UserRole
                {
                    StoreId = storeId,
                    Name = "业务部经理",
                    Active = true,
                    IsSystemRole = true,
                    Description = "业务部经理",
                    SystemName = DCMSDefaults.BusinessManagers
                };

                var crSalesmans = new UserRole
                {
                    StoreId = storeId,
                    Name = "业务员",
                    Active = true,
                    IsSystemRole = true,
                    Description = "业务员",
                    SystemName = DCMSDefaults.Salesmans
                };

                var crEmployees = new UserRole
                {
                    StoreId = storeId,
                    Name = "员工",
                    Active = true,
                    IsSystemRole = true,
                    Description = "员工",
                    SystemName = DCMSDefaults.Employees
                };

                var crDelivers = new UserRole
                {
                    StoreId = storeId,
                    Name = "配送员",
                    Active = true,
                    IsSystemRole = true,
                    Description = "配送员",
                    SystemName = DCMSDefaults.Delivers
                };

                var crRegisteredRoleName = new UserRole
                {
                    StoreId = storeId,
                    Name = "注册用户",
                    Active = true,
                    IsSystemRole = true,
                    Description = "注册用户",
                    SystemName = DCMSDefaults.RegisteredRoleName
                };

                //注册角色
                var userRoles = new List<UserRole>
            {
                //crAdministrators,
                crMarketManagers,
                crBusinessManagers,
                //crSalesmans,
                crEmployees,
                //crDelivers,
                crRegisteredRoleName
            };
                var uow = UserRepository.UnitOfWork;
                UserRoleRepository.Insert(userRoles);


                //注册管理员
                var adminUser = new User
                {
                    StoreId = storeId,
                    UserGuid = Guid.NewGuid().ToString(),
                    Email = defaultUserEmail,
                    Username = defaultUserName,
                    Active = true,
                    CreatedOnUtc = DateTime.UtcNow,
                    LastActivityDateUtc = DateTime.UtcNow,
                    UserRealName = defaultUserName,
                    MobileNumber = defaultMobileNumber,
                    AdminComment = "",
                    IsTaxExempt = false,
                    ActivationTime = DateTime.Now,
                    Deleted = false,
                    IsSystemAccount = true, //保证是系统账户
                    SystemName = "",
                    IsPlatformCreate = true, //保证平台创建
                    LastIpAddress = "",
                    LastLoginDateUtc = DateTime.UtcNow,
                    BranchCode = 0,
                    BranchId = 0,
                    SalesmanExtractPlanId = 0,
                    DeliverExtractPlanId = 0,
                    MaxAmountOfArrears = 0,
                    FaceImage = "",
                    AppId = "",
                    RequireReLogin = false
                };

                UserRepository.Insert(adminUser);
                uow.SaveChanges();

                var userService = EngineContext.Current.Resolve<IUserService>();
                var newRoles = UserRoleRepository.TableNoTracking.Where(c => c.StoreId == storeId).ToList();
                //给管理员赋角色
                newRoles.ForEach(r =>
                {
                    if (r.SystemName == crAdministrators.SystemName || r.SystemName == crEmployees.SystemName || r.SystemName == crRegisteredRoleName.SystemName)
                    {
                        userService.InsertUserRoleMapping(adminUser.Id, r.Id, storeId);
                    }
                });

                //设置默认用户名
                _genericAttributeService.SaveAttribute(adminUser, DCMSDefaults.RealNameAttribute, defaultUserName);

                //设置密码
                var customerRegistrationService = EngineContext.Current.Resolve<IUserRegistrationService>();
                customerRegistrationService.ChangePassword(new ChangePasswordRequest(defaultUserEmail, false, PasswordFormat.Hashed, defaultUserPassword, null));

                return new ProcessStatus { Result = true };
            }
            catch (Exception ex)
            {
                var message = string.Empty;
                for (var inner = ex; inner != null; inner = inner.InnerException)
                {
                    message = $"{message}{inner.Message}{Environment.NewLine}";
                }

                return new ProcessStatus { Result = false, Errors = message };
            }
        }

        /// <summary>
        /// 安装初始标准权限
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public virtual ProcessStatus InstallStandardPermissions(int storeId)
        {
            if (storeId == 0)
            {
                return new ProcessStatus { Result = false, Errors = "指定经销商不存在" };
            }

            try
            {
                var permissionService = EngineContext.Current.Resolve<IPermissionService>();
                if (permissionService == null)
                {
                    throw new Exception("不能解析服务IPermissionService");
                }

                //获取全部平台创建的权限记录
                var allPermissionRecords = permissionService.GetAllPermissionRecordsByStore(0);

                //获取管理员角色
                var adminRole = UserRoleRepository.TableNoTracking.Where(s => s.StoreId == storeId && s.SystemName == "Administrators").FirstOrDefault();

                //注册管理员默认权限
                //一般管理员拥有这4项权力就可以访问并配置管理其它
                var p1 = StandardPermissionProvider.PublicStoreAllowNavigation;
                var p2 = StandardPermissionProvider.AccessClosedStore;
                var p3 = StandardPermissionProvider.UserRoleView;
                var p4 = StandardPermissionProvider.UserRoleAdd;

                var prds = allPermissionRecords.Where(s => new string[] { p1.SystemName, p2.SystemName, p3.SystemName, p4.SystemName }.Contains(s.SystemName)).ToList();

                var uow = PermissionRecordRolesMappingRepository.UnitOfWork;
                prds.ForEach(pr =>
                {
                    //添加映射
                    var prr = new PermissionRecordRoles()
                    {
                        StoreId = storeId,
                        PermissionRecord_Id = pr.Id,
                        UserRole_Id = adminRole.Id,
                        //这里只开启PC端映射，APP 端需要稍后登录平台配置
                        Platform = 0,
                    };

                    PermissionRecordRolesMappingRepository.Insert(prr);
                });
                uow.SaveChanges();

                return new ProcessStatus { Result = true };
            }
            catch (Exception ex)
            {
                var message = string.Empty;
                for (var inner = ex; inner != null; inner = inner.InnerException)
                {
                    message = $"{message}{inner.Message}{Environment.NewLine}";
                }

                return new ProcessStatus { Result = false, Errors = message };
            }
        }

        /// <summary>
        /// 安装公司配置初始(CompanySetting.csv)
        /// </summary>
        public virtual ProcessStatus InstallCompanySettings(Store store, string path)
        {
            if (store.Id == 0)
            {
                return new ProcessStatus { Result = false, Errors = "指定经销商不存在" };
            }

            try
            {
                //TODO...
                var _settingService = EngineContext.Current.Resolve<ISettingService>();
                if (_settingService == null)
                {
                    throw new Exception("不能解析服务ISettingService");
                }

                path = AppContext.BaseDirectory + "InitData\\CompanySetting.csv";
                FileInfo myFile = new FileInfo(path);
                StreamReader sW5 = myFile.OpenText();
                var result = sW5.ReadToEnd();
                sW5.Close();

                var newSetting = JsonConvert.DeserializeObject<CompanySetting>(result);

                if (store.Setuped)
                {
                    var companySetting = _settingService.LoadSetting<CompanySetting>(store.Id);

                    if (companySetting != null)
                    {
                        companySetting.OpenBillMakeDate = newSetting.OpenBillMakeDate;
                        companySetting.MulProductPriceUnit = newSetting.MulProductPriceUnit;
                        companySetting.AllowCreateMulSameBarcode = newSetting.AllowCreateMulSameBarcode;
                        companySetting.DefaultPricePlan = newSetting.DefaultPricePlan;
                        companySetting.DefaultPurchasePrice = newSetting.DefaultPurchasePrice;
                        companySetting.VariablePriceCommodity = newSetting.VariablePriceCommodity;
                        companySetting.AccuracyRounding = newSetting.AccuracyRounding;
                        companySetting.MakeBillDisplayBarCode = newSetting.MakeBillDisplayBarCode;
                        companySetting.AllowSelectionDateRange = newSetting.AllowSelectionDateRange;
                        companySetting.DockingTicketPassSystem = newSetting.DockingTicketPassSystem;
                        companySetting.AllowReturnInSalesAndOrders = newSetting.AllowReturnInSalesAndOrders;
                        companySetting.AppMaybeDeliveryPersonnel = newSetting.AppMaybeDeliveryPersonnel;
                        companySetting.AppSubmitOrderAutoAudits = newSetting.AppSubmitOrderAutoAudits;
                        companySetting.AppSubmitTransferAutoAudits = newSetting.AppSubmitTransferAutoAudits;
                        companySetting.AppSubmitExpenseAutoAudits = newSetting.AppSubmitExpenseAutoAudits;
                        companySetting.AppSubmitBillReturnAutoAudits = newSetting.AppSubmitBillReturnAutoAudits;
                        companySetting.AppAllowWriteBack = newSetting.AppAllowWriteBack;
                        companySetting.AllowAdvancePaymentsNegative = newSetting.AllowAdvancePaymentsNegative;
                        companySetting.ShowOnlyPrepaidAccountsWithPrepaidReceipts = newSetting.ShowOnlyPrepaidAccountsWithPrepaidReceipts;
                        companySetting.TasteByTasteAccountingOnlyPrintMainProduct = newSetting.TasteByTasteAccountingOnlyPrintMainProduct;
                        companySetting.APPOnlyShowHasStockProduct = newSetting.APPOnlyShowHasStockProduct;
                        companySetting.APPShowOrderStock = newSetting.APPShowOrderStock;
                        companySetting.SalesmanDeliveryDistance = newSetting.SalesmanDeliveryDistance;
                        companySetting.OnStoreStopSeconds = newSetting.OnStoreStopSeconds;
                        companySetting.EnableSalesmanTrack = newSetting.EnableSalesmanTrack;

                        companySetting.DoorheadPhotoNum = newSetting.DoorheadPhotoNum;
                        companySetting.DisplayPhotoNum = newSetting.DisplayPhotoNum;

                        companySetting.Start = newSetting.Start;
                        companySetting.End = newSetting.End;
                        companySetting.FrequencyTimer = newSetting.FrequencyTimer;
                        companySetting.SalesmanOnlySeeHisCustomer = newSetting.SalesmanOnlySeeHisCustomer;
                        companySetting.SalesmanVisitStoreBefore = newSetting.SalesmanVisitStoreBefore;
                        companySetting.SalesmanVisitMustPhotographed = newSetting.SalesmanVisitMustPhotographed;
                        companySetting.ReferenceCostPrice = newSetting.ReferenceCostPrice;
                        companySetting.AveragePurchasePriceCalcNumber = newSetting.AveragePurchasePriceCalcNumber;
                        companySetting.AllowNegativeInventoryMonthlyClosure = newSetting.AllowNegativeInventoryMonthlyClosure;
                        companySetting.EnableTaxRate = newSetting.EnableTaxRate;
                        companySetting.TaxRate = newSetting.TaxRate;
                        companySetting.PhotographedWater = newSetting.PhotographedWater;
                        companySetting.ClearArchiveDatas = newSetting.ClearArchiveDatas;
                        companySetting.ClearBillDatas = newSetting.ClearBillDatas;

                        companySetting.EnableBusinessTime = newSetting.EnableBusinessTime;
                        companySetting.BusinessStart = newSetting.BusinessStart;
                        companySetting.BusinessEnd = newSetting.BusinessEnd;
                        companySetting.EnableBusinessVisitLine = newSetting.EnableBusinessVisitLine;

                        _settingService.SaveSetting(companySetting, x => x.OpenBillMakeDate, store.Id, false);
                        _settingService.SaveSetting(companySetting, x => x.MulProductPriceUnit, store.Id, false);
                        _settingService.SaveSetting(companySetting, x => x.AllowCreateMulSameBarcode, store.Id, false);
                        _settingService.SaveSetting(companySetting, x => x.DefaultPricePlan, store.Id, false);
                        _settingService.SaveSetting(companySetting, x => x.DefaultPurchasePrice, store.Id, false);
                        _settingService.SaveSetting(companySetting, x => x.VariablePriceCommodity, store.Id, false);
                        _settingService.SaveSetting(companySetting, x => x.AccuracyRounding, store.Id, false);
                        _settingService.SaveSetting(companySetting, x => x.MakeBillDisplayBarCode, store.Id, false);
                        _settingService.SaveSetting(companySetting, x => x.AllowSelectionDateRange, store.Id, false);
                        _settingService.SaveSetting(companySetting, x => x.DockingTicketPassSystem, store.Id, false);
                        _settingService.SaveSetting(companySetting, x => x.AllowReturnInSalesAndOrders, store.Id, false);
                        _settingService.SaveSetting(companySetting, x => x.AppMaybeDeliveryPersonnel, store.Id, false);
                        _settingService.SaveSetting(companySetting, x => x.AppSubmitOrderAutoAudits, store.Id, false);
                        _settingService.SaveSetting(companySetting, x => x.AppSubmitTransferAutoAudits, store.Id, false);
                        _settingService.SaveSetting(companySetting, x => x.AppSubmitExpenseAutoAudits, store.Id, false);
                        _settingService.SaveSetting(companySetting, x => x.AppSubmitBillReturnAutoAudits, store.Id, false);
                        _settingService.SaveSetting(companySetting, x => x.AppAllowWriteBack, store.Id, false);
                        _settingService.SaveSetting(companySetting, x => x.AllowAdvancePaymentsNegative, store.Id, false);
                        _settingService.SaveSetting(companySetting, x => x.ShowOnlyPrepaidAccountsWithPrepaidReceipts, store.Id, false);
                        _settingService.SaveSetting(companySetting, x => x.TasteByTasteAccountingOnlyPrintMainProduct, store.Id, false);
                        _settingService.SaveSetting(companySetting, x => x.APPOnlyShowHasStockProduct, store.Id, false);
                        _settingService.SaveSetting(companySetting, x => x.APPShowOrderStock, store.Id, false);
                        _settingService.SaveSetting(companySetting, x => x.SalesmanDeliveryDistance, store.Id, false);
                        _settingService.SaveSetting(companySetting, x => x.OnStoreStopSeconds, store.Id, false);
                        _settingService.SaveSetting(companySetting, x => x.EnableSalesmanTrack, store.Id, false);
                        _settingService.SaveSetting(companySetting, x => x.Start, store.Id, false);
                        _settingService.SaveSetting(companySetting, x => x.End, store.Id, false);
                        _settingService.SaveSetting(companySetting, x => x.FrequencyTimer, store.Id, false);
                        _settingService.SaveSetting(companySetting, x => x.SalesmanOnlySeeHisCustomer, store.Id, false);
                        _settingService.SaveSetting(companySetting, x => x.SalesmanVisitStoreBefore, store.Id, false);
                        _settingService.SaveSetting(companySetting, x => x.SalesmanVisitMustPhotographed, store.Id, false);
                        _settingService.SaveSetting(companySetting, x => x.ReferenceCostPrice, store.Id, false);
                        _settingService.SaveSetting(companySetting, x => x.AveragePurchasePriceCalcNumber, store.Id, false);
                        _settingService.SaveSetting(companySetting, x => x.AllowNegativeInventoryMonthlyClosure, store.Id, false);
                        _settingService.SaveSetting(companySetting, x => x.EnableTaxRate, store.Id, false);
                        _settingService.SaveSetting(companySetting, x => x.TaxRate, store.Id, false);
                        _settingService.SaveSetting(companySetting, x => x.PhotographedWater, store.Id, false);
                        _settingService.SaveSetting(companySetting, x => x.ClearArchiveDatas, store.Id, false);
                        _settingService.SaveSetting(companySetting, x => x.ClearBillDatas, store.Id, false);

                        _settingService.SaveSetting(companySetting, x => x.EnableBusinessTime, store.Id, false);
                        _settingService.SaveSetting(companySetting, x => x.BusinessStart, store.Id, false);
                        _settingService.SaveSetting(companySetting, x => x.BusinessEnd, store.Id, false);
                        _settingService.SaveSetting(companySetting, x => x.EnableBusinessVisitLine, store.Id, false);
                    }

                }
                else
                {
                    _settingService.SaveSetting(newSetting, store.Id);

                    store.Setuped = true;
                    EngineContext.Current.Resolve<IStoreService>().UpdateStore(store);
                }

                return new ProcessStatus { Result = true };
            }
            catch (Exception ex)
            {
                var message = string.Empty;
                for (var inner = ex; inner != null; inner = inner.InnerException)
                {
                    message = $"{message}{inner.Message}{Environment.NewLine}";
                }

                return new ProcessStatus { Result = false, Errors = message };
            }
        }

        /// <summary>
        /// 安装APP打印初始 AppPrintSetting.csv
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public virtual ProcessStatus InstallAppPrintSettings(Store store, string path)
        {
            if (store.Id == 0)
            {
                return new ProcessStatus { Result = false, Errors = "指定经销商不存在" };
            }

            try
            {
                //TODO...
                var _settingService = EngineContext.Current.Resolve<ISettingService>();
                if (_settingService == null)
                {
                    throw new Exception("不能解析服务ISettingService");
                }

                path = AppContext.BaseDirectory + "InitData\\AppPrintSetting.csv";
                FileInfo myFile = new FileInfo(path);
                using (StreamReader sW5 = myFile.OpenText())
                {
                    var result = sW5.ReadToEnd();
                    var newSetting = JsonConvert.DeserializeObject<APPPrintSetting>(result);
                    if (store.Setuped)
                    {
                        var appPrintSetting = _settingService.LoadSetting<APPPrintSetting>(store.Id);

                        if (appPrintSetting != null)
                        {
                            appPrintSetting.AllowPrintPackPrice = newSetting.AllowPrintPackPrice;
                            appPrintSetting.PrintMode = newSetting.PrintMode;
                            appPrintSetting.PrintingNumber = newSetting.PrintingNumber;
                            appPrintSetting.AllowAutoPrintSalesAndReturn = newSetting.AllowAutoPrintSalesAndReturn;
                            appPrintSetting.AllowAutoPrintOrderAndReturn = newSetting.AllowAutoPrintOrderAndReturn;
                            appPrintSetting.AllowAutoPrintAdvanceReceipt = newSetting.AllowAutoPrintAdvanceReceipt;
                            appPrintSetting.AllowAutoPrintArrears = newSetting.AllowAutoPrintArrears;
                            appPrintSetting.AllowPrintOnePass = newSetting.AllowPrintOnePass;
                            appPrintSetting.AllowPringMobile = newSetting.AllowPringMobile;
                            appPrintSetting.AllowPrintingTimeAndNumber = newSetting.AllowPrintingTimeAndNumber;
                            appPrintSetting.AllowPrintCustomerBalance = newSetting.AllowPrintCustomerBalance;
                            appPrintSetting.PageHeaderText = newSetting.PageHeaderText;
                            appPrintSetting.PageFooterText1 = newSetting.PageFooterText1;
                            appPrintSetting.PageFooterText2 = newSetting.PageFooterText2;
                            appPrintSetting.PageHeaderImage = newSetting.PageHeaderImage;
                            appPrintSetting.PageFooterImage = newSetting.PageFooterImage;

                            _settingService.SaveSetting(appPrintSetting, x => x.AllowPrintPackPrice, store.Id, false);
                            _settingService.SaveSetting(appPrintSetting, x => x.PrintMode, store.Id, false);
                            _settingService.SaveSetting(appPrintSetting, x => x.PrintingNumber, store.Id, false);
                            _settingService.SaveSetting(appPrintSetting, x => x.AllowAutoPrintSalesAndReturn, store.Id, false);
                            _settingService.SaveSetting(appPrintSetting, x => x.AllowAutoPrintOrderAndReturn, store.Id, false);
                            _settingService.SaveSetting(appPrintSetting, x => x.AllowAutoPrintAdvanceReceipt, store.Id, false);
                            _settingService.SaveSetting(appPrintSetting, x => x.AllowAutoPrintArrears, store.Id, false);
                            _settingService.SaveSetting(appPrintSetting, x => x.AllowPrintOnePass, store.Id, false);
                            _settingService.SaveSetting(appPrintSetting, x => x.AllowPringMobile, store.Id, false);
                            _settingService.SaveSetting(appPrintSetting, x => x.AllowPrintingTimeAndNumber, store.Id, false);
                            _settingService.SaveSetting(appPrintSetting, x => x.AllowPrintCustomerBalance, store.Id, false);
                            _settingService.SaveSetting(appPrintSetting, x => x.PageHeaderText, store.Id, false);
                            _settingService.SaveSetting(appPrintSetting, x => x.PageFooterText1, store.Id, false);
                            _settingService.SaveSetting(appPrintSetting, x => x.PageFooterText2, store.Id, false);
                            _settingService.SaveSetting(appPrintSetting, x => x.PageHeaderImage, store.Id, false);
                            _settingService.SaveSetting(appPrintSetting, x => x.PageFooterImage, store.Id, false);
                        }

                    }
                    else
                    {
                        _settingService.SaveSetting(newSetting, store.Id);
                        store.Setuped = true;
                        EngineContext.Current.Resolve<IStoreService>().UpdateStore(store);
                    }
                }

                return new ProcessStatus { Result = true };
            }
            catch (Exception ex)
            {
                var message = string.Empty;
                for (var inner = ex; inner != null; inner = inner.InnerException)
                {
                    message = $"{message}{inner.Message}{Environment.NewLine}";
                }

                return new ProcessStatus { Result = false, Errors = message };
            }
        }

        /// <summary>
        /// 安装PC打印初始 PCPrintSetting.csv
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public virtual ProcessStatus InstallPCPrintSettings(Store store, string path)
        {
            if (store.Id == 0)
            {
                return new ProcessStatus { Result = false, Errors = "指定经销商不存在" };
            }

            try
            {
                //TODO...
                var _settingService = EngineContext.Current.Resolve<ISettingService>();
                if (_settingService == null)
                {
                    throw new Exception("不能解析服务ISettingService");
                }


                path = AppContext.BaseDirectory + "InitData\\PCPrintSetting.csv";
                FileInfo myFile = new FileInfo(path);
                StreamReader sW5 = myFile.OpenText();
                var result = sW5.ReadToEnd();
                sW5.Close();

                var newSetting = JsonConvert.DeserializeObject<PCPrintSetting>(result);

                if (store.Setuped)
                {
                    var appPrintSetting = _settingService.LoadSetting<PCPrintSetting>(store.Id);

                    if (appPrintSetting != null)
                    {
                        appPrintSetting.StoreName = store.Name;
                        appPrintSetting.Address = "";
                        appPrintSetting.PlaceOrderTelphone = "";
                        appPrintSetting.PrintMethod = newSetting.PrintMethod;
                        appPrintSetting.PaperType = newSetting.PaperType;
                        appPrintSetting.PaperWidth = newSetting.PaperWidth;
                        appPrintSetting.PaperHeight = newSetting.PaperHeight;
                        appPrintSetting.BorderType = newSetting.BorderType;
                        appPrintSetting.MarginTop = newSetting.MarginTop;
                        appPrintSetting.MarginBottom = newSetting.MarginBottom;
                        appPrintSetting.MarginLeft = newSetting.MarginLeft;
                        appPrintSetting.MarginRight = newSetting.MarginRight;
                        appPrintSetting.IsPrintPageNumber = newSetting.IsPrintPageNumber;
                        appPrintSetting.PrintHeader = newSetting.PrintHeader;
                        appPrintSetting.PrintFooter = newSetting.PrintFooter;
                        appPrintSetting.IsFixedRowNumber = newSetting.IsFixedRowNumber;
                        appPrintSetting.FixedRowNumber = newSetting.FixedRowNumber;
                        appPrintSetting.PrintSubtotal = newSetting.PrintSubtotal;
                        appPrintSetting.PrintPort = newSetting.PrintPort;

                        _settingService.SaveSetting(appPrintSetting, x => x.StoreName, store.Id, false);
                        _settingService.SaveSetting(appPrintSetting, x => x.Address, store.Id, false);
                        _settingService.SaveSetting(appPrintSetting, x => x.PlaceOrderTelphone, store.Id, false);
                        _settingService.SaveSetting(appPrintSetting, x => x.PrintMethod, store.Id, false);
                        _settingService.SaveSetting(appPrintSetting, x => x.PaperType, store.Id, false);
                        _settingService.SaveSetting(appPrintSetting, x => x.PaperWidth, store.Id, false);
                        _settingService.SaveSetting(appPrintSetting, x => x.PaperHeight, store.Id, false);
                        _settingService.SaveSetting(appPrintSetting, x => x.BorderType, store.Id, false);
                        _settingService.SaveSetting(appPrintSetting, x => x.MarginTop, store.Id, false);
                        _settingService.SaveSetting(appPrintSetting, x => x.MarginBottom, store.Id, false);
                        _settingService.SaveSetting(appPrintSetting, x => x.MarginLeft, store.Id, false);
                        _settingService.SaveSetting(appPrintSetting, x => x.MarginRight, store.Id, false);
                        _settingService.SaveSetting(appPrintSetting, x => x.IsPrintPageNumber, store.Id, false);
                        _settingService.SaveSetting(appPrintSetting, x => x.PrintHeader, store.Id, false);
                        _settingService.SaveSetting(appPrintSetting, x => x.PrintFooter, store.Id, false);
                        _settingService.SaveSetting(appPrintSetting, x => x.IsFixedRowNumber, store.Id, false);
                        _settingService.SaveSetting(appPrintSetting, x => x.FixedRowNumber, store.Id, false);
                        _settingService.SaveSetting(appPrintSetting, x => x.PrintSubtotal, store.Id, false);
                        _settingService.SaveSetting(appPrintSetting, x => x.PrintPort, store.Id, false);
                    }
                }
                else
                {
                    newSetting.StoreName = store.Name;
                    newSetting.Address = "";
                    newSetting.PlaceOrderTelphone = "";
                    _settingService.SaveSetting(newSetting, store.Id);

                    store.Setuped = true;
                    EngineContext.Current.Resolve<IStoreService>().UpdateStore(store);
                }

                return new ProcessStatus { Result = true };
            }
            catch (Exception ex)
            {
                var message = string.Empty;
                for (var inner = ex; inner != null; inner = inner.InnerException)
                {
                    message = $"{message}{inner.Message}{Environment.NewLine}";
                }

                return new ProcessStatus { Result = false, Errors = message };
            }
        }

        /// <summary>
        /// 安装商品配置初始 ProductSetting.csv
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public virtual ProcessStatus InstallProductSettings(Store store, string path)
        {
            if (store.Id == 0)
            {
                return new ProcessStatus { Result = false, Errors = "指定经销商不存在" };
            }

            try
            {
                //TODO...
                var _settingService = EngineContext.Current.Resolve<ISettingService>();
                if (_settingService == null)
                {
                    throw new Exception("不能解析服务ISettingService");
                }

                path = AppContext.BaseDirectory + "InitData\\ProductSetting.csv";
                FileInfo myFile = new FileInfo(path);
                StreamReader sW5 = myFile.OpenText();
                var result = sW5.ReadToEnd();
                sW5.Close();

                var newSetting = JsonConvert.DeserializeObject<ProductSetting>(result);

                if (store.Setuped)
                {
                    var productSetting = _settingService.LoadSetting<ProductSetting>(store.Id);

                    if (productSetting != null)
                    {
                        productSetting.SmallUnitSpecificationAttributeOptionsMapping = newSetting.SmallUnitSpecificationAttributeOptionsMapping;
                        productSetting.StrokeUnitSpecificationAttributeOptionsMapping = newSetting.StrokeUnitSpecificationAttributeOptionsMapping;
                        productSetting.BigUnitSpecificationAttributeOptionsMapping = newSetting.BigUnitSpecificationAttributeOptionsMapping;

                        _settingService.SaveSetting(productSetting, x => x.SmallUnitSpecificationAttributeOptionsMapping, store.Id, false);
                        _settingService.SaveSetting(productSetting, x => x.StrokeUnitSpecificationAttributeOptionsMapping, store.Id, false);
                        _settingService.SaveSetting(productSetting, x => x.BigUnitSpecificationAttributeOptionsMapping, store.Id, false);
                    }
                }
                else
                {
                    _settingService.SaveSetting(newSetting, store.Id);

                    store.Setuped = true;
                    EngineContext.Current.Resolve<IStoreService>().UpdateStore(store);
                }

                return new ProcessStatus { Result = true };
            }
            catch (Exception ex)
            {
                var message = string.Empty;
                for (var inner = ex; inner != null; inner = inner.InnerException)
                {
                    message = $"{message}{inner.Message}{Environment.NewLine}";
                }

                return new ProcessStatus { Result = false, Errors = message };
            }
        }

        /// <summary>
        /// 安装财务设置初始  FinanceSetting.csv
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public virtual ProcessStatus InstallFinanceSettings(Store store, string path)
        {
            if (store.Id == 0)
            {
                return new ProcessStatus { Result = false, Errors = "指定经销商不存在" };
            }

            try
            {
                //TODO...
                var _settingService = EngineContext.Current.Resolve<ISettingService>();
                if (_settingService == null)
                {
                    throw new Exception("不能解析服务ISettingService");
                }

                path = AppContext.BaseDirectory + "InitData\\FinanceSetting.csv";
                FileInfo myFile = new FileInfo(path);
                StreamReader sW5 = myFile.OpenText();
                var result = sW5.ReadToEnd();
                sW5.Close();

                var newSetting = JsonConvert.DeserializeObject<FinanceSetting>(result);

                if (store.Setuped)
                {
                    var financeSetting = _settingService.LoadSetting<FinanceSetting>(store.Id);

                    if (financeSetting != null)
                    {

                        //默认账户集
                        var options = (from option in EngineContext.Current.Resolve<IAccountingService>().GetAccountingOptions(store.Id) select option).ToList();


                        #region 销售
                        var saleFinanceConfiguer = string.IsNullOrEmpty(newSetting.SaleBillAccountingOptionConfiguration) ? null : JsonConvert.DeserializeObject<FinanceAccountingMap>(newSetting.SaleBillAccountingOptionConfiguration);

                        if (saleFinanceConfiguer.Options.Count > 0)
                        {
                            var ids = saleFinanceConfiguer.Options.Select(o => o.Number);
                            saleFinanceConfiguer.Options = options.Where(s => ids.Contains(s.Number)).ToList();
                            saleFinanceConfiguer.DefaultOption = options.FirstOrDefault(s => s.Number == saleFinanceConfiguer.DefaultOption)?.Number ?? 0;
                            saleFinanceConfiguer.DebitOption = options.FirstOrDefault(s => s.Number == saleFinanceConfiguer.DebitOption)?.Number ?? 0;
                            saleFinanceConfiguer.CreditOption = options.FirstOrDefault(s => s.Number == saleFinanceConfiguer.CreditOption)?.Number ?? 0;
                        }

                        var saleReservationFinanceConfiguer = string.IsNullOrEmpty(newSetting.SaleReservationBillAccountingOptionConfiguration) ? null : JsonConvert.DeserializeObject<FinanceAccountingMap>(newSetting.SaleReservationBillAccountingOptionConfiguration);

                        if (saleReservationFinanceConfiguer.Options.Count > 0)
                        {
                            var ids = saleReservationFinanceConfiguer.Options.Select(o => o.Number);
                            saleReservationFinanceConfiguer.Options = options.Where(s => ids.Contains(s.Number)).ToList();

                            saleReservationFinanceConfiguer.DefaultOption = options.FirstOrDefault(s => s.Number == saleReservationFinanceConfiguer.DefaultOption)?.Number ?? 0;
                            saleReservationFinanceConfiguer.DebitOption = options.FirstOrDefault(s => s.Number == saleReservationFinanceConfiguer.DebitOption)?.Number ?? 0;
                            saleReservationFinanceConfiguer.CreditOption = options.FirstOrDefault(s => s.Number == saleReservationFinanceConfiguer.CreditOption)?.Number ?? 0;
                        }

                        var returnFinanceConfiguer = string.IsNullOrEmpty(newSetting.ReturnBillAccountingOptionConfiguration) ? null : JsonConvert.DeserializeObject<FinanceAccountingMap>(newSetting.ReturnBillAccountingOptionConfiguration);
                        if (returnFinanceConfiguer.Options.Count > 0)
                        {
                            var ids = returnFinanceConfiguer.Options.Select(o => o.Number);
                            returnFinanceConfiguer.Options = options.Where(s => ids.Contains(s.Number)).ToList();

                            returnFinanceConfiguer.DefaultOption = options.FirstOrDefault(s => s.Number == returnFinanceConfiguer.DefaultOption)?.Number ?? 0;
                            returnFinanceConfiguer.DebitOption = options.FirstOrDefault(s => s.Number == returnFinanceConfiguer.DebitOption)?.Number ?? 0;
                            returnFinanceConfiguer.CreditOption = options.FirstOrDefault(s => s.Number == returnFinanceConfiguer.CreditOption)?.Number ?? 0;
                        }

                        var returnReservationFinanceConfiguer = string.IsNullOrEmpty(newSetting.ReturnReservationBillAccountingOptionConfiguration) ? null : JsonConvert.DeserializeObject<FinanceAccountingMap>(newSetting.ReturnReservationBillAccountingOptionConfiguration);
                        if (returnReservationFinanceConfiguer.Options.Count > 0)
                        {
                            var ids = returnReservationFinanceConfiguer.Options.Select(o => o.Number);
                            returnReservationFinanceConfiguer.Options = options.Where(s => ids.Contains(s.Number)).ToList();

                            returnReservationFinanceConfiguer.DefaultOption = options.FirstOrDefault(s => s.Number == returnReservationFinanceConfiguer.DefaultOption)?.Number ?? 0;
                            returnReservationFinanceConfiguer.DebitOption = options.FirstOrDefault(s => s.Number == returnReservationFinanceConfiguer.DebitOption)?.Number ?? 0;
                            returnReservationFinanceConfiguer.CreditOption = options.FirstOrDefault(s => s.Number == returnReservationFinanceConfiguer.CreditOption)?.Number ?? 0;

                        }
                        #endregion

                        #region 采购
                        var purchaseFinanceConfiguer = string.IsNullOrEmpty(newSetting.PurchaseBillAccountingOptionConfiguration) ? null : JsonConvert.DeserializeObject<FinanceAccountingMap>(newSetting.PurchaseBillAccountingOptionConfiguration);
                        if (purchaseFinanceConfiguer.Options.Count > 0)
                        {
                            var ids = purchaseFinanceConfiguer.Options.Select(o => o.Number);
                            purchaseFinanceConfiguer.Options = options.Where(s => ids.Contains(s.Number)).ToList();

                            purchaseFinanceConfiguer.DefaultOption = options.FirstOrDefault(s => s.Number == purchaseFinanceConfiguer.DefaultOption)?.Number ?? 0;
                            purchaseFinanceConfiguer.DebitOption = options.FirstOrDefault(s => s.Number == purchaseFinanceConfiguer.DebitOption)?.Number ?? 0;
                            purchaseFinanceConfiguer.CreditOption = options.FirstOrDefault(s => s.Number == purchaseFinanceConfiguer.CreditOption)?.Number ?? 0;
                        }

                        var purchaseReturnFinanceConfiguer = string.IsNullOrEmpty(newSetting.PurchaseReturnBillAccountingOptionConfiguration) ? null : JsonConvert.DeserializeObject<FinanceAccountingMap>(newSetting.PurchaseReturnBillAccountingOptionConfiguration);
                        if (purchaseReturnFinanceConfiguer.Options.Count > 0)
                        {
                            var ids = purchaseReturnFinanceConfiguer.Options.Select(o => o.Number);
                            purchaseReturnFinanceConfiguer.Options = options.Where(s => ids.Contains(s.Number)).ToList();

                            purchaseReturnFinanceConfiguer.DefaultOption = options.FirstOrDefault(s => s.Number == purchaseReturnFinanceConfiguer.DefaultOption)?.Number ?? 0;
                            purchaseReturnFinanceConfiguer.DebitOption = options.FirstOrDefault(s => s.Number == purchaseReturnFinanceConfiguer.DebitOption)?.Number ?? 0;
                            purchaseReturnFinanceConfiguer.CreditOption = options.FirstOrDefault(s => s.Number == purchaseReturnFinanceConfiguer.CreditOption)?.Number ?? 0;
                        }
                        #endregion

                        #region 仓储
                        var inventoryProfitLossFinanceConfiguer = string.IsNullOrEmpty(newSetting.InventoryProfitLossAccountingOptionConfiguration) ? null : JsonConvert.DeserializeObject<FinanceAccountingMap>(newSetting.InventoryProfitLossAccountingOptionConfiguration);
                        if (inventoryProfitLossFinanceConfiguer.Options.Count > 0)
                        {
                            var ids = inventoryProfitLossFinanceConfiguer.Options.Select(o => o.Number);
                            inventoryProfitLossFinanceConfiguer.Options = options.Where(s => ids.Contains(s.Number)).ToList();

                            inventoryProfitLossFinanceConfiguer.DefaultOption = options.FirstOrDefault(s => s.Number == inventoryProfitLossFinanceConfiguer.DefaultOption)?.Number ?? 0;
                            inventoryProfitLossFinanceConfiguer.DebitOption = options.FirstOrDefault(s => s.Number == inventoryProfitLossFinanceConfiguer.DebitOption)?.Number ?? 0;
                            inventoryProfitLossFinanceConfiguer.CreditOption = options.FirstOrDefault(s => s.Number == inventoryProfitLossFinanceConfiguer.CreditOption)?.Number ?? 0;
                        }

                        var scrapProductFinanceConfiguer = string.IsNullOrEmpty(newSetting.ScrapProductAccountingOptionConfiguration) ? null : JsonConvert.DeserializeObject<FinanceAccountingMap>(newSetting.ScrapProductAccountingOptionConfiguration);
                        if (scrapProductFinanceConfiguer.Options.Count > 0)
                        {
                            var ids = scrapProductFinanceConfiguer.Options.Select(o => o.Number);
                            scrapProductFinanceConfiguer.Options = options.Where(s => ids.Contains(s.Number)).ToList();

                            scrapProductFinanceConfiguer.DefaultOption = options.FirstOrDefault(s => s.Number == scrapProductFinanceConfiguer.DefaultOption)?.Number ?? 0;
                            scrapProductFinanceConfiguer.DebitOption = options.FirstOrDefault(s => s.Number == scrapProductFinanceConfiguer.DebitOption)?.Number ?? 0;
                            scrapProductFinanceConfiguer.CreditOption = options.FirstOrDefault(s => s.Number == scrapProductFinanceConfiguer.CreditOption)?.Number ?? 0;
                        }
                        #endregion

                        #region 财务
                        var receiptFinanceConfiguer = string.IsNullOrEmpty(newSetting.ReceiptAccountingOptionConfiguration) ? null : JsonConvert.DeserializeObject<FinanceAccountingMap>(newSetting.ReceiptAccountingOptionConfiguration);
                        if (receiptFinanceConfiguer.Options.Count > 0)
                        {
                            var ids = receiptFinanceConfiguer.Options.Select(o => o.Number);
                            receiptFinanceConfiguer.Options = options.Where(s => ids.Contains(s.Number)).ToList();

                            receiptFinanceConfiguer.DefaultOption = options.FirstOrDefault(s => s.Number == receiptFinanceConfiguer.DefaultOption)?.Number ?? 0;
                            receiptFinanceConfiguer.DebitOption = options.FirstOrDefault(s => s.Number == receiptFinanceConfiguer.DebitOption)?.Number ?? 0;
                            receiptFinanceConfiguer.CreditOption = options.FirstOrDefault(s => s.Number == receiptFinanceConfiguer.CreditOption)?.Number ?? 0;
                        }

                        var paymentFinanceConfiguer = string.IsNullOrEmpty(newSetting.PaymentAccountingOptionConfiguration) ? null : JsonConvert.DeserializeObject<FinanceAccountingMap>(newSetting.PaymentAccountingOptionConfiguration);
                        if (paymentFinanceConfiguer.Options.Count > 0)
                        {
                            var ids = paymentFinanceConfiguer.Options.Select(o => o.Number);
                            paymentFinanceConfiguer.Options = options.Where(s => ids.Contains(s.Number)).ToList();

                            paymentFinanceConfiguer.DefaultOption = options.FirstOrDefault(s => s.Number == paymentFinanceConfiguer.DefaultOption)?.Number ?? 0;
                            paymentFinanceConfiguer.DebitOption = options.FirstOrDefault(s => s.Number == paymentFinanceConfiguer.DebitOption)?.Number ?? 0;
                            paymentFinanceConfiguer.CreditOption = options.FirstOrDefault(s => s.Number == paymentFinanceConfiguer.CreditOption)?.Number ?? 0;
                        }

                        var advanceReceiptFinanceConfiguer = string.IsNullOrEmpty(newSetting.AdvanceReceiptAccountingOptionConfiguration) ? null : JsonConvert.DeserializeObject<FinanceAccountingMap>(newSetting.AdvanceReceiptAccountingOptionConfiguration);
                        if (advanceReceiptFinanceConfiguer.Options.Count > 0)
                        {
                            var ids = advanceReceiptFinanceConfiguer.Options.Select(o => o.Number);
                            advanceReceiptFinanceConfiguer.Options = options.Where(s => ids.Contains(s.Number)).ToList();

                            advanceReceiptFinanceConfiguer.DefaultOption = options.FirstOrDefault(s => s.Number == advanceReceiptFinanceConfiguer.DefaultOption)?.Number ?? 0;
                            advanceReceiptFinanceConfiguer.DebitOption = options.FirstOrDefault(s => s.Number == advanceReceiptFinanceConfiguer.DebitOption)?.Number ?? 0;
                            advanceReceiptFinanceConfiguer.CreditOption = options.FirstOrDefault(s => s.Number == advanceReceiptFinanceConfiguer.CreditOption)?.Number ?? 0;
                        }

                        var advancePaymentFinanceConfiguer = string.IsNullOrEmpty(newSetting.AdvancePaymentAccountingOptionConfiguration) ? null : JsonConvert.DeserializeObject<FinanceAccountingMap>(newSetting.AdvancePaymentAccountingOptionConfiguration);
                        if (advancePaymentFinanceConfiguer.Options.Count > 0)
                        {
                            var ids = advancePaymentFinanceConfiguer.Options.Select(o => o.Number);
                            advancePaymentFinanceConfiguer.Options = options.Where(s => ids.Contains(s.Number)).ToList();

                            advancePaymentFinanceConfiguer.DefaultOption = options.FirstOrDefault(s => s.Number == advancePaymentFinanceConfiguer.DefaultOption)?.Number ?? 0;
                            advancePaymentFinanceConfiguer.DebitOption = options.FirstOrDefault(s => s.Number == advancePaymentFinanceConfiguer.DebitOption)?.Number ?? 0;
                            advancePaymentFinanceConfiguer.CreditOption = options.FirstOrDefault(s => s.Number == advancePaymentFinanceConfiguer.CreditOption)?.Number ?? 0;
                        }

                        var costExpenditureFinanceConfiguer = string.IsNullOrEmpty(newSetting.CostExpenditureAccountingOptionConfiguration) ? null : JsonConvert.DeserializeObject<FinanceAccountingMap>(newSetting.CostExpenditureAccountingOptionConfiguration);
                        if (costExpenditureFinanceConfiguer.Options.Count > 0)
                        {
                            var ids = costExpenditureFinanceConfiguer.Options.Select(o => o.Number);
                            costExpenditureFinanceConfiguer.Options = options.Where(s => ids.Contains(s.Number)).ToList();

                            costExpenditureFinanceConfiguer.DefaultOption = options.FirstOrDefault(s => s.Number == costExpenditureFinanceConfiguer.DefaultOption)?.Number ?? 0;
                            costExpenditureFinanceConfiguer.DebitOption = options.FirstOrDefault(s => s.Number == costExpenditureFinanceConfiguer.DebitOption)?.Number ?? 0;
                            costExpenditureFinanceConfiguer.CreditOption = options.FirstOrDefault(s => s.Number == costExpenditureFinanceConfiguer.CreditOption)?.Number ?? 0;
                        }

                        var financialIncomeFinanceConfiguer = string.IsNullOrEmpty(newSetting.FinancialIncomeAccountingOptionConfiguration) ? null : JsonConvert.DeserializeObject<FinanceAccountingMap>(newSetting.FinancialIncomeAccountingOptionConfiguration);
                        if (financialIncomeFinanceConfiguer.Options.Count > 0)
                        {
                            var ids = financialIncomeFinanceConfiguer.Options.Select(o => o.Number);
                            financialIncomeFinanceConfiguer.Options = options.Where(s => ids.Contains(s.Number)).ToList();

                            financialIncomeFinanceConfiguer.DefaultOption = options.FirstOrDefault(s => s.Number == financialIncomeFinanceConfiguer.DefaultOption)?.Number ?? 0;
                            financialIncomeFinanceConfiguer.DebitOption = options.FirstOrDefault(s => s.Number == financialIncomeFinanceConfiguer.DebitOption)?.Number ?? 0;
                            financialIncomeFinanceConfiguer.CreditOption = options.FirstOrDefault(s => s.Number == financialIncomeFinanceConfiguer.CreditOption)?.Number ?? 0;
                        }

                        var costcontractFinanceConfiguer = string.IsNullOrEmpty(newSetting.CostContractAccountingOptionConfiguration) ? null : JsonConvert.DeserializeObject<FinanceAccountingMap>(newSetting.CostContractAccountingOptionConfiguration);
                        if (costcontractFinanceConfiguer.Options.Count > 0)
                        {
                            var ids = costcontractFinanceConfiguer.Options.Select(o => o.Number);
                            costcontractFinanceConfiguer.Options = options.Where(s => ids.Contains(s.Number)).ToList();

                            costcontractFinanceConfiguer.DefaultOption = options.FirstOrDefault(s => s.Number == costcontractFinanceConfiguer.DefaultOption)?.Number ?? 0;
                            costcontractFinanceConfiguer.DebitOption = options.FirstOrDefault(s => s.Number == costcontractFinanceConfiguer.DebitOption)?.Number ?? 0;
                            costcontractFinanceConfiguer.CreditOption = options.FirstOrDefault(s => s.Number == costcontractFinanceConfiguer.CreditOption)?.Number ?? 0;
                        }
                        #endregion



                        //转化
                        financeSetting.SaleBillAccountingOptionConfiguration = JsonConvert.SerializeObject(saleFinanceConfiguer);
                        financeSetting.SaleReservationBillAccountingOptionConfiguration = JsonConvert.SerializeObject(saleReservationFinanceConfiguer);

                        financeSetting.ReturnBillAccountingOptionConfiguration = JsonConvert.SerializeObject(returnFinanceConfiguer);
                        financeSetting.ReturnReservationBillAccountingOptionConfiguration = JsonConvert.SerializeObject(returnReservationFinanceConfiguer);

                        financeSetting.PurchaseBillAccountingOptionConfiguration = JsonConvert.SerializeObject(purchaseFinanceConfiguer);
                        financeSetting.PurchaseReturnBillAccountingOptionConfiguration = JsonConvert.SerializeObject(purchaseReturnFinanceConfiguer);

                        financeSetting.InventoryProfitLossAccountingOptionConfiguration = JsonConvert.SerializeObject(inventoryProfitLossFinanceConfiguer);
                        financeSetting.ScrapProductAccountingOptionConfiguration = JsonConvert.SerializeObject(scrapProductFinanceConfiguer);

                        financeSetting.ReceiptAccountingOptionConfiguration = JsonConvert.SerializeObject(receiptFinanceConfiguer);
                        financeSetting.PaymentAccountingOptionConfiguration = JsonConvert.SerializeObject(paymentFinanceConfiguer);

                        financeSetting.AdvanceReceiptAccountingOptionConfiguration = JsonConvert.SerializeObject(advanceReceiptFinanceConfiguer);
                        financeSetting.AdvancePaymentAccountingOptionConfiguration = JsonConvert.SerializeObject(advancePaymentFinanceConfiguer);

                        financeSetting.CostExpenditureAccountingOptionConfiguration = JsonConvert.SerializeObject(costExpenditureFinanceConfiguer);

                        financeSetting.FinancialIncomeAccountingOptionConfiguration = JsonConvert.SerializeObject(financialIncomeFinanceConfiguer);

                        financeSetting.CostContractAccountingOptionConfiguration = JsonConvert.SerializeObject(costcontractFinanceConfiguer);

                        //保存
                        _settingService.SaveSetting(financeSetting, x => x.SaleBillAccountingOptionConfiguration, store.Id, false);
                        _settingService.SaveSetting(financeSetting, x => x.SaleReservationBillAccountingOptionConfiguration, store.Id, false);

                        _settingService.SaveSetting(financeSetting, x => x.ReturnBillAccountingOptionConfiguration, store.Id, false);
                        _settingService.SaveSetting(financeSetting, x => x.ReturnReservationBillAccountingOptionConfiguration, store.Id, false);

                        _settingService.SaveSetting(financeSetting, x => x.PurchaseBillAccountingOptionConfiguration, store.Id, false);
                        _settingService.SaveSetting(financeSetting, x => x.PurchaseReturnBillAccountingOptionConfiguration, store.Id, false);

                        _settingService.SaveSetting(financeSetting, x => x.InventoryProfitLossAccountingOptionConfiguration, store.Id, false);
                        _settingService.SaveSetting(financeSetting, x => x.ScrapProductAccountingOptionConfiguration, store.Id, false);

                        _settingService.SaveSetting(financeSetting, x => x.ReceiptAccountingOptionConfiguration, store.Id, false);
                        _settingService.SaveSetting(financeSetting, x => x.PaymentAccountingOptionConfiguration, store.Id, false);

                        _settingService.SaveSetting(financeSetting, x => x.AdvanceReceiptAccountingOptionConfiguration, store.Id, false);
                        _settingService.SaveSetting(financeSetting, x => x.AdvancePaymentAccountingOptionConfiguration, store.Id, false);

                        _settingService.SaveSetting(financeSetting, x => x.CostExpenditureAccountingOptionConfiguration, store.Id, false);

                        _settingService.SaveSetting(financeSetting, x => x.FinancialIncomeAccountingOptionConfiguration, store.Id, false);

                        _settingService.SaveSetting(financeSetting, x => x.CostContractAccountingOptionConfiguration, store.Id, false);
                    }
                }
                else
                {
                    _settingService.SaveSetting(newSetting, store.Id);

                    store.Setuped = true;
                    EngineContext.Current.Resolve<IStoreService>().UpdateStore(store);
                }

                return new ProcessStatus { Result = true };
            }
            catch (Exception ex)
            {
                var message = string.Empty;
                for (var inner = ex; inner != null; inner = inner.InnerException)
                {
                    message = $"{message}{inner.Message}{Environment.NewLine}";
                }

                return new ProcessStatus { Result = false, Errors = message };
            }
        }

        /// <summary>
        /// 安装备注初始 RemarkConfig.csv
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public virtual ProcessStatus InstallRemarkConfigs(int storeId, string path)
        {
            if (storeId == 0)
            {
                return new ProcessStatus { Result = false, Errors = "指定经销商不存在" };
            }

            try
            {
                path = AppContext.BaseDirectory + "InitData\\RemarkConfig.csv";
                FileInfo myFile = new FileInfo(path);
                StreamReader sW5 = new StreamReader(myFile.OpenRead(),Encoding.GetEncoding("gb2312"));
                var result = sW5.ReadToEnd();
                sW5.Close();

                var remarkConfigs = JsonConvert.DeserializeObject<List<RemarkConfig>>(result);

                if (remarkConfigs != null && remarkConfigs.Count > 0)
                {
                    remarkConfigs.ForEach(p =>
                    {
                        var uow = RemarkConfigsRepository.UnitOfWork;
                        var temp = RemarkConfigsRepository.TableNoTracking.FirstOrDefault(s => s.StoreId == storeId && s.Name == p.Name);
                        if (temp != null)
                        {
                            temp.Name = p.Name;
                            temp.RemberPrice = p.RemberPrice;

                            RemarkConfigsRepository.Update(temp);
                        }
                        else
                        {
                            var temp2 = new RemarkConfig
                            {
                                StoreId = storeId,
                                Name = p.Name,
                                RemberPrice = p.RemberPrice
                            };

                            RemarkConfigsRepository.Insert(temp2);
                        }
                        uow.SaveChanges();
                    });
                }

                return new ProcessStatus { Result = true };
            }
            catch (Exception ex)
            {
                var message = string.Empty;
                for (var inner = ex; inner != null; inner = inner.InnerException)
                {
                    message = $"{message}{inner.Message}{Environment.NewLine}";
                }

                return new ProcessStatus { Result = false, Errors = message };
            }
        }

        /// <summary>
        /// 安装商品分类/商品预设（雪花必须）
        /// </summary>
        public virtual ProcessStatus InstallCategories(int storeId)
        {
            if (storeId == 0)
            {
                return new ProcessStatus { Result = false, Errors = "指定经销商不存在" };
            }

            try
            {
                //TODO...
                var category = new Category()
                {
                    Name = "全部",
                    ParentId = 0,
                    OrderNo = 1,
                    StatisticalType = 0,
                    BrandId = 0,
                    BrandName = "",
                    Published = true,
                    StoreId = storeId
                };
                var uow = CategoriesRepository.UnitOfWork;

                var exits = CategoriesRepository.Table
                                         .Where(s => s.Name == category.Name && s.StoreId == category.StoreId)
                                         .Count() > 0;
                if (!exits)
                    CategoriesRepository.Insert(category);

                uow.SaveChanges();

                return new ProcessStatus { Result = true };
            }
            catch (Exception ex)
            {
                var message = string.Empty;
                for (var inner = ex; inner != null; inner = inner.InnerException)
                {
                    message = $"{message}{inner.Message}{Environment.NewLine}";
                }

                return new ProcessStatus { Result = false, Errors = message };
            }
        }

        /// <summary>
        /// 安装供应商
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public virtual ProcessStatus InstallManufacturers(int storeId)
        {
            if (storeId == 0)
            {
                return new ProcessStatus { Result = false, Errors = "指定经销商不存在" };
            }

            try
            {
               
                var path = AppContext.BaseDirectory + "InitData\\Manufacturer.csv";
                FileInfo myFile = new FileInfo(path);
                StreamReader sW5 = myFile.OpenText();
                var result = sW5.ReadToEnd();
                sW5.Close();

                var manufacturers = JsonConvert.DeserializeObject<List<Manufacturer>>(result);

                manufacturers.ForEach(s =>
                {
                    s.StoreId = storeId;
                    s.Status = true;
                    s.Deleted = false;
                    s.PriceRanges = "";
                    s.DisplayOrder = 0;
                    s.CreatedOnUtc = DateTime.Now;
                    s.UpdatedOnUtc = DateTime.Now;
                });

                var uow = ManufacturerRepository.UnitOfWork;

                manufacturers.ForEach(s =>
                {
                    var exits = ManufacturerRepository.Table
                                         .Where(s => s.Name == s.Name && s.StoreId == storeId)
                                         .Count() > 0;
                    if (!exits)
                    { 
                        s.Id = 0;
                        ManufacturerRepository.Insert(s);
                    }
                });
                uow.SaveChanges();

                return new ProcessStatus { Result = true };
            }
            catch (Exception ex)
            {
                var message = string.Empty;
                for (var inner = ex; inner != null; inner = inner.InnerException)
                {
                    message = $"{message}{inner.Message}{Environment.NewLine}";
                }

                return new ProcessStatus { Result = false, Errors = message };
            }
        }

        /// <summary>
        /// 安装品牌
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public virtual ProcessStatus InstallBrands(int storeId)
        {
            if (storeId == 0)
            {
                return new ProcessStatus { Result = false, Errors = "指定经销商不存在" };
            }
            return new ProcessStatus { Result = true };
            //try
            //{
            //    var brand1 = new Brand()
            //    {
            //        StoreId = storeId,
            //        Name = "雪花啤酒",
            //        Status = true,
            //        DisplayOrder = 0,
            //        CreatedOnUtc = DateTime.Now
            //    };

            //    var brand2 = new Brand()
            //    {
            //        StoreId = storeId,
            //        Name = "青岛啤酒",
            //        Status = true,
            //        DisplayOrder = 0,
            //        CreatedOnUtc = DateTime.Now
            //    };

            //    var brand3 = new Brand()
            //    {
            //        StoreId = storeId,
            //        Name = "Budweiser百威",
            //        Status = true,
            //        DisplayOrder = 0,
            //        CreatedOnUtc = DateTime.Now
            //    };

            //    var brand4 = new Brand()
            //    {
            //        StoreId = storeId,
            //        Name = "其他",
            //        Status = true,
            //        DisplayOrder = 0,
            //        CreatedOnUtc = DateTime.Now
            //    };

            //    var brands = new List<Brand>
            //    {
            //        brand1,
            //        brand2,
            //        brand3,
            //        brand4
            //    };

            //    var uow = BrandsRepository.UnitOfWork;

            //    brands.ForEach(b =>
            //    {
            //        var exits = BrandsRepository.Table
            //                               .Where(s => s.Name == b.Name && s.StoreId == b.StoreId)
            //                               .Count() > 0;
            //        if (!exits)
            //            BrandsRepository.Insert(b);
            //    });

            //    uow.SaveChanges();

            //    return new ProcessStatus { Result = true };
            //}
            //catch (Exception ex)
            //{
            //    var message = string.Empty;
            //    for (var inner = ex; inner != null; inner = inner.InnerException)
            //    {
            //        message = $"{message}{inner.Message}{Environment.NewLine}";
            //    }

            //    return new ProcessStatus { Result = false, Errors = message };
            //}
        }


        /// <summary>
        /// 安装初始仓库
        /// </summary>
        public virtual ProcessStatus InstallWarehouses(int storeId)
        {
            if (storeId == 0)
            {
                return new ProcessStatus { Result = false, Errors = "指定经销商不存在" };
            }

            try
            {
                var warehouses = new List<WareHouse>()
                {
                    new WareHouse
                    {
                        StoreId = storeId,
                        Name = "主仓库",
                        Code = "zck",
                        Deleted = false,
                        CreatedUserId = 0,
                        CreatedOnUtc = DateTime.Now,
                        IsSystem=true,
                    },
                    new WareHouse
                    {
                        StoreId = storeId,
                        Name = "车舱库",
                        Code = "cc",
                        Deleted = false,
                        CreatedUserId = 0,
                        CreatedOnUtc = DateTime.Now,
                         IsSystem=true,
                    }
                  };
                if (warehouses != null && warehouses.Count > 0)
                {
                    List<WareHouse> WareHouseList = new List<WareHouse>();
                    warehouses.ForEach(o =>
                    {
                        var warehouse = new WareHouse
                        {
                            StoreId = o.StoreId,
                            Name = o.Name,
                            Code = o.Code,
                            Deleted = false,
                            WareHouseAccessSettings = "",
                            CreatedUserId = o.CreatedUserId,
                            CreatedOnUtc = o.CreatedOnUtc
                        };

                        var exits = WareHousesRepository.Table
                        .Where(s => s.Name == o.Name && s.StoreId == o.StoreId)
                        .Count() > 0;
                        if (!exits)
                            WareHouseList.Add(warehouse);
                    });

                    var uow = WareHousesRepository.UnitOfWork;
                    WareHousesRepository.Insert(WareHouseList);
                    uow.SaveChanges();
                }
                return new ProcessStatus { Result = true };
            }
            catch (Exception ex)
            {
                var message = string.Empty;
                for (var inner = ex; inner != null; inner = inner.InnerException)
                {
                    message = $"{message}{inner.Message}{Environment.NewLine}";
                }

                return new ProcessStatus { Result = false, Errors = message };
            }
        }

        /// <summary>
        /// 安装会计科目
        /// </summary>
        public virtual ProcessStatus InstallAccounting(int storeId)
        {
            if (storeId == 0)
            {
                return new ProcessStatus { Result = false, Errors = "指定经销商不存在" };
            }

            try
            {
                var lists = new List<AccountingOption>()
                {
                new AccountingOption { StoreId = storeId, AccountingTypeId = 3, Number = 35, ParentId = 0, Name = "利润分配", Code = "3005", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 35, IsLeaf = false },
                new AccountingOption { StoreId = storeId, AccountingTypeId = 2, Number = 19, ParentId = 0, Name = "应付职工薪酬", Code = "2005", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 19, IsLeaf = false },
                new AccountingOption { StoreId = storeId, AccountingTypeId = 2, Number = 20, ParentId = 0, Name = "应交税费", Code = "2006", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 20, IsLeaf = false },
                new AccountingOption { StoreId = storeId, AccountingTypeId = 2, Number = 21, ParentId = 0, Name = "应付利息", Code = "2007", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 21, IsLeaf = false },
                new AccountingOption { StoreId = storeId, AccountingTypeId = 2, Number = 23, ParentId = 0, Name = "长期借款", Code = "2009", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 23, IsLeaf = false },
                new AccountingOption { StoreId = storeId, AccountingTypeId = 3, Number = 31, ParentId = 0, Name = "实收资本", Code = "3001", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 31, IsLeaf = false },
                new AccountingOption { StoreId = storeId, AccountingTypeId = 3, Number = 32, ParentId = 0, Name = "资本公积", Code = "3002", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 32, IsLeaf = false },
                new AccountingOption { StoreId = storeId, AccountingTypeId = 3, Number = 33, ParentId = 0, Name = "盈余公积", Code = "3003", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 33, IsLeaf = false },
                new AccountingOption { StoreId = storeId, AccountingTypeId = 3, Number = 34, ParentId = 0, Name = "本年利润", Code = "3004", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 34, IsLeaf = false },
                new AccountingOption { StoreId = storeId, AccountingTypeId = 2, Number = 22, ParentId = 0, Name = "其他应付款", Code = "2008", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 22, IsLeaf = false },
                new AccountingOption { StoreId = storeId, AccountingTypeId = 1, Number = 73, ParentId = 0, Name = "其他账户", Code = "1003", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 73, IsLeaf = false },
                new AccountingOption { StoreId = storeId, AccountingTypeId = 5, Number = 50, ParentId = 0, Name = "财务费用", Code = "5005", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 50, IsLeaf = false },
                new AccountingOption { StoreId = storeId, AccountingTypeId = 1, Number = 1, ParentId = 0, Name = "库存现金", Code = "1001", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 1, IsLeaf = false },
                new AccountingOption { StoreId = storeId, AccountingTypeId = 4, Number = 39, ParentId = 0, Name = "主营业务收入", Code = "4001", DisplayOrder = 0, Enabled = true, IsDefault = true, AccountCodeTypeId = 39, IsLeaf = false },
                new AccountingOption { StoreId = storeId, AccountingTypeId = 5, Number = 49, ParentId = 0, Name = "管理费用", Code = "5004", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 49, IsLeaf = false },
                new AccountingOption { StoreId = storeId, AccountingTypeId = 5, Number = 47, ParentId = 0, Name = "其他业务成本", Code = "5002", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 47, IsLeaf = false },
                new AccountingOption { StoreId = storeId, AccountingTypeId = 5, Number = 46, ParentId = 0, Name = "主营业务成本", Code = "5001", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 46, IsLeaf = false },
                new AccountingOption { StoreId = storeId, AccountingTypeId = 2, Number = 17, ParentId = 0, Name = "预收账款", Code = "2003", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 17, IsLeaf = false },
                new AccountingOption { StoreId = storeId, AccountingTypeId = 1, Number = 2, ParentId = 0, Name = "银行存款", Code = "1002", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 2, IsLeaf = false },
                new AccountingOption { StoreId = storeId, AccountingTypeId = 1, Number = 4, ParentId = 0, Name = "预付账款", Code = "1005", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 4, IsLeaf = false },
                new AccountingOption { StoreId = storeId, AccountingTypeId = 1, Number = 5, ParentId = 0, Name = "应收利息", Code = "1006", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 5, IsLeaf = false },
                new AccountingOption { StoreId = storeId, AccountingTypeId = 1, Number = 7, ParentId = 0, Name = "固定资产", Code = "1008", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 7, IsLeaf = false },
                new AccountingOption { StoreId = storeId, AccountingTypeId = 1, Number = 8, ParentId = 0, Name = "累计折旧", Code = "1009", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 8, IsLeaf = false },
                new AccountingOption { StoreId = storeId, AccountingTypeId = 1, Number = 9, ParentId = 0, Name = "固定资产清理", Code = "1010", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 9, IsLeaf = false },
                new AccountingOption { StoreId = storeId, AccountingTypeId = 2, Number = 18, ParentId = 0, Name = "订货款", Code = "2004", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 18, IsLeaf = false },
                new AccountingOption { StoreId = storeId, AccountingTypeId = 2, Number = 15, ParentId = 0, Name = "短期借款", Code = "2001", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 15, IsLeaf = false },
                new AccountingOption { StoreId = storeId, AccountingTypeId = 1, Number = 10, ParentId = 1, Name = "现金", Code = "100101", DisplayOrder = 0, Enabled = true, IsDefault = true, AccountCodeTypeId = 10, IsLeaf = true },
                new AccountingOption { StoreId = storeId, AccountingTypeId = 1, Number = 3, ParentId = 0, Name = "应收账款", Code = "1004", DisplayOrder = 0, Enabled = true, IsDefault = true, AccountCodeTypeId = 3, IsLeaf = false },
                 new AccountingOption { StoreId = storeId, AccountingTypeId = 1, Number = 6, ParentId = 0, Name = "库存商品", Code = "1007", DisplayOrder = 0, Enabled = true, IsDefault = true, AccountCodeTypeId = 6, IsLeaf = false },
                 new AccountingOption { StoreId = storeId, AccountingTypeId = 1, Number = 13, ParentId = 2, Name = "支付宝", Code = "100203", DisplayOrder = 0, Enabled = true, IsDefault = true, AccountCodeTypeId = 13, IsLeaf = true },
                 new AccountingOption { StoreId = storeId, AccountingTypeId = 1, Number = 12, ParentId = 2, Name = "微信", Code = "100202", DisplayOrder = 0, Enabled = true, IsDefault = true, AccountCodeTypeId = 12, IsLeaf = true },
                 new AccountingOption { StoreId = storeId, AccountingTypeId = 1, Number = 11, ParentId = 2, Name = "银行", Code = "100201", DisplayOrder = 0, Enabled = true, IsDefault = true, AccountCodeTypeId = 11, IsLeaf = true },
                 new AccountingOption { StoreId = storeId, AccountingTypeId = 1, Number = 14, ParentId = 4, Name = "预付款", Code = "100501", DisplayOrder = 0, Enabled = true, IsDefault = true, AccountCodeTypeId = 14, IsLeaf = true },
                 new AccountingOption { StoreId = storeId, AccountingTypeId = 2, Number = 16, ParentId = 0, Name = "应付账款", Code = "2002", DisplayOrder = 0, Enabled = true, IsDefault = true, AccountCodeTypeId = 16, IsLeaf = false },
                 new AccountingOption { StoreId = storeId, AccountingTypeId = 2, Number = 24, ParentId = 17, Name = "预收款", Code = "200301", DisplayOrder = 0, Enabled = true, IsDefault = true, AccountCodeTypeId = 24, IsLeaf = true },
                 new AccountingOption { StoreId = storeId, AccountingTypeId = 2, Number = 30, ParentId = 20, Name = "未交增值税", Code = "200602", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 30, IsLeaf = true },
                 new AccountingOption { StoreId = storeId, AccountingTypeId = 2, Number = 25, ParentId = 20, Name = "应交增值税", Code = "200601", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 25, IsLeaf = true },
                 new AccountingOption { StoreId = storeId, AccountingTypeId = 2, Number = 29, ParentId = 25, Name = "销项税额", Code = "20060104", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 29, IsLeaf = true },
                 new AccountingOption { StoreId = storeId, AccountingTypeId = 2, Number = 28, ParentId = 25, Name = "转出未交增值税", Code = "20060103", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 28, IsLeaf = true },
                 new AccountingOption { StoreId = storeId, AccountingTypeId = 2, Number = 27, ParentId = 25, Name = "已交税金", Code = "20060102", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 27, IsLeaf = true },
                 new AccountingOption { StoreId = storeId, AccountingTypeId = 2, Number = 26, ParentId = 25, Name = "进项税额", Code = "20060101", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 26, IsLeaf = true },
                 new AccountingOption { StoreId = storeId, AccountingTypeId = 3, Number = 38, ParentId = 33, Name = "任意盈余公积", Code = "300302", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 39, IsLeaf = true },
                 new AccountingOption { StoreId = storeId, AccountingTypeId = 3, Number = 36, ParentId = 33, Name = "法定盈余公积", Code = "300301", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 36, IsLeaf = true },
                 new AccountingOption { StoreId = storeId, AccountingTypeId = 3, Number = 37, ParentId = 35, Name = "未分配利润", Code = "300501", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 37, IsLeaf = true },
                 new AccountingOption { StoreId = storeId, AccountingTypeId = 4, Number = 40, ParentId = 0, Name = "其他业务收入", Code = "4002", DisplayOrder = 0, Enabled = true, IsDefault = true, AccountCodeTypeId = 40, IsLeaf = false },
                 new AccountingOption { StoreId = storeId, AccountingTypeId = 4, Number = 44, ParentId = 40, Name = "商品拆装收入", Code = "400204", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 44, IsLeaf = true },
                 new AccountingOption { StoreId = storeId, AccountingTypeId = 4, Number = 45, ParentId = 40, Name = "采购退货收入", Code = "400205", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 45, IsLeaf = true },
                 new AccountingOption { StoreId = storeId, AccountingTypeId = 4, Number = 42, ParentId = 40, Name = "成本调价收入", Code = "400202", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 42, IsLeaf = true },
                 new AccountingOption { StoreId = storeId, AccountingTypeId = 4, Number = 41, ParentId = 40, Name = "盘点报溢收入", Code = "400201", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 41, IsLeaf = true },
                 new AccountingOption { StoreId = storeId, AccountingTypeId = 4, Number = 43, ParentId = 40, Name = "厂家返点", Code = "400203", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 43, IsLeaf = true },
                 new AccountingOption { StoreId = storeId, AccountingTypeId = 5, Number = 51, ParentId = 47, Name = "盘点亏损", Code = "500201", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 51, IsLeaf = true },
                 new AccountingOption { StoreId = storeId, AccountingTypeId = 5, Number = 53, ParentId = 47, Name = "采购退货损失", Code = "500203", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 53, IsLeaf = true },
                 new AccountingOption { StoreId = storeId, AccountingTypeId = 5, Number = 52, ParentId = 47, Name = "成本调价损失", Code = "500202", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 52, IsLeaf = true },
                 new AccountingOption { StoreId = storeId, AccountingTypeId = 5, Number = 75, ParentId = 48, Name = "赠品", Code = "500312", DisplayOrder = 0, Enabled = true, IsDefault = true, AccountCodeTypeId = 75, IsLeaf = false },
                 new AccountingOption { StoreId = storeId, AccountingTypeId = 5, Number = 64, ParentId = 48, Name = "50元瓶盖", Code = "500311", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 64, IsLeaf = true },
                 new AccountingOption { StoreId = storeId, AccountingTypeId = 5, Number = 63, ParentId = 48, Name = "2元瓶盖", Code = "500310", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 63, IsLeaf = true },
                 new AccountingOption { StoreId = storeId, AccountingTypeId = 5, Number = 62, ParentId = 48, Name = "0.5元奖盖", Code = "500309", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 62, IsLeaf = true },
                 new AccountingOption { StoreId = storeId, AccountingTypeId = 5, Number = 61, ParentId = 48, Name = "折旧费用", Code = "500308", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 62, IsLeaf = true },
                 new AccountingOption { StoreId = storeId, AccountingTypeId = 5, Number = 60, ParentId = 48, Name = "运费", Code = "500307", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 60, IsLeaf = true },
                 new AccountingOption { StoreId = storeId, AccountingTypeId = 5, Number = 59, ParentId = 48, Name = "用餐费", Code = "500306", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 59, IsLeaf = true },
                 new AccountingOption { StoreId = storeId, AccountingTypeId = 5, Number = 58, ParentId = 48, Name = "车辆费", Code = "500305", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 58, IsLeaf = true },
                 new AccountingOption { StoreId = storeId, AccountingTypeId = 5, Number = 57, ParentId = 48, Name = "油费", Code = "500304", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 57, IsLeaf = true },
                 new AccountingOption { StoreId = storeId,AccountingTypeId = 5, Number = 56, ParentId = 48, Name = "陈列费", Code = "500303", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 56, IsLeaf = true },
                 new AccountingOption { StoreId = storeId, AccountingTypeId = 5, Number = 55, ParentId = 48, Name = "刷卡手续费", Code = "500302", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 55, IsLeaf = true },
                    new AccountingOption { StoreId = storeId, AccountingTypeId = 5, Number = 54, ParentId = 48, Name = "优惠", Code = "500301", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 54, IsLeaf = true },
                 new AccountingOption { StoreId = storeId, AccountingTypeId = 5, Number = 65, ParentId = 49, Name = "办公费", Code = "500401", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 65, IsLeaf = true },
                 new AccountingOption { StoreId = storeId, AccountingTypeId = 5, Number = 66, ParentId = 49, Name = "房租", Code = "500402", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 66, IsLeaf = true },
                 new AccountingOption { StoreId = storeId, AccountingTypeId = 5, Number = 67, ParentId = 49, Name = "物业管理费", Code = "500403", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 67, IsLeaf = false },
                 new AccountingOption { StoreId = storeId, AccountingTypeId = 5, Number = 68, ParentId = 49, Name = "水电费", Code = "500404", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 68, IsLeaf = true },
                 new AccountingOption { StoreId = storeId, AccountingTypeId = 5, Number = 69, ParentId = 49, Name = "累计折旧", Code = "500405", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 69, IsLeaf = true },
                 new AccountingOption { StoreId = storeId, AccountingTypeId = 5, Number = 70, ParentId = 50, Name = "汇兑损益", Code = "500501", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 70, IsLeaf = true },
                 new AccountingOption { StoreId = storeId, AccountingTypeId = 5, Number = 71, ParentId = 50, Name = "利息", Code = "500502", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 71, IsLeaf = true },
                 new AccountingOption { StoreId = storeId, AccountingTypeId = 5, Number = 72, ParentId = 50, Name = "手续费", Code = "500503", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 72, IsLeaf = true },
                 new AccountingOption { StoreId = storeId, AccountingTypeId = 5, Number = 48, ParentId = 0, Name = "销售费用", Code = "5003", DisplayOrder = 0, Enabled = true, IsDefault = true, AccountCodeTypeId = 48, IsLeaf = false },
                 new AccountingOption { StoreId = storeId, AccountingTypeId = 1, Number = 74, ParentId = 73, Name = "其他银行", Code = "100301", DisplayOrder = 0, Enabled = true, IsDefault = false, AccountCodeTypeId = 74, IsLeaf = true },
                 new AccountingOption { StoreId = storeId, AccountingTypeId = 5, Number = 78, ParentId = 0, Name = "营业外支出", Code = "5006", DisplayOrder = 0, Enabled = true, IsDefault = true, AccountCodeTypeId = 78, IsLeaf = false },
                 new AccountingOption { StoreId = storeId, AccountingTypeId = 4, Number = 79, ParentId = 0, Name = "营业外收入", Code = "4003", DisplayOrder = 0, Enabled = true, IsDefault = true, AccountCodeTypeId = 79, IsLeaf = false },
            };
                var uow = AccountingOptionsRepository.UnitOfWork;
                var updates = new List<AccountingOption>();
                var inserts = new List<AccountingOption>();

                lists.ForEach(o =>
                {
                    var temp = AccountingOptionsRepository.Table
                    .Where(a => a.StoreId == storeId && a.Code == o.Code)
                    .FirstOrDefault();

                    if (temp != null)
                    {
                        temp.StoreId = storeId;
                        temp.AccountCodeTypeId = o.AccountCodeTypeId;
                        temp.AccountingTypeId = o.AccountingTypeId;
                        temp.Number = o.Number;
                        temp.ParentId = o.ParentId;
                        temp.Name = o.Name;
                        temp.Code = o.Code;
                        temp.DisplayOrder = o.DisplayOrder;
                        temp.Enabled = o.Enabled;
                        temp.IsDefault = o.IsDefault;
                        temp.IsLeaf = o.IsLeaf;
                        temp.IsCustom = false;

                        updates.Add(temp);
                    }
                    else
                    {
                        var accountOption = new AccountingOption
                        {
                            StoreId = storeId,
                            AccountCodeTypeId = o.AccountCodeTypeId,
                            AccountingTypeId = o.AccountingTypeId,
                            ParentId = o.ParentId,
                            Number = o.Number, //编号
                            Code = o.Code,
                            Name = o.Name,
                            DisplayOrder = o.DisplayOrder,
                            Enabled = o.Enabled,
                            IsDefault = o.IsDefault,
                            IsLeaf = o.IsLeaf,
                            IsCustom = false
                        };
                        inserts.Add(accountOption);
                    }
                });

                AccountingOptionsRepository.Update(updates);
                AccountingOptionsRepository.Insert(inserts);
                
                uow.SaveChanges();

                return new ProcessStatus { Result = true };
            }
            catch (Exception ex)
            {
                var message = string.Empty;
                for (var inner = ex; inner != null; inner = inner.InnerException)
                {
                    message = $"{message}{inner.Message}{Environment.NewLine}";
                }

                return new ProcessStatus { Result = false, Errors = message };
            }
        }

        /// <summary>
        /// 安装打印模板
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public virtual ProcessStatus InstallPrintTemplate(int storeId)
        {
            if (storeId == 0)
            {
                return new ProcessStatus { Result = false, Errors = "指定经销商不存在" };
            }

            try
            {
                var path = AppContext.BaseDirectory + "InitData\\AppPrintTemplete.csv";
                FileInfo myFile = new FileInfo(path);
                StreamReader sW5 = myFile.OpenText();
                var result = sW5.ReadToEnd();
                sW5.Close();

                var allPrintTemplates = JsonConvert.DeserializeObject<List<PrintTemplate>>(result);
                if (allPrintTemplates != null && allPrintTemplates.Count > 0)
                {
                    var uow = PrintTemplatesRepository.UnitOfWork;

                    var updates = new List<PrintTemplate>();
                    var inserts = new List<PrintTemplate>();

                    allPrintTemplates.Where(s => s.StoreId == 797).ToList().ForEach(p =>
                    {
                        var temp = PrintTemplatesRepository.TableNoTracking
                        .FirstOrDefault(s => s.StoreId == storeId && s.Title == p.Title);
                        if (temp != null)
                        {
                            temp.TemplateType = p.TemplateType;
                            temp.BillType = p.BillType;
                            temp.Title = p.Title;
                            temp.Content = p.Content;
                            updates.Add(temp);
                        }
                        else
                        {
                            var temp2 = new PrintTemplate
                            {
                                StoreId = storeId,
                                TemplateType = p.TemplateType,
                                BillType = p.BillType,
                                Title = p.Title,
                                Content = p.Content
                            };
                            inserts.Add(temp2);
                        }
                    });

                    if (updates.Any())
                        PrintTemplatesRepository.Update(updates);

                    if (inserts.Any())
                        PrintTemplatesRepository.Insert(inserts);

                    uow.SaveChanges();
                }

                return new ProcessStatus { Result = true };
            }
            catch (Exception ex)
            {
                var message = string.Empty;
                for (var inner = ex; inner != null; inner = inner.InnerException)
                {
                    message = $"{message}{inner.Message}{Environment.NewLine}";
                }

                RollBackInstall(storeId);
                return new ProcessStatus { Result = false, Errors = message };
            }
            finally 
            {
                ConfirmCompletion(storeId);
            }
        }

        /// <summary>
        /// 安装初始片区
        /// </summary>
        public virtual ProcessStatus InstallDistrictTemplate(int storeId)
        {
            if (storeId == 0)
            {
                return new ProcessStatus { Result = false, Errors = "指定经销商不存在" };
            }

            try
            {
                var path = AppContext.BaseDirectory + "InitData\\AppDistrictTemplete.csv";
                FileInfo myFile = new FileInfo(path);
                StreamReader sW5 = myFile.OpenText();
                var result = sW5.ReadToEnd();
                sW5.Close();

                var allDistricts = JsonConvert.DeserializeObject<List<District>>(result);
                if (allDistricts != null && allDistricts.Count > 0)
                {
                    var uow = DistrictsRepository.UnitOfWork;
                    DistrictsRepository.Insert(allDistricts);
                    uow.SaveChanges();
                }

                return new ProcessStatus { Result = true };
            }
            catch (Exception ex)
            {
                var message = string.Empty;
                for (var inner = ex; inner != null; inner = inner.InnerException)
                {
                    message = $"{message}{inner.Message}{Environment.NewLine}";
                }

                RollBackInstall(storeId);
                return new ProcessStatus { Result = false, Errors = message };
            }
            finally
            {
                ConfirmCompletion(storeId);
            }
        }


        /// <summary>
        /// 安装商品规格属性
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public virtual ProcessStatus InstallProductUnit(int storeId)
        {
            if (storeId == 0)
            {
                return new ProcessStatus { Result = false, Errors = "指定经销商不存在" };
            }

            return new ProcessStatus { Result = true };
            //try
            //{

            //   var path = AppContext.BaseDirectory + "InitData\\SpecificationAttributes.csv";
            //    FileInfo myFile = new FileInfo(path);
            //    StreamReader sW5 = myFile.OpenText();
            //    var result = sW5.ReadToEnd();
            //    sW5.Close();


            //    path = AppContext.BaseDirectory + "InitData\\SpecificationAttributeOptions.csv";
            //    FileInfo myFile2 = new FileInfo(path);
            //    StreamReader sW52 = myFile2.OpenText();
            //    var result2 = sW52.ReadToEnd();
            //    sW52.Close();

            //    var uow = SpecificationAttributesRepository.UnitOfWork;

            //    var specificationAttributes = JsonConvert.DeserializeObject<List<SpecificationAttribute>>(result);
            //    var options = JsonConvert.DeserializeObject<List<SpecificationAttributeOption>>(result2);

            //    if (specificationAttributes != null && specificationAttributes.Count > 0)
            //    {
            //        var uow1 = SpecificationAttributeOptionsRepository.UnitOfWork;

            //        specificationAttributes.ForEach(sp =>
            //        {
            //            var specificationAttributeOptions = new List<SpecificationAttributeOption>();
            //            sp.StoreId = storeId;
            //            var sa = SpecificationAttributesRepository.Table
            //            .Where(s => s.Name == sp.Name && s.StoreId == storeId).FirstOrDefault();
            //            var specificationAttributeId = sa?.Id ?? 0;
            //            if (sa == null)
            //            {
            //                SpecificationAttributesRepository.Insert(sp);
            //                specificationAttributeId = sp.Id;
            //            }

            //            if (options != null && options.Count > 0)
            //            {
            //                options.ForEach(op =>
            //                {
            //                    var option = new SpecificationAttributeOption
            //                    {
            //                        StoreId = storeId,
            //                        SpecificationAttributeId = specificationAttributeId,
            //                        Name = op.Name,
            //                        DisplayOrder = op.DisplayOrder,
            //                        ConvertedQuantity = op.ConvertedQuantity,
            //                        UnitConversion = op.UnitConversion
            //                    };

            //                    var exits = SpecificationAttributeOptionsRepository.Table
            //                    .Where(s => s.Name == op.Name && s.StoreId == storeId && s.SpecificationAttributeId == specificationAttributeId)
            //                    .Count() > 0;
            //                    if (!exits)
            //                        specificationAttributeOptions.Add(option);
            //                });
            //            }

            //            SpecificationAttributeOptionsRepository.Insert(specificationAttributeOptions);
            //        });
            //        uow1.SaveChanges();
            //    }

            //    return new ProcessStatus { Result = true };
            //}
            //catch (Exception ex)
            //{
            //    var message = string.Empty;
            //    for (var inner = ex; inner != null; inner = inner.InnerException)
            //    {
            //        message = $"{message}{inner.Message}{Environment.NewLine}";
            //    }

            //    RollBackInstall(storeId);
            //    return new ProcessStatus { Result = false, Errors = message };
            //}
        }

        /// <summary>
        /// 初始提成方案
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        public ProcessStatus InstallPercentage(int storeId)
        {
            if (storeId == 0)
            {
                return new ProcessStatus { Result = false, Errors = "指定经销商不存在" };
            }

            try
            {

                var path = AppContext.BaseDirectory + "InitData\\PercentagePlan.csv";
                FileInfo myFile = new FileInfo(path);
                StreamReader sW5 = myFile.OpenText();
                var result = sW5.ReadToEnd();
                sW5.Close();

                var plans = JsonConvert.DeserializeObject<List<PercentagePlan>>(result);

                plans.ForEach(s => {
                    s.StoreId = storeId;
                });

                 var uow = PercentagePlanRepository.UnitOfWork;
                PercentagePlanRepository.Insert(plans);
                uow.SaveChanges();

                return new ProcessStatus { Result = true };
            }
            catch (Exception ex)
            {
                var message = string.Empty;
                for (var inner = ex; inner != null; inner = inner.InnerException)
                {
                    message = $"{message}{inner.Message}{Environment.NewLine}";
                }

                return new ProcessStatus { Result = false, Errors = message };
            }
        }



        /// <summary>
        /// 确认安装
        /// </summary>
        /// <param name="storeId"></param>
        public virtual void ConfirmCompletion(int storeId)
        {
            var uow1 = StoreRepository.UnitOfWork;
            var store = StoreRepository.Table.Where(s => s.Id == storeId).FirstOrDefault();
            if (store != null)
            {
                store.Setuped = true;
                StoreRepository.Update(store);
            }
            uow1.SaveChanges();
        }

        public virtual void RollBackInstall(int storeId)
        {
            if (storeId > 0)
            {
                string deleteSqlString1 = $"delete from dcms.PrintTemplates where StoreId={storeId};"; //回滚打印模板
                PrintTemplatesRepository.ExecuteSqlScript(deleteSqlString1); //dcms回滚
                RollBackInstallCompanySettings(storeId);
                RollBackInstallAppPrintSettings(storeId);
                RollBackInstallPCPrintSettings(storeId);
                RollBackInstallProductSettings(storeId);
                RollBackInstallFinanceSettings(storeId);
            }
        }

        private void RollBackInstallCompanySettings(int storeId)
        {
            var _settingService = EngineContext.Current.Resolve<ISettingService>();
            if (_settingService == null)
            {
                throw new Exception("不能解析服务ISettingService");
            }

            var companySetting = _settingService.LoadSetting<CompanySetting>(storeId);
            if (companySetting != null)
            {
                _settingService.DeleteSetting<CompanySetting>(storeId);
            }
        }

        private void RollBackInstallAppPrintSettings(int storeId)
        {
            var _settingService = EngineContext.Current.Resolve<ISettingService>();
            if (_settingService == null)
            {
                throw new Exception("不能解析服务ISettingService");
            }

            var appPrintSetting = _settingService.LoadSetting<APPPrintSetting>(storeId);
            if (appPrintSetting != null)
            {
                _settingService.DeleteSetting<APPPrintSetting>(storeId);
            }
        }

        private void RollBackInstallPCPrintSettings(int storeId)
        {
            var _settingService = EngineContext.Current.Resolve<ISettingService>();
            if (_settingService == null)
            {
                throw new Exception("不能解析服务ISettingService");
            }

            var pcPrintSetting = _settingService.LoadSetting<PCPrintSetting>(storeId);
            if (pcPrintSetting != null)
            {
                _settingService.DeleteSetting<PCPrintSetting>(storeId);
            }
        }

        private void RollBackInstallProductSettings(int storeId)
        {
            var _settingService = EngineContext.Current.Resolve<ISettingService>();
            if (_settingService == null)
            {
                throw new Exception("不能解析服务ISettingService");
            }

            var productSetting = _settingService.LoadSetting<ProductSetting>(storeId);
            if (productSetting != null)
            {
                _settingService.DeleteSetting<ProductSetting>(storeId);
            }
        }

        private void RollBackInstallFinanceSettings(int storeId)
        {
            var _settingService = EngineContext.Current.Resolve<ISettingService>();
            if (_settingService == null)
            {
                throw new Exception("不能解析服务ISettingService");
            }

            var financeSetting = _settingService.LoadSetting<FinanceSetting>(storeId);
            if (financeSetting != null)
            {
                _settingService.DeleteSetting<FinanceSetting>(storeId);
            }
        }



    }
}
