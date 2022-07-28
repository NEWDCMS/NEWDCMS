using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DCMS.Core.Domain.Messages
{

    public partial class NewsLetterSubscription : BaseEntity
    {

        public Guid NewsLetterSubscriptionGuid { get; set; }

        public string Email { get; set; }

        [Column(TypeName = "BIT(1)")]
        public bool Active { get; set; }


        public DateTime CreatedOnUtc { get; set; }
    }
}
