using DCMS.Core.Caching;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Core.ZXing;
using DCMS.Services.Events;

namespace DCMS.Services.Common
{
    public class MediaService : BaseService, IMediaService
    {

        public MediaService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {
        }

        public string GenerateBarCodeForBase64(string input, int width, int height)
        {
            var image = BarcodeHelper.GenerateBarCode(input, width, height);
            return image;
        }

        public string GenerateGenerateQRForBase64(string text, int width, int height)
        {
            var image = BarcodeHelper.GenerateQR(text, width, height);
            return image;
        }

    }
}
