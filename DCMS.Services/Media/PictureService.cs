using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Media;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Configuration;
using DCMS.Services.Events;
using DCMS.Services.Logging;



namespace DCMS.Services.Media
{
    /// <summary>
    ///  Í¼Æ¬·þÎñ
    /// </summary>
    public partial class PictureService : BaseService, IPictureService
    {
        private static readonly object s_lock = new object();
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly ILogger _logger;
        private readonly MediaSettings _mediaSettings;

        protected const int MULTIPLE_THUMB_DIRECTORIES_LENGTH = 3;

        public PictureService(IServiceGetter getter,
        IStaticCacheManager cacheManager,
        ISettingService settingService,
        IWebHelper webHelper,
        ILogger logger,
        IEventPublisher eventPublisher,
        MediaSettings mediaSettings) : base(getter, cacheManager, eventPublisher)
        {
            _settingService = settingService;
            _webHelper = webHelper;
            _logger = logger;
            _mediaSettings = mediaSettings;

        }

    }
}
