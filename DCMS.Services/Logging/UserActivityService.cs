using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Common;
using DCMS.Core.Domain.Logging;
using DCMS.Core.Domain.Users;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using DCMS.Services.Caching;

namespace DCMS.Services.Logging
{

    public class UserActivityService : BaseService, IUserActivityService
    {
        #region Fields

        private readonly CommonSettings _commonSettings;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        #endregion

        [Serializable]
        public class ActivityLogTypeForCaching
        {
            /// <summary>
            /// Identifier
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// System keyword
            /// </summary>
            public string SystemKeyword { get; set; }

            /// <summary>
            /// Name
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Enabled
            /// </summary>
            public bool Enabled { get; set; }
        }


        #region Ctor
        public UserActivityService(IStaticCacheManager cacheManager,
            IServiceGetter getter,
            IWorkContext workContext,
            IWebHelper webHelper,
            CommonSettings commonSettings,
            IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {
            _commonSettings = commonSettings;
            _webHelper = webHelper;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        protected virtual IList<ActivityLogTypeForCaching> GetAllActivityTypesCached()
        {
            //cache
            return _cacheManager.Get(DCMSLoggingDefaults.ActivityTypeAllCacheKey.FillCacheKey(), () =>
            {
                var result = new List<ActivityLogTypeForCaching>();
                var activityLogTypes = GetAllActivityTypes(0);
                foreach (var alt in activityLogTypes)
                {
                    var altForCaching = new ActivityLogTypeForCaching
                    {
                        Id = alt.Id,
                        SystemKeyword = alt.SystemKeyword,
                        Name = alt.Name,
                        Enabled = alt.Enabled
                    };
                    result.Add(altForCaching);
                }

                return result;
            });
        }


        /// <summary>
        /// Inserts an activity log type item
        /// </summary>
        /// <param name="activityLogType">Activity log type item</param>
        public virtual void InsertActivityType(ActivityLogType activityLogType)
        {
            if (activityLogType == null)
            {
                throw new ArgumentNullException("activityLogType");
            }

            var uow = ActivityLogTypeRepository.UnitOfWork;
            ActivityLogTypeRepository.Insert(activityLogType);
            uow.SaveChanges();

            _eventPublisher.EntityInserted(activityLogType);
        }

        public virtual ActivityLog InsertActivity(User user, string systemKeyword, string comment, BaseEntity entity = null)
        {
            if (user == null)
            {
                return null;
            }

            var uow = ActivityLogRepository.UnitOfWork;
            //try to get activity log type by passed system keyword
            var activityLogType = GetAllActivityTypesCached().FirstOrDefault(type => type.SystemKeyword.Equals(systemKeyword));
            if (!activityLogType?.Enabled ?? true)
            {
                return null;
            }

            //insert log item
            var logItem = new ActivityLog
            {
                ActivityLogTypeId = activityLogType.Id,
                //EntityId = entity?.Id,
                //EntityName = entity?.GetUnproxiedEntityType().Name,
                UserId = user.Id,
                Comment = CommonHelper.EnsureMaximumLength(comment ?? string.Empty, 4000),
                CreatedOnUtc = DateTime.UtcNow
                //IpAddress = _webHelper.GetCurrentIpAddress()
            };
            ActivityLogRepository.Insert(logItem);
            uow.SaveChanges();

            _eventPublisher.EntityInserted(logItem);

            return logItem;
        }



        /// <summary>
        /// Updates an activity log type item
        /// </summary>
        /// <param name="activityLogType">Activity log type item</param>
        public virtual void UpdateActivityType(ActivityLogType activityLogType)
        {
            if (activityLogType == null)
            {
                throw new ArgumentNullException("activityLogType");
            }

            var uow = ActivityLogTypeRepository.UnitOfWork;
            ActivityLogTypeRepository.Update(activityLogType);
            uow.SaveChanges();

            _eventPublisher.EntityUpdated(activityLogType);
        }

        /// <summary>
        /// Deletes an activity log type item
        /// </summary>
        /// <param name="activityLogType">Activity log type</param>
        public virtual void DeleteActivityType(ActivityLogType activityLogType)
        {
            if (activityLogType == null)
            {
                throw new ArgumentNullException("activityLogType");
            }

            var uow = ActivityLogTypeRepository.UnitOfWork;
            ActivityLogTypeRepository.Delete(activityLogType);
            uow.SaveChanges();

            _eventPublisher.EntityDeleted(activityLogType);
        }

        /// <summary>
        /// Gets all activity log type items
        /// </summary>
        /// <returns>Activity log type collection</returns>
        public virtual IList<ActivityLogType> GetAllActivityTypes(int? store)
        {
            var key = DCMSDefaults.ACTIVITYTYPE_ALL_KEY.FillCacheKey(store ?? 0);
            return _cacheManager.Get(key, () =>
            {
                var query = from alt in ActivityLogTypeRepository.Table
                            orderby alt.Name
                            select alt;
                var activityLogTypes = query.ToList();
                return activityLogTypes;
            });
        }

        /// <summary>
        /// Gets an activity log type item
        /// </summary>
        /// <param name="activityLogTypeId">Activity log type identifier</param>
        /// <returns>Activity log type item</returns>
        public virtual ActivityLogType GetActivityTypeById(int activityLogTypeId)
        {
            if (activityLogTypeId == 0)
            {
                return null;
            }

            return ActivityLogTypeRepository.ToCachedGetById(activityLogTypeId);
        }

        /// <summary>
        /// Inserts an activity log item
        /// </summary>
        /// <param name="systemKeyword">The system keyword</param>
        /// <param name="comment">The activity comment</param>
        /// <param name="commentParams">The activity comment parameters for string.Format() function.</param>
        /// <returns>Activity log item</returns>
        public virtual ActivityLog InsertActivity(string systemKeyword,
            string comment, params object[] commentParams)
        {
            return InsertActivity(systemKeyword, comment, _workContext.CurrentUser, commentParams);
        }


        /// <summary>
        /// Inserts an activity log item
        /// </summary>
        /// <param name="systemKeyword">The system keyword</param>
        /// <param name="comment">The activity comment</param>
        /// <param name="user">The user</param>
        /// <param name="commentParams">The activity comment parameters for string.Format() function.</param>
        /// <returns>Activity log item</returns>
        public virtual ActivityLog InsertActivity(string systemKeyword,
            string comment, User user, params object[] commentParams)
        {
            //if (user == null)
            //{
            //    return null;
            //}

            var uow = ActivityLogRepository.UnitOfWork;
            var activityTypes = GetAllActivityTypes(0);
            var activityType = activityTypes.ToList().Find(at => at.SystemKeyword == systemKeyword);
            if (activityType == null || !activityType.Enabled)
            {
                return null;
            }

            comment = CommonHelper.EnsureNotNull(comment);
            comment = string.Format(comment, commentParams);
            comment = CommonHelper.EnsureMaximumLength(comment, 4000);



            var activity = new ActivityLog
            {
                ActivityLogType = activityType,
                ActivityLogTypeId = activityType.Id,
                //activity.User = user;
                UserId = Convert.ToInt32(commentParams[0]),
                Comment = comment,
                CreatedOnUtc = DateTime.UtcNow,
            };

            ActivityLogRepository.Insert(activity);
            uow.SaveChanges();

            _eventPublisher.EntityInserted(activity);

            return activity;
        }

        /// <summary>
        /// Deletes an activity log item
        /// </summary>
        /// <param name="activityLog">Activity log type</param>
        public virtual void DeleteActivity(ActivityLog activityLog)
        {
            if (activityLog == null)
            {
                throw new ArgumentNullException("activityLog");
            }

            var uow = ActivityLogRepository.UnitOfWork;
            ActivityLogRepository.Delete(activityLog);
            uow.SaveChanges();

            _eventPublisher.EntityDeleted(activityLog);
        }

        /// <summary>
        /// Gets all activity log items
        /// </summary>
        /// <param name="createdOnFrom">Log item creation from; null to load all users</param>
        /// <param name="createdOnTo">Log item creation to; null to load all users</param>
        /// <param name="userId">User identifier; null to load all users</param>
        /// <param name="activityLogTypeId">Activity log type identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Activity log collection</returns>
        public virtual IPagedList<ActivityLog> GetAllActivities(DateTime? createdOnFrom,
            DateTime? createdOnTo, int? userId, int activityLogTypeId,
            int pageIndex, int pageSize)
        {
            if (pageSize >= 50)
                pageSize = 50;
            var query = ActivityLogRepository.Table;
            if (createdOnFrom.HasValue)
            {
                query = query.Where(al => createdOnFrom.Value <= al.CreatedOnUtc);
            }

            if (createdOnTo.HasValue)
            {
                query = query.Where(al => createdOnTo.Value >= al.CreatedOnUtc);
            }

            if (activityLogTypeId > 0)
            {
                query = query.Where(al => activityLogTypeId == al.ActivityLogTypeId);
            }

            if (userId.HasValue)
            {
                query = query.Where(al => userId.Value == al.UserId);
            }

            query = query.OrderByDescending(al => al.CreatedOnUtc);

            //var activityLog = new PagedList<ActivityLog>(query.ToList(), pageIndex, pageSize);
            //return activityLog;

            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<ActivityLog>(plists, pageIndex, pageSize, totalCount);
        }

        /// <summary>
        /// Gets an activity log item
        /// </summary>
        /// <param name="activityLogId">Activity log identifier</param>
        /// <returns>Activity log item</returns>
        public virtual ActivityLog GetActivityById(int activityLogId)
        {
            if (activityLogId == 0)
            {
                return null;
            }

            return ActivityLogRepository.ToCachedGetById(activityLogId);
        }

        /// <summary>
        /// Clears activity log
        /// </summary>
        public virtual void ClearAllActivities()
        {
            //启用存储过程(如果支持)
            if (_commonSettings.UseStoredProceduresIfSupported)
            {
                //although it's not a stored procedure we use it to ensure that a database supports them
                //we cannot wait until EF team has it implemented - http://data.uservoice.com/forums/72025-entity-framework-feature-suggestions/suggestions/1015357-batch-cud-support
                //do all databases support "Truncate command"?
                //TODO: do not hard-code the table name
                //_dbContext.ExecuteSqlCommand("TRUNCATE TABLE [ActivityLog]");
                //_activityLogRepository.ExecuteSqlCommand("TRUNCATE TABLE [ActivityLog]");

            }
            else
            {
                var uow = ActivityLogRepository.UnitOfWork;
                var activityLog = ActivityLogRepository.Table.ToList();
                foreach (var activityLogItem in activityLog)
                {
                    ActivityLogRepository.Delete(activityLogItem);
                }

                uow.SaveChanges();
            }
        }
        #endregion

    }
}
