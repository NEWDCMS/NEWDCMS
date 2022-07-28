using DCMS.Web.Framework.Models;
using System.ComponentModel.DataAnnotations;

namespace DCMS.ViewModel.Models.Messages
{

    public partial class NewsletterSubscriptionModel : BaseEntityModel
    {

        [DataType(DataType.EmailAddress)]

        public string Email { get; set; }

        public bool Active { get; set; }

        public string StoreName { get; set; }

        public string CreatedOn { get; set; }

    }
}