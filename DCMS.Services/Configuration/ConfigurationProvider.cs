using DCMS.Core;
using DCMS.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;


namespace DCMS.Services.Configuration
{
    public class ConfigurationProvider<TSettings> : IConfigurationProvider<TSettings> where TSettings : ISettings, new()
    {
        readonly ISettingService _settingService;

        public ConfigurationProvider(ISettingService settingService)
        {
            _settingService = settingService;
            BuildConfiguration();
        }

        public TSettings Settings { get; protected set; }

        private void BuildConfiguration()
        {
            Settings = Activator.CreateInstance<TSettings>();

            // 获取以写入的属性
            var properties = from prop in typeof(TSettings).GetProperties()
                             where prop.CanWrite && prop.CanRead
                             let setting = _settingService.GetSettingByKey<string>(typeof(TSettings).Name + "." + prop.Name)
                             where setting != null
                             where CommonHelper.GetDCMSCustomTypeConverter(prop.PropertyType).CanConvertFrom(typeof(string))
                             where CommonHelper.GetDCMSCustomTypeConverter(prop.PropertyType).IsValid(setting)
                             let value = CommonHelper.GetDCMSCustomTypeConverter(prop.PropertyType).ConvertFromInvariantString(setting)
                             select new { prop, value };

            // 分配属性
            properties.ToList().ForEach(p => p.prop.SetValue(Settings, p.value, null));
        }

        public void SaveSettings(TSettings settings)
        {
            var properties = from prop in typeof(TSettings).GetProperties()
                             where prop.CanWrite && prop.CanRead
                             where CommonHelper.GetDCMSCustomTypeConverter(prop.PropertyType).CanConvertFrom(typeof(string))
                             select prop;


            foreach (var prop in properties)
            {
                string key = typeof(TSettings).Name + "." + prop.Name;
                //Duck typing is not supported in C#. That's why we're using dynamic type
                dynamic value = prop.GetValue(settings, null);
                if (value != null)
                {
                    _settingService.SetSetting(key, value, 0, false);
                }
                else
                {
                    _settingService.SetSetting(key, "", 0, false);
                }
            }

            //and now clear cache
            //_settingService.ClearCache();

            Settings = settings;
        }

        public void DeleteSettings()
        {
            var properties = from prop in typeof(TSettings).GetProperties()
                             select prop;

            var settingList = new List<DCMS.Core.Domain.Configuration.Setting>();
            foreach (var prop in properties)
            {
                string key = typeof(TSettings).Name + "." + prop.Name;
                var setting = _settingService.GetSettingByKey<DCMS.Core.Domain.Configuration.Setting>(key);
                if (setting != null)
                {
                    settingList.Add(setting);
                }
            }

            foreach (var setting in settingList)
            {
                _settingService.DeleteSetting(setting, setting.StoreId);
            }
        }
    }
}
