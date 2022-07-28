using DCMS.Web.Framework.Models;


namespace DCMS.ViewModel.Models.Configuration
{
    /// <summary>
    /// Represents a setting mode model
    /// </summary>
    public partial class SettingModeModel : BaseModel
    {
        #region Properties

        public string ModeName { get; set; }

        public bool Enabled { get; set; }

        #endregion
    }
}