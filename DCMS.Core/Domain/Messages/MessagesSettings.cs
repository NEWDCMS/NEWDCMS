using DCMS.Core.Configuration;

namespace DCMS.Core.Domain.Messages
{

    public class MessagesSettings : ISettings
    {
        /// <summary>
        /// A value indicating whether popup notifications set as default 
        /// </summary>
        //[Column(TypeName = "BIT(1)")]
        public bool UsePopupNotifications { get; set; }
    }
}
