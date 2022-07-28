using DCMS.Web.Framework.Models;

namespace DCMS.ViewModel.Models.Common
{

    public partial class BackupFileModel : BaseModel
    {
        #region Properties

        public string Name { get; set; }

        public string Length { get; set; }

        public string Link { get; set; }

        #endregion
    }
}