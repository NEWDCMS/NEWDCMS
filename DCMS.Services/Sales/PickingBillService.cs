using DCMS.Core.Caching;
using DCMS.Core.Domain.Sales;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using System;
using System.Linq;

namespace DCMS.Services.Sales
{
    public class PickingBillService : BaseService, IPickingBillService
    {


        public PickingBillService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {

        }

        public PickingBill GetPickingById(int pickingId)
        {
            PickingBill picking;
            var query = PickingsRepository.Table;
            picking = query.Where(a => a.Id == pickingId).FirstOrDefault();
            return picking;
        }

        public void InsertPicking(PickingBill picking)
        {
            var uow = PickingsRepository.UnitOfWork;
            PickingsRepository.Insert(picking);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(picking);
        }

        public void UpdatePicking(PickingBill picking)
        {
            if (picking == null)
            {
                throw new ArgumentNullException("picking");
            }

            var uow = PickingsRepository.UnitOfWork;
            PickingsRepository.Update(picking);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(picking);
        }

        public void DeletePicking(PickingBill picking)
        {
            if (picking == null)
            {
                throw new ArgumentNullException("picking");
            }

            var uow = PickingsRepository.UnitOfWork;
            PickingsRepository.Delete(picking);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(picking);
        }

        #region 仓库分拣单明细
        public PickingItem GetPickingDetailById(int pickingDetailId)
        {
            PickingItem pickingDetail;
            var query = PickingItemsRepository.Table;
            pickingDetail = query.Where(a => a.Id == pickingDetailId).FirstOrDefault();
            return pickingDetail;
        }

        public void DeletePickingDetail(PickingItem pickingDetail)
        {
            if (pickingDetail == null)
            {
                throw new ArgumentNullException("pickingDetail");
            }

            var uow = PickingItemsRepository.UnitOfWork;
            PickingItemsRepository.Delete(pickingDetail);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(pickingDetail);
        }

        public void InsertPickingDetail(PickingItem pickingDetail)
        {
            var uow = PickingItemsRepository.UnitOfWork;
            PickingItemsRepository.Insert(pickingDetail);
            uow.SaveChanges();
            //通知
            _eventPublisher.EntityInserted(pickingDetail);
        }

        public void UpdatePickingDetail(PickingItem pickingDetail)
        {
            if (pickingDetail == null)
            {
                throw new ArgumentNullException("pickingDetail");
            }

            var uow = PickingItemsRepository.UnitOfWork;
            PickingItemsRepository.Update(pickingDetail);
            uow.SaveChanges();
            //通知
            _eventPublisher.EntityUpdated(pickingDetail);
        }
        #endregion


    }
}
