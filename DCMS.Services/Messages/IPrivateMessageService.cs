using DCMS.Core;
using DCMS.Core.Domain.Messages;

namespace DCMS.Services.Messages
{
    public interface IPrivateMessageService
    {
        void DeletePrivateMessage(PrivateMessage privateMessage);
        IPagedList<PrivateMessage> GetAllPrivateMessages(int storeId, int fromCustomerId, int toCustomerId, bool? isRead, bool? isDeletedByAuthor, bool? isDeletedByRecipient, string keywords, int pageIndex = 0, int pageSize = int.MaxValue);
        PrivateMessage GetPrivateMessageById(int privateMessageId);
        void InsertPrivateMessage(PrivateMessage privateMessage);
        void UpdatePrivateMessage(PrivateMessage privateMessage);
    }
}