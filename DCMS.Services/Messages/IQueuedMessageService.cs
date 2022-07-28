using DCMS.Core;
using DCMS.Core.Domain.Tasks;
using System;
using System.Collections.Generic;

namespace DCMS.Services.Tasks
{
    public interface IQueuedMessageService
    {
        void InsertQueuedMessage(List<string> ToUsers, QueuedMessage queuedMessage);
        void UpdateQueuedMessage(QueuedMessage queuedMessage);
        void DeleteQueuedMessage(QueuedMessage queuedMessage);
        QueuedMessage GetQueuedMessageById(int queuedMessageId);
        IList<QueuedMessage> GetQueuedMessagesByIds(int[] queuedMessageIds);

        IList<QueuedMessage> SearchMessages(int? storeId,
              int? mType, DateTime? createdFromUtc, int maxSendTries, DateTime? createdToUtc,
              bool loadNotSentItemsOnly,
              bool loadNewest);

        IPagedList<QueuedMessage> SearchMessages(int? storeId, int[] mTypeId, string toUser, bool? sentStatus, bool? orderByCreatedOnUtc, int? maxSendTries, DateTime? startTime = null, DateTime? endTime = null, int pageIndex = 0, int pageSize = int.MaxValue);

        void WriteLogs(string message = "");

    }
}