using DCMS.Core;
using DCMS.Core.Domain.Configuration;
using System.Collections.Generic;

namespace DCMS.Services.Configuration
{
    public interface IRemarkConfigService
    {
        void DeleteRemarkConfig(RemarkConfig remarkConfigs);
        IList<RemarkConfig> GetAllRemarkConfigs(int? store);
        IPagedList<RemarkConfig> GetAllRemarkConfigs(int? store, int pageIndex = 0, int pageSize = int.MaxValue);
        RemarkConfig GetRemarkConfigById(int? store, int remarkConfigsId);
        IList<RemarkConfig> GetRemarkConfigsByIds(int[] sIds);
        void InsertRemarkConfig(RemarkConfig remarkConfigs);
        void UpdateRemarkConfig(RemarkConfig remarkConfigs);

        /// <summary>
        /// 绑定备注信息
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        IList<RemarkConfig> BindRemarkConfigList(int? storeId);

    }
}