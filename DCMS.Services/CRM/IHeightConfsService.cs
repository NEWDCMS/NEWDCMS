using DCMS.Core;
using DCMS.Core.Domain.CRM;
using System.Collections.Generic;

namespace DCMS.Services.CRM
{
    public interface IHeightConfsService
    {
        void DeleteHeightConf(CRM_HEIGHT_CONF heightConf);
        void DeleteHeightConf(IList<CRM_HEIGHT_CONF> heightConfs);
        IPagedList<CRM_HEIGHT_CONF> GetAllHeightConfs(string name = null, int pageIndex = 0, int pageSize = int.MaxValue);
        CRM_HEIGHT_CONF GetHeightConfById(int heightConfId);
        IList<CRM_HEIGHT_CONF> GetHeightConfsByIds(int[] sIds);
        void InsertHeightConf(CRM_HEIGHT_CONF heightConf);
        void InsertHeightConf(IList<CRM_HEIGHT_CONF> heightConfs);
        void UpdateHeightConf(CRM_HEIGHT_CONF heightConf);
        void UpdateHeightConf(IList<CRM_HEIGHT_CONF> heightConfs);
    }
}