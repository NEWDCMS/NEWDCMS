using DCMS.Core;
using DCMS.Core.Domain.Terminals;
using DCMS.Core.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using DCMS.Core.Domain.CRM;

namespace DCMS.Services.Terminals
{
    /// <summary>
    /// 终端信息接口
    /// </summary>
    public partial interface ITerminalService
    {
        IList<Terminal> GetAllTerminal(int? storeId);
        IPagedList<Terminal> GetTerminals(int? storeId, int? userId, IList<int> districtIds, string searchStr, int? channelId,
            int? rankId,
           int? lineId = 0,
            bool? status = true,
            double lat = 0,
            double lng = 0, double range = 0.5, int pageIndex = 0, int pageSize = int.MaxValue);
        IPagedList<Terminal> GetTerminals(int? storeId, int? userId, IList<int> districtIds, string searchStr, int? channelId,
            int? rankId,
           int? lineId = 0,
            bool? status = true,
            bool isWeb = false,
            int pageIndex = 0, int pageSize = int.MaxValue);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="userId"></param>
        /// <param name="districtIds"></param>
        /// <param name="searchStr"></param>
        /// <param name="channelId"></param>
        /// <param name="rankId"></param>
        /// <param name="lineId"></param>
        /// <param name="status"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        IPagedList<Terminal> GetAllTerminals(int? storeId, int? userId, IList<int> districtIds, string searchStr, int? channelId,
                    int? rankId,
                    double lat = 0,
                    double lng = 0,
                    double range = 0.5,
                    IList<int> lineIds = null,
                    bool? status = true,
                    int pageIndex = 0, int pageSize = int.MaxValue);
        /// <summary>
        /// 客户弹出框选择，根据条件可能会包括 供应商
        /// </summary>
        /// <param name="storeId">经销商Id</param>
        /// <param name="searchStr">查询关键字</param>
        /// <param name="status">客户状态</param>
        /// <param name="currentUser">当前登录用户</param>
        /// <param name="salesmanOnlySeeHisCustomer">业务员只能看到自己片区的客户（在公司设置配置中）</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        IPagedList<Terminal> GetPopupTerminals(int? storeId, string searchStr, bool? status, User currentUser, bool salesmanOnlySeeHisCustomer = false, int pageIndex = 0, int pageSize = int.MaxValue);


        IPagedList<Terminal> GetTerminals(int? storeId, int[] ids, string key = null, int pageIndex = 0, int pageSize = int.MaxValue);
        Terminal GetTerminalById(int? store, int id);
        string GetTerminalName(int? store, int id);
        string GetTerminalCode(int? store, int id);
        IList<int> GetTerminalIds(int? store, string terminalName, bool platform = false);
        IList<int> GetDisTerminalIds(int? store, int districtId);
        IList<Terminal> GetTerminalsByChannelid(int storeId, int channelId);
        IList<int> GetLineTierOptionLineids(int storeId, int lineid);
        IList<int> GetRankTerminalIds(int? store, int rankId);
        decimal GetTerminalMaxAmountOwed(int id);
        Dictionary<int, string> GetTerminalsDictsByIds(int storeId, int[] ids);
        IList<Terminal> GetTerminalsByIds(int? store, int[] sids, bool platform = false);
        IList<Terminal> GetTerminalsByDistrictId(int storeId, int districtId);
        IList<Terminal> GetTerminalsByDistrictId(int storeId, string key, int[] districtIds);
        IList<Terminal> GetTerminalsByLineId(int storeId, int lineId);
        void InsertTerminal(Terminal terminal, string storeCode);
        void InsertNewTerminal(NewTerminal terminal);
        void DeleteTerminal(Terminal terminal);
        void UpdateTerminal(Terminal terminal);
        void InsertRelation(CRM_RELATION relation);
        void UpdateTerminals(List<Terminal> terminals);
        bool CheckTerminal(int? store, string name);
        IList<IGrouping<DateTime, Terminal>> GetTerminalsAnalysisByCreate(int? storeId, int? user, DateTime date);
        bool CheckTerminalHasCampaignGives(int storeId, int terminalId);
        bool CheckTerminalHasCostContractGives(int storeId, int terminalId, int businessUserId);
        bool CheckRelated(int terminalId);

        decimal GetMaxAmountOwed(int storeId, int terminalId);

        IList<Terminal> GetTerminalsByKeyWord(int storeId, string keyWord);

        Terminal FindTerminalById(int? store, int id);

        IList<Terminal> GetTerminalsByDistrictIds(int? store, IList<int> districtIds, int lineId = 0);
    }
}
