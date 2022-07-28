using DCMS.Web.Factories;
using DCMS.Web.Framework.Components;
using Microsoft.AspNetCore.Mvc;

namespace DCMS.Web.Components
{
    public class TopicBlockViewComponent : DCMSViewComponent
    {
        private readonly ITopicModelFactory _topicModelFactory;

        public TopicBlockViewComponent(ITopicModelFactory topicModelFactory)
        {
            _topicModelFactory = topicModelFactory;
        }

        public IViewComponentResult Invoke(string systemName)
        {
            //var model = _topicModelFactory.PrepareTopicModelBySystemName(systemName);
            //if (model == null)
            //    return Content("");
            //return View(model);
            return View();
        }
    }
}
