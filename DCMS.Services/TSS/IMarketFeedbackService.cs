using DCMS.Core;
using DCMS.Core.Domain.TSS;

namespace DCMS.Services.TSS
{
    public interface IMarketFeedbackService
    {
        void DeleteMarketFeedback(MarketFeedback marketFeedback);
        MarketFeedback GetMarketFeedbackById(int feedBackId);
        void InsertMarketFeedback(MarketFeedback marketFeedback);
        IPagedList<MarketFeedback> SearchMarketFeedbacks(int? storeId, int? userId, int? type, int pageIndex = 0, int pageSize = int.MaxValue);
        void UpdateMarketFeedback(MarketFeedback marketFeedback);
    }
}