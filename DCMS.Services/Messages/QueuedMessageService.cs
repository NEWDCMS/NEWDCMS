using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Tasks;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using DCMS.Services.Caching;

namespace DCMS.Services.Tasks
{
    public partial class QueuedMessageService : BaseService, IQueuedMessageService
    {
        public QueuedMessageService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {

        }

        public virtual void InsertQueuedMessage(List<string> ToUsers, QueuedMessage queuedMessage)
        {
            var messages = new List<QueuedMessage>();
            var uow = QueuedMessageRepository.UnitOfWork;
            ToUsers.ForEach(to => 
            {
                if (queuedMessage != null)
                {
                    var msg = new QueuedMessage()
                    {
                        StoreId = queuedMessage.StoreId,
                        MType = queuedMessage.MType,
                        Title = queuedMessage.Title,
                        Date = queuedMessage.Date,
                        BillType = queuedMessage.BillType,
                        BillNumber = queuedMessage.BillNumber,
                        BillId = queuedMessage.BillId,
                        CreatedOnUtc = queuedMessage.CreatedOnUtc,
                        ToUser = to,
                        BusinessUser=queuedMessage.BusinessUser,
                        Distance=queuedMessage.Distance,
                        TerminalName=queuedMessage.TerminalName,
                        TerminalNames=queuedMessage.TerminalNames,
                        Month=queuedMessage.Month,
                        Amount=queuedMessage.Amount,
                        BillNumbers=queuedMessage.BillNumbers,
                        ProductNames=queuedMessage.ProductNames
                    };
                    messages.Add(msg);
                }
            });
            QueuedMessageRepository.Insert(messages);
            uow.SaveChanges();
        }



        public virtual void UpdateQueuedMessage(QueuedMessage queuedMessage)
        {
            if (queuedMessage == null)
            {
                throw new ArgumentNullException("queuedMessage");
            }

            var uow = QueuedMessageRepository.UnitOfWork;
            QueuedMessageRepository.Update(queuedMessage);
            uow.SaveChanges();
            //event notification
            _eventPublisher.EntityUpdated(queuedMessage);
        }



        public virtual void DeleteQueuedMessage(QueuedMessage queuedMessage)
        {
            if (queuedMessage == null)
            {
                throw new ArgumentNullException("queuedMessage");
            }

            var uow = QueuedMessageRepository.UnitOfWork;
            QueuedMessageRepository.Delete(queuedMessage);
            uow.SaveChanges();
            //event notification
            _eventPublisher.EntityDeleted(queuedMessage);
        }



        public virtual QueuedMessage GetQueuedMessageById(int queuedMessageId)
        {
            if (queuedMessageId == 0)
            {
                return null;
            }

            return QueuedMessageRepository.ToCachedGetById(queuedMessageId);

        }


        public virtual IList<QueuedMessage> GetQueuedMessagesByIds(int[] queuedMessageIds)
        {
            if (queuedMessageIds == null || queuedMessageIds.Length == 0)
            {
                return new List<QueuedMessage>();
            }

            var query = from qe in QueuedMessageRepository.Table
                        where queuedMessageIds.Contains(qe.Id)
                        select qe;
            var queuedMessages = query.ToList();
            //sort by passed identifiers
            var sortedQueuedMessages = new List<QueuedMessage>();
            foreach (int id in queuedMessageIds)
            {
                var queuedMessage = queuedMessages.Find(x => x.Id == id);
                if (queuedMessage != null)
                {
                    sortedQueuedMessages.Add(queuedMessage);
                }
            }
            return sortedQueuedMessages;
        }


        public virtual IList<QueuedMessage> SearchMessages(int? storeId,
             int? mType, DateTime? createdFromUtc, int maxSendTries, DateTime? createdToUtc,
            bool loadNotSentItemsOnly,
            bool loadNewest)
        {

            var query = QueuedMessageRepository.TableNoTracking;

            if (storeId.HasValue)
            {
                query = query.Where(qe => qe.StoreId == storeId);
            }

            if (mType.HasValue)
            {
                query = query.Where(qe => qe.MTypeId == mType);
            }

            if (createdFromUtc.HasValue)
            {
                query = query.Where(qe => qe.CreatedOnUtc >= createdFromUtc);
            }

            if (createdToUtc.HasValue)
            {
                query = query.Where(qe => qe.CreatedOnUtc <= createdToUtc);
            }

            if (loadNotSentItemsOnly)
            {
                query = query.Where(qe => !qe.SentOnUtc.HasValue);
            }

            query = query.Where(qe => qe.IsRead == false);
            //query = query.Where(qe => qe.SentTries < maxSendTries);

            query = query.OrderByDescending(qe => qe.Priority);
            query = loadNewest ?
                ((IOrderedQueryable<QueuedMessage>)query).ThenByDescending(qe => qe.CreatedOnUtc) :
                ((IOrderedQueryable<QueuedMessage>)query).ThenBy(qe => qe.CreatedOnUtc);

            return query.ToList();
        }

        public virtual IPagedList<QueuedMessage> SearchMessages(int? storeId, int[] mTypeId,string toUser, bool? sentStatus, bool? orderByCreatedOnUtc, int? maxSendTries, DateTime? startTime = null, DateTime? endTime = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;
            var query = QueuedMessageRepository.Table;

            //经销商ID
            if (storeId.HasValue)
            {
                query = query.Where(q => q.StoreId == storeId);
            }

            if (!string.IsNullOrEmpty(toUser))
            {
                query = query.Where(q => q.ToUser == toUser);
            }

            //消息类型
            if (mTypeId != null && mTypeId.Count() > 0)
            {
                query = query.Where(qe => mTypeId.Contains(qe.MTypeId));
            }

            //发送状态
            if (sentStatus.HasValue)
            {
                //已发送
                if (sentStatus.Value)
                {
                    query = query.Where(q => q.SentOnUtc.HasValue);
                }
                //未发送
                else
                {
                    query = query.Where(q => !q.SentOnUtc.HasValue);
                }
            }

            //开始时间
            if (startTime.HasValue)
            {
                query = query.Where(qe => qe.CreatedOnUtc >= startTime);
            }

            //结束时间
            if (endTime.HasValue)
            {
                query = query.Where(qe => qe.CreatedOnUtc <= endTime);
            }

            //发送次数大于
            if (maxSendTries.HasValue)
            {
                query = query.Where(q => q.SentTries >= maxSendTries);
            }

            //按创建日期排序
            query = query.OrderByDescending(qe => qe.CreatedOnUtc);
            if (orderByCreatedOnUtc != null)
            {
                query = orderByCreatedOnUtc.Value ?
                    ((IOrderedQueryable<QueuedMessage>)query).ThenByDescending(qe => qe.CreatedOnUtc) :
                    ((IOrderedQueryable<QueuedMessage>)query).ThenBy(qe => qe.CreatedOnUtc);
            }
            var queuedMessages = new PagedList<QueuedMessage>(query, pageIndex, pageSize);
            return queuedMessages;
        }

        public virtual void WriteLogs(string message = "")
        {
            //LogLevel logLevel, string shortMessage, 
        }
    }
}
