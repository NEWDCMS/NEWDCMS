using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Purchases;
using DCMS.Core.Events;
using DCMS.Services.Events;

namespace DCMS.Web.Infrastructure.Cache
{
    public partial class PurchasesCacheEventConsumer :
        //PurchaseBill
        IConsumer<EntityInsertedEvent<PurchaseBill>>,
         IConsumer<EntityUpdatedEvent<PurchaseBill>>,
         IConsumer<EntityDeletedEvent<PurchaseBill>>,

        //PurchaseItem
        IConsumer<EntityInsertedEvent<PurchaseItem>>,
         IConsumer<EntityUpdatedEvent<PurchaseItem>>,
         IConsumer<EntityDeletedEvent<PurchaseItem>>,

        //PurchaseBillAccounting
        IConsumer<EntityInsertedEvent<PurchaseBillAccounting>>,
         IConsumer<EntityUpdatedEvent<PurchaseBillAccounting>>,
         IConsumer<EntityDeletedEvent<PurchaseBillAccounting>>,

        //PurchaseReturnBill
        IConsumer<EntityInsertedEvent<PurchaseReturnBill>>,
         IConsumer<EntityUpdatedEvent<PurchaseReturnBill>>,
         IConsumer<EntityDeletedEvent<PurchaseReturnBill>>,

        //PurchaseReturnItem
        IConsumer<EntityInsertedEvent<PurchaseReturnItem>>,
         IConsumer<EntityUpdatedEvent<PurchaseReturnItem>>,
         IConsumer<EntityDeletedEvent<PurchaseReturnItem>>,

        //PurchaseReturnBillAccounting
        IConsumer<EntityInsertedEvent<PurchaseReturnBillAccounting>>,
         IConsumer<EntityUpdatedEvent<PurchaseReturnBillAccounting>>,
         IConsumer<EntityDeletedEvent<PurchaseReturnBillAccounting>>
    {

        protected readonly IStaticCacheManager _cacheManager;
        public PurchasesCacheEventConsumer(IStaticCacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        #region PurchaseBill
        public void HandleEvent(EntityInsertedEvent<PurchaseBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PURCHASEBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<PurchaseBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PURCHASEBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<PurchaseBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PURCHASEBILL_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region PurchaseItem
        public void HandleEvent(EntityInsertedEvent<PurchaseItem> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<PurchaseItem> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<PurchaseItem> eventMessage)
        {
        }
        #endregion

        #region PurchaseBillAccounting
        public void HandleEvent(EntityInsertedEvent<PurchaseBillAccounting> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PURCHASEBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<PurchaseBillAccounting> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PURCHASEBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<PurchaseBillAccounting> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PURCHASEBILL_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region PurchaseReturnBill
        public void HandleEvent(EntityInsertedEvent<PurchaseReturnBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PURCHASERETURNBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<PurchaseReturnBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PURCHASERETURNBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<PurchaseReturnBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PURCHASERETURNBILL_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region PurchaseReturnItem
        public void HandleEvent(EntityInsertedEvent<PurchaseReturnItem> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<PurchaseReturnItem> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<PurchaseReturnItem> eventMessage)
        {
        }
        #endregion

        #region PurchaseReturnBillAccounting
        public void HandleEvent(EntityInsertedEvent<PurchaseReturnBillAccounting> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PURCHASEBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<PurchaseReturnBillAccounting> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PURCHASEBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<PurchaseReturnBillAccounting> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PURCHASEBILL_PK, eventMessage.Entity.StoreId));
        }
        #endregion
    }
}
