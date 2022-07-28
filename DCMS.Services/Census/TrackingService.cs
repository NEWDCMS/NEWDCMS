using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Census;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using DCMS.Services.Caching;

namespace DCMS.Services.Census
{
    public partial class TrackingService : BaseService, ITrackingService
    {

        public TrackingService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {

        }

        /// <summary>
        /// 获取全部
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public virtual IList<Tracking> GetAll(int? storeId)
        {
            return _cacheManager.Get(DCMSDefaults.TRACKING_ALL_STORE_KEY.FillCacheKey(storeId), () =>
           {
               var query = TrackingRepository.Table;

               if (storeId != null)
               {
                   query = query.Where(c => c.StoreId == storeId);
               }

               return query.ToList();
           });
        }

        /// <summary>
        /// 单笔查询
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Tracking GetTrackingById(int? store, int id)
        {
            if (id == 0)
            {
                return null;
            }

            return TrackingRepository.ToCachedGetById(id);
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="businessUserId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual IList<Tracking> GetTrackings(int? storeId, int? businessUserId, DateTime? start, DateTime? end)
        {
            var query = TrackingRepository.Table;

            if (storeId != null)
            {
                query = query.Where(c => c.StoreId == storeId);
            }

            if (businessUserId.HasValue && businessUserId.Value != 0)
            {
                query = query.Where(t => t.BusinessUserId == businessUserId);
            }

            if (start.HasValue)
            {
                var startTime = DateTime.Parse(start.Value.ToString("yyyy-MM-dd 00:00:00"));
                query = query.Where(o => o.CreateDateTime >= startTime);
            }

            if (end.HasValue)
            {
                var endTime = DateTime.Parse(end.Value.ToString("yyyy-MM-dd 23:59:59"));
                query = query.Where(o => o.CreateDateTime <= endTime);
            }

            return query.ToList();
        }

        /// <summary>
        /// 批量获取
        /// </summary>
        /// <param name="idArr"></param>
        /// <returns></returns>
        public virtual IList<Tracking> GetTrackingByIds(int[] idArr)
        {
            if (idArr == null || idArr.Length == 0)
            {
                return new List<Tracking>();
            }

            var query = from c in TrackingRepository.Table
                        where idArr.Contains(c.Id)
                        select c;
            var list = query.ToList();

            var result = new List<Tracking>();
            foreach (int id in idArr)
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
        /// 新增
        /// </summary>
        /// <param name="tracking"></param>
        public BaseResult InsertTracking(Tracking tracking)
        {
            try
            {
                if (tracking == null)
                {
                    throw new ArgumentNullException("tracking");
                }

                var uow = TrackingRepository.UnitOfWork;
                TrackingRepository.Insert(tracking);
                uow.SaveChanges();

                //通知
                _eventPublisher.EntityInserted(tracking);
                return new BaseResult { Success = true, Message = Resources.Bill_CreateOrUpdateSuccessful };
            }
            catch (Exception)
            {

                return new BaseResult { Success = false, Message = Resources.Bill_CreateOrUpdateFailed };
            }

        }

        public BaseResult InsertTrackings(List<Tracking> trackings)
        {
            try
            {
                if (trackings != null && trackings.Count > 0)
                {
                    var uow = TrackingRepository.UnitOfWork;
                    TrackingRepository.Insert(trackings);
                    uow.SaveChanges();
                }
                return new BaseResult { Success = true, Message = Resources.Bill_CreateOrUpdateSuccessful };
            }
            catch (Exception)
            {

                return new BaseResult { Success = false, Message = Resources.Bill_CreateOrUpdateFailed };
            }
        }


        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="tracking"></param>
        public virtual void DeleteTracking(Tracking tracking)
        {
            if (tracking == null)
            {
                throw new ArgumentNullException("tracking");
            }

            var uow = TrackingRepository.UnitOfWork;
            TrackingRepository.Delete(tracking);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(tracking);
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="tracking"></param>
        public virtual void UpdateTracking(Tracking tracking)
        {
            if (tracking == null)
            {
                throw new ArgumentNullException("tracking");
            }

            var uow = TrackingRepository.UnitOfWork;
            TrackingRepository.Update(tracking);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(tracking);
        }
    }
}
