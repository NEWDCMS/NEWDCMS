using DCMS.Core;
using DCMS.Core.Domain.Terminals;
using System.Collections.Generic;

namespace DCMS.Services.Finances
{
    public partial interface IReceivableService
    {
        IList<Receivable> GetAll(int? storeId = null);
        IPagedList<Receivable> GetReceivables(int? terminalId = null, string terminalName = null, int? storeId = null, int pageIndex = 0, int pageSize = int.MaxValue);
        Receivable GetReceivableById(int? store, int id);
        IList<Receivable> GetReceivablesByIds(int[] ids);
        void InsertReceivable(Receivable receivable);
        void DeleteReceivable(Receivable receivable);
        void UpdateReceivable(Receivable receivable);
    }
}
