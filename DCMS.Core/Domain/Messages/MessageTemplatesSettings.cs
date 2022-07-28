using DCMS.Core.Configuration;

namespace DCMS.Core.Domain.Messages
{

    public class MessageTemplatesSettings : ISettings
    {
        //[Column(TypeName = "BIT(1)")]
        public bool CaseInvariantReplacement { get; set; }


        public string Color1 { get; set; }


        public string Color2 { get; set; }


        public string Color3 { get; set; }
    }
}
