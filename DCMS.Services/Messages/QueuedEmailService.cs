using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Messages;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using DCMS.Services.Caching;

namespace DCMS.Services.Tasks
{
    public partial class QueuedEmailService : BaseService, IQueuedEmailService
    {
        private readonly IWorkContext _workContext;
        public QueuedEmailService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IWorkContext workContext,
            IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {
            _workContext = workContext;
        }



        public virtual void InsertQueuedEmail(QueuedEmail queuedEmail)
        {
            if (queuedEmail == null)
            {
                throw new ArgumentNullException("queuedEmail");
            }

            var uow = QueuedEmailRepository.UnitOfWork;
            QueuedEmailRepository.Insert(queuedEmail);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityInserted(queuedEmail);
        }



        public virtual void UpdateQueuedEmail(QueuedEmail queuedEmail)
        {
            if (queuedEmail == null)
            {
                throw new ArgumentNullException("queuedEmail");
            }

            var uow = QueuedEmailRepository.UnitOfWork;
            QueuedEmailRepository.Update(queuedEmail);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityUpdated(queuedEmail);
        }



        public virtual void DeleteQueuedEmail(QueuedEmail queuedEmail)
        {
            if (queuedEmail == null)
            {
                throw new ArgumentNullException("queuedEmail");
            }

            var uow = QueuedEmailRepository.UnitOfWork;
            QueuedEmailRepository.Delete(queuedEmail);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityDeleted(queuedEmail);
        }



        public virtual QueuedEmail GetQueuedEmailById(int queuedEmailId)
        {
            if (queuedEmailId == 0)
            {
                return null;
            }

            return QueuedEmailRepository.ToCachedGetById(queuedEmailId);

        }


        public virtual IList<QueuedEmail> GetQueuedEmailsByIds(int[] queuedEmailIds)
        {
            if (queuedEmailIds == null || queuedEmailIds.Length == 0)
            {
                return new List<QueuedEmail>();
            }

            var query = from qe in QueuedEmailRepository.Table
                        where queuedEmailIds.Contains(qe.Id)
                        select qe;
            var queuedEmails = query.ToList();
            //sort by passed identifiers
            var sortedQueuedEmails = new List<QueuedEmail>();
            foreach (int id in queuedEmailIds)
            {
                var queuedEmail = queuedEmails.Find(x => x.Id == id);
                if (queuedEmail != null)
                {
                    sortedQueuedEmails.Add(queuedEmail);
                }
            }
            return sortedQueuedEmails;
        }

        public virtual IPagedList<QueuedEmail> SearchEmails(string fromEmail,
           string toEmail, DateTime? createdFromUtc, DateTime? createdToUtc,
           bool loadNotSentItemsOnly, int maxSendTries,
           bool loadNewest, int pageIndex, int pageSize)
        {
            if (pageSize >= 50)
                pageSize = 50;
            fromEmail = (fromEmail ?? string.Empty).Trim();
            toEmail = (toEmail ?? string.Empty).Trim();

            var query = QueuedEmailRepository.Table;
            if (!string.IsNullOrEmpty(fromEmail))
            {
                query = query.Where(qe => qe.From.Contains(fromEmail));
            }

            if (!string.IsNullOrEmpty(toEmail))
            {
                query = query.Where(qe => qe.To.Contains(toEmail));
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

            query = query.Where(qe => qe.SentTries < maxSendTries);
            query = query.OrderByDescending(qe => qe.Priority);
            query = loadNewest ?
                ((IOrderedQueryable<QueuedEmail>)query).ThenByDescending(qe => qe.CreatedOnUtc) :
                ((IOrderedQueryable<QueuedEmail>)query).ThenBy(qe => qe.CreatedOnUtc);

            var queuedEmails = new PagedList<QueuedEmail>(query, pageIndex, pageSize);
            return queuedEmails;
        }


        public virtual IPagedList<QueuedEmail> SearchEmails(int? storeId, string from, string to, bool? sentStatus, bool? orderByCreatedOnUtc, int? maxSendTries, DateTime? startTime = null, DateTime? endTime = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = QueuedEmailRepository.Table;

            //经销商ID
            if (storeId.HasValue)
            {
                query = query.Where(q => q.StoreId == storeId);
            }

            //发件人
            if (!string.IsNullOrEmpty(from))
            {
                query = query.Where(q => q.From.Contains(from.Trim()));
            }

            //收件人
            if (!string.IsNullOrEmpty(to))
            {
                query = query.Where(q => q.To.Contains(to.Trim()));
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
                    ((IOrderedQueryable<QueuedEmail>)query).ThenByDescending(qe => qe.CreatedOnUtc) :
                    ((IOrderedQueryable<QueuedEmail>)query).ThenBy(qe => qe.CreatedOnUtc);
            }
            var queuedEmails = new PagedList<QueuedEmail>(query, pageIndex, pageSize);
            return queuedEmails;
        }


    }



    //public partial class QueuedEmailService : IQueuedEmailService
    //{

    //    private readonly IDbContext _dbContext;
    //    private readonly IEventPublisher _eventPublisher;
    //    private readonly IRepository<QueuedEmail> _queuedEmailRepository;

    //    public QueuedEmailService(IDbContext dbContext,
    //        IEventPublisher eventPublisher,
    //        IRepository<QueuedEmail> queuedEmailRepository)
    //    {
    //        _dbContext = dbContext;
    //        _eventPublisher = eventPublisher;
    //        _queuedEmailRepository = queuedEmailRepository;
    //    }


    //    public virtual void InsertQueuedEmail(QueuedEmail queuedEmail)
    //    {
    //        if (queuedEmail == null)
    //            throw new ArgumentNullException(nameof(queuedEmail));

    //        _queuedEmailRepository.Insert(queuedEmail);

    //        //event notification
    //        _eventPublisher.EntityInserted(queuedEmail);
    //    }


    //    public virtual void UpdateQueuedEmail(QueuedEmail queuedEmail)
    //    {
    //        if (queuedEmail == null)
    //            throw new ArgumentNullException(nameof(queuedEmail));

    //        _queuedEmailRepository.Update(queuedEmail);

    //        //event notification
    //        _eventPublisher.EntityUpdated(queuedEmail);
    //    }


    //    public virtual void DeleteQueuedEmail(QueuedEmail queuedEmail)
    //    {
    //        if (queuedEmail == null)
    //            throw new ArgumentNullException(nameof(queuedEmail));

    //        _queuedEmailRepository.Delete(queuedEmail);

    //        //event notification
    //        _eventPublisher.EntityDeleted(queuedEmail);
    //    }


    //    public virtual void DeleteQueuedEmails(IList<QueuedEmail> queuedEmails)
    //    {
    //        if (queuedEmails == null)
    //            throw new ArgumentNullException(nameof(queuedEmails));

    //        _queuedEmailRepository.Delete(queuedEmails);

    //        //event notification
    //        foreach (var queuedEmail in queuedEmails)
    //        {
    //            _eventPublisher.EntityDeleted(queuedEmail);
    //        }
    //    }


    //    public virtual QueuedEmail GetQueuedEmailById(int queuedEmailId)
    //    {
    //        if (queuedEmailId == 0)
    //            return null;

    //        return _queuedEmailRepository.ToCachedGetById(queuedEmailId);
    //    }


    //    public virtual IList<QueuedEmail> GetQueuedEmailsByIds(int[] queuedEmailIds)
    //    {
    //        if (queuedEmailIds == null || queuedEmailIds.Length == 0)
    //            return new List<QueuedEmail>();

    //        var query = from qe in _queuedEmailRepository.Table
    //                    where queuedEmailIds.Contains(qe.Id)
    //                    select qe;
    //        var queuedEmails = query.ToList();
    //        //sort by passed identifiers
    //        var sortedQueuedEmails = new List<QueuedEmail>();
    //        foreach (var id in queuedEmailIds)
    //        {
    //            var queuedEmail = queuedEmails.Find(x => x.Id == id);
    //            if (queuedEmail != null)
    //                sortedQueuedEmails.Add(queuedEmail);
    //        }

    //        return sortedQueuedEmails;
    //    }


    //    public virtual IPagedList<QueuedEmail> SearchEmails(string fromEmail,
    //        string toEmail, DateTime? createdFromUtc, DateTime? createdToUtc,
    //        bool loadNotSentItemsOnly, bool loadOnlyItemsToBeSent, int maxSendTries,
    //        bool loadNewest, int pageIndex = 0, int pageSize = int.MaxValue)
    //    {
    //        fromEmail = (fromEmail ?? string.Empty).Trim();
    //        toEmail = (toEmail ?? string.Empty).Trim();

    //        var query = _queuedEmailRepository.Table;
    //        if (!string.IsNullOrEmpty(fromEmail))
    //            query = query.Where(qe => qe.From.Contains(fromEmail));
    //        if (!string.IsNullOrEmpty(toEmail))
    //            query = query.Where(qe => qe.To.Contains(toEmail));
    //        if (createdFromUtc.HasValue)
    //            query = query.Where(qe => qe.CreatedOnUtc >= createdFromUtc);
    //        if (createdToUtc.HasValue)
    //            query = query.Where(qe => qe.CreatedOnUtc <= createdToUtc);
    //        if (loadNotSentItemsOnly)
    //            query = query.Where(qe => !qe.SentOnUtc.HasValue);
    //        if (loadOnlyItemsToBeSent)
    //        {
    //            var nowUtc = DateTime.UtcNow;
    //            query = query.Where(qe => !qe.DontSendBeforeDateUtc.HasValue || qe.DontSendBeforeDateUtc.Value <= nowUtc);
    //        }

    //        query = query.Where(qe => qe.SentTries < maxSendTries);
    //        query = loadNewest ?
    //            //load the newest records
    //            query.OrderByDescending(qe => qe.CreatedOnUtc) :
    //            //load by priority
    //            query.OrderByDescending(qe => qe.PriorityId).ThenBy(qe => qe.CreatedOnUtc);

    //        var queuedEmails = new PagedList<QueuedEmail>(query, pageIndex, pageSize);
    //        return queuedEmails;
    //    }


    //    public virtual void DeleteAllEmails()
    //    {
    //        //do all databases support "Truncate command"?
    //        var queuedEmailTableName = _dbContext.GetTableName<QueuedEmail>();
    //        _dbContext.ExecuteSqlCommand($"TRUNCATE TABLE [{queuedEmailTableName}]");

    //        //var queuedEmails = _queuedEmailRepository.Table.ToList();
    //        //foreach (var qe in queuedEmails)
    //        //    _queuedEmailRepository.Delete(qe);
    //    }

    //}
}
