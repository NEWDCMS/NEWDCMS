using DCMS.Core.Caching;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using DCMS.Services.Security;
using DCMS.Services.Settings;
using DCMS.Services.Stores;
using DCMS.Services.Users;

namespace DCMS.Services.Configuration
{
    public class SetupGuideService : BaseService
    {
        private readonly IUserService _userService;
        private readonly IStoreService _storeService;
        private readonly IPermissionService _permissionService;
        private readonly IPermissionProvider _permissionProvider;
        private readonly IUserGroupService _userGroupService;
        private readonly IModuleService _moduleService;
        private readonly IAccountingService _accountingService;
        private readonly ISettingService _settingService;
        private readonly IPrintTemplateService _printTemplateService;
        private readonly IUserRegistrationService _userRegistrationService;

        public SetupGuideService(
            IServiceGetter serviceGetter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher,
            IUserService userService,
            IStoreService storeService,
            IPermissionService permissionService,
            IPermissionProvider permissionProvider,
            IUserGroupService userGroupService,
            IModuleService moduleService,
            IAccountingService accountingService,
            ISettingService settingService,
            IPrintTemplateService printTemplateService,
            IUserRegistrationService userRegistrationService
            ) : base(serviceGetter, cacheManager, eventPublisher)
        {
            _userService = userService;
            _storeService = storeService;
            _permissionService = permissionService;
            _permissionProvider = permissionProvider;
            _userGroupService = userGroupService;
            _moduleService = moduleService;
            _accountingService = accountingService;
            _settingService = settingService;
            _printTemplateService = printTemplateService;
            _userRegistrationService = userRegistrationService;
        }

        /*
        public BaseResult StoreInitData(Store curStore, User curUser, string password, int[] roleIds, int[] groupIds, List<UserRole> userRoles, List<DCMS.Core.Domain.Security.Module> modules, List<PermissionRecord> permissionRecords, DataChannelPermission dataChannelPermission, List<PrintTemplate> printTemplates, List<AccountingOption> accountingOptions)
        {
            var uow = UserRepository.UnitOfWork;
            
            ITransaction transaction = null;
            try
            {
                
                transaction = uow.BeginOrUseTransaction();
                
                
                //添加经销商
                _storeService.InsertStore(curStore);

                curStore.Id= _storeService.GetAllStores(null).FirstOrDefault(c => c.Name == curStore.Name)?.Id ?? 0;

                curUser.StoreId = curStore.Id;
                _userService.InsertUser(curUser);

                if (!String.IsNullOrWhiteSpace(password))
                {
                    var changePassRequest = new ChangePasswordRequest(curUser.Email, false, PasswordFormat.Hashed, password);
                    var changePassResult = _userRegistrationService.ChangePassword(changePassRequest);
                    if (!changePassResult.Success)
                    {
                        foreach (var changePassError in changePassResult.Errors)
                        {
                            //如果事务不存在或者为控则回滚
                            transaction?.Rollback();
                            return new BaseResult { Success = false, Message = "初始化失败" };
                        }
                    }
                }

                curUser.Id= _userService.GetUserByEmail(curUser.Email)?.Id ?? 0;

                var defaultpermissions = ((DefaultPermissionRecord[])_permissionProvider.GetDistributorsDefaultPermissions().ToArray())[0].PermissionRecords.ToList(); //默认权限集合

                userRoles.ForEach(u =>
                {
                    var temRole = new UserRole
                    {
                        StoreId=curStore.Id,
                        Name=u.Name,
                        Active=u.Active,
                        IsSystemRole=u.IsSystemRole,
                        SystemName=u.SystemName,
                        Description=u.Description,
                        EnablePasswordLifetime=u.EnablePasswordLifetime
                    };

                    _userService.InsertUserRole(temRole);
                });

                var curStoreAllRoles = _userService.GetAllUserRolesByStore(false, curStore.Id); //当前经销商角色信息
                if (roleIds != null && roleIds.Length > 0) //当前用户选中的角色集合
                {
                    for (var i = 0; i < roleIds.Length; i++)
                    {
                        //用户和角色
                        int roleId = curStoreAllRoles.FirstOrDefault(f => f.Name == userRoles.FirstOrDefault(c => c.Id == roleIds[i])?.Name)?.Id??0;
                        _userService.InsertUserRoleMapping(curUser.Id, roleId);

                        //频道权限
                        var permission = new DataChannelPermission
                        {
                            StoreId = curStore.Id,
                            UserRoleId = roleId,
                            ViewPurchasePrice = dataChannelPermission.ViewPurchasePrice,
                            PlaceOrderPricePermitsLowerThanMinPriceOnWeb = dataChannelPermission.PlaceOrderPricePermitsLowerThanMinPriceOnWeb,
                            APPSaleBillsAllowPreferences = dataChannelPermission.APPSaleBillsAllowPreferences,
                            APPAdvanceReceiptFormAllowsPreference = dataChannelPermission.APPAdvanceReceiptFormAllowsPreference,
                            MaximumDiscountAmount = dataChannelPermission.MaximumDiscountAmount,
                            APPSaleBillsAllowArrears = dataChannelPermission.APPSaleBillsAllowArrears,
                            AppOpenChoiceGift = dataChannelPermission.AppOpenChoiceGift,
                            PrintingIsNotAudited = dataChannelPermission.PrintingIsNotAudited,
                            AllowViewReportId = dataChannelPermission.AllowViewReportId,
                            APPAllowModificationUserInfo = dataChannelPermission.APPAllowModificationUserInfo,
                            Auditcompleted = dataChannelPermission.Auditcompleted,
                            EnableSchedulingCompleted = dataChannelPermission.EnableSchedulingCompleted,
                            EnableInventoryCompleted = dataChannelPermission.EnableInventoryCompleted,
                            EnableTransfeCompleted = dataChannelPermission.EnableTransfeCompleted,
                            EnableStockEarlyWarning = dataChannelPermission.EnableStockEarlyWarning,
                            EnableUserLossWarning = dataChannelPermission.EnableUserLossWarning,
                            EnableBillingException = dataChannelPermission.EnableBillingException,
                            EnableCompletionOrCancellationOfAccounts = dataChannelPermission.EnableCompletionOrCancellationOfAccounts,
                            EnableApprovalAcl = dataChannelPermission.EnableApprovalAcl,
                            EnableReceivablesAcl = dataChannelPermission.EnableReceivablesAcl,
                            EnableAccountAcl = dataChannelPermission.EnableAccountAcl,
                            EnableDataAuditAcl = dataChannelPermission.EnableDataAuditAcl
                        };

                        _permissionService.InsertDataChannelPermission(permission);
                    }
                }

                //用户与组映射
                if (groupIds != null && groupIds.Length > 0)
                {
                    for (var i = 0; i < groupIds.Length; i++)
                    {
                        _userGroupService.InsertUserGroupUser(groupIds[i], curUser.Id);
                    }
                }

                //模块初始化及模块与角色映射
                modules.ForEach(m =>
                {
                    var entity = new Module
                    {
                        StoreId = curStore.Id,
                        Name = m.Name,
                        ParentId = m.ParentId,
                        LinkUrl = m.LinkUrl,
                        Icon = m.Icon,
                        Controller = m.Controller,
                        Action = m.Action,
                        Code = m.Action,
                        Description = m.Description,
                        IsMenu = m.IsMenu,
                        IsSystem = m.IsSystem,
                        ShowMobile = m.ShowMobile,
                        IsPaltform = m.IsPaltform,
                        Enabled = m.Enabled,
                        CreatedOnUtc = DateTime.Now,
                        DisplayOrder = m.DisplayOrder,
                        LayoutPositionId = m.LayoutPositionId
                    };

                    _moduleService.InsertModule(entity);
                });

                var newModues = _moduleService.GetModulesByStore(curStore.Id).ToList(); //当前经销商所有模块
                if (newModues != null)
                {
                    newModues.ForEach(m =>
                    {
                        for (var i = 0; i < roleIds.Length; i++)
                        {
                            ModuleRole moduleRole = new ModuleRole()
                            {
                                Module_Id = m.Id,
                                UserRole_Id = curStoreAllRoles.FirstOrDefault(f => f.Name == userRoles.FirstOrDefault(c => c.Id == roleIds[i])?.Name)?.Id ?? 0
                            };

                            ModuleRoleRepository.Insert(moduleRole);
                        } 
                    });
                }

                //操作记录权限及记录与角色映射
                permissionRecords.ForEach(p =>
                {
                    var percord = new PermissionRecord
                    {
                        StoreId=curStore.Id,
                        Name=p.Name,
                        SystemName=p.SystemName,
                        Code=p.Code,
                        Description=p.Description,
                        ModuleId=p.ModuleId,
                        ShowMobile=p.ShowMobile,
                        Enabled=p.Enabled,
                        CreatedOn=DateTime.Now
                    };
                    _permissionService.InsertPermissionRecord(percord);

                    if (defaultpermissions.Count(c => c.Name == p.Name) > 0)
                    {
                        for (var r = 0; r < roleIds.Length; r++)
                        {
                            var rolePermission = new PermissionRecordRoles
                            {
                                StoreId = curStore.Id,
                                UserRole_Id = curStoreAllRoles.FirstOrDefault(f => f.Name == userRoles.FirstOrDefault(c => c.Id == roleIds[r])?.Name)?.Id??0,
                                PermissionRecord_Id = p.Id,
                                Platform = 0
                            };

                            _permissionService.InsertPermissionRecordRoles(rolePermission);
                        }
                    }
                });

                accountingOptions.ForEach(o =>
                {
                    var accountOption = new AccountingOption
                    {
                        StoreId=curStore.Id,
                        AccountCodeTypeId=o.AccountCodeTypeId,
                        AccountingTypeId=o.AccountingTypeId,
                        ParentId=o.ParentId,
                        Code=o.Code,
                        Name=o.Name,
                        DisplayOrder=o.DisplayOrder,
                        Enabled=o.Enabled,
                        IsDefault=o.IsDefault,
                        IsLeaf=o.IsLeaf,
                        Balance=o.Balance
                    };

                    _accountingService.InsertAccountingOption(accountOption);
                });

                //打印模板
                printTemplates.ForEach(p =>
                {
                    var temp = new PrintTemplate
                    {
                        StoreId = curStore.Id,
                        TemplateType = p.TemplateType,
                        BillType = p.BillType,
                        Title = p.Title,
                        Content = p.Content
                    };

                    _printTemplateService.InsertPrintTemplate(temp);
                });

                //APP打印配置
                var appPrintSetting = _settingService.LoadSetting<APPPrintSetting>(2);
                _settingService.SaveSetting(appPrintSetting, x => x.AllowPrintPackPrice, curStore.Id, false);
                _settingService.SaveSetting(appPrintSetting, x => x.PrintMode, curStore.Id, false);
                _settingService.SaveSetting(appPrintSetting, x => x.PrintingNumber, curStore.Id, false);
                _settingService.SaveSetting(appPrintSetting, x => x.AllowAutoPrintSalesAndReturn, curStore.Id, false);
                _settingService.SaveSetting(appPrintSetting, x => x.AllowAutoPrintOrderAndReturn, curStore.Id, false);
                _settingService.SaveSetting(appPrintSetting, x => x.AllowAutoPrintAdvanceReceipt, curStore.Id, false);
                _settingService.SaveSetting(appPrintSetting, x => x.AllowAutoPrintArrears, curStore.Id, false);
                _settingService.SaveSetting(appPrintSetting, x => x.AllowPrintOnePass, curStore.Id, false);
                _settingService.SaveSetting(appPrintSetting, x => x.AllowPringMobile, curStore.Id, false);
                _settingService.SaveSetting(appPrintSetting, x => x.AllowPrintingTimeAndNumber, curStore.Id, false);
                _settingService.SaveSetting(appPrintSetting, x => x.AllowPrintCustomerBalance, curStore.Id, false);
                _settingService.SaveSetting(appPrintSetting, x => x.PageHeaderText, curStore.Id, false);
                _settingService.SaveSetting(appPrintSetting, x => x.PageFooterText1, curStore.Id, false);
                _settingService.SaveSetting(appPrintSetting, x => x.PageFooterText2, curStore.Id, false);
                _settingService.SaveSetting(appPrintSetting, x => x.PageHeaderImage, curStore.Id, false);
                _settingService.SaveSetting(appPrintSetting, x => x.PageFooterImage, curStore.Id, false);

                //PC打印配置
                var pcPrintSetting = _settingService.LoadSetting<PCPrintSetting>(2);
                _settingService.SaveSetting(pcPrintSetting, x => x.StoreName, curStore.Id, false);
                _settingService.SaveSetting(pcPrintSetting, x => x.Address, curStore.Id, false);
                _settingService.SaveSetting(pcPrintSetting, x => x.PlaceOrderTelphone, curStore.Id, false);
                _settingService.SaveSetting(pcPrintSetting, x => x.PrintMethod, curStore.Id, false);
                _settingService.SaveSetting(pcPrintSetting, x => x.PaperType, curStore.Id, false);
                _settingService.SaveSetting(pcPrintSetting, x => x.PaperWidth, curStore.Id, false);
                _settingService.SaveSetting(pcPrintSetting, x => x.PaperHeight, curStore.Id, false);
                _settingService.SaveSetting(pcPrintSetting, x => x.BorderType, curStore.Id, false);
                _settingService.SaveSetting(pcPrintSetting, x => x.MarginTop, curStore.Id, false);
                _settingService.SaveSetting(pcPrintSetting, x => x.MarginBottom, curStore.Id, false);
                _settingService.SaveSetting(pcPrintSetting, x => x.MarginLeft, curStore.Id, false);
                _settingService.SaveSetting(pcPrintSetting, x => x.MarginRight, curStore.Id, false);
                _settingService.SaveSetting(pcPrintSetting, x => x.IsPrintPageNumber, curStore.Id, false);
                _settingService.SaveSetting(pcPrintSetting, x => x.PrintHeader, curStore.Id, false);
                _settingService.SaveSetting(pcPrintSetting, x => x.PrintFooter, curStore.Id, false);
                _settingService.SaveSetting(pcPrintSetting, x => x.IsFixedRowNumber, curStore.Id, false);
                _settingService.SaveSetting(pcPrintSetting, x => x.FixedRowNumber, curStore.Id, false);
                _settingService.SaveSetting(pcPrintSetting, x => x.PrintSubtotal, curStore.Id, false);
                _settingService.SaveSetting(pcPrintSetting, x => x.PrintPort, curStore.Id, false);

                //财务配置
                var financeSetting = _settingService.LoadSetting<FinanceSetting>(2);

                _settingService.SaveSetting(financeSetting, x => x.SaleBillAccountingOptionConfiguration, curStore.Id, false);
                _settingService.SaveSetting(financeSetting, x => x.SaleReservationBillAccountingOptionConfiguration, curStore.Id, false);

                _settingService.SaveSetting(financeSetting, x => x.ReturnBillAccountingOptionConfiguration, curStore.Id, false);
                _settingService.SaveSetting(financeSetting, x => x.ReturnReservationBillAccountingOptionConfiguration, curStore.Id, false);

                _settingService.SaveSetting(financeSetting, x => x.ReceiptAccountingOptionConfiguration, curStore.Id, false);
                _settingService.SaveSetting(financeSetting, x => x.PaymentAccountingOptionConfiguration, curStore.Id, false);

                _settingService.SaveSetting(financeSetting, x => x.AdvanceReceiptAccountingOptionConfiguration, curStore.Id, false);
                _settingService.SaveSetting(financeSetting, x => x.AdvancePaymentAccountingOptionConfiguration, curStore.Id, false);

                _settingService.SaveSetting(financeSetting, x => x.PurchaseBillAccountingOptionConfiguration, curStore.Id, false);
                _settingService.SaveSetting(financeSetting, x => x.PurchaseReturnBillAccountingOptionConfiguration, curStore.Id, false);

                _settingService.SaveSetting(financeSetting, x => x.CostExpenditureAccountingOptionConfiguration, curStore.Id, false);

                _settingService.SaveSetting(financeSetting, x => x.FinancialIncomeAccountingOptionConfiguration, curStore.Id, false);

                //商品配置
                var productSetting = _settingService.LoadSetting<ProductSetting>(2);
                _settingService.SaveSetting(productSetting, x => x.SmallUnitSpecificationAttributeOptionsMapping, curStore.Id, false);
                _settingService.SaveSetting(productSetting, x => x.StrokeUnitSpecificationAttributeOptionsMapping, curStore.Id, false);
                _settingService.SaveSetting(productSetting, x => x.BigUnitSpecificationAttributeOptionsMapping, curStore.Id, false);

                //公司配置
                var companySetting = _settingService.LoadSetting<CompanySetting>(2);
                _settingService.SaveSetting(companySetting, x => x.OpenBillMakeDate, curStore.Id, false);
                _settingService.SaveSetting(companySetting, x => x.MulProductPriceUnit, curStore.Id, false);
                _settingService.SaveSetting(companySetting, x => x.AllowCreateMulSameBarcode, curStore.Id, false);
                _settingService.SaveSetting(companySetting, x => x.DefaultPurchasePrice, curStore.Id, false);
                _settingService.SaveSetting(companySetting, x => x.VariablePriceCommodity, curStore.Id, false);
                _settingService.SaveSetting(companySetting, x => x.AccuracyRounding, curStore.Id, false);
                _settingService.SaveSetting(companySetting, x => x.MakeBillDisplayBarCode, curStore.Id, false);
                _settingService.SaveSetting(companySetting, x => x.AllowSelectionDateRange, curStore.Id, false);
                _settingService.SaveSetting(companySetting, x => x.DockingTicketPassSystem, curStore.Id, false);
                _settingService.SaveSetting(companySetting, x => x.AllowReturnInSalesAndOrders, curStore.Id, false);
                _settingService.SaveSetting(companySetting, x => x.AppMaybeDeliveryPersonnel, curStore.Id, false);
                _settingService.SaveSetting(companySetting, x => x.AppSubmitOrderAutoAudits, curStore.Id, false);
                _settingService.SaveSetting(companySetting, x => x.AppSubmitTransferAutoAudits, curStore.Id, false);
                _settingService.SaveSetting(companySetting, x => x.AppSubmitExpenseAutoAudits, curStore.Id, false);
                _settingService.SaveSetting(companySetting, x => x.AppSubmitBillReturnAutoAudits, curStore.Id, false);
                _settingService.SaveSetting(companySetting, x => x.AppAllowWriteBack, curStore.Id, false);
                _settingService.SaveSetting(companySetting, x => x.AllowAdvancePaymentsNegative, curStore.Id, false);
                _settingService.SaveSetting(companySetting, x => x.ShowOnlyPrepaidAccountsWithPrepaidReceipts, curStore.Id, false);
                _settingService.SaveSetting(companySetting, x => x.TasteByTasteAccountingOnlyPrintMainProduct, curStore.Id, false);
                _settingService.SaveSetting(companySetting, x => x.APPOnlyShowHasStockProduct, curStore.Id, false);
                _settingService.SaveSetting(companySetting, x => x.APPShowOrderStock, curStore.Id, false);
                _settingService.SaveSetting(companySetting, x => x.SalesmanDeliveryDistance, curStore.Id, false);
                _settingService.SaveSetting(companySetting, x => x.OnStoreStopSeconds, curStore.Id, false);
                _settingService.SaveSetting(companySetting, x => x.EnableSalesmanTrack, curStore.Id, false);
                _settingService.SaveSetting(companySetting, x => x.Start, curStore.Id, false);
                _settingService.SaveSetting(companySetting, x => x.End, curStore.Id, false);
                _settingService.SaveSetting(companySetting, x => x.FrequencyTimer, curStore.Id, false);
                _settingService.SaveSetting(companySetting, x => x.SalesmanOnlySeeHisCustomer, curStore.Id, false);
                _settingService.SaveSetting(companySetting, x => x.SalesmanVisitStoreBefore, curStore.Id, false);
                _settingService.SaveSetting(companySetting, x => x.SalesmanVisitMustPhotographed, curStore.Id, false);
                _settingService.SaveSetting(companySetting, x => x.ReferenceCostPrice, curStore.Id, false);
                _settingService.SaveSetting(companySetting, x => x.AveragePurchasePriceCalcNumber, curStore.Id, false);
                _settingService.SaveSetting(companySetting, x => x.AllowNegativeInventoryMonthlyClosure, curStore.Id, false);
                _settingService.SaveSetting(companySetting, x => x.EnableTaxRate, curStore.Id, false);
                _settingService.SaveSetting(companySetting, x => x.TaxRate, curStore.Id, false);
                _settingService.SaveSetting(companySetting, x => x.PhotographedWater, curStore.Id, false);
                _settingService.SaveSetting(companySetting, x => x.ClearArchiveDatas, curStore.Id, false);
                _settingService.SaveSetting(companySetting, x => x.ClearBillDatas, curStore.Id, false);

                //清除缓存
                _settingService.ClearCache();

                
                //保存事务
                transaction.Commit();

                return new BaseResult { Success = true, Message = "初始化成功" };
            }
            catch (Exception ex)
            {
                //如果事务不存在或者为控则回滚
                transaction?.Rollback();
                return new BaseResult { Success = false, Message = "初始化失败" };
            }
            finally
            {
                //不管怎样最后都会关闭掉这个事务
                using (transaction) { }
            }
        }
        */
    }
}
