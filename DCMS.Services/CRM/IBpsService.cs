using DCMS.Core;
using DCMS.Core.Domain.CRM;
using System.Collections.Generic;

namespace DCMS.Services.CRM
{
    public interface IBpsService
    {
        void DeleteBps(CRM_BP bp);
        void DeleteBps(IList<CRM_BP> bps);
        IPagedList<CRM_BP> GetAllBpss(string name = null, int pageIndex = 0, int pageSize = int.MaxValue);
        CRM_BP GetBpsById(int bpId);
        IList<CRM_BP> GetBpssByIds(int[] sIds);
        void InsertBps(CRM_BP bp);
        void InsertBps(IList<CRM_BP> bps);
        void UpdateBps(CRM_BP bp);
        void UpdateBps(IList<CRM_BP> bps);
    }
}