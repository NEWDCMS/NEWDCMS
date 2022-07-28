using DCMS.Core;
using DCMS.Core.Domain.Visit;
using System.Collections.Generic;

namespace DCMS.Services.Visit
{
    public partial interface ILineTierService
    {
        IList<LineTier> GetAll(int? storeId);
        /// <summary>
        /// 绑定线路信息
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        IList<LineTier> BindLineTier(int? storeId);

        IPagedList<LineTier> GetLineTiers(int? storeId = null, int? userId = null, int pageIndex = 0, int pageSize = int.MaxValue);
        LineTier GetLineTierById(int? store, int id, bool isInClude=false);
        IList<LineTier> GetLineTiersByIds(int? store, int[] idArr);
        void InsertLineTier(LineTier lineTier);
        void DeleteLineTier(LineTier lineTier);
        void UpdateLineTier(LineTier lineTier);

        bool LineTierOptionExists(int? store, int lineTierId, int terminalId);
        IList<LineTierOption> GetLineTierOptions(int? store, int lineTierId);
        IList<int> GetLineTierOptionsIDS(int? store, int lineTierId);
        LineTierOption GetLineTierOptionById(int? store, int id);
        LineTierOption GetLineTierOptionByLineTierIdAndTerminalId(int terminalId, int lineTierId);
        IList<LineTierOption> GetLineTierOptionsByIds(int[] idArr);
        LineTierOption GetLineTierOptionOnlyOne(int? store, int Terminalid);
        void InsertLineTierOption(LineTierOption lineTierOption);
        void DeleteLineTierOption(LineTierOption lineTierOption);
        void DeleteLineTierOptions(List<LineTierOption> lineTierOptions);
        void UpdateLineTierOption(LineTierOption lineTierOption);
        void InsertLineTierOptions(List<LineTierOption> lineTierOptions);
        IList<UserLineTierAssign> GetUserLineTierAssigns(int userId);
        UserLineTierAssign GetUserLineTierAssignById(int? store, int id);
        UserLineTierAssign GetUserLineTierAssignByLineTierIdAndUserId(int userId, int lineTierId);
        IList<UserLineTierAssign> GetUserLineTierAssignsByIds(int[] idArr);
        void InsertUserLineTierAssign(UserLineTierAssign userLineTierAssign);
        void DeleteUserLineTierAssign(UserLineTierAssign userLineTierAssign);
        void UpdateUserLineTierAssign(UserLineTierAssign userLineTierAssign);

        int GetLineTierByName(int store, string name);
        /// <summary>
        /// 获取用户指定线路ID
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        IList<int> GetUserLineTier(int storeId, int userId);
        /// <summary>
        /// 获取终端线路ID
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="terminalId"></param>
        /// <returns></returns>
        int GetTerminalLineId(int userId,int terminalId);
    }
}
