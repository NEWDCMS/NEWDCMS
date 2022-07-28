using DCMS.Core;
using DCMS.Core.Domain.CRM;
using System.Collections.Generic;

namespace DCMS.Services.CRM
{
    public interface IRelationsService
    {
        void DeleteRelation(CRM_RELATION relation);
        void DeleteRelation(IList<CRM_RELATION> relations);
        IPagedList<CRM_RELATION> GetAllRelations(string name = null, int pageIndex = 0, int pageSize = int.MaxValue);
        CRM_RELATION GetRelationById(int relationId);
        IList<CRM_RELATION> GetRelationsByIds(int[] sIds);
        void InsertRelation(CRM_RELATION relation);
        void InsertRelation(IList<CRM_RELATION> relations);
        void UpdateRelation(CRM_RELATION relation);
        void UpdateRelation(IList<CRM_RELATION> relations);
    }
}