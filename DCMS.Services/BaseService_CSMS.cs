using DCMS.Core.Data;
using DCMS.Core.Domain.CSMS;

namespace DCMS.Services
{
    public partial class BaseService
    {
        #region RW
        protected IRepository<TerminalSignReport> TerminalSignReportRepository => _getter.RW<TerminalSignReport>(CSMS);
        protected IRepository<OrderDetail> OrderDetailRepository => _getter.RW<OrderDetail>(CSMS);
        #endregion



    }

}
