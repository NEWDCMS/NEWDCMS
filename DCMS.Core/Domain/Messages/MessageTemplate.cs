//
using DCMS.Core.Domain.Stores;
using System.ComponentModel.DataAnnotations.Schema;

namespace DCMS.Core.Domain.Messages
{

    public partial class MessageTemplate : BaseEntity, IStoreMappingSupported
    {
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the BCC Email addresses
        /// </summary>
        public string BccEmailAddresses { get; set; }

        /// <summary>
        /// Gets or sets the subject
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the body
        /// </summary>
        public string Body { get; set; }

        [Column(TypeName = "BIT(1)")]
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the delay before sending message
        /// </summary>
        public int? DelayBeforeSend { get; set; }

        /// <summary>
        /// Gets or sets the period of message delay 
        /// </summary>
        public int DelayPeriodId { get; set; }

        /// <summary>
        /// Gets or sets the download identifier of attached file
        /// </summary>
        public int AttachedDownloadId { get; set; }

        /// <summary>
        /// Gets or sets the used email account identifier
        /// </summary>
        public int EmailAccountId { get; set; }

        [Column(TypeName = "BIT(1)")]
        public bool LimitedToStores { get; set; }

        /// <summary>
        /// Gets or sets the period of message delay
        /// </summary>
        public MessageDelayPeriod DelayPeriod
        {
            get => (MessageDelayPeriod)DelayPeriodId;
            set => DelayPeriodId = (int)value;
        }
    }
}
