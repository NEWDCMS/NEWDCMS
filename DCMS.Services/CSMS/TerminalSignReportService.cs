using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Common;
using DCMS.Core.Domain.Security;
using DCMS.Core.Domain.Stores;
using DCMS.Core.Domain.Users;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Common;
using DCMS.Services.Events;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Module = DCMS.Core.Domain.Security.Module;
using DCMS.Services.Caching;
using Newtonsoft.Json;
using System.Linq.Expressions;
using DCMS.Core.Domain.CSMS;

namespace DCMS.Services.CSMS
{
    public class TerminalSignReportService : BaseService, ITerminalSignReportService
    {


        public TerminalSignReportService(IServiceGetter serviceGetter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher
            ) : base(serviceGetter, cacheManager, eventPublisher)
        {

        }
        public List<TerminalSignReport> GetTerminalSignReports()
        {
            return TerminalSignReportRepository.Table.ToList();
        }

        public TerminalSignReport InsertTerminalSignReport(TerminalSignReport terminalSignReport)
        {
            if (terminalSignReport == null)
            {
                throw new ArgumentNullException("terminalSignReport");
            }

            var uow = TerminalSignReportRepository.UnitOfWork;
            TerminalSignReportRepository.Insert(terminalSignReport);
            uow.SaveChanges();

            //事件通知
            _eventPublisher.EntityInserted(terminalSignReport);
            return terminalSignReport;
        }
    }
}
