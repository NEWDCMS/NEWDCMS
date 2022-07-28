using DCMS.Core;
using DCMS.Core.Domain.CRM;
using System.Collections.Generic;

namespace DCMS.Services.CRM
{
    public interface IReturnsService
    {
        void DeleteReturn(CRM_RETURN @return);
        void DeleteReturn(IList<CRM_RETURN> returns);
        IPagedList<CRM_RETURN> GetAllReturns(string name = null, int pageIndex = 0, int pageSize = int.MaxValue);
        CRM_RETURN GetReturnById(int returnId);
        IList<CRM_RETURN> GetReturnsByIds(int[] sIds);
        void InsertReturn(CRM_RETURN @return);
        void InsertReturn(IList<CRM_RETURN> returns);
        void UpdateReturn(CRM_RETURN @return);
        void UpdateReturn(IList<CRM_RETURN> returns);
    }
}