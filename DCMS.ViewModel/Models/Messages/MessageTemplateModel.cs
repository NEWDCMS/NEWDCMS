using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DCMS.ViewModel.Models.Messages
{

    public partial class MessageTemplateModel : BaseEntityModel, ILocalizedModel<MessageTemplateLocalizedModel>, IStoreMappingSupportedModel
    {
        #region Ctor

        public MessageTemplateModel()
        {
            Locales = new List<MessageTemplateLocalizedModel>();
            AvailableEmailAccounts = new List<SelectListItem>();

            SelectedStoreIds = new List<int>();
            AvailableStores = new List<SelectListItem>();
        }

        #endregion

        #region Properties


        public string AllowedTokens { get; set; }

        public string Name { get; set; }


        public string BccEmailAddresses { get; set; }


        public string Subject { get; set; }

        public string Body { get; set; }


        public bool IsActive { get; set; }


        public bool SendImmediately { get; set; }


        [UIHint("Int32Nullable")]
        public int? DelayBeforeSend { get; set; }

        public int DelayPeriodId { get; set; }

        public bool HasAttachedDownload { get; set; }
        [UIHint("Download")]
        public int AttachedDownloadId { get; set; }

        public int EmailAccountId { get; set; }

        public IList<SelectListItem> AvailableEmailAccounts { get; set; }

        //store mapping
        public IList<int> SelectedStoreIds { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }

        //comma-separated list of stores used on the list page
        public string ListOfStores { get; set; }

        public IList<MessageTemplateLocalizedModel> Locales { get; set; }

        #endregion
    }

    public partial class MessageTemplateLocalizedModel : ILocalizedLocaleModel
    {
        public MessageTemplateLocalizedModel()
        {
            AvailableEmailAccounts = new List<SelectListItem>();
        }

        public int LanguageId { get; set; }


        public string BccEmailAddresses { get; set; }


        public string Subject { get; set; }

        public string Body { get; set; }


        public int EmailAccountId { get; set; }
        public IList<SelectListItem> AvailableEmailAccounts { get; set; }
    }
}