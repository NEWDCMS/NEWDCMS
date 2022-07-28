using DCMS.Core.Data;
using DCMS.Core.Domain.Census;

namespace DCMS.Services
{
    /// <summary>
    /// 服务基类
    /// </summary>
    public partial class BaseService
    {
      
        #region CENSUS

        #region RO
        protected IRepositoryReadOnly<Tracking> TrackingRepository_RO => _getter.RO<Tracking>(CENSUS);
        protected IRepositoryReadOnly<DisplayPhoto> DisplayPhotoRepository_RO => _getter.RO<DisplayPhoto>(CENSUS);
        protected IRepositoryReadOnly<DoorheadPhoto> DoorheadPhotoRepository_RO => _getter.RO<DoorheadPhoto>(CENSUS);
        protected IRepositoryReadOnly<Restaurant> RestaurantRepository_RO => _getter.RO<Restaurant>(CENSUS);
        protected IRepositoryReadOnly<RestaurantBaseInfo> RestaurantBaseInfoRepository_RO => _getter.RO<RestaurantBaseInfo>(CENSUS);
        protected IRepositoryReadOnly<RestaurantBusinessInfo> RestaurantBusinessInfoRepository_RO => _getter.RO<RestaurantBusinessInfo>(CENSUS);
        protected IRepositoryReadOnly<SalesProduct> SalesProductRepository_RO => _getter.RO<SalesProduct>(CENSUS);
        protected IRepositoryReadOnly<Tradition> TraditionRepository_RO => _getter.RO<Tradition>(CENSUS);
        #endregion

        #region RW
        protected IRepository<Tracking> TrackingRepository => _getter.RW<Tracking>(CENSUS);
        protected IRepository<DisplayPhoto> DisplayPhotoRepository => _getter.RW<DisplayPhoto>(CENSUS);
        protected IRepository<DoorheadPhoto> DoorheadPhotoRepository => _getter.RW<DoorheadPhoto>(CENSUS);
        protected IRepository<Restaurant> RestaurantRepository => _getter.RW<Restaurant>(CENSUS);
        protected IRepository<RestaurantBaseInfo> RestaurantBaseInfoRepository => _getter.RW<RestaurantBaseInfo>(CENSUS);
        protected IRepository<RestaurantBusinessInfo> RestaurantBusinessInfoRepository => _getter.RW<RestaurantBusinessInfo>(CENSUS);
        protected IRepository<SalesProduct> SalesProductRepository => _getter.RW<SalesProduct>(CENSUS);
        protected IRepository<Tradition> TraditionRepository => _getter.RW<Tradition>(CENSUS);
        #endregion

        #endregion

    }

}
