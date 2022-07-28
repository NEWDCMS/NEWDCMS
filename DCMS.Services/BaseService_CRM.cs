using DCMS.Core.Data;
using DCMS.Core.Domain.CRM;
using DCMS.Core.Domain.Stores;
using DCMS.Core.Domain.Terminals;

namespace DCMS.Services
{
    /// <summary>
    /// 服务基类
    /// </summary>
    public partial class BaseService
    {

        #region CRM

        #region RO

        protected IRepositoryReadOnly<Store> StoreRepository_RO => _getter.RO<Store>(CRM);
        protected IRepositoryReadOnly<Terminal> TerminalsRepository_RO => _getter.RO<Terminal>(CRM);
        protected IRepositoryReadOnly<NewTerminal> NewTerminalRepository_RO => _getter.RO<NewTerminal>(CRM);
        protected IRepositoryReadOnly<CRM_RELATION> CRM_RELATIONRepository_RO => _getter.RO<CRM_RELATION>(CRM);
        protected IRepositoryReadOnly<CRM_RETURN> CRM_RETURNRepository_RO => _getter.RO<CRM_RETURN>(CRM);
        protected IRepositoryReadOnly<CRM_ORG> CRM_ORGRepository_RO => _getter.RO<CRM_ORG>(CRM);
        protected IRepositoryReadOnly<CRM_BP> CRM_BPRepository_RO => _getter.RO<CRM_BP>(CRM);
        protected IRepositoryReadOnly<CRM_ZSNTM0040> CRM_ZSNTM0040Repository_RO => _getter.RO<CRM_ZSNTM0040>(CRM);
        protected IRepositoryReadOnly<CRM_HEIGHT_CONF> CRM_HEIGHT_CONFRepository_RO => _getter.RO<CRM_HEIGHT_CONF>(CRM);
        protected IRepositoryReadOnly<CRM_BUSTAT> CRM_BUSTATRepository_RO => _getter.RO<CRM_BUSTAT>(CRM);


        #endregion

        #region RW

        protected IRepository<Store> StoreRepository => _getter.RW<Store>(CRM);
        protected IRepository<Terminal> TerminalsRepository => _getter.RW<Terminal>(CRM);
        protected IRepository<NewTerminal> NewTerminalRepository => _getter.RW<NewTerminal>(CRM);
        protected IRepository<CRM_RELATION> CRM_RELATIONRepository => _getter.RW<CRM_RELATION>(CRM);
        protected IRepository<CRM_RETURN> CRM_RETURNRepository => _getter.RW<CRM_RETURN>(CRM);
        protected IRepository<CRM_ORG> CRM_ORGRepository => _getter.RW<CRM_ORG>(CRM);
        protected IRepository<CRM_BP> CRM_BPRepository => _getter.RW<CRM_BP>(CRM);
        protected IRepository<CRM_ZSNTM0040> CRM_ZSNTM0040Repository => _getter.RW<CRM_ZSNTM0040>(CRM);
        protected IRepository<CRM_HEIGHT_CONF> CRM_HEIGHT_CONFRepository => _getter.RW<CRM_HEIGHT_CONF>(CRM);
        protected IRepository<CRM_BUSTAT> CRM_BUSTATRepository => _getter.RW<CRM_BUSTAT>(CRM);


        #endregion

        #endregion

    }

}
