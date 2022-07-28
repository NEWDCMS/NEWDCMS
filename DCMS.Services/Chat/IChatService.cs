using DCMS.Core.Domain.Chat;
using System.Collections.Generic;

namespace DCMS.Services.Chat
{
    public interface IChatService
    {
        void AddMessageToChatByNames(Message message, int storeId, string recieverName, int recieverId);
        ChatRoom GetChatByChatId(int id);
        ChatRoom GetChatByUserIds(int senderId, int recieverId);
        ChatRoom GetChatByUserNames(string user1, string user2);
        IEnumerable<Message> GetMessagesByChatId(int id, int limit = 100);
        IEnumerable<Message> GetMessagesByUserNames(string sender, string reciever);
        IEnumerable<Message> GetMessagesByUsers(int sender, int reciever, int limit = 100);
        void InsertChatRoom(ChatRoom chatRoom);
        void InsertMessage(Message message);
        void InsertUser(User user);
        void UpdateChatRoom(ChatRoom chatRoom);
        void UpdateMessage(Message message);
        void UpdateUser(User user);

     
    }
}