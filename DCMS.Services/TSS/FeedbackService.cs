using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.TSS;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Caching;
using DCMS.Services.Events;
using System;
using System.Linq;
using System.Collections.Generic ;

namespace DCMS.Services.TSS
{
    public class FeedbackService: BaseService, IFeedbackService
    {
        public FeedbackService(IServiceGetter serviceGetter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher) : base(serviceGetter, cacheManager, eventPublisher) { }

        public void InsertFeedback(Feedback feedback)
        {
            if (feedback == null)
            {
                throw new ArgumentNullException("feedback");
            }

            var uow = FeedbackRepository.UnitOfWork;
            FeedbackRepository.Insert(feedback);
            uow.SaveChanges();

            _eventPublisher.EntityInserted(feedback);
        }

        public void UpdateFeedback(Feedback feedback)
        {
            if (feedback == null)
            {
                throw new ArgumentNullException("feedback");
            }

            var uow = FeedbackRepository.UnitOfWork;
            FeedbackRepository.Update(feedback);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityUpdated(feedback);
        }

        public void DeleteFeedback(Feedback feedback)
        {
            if (feedback == null)
            {
                throw new ArgumentNullException("feedback");
            }

            var uow = FeedbackRepository.UnitOfWork;
            FeedbackRepository.Delete(feedback);
            uow.SaveChanges();
            //event notification
            _eventPublisher.EntityDeleted(feedback);
        }

        public Feedback GetFeedbackById(int feedBackId)
        {
            if (feedBackId == 0)
            {
                return null;
            }

            return FeedbackRepository.ToCachedGetById(feedBackId);
        }

        public IPagedList<Feedback> SearchFeedbacks(int? storeId, int? type, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;

            var query = FeedbackRepository.Table;

            //经销商ID
            if (storeId.HasValue && storeId > 0)
            {
                query = query.Where(q => q.StoreId == storeId);
            }

            //消息类型
            if (type.HasValue)
            {
                query = query.Where(q => q.FeedbackTyoe == type);
            }

            var queuedMessages = new PagedList<Feedback>(query, pageIndex, pageSize);
            return queuedMessages;
        }


        public IList<Feedback> Others()
        {
            var queuedMessages = FeedbackRepository.Table
                .Where(q => q.StoreId == 797 && q.FeedbackTyoe == 3)
                .ToList();
            return queuedMessages;
        }
    }
}
