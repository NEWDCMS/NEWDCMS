using DCMS.Core;
using DCMS.Core.Domain.Logging;
using DCMS.Core.Domain.Users;
using System;
using System.Collections.Generic;

namespace DCMS.Services.Logging
{

    public partial interface ILogger
    {
        bool IsEnabled(LogLevel level);
        void DeleteLog(Log log);
        void DeleteLogs(IList<Log> logs);
        void ClearLog();
        IPagedList<Log> GetAllLogs(DateTime? fromUtc = null, DateTime? toUtc = null, string message = "", LogLevel? logLevel = null, int pageIndex = 0, int pageSize = int.MaxValue);
        Log GetLogById(int logId);
        IList<Log> GetLogByIds(int[] logIds);
        Log InsertLog(LogLevel logLevel, string shortMessage, string fullMessage = "", User user = null);
        void Information(string message, Exception exception = null, User user = null);
        void Warning(string message, Exception exception = null, User user = null);
        void Error(string message, Exception exception = null, User user = null);
    }
}
