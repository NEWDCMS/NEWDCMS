using DCMS.Core;
using DCMS.Core.Domain.Messages;
using System;
using System.Collections.Generic;

namespace DCMS.Services.Tasks
{
    public partial interface IQueuedEmailService
    {

        void InsertQueuedEmail(QueuedEmail queuedEmail);
        void UpdateQueuedEmail(QueuedEmail queuedEmail);
        void DeleteQueuedEmail(QueuedEmail queuedEmail);
        QueuedEmail GetQueuedEmailById(int queuedEmailId);
        IList<QueuedEmail> GetQueuedEmailsByIds(int[] queuedEmailIds);

        IPagedList<QueuedEmail> SearchEmails(string fromEmail,
         string toEmail, DateTime? createdFromUtc, DateTime? createdToUtc,
         bool loadNotSentItemsOnly, int maxSendTries,
         bool loadNewest, int pageIndex, int pageSize);

        IPagedList<QueuedEmail> SearchEmails(int? storeId, string from, string to, bool? sentStatus, bool? orderByCreatedOnUtc, int? maxSendTries, DateTime? startTime = null, DateTime? endTime = null, int pageIndex = 0, int pageSize = int.MaxValue);

    }
}
