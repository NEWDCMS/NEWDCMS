using DCMS.Core;
using DCMS.Core.Domain.Terminals;
using System.Collections.Generic;

namespace DCMS.Services.Terminals
{
    public partial interface IChannelService
    {
        IList<Channel> GetAll(int? storeId);
        /// <summary>
        /// 绑定客户渠道信息
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        IList<Channel> BindChannelList(int? storeId);

        IPagedList<Channel> GetChannels(string searchStr, int? storeId = null, int pageIndex = 0, int pageSize = int.MaxValue);
        Channel GetChannelById(int? store, int id);
        string GetChannelName(int store, int id);
        IList<Channel> GetChannelsByIds(int store, int[] ids);
        void InsertChannel(Channel channel);
        void DeleteChannel(Channel channel);
        void UpdateChannel(Channel channel);

        int GetChannelByName(int store, string name);

    }
}
