using DCMS.Core;
using DCMS.Core.Domain.CRM;
using System.Collections.Generic;

namespace DCMS.Services.CRM
{
    public interface IZsntm0040Service
    {
        void DeleteZsntm0040(CRM_ZSNTM0040 zsntm0040);
        void DeleteZsntm0040(IList<CRM_ZSNTM0040> zsntm0040s);
        IPagedList<CRM_ZSNTM0040> GetAllZsntm0040s(string name = null, int pageIndex = 0, int pageSize = int.MaxValue);
        CRM_ZSNTM0040 GetZsntm0040ById(int zsntm0040Id);
        IList<CRM_ZSNTM0040> GetZsntm0040sByIds(int[] sIds);
        void InsertZsntm0040(CRM_ZSNTM0040 zsntm0040);
        void InsertZsntm0040(IList<CRM_ZSNTM0040> zsntm0040s);
        void UpdateZsntm0040(CRM_ZSNTM0040 zsntm0040);
        void UpdateZsntm0040(IList<CRM_ZSNTM0040> zsntm0040s);
    }
}