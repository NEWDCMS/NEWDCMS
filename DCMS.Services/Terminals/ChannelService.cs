using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Terminals;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using DCMS.Services.Caching;

namespace DCMS.Services.Terminals
{
    /// <summary>
    /// 渠道服务
    /// </summary>
    public partial class ChannelService : BaseService, IChannelService
    {
       
        public ChannelService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
           
            IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {
            
        }

        /// <summary>
        /// 获取所有渠道信息
        /// </summary>
        /// <returns></returns>
        public virtual IList<Channel> GetAll(int? storeId)
        {
            var key = DCMSDefaults.CHANNEL_ALL_STORE_KEY.FillCacheKey(storeId);
            return _cacheManager.Get(key, () =>
             {
                 var query = from c in ChannelsRepository.Table
                             orderby c.Name
                             where !c.Deleted
                             select c;
                 if (storeId != null)
                 {
                     query = query.Where(c => c.StoreId == storeId);
                 }

                 var list = query.ToList();
                 return list;
             });
        }

        /// <summary>
        /// 绑定客户渠道信息
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public virtual IList<Channel> BindChannelList(int? storeId)
        {
            var key = DCMSDefaults.BINDCHANNEL_ALL_STORE_KEY.FillCacheKey(storeId);
            return _cacheManager.Get(key, () =>
            {
                var query = from c in ChannelsRepository.Table
                            where !c.Deleted
                            select c;
                if (storeId != null)
                {
                    query = query.Where(c => c.StoreId == storeId);
                }

                var result = query.Select(q => new { q.Id, q.Name }).ToList().Select(x => new Channel { Id = x.Id, Name = x.Name }).ToList();
                return result;
            });
        }

        /// <summary>
        /// 分页获取渠道信息
        /// </summary>
        /// <param name="searchStr"></param>
        /// <param name="totalCount"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual IPagedList<Channel> GetChannels(string searchStr, int? storeId, int pageIndex, int pageSize)
        {
            if (pageSize >= 50)
                pageSize = 50;
            var query = from p in ChannelsRepository.Table
                        orderby p.Id descending
                        where !p.Deleted
                        select p;

            if (storeId != null)
            {
                query = query.Where(c => c.StoreId == storeId);
            }

            if (!string.IsNullOrEmpty(searchStr))
            {
                query = query.Where(t => t.Name.Contains(searchStr));
            }

            return new PagedList<Channel>(query, pageIndex, pageSize);
        }
        /// <summary>
        /// 根据主键Id获取渠道信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual Channel GetChannelById(int? store, int id)
        {
            if (id == 0)
            {
                return null;
            }
            return ChannelsRepository.ToCachedGetById(id);
        }
        public virtual string GetChannelName(int store, int id)
        {
            if (id == 0)
            {
                return "";
            }
            var key = DCMSDefaults.CHANNEL_NAME_BY_ID_KEY.FillCacheKey(store, id);
            return _cacheManager.Get(key, () =>
            {
                return ChannelsRepository.Table.Where(a => a.Id == id && a.StoreId == store).Select(a => a.Name).FirstOrDefault();
            });
        }

        public virtual int GetChannelByName(int store, string name)
        {
            var query = ChannelsRepository.Table;
            if (string.IsNullOrWhiteSpace(name))
            {
                return 0;
            }

            return query.Where(s => s.StoreId == store && s.Name == name).Select(s => s.Id).FirstOrDefault();
        }


        public virtual IList<Channel> GetChannelsByIds(int store, int[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                return new List<Channel>();
            }

            var query = from c in ChannelsRepository.Table
                        where c.StoreId == store && ids.Contains(c.Id)
                        select c;
            var list = query.ToList();

            var result = new List<Channel>();
            foreach (int id in ids)
            {
                var model = list.Find(x => x.Id == id);
                if (model != null)
                {
                    result.Add(model);
                }
            }
            return result;
        }

        /// <summary>
        /// 新增渠道信息
        /// </summary>
        /// <param name="channel"></param>
        public virtual void InsertChannel(Channel channel)
        {
            if (channel == null)
            {
                throw new ArgumentNullException("channel");
            }

            var uow = ChannelsRepository.UnitOfWork;
            ChannelsRepository.Insert(channel);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(channel);
        }
        /// <summary>
        /// 删除渠道信息
        /// </summary>
        /// <param name="channel"></param>
        public virtual void DeleteChannel(Channel channel)
        {
            if (channel == null)
            {
                throw new ArgumentNullException("channel");
            }

            var uow = ChannelsRepository.UnitOfWork;
            ChannelsRepository.Delete(channel);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityDeleted(channel);
        }
        /// <summary>
        /// 修改渠道信息
        /// </summary>
        /// <param name="channel"></param>
        public virtual void UpdateChannel(Channel channel)
        {
            if (channel == null)
            {
                throw new ArgumentNullException("channel");
            }

            var uow = ChannelsRepository.UnitOfWork;
            ChannelsRepository.Update(channel);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(channel);
        }

    }
}
