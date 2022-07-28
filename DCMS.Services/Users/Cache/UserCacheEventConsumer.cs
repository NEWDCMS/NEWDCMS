using DCMS.Core.Caching;
using DCMS.Core.Domain.Users;
using DCMS.Services.Events;

namespace DCMS.Services.Users.Cache
{

    public partial class UserCacheEventConsumer : IConsumer<UserPasswordChangedEvent>
    {

        

        public UserCacheEventConsumer(IStaticCacheManager cacheManager)
        {
            
        }

        //password changed
        public void HandleEvent(UserPasswordChangedEvent eventMessage)
        {
            //_cacheManager.Remove(string.Format(DCMSUserServiceDefaults.UserPasswordLifetimeCacheKey, eventMessage.Password.UserId));
        }


    }
}