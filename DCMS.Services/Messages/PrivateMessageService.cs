using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Messages;
using DCMS.Core.Domain.Users;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Common;
using DCMS.Services.Events;
using DCMS.Services.Users;
using System;
using System.Linq;
using DCMS.Services.Caching;

namespace DCMS.Services.Messages
{
    public class PrivateMessageService : BaseService, IPrivateMessageService
    {
        private readonly UserSettings _userSettings;
        
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IUserService _userService;

        public PrivateMessageService(
            UserSettings userSettings,
            IServiceGetter getter,
            IStaticCacheManager cacheManager,
           
            IGenericAttributeService genericAttributeService,
            IUserService userService,
            IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {
            _userSettings = userSettings;
            
            _genericAttributeService = genericAttributeService;
            _userService = userService;
        }

        public virtual void DeletePrivateMessage(PrivateMessage privateMessage)
        {
            if (privateMessage == null)
            {
                throw new ArgumentNullException(nameof(privateMessage));
            }

            PrivateMessageRepository.Delete(privateMessage);
            _eventPublisher.EntityDeleted(privateMessage);
        }


        public virtual PrivateMessage GetPrivateMessageById(int privateMessageId)
        {
            if (privateMessageId == 0)
            {
                return null;
            }

            return PrivateMessageRepository.ToCachedGetById(privateMessageId);
        }


        public virtual IPagedList<PrivateMessage> GetAllPrivateMessages(int storeId, int fromUserId,
            int toUserId, bool? isRead, bool? isDeletedByAuthor, bool? isDeletedByRecipient,
            string keywords, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;
            var query = PrivateMessageRepository.Table;
            if (storeId > 0)
            {
                query = query.Where(pm => storeId == pm.StoreId);
            }

            if (fromUserId > 0)
            {
                query = query.Where(pm => fromUserId == pm.FromUserId);
            }

            if (toUserId > 0)
            {
                query = query.Where(pm => toUserId == pm.ToUserId);
            }

            if (isRead.HasValue)
            {
                query = query.Where(pm => isRead.Value == pm.IsRead);
            }

            if (isDeletedByAuthor.HasValue)
            {
                query = query.Where(pm => isDeletedByAuthor.Value == pm.IsDeletedByAuthor);
            }

            if (isDeletedByRecipient.HasValue)
            {
                query = query.Where(pm => isDeletedByRecipient.Value == pm.IsDeletedByRecipient);
            }

            if (!string.IsNullOrEmpty(keywords))
            {
                query = query.Where(pm => pm.Subject.Contains(keywords));
                query = query.Where(pm => pm.Text.Contains(keywords));
            }

            query = query.OrderByDescending(pm => pm.CreatedOnUtc);

            var privateMessages = new PagedList<PrivateMessage>(query, pageIndex, pageSize);

            return privateMessages;
        }


        public virtual void InsertPrivateMessage(PrivateMessage privateMessage)
        {
            if (privateMessage == null)
            {
                throw new ArgumentNullException(nameof(privateMessage));
            }

            PrivateMessageRepository.Insert(privateMessage);

            _eventPublisher.EntityInserted(privateMessage);

            var customerTo = _userService.GetUserById(privateMessage.StoreId, privateMessage.ToUserId);
            if (customerTo == null)
            {
                throw new DCMSException("Recipient could not be loaded");
            }

            //UI通知
            _genericAttributeService.SaveAttribute(customerTo, DCMSDefaults.NotifiedAboutNewPrivateMessagesAttribute, false, privateMessage.StoreId);

            //发送通知(未实现)
            //_workflowMessageService.SendPrivateMessageNotification(privateMessage);

        }


        public virtual void UpdatePrivateMessage(PrivateMessage privateMessage)
        {
            if (privateMessage == null)
            {
                throw new ArgumentNullException(nameof(privateMessage));
            }

            if (privateMessage.IsDeletedByAuthor && privateMessage.IsDeletedByRecipient)
            {
                PrivateMessageRepository.Delete(privateMessage);
                _eventPublisher.EntityDeleted(privateMessage);
            }
            else
            {
                PrivateMessageRepository.Update(privateMessage);
                _eventPublisher.EntityUpdated(privateMessage);
            }
        }
    }
}
