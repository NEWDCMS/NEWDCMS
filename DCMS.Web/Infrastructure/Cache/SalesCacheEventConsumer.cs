using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Sales;
using DCMS.Core.Events;
using DCMS.Services.Events;

namespace DCMS.Web.Infrastructure.Cache
{
    public partial class SalesCacheEventConsumer :
        //DispatchBill
        IConsumer<EntityInsertedEvent<DispatchBill>>,
         IConsumer<EntityUpdatedEvent<DispatchBill>>,
         IConsumer<EntityDeletedEvent<DispatchBill>>,

        //DispatchItem
        IConsumer<EntityInsertedEvent<DispatchItem>>,
         IConsumer<EntityUpdatedEvent<DispatchItem>>,
         IConsumer<EntityDeletedEvent<DispatchItem>>,

        //DeliverySign
        IConsumer<EntityInsertedEvent<DeliverySign>>,
         IConsumer<EntityUpdatedEvent<DeliverySign>>,
         IConsumer<EntityDeletedEvent<DeliverySign>>,

        //FinanceReceiveAccountBill
        IConsumer<EntityInsertedEvent<FinanceReceiveAccountBill>>,
         IConsumer<EntityUpdatedEvent<FinanceReceiveAccountBill>>,
         IConsumer<EntityDeletedEvent<FinanceReceiveAccountBill>>,

        //FinanceReceiveAccountBillAccounting
        IConsumer<EntityInsertedEvent<FinanceReceiveAccountBillAccounting>>,
         IConsumer<EntityUpdatedEvent<FinanceReceiveAccountBillAccounting>>,
         IConsumer<EntityDeletedEvent<FinanceReceiveAccountBillAccounting>>,

        //PickingBill
        IConsumer<EntityInsertedEvent<PickingBill>>,
         IConsumer<EntityUpdatedEvent<PickingBill>>,
         IConsumer<EntityDeletedEvent<PickingBill>>,

        //PickingItem
        IConsumer<EntityInsertedEvent<PickingItem>>,
         IConsumer<EntityUpdatedEvent<PickingItem>>,
         IConsumer<EntityDeletedEvent<PickingItem>>,

        //RetainPhoto
        IConsumer<EntityInsertedEvent<RetainPhoto>>,
         IConsumer<EntityUpdatedEvent<RetainPhoto>>,
         IConsumer<EntityDeletedEvent<RetainPhoto>>,

        //ReturnBill
        IConsumer<EntityInsertedEvent<ReturnBill>>,
         IConsumer<EntityUpdatedEvent<ReturnBill>>,
         IConsumer<EntityDeletedEvent<ReturnBill>>,

        //ReturnItem
        IConsumer<EntityInsertedEvent<ReturnItem>>,
         IConsumer<EntityUpdatedEvent<ReturnItem>>,
         IConsumer<EntityDeletedEvent<ReturnItem>>,

        //ReturnBillAccounting
        IConsumer<EntityInsertedEvent<ReturnBillAccounting>>,
         IConsumer<EntityUpdatedEvent<ReturnBillAccounting>>,
         IConsumer<EntityDeletedEvent<ReturnBillAccounting>>,

        //ReturnReservationBill
        IConsumer<EntityInsertedEvent<ReturnReservationBill>>,
         IConsumer<EntityUpdatedEvent<ReturnReservationBill>>,
         IConsumer<EntityDeletedEvent<ReturnReservationBill>>,

        //ReturnReservationItem
        IConsumer<EntityInsertedEvent<ReturnReservationItem>>,
         IConsumer<EntityUpdatedEvent<ReturnReservationItem>>,
         IConsumer<EntityDeletedEvent<ReturnReservationItem>>,

        //ReturnReservationBillAccounting
        IConsumer<EntityInsertedEvent<ReturnReservationBillAccounting>>,
         IConsumer<EntityUpdatedEvent<ReturnReservationBillAccounting>>,
         IConsumer<EntityDeletedEvent<ReturnReservationBillAccounting>>,

        //SaleBill
        IConsumer<EntityInsertedEvent<SaleBill>>,
         IConsumer<EntityUpdatedEvent<SaleBill>>,
         IConsumer<EntityDeletedEvent<SaleBill>>,

        //SaleItem
        IConsumer<EntityInsertedEvent<SaleItem>>,
         IConsumer<EntityUpdatedEvent<SaleItem>>,
         IConsumer<EntityDeletedEvent<SaleItem>>,

        //SaleBillAccounting
        IConsumer<EntityInsertedEvent<SaleBillAccounting>>,
         IConsumer<EntityUpdatedEvent<SaleBillAccounting>>,
         IConsumer<EntityDeletedEvent<SaleBillAccounting>>,

        //SaleReservationBill
        IConsumer<EntityInsertedEvent<SaleReservationBill>>,
         IConsumer<EntityUpdatedEvent<SaleReservationBill>>,
         IConsumer<EntityDeletedEvent<SaleReservationBill>>,

        //SaleReservationItem
        IConsumer<EntityInsertedEvent<SaleReservationItem>>,
         IConsumer<EntityUpdatedEvent<SaleReservationItem>>,
         IConsumer<EntityDeletedEvent<SaleReservationItem>>,

        //SaleReservationBillAccounting
        IConsumer<EntityInsertedEvent<SaleReservationBillAccounting>>,
         IConsumer<EntityUpdatedEvent<SaleReservationBillAccounting>>,
         IConsumer<EntityDeletedEvent<SaleReservationBillAccounting>>
    {

        protected readonly IStaticCacheManager _cacheManager;
        public SalesCacheEventConsumer(IStaticCacheManager cacheManager)
        {
            _cacheManager = cacheManager;

        }

        #region DispatchBill
        public void HandleEvent(EntityInsertedEvent<DispatchBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.DISPATCHBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<DispatchBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.DISPATCHBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<DispatchBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.DISPATCHBILL_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region DispatchItem
        public void HandleEvent(EntityInsertedEvent<DispatchItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.DISPATCHBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<DispatchItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.DISPATCHBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<DispatchItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.DISPATCHBILL_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region DeliverySign 
        public void HandleEvent(EntityInsertedEvent<DeliverySign> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.DELIVERYSIGN_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<DeliverySign> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.DELIVERYSIGN_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<DeliverySign> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.DELIVERYSIGN_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region FinanceReceiveAccountBill 
        public void HandleEvent(EntityInsertedEvent<FinanceReceiveAccountBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.FINANCERECEIVEACCOUNTBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<FinanceReceiveAccountBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.FINANCERECEIVEACCOUNTBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<FinanceReceiveAccountBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.FINANCERECEIVEACCOUNTBILL_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region FinanceReceiveAccountBillAccounting 
        public void HandleEvent(EntityInsertedEvent<FinanceReceiveAccountBillAccounting> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.FINANCERECEIVEACCOUNTBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<FinanceReceiveAccountBillAccounting> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.FINANCERECEIVEACCOUNTBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<FinanceReceiveAccountBillAccounting> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.FINANCERECEIVEACCOUNTBILL_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region PickingBill 
        public void HandleEvent(EntityInsertedEvent<PickingBill> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<PickingBill> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<PickingBill> eventMessage)
        {
        }
        #endregion

        #region PickingItem 
        public void HandleEvent(EntityInsertedEvent<PickingItem> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<PickingItem> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<PickingItem> eventMessage)
        {
        }
        #endregion

        #region RetainPhoto  
        public void HandleEvent(EntityInsertedEvent<RetainPhoto> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.RETAINPHOTO_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<RetainPhoto> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.RETAINPHOTO_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<RetainPhoto> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.RETAINPHOTO_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region ReturnBill  
        public void HandleEvent(EntityInsertedEvent<ReturnBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.RETURNBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<ReturnBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.RETURNBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<ReturnBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.RETURNBILL_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region ReturnItem  
        public void HandleEvent(EntityInsertedEvent<ReturnItem> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<ReturnItem> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<ReturnItem> eventMessage)
        {
        }
        #endregion

        #region ReturnBillAccounting  
        public void HandleEvent(EntityInsertedEvent<ReturnBillAccounting> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.RETURNRESERVATIONBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<ReturnBillAccounting> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.RETURNRESERVATIONBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<ReturnBillAccounting> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.RETURNRESERVATIONBILL_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region ReturnReservationBill  
        public void HandleEvent(EntityInsertedEvent<ReturnReservationBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.RETURNRESERVATIONBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<ReturnReservationBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.RETURNRESERVATIONBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<ReturnReservationBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.RETURNRESERVATIONBILL_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region ReturnReservationItem  
        public void HandleEvent(EntityInsertedEvent<ReturnReservationItem> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<ReturnReservationItem> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<ReturnReservationItem> eventMessage)
        {
        }
        #endregion

        #region ReturnReservationBillAccounting  
        public void HandleEvent(EntityInsertedEvent<ReturnReservationBillAccounting> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.RETURNRESERVATIONBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<ReturnReservationBillAccounting> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.RETURNRESERVATIONBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<ReturnReservationBillAccounting> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.RETURNRESERVATIONBILL_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region SaleBill  
        public void HandleEvent(EntityInsertedEvent<SaleBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.SALEBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<SaleBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.SALEBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<SaleBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.SALEBILL_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region SaleItem  
        public void HandleEvent(EntityInsertedEvent<SaleItem> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<SaleItem> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<SaleItem> eventMessage)
        {
        }
        #endregion

        #region SaleBillAccounting   
        public void HandleEvent(EntityInsertedEvent<SaleBillAccounting> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.SALEBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<SaleBillAccounting> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.SALEBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<SaleBillAccounting> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.SALEBILL_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region SaleReservationBill   
        public void HandleEvent(EntityInsertedEvent<SaleReservationBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.SALERESERVATIONBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<SaleReservationBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.SALERESERVATIONBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<SaleReservationBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.SALERESERVATIONBILL_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region SaleReservationItem  
        public void HandleEvent(EntityInsertedEvent<SaleReservationItem> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<SaleReservationItem> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<SaleReservationItem> eventMessage)
        {
        }
        #endregion

        #region SaleReservationBillAccounting   
        public void HandleEvent(EntityInsertedEvent<SaleReservationBillAccounting> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.SALEBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<SaleReservationBillAccounting> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.SALEBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<SaleReservationBillAccounting> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.SALEBILL_PK, eventMessage.Entity.StoreId));
        }
        #endregion
    }
}
