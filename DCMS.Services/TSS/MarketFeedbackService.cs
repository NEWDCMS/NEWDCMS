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
    public class MarketFeedbackService : BaseService, IMarketFeedbackService
    {

        public MarketFeedbackService(IServiceGetter serviceGetter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher) : base(serviceGetter, cacheManager, eventPublisher)
        {
        }

        public void InsertMarketFeedback(MarketFeedback marketFeedback)
        {
            if (marketFeedback == null)
            {
                throw new ArgumentNullException("marketFeedback");
            }

            var uow = MarketFeedbackRepository.UnitOfWork;
            MarketFeedbackRepository.Insert(marketFeedback);
            uow.SaveChanges();

            _eventPublisher.EntityInserted(marketFeedback);
        }

        public void UpdateMarketFeedback(MarketFeedback marketFeedback)
        {
            if (marketFeedback == null)
            {
                throw new ArgumentNullException("marketFeedback");
            }

            var uow = MarketFeedbackRepository.UnitOfWork;
            MarketFeedbackRepository.Update(marketFeedback);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityUpdated(marketFeedback);
        }

        public void DeleteMarketFeedback(MarketFeedback marketFeedback)
        {
            if (marketFeedback == null)
            {
                throw new ArgumentNullException("marketFeedback");
            }

            var uow = MarketFeedbackRepository.UnitOfWork;
            MarketFeedbackRepository.Delete(marketFeedback);
            uow.SaveChanges();
            //event notification
            _eventPublisher.EntityDeleted(marketFeedback);
        }

        public MarketFeedback GetMarketFeedbackById(int feedBackId)
        {
            if (feedBackId == 0)
            {
                return null;
            }

            return MarketFeedbackRepository.ToCachedGetById(feedBackId);
        }

        public IPagedList<MarketFeedback> SearchMarketFeedbacks(int? storeId, int? userId, int? type, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;

            var query = MarketFeedbackRepository.Table;

            //经销商ID
            if (storeId.HasValue && storeId > 0)
            {
                query = query.Where(q => q.StoreId == storeId);
            }

            if (userId.HasValue && userId > 0)
            {
                query = query.Where(q => q.UserId == userId);

            }
            var queuedMessages = new PagedList<MarketFeedback>(query, pageIndex, pageSize);
            return queuedMessages;
        }

    }
}
