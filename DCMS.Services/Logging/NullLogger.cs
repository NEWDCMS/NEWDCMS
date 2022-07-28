using DCMS.Core;
using DCMS.Core.Domain.Logging;
using DCMS.Core.Domain.Users;
using System;
using System.Collections.Generic;

namespace DCMS.Services.Logging
{
    public partial class NullLogger : ILogger
    {


        public virtual bool IsEnabled(LogLevel level)
        {
            return false;
        }

        public virtual void DeleteLog(Log log)
        {
        }


        public virtual void DeleteLogs(IList<Log> logs)
        {
        }


        public virtual void ClearLog()
        {
        }


        public virtual IPagedList<Log> GetAllLogs(DateTime? fromUtc = null, DateTime? toUtc = null,
            string message = "", LogLevel? logLevel = null,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            return new PagedList<Log>(new List<Log>(), pageIndex, pageSize);
        }


        public virtual Log GetLogById(int logId)
        {
            return null;
        }


        public virtual IList<Log> GetLogByIds(int[] logIds)
        {
            return new List<Log>();
        }


        public virtual Log InsertLog(LogLevel logLevel, string shortMessage, string fullMessage = "", User user = null)
        {
            return null;
        }


        public virtual void Information(string message, Exception exception = null, User user = null)
        {
        }


        public virtual void Warning(string message, Exception exception = null, User user = null)
        {
        }

        public virtual void Error(string message, Exception exception = null, User user = null)
        {
        }

    }
}
