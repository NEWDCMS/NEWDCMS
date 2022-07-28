using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DCMS.ViewModel.Models.Messages
{

    public partial class NewsletterSubscriptionSearchModel : BaseSearchModel
    {


        public NewsletterSubscriptionSearchModel()
        {
            AvailableStores = new List<SelectListItem>();
            ActiveList = new List<SelectListItem>();
            AvailableCustomerRoles = new List<SelectListItem>();
        }


        [DataType(DataType.EmailAddress)]
        public string SearchEmail { get; set; }


        public int StoreId { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }

        public int ActiveId { get; set; }


        public IList<SelectListItem> ActiveList { get; set; }


        public int CustomerRoleId { get; set; }

        public IList<SelectListItem> AvailableCustomerRoles { get; set; }

        [UIHint("DateNullable")]
        public DateTime? StartDate { get; set; }

        [UIHint("DateNullable")]
        public DateTime? EndDate { get; set; }

        public bool HideStoresList { get; set; }

    }
}