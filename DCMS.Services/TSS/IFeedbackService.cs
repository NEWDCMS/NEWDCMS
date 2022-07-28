using DCMS.Core;
using DCMS.Core.Domain.TSS;
using System.Linq;
using System.Collections.Generic;

namespace DCMS.Services.TSS
{
    public interface IFeedbackService
    {
        void InsertFeedback(Feedback feedback);
        void UpdateFeedback(Feedback feedback);
        void DeleteFeedback(Feedback feedback);
        Feedback GetFeedbackById(int feedBackId);
        IList<Feedback> Others();
        IPagedList<Feedback> SearchFeedbacks(int? storeId, int? type, int pageIndex = 0, int pageSize = int.MaxValue);
    }
}
