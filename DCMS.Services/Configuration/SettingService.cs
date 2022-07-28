using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Configuration;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Caching.Extensions;
using DCMS.Services.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DCMS.Services.Caching;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DCMS.Services.Configuration
{

    /// <summary>
    /// 系统配置服务
    /// </summary>
    public partial class SettingService : BaseService, ISettingService
    {
        public SettingService(IStaticCacheManager cacheManager,
            IServiceGetter getter,
            IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {
        }


        protected virtual IDictionary<string, IList<Setting>> GetAllSettingsDictionary(int storeId)
        {
            //cache
            var settings = GetAllSettings(storeId);

            var dictionary = new Dictionary<string, IList<Setting>>();
            foreach (var s in settings)
            {
                var resourceName = s.Name.ToLowerInvariant();
                var settingForCaching = new Setting
                {
                    Id = s.Id,
                    Name = s.Name,
                    Value = s.Value,
                    StoreId = s.StoreId
                };
                if (!dictionary.ContainsKey(resourceName))
                {
                    //first setting
                    dictionary.Add(resourceName, new List<Setting>
                        {
                            settingForCaching
                        });
                }
                else
                {
                    dictionary[resourceName].Add(settingForCaching);
                }
            }

            return dictionary;
        }




        /// <summary>
        /// 添加配置
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="clearCache"></param>
        public virtual void InsertSetting(Setting setting, bool clearCache = true)
        {
            if (setting == null)
            {
                throw new ArgumentNullException("setting");
            }

            var uow = SettingsRepository.UnitOfWork;
            SettingsRepository.Insert(setting);
            uow.SaveChanges();

            if (clearCache)
                ClearCache(setting.StoreId);

            //event notification
            _eventPublisher.EntityInserted(setting);
        }

        /// <summary>
        /// 更新配置
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="clearCache"></param>
        public virtual void UpdateSetting(DCMS.Core.Domain.Configuration.Setting setting, bool clearCache = true)
        {
            if (setting == null)
            {
                throw new ArgumentNullException("setting");
            }

            var uow = SettingsRepository.UnitOfWork;
            SettingsRepository.Update(setting);
            uow.SaveChanges();

            if (clearCache)
                ClearCache(setting.StoreId);

            _eventPublisher.EntityUpdated(setting);
        }

        public virtual void ClearCache(int store)
        {
            _cacheManager.RemoveByPrefix("DCMS.Setting");
            _cacheManager.RemoveByPrefix(String.Format(DCMSDefaults.SETTINGS_PK, store));
        }

        /// <summary>
        /// 删除配置
        /// </summary>
        /// <param name="setting"></param>
        public virtual void DeleteSetting(Setting setting, int store)
        {
            if (setting == null)
            {
                throw new ArgumentNullException("setting");
            }

            var uow = SettingsRepository.UnitOfWork;
            SettingsRepository.Delete(setting);
            uow.SaveChanges();

            ClearCache(setting.StoreId);

            //event notification
            _eventPublisher.EntityDeleted(setting);
        }


        public virtual void DeleteSettings(IList<Setting> settings, int storeId)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            var uow = SettingsRepository.UnitOfWork;
            SettingsRepository.Delete(settings);
            uow.SaveChanges();

            //cache
            ClearCache(storeId);

            //event notification
            foreach (var setting in settings)
            {
                _eventPublisher.EntityDeleted(setting);
            }
        }

        /// <summary>
        /// 获取指定配置
        /// </summary>
        /// <param name="settingId"></param>
        /// <returns></returns>
        public virtual Setting GetSettingById(int settingId)
        {
            if (settingId == 0)
            {
                return null;
            }

            return SettingsRepository.ToCachedGetById(settingId);
        }


        /// <summary>
        /// 根据指定的Key获取配置值
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="key">Key</param>
        /// <param name="storeId">经销商标识</param>
        /// <param name="loadSharedValueIfNotFound">找不到某个特定值时是否应加载共享（对于所有存储）值</param>
        /// <returns></returns>
        public virtual Setting GetSetting(string key, int storeId = 0, bool loadSharedValueIfNotFound = false)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            var settings = GetAllSettingsDictionary(storeId);
            key = key.Trim().ToLowerInvariant();
            if (!settings.ContainsKey(key))
                return null;

            var settingsByKey = settings[key];
            var setting = settingsByKey.FirstOrDefault(x => x.StoreId == storeId);

            //load shared value?
            if (setting == null && storeId > 0 && loadSharedValueIfNotFound)
                setting = settingsByKey.FirstOrDefault(x => x.StoreId == 0);

            return setting != null ? GetSettingById(setting.Id) : null;
        }

        /// <summary>
        ///  设置指定Key的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="storeId"></param>
        /// <param name="clearCache"></param>
        public virtual void SetSetting<T>(string key, T value, int storeId = 0, bool clearCache = true)
        {
            SetSetting(typeof(T), key, value, storeId, clearCache);
        }

        /// <summary>
        /// 设置指定Key的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="storeId"></param>
        /// <param name="clearCache"></param>
        protected virtual void SetSetting(Type type, string key, object value, int storeId = 0, bool clearCache = true)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            key = key.Trim().ToLowerInvariant();

            var valueStr = TypeDescriptor.GetConverter(type).ConvertToInvariantString(value);
            if (valueStr == "(Collection)")
            {
                var t = TypeDescriptor.GetConverter(type).GetType();
                valueStr = JsonConvert.SerializeObject(value);
                System.Diagnostics.Debug.Print(valueStr);
            }

            var allSettings = GetAllSettingsDictionary(storeId);
            var settingForCaching = allSettings.ContainsKey(key) ?
                allSettings[key].FirstOrDefault(x => x.StoreId == storeId) : null;

            if (settingForCaching != null)
            {
                //update
                var setting = GetSettingById(settingForCaching.Id);
                setting.Value = valueStr;
                UpdateSetting(setting, clearCache);
            }
            else
            {
                //insert
                var setting = new Setting
                {
                    Name = key,
                    Value = valueStr,
                    StoreId = storeId
                };
                InsertSetting(setting, clearCache);
            }
        }

        /// <summary>
        /// 获取全部配置
        /// </summary>
        /// <returns></returns>
        public virtual IList<Setting> GetAllSettings(int storeId = 0)
        {
            var query = (from s in SettingsRepository.TableNoTracking
                         orderby s.Name, s.StoreId
                         select s).Where(s => s.StoreId == storeId);

            var cache = new CacheKey(string.Format(DCMSDefaults.SETTINGS_PK, storeId));
            var settings = query.ToCachedList(cache);
            return settings;
        }

        /// <summary>
        /// 确定设置是否存在
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <typeparam name="TPropType">属性类型</typeparam>
        /// <param name="settings">Entity</param>
        /// <param name="keySelector">Key 选择器</param>
        /// <param name="storeId">经销商标识</param>
        /// <returns>true -存在; false - 不存在</returns>
        public virtual bool SettingExists<T, TPropType>(T settings,
            Expression<Func<T, TPropType>> keySelector, int storeId = 0)
            where T : ISettings, new()
        {
            var key = GetSettingKey(settings, keySelector);

            var setting = GetSettingByKey<string>(key, storeId: storeId);
            return setting != null;
        }


        public virtual T GetSettingByKey<T>(string key, T defaultValue = default, int storeId = 0, bool loadSharedValueIfNotFound = false)
        {
            if (string.IsNullOrEmpty(key))
                return defaultValue;

            var settings = GetAllSettingsDictionary(storeId);
            key = key.Trim().ToLowerInvariant();
            if (!settings.ContainsKey(key))
                return defaultValue;

            var settingsByKey = settings[key];
            var setting = settingsByKey.FirstOrDefault(x => x.StoreId == storeId);

            //load shared value?
            if (setting == null && storeId > 0 && loadSharedValueIfNotFound)
                setting = settingsByKey.FirstOrDefault(x => x.StoreId == 0);

            return setting != null ? CommonHelper.To<T>(setting.Value) : defaultValue;
        }

        public virtual T LoadSetting<T>(int storeId = 0) where T : ISettings, new()
        {
            return (T)LoadSetting(typeof(T), storeId);
        }
        /// <summary>
        ///  加载配置
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="storeId">经销商</param>
        public virtual ISettings LoadSetting(Type type, int storeId = 0)
        {
            var settings = Activator.CreateInstance(type);

            foreach (var prop in type.GetProperties())
            {
                // get properties we can read and write to
                if (!prop.CanRead || !prop.CanWrite)
                    continue;

                var key = type.Name + "." + prop.Name;
                //load by store
                var setting = GetSettingByKey<string>(key, storeId: storeId, loadSharedValueIfNotFound: true);
                if (setting == null)
                    continue;

                var isGenericType = false;
                if (key.ToLower() == "companysetting.salesmanmanagements")
                {
                    isGenericType = prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>);
                }

                if (!TypeDescriptor.GetConverter(prop.PropertyType).CanConvertFrom(typeof(string)) && !isGenericType)
                    continue;

                if (!TypeDescriptor.GetConverter(prop.PropertyType).IsValid(setting) && !isGenericType)
                    continue;

                if (isGenericType && key.ToLower() == "companysetting.salesmanmanagements")
                {
                    //var valueStr = TypeDescriptor.GetConverter(prop.PropertyType).ConvertToInvariantString(setting);
                    var value = JsonConvert.DeserializeObject<List<SalesmanManagement>>(setting);
                    //设置属性
                    prop.SetValue(settings, value, null);
                }
                else
                {
                    var value = TypeDescriptor.GetConverter(prop.PropertyType).ConvertFromInvariantString(setting);
                    //设置属性
                    prop.SetValue(settings, value, null);
                }
            }

            return settings as ISettings;
        }

        /// <summary>
        /// 配置配置对象
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="storeId">经销商</param>
        /// <param name="settings">配置实例</param>
        public virtual void SaveSetting<T>(T settings, int storeId = 0) where T : ISettings, new()
        {

            foreach (var prop in typeof(T).GetProperties())
            {
                if (!prop.CanRead || !prop.CanWrite)
                    continue;

                if (!TypeDescriptor.GetConverter(prop.PropertyType).CanConvertFrom(typeof(string)))
                    continue;

                var key = typeof(T).Name + "." + prop.Name;
                var value = prop.GetValue(settings, null);
                if (value != null)
                    SetSetting(prop.PropertyType, key, value, storeId, false);
                else
                    SetSetting(key, string.Empty, storeId, false);
            }

            //and now clear cache
            ClearCache(storeId);
        }

        /// <summary>
        /// 保存配置对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TPropType"></typeparam>
        /// <param name="settings"></param>
        /// <param name="keySelector"></param>
        /// <param name="storeId"></param>
        /// <param name="clearCache"></param>
        public virtual void SaveSetting<T, TPropType>(T settings,
                  Expression<Func<T, TPropType>> keySelector,
                  int storeId = 0, bool clearCache = true) where T : ISettings, new()
        {
            if (!(keySelector.Body is MemberExpression member))
            {
                throw new ArgumentException($"Expression '{keySelector}' refers to a method, not a property.");
            }

            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
            {
                throw new ArgumentException($"Expression '{keySelector}' refers to a field, not a property.");
            }

            var key = GetSettingKey(settings, keySelector);
            var value = (TPropType)propInfo.GetValue(settings, null);

            //companysetting.salesmanmanagements
            //(Collection)
            //if (key == "companysetting.salesmanmanagements")
            //{
            //    System.Diagnostics.Debug.Print(key);
            //}

            if (value != null)
                SetSetting(key, value, storeId, clearCache);
            else
                SetSetting(key, string.Empty, storeId, clearCache);
        }

        public virtual void SaveSettingOverridablePerStore<T, TPropType>(T settings,
    Expression<Func<T, TPropType>> keySelector,
    bool overrideForStore, int storeId = 0, bool clearCache = true) where T : ISettings, new()
        {
            if (overrideForStore || storeId == 0)
                SaveSetting(settings, keySelector, storeId, clearCache);
            else if (storeId > 0)
                DeleteSetting(settings, keySelector, storeId);
        }

        /// <summary>
        /// 删除全部配置
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        public virtual void DeleteSetting<T>(int storeId = 0) where T : ISettings, new()
        {
            var settingsToDelete = new List<Setting>();
            var allSettings = GetAllSettings(storeId);
            foreach (var prop in typeof(T).GetProperties())
            {
                var key = typeof(T).Name + "." + prop.Name;
                settingsToDelete.AddRange(allSettings.Where(x => x.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase)));
            }

            DeleteSettings(settingsToDelete, storeId);
        }

        public virtual void DeleteSetting<T, TPropType>(T settings,
             Expression<Func<T, TPropType>> keySelector, int storeId = 0) where T : ISettings, new()
        {
            var key = GetSettingKey(settings, keySelector);
            key = key.Trim().ToLowerInvariant();

            var allSettings = GetAllSettingsDictionary(storeId);
            var settingForCaching = allSettings.ContainsKey(key) ?
                allSettings[key].FirstOrDefault(x => x.StoreId == storeId) : null;
            if (settingForCaching == null)
                return;

            //update
            var setting = GetSettingById(settingForCaching.Id);
            DeleteSetting(setting, storeId);
        }

        /// <summary>
        /// 清除配置缓存
        /// </summary>
        public virtual void ClearCache(int? store)
        {
            _cacheManager.RemoveByPrefix("DCMS.Setting");
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.SETTINGS_PK, store));
        }

        public virtual void ClearData(int? store)
        {
            var sqlBuilder = new System.Text.StringBuilder();

            if (store.HasValue)
            {
                sqlBuilder.Append("DELETE FROM `dcms`.`StockInOutRecords` where StoreId = " + store.Value + ";");
                sqlBuilder.Append("DELETE FROM `dcms`.`StockFlows` where StoreId = " + store.Value + ";");
                sqlBuilder.Append("DELETE FROM `dcms`.`StockInOutRecords_StockFlows_Mapping` where StoreId = " + store.Value + ";");
                sqlBuilder.Append("DELETE FROM `dcms`.`Stocks` where StoreId = " + store.Value + ";");
                sqlBuilder.Append("DELETE FROM `dcms`.`StockInOutDetails` where StoreId = " + store.Value + ";");

                sqlBuilder.Append("DELETE FROM `dcms`.`PurchaseItems` where StoreId = " + store.Value + ";");
                sqlBuilder.Append("DELETE FROM `dcms`.`PurchaseReturnBill_Accounting_Mapping` where StoreId = " + store.Value + ";");
                sqlBuilder.Append("DELETE FROM `dcms`.`PurchaseBills` where StoreId = " + store.Value + ";");


                sqlBuilder.Append("DELETE FROM `dcms`.`SaleItems`  where StoreId = " + store.Value + ";");
                sqlBuilder.Append("DELETE FROM `dcms`.`SaleBill_Accounting_Mapping`  where StoreId = " + store.Value + ";");
                sqlBuilder.Append("DELETE FROM `dcms`.`SaleBills` where StoreId = " + store.Value + ";");

                sqlBuilder.Append("DELETE FROM `dcms`.`SaleReservationItems` where StoreId = " + store.Value + ";");
                sqlBuilder.Append("DELETE FROM `dcms`.`SaleReservationBill_Accounting_Mapping` where StoreId = " + store.Value + ";");
                sqlBuilder.Append("DELETE FROM `dcms`.`SaleReservationBills` where StoreId = " + store.Value + ";");

                sqlBuilder.Append("DELETE FROM `dcms`.`ReturnItems`  where StoreId = " + store.Value + ";");
                sqlBuilder.Append("DELETE FROM `dcms`.`ReturnBill_Accounting_Mapping` where StoreId = " + store.Value + ";");
                sqlBuilder.Append("DELETE FROM `dcms`.`ReturnBills`  where StoreId = " + store.Value + ";");

                sqlBuilder.Append("DELETE FROM `dcms`.`ReturnReservationItems` where StoreId = " + store.Value + ";");
                sqlBuilder.Append("DELETE FROM `dcms`.`ReturnReservationBill_Accounting_Mapping` where StoreId = " + store.Value + ";");
                sqlBuilder.Append("DELETE FROM `dcms`.`ReturnReservationBills` where StoreId = " + store.Value + ";");

                sqlBuilder.Append("DELETE FROM `dcms`.`VoucherItems` where StoreId = " + store.Value + ";");
                sqlBuilder.Append("DELETE FROM `dcms`.`RecordingVouchers`  where StoreId = " + store.Value + ";");

                sqlBuilder.Append("DELETE FROM `dcms`.`AdvanceReceiptBills`  where StoreId = " + store.Value + ";");
                sqlBuilder.Append("DELETE FROM `dcms`.`AdvancePaymentBills`  where StoreId = " + store.Value + ";");

                sqlBuilder.Append("DELETE FROM `dcms`.`PurchaseBill_Accounting_Mapping`  where StoreId = " + store.Value + ";");
                sqlBuilder.Append("DELETE FROM `dcms`.`PurchaseReturnBill_Accounting_Mapping`  where StoreId = " + store.Value + ";");

                sqlBuilder.Append("DELETE FROM `dcms`.`CashReceiptBills` where StoreId = " + store.Value + ";");
                sqlBuilder.Append("DELETE FROM `dcms`.`CashReceiptItems` where StoreId = " + store.Value + ";");

                sqlBuilder.Append("DELETE FROM `dcms`.`PaymentReceiptBills` where StoreId = " + store.Value + ";");
                sqlBuilder.Append("DELETE FROM `dcms`.`PaymentReceiptItems` where StoreId = " + store.Value + ";");

                sqlBuilder.Append("DELETE FROM `dcms`.`FinancialIncomeBills` where StoreId = " + store.Value + ";");
                sqlBuilder.Append("DELETE FROM `dcms`.`FinancialIncomeItems` where StoreId = " + store.Value + ";");
                sqlBuilder.Append("DELETE FROM `dcms`.`FinancialIncomeBill_Accounting_Mapping` where StoreId = " + store.Value + ";");

                sqlBuilder.Append("DELETE FROM `dcms`.`CostContractBills` where StoreId = " + store.Value + ";");
                sqlBuilder.Append("DELETE FROM `dcms`.`CostContractItems` where StoreId = " + store.Value + ";");
                sqlBuilder.Append("DELETE FROM `dcms`.`CostExpenditureBills` where StoreId = " + store.Value + ";");
                sqlBuilder.Append("DELETE FROM `dcms`.`CostExpenditureItems`  where StoreId = " + store.Value + ";");

                sqlBuilder.Append("DELETE FROM `dcms`.`DispatchBills` where StoreId = " + store.Value + ";");
                sqlBuilder.Append("DELETE FROM `dcms`.`DispatchItems` where StoreId = " + store.Value + ";");
                sqlBuilder.Append("DELETE FROM `dcms`.`AllocationBills` where StoreId = " + store.Value + ";");
                sqlBuilder.Append(" DELETE FROM `dcms`.`AllocationItems` where StoreId = " + store.Value + ";");
                sqlBuilder.Append("DELETE FROM `dcms`.`DeliverySigns` where StoreId = " + store.Value + ";");

                sqlBuilder.Append("DELETE FROM `dcms`.`PaymentReceiptBill_Accounting_Mapping` where StoreId = " + store.Value + ";");
                sqlBuilder.Append("DELETE FROM `dcms`.`FinancialIncomeBill_Accounting_Mapping` where StoreId = " + store.Value + ";");
                sqlBuilder.Append("DELETE FROM `dcms`.`FinanceReceiveAccountBills` where StoreId = " + store.Value + ";");
                sqlBuilder.Append("DELETE FROM `dcms`.`FinanceReceiveAccountBill_Accounting_Mapping` where StoreId = " + store.Value + ";");
                sqlBuilder.Append("DELETE FROM `dcms`.`CostExpenditureBill_Accounting_Mapping`  where StoreId = " + store.Value + ";");
                sqlBuilder.Append("DELETE FROM `dcms`.`CashReceiptBill_Accounting_Mapping` where StoreId = " + store.Value + ";");
                sqlBuilder.Append("DELETE FROM `dcms`.`AdvanceReceiptBill_Accounting_Mapping`  where StoreId = " + store.Value + ";");
                sqlBuilder.Append("DELETE FROM `dcms`.`AdvancePaymentBill_Accounting_Mapping` where StoreId = " + store.Value + ";");
                sqlBuilder.Append("DELETE FROM `dcms`.`SaleReservationBill_Accounting_Mapping` where StoreId = " + store.Value + ";");
                sqlBuilder.Append("DELETE FROM `dcms`.`SaleBill_Accounting_Mapping` where StoreId = " + store.Value + ";");
                sqlBuilder.Append("DELETE FROM `dcms`.`ReturnReservationBill_Accounting_Mapping` where StoreId = " + store.Value + ";");
                sqlBuilder.Append("DELETE FROM `dcms`.`ReturnBill_Accounting_Mapping` where StoreId = " + store.Value + ";");

                SettingsRepository.ExecuteSqlScript(sqlBuilder.ToString());
            }
        }

        public virtual string GetSettingKey<TSettings, T>(TSettings settings, Expression<Func<TSettings, T>> keySelector)
       where TSettings : ISettings, new()
        {
            if (!(keySelector.Body is MemberExpression member))
                throw new ArgumentException($"Expression '{keySelector}' refers to a method, not a property.");

            if (!(member.Member is PropertyInfo propInfo))
                throw new ArgumentException($"Expression '{keySelector}' refers to a field, not a property.");

            var key = $"{typeof(TSettings).Name}.{propInfo.Name}";

            return key;
        }

        /// <summary>
        /// App单据是否自动审核
        /// </summary>
        /// <param name="storeId">经销商Id</param>
        /// <param name="billTypeEnum">单据类型</param>
        /// <returns></returns>
        public bool AppBillAutoAudits(int storeId, BillTypeEnum billTypeEnum)
        {
            bool result = false;
            try
            {
                CompanySetting companySetting = LoadSetting<CompanySetting>(storeId);
                if (companySetting != null)
                {
                    switch (billTypeEnum)
                    {
                        //App保存销售订单/退货订单后，系统自动审核
                        case BillTypeEnum.SaleReservationBill:
                        result = companySetting.AppSubmitOrderAutoAudits;
                        break;
                        // App保存销售订单/退货订单后，系统自动审核
                        case BillTypeEnum.ReturnReservationBill:
                        result = companySetting.AppSubmitOrderAutoAudits;
                        break;
                        // App保存调拨单后，系统自动审核
                        case BillTypeEnum.AllocationBill:
                        result = companySetting.AppSubmitTransferAutoAudits;
                        break;
                        // App保存费用支出单后，系统自动审核
                        case BillTypeEnum.CostExpenditureBill:
                        result = companySetting.AppSubmitExpenseAutoAudits;
                        break;
                        // App保存销售单/销售退货单后，系统自动审核
                        case BillTypeEnum.SaleBill:
                        result = companySetting.AppSubmitBillReturnAutoAudits;
                        break;
                        //App保存销售单/销售退货单后，系统自动审核
                        case BillTypeEnum.ReturnBill:
                        result = companySetting.AppSubmitBillReturnAutoAudits;
                        break;
                        // APP提交收款单后，系统自动审核
                        case BillTypeEnum.CashReceiptBill:
                        result = companySetting.AutoApproveConsumerPaidBill;
                        break;
                        default:
                        break;
                    }
                }
                return result;
            }
            catch (Exception)
            {
                return false;
            }

        }


    }
}