using DCMS.Core;
using DCMS.Core.Domain.CRM;
using System.Collections.Generic;

namespace DCMS.Services.CRM
{
    public interface IOrgsService
    {
        void DeleteOrg(CRM_ORG org);
        void DeleteOrg(IList<CRM_ORG> orgs);
        IPagedList<CRM_ORG> GetAllOrgs(string name = null, int pageIndex = 0, int pageSize = int.MaxValue);
        CRM_ORG GetOrgById(int orgId);
        IList<CRM_ORG> GetOrgsByIds(int[] sIds);
        void InsertOrg(CRM_ORG org);
        void InsertOrg(IList<CRM_ORG> orgs);
        void UpdateOrg(CRM_ORG org);
        void UpdateOrg(IList<CRM_ORG> orgs);
    }
}