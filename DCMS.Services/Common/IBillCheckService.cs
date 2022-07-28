using DCMS.Core;
using System;
using System.Collections.Generic;

namespace DCMS.Services.Common
{
    public interface IBillCheckService
    {
        IList<string> CheckAudited(int storeId, BillTypeEnum billTypeEnum, Tuple<DateTime, DateTime> period);
        IList<string> CheckAllBills(int storeId, Tuple<DateTime, DateTime> tuple);
    }
}