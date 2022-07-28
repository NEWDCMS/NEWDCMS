using DCMS.Core.Data;
using DCMS.Core.Domain.Messages;
using DCMS.Core.Domain.Tasks;

namespace DCMS.Services
{
    /// <summary>
    /// 服务基类
    /// </summary>
    public partial class BaseService
    {

        #region SKD

        #region RO
        protected IRepositoryReadOnly<ScheduleTask> ScheduleTaskRepository_RO => _getter.RO<ScheduleTask>(SKD);
        protected IRepositoryReadOnly<QueuedEmail> QueuedEmailRepository_RO => _getter.RO<QueuedEmail>(SKD);
        protected IRepositoryReadOnly<EmailAccount> EmailAccountRepository_RO => _getter.RO<EmailAccount>(SKD);
        protected IRepositoryReadOnly<QueuedMessage> QueuedMessageRepository_RO => _getter.RO<QueuedMessage>(SKD);
        #endregion

        #region RW
        protected IRepository<ScheduleTask> ScheduleTaskRepository => _getter.RW<ScheduleTask>(SKD);
        protected IRepository<QueuedEmail> QueuedEmailRepository => _getter.RW<QueuedEmail>(SKD);
        protected IRepository<EmailAccount> EmailAccountRepository => _getter.RW<EmailAccount>(SKD);
        protected IRepository<QueuedMessage> QueuedMessageRepository => _getter.RW<QueuedMessage>(SKD);
        #endregion

        #endregion

    }

}
