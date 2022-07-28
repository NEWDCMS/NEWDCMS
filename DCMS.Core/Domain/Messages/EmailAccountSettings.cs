using DCMS.Core.Configuration;

namespace DCMS.Core.Domain.Messages
{

    public class EmailAccountSettings : ISettings
    {
        /// <summary>
        /// Gets or sets a store default email account identifier
        /// </summary>
        public int DefaultEmailAccountId { get; set; }
    }
}
