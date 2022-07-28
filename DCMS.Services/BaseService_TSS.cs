using DCMS.Core.Data;
using DCMS.Core.Domain.TSS;
using DCMS.Core.Domain.Chat;

namespace DCMS.Services
{
    /// <summary>
    /// 服务基类
    /// </summary>
    public partial class BaseService
    {

        #region TSS
        #region RO

        protected IRepositoryReadOnly<Feedback> FeedbackRepository_RO => _getter.RO<Feedback>(TSS);
        protected IRepositoryReadOnly<MarketFeedback> MarketFeedbackRepository_RO => _getter.RO<MarketFeedback>(TSS);
        protected IRepositoryReadOnly<ChatRoom> ChatRoomRepository_RO => _getter.RO<ChatRoom>(TSS);
        protected IRepositoryReadOnly<Message> MessageRepository_RO => _getter.RO<Message>(TSS);
        protected IRepositoryReadOnly<User> ChatUserRepository_RO => _getter.RO<User>(TSS);

        #endregion

        #region RW

        protected IRepository<Feedback> FeedbackRepository => _getter.RW<Feedback>(TSS);
        protected IRepository<MarketFeedback> MarketFeedbackRepository => _getter.RW<MarketFeedback>(TSS);
        protected IRepository<ChatRoom> ChatRoomRepository => _getter.RW<ChatRoom>(TSS);
        protected IRepository<Message> MessageRepository => _getter.RW<Message>(TSS);
        protected IRepository<User> ChatUserRepository => _getter.RW<User>(TSS);

        #endregion
        #endregion

    }

}
