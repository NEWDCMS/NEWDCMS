using DCMS.Core.Data;
using DCMS.Core.Domain.OCMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace DCMS.Services
{
    public partial class BaseService
    {
        #region RO
        protected IRepositoryReadOnly<OCMS_CharacterSetting> OCMS_CharacterSettingRepository_RO => _getter.RO<OCMS_CharacterSetting>("OCMS");

        protected IRepositoryReadOnly<OCMS_Products> OCMS_ProductsRepository_RO => _getter.RO<OCMS_Products>("OCMS");
        #endregion
    }
}
