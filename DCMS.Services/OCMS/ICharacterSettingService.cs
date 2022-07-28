using System;
using System.Collections.Generic;
using System.Text;

namespace DCMS.Services.OCMS
{
    public interface ICharacterSettingService
    {
        bool Exists(string customerCode,int productId);
    }
}
