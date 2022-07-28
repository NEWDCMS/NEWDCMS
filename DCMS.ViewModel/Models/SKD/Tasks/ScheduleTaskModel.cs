//
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using System.Collections.Generic;

namespace DCMS.ViewModel.Models.Tasks
{

    public partial class ScheduleTaskListModel : BaseModel
    {
        public ScheduleTaskListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
        }
        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<ScheduleTaskModel> Tasks { get; set; }

        public bool TaskWorking { get; set; }

    }


    //[Validator(typeof(ScheduleTaskValidator))]
    public partial class ScheduleTaskModel : BaseEntityModel
    {
        [HintDisplayName("任务名", "任务名")]

        public string Name { get; set; }

        [HintDisplayName("秒(运行时间)", "秒(运行时间)")]
        public int Seconds { get; set; }

        [HintDisplayName("是否启用", "是否启用")]
        public bool Enabled { get; set; }

        [HintDisplayName("是否在出错时停止", "是否在出错时停止")]
        public bool StopOnError { get; set; }

        [HintDisplayName("最后开始时间", "最后开始时间")]
        public string LastStartUtc { get; set; }

        [HintDisplayName("最后结束时间", "最后结束时间")]
        public string LastEndUtc { get; set; }

        [HintDisplayName("最后成功时间", "最后成功时间")]
        public string LastSuccessUtc { get; set; }
    }
}