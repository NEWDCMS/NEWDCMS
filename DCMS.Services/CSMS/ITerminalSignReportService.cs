using DCMS.Core;
using DCMS.Core.Domain.Common;
using DCMS.Core.Domain.Security;
using DCMS.Core.Domain.CSMS;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace DCMS.Services.CSMS
{
    public interface ITerminalSignReportService
    {
        List<TerminalSignReport> GetTerminalSignReports();
        TerminalSignReport InsertTerminalSignReport(TerminalSignReport terminalSignReport);
    }
}
