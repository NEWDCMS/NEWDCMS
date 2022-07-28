using System;
using System.Linq;
using DCMS.Core;
using DCMS.Core.Data;
using DCMS.Core.Domain.Users;
using DCMS.Core.Domain.Messages;
using DCMS.Data;
using DCMS.Data.Extensions;
using DCMS.Services.Users;
using DCMS.Services.Events;

namespace DCMS.Services.Messages
{
    /// <summary>
    /// Newsletter subscription service
    /// </summary>
    public class NewsLetterSubscriptionService : INewsLetterSubscriptionService
    {
        #region Fields

        private readonly IUserService _userService;
        private readonly IDbContext _context;
        private readonly IEventPublisher _eventPublisher;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<NewsLetterSubscription> _subscriptionRepository;

        #endregion

        #region Ctor

        public NewsLetterSubscriptionService(IUserService userService,
            IDbContext context,
            IEventPublisher eventPublisher,
            IRepository<User> userRepository,
            IRepository<NewsLetterSubscription> subscriptionRepository)
        {
            _userService = userService;
            _context = context;
            _eventPublisher = eventPublisher;
            _userRepository = userRepository;
            _subscriptionRepository = subscriptionRepository;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Publishes the subscription event.
        /// </summary>
        /// <param name="subscription">The newsletter subscription.</param>
        /// <param name="isSubscribe">if set to <c>true</c> [is subscribe].</param>
        /// <param name="publishSubscriptionEvents">if set to <c>true</c> [publish subscription events].</param>
        private void PublishSubscriptionEvent(NewsLetterSubscription subscription, bool isSubscribe, bool publishSubscriptionEvents)
        {
            if (!publishSubscriptionEvents) 
                return;

            if (isSubscribe)
            {
                _eventPublisher.PublishNewsletterSubscribe(subscription);
            }
            else
            {
                _eventPublisher.PublishNewsletterUnsubscribe(subscription);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Inserts a newsletter subscription
        /// </summary>
        /// <param name="newsLetterSubscription">NewsLetter subscription</param>
        /// <param name="publishSubscriptionEvents">if set to <c>true</c> [publish subscription events].</param>
        public virtual void InsertNewsLetterSubscription(NewsLetterSubscription newsLetterSubscription, bool publishSubscriptionEvents = true)
        {
            if (newsLetterSubscription == null)
            {
                throw new ArgumentNullException(nameof(newsLetterSubscription));
            }

            //Handle e-mail
            newsLetterSubscription.Email = CommonHelper.EnsureSubscriberEmailOrThrow(newsLetterSubscription.Email);

            //Persist
            _subscriptionRepository.Insert(newsLetterSubscription);

            //Publish the subscription event 
            if (newsLetterSubscription.Active)
            {
                PublishSubscriptionEvent(newsLetterSubscription, true, publishSubscriptionEvents);
            }

            //Publish event
            _eventPublisher.EntityInserted(newsLetterSubscription);
        }

        /// <summary>
        /// Updates a newsletter subscription
        /// </summary>
        /// <param name="newsLetterSubscription">NewsLetter subscription</param>
        /// <param name="publishSubscriptionEvents">if set to <c>true</c> [publish subscription events].</param>
        public virtual void UpdateNewsLetterSubscription(NewsLetterSubscription newsLetterSubscription, bool publishSubscriptionEvents = true)
        {
            if (newsLetterSubscription == null)
            {
                throw new ArgumentNullException(nameof(newsLetterSubscription));
            }

            //Handle e-mail
            newsLetterSubscription.Email = CommonHelper.EnsureSubscriberEmailOrThrow(newsLetterSubscription.Email);

            //Get original subscription record
            var originalSubscription = _context.LoadOriginalCopy(newsLetterSubscription);

            //Persist
            _subscriptionRepository.Update(newsLetterSubscription);

            //Publish the subscription event 
            if ((originalSubscription.Active == false && newsLetterSubscription.Active) ||
                (newsLetterSubscription.Active && originalSubscription.Email != newsLetterSubscription.Email))
            {
                //If the previous entry was false, but this one is true, publish a subscribe.
                PublishSubscriptionEvent(newsLetterSubscription, true, publishSubscriptionEvents);
            }

            if (originalSubscription.Active && newsLetterSubscription.Active &&
                originalSubscription.Email != newsLetterSubscription.Email)
            {
                //If the two emails are different publish an unsubscribe.
                PublishSubscriptionEvent(originalSubscription, false, publishSubscriptionEvents);
            }

            if (originalSubscription.Active && !newsLetterSubscription.Active)
            {
                //If the previous entry was true, but this one is false
                PublishSubscriptionEvent(originalSubscription, false, publishSubscriptionEvents);
            }

            //Publish event
            _eventPublisher.EntityUpdated(newsLetterSubscription);
        }

        /// <summary>
        /// Deletes a newsletter subscription
        /// </summary>
        /// <param name="newsLetterSubscription">NewsLetter subscription</param>
        /// <param name="publishSubscriptionEvents">if set to <c>true</c> [publish subscription events].</param>
        public virtual void DeleteNewsLetterSubscription(NewsLetterSubscription newsLetterSubscription, bool publishSubscriptionEvents = true)
        {
            if (newsLetterSubscription == null) throw new ArgumentNullException(nameof(newsLetterSubscription));

            _subscriptionRepository.Delete(newsLetterSubscription);

            //Publish the unsubscribe event 
            PublishSubscriptionEvent(newsLetterSubscription, false, publishSubscriptionEvents);

            //event notification
            _eventPublisher.EntityDeleted(newsLetterSubscription);
        }

        /// <summary>
        /// Gets a newsletter subscription by newsletter subscription identifier
        /// </summary>
        /// <param name="newsLetterSubscriptionId">The newsletter subscription identifier</param>
        /// <returns>NewsLetter subscription</returns>
        public virtual NewsLetterSubscription GetNewsLetterSubscriptionById(int newsLetterSubscriptionId)
        {
            if (newsLetterSubscriptionId == 0) return null;

            return _subscriptionRepository.GetById(newsLetterSubscriptionId);
        }

        /// <summary>
        /// Gets a newsletter subscription by newsletter subscription GUID
        /// </summary>
        /// <param name="newsLetterSubscriptionGuid">The newsletter subscription GUID</param>
        /// <returns>NewsLetter subscription</returns>
        public virtual NewsLetterSubscription GetNewsLetterSubscriptionByGuid(Guid newsLetterSubscriptionGuid)
        {
            if (newsLetterSubscriptionGuid == Guid.Empty) return null;

            var newsLetterSubscriptions = from nls in _subscriptionRepository.Table
                                          where nls.NewsLetterSubscriptionGuid == newsLetterSubscriptionGuid
                                          orderby nls.Id
                                          select nls;

            return newsLetterSubscriptions.FirstOrDefault();
        }

        /// <summary>
        /// Gets a newsletter subscription by email and store ID
        /// </summary>
        /// <param name="email">The newsletter subscription email</param>
        /// <param name="storeId">Store identifier</param>
        /// <returns>NewsLetter subscription</returns>
        public virtual NewsLetterSubscription GetNewsLetterSubscriptionByEmailAndStoreId(string email, int storeId)
        {
            if (!CommonHelper.IsValidEmail(email))
                return null;

            email = email.Trim();

            var newsLetterSubscriptions = from nls in _subscriptionRepository.Table
                                          where nls.Email == email && nls.StoreId == storeId
                                          orderby nls.Id
                                          select nls;

            return newsLetterSubscriptions.FirstOrDefault();
        }


        #endregion
    }
}