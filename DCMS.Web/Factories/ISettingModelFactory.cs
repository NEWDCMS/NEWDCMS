using DCMS.ViewModel.Models.Configuration;

namespace DCMS.Web.Factories
{
    public interface ISettingModelFactory
    {
        SettingModeModel PrepareSettingModeModel(string modeName);
    }
}