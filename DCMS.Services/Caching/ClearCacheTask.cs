using DCMS.Core.Caching;
using DCMS.Services.Tasks;

namespace DCMS.Services.Caching
{
    /// <summary>
    /// 用于缓存清理的计划任务实现
    /// </summary>
    public partial class ClearCacheTask : IScheduleTask
    {

        private readonly IStaticCacheManager _staticCacheManager;

        public ClearCacheTask(IStaticCacheManager staticCacheManager)
        {
            _staticCacheManager = staticCacheManager;
        }

        public void Execute()
        {
            _staticCacheManager.Clear();
        }

    }
}