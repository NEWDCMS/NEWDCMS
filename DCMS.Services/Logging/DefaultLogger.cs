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

    public partial class DefaultLogger : BaseService, ILogger
    {

        private readonly CommonSettings _commonSettings;
        private readonly IWebHelper _webHelper;

        public DefaultLogger(IStaticCacheManager cacheManager,
            IServiceGetter getter,
            IEventPublisher eventPublisher,
            CommonSettings commonSettings,
            IWebHelper webHelper) : base(getter, cacheManager, eventPublisher)
        {
            _commonSettings = commonSettings;
            _webHelper = webHelper;
        }


        protected virtual bool IgnoreLog(string message)
        {
            if (!_commonSettings.IgnoreLogWordlist.Any())
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                return false;
            }

            return _commonSettings
                .IgnoreLogWordlist
                .Any(x => message.IndexOf(x, StringComparison.InvariantCultureIgnoreCase) >= 0);
        }




        public virtual bool IsEnabled(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    return false;
                default:
                    return true;
            }
        }


        public virtual void DeleteLog(Log log)
        {
            if (log == null)
            {
                throw new ArgumentNullException(nameof(log));
            }

            var uow = LogRepository.UnitOfWork;
            LogRepository.Delete(log);
            uow.SaveChanges();
        }


        public virtual void DeleteLogs(IList<Log> logs)
        {
            if (logs == null)
            {
                throw new ArgumentNullException(nameof(logs));
            }

            var uow = LogRepository.UnitOfWork;
            LogRepository.Delete(logs);
            uow.SaveChanges();
        }


        public virtual void ClearLog()
        {
            //do all databases support "Truncate command"?
            //var logTableName = _dbContext.GetTableName<Log>();
            //_dbContext.ExecuteSqlCommand($"TRUNCATE TABLE [{logTableName}]");
            var logTableName = "Log";
            LogRepository.QueryFromSql<Log>($"TRUNCATE TABLE [{logTableName}]");

            //var log = LogRepository.Table.ToList();
            //foreach (var logItem in log)
            //    LogRepository.Delete(logItem);
        }


        public virtual IPagedList<Log> GetAllLogs(DateTime? fromUtc = null, DateTime? toUtc = null,
            string message = "", LogLevel? logLevel = null,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;
            var query = LogRepository.Table;
            if (fromUtc.HasValue)
            {
                query = query.Where(l => fromUtc.Value <= l.CreatedOnUtc);
            }

            if (toUtc.HasValue)
            {
                query = query.Where(l => toUtc.Value >= l.CreatedOnUtc);
            }

            if (logLevel.HasValue)
            {
                var logLevelId = (int)logLevel.Value;
                query = query.Where(l => logLevelId == l.LogLevelId);
            }

            if (!string.IsNullOrEmpty(message))
            {
                query = query.Where(l => l.ShortMessage.Contains(message) || l.FullMessage.Contains(message));
            }

            query = query.OrderByDescending(l => l.CreatedOnUtc);

            var log = new PagedList<Log>(query, pageIndex, pageSize);
            return log;
        }


        public virtual Log GetLogById(int logId)
        {
            if (logId == 0)
            {
                return null;
            }

            return LogRepository.ToCachedGetById(logId);
        }


        public virtual IList<Log> GetLogByIds(int[] logIds)
        {
            if (logIds == null || logIds.Length == 0)
            {
                return new List<Log>();
            }

            var query = from l in LogRepository.Table
                        where logIds.Contains(l.Id)
                        select l;
            var logItems = query.ToList();
            //sort by passed identifiers
            var sortedLogItems = new List<Log>();
            foreach (var id in logIds)
            {
                var log = logItems.Find(x => x.Id == id);
                if (log != null)
                {
                    sortedLogItems.Add(log);
                }
            }

            return sortedLogItems;
        }


        public virtual Log InsertLog(LogLevel logLevel, string shortMessage, string fullMessage = "", User user = null)
        {
            try
            {
                //check ignore word/phrase list?
                if (IgnoreLog(shortMessage) || IgnoreLog(fullMessage))
                {
                    return null;
                }

                var uow = LogRepository.UnitOfWork;

                var log = new Log
                {
                    LogLevel = logLevel,
                    ShortMessage = shortMessage,
                    FullMessage = fullMessage,
                    IpAddress = _webHelper.GetCurrentIpAddress(),
                    User = user,
                    UserId = user == null ? 0 : user.Id,
                    PageUrl = _webHelper.GetThisPageUrl(true),
                    ReferrerUrl = _webHelper.GetUrlReferrer(),
                    CreatedOnUtc = DateTime.UtcNow
                };

                LogRepository.Insert(log);

                uow.SaveChanges();

                return log;
            }
            catch (Exception)
            {
                return null;
            }
        }


        public virtual void Information(string message, Exception exception = null, User user = null)
        {
            //don't log thread abort exception
            if (exception is System.Threading.ThreadAbortException)
            {
                return;
            }

            if (IsEnabled(LogLevel.Information))
            {
                InsertLog(LogLevel.Information, message, exception?.ToString() ?? string.Empty, user);
            }
        }


        public virtual void Warning(string message, Exception exception = null, User user = null)
        {
            //don't log thread abort exception
            if (exception is System.Threading.ThreadAbortException)
            {
                return;
            }

            if (IsEnabled(LogLevel.Warning))
            {
                InsertLog(LogLevel.Warning, message, exception?.ToString() ?? string.Empty, user);
            }
        }


        public virtual void Error(string message, Exception exception = null, User user = null)
        {
            //don't log thread abort exception
            if (exception is System.Threading.ThreadAbortException)
            {
                return;
            }

            if (IsEnabled(LogLevel.Error))
            {
                InsertLog(LogLevel.Error, message, exception?.ToString() ?? string.Empty, user);
            }
        }


    }
}