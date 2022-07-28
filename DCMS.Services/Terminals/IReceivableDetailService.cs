using DCMS.Core;
using DCMS.Core.Domain.Terminals;
using System.Collections.Generic;

namespace DCMS.Services.Finances
{
    public partial interface IReceivableDetailService
    {
        IList<ReceivableDetail> GetAll(int? storeId);
        IPagedList<ReceivableDetail> GetReceivableDetails(string searchStr, int? storeId = null, int pageIndex = 0, int pageSize = int.MaxValue);
        ReceivableDetail GetReceivableDetailById(int? store, int id);
        IList<ReceivableDetail> GetReceivableDetailsByFinanceReceivableId(int financeReceivableId);
        IList<ReceivableDetail> GetReceivableDetailsByIds(int[] ids);
        void InsertReceivableDetail(ReceivableDetail receivableDetail);
        void DeleteReceivableDetail(ReceivableDetail receivableDetail);
        void UpdateReceivableDetail(ReceivableDetail receivableDetail);
    }
}
