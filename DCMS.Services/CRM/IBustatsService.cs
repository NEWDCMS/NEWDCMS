using DCMS.Core;
using DCMS.Core.Domain.CRM;
using System.Collections.Generic;

namespace DCMS.Services.CRM
{
    public interface IBustatsService
    {
        void DeleteBustat(CRM_BUSTAT bustat);
        void DeleteBustat(IList<CRM_BUSTAT> bustats);
        IPagedList<CRM_BUSTAT> GetAllBustats(string name = null, int pageIndex = 0, int pageSize = int.MaxValue);
        CRM_BUSTAT GetBustatById(int bustatId);
        IList<CRM_BUSTAT> GetBustatsByIds(int[] sIds);
        void InsertBustat(CRM_BUSTAT bustat);
        void InsertBustat(IList<CRM_BUSTAT> bustats);
        void UpdateBustat(CRM_BUSTAT bustat);
        void UpdateBustat(IList<CRM_BUSTAT> bustats);
    }
}