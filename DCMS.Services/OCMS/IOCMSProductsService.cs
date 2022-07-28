using DCMS.Core.Domain.OCMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace DCMS.Services.OCMS
{
    public interface IOCMSProductsService
    {
        OCMS_Products FindByCode(string code);
    }
}
