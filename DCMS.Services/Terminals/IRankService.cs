using DCMS.Core;
using DCMS.Core.Domain.Terminals;
using System.Collections.Generic;

namespace DCMS.Services.Terminals
{
    public interface IRankService
    {
        IList<Rank> GetAll(int? storeId);
        /// <summary>
        /// 绑定经销商等级信息
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        IList<Rank> BindRankList(int? storeId);

        Rank GetRankById(int? store, int id);
        IPagedList<Rank> GetRanks(string searchStr, int? storeId, int pageIndex, int pageSize);
        IList<Rank> GetRanksByIds(int? store, int[] idArr);
        void InsertRank(Rank rank);
        void DeleteRank(Rank rank);
        void UpdateRank(Rank rank);

        int GetRankByName(int store, string name);
    }
}