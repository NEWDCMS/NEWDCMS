using DCMS.Core.Caching;
using DCMS.Core.Domain.Chat;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.SignalR;


namespace DCMS.Services.Chat
{
    public class ChatService : BaseService, IChatService
    {
        public ChatService(IServiceGetter serviceGetter,
         IStaticCacheManager cacheManager,
         IEventPublisher eventPublisher) : base(serviceGetter, cacheManager, eventPublisher) { }

        public void InsertChatRoom(ChatRoom chatRoom)
        {
            if (chatRoom == null)
            {
                throw new ArgumentNullException("chatRoom");
            }

            var uow = ChatRoomRepository.UnitOfWork;
            ChatRoomRepository.Insert(chatRoom);
            uow.SaveChanges();

            _eventPublisher.EntityInserted(chatRoom);
        }
        public void UpdateChatRoom(ChatRoom chatRoom)
        {
            if (chatRoom == null)
            {
                throw new ArgumentNullException("chatRoom");
            }

            var uow = ChatRoomRepository.UnitOfWork;
            ChatRoomRepository.Update(chatRoom);
            uow.SaveChanges();

            _eventPublisher.EntityUpdated(chatRoom);
        }

        public void InsertMessage(Message message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            var uow = MessageRepository.UnitOfWork;
            message.Time = DateTime.Now;
            MessageRepository.Insert(message);
            uow.SaveChanges();

            _eventPublisher.EntityInserted(message);
        }
        public void UpdateMessage(Message message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            var uow = MessageRepository.UnitOfWork;
            MessageRepository.Update(message);
            uow.SaveChanges();

            _eventPublisher.EntityUpdated(message);
        }

        public ChatRoom GetChatByUserNames(string user1, string user2)
        {
            var query = ChatRoomRepository.Table;
            if (query.Count() < 1)
                return null;

            ChatRoom filterChats = query.FirstOrDefault(c => c.SenderName == user1 && c.RecieverName == user2 || c.SenderName == user2 && c.RecieverName == user1);

            return filterChats;
        }
        public ChatRoom GetChatByUserIds(int senderId, int recieverId)
        {
            var query = ChatRoomRepository.Table;
            if (query.Count() < 1)
                return null;

            ChatRoom filterChats = query.FirstOrDefault(c => c.SenderId == senderId && c.RecieverId == recieverId || c.SenderId == recieverId && c.RecieverId == senderId);

            return filterChats;
        }

        public ChatRoom GetChatByChatId(int id) => ChatRoomRepository.Table.FirstOrDefault(c => c.Id == id);

        public IEnumerable<Message> GetMessagesByChatId(int id, int limit = 100)
        {
            var query = ChatRoomRepository.Table;
            ChatRoom chat = query.FirstOrDefault(c => c.Id == id);
            if (chat != null)
            {
                var mquery = MessageRepository.Table.Where(s => s.ChatId == chat.Id);
                //mquery = mquery.Skip(0).Take(100);
                //mquery = mquery.Skip(pageIndex * pageSize).Take(pageSize);
                mquery = mquery.OrderByDescending(s => s.Time).Take(limit);
                return mquery.OrderBy(s => s.Time).ToList();
            }
            else
                return new List<Message>();
          
        }
        public void AddMessageToChatByNames(Message message, int storeId, string recieverName, int recieverId)
        {
            var chat = GetChatByUserIds(message.SenderId, recieverId);
            if (chat == null)
            {
                chat = new ChatRoom
                {
                    StoreId = storeId,
                    SenderName = message.SenderName,
                    SenderId = message.SenderId,
                    RecieverName = recieverName,
                    RecieverId = recieverId
                };

                InsertChatRoom(chat);

                message.ChatId = chat.Id;

                InsertMessage(message);

                return;
            }

            message.ChatId = chat.Id;
            InsertMessage(message);
        }


        public void InsertUser(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            var uow = ChatUserRepository.UnitOfWork;
            ChatUserRepository.Insert(user);
            uow.SaveChanges();

            _eventPublisher.EntityInserted(user);
        }
        public void UpdateUser(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            var uow = ChatUserRepository.UnitOfWork;
            ChatUserRepository.Update(user);
            uow.SaveChanges();

            _eventPublisher.EntityUpdated(user);
        }

        /// <summary>
        /// 获取消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="reciever"></param>
        /// <returns></returns>
        public IEnumerable<Message> GetMessagesByUserNames(string sender, string reciever)
        {
            ChatRoom chat = GetChatByUserNames(sender, reciever);
            if (chat != null)
                return GetMessagesByChatId(chat.Id);
            else
                return new List<Message>();
        }

        /// <summary>
        /// 获取消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="reciever"></param>
        /// <returns></returns>
        public IEnumerable<Message> GetMessagesByUsers(int sender, int reciever, int limit = 100)
        {
            ChatRoom chat = GetChatByUserIds(sender, reciever);
            if (chat != null)
                return GetMessagesByChatId(chat.Id, limit);
            else
                return new List<Message>();
        }
    }
}
