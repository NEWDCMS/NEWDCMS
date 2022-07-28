using DCMS.Web.Framework.Models;
using System.Collections.Generic;

namespace DCMS.ViewModel.Models.Messages
{
    public partial class TestMessageTemplateModel : BaseEntityModel
    {
        public TestMessageTemplateModel()
        {
            Tokens = new List<string>();
        }


        public List<string> Tokens { get; set; }

        public string SendTo { get; set; }
    }
}