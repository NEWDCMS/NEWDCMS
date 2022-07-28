using DCMS.Core;
using DCMS.Core.Domain.Census;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DCMS.Services.Census
{
    public interface ITraditionService
    {
        IQueryable<Tradition> Traditions { get; }

        IPagedList<Tradition> GetAllTraditions(int userid = 0, int pageIndex = 0, int pageSize = int.MaxValue);
        int GetAllTraditionsCount();
        Tradition GetTradition(int userid = 0, int traditionId = 0);
        Tradition GetTraditionById(int traditionId = 0);
        IList<Tradition> GetTraditions(DateTime date, int userid = 0, int limit = 10);
        int GetTraditionsCount(int userid = 0);
        int Insert(Tradition tradition);
    }
}