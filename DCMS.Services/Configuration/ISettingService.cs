using DCMS.Core;
using DCMS.Core.Configuration;
using DCMS.Core.Domain.Configuration;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DCMS.Services.Configuration
{

    public partial interface ISettingService
    {

        Setting GetSettingById(int settingId);

        void DeleteSetting(Setting setting, int storeId);


        void DeleteSettings(IList<Setting> settings, int storeId);


        Setting GetSetting(string key, int storeId = 0, bool loadSharedValueIfNotFound = false);


        T GetSettingByKey<T>(string key, T defaultValue = default,
            int storeId = 0, bool loadSharedValueIfNotFound = false);


        void SetSetting<T>(string key, T value, int storeId = 0, bool clearCache = true);


        IList<Setting> GetAllSettings(int storeId = 0);

        bool SettingExists<T, TPropType>(T settings,
            Expression<Func<T, TPropType>> keySelector, int storeId = 0)
            where T : ISettings, new();

        T LoadSetting<T>(int storeId = 0) where T : ISettings, new();


        ISettings LoadSetting(Type type, int storeId = 0);

        void SaveSetting<T>(T settings, int storeId = 0) where T : ISettings, new();

        void SaveSetting<T, TPropType>(T settings,
            Expression<Func<T, TPropType>> keySelector,
            int storeId = 0, bool clearCache = true) where T : ISettings, new();


        void SaveSettingOverridablePerStore<T, TPropType>(T settings,
            Expression<Func<T, TPropType>> keySelector,
            bool overrideForStore, int storeId = 0, bool clearCache = true) where T : ISettings, new();


        void DeleteSetting<T>(int storeId = 0) where T : ISettings, new();


        void DeleteSetting<T, TPropType>(T settings,
            Expression<Func<T, TPropType>> keySelector, int storeId = 0) where T : ISettings, new();

        void ClearCache(int? store);
        void ClearData(int? store);

        string GetSettingKey<TSettings, T>(TSettings settings, Expression<Func<TSettings, T>> keySelector)
            where TSettings : ISettings, new();

        bool AppBillAutoAudits(int storeId, BillTypeEnum billTypeEnum);
    }
}
